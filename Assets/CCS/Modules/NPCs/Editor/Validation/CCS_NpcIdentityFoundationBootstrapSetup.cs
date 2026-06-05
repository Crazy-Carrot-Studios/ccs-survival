using CCS.Modules.Playtesting;
using CCS.Modules.Settlements;
using CCS.Modules.WorldSimulation;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcIdentityFoundationBootstrapSetup
// CATEGORY: Modules / NPCs / Editor / Validation
// PURPOSE: Batch-creates NPC identity profile, world wiring, and playtest steps.
// PLACEMENT: Invoked by milestone bootstrap / validation pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.1.0 NPC identity and role foundation.
// =============================================================================

namespace CCS.Modules.NPCs.Editor
{
    public static class CCS_NpcIdentityFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_NpcIdentityFoundationBootstrapSetup]";
        private const string MilestoneVersion = "4.1.0";
        private const string WorldSimulationProfilePath =
            "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset";
        private const string PlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolder(CCS_NpcIdentityContentIds.IdentityProfilesRoot);

            CCS_NpcIdentityProfile identityProfile = EnsureIdentityProfile();
            EnsureWorldSimulationIdentityProfile(identityProfile);
            EnsurePlaytestNpcIdentitySteps();

            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} NPC identity bootstrap complete ({MilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static CCS_NpcIdentityProfile EnsureIdentityProfile()
        {
            CCS_NpcIdentityProfile profile = AssetDatabase.LoadAssetAtPath<CCS_NpcIdentityProfile>(
                CCS_NpcIdentityContentIds.DefaultIdentityProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_NpcIdentityProfile>();
                AssetDatabase.CreateAsset(profile, CCS_NpcIdentityContentIds.DefaultIdentityProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue = CCS_NpcIdentityContentIds.DefaultIdentityProfileId;
            serialized.FindProperty("profileDisplayName").stringValue = "Default NPC Identity Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Name pools and workforce/business role mappings for population placeholder NPCs.";
            serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;

            SetStringArray(serialized.FindProperty("defaultFirstNamePool"), new[]
            {
                "Elias", "Clara", "Jonah", "Mae", "Silas", "Ada", "Cole", "Iris", "Levi", "Nora"
            });
            SetStringArray(serialized.FindProperty("defaultLastNamePool"), new[]
            {
                "Carter", "Whitfield", "Hawthorne", "McAllister", "Langston", "Pike", "Dalton", "Mercer", "Sutton", "Reed"
            });

            SerializedProperty settlements = serialized.FindProperty("settlementDefinitions");
            settlements.arraySize = 4;
            SetSettlementNames(settlements, 0, CCS_SettlementGrowthContentIds.TradingPostSettlementId);
            SetSettlementNames(settlements, 1, CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId);
            SetSettlementNames(settlements, 2, CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId);
            SetSettlementNames(settlements, 3, CCS_MultiSettlementContentIds.PineRidgeCampSettlementId);

            SerializedProperty roles = serialized.FindProperty("roleAssignments");
            roles.arraySize = 12;
            SetRoleAssignment(roles, 0, string.Empty, CCS_SettlementPopulationCategory.Merchants,
                "ccs.survival.business.generalstore", CCS_NpcRoleType.Merchant);
            SetRoleAssignment(roles, 1, string.Empty, CCS_SettlementPopulationCategory.Ranchers,
                "ccs.survival.business.stable", CCS_NpcRoleType.StableHand);
            SetRoleAssignment(roles, 2, string.Empty, CCS_SettlementPopulationCategory.Farmers,
                "ccs.survival.business.farmsupply", CCS_NpcRoleType.Farmer);
            SetRoleAssignment(roles, 3, string.Empty, CCS_SettlementPopulationCategory.Miners,
                "ccs.survival.business.miningsupplier", CCS_NpcRoleType.Miner);
            SetRoleAssignment(roles, 4, string.Empty, CCS_SettlementPopulationCategory.LumberWorkers,
                "ccs.survival.business.lumberyard", CCS_NpcRoleType.LumberWorker);
            SetRoleAssignment(roles, 5, string.Empty, CCS_SettlementPopulationCategory.Merchants,
                string.Empty, CCS_NpcRoleType.Merchant);
            SetRoleAssignment(roles, 6, string.Empty, CCS_SettlementPopulationCategory.Farmers,
                string.Empty, CCS_NpcRoleType.Farmer);
            SetRoleAssignment(roles, 7, string.Empty, CCS_SettlementPopulationCategory.Ranchers,
                string.Empty, CCS_NpcRoleType.Rancher);
            SetRoleAssignment(roles, 8, string.Empty, CCS_SettlementPopulationCategory.Miners,
                string.Empty, CCS_NpcRoleType.Miner);
            SetRoleAssignment(roles, 9, string.Empty, CCS_SettlementPopulationCategory.LumberWorkers,
                string.Empty, CCS_NpcRoleType.LumberWorker);
            SetRoleAssignment(roles, 10, string.Empty, CCS_SettlementPopulationCategory.Laborers,
                string.Empty, CCS_NpcRoleType.Laborer);
            SetRoleAssignment(roles, 11, CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                CCS_SettlementPopulationCategory.Laborers, string.Empty, CCS_NpcRoleType.Clerk);

            SerializedProperty displayNames = serialized.FindProperty("roleDisplayNames");
            displayNames.arraySize = 13;
            SetRoleDisplay(displayNames, 0, CCS_NpcRoleType.Merchant, "Merchant");
            SetRoleDisplay(displayNames, 1, CCS_NpcRoleType.Banker, "Banker");
            SetRoleDisplay(displayNames, 2, CCS_NpcRoleType.StableHand, "Stable Hand");
            SetRoleDisplay(displayNames, 3, CCS_NpcRoleType.Gunsmith, "Gunsmith");
            SetRoleDisplay(displayNames, 4, CCS_NpcRoleType.Blacksmith, "Blacksmith");
            SetRoleDisplay(displayNames, 5, CCS_NpcRoleType.Farmer, "Farmer");
            SetRoleDisplay(displayNames, 6, CCS_NpcRoleType.Rancher, "Rancher");
            SetRoleDisplay(displayNames, 7, CCS_NpcRoleType.Miner, "Miner");
            SetRoleDisplay(displayNames, 8, CCS_NpcRoleType.LumberWorker, "Lumber Worker");
            SetRoleDisplay(displayNames, 9, CCS_NpcRoleType.Laborer, "Laborer");
            SetRoleDisplay(displayNames, 10, CCS_NpcRoleType.Clerk, "Clerk");
            SetRoleDisplay(displayNames, 11, CCS_NpcRoleType.DoctorPlaceholder, "Doctor (Placeholder)");
            SetRoleDisplay(displayNames, 12, CCS_NpcRoleType.SheriffPlaceholder, "Sheriff (Placeholder)");

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void SetStringArray(SerializedProperty property, string[] values)
        {
            property.arraySize = values.Length;
            for (int index = 0; index < values.Length; index++)
            {
                property.GetArrayElementAtIndex(index).stringValue = values[index];
            }
        }

        private static void SetSettlementNames(SerializedProperty settlements, int index, string settlementId)
        {
            SerializedProperty entry = settlements.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("settlementId").stringValue = settlementId;
            SetStringArray(entry.FindPropertyRelative("firstNamePool"), new[]
            {
                "Elias", "Clara", "Jonah", "Mae", "Silas"
            });
            SetStringArray(entry.FindPropertyRelative("lastNamePool"), new[]
            {
                "Carter", "Whitfield", "Hawthorne", "McAllister", "Langston"
            });
        }

        private static void SetRoleAssignment(
            SerializedProperty roles,
            int index,
            string settlementId,
            CCS_SettlementPopulationCategory category,
            string businessId,
            CCS_NpcRoleType roleType)
        {
            SerializedProperty entry = roles.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("settlementId").stringValue = settlementId;
            entry.FindPropertyRelative("workforceCategory").enumValueIndex = (int)category;
            entry.FindPropertyRelative("businessId").stringValue = businessId;
            entry.FindPropertyRelative("roleType").enumValueIndex = (int)roleType;
        }

        private static void SetRoleDisplay(
            SerializedProperty displayNames,
            int index,
            CCS_NpcRoleType roleType,
            string displayName)
        {
            SerializedProperty entry = displayNames.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("roleType").enumValueIndex = (int)roleType;
            entry.FindPropertyRelative("displayName").stringValue = displayName;
        }

        private static void EnsureWorldSimulationIdentityProfile(CCS_NpcIdentityProfile identityProfile)
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
            serialized.FindProperty("settlementNpcIdentityProfile").objectReferenceValue = identityProfile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsurePlaytestNpcIdentitySteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(PlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(profile, "ccs.survival.playtest.npcidentity.discover",
                "Discover settlement for NPC identity",
                CCS_PlaytestStepType.DiscoverSettlementForNpcIdentity);
            InsertStep(profile, "ccs.survival.playtest.npcidentity.population",
                "Trigger population presence for NPC identity",
                CCS_PlaytestStepType.TriggerPopulationPresenceForNpcIdentity);
            InsertStep(profile, "ccs.survival.playtest.npcidentity.verify.identity",
                "Verify placeholder actor has identity",
                CCS_PlaytestStepType.VerifyPlaceholderActorHasIdentity);
            InsertStep(profile, "ccs.survival.playtest.npcidentity.verify.role",
                "Verify NPC role matches workforce category",
                CCS_PlaytestStepType.VerifyNpcRoleMatchesWorkforce);
            InsertStep(profile, "ccs.survival.playtest.npcidentity.save",
                "Save NPC identity state",
                CCS_PlaytestStepType.SaveNpcIdentityState);
            InsertStep(profile, "ccs.survival.playtest.npcidentity.load",
                "Verify NPC identity restored after load",
                CCS_PlaytestStepType.VerifyNpcIdentityAfterLoad);
            EditorUtility.SetDirty(profile);
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
                $"{displayName}. Ctrl+Shift+E shortcut available.";
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
