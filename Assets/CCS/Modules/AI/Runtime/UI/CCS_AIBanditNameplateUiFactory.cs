using TMPro;
using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_AIBanditNameplateUiFactory
// CATEGORY: Modules / AI / Runtime / UI
// PURPOSE: Builds world-space nameplate canvas, health bar, and name text UI.
// PLACEMENT: Static factory used by nameplate runtime and prefab builder.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Uses Image fill anchors for reliable world-space health bar visibility.
// =============================================================================

namespace CCS.Modules.AI
{
    public static class CCS_AIBanditNameplateUiFactory
    {
        private static readonly Color HealthFillColor = new Color(0.18f, 0.82f, 0.28f, 1f);
        private static readonly Color HealthBackgroundColor = new Color(0f, 0f, 0f, 0.65f);

        public static Transform EnsureNameplateAnchor(Transform aiRoot)
        {
            if (aiRoot == null)
            {
                return null;
            }

            Transform anchor = aiRoot.Find(CCS_AIConstants.NameplateAnchorObjectName);
            GameObject anchorObject;
            if (anchor == null)
            {
                anchorObject = new GameObject(CCS_AIConstants.NameplateAnchorObjectName);
                anchor = anchorObject.transform;
                anchor.SetParent(aiRoot, false);
            }
            else
            {
                anchorObject = anchor.gameObject;
            }

            anchor.localPosition = new Vector3(0f, CCS_AIConstants.NameplateWorldHeight, 0f);
            anchor.localRotation = Quaternion.identity;
            anchor.localScale = Vector3.one;
            anchorObject.SetActive(true);
            return anchor;
        }

        public static Transform EnsureNameplateRoot(Transform anchorTransform)
        {
            if (anchorTransform == null)
            {
                return null;
            }

            Transform nameplateRoot = anchorTransform.Find(CCS_AIConstants.NameplateRootObjectName);
            GameObject rootObject;
            if (nameplateRoot == null)
            {
                rootObject = new GameObject(CCS_AIConstants.NameplateRootObjectName);
                nameplateRoot = rootObject.transform;
                nameplateRoot.SetParent(anchorTransform, false);
            }
            else
            {
                rootObject = nameplateRoot.gameObject;
            }

            nameplateRoot.localPosition = Vector3.zero;
            nameplateRoot.localRotation = Quaternion.identity;
            nameplateRoot.localScale = Vector3.one;
            rootObject.SetActive(true);
            return nameplateRoot;
        }

        public static Transform EnsureCanvasRoot(Transform nameplateRoot)
        {
            if (nameplateRoot == null)
            {
                return null;
            }

            Transform canvasTransform = nameplateRoot.Find(CCS_AIConstants.NameplateCanvasObjectName);
            GameObject canvasObject;
            if (canvasTransform == null)
            {
                canvasObject = new GameObject(
                    CCS_AIConstants.NameplateCanvasObjectName,
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasRenderer));
                canvasTransform = canvasObject.transform;
                canvasTransform.SetParent(nameplateRoot, false);
            }
            else
            {
                canvasObject = canvasTransform.gameObject;
            }

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = null;
            canvas.overrideSorting = true;
            canvas.sortingOrder = CCS_AIConstants.NameplateCanvasSortingOrder;
            canvas.enabled = true;

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(180f, 64f);
            canvasRect.localScale = Vector3.one * CCS_AIConstants.NameplateCanvasScale;
            canvasRect.localPosition = Vector3.zero;
            canvasRect.localRotation = Quaternion.identity;
            canvasObject.SetActive(true);
            return canvasTransform;
        }

        public static Image EnsureHealthBar(Transform canvasRoot, out RectTransform fillRect)
        {
            fillRect = null;
            if (canvasRoot == null)
            {
                return null;
            }

            Transform backgroundTransform = canvasRoot.Find(CCS_AIConstants.NameplateHealthBackgroundObjectName);
            GameObject backgroundObject;
            if (backgroundTransform == null)
            {
                backgroundObject = new GameObject(
                    CCS_AIConstants.NameplateHealthBackgroundObjectName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                backgroundTransform = backgroundObject.transform;
                backgroundTransform.SetParent(canvasRoot, false);
            }
            else
            {
                backgroundObject = backgroundTransform.gameObject;
            }

            RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0.5f, 1f);
            backgroundRect.anchorMax = new Vector2(0.5f, 1f);
            backgroundRect.pivot = new Vector2(0.5f, 1f);
            backgroundRect.sizeDelta = new Vector2(140f, 14f);
            backgroundRect.anchoredPosition = new Vector2(0f, 12f);

            Image backgroundImage = backgroundObject.GetComponent<Image>();
            backgroundImage.color = HealthBackgroundColor;
            backgroundImage.raycastTarget = false;
            backgroundObject.SetActive(true);

            Transform fillTransform = backgroundTransform.Find(CCS_AIConstants.NameplateHealthFillObjectName);
            GameObject fillObject;
            if (fillTransform == null)
            {
                fillObject = new GameObject(
                    CCS_AIConstants.NameplateHealthFillObjectName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                fillTransform = fillObject.transform;
                fillTransform.SetParent(backgroundTransform, false);
            }
            else
            {
                fillObject = fillTransform.gameObject;
            }

            fillRect = fillObject.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.pivot = new Vector2(1f, 0.5f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;

            Image fillImage = fillObject.GetComponent<Image>();
            fillImage.color = HealthFillColor;
            fillImage.raycastTarget = false;
            fillObject.SetActive(true);
            SetHealthFillPercent(fillRect, fillImage, 1f);
            return fillImage;
        }

        public static TMP_Text EnsureNameText(Transform canvasRoot)
        {
            if (canvasRoot == null)
            {
                return null;
            }

            Transform textTransform = canvasRoot.Find(CCS_AIConstants.NameplateNameTextObjectName);
            GameObject textObject;
            if (textTransform == null)
            {
                textObject = new GameObject(
                    CCS_AIConstants.NameplateNameTextObjectName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(TextMeshProUGUI));
                textObject.transform.SetParent(canvasRoot, false);
            }
            else
            {
                textObject = textTransform.gameObject;
            }

            RectTransform nameRect = textObject.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.5f, 1f);
            nameRect.anchorMax = new Vector2(0.5f, 1f);
            nameRect.pivot = new Vector2(0.5f, 1f);
            nameRect.sizeDelta = new Vector2(160f, 24f);
            nameRect.anchoredPosition = new Vector2(0f, -12f);

            TMP_Text text = textObject.GetComponent<TMP_Text>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 22f;
            text.color = new Color(0.95f, 0.95f, 0.95f, 1f);
            text.text = CCS_AIConstants.AIBanditLabel;
            text.raycastTarget = false;
            textObject.SetActive(true);
            return text;
        }

        public static void SetHealthFillPercent(RectTransform fillRect, Image fillImage, float healthPercent)
        {
            healthPercent = Mathf.Clamp01(healthPercent);

            if (fillRect != null)
            {
                fillRect.anchorMin = new Vector2(1f - healthPercent, 0f);
                fillRect.anchorMax = new Vector2(1f, 1f);
                fillRect.pivot = new Vector2(1f, 0.5f);
                fillRect.offsetMin = Vector2.zero;
                fillRect.offsetMax = Vector2.zero;
                fillRect.anchoredPosition = Vector2.zero;
            }

            if (fillImage != null)
            {
                fillImage.type = Image.Type.Filled;
                fillImage.fillMethod = Image.FillMethod.Horizontal;
                fillImage.fillOrigin = (int)Image.OriginHorizontal.Right;
                fillImage.fillAmount = healthPercent;
            }
        }

        public static Slider EnsureHealthSlider(Transform canvasRoot)
        {
            Image fillImage = EnsureHealthBar(canvasRoot, out RectTransform fillRect);
            if (fillRect == null)
            {
                return null;
            }

            Transform backgroundTransform = canvasRoot.Find(CCS_AIConstants.NameplateHealthBackgroundObjectName);
            if (backgroundTransform == null)
            {
                return null;
            }

            Transform sliderTransform = canvasRoot.Find(CCS_AIConstants.NameplateHealthSliderObjectName);
            GameObject sliderObject;
            if (sliderTransform == null)
            {
                sliderObject = new GameObject(
                    CCS_AIConstants.NameplateHealthSliderObjectName,
                    typeof(RectTransform),
                    typeof(Slider));
                sliderTransform = sliderObject.transform;
                sliderTransform.SetParent(canvasRoot, false);
                sliderObject.SetActive(false);
            }
            else
            {
                sliderObject = sliderTransform.gameObject;
            }

            Slider slider = sliderObject.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.interactable = false;
            slider.direction = Slider.Direction.LeftToRight;
            slider.fillRect = fillRect;
            slider.targetGraphic = fillImage;
            slider.value = 1f;
            return slider;
        }
    }
}
