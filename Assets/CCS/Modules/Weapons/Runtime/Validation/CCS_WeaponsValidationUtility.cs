using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController;
using CCS.Project;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WeaponsValidationUtility
// CATEGORY: Modules / Weapons / Runtime / Validation
// PURPOSE: Runtime validation helpers for the Weapons module foundation.
// PLACEMENT: Called from editor validators and future module installers.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.0 validates revolver profile, test player wiring, and source contracts.
// =============================================================================

namespace CCS.Modules.Weapons
{
    public static class CCS_WeaponsValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateModuleFoundation()
        {
            List<string> failures = new List<string>();

            AppendIfMissing(
                failures,
                Directory.Exists(CCS_WeaponsConstants.ModuleRootPath + "/Runtime"),
                "Missing Weapons Runtime folder.");
            AppendIfMissing(
                failures,
                Directory.Exists(CCS_WeaponsConstants.ModuleRootPath + "/Editor"),
                "Missing Weapons Editor folder.");
            AppendIfMissing(
                failures,
                Directory.Exists(CCS_WeaponsConstants.ModuleRootPath + "/Tests"),
                "Missing Weapons Tests folder.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_WeaponsConstants.ModuleRootPath + "/Runtime/CCS.Modules.Weapons.Runtime.asmdef"),
                "Missing CCS.Modules.Weapons.Runtime.asmdef.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_WeaponsConstants.ModuleRootPath + "/Editor/CCS.Modules.Weapons.Editor.asmdef"),
                "Missing CCS.Modules.Weapons.Editor.asmdef.");

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Weapons module foundation folders and asmdefs are present.");
        }

        public static CCS_SurvivalValidationResult ValidateRevolverDefinition(CCS_RevolverDefinition definition)
        {
            List<string> failures = new List<string>();
            AppendIfMissing(failures, definition != null, "Revolver definition asset is missing.");

            if (definition == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            CCS_SurvivalValidationResult profileValidation =
                CCS_SurvivalProfileValidationUtility.ValidateProfile(definition);
            AppendIfMissing(failures, profileValidation.IsSuccess, profileValidation.Message);
            AppendIfMissing(
                failures,
                definition.ProfileId == CCS_WeaponsConstants.RevolverDefinitionProfileId,
                $"Revolver profileId must be {CCS_WeaponsConstants.RevolverDefinitionProfileId}.");
            AppendIfMissing(
                failures,
                definition.CylinderCapacity == 6,
                "Test revolver cylinderCapacity must be 6.");
            AppendIfMissing(
                failures,
                Mathf.Approximately(definition.Damage, 25f),
                "Test revolver damage must be 25.");

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Revolver definition profile is valid.");
        }

        public static CCS_SurvivalValidationResult ValidatePlayerRevolverComponents(GameObject prefabRoot)
        {
            List<string> failures = new List<string>();
            AppendIfMissing(failures, prefabRoot != null, "Canonical test player prefab is missing.");

            if (prefabRoot == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            CCS_RevolverController revolverController = prefabRoot.GetComponent<CCS_RevolverController>();
            AppendIfMissing(
                failures,
                revolverController != null,
                $"{CCS_WeaponsConstants.NetworkedTestPlayerPrefabPath} must contain CCS_RevolverController.");

            CCS_CharacterInputActionProvider inputProvider = prefabRoot.GetComponent<CCS_CharacterInputActionProvider>();
            AppendIfMissing(
                failures,
                inputProvider != null,
                $"{CCS_WeaponsConstants.NetworkedTestPlayerPrefabPath} must contain CCS_CharacterInputActionProvider.");

            CCS_RevolverHudPresenter hudPresenter = prefabRoot.GetComponentInChildren<CCS_RevolverHudPresenter>(true);
            AppendIfMissing(
                failures,
                hudPresenter != null,
                $"{CCS_WeaponsConstants.NetworkedTestPlayerPrefabPath} must contain CCS_RevolverHudPresenter.");

            Transform muzzlePoint = FindDeepChild(prefabRoot.transform, CCS_WeaponsConstants.MuzzlePointObjectName);
            AppendIfMissing(
                failures,
                muzzlePoint != null,
                $"{CCS_WeaponsConstants.NetworkedTestPlayerPrefabPath} must contain {CCS_WeaponsConstants.MuzzlePointObjectName}.");

            if (revolverController != null && revolverController.RevolverDefinition == null)
            {
                failures.Add("Test player revolver controller must assign CCS_RevolverDefinition_Test.");
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Canonical test player revolver wiring is valid.");
        }

        #endregion

        #region Private Methods

        private static Transform FindDeepChild(Transform root, string childName)
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
                Transform match = FindDeepChild(root.GetChild(i), childName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static void AppendIfMissing(List<string> failures, bool condition, string message)
        {
            if (!condition)
            {
                failures.Add(message);
            }
        }

        #endregion
    }
}
