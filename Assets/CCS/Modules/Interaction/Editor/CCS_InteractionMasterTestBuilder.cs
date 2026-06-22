using UnityEditor;

// =============================================================================
// SCRIPT: CCS_InteractionMasterTestBuilder
// CATEGORY: Modules / Interaction / Editor
// PURPOSE: Ensures Master Test interaction wiring via the detection-cube builder.
// PLACEMENT: Editor utility invoked from master test setup and Interaction validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Delegates to CCS_InteractionDetectionTestBuilder for a single scene-baked cube.
// =============================================================================

namespace CCS.Modules.Interaction.Editor
{
    public static class CCS_InteractionMasterTestBuilder
    {
        #region Public Methods

        public static bool BuildMasterTestPickupInteraction()
        {
            CCS_InteractionAssetBuilder.EnsureInteractionAssets();
            CCS_InteractionTestPlayerPrefabBuilder.EnsureTestPlayerInteractionWiring();
            CCS_InteractionPromptHudPrefabBuilder.EnsureTestPlayerInteractionPromptHud();
            return EnsureMasterTestPickupInteraction();
        }

        public static bool EnsureMasterTestPickupInteraction()
        {
            return CCS_InteractionDetectionTestBuilder.BuildMasterTestInteractions();
        }

        #endregion
    }
}
