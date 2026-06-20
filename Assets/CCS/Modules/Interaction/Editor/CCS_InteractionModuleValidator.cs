using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController.Tests.Netcode;
using CCS.Project;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_InteractionModuleValidator
// CATEGORY: Modules / Interaction / Editor
// PURPOSE: Validates Interaction module foundation, assets, and test integration wiring.
// PLACEMENT: Editor validator invoked from CCS/Interaction/Validate Interaction Module.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.4.0 validates owner scanner flow and server-authoritative interactable path.
// =============================================================================

namespace CCS.Modules.Interaction.Editor
{
    public static class CCS_InteractionModuleValidator
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateInteractionModule()
        {
            List<string> failures = new List<string>();

            CCS_SurvivalValidationResult foundationResult =
                CCS_InteractionValidationUtility.ValidateModuleFoundation();
            AppendResult(failures, foundationResult);

            CCS_InteractionScannerProfile scannerProfile = AssetDatabase.LoadAssetAtPath<CCS_InteractionScannerProfile>(
                CCS_InteractionConstants.ScannerProfilePath);
            AppendIfMissing(
                failures,
                File.Exists(CCS_InteractionConstants.ScannerProfilePath),
                $"Missing scanner profile asset at {CCS_InteractionConstants.ScannerProfilePath}.");
            AppendResult(failures, CCS_InteractionValidationUtility.ValidateScannerProfile(scannerProfile));

            GameObject testPlayerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_InteractionConstants.NetworkedTestPlayerPrefabPath);
            AppendIfMissing(
                failures,
                testPlayerPrefab != null,
                $"Missing networked test player prefab at {CCS_InteractionConstants.NetworkedTestPlayerPrefabPath}.");
            AppendResult(
                failures,
                CCS_InteractionValidationUtility.ValidatePlayerScannerComponents(testPlayerPrefab));

            GameObject togglePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_InteractionConstants.TestToggleInteractablePrefabPath);
            AppendIfMissing(
                failures,
                togglePrefab != null,
                $"Missing test interactable prefab at {CCS_InteractionConstants.TestToggleInteractablePrefabPath}.");
            AppendResult(
                failures,
                CCS_InteractionValidationUtility.ValidateTestInteractablePrefab(togglePrefab));

            ValidateMasterTestSceneInteractable(failures);
            ValidateSourceContracts(failures);

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass(
                    "Interaction module foundation, scanner profile, test prefabs, and scene wiring are valid.");
        }

        #endregion

        #region Private Methods

        private static void ValidateMasterTestSceneInteractable(List<string> failures)
        {
            Scene scene = EditorSceneManager.OpenScene(
                CCS_InteractionConstants.MasterTestScenePath,
                OpenSceneMode.Additive);
            if (!scene.IsValid())
            {
                failures.Add($"Could not open master test scene at {CCS_InteractionConstants.MasterTestScenePath}.");
                return;
            }

            CCS_MasterTestInteractableSpawnController controller =
                Object.FindFirstObjectByType<CCS_MasterTestInteractableSpawnController>();
            GameObject assignedPrefab = null;
            if (controller != null)
            {
                SerializedObject serializedController = new SerializedObject(controller);
                SerializedProperty prefabProperty = serializedController.FindProperty("toggleInteractablePrefab");
                assignedPrefab = prefabProperty != null ? prefabProperty.objectReferenceValue as GameObject : null;
            }

            EditorSceneManager.CloseScene(scene, true);
            AppendIfMissing(
                failures,
                controller != null,
                $"Master test scene must contain {nameof(CCS_MasterTestInteractableSpawnController)}.");
            AppendIfMissing(
                failures,
                assignedPrefab != null,
                "Master test interactable spawn controller must reference PF_CCS_TestInteractable_ToggleCube.");
            AppendIfMissing(
                failures,
                assignedPrefab != null
                && AssetDatabase.GetAssetPath(assignedPrefab) == CCS_InteractionConstants.TestToggleInteractablePrefabPath,
                $"Master test interactable spawn controller must reference {CCS_InteractionConstants.TestToggleInteractablePrefabPath}.");
        }

        private static void ValidateSourceContracts(List<string> failures)
        {
            string scannerPath = CCS_InteractionConstants.ModuleRootPath
                + "/Runtime/Components/CCS_NetworkInteractionScanner.cs";
            if (File.Exists(scannerPath))
            {
                string source = File.ReadAllText(scannerPath);
                AppendIfMissing(
                    failures,
                    source.Contains("SubmitInteractionServerRpc"),
                    "CCS_NetworkInteractionScanner must expose a server-authoritative interaction ServerRpc.");
                AppendIfMissing(
                    failures,
                    source.Contains("IsOwner"),
                    "CCS_NetworkInteractionScanner must gate scanning to the local owner.");
                AppendIfMissing(
                    failures,
                    source.Contains("!NetworkManager.IsListening"),
                    "CCS_NetworkInteractionScanner must support solo/offline without NetworkManager.");
            }

            string togglePath = CCS_InteractionConstants.ModuleRootPath
                + "/Runtime/Components/CCS_TestToggleInteractable.cs";
            if (File.Exists(togglePath))
            {
                string source = File.ReadAllText(togglePath);
                AppendIfMissing(
                    failures,
                    source.Contains("NetworkVariableWritePermission.Server"),
                    "CCS_TestToggleInteractable must use server write permission for replicated state.");
                AppendIfMissing(
                    failures,
                    source.Contains("!IsServer"),
                    "CCS_TestToggleInteractable must reject non-server networked apply.");
            }
        }

        private static void AppendResult(List<string> failures, CCS_SurvivalValidationResult result)
        {
            if (!result.IsSuccess)
            {
                failures.Add(result.Message);
            }
        }

        private static void AppendIfMissing(List<string> failures, bool condition, string message)
        {
            if (!condition)
            {
                failures.Add(message);
            }
        }

        #endregion
    }
}
