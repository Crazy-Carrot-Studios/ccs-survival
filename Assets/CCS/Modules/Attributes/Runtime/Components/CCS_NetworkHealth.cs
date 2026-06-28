using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NetworkHealth
// CATEGORY: Modules / Attributes / Runtime / Components
// PURPOSE: Server-authoritative networked health component implementing CCS_IDamageable.
// PLACEMENT: Networked character root with CCS_AttributeContainer.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Offline health never writes NetworkVariables before spawn.
// =============================================================================

namespace CCS.Modules.Attributes
{
    public sealed class CCS_NetworkHealth : NetworkBehaviour, CCS_IDamageable
    {
        [SerializeField] private CCS_AttributeContainer attributeContainer;
        [SerializeField] private CCS_AttributeDefinition healthDefinition;
        [SerializeField] private bool enableHealthDebugLogs;

        private readonly NetworkVariable<float> replicatedHealth =
            new NetworkVariable<float>(
                0f,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<bool> replicatedDead =
            new NetworkVariable<bool>(
                false,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        private float offlineCurrentHealth = -1f;
        private bool offlineIsDead;
        private bool offlineInitialized;
        private Vector3 lastDamageDirection;

        public event System.Action<float, float> HealthChanged;

        public event System.Action<bool> DeadStateChanged;

        public Vector3 LastDamageDirection => lastDamageDirection;

        public float MaxHealth
        {
            get
            {
                if (attributeContainer != null
                    && attributeContainer.TryGetValue(ResolveHealthAttributeId(), out CCS_AttributeValue value))
                {
                    return value.Max;
                }

                return healthDefinition != null ? healthDefinition.MaxValue : 100f;
            }
        }

        public float CurrentHealth
        {
            get
            {
                if (IsNetworkHealthActive)
                {
                    return replicatedHealth.Value;
                }

                return offlineInitialized ? offlineCurrentHealth : ResolveInitialHealth();
            }
        }

        public bool IsDead
        {
            get
            {
                if (IsNetworkHealthActive)
                {
                    return replicatedDead.Value;
                }

                return offlineInitialized && offlineIsDead;
            }
        }

        public bool IsDamageReady
        {
            get
            {
                if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
                {
                    return IsSpawned && NetworkObject != null && NetworkObject.IsSpawned;
                }

                return true;
            }
        }

        private bool IsNetworkHealthActive =>
            NetworkManager.Singleton != null
            && NetworkManager.Singleton.IsListening
            && IsSpawned;

        private void Awake()
        {
            if (attributeContainer == null)
            {
                attributeContainer = GetComponent<CCS_AttributeContainer>();
            }
        }

        private void Start()
        {
            if (!IsNetworkHealthActive)
            {
                InitializeOfflineHealth();
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            replicatedHealth.OnValueChanged += HandleReplicatedHealthChanged;
            replicatedDead.OnValueChanged += HandleReplicatedDeadChanged;

            if (IsServer)
            {
                float initialHealth = ResolveInitialHealth();
                replicatedHealth.Value = initialHealth;
                replicatedDead.Value = initialHealth <= 0f;
                ApplyHealthToContainer(initialHealth, raiseEvents: false);
            }
            else
            {
                ApplyHealthToContainer(replicatedHealth.Value, raiseEvents: true);
            }
        }

        public override void OnNetworkDespawn()
        {
            replicatedHealth.OnValueChanged -= HandleReplicatedHealthChanged;
            replicatedDead.OnValueChanged -= HandleReplicatedDeadChanged;
            base.OnNetworkDespawn();
        }

        public bool ApplyDamage(CCS_DamageInfo damageInfo)
        {
            if (damageInfo.Amount <= 0f || IsDead || !IsDamageReady)
            {
                return false;
            }

            if (!IsNetworkHealthActive)
            {
                return ApplyDamageLocally(damageInfo);
            }

            if (IsServer)
            {
                return ApplyDamageOnServer(damageInfo);
            }

            RequestDamageServerRpc(
                damageInfo.Amount,
                (int)damageInfo.SourceType,
                damageInfo.HitPoint,
                damageInfo.HitDirection,
                damageInfo.SourceNetworkObjectId,
                damageInfo.AttributeId);
            return true;
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestDamageServerRpc(
            float amount,
            int sourceType,
            Vector3 hitPoint,
            Vector3 hitDirection,
            ulong sourceNetworkObjectId,
            string attributeId,
            ServerRpcParams rpcParams = default)
        {
            if (!IsDamageReady)
            {
                return;
            }

            CCS_DamageInfo damageInfo = new CCS_DamageInfo(
                amount,
                hitPoint,
                hitDirection,
                (CCS_DamageSourceType)sourceType,
                sourceObject: null,
                sourceNetworkObjectId,
                attributeId);
            ApplyDamageOnServer(damageInfo);
        }

        private bool ApplyDamageOnServer(CCS_DamageInfo damageInfo)
        {
            if (!IsServer || attributeContainer == null || IsDead || !IsDamageReady)
            {
                return false;
            }

            string attributeId = string.IsNullOrWhiteSpace(damageInfo.AttributeId)
                ? ResolveHealthAttributeId()
                : damageInfo.AttributeId;
            CCS_DamageRequest request = new CCS_DamageRequest(
                attributeId,
                damageInfo.Amount,
                damageInfo.SourceType.ToString());
            if (!attributeContainer.ApplyDamage(request, out CCS_DamageAppliedEvent damageEvent))
            {
                return false;
            }

            replicatedHealth.Value = damageEvent.ResultingValue.Current;
            replicatedDead.Value = damageEvent.ResultingValue.IsAtMin;
            lastDamageDirection = ResolveDamageDirection(damageInfo);
            RaiseHealthChanged(replicatedHealth.Value, MaxHealth);
            if (replicatedDead.Value)
            {
                DeadStateChanged?.Invoke(true);
            }

            if (enableHealthDebugLogs)
            {
                Debug.Log(
                    $"[Attributes] NetworkHealth damage={damageEvent.AppliedAmount:0.##} health={replicatedHealth.Value:0.##}",
                    this);
            }

            return damageEvent.AppliedAmount > 0f;
        }

        private bool ApplyDamageLocally(CCS_DamageInfo damageInfo)
        {
            if (attributeContainer == null || IsDead)
            {
                return false;
            }

            if (!offlineInitialized)
            {
                InitializeOfflineHealth();
            }

            CCS_DamageRequest request = new CCS_DamageRequest(
                ResolveHealthAttributeId(),
                damageInfo.Amount,
                damageInfo.SourceType.ToString());
            if (!attributeContainer.ApplyDamage(request, out CCS_DamageAppliedEvent damageEvent))
            {
                return false;
            }

            offlineCurrentHealth = damageEvent.ResultingValue.Current;
            offlineIsDead = damageEvent.ResultingValue.IsAtMin;
            lastDamageDirection = ResolveDamageDirection(damageInfo);
            RaiseHealthChanged(offlineCurrentHealth, MaxHealth);
            if (offlineIsDead)
            {
                DeadStateChanged?.Invoke(true);
            }

            return damageEvent.AppliedAmount > 0f;
        }

        private void InitializeOfflineHealth()
        {
            offlineCurrentHealth = ResolveInitialHealth();
            offlineIsDead = offlineCurrentHealth <= 0f;
            offlineInitialized = true;
            ApplyHealthToContainer(offlineCurrentHealth, raiseEvents: false);
            RaiseHealthChanged(offlineCurrentHealth, MaxHealth);
        }

        private void RaiseHealthChanged(float currentHealth, float maxHealth)
        {
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private static Vector3 ResolveDamageDirection(CCS_DamageInfo damageInfo)
        {
            if (damageInfo.HitDirection.sqrMagnitude > 0.0001f)
            {
                return damageInfo.HitDirection.normalized;
            }

            return Vector3.zero;
        }

        private void HandleReplicatedHealthChanged(float previousValue, float newValue)
        {
            if (IsServer)
            {
                return;
            }

            ApplyHealthToContainer(newValue, raiseEvents: !Mathf.Approximately(previousValue, newValue));
            if (!Mathf.Approximately(previousValue, newValue))
            {
                RaiseHealthChanged(newValue, MaxHealth);
            }
        }

        private void HandleReplicatedDeadChanged(bool previousValue, bool newValue)
        {
            if (previousValue != newValue)
            {
                DeadStateChanged?.Invoke(newValue);
            }

            if (previousValue == newValue || !enableHealthDebugLogs)
            {
                return;
            }

            Debug.Log(newValue ? "[Attributes] NetworkHealth entered dead state." : "[Attributes] NetworkHealth revived.", this);
        }

        private void ApplyHealthToContainer(float healthValue, bool raiseEvents)
        {
            if (attributeContainer == null)
            {
                return;
            }

            attributeContainer.SetValue(ResolveHealthAttributeId(), healthValue, raiseEvents);
        }

        private float ResolveInitialHealth()
        {
            if (attributeContainer != null
                && attributeContainer.TryGetValue(ResolveHealthAttributeId(), out CCS_AttributeValue value))
            {
                return value.Current;
            }

            return healthDefinition != null ? healthDefinition.DefaultValue : 100f;
        }

        private string ResolveHealthAttributeId()
        {
            if (healthDefinition != null && !string.IsNullOrWhiteSpace(healthDefinition.ProfileId))
            {
                return healthDefinition.ProfileId;
            }

            return CCS_AttributesConstants.HealthAttributeId;
        }
    }
}
