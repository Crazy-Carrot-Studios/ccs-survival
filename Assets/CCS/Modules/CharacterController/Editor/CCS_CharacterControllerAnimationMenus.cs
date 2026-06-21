using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterControllerAnimationMenus
// CATEGORY: Modules / CharacterController / Editor
// PURPOSE: Registers animation isolation build and validation editor menus.
// PLACEMENT: Editor-only static registration. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.5.6 duplicates vendor clips into Content/Animations and rewires player AC.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerAnimationMenus
    {
        private const string MenuRoot = "CCS/Character Controller/Animations/";

        #region Public Methods

        [MenuItem(MenuRoot + "Isolate Player Animation Clips")]
        public static void IsolatePlayerAnimationClipsMenu()
        {
            bool changed = CCS_CharacterControllerAnimationIsolationBuilder.EnsurePlayerAnimationIsolation();
            Debug.Log(
                changed
                    ? "[Character Controller] Player animation clips isolated and Animator Controller rewired."
                    : "[Character Controller] Player animation clips already isolated.");
        }

        [MenuItem(MenuRoot + "Validate Player Animation Isolation")]
        public static void ValidatePlayerAnimationIsolationMenu()
        {
            CCS_CharacterControllerAnimationIsolationBuilder.EnsurePlayerAnimationIsolation();
            LogResult(
                CCS_CharacterControllerAnimationValidationUtility.ValidatePlayerAnimatorControllerAnimationIsolation());
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
