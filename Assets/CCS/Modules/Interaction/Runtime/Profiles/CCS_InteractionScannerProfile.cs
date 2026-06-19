using CCS.Project;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_InteractionScannerProfile
// CATEGORY: Modules / Interaction / Runtime / Profiles
// PURPOSE: Profile-driven scanner range, layer mask, and cooldown settings.
// PLACEMENT: ScriptableObject asset under Tests/Profiles/.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Configuration only. Runtime scanning lives on CCS_NetworkInteractionScanner.
// =============================================================================

namespace CCS.Modules.Interaction
{
    [CreateAssetMenu(
        fileName = "CCS_InteractionScannerProfile",
        menuName = "CCS/Interaction/Scanner Profile",
        order = 0)]
    public sealed class CCS_InteractionScannerProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Scanner")]
        [Tooltip("Maximum interaction raycast distance in meters.")]
        [SerializeField] private float interactionRange = 3f;

        [Tooltip("Physics layers considered when scanning for interactables.")]
        [SerializeField] private LayerMask interactionLayerMask = ~0;

        [Tooltip("Minimum seconds between interaction attempts.")]
        [SerializeField] private float interactionCooldownSeconds = 0.25f;

        [Tooltip("When true, raycasts from the active camera forward. Otherwise uses player forward.")]
        [SerializeField] private bool useCameraForward = true;

        #endregion

        #region Properties

        public float InteractionRange => interactionRange;

        public LayerMask InteractionLayerMask => interactionLayerMask;

        public float InteractionCooldownSeconds => interactionCooldownSeconds;

        public bool UseCameraForward => useCameraForward;

        #endregion
    }
}
