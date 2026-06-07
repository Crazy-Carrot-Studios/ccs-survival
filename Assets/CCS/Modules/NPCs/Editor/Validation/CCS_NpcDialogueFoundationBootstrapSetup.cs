using CCS.Modules.Playtesting;
using CCS.Modules.Settlements;
using CCS.Modules.WorldSimulation;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcDialogueFoundationBootstrapSetup
// CATEGORY: Modules / NPCs / Editor / Validation
// PURPOSE: Batch-creates dialogue stub profile, world wiring, and playtest steps.
// PLACEMENT: Invoked by milestone bootstrap / validation pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.9.0 NPC dialogue stub foundation.
// =============================================================================

namespace CCS.Modules.NPCs.Editor
{
    public static class CCS_NpcDialogueFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_NpcDialogueFoundationBootstrapSetup]";
        private const string MilestoneVersion = "4.9.0";
        private const string WorldSimulationProfilePath =
            "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset";
        private const string PlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolder(CCS_NpcDialogueStubContentIds.DialogueProfilesRoot);

            CCS_NpcDialogueStubProfile dialogueProfile = EnsureDialogueStubProfile();
            EnsureWorldSimulationDialogueProfile(dialogueProfile);
            EnsurePlaytestDialogueSteps();

            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} NPC dialogue stub bootstrap complete ({MilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static CCS_NpcDialogueStubProfile EnsureDialogueStubProfile()
        {
            CCS_NpcDialogueStubProfile profile = AssetDatabase.LoadAssetAtPath<CCS_NpcDialogueStubProfile>(
                CCS_NpcDialogueStubContentIds.DefaultDialogueStubProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_NpcDialogueStubProfile>();
                AssetDatabase.CreateAsset(profile, CCS_NpcDialogueStubContentIds.DefaultDialogueStubProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue = CCS_NpcDialogueStubContentIds.DefaultDialogueStubProfileId;
            serialized.FindProperty("profileDisplayName").stringValue = "Default NPC Dialogue Stub Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Profile-driven greeting, role, settlement, business, and service hint stub lines.";
            serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;
            serialized.FindProperty("genericFallbackLine").stringValue =
                "Good to see you. Frontier life keeps us all busy.";
            serialized.FindProperty("requireAffiliationForDialogue").boolValue = true;

            SerializedProperty roleDefinitions = serialized.FindProperty("roleDefinitions");
            roleDefinitions.arraySize = 10;
            WriteRoleDefinition(roleDefinitions.GetArrayElementAtIndex(0), CCS_NpcRoleType.Merchant, 1,
                CCS_NpcDialogueStubCategory.RoleIntroduction,
                "Welcome. I handle supplies and trade goods here.");
            WriteRoleDefinition(roleDefinitions.GetArrayElementAtIndex(1), CCS_NpcRoleType.Banker, 1,
                CCS_NpcDialogueStubCategory.RoleIntroduction,
                "Savings, deposits, and loans are handled at the bank.");
            WriteRoleDefinition(roleDefinitions.GetArrayElementAtIndex(2), CCS_NpcRoleType.Miner, 2,
                CCS_NpcDialogueStubCategory.RoleIntroduction,
                "Mining camps run on ore, coal, and hard work.",
                CCS_NpcDialogueStubCategory.RoleIntroduction,
                "Iron Ridge runs on ore, coal, and hard work.",
                settlementId: CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId);
            WriteRoleDefinition(roleDefinitions.GetArrayElementAtIndex(3), CCS_NpcRoleType.Farmer, 2,
                CCS_NpcDialogueStubCategory.RoleIntroduction,
                "Farmsteads keep food moving through the frontier.",
                CCS_NpcDialogueStubCategory.RoleIntroduction,
                "Broken Creek keeps food moving through the frontier.",
                settlementId: CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId);
            WriteRoleDefinition(roleDefinitions.GetArrayElementAtIndex(4), CCS_NpcRoleType.LumberWorker, 2,
                CCS_NpcDialogueStubCategory.RoleIntroduction,
                "Timber crews keep frontier camps standing.",
                CCS_NpcDialogueStubCategory.RoleIntroduction,
                "Pine Ridge timber keeps half these camps standing.",
                settlementId: CCS_MultiSettlementContentIds.PineRidgeCampSettlementId);
            WriteRoleDefinition(roleDefinitions.GetArrayElementAtIndex(5), CCS_NpcRoleType.Laborer, 1,
                CCS_NpcDialogueStubCategory.RoleIntroduction,
                "Always more work than hands out here.");
            WriteRoleDefinition(roleDefinitions.GetArrayElementAtIndex(6), CCS_NpcRoleType.StableHand, 1,
                CCS_NpcDialogueStubCategory.RoleIntroduction,
                "The stable keeps horses fed and tack ready.");
            WriteRoleDefinition(roleDefinitions.GetArrayElementAtIndex(7), CCS_NpcRoleType.Gunsmith, 1,
                CCS_NpcDialogueStubCategory.RoleIntroduction,
                "I keep rifles ready and powder dry.");
            WriteRoleDefinition(roleDefinitions.GetArrayElementAtIndex(8), CCS_NpcRoleType.Blacksmith, 1,
                CCS_NpcDialogueStubCategory.RoleIntroduction,
                "Steel and horseshoes — bring metal and patience.");
            WriteRoleDefinition(roleDefinitions.GetArrayElementAtIndex(9), CCS_NpcRoleType.Clerk, 1,
                CCS_NpcDialogueStubCategory.RoleIntroduction,
                "Paperwork, contracts, and town notices pass through here.");

            SerializedProperty globalLines = serialized.FindProperty("globalLines");
            globalLines.arraySize = 5;
            WriteLine(globalLines.GetArrayElementAtIndex(0), CCS_NpcDialogueStubCategory.Greeting,
                "Good day. What brings you through town?");
            WriteLine(globalLines.GetArrayElementAtIndex(1), CCS_NpcDialogueStubCategory.ServiceHint,
                "Browse our goods at the counter when you're ready.",
                serviceRoute: CCS_SettlementServiceRouteType.Vendor);
            WriteLine(globalLines.GetArrayElementAtIndex(2), CCS_NpcDialogueStubCategory.ServiceHint,
                "Visit the teller window for deposits, withdrawals, or loans.",
                serviceRoute: CCS_SettlementServiceRouteType.Bank);
            WriteLine(globalLines.GetArrayElementAtIndex(3), CCS_NpcDialogueStubCategory.ServiceHint,
                "The forge handles repairs and specialty orders.",
                serviceRoute: CCS_SettlementServiceRouteType.Industry);
            WriteLine(globalLines.GetArrayElementAtIndex(4), CCS_NpcDialogueStubCategory.ServiceHint,
                "Check the contract board for local work.",
                serviceRoute: CCS_SettlementServiceRouteType.ContractBoard);

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureWorldSimulationDialogueProfile(CCS_NpcDialogueStubProfile dialogueProfile)
        {
            CCS_WorldSimulationProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_WorldSimulationProfile>(WorldSimulationProfilePath);
            if (profile == null)
            {
                Debug.LogError($"{LogPrefix} Missing world simulation profile.");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;
            serialized.FindProperty("settlementNpcDialogueStubProfile").objectReferenceValue = dialogueProfile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsurePlaytestDialogueSteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(PlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(profile, "ccs.survival.playtest.npcdialogue.discover", "Discover settlement for NPC dialogue",
                CCS_PlaytestStepType.DiscoverSettlementForNpcDialogue);
            InsertStep(profile, "ccs.survival.playtest.npcdialogue.spawn", "Spawn named NPC for dialogue",
                CCS_PlaytestStepType.SpawnNamedNpcForDialogue);
            InsertStep(profile, "ccs.survival.playtest.npcdialogue.interact", "Interact with NPC for dialogue",
                CCS_PlaytestStepType.InteractWithNpcForDialogue);
            InsertStep(profile, "ccs.survival.playtest.npcdialogue.verify.greeting", "Verify NPC dialogue greeting",
                CCS_PlaytestStepType.VerifyNpcDialogueGreeting);
            InsertStep(profile, "ccs.survival.playtest.npcdialogue.verify.role", "Verify NPC dialogue role introduction",
                CCS_PlaytestStepType.VerifyNpcDialogueRoleIntroduction);
            InsertStep(profile, "ccs.survival.playtest.npcdialogue.verify.service", "Verify NPC dialogue service hint",
                CCS_PlaytestStepType.VerifyNpcDialogueServiceHint);
            InsertStep(profile, "ccs.survival.playtest.npcdialogue.save", "Save NPC dialogue state",
                CCS_PlaytestStepType.SaveNpcDialogueState);
            InsertStep(profile, "ccs.survival.playtest.npcdialogue.load", "Load NPC dialogue state",
                CCS_PlaytestStepType.LoadNpcDialogueState);
            InsertStep(profile, "ccs.survival.playtest.npcdialogue.verify.load", "Verify NPC dialogue after load",
                CCS_PlaytestStepType.VerifyNpcDialogueAfterLoad);
            EditorUtility.SetDirty(profile);
        }

        private static void WriteRoleDefinition(
            SerializedProperty definitionProperty,
            CCS_NpcRoleType roleType,
            int lineCount,
            CCS_NpcDialogueStubCategory firstCategory,
            string firstText,
            CCS_NpcDialogueStubCategory secondCategory = CCS_NpcDialogueStubCategory.Unknown,
            string secondText = "",
            string settlementId = "")
        {
            definitionProperty.FindPropertyRelative("roleType").intValue = (int)roleType;
            SerializedProperty lines = definitionProperty.FindPropertyRelative("lines");
            lines.arraySize = lineCount;
            WriteLine(lines.GetArrayElementAtIndex(0), firstCategory, firstText, roleType);
            if (lineCount > 1)
            {
                WriteLine(
                    lines.GetArrayElementAtIndex(1),
                    secondCategory,
                    secondText,
                    roleType,
                    settlementId);
            }
        }

        private static void WriteLine(
            SerializedProperty lineProperty,
            CCS_NpcDialogueStubCategory category,
            string lineText,
            CCS_NpcRoleType roleType = CCS_NpcRoleType.Unknown,
            string settlementId = "",
            string businessId = "",
            CCS_SettlementServiceRouteType serviceRoute = CCS_SettlementServiceRouteType.Unknown)
        {
            lineProperty.FindPropertyRelative("category").enumValueIndex = (int)category;
            lineProperty.FindPropertyRelative("lineText").stringValue = lineText;
            lineProperty.FindPropertyRelative("roleType").intValue = (int)roleType;
            lineProperty.FindPropertyRelative("settlementId").stringValue = settlementId ?? string.Empty;
            lineProperty.FindPropertyRelative("businessId").stringValue = businessId ?? string.Empty;
            lineProperty.FindPropertyRelative("affiliationType").intValue = (int)CCS_NpcAffiliationType.None;
            lineProperty.FindPropertyRelative("serviceRoute").intValue = (int)serviceRoute;
        }

        private static void InsertStep(
            CCS_PlaytestProfile profile,
            string stepId,
            string displayName,
            CCS_PlaytestStepType stepType)
        {
            SerializedObject serialized = new SerializedObject(profile);
            SerializedProperty steps = serialized.FindProperty("stepDefinitions");
            for (int index = 0; index < steps.arraySize; index++)
            {
                if (steps.GetArrayElementAtIndex(index).FindPropertyRelative("stepId").stringValue == stepId)
                {
                    return;
                }
            }

            steps.InsertArrayElementAtIndex(steps.arraySize);
            SerializedProperty step = steps.GetArrayElementAtIndex(steps.arraySize - 1);
            step.FindPropertyRelative("stepId").stringValue = stepId;
            step.FindPropertyRelative("displayName").stringValue = displayName;
            step.FindPropertyRelative("stepType").enumValueIndex = (int)stepType;
            step.FindPropertyRelative("instructionText").stringValue =
                $"{displayName}. Ctrl+Alt+D shortcut available.";
            step.FindPropertyRelative("targetItemId").stringValue = string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string folderName = System.IO.Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
