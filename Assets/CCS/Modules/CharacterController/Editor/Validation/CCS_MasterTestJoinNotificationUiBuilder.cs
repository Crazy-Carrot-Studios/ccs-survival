using CCS.Modules.CharacterController.Tests;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_MasterTestJoinNotificationUiBuilder
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Builds the master test join notification overlay in the upper-right corner.
// PLACEMENT: Editor builder utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Matches CCS multiplayer hosting dark translucent panel styling.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_MasterTestJoinNotificationUiBuilder
    {
        #region Public Methods

        public static bool EnsureJoinNotificationFeed()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                return false;
            }

            bool changed = false;
            Canvas canvas = FindOrCreateCanvas(ref changed);
            Transform existingFeed = canvas.transform.Find(CCS_MasterTestUiConstants.JoinNotificationFeedObjectName);
            GameObject feedObject;
            if (existingFeed == null)
            {
                feedObject = new GameObject(
                    CCS_MasterTestUiConstants.JoinNotificationFeedObjectName,
                    typeof(RectTransform));
                feedObject.transform.SetParent(canvas.transform, false);
                changed = true;
            }
            else
            {
                feedObject = existingFeed.gameObject;
            }

            EnsureFeedRootAnchoring(feedObject, ref changed);

            CCS_PlayerJoinNotificationFeed feed = feedObject.GetComponent<CCS_PlayerJoinNotificationFeed>();
            if (feed == null)
            {
                feed = feedObject.AddComponent<CCS_PlayerJoinNotificationFeed>();
                changed = true;
            }

            RectTransform panel = EnsurePanel(feedObject.transform, ref changed);
            TMP_Text titleText = EnsureTitle(panel, ref changed);
            RectTransform entriesContainer = EnsureEntriesContainer(panel, ref changed);
            TMP_Text entryTemplate = EnsureEntryTemplate(entriesContainer, ref changed);

            SerializedObject serializedFeed = new SerializedObject(feed);
            if (SetReference(serializedFeed, "feedRoot", feedObject.GetComponent<RectTransform>()))
            {
                changed = true;
            }

            if (SetReference(serializedFeed, "panelRoot", panel.gameObject))
            {
                changed = true;
            }

            if (SetReference(serializedFeed, "entriesContainer", entriesContainer))
            {
                changed = true;
            }

            if (SetReference(serializedFeed, "entryTemplate", entryTemplate))
            {
                changed = true;
            }

            SerializedProperty maxEntriesProperty = serializedFeed.FindProperty("maxEntries");
            if (maxEntriesProperty != null
                && maxEntriesProperty.intValue != CCS_MasterTestUiConstants.JoinNotificationMaxEntries)
            {
                maxEntriesProperty.intValue = CCS_MasterTestUiConstants.JoinNotificationMaxEntries;
                changed = true;
            }

            SerializedProperty lifetimeProperty = serializedFeed.FindProperty("entryLifetimeSeconds");
            if (lifetimeProperty != null
                && !Mathf.Approximately(
                    lifetimeProperty.floatValue,
                    CCS_MasterTestUiConstants.JoinNotificationEntryLifetimeSeconds))
            {
                lifetimeProperty.floatValue = CCS_MasterTestUiConstants.JoinNotificationEntryLifetimeSeconds;
                changed = true;
            }

            SerializedProperty panelVisibleProperty = serializedFeed.FindProperty("panelVisibleSeconds");
            if (panelVisibleProperty != null
                && !Mathf.Approximately(
                    panelVisibleProperty.floatValue,
                    CCS_MasterTestUiConstants.JoinNotificationPanelVisibleSeconds))
            {
                panelVisibleProperty.floatValue = CCS_MasterTestUiConstants.JoinNotificationPanelVisibleSeconds;
                changed = true;
            }

            if (changed)
            {
                serializedFeed.ApplyModifiedPropertiesWithoutUndo();
            }

            if (titleText != null && titleText.text != CCS_MasterTestUiConstants.JoinNotificationHeaderText)
            {
                titleText.text = CCS_MasterTestUiConstants.JoinNotificationHeaderText;
                changed = true;
            }

            if (panel.gameObject.activeSelf)
            {
                panel.gameObject.SetActive(false);
                changed = true;
            }

            return changed;
        }

        #endregion

        #region Private Methods

        private static Canvas FindOrCreateCanvas(ref bool changed)
        {
            GameObject canvasObject = GameObject.Find(CCS_MasterTestUiConstants.MasterTestUiCanvasObjectName);
            if (canvasObject == null)
            {
                canvasObject = new GameObject(
                    CCS_MasterTestUiConstants.MasterTestUiCanvasObjectName,
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster));
                changed = true;
            }

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                changed = true;
            }

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvasObject.AddComponent<CanvasScaler>();
                changed = true;
            }

            if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
                changed = true;
            }

            return canvas;
        }

        private static void EnsureFeedRootAnchoring(GameObject feedObject, ref bool changed)
        {
            RectTransform feedRect = feedObject.GetComponent<RectTransform>();
            if (feedRect == null)
            {
                feedRect = feedObject.AddComponent<RectTransform>();
                changed = true;
            }

            Vector2 topRightAnchor = new Vector2(1f, 1f);
            if (feedRect.anchorMin != topRightAnchor
                || feedRect.anchorMax != topRightAnchor
                || feedRect.pivot != topRightAnchor
                || feedRect.anchoredPosition != Vector2.zero)
            {
                feedRect.anchorMin = topRightAnchor;
                feedRect.anchorMax = topRightAnchor;
                feedRect.pivot = topRightAnchor;
                feedRect.anchoredPosition = Vector2.zero;
                feedRect.sizeDelta = Vector2.zero;
                changed = true;
            }
        }

        private static RectTransform EnsurePanel(Transform feedRoot, ref bool changed)
        {
            Transform panelTransform = feedRoot.Find(CCS_MasterTestUiConstants.JoinNotificationPanelObjectName);
            GameObject panelObject;
            if (panelTransform == null)
            {
                panelObject = new GameObject(
                    CCS_MasterTestUiConstants.JoinNotificationPanelObjectName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                    typeof(VerticalLayoutGroup),
                    typeof(ContentSizeFitter));
                panelObject.transform.SetParent(feedRoot, false);
                changed = true;
            }
            else
            {
                panelObject = panelTransform.gameObject;
            }

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f);
            panelRect.anchoredPosition = new Vector2(
                -CCS_MasterTestUiConstants.JoinNotificationPanelMargin,
                -CCS_MasterTestUiConstants.JoinNotificationPanelMargin);
            panelRect.sizeDelta = new Vector2(
                CCS_MasterTestUiConstants.JoinNotificationPanelWidth,
                CCS_MasterTestUiConstants.JoinNotificationPanelMinHeight);

            Image panelImage = panelObject.GetComponent<Image>();
            if (panelImage.color != CCS_MasterTestUiConstants.JoinPanelColor)
            {
                panelImage.color = CCS_MasterTestUiConstants.JoinPanelColor;
                changed = true;
            }

            panelImage.raycastTarget = false;

            VerticalLayoutGroup layout = panelObject.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 12, 12);
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = panelObject.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return panelRect;
        }

        private static TMP_Text EnsureTitle(RectTransform panel, ref bool changed)
        {
            Transform titleTransform = panel.Find(CCS_MasterTestUiConstants.JoinNotificationTitleObjectName);
            GameObject titleObject;
            if (titleTransform == null)
            {
                titleObject = new GameObject(
                    CCS_MasterTestUiConstants.JoinNotificationTitleObjectName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(TextMeshProUGUI),
                    typeof(LayoutElement));
                titleObject.transform.SetParent(panel, false);
                changed = true;
            }
            else
            {
                titleObject = titleTransform.gameObject;
            }

            LayoutElement layoutElement = titleObject.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = 24f;
            layoutElement.minHeight = 24f;

            TMP_Text titleText = titleObject.GetComponent<TextMeshProUGUI>();
            titleText.text = CCS_MasterTestUiConstants.JoinNotificationHeaderText;
            titleText.fontSize = 18f;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = CCS_MasterTestUiConstants.JoinTitleTextColor;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            titleText.raycastTarget = false;
            return titleText;
        }

        private static RectTransform EnsureEntriesContainer(RectTransform panel, ref bool changed)
        {
            Transform entriesTransform = panel.Find(CCS_MasterTestUiConstants.JoinNotificationEntriesObjectName);
            GameObject entriesObject;
            if (entriesTransform == null)
            {
                entriesObject = new GameObject(
                    CCS_MasterTestUiConstants.JoinNotificationEntriesObjectName,
                    typeof(RectTransform),
                    typeof(VerticalLayoutGroup),
                    typeof(ContentSizeFitter));
                entriesObject.transform.SetParent(panel, false);
                changed = true;
            }
            else
            {
                entriesObject = entriesTransform.gameObject;
            }

            VerticalLayoutGroup layout = entriesObject.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 4f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = entriesObject.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return entriesObject.GetComponent<RectTransform>();
        }

        private static TMP_Text EnsureEntryTemplate(RectTransform entriesContainer, ref bool changed)
        {
            Transform templateTransform = entriesContainer.Find(
                CCS_MasterTestUiConstants.JoinNotificationEntryTemplateObjectName);
            GameObject templateObject;
            if (templateTransform == null)
            {
                templateObject = new GameObject(
                    CCS_MasterTestUiConstants.JoinNotificationEntryTemplateObjectName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(CanvasGroup),
                    typeof(TextMeshProUGUI),
                    typeof(LayoutElement));
                templateObject.transform.SetParent(entriesContainer, false);
                changed = true;
            }
            else
            {
                templateObject = templateTransform.gameObject;
            }

            templateObject.SetActive(false);

            LayoutElement layoutElement = templateObject.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = 28f;
            layoutElement.minHeight = 28f;

            TMP_Text entryText = templateObject.GetComponent<TextMeshProUGUI>();
            entryText.text = "Player joined";
            entryText.fontSize = 16f;
            entryText.color = CCS_MasterTestUiConstants.JoinEntryTextColor;
            entryText.alignment = TextAlignmentOptions.MidlineLeft;
            entryText.raycastTarget = false;
            return entryText;
        }

        private static bool SetReference(SerializedObject serializedObject, string propertyName, Object value)
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
