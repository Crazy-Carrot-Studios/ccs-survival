using CCS.Modules.Playtesting;
using CCS.Modules.WorldSimulation;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementNewsFoundationBootstrapSetup
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Batch-creates news profile, world wiring, and playtest steps.
// PLACEMENT: Invoked by milestone bootstrap / validation pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.2.0 settlement news and rumors foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public static class CCS_SettlementNewsFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_SettlementNewsFoundationBootstrapSetup]";
        private const string MilestoneVersion = "5.2.0";
        private const string WorldSimulationProfilePath =
            "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset";
        private const string PlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolder(CCS_SettlementNewsContentIds.NewsProfilesRoot);

            CCS_SettlementNewsProfile newsProfile = EnsureNewsProfile();
            EnsureWorldSimulationNewsProfile(newsProfile);
            EnsurePlaytestNewsSteps();

            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Settlement news bootstrap complete ({MilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static CCS_SettlementNewsProfile EnsureNewsProfile()
        {
            CCS_SettlementNewsProfile profile = AssetDatabase.LoadAssetAtPath<CCS_SettlementNewsProfile>(
                CCS_SettlementNewsContentIds.DefaultNewsProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_SettlementNewsProfile>();
                AssetDatabase.CreateAsset(profile, CCS_SettlementNewsContentIds.DefaultNewsProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue = CCS_SettlementNewsContentIds.DefaultNewsProfileId;
            serialized.FindProperty("profileDisplayName").stringValue = "Default Settlement News Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Event-driven settlement news headlines and trade-route rumor propagation.";
            serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;
            serialized.FindProperty("maxRecentNewsEntries").intValue = 3;
            serialized.FindProperty("evaluationIntervalHours").intValue = 6;

            SerializedProperty definitions = serialized.FindProperty("newsDefinitions");
            definitions.arraySize = 5;
            WriteDefinition(
                definitions.GetArrayElementAtIndex(0),
                CCS_SettlementNewsContentIds.MarketDayNewsDefinitionId,
                CCS_SettlementNewsType.MarketDay,
                "Frontier Trading Post is hosting a Market Day.",
                "Word is the trading post market is busy today.",
                3,
                1);
            WriteDefinition(
                definitions.GetArrayElementAtIndex(1),
                CCS_SettlementNewsContentIds.SupplyShipmentNewsDefinitionId,
                CCS_SettlementNewsType.SupplyShipment,
                "Frontier Trading Post received a major supply shipment.",
                "Folks say fresh supplies arrived at the trading post.",
                3,
                1);
            WriteDefinition(
                definitions.GetArrayElementAtIndex(2),
                CCS_SettlementNewsContentIds.HarvestFestivalNewsDefinitionId,
                CCS_SettlementNewsType.HarvestFestival,
                "Broken Creek celebrates a successful harvest.",
                "Word is the harvest was good in Broken Creek.",
                3,
                1);
            WriteDefinition(
                definitions.GetArrayElementAtIndex(3),
                CCS_SettlementNewsContentIds.MiningShipmentNewsDefinitionId,
                CCS_SettlementNewsType.MiningShipment,
                "Iron Ridge reports a major ore shipment.",
                "Folks are talking about a big shipment from Iron Ridge.",
                3,
                1);
            WriteDefinition(
                definitions.GetArrayElementAtIndex(4),
                CCS_SettlementNewsContentIds.TimberDeliveryNewsDefinitionId,
                CCS_SettlementNewsType.TimberDelivery,
                "Pine Ridge reports a major timber delivery.",
                "Word is Pine Ridge crews hauled fresh timber loads.",
                3,
                1);

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureWorldSimulationNewsProfile(CCS_SettlementNewsProfile newsProfile)
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
            serialized.FindProperty("settlementNewsProfile").objectReferenceValue = newsProfile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsurePlaytestNewsSteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(PlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(profile, "ccs.survival.playtest.settlementnews.discover", "Discover settlements for news",
                CCS_PlaytestStepType.DiscoverSettlementsForSettlementNews);
            InsertStep(profile, "ccs.survival.playtest.settlementnews.force.event", "Force settlement event for news",
                CCS_PlaytestStepType.ForceEventForSettlementNews);
            InsertStep(profile, "ccs.survival.playtest.settlementnews.verify.created", "Verify settlement news created",
                CCS_PlaytestStepType.VerifySettlementNewsCreated);
            InsertStep(profile, "ccs.survival.playtest.settlementnews.verify.board", "Verify contract board news",
                CCS_PlaytestStepType.VerifyContractBoardSettlementNews);
            InsertStep(profile, "ccs.survival.playtest.settlementnews.verify.dialogue", "Verify settlement rumor dialogue",
                CCS_PlaytestStepType.VerifySettlementNewsRumorDialogue);
            InsertStep(profile, "ccs.survival.playtest.settlementnews.force.propagation", "Force settlement news propagation",
                CCS_PlaytestStepType.ForceSettlementNewsPropagation);
            InsertStep(profile, "ccs.survival.playtest.settlementnews.verify.propagation", "Verify settlement news propagated",
                CCS_PlaytestStepType.VerifySettlementNewsPropagated);
            InsertStep(profile, "ccs.survival.playtest.settlementnews.save", "Save settlement news state",
                CCS_PlaytestStepType.SaveSettlementNewsState);
            InsertStep(profile, "ccs.survival.playtest.settlementnews.load", "Load settlement news state",
                CCS_PlaytestStepType.LoadSettlementNewsState);
            InsertStep(profile, "ccs.survival.playtest.settlementnews.verify.load", "Verify settlement news after load",
                CCS_PlaytestStepType.VerifySettlementNewsAfterLoad);
            EditorUtility.SetDirty(profile);
        }

        private static void WriteDefinition(
            SerializedProperty definitionProperty,
            string definitionId,
            CCS_SettlementNewsType newsType,
            string headlineTemplate,
            string rumorLineTemplate,
            int newsDurationDays,
            int propagationDelayDays)
        {
            definitionProperty.FindPropertyRelative("definitionId").stringValue = definitionId;
            definitionProperty.FindPropertyRelative("newsType").intValue = (int)newsType;
            definitionProperty.FindPropertyRelative("headlineTemplate").stringValue = headlineTemplate;
            definitionProperty.FindPropertyRelative("rumorLineTemplate").stringValue = rumorLineTemplate;
            definitionProperty.FindPropertyRelative("newsDurationDays").intValue = newsDurationDays;
            definitionProperty.FindPropertyRelative("propagationDelayDays").intValue = propagationDelayDays;
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
                $"{displayName}. Ctrl+Alt+N shortcut available.";
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

            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(folderName))
            {
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }
    }
}
