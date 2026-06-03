using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BootstrapZoneLabel
// CATEGORY: Survival / Runtime / Development / Bootstrap
// PURPOSE: World-space dev label for bootstrap scene zone readability.
// PLACEMENT: Child of CCS_BootstrapZone_* empty objects in SCN_CCS_Survival_Bootstrap.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Milestone 2.1.2 — dev-only scene organization; no gameplay logic.
// =============================================================================

namespace CCS.Survival.Development
{
    [ExecuteAlways]
    public sealed class CCS_BootstrapZoneLabel : MonoBehaviour
    {
        #region Variables

        [Tooltip("Label text shown above the bootstrap zone anchor.")]
        [SerializeField] private string labelText = "Bootstrap Zone";

        [Tooltip("World-space height above the zone anchor.")]
        [SerializeField] private float labelHeight = 3.5f;

        [Tooltip("TextMesh character size.")]
        [SerializeField] private float characterSize = 0.35f;

        [Tooltip("Label tint for quick zone identification.")]
        [SerializeField] private Color labelColor = Color.white;

        private TextMesh textMesh;

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            EnsureLabel();
            ApplyLabel();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsureLabel();
            ApplyLabel();
        }
#endif

        #endregion

        #region Public Methods

        public void ConfigureLabel(string text, Color color, float height = 3.5f, float size = 0.35f)
        {
            labelText = text;
            labelColor = color;
            labelHeight = height;
            characterSize = size;
            ApplyLabel();
        }

        #endregion

        #region Private Methods

        private void EnsureLabel()
        {
            if (textMesh == null)
            {
                textMesh = GetComponentInChildren<TextMesh>(true);
            }

            if (textMesh != null)
            {
                return;
            }

            GameObject labelObject = new GameObject("ZoneLabel");
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = new Vector3(0f, labelHeight, 0f);
            labelObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            textMesh = labelObject.AddComponent<TextMesh>();
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontSize = 64;
        }

        private void ApplyLabel()
        {
            if (textMesh == null)
            {
                return;
            }

            textMesh.text = labelText;
            textMesh.color = labelColor;
            textMesh.characterSize = characterSize;
            textMesh.transform.localPosition = new Vector3(0f, labelHeight, 0f);
        }

        #endregion
    }
}
