using CCS.Modules.Attributes.Tests;
using CCS.Project;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_AttributesTestPlayerPrefabBuilder
// CATEGORY: Modules / Attributes / Editor
// PURPOSE: Wires attribute components onto the canonical networked test player prefab.
// PLACEMENT: Editor utility invoked from Attributes validation and master test setup.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Does not modify CharacterController movement, camera, or netcode ownership flow.
// =============================================================================

namespace CCS.Modules.Attributes.Editor
{
    public static class CCS_AttributesTestPlayerPrefabBuilder
    {
        #region Public Methods

        public static bool EnsureTestPlayerAttributes()
        {
            bool changed = EnsureTestPlayerAttributes(CCS_AttributesTestConstants.NetworkedTestPlayerPrefabPath);
            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return changed;
        }

        public static bool EnsureTestPlayerAttributes(string prefabPath)
        {
            CCS_AttributesAssetBuilder.EnsureHealthDefinitionAsset();

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError("[Attributes Prefab Builder] Missing prefab: " + prefabPath);
                return false;
            }

            try
            {
                CCS_AttributeDefinition healthDefinition =
                    AssetDatabase.LoadAssetAtPath<CCS_AttributeDefinition>(CCS_AttributesConstants.HealthDefinitionPath);
                if (healthDefinition == null)
                {
                    Debug.LogError(
                        "[Attributes Prefab Builder] Missing health definition: "
                        + CCS_AttributesConstants.HealthDefinitionPath);
                    return false;
                }

                RemoveMissingScriptsRecursive(prefabRoot.transform);

                bool changed = false;
                changed |= EnsureAttributeContainer(prefabRoot, healthDefinition);
                changed |= EnsureAttributeService(prefabRoot);
                changed |= EnsureNetworkAttributeReplicator(prefabRoot, healthDefinition);
                changed |= EnsureAttributeHud(prefabRoot, healthDefinition);
                changed |= EnsureDebugDamageInput(prefabRoot);
                RemoveMissingScriptsRecursive(prefabRoot.transform);

                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                }

                return changed;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        #endregion

        #region Private Methods

        private static bool EnsureAttributeContainer(GameObject prefabRoot, CCS_AttributeDefinition healthDefinition)
        {
            CCS_AttributeContainer container = prefabRoot.GetComponent<CCS_AttributeContainer>();
            bool changed = false;
            if (container == null)
            {
                container = prefabRoot.AddComponent<CCS_AttributeContainer>();
                changed = true;
            }

            SerializedObject serializedContainer = new SerializedObject(container);
            SerializedProperty definitionsProperty = serializedContainer.FindProperty("attributeDefinitions");
            if (definitionsProperty != null)
            {
                if (definitionsProperty.arraySize != 1
                    || definitionsProperty.GetArrayElementAtIndex(0).objectReferenceValue != healthDefinition)
                {
                    definitionsProperty.arraySize = 1;
                    definitionsProperty.GetArrayElementAtIndex(0).objectReferenceValue = healthDefinition;
                    changed = true;
                }
            }

            if (changed)
            {
                serializedContainer.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool EnsureAttributeService(GameObject prefabRoot)
        {
            if (prefabRoot.GetComponent<CCS_AttributeService>() != null)
            {
                return false;
            }

            CCS_AttributeService service = prefabRoot.AddComponent<CCS_AttributeService>();
            SerializedObject serializedService = new SerializedObject(service);
            SerializedProperty containerProperty = serializedService.FindProperty("attributeContainer");
            if (containerProperty != null)
            {
                containerProperty.objectReferenceValue = prefabRoot.GetComponent<CCS_AttributeContainer>();
                serializedService.ApplyModifiedPropertiesWithoutUndo();
            }

            return true;
        }

        private static bool EnsureNetworkAttributeReplicator(
            GameObject prefabRoot,
            CCS_AttributeDefinition healthDefinition)
        {
            CCS_NetworkAttributeReplicator replicator = prefabRoot.GetComponent<CCS_NetworkAttributeReplicator>();
            if (replicator == null)
            {
                replicator = prefabRoot.AddComponent<CCS_NetworkAttributeReplicator>();
            }

            CCS_AttributeContainer container = prefabRoot.GetComponent<CCS_AttributeContainer>();
            SerializedObject serializedReplicator = new SerializedObject(replicator);
            bool changed = replicator == null;
            changed |= SetObjectReference(serializedReplicator, "attributeContainer", container);
            changed |= SetObjectReference(serializedReplicator, "healthDefinition", healthDefinition);

            if (changed)
            {
                serializedReplicator.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool EnsureAttributeHud(GameObject prefabRoot, CCS_AttributeDefinition healthDefinition)
        {
            bool changed = false;
            GameObject hudObject = ResolveHudRootGameObject(prefabRoot, ref changed);

            Canvas canvas = hudObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = hudObject.AddComponent<Canvas>();
                changed = true;
            }

            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                changed = true;
            }

            hudObject = ResolveHudRootGameObject(prefabRoot, ref changed);
            if (hudObject == null)
            {
                return changed;
            }

            if (hudObject.GetComponent<CanvasScaler>() == null)
            {
                CanvasScaler scaler = hudObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                changed = true;
            }

            if (hudObject.GetComponent<GraphicRaycaster>() == null)
            {
                hudObject.AddComponent<GraphicRaycaster>();
                changed = true;
            }

            hudObject = ResolveHudRootGameObject(prefabRoot, ref changed);
            if (hudObject == null)
            {
                return changed;
            }

            CCS_PlayerAttributeHud hud = hudObject.GetComponent<CCS_PlayerAttributeHud>();
            if (hud == null)
            {
                hud = hudObject.AddComponent<CCS_PlayerAttributeHud>();
                changed = true;
            }

            GameObject textObject = ResolveHudTextObject(hudObject, ref changed);
            if (textObject == null)
            {
                return changed;
            }

            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                if (textObject == null)
                {
                    return changed;
                }

                rectTransform = textObject.AddComponent<RectTransform>();
                changed = true;
            }

            if (rectTransform == null)
            {
                return changed;
            }

            if (rectTransform.anchorMin != new Vector2(0f, 1f)
                || rectTransform.anchorMax != new Vector2(0f, 1f)
                || rectTransform.pivot != new Vector2(0f, 1f)
                || rectTransform.anchoredPosition != new Vector2(24f, -24f)
                || rectTransform.sizeDelta != new Vector2(320f, 48f))
            {
                rectTransform.anchorMin = new Vector2(0f, 1f);
                rectTransform.anchorMax = new Vector2(0f, 1f);
                rectTransform.pivot = new Vector2(0f, 1f);
                rectTransform.anchoredPosition = new Vector2(24f, -24f);
                rectTransform.sizeDelta = new Vector2(320f, 48f);
                changed = true;
            }

            textObject = ResolveHudTextObject(hudObject, ref changed);
            if (textObject == null)
            {
                return changed;
            }

            TextMeshProUGUI healthText = textObject.GetComponent<TextMeshProUGUI>();
            if (healthText == null)
            {
                healthText = textObject.AddComponent<TextMeshProUGUI>();
                changed = true;
            }

            if (healthText.fontSize != 28f)
            {
                healthText.fontSize = 28f;
                changed = true;
            }

            if (healthText.color != healthDefinition.UiColor)
            {
                healthText.color = healthDefinition.UiColor;
                changed = true;
            }

            if (healthText.raycastTarget)
            {
                healthText.raycastTarget = false;
                changed = true;
            }

            CCS_AttributeContainer container = prefabRoot.GetComponent<CCS_AttributeContainer>();
            SerializedObject serializedHud = new SerializedObject(hud);
            bool hudChanged = SetObjectReference(serializedHud, "attributeContainer", container);
            hudChanged |= SetObjectReference(serializedHud, "healthDefinition", healthDefinition);
            hudChanged |= SetObjectReference(serializedHud, "healthText", healthText);
            hudChanged |= SetObjectReference(serializedHud, "hudCanvas", canvas);

            if (hudChanged)
            {
                serializedHud.ApplyModifiedPropertiesWithoutUndo();
                changed = true;
            }

            return changed;
        }

        private static GameObject ResolveHudRootGameObject(GameObject prefabRoot, ref bool changed)
        {
            if (prefabRoot == null)
            {
                return null;
            }

            Transform existing = FindDirectChild(
                prefabRoot.transform,
                CCS_AttributesTestConstants.AttributeHudRootObjectName);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject hudObject = new GameObject(
                CCS_AttributesTestConstants.AttributeHudRootObjectName,
                typeof(RectTransform));
            hudObject.transform.SetParent(prefabRoot.transform, false);
            changed = true;
            return hudObject;
        }

        private static GameObject ResolveHudTextObject(GameObject hudObject, ref bool changed)
        {
            if (hudObject == null)
            {
                return null;
            }

            Transform existing = FindDirectChild(
                hudObject.transform,
                CCS_AttributesTestConstants.AttributeHudTextObjectName);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject textObject = new GameObject(
                CCS_AttributesTestConstants.AttributeHudTextObjectName,
                typeof(RectTransform),
                typeof(TextMeshProUGUI));
            textObject.transform.SetParent(hudObject.transform, false);
            changed = true;
            return textObject;
        }

        private static Transform FindDirectChild(Transform parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                if (child.name == childName)
                {
                    return child;
                }
            }

            return null;
        }

        private static bool EnsureDebugDamageInput(GameObject prefabRoot)
        {
            CCS_TestPlayerAttributeDebugInput debugInput =
                prefabRoot.GetComponent<CCS_TestPlayerAttributeDebugInput>();
            if (debugInput == null)
            {
                debugInput = prefabRoot.AddComponent<CCS_TestPlayerAttributeDebugInput>();
            }

            CCS_NetworkAttributeReplicator replicator = prefabRoot.GetComponent<CCS_NetworkAttributeReplicator>();
            SerializedObject serializedDebug = new SerializedObject(debugInput);
            bool changed = debugInput == null;
            changed |= SetObjectReference(serializedDebug, "attributeReplicator", replicator);

            SerializedProperty damageAmountProperty = serializedDebug.FindProperty("damageAmount");
            if (damageAmountProperty != null
                && !Mathf.Approximately(damageAmountProperty.floatValue, CCS_AttributesTestConstants.TestDamageAmount))
            {
                damageAmountProperty.floatValue = CCS_AttributesTestConstants.TestDamageAmount;
                changed = true;
            }

            if (changed)
            {
                serializedDebug.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool SetObjectReference(
            SerializedObject serializedObject,
            string propertyName,
            Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == value)
            {
                return false;
            }

            property.objectReferenceValue = value;
            return true;
        }

        private static void RemoveMissingScriptsRecursive(Transform root)
        {
            if (root == null)
            {
                return;
            }

            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root.gameObject);
            for (int i = 0; i < root.childCount; i++)
            {
                RemoveMissingScriptsRecursive(root.GetChild(i));
            }
        }

        #endregion
    }
}
