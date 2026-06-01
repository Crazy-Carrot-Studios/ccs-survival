using System.Collections.Generic;
using System.IO;
using CCS.Modules.Gathering;
using CCS.Modules.Resources;
using CCS.Modules.WorldResources;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ResourceFrameworkValidationValidator
// CATEGORY: Modules / Resources / Editor / Validation
// PURPOSE: Validates frontier resource framework enums, metadata, and bootstrap content.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Milestone 1.2.4 frontier resource framework audit.
// =============================================================================

namespace CCS.Modules.Resources.Editor
{
    public sealed class CCS_ResourceFrameworkValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/Resources";
        private const string DefinitionsRoot = ModuleRoot + "/Runtime/Definitions";
        private const string FrontierWorldResourcesRoot = "Assets/CCS/Survival/Profiles/WorldResources/Frontier";
        private const string GatheringProfilePath = "Assets/CCS/Survival/Profiles/Gathering/CCS_DefaultGatheringProfile.asset";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Resource_Framework.md";

        private static readonly string[] RequiredFrontierResourceIds =
        {
            "ccs.survival.resource.frontier.tree",
            "ccs.survival.resource.frontier.stoneoutcrop",
            "ccs.survival.resource.frontier.orevein",
            "ccs.survival.resource.frontier.coalvein",
            "ccs.survival.resource.frontier.fiberplant",
            "ccs.survival.resource.frontier.claydeposit",
            "ccs.survival.resource.frontier.salvage.wagon",
            "ccs.survival.resource.frontier.watersource"
        };

        public string ValidatorId => "ccs.survival.validation.resourceframework";

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFile(report, "ResourceSourceType", DefinitionsRoot + "/CCS_ResourceSourceType.cs");
            ValidateRequiredFile(report, "HarvestMethodType", DefinitionsRoot + "/CCS_HarvestMethodType.cs");
            ValidateRequiredFile(
                report,
                "HarvestMethodToolRulesUtility",
                ModuleRoot + "/Runtime/Utilities/CCS_HarvestMethodToolRulesUtility.cs");
            ValidateDocumentation(report);

            CCS_SurvivalValidationResult fishReserved = ValidateFishFoundationRouting();
            report.AddIssue(
                fishReserved.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Fish Harvest Method",
                fishReserved.Message);

            ValidateGatheringProfileMetadata(report);
            ValidateFrontierWorldResourceDefinitions(report);

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Resource framework validator completed (1.2.5 fishing foundation).");
        }

        private static CCS_SurvivalValidationResult ValidateFishFoundationRouting()
        {
            if (!CCS_HarvestMethodToolRulesUtility.IsHarvestMethodImplemented(CCS_HarvestMethodType.Fish))
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Fish harvest method must be implemented for fishing foundation (1.2.5).");
            }

            if (CCS_HarvestMethodToolRulesUtility.IsHarvestMethodImplementedForGatheringRouting(
                    CCS_HarvestMethodType.Fish))
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Fish must not route through generic gathering harvest (use CCS_FishingService).");
            }

            return CCS_SurvivalValidationResult.Pass(
                "Fish harvest method routes through fishing foundation, not gathering harvest.");
        }

        private static void ValidateGatheringProfileMetadata(CCS_SurvivalValidationReport report)
        {
            CCS_GatheringProfile profile = AssetDatabase.LoadAssetAtPath<CCS_GatheringProfile>(GatheringProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Gathering Profile Metadata",
                    $"Missing gathering profile: {GatheringProfilePath}");
                return;
            }

            CCS_SurvivalValidationResult profileValidation = CCS_GatheringValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                profileValidation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Gathering Profile Metadata",
                profileValidation.Message);

            if (!profile.TryGetNodeRewardSettings(CCS_GatheringNodeType.Tree, out CCS_GatheringNodeRewardSettings treeSettings))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Frontier Gathering Tree",
                    "Tree gathering node metadata is missing from the default gathering profile.");
                return;
            }

            if (treeSettings.rewards == null || treeSettings.rewards.Length < 2)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Frontier Gathering Tree",
                    "Tree gathering rewards must support multi-drop yields.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Frontier Gathering Tree",
                    $"Tree node has {treeSettings.rewards.Length} configured reward drops.");
            }
        }

        private static void ValidateFrontierWorldResourceDefinitions(CCS_SurvivalValidationReport report)
        {
            if (!Directory.Exists(FrontierWorldResourcesRoot))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Frontier World Resource Definitions",
                    $"Missing folder: {FrontierWorldResourcesRoot}");
                return;
            }

            List<string> discoveredIds = new List<string>();
            for (int index = 0; index < RequiredFrontierResourceIds.Length; index++)
            {
                string requiredId = RequiredFrontierResourceIds[index];
                CCS_ResourceDefinition definition = FindResourceDefinitionById(requiredId);
                if (definition == null)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Frontier World Resource Definitions",
                        $"Missing frontier resource definition for id: {requiredId}");
                    continue;
                }

                discoveredIds.Add(definition.ResourceId);
                CCS_SurvivalValidationResult definitionValidation =
                    CCS_WorldResourceValidationUtility.ValidateResourceDefinition(definition);
                report.AddIssue(
                    definitionValidation.IsSuccess
                        ? CCS_SurvivalValidationIssueSeverity.Info
                        : CCS_SurvivalValidationIssueSeverity.Error,
                    $"Frontier Resource ({definition.DisplayName})",
                    definitionValidation.Message);
            }

            CCS_SurvivalValidationResult uniqueValidation =
                CCS_ResourceFrameworkValidationUtility.ValidateUniqueIds(
                    discoveredIds,
                    "Frontier world resource definitions");
            report.AddIssue(
                uniqueValidation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Frontier Resource IDs",
                uniqueValidation.Message);
        }

        private static CCS_ResourceDefinition FindResourceDefinitionById(string resourceId)
        {
            string[] guids = AssetDatabase.FindAssets("t:CCS_ResourceDefinition", new[] { FrontierWorldResourcesRoot });
            for (int index = 0; index < guids.Length; index++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[index]);
                CCS_ResourceDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_ResourceDefinition>(path);
                if (definition != null && definition.ResourceId == resourceId)
                {
                    return definition;
                }
            }

            return null;
        }

        private static void ValidateDocumentation(CCS_SurvivalValidationReport report)
        {
            if (File.Exists(ModuleDocPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Resource Framework Documentation",
                    $"Documentation present: {ModuleDocPath}");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Resource Framework Documentation",
                    $"Missing documentation: {ModuleDocPath}");
            }
        }

        private static void ValidateRequiredFile(CCS_SurvivalValidationReport report, string context, string filePath)
        {
            if (File.Exists(filePath))
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Info, context, $"File present: {filePath}");
                return;
            }

            report.AddIssue(CCS_SurvivalValidationIssueSeverity.Error, context, $"Missing required file: {filePath}");
        }
    }
}
