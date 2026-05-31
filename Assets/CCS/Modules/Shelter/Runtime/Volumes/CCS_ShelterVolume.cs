using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ShelterVolume
// CATEGORY: Modules / Shelter / Runtime / Volumes
// PURPOSE: Trigger volume that applies local shelter protection on entry and exit.
// PLACEMENT: Bootstrap test volumes and future building interior volumes.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Requires trigger collider. No building placement or final art in 0.7.5.
// =============================================================================

namespace CCS.Modules.Shelter
{
    [RequireComponent(typeof(Collider))]
    public sealed class CCS_ShelterVolume : MonoBehaviour
    {
        #region Variables

        [Header("Identity")]
        [Tooltip("Stable shelter identifier used by save/load and environment integration.")]
        [SerializeField] private string shelterId = "ccs.survival.shelter.test.bootstrap";

        [Tooltip("Readable label for debug logs and future UI.")]
        [SerializeField] private string displayName = "Test Shelter";

        [Header("Protection")]
        [Tooltip("Wetness protection applied while inside this volume.")]
        [SerializeField] private float wetnessProtection = 1f;

        [Tooltip("Exposure protection applied while inside this volume.")]
        [SerializeField] private float exposureProtection = 0.6f;

        [Tooltip("Temperature protection placeholder applied while inside this volume.")]
        [SerializeField] private float temperatureProtection = 1f;

        [Tooltip("Multiplier applied to this volume's protection values.")]
        [SerializeField] private float protectionMultiplier = 1f;

        [Header("Trigger Rules")]
        [Tooltip("When enabled, any collider entering the trigger may activate shelter.")]
        [SerializeField] private bool acceptAnyTriggerSubject = true;

        private Collider volumeCollider;

        #endregion

        #region Properties

        public string ShelterId => shelterId;

        public string DisplayName => displayName;

        public float WetnessProtection => wetnessProtection < 0f ? 0f : wetnessProtection;

        public float ExposureProtection => exposureProtection < 0f ? 0f : exposureProtection;

        public float TemperatureProtection => temperatureProtection;

        public float ProtectionMultiplier => protectionMultiplier <= 0f ? 1f : protectionMultiplier;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            volumeCollider = GetComponent<Collider>();
            if (volumeCollider != null)
            {
                volumeCollider.isTrigger = true;
            }
        }

        private void OnEnable()
        {
            if (CCS_ShelterRuntimeBridge.TryGetShelterService(out CCS_ShelterService shelterService)
                && shelterService.IsInitialized)
            {
                shelterService.RegisterVolume(this);
            }
        }

        private void OnDisable()
        {
            if (CCS_ShelterRuntimeBridge.TryGetShelterService(out CCS_ShelterService shelterService)
                && shelterService.IsInitialized)
            {
                shelterService.UnregisterVolume(this);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsValidTriggerSubject(other))
            {
                return;
            }

            if (!CCS_ShelterRuntimeBridge.TryGetShelterService(out CCS_ShelterService shelterService)
                || !shelterService.IsInitialized)
            {
                return;
            }

            shelterService.EnterShelter(this);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsValidTriggerSubject(other))
            {
                return;
            }

            if (!CCS_ShelterRuntimeBridge.TryGetShelterService(out CCS_ShelterService shelterService)
                || !shelterService.IsInitialized)
            {
                return;
            }

            CCS_ShelterSnapshot snapshot = shelterService.GetSnapshot();
            if (snapshot.IsSheltered && snapshot.ActiveShelterId == ShelterId)
            {
                shelterService.ExitShelter($"Exited shelter '{DisplayName}'.");
            }
        }

        #endregion

        #region Private Methods

        private bool IsValidTriggerSubject(Collider other)
        {
            if (other == null)
            {
                return false;
            }

            if (acceptAnyTriggerSubject)
            {
                return true;
            }

            return other.CompareTag("Player");
        }

        #endregion
    }
}
