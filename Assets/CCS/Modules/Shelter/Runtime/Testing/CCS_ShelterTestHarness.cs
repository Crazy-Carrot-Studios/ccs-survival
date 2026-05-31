using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ShelterTestHarness
// CATEGORY: Modules / Shelter / Runtime / Testing
// PURPOSE: Development-only harness that toggles shelter state for bootstrap verification.
// PLACEMENT: Bootstrap verification scenes only. Disable for shipping builds.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Simulates enter/exit when no final player trigger exists yet.
// =============================================================================

namespace CCS.Modules.Shelter
{
    [DefaultExecutionOrder(260)]
    public sealed class CCS_ShelterTestHarness : MonoBehaviour
    {
        #region Variables

        [Header("Development Testing")]
        [Tooltip("When enabled, the harness toggles shelter state on an interval.")]
        [SerializeField] private bool enableHarness = true;

        [Tooltip("Seconds between automated shelter enter/exit toggles.")]
        [SerializeField] private float toggleIntervalSeconds = 5f;

        [Tooltip("Shelter ID used when the harness enters shelter.")]
        [SerializeField] private string testShelterId = "ccs.survival.shelter.test.harness";

        private float nextToggleTime;
        private bool isHarnessSheltered;

        #endregion

        #region Unity Callbacks

        private void Update()
        {
            if (!enableHarness)
            {
                return;
            }

            if (Time.time < nextToggleTime)
            {
                return;
            }

            nextToggleTime = Time.time + toggleIntervalSeconds;

            if (!CCS_ShelterRuntimeBridge.TryGetShelterService(out CCS_ShelterService shelterService)
                || !shelterService.IsInitialized)
            {
                return;
            }

            if (isHarnessSheltered)
            {
                if (shelterService.ExitShelter("[CCS_ShelterTestHarness] Exited test shelter."))
                {
                    isHarnessSheltered = false;
                }

                return;
            }

            if (shelterService.EnterShelterWithProfileDefaults(
                testShelterId,
                "[CCS_ShelterTestHarness] Entered test shelter."))
            {
                isHarnessSheltered = true;
            }
        }

        #endregion
    }
}
