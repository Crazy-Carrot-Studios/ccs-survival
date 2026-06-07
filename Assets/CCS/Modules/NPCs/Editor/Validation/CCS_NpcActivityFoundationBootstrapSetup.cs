using CCS.Modules.Playtesting;
using CCS.Modules.WorldSimulation;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcActivityFoundationBootstrapSetup
// CATEGORY: Modules / NPCs / Editor / Validation
// PURPOSE: Batch-creates activity profile, world wiring, and playtest steps.
// PLACEMENT: Invoked by milestone bootstrap / validation pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.7.0 NPC activity state foundation.
// =============================================================================

namespace CCS.Modules.NPCs.Editor
{
    public static class CCS_NpcActivityFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_NpcActivityFoundationBootstrapSetup]";
        private const string MilestoneVersion = "4.7.0";
        private const string WorldSimulationProfilePath =
            "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset";
        private const string PlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolder(CCS_NpcActivityContentIds.ActivityProfilesRoot);

            CCS_NpcActivityProfile activityProfile = EnsureActivityProfile();
            EnsureWorldSimulationActivityProfile(activityProfile);
            EnsurePlaytestActivitySteps();

            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} NPC activity bootstrap complete ({MilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static CCS_NpcActivityProfile EnsureActivityProfile()
        {
            CCS_NpcActivityProfile profile = AssetDatabase.LoadAssetAtPath<CCS_NpcActivityProfile>(
                CCS_NpcActivityContentIds.DefaultActivityProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_NpcActivityProfile>();
                AssetDatabase.CreateAsset(profile, CCS_NpcActivityContentIds.DefaultActivityProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue = CCS_NpcActivityContentIds.DefaultActivityProfileId;
            serialized.FindProperty("profileDisplayName").stringValue = "Default NPC Activity Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Schedule block to activity mappings for placeholder NPC dev presentation.";
            serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;
            serialized.FindProperty("scheduleMissingFallbackActivity").enumValueIndex =
                (int)CCS_NpcActivityType.Idle;
            serialized.FindProperty("movementMissingFallbackActivity").enumValueIndex =
                (int)CCS_NpcActivityType.Idle;

            SerializedProperty mappings = serialized.FindProperty("blockMappings");
            mappings.arraySize = 7;
            WriteMapping(mappings.GetArrayElementAtIndex(0), CCS_NpcScheduleBlockType.Sleep, CCS_NpcActivityType.Sleeping);
            WriteMapping(mappings.GetArrayElementAtIndex(1), CCS_NpcScheduleBlockType.Home, CCS_NpcActivityType.Resting);
            WriteMapping(mappings.GetArrayElementAtIndex(2), CCS_NpcScheduleBlockType.Work, CCS_NpcActivityType.Working);
            WriteMapping(mappings.GetArrayElementAtIndex(3), CCS_NpcScheduleBlockType.Service, CCS_NpcActivityType.Serving);
            WriteMapping(mappings.GetArrayElementAtIndex(4), CCS_NpcScheduleBlockType.Break, CCS_NpcActivityType.Resting);
            WriteMapping(mappings.GetArrayElementAtIndex(5), CCS_NpcScheduleBlockType.Leisure, CCS_NpcActivityType.Leisure);
            WriteMapping(mappings.GetArrayElementAtIndex(6), CCS_NpcScheduleBlockType.Idle, CCS_NpcActivityType.Idle);

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureWorldSimulationActivityProfile(CCS_NpcActivityProfile activityProfile)
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
            serialized.FindProperty("settlementNpcActivityProfile").objectReferenceValue = activityProfile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsurePlaytestActivitySteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(PlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(profile, "ccs.survival.playtest.npcactivity.discover", "Discover settlement for NPC activity",
                CCS_PlaytestStepType.DiscoverSettlementForNpcActivity);
            InsertStep(profile, "ccs.survival.playtest.npcactivity.spawn", "Spawn named NPC for activity",
                CCS_PlaytestStepType.SpawnNamedNpcForActivity);
            InsertStep(profile, "ccs.survival.playtest.npcactivity.force.work", "Force work/service block for activity",
                CCS_PlaytestStepType.ForceNpcActivityWorkBlock);
            InsertStep(profile, "ccs.survival.playtest.npcactivity.verify.work", "Verify NPC activity Working/Serving",
                CCS_PlaytestStepType.VerifyNpcActivityWorkingOrServing);
            InsertStep(profile, "ccs.survival.playtest.npcactivity.force.home", "Force home/sleep block for activity",
                CCS_PlaytestStepType.ForceNpcActivityHomeBlock);
            InsertStep(profile, "ccs.survival.playtest.npcactivity.verify.home", "Verify NPC activity Resting/Sleeping",
                CCS_PlaytestStepType.VerifyNpcActivityRestingOrSleeping);
            InsertStep(profile, "ccs.survival.playtest.npcactivity.force.travel", "Force NPC movement for activity",
                CCS_PlaytestStepType.ForceNpcActivityTraveling);
            InsertStep(profile, "ccs.survival.playtest.npcactivity.verify.travel", "Verify NPC activity Traveling",
                CCS_PlaytestStepType.VerifyNpcActivityTraveling);
            InsertStep(profile, "ccs.survival.playtest.npcactivity.save", "Save NPC activity state",
                CCS_PlaytestStepType.SaveNpcActivityState);
            InsertStep(profile, "ccs.survival.playtest.npcactivity.load", "Load NPC activity state",
                CCS_PlaytestStepType.LoadNpcActivityState);
            InsertStep(profile, "ccs.survival.playtest.npcactivity.verify.load", "Verify NPC activity restored after load",
                CCS_PlaytestStepType.VerifyNpcActivityAfterLoad);
            EditorUtility.SetDirty(profile);
        }

        private static void WriteMapping(
            SerializedProperty mappingProperty,
            CCS_NpcScheduleBlockType blockType,
            CCS_NpcActivityType activityType)
        {
            mappingProperty.FindPropertyRelative("scheduleBlockType").enumValueIndex = (int)blockType;
            mappingProperty.FindPropertyRelative("activityType").enumValueIndex = (int)activityType;
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
                $"{displayName}. Ctrl+Alt+A shortcut available.";
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
