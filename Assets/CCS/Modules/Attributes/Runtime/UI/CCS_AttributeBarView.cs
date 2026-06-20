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
// NOTES: Fill scales left-to-right via Image fill amount. Values clamp to max.
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

            fillImage.fillAmount = Mathf.Clamp01(normalizedAmount);
        }

        #endregion
    }
}
