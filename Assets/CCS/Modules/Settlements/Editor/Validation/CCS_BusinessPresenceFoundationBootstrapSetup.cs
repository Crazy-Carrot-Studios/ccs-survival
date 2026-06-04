using CCS.Modules.Playtesting;
using CCS.Modules.WorldSimulation;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_BusinessPresenceFoundationBootstrapSetup
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Batch-creates presence profile, bootstrap scene anchors, and playtest steps.
// PLACEMENT: Invoked by milestone bootstrap / validation pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.8.0 — visible business presence foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public static class CCS_BusinessPresenceFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_BusinessPresenceFoundationBootstrapSetup]";
        private const string MilestoneVersion = "3.8.0";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string WorldSimulationProfilePath =
            "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset";
        private const string PlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolder(CCS_BusinessPresenceContentIds.PresenceProfilesRoot);

            CCS_BusinessPresenceProfile presenceProfile = EnsurePresenceProfile();
            EnsureWorldSimulationPresenceProfile(presenceProfile);
            EnsureBootstrapPresenceAnchors();
            EnsurePlaytestPresenceSteps();

            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Business presence bootstrap complete ({MilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static CCS_BusinessPresenceProfile EnsurePresenceProfile()
        {
            CCS_BusinessPresenceProfile profile = AssetDatabase.LoadAssetAtPath<CCS_BusinessPresenceProfile>(
                CCS_BusinessPresenceContentIds.DefaultPresenceProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_BusinessPresenceProfile>();
                AssetDatabase.CreateAsset(profile, CCS_BusinessPresenceContentIds.DefaultPresenceProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue =
                CCS_BusinessPresenceContentIds.DefaultPresenceProfileId;
            serialized.FindProperty("profileDisplayName").stringValue = "Default Business Presence Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Bootstrap anchors mapping business activation to visible world markers.";
            serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;

            SerializedProperty anchors = serialized.FindProperty("anchorDefinitions");
            anchors.arraySize = 8;
            SetAnchor(anchors, 0, CCS_BusinessPresenceContentIds.TradingPostGeneralStoreAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId, "ccs.survival.business.generalstore",
                CCS_BusinessType.GeneralStore, "General Store");
            SetAnchor(anchors, 1, CCS_BusinessPresenceContentIds.TradingPostStableAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId, "ccs.survival.business.stable",
                CCS_BusinessType.Stable, "Stable");
            SetAnchor(anchors, 2, CCS_BusinessPresenceContentIds.TradingPostGunsmithAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId, "ccs.survival.business.gunsmith",
                CCS_BusinessType.Gunsmith, "Gunsmith");
            SetAnchor(anchors, 3, CCS_BusinessPresenceContentIds.TradingPostBankAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId, "ccs.survival.business.bank",
                CCS_BusinessType.Bank, "Bank");
            SetAnchor(anchors, 4, CCS_BusinessPresenceContentIds.TradingPostContractOfficeAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId, "ccs.survival.business.contractoffice",
                CCS_BusinessType.ContractOffice, "Contract Office");
            SetAnchor(anchors, 5, CCS_BusinessPresenceContentIds.BrokenCreekFarmSupplyAnchorId,
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId, "ccs.survival.business.farmsupply",
                CCS_BusinessType.FarmSupply, "Farm Supply");
            SetAnchor(anchors, 6, CCS_BusinessPresenceContentIds.IronRidgeMiningSupplierAnchorId,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId, "ccs.survival.business.miningsupplier",
                CCS_BusinessType.MiningSupplier, "Mining Supplier");
            SetAnchor(anchors, 7, CCS_BusinessPresenceContentIds.PineRidgeLumberYardAnchorId,
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId, "ccs.survival.business.lumberyard",
                CCS_BusinessType.LumberYard, "Lumber Yard");

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void SetAnchor(
            SerializedProperty anchors,
            int index,
            string anchorId,
            string settlementId,
            string businessId,
            CCS_BusinessType businessType,
            string displayName)
        {
            SerializedProperty entry = anchors.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("anchorId").stringValue = anchorId;
            entry.FindPropertyRelative("settlementId").stringValue = settlementId;
            entry.FindPropertyRelative("businessId").stringValue = businessId;
            entry.FindPropertyRelative("businessType").enumValueIndex = (int)businessType;
            entry.FindPropertyRelative("displayName").stringValue = displayName;
        }

        private static void EnsureWorldSimulationPresenceProfile(CCS_BusinessPresenceProfile presenceProfile)
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
            serialized.FindProperty("settlementBusinessPresenceProfile").objectReferenceValue = presenceProfile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsureBootstrapPresenceAnchors()
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            Transform sceneRoot = FindSceneRoot();
            if (sceneRoot == null)
            {
                EditorApplication.Exit(1);
                return;
            }

            EnsureTradingPostAnchors(sceneRoot);
            EnsureCampAnchor(
                sceneRoot,
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadObjectName,
                CCS_BusinessPresenceContentIds.BrokenCreekFarmSupplyAnchorId,
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId,
                "ccs.survival.business.farmsupply",
                CCS_BusinessType.FarmSupply,
                "Farm Supply",
                new Vector3(2f, 1.2f, -3f),
                new Color(0.75f, 0.7f, 0.35f));
            EnsureCampAnchor(
                sceneRoot,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampObjectName,
                CCS_BusinessPresenceContentIds.IronRidgeMiningSupplierAnchorId,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId,
                "ccs.survival.business.miningsupplier",
                CCS_BusinessType.MiningSupplier,
                "Mining Supplier",
                new Vector3(2f, 1.2f, -3f),
                new Color(0.55f, 0.55f, 0.6f));
            EnsureCampAnchor(
                sceneRoot,
                CCS_MultiSettlementContentIds.PineRidgeCampObjectName,
                CCS_BusinessPresenceContentIds.PineRidgeLumberYardAnchorId,
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId,
                "ccs.survival.business.lumberyard",
                CCS_BusinessType.LumberYard,
                "Lumber Yard",
                new Vector3(2f, 1.2f, -3f),
                new Color(0.45f, 0.65f, 0.35f));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureTradingPostAnchors(Transform sceneRoot)
        {
            Transform tradingPost = sceneRoot.Find(CCS_SettlementContentIds.TestTradingPostObjectName);
            if (tradingPost == null)
            {
                Debug.LogError($"{LogPrefix} Missing trading post root.");
                return;
            }

            EnsureAnchorObject(tradingPost, "CCS_BusinessPresence_GeneralStore",
                CCS_BusinessPresenceContentIds.TradingPostGeneralStoreAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                "ccs.survival.business.generalstore", CCS_BusinessType.GeneralStore, "General Store",
                new Vector3(0f, 1.2f, -2.5f), PrimitiveType.Cube, new Color(0.55f, 0.45f, 0.3f));
            EnsureAnchorObject(tradingPost, "CCS_BusinessPresence_Stable",
                CCS_BusinessPresenceContentIds.TradingPostStableAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                "ccs.survival.business.stable", CCS_BusinessType.Stable, "Stable",
                new Vector3(4f, 1.2f, -2.5f), PrimitiveType.Cylinder, new Color(0.45f, 0.55f, 0.75f));
            EnsureAnchorObject(tradingPost, "CCS_BusinessPresence_Gunsmith",
                CCS_BusinessPresenceContentIds.TradingPostGunsmithAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                "ccs.survival.business.gunsmith", CCS_BusinessType.Gunsmith, "Gunsmith",
                new Vector3(8f, 1.2f, -2.5f), PrimitiveType.Cube, new Color(0.65f, 0.35f, 0.35f));
            EnsureAnchorObject(tradingPost, "CCS_BusinessPresence_Bank",
                CCS_BusinessPresenceContentIds.TradingPostBankAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                "ccs.survival.business.bank", CCS_BusinessType.Bank, "Bank",
                new Vector3(16f, 1.2f, -2.5f), PrimitiveType.Cube, new Color(0.75f, 0.75f, 0.35f));
            EnsureAnchorObject(tradingPost, "CCS_BusinessPresence_ContractOffice",
                CCS_BusinessPresenceContentIds.TradingPostContractOfficeAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                "ccs.survival.business.contractoffice", CCS_BusinessType.ContractOffice, "Contract Office",
                new Vector3(12f, 1.2f, -2.5f), PrimitiveType.Capsule, new Color(0.35f, 0.55f, 0.85f));
        }

        private static void EnsureCampAnchor(
            Transform sceneRoot,
            string settlementObjectName,
            string anchorId,
            string settlementId,
            string businessId,
            CCS_BusinessType businessType,
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

            EnsureAnchorObject(
                settlementRoot,
                "CCS_BusinessPresence_" + businessType,
                anchorId,
                settlementId,
                businessId,
                businessType,
                displayName,
                localPosition,
                PrimitiveType.Cylinder,
                markerColor);
        }

        private static void EnsureAnchorObject(
            Transform parent,
            string objectName,
            string anchorId,
            string settlementId,
            string businessId,
            CCS_BusinessType businessType,
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

            CCS_BusinessPresenceAnchor anchor = root.GetComponent<CCS_BusinessPresenceAnchor>();
            if (anchor == null)
            {
                anchor = root.AddComponent<CCS_BusinessPresenceAnchor>();
            }

            Transform markerTransform = root.transform.Find("CCS_BusinessPresence_Marker");
            GameObject markerObject = markerTransform != null
                ? markerTransform.gameObject
                : GameObject.CreatePrimitive(primitiveType);
            if (markerTransform == null)
            {
                markerObject.name = "CCS_BusinessPresence_Marker";
                markerObject.transform.SetParent(root.transform, false);
            }

            markerObject.transform.localPosition = Vector3.zero;
            markerObject.transform.localScale = new Vector3(1f, 1.2f, 1f);
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

            CCS_BusinessPresenceMarker marker = markerObject.GetComponent<CCS_BusinessPresenceMarker>();
            if (marker == null)
            {
                marker = markerObject.AddComponent<CCS_BusinessPresenceMarker>();
            }

            Transform labelTransform = root.transform.Find("CCS_BusinessPresence_LabelRoot");
            GameObject labelRoot = labelTransform != null ? labelTransform.gameObject : new GameObject("CCS_BusinessPresence_LabelRoot");
            if (labelTransform == null)
            {
                labelRoot.transform.SetParent(root.transform, false);
            }

            labelRoot.transform.localPosition = Vector3.zero;
            CCS_BusinessPresenceLabel label = labelRoot.GetComponent<CCS_BusinessPresenceLabel>();
            if (label == null)
            {
                label = labelRoot.AddComponent<CCS_BusinessPresenceLabel>();
            }

            SerializedObject serializedAnchor = new SerializedObject(anchor);
            serializedAnchor.FindProperty("anchorId").stringValue = anchorId;
            serializedAnchor.FindProperty("settlementId").stringValue = settlementId;
            serializedAnchor.FindProperty("businessId").stringValue = businessId;
            serializedAnchor.FindProperty("businessType").enumValueIndex = (int)businessType;
            serializedAnchor.FindProperty("displayName").stringValue = displayName;
            serializedAnchor.FindProperty("syncLinkedServicePointVisual").boolValue = true;
            serializedAnchor.FindProperty("presenceMarker").objectReferenceValue = marker;
            serializedAnchor.FindProperty("presenceLabel").objectReferenceValue = label;
            serializedAnchor.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(root);
        }

        private static void EnsurePlaytestPresenceSteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(PlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(profile, "ccs.survival.playtest.businesspresence.discover", "Discover settlement for business presence",
                CCS_PlaytestStepType.DiscoverSettlementForBusinessPresence);
            InsertStep(profile, "ccs.survival.playtest.businesspresence.verify.markers", "Verify business markers exist",
                CCS_PlaytestStepType.VerifyBusinessMarkersExist);
            InsertStep(profile, "ccs.survival.playtest.businesspresence.activate", "Trigger business activation for markers",
                CCS_PlaytestStepType.TriggerBusinessActivationForMarkers);
            InsertStep(profile, "ccs.survival.playtest.businesspresence.verify.active", "Verify marker shows active",
                CCS_PlaytestStepType.VerifyBusinessMarkerActive);
            InsertStep(profile, "ccs.survival.playtest.businesspresence.save", "Save business presence state",
                CCS_PlaytestStepType.SaveBusinessPresenceState);
            InsertStep(profile, "ccs.survival.playtest.businesspresence.load", "Verify marker restored after load",
                CCS_PlaytestStepType.VerifyBusinessPresenceAfterLoad);
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
                $"{displayName}. Ctrl+Shift+V shortcut available.";
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
