using CCS.Modules.Playtesting;
using CCS.Modules.Settlements;
using CCS.Modules.WorldSimulation;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_NpcSocialFoundationBootstrapSetup
// CATEGORY: Modules / NPCs / Editor / Validation
// PURPOSE: Batch-creates social profile, world wiring, scene anchors, and playtest steps.
// PLACEMENT: Invoked by milestone bootstrap / validation pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.0.0 NPC social presence foundation.
// =============================================================================

namespace CCS.Modules.NPCs.Editor
{
    public static class CCS_NpcSocialFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_NpcSocialFoundationBootstrapSetup]";
        private const string MilestoneVersion = "5.0.0";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string WorldSimulationProfilePath =
            "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset";
        private const string PlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolder(CCS_NpcSocialContentIds.SocialProfilesRoot);

            CCS_NpcSocialProfile socialProfile = EnsureSocialProfile();
            EnsureWorldSimulationSocialProfile(socialProfile);
            EnsureBootstrapSocialAnchors();
            EnsurePlaytestSocialSteps();

            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} NPC social presence bootstrap complete ({MilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static CCS_NpcSocialProfile EnsureSocialProfile()
        {
            CCS_NpcSocialProfile profile = AssetDatabase.LoadAssetAtPath<CCS_NpcSocialProfile>(
                CCS_NpcSocialContentIds.DefaultSocialProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_NpcSocialProfile>();
                AssetDatabase.CreateAsset(profile, CCS_NpcSocialContentIds.DefaultSocialProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue = CCS_NpcSocialContentIds.DefaultSocialProfileId;
            serialized.FindProperty("profileDisplayName").stringValue = "Default NPC Social Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Settlement social gathering areas for leisure-period NPC presence.";
            serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;
            serialized.FindProperty("socialArrivalTolerance").floatValue = 1.25f;
            serialized.FindProperty("requireSocialAnchorForLeisure").boolValue = true;

            SerializedProperty definitions = serialized.FindProperty("gatheringDefinitions");
            definitions.arraySize = 5;
            WriteGatheringDefinition(
                definitions.GetArrayElementAtIndex(0),
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                CCS_NpcSocialContentIds.TradingPostCampfireAnchorId,
                "Campfire",
                10);
            WriteGatheringDefinition(
                definitions.GetArrayElementAtIndex(1),
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                CCS_NpcSocialContentIds.TradingPostHitchingRailAnchorId,
                "Hitching Rail",
                8);
            WriteGatheringDefinition(
                definitions.GetArrayElementAtIndex(2),
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId,
                CCS_NpcSocialContentIds.BrokenCreekCommunityFireAnchorId,
                "Community Fire",
                10);
            WriteGatheringDefinition(
                definitions.GetArrayElementAtIndex(3),
                CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId,
                CCS_NpcSocialContentIds.IronRidgeMineFireAnchorId,
                "Mine Fire",
                10);
            WriteGatheringDefinition(
                definitions.GetArrayElementAtIndex(4),
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId,
                CCS_NpcSocialContentIds.PineRidgeLumberCampFireAnchorId,
                "Lumber Camp Fire",
                10);

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureWorldSimulationSocialProfile(CCS_NpcSocialProfile socialProfile)
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
            serialized.FindProperty("settlementNpcSocialProfile").objectReferenceValue = socialProfile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsureBootstrapSocialAnchors()
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
                EnsureSocialAnchorObject(
                    tradingPost,
                    "CCS_SettlementSocial_Campfire",
                    CCS_NpcSocialContentIds.TradingPostCampfireAnchorId,
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                    "Campfire",
                    new Vector3(6f, 0.4f, -2f),
                    new Color(0.95f, 0.45f, 0.2f, 1f));
                EnsureSocialAnchorObject(
                    tradingPost,
                    "CCS_SettlementSocial_HitchingRail",
                    CCS_NpcSocialContentIds.TradingPostHitchingRailAnchorId,
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                    "Hitching Rail",
                    new Vector3(-6f, 0.4f, 2f),
                    new Color(0.55f, 0.35f, 0.2f, 1f));
            }

            EnsureCampSocialAnchor(
                sceneRoot,
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadObjectName,
                "CCS_SettlementSocial_CommunityFire",
                CCS_NpcSocialContentIds.BrokenCreekCommunityFireAnchorId,
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId,
                "Community Fire",
                new Vector3(2f, 0.4f, -2f),
                new Color(0.95f, 0.5f, 0.15f, 1f));
            EnsureCampSocialAnchor(
                sceneRoot,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampObjectName,
                "CCS_SettlementSocial_MineFire",
                CCS_NpcSocialContentIds.IronRidgeMineFireAnchorId,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId,
                "Mine Fire",
                new Vector3(2f, 0.4f, -2f),
                new Color(0.85f, 0.35f, 0.1f, 1f));
            EnsureCampSocialAnchor(
                sceneRoot,
                CCS_MultiSettlementContentIds.PineRidgeCampObjectName,
                "CCS_SettlementSocial_LumberCampFire",
                CCS_NpcSocialContentIds.PineRidgeLumberCampFireAnchorId,
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId,
                "Lumber Camp Fire",
                new Vector3(2f, 0.4f, -2f),
                new Color(0.7f, 0.45f, 0.2f, 1f));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureCampSocialAnchor(
            Transform sceneRoot,
            string settlementObjectName,
            string objectName,
            string anchorId,
            string settlementId,
            string displayName,
            Vector3 localPosition,
            Color markerColor)
        {
            Transform settlementRoot = sceneRoot.Find(settlementObjectName);
            if (settlementRoot == null)
            {
                Debug.LogWarning($"{LogPrefix} Missing settlement root {settlementObjectName}.");
                return;
            }

            EnsureSocialAnchorObject(
                settlementRoot,
                objectName,
                anchorId,
                settlementId,
                displayName,
                localPosition,
                markerColor);
        }

        private static void EnsureSocialAnchorObject(
            Transform parent,
            string objectName,
            string anchorId,
            string settlementId,
            string displayName,
            Vector3 localPosition,
            Color markerColor)
        {
            Transform existing = parent.Find(objectName);
            GameObject root = existing != null ? existing.gameObject : new GameObject(objectName);
            if (existing == null)
            {
                root.transform.SetParent(parent, false);
            }

            root.transform.localPosition = localPosition;

            CCS_SettlementSocialAnchor anchor = root.GetComponent<CCS_SettlementSocialAnchor>();
            if (anchor == null)
            {
                anchor = root.AddComponent<CCS_SettlementSocialAnchor>();
            }

            Transform markerTransform = root.transform.Find("CCS_SettlementSocial_Marker");
            GameObject markerObject = markerTransform != null
                ? markerTransform.gameObject
                : GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            if (markerTransform == null)
            {
                markerObject.name = "CCS_SettlementSocial_Marker";
                markerObject.transform.SetParent(root.transform, false);
            }

            markerObject.transform.localPosition = Vector3.zero;
            markerObject.transform.localScale = new Vector3(1.2f, 0.6f, 1.2f);
            Collider markerCollider = markerObject.GetComponent<Collider>();
            if (markerCollider != null)
            {
                Object.DestroyImmediate(markerCollider);
            }

            Renderer markerRenderer = markerObject.GetComponent<Renderer>();
            if (markerRenderer != null)
            {
                markerRenderer.sharedMaterial.color = markerColor;
            }

            CCS_SettlementSocialMarker marker = markerObject.GetComponent<CCS_SettlementSocialMarker>();
            if (marker == null)
            {
                marker = markerObject.AddComponent<CCS_SettlementSocialMarker>();
            }

            Transform labelTransform = root.transform.Find("CCS_SettlementSocial_LabelRoot");
            GameObject labelRoot = labelTransform != null
                ? labelTransform.gameObject
                : new GameObject("CCS_SettlementSocial_LabelRoot");
            if (labelTransform == null)
            {
                labelRoot.transform.SetParent(root.transform, false);
            }

            labelRoot.transform.localPosition = Vector3.zero;
            CCS_SettlementSocialLabel label = labelRoot.GetComponent<CCS_SettlementSocialLabel>();
            if (label == null)
            {
                label = labelRoot.AddComponent<CCS_SettlementSocialLabel>();
            }

            SerializedObject serializedAnchor = new SerializedObject(anchor);
            serializedAnchor.FindProperty("anchorId").stringValue = anchorId;
            serializedAnchor.FindProperty("settlementId").stringValue = settlementId;
            serializedAnchor.FindProperty("displayName").stringValue = displayName;
            serializedAnchor.FindProperty("socialMarker").objectReferenceValue = marker;
            serializedAnchor.FindProperty("socialLabel").objectReferenceValue = label;
            serializedAnchor.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(root);
        }

        private static void EnsurePlaytestSocialSteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(PlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(profile, "ccs.survival.playtest.npcsocial.discover", "Discover settlement for NPC social presence",
                CCS_PlaytestStepType.DiscoverSettlementForNpcSocial);
            InsertStep(profile, "ccs.survival.playtest.npcsocial.spawn", "Spawn population for NPC social presence",
                CCS_PlaytestStepType.SpawnPopulationForNpcSocial);
            InsertStep(profile, "ccs.survival.playtest.npcsocial.force.leisure", "Force leisure period for NPC social presence",
                CCS_PlaytestStepType.ForceLeisurePeriodForNpcSocial);
            InsertStep(profile, "ccs.survival.playtest.npcsocial.verify.workers", "Verify workers gather for NPC social presence",
                CCS_PlaytestStepType.VerifyWorkersGatherForNpcSocial);
            InsertStep(profile, "ccs.survival.playtest.npcsocial.verify.representatives", "Verify representatives gather for NPC social presence",
                CCS_PlaytestStepType.VerifyRepresentativesGatherForNpcSocial);
            InsertStep(profile, "ccs.survival.playtest.npcsocial.verify.groups", "Verify NPC social group count",
                CCS_PlaytestStepType.VerifyNpcSocialGroupCount);
            InsertStep(profile, "ccs.survival.playtest.npcsocial.save", "Save NPC social state",
                CCS_PlaytestStepType.SaveNpcSocialState);
            InsertStep(profile, "ccs.survival.playtest.npcsocial.load", "Load NPC social state",
                CCS_PlaytestStepType.LoadNpcSocialState);
            InsertStep(profile, "ccs.survival.playtest.npcsocial.verify.load", "Verify NPC social presence after load",
                CCS_PlaytestStepType.VerifyNpcSocialAfterLoad);
            EditorUtility.SetDirty(profile);
        }

        private static void WriteGatheringDefinition(
            SerializedProperty definitionProperty,
            string settlementId,
            string anchorId,
            string displayName,
            int priority)
        {
            definitionProperty.FindPropertyRelative("settlementId").stringValue = settlementId;
            definitionProperty.FindPropertyRelative("anchorId").stringValue = anchorId;
            definitionProperty.FindPropertyRelative("displayName").stringValue = displayName;
            definitionProperty.FindPropertyRelative("priority").intValue = priority;
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
                $"{displayName}. Ctrl+Alt+P shortcut available.";
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
