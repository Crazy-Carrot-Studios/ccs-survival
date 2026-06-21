using TMPro;
using CCS.Modules.CharacterController;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_InteractionPromptHudPrefabBuilder
// CATEGORY: Modules / Interaction / Editor
// PURPOSE: Builds the local-owner interaction prompt presenter on the test player prefab.
// PLACEMENT: Editor utility invoked from Interaction validation and master test setup.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Wires CCS_InteractionPromptPresenter. Removes legacy label HUD objects.
// =============================================================================

namespace CCS.Modules.Interaction.Editor
{
    public static class CCS_InteractionPromptHudPrefabBuilder
    {
        #region Public Methods

        public static bool EnsureTestPlayerInteractionPromptHud()
        {
            bool changed = EnsureTestPlayerInteractionPromptHud(CCS_InteractionConstants.NetworkedTestPlayerPrefabPath);
            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return changed;
        }

        public static bool EnsureTestPlayerInteractionPromptHud(string prefabPath)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError("[Interaction Prompt Builder] Missing prefab: " + prefabPath);
                return false;
            }

            try
            {
                bool changed = EnsurePromptPresenter(prefabRoot);
                changed |= RemoveLegacyLabelHud(prefabRoot);
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

        public static void BatchSetupInteractionPromptHud()
        {
            EnsureTestPlayerInteractionPromptHud();
        }

        #endregion

        #region Private Methods

        private static bool EnsurePromptPresenter(GameObject prefabRoot)
        {
            bool changed = false;
            Transform hudRootTransform = prefabRoot.transform.Find(CCS_InteractionConstants.InteractionPromptHudRootName);
            GameObject hudRootObject;
            if (hudRootTransform == null)
            {
                hudRootObject = new GameObject(CCS_InteractionConstants.InteractionPromptHudRootName, typeof(RectTransform));
                hudRootObject.transform.SetParent(prefabRoot.transform, false);
                changed = true;
            }
            else
            {
                hudRootObject = hudRootTransform.gameObject;
            }

            if (!hudRootObject.activeSelf)
            {
                hudRootObject.SetActive(true);
                changed = true;
            }

            changed |= EnsureHudCanvas(hudRootObject);

            CCS_InteractionPromptPresenter presenter = hudRootObject.GetComponent<CCS_InteractionPromptPresenter>();
            if (presenter == null)
            {
                RemoveLegacyHudComponents(hudRootObject);
                presenter = hudRootObject.AddComponent<CCS_InteractionPromptPresenter>();
                changed = true;
            }

            TextMeshProUGUI promptText = EnsurePromptText(hudRootObject.transform, ref changed);
            CCS_NetworkInteractionScanner scanner = prefabRoot.GetComponent<CCS_NetworkInteractionScanner>();
            CCS_PlayerInteractionAnimator interactionAnimator =
                prefabRoot.GetComponentInChildren<CCS_PlayerInteractionAnimator>(true);
            Canvas canvas = hudRootObject.GetComponent<Canvas>();

            SerializedObject serializedPresenter = new SerializedObject(presenter);
            bool presenterChanged = SetObjectReference(serializedPresenter, "hudCanvas", canvas);
            presenterChanged |= SetObjectReference(serializedPresenter, "promptText", promptText);
            presenterChanged |= SetObjectReference(serializedPresenter, "interactionTargetSourceComponent", scanner);
            presenterChanged |= SetObjectReference(
                serializedPresenter,
                "interactionBusySourceComponent",
                interactionAnimator);

            if (presenterChanged)
            {
                serializedPresenter.ApplyModifiedPropertiesWithoutUndo();
                changed = true;
            }

            if (promptText.enabled)
            {
                promptText.enabled = false;
                changed = true;
            }

            changed |= EnsurePromptPanelVisible(hudRootObject.transform, promptText.transform);

            return changed;
        }

        private static bool EnsurePromptPanelVisible(Transform hudRoot, Transform promptTextTransform)
        {
            bool changed = false;
            Transform panelTransform = hudRoot.Find(CCS_InteractionConstants.InteractionPromptPanelObjectName);
            if (panelTransform == null)
            {
                return false;
            }

            if (!panelTransform.gameObject.activeSelf)
            {
                panelTransform.gameObject.SetActive(true);
                changed = true;
            }

            Image panelImage = panelTransform.GetComponent<Image>();
            if (panelImage != null && panelImage.color.a > 0f)
            {
                Color transparent = panelImage.color;
                transparent.a = 0f;
                panelImage.color = transparent;
                changed = true;
            }

            if (promptTextTransform.parent != panelTransform && promptTextTransform.parent != hudRoot)
            {
                return changed;
            }

            return changed;
        }

        private static bool RemoveLegacyLabelHud(GameObject prefabRoot)
        {
            Transform labelHudRoot = prefabRoot.transform.Find("InteractionLabelHudRoot");
            if (labelHudRoot == null)
            {
                return false;
            }

            Object.DestroyImmediate(labelHudRoot.gameObject);
            return true;
        }

        private static void RemoveLegacyHudComponents(GameObject hudRootObject)
        {
            MonoBehaviour[] behaviours = hudRootObject.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null || behaviour is CCS_InteractionPromptPresenter || behaviour is GraphicRaycaster)
                {
                    continue;
                }

                Object.DestroyImmediate(behaviour);
            }
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

            CanvasScaler scaler = hudObject.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = hudObject.AddComponent<CanvasScaler>();
                changed = true;
            }

            if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                changed = true;
            }

            if (scaler.referenceResolution != new Vector2(1920f, 1080f))
            {
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                changed = true;
            }

            if (hudObject.GetComponent<GraphicRaycaster>() == null)
            {
                hudObject.AddComponent<GraphicRaycaster>();
                changed = true;
            }

            RectTransform rootRect = hudObject.GetComponent<RectTransform>();
            if (rootRect.localScale != Vector3.one)
            {
                rootRect.localScale = Vector3.one;
                changed = true;
            }

            return changed;
        }

        private static TextMeshProUGUI EnsurePromptText(Transform hudRoot, ref bool changed)
        {
            Transform existing = hudRoot.Find(CCS_InteractionConstants.InteractionPromptTextObjectName);
            TextMeshProUGUI promptText;
            if (existing != null)
            {
                promptText = existing.GetComponent<TextMeshProUGUI>();
                if (promptText == null)
                {
                    promptText = existing.gameObject.AddComponent<TextMeshProUGUI>();
                    changed = true;
                }
            }
            else
            {
                GameObject textObject = new GameObject(
                    CCS_InteractionConstants.InteractionPromptTextObjectName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(TextMeshProUGUI));
                textObject.transform.SetParent(hudRoot, false);
                promptText = textObject.GetComponent<TextMeshProUGUI>();
                changed = true;
            }

            RectTransform textRect = promptText.rectTransform;
            Vector2 anchorMin = new Vector2(0.5f, 1f);
            Vector2 anchorMax = new Vector2(0.5f, 1f);
            Vector2 pivot = new Vector2(0.5f, 1f);
            Vector2 anchoredPosition = new Vector2(0f, -80f);
            Vector2 sizeDelta = new Vector2(640f, 56f);

            if (textRect.anchorMin != anchorMin)
            {
                textRect.anchorMin = anchorMin;
                changed = true;
            }

            if (textRect.anchorMax != anchorMax)
            {
                textRect.anchorMax = anchorMax;
                changed = true;
            }

            if (textRect.pivot != pivot)
            {
                textRect.pivot = pivot;
                changed = true;
            }

            if (textRect.anchoredPosition != anchoredPosition)
            {
                textRect.anchoredPosition = anchoredPosition;
                changed = true;
            }

            if (textRect.sizeDelta != sizeDelta)
            {
                textRect.sizeDelta = sizeDelta;
                changed = true;
            }

            if (promptText.text != CCS_InteractionConstants.DefaultInteractionPromptText)
            {
                promptText.text = CCS_InteractionConstants.DefaultInteractionPromptText;
                changed = true;
            }

            if (!Mathf.Approximately(promptText.fontSize, CCS_InteractionConstants.InteractionPromptFontSize))
            {
                promptText.fontSize = CCS_InteractionConstants.InteractionPromptFontSize;
                changed = true;
            }

            if (promptText.color != Color.white)
            {
                promptText.color = Color.white;
                changed = true;
            }

            if (promptText.alignment != TextAlignmentOptions.Center)
            {
                promptText.alignment = TextAlignmentOptions.Center;
                changed = true;
            }

            if (promptText.raycastTarget)
            {
                promptText.raycastTarget = false;
                changed = true;
            }

            return promptText;
        }

        private static bool SetObjectReference(SerializedObject serializedObject, string propertyName, Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == value)
            {
                return false;
            }

            property.objectReferenceValue = value;
            return true;
        }

        #endregion
    }
}
