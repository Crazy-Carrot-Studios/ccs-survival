using System.IO;
using CCS.Modules.Equipment;
using CCS.Modules.Inventory;
using CCS.Modules.Wildlife;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using CCS.Survival.Player;
using CCS.Survival.Player.Loadout;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CombatValidationValidator
// CATEGORY: Modules / Combat / Editor / Validation
// PURPOSE: Validates combat module layout, assets, bootstrap wiring, and HUD integration.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Validates primitive melee hunting foundation for milestone 0.9.8.
// =============================================================================

namespace CCS.Modules.Combat.Editor
{
    public sealed class CCS_CombatValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/Combat";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultProfilePath = SurvivalRoot + "/Profiles/Combat/CCS_DefaultCombatProfile.asset";
        private const string KnifeItemPath = SurvivalRoot + "/Content/Items/Starter/CCS_Item_Knife.asset";
        private const string SpearItemPath = SurvivalRoot + "/Content/Items/Starter/CCS_Item_Spear.asset";
        private const string SpearEquipmentPath = SurvivalRoot + "/Content/Equipment/Primitive/CCS_Equipment_Spear.asset";
        private const string StarterLoadoutPath = SurvivalRoot + "/Profiles/StarterLoadout/CCS_DefaultStarterLoadoutProfile.asset";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Combat_Module.md";
        private const string BootstrapScenePath = SurvivalRoot + "/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string BootstrapPrefabPath = SurvivalRoot + "/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string PlayerPrefabPath = SurvivalRoot + "/Prefabs/Player/PF_CCS_Player.prefab";
        private const string CompositionRegistrationPath =
            SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string HudPresentationPath = "Assets/CCS/Modules/UI/Runtime/Services/CCS_HudPresentationService.cs";
        private const string HudWiringPath = "Assets/CCS/Modules/UI/Runtime/Services/CCS_HudGameplayServiceWiring.cs";
        private const string InputProviderPath =
            "Assets/CCS/Modules/CharacterController/Runtime/Input/CCS_CharacterInputActionProvider.cs";

        private static readonly string[] RequiredLivingWildlifeNames =
        {
            "CCS_TestRabbit",
            "CCS_TestDeer"
        };

        #region Properties

        public string ValidatorId => "ccs.survival.validation.combat";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Combat", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Events", RuntimeRoot + "/Events");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");
            ValidateRequiredFolder(report, "Wildlife Health", "Assets/CCS/Modules/Wildlife/Runtime/Health");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Combat.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Combat.Editor.asmdef");

            ValidateRequiredScript(report, "CCS_CombatProfile", RuntimeRoot + "/Profiles/CCS_CombatProfile.cs");
            ValidateRequiredScript(report, "CCS_CombatService", RuntimeRoot + "/Services/CCS_CombatService.cs");
            ValidateRequiredScript(report, "CCS_CombatRuntimeBridge", RuntimeRoot + "/Services/CCS_CombatRuntimeBridge.cs");
            ValidateRequiredScript(report, "CCS_CombatHitResult", RuntimeRoot + "/Data/CCS_CombatHitResult.cs");
            ValidateRequiredScript(report, "CCS_CombatDamageType", RuntimeRoot + "/Data/CCS_CombatDamageType.cs");
            ValidateRequiredScript(report, "CCS_CombatRangeType", RuntimeRoot + "/Data/CCS_CombatRangeType.cs");
            ValidateRequiredScript(report, "CCS_CombatEventArgs", RuntimeRoot + "/Events/CCS_CombatEventArgs.cs");
            ValidateRequiredScript(report, "CCS_CombatEvents", RuntimeRoot + "/Events/CCS_CombatEvents.cs");
            ValidateRequiredScript(report, "CCS_CombatValidationUtility", RuntimeRoot + "/Validation/CCS_CombatValidationUtility.cs");
            ValidateRequiredScript(report, "CCS_WildlifeHealthState", "Assets/CCS/Modules/Wildlife/Runtime/Health/CCS_WildlifeHealthState.cs");
            ValidateRequiredScript(report, "CCS_WildlifeDamageable", "Assets/CCS/Modules/Wildlife/Runtime/Health/CCS_WildlifeDamageable.cs");
            ValidateRequiredScript(report, "CCS_PlayerCombatDriver", SurvivalRoot + "/Runtime/Player/CCS_PlayerCombatDriver.cs");

            ValidateDocumentationAsset(report, "Combat Module Doc", ModuleDocPath);
            ValidateRequiredAsset(report, "Default Combat Profile", DefaultProfilePath);
            ValidateWeaponItems(report);
            ValidateSpearEquipment(report);
            ValidateStarterLoadoutSpear(report);
            ValidateCombatProfileAsset(report);
            ValidateCompositionRegistration(report);
            ValidateBootstrapGameplayServiceHost(report);
            ValidatePlayerCombatDriver(report);
            ValidateBootstrapLivingWildlife(report);
            ValidatePrimaryActionInput(report);
            ValidateHudIntegration(report);
            ValidateRuntimeScriptsAvoidUnityEditor(report, RuntimeRoot);
        }

        #endregion

        #region Private Methods

        private static void ValidateWeaponItems(CCS_SurvivalValidationReport report)
        {
            ValidateMeleeWeaponItem(report, "Knife", KnifeItemPath, 10f, 2f);
            ValidateMeleeWeaponItem(report, "Spear", SpearItemPath, 20f, 3f);
        }

        private static void ValidateMeleeWeaponItem(
            CCS_SurvivalValidationReport report,
            string label,
            string assetPath,
            float expectedDamage,
            float expectedRange)
        {
            CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(assetPath);
            if (item == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    $"{label} Melee Stats",
                    $"Missing item asset: {assetPath}");
                return;
            }

            bool statsValid = Mathf.Approximately(item.MeleeDamage, expectedDamage)
                && Mathf.Approximately(item.MeleeRange, expectedRange);
            report.AddIssue(
                statsValid
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                $"{label} Melee Stats",
                statsValid
                    ? $"{label} melee damage {expectedDamage} and range {expectedRange}m configured."
                    : $"{label} expected damage {expectedDamage} and range {expectedRange}m but found {item.MeleeDamage}/{item.MeleeRange}.");
        }

        private static void ValidateSpearEquipment(CCS_SurvivalValidationReport report)
        {
            CCS_EquipmentItemDefinition equipment =
                AssetDatabase.LoadAssetAtPath<CCS_EquipmentItemDefinition>(SpearEquipmentPath);
            report.AddIssue(
                equipment != null && equipment.ItemDefinition != null
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Spear Equipment",
                equipment != null && equipment.ItemDefinition != null
                    ? "Spear equipment definition references CCS_Item_Spear."
                    : $"Missing or incomplete spear equipment: {SpearEquipmentPath}");
        }

        private static void ValidateStarterLoadoutSpear(CCS_SurvivalValidationReport report)
        {
            CCS_StarterLoadoutProfile loadout =
                AssetDatabase.LoadAssetAtPath<CCS_StarterLoadoutProfile>(StarterLoadoutPath);
            CCS_ItemDefinition spearItem = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(SpearItemPath);
            if (loadout == null || spearItem == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Starter Loadout Spear",
                    "Starter loadout or spear item asset is missing.");
                return;
            }

            bool includesSpear = false;
            CCS_StarterLoadoutEntry[] startingItems = loadout.StartingItems;
            for (int index = 0; index < startingItems.Length; index++)
            {
                CCS_StarterLoadoutEntry entry = startingItems[index];
                if (entry != null && entry.ItemDefinition == spearItem)
                {
                    includesSpear = true;
                    break;
                }
            }

            report.AddIssue(
                includesSpear
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Starter Loadout Spear",
                includesSpear
                    ? "Default starter loadout grants spear for bootstrap hunting verification."
                    : "Default starter loadout must include spear for bootstrap hunt path.");
        }

        private static void ValidateCombatProfileAsset(CCS_SurvivalValidationReport report)
        {
            CCS_CombatProfile profile = AssetDatabase.LoadAssetAtPath<CCS_CombatProfile>(DefaultProfilePath);
            if (profile == null)
            {
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_CombatValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Default Combat Profile Validation",
                validation.Message);

            if (profile.ProfileVersion != "0.9.8")
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Combat Profile Version",
                    $"Expected profileVersion 0.9.8 but found '{profile.ProfileVersion}'.");
            }

            CCS_CombatWildlifeSpeciesSettings rabbitSettings =
                profile.GetSpeciesSettings(CCS_WildlifeAiSpecies.Rabbit);
            CCS_CombatWildlifeSpeciesSettings deerSettings =
                profile.GetSpeciesSettings(CCS_WildlifeAiSpecies.Deer);

            report.AddIssue(
                Mathf.Approximately(rabbitSettings.maxHealth, 20f)
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Rabbit Health",
                "Rabbit max health is 20.");

            report.AddIssue(
                Mathf.Approximately(deerSettings.maxHealth, 50f)
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Deer Health",
                "Deer max health is 50.");
        }

        private static void ValidateCompositionRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(CompositionRegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Combat Service Registration",
                    $"Missing script: {CompositionRegistrationPath}");
                return;
            }

            string source = File.ReadAllText(CompositionRegistrationPath);
            report.AddIssue(
                source.Contains("CreateCombatService")
                    && source.Contains("CCS_CombatService")
                    && source.Contains("combatProfile")
                    && source.Contains("RegisterService(runtimeHost, combatService")
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Combat Service Registration",
                "Gameplay composition registers and initializes CCS_CombatService.");
        }

        private static void ValidateBootstrapGameplayServiceHost(CCS_SurvivalValidationReport report)
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefabRoot == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Combat Profile Wiring",
                    $"Missing prefab: {BootstrapPrefabPath}");
                return;
            }

            CCS_SurvivalGameplayServiceHost host = prefabRoot.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (host == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Combat Profile Wiring",
                    "PF_CCS_Survival_BootstrapRoot is missing CCS_SurvivalGameplayServiceHost.");
                return;
            }

            SerializedObject serializedHost = new SerializedObject(host);
            Object combatProfile = serializedHost.FindProperty("combatProfile").objectReferenceValue;
            report.AddIssue(
                combatProfile != null
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Bootstrap Combat Profile Wiring",
                combatProfile != null
                    ? "Bootstrap gameplay host references CCS_DefaultCombatProfile."
                    : "Bootstrap gameplay host is missing combatProfile assignment.");
        }

        private static void ValidatePlayerCombatDriver(CCS_SurvivalValidationReport report)
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefabRoot == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Player Combat Driver",
                    $"Missing player prefab: {PlayerPrefabPath}");
                return;
            }

            report.AddIssue(
                prefabRoot.GetComponent<CCS_PlayerCombatDriver>() != null
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Player Combat Driver",
                "PF_CCS_Player includes CCS_PlayerCombatDriver for primary melee attacks.");
        }

        private static void ValidateBootstrapLivingWildlife(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(BootstrapScenePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Scene",
                    $"Missing scene: {BootstrapScenePath}");
                return;
            }

            string sceneText = File.ReadAllText(BootstrapScenePath);
            for (int index = 0; index < RequiredLivingWildlifeNames.Length; index++)
            {
                string objectName = RequiredLivingWildlifeNames[index];
                report.AddIssue(
                    sceneText.Contains(objectName)
                        ? CCS_SurvivalValidationIssueSeverity.Info
                        : CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Living Wildlife",
                    sceneText.Contains(objectName)
                        ? $"Bootstrap scene contains {objectName}."
                        : $"Bootstrap scene is missing {objectName}.");
            }

            report.AddIssue(
                sceneText.Contains("CCS_WildlifeDamageable")
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Bootstrap Wildlife Health",
                "Living bootstrap wildlife includes CCS_WildlifeDamageable components.");
        }

        private static void ValidatePrimaryActionInput(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(InputProviderPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Primary Action Input",
                    $"Missing script: {InputProviderPath}");
                return;
            }

            string source = File.ReadAllText(InputProviderPath);
            report.AddIssue(
                source.Contains("PrimaryActionPressedThisFrame")
                    && source.Contains("FindAction(\"PrimaryAction\"")
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Primary Action Input",
                "Character input provider exposes Gameplay/PrimaryAction for melee attacks.");
        }

        private static void ValidateHudIntegration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(HudPresentationPath) || !File.Exists(HudWiringPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "HUD Combat Integration",
                    "HUD presentation or wiring scripts are missing.");
                return;
            }

            string presentationSource = File.ReadAllText(HudPresentationPath);
            string wiringSource = File.ReadAllText(HudWiringPath);

            report.AddIssue(
                presentationSource.Contains("BindCombatService")
                    && presentationSource.Contains("HandleWildlifeDamaged")
                    && presentationSource.Contains("HandleWildlifeKilled")
                    && presentationSource.Contains("Hit ")
                    && presentationSource.Contains(" Killed")
                    && wiringSource.Contains("BindCombatService")
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "HUD Combat Notifications",
                "HUD binds combat service and queues hit/kill notifications.");
        }

        private static void ValidateRequiredFolder(
            CCS_SurvivalValidationReport report,
            string label,
            string folderPath)
        {
            report.AddIssue(
                AssetDatabase.IsValidFolder(folderPath) || Directory.Exists(folderPath)
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                label,
                AssetDatabase.IsValidFolder(folderPath) || Directory.Exists(folderPath)
                    ? $"Folder exists: {folderPath}"
                    : $"Missing folder: {folderPath}");
        }

        private static void ValidateRequiredFile(
            CCS_SurvivalValidationReport report,
            string label,
            string assetPath)
        {
            report.AddIssue(
                File.Exists(assetPath)
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                label,
                File.Exists(assetPath)
                    ? $"File exists: {assetPath}"
                    : $"Missing file: {assetPath}");
        }

        private static void ValidateRequiredScript(
            CCS_SurvivalValidationReport report,
            string label,
            string scriptPath)
        {
            ValidateRequiredFile(report, label, scriptPath);
        }

        private static void ValidateRequiredAsset(
            CCS_SurvivalValidationReport report,
            string label,
            string assetPath)
        {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            report.AddIssue(
                asset != null
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                label,
                asset != null
                    ? $"Asset exists: {assetPath}"
                    : $"Missing asset: {assetPath}");
        }

        private static void ValidateDocumentationAsset(
            CCS_SurvivalValidationReport report,
            string label,
            string assetPath)
        {
            report.AddIssue(
                File.Exists(assetPath)
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Warning,
                label,
                File.Exists(assetPath)
                    ? $"Documentation present: {assetPath}"
                    : $"Documentation missing: {assetPath}");
        }

        private static void ValidateRuntimeScriptsAvoidUnityEditor(
            CCS_SurvivalValidationReport report,
            string runtimeFolderPath)
        {
            if (!Directory.Exists(runtimeFolderPath))
            {
                return;
            }

            string[] scriptPaths = Directory.GetFiles(runtimeFolderPath, "*.cs", SearchOption.AllDirectories);
            for (int index = 0; index < scriptPaths.Length; index++)
            {
                string contents = File.ReadAllText(scriptPaths[index]);
                if (contents.Contains("using UnityEditor"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Runtime Editor Leak",
                        $"{scriptPaths[index]} references UnityEditor.");
                }
            }
        }

        #endregion
    }
}
