using CCS.Modules.Playtesting;
using CCS.Modules.WorldSimulation;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcMovementFoundationBootstrapSetup
// CATEGORY: Modules / NPCs / Editor / Validation
// PURPOSE: Batch-creates movement profile, world wiring, and playtest steps.
// PLACEMENT: Invoked by milestone bootstrap / validation pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.5.0 NPC movement foundation.
// =============================================================================

namespace CCS.Modules.NPCs.Editor
{
    public static class CCS_NpcMovementFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_NpcMovementFoundationBootstrapSetup]";
        private const string MilestoneVersion = "4.5.0";
        private const string WorldSimulationProfilePath =
            "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset";
        private const string PlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolder(CCS_NpcMovementContentIds.MovementProfilesRoot);

            CCS_NpcMovementProfile movementProfile = EnsureMovementProfile();
            EnsureWorldSimulationMovementProfile(movementProfile);
            EnsurePlaytestMovementSteps();

            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} NPC movement bootstrap complete ({MilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static CCS_NpcMovementProfile EnsureMovementProfile()
        {
            CCS_NpcMovementProfile profile = AssetDatabase.LoadAssetAtPath<CCS_NpcMovementProfile>(
                CCS_NpcMovementContentIds.DefaultMovementProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_NpcMovementProfile>();
                AssetDatabase.CreateAsset(profile, CCS_NpcMovementContentIds.DefaultMovementProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue = CCS_NpcMovementContentIds.DefaultMovementProfileId;
            serialized.FindProperty("profileDisplayName").stringValue = "Default NPC Movement Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Schedule-driven transform movement for workforce placeholders and service representatives.";
            serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;
            serialized.FindProperty("workStartHour").intValue = 6;
            serialized.FindProperty("workEndHour").intValue = 18;
            serialized.FindProperty("moveSpeed").floatValue = 1.75f;
            serialized.FindProperty("arrivalTolerance").floatValue = 0.5f;
            serialized.FindProperty("idleRotationSpeed").floatValue = 45f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureWorldSimulationMovementProfile(CCS_NpcMovementProfile movementProfile)
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
            serialized.FindProperty("settlementNpcMovementProfile").objectReferenceValue = movementProfile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsurePlaytestMovementSteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(PlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(profile, "ccs.survival.playtest.npcmovement.discover", "Discover settlement for NPC movement",
                CCS_PlaytestStepType.DiscoverSettlementForNpcMovement);
            InsertStep(profile, "ccs.survival.playtest.npcmovement.verify.worker", "Verify worker movement active",
                CCS_PlaytestStepType.VerifyWorkerMovementActive);
            InsertStep(profile, "ccs.survival.playtest.npcmovement.verify.representative",
                "Verify representative movement active",
                CCS_PlaytestStepType.VerifyRepresentativeMovementActive);
            InsertStep(profile, "ccs.survival.playtest.npcmovement.verify.home", "Verify schedule transition to home",
                CCS_PlaytestStepType.VerifyScheduleTransitionToHome);
            InsertStep(profile, "ccs.survival.playtest.npcmovement.verify.work", "Verify schedule transition to work",
                CCS_PlaytestStepType.VerifyScheduleTransitionToWork);
            InsertStep(profile, "ccs.survival.playtest.npcmovement.save", "Save NPC movement state",
                CCS_PlaytestStepType.SaveNpcMovementState);
            InsertStep(profile, "ccs.survival.playtest.npcmovement.load", "Load NPC movement state",
                CCS_PlaytestStepType.LoadNpcMovementState);
            InsertStep(profile, "ccs.survival.playtest.npcmovement.verify.load", "Verify NPC movement restored after load",
                CCS_PlaytestStepType.VerifyNpcMovementAfterLoad);
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
                $"{displayName}. Ctrl+Alt+M shortcut available.";
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
