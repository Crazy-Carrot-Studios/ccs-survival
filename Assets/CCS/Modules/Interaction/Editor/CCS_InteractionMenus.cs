using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_InteractionMenus
// CATEGORY: Modules / Interaction / Editor
// PURPOSE: Registers the Interaction module validation and setup editor menus.
// PLACEMENT: Editor-only static registration. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Repairs assets, player wiring, pickup cube, and building door before validation.
// =============================================================================

namespace CCS.Modules.Interaction.Editor
{
    public static class CCS_InteractionMenus
    {
        private const string MenuRoot = "CCS/Interaction/";

        #region Public Methods

        [MenuItem(MenuRoot + "Build Master Test Interactions")]
        public static void BuildMasterTestInteractionsMenu()
        {
            bool changed = CCS_InteractionDetectionTestBuilder.BuildMasterTestInteractions();
            Debug.Log(
                changed
                    ? "[Interaction] Master Test interactions updated (pickup cube + building door)."
                    : "[Interaction] Master Test interactions already up to date.");
        }

        [MenuItem(MenuRoot + "Validate Interaction Module")]
        public static void ValidateInteractionModuleMenu()
        {
            CCS_InteractionAssetBuilder.EnsureInteractionAssets();
            CCS_InteractionTestPlayerPrefabBuilder.EnsureTestPlayerInteractionWiring();
            CCS_InteractionDetectionTestBuilder.BuildMasterTestInteractions();
            LogResult(CCS_InteractionModuleValidator.ValidateInteractionModule());
        }

        #endregion

        #region Private Methods

        private static void LogResult(CCS_SurvivalValidationResult result)
        {
            if (result.IsSuccess)
            {
                Debug.Log($"[Validation] Passed: {result.Message}");
            }
            else
            {
                Debug.LogError($"[Validation] Failed: {result.Message}");
            }
        }

        #endregion
    }
}
