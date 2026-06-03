using System.Collections.Generic;
using System.IO;
using CCS.Modules.Economy;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FrontierSettlementValidationValidator
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Validates settlement module layout, content, composition, and bootstrap wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 1.8.1 settlement services polish and blacksmith routing.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public sealed class CCS_FrontierSettlementValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/Settlements";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Settlements_Module.md";
        private const string TradingPostDefinitionPath = "Assets/CCS/Survival/Content/Settlements/CCS_Settlement_TestTradingPost.asset";
        private const string DefaultSettlementProfilePath = "Assets/CCS/Survival/Profiles/Settlements/CCS_DefaultSettlementProfile.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string CompositionRegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string GeneralStoreVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_GeneralStore.asset";
        private const string StableVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_FrontierStable.asset";
        private const string GunsmithVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_FrontierGunsmith.asset";

        public string ValidatorId => "ccs.survival.validation.settlements";

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Settlements", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Components", RuntimeRoot + "/Components");
            ValidateRequiredFolder(report, "Runtime/Definitions", RuntimeRoot + "/Definitions");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Settlements.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Settlements.Editor.asmdef");
            ValidateRequiredScript(report, "CCS_SettlementService", RuntimeRoot + "/Services/CCS_SettlementService.cs");
            ValidateRequiredScript(report, "CCS_SettlementServiceRouteResolver", RuntimeRoot + "/Services/CCS_SettlementServiceRouteResolver.cs");
            ValidateRequiredScript(report, "CCS_SettlementServicePoint", RuntimeRoot + "/Components/CCS_SettlementServicePoint.cs");
            ValidateRequiredScript(report, "CCS_SettlementIndustryServiceHud", RuntimeRoot + "/UI/CCS_SettlementIndustryServiceHud.cs");
            ValidateRequiredScript(report, "CCS_SettlementLocation", RuntimeRoot + "/Components/CCS_SettlementLocation.cs");

            ValidateSettlementContent(report);
            ValidateCompositionRegistration(report);
            ValidateBootstrapWiring(report);
            ValidateSaveSupport(report);
            ValidateDocumentation(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                "Settlements Version",
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion,
                "Run CCS_FrontierSettlementBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalBootstrapVersionUtility.ValidateNoHardcodedBootstrapVersionWrites(report, "Settlements Version");
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(report, "Settlements Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(report, "Settlements Obsolete API Scan");
        }

        private static void ValidateSettlementContent(CCS_SurvivalValidationReport report)
        {
            CCS_SettlementDefinition tradingPost =
                AssetDatabase.LoadAssetAtPath<CCS_SettlementDefinition>(TradingPostDefinitionPath);
            if (tradingPost == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Settlements Content",
                    $"Missing trading post definition: {TradingPostDefinitionPath}");
                return;
            }

            if (tradingPost.SettlementId != CCS_SettlementContentIds.TestTradingPostSettlementId)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Settlements Content",
                    $"Trading post settlementId expected '{CCS_SettlementContentIds.TestTradingPostSettlementId}'.");
            }

            CCS_SettlementProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_SettlementProfile>(DefaultSettlementProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Settlements Profile",
                    $"Missing default settlement profile: {DefaultSettlementProfilePath}");
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_SettlementValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Settlements Profile",
                validation.Message);

            HashSet<string> settlementIds = new HashSet<string>();
            CCS_SettlementDefinition[] definitions = profile.SettlementDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_SettlementDefinition definition = definitions[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.SettlementId))
                {
                    continue;
                }

                if (!settlementIds.Add(definition.SettlementId))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Settlements Profile",
                        $"Duplicate settlement id '{definition.SettlementId}'.");
                }
            }

            ValidateVendorAsset(report, "General Store vendor", GeneralStoreVendorPath);
            ValidateVendorAsset(report, "Stable vendor", StableVendorPath);
            ValidateVendorAsset(report, "Gunsmith vendor", GunsmithVendorPath);
        }

        private static void ValidateVendorAsset(CCS_SurvivalValidationReport report, string context, string path)
        {
            if (AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(path) == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Settlements Vendor Routing",
                    $"Missing vendor asset for {context}: {path}");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Settlements Vendor Routing",
                    $"{context} vendor present: {path}");
            }
        }

        private static void ValidateCompositionRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(CompositionRegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Settlements Composition",
                    $"Missing file: {CompositionRegistrationPath}");
                return;
            }

            string source = File.ReadAllText(CompositionRegistrationPath);
            if (source.Contains("CreateSettlementService") && source.Contains("CCS_SettlementService"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Settlements Composition",
                    "CCS_SettlementService is registered in gameplay composition.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Settlements Composition",
                    "CCS_SettlementService registration is missing.");
            }
        }

        private static void ValidateBootstrapWiring(CCS_SurvivalValidationReport report)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefab == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Settlements Bootstrap",
                    $"Missing prefab: {BootstrapPrefabPath}");
                return;
            }

            CCS_SurvivalGameplayServiceHost host = prefab.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (host == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Settlements Bootstrap",
                    "Bootstrap prefab is missing CCS_SurvivalGameplayServiceHost.");
                return;
            }

            SerializedObject serializedHost = new SerializedObject(host);
            if (serializedHost.FindProperty("settlementProfile").objectReferenceValue == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Settlements Bootstrap",
                    "Gameplay service host settlementProfile is not assigned.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Settlements Bootstrap",
                    "Gameplay service host references settlement profile.");
            }

            string sceneText = File.Exists(BootstrapScenePath) ? File.ReadAllText(BootstrapScenePath) : string.Empty;
            if (sceneText.Contains(CCS_SettlementContentIds.TestTradingPostObjectName))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Settlements Bootstrap",
                    "Bootstrap scene contains CCS_TestTradingPost.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Settlements Bootstrap",
                    "Bootstrap scene is missing CCS_TestTradingPost.");
            }

            ValidateSceneServicePointIds(report, sceneText);
            ValidateBlacksmithRouting(report, sceneText);
            ValidatePlaytestSettlementSteps(report);
        }

        private static void ValidateBlacksmithRouting(CCS_SurvivalValidationReport report, string sceneText)
        {
            if (sceneText.Contains("Blacksmith services coming in a future industry milestone"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Settlements Blacksmith Routing",
                    "Blacksmith service point still uses orphan placeholder message. Run settlement bootstrap.");
            }

            if (!sceneText.Contains(CCS_SettlementContentIds.BlacksmithServicePointId))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Settlements Blacksmith Routing",
                    "Bootstrap scene is missing blacksmith service point id.");
                return;
            }

            int industryRouteIndex = (int)CCS_SettlementServiceRouteType.Industry;
            string industryRouteToken = "routeOverride: " + industryRouteIndex;
            int blacksmithIndex = sceneText.IndexOf(CCS_SettlementContentIds.BlacksmithServicePointId, System.StringComparison.Ordinal);
            int routeIndex = sceneText.IndexOf(industryRouteToken, blacksmithIndex, System.StringComparison.Ordinal);
            if (blacksmithIndex >= 0 && routeIndex >= blacksmithIndex && routeIndex - blacksmithIndex < 512)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Settlements Blacksmith Routing",
                    "Blacksmith service point routes to industry context.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Settlements Blacksmith Routing",
                    "Blacksmith route override may be missing. Run settlement bootstrap.");
            }
        }

        private static void ValidatePlaytestSettlementSteps(CCS_SurvivalValidationReport report)
        {
            const string playtestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
            CCS.Modules.Playtesting.CCS_PlaytestProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS.Modules.Playtesting.CCS_PlaytestProfile>(playtestProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Settlements Playtest",
                    $"Missing playtest profile: {playtestProfilePath}");
                return;
            }

            bool hasBlacksmithInteract = false;
            bool hasBlacksmithRouting = false;
            System.Collections.Generic.IReadOnlyList<CCS.Modules.Playtesting.CCS_PlaytestStepDefinition> steps =
                profile.StepDefinitions;
            for (int index = 0; index < steps.Count; index++)
            {
                CCS.Modules.Playtesting.CCS_PlaytestStepDefinition step = steps[index];
                if (step == null)
                {
                    continue;
                }

                if (step.StepType == CCS.Modules.Playtesting.CCS_PlaytestStepType.InteractBlacksmithServicePoint)
                {
                    hasBlacksmithInteract = true;
                }

                if (step.StepType == CCS.Modules.Playtesting.CCS_PlaytestStepType.VerifySettlementBlacksmithRouting)
                {
                    hasBlacksmithRouting = true;
                }
            }

            if (hasBlacksmithInteract && hasBlacksmithRouting)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Settlements Playtest",
                    "Settlement playtest includes blacksmith activation and routing verification.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Settlements Playtest",
                    "Settlement playtest is missing blacksmith steps. Run settlement bootstrap.");
            }
        }

        private static void ValidateSceneServicePointIds(CCS_SurvivalValidationReport report, string sceneText)
        {
            HashSet<string> servicePointIds = new HashSet<string>();
            string[] requiredIds =
            {
                CCS_SettlementContentIds.GeneralStoreServicePointId,
                CCS_SettlementContentIds.StableServicePointId,
                CCS_SettlementContentIds.GunsmithServicePointId,
                CCS_SettlementContentIds.BlacksmithServicePointId,
                CCS_SettlementContentIds.BankServicePointId,
                CCS_SettlementContentIds.LandOfficeServicePointId
            };

            for (int index = 0; index < requiredIds.Length; index++)
            {
                string servicePointId = requiredIds[index];
                if (!sceneText.Contains(servicePointId))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Warning,
                        "Settlements Service Points",
                        $"Bootstrap scene may be missing service point id '{servicePointId}'. Run settlement bootstrap.");
                    continue;
                }

                if (!servicePointIds.Add(servicePointId))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Settlements Service Points",
                        $"Duplicate service point id '{servicePointId}' in bootstrap scene.");
                }
            }
        }

        private static void ValidateSaveSupport(CCS_SurvivalValidationReport report)
        {
            const string saveDataPath = "Assets/CCS/Modules/SaveSystem/Runtime/Data/CCS_SaveData.cs";
            if (!File.Exists(saveDataPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Settlements Save",
                    $"Missing save data file: {saveDataPath}");
                return;
            }

            string saveDataSource = File.ReadAllText(saveDataPath);
            if (saveDataSource.Contains("CCS_SaveSettlementsWorldData"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Settlements Save",
                    "Unified save payload includes settlement discovery data.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Settlements Save",
                    "CCS_SaveData is missing CCS_SaveSettlementsWorldData.");
            }
        }

        private static void ValidateDocumentation(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(ModuleDocPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Settlements Documentation",
                    $"Missing module documentation: {ModuleDocPath}");
                return;
            }

            string documentation = File.ReadAllText(ModuleDocPath);
            if (documentation.Contains("Settlement Service Hub Loop")
                && documentation.Contains("CCS_TestTradingPost")
                && documentation.Contains("Blacksmith"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Settlements Documentation",
                    "Settlement module documentation validated.");
            }
            else if (documentation.Contains("Discover Trading Post") && documentation.Contains("CCS_TestTradingPost"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Settlements Documentation",
                    "Settlement module documentation validated.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Settlements Documentation",
                    "Settlement module documentation missing frontier loop details.");
            }
        }

        private static void ValidateRequiredFolder(
            CCS_SurvivalValidationReport report,
            string context,
            string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    $"Folder present: {folderPath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                context,
                $"Missing folder: {folderPath}");
        }

        private static void ValidateRequiredFile(
            CCS_SurvivalValidationReport report,
            string context,
            string filePath)
        {
            if (File.Exists(filePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    $"File present: {filePath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                context,
                $"Missing file: {filePath}");
        }

        private static void ValidateRequiredScript(
            CCS_SurvivalValidationReport report,
            string context,
            string scriptPath)
        {
            ValidateRequiredFile(report, context, scriptPath);
        }
    }
}
