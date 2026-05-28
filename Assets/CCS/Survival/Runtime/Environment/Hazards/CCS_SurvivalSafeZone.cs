using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalSafeZone
// CATEGORY: Survival / Environment / Hazards
// PURPOSE: Trigger volume that suppresses hazard pressure and optionally restores vitals.
// PLACEMENT: Scene safe volumes under CCS_PrototypeHazardsRoot in SCN_CCS_Survival_Bootstrap.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Requires a trigger collider on this GameObject or a child collider.
// =============================================================================

namespace CCS.Survival.Environment.Hazards
{
    [DisallowMultipleComponent]
    public sealed class CCS_SurvivalSafeZone : MonoBehaviour
    {
        #region Variables

        [Header("Zone")]
        [Tooltip("When disabled, this safe zone has no effect.")]
        [SerializeField] private bool isZoneEnabled = true;

        [Tooltip("Optional display name for logs and debugging.")]
        [SerializeField] private string zoneDisplayName = "Safe zone";

        [Header("Recovery")]
        [Tooltip("Health restored per second while inside the safe zone.")]
        [SerializeField] private float healthRecoveryPerSecond = 1f;

        [Tooltip("Stamina restored per second while inside the safe zone.")]
        [SerializeField] private float staminaRecoveryPerSecond = 4f;

        [Tooltip("Exposure reduced per second while inside the safe zone.")]
        [SerializeField] private float exposureReductionPerSecond = 2f;

        [Tooltip("When enabled, exposure is cleared while inside the safe zone.")]
        [SerializeField] private bool clearExposureWhileInside = true;

        [Header("Gizmos")]
        [SerializeField] private bool drawZoneGizmo = true;

        private Collider zoneCollider;

        #endregion

        #region Properties

        public bool IsZoneEnabled => isZoneEnabled;

        public string TelemetryLabel => string.IsNullOrWhiteSpace(zoneDisplayName) ? "Safe zone" : zoneDisplayName;

        public float HealthRecoveryPerSecond => healthRecoveryPerSecond;

        public float StaminaRecoveryPerSecond => staminaRecoveryPerSecond;

        public float ExposureReductionPerSecond => exposureReductionPerSecond;

        public bool ClearExposureWhileInside => clearExposureWhileInside;

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
                Debug.LogWarning($"{CCS_SurvivalHazardReceiver.LogPrefix} Safe zone '{name}' has no trigger collider.");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isZoneEnabled)
            {
                return;
            }

            if (!other.TryGetComponent(out CCS_SurvivalHazardReceiver receiver))
            {
                receiver = other.GetComponentInParent<CCS_SurvivalHazardReceiver>();
            }

            if (receiver != null)
            {
                receiver.RegisterSafeZone(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out CCS_SurvivalHazardReceiver receiver))
            {
                receiver = other.GetComponentInParent<CCS_SurvivalHazardReceiver>();
            }

            if (receiver != null)
            {
                receiver.UnregisterSafeZone(this);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!drawZoneGizmo)
            {
                return;
            }

            Gizmos.color = new Color(0.2f, 0.95f, 1f, 0.75f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
#endif

        #endregion
    }
}
