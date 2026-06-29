using System.Collections.Generic;
using System.IO;
using CCS.Modules.Attributes.Tests;
using CCS.Modules.CharacterController.Tests;
using CCS.Modules.CharacterController.Tests.Netcode;
using CCS.Project;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_CharacterControllerPhase2DValidationUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Validates Phase 2D test-only separation (v0.7.1f).
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-29
// NOTES: Invoked from Master Test validator and audit batch paths.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerPhase2DValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidatePhase2DSeparation()
        {
            List<string> failures = new List<string>();

            CCS_SurvivalValidationResult phase2C =
                CCS_CharacterControllerPhase2CValidationUtility.ValidatePhase2CAuditFoundation();
            if (!phase2C.IsSuccess)
            {
                failures.Add(phase2C.Message);
            }

            ValidateMasterTestTestingManagerMigration(failures);
            ValidateCompatibilityWrapperRemoved(failures);
            ValidateTestOnlyRootComponentsRemovedFromPrefab(failures);
            ValidateSceneTestOnlyReplacements(failures);
            ValidateNetworkManagerPlayerPrefabReference(failures);

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Character Controller Phase 2D test-only separation validated.");
        }

        private static void ValidateMasterTestTestingManagerMigration(List<string> failures)
        {
            if (!File.Exists(CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath))
            {
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(
                CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath,
                OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                failures.Add("Could not open Master Test scene for Phase 2D validation.");
                return;
            }

            CCS_CharacterControllerTestingManager[] managers =
                Object.FindObjectsByType<CCS_CharacterControllerTestingManager>(FindObjectsSortMode.None);
            AppendIfMissing(
                failures,
                managers.Length == 1,
                "Master Test scene must contain exactly one CCS_CharacterControllerTestingManager (found "
                + managers.Length
                + ").");

            AppendIfMissing(
                failures,
                CCS_CharacterControllerPhase2DMigrationUtility.CountLegacyTestingManagerWrappersInScene(scene) == 0,
                "Master Test scene must not contain CCS_MasterTestSceneTestingManager after Phase 2D migration.");
        }

        private static void ValidateCompatibilityWrapperRemoved(List<string> failures)
        {
            AppendIfMissing(
                failures,
                !File.Exists("Assets/CCS/Modules/CharacterController/Tests/Runtime/CCS_MasterTestSceneTestingManager.cs"),
                "CCS_MasterTestSceneTestingManager compatibility wrapper must be removed after migration.");
        }

        private static void ValidateTestOnlyRootComponentsRemovedFromPrefab(List<string> failures)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (prefab == null)
            {
                failures.Add("Missing networked test player prefab for Phase 2D validation.");
                return;
            }

            AppendIfMissing(
                failures,
                prefab.GetComponent<CCS_TestPlayerOfflineBootstrap>() == null,
                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath
                + " must not contain CCS_TestPlayerOfflineBootstrap after Phase 2D separation.");
            AppendIfMissing(
                failures,
                prefab.GetComponent<CCS_TestPlayerAttributeDebugInput>() == null,
                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath
                + " must not contain CCS_TestPlayerAttributeDebugInput after Phase 2D separation.");
        }

        private static void ValidateSceneTestOnlyReplacements(List<string> failures)
        {
            if (!File.Exists(CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath))
            {
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(
                CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath,
                OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                return;
            }

            GameObject testingManagerObject = null;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] != null && roots[i].name == CCS_ProjectAudioConstants.MasterTestTestingManagerObjectName)
                {
                    testingManagerObject = roots[i];
                    break;
                }
            }

            AppendIfMissing(
                failures,
                testingManagerObject != null,
                "Master Test scene must contain " + CCS_ProjectAudioConstants.MasterTestTestingManagerObjectName + ".");

            if (testingManagerObject == null)
            {
                return;
            }

            AppendIfMissing(
                failures,
                testingManagerObject.GetComponent<CCS_MasterTestPlayerOfflineBootstrapper>() != null,
                "Master Test scene must contain CCS_MasterTestPlayerOfflineBootstrapper on CCS_TestingManager.");
            AppendIfMissing(
                failures,
                testingManagerObject.GetComponent<CCS_TestPlayerAttributeDebugInputRouter>() != null,
                "Master Test scene must contain CCS_TestPlayerAttributeDebugInputRouter on CCS_TestingManager.");

            AppendIfMissing(
                failures,
                File.Exists("Assets/CCS/Modules/CharacterController/Tests/Runtime/Managers/CCS_MasterTestPlayerOfflineBootstrapper.cs"),
                "Missing CCS_MasterTestPlayerOfflineBootstrapper source.");
            AppendIfMissing(
                failures,
                File.Exists("Assets/CCS/Modules/CharacterController/Tests/Runtime/Diagnostics/CCS_TestPlayerAttributeDebugInputRouter.cs"),
                "Missing CCS_TestPlayerAttributeDebugInputRouter source.");
        }

        private static void ValidateNetworkManagerPlayerPrefabReference(List<string> failures)
        {
            CCS_SurvivalValidationResult networkResult =
                CCS_CharacterControllerPlayerPrefabAuditUtility.ValidatePhase2CAuditFoundation();
            if (!networkResult.IsSuccess)
            {
                failures.Add(networkResult.Message);
            }
        }

        private static void AppendIfMissing(List<string> failures, bool condition, string message)
        {
            if (!condition)
            {
                failures.Add(message);
            }
        }
    }
}
