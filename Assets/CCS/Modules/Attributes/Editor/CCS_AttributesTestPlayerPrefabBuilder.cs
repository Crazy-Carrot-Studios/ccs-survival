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
            CCS_AttributesAssetBuilder.EnsureStaminaDefinitionAsset();

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
                CCS_AttributeDefinition staminaDefinition =
                    AssetDatabase.LoadAssetAtPath<CCS_AttributeDefinition>(CCS_AttributesConstants.StaminaDefinitionPath);
                if (healthDefinition == null)
                {
                    Debug.LogError(
                        "[Attributes Prefab Builder] Missing health definition: "
                        + CCS_AttributesConstants.HealthDefinitionPath);
                    return false;
                }

                if (staminaDefinition == null)
                {
                    Debug.LogError(
                        "[Attributes Prefab Builder] Missing stamina definition: "
                        + CCS_AttributesConstants.StaminaDefinitionPath);
                    return false;
                }

                RemoveMissingScriptsRecursive(prefabRoot.transform);

                bool changed = false;
                changed |= RemoveCharacterControllerDebugHud(prefabRoot);
                changed |= EnsureAttributeContainer(prefabRoot, healthDefinition, staminaDefinition);
                changed |= EnsureAttributeService(prefabRoot);
                changed |= EnsureStaminaController(prefabRoot, staminaDefinition);
                changed |= EnsureHealthRegenController(prefabRoot, healthDefinition);
                changed |= EnsureNetworkAttributeReplicator(prefabRoot, healthDefinition);
                changed |= EnsureNetworkHealth(prefabRoot, healthDefinition);
                changed |= EnsureAttributeBarsHud(prefabRoot, healthDefinition, staminaDefinition);
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

        private static bool RemoveCharacterControllerDebugHud(GameObject prefabRoot)
        {
            MonoBehaviour[] behaviours = prefabRoot.GetComponents<MonoBehaviour>();
            bool changed = false;
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null && behaviour.GetType().Name == "CCS_CharacterControllerDebugHud")
                {
                    Object.DestroyImmediate(behaviour, true);
                    changed = true;
                }
            }

            return changed;
        }

        private static bool EnsureAttributeContainer(
            GameObject prefabRoot,
            CCS_AttributeDefinition healthDefinition,
            CCS_AttributeDefinition staminaDefinition)
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
                bool definitionsChanged = definitionsProperty.arraySize != 2;
                if (!definitionsChanged)
                {
                    definitionsChanged |= definitionsProperty.GetArrayElementAtIndex(0).objectReferenceValue != healthDefinition;
                    definitionsChanged |= definitionsProperty.GetArrayElementAtIndex(1).objectReferenceValue != staminaDefinition;
                }

                if (definitionsChanged)
                {
                    definitionsProperty.arraySize = 2;
                    definitionsProperty.GetArrayElementAtIndex(0).objectReferenceValue = healthDefinition;
                    definitionsProperty.GetArrayElementAtIndex(1).objectReferenceValue = staminaDefinition;
                    changed = true;
                }
            }

            if (changed)
            {
                serializedContainer.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool EnsureStaminaController(GameObject prefabRoot, CCS_AttributeDefinition staminaDefinition)
        {
            CCS_StaminaController staminaController = prefabRoot.GetComponent<CCS_StaminaController>();
            if (staminaController == null)
            {
                staminaController = prefabRoot.AddComponent<CCS_StaminaController>();
            }

            CCS_AttributeContainer container = prefabRoot.GetComponent<CCS_AttributeContainer>();
            SerializedObject serializedStamina = new SerializedObject(staminaController);
            bool changed = staminaController == null;
            changed |= SetObjectReference(serializedStamina, "attributeContainer", container);
            changed |= SetObjectReference(serializedStamina, "staminaDefinition", staminaDefinition);

            SerializedProperty drainProperty = serializedStamina.FindProperty("drainPerSecond");
            if (drainProperty != null
                && !Mathf.Approximately(drainProperty.floatValue, CCS_AttributesConstants.StaminaDrainPerSecond))
            {
                drainProperty.floatValue = CCS_AttributesConstants.StaminaDrainPerSecond;
                changed = true;
            }

            SerializedProperty regenProperty = serializedStamina.FindProperty("regenPerSecond");
            if (regenProperty != null
                && !Mathf.Approximately(regenProperty.floatValue, CCS_AttributesConstants.StaminaRegenPerSecond))
            {
                regenProperty.floatValue = CCS_AttributesConstants.StaminaRegenPerSecond;
                changed = true;
            }

            SerializedProperty unlockProperty = serializedStamina.FindProperty("sprintUnlockThreshold");
            if (unlockProperty != null
                && !Mathf.Approximately(unlockProperty.floatValue, CCS_AttributesConstants.StaminaSprintUnlockThreshold))
            {
                unlockProperty.floatValue = CCS_AttributesConstants.StaminaSprintUnlockThreshold;
                changed = true;
            }

            SerializedProperty walkRecoveryProperty = serializedStamina.FindProperty("walkRecoveryThreshold");
            if (walkRecoveryProperty != null
                && !Mathf.Approximately(walkRecoveryProperty.floatValue, CCS_AttributesConstants.StaminaWalkRecoveryThreshold))
            {
                walkRecoveryProperty.floatValue = CCS_AttributesConstants.StaminaWalkRecoveryThreshold;
                changed = true;
            }

            SerializedProperty exhaustedMultiplierProperty = serializedStamina.FindProperty("exhaustedWalkMultiplier");
            if (exhaustedMultiplierProperty != null
                && !Mathf.Approximately(
                    exhaustedMultiplierProperty.floatValue,
                    CCS_AttributesConstants.StaminaExhaustedWalkMultiplier))
            {
                exhaustedMultiplierProperty.floatValue = CCS_AttributesConstants.StaminaExhaustedWalkMultiplier;
                changed = true;
            }

            if (changed)
            {
                serializedStamina.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool EnsureHealthRegenController(
            GameObject prefabRoot,
            CCS_AttributeDefinition healthDefinition)
        {
            CCS_HealthRegenController healthRegenController = prefabRoot.GetComponent<CCS_HealthRegenController>();
            if (healthRegenController == null)
            {
                healthRegenController = prefabRoot.AddComponent<CCS_HealthRegenController>();
            }

            CCS_AttributeContainer container = prefabRoot.GetComponent<CCS_AttributeContainer>();
            CCS_NetworkAttributeReplicator replicator = prefabRoot.GetComponent<CCS_NetworkAttributeReplicator>();
            SerializedObject serializedHealthRegen = new SerializedObject(healthRegenController);
            bool changed = healthRegenController == null;
            changed |= SetObjectReference(serializedHealthRegen, "attributeContainer", container);
            changed |= SetObjectReference(serializedHealthRegen, "healthDefinition", healthDefinition);
            changed |= SetObjectReference(serializedHealthRegen, "networkAttributeReplicator", replicator);

            SerializedProperty delayProperty = serializedHealthRegen.FindProperty("regenDelaySeconds");
            if (delayProperty != null
                && !Mathf.Approximately(delayProperty.floatValue, CCS_AttributesConstants.HealthRegenDelaySeconds))
            {
                delayProperty.floatValue = CCS_AttributesConstants.HealthRegenDelaySeconds;
                changed = true;
            }

            SerializedProperty regenProperty = serializedHealthRegen.FindProperty("regenPerSecond");
            if (regenProperty != null
                && !Mathf.Approximately(regenProperty.floatValue, CCS_AttributesConstants.HealthRegenPerSecond))
            {
                regenProperty.floatValue = CCS_AttributesConstants.HealthRegenPerSecond;
                changed = true;
            }

            if (changed)
            {
                serializedHealthRegen.ApplyModifiedPropertiesWithoutUndo();
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

        private static bool EnsureNetworkHealth(
            GameObject prefabRoot,
            CCS_AttributeDefinition healthDefinition)
        {
            CCS_NetworkHealth networkHealth = prefabRoot.GetComponent<CCS_NetworkHealth>();
            if (networkHealth == null)
            {
                networkHealth = prefabRoot.AddComponent<CCS_NetworkHealth>();
            }

            CCS_AttributeContainer container = prefabRoot.GetComponent<CCS_AttributeContainer>();
            SerializedObject serializedHealth = new SerializedObject(networkHealth);
            bool changed = networkHealth == null;
            changed |= SetObjectReference(serializedHealth, "attributeContainer", container);
            changed |= SetObjectReference(serializedHealth, "healthDefinition", healthDefinition);

            if (changed)
            {
                serializedHealth.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool EnsureAttributeBarsHud(
            GameObject prefabRoot,
            CCS_AttributeDefinition healthDefinition,
            CCS_AttributeDefinition staminaDefinition)
        {
            bool changed = false;
            GameObject hudObject = ResolveHudRootGameObject(prefabRoot, ref changed);
            if (hudObject == null)
            {
                return changed;
            }

            changed |= RemoveLegacyDebugHud(hudObject);
            changed |= EnsureHudCanvas(hudObject);

            CCS_PlayerAttributeBarsHud barsHud = hudObject.GetComponent<CCS_PlayerAttributeBarsHud>();
            if (barsHud == null)
            {
                barsHud = hudObject.AddComponent<CCS_PlayerAttributeBarsHud>();
                changed = true;
            }

            GameObject panelObject = ResolvePanelGameObject(hudObject, ref changed);
            if (panelObject == null)
            {
                return changed;
            }

            CCS_AttributeBarView healthBar = EnsureAttributeBar(
                panelObject.transform,
                CCS_AttributesTestConstants.HealthBarObjectName,
                CCS_AttributeBarsHudStyle.HealthBarLabel,
                CCS_AttributeBarsHudStyle.HealthFillColor,
                0,
                true,
                -26f,
                string.Empty,
                ref changed);
            CCS_AttributeBarView staminaBar = EnsureAttributeBar(
                panelObject.transform,
                CCS_AttributesTestConstants.StaminaBarObjectName,
                CCS_AttributeBarsHudStyle.StaminaBarLabel,
                CCS_AttributeBarsHudStyle.StaminaFillColor,
                1,
                true,
                -26f,
                string.Empty,
                ref changed);
            CCS_AttributeBarView hungerBar = EnsureAttributeBar(
                panelObject.transform,
                CCS_AttributesTestConstants.HungerBarObjectName,
                CCS_AttributeBarsHudStyle.HungerBarLabel,
                CCS_AttributeBarsHudStyle.HungerFillColor,
                2,
                true,
                -34f,
                CCS_AttributeBarsHudStyle.PlaceholderStatusSuffix,
                ref changed);
            CCS_AttributeBarView thirstBar = EnsureAttributeBar(
                panelObject.transform,
                CCS_AttributesTestConstants.ThirstBarObjectName,
                CCS_AttributeBarsHudStyle.ThirstBarLabel,
                CCS_AttributeBarsHudStyle.ThirstFillColor,
                3,
                true,
                -34f,
                CCS_AttributeBarsHudStyle.PlaceholderStatusSuffix,
                ref changed);

            Canvas canvas = hudObject.GetComponent<Canvas>();
            CCS_AttributeContainer container = prefabRoot.GetComponent<CCS_AttributeContainer>();
            CCS_StaminaController staminaController = prefabRoot.GetComponent<CCS_StaminaController>();
            CCS_HealthRegenController healthRegenController = prefabRoot.GetComponent<CCS_HealthRegenController>();
            SerializedObject serializedHud = new SerializedObject(barsHud);
            bool hudChanged = SetObjectReference(serializedHud, "attributeContainer", container);
            hudChanged |= SetObjectReference(serializedHud, "healthDefinition", healthDefinition);
            hudChanged |= SetObjectReference(serializedHud, "staminaDefinition", staminaDefinition);
            hudChanged |= SetObjectReference(serializedHud, "staminaController", staminaController);
            hudChanged |= SetObjectReference(serializedHud, "healthRegenController", healthRegenController);
            hudChanged |= SetObjectReference(serializedHud, "hudCanvas", canvas);
            hudChanged |= SetObjectReference(serializedHud, "healthBar", healthBar);
            hudChanged |= SetObjectReference(serializedHud, "staminaBar", staminaBar);
            hudChanged |= SetObjectReference(serializedHud, "hungerBar", hungerBar);
            hudChanged |= SetObjectReference(serializedHud, "thirstBar", thirstBar);

            if (hudChanged)
            {
                serializedHud.ApplyModifiedPropertiesWithoutUndo();
                changed = true;
            }

            return changed;
        }

        private static bool EnsureHudCanvas(GameObject hudObject)
        {
            bool changed = false;

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

            return changed;
        }

        private static bool RemoveLegacyDebugHud(GameObject hudObject)
        {
            bool changed = false;

            MonoBehaviour[] behaviours = hudObject.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null && behaviour.GetType().Name == "CCS_PlayerAttributeHud")
                {
                    Object.DestroyImmediate(behaviour, true);
                    changed = true;
                }
            }

            Transform legacyText = FindDirectChild(
                hudObject.transform,
                CCS_AttributesTestConstants.LegacyAttributeHudTextObjectName);
            if (legacyText != null)
            {
                Object.DestroyImmediate(legacyText.gameObject, true);
                changed = true;
            }

            return changed;
        }

        private static GameObject ResolvePanelGameObject(GameObject hudObject, ref bool changed)
        {
            Transform existing = FindDirectChild(
                hudObject.transform,
                CCS_AttributesTestConstants.AttributeBarsPanelObjectName);
            if (existing != null)
            {
                bool layoutChanged = ApplyPanelLayout(existing.gameObject);
                if (layoutChanged)
                {
                    changed = true;
                }

                return existing.gameObject;
            }

            GameObject panelObject = new GameObject(
                CCS_AttributesTestConstants.AttributeBarsPanelObjectName,
                typeof(RectTransform),
                typeof(Image),
                typeof(Outline));
            panelObject.transform.SetParent(hudObject.transform, false);

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(0f, 0f);
            panelRect.pivot = new Vector2(0f, 0f);
            panelRect.anchoredPosition = new Vector2(
                CCS_AttributeBarsHudStyle.PanelOffsetX,
                CCS_AttributeBarsHudStyle.PanelOffsetY);
            panelRect.sizeDelta = new Vector2(
                CCS_AttributeBarsHudStyle.PanelWidth,
                CCS_AttributeBarsHudStyle.PanelHeight);

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = CCS_AttributeBarsHudStyle.PanelBackgroundColor;
            panelImage.raycastTarget = false;

            Outline panelOutline = panelObject.GetComponent<Outline>();
            panelOutline.effectColor = CCS_AttributeBarsHudStyle.BorderColor;
            panelOutline.effectDistance = new Vector2(1.5f, -1.5f);

            changed = true;
            return panelObject;
        }

        private static bool ApplyPanelLayout(GameObject panelObject)
        {
            bool changed = false;

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                Vector2 anchoredPosition = new Vector2(
                    CCS_AttributeBarsHudStyle.PanelOffsetX,
                    CCS_AttributeBarsHudStyle.PanelOffsetY);
                Vector2 panelSize = new Vector2(
                    CCS_AttributeBarsHudStyle.PanelWidth,
                    CCS_AttributeBarsHudStyle.PanelHeight);
                if (panelRect.anchorMin != new Vector2(0f, 0f)
                    || panelRect.anchorMax != new Vector2(0f, 0f)
                    || panelRect.pivot != new Vector2(0f, 0f)
                    || panelRect.anchoredPosition != anchoredPosition
                    || panelRect.sizeDelta != panelSize)
                {
                    panelRect.anchorMin = new Vector2(0f, 0f);
                    panelRect.anchorMax = new Vector2(0f, 0f);
                    panelRect.pivot = new Vector2(0f, 0f);
                    panelRect.anchoredPosition = anchoredPosition;
                    panelRect.sizeDelta = panelSize;
                    changed = true;
                }
            }

            Image panelImage = panelObject.GetComponent<Image>();
            if (panelImage != null)
            {
                if (panelImage.color != CCS_AttributeBarsHudStyle.PanelBackgroundColor)
                {
                    panelImage.color = CCS_AttributeBarsHudStyle.PanelBackgroundColor;
                    changed = true;
                }

                if (panelImage.raycastTarget)
                {
                    panelImage.raycastTarget = false;
                    changed = true;
                }
            }

            Outline panelOutline = panelObject.GetComponent<Outline>();
            if (panelOutline == null)
            {
                panelOutline = panelObject.AddComponent<Outline>();
                changed = true;
            }

            if (panelOutline.effectColor != CCS_AttributeBarsHudStyle.BorderColor)
            {
                panelOutline.effectColor = CCS_AttributeBarsHudStyle.BorderColor;
                changed = true;
            }

            if (panelOutline.effectDistance != new Vector2(1.5f, -1.5f))
            {
                panelOutline.effectDistance = new Vector2(1.5f, -1.5f);
                changed = true;
            }

            return changed;
        }

        private static CCS_AttributeBarView EnsureAttributeBar(
            Transform panelTransform,
            string barObjectName,
            string label,
            Color fillColor,
            int barIndex,
            bool includeStatusSuffix,
            float statusYOffset,
            string defaultStatusText,
            ref bool changed)
        {
            Transform barTransform = FindDirectChild(panelTransform, barObjectName);
            GameObject barObject;
            if (barTransform != null)
            {
                barObject = barTransform.gameObject;
            }
            else
            {
                barObject = new GameObject(barObjectName, typeof(RectTransform), typeof(CCS_AttributeBarView));
                barObject.transform.SetParent(panelTransform, false);
                changed = true;
            }

            RectTransform barRect = barObject.GetComponent<RectTransform>();
            float blockHeight = GetConfiguredBarBlockHeight(barIndex);
            float topOffset = ResolveBarTopOffset(barIndex);
            Vector2 anchoredPosition = new Vector2(CCS_AttributeBarsHudStyle.PanelPaddingX, -topOffset);
            Vector2 barSize = new Vector2(CCS_AttributeBarsHudStyle.BarWidth, blockHeight);
            if (barRect.anchorMin != new Vector2(0f, 1f)
                || barRect.anchorMax != new Vector2(0f, 1f)
                || barRect.pivot != new Vector2(0f, 1f)
                || barRect.anchoredPosition != anchoredPosition
                || barRect.sizeDelta != barSize)
            {
                barRect.anchorMin = new Vector2(0f, 1f);
                barRect.anchorMax = new Vector2(0f, 1f);
                barRect.pivot = new Vector2(0f, 1f);
                barRect.anchoredPosition = anchoredPosition;
                barRect.sizeDelta = barSize;
                changed = true;
            }

            CCS_AttributeBarView barView = barObject.GetComponent<CCS_AttributeBarView>();
            if (barView == null)
            {
                barView = barObject.AddComponent<CCS_AttributeBarView>();
                changed = true;
            }

            TextMeshProUGUI labelText = EnsureBarText(
                barObject.transform,
                "LabelText",
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 0f),
                new Vector2(160f, 14f),
                TextAlignmentOptions.BottomLeft,
                CCS_AttributeBarsHudStyle.LabelFontSize,
                CCS_AttributeBarsHudStyle.TextColor,
                label,
                ref changed);

            TextMeshProUGUI valueText = EnsureBarText(
                barObject.transform,
                "ValueText",
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, 0f),
                new Vector2(160f, 14f),
                TextAlignmentOptions.BottomRight,
                CCS_AttributeBarsHudStyle.ValueFontSize,
                CCS_AttributeBarsHudStyle.TextColor,
                "100 / 100",
                ref changed);

            TextMeshProUGUI statusText = null;
            if (includeStatusSuffix)
            {
                statusText = EnsureBarText(
                    barObject.transform,
                    "StatusText",
                    new Vector2(0f, 1f),
                    new Vector2(0f, 1f),
                    new Vector2(0f, 1f),
                    new Vector2(0f, statusYOffset),
                    new Vector2(CCS_AttributeBarsHudStyle.BarWidth, 12f),
                    TextAlignmentOptions.TopLeft,
                    CCS_AttributeBarsHudStyle.StatusFontSize,
                    CCS_AttributeBarsHudStyle.MutedTextColor,
                    defaultStatusText,
                    ref changed);
            }

            Image fillImage = EnsureBarFill(barObject.transform, fillColor, ref changed);

            SerializedObject serializedBar = new SerializedObject(barView);
            bool barChanged = SetObjectReference(serializedBar, "labelText", labelText);
            barChanged |= SetObjectReference(serializedBar, "valueText", valueText);
            barChanged |= SetObjectReference(serializedBar, "statusText", statusText);
            barChanged |= SetObjectReference(serializedBar, "fillImage", fillImage);
            if (barChanged)
            {
                serializedBar.ApplyModifiedPropertiesWithoutUndo();
                changed = true;
            }

            barView.SetFillColor(fillColor);
            return barView;
        }

        private static Sprite GetDefaultBarFillSprite()
        {
            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        }

        private static float GetConfiguredBarBlockHeight(int barIndex)
        {
            if (barIndex == 2 || barIndex == 3)
            {
                return CCS_AttributeBarsHudStyle.PlaceholderBarBlockHeight;
            }

            return CCS_AttributeBarsHudStyle.BarBlockHeight;
        }

        private static float ResolveBarTopOffset(int barIndex)
        {
            float offset = CCS_AttributeBarsHudStyle.PanelPaddingTop;
            for (int i = 0; i < barIndex; i++)
            {
                offset += GetConfiguredBarBlockHeight(i) + CCS_AttributeBarsHudStyle.BarSpacing;
            }

            return offset;
        }

        private static TextMeshProUGUI EnsureBarText(
            Transform parent,
            string objectName,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            TextAlignmentOptions alignment,
            float fontSize,
            Color color,
            string defaultText,
            ref bool changed)
        {
            Transform existing = FindDirectChild(parent, objectName);
            GameObject textObject;
            if (existing != null)
            {
                textObject = existing.gameObject;
            }
            else
            {
                textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
                textObject.transform.SetParent(parent, false);
                changed = true;
            }

            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            if (rectTransform.anchorMin != anchorMin
                || rectTransform.anchorMax != anchorMax
                || rectTransform.pivot != pivot
                || rectTransform.anchoredPosition != anchoredPosition
                || rectTransform.sizeDelta != sizeDelta)
            {
                rectTransform.anchorMin = anchorMin;
                rectTransform.anchorMax = anchorMax;
                rectTransform.pivot = pivot;
                rectTransform.anchoredPosition = anchoredPosition;
                rectTransform.sizeDelta = sizeDelta;
                changed = true;
            }

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            if (text.fontSize != fontSize)
            {
                text.fontSize = fontSize;
                changed = true;
            }

            if (text.color != color)
            {
                text.color = color;
                changed = true;
            }

            if (text.alignment != alignment)
            {
                text.alignment = alignment;
                changed = true;
            }

            if (text.raycastTarget)
            {
                text.raycastTarget = false;
                changed = true;
            }

            if (text.text != defaultText)
            {
                text.text = defaultText;
                changed = true;
            }

            return text;
        }

        private static Image EnsureBarFill(Transform barTransform, Color fillColor, ref bool changed)
        {
            Transform backgroundTransform = FindDirectChild(barTransform, "BarBackground");
            GameObject backgroundObject;
            if (backgroundTransform != null)
            {
                backgroundObject = backgroundTransform.gameObject;
            }
            else
            {
                backgroundObject = new GameObject("BarBackground", typeof(RectTransform), typeof(Image));
                backgroundObject.transform.SetParent(barTransform, false);
                changed = true;
            }

            RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
            Vector2 backgroundPosition = new Vector2(0f, -18f);
            Vector2 backgroundSize = new Vector2(
                CCS_AttributeBarsHudStyle.BarWidth,
                CCS_AttributeBarsHudStyle.BarHeight);
            if (backgroundRect.anchorMin != new Vector2(0f, 1f)
                || backgroundRect.anchorMax != new Vector2(0f, 1f)
                || backgroundRect.pivot != new Vector2(0f, 1f)
                || backgroundRect.anchoredPosition != backgroundPosition
                || backgroundRect.sizeDelta != backgroundSize)
            {
                backgroundRect.anchorMin = new Vector2(0f, 1f);
                backgroundRect.anchorMax = new Vector2(0f, 1f);
                backgroundRect.pivot = new Vector2(0f, 1f);
                backgroundRect.anchoredPosition = backgroundPosition;
                backgroundRect.sizeDelta = backgroundSize;
                changed = true;
            }

            Image backgroundImage = backgroundObject.GetComponent<Image>();
            if (backgroundImage.color != CCS_AttributeBarsHudStyle.BarBackgroundColor)
            {
                backgroundImage.color = CCS_AttributeBarsHudStyle.BarBackgroundColor;
                changed = true;
            }

            if (backgroundImage.raycastTarget)
            {
                backgroundImage.raycastTarget = false;
                changed = true;
            }

            Transform fillTransform = FindDirectChild(backgroundObject.transform, "BarFill");
            GameObject fillObject;
            if (fillTransform != null)
            {
                fillObject = fillTransform.gameObject;
            }
            else
            {
                fillObject = new GameObject("BarFill", typeof(RectTransform), typeof(Image));
                fillObject.transform.SetParent(backgroundObject.transform, false);
                changed = true;
            }

            RectTransform fillRect = fillObject.GetComponent<RectTransform>();
            if (fillRect.anchorMin != new Vector2(0f, 0f)
                || fillRect.anchorMax != new Vector2(0f, 1f)
                || fillRect.pivot != new Vector2(0f, 0.5f)
                || fillRect.anchoredPosition != Vector2.zero
                || !Mathf.Approximately(fillRect.sizeDelta.x, backgroundSize.x)
                || fillRect.sizeDelta.y != 0f)
            {
                fillRect.anchorMin = new Vector2(0f, 0f);
                fillRect.anchorMax = new Vector2(0f, 1f);
                fillRect.pivot = new Vector2(0f, 0.5f);
                fillRect.anchoredPosition = Vector2.zero;
                fillRect.offsetMin = Vector2.zero;
                fillRect.offsetMax = Vector2.zero;
                fillRect.sizeDelta = new Vector2(backgroundSize.x, 0f);
                changed = true;
            }

            Image fillImage = fillObject.GetComponent<Image>();
            Sprite fillSprite = GetDefaultBarFillSprite();
            if (fillSprite != null && fillImage.sprite != fillSprite)
            {
                fillImage.sprite = fillSprite;
                changed = true;
            }
            if (fillImage.color != fillColor)
            {
                fillImage.color = fillColor;
                changed = true;
            }

            if (fillImage.type != Image.Type.Filled)
            {
                fillImage.type = Image.Type.Filled;
                changed = true;
            }

            if (fillImage.fillMethod != Image.FillMethod.Horizontal)
            {
                fillImage.fillMethod = Image.FillMethod.Horizontal;
                changed = true;
            }

            if (fillImage.fillOrigin != (int)Image.OriginHorizontal.Left)
            {
                fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
                changed = true;
            }

            if (fillImage.fillAmount != 1f)
            {
                fillImage.fillAmount = 1f;
                changed = true;
            }

            if (fillImage.raycastTarget)
            {
                fillImage.raycastTarget = false;
                changed = true;
            }

            return fillImage;
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
