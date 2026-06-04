using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BusinessPresenceLabel
// CATEGORY: Modules / Settlements / Runtime / BusinessPresence
// PURPOSE: World-space label for business name and presence status.
// PLACEMENT: Child of CCS_BusinessPresenceAnchor.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.8.0 — Active / Inactive / Locked dev-readable text.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_BusinessPresenceLabel : MonoBehaviour
    {
        [SerializeField] private float labelHeight = 2.2f;

        [SerializeField] private float characterSize = 0.12f;

        [SerializeField] private Color activeColor = new Color(0.3f, 1f, 0.45f, 1f);

        [SerializeField] private Color inactiveColor = new Color(1f, 0.9f, 0.35f, 1f);

        [SerializeField] private Color lockedColor = new Color(0.75f, 0.75f, 0.75f, 1f);

        private TextMesh textMesh;

        private void Awake()
        {
            EnsureLabel();
        }

        public void ApplyPresence(string displayName, CCS_BusinessPresenceStatus status)
        {
            EnsureLabel();
            if (textMesh == null)
            {
                return;
            }

            string statusText = status switch
            {
                CCS_BusinessPresenceStatus.Active => "Active",
                CCS_BusinessPresenceStatus.Inactive => "Inactive",
                _ => "Locked"
            };

            textMesh.text = $"{displayName}\n{statusText}";
            textMesh.color = status switch
            {
                CCS_BusinessPresenceStatus.Active => activeColor,
                CCS_BusinessPresenceStatus.Inactive => inactiveColor,
                _ => lockedColor
            };
        }

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

            GameObject labelObject = new GameObject("BusinessPresenceLabel");
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = new Vector3(0f, labelHeight, 0f);
            labelObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            textMesh = labelObject.AddComponent<TextMesh>();
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontSize = 48;
            textMesh.characterSize = characterSize;
        }
    }
}
