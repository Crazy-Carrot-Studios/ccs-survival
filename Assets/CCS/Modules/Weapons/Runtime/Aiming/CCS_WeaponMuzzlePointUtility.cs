using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WeaponMuzzlePointUtility
// CATEGORY: Modules / Weapons / Runtime / Aiming
// PURPOSE: Locates MuzzlePoint on weapon visual hierarchies without hardcoded scene names.
// PLACEMENT: Runtime utility used by equipment visual controller and aim resolver callers.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Prefers FitGuides/MuzzlePoint, then any descendant named MuzzlePoint.
// =============================================================================

namespace CCS.Modules.Weapons
{
    public static class CCS_WeaponMuzzlePointUtility
    {
        public static Transform FindMuzzlePoint(Transform visualRoot)
        {
            if (visualRoot == null)
            {
                return null;
            }

            Transform fitGuides = visualRoot.Find(CCS_WeaponsConstants.FitGuidesObjectName);
            if (fitGuides != null)
            {
                Transform fitGuideMuzzle = fitGuides.Find(CCS_WeaponsConstants.MuzzlePointObjectName);
                if (fitGuideMuzzle != null)
                {
                    return fitGuideMuzzle;
                }
            }

            Transform directMuzzle = visualRoot.Find(CCS_WeaponsConstants.MuzzlePointObjectName);
            if (directMuzzle != null)
            {
                return directMuzzle;
            }

            return FindDeepChildByName(visualRoot, CCS_WeaponsConstants.MuzzlePointObjectName);
        }

        public static Vector3 ComputeBarrelAlignedLocalEuler(Vector3 muzzleLocalPosition)
        {
            Vector3 barrelForward = muzzleLocalPosition.sqrMagnitude > 0.0001f
                ? muzzleLocalPosition.normalized
                : Vector3.forward;
            return Quaternion.LookRotation(barrelForward).eulerAngles;
        }

        public static bool IsMuzzleForwardAlignedWithBarrel(Transform muzzlePoint, float minimumDot = 0.85f)
        {
            if (muzzlePoint == null)
            {
                return false;
            }

            Vector3 barrelAxisLocal = muzzlePoint.localPosition.sqrMagnitude > 0.0001f
                ? muzzlePoint.localPosition.normalized
                : Vector3.forward;
            float alignmentDot = Vector3.Dot(muzzlePoint.localRotation * Vector3.forward, barrelAxisLocal);
            return alignmentDot >= minimumDot;
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
