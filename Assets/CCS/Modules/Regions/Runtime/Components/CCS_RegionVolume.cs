using CCS.Survival.Player;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RegionVolume
// CATEGORY: Modules / Regions / Runtime / Components
// PURPOSE: Trigger volume that discovers and tracks region entry and exit.
// PLACEMENT: Bootstrap frontier region volumes under SCN_CCS_Survival_Bootstrap.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Requires trigger collider. No procedural generation or final map UI.
// =============================================================================

namespace CCS.Modules.Regions
{
    [RequireComponent(typeof(Collider))]
    public sealed class CCS_RegionVolume : MonoBehaviour
    {
        #region Variables

        [Header("Region")]
        [SerializeField] private CCS_RegionDefinition regionDefinition;

        #endregion

        #region Properties

        public CCS_RegionDefinition RegionDefinition => regionDefinition;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            Collider volumeCollider = GetComponent<Collider>();
            if (volumeCollider != null)
            {
                volumeCollider.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsPlayerCollider(other) || regionDefinition == null)
            {
                return;
            }

            if (!CCS_RegionRuntimeBridge.TryGetRegionService(out CCS_RegionService regionService)
                || !regionService.IsInitialized)
            {
                return;
            }

            regionService.NotifyRegionEntered(regionDefinition, ResolveSubjectPosition(other));
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsPlayerCollider(other) || regionDefinition == null)
            {
                return;
            }

            if (!CCS_RegionRuntimeBridge.TryGetRegionService(out CCS_RegionService regionService)
                || !regionService.IsInitialized)
            {
                return;
            }

            regionService.NotifyRegionExited(regionDefinition, ResolveSubjectPosition(other));
        }

        #endregion

        #region Private Methods

        private static bool IsPlayerCollider(Collider other)
        {
            return other != null && other.GetComponentInParent<CCS_PlayerGameplayController>() != null;
        }

        private static Vector3 ResolveSubjectPosition(Collider other)
        {
            CCS_PlayerGameplayController controller = other.GetComponentInParent<CCS_PlayerGameplayController>();
            return controller != null ? controller.transform.position : other.transform.position;
        }

        #endregion
    }
}
