using System.Collections.Generic;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WeaponsModuleValidator
// CATEGORY: Modules / Weapons / Editor
// PURPOSE: Validates Weapons module foundation, assets, and test integration wiring.
// PLACEMENT: Editor validator invoked from CCS/Weapons/Validate Weapons Module.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.7.13 validates shared weapon visuals/profiles for stripped baseline only.
// =============================================================================

namespace CCS.Modules.Weapons.Editor
{
    public static class CCS_WeaponsModuleValidator
    {
        public static CCS_SurvivalValidationResult ValidateWeaponsModule()
        {
            List<string> failures = new List<string>();

            GameObject testPlayerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_WeaponsConstants.NetworkedTestPlayerPrefabPath);
            AppendIfMissing(
                failures,
                testPlayerPrefab != null,
                $"Missing networked test player prefab at {CCS_WeaponsConstants.NetworkedTestPlayerPrefabPath}.");

            AppendResult(
                failures,
                CCS_WeaponsStrippedBaselineValidationUtility.ValidateStrippedBaselineWeaponsFoundation(testPlayerPrefab));

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass(
                    "Stripped baseline weapons visual/profile assets and player wiring are valid.");
        }

        private static void AppendResult(List<string> failures, CCS_SurvivalValidationResult result)
        {
            if (!result.IsSuccess)
            {
                failures.Add(result.Message);
            }
        }

        private static void AppendIfMissing(List<string> failures, bool condition, string message)
        {
            if (!condition)
            {
                failures.Add(message);
            }
        }
    }
}
