using CCS.Modules.Playtesting;
using CCS.Modules.WorldSimulation;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_SettlementVisualGrowthFoundationBootstrapSetup
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Batch-creates visual growth profile, bootstrap scene anchors, and playtest steps.
// PLACEMENT: Invoked by milestone bootstrap / validation pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.9.0 settlement visual growth foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public static class CCS_SettlementVisualGrowthFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_SettlementVisualGrowthFoundationBootstrapSetup]";
        private const string MilestoneVersion = "3.9.0";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string WorldSimulationProfilePath =
            "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset";
        private const string PlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolder(CCS_SettlementVisualGrowthContentIds.VisualGrowthProfilesRoot);

            CCS_SettlementVisualGrowthProfile visualProfile = EnsureVisualGrowthProfile();
            EnsureWorldSimulationVisualGrowthProfile(visualProfile);
            EnsureBootstrapVisualGrowthAnchors();
            EnsurePlaytestVisualGrowthSteps();

            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Settlement visual growth bootstrap complete ({MilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static CCS_SettlementVisualGrowthProfile EnsureVisualGrowthProfile()
        {
            CCS_SettlementVisualGrowthProfile profile = AssetDatabase.LoadAssetAtPath<CCS_SettlementVisualGrowthProfile>(
                CCS_SettlementVisualGrowthContentIds.DefaultVisualGrowthProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_SettlementVisualGrowthProfile>();
                AssetDatabase.CreateAsset(profile, CCS_SettlementVisualGrowthContentIds.DefaultVisualGrowthProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue =
                CCS_SettlementVisualGrowthContentIds.DefaultVisualGrowthProfileId;
            serialized.FindProperty("profileDisplayName").stringValue = "Default Settlement Visual Growth Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Bootstrap anchors mapping settlement growth stages to visible world markers.";
            serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;

            SerializedProperty anchors = serialized.FindProperty("anchorDefinitions");
            anchors.arraySize = 27;
            int index = 0;
            SetAnchor(anchors, index++, CCS_SettlementVisualGrowthContentIds.TradingPostOutpostCampAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId, CCS_SettlementGrowthStage.Outpost,
                CCS_SettlementVisualGrowthMarkerType.CampMarker, "Camp Marker");
            SetAnchor(anchors, index++, CCS_SettlementVisualGrowthContentIds.TradingPostOutpostSupplyAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId, CCS_SettlementGrowthStage.Outpost,
                CCS_SettlementVisualGrowthMarkerType.SupplyCrates, "Supply Crates");
            SetAnchor(anchors, index++, CCS_SettlementVisualGrowthContentIds.TradingPostOutpostSignAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId, CCS_SettlementGrowthStage.Outpost,
                CCS_SettlementVisualGrowthMarkerType.SettlementSign, "Settlement Sign");
            SetAnchor(anchors, index++, CCS_SettlementVisualGrowthContentIds.TradingPostTradingSignAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId, CCS_SettlementGrowthStage.TradingPost,
                CCS_SettlementVisualGrowthMarkerType.TradingSign, "Trading Post Sign");
            SetAnchor(anchors, index++, CCS_SettlementVisualGrowthContentIds.TradingPostTradeCratesAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId, CCS_SettlementGrowthStage.TradingPost,
                CCS_SettlementVisualGrowthMarkerType.TradeCrates, "Trade Crates");
            SetAnchor(anchors, index++, CCS_SettlementVisualGrowthContentIds.TradingPostServiceHubAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId, CCS_SettlementGrowthStage.TradingPost,
                CCS_SettlementVisualGrowthMarkerType.ServiceHub, "Service Hub");
            SetAnchor(anchors, index++, CCS_SettlementVisualGrowthContentIds.TradingPostHitchingRailAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId, CCS_SettlementGrowthStage.TradingPost,
                CCS_SettlementVisualGrowthMarkerType.HitchingRail, "Hitching Rail");
            SetAnchor(anchors, index++, CCS_SettlementVisualGrowthContentIds.TradingPostFrontierTownPlaceholderAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId, CCS_SettlementGrowthStage.FrontierTown,
                CCS_SettlementVisualGrowthMarkerType.FrontierTownPlaceholder, "Frontier Town Placeholder");
            SetAnchor(anchors, index++, CCS_SettlementVisualGrowthContentIds.TradingPostEstablishedTownPlaceholderAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId, CCS_SettlementGrowthStage.EstablishedTown,
                CCS_SettlementVisualGrowthMarkerType.EstablishedTownPlaceholder, "Established Town Placeholder");

            SetAnchor(anchors, index++, CCS_SettlementVisualGrowthContentIds.BrokenCreekOutpostCampAnchorId,
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId, CCS_SettlementGrowthStage.Outpost,
                CCS_SettlementVisualGrowthMarkerType.CampMarker, "Camp Marker");
            SetAnchor(anchors, index++, CCS_SettlementVisualGrowthContentIds.BrokenCreekOutpostSupplyAnchorId,
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId, CCS_SettlementGrowthStage.Outpost,
                CCS_SettlementVisualGrowthMarkerType.SupplyCrates, "Supply Crates");
            SetAnchor(anchors, index++, CCS_SettlementVisualGrowthContentIds.BrokenCreekOutpostSignAnchorId,
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId, CCS_SettlementGrowthStage.Outpost,
                CCS_SettlementVisualGrowthMarkerType.SettlementSign, "Settlement Sign");
            SetAnchor(anchors, index++, CCS_SettlementVisualGrowthContentIds.BrokenCreekTradingSignAnchorId,
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId, CCS_SettlementGrowthStage.TradingPost,
                CCS_SettlementVisualGrowthMarkerType.TradingSign, "Trading Sign");
            SetAnchor(anchors, index++, CCS_SettlementVisualGrowthContentIds.BrokenCreekTradeCratesAnchorId,
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId, CCS_SettlementGrowthStage.TradingPost,
                CCS_SettlementVisualGrowthMarkerType.TradeCrates, "Trade Crates");
            SetAnchor(anchors, index++, CCS_SettlementVisualGrowthContentIds.BrokenCreekServiceHubAnchorId,
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId, CCS_SettlementGrowthStage.TradingPost,
                CCS_SettlementVisualGrowthMarkerType.ServiceHub, "Service Hub");

            SetAnchor(anchors, index++, CCS_SettlementVisualGrowthContentIds.IronRidgeOutpostCampAnchorId,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId, CCS_SettlementGrowthStage.Outpost,
                CCS_SettlementVisualGrowthMarkerType.CampMarker, "Camp Marker");
            SetAnchor(anchors, index++, CCS_SettlementVisualGrowthContentIds.IronRidgeOutpostSupplyAnchorId,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId, CCS_SettlementGrowthStage.Outpost,
                CCS_SettlementVisualGrowthMarkerType.SupplyCrates, "Supply Crates");
            SetAnchor(anchors, index++, CCS_SettlementVisualGrowthContentIds.IronRidgeOutpostSignAnchorId,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId, CCS_SettlementGrowthStage.Outpost,
                CCS_SettlementVisualGrowthMarkerType.SettlementSign, "Settlement Sign");
            SetAnchor(anchors, index++, CCS_SettlementVisualGrowthContentIds.IronRidgeTradingSignAnchorId,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId, CCS_SettlementGrowthStage.TradingPost,
                CCS_SettlementVisualGrowthMarkerType.TradingSign, "Trading Sign");
            SetAnchor(anchors, index++, CCS_SettlementVisualGrowthContentIds.IronRidgeTradeCratesAnchorId,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId, CCS_SettlementGrowthStage.TradingPost,
                CCS_SettlementVisualGrowthMarkerType.TradeCrates, "Trade Crates");
            SetAnchor(anchors, index++, CCS_SettlementVisualGrowthContentIds.IronRidgeServiceHubAnchorId,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId, CCS_SettlementGrowthStage.TradingPost,
                CCS_SettlementVisualGrowthMarkerType.ServiceHub, "Service Hub");

            SetAnchor(anchors, index++, CCS_SettlementVisualGrowthContentIds.PineRidgeOutpostCampAnchorId,
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId, CCS_SettlementGrowthStage.Outpost,
                CCS_SettlementVisualGrowthMarkerType.CampMarker, "Camp Marker");
            SetAnchor(anchors, index++, CCS_SettlementVisualGrowthContentIds.PineRidgeOutpostSupplyAnchorId,
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId, CCS_SettlementGrowthStage.Outpost,
                CCS_SettlementVisualGrowthMarkerType.SupplyCrates, "Supply Crates");
            SetAnchor(anchors, index++, CCS_SettlementVisualGrowthContentIds.PineRidgeOutpostSignAnchorId,
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId, CCS_SettlementGrowthStage.Outpost,
                CCS_SettlementVisualGrowthMarkerType.SettlementSign, "Settlement Sign");
            SetAnchor(anchors, index++, CCS_SettlementVisualGrowthContentIds.PineRidgeTradingSignAnchorId,
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId, CCS_SettlementGrowthStage.TradingPost,
                CCS_SettlementVisualGrowthMarkerType.TradingSign, "Trading Sign");
            SetAnchor(anchors, index++, CCS_SettlementVisualGrowthContentIds.PineRidgeTradeCratesAnchorId,
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId, CCS_SettlementGrowthStage.TradingPost,
                CCS_SettlementVisualGrowthMarkerType.TradeCrates, "Trade Crates");
            SetAnchor(anchors, index, CCS_SettlementVisualGrowthContentIds.PineRidgeServiceHubAnchorId,
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId, CCS_SettlementGrowthStage.TradingPost,
                CCS_SettlementVisualGrowthMarkerType.ServiceHub, "Service Hub");

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void SetAnchor(
            SerializedProperty anchors,
            int index,
            string anchorId,
            string settlementId,
            CCS_SettlementGrowthStage stage,
            CCS_SettlementVisualGrowthMarkerType markerType,
            string displayName)
        {
            SerializedProperty entry = anchors.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("anchorId").stringValue = anchorId;
            entry.FindPropertyRelative("settlementId").stringValue = settlementId;
            entry.FindPropertyRelative("requiredGrowthStage").enumValueIndex = (int)stage;
            entry.FindPropertyRelative("markerType").enumValueIndex = (int)markerType;
            entry.FindPropertyRelative("displayName").stringValue = displayName;
        }

        private static void EnsureWorldSimulationVisualGrowthProfile(CCS_SettlementVisualGrowthProfile visualProfile)
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
            serialized.FindProperty("settlementVisualGrowthProfile").objectReferenceValue = visualProfile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsureBootstrapVisualGrowthAnchors()
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            Transform sceneRoot = FindSceneRoot();
            if (sceneRoot == null)
            {
                EditorApplication.Exit(1);
                return;
            }

            EnsureTradingPostVisualAnchors(sceneRoot);
            EnsureCampVisualAnchors(
                sceneRoot,
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadObjectName,
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId);
            EnsureCampVisualAnchors(
                sceneRoot,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampObjectName,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId);
            EnsureCampVisualAnchors(
                sceneRoot,
                CCS_MultiSettlementContentIds.PineRidgeCampObjectName,
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureTradingPostVisualAnchors(Transform sceneRoot)
        {
            Transform tradingPost = sceneRoot.Find(CCS_SettlementContentIds.TestTradingPostObjectName);
            if (tradingPost == null)
            {
                Debug.LogError($"{LogPrefix} Missing trading post root.");
                return;
            }

            EnsureVisualAnchor(tradingPost, "CCS_VisualGrowth_OutpostCamp",
                CCS_SettlementVisualGrowthContentIds.TradingPostOutpostCampAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                CCS_SettlementGrowthStage.Outpost, CCS_SettlementVisualGrowthMarkerType.CampMarker,
                "Camp Marker", new Vector3(-8f, 1f, -6f), PrimitiveType.Cylinder, new Color(0.5f, 0.4f, 0.3f));
            EnsureVisualAnchor(tradingPost, "CCS_VisualGrowth_OutpostSupply",
                CCS_SettlementVisualGrowthContentIds.TradingPostOutpostSupplyAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                CCS_SettlementGrowthStage.Outpost, CCS_SettlementVisualGrowthMarkerType.SupplyCrates,
                "Supply Crates", new Vector3(-6f, 0.6f, -8f), PrimitiveType.Cube, new Color(0.6f, 0.45f, 0.25f));
            EnsureVisualAnchor(tradingPost, "CCS_VisualGrowth_OutpostSign",
                CCS_SettlementVisualGrowthContentIds.TradingPostOutpostSignAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                CCS_SettlementGrowthStage.Outpost, CCS_SettlementVisualGrowthMarkerType.SettlementSign,
                "Settlement Sign", new Vector3(0f, 2f, -10f), PrimitiveType.Cube, new Color(0.55f, 0.5f, 0.35f));
            EnsureVisualAnchor(tradingPost, "CCS_VisualGrowth_TradingSign",
                CCS_SettlementVisualGrowthContentIds.TradingPostTradingSignAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                CCS_SettlementGrowthStage.TradingPost, CCS_SettlementVisualGrowthMarkerType.TradingSign,
                "Trading Post Sign", new Vector3(0f, 2.5f, -12f), PrimitiveType.Cube, new Color(0.25f, 0.55f, 0.85f));
            EnsureVisualAnchor(tradingPost, "CCS_VisualGrowth_TradeCrates",
                CCS_SettlementVisualGrowthContentIds.TradingPostTradeCratesAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                CCS_SettlementGrowthStage.TradingPost, CCS_SettlementVisualGrowthMarkerType.TradeCrates,
                "Trade Crates", new Vector3(3f, 0.6f, -8f), PrimitiveType.Cube, new Color(0.7f, 0.55f, 0.3f));
            EnsureVisualAnchor(tradingPost, "CCS_VisualGrowth_ServiceHub",
                CCS_SettlementVisualGrowthContentIds.TradingPostServiceHubAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                CCS_SettlementGrowthStage.TradingPost, CCS_SettlementVisualGrowthMarkerType.ServiceHub,
                "Service Hub", new Vector3(-3f, 1.2f, -4f), PrimitiveType.Cylinder, new Color(0.35f, 0.65f, 0.75f));
            EnsureVisualAnchor(tradingPost, "CCS_VisualGrowth_HitchingRail",
                CCS_SettlementVisualGrowthContentIds.TradingPostHitchingRailAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                CCS_SettlementGrowthStage.TradingPost, CCS_SettlementVisualGrowthMarkerType.HitchingRail,
                "Hitching Rail", new Vector3(6f, 0.5f, -5f), PrimitiveType.Cylinder, new Color(0.45f, 0.35f, 0.25f));
            EnsureVisualAnchor(tradingPost, "CCS_VisualGrowth_FrontierTownPlaceholder",
                CCS_SettlementVisualGrowthContentIds.TradingPostFrontierTownPlaceholderAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                CCS_SettlementGrowthStage.FrontierTown, CCS_SettlementVisualGrowthMarkerType.FrontierTownPlaceholder,
                "Frontier Town", new Vector3(8f, 1f, 8f), PrimitiveType.Cube, new Color(0.4f, 0.4f, 0.42f));
            EnsureVisualAnchor(tradingPost, "CCS_VisualGrowth_EstablishedTownPlaceholder",
                CCS_SettlementVisualGrowthContentIds.TradingPostEstablishedTownPlaceholderAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                CCS_SettlementGrowthStage.EstablishedTown,
                CCS_SettlementVisualGrowthMarkerType.EstablishedTownPlaceholder,
                "Established Town", new Vector3(-8f, 1f, 8f), PrimitiveType.Cube, new Color(0.38f, 0.38f, 0.4f));
        }

        private static void EnsureCampVisualAnchors(Transform sceneRoot, string objectName, string settlementId)
        {
            Transform campRoot = sceneRoot.Find(objectName);
            if (campRoot == null)
            {
                Debug.LogWarning($"{LogPrefix} Missing settlement root '{objectName}'.");
                return;
            }

            string prefix = objectName.Replace("CCS_", "CCS_VisualGrowth_");
            EnsureVisualAnchor(campRoot, prefix + "_OutpostCamp",
                GetCampAnchorId(settlementId, "camp"), settlementId,
                CCS_SettlementGrowthStage.Outpost, CCS_SettlementVisualGrowthMarkerType.CampMarker,
                "Camp Marker", new Vector3(-4f, 1f, -4f), PrimitiveType.Cylinder, new Color(0.5f, 0.42f, 0.32f));
            EnsureVisualAnchor(campRoot, prefix + "_OutpostSupply",
                GetCampAnchorId(settlementId, "supply"), settlementId,
                CCS_SettlementGrowthStage.Outpost, CCS_SettlementVisualGrowthMarkerType.SupplyCrates,
                "Supply Crates", new Vector3(-2f, 0.6f, -5f), PrimitiveType.Cube, new Color(0.62f, 0.48f, 0.28f));
            EnsureVisualAnchor(campRoot, prefix + "_OutpostSign",
                GetCampAnchorId(settlementId, "sign"), settlementId,
                CCS_SettlementGrowthStage.Outpost, CCS_SettlementVisualGrowthMarkerType.SettlementSign,
                "Settlement Sign", new Vector3(0f, 1.8f, -6f), PrimitiveType.Cube, new Color(0.52f, 0.48f, 0.34f));
            EnsureVisualAnchor(campRoot, prefix + "_TradingSign",
                GetCampAnchorId(settlementId, "trading"), settlementId,
                CCS_SettlementGrowthStage.TradingPost, CCS_SettlementVisualGrowthMarkerType.TradingSign,
                "Trading Sign", new Vector3(2f, 2f, -6f), PrimitiveType.Cube, new Color(0.28f, 0.52f, 0.82f));
            EnsureVisualAnchor(campRoot, prefix + "_TradeCrates",
                GetCampAnchorId(settlementId, "crates"), settlementId,
                CCS_SettlementGrowthStage.TradingPost, CCS_SettlementVisualGrowthMarkerType.TradeCrates,
                "Trade Crates", new Vector3(4f, 0.6f, -4f), PrimitiveType.Cube, new Color(0.68f, 0.52f, 0.3f));
            EnsureVisualAnchor(campRoot, prefix + "_ServiceHub",
                GetCampAnchorId(settlementId, "hub"), settlementId,
                CCS_SettlementGrowthStage.TradingPost, CCS_SettlementVisualGrowthMarkerType.ServiceHub,
                "Service Hub", new Vector3(-1f, 1f, -2f), PrimitiveType.Cylinder, new Color(0.34f, 0.62f, 0.72f));
        }

        private static string GetCampAnchorId(string settlementId, string suffix)
        {
            if (string.Equals(settlementId, CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                return suffix switch
                {
                    "camp" => CCS_SettlementVisualGrowthContentIds.BrokenCreekOutpostCampAnchorId,
                    "supply" => CCS_SettlementVisualGrowthContentIds.BrokenCreekOutpostSupplyAnchorId,
                    "sign" => CCS_SettlementVisualGrowthContentIds.BrokenCreekOutpostSignAnchorId,
                    "trading" => CCS_SettlementVisualGrowthContentIds.BrokenCreekTradingSignAnchorId,
                    "crates" => CCS_SettlementVisualGrowthContentIds.BrokenCreekTradeCratesAnchorId,
                    _ => CCS_SettlementVisualGrowthContentIds.BrokenCreekServiceHubAnchorId
                };
            }

            if (string.Equals(settlementId, CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                return suffix switch
                {
                    "camp" => CCS_SettlementVisualGrowthContentIds.IronRidgeOutpostCampAnchorId,
                    "supply" => CCS_SettlementVisualGrowthContentIds.IronRidgeOutpostSupplyAnchorId,
                    "sign" => CCS_SettlementVisualGrowthContentIds.IronRidgeOutpostSignAnchorId,
                    "trading" => CCS_SettlementVisualGrowthContentIds.IronRidgeTradingSignAnchorId,
                    "crates" => CCS_SettlementVisualGrowthContentIds.IronRidgeTradeCratesAnchorId,
                    _ => CCS_SettlementVisualGrowthContentIds.IronRidgeServiceHubAnchorId
                };
            }

            return suffix switch
            {
                "camp" => CCS_SettlementVisualGrowthContentIds.PineRidgeOutpostCampAnchorId,
                "supply" => CCS_SettlementVisualGrowthContentIds.PineRidgeOutpostSupplyAnchorId,
                "sign" => CCS_SettlementVisualGrowthContentIds.PineRidgeOutpostSignAnchorId,
                "trading" => CCS_SettlementVisualGrowthContentIds.PineRidgeTradingSignAnchorId,
                "crates" => CCS_SettlementVisualGrowthContentIds.PineRidgeTradeCratesAnchorId,
                _ => CCS_SettlementVisualGrowthContentIds.PineRidgeServiceHubAnchorId
            };
        }

        private static void EnsureVisualAnchor(
            Transform parent,
            string objectName,
            string anchorId,
            string settlementId,
            CCS_SettlementGrowthStage requiredStage,
            CCS_SettlementVisualGrowthMarkerType markerType,
            string displayName,
            Vector3 localPosition,
            PrimitiveType primitiveType,
            Color markerColor)
        {
            Transform existing = parent.Find(objectName);
            GameObject root = existing != null ? existing.gameObject : new GameObject(objectName);
            if (existing == null)
            {
                root.transform.SetParent(parent, false);
            }

            root.transform.localPosition = localPosition;

            CCS_SettlementVisualGrowthAnchor anchor = root.GetComponent<CCS_SettlementVisualGrowthAnchor>();
            if (anchor == null)
            {
                anchor = root.AddComponent<CCS_SettlementVisualGrowthAnchor>();
            }

            Transform markerTransform = root.transform.Find("CCS_VisualGrowth_Marker");
            GameObject markerObject = markerTransform != null
                ? markerTransform.gameObject
                : GameObject.CreatePrimitive(primitiveType);
            if (markerTransform == null)
            {
                markerObject.name = "CCS_VisualGrowth_Marker";
                markerObject.transform.SetParent(root.transform, false);
            }

            markerObject.transform.localPosition = Vector3.zero;
            markerObject.transform.localScale = new Vector3(1f, 1.1f, 1f);
            Collider markerCollider = markerObject.GetComponent<Collider>();
            if (markerCollider != null)
            {
                Object.DestroyImmediate(markerCollider);
            }

            Rigidbody markerRigidbody = markerObject.GetComponent<Rigidbody>();
            if (markerRigidbody != null)
            {
                Object.DestroyImmediate(markerRigidbody);
            }

            Renderer markerRenderer = markerObject.GetComponent<Renderer>();
            if (markerRenderer != null)
            {
                markerRenderer.sharedMaterial.color = markerColor;
            }

            CCS_SettlementVisualGrowthMarker marker = markerObject.GetComponent<CCS_SettlementVisualGrowthMarker>();
            if (marker == null)
            {
                marker = markerObject.AddComponent<CCS_SettlementVisualGrowthMarker>();
            }

            Transform labelTransform = root.transform.Find("CCS_VisualGrowth_LabelRoot");
            GameObject labelRoot = labelTransform != null
                ? labelTransform.gameObject
                : new GameObject("CCS_VisualGrowth_LabelRoot");
            if (labelTransform == null)
            {
                labelRoot.transform.SetParent(root.transform, false);
            }

            labelRoot.transform.localPosition = Vector3.zero;
            CCS_SettlementVisualGrowthLabel label = labelRoot.GetComponent<CCS_SettlementVisualGrowthLabel>();
            if (label == null)
            {
                label = labelRoot.AddComponent<CCS_SettlementVisualGrowthLabel>();
            }

            SerializedObject serializedAnchor = new SerializedObject(anchor);
            serializedAnchor.FindProperty("anchorId").stringValue = anchorId;
            serializedAnchor.FindProperty("settlementId").stringValue = settlementId;
            serializedAnchor.FindProperty("requiredGrowthStage").enumValueIndex = (int)requiredStage;
            serializedAnchor.FindProperty("markerType").enumValueIndex = (int)markerType;
            serializedAnchor.FindProperty("displayName").stringValue = displayName;
            serializedAnchor.FindProperty("growthMarker").objectReferenceValue = marker;
            serializedAnchor.FindProperty("growthLabel").objectReferenceValue = label;
            serializedAnchor.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(root);
        }

        private static void EnsurePlaytestVisualGrowthSteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(PlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(profile, "ccs.survival.playtest.visualgrowth.discover", "Discover settlement for visual growth",
                CCS_PlaytestStepType.DiscoverSettlementForVisualGrowth);
            InsertStep(profile, "ccs.survival.playtest.visualgrowth.verify.outpost", "Verify Outpost visual markers",
                CCS_PlaytestStepType.VerifyOutpostVisualMarkers);
            InsertStep(profile, "ccs.survival.playtest.visualgrowth.trigger.tradingpost", "Trigger TradingPost growth visuals",
                CCS_PlaytestStepType.TriggerTradingPostGrowthForVisuals);
            InsertStep(profile, "ccs.survival.playtest.visualgrowth.verify.tradingpost", "Verify TradingPost markers active",
                CCS_PlaytestStepType.VerifyTradingPostVisualMarkersActive);
            InsertStep(profile, "ccs.survival.playtest.visualgrowth.save", "Save visual growth state",
                CCS_PlaytestStepType.SaveVisualGrowthState);
            InsertStep(profile, "ccs.survival.playtest.visualgrowth.load", "Verify visual growth restored after load",
                CCS_PlaytestStepType.VerifyVisualGrowthAfterLoad);
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
                $"{displayName}. Ctrl+Shift+Z shortcut available.";
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
