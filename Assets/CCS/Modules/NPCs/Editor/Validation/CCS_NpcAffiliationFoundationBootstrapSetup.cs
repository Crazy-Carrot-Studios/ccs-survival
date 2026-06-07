using CCS.Modules.Playtesting;
using CCS.Modules.WorldSimulation;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcAffiliationFoundationBootstrapSetup
// CATEGORY: Modules / NPCs / Editor / Validation
// PURPOSE: Batch-creates affiliation profile, world wiring, and playtest steps.
// PLACEMENT: Invoked by milestone bootstrap / validation pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.8.0 NPC settlement affiliation foundation.
// =============================================================================

namespace CCS.Modules.NPCs.Editor
{
    public static class CCS_NpcAffiliationFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_NpcAffiliationFoundationBootstrapSetup]";
        private const string MilestoneVersion = "4.8.0";
        private const string WorldSimulationProfilePath =
            "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset";
        private const string PlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolder(CCS_NpcAffiliationContentIds.AffiliationProfilesRoot);

            CCS_NpcAffiliationProfile affiliationProfile = EnsureAffiliationProfile();
            EnsureWorldSimulationAffiliationProfile(affiliationProfile);
            EnsurePlaytestAffiliationSteps();

            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} NPC affiliation bootstrap complete ({MilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static CCS_NpcAffiliationProfile EnsureAffiliationProfile()
        {
            CCS_NpcAffiliationProfile profile = AssetDatabase.LoadAssetAtPath<CCS_NpcAffiliationProfile>(
                CCS_NpcAffiliationContentIds.DefaultAffiliationProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_NpcAffiliationProfile>();
                AssetDatabase.CreateAsset(profile, CCS_NpcAffiliationContentIds.DefaultAffiliationProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue = CCS_NpcAffiliationContentIds.DefaultAffiliationProfileId;
            serialized.FindProperty("profileDisplayName").stringValue = "Default NPC Affiliation Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Settlement, business, workforce, and region affiliation metadata for placeholder NPCs.";
            serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;
            serialized.FindProperty("defaultLoyaltyValue").intValue = 50;
            serialized.FindProperty("minimumLoyaltyValue").intValue = 0;
            serialized.FindProperty("maximumLoyaltyValue").intValue = 100;
            serialized.FindProperty("requireSettlementAffiliation").boolValue = true;
            serialized.FindProperty("requireWorkforceAffiliationForWorkers").boolValue = true;
            serialized.FindProperty("requireBusinessAffiliationForRepresentatives").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureWorldSimulationAffiliationProfile(CCS_NpcAffiliationProfile affiliationProfile)
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
            serialized.FindProperty("settlementNpcAffiliationProfile").objectReferenceValue = affiliationProfile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsurePlaytestAffiliationSteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(PlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(profile, "ccs.survival.playtest.npcaffiliation.discover", "Discover settlement for NPC affiliations",
                CCS_PlaytestStepType.DiscoverSettlementForNpcAffiliation);
            InsertStep(profile, "ccs.survival.playtest.npcaffiliation.spawn", "Spawn workforce NPC for affiliations",
                CCS_PlaytestStepType.SpawnWorkforceNpcForAffiliation);
            InsertStep(profile, "ccs.survival.playtest.npcaffiliation.verify.settlement", "Verify NPC settlement affiliation",
                CCS_PlaytestStepType.VerifyNpcSettlementAffiliation);
            InsertStep(profile, "ccs.survival.playtest.npcaffiliation.verify.workforce", "Verify NPC workforce affiliation",
                CCS_PlaytestStepType.VerifyNpcWorkforceAffiliation);
            InsertStep(profile, "ccs.survival.playtest.npcaffiliation.verify.representative", "Verify representative affiliation",
                CCS_PlaytestStepType.VerifyNpcRepresentativeAffiliation);
            InsertStep(profile, "ccs.survival.playtest.npcaffiliation.save", "Save NPC affiliation state",
                CCS_PlaytestStepType.SaveNpcAffiliationState);
            InsertStep(profile, "ccs.survival.playtest.npcaffiliation.load", "Load NPC affiliation state",
                CCS_PlaytestStepType.LoadNpcAffiliationState);
            InsertStep(profile, "ccs.survival.playtest.npcaffiliation.verify.load", "Verify NPC affiliations restored after load",
                CCS_PlaytestStepType.VerifyNpcAffiliationAfterLoad);
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
                $"{displayName}. Ctrl+Alt+F shortcut available.";
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
