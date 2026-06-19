using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterControllerMasterTestMenus
// CATEGORY: Modules / CharacterController / Editor
// PURPOSE: Registers the master test scene setup-and-validate editor menu.
// PLACEMENT: Editor-only static registration. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Single menu action runs builder then validator in sequence.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerMasterTestMenus
    {
        private const string SceneMenuRoot = "CCS/Character Controller/Scene/";

        #region Public Methods

        [MenuItem(SceneMenuRoot + "Setup And Validate Master Test Scene")]
        public static void SetupAndValidateMasterTestSceneMenu()
        {
            RunBatchSetupAndValidate();
        }

        public static void RunBatchSetupAndValidate()
        {
            SetupMasterTestScene();
            LogResult(ValidateMasterTestScene());
        }

        #endregion

        #region Private Methods

        private static void SetupMasterTestScene()
        {
            CCS_CharacterControllerMasterTestBuilder.SetupMasterTestScene();
        }

        private static CCS_SurvivalValidationResult ValidateMasterTestScene()
        {
            return CCS_CharacterControllerMasterTestValidator.ValidateMasterTestScene();
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
