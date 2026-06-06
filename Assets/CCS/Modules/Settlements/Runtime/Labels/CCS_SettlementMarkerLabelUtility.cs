using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementMarkerLabelUtility
// CATEGORY: Modules / Settlements / Runtime / Labels
// PURPOSE: Shared world-space TextMesh sizing for settlement dev marker labels.
// PLACEMENT: Used by housing, business, population, and visual growth label components.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Keeps labels readable without parent-scale or default characterSize blow-up.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_SettlementMarkerLabelUtility
    {
        public const int DefaultFontSize = 28;

        public const float DefaultCharacterSize = 0.08f;

        public static void ApplyStandardLayout(Transform labelTransform, TextMesh textMesh, float labelHeight)
        {
            if (labelTransform == null || textMesh == null)
            {
                return;
            }

            labelTransform.localScale = Vector3.one;
            labelTransform.localRotation = Quaternion.identity;
            labelTransform.localPosition = new Vector3(0f, labelHeight, 0f);
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontSize = DefaultFontSize;
            textMesh.characterSize = DefaultCharacterSize;
        }

        public static TextMesh EnsureTextMeshOnHost(MonoBehaviour host, ref TextMesh cachedMesh)
        {
            if (host == null)
            {
                return cachedMesh;
            }

            RemoveLegacyChildTextMeshes(host.transform, host.gameObject);
            if (cachedMesh == null)
            {
                cachedMesh = host.GetComponent<TextMesh>();
            }

            if (cachedMesh == null)
            {
                cachedMesh = host.gameObject.AddComponent<TextMesh>();
            }

            return cachedMesh;
        }

        private static void RemoveLegacyChildTextMeshes(Transform hostTransform, GameObject hostObject)
        {
            TextMesh[] childMeshes = hostTransform.GetComponentsInChildren<TextMesh>(true);
            for (int index = 0; index < childMeshes.Length; index++)
            {
                TextMesh childMesh = childMeshes[index];
                if (childMesh == null || childMesh.gameObject == hostObject)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Object.Destroy(childMesh.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(childMesh.gameObject);
                }
            }
        }
    }
}
