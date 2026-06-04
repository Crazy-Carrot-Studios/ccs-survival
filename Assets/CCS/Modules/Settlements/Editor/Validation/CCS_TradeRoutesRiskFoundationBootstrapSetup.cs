using CCS.Modules.Settlements;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TradeRoutesRiskFoundationBootstrapSetup
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Batch-assigns route risk and freight reward modifiers to frontier trade routes.
// PLACEMENT: Invoked by milestone bootstrap / validation pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.5.0 route risk and freight bonus foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public static class CCS_TradeRoutesRiskFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_TradeRoutesRiskFoundationBootstrapSetup]";
        private const string MilestoneVersion = "3.5.0";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();

            ApplyRouteRisk(
                CCS_MultiSettlementContentIds.TradeRoutesContentRoot + "/CCS_TradeRoute_PineRidge_TradingPost.asset",
                CCS_TradeRouteRiskLevel.Low,
                1.1f,
                1.08f,
                "ccs.survival.vehicle.wagon.frontier",
                "frontier.clear");
            ApplyRouteRisk(
                CCS_MultiSettlementContentIds.TradeRoutesContentRoot + "/CCS_TradeRoute_BrokenCreek_TradingPost.asset",
                CCS_TradeRouteRiskLevel.Safe,
                1.06f,
                1.1f,
                "ccs.survival.vehicle.wagon.frontier",
                "frontier.clear");
            ApplyRouteRisk(
                CCS_MultiSettlementContentIds.TradeRoutesContentRoot + "/CCS_TradeRoute_IronRidge_TradingPost.asset",
                CCS_TradeRouteRiskLevel.Moderate,
                1.12f,
                1.12f,
                "ccs.survival.vehicle.wagon.frontier",
                "frontier.dust");
            ApplyRouteRisk(
                CCS_MultiSettlementContentIds.TradeRoutesContentRoot + "/CCS_TradeRoute_TradingPost_PineRidge.asset",
                CCS_TradeRouteRiskLevel.Safe,
                1.02f,
                1f,
                "ccs.survival.vehicle.wagon.frontier",
                "frontier.clear");
            ApplyRouteRisk(
                CCS_MultiSettlementContentIds.TradeRoutesContentRoot + "/CCS_TradeRoute_TradingPost_BrokenCreek.asset",
                CCS_TradeRouteRiskLevel.Low,
                1.02f,
                1f,
                "ccs.survival.vehicle.wagon.frontier",
                "frontier.clear");
            ApplyRouteRisk(
                CCS_MultiSettlementContentIds.TradeRoutesContentRoot + "/CCS_TradeRoute_TradingPost_IronRidge.asset",
                CCS_TradeRouteRiskLevel.Low,
                1.02f,
                1.02f,
                "ccs.survival.vehicle.wagon.frontier",
                "frontier.clear");

            CCS_TradeRouteProfile profile = AssetDatabase.LoadAssetAtPath<CCS_TradeRouteProfile>(
                CCS_MultiSettlementContentIds.TradeRoutesProfilePath);
            if (profile != null)
            {
                SerializedObject serialized = new SerializedObject(profile);
                serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(profile);
            }

            EnsurePlaytestRouteRiskSteps();

            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Route risk and freight bonus bootstrap complete ({MilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static void ApplyRouteRisk(
            string assetPath,
            CCS_TradeRouteRiskLevel riskLevel,
            float baseFreightMultiplier,
            float distanceMultiplier,
            string wagonRequirementPlaceholder,
            string routeConditionPlaceholder)
        {
            CCS_TradeRouteDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_TradeRouteDefinition>(assetPath);
            if (definition == null)
            {
                Debug.LogWarning($"{LogPrefix} Missing route asset: {assetPath}");
                return;
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("riskRating").enumValueIndex = (int)riskLevel;
            serialized.FindProperty("baseFreightMultiplier").floatValue = baseFreightMultiplier;
            serialized.FindProperty("distanceMultiplier").floatValue = distanceMultiplier;
            serialized.FindProperty("preferredWagonRequirementPlaceholder").stringValue = wagonRequirementPlaceholder;
            serialized.FindProperty("routeConditionPlaceholder").stringValue = routeConditionPlaceholder;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
        }

        private static void EnsurePlaytestRouteRiskSteps()
        {
            const string playtestProfilePath =
                "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
            CCS.Modules.Playtesting.CCS_PlaytestProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS.Modules.Playtesting.CCS_PlaytestProfile>(playtestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(profile, "ccs.survival.playtest.routerisk.accept.low", "Accept low-risk freight contract", CCS.Modules.Playtesting.CCS_PlaytestStepType.AcceptLowRiskFreightContract);
            InsertStep(profile, "ccs.survival.playtest.routerisk.complete.low", "Complete low-risk freight contract", CCS.Modules.Playtesting.CCS_PlaytestStepType.CompleteLowRiskFreightContract);
            InsertStep(profile, "ccs.survival.playtest.routerisk.verify.low.reward", "Verify low-risk base and final reward", CCS.Modules.Playtesting.CCS_PlaytestStepType.VerifyLowRiskFreightReward);
            InsertStep(profile, "ccs.survival.playtest.routerisk.accept.moderate", "Accept moderate-risk freight contract", CCS.Modules.Playtesting.CCS_PlaytestStepType.AcceptModerateRiskFreightContract);
            InsertStep(profile, "ccs.survival.playtest.routerisk.complete.moderate", "Complete moderate-risk freight contract", CCS.Modules.Playtesting.CCS_PlaytestStepType.CompleteModerateRiskFreightContract);
            InsertStep(profile, "ccs.survival.playtest.routerisk.verify.moderate.reward", "Verify moderate-risk higher final reward", CCS.Modules.Playtesting.CCS_PlaytestStepType.VerifyModerateRiskFreightHigherReward);
            InsertStep(profile, "ccs.survival.playtest.routerisk.save", "Save route risk freight state", CCS.Modules.Playtesting.CCS_PlaytestStepType.SaveRouteRiskFreightState);
            InsertStep(profile, "ccs.survival.playtest.routerisk.load", "Verify route state after load", CCS.Modules.Playtesting.CCS_PlaytestStepType.VerifyRouteRiskFreightStateAfterLoad);
            EditorUtility.SetDirty(profile);
        }

        private static void InsertStep(
            CCS.Modules.Playtesting.CCS_PlaytestProfile profile,
            string stepId,
            string displayName,
            CCS.Modules.Playtesting.CCS_PlaytestStepType stepType)
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
                $"Route risk freight playtest: {displayName}. Ctrl+Shift+Q shortcut available.";
            step.FindPropertyRelative("targetItemId").stringValue = string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
