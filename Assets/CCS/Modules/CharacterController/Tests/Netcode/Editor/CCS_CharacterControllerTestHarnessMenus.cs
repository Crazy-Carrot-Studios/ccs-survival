using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterControllerTestHarnessMenus
// CATEGORY: Modules / CharacterController / Tests / Netcode / Editor
// PURPOSE: Registers the multiplayer hosting scene setup-and-validate editor menu.
// PLACEMENT: Editor-only static registration. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Single menu action runs scene repair, UI rebuild, then validation.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode.Editor
{
    public static class CCS_CharacterControllerTestHarnessMenus
    {
        private const string SceneMenuRoot = "CCS/Character Controller/Scene/";

        #region Public Methods

        [MenuItem(SceneMenuRoot + "Setup And Validate Multiplayer Hosting Scene")]
        public static void SetupAndValidateMultiplayerHostingSceneMenu()
        {
            RunBatchBuildAndValidate();
        }

        public static void RunBatchBuildAndValidate()
        {
            SetupMultiplayerHostingScene();
            LogResult(ValidateMultiplayerHostingScene());
        }

        public static void RunFromBatchMode()
        {
            RunBatchBuildAndValidate();
            CCS_SurvivalValidationResult result = ValidateMultiplayerHostingScene();
            LogResult(result);
            EditorApplication.Exit(result.IsSuccess ? 0 : 1);
        }

        #endregion

        #region Private Methods

        private static void SetupMultiplayerHostingScene()
        {
            CCS_MultiplayerHostingBuilder.VerifyAndRepairScene();
        }

        private static CCS_SurvivalValidationResult ValidateMultiplayerHostingScene()
        {
            return CCS_MultiplayerHostingValidator.ValidateHostingScene();
        }

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
