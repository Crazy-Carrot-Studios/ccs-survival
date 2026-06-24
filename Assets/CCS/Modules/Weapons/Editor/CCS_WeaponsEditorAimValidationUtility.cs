using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WeaponsEditorAimValidationUtility
// CATEGORY: Modules / Weapons / Editor
// PURPOSE: Editor-only aim/muzzle orientation checks for visual-only weapon prefabs.
// PLACEMENT: Called from CCS_WeaponsModuleValidator during batch validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Validates FitGuides/MuzzlePoint forward axis points down the barrel.
// =============================================================================

namespace CCS.Modules.Weapons.Editor
{
    public static class CCS_WeaponsEditorAimValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateVisualOnlyMuzzlePointOrientation()
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_WeaponsConstants.RevolverM1879VisualOnlyPrefabPath);
            if (prefabRoot == null)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Missing PF_CCS_RevolverM1879_VisualOnly prefab for muzzle orientation validation.");
            }

            Transform muzzlePoint = CCS_WeaponMuzzlePointUtility.FindMuzzlePoint(prefabRoot.transform);
            if (muzzlePoint == null)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Visual-only revolver prefab is missing FitGuides/MuzzlePoint.");
            }

            if (!CCS_WeaponMuzzlePointUtility.IsMuzzleForwardAlignedWithBarrel(muzzlePoint))
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Visual-only revolver MuzzlePoint.forward appears misaligned with barrel axis. "
                    + "Fix the prefab guide orientation instead of runtime math.");
            }

            return CCS_SurvivalValidationResult.Pass(
                "Visual-only revolver MuzzlePoint forward axis aligns with barrel direction.");
        }
    }
}
