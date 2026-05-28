using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalHazardZone
// CATEGORY: Survival / Environment / Hazards
// PURPOSE: Trigger volume that applies environmental survival pressure through CCS_SurvivalHazardReceiver.
// PLACEMENT: Scene hazard volumes under CCS_PrototypeHazardsRoot in SCN_CCS_Survival_Bootstrap.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Requires a trigger collider on this GameObject or a child collider.
// =============================================================================

namespace CCS.Survival.Environment.Hazards
{
    [DisallowMultipleComponent]
    public sealed class CCS_SurvivalHazardZone : MonoBehaviour
    {
        #region Variables

        [Header("Zone")]
        [Tooltip("When disabled, this hazard volume has no effect.")]
        [SerializeField] private bool isZoneEnabled = true;

        [Tooltip("Optional display name for logs and debugging.")]
        [SerializeField] private string zoneDisplayName;

        [Tooltip("Optional profile preset. Inline values below are still editable.")]
        [SerializeField] private CCS_SurvivalHazardProfile hazardProfile;

        [Header("Hazard Identity")]
        [SerializeField] private CCS_SurvivalHazardType hazardType = CCS_SurvivalHazardType.GenericDamage;

        [Header("Pressure Rates")]
        [Tooltip("Health damage applied per second.")]
        [SerializeField] private float healthDamagePerSecond = 2f;

        [Tooltip("Exposure added per second.")]
        [SerializeField] private float exposurePerSecond;

        [Tooltip("Stamina drained per second.")]
        [SerializeField] private float staminaDrainPerSecond;

        [Tooltip("Body temperature change per second.")]
        [SerializeField] private float temperatureChangePerSecond;

        [Header("Gizmos")]
        [Tooltip("Draws a wire cube for this hazard zone in the Scene view.")]
        [SerializeField] private bool drawZoneGizmo = true;

        private Collider zoneCollider;

        #endregion

        #region Properties

        public bool IsZoneEnabled => isZoneEnabled;

        public CCS_SurvivalHazardType HazardType => hazardProfile != null ? hazardProfile.HazardType : hazardType;

        public string TelemetryLabel => string.IsNullOrWhiteSpace(zoneDisplayName)
            ? $"{HazardType} zone"
            : zoneDisplayName;

        public float HealthDamagePerSecond =>
            hazardProfile != null ? hazardProfile.HealthDamagePerSecond : healthDamagePerSecond;

        public float ExposurePerSecond =>
            hazardProfile != null ? hazardProfile.ExposurePerSecond : exposurePerSecond;

        public float StaminaDrainPerSecond =>
            hazardProfile != null ? hazardProfile.StaminaDrainPerSecond : staminaDrainPerSecond;

        public float TemperatureChangePerSecond =>
            hazardProfile != null ? hazardProfile.TemperatureChangePerSecond : temperatureChangePerSecond;

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
                Debug.LogWarning($"{CCS_SurvivalHazardReceiver.LogPrefix} Hazard zone '{name}' has no trigger collider.");
            }

            ApplyProfileDefaultsIfUnset();
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
                receiver.RegisterHazardZone(this);
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
                receiver.UnregisterHazardZone(this);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!drawZoneGizmo)
            {
                return;
            }

            Gizmos.color = GetGizmoColor(HazardType);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
#endif

        #endregion

        #region Private Methods

        private void ApplyProfileDefaultsIfUnset()
        {
            if (hazardProfile == null)
            {
                return;
            }

            hazardType = hazardProfile.HazardType;
            healthDamagePerSecond = hazardProfile.HealthDamagePerSecond;
            exposurePerSecond = hazardProfile.ExposurePerSecond;
            staminaDrainPerSecond = hazardProfile.StaminaDrainPerSecond;
            temperatureChangePerSecond = hazardProfile.TemperatureChangePerSecond;
        }

        private static Color GetGizmoColor(CCS_SurvivalHazardType type)
        {
            return type switch
            {
                CCS_SurvivalHazardType.Cold => new Color(0.2f, 0.55f, 1f, 0.85f),
                CCS_SurvivalHazardType.Heat => new Color(1f, 0.45f, 0.1f, 0.85f),
                CCS_SurvivalHazardType.Toxic => new Color(0.2f, 0.9f, 0.25f, 0.85f),
                CCS_SurvivalHazardType.Radiation => new Color(0.85f, 0.2f, 0.95f, 0.85f),
                _ => new Color(1f, 0.2f, 0.2f, 0.85f)
            };
        }

        #endregion
    }
}
