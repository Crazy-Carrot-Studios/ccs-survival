using System.Collections.Generic;
using System.IO;
using CCS.Project;
using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_InteractionValidationUtility
// CATEGORY: Modules / Interaction / Runtime / Validation
// PURPOSE: Runtime validation helpers for the Interaction module foundation.
// PLACEMENT: Called from editor validators and future module installers.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.4.0 validates scanner profile, player wiring, and test interactable assets.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public static class CCS_InteractionValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateModuleFoundation()
        {
            List<string> failures = new List<string>();

            AppendIfMissing(
                failures,
                Directory.Exists(CCS_InteractionConstants.ModuleRootPath + "/Runtime"),
                "Missing Interaction Runtime folder.");
            AppendIfMissing(
                failures,
                Directory.Exists(CCS_InteractionConstants.ModuleRootPath + "/Editor"),
                "Missing Interaction Editor folder.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_InteractionConstants.ModuleRootPath + "/Runtime/CCS.Modules.Interaction.Runtime.asmdef"),
                "Missing CCS.Modules.Interaction.Runtime.asmdef.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_InteractionConstants.ModuleRootPath + "/Editor/CCS.Modules.Interaction.Editor.asmdef"),
                "Missing CCS.Modules.Interaction.Editor.asmdef.");

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Interaction module foundation folders and asmdefs are present.");
        }

        public static CCS_SurvivalValidationResult ValidateScannerProfile(CCS_InteractionScannerProfile profile)
        {
            List<string> failures = new List<string>();
            AppendIfMissing(failures, profile != null, "Scanner profile asset is missing.");

            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            CCS_SurvivalValidationResult profileValidation =
                CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            AppendIfMissing(failures, profileValidation.IsSuccess, profileValidation.Message);
            AppendIfMissing(
                failures,
                profile.ProfileId == CCS_InteractionConstants.ScannerProfileId,
                $"Scanner profileId must be {CCS_InteractionConstants.ScannerProfileId}.");
            AppendIfMissing(
                failures,
                Mathf.Approximately(profile.InteractionRange, 3f),
                "Scanner interactionRange must be 3 meters.");

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Interaction scanner profile is valid.");
        }

        public static CCS_SurvivalValidationResult ValidatePlayerScannerComponents(GameObject prefabRoot)
        {
            List<string> failures = new List<string>();
            AppendIfMissing(failures, prefabRoot != null, "Canonical test player prefab is missing.");

            if (prefabRoot == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            CCS_NetworkInteractionScanner scanner = prefabRoot.GetComponent<CCS_NetworkInteractionScanner>();
            AppendIfMissing(
                failures,
                scanner != null,
                $"{CCS_InteractionConstants.NetworkedTestPlayerPrefabPath} must contain CCS_NetworkInteractionScanner.");

            if (scanner != null && scanner.ScannerProfile == null)
            {
                failures.Add($"{CCS_InteractionConstants.NetworkedTestPlayerPrefabPath} must assign a scanner profile.");
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Canonical test player interaction scanner is wired.");
        }

        public static CCS_SurvivalValidationResult ValidateTestInteractablePrefab(GameObject prefabRoot)
        {
            List<string> failures = new List<string>();
            AppendIfMissing(failures, prefabRoot != null, "Test interactable prefab is missing.");

            if (prefabRoot == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            AppendIfMissing(
                failures,
                prefabRoot.GetComponent<NetworkObject>() != null,
                $"{CCS_InteractionConstants.TestToggleInteractablePrefabPath} must contain NetworkObject.");
            AppendIfMissing(
                failures,
                prefabRoot.GetComponent<CCS_TestToggleInteractable>() != null,
                $"{CCS_InteractionConstants.TestToggleInteractablePrefabPath} must contain CCS_TestToggleInteractable.");
            AppendIfMissing(
                failures,
                prefabRoot.GetComponentInChildren<Collider>() != null,
                $"{CCS_InteractionConstants.TestToggleInteractablePrefabPath} must contain a collider for scanning.");

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Test interactable prefab is valid.");
        }

        #endregion

        #region Private Methods

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
