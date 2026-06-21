using System.Collections.Generic;
using System.IO;
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

            GameObject pickupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_InteractionConstants.TestPickupInteractablePrefabPath);
            AppendIfMissing(
                failures,
                pickupPrefab != null,
                $"Missing test pickup prefab at {CCS_InteractionConstants.TestPickupInteractablePrefabPath}.");
            AppendResult(
                failures,
                CCS_InteractionValidationUtility.ValidateTestPickupInteractablePrefab(pickupPrefab));

            ValidateMasterTestScenePickupSpawner(failures);
            ValidateSourceContracts(failures);

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass(
                    "Interaction module foundation, scanner profile, test prefabs, and scene wiring are valid.");
        }

        #endregion

        #region Private Methods

        private static void ValidateMasterTestScenePickupSpawner(List<string> failures)
        {
            Scene scene = EditorSceneManager.OpenScene(
                CCS_InteractionConstants.MasterTestScenePath,
                OpenSceneMode.Additive);
            if (!scene.IsValid())
            {
                failures.Add($"Could not open master test scene at {CCS_InteractionConstants.MasterTestScenePath}.");
                return;
            }

            CCS_TestPickupItemSpawner spawner = Object.FindAnyObjectByType<CCS_TestPickupItemSpawner>();
            GameObject assignedPrefab = null;
            Transform assignedOrigin = null;
            if (spawner != null)
            {
                SerializedObject serializedSpawner = new SerializedObject(spawner);
                SerializedProperty prefabProperty = serializedSpawner.FindProperty("pickupItemPrefab");
                SerializedProperty originProperty = serializedSpawner.FindProperty("spawnOrigin");
                assignedPrefab = prefabProperty != null ? prefabProperty.objectReferenceValue as GameObject : null;
                assignedOrigin = originProperty != null ? originProperty.objectReferenceValue as Transform : null;
            }

            EditorSceneManager.CloseScene(scene, true);
            AppendIfMissing(
                failures,
                spawner != null,
                $"Master test scene must contain {nameof(CCS_TestPickupItemSpawner)}.");
            AppendIfMissing(
                failures,
                assignedPrefab != null,
                "Master test pickup spawner must reference PF_CCS_TestInteractable_PickupItem.");
            AppendIfMissing(
                failures,
                assignedPrefab != null
                && AssetDatabase.GetAssetPath(assignedPrefab) == CCS_InteractionConstants.TestPickupInteractablePrefabPath,
                $"Master test pickup spawner must reference {CCS_InteractionConstants.TestPickupInteractablePrefabPath}.");
            AppendIfMissing(
                failures,
                assignedOrigin != null,
                "Master test pickup spawner must reference TP_Spawn_Host as spawn origin.");
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
                AppendIfMissing(
                    failures,
                    source.Contains("Keyboard.current"),
                    "CCS_NetworkInteractionScanner must use the Input System keyboard.");
            }

            string pickupPath = CCS_InteractionConstants.ModuleRootPath
                + "/Runtime/Components/CCS_TestPickupInteractable.cs";
            if (File.Exists(pickupPath))
            {
                string source = File.ReadAllText(pickupPath);
                AppendIfMissing(
                    failures,
                    source.Contains("RequireComponent(typeof(BoxCollider))"),
                    "CCS_TestPickupInteractable must require BoxCollider.");
                AppendIfMissing(
                    failures,
                    source.Contains("CCS_IInteractable"),
                    "CCS_TestPickupInteractable must implement CCS_IInteractable.");
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
