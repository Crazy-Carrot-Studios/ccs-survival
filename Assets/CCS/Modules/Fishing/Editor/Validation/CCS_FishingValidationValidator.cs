using System.Collections.Generic;
using System.IO;
using CCS.Modules.Fishing;
using CCS.Modules.Inventory;
using CCS.Modules.Resources;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_FishingValidationValidator
// CATEGORY: Modules / Fishing / Editor / Validation
// PURPOSE: Validates fishing module layout, assets, bootstrap wiring, and catch tables.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Milestone 1.2.5 fishing foundation validation.
// =============================================================================

namespace CCS.Modules.Fishing.Editor
{
    public sealed class CCS_FishingValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/Fishing";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultProfilePath = SurvivalRoot + "/Profiles/Fishing/CCS_DefaultFishingProfile.asset";
        private const string FishingPolePath = SurvivalRoot + "/Content/Items/Tools/Fishing/CCS_Item_FishingPole.asset";
        private const string TestSpotPrefabPath = SurvivalRoot + "/Prefabs/Fishing/PF_CCS_TestFishingSpot.prefab";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Fishing_Module.md";
        private const string BootstrapScenePath = SurvivalRoot + "/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string BootstrapPrefabPath = SurvivalRoot + "/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string CompositionRegistrationPath =
            SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string FishingPoleItemId = "ccs.survival.item.tool.fishingpole";

        public string ValidatorId => "ccs.survival.validation.fishing";

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Fishing", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Definitions", RuntimeRoot + "/Definitions");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Spots", RuntimeRoot + "/Spots");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Fishing.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Fishing.Editor.asmdef");

            ValidateRequiredScript(report, "CCS_FishingProfile", RuntimeRoot + "/Profiles/CCS_FishingProfile.cs");
            ValidateRequiredScript(report, "CCS_FishingService", RuntimeRoot + "/Services/CCS_FishingService.cs");
            ValidateRequiredScript(report, "CCS_FishingRuntimeBridge", RuntimeRoot + "/Services/CCS_FishingRuntimeBridge.cs");
            ValidateRequiredScript(report, "CCS_FishingSpot", RuntimeRoot + "/Spots/CCS_FishingSpot.cs");
            ValidateRequiredScript(report, "CCS_FishingValidationUtility", RuntimeRoot + "/Utilities/CCS_FishingValidationUtility.cs");

            ValidateDocumentationAsset(report, "Fishing Module Doc", ModuleDocPath);
            ValidateRequiredAsset(report, "Default Fishing Profile", DefaultProfilePath);
            ValidateFishingProfile(report);
            ValidateFishingPoleItem(report);
            ValidateCompositionRegistration(report);
            ValidateBootstrapHost(report);
            ValidateTestSpotPrefab(report);
            ValidateBootstrapSceneSpot(report);
            ValidateActiveItemBinding(report);
            ValidateRuntimeScriptsAvoidUnityEditor(report, RuntimeRoot);
        }

        private static void ValidateFishingProfile(CCS_SurvivalValidationReport report)
        {
            CCS_FishingProfile profile = AssetDatabase.LoadAssetAtPath<CCS_FishingProfile>(DefaultProfilePath);
            if (profile == null)
            {
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_FishingValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Default Fishing Profile Validation",
                validation.Message);

            if (profile.ProfileVersion != "1.2.5")
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Fishing Profile Version",
                    $"Expected profileVersion 1.2.5 but found '{profile.ProfileVersion}'.");
            }
        }

        private static void ValidateFishingPoleItem(CCS_SurvivalValidationReport report)
        {
            CCS_ItemDefinition pole = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(FishingPolePath);
            if (pole == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Fishing Pole Item",
                    $"Missing fishing pole item at {FishingPolePath}. Run CCS_FishingBootstrapSetup.ExecuteBatch.");
                return;
            }

            if (pole.ItemId != FishingPoleItemId)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Fishing Pole Item Id",
                    $"Expected {FishingPoleItemId} but found {pole.ItemId}.");
            }

            if (!CCS_FishingValidationUtility.IsFishingPoleItemDefinition(pole))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Fishing Pole Tool Type",
                    "Fishing pole must resolve to CCS_ItemToolType.FishingPole.");
            }

            if (!CCS_ItemGameplayUtility.IsToolItem(pole))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Fishing Pole Classification",
                    "Fishing pole must be classified as a Tool item.");
            }
        }

        private static void ValidateCompositionRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(CompositionRegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Composition Registration File",
                    $"Missing {CompositionRegistrationPath}.");
                return;
            }

            string source = File.ReadAllText(CompositionRegistrationPath);
            bool hasFishing = source.Contains("CCS_FishingService")
                && source.Contains("fishingProfile")
                && source.Contains("BindFishingService");
            report.AddIssue(
                hasFishing
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Fishing Service Registration",
                hasFishing
                    ? "CCS_SurvivalGameplayServiceRegistration registers and binds fishing service."
                    : "Composition registration is missing fishing service wiring.");
        }

        private static void ValidateBootstrapHost(CCS_SurvivalValidationReport report)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefab == null)
            {
                return;
            }

            CCS_SurvivalGameplayServiceHost host = prefab.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (host == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Gameplay Host",
                    "Bootstrap prefab is missing CCS_SurvivalGameplayServiceHost.");
                return;
            }

            SerializedObject serializedHost = new SerializedObject(host);
            Object fishingProfile = serializedHost.FindProperty("fishingProfile").objectReferenceValue;
            report.AddIssue(
                fishingProfile != null
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Bootstrap Fishing Profile",
                fishingProfile != null
                    ? "Bootstrap gameplay host has fishingProfile assigned."
                    : "Bootstrap gameplay host is missing fishingProfile assignment.");
        }

        private static void ValidateTestSpotPrefab(CCS_SurvivalValidationReport report)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TestSpotPrefabPath);
            if (prefab == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Test Fishing Spot Prefab",
                    $"Missing {TestSpotPrefabPath}. Run CCS_FishingBootstrapSetup.ExecuteBatch.");
                return;
            }

            CCS_FishingSpot spot = prefab.GetComponent<CCS_FishingSpot>();
            if (spot == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Test Fishing Spot Component",
                    "PF_CCS_TestFishingSpot must include CCS_FishingSpot.");
                return;
            }

            ValidateSpotDefinitionMetadata(report, spot.SpotDefinition, "Test Fishing Spot Prefab");
        }

        private static void ValidateBootstrapSceneSpot(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(BootstrapScenePath))
            {
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Additive);
            try
            {
                CCS_FishingSpot[] spots = Object.FindObjectsByType<CCS_FishingSpot>();
                bool foundTestSpot = false;
                HashSet<string> spotIds = new HashSet<string>();
                for (int index = 0; index < spots.Length; index++)
                {
                    CCS_FishingSpot spot = spots[index];
                    if (spot == null)
                    {
                        continue;
                    }

                    if (spot.name == "CCS_TestFishingSpot")
                    {
                        foundTestSpot = true;
                    }

                    string spotId = spot.SpotId;
                    if (!string.IsNullOrWhiteSpace(spotId) && !spotIds.Add(spotId))
                    {
                        report.AddIssue(
                            CCS_SurvivalValidationIssueSeverity.Error,
                            "Duplicate Fishing Spot Id",
                            $"Duplicate fishing spot id in bootstrap scene: {spotId}");
                    }

                    ValidateSpotDefinitionMetadata(report, spot.SpotDefinition, spot.name);
                }

                report.AddIssue(
                    foundTestSpot
                        ? CCS_SurvivalValidationIssueSeverity.Info
                        : CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Test Fishing Spot",
                    foundTestSpot
                        ? "Bootstrap scene contains CCS_TestFishingSpot."
                        : "Bootstrap scene is missing CCS_TestFishingSpot. Run CCS_FishingBootstrapSetup.ExecuteBatch.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void ValidateSpotDefinitionMetadata(
            CCS_SurvivalValidationReport report,
            CCS_FishingSpotDefinition definition,
            string label)
        {
            if (definition == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    $"{label} Definition",
                    $"{label} is missing CCS_FishingSpotDefinition.");
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_FishingValidationUtility.ValidateSpotDefinition(definition);
            report.AddIssue(
                validation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                $"{label} Metadata",
                validation.Message);
        }

        private static void ValidateActiveItemBinding(CCS_SurvivalValidationReport report)
        {
            string activeItemServicePath =
                "Assets/CCS/Modules/Hotbar/Runtime/ActiveItem/CCS_ActiveItemService.cs";
            if (!File.Exists(activeItemServicePath))
            {
                return;
            }

            string source = File.ReadAllText(activeItemServicePath);
            bool bound = source.Contains("BindFishingService") && source.Contains("TryUseFishingPole");
            report.AddIssue(
                bound
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Active Item Fishing Routing",
                bound
                    ? "CCS_ActiveItemService binds and routes fishing pole use."
                    : "CCS_ActiveItemService is missing fishing routing.");
        }

        private static void ValidateRequiredFolder(
            CCS_SurvivalValidationReport report,
            string label,
            string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    $"Missing Folder: {label}",
                    path);
            }
        }

        private static void ValidateRequiredFile(
            CCS_SurvivalValidationReport report,
            string label,
            string path)
        {
            if (!File.Exists(path))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    $"Missing File: {label}",
                    path);
            }
        }

        private static void ValidateRequiredScript(
            CCS_SurvivalValidationReport report,
            string label,
            string path)
        {
            if (!File.Exists(path))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    $"Missing Script: {label}",
                    path);
            }
        }

        private static void ValidateDocumentationAsset(
            CCS_SurvivalValidationReport report,
            string label,
            string path)
        {
            if (!File.Exists(path))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    $"Missing Documentation: {label}",
                    path);
            }
        }

        private static void ValidateRequiredAsset(
            CCS_SurvivalValidationReport report,
            string label,
            string path)
        {
            if (AssetDatabase.LoadAssetAtPath<Object>(path) == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    $"Missing Asset: {label}",
                    $"{path}. Run CCS_FishingBootstrapSetup.ExecuteBatch.");
            }
        }

        private static void ValidateRuntimeScriptsAvoidUnityEditor(
            CCS_SurvivalValidationReport report,
            string runtimeRoot)
        {
            if (!Directory.Exists(runtimeRoot))
            {
                return;
            }

            string[] files = Directory.GetFiles(runtimeRoot, "*.cs", SearchOption.AllDirectories);
            for (int index = 0; index < files.Length; index++)
            {
                string file = files[index].Replace('\\', '/');
                string contents = File.ReadAllText(file);
                if (contents.Contains("using UnityEditor"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Fishing Runtime Editor Reference",
                        $"{file} must not reference UnityEditor.");
                }
            }
        }
    }
}
