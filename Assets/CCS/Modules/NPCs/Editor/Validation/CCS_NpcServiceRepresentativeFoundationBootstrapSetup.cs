using CCS.Modules.Playtesting;
using CCS.Modules.Settlements;
using CCS.Modules.WorldSimulation;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcServiceRepresentativeFoundationBootstrapSetup
// CATEGORY: Modules / NPCs / Editor / Validation
// PURPOSE: Batch-creates representative profile, world wiring, and playtest steps.
// PLACEMENT: Invoked by milestone bootstrap / validation pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-06-05
// NOTES: Milestone 4.3.0 NPC service representatives foundation.
// =============================================================================

namespace CCS.Modules.NPCs.Editor
{
    public static class CCS_NpcServiceRepresentativeFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_NpcServiceRepresentativeFoundationBootstrapSetup]";
        private const string MilestoneVersion = "4.3.0";
        private const string WorldSimulationProfilePath =
            "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset";
        private const string PlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
        private const string TradingPostSettlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId;
        private const string GeneralStoreBusinessId = "ccs.survival.business.generalstore";
        private const string BankBusinessId = "ccs.survival.business.bank";
        private const string StableBusinessId = "ccs.survival.business.stable";
        private const string GunsmithBusinessId = "ccs.survival.business.gunsmith";
        private const string BlacksmithBusinessId = "ccs.survival.business.blacksmith";
        private const string ContractOfficeBusinessId = "ccs.survival.business.contractoffice";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolder(CCS_NpcServiceRepresentativeContentIds.RepresentativeProfilesRoot);

            CCS_NpcServiceRepresentativeProfile representativeProfile = EnsureRepresentativeProfile();
            EnsureWorldSimulationRepresentativeProfile(representativeProfile);
            EnsurePlaytestNpcServiceRepresentativeSteps();

            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} NPC service representative bootstrap complete ({MilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static CCS_NpcServiceRepresentativeProfile EnsureRepresentativeProfile()
        {
            CCS_NpcServiceRepresentativeProfile profile = AssetDatabase.LoadAssetAtPath<CCS_NpcServiceRepresentativeProfile>(
                CCS_NpcServiceRepresentativeContentIds.DefaultRepresentativeProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_NpcServiceRepresentativeProfile>();
                AssetDatabase.CreateAsset(profile, CCS_NpcServiceRepresentativeContentIds.DefaultRepresentativeProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue =
                CCS_NpcServiceRepresentativeContentIds.DefaultRepresentativeProfileId;
            serialized.FindProperty("profileDisplayName").stringValue = "Default NPC Service Representative Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Maps active settlement businesses to named NPC service representatives and service points.";
            serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;

            SerializedProperty definitions = serialized.FindProperty("representativeDefinitions");
            definitions.arraySize = 6;
            SetRepresentativeDefinition(
                definitions,
                0,
                TradingPostSettlementId,
                GeneralStoreBusinessId,
                CCS_SettlementContentIds.GeneralStoreServicePointId,
                CCS_NpcRoleType.Merchant,
                "Frontier Merchant");
            SetRepresentativeDefinition(
                definitions,
                1,
                TradingPostSettlementId,
                BankBusinessId,
                CCS_SettlementContentIds.BankServicePointId,
                CCS_NpcRoleType.Banker,
                "Frontier Banker");
            SetRepresentativeDefinition(
                definitions,
                2,
                TradingPostSettlementId,
                StableBusinessId,
                CCS_SettlementContentIds.StableServicePointId,
                CCS_NpcRoleType.StableHand,
                "Settlement Stable Hand");
            SetRepresentativeDefinition(
                definitions,
                3,
                TradingPostSettlementId,
                GunsmithBusinessId,
                CCS_SettlementContentIds.GunsmithServicePointId,
                CCS_NpcRoleType.Gunsmith,
                "Settlement Gunsmith");
            SetRepresentativeDefinition(
                definitions,
                4,
                TradingPostSettlementId,
                BlacksmithBusinessId,
                CCS_SettlementContentIds.BlacksmithServicePointId,
                CCS_NpcRoleType.Blacksmith,
                "Settlement Blacksmith");
            SetRepresentativeDefinition(
                definitions,
                5,
                TradingPostSettlementId,
                ContractOfficeBusinessId,
                CCS_SettlementContentIds.ContractBoardServicePointId,
                CCS_NpcRoleType.Clerk,
                "Settlement Clerk");

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void SetRepresentativeDefinition(
            SerializedProperty definitions,
            int index,
            string settlementId,
            string businessId,
            string servicePointId,
            CCS_NpcRoleType requiredRole,
            string displayTitle)
        {
            SerializedProperty entry = definitions.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("representativeId").stringValue =
                CCS_NpcServiceRepresentativeUtility.BuildRepresentativeId(settlementId, businessId);
            entry.FindPropertyRelative("settlementId").stringValue = settlementId;
            entry.FindPropertyRelative("businessId").stringValue = businessId;
            entry.FindPropertyRelative("servicePointId").stringValue = servicePointId;
            entry.FindPropertyRelative("requiredRole").enumValueIndex = (int)requiredRole;
            entry.FindPropertyRelative("displayTitle").stringValue = displayTitle;
            entry.FindPropertyRelative("fallbackToServicePoint").boolValue = true;
            entry.FindPropertyRelative("populationPresenceAnchorId").stringValue = string.Empty;
        }

        private static void EnsureWorldSimulationRepresentativeProfile(
            CCS_NpcServiceRepresentativeProfile representativeProfile)
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
            serialized.FindProperty("settlementNpcServiceRepresentativeProfile").objectReferenceValue =
                representativeProfile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsurePlaytestNpcServiceRepresentativeSteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(PlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(profile, "ccs.survival.playtest.npcservicerepresentative.discover",
                "Discover trading post for NPC service representatives",
                CCS_PlaytestStepType.DiscoverSettlementForNpcServiceRepresentatives);
            InsertStep(profile, "ccs.survival.playtest.npcservicerepresentative.trigger",
                "Trigger population and business presence for representatives",
                CCS_PlaytestStepType.TriggerPopulationAndBusinessForNpcServiceRepresentatives);
            InsertStep(profile, "ccs.survival.playtest.npcservicerepresentative.verify.generalstore",
                "Verify General Store representative assigned",
                CCS_PlaytestStepType.VerifyGeneralStoreRepresentativeAssigned);
            InsertStep(profile, "ccs.survival.playtest.npcservicerepresentative.interact.generalstore",
                "Interact with General Store representative",
                CCS_PlaytestStepType.InteractGeneralStoreRepresentative);
            InsertStep(profile, "ccs.survival.playtest.npcservicerepresentative.verify.vendor",
                "Verify General Store representative vendor route",
                CCS_PlaytestStepType.VerifyGeneralStoreRepresentativeVendorRoute);
            InsertStep(profile, "ccs.survival.playtest.npcservicerepresentative.verify.bank",
                "Verify Bank representative assigned",
                CCS_PlaytestStepType.VerifyBankRepresentativeAssigned);
            InsertStep(profile, "ccs.survival.playtest.npcservicerepresentative.interact.bank",
                "Interact with Bank representative",
                CCS_PlaytestStepType.InteractBankRepresentative);
            InsertStep(profile, "ccs.survival.playtest.npcservicerepresentative.verify.banking",
                "Verify Bank representative banking route",
                CCS_PlaytestStepType.VerifyBankRepresentativeBankRoute);
            InsertStep(profile, "ccs.survival.playtest.npcservicerepresentative.save",
                "Save NPC service representative state",
                CCS_PlaytestStepType.SaveNpcServiceRepresentativeState);
            InsertStep(profile, "ccs.survival.playtest.npcservicerepresentative.load",
                "Load NPC service representative state",
                CCS_PlaytestStepType.LoadNpcServiceRepresentativeState);
            InsertStep(profile, "ccs.survival.playtest.npcservicerepresentative.verify.load",
                "Verify representative assignment restored after load",
                CCS_PlaytestStepType.VerifyNpcServiceRepresentativeAfterLoad);
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
                $"{displayName}. Ctrl+Alt+R shortcut available.";
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
