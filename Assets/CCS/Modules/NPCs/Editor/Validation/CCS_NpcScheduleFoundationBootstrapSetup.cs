using CCS.Modules.Playtesting;
using CCS.Modules.Settlements;
using CCS.Modules.WorldSimulation;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcScheduleFoundationBootstrapSetup
// CATEGORY: Modules / NPCs / Editor / Validation
// PURPOSE: Batch-creates schedule profile, world wiring, and playtest steps.
// PLACEMENT: Invoked by milestone bootstrap / validation pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.6.0 NPC schedule state foundation.
// =============================================================================

namespace CCS.Modules.NPCs.Editor
{
    public static class CCS_NpcScheduleFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_NpcScheduleFoundationBootstrapSetup]";
        private const string MilestoneVersion = "4.6.0";
        private const string WorldSimulationProfilePath =
            "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset";
        private const string PlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolder(CCS_NpcScheduleContentIds.ScheduleProfilesRoot);

            CCS_NpcScheduleProfile scheduleProfile = EnsureScheduleProfile();
            EnsureWorldSimulationScheduleProfile(scheduleProfile);
            EnsurePlaytestScheduleSteps();

            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} NPC schedule bootstrap complete ({MilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static CCS_NpcScheduleProfile EnsureScheduleProfile()
        {
            CCS_NpcScheduleProfile profile = AssetDatabase.LoadAssetAtPath<CCS_NpcScheduleProfile>(
                CCS_NpcScheduleContentIds.DefaultScheduleProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_NpcScheduleProfile>();
                AssetDatabase.CreateAsset(profile, CCS_NpcScheduleContentIds.DefaultScheduleProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue = CCS_NpcScheduleContentIds.DefaultScheduleProfileId;
            serialized.FindProperty("profileDisplayName").stringValue = "Default NPC Schedule Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Profile-driven daily schedule blocks for workforce placeholders and service representatives.";
            serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;
            serialized.FindProperty("fallbackScheduleId").stringValue = CCS_NpcScheduleContentIds.WorkerScheduleId;
            serialized.FindProperty("gapFallbackBlockType").enumValueIndex =
                (int)CCS_NpcScheduleBlockType.Idle;

            SerializedProperty definitions = serialized.FindProperty("scheduleDefinitions");
            definitions.arraySize = 2;
            WriteDefinition(
                definitions.GetArrayElementAtIndex(0),
                CCS_NpcScheduleContentIds.WorkerScheduleId,
                "Worker Schedule",
                new[]
                {
                    Block(CCS_NpcScheduleBlockType.Home, 20, 6),
                    Block(CCS_NpcScheduleBlockType.Work, 6, 18),
                    Block(CCS_NpcScheduleBlockType.Leisure, 18, 20)
                });
            WriteDefinition(
                definitions.GetArrayElementAtIndex(1),
                CCS_NpcScheduleContentIds.ServiceRepresentativeScheduleId,
                "Service Representative Schedule",
                new[]
                {
                    Block(CCS_NpcScheduleBlockType.Home, 20, 7),
                    Block(CCS_NpcScheduleBlockType.Service, 7, 18),
                    Block(CCS_NpcScheduleBlockType.Leisure, 18, 20)
                });

            SerializedProperty mappings = serialized.FindProperty("roleMappings");
            mappings.arraySize = 6;
            WriteMapping(mappings.GetArrayElementAtIndex(0), CCS_NpcRoleType.Merchant, true, string.Empty,
                CCS_SettlementPopulationCategory.Unknown, CCS_NpcScheduleContentIds.ServiceRepresentativeScheduleId);
            WriteMapping(mappings.GetArrayElementAtIndex(1), CCS_NpcRoleType.Banker, false, string.Empty,
                CCS_SettlementPopulationCategory.Unknown, CCS_NpcScheduleContentIds.ServiceRepresentativeScheduleId);
            WriteMapping(mappings.GetArrayElementAtIndex(2), CCS_NpcRoleType.Miner, false, string.Empty,
                CCS_SettlementPopulationCategory.Miners, CCS_NpcScheduleContentIds.WorkerScheduleId);
            WriteMapping(mappings.GetArrayElementAtIndex(3), CCS_NpcRoleType.Farmer, false, string.Empty,
                CCS_SettlementPopulationCategory.Farmers, CCS_NpcScheduleContentIds.WorkerScheduleId);
            WriteMapping(mappings.GetArrayElementAtIndex(4), CCS_NpcRoleType.LumberWorker, false, string.Empty,
                CCS_SettlementPopulationCategory.LumberWorkers, CCS_NpcScheduleContentIds.WorkerScheduleId);
            WriteMapping(mappings.GetArrayElementAtIndex(5), CCS_NpcRoleType.Unknown, true, string.Empty,
                CCS_SettlementPopulationCategory.Unknown, CCS_NpcScheduleContentIds.ServiceRepresentativeScheduleId);

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureWorldSimulationScheduleProfile(CCS_NpcScheduleProfile scheduleProfile)
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
            serialized.FindProperty("settlementNpcScheduleProfile").objectReferenceValue = scheduleProfile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsurePlaytestScheduleSteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(PlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(profile, "ccs.survival.playtest.npcschedule.discover", "Discover settlement for NPC schedule",
                CCS_PlaytestStepType.DiscoverSettlementForNpcSchedule);
            InsertStep(profile, "ccs.survival.playtest.npcschedule.spawn", "Spawn named NPC for schedule",
                CCS_PlaytestStepType.SpawnNamedNpcForSchedule);
            InsertStep(profile, "ccs.survival.playtest.npcschedule.verify.assigned", "Verify NPC schedule assigned",
                CCS_PlaytestStepType.VerifyNpcScheduleAssigned);
            InsertStep(profile, "ccs.survival.playtest.npcschedule.force.work", "Force evaluate NPC schedule work block",
                CCS_PlaytestStepType.ForceEvaluateNpcScheduleWorkBlock);
            InsertStep(profile, "ccs.survival.playtest.npcschedule.verify.workplace",
                "Verify NPC schedule workplace target",
                CCS_PlaytestStepType.VerifyNpcScheduleWorkplaceTarget);
            InsertStep(profile, "ccs.survival.playtest.npcschedule.force.home", "Force evaluate NPC schedule home block",
                CCS_PlaytestStepType.ForceEvaluateNpcScheduleHomeBlock);
            InsertStep(profile, "ccs.survival.playtest.npcschedule.verify.housing",
                "Verify NPC schedule housing target",
                CCS_PlaytestStepType.VerifyNpcScheduleHousingTarget);
            InsertStep(profile, "ccs.survival.playtest.npcschedule.save", "Save NPC schedule state",
                CCS_PlaytestStepType.SaveNpcScheduleState);
            InsertStep(profile, "ccs.survival.playtest.npcschedule.load", "Load NPC schedule state",
                CCS_PlaytestStepType.LoadNpcScheduleState);
            InsertStep(profile, "ccs.survival.playtest.npcschedule.verify.load", "Verify NPC schedule restored after load",
                CCS_PlaytestStepType.VerifyNpcScheduleAfterLoad);
            EditorUtility.SetDirty(profile);
        }

        private static void WriteDefinition(
            SerializedProperty definitionProperty,
            string scheduleId,
            string displayName,
            (CCS_NpcScheduleBlockType blockType, int startHour, int endHour)[] blocks)
        {
            definitionProperty.FindPropertyRelative("scheduleId").stringValue = scheduleId;
            definitionProperty.FindPropertyRelative("displayName").stringValue = displayName;
            SerializedProperty blockArray = definitionProperty.FindPropertyRelative("blocks");
            blockArray.arraySize = blocks.Length;
            for (int index = 0; index < blocks.Length; index++)
            {
                SerializedProperty block = blockArray.GetArrayElementAtIndex(index);
                block.FindPropertyRelative("startHour").intValue = blocks[index].startHour;
                block.FindPropertyRelative("endHour").intValue = blocks[index].endHour;
                block.FindPropertyRelative("blockType").enumValueIndex = (int)blocks[index].blockType;
            }
        }

        private static void WriteMapping(
            SerializedProperty mappingProperty,
            CCS_NpcRoleType roleType,
            bool requiresServiceRepresentative,
            string businessId,
            CCS_SettlementPopulationCategory workforceCategory,
            string scheduleId)
        {
            mappingProperty.FindPropertyRelative("roleType").enumValueIndex = (int)roleType;
            mappingProperty.FindPropertyRelative("businessId").stringValue = businessId ?? string.Empty;
            mappingProperty.FindPropertyRelative("workforceCategory").enumValueIndex = (int)workforceCategory;
            mappingProperty.FindPropertyRelative("requiresServiceRepresentative").boolValue = requiresServiceRepresentative;
            mappingProperty.FindPropertyRelative("scheduleId").stringValue = scheduleId;
        }

        private static (CCS_NpcScheduleBlockType blockType, int startHour, int endHour) Block(
            CCS_NpcScheduleBlockType blockType,
            int startHour,
            int endHour)
        {
            return (blockType, startHour, endHour);
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
                $"{displayName}. Ctrl+Alt+S shortcut available.";
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
