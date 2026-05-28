using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalVitalsModifierZone
// CATEGORY: Survival / Environment / VitalsZones
// PURPOSE: Trigger volume applying direct vitals modifiers through CCS_SurvivalVitalsZoneReceiver.
// PLACEMENT: Under CCS_PrototypeVitalsZonesRoot in SCN_CCS_Survival_Bootstrap.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Requires a trigger collider on this GameObject or a child collider.
// =============================================================================

namespace CCS.Survival.Environment.VitalsZones
{
    [DisallowMultipleComponent]
    public sealed class CCS_SurvivalVitalsModifierZone : MonoBehaviour
    {
        #region Variables

        [Header("Zone")]
        [Tooltip("When disabled, this modifier volume has no effect.")]
        [SerializeField] private bool isZoneEnabled = true;

        [Tooltip("Optional display name for logs and debugging.")]
        [SerializeField] private string zoneDisplayName;

        [Tooltip("Optional profile preset. Inline values below remain editable.")]
        [SerializeField] private CCS_SurvivalVitalsModifierProfile modifierProfile;

        [Header("Modifier")]
        [SerializeField] private CCS_SurvivalVitalsModifierType modifierType = CCS_SurvivalVitalsModifierType.HungerDrain;

        [Tooltip("Magnitude applied per second while a receiver remains inside.")]
        [SerializeField] private float ratePerSecond = 5f;

        [Tooltip("Optional minimum vital value after apply (-1 = unused).")]
        [SerializeField] private float minVitalClamp = -1f;

        [Tooltip("Optional maximum vital value after apply (-1 = unused).")]
        [SerializeField] private float maxVitalClamp = -1f;

        [Header("Telemetry")]
        [Tooltip("Logs concise enter/exit messages for this zone.")]
        [SerializeField] private bool enableTelemetryLogging;

        [Header("Gizmos")]
        [Tooltip("Draws a wire primitive for this zone in the Scene view.")]
        [SerializeField] private bool drawZoneGizmo = true;

        private Collider zoneCollider;

        #endregion

        #region Properties

        public bool IsZoneEnabled => isZoneEnabled;

        public CCS_SurvivalVitalsModifierType ModifierType =>
            modifierProfile != null ? modifierProfile.ModifierType : modifierType;

        public float RatePerSecond => modifierProfile != null ? modifierProfile.RatePerSecond : ratePerSecond;

        public float MinVitalClamp => modifierProfile != null ? modifierProfile.MinVitalClamp : minVitalClamp;

        public float MaxVitalClamp => modifierProfile != null ? modifierProfile.MaxVitalClamp : maxVitalClamp;

        public string TelemetryLabel => string.IsNullOrWhiteSpace(zoneDisplayName)
            ? $"{ModifierType} zone"
            : zoneDisplayName;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            zoneCollider = GetComponent<Collider>();
            if (zoneCollider == null)
            {
                zoneCollider = GetComponentInChildren<Collider>();
            }

            if (zoneCollider != null)
            {
                zoneCollider.isTrigger = true;
            }
            else
            {
                Debug.LogWarning(
                    $"{CCS_SurvivalVitalsZoneReceiver.LogPrefix} Modifier zone '{name}' has no trigger collider.");
            }

            ApplyProfileDefaultsIfAssigned();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isZoneEnabled)
            {
                return;
            }

            if (!TryGetReceiver(other, out CCS_SurvivalVitalsZoneReceiver receiver))
            {
                return;
            }

            receiver.RegisterModifierZone(this);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!TryGetReceiver(other, out CCS_SurvivalVitalsZoneReceiver receiver))
            {
                return;
            }

            receiver.UnregisterModifierZone(this);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!drawZoneGizmo)
            {
                return;
            }

            Gizmos.color = GetGizmoColor(ModifierType);
            Gizmos.matrix = transform.localToWorldMatrix;

            if (zoneCollider == null)
            {
                zoneCollider = GetComponent<Collider>();
            }

            if (zoneCollider is BoxCollider)
            {
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                return;
            }

            if (zoneCollider is CapsuleCollider capsule)
            {
                DrawWireCapsule(capsule);
                return;
            }

            Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
        }
#endif

        #endregion

        #region Private Methods

        private static bool TryGetReceiver(Collider other, out CCS_SurvivalVitalsZoneReceiver receiver)
        {
            if (other.TryGetComponent(out receiver))
            {
                return true;
            }

            receiver = other.GetComponentInParent<CCS_SurvivalVitalsZoneReceiver>();
            return receiver != null;
        }

        private void ApplyProfileDefaultsIfAssigned()
        {
            if (modifierProfile == null)
            {
                return;
            }

            modifierType = modifierProfile.ModifierType;
            ratePerSecond = modifierProfile.RatePerSecond;
            minVitalClamp = modifierProfile.MinVitalClamp;
            maxVitalClamp = modifierProfile.MaxVitalClamp;
        }

        private static Color GetGizmoColor(CCS_SurvivalVitalsModifierType type)
        {
            return type switch
            {
                CCS_SurvivalVitalsModifierType.HungerDrain => new Color(0.85f, 0.35f, 0.15f, 0.9f),
                CCS_SurvivalVitalsModifierType.HungerRestore => new Color(0.35f, 0.85f, 0.25f, 0.9f),
                CCS_SurvivalVitalsModifierType.ThirstDrain => new Color(0.2f, 0.45f, 0.95f, 0.9f),
                CCS_SurvivalVitalsModifierType.ThirstRestore => new Color(0.25f, 0.85f, 0.95f, 0.9f),
                CCS_SurvivalVitalsModifierType.StaminaDrain => new Color(0.75f, 0.75f, 0.2f, 0.9f),
                CCS_SurvivalVitalsModifierType.StaminaRestore => new Color(0.95f, 0.9f, 0.3f, 0.9f),
                CCS_SurvivalVitalsModifierType.ExposureIncrease => new Color(0.55f, 0.2f, 0.85f, 0.9f),
                CCS_SurvivalVitalsModifierType.ExposureRecovery => new Color(0.45f, 0.85f, 0.75f, 0.9f),
                CCS_SurvivalVitalsModifierType.TemperatureIncrease => new Color(1f, 0.35f, 0.15f, 0.9f),
                CCS_SurvivalVitalsModifierType.TemperatureDecrease => new Color(0.25f, 0.6f, 1f, 0.9f),
                CCS_SurvivalVitalsModifierType.HealthDrain => new Color(0.9f, 0.15f, 0.15f, 0.9f),
                CCS_SurvivalVitalsModifierType.HealthRestore => new Color(0.2f, 0.95f, 0.35f, 0.9f),
                _ => new Color(0.8f, 0.8f, 0.8f, 0.9f)
            };
        }

#if UNITY_EDITOR
        private static void DrawWireCapsule(CapsuleCollider capsule)
        {
            float radius = Mathf.Max(0.05f, capsule.radius);
            float height = Mathf.Max(radius * 2f, capsule.height);
            float cylinderHeight = Mathf.Max(0f, height - radius * 2f);
            Vector3 center = capsule.center;
            int direction = capsule.direction;

            Vector3 up = direction switch
            {
                0 => Vector3.right,
                2 => Vector3.forward,
                _ => Vector3.up
            };

            Vector3 top = center + up * (cylinderHeight * 0.5f);
            Vector3 bottom = center - up * (cylinderHeight * 0.5f);
            Gizmos.DrawWireSphere(top, radius);
            Gizmos.DrawWireSphere(bottom, radius);
        }
#endif

        #endregion
    }
}
