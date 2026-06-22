using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController;
using CCS.Project;
using TMPro;
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

            CCS_RevolverFireFeedback fireFeedback = prefabRoot.GetComponentInChildren<CCS_RevolverFireFeedback>(true);
            AppendIfMissing(
                failures,
                fireFeedback != null,
                $"{CCS_WeaponsConstants.NetworkedTestPlayerPrefabPath} must contain CCS_RevolverFireFeedback.");

            if (revolverController != null && revolverController is not CCS_IRevolverAnimationState)
            {
                failures.Add("CCS_RevolverController must implement CCS_IRevolverAnimationState.");
            }

            Transform muzzlePoint = FindDeepChild(prefabRoot.transform, CCS_WeaponsConstants.MuzzlePointObjectName);
            AppendIfMissing(
                failures,
                muzzlePoint != null,
                $"{CCS_WeaponsConstants.NetworkedTestPlayerPrefabPath} must contain {CCS_WeaponsConstants.MuzzlePointObjectName}.");

            if (muzzlePoint != null)
            {
                if (muzzlePoint.localPosition.y < CCS_WeaponsConstants.MuzzlePointMinimumLocalHeight)
                {
                    failures.Add(
                        $"{CCS_WeaponsConstants.MuzzlePointObjectName} local height must be above "
                        + $"{CCS_WeaponsConstants.MuzzlePointMinimumLocalHeight}m (hand/muzzle height, not feet).");
                }

                if (fireFeedback != null && fireFeedback.transform != muzzlePoint)
                {
                    failures.Add("CCS_RevolverFireFeedback must live on MuzzlePoint for muzzle-origin tracers.");
                }
            }

            CCS_RevolverHudPresenter hudPresenter = prefabRoot.GetComponentInChildren<CCS_RevolverHudPresenter>(true);
            AppendIfMissing(
                failures,
                hudPresenter != null,
                $"{CCS_WeaponsConstants.NetworkedTestPlayerPrefabPath} must contain CCS_RevolverHudPresenter.");

            Transform hudRoot = prefabRoot.transform.Find(CCS_WeaponsConstants.WeaponHudRootName);
            if (hudRoot != null && hudRoot.localScale == Vector3.zero)
            {
                failures.Add("WeaponHudRoot localScale must not be zero.");
            }

            TextMeshProUGUI hudText = prefabRoot.GetComponentInChildren<TextMeshProUGUI>(true);
            if (hudText != null)
            {
                if (hudText.color.a < 0.9f)
                {
                    failures.Add("Weapon HUD text alpha must be fully opaque for readability.");
                }

                if (hudText.color == Color.white)
                {
                    failures.Add("Weapon HUD ammo text must use readable dark green or red styling, not white.");
                }
            }

            Transform reticleTransform = hudRoot != null
                ? hudRoot.Find(CCS_WeaponsConstants.WeaponReticleObjectName)
                : null;
            if (reticleTransform != null)
            {
                RectTransform reticleRect = reticleTransform as RectTransform;
                if (reticleRect != null
                    && (reticleRect.anchorMin != new Vector2(0.5f, 0.5f)
                        || reticleRect.anchorMax != new Vector2(0.5f, 0.5f)
                        || reticleRect.anchoredPosition != Vector2.zero))
                {
                    failures.Add("Weapon reticle must stay screen-centered for aim alignment tests.");
                }
            }

            if (revolverController != null && revolverController.RevolverDefinition == null)
            {
                failures.Add("Test player revolver controller must assign CCS_RevolverDefinition_Test.");
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Canonical test player revolver wiring is valid.");
        }

        public static CCS_SurvivalValidationResult ValidateRevolverAnimationStateContract(
            CCS_RevolverController revolverController)
        {
            if (revolverController == null)
            {
                return CCS_SurvivalValidationResult.Pass(
                    "Revolver animation state contract skipped because controller is missing.");
            }

            if (revolverController is CCS_IRevolverAnimationState)
            {
                return CCS_SurvivalValidationResult.Pass(
                    "Revolver controller implements CCS_IRevolverAnimationState.");
            }

            return CCS_SurvivalValidationResult.Fail(
                "CCS_RevolverController must implement CCS_IRevolverAnimationState.");
        }

        public static CCS_SurvivalValidationResult ValidateRevolverFireFeedbackSourceContract()
        {
            List<string> failures = new List<string>();
            string feedbackPath = CCS_WeaponsConstants.ModuleRootPath
                + "/Runtime/Components/CCS_RevolverFireFeedback.cs";
            if (!File.Exists(feedbackPath))
            {
                return CCS_SurvivalValidationResult.Fail("Missing CCS_RevolverFireFeedback source.");
            }

            string source = File.ReadAllText(feedbackPath);
            AppendIfMissing(
                failures,
                source.Contains("muzzlePoint.position"),
                "CCS_RevolverFireFeedback must start tracers from muzzlePoint.position.");
            AppendIfMissing(
                failures,
                source.Contains("transform.root"),
                "CCS_RevolverFireFeedback must resolve muzzle from player hierarchy when miswired.");

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Revolver fire feedback uses muzzle-origin tracers.");
        }

        public static CCS_SurvivalValidationResult ValidateHitscanUsesCameraCenterAim()
        {
            List<string> failures = new List<string>();
            string raycasterPath = CCS_WeaponsConstants.ModuleRootPath
                + "/Runtime/Components/CCS_HitscanWeaponRaycaster.cs";
            if (!File.Exists(raycasterPath))
            {
                return CCS_SurvivalValidationResult.Fail("Missing CCS_HitscanWeaponRaycaster source.");
            }

            string source = File.ReadAllText(raycasterPath);
            AppendIfMissing(
                failures,
                source.Contains("ViewportPointToRay"),
                "Hitscan raycaster must build aim from camera viewport center.");
            AppendIfMissing(
                failures,
                source.Contains("CastFromCameraCenter"),
                "Hitscan raycaster must expose CastFromCameraCenter for reticle-aligned shots.");

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Hitscan raycaster uses camera-center aim selection.");
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
