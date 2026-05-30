using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_InteractionProfile
// CATEGORY: Modules / Interaction / Runtime / Profiles
// PURPOSE: Tuning profile for interaction scan distance and physics layers.
// PLACEMENT: Assets/CCS/Survival/Profiles/Interaction/ (project shell configuration).
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Forward raycast only in 0.3.9. No gameplay module coupling.
// =============================================================================

namespace CCS.Modules.Interaction
{
    [CreateAssetMenu(
        fileName = "CCS_InteractionProfile",
        menuName = "CCS/Survival/Interaction/Interaction Profile")]
    public sealed class CCS_InteractionProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Detection")]
        [Tooltip("Maximum forward raycast distance in meters.")]
        [SerializeField] private float interactionDistance = 3f;

        [Tooltip("Physics layers considered by the interaction scanner.")]
        [SerializeField] private LayerMask interactionLayers = ~0;

        #endregion

        #region Properties

        public float InteractionDistance => interactionDistance;

        public LayerMask InteractionLayers => interactionLayers;

        #endregion
    }
}
