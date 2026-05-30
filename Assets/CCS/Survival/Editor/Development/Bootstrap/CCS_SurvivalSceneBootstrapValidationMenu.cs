using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalSceneBootstrapValidationMenu
// CATEGORY: Survival / Editor / Development / Bootstrap
// PURPOSE: Editor menu for validating active scene bootstrap against development profile expectations.
// PLACEMENT: Menu path CCS/Survival/Bootstrap/Validate Active Scene Bootstrap.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Uses development bootstrap validation utility. Safe when no profile is assigned.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public static class CCS_SurvivalSceneBootstrapValidationMenu
    {
        private const string MenuPath = "CCS/Survival/Bootstrap/Validate Active Scene Bootstrap";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string LogPrefix = "CCS_SurvivalSceneBootstrapValidationMenu";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);

            CCS.Survival.Development.CCS_SurvivalSceneBootstrapper bootstrapper =
                Object.FindAnyObjectByType<CCS.Survival.Development.CCS_SurvivalSceneBootstrapper>();

            CCS.Survival.Development.CCS_SurvivalSceneBootstrapProfile profile =
                bootstrapper != null ? bootstrapper.BootstrapProfile : null;

            CCS.Survival.CCS_SurvivalValidationResult validationResult =
                CCS.Survival.Development.CCS_SurvivalSceneBootstrapValidationUtility.ValidateActiveSceneBootstrap(profile);

            if (!validationResult.IsSuccess)
            {
                Debug.LogError($"[{LogPrefix}] {validationResult.Message}");
                EditorApplication.Exit(1);
                return;
            }

            if (validationResult.IsWarning)
            {
                Debug.LogError($"[{LogPrefix}] {validationResult.Message}");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log($"[{LogPrefix}] {validationResult.Message}");
            EditorApplication.Exit(0);
        }

        [MenuItem(MenuPath, priority = 150)]
        public static void ValidateActiveSceneBootstrap()
        {
            CCS.Survival.Development.CCS_SurvivalSceneBootstrapper bootstrapper =
                Object.FindAnyObjectByType<CCS.Survival.Development.CCS_SurvivalSceneBootstrapper>();

            CCS.Survival.Development.CCS_SurvivalSceneBootstrapProfile profile =
                bootstrapper != null ? bootstrapper.BootstrapProfile : null;

            CCS.Survival.CCS_SurvivalValidationResult validationResult =
                CCS.Survival.Development.CCS_SurvivalSceneBootstrapValidationUtility.ValidateActiveSceneBootstrap(profile);

            if (!validationResult.IsSuccess)
            {
                Debug.LogError(
                    $"[CCS_SurvivalSceneBootstrapValidationMenu] {validationResult.Message}");
                EditorUtility.DisplayDialog(
                    "Scene Bootstrap Validation",
                    validationResult.Message,
                    "OK");
                return;
            }

            if (validationResult.IsWarning)
            {
                Debug.LogWarning(
                    $"[CCS_SurvivalSceneBootstrapValidationMenu] {validationResult.Message}");
            }
            else
            {
                Debug.Log(
                    $"[CCS_SurvivalSceneBootstrapValidationMenu] {validationResult.Message}");
            }

            EditorUtility.DisplayDialog(
                "Scene Bootstrap Validation",
                validationResult.Message,
                "OK");
        }

        #endregion
    }
}
