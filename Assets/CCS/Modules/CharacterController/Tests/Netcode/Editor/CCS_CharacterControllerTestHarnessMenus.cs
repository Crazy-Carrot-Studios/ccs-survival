using CCS.Project;

using UnityEditor;

using UnityEngine;



// =============================================================================

// SCRIPT: CCS_CharacterControllerTestHarnessMenus

// CATEGORY: Modules / CharacterController / Tests / Netcode / Editor

// PURPOSE: Registers multiplayer hosting scene setup and validation menus.

// PLACEMENT: Editor-only static registration. Not attached to GameObjects.

// AUTHOR: James Schilz

// CREATED: 2026-06-07

// NOTES: Setup runs builder only. Validator reports problems only.

// =============================================================================



namespace CCS.Modules.CharacterController.Tests.Netcode.Editor

{

    public static class CCS_CharacterControllerTestHarnessMenus

    {

        private const string SceneMenuRoot = "CCS/Modules/Character Controller/Scene/";



        #region Public Methods



        [MenuItem(SceneMenuRoot + "Rebuild Multiplayer Hosting UI")]
        public static void RebuildMultiplayerHostingUiMenu()
        {
            if (CCS_MultiplayerHostingSceneLayoutEditor.BuildOrRebuildLayout())
            {
                Debug.Log("[Hosting Layout] UI rebuild complete.");
            }
        }

        [MenuItem(SceneMenuRoot + "Setup Multiplayer Hosting Scene")]

        public static void SetupMultiplayerHostingSceneMenu()

        {

            CCS_MultiplayerHostingBuilder.VerifyAndRepairScene();

        }



        [MenuItem(SceneMenuRoot + "Validate Multiplayer Hosting Scene")]

        public static void ValidateMultiplayerHostingSceneMenu()

        {

            LogResult(CCS_MultiplayerHostingValidator.ValidateHostingScene());

        }



        public static void RunBatchBuildAndValidate()

        {

            CCS_MultiplayerHostingBuilder.VerifyAndRepairScene();

            LogResult(CCS_MultiplayerHostingValidator.ValidateHostingScene());

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


