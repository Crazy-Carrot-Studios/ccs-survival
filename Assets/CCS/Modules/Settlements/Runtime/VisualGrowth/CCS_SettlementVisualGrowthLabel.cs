using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementVisualGrowthLabel
// CATEGORY: Modules / Settlements / Runtime / VisualGrowth
// PURPOSE: TextMesh label for settlement growth marker name and stage status.
// PLACEMENT: Child of CCS_SettlementVisualGrowthAnchor.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.9.0 — dev-readable labels only.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementVisualGrowthLabel : MonoBehaviour
    {
        [SerializeField] private float labelHeight = 2.2f;

        [SerializeField] private int fontSize = 28;

        private TextMesh textMesh;

        private void Awake()
        {
            EnsureTextMesh();
        }

        public void ApplyGrowthVisual(string displayName, CCS_SettlementVisualGrowthStatus status)
        {
            EnsureTextMesh();
            if (textMesh == null)
            {
                return;
            }

            string statusLabel = status switch
            {
                CCS_SettlementVisualGrowthStatus.Active => "Active",
                CCS_SettlementVisualGrowthStatus.Inactive => "Inactive",
                _ => "Locked"
            };

            textMesh.text = $"{displayName}\n{statusLabel}";
            textMesh.color = status == CCS_SettlementVisualGrowthStatus.Active
                ? new Color(0.85f, 0.95f, 1f, 1f)
                : new Color(0.75f, 0.75f, 0.75f, 1f);
        }

        private void EnsureTextMesh()
        {
            if (textMesh != null)
            {
                return;
            }

            textMesh = GetComponent<TextMesh>();
            if (textMesh == null)
            {
                textMesh = gameObject.AddComponent<TextMesh>();
            }

            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontSize = fontSize;
            textMesh.characterSize = 0.08f;
            transform.localPosition = new Vector3(0f, labelHeight, 0f);
        }
    }
}
