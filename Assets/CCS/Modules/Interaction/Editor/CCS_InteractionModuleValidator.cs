using System.Collections.Generic;
using System.IO;
using CCS.Modules.Interaction;
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
// NOTES: v0.5.4 validates pickup/door flow, forward volume, and Master Test wiring.
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

            ValidateMasterTestSceneDetectionCube(failures);
            ValidateSourceContracts(failures);

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass(
                    "Interaction module foundation, scanner profile, test prefabs, and scene wiring are valid.");
        }

        #endregion

        #region Private Methods

        private static void ValidateMasterTestSceneDetectionCube(List<string> failures)
        {
            Scene scene = EditorSceneManager.OpenScene(
                CCS_InteractionConstants.MasterTestScenePath,
                OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                failures.Add($"Could not open master test scene at {CCS_InteractionConstants.MasterTestScenePath}.");
                return;
            }

            CCS_TestPickupItemSpawner[] spawners =
                Object.FindObjectsByType<CCS_TestPickupItemSpawner>(FindObjectsSortMode.None);
            int spawnerCount = 0;
            for (int i = 0; i < spawners.Length; i++)
            {
                if (spawners[i] != null && spawners[i].gameObject.scene == scene)
                {
                    spawnerCount++;
                }
            }

            AppendIfMissing(
                failures,
                spawnerCount == 0,
                $"Master test scene must not contain {nameof(CCS_TestPickupItemSpawner)} when using the baked detection cube.");

            GameObject detectionCube = FindDetectionCubeInScene(scene);
            AppendIfMissing(
                failures,
                detectionCube != null,
                $"Master test scene must contain {CCS_InteractionConstants.TestDetectionCubeObjectName}.");

            if (detectionCube == null)
            {
                return;
            }

            int interactableLayer = LayerMask.NameToLayer(CCS_InteractionConstants.InteractableLayerName);
            AppendIfMissing(
                failures,
                detectionCube.activeInHierarchy,
                $"{CCS_InteractionConstants.TestDetectionCubeObjectName} must be active.");
            AppendIfMissing(
                failures,
                detectionCube.layer == interactableLayer,
                $"{CCS_InteractionConstants.TestDetectionCubeObjectName} must use the Interactable layer.");
            AppendIfMissing(
                failures,
                detectionCube.CompareTag(CCS_InteractionConstants.InteractableTagName),
                $"{CCS_InteractionConstants.TestDetectionCubeObjectName} must use the Interactable tag.");

            CCS_InteractableLabelTarget labelTarget = detectionCube.GetComponent<CCS_InteractableLabelTarget>();
            AppendIfMissing(
                failures,
                labelTarget != null,
                $"{CCS_InteractionConstants.TestDetectionCubeObjectName} must include {nameof(CCS_InteractableLabelTarget)}.");
            AppendIfMissing(
                failures,
                detectionCube.GetComponent<CCS_InteractableExecutor>() != null,
                $"{CCS_InteractionConstants.TestDetectionCubeObjectName} must include {nameof(CCS_InteractableExecutor)}.");
            AppendIfMissing(
                failures,
                detectionCube.GetComponent<BoxCollider>() != null,
                $"{CCS_InteractionConstants.TestDetectionCubeObjectName} must include a BoxCollider.");
        }

        private static GameObject FindDetectionCubeInScene(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
                for (int j = 0; j < transforms.Length; j++)
                {
                    Transform candidate = transforms[j];
                    if (candidate != null
                        && candidate.name == CCS_InteractionConstants.TestDetectionCubeObjectName
                        && candidate.gameObject.scene == scene)
                    {
                        return candidate.gameObject;
                    }
                }
            }

            return null;
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
