using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WeaponsStrippedBaselineValidationUtility
// CATEGORY: Modules / Weapons / Editor
// PURPOSE: Validates shared weapon visuals/profiles for v0.7.13 stripped baseline.
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.Weapons.Editor
{
    public static class CCS_WeaponsStrippedBaselineValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateStrippedBaselineWeaponsFoundation(GameObject testPlayerPrefab)
        {
            List<string> failures = new List<string>();
            List<string> warnings = new List<string>();

            AppendResult(failures, CCS_WeaponsValidationUtility.ValidateModuleFoundation());

            CCS_RevolverDefinition revolverDefinition = AssetDatabase.LoadAssetAtPath<CCS_RevolverDefinition>(
                CCS_WeaponsConstants.RevolverDefinitionProfilePath);
            AppendIfMissing(
                failures,
                File.Exists(CCS_WeaponsConstants.RevolverDefinitionProfilePath),
                "Missing revolver definition asset at " + CCS_WeaponsConstants.RevolverDefinitionProfilePath + ".");
            AppendResult(failures, CCS_WeaponsValidationUtility.ValidateRevolverDefinition(revolverDefinition));
            AppendResult(failures, CCS_WeaponsValidationUtility.ValidateRevolverM1879VisualFoundation());

            AppendIfMissing(
                failures,
                File.Exists(CCS_EquipmentConstants.RevolverM1879VisualOnlyPrefabPath),
                "Missing visual-only revolver prefab at " + CCS_EquipmentConstants.RevolverM1879VisualOnlyPrefabPath + ".");
            AppendIfMissing(
                failures,
                File.Exists(CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath),
                "Missing right-hand pistol grip fit profile at "
                + CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath
                + ".");

            if (testPlayerPrefab != null)
            {
                AppendIfMissing(
                    failures,
                    testPlayerPrefab.GetComponent<CCS_RevolverController>() == null,
                    "Stripped baseline player prefab must not contain CCS_RevolverController.");
                AppendIfMissing(
                    failures,
                    !PrefabContainsTypeName(testPlayerPrefab, "CCS_MuzzleDrivenReticleController"),
                    "Stripped baseline player prefab must not contain CCS_MuzzleDrivenReticleController.");
                AppendIfMissing(
                    failures,
                    testPlayerPrefab.GetComponentInChildren<CCS_PlayerHolsteredRevolverVisualPresenter>(true) != null,
                    "Stripped baseline player prefab must contain CCS_PlayerHolsteredRevolverVisualPresenter.");
                AppendIfMissing(
                    failures,
                    testPlayerPrefab.GetComponentInChildren<CCS_PlayerEquippedRevolverAimVisualPresenter>(true) != null,
                    "Stripped baseline player prefab must contain CCS_PlayerEquippedRevolverAimVisualPresenter.");
                AppendIfMissing(
                    failures,
                    testPlayerPrefab.GetComponent<CCS_RevolverAimPresentationGate>() != null,
                    "Stripped baseline player prefab must contain CCS_RevolverAimPresentationGate.");
                AppendResult(
                    failures,
                    CCS_EquipmentSocketValidationUtility.ValidateStrippedBaselinePlayerEquipmentSocketFoundation(testPlayerPrefab));
            }
            else
            {
                failures.Add(
                    "Missing networked test player prefab at " + CCS_WeaponsConstants.NetworkedTestPlayerPrefabPath + ".");
            }

            if (File.Exists(CCS_WeaponsConstants.ModuleRootPath + "/Runtime/Components/CCS_RevolverController.cs"))
            {
                warnings.Add("CCS_RevolverController source remains for deferred gameplay review.");
            }

            string message = "Stripped baseline weapons visual/profile foundation validated.";
            if (warnings.Count > 0)
            {
                message += " Warnings: " + string.Join(" ", warnings);
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass(message);
        }

        private static bool PrefabContainsTypeName(GameObject prefabRoot, string typeName)
        {
            Component[] components = prefabRoot.GetComponentsInChildren<Component>(true);
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component != null && component.GetType().Name == typeName)
                {
                    return true;
                }
            }

            return false;
        }

        private static void AppendIfMissing(List<string> failures, bool condition, string message)
        {
            if (!condition)
            {
                failures.Add(message);
            }
        }

        private static void AppendResult(List<string> failures, CCS_SurvivalValidationResult result)
        {
            if (!result.IsSuccess)
            {
                failures.Add(result.Message);
            }
        }
    }
}
