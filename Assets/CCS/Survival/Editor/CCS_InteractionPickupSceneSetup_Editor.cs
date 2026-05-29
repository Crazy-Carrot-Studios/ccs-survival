#if UNITY_EDITOR
using CCS.Core;
using CCS.Survival.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_InteractionPickupSceneSetup_Editor
// CATEGORY: Survival / Editor
// PURPOSE: Builds Phase 1I interaction scanner/input on player and prototype pickups.
// PLACEMENT: Editor only. Menu CCS/Survival/Setup Phase 1I Interaction Pickups.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Idempotent setup. Safe to re-run. Does not modify traversal enable defaults.
// =============================================================================

namespace CCS.Survival.Editor
{
    public static class CCS_InteractionPickupSceneSetup_Editor
    {
        private const string ScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string InputActionsPath = "Assets/CCS/Survival/Settings/Input/CCS_Survival_InputActions.inputactions";
        private const string RootName = "CCS_PrototypePickupsRoot";

        [MenuItem("CCS/Survival/Setup Phase 1I Interaction Pickups")]
        public static void SetupPhase1IInteractionPickups()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            CCS_RuntimeHost runtimeHost = Object.FindFirstObjectByType<CCS_RuntimeHost>();
            InputActionReference interactActionReference = LoadInteractActionReference();

            EnsurePlayerInteractionComponents(runtimeHost, interactActionReference);

            Transform root = EnsureRoot();
            ClearChildren(root);
            CreatePrototypePickups(root, runtimeHost);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[CCS Survival] Phase 1I interaction pickup setup complete.");
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

        private static void EnsurePlayerInteractionComponents(
            CCS_RuntimeHost runtimeHost,
            InputActionReference interactActionReference)
        {
            GameObject playerRoot = GameObject.Find("CCS_PlayerRoot");
            if (playerRoot == null)
            {
                Debug.LogError("[CCS Survival] CCS_PlayerRoot not found. Interaction setup aborted.");
                return;
            }

            CCS_SurvivalInteractionScanner scanner = playerRoot.GetComponent<CCS_SurvivalInteractionScanner>();
            if (scanner == null)
            {
                scanner = playerRoot.AddComponent<CCS_SurvivalInteractionScanner>();
            }

            SerializedObject serializedScanner = new SerializedObject(scanner);
            serializedScanner.FindProperty("scanRadius").floatValue = 2.25f;
            serializedScanner.FindProperty("runtimeHost").objectReferenceValue = runtimeHost;
            serializedScanner.FindProperty("enableDebugLogs").boolValue = false;
            serializedScanner.ApplyModifiedPropertiesWithoutUndo();

            CCS_SurvivalInteractionInput input = playerRoot.GetComponent<CCS_SurvivalInteractionInput>();
            if (input == null)
            {
                input = playerRoot.AddComponent<CCS_SurvivalInteractionInput>();
            }

            SerializedObject serializedInput = new SerializedObject(input);
            serializedInput.FindProperty("interactionScanner").objectReferenceValue = scanner;
            serializedInput.FindProperty("interactAction").objectReferenceValue = interactActionReference;
            serializedInput.ApplyModifiedPropertiesWithoutUndo();

            CCS_SurvivalDebugOverlay overlay = Object.FindFirstObjectByType<CCS_SurvivalDebugOverlay>();
            if (overlay != null)
            {
                SerializedObject serializedOverlay = new SerializedObject(overlay);
                serializedOverlay.FindProperty("interactionScanner").objectReferenceValue = scanner;
                serializedOverlay.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void CreatePrototypePickups(Transform root, CCS_RuntimeHost runtimeHost)
        {
            CreatePickup(
                root,
                "PU_FoodTin",
                new Vector3(1.8f, 0.35f, 1.2f),
                PrimitiveType.Cylinder,
                new Vector3(0.35f, 0.25f, 0.35f),
                new Color(0.82f, 0.48f, 0.18f, 1f),
                "survival.pickup.food_tin",
                "Food Tin",
                1,
                runtimeHost);

            CreatePickup(
                root,
                "PU_WaterCanteen",
                new Vector3(2.4f, 0.35f, 0.8f),
                PrimitiveType.Capsule,
                new Vector3(0.35f, 0.35f, 0.35f),
                new Color(0.25f, 0.55f, 0.95f, 1f),
                "survival.pickup.water_canteen",
                "Water Canteen",
                1,
                runtimeHost);

            CreatePickup(
                root,
                "PU_Kindling",
                new Vector3(1.2f, 0.25f, 3.4f),
                PrimitiveType.Cube,
                new Vector3(0.5f, 0.25f, 0.5f),
                new Color(0.55f, 0.38f, 0.2f, 1f),
                "survival.pickup.kindling",
                "Kindling",
                3,
                runtimeHost);
        }

        private static void CreatePickup(
            Transform parent,
            string objectName,
            Vector3 worldPosition,
            PrimitiveType primitiveType,
            Vector3 colliderScale,
            Color visualColor,
            string pickupId,
            string displayName,
            int amount,
            CCS_RuntimeHost runtimeHost)
        {
            GameObject pickupObject = new GameObject(objectName);
            pickupObject.transform.SetParent(parent, false);
            pickupObject.transform.position = worldPosition;

            BoxCollider collider = pickupObject.AddComponent<BoxCollider>();
            collider.isTrigger = false;
            collider.size = colliderScale;
            collider.center = new Vector3(0f, colliderScale.y * 0.5f, 0f);

            CCS_SurvivalPickupInteractable pickup = pickupObject.AddComponent<CCS_SurvivalPickupInteractable>();
            SerializedObject serializedPickup = new SerializedObject(pickup);
            serializedPickup.FindProperty("pickupId").stringValue = pickupId;
            serializedPickup.FindProperty("displayName").stringValue = displayName;
            serializedPickup.FindProperty("amount").intValue = amount;
            serializedPickup.FindProperty("runtimeHost").objectReferenceValue = runtimeHost;
            serializedPickup.FindProperty("enableDebugLogs").boolValue = true;
            serializedPickup.ApplyModifiedPropertiesWithoutUndo();

            CreateVisualPrimitive(pickupObject.transform, primitiveType, visualColor, colliderScale.y);
        }

        private static void CreateVisualPrimitive(
            Transform parent,
            PrimitiveType primitiveType,
            Color color,
            float height)
        {
            GameObject visual = GameObject.CreatePrimitive(primitiveType);
            visual.name = $"{parent.name}_Visual";
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one * 0.35f;

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

        private static InputActionReference LoadInteractActionReference()
        {
            InputActionAsset inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (inputAsset == null)
            {
                Debug.LogWarning("[CCS Survival] Input actions asset not found. Interaction keyboard fallback will be used.");
                return null;
            }

            InputAction interactAction = inputAsset.FindActionMap("Gameplay").FindAction("Interact");
            if (interactAction == null)
            {
                Debug.LogWarning("[CCS Survival] Gameplay/Interact action not found. Interaction keyboard fallback will be used.");
                return null;
            }

            return InputActionReference.Create(interactAction);
        }
    }
}
#endif
