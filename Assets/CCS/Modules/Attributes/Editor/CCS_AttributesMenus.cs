using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AttributesMenus
// CATEGORY: Modules / Attributes / Editor
// PURPOSE: Registers the Attributes module validation editor menu.
// PLACEMENT: Editor-only static registration. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Single menu item repairs test player wiring then validates module assets.
// =============================================================================

namespace CCS.Modules.Attributes.Editor
{
    public static class CCS_AttributesMenus
    {
        private const string MenuRoot = "CCS/Attributes/";

        #region Public Methods

        [MenuItem(MenuRoot + "Validate Attributes Module")]
        public static void ValidateAttributesModuleMenu()
        {
            CCS_AttributesAssetBuilder.EnsureAttributesAssets();
            CCS_AttributesTestPlayerPrefabBuilder.EnsureTestPlayerAttributes();
            LogResult(CCS_AttributesModuleValidator.ValidateAttributesModule());
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
