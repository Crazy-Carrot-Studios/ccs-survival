using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WeaponVisualAnchorUtility
// CATEGORY: Modules / Weapons / Runtime / Aiming
// PURPOSE: Locates FitGuides anchor transforms on equipped weapon visuals.
// PLACEMENT: Runtime utility used by fire feedback and reload shell extraction.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Prefers FitGuides children; no hardcoded scene object names.
// =============================================================================

namespace CCS.Modules.Weapons
{
    public static class CCS_WeaponVisualAnchorUtility
    {
        public static Transform FindFitGuideAnchor(Transform visualRoot, string anchorName)
        {
            if (visualRoot == null || string.IsNullOrEmpty(anchorName))
            {
                return null;
            }

            Transform fitGuides = visualRoot.Find(CCS_WeaponsConstants.FitGuidesObjectName);
            if (fitGuides != null)
            {
                Transform direct = fitGuides.Find(anchorName);
                if (direct != null)
                {
                    return direct;
                }
            }

            return FindDeepChildByName(visualRoot, anchorName);
        }

        public static Transform FindCylinderPoint(Transform visualRoot)
        {
            return FindFitGuideAnchor(visualRoot, CCS_WeaponsConstants.CylinderPointObjectName);
        }

        public static Transform FindShellEjectPoint(Transform visualRoot)
        {
            return FindFitGuideAnchor(visualRoot, CCS_WeaponsConstants.ShellEjectPointObjectName);
        }

        private static Transform FindDeepChildByName(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform match = FindDeepChildByName(root.GetChild(i), childName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }
    }
}
