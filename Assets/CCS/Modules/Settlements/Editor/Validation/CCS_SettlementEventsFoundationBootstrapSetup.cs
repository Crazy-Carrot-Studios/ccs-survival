using CCS.Modules.NPCs;
using CCS.Modules.Playtesting;
using CCS.Modules.WorldSimulation;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_SettlementEventsFoundationBootstrapSetup
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Batch-creates event profile, world wiring, scene markers, and playtest steps.
// PLACEMENT: Invoked by milestone bootstrap / validation pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.1.0 dynamic settlement events foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public static class CCS_SettlementEventsFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_SettlementEventsFoundationBootstrapSetup]";
        private const string MilestoneVersion = "5.1.0";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string WorldSimulationProfilePath =
            "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset";
        private const string PlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolder(CCS_SettlementEventContentIds.EventProfilesRoot);

            CCS_SettlementEventProfile eventProfile = EnsureEventProfile();
            EnsureWorldSimulationEventProfile(eventProfile);
            EnsureBootstrapEventAnchors();
            EnsurePlaytestEventSteps();

            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Settlement events bootstrap complete ({MilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static CCS_SettlementEventProfile EnsureEventProfile()
        {
            CCS_SettlementEventProfile profile = AssetDatabase.LoadAssetAtPath<CCS_SettlementEventProfile>(
                CCS_SettlementEventContentIds.DefaultEventProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_SettlementEventProfile>();
                AssetDatabase.CreateAsset(profile, CCS_SettlementEventContentIds.DefaultEventProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue = CCS_SettlementEventContentIds.DefaultEventProfileId;
            serialized.FindProperty("profileDisplayName").stringValue = "Default Settlement Event Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Simulation-driven settlement events with temporary modifiers and dev markers.";
            serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;
            serialized.FindProperty("evaluationIntervalHours").intValue = 6;
            serialized.FindProperty("allowSimulationGeneration").boolValue = true;

            SerializedProperty definitions = serialized.FindProperty("eventDefinitions");
            definitions.arraySize = 5;
            WriteDefinition(
                definitions.GetArrayElementAtIndex(0),
                CCS_SettlementEventContentIds.TradingPostMarketDayEventId,
                CCS_SettlementEventType.MarketDay,
                "Market Day",
                new[] { CCS_SettlementGrowthContentIds.TradingPostSettlementId },
                new[] { (int)CCS_SettlementType.TradingPost },
                10,
                20f,
                2,
                0,
                "Busy day. Market's in full swing.",
                CCS_NpcSocialContentIds.TradingPostCampfireAnchorId,
                CCS_SettlementEventContentIds.TradingPostMarketDayAnchorId);
            WriteDefinition(
                definitions.GetArrayElementAtIndex(1),
                CCS_SettlementEventContentIds.TradingPostSupplyShipmentEventId,
                CCS_SettlementEventType.SupplyShipment,
                "Supply Shipment",
                new[] { CCS_SettlementGrowthContentIds.TradingPostSettlementId },
                new[] { (int)CCS_SettlementType.TradingPost },
                15,
                25f,
                2,
                1,
                "Fresh supplies rolling in from the frontier routes.",
                CCS_NpcSocialContentIds.TradingPostHitchingRailAnchorId,
                CCS_SettlementEventContentIds.TradingPostSupplyShipmentAnchorId);
            WriteDefinition(
                definitions.GetArrayElementAtIndex(2),
                CCS_SettlementEventContentIds.BrokenCreekHarvestFestivalEventId,
                CCS_SettlementEventType.HarvestFestival,
                "Harvest Festival",
                new[] { CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId },
                new[] { (int)CCS_SettlementType.Homestead },
                8,
                18f,
                1,
                0,
                "Harvest season keeps the farmstead busy.",
                CCS_NpcSocialContentIds.BrokenCreekCommunityFireAnchorId,
                CCS_SettlementEventContentIds.BrokenCreekHarvestFestivalAnchorId);
            WriteDefinition(
                definitions.GetArrayElementAtIndex(3),
                CCS_SettlementEventContentIds.IronRidgeMiningShipmentEventId,
                CCS_SettlementEventType.MiningShipment,
                "Mining Shipment",
                new[] { CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId },
                new[] { (int)CCS_SettlementType.MiningCamp },
                8,
                18f,
                1,
                0,
                "Ore wagons are rolling out of Iron Ridge.",
                CCS_NpcSocialContentIds.IronRidgeMineFireAnchorId,
                CCS_SettlementEventContentIds.IronRidgeMiningShipmentAnchorId);
            WriteDefinition(
                definitions.GetArrayElementAtIndex(4),
                CCS_SettlementEventContentIds.PineRidgeTimberDeliveryEventId,
                CCS_SettlementEventType.TimberDelivery,
                "Timber Delivery",
                new[] { CCS_MultiSettlementContentIds.PineRidgeCampSettlementId },
                new[] { (int)CCS_SettlementType.Other },
                8,
                18f,
                1,
                0,
                "Timber crews are hauling fresh loads down from Pine Ridge.",
                CCS_NpcSocialContentIds.PineRidgeLumberCampFireAnchorId,
                CCS_SettlementEventContentIds.PineRidgeTimberDeliveryAnchorId);

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureWorldSimulationEventProfile(CCS_SettlementEventProfile eventProfile)
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
            serialized.FindProperty("settlementEventProfile").objectReferenceValue = eventProfile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsureBootstrapEventAnchors()
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            Transform sceneRoot = FindSceneRoot();
            if (sceneRoot == null)
            {
                EditorApplication.Exit(1);
                return;
            }

            Transform tradingPost = sceneRoot.Find(CCS_SettlementContentIds.TestTradingPostObjectName);
            if (tradingPost != null)
            {
                EnsureEventAnchorObject(
                    tradingPost,
                    "CCS_SettlementEvent_MarketDay",
                    CCS_SettlementEventContentIds.TradingPostMarketDayAnchorId,
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                    new Vector3(-4f, 0.4f, 4f),
                    new Color(0.35f, 0.75f, 1f, 1f));
                EnsureEventAnchorObject(
                    tradingPost,
                    "CCS_SettlementEvent_SupplyShipment",
                    CCS_SettlementEventContentIds.TradingPostSupplyShipmentAnchorId,
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                    new Vector3(4f, 0.4f, 4f),
                    new Color(0.45f, 0.65f, 0.95f, 1f));
            }

            Transform brokenCreek = sceneRoot.Find("CCS_BootstrapZone_Regions/CCS_BrokenCreekFarmstead");
            if (brokenCreek == null)
            {
                brokenCreek = FindDeep(sceneRoot, "CCS_BrokenCreekFarmstead");
            }

            if (brokenCreek != null)
            {
                EnsureEventAnchorObject(
                    brokenCreek,
                    "CCS_SettlementEvent_HarvestFestival",
                    CCS_SettlementEventContentIds.BrokenCreekHarvestFestivalAnchorId,
                    CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId,
                    new Vector3(0f, 0.4f, 3f),
                    new Color(0.55f, 0.85f, 0.35f, 1f));
            }

            Transform ironRidge = FindDeep(sceneRoot, "CCS_IronRidgeMiningCamp");
            if (ironRidge != null)
            {
                EnsureEventAnchorObject(
                    ironRidge,
                    "CCS_SettlementEvent_MiningShipment",
                    CCS_SettlementEventContentIds.IronRidgeMiningShipmentAnchorId,
                    CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId,
                    new Vector3(0f, 0.4f, 3f),
                    new Color(0.65f, 0.65f, 0.75f, 1f));
            }

            Transform pineRidge = FindDeep(sceneRoot, "CCS_PineRidgeCamp");
            if (pineRidge != null)
            {
                EnsureEventAnchorObject(
                    pineRidge,
                    "CCS_SettlementEvent_TimberDelivery",
                    CCS_SettlementEventContentIds.PineRidgeTimberDeliveryAnchorId,
                    CCS_MultiSettlementContentIds.PineRidgeCampSettlementId,
                    new Vector3(0f, 0.4f, 3f),
                    new Color(0.45f, 0.55f, 0.35f, 1f));
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsurePlaytestEventSteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(PlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(profile, "ccs.survival.playtest.settlementevents.discover", "Discover settlement for events",
                CCS_PlaytestStepType.DiscoverSettlementForSettlementEvents);
            InsertStep(profile, "ccs.survival.playtest.settlementevents.force.marketday", "Force Market Day event",
                CCS_PlaytestStepType.ForceMarketDayForSettlementEvents);
            InsertStep(profile, "ccs.survival.playtest.settlementevents.verify.marker", "Verify settlement event marker",
                CCS_PlaytestStepType.VerifySettlementEventMarker);
            InsertStep(profile, "ccs.survival.playtest.settlementevents.verify.modifiers", "Verify settlement event modifiers",
                CCS_PlaytestStepType.VerifySettlementEventModifiers);
            InsertStep(profile, "ccs.survival.playtest.settlementevents.verify.dialogue", "Verify settlement event dialogue line",
                CCS_PlaytestStepType.VerifySettlementEventDialogueLine);
            InsertStep(profile, "ccs.survival.playtest.settlementevents.save", "Save settlement event state",
                CCS_PlaytestStepType.SaveSettlementEventState);
            InsertStep(profile, "ccs.survival.playtest.settlementevents.load", "Load settlement event state",
                CCS_PlaytestStepType.LoadSettlementEventState);
            InsertStep(profile, "ccs.survival.playtest.settlementevents.verify.load", "Verify settlement event after load",
                CCS_PlaytestStepType.VerifySettlementEventAfterLoad);
            EditorUtility.SetDirty(profile);
        }

        private static void WriteDefinition(
            SerializedProperty definitionProperty,
            string eventId,
            CCS_SettlementEventType eventType,
            string displayName,
            string[] settlementIds,
            int[] settlementTypes,
            int minimumPopulation,
            float minimumProsperity,
            int minimumActiveBusinesses,
            int minimumTradeRouteUsage,
            string dialogueAppendLine,
            string preferredSocialAnchorId,
            string eventMarkerAnchorId)
        {
            definitionProperty.FindPropertyRelative("eventId").stringValue = eventId;
            definitionProperty.FindPropertyRelative("eventType").intValue = (int)eventType;
            definitionProperty.FindPropertyRelative("displayName").stringValue = displayName;
            SerializedProperty eligibleSettlementIds = definitionProperty.FindPropertyRelative("eligibleSettlementIds");
            eligibleSettlementIds.arraySize = settlementIds.Length;
            for (int index = 0; index < settlementIds.Length; index++)
            {
                eligibleSettlementIds.GetArrayElementAtIndex(index).stringValue = settlementIds[index];
            }

            SerializedProperty eligibleSettlementTypes = definitionProperty.FindPropertyRelative("eligibleSettlementTypes");
            eligibleSettlementTypes.arraySize = settlementTypes.Length;
            for (int index = 0; index < settlementTypes.Length; index++)
            {
                eligibleSettlementTypes.GetArrayElementAtIndex(index).intValue = settlementTypes[index];
            }

            definitionProperty.FindPropertyRelative("minimumPopulation").intValue = minimumPopulation;
            definitionProperty.FindPropertyRelative("minimumProsperity").floatValue = minimumProsperity;
            definitionProperty.FindPropertyRelative("minimumActiveBusinesses").intValue = minimumActiveBusinesses;
            definitionProperty.FindPropertyRelative("minimumTradeRouteUsage").intValue = minimumTradeRouteUsage;
            definitionProperty.FindPropertyRelative("durationHours").intValue = 24;
            definitionProperty.FindPropertyRelative("prosperityBonus").floatValue = 2f;
            definitionProperty.FindPropertyRelative("supplyBonus").floatValue = 5f;
            definitionProperty.FindPropertyRelative("contractRewardMultiplier").floatValue = 1.05f;
            definitionProperty.FindPropertyRelative("reputationGainMultiplier").floatValue = 1.05f;
            definitionProperty.FindPropertyRelative("dialogueAppendLine").stringValue = dialogueAppendLine;
            definitionProperty.FindPropertyRelative("preferredSocialAnchorId").stringValue = preferredSocialAnchorId;
            definitionProperty.FindPropertyRelative("eventMarkerAnchorId").stringValue = eventMarkerAnchorId;
        }

        private static void EnsureEventAnchorObject(
            Transform parent,
            string objectName,
            string anchorId,
            string settlementId,
            Vector3 localPosition,
            Color markerColor)
        {
            Transform existing = parent.Find(objectName);
            GameObject root = existing != null ? existing.gameObject : new GameObject(objectName);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = localPosition;

            CCS_SettlementEventAnchor anchor = root.GetComponent<CCS_SettlementEventAnchor>();
            if (anchor == null)
            {
                anchor = root.AddComponent<CCS_SettlementEventAnchor>();
            }

            SerializedObject serializedAnchor = new SerializedObject(anchor);
            serializedAnchor.FindProperty("anchorId").stringValue = anchorId;
            serializedAnchor.FindProperty("settlementId").stringValue = settlementId;
            serializedAnchor.FindProperty("idleDisplayName").stringValue = "Event Area";
            serializedAnchor.ApplyModifiedPropertiesWithoutUndo();

            Transform markerTransform = root.transform.Find("Marker");
            GameObject markerObject = markerTransform != null
                ? markerTransform.gameObject
                : GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            markerObject.name = "Marker";
            markerObject.transform.SetParent(root.transform, false);
            markerObject.transform.localPosition = Vector3.zero;
            CCS_SettlementEventMarker marker = markerObject.GetComponent<CCS_SettlementEventMarker>();
            if (marker == null)
            {
                marker = markerObject.AddComponent<CCS_SettlementEventMarker>();
            }

            SerializedObject serializedMarker = new SerializedObject(marker);
            serializedMarker.FindProperty("markerColor").colorValue = markerColor;
            serializedMarker.ApplyModifiedPropertiesWithoutUndo();

            Transform labelTransform = root.transform.Find("Label");
            GameObject labelObject = labelTransform != null ? labelTransform.gameObject : new GameObject("Label");
            labelObject.transform.SetParent(root.transform, false);
            labelObject.transform.localPosition = Vector3.zero;
            if (labelObject.GetComponent<CCS_SettlementEventLabel>() == null)
            {
                labelObject.AddComponent<CCS_SettlementEventLabel>();
            }

            EditorUtility.SetDirty(root);
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
                $"{displayName}. Ctrl+Alt+E shortcut available.";
            step.FindPropertyRelative("targetItemId").stringValue = string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Transform FindSceneRoot()
        {
            GameObject sceneRoot = GameObject.Find("CCS_BuildVerificationScene");
            if (sceneRoot != null)
            {
                return sceneRoot.transform;
            }

            sceneRoot = GameObject.Find("CCS_SurvivalBootstrapScene");
            return sceneRoot != null ? sceneRoot.transform : null;
        }

        private static Transform FindDeep(Transform root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            if (string.Equals(root.name, objectName, System.StringComparison.Ordinal))
            {
                return root;
            }

            for (int index = 0; index < root.childCount; index++)
            {
                Transform match = FindDeep(root.GetChild(index), objectName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
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
