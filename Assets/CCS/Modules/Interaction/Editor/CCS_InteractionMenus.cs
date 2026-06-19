using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_InteractionMenus
// CATEGORY: Modules / Interaction / Editor
// PURPOSE: Registers the Interaction module validation editor menu.
// PLACEMENT: Editor-only static registration. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Repairs assets, player wiring, and scene placement before validation.
// =============================================================================

namespace CCS.Modules.Interaction.Editor
{
    public static class CCS_InteractionMenus
    {
        private const string MenuRoot = "CCS/Interaction/";

        #region Public Methods

        [MenuItem(MenuRoot + "Validate Interaction Module")]
        public static void ValidateInteractionModuleMenu()
        {
            CCS_InteractionAssetBuilder.EnsureInteractionAssets();
            CCS_InteractionTestPlayerPrefabBuilder.EnsureTestPlayerInteractionScanner();
            CCS_InteractionMasterTestBuilder.EnsureMasterTestInteractable();
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
