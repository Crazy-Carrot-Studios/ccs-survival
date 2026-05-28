#if UNITY_EDITOR
using CCS.Survival.Environment.Hazards;
using CCS.Survival.Environment.VitalsZones;
using CCS.Survival.Testing.Traversal;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_VitalsModifierZoneSceneSetup_Editor
// CATEGORY: Survival / Editor
// PURPOSE: Builds Phase 1H.5 overlapping vitals modifier zone testbed in the bootstrap scene.
// PLACEMENT: Editor only. Menu CCS/Survival/Setup Phase 1H.5 Vitals Modifier Testbed.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Idempotent setup. Safe to re-run. Does not modify traversal enable defaults.
// =============================================================================

namespace CCS.Survival.Editor
{
    public static class CCS_VitalsModifierZoneSceneSetup_Editor
    {
        private const string ScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string RootName = "CCS_PrototypeVitalsZonesRoot";

        [MenuItem("CCS/Survival/Setup Phase 1H.5 Vitals Modifier Testbed")]
        public static void SetupPhase1H5Testbed()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Transform root = EnsureRoot();

            ClearChildren(root);
            CreateBroadZones(root);
            CreateNestedModifierZones(root);
            EnsureReceiversOnPlayerAndAgent();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[CCS Survival] Phase 1H.5 vitals modifier testbed setup complete.");
        }

        private static Transform EnsureRoot()
        {
            GameObject existing = GameObject.Find(RootName);
            if (existing != null)
            {
                return existing.transform;
            }

            GameObject rootObject = new GameObject(RootName);
            return rootObject.transform;
        }

        private static void ClearChildren(Transform root)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(root.GetChild(i).gameObject);
            }
        }

        private static void CreateBroadZones(Transform root)
        {
            CreateBoxModifierZone(
                root,
                "VZ_WeatherCold_Box",
                new Vector3(0f, 1.25f, 2.8f),
                new Vector3(10f, 3f, 5f),
                CCS_SurvivalVitalsModifierType.TemperatureDecrease,
                0.35f,
                "Weather cold box",
                new Color(0.25f, 0.55f, 1f, 0.25f));

            CreateBoxModifierZone(
                root,
                "VZ_ToxicCloud_Box",
                new Vector3(0f, 1.5f, 7.2f),
                new Vector3(5f, 3f, 4.5f),
                CCS_SurvivalVitalsModifierType.ExposureIncrease,
                0.25f,
                "Toxic cloud box",
                new Color(0.2f, 0.9f, 0.25f, 0.25f));

            CreateBoxModifierZone(
                root,
                "VZ_Shelter_Box",
                new Vector3(-3f, 1f, 3f),
                new Vector3(4f, 2.5f, 4f),
                CCS_SurvivalVitalsModifierType.HungerRestore,
                4f,
                "Shelter restore box",
                new Color(0.35f, 0.85f, 0.95f, 0.25f));
        }

        private static void CreateNestedModifierZones(Transform root)
        {
            CreateCapsuleModifierZone(
                root,
                "VZ_HungerDrain_Capsule",
                new Vector3(0f, 1f, 2.2f),
                new Vector3(1.6f, 2f, 1.6f),
                CCS_SurvivalVitalsModifierType.HungerDrain,
                6f,
                "Hunger drain capsule",
                new Color(0.85f, 0.35f, 0.15f, 0.35f));

            CreateCapsuleModifierZone(
                root,
                "VZ_HungerRestore_Capsule",
                new Vector3(-3f, 1f, 3f),
                new Vector3(1.4f, 2f, 1.4f),
                CCS_SurvivalVitalsModifierType.HungerRestore,
                8f,
                "Hunger restore capsule",
                new Color(0.35f, 0.85f, 0.25f, 0.35f));

            CreateCapsuleModifierZone(
                root,
                "VZ_ThirstDrain_Cylinder",
                new Vector3(0.5f, 1f, 4.5f),
                new Vector3(1.2f, 2.4f, 1.2f),
                CCS_SurvivalVitalsModifierType.ThirstDrain,
                7f,
                "Thirst drain cylinder",
                new Color(0.2f, 0.45f, 0.95f, 0.35f));

            CreateCapsuleModifierZone(
                root,
                "VZ_ThirstRestore_Cylinder",
                new Vector3(-2.5f, 1f, 2.5f),
                new Vector3(1.2f, 2.2f, 1.2f),
                CCS_SurvivalVitalsModifierType.ThirstRestore,
                8f,
                "Thirst restore cylinder",
                new Color(0.25f, 0.85f, 0.95f, 0.35f));

            CreateCapsuleModifierZone(
                root,
                "VZ_StaminaDrain_Capsule",
                new Vector3(0f, 1.5f, 7f),
                new Vector3(2f, 2.5f, 2f),
                CCS_SurvivalVitalsModifierType.StaminaDrain,
                10f,
                "Stamina drain capsule",
                new Color(0.75f, 0.75f, 0.2f, 0.35f));

            CreateCapsuleModifierZone(
                root,
                "VZ_StaminaRestore_Capsule",
                new Vector3(-3.5f, 1f, 3.5f),
                new Vector3(1.3f, 2f, 1.3f),
                CCS_SurvivalVitalsModifierType.StaminaRestore,
                12f,
                "Stamina restore capsule",
                new Color(0.95f, 0.9f, 0.3f, 0.35f));

            CreateCapsuleModifierZone(
                root,
                "VZ_ExposureRecovery_Cylinder",
                new Vector3(-3f, 1.2f, 2.8f),
                new Vector3(1.5f, 2.2f, 1.5f),
                CCS_SurvivalVitalsModifierType.ExposureRecovery,
                0.5f,
                "Exposure recovery cylinder",
                new Color(0.45f, 0.85f, 0.75f, 0.35f));
        }

        private static void CreateBoxModifierZone(
            Transform parent,
            string objectName,
            Vector3 worldPosition,
            Vector3 worldScale,
            CCS_SurvivalVitalsModifierType modifierType,
            float ratePerSecond,
            string displayName,
            Color visualColor)
        {
            GameObject zoneObject = new GameObject(objectName);
            zoneObject.transform.SetParent(parent, false);
            zoneObject.transform.position = worldPosition;
            zoneObject.transform.localScale = worldScale;

            BoxCollider collider = zoneObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;

            CCS_SurvivalVitalsModifierZone zone = zoneObject.AddComponent<CCS_SurvivalVitalsModifierZone>();
            ConfigureZone(zone, modifierType, ratePerSecond, displayName);
            CreateVisualPrimitive(zoneObject.transform, PrimitiveType.Cube, visualColor);
        }

        private static void CreateCapsuleModifierZone(
            Transform parent,
            string objectName,
            Vector3 worldPosition,
            Vector3 worldScale,
            CCS_SurvivalVitalsModifierType modifierType,
            float ratePerSecond,
            string displayName,
            Color visualColor)
        {
            GameObject zoneObject = new GameObject(objectName);
            zoneObject.transform.SetParent(parent, false);
            zoneObject.transform.position = worldPosition;
            zoneObject.transform.localScale = worldScale;

            CapsuleCollider collider = zoneObject.AddComponent<CapsuleCollider>();
            collider.isTrigger = true;
            collider.direction = 1;
            collider.height = 1f;
            collider.radius = 0.5f;

            CCS_SurvivalVitalsModifierZone zone = zoneObject.AddComponent<CCS_SurvivalVitalsModifierZone>();
            ConfigureZone(zone, modifierType, ratePerSecond, displayName);
            CreateVisualPrimitive(zoneObject.transform, PrimitiveType.Capsule, visualColor);
        }

        private static void ConfigureZone(
            CCS_SurvivalVitalsModifierZone zone,
            CCS_SurvivalVitalsModifierType modifierType,
            float ratePerSecond,
            string displayName)
        {
            SerializedObject serializedZone = new SerializedObject(zone);
            serializedZone.FindProperty("modifierType").enumValueIndex = (int)modifierType;
            serializedZone.FindProperty("ratePerSecond").floatValue = ratePerSecond;
            serializedZone.FindProperty("zoneDisplayName").stringValue = displayName;
            serializedZone.FindProperty("isZoneEnabled").boolValue = true;
            serializedZone.FindProperty("drawZoneGizmo").boolValue = true;
            serializedZone.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateVisualPrimitive(Transform parent, PrimitiveType primitiveType, Color color)
        {
            GameObject visual = GameObject.CreatePrimitive(primitiveType);
            visual.name = $"{parent.name}_Visual";
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;

            if (visual.TryGetComponent(out Collider primitiveCollider))
            {
                Object.DestroyImmediate(primitiveCollider);
            }

            if (visual.TryGetComponent(out Renderer renderer))
            {
                Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.color = color;
                renderer.sharedMaterial = material;
            }
        }

        private static void EnsureReceiversOnPlayerAndAgent()
        {
            GameObject playerRoot = GameObject.Find("CCS_PlayerRoot");
            if (playerRoot != null)
            {
                EnsureVitalsZoneReceiver(playerRoot, applyToVitals: true, telemetry: false);
                EnsureHazardReceiver(playerRoot, applyToVitals: true, telemetry: false);
            }

            GameObject traversalAgent = GameObject.Find("CCS_TraversalTestAgent");
            if (traversalAgent != null)
            {
                EnsureVitalsZoneReceiver(traversalAgent, applyToVitals: false, telemetry: true);
                EnsureHazardReceiver(traversalAgent, applyToVitals: false, telemetry: true);
            }
        }

        private static void EnsureVitalsZoneReceiver(
            GameObject target,
            bool applyToVitals,
            bool telemetry)
        {
            CCS_SurvivalVitalsZoneReceiver receiver = target.GetComponent<CCS_SurvivalVitalsZoneReceiver>();
            if (receiver == null)
            {
                receiver = target.AddComponent<CCS_SurvivalVitalsZoneReceiver>();
            }

            SerializedObject serializedReceiver = new SerializedObject(receiver);
            serializedReceiver.FindProperty("applyToSurvivalVitals").boolValue = applyToVitals;
            serializedReceiver.FindProperty("enableVitalsZoneTelemetryLogging").boolValue = telemetry;
            serializedReceiver.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureHazardReceiver(
            GameObject target,
            bool applyToVitals,
            bool telemetry)
        {
            CCS_SurvivalHazardReceiver receiver = target.GetComponent<CCS_SurvivalHazardReceiver>();
            if (receiver == null)
            {
                receiver = target.AddComponent<CCS_SurvivalHazardReceiver>();
            }

            SerializedObject serializedReceiver = new SerializedObject(receiver);
            serializedReceiver.FindProperty("applyToSurvivalVitals").boolValue = applyToVitals;
            serializedReceiver.FindProperty("enableHazardTelemetryLogging").boolValue = telemetry;
            serializedReceiver.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
