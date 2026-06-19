using CCS.Project;

using UnityEditor;

using UnityEngine;



// =============================================================================

// SCRIPT: CCS_CharacterControllerMasterTestMenus

// CATEGORY: Modules / CharacterController / Editor

// PURPOSE: Registers master test scene setup and validation menus.

// PLACEMENT: Editor-only static registration. Not attached to GameObjects.

// AUTHOR: James Schilz

// CREATED: 2026-06-07

// NOTES: Setup runs builder only. Validator reports problems only.

// =============================================================================



namespace CCS.Modules.CharacterController.Editor

{

    public static class CCS_CharacterControllerMasterTestMenus

    {

        private const string SceneMenuRoot = "CCS/Modules/Character Controller/Scene/";



        #region Public Methods



        [MenuItem(SceneMenuRoot + "Setup Master Test Scene")]

        public static void SetupMasterTestSceneMenu()

        {

            CCS_CharacterControllerMasterTestBuilder.SetupMasterTestScene();

        }



        [MenuItem(SceneMenuRoot + "Validate Master Test Scene")]

        public static void ValidateMasterTestSceneMenu()

        {

            LogResult(CCS_CharacterControllerMasterTestValidator.ValidateMasterTestScene());

        }



        [MenuItem(SceneMenuRoot + "Setup And Validate Master Test Scene")]

        public static void RunBatchSetupAndValidateMenu()

        {

            RunBatchSetupAndValidate();

        }



        public static void RunBatchSetupAndValidate()

        {

            CCS_CharacterControllerMasterTestBuilder.SetupMasterTestScene();

            LogResult(CCS_CharacterControllerMasterTestValidator.ValidateMasterTestScene());

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


