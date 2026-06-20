using TMPro;
using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_AttributeBarView
// CATEGORY: Modules / Attributes / Runtime / UI
// PURPOSE: Single attribute bar with label, value text, and horizontal fill image.
// PLACEMENT: Child of AttributeBarsPanel on networked test player prefab.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Fill scales left-to-right via width and Image fill amount. Values clamp to max.
// =============================================================================

namespace CCS.Modules.Attributes
{
    public sealed class CCS_AttributeBarView : MonoBehaviour
    {
        #region Variables

        [SerializeField] private TMP_Text labelText;

        [SerializeField] private TMP_Text valueText;

        [SerializeField] private TMP_Text statusText;

        [SerializeField] private Image fillImage;

        private static Sprite defaultFillSprite;

        private RectTransform fillRect;

        private RectTransform fillBackgroundRect;

        private bool fillLayoutConfigured;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveFillReferences();
        }

        #endregion

        #region Public Methods

        public void SetValues(string label, float current, float max, string statusSuffix = null)
        {
            float clampedMax = Mathf.Max(0f, max);
            float clampedCurrent = Mathf.Clamp(current, 0f, clampedMax > 0f ? clampedMax : 0f);

            if (labelText != null)
            {
                labelText.text = label;
            }

            if (valueText != null)
            {
                valueText.text = $"{Mathf.RoundToInt(clampedCurrent)} / {Mathf.RoundToInt(clampedMax)}";
            }

            if (statusText != null)
            {
                bool hasStatus = !string.IsNullOrEmpty(statusSuffix);
                statusText.gameObject.SetActive(hasStatus);
                if (hasStatus)
                {
                    statusText.text = statusSuffix;
                }
            }

            SetFillAmount(clampedMax > 0f ? clampedCurrent / clampedMax : 0f);
        }

        public void SetFillColor(Color color)
        {
            if (fillImage != null)
            {
                fillImage.color = color;
            }
        }

        public void SetFillAmount(float normalizedAmount)
        {
            if (fillImage == null)
            {
                return;
            }

            ResolveFillReferences();
            EnsureFillImageFilledMode();

            float clampedAmount = Mathf.Clamp01(normalizedAmount);
            fillImage.fillAmount = clampedAmount;
            ApplyFillWidth(clampedAmount);
        }

        #endregion

        #region Private Methods

        private void ResolveFillReferences()
        {
            if (fillImage == null)
            {
                return;
            }

            fillRect = fillImage.rectTransform;
            if (fillBackgroundRect == null && fillRect.parent is RectTransform parentRect)
            {
                fillBackgroundRect = parentRect;
            }

            EnsureFillSprite();
            ConfigureFillRectAnchors();
        }

        private void EnsureFillSprite()
        {
            if (fillImage.sprite != null)
            {
                return;
            }

            if (defaultFillSprite == null)
            {
                defaultFillSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            }

            if (defaultFillSprite != null)
            {
                fillImage.sprite = defaultFillSprite;
            }
        }

        private void ConfigureFillRectAnchors()
        {
            if (fillRect == null || fillLayoutConfigured)
            {
                return;
            }

            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.anchoredPosition = Vector2.zero;
            fillRect.offsetMin = new Vector2(0f, 0f);
            fillRect.offsetMax = new Vector2(0f, 0f);
            fillLayoutConfigured = true;
        }

        private void EnsureFillImageFilledMode()
        {
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        }

        private void ApplyFillWidth(float normalizedAmount)
        {
            if (fillRect == null || fillBackgroundRect == null)
            {
                return;
            }

            float backgroundWidth = fillBackgroundRect.rect.width;
            if (backgroundWidth <= 0.01f)
            {
                backgroundWidth = fillBackgroundRect.sizeDelta.x;
            }

            fillRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, backgroundWidth * normalizedAmount);
        }

        #endregion
    }
}
