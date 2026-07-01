using System;
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
                source.Contains("HitscanResult.RayOrigin"),
                "CCS_RevolverFireFeedback must start tracers from resolved hitscan muzzle origin.");
            AppendIfMissing(
                failures,
                source.Contains("HitscanResult.HitPoint") || source.Contains("hitscan.HitPoint")
                    || source.Contains("HitscanResult.RayDirection") || source.Contains("hitscan.RayDirection"),
                "CCS_RevolverFireFeedback must end tracers at resolved hit or aim direction.");
            AppendIfMissing(
                failures,
                source.Contains("transform.root"),
                "CCS_RevolverFireFeedback must resolve muzzle from player hierarchy when miswired.");

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Revolver fire feedback uses aim-resolved muzzle-origin tracers.");
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
                source.Contains("CCS_WeaponAimResolver"),
                "Hitscan raycaster must delegate to CCS_WeaponAimResolver.");
            AppendIfMissing(
                failures,
                source.Contains("CastFromAimResolver"),
                "Hitscan raycaster must expose CastFromAimResolver.");

            AppendResult(failures, ValidateWeaponShotResolverFoundation());

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Hitscan raycaster uses camera-center aim selection.");
        }

        public static CCS_SurvivalValidationResult ValidateWeaponShotResolverFoundation()
        {
            List<string> failures = new List<string>();
            string shotResolverPath = CCS_WeaponsConstants.ModuleRootPath
                + "/Runtime/Aiming/CCS_WeaponShotResolver.cs";
            AppendIfMissing(failures, File.Exists(shotResolverPath), "Missing CCS_WeaponShotResolver.");

            string controllerPath = CCS_WeaponsConstants.ModuleRootPath
                + "/Runtime/Components/CCS_RevolverController.cs";
            if (File.Exists(shotResolverPath))
            {
                string shotResolverSource = File.ReadAllText(shotResolverPath);
                AppendIfMissing(
                    failures,
                    shotResolverSource.Contains("LocalPlayerCameraCenter"),
                    "CCS_WeaponShotResolver must support LocalPlayerCameraCenter aim mode.");
                AppendIfMissing(
                    failures,
                    shotResolverSource.Contains("AIAimTarget"),
                    "CCS_WeaponShotResolver must support AIAimTarget aim mode.");
                AppendIfMissing(
                    failures,
                    shotResolverSource.Contains("DebugMuzzleForwardOnly"),
                    "CCS_WeaponShotResolver must support DebugMuzzleForwardOnly aim mode.");
            }

            if (File.Exists(controllerPath))
            {
                string controllerSource = File.ReadAllText(controllerPath);
                AppendIfMissing(
                    failures,
                    controllerSource.Contains("CCS_WeaponShotResolver.ResolveShot"),
                    "CCS_RevolverController fire path must use CCS_WeaponShotResolver.");
                AppendIfMissing(
                    failures,
                    controllerSource.Contains("LocalPlayerCameraCenter"),
                    "CCS_RevolverController must default player shots to LocalPlayerCameraCenter.");
                AppendIfMissing(
                    failures,
                    !controllerSource.Contains("enableMuzzleAuthoritativeShots = true"),
                    "CCS_RevolverController must not default enableMuzzleAuthoritativeShots to true.");
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Weapon shot resolver uses camera-center aim by default.");
        }

        public static CCS_SurvivalValidationResult ValidateRevolverM1879VisualFoundation()
        {
            List<string> failures = new List<string>();
            AppendIfMissing(
                failures,
                Directory.Exists(CCS_WeaponsConstants.RevolverM1879ContentRootPath),
                "Missing RevolverM1879 content root folder.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_WeaponsConstants.RevolverM1879ModelAssetPath),
                "Missing CCS_RevolverM1879_Model.fbx.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_WeaponsConstants.RevolverM1879VisualDefinitionPath),
                "Missing CCS_RevolverM1879VisualDefinition.asset.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_WeaponsConstants.RevolverM1879WorldPickupPrefabPath),
                "Missing PF_CCS_RevolverM1879_WorldPickup.prefab.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_WeaponsConstants.RevolverM1879MaterializedVisualPrefabPath),
                "Missing PF_CCS_RevolverM1879_MaterializedVisual.prefab.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_WeaponsConstants.RevolverM1879VisualOnlyPrefabPath),
                "Missing PF_CCS_RevolverM1879_VisualOnly.prefab.");
            AppendIfMissing(
                failures,
                !File.Exists(CCS_WeaponsConstants.RevolverM1879HolsteredPrefabPath),
                "Holstered revolver prefab must be removed for profile-driven runtime visual scope.");
            AppendIfMissing(
                failures,
                !File.Exists(CCS_WeaponsConstants.RevolverM1879EquippedPrefabPath),
                "Equipped revolver prefab must be removed for world-pickup-only scope.");
            AppendIfMissing(
                failures,
                !File.Exists(CCS_WeaponsConstants.RevolverM1879BulletVisualPrefabPath),
                "Bullet visual prefab must be removed for world-pickup-only scope.");
            AppendIfMissing(
                failures,
                !File.Exists(CCS_WeaponsConstants.RevolverM1879ShellVisualPrefabPath),
                "Shell visual prefab must be removed for world-pickup-only scope.");

            ValidatePrefabUsesCcsAssetsOnly(failures, CCS_WeaponsConstants.RevolverM1879MaterializedVisualPrefabPath);
            ValidatePrefabUsesCcsAssetsOnly(failures, CCS_WeaponsConstants.RevolverM1879VisualOnlyPrefabPath);
            ValidatePrefabUsesCcsAssetsOnly(failures, CCS_WeaponsConstants.RevolverM1879WorldPickupPrefabPath);
            ValidateVisualOnlyPrefabContent(failures, CCS_WeaponsConstants.RevolverM1879VisualOnlyPrefabPath);
            ValidateWorldPickupPrefabContent(failures, CCS_WeaponsConstants.RevolverM1879WorldPickupPrefabPath);

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Revolver M1879 visual foundation and visual-only prefab are present.");
        }

        public static CCS_SurvivalValidationResult ValidatePlayerEquipmentVisualBridge(GameObject prefabRoot)
        {
            List<string> failures = new List<string>();
            AppendIfMissing(failures, prefabRoot != null, "Canonical test player prefab is missing.");
            if (prefabRoot == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            CCS_PlayerEquipmentVisualController visualController =
                prefabRoot.GetComponent<CCS_PlayerEquipmentVisualController>();
            AppendIfMissing(
                failures,
                visualController != null,
                "Test player must contain CCS_PlayerEquipmentVisualController.");
            AppendIfMissing(
                failures,
                prefabRoot.GetComponent<CCS_PlayerWeaponLoadout>() != null,
                "Test player must contain CCS_PlayerWeaponLoadout.");
            AppendIfMissing(
                failures,
                prefabRoot.GetComponent<CCS_EquipmentSocketRegistry>() != null,
                "Test player must contain CCS_EquipmentSocketRegistry.");
            AppendIfMissing(
                failures,
                prefabRoot.GetComponent<CCS_CharacterAimLocomotionController>() != null,
                "Test player must contain CCS_CharacterAimLocomotionController.");

            if (visualController != null)
            {
                AppendIfMissing(
                    failures,
                    visualController.HasVisualBridgeWiring,
                    "Equipment visual controller must reference socket registry, loadout, aim state, visual-only prefab, and fit profiles.");

                if (visualController.RightHipHolsterFitProfile != null)
                {
                    AppendIfMissing(
                        failures,
                        visualController.RightHipHolsterFitProfile.name
                            == "CCS_RevolverM1879_RightHipHolster_Fit",
                        "Equipment visual controller must reference CCS_RevolverM1879_RightHipHolster_Fit.asset.");
                }

                if (visualController.RightHandEquippedFitProfile != null)
                {
                    AppendIfMissing(
                        failures,
                        visualController.RightHandEquippedFitProfile.name
                            == "CCS_RevolverM1879_RightHandEquipped_Fit",
                        "Equipment visual controller must reference CCS_RevolverM1879_RightHandEquipped_Fit.asset.");
                }
            }

            for (int i = 0; i < CCS_EquipmentConstants.RuntimeTemporaryObjectNames.Length; i++)
            {
                AppendIfMissing(
                    failures,
                    FindDeepChild(prefabRoot.transform, CCS_EquipmentConstants.RuntimeTemporaryObjectNames[i]) == null,
                    "Test player prefab must not contain saved runtime visual object "
                    + CCS_EquipmentConstants.RuntimeTemporaryObjectNames[i]
                    + ".");
            }

            AppendIfMissing(
                failures,
                FindDeepChild(prefabRoot.transform, "PF_CCS_RevolverM1879_Holstered_Instance") == null,
                "Test player must not contain holstered revolver visual instance.");
            AppendIfMissing(
                failures,
                FindDeepChild(prefabRoot.transform, "PF_CCS_RevolverM1879_Equipped_Instance") == null,
                "Test player must not contain equipped revolver visual instance.");

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Player equipment visual bridge is wired without saved runtime visuals.");
        }

        public static CCS_SurvivalValidationResult ValidatePlayerWeaponLoadoutComponents(GameObject prefabRoot)
        {
            return ValidatePlayerEquipmentVisualBridge(prefabRoot);
        }

        public static CCS_SurvivalValidationResult ValidateRevolverOwnershipAndMuzzleContract()
        {
            List<string> failures = new List<string>();
            string controllerPath = CCS_WeaponsConstants.ModuleRootPath + "/Runtime/Components/CCS_RevolverController.cs";
            if (!File.Exists(controllerPath))
            {
                return CCS_SurvivalValidationResult.Fail("Missing CCS_RevolverController source.");
            }

            string source = File.ReadAllText(controllerPath);
            AppendIfMissing(
                failures,
                source.Contains("SetWeaponOwnershipActive"),
                "CCS_RevolverController must gate gameplay on weapon ownership.");
            AppendIfMissing(
                failures,
                source.Contains("weaponOwnershipActive"),
                "CCS_RevolverController must track weapon ownership state.");
            AppendIfMissing(
                failures,
                source.Contains("!IsAiming"),
                "CCS_RevolverController must block fire/reload when not aiming.");

            string loadoutPath = CCS_WeaponsConstants.ModuleRootPath + "/Runtime/Components/CCS_PlayerWeaponLoadout.cs";
            if (File.Exists(loadoutPath))
            {
                string loadoutSource = File.ReadAllText(loadoutPath);
                AppendIfMissing(
                    failures,
                    loadoutSource.Contains("GrantWeapon"),
                    "CCS_PlayerWeaponLoadout must grant weapon ownership from world pickup.");
                AppendIfMissing(
                    failures,
                    loadoutSource.Contains("RevolverGranted"),
                    "CCS_PlayerWeaponLoadout must expose RevolverGranted for visual bridge wiring.");
            }

            string visualControllerPath =
                CCS_WeaponsConstants.ModuleRootPath + "/Runtime/Components/CCS_PlayerEquipmentVisualController.cs";
            AppendIfMissing(
                failures,
                File.Exists(visualControllerPath),
                "Missing CCS_PlayerEquipmentVisualController source.");
            if (File.Exists(visualControllerPath))
            {
                string visualSource = File.ReadAllText(visualControllerPath);
                AppendIfMissing(
                    failures,
                    visualSource.Contains("RuntimeHolsteredVisualObjectName")
                        || visualSource.Contains("CCS_RUNTIME_Revolver_HolsteredVisual"),
                    "Equipment visual controller must spawn holstered runtime visual instances.");
                AppendIfMissing(
                    failures,
                    visualSource.Contains("RuntimeEquippedVisualObjectName")
                        || visualSource.Contains("CCS_RUNTIME_Revolver_EquippedVisual"),
                    "Equipment visual controller must spawn equipped runtime visual instances.");
                AppendIfMissing(
                    failures,
                    visualSource.Contains("CCS_WeaponMuzzlePointUtility")
                        || visualSource.Contains("CurrentAimConvergence")
                        || visualSource.Contains("BindEquippedVisual"),
                    "Equipment visual controller must resolve equipped visual muzzle via convergence or utility.");
                AppendIfMissing(
                    failures,
                    !visualSource.Contains("SetMuzzlePoint"),
                    "Equipment visual controller must not rebind gameplay muzzle.");
                AppendIfMissing(
                    failures,
                    visualSource.Contains("ReloadFitProfilesFromDisk"),
                    "Equipment visual controller must reload fit profiles from disk in editor.");
                AppendIfMissing(
                    failures,
                    visualSource.Contains("CCS_WeaponAttachmentFitProfileApplicator"),
                    "Equipment visual controller must apply saved profiles through attachment-root applicator.");
                AppendIfMissing(
                    failures,
                    visualSource.Contains("debugEquipmentVisualProfileApplication"),
                    "Equipment visual controller must gate profile application logs behind debugEquipmentVisualProfileApplication.");
                AppendIfMissing(
                    failures,
                    visualSource.Contains("debugRuntimeFitParity"),
                    "Equipment visual controller must expose debugRuntimeFitParity.");
                AppendIfMissing(
                    failures,
                    !visualSource.Contains("LogHolsterProfileApplication(profile);")
                        || visualSource.Contains("debugEquipmentVisualProfileApplication"),
                    "Equipment visual controller must not always log holster profile application.");
                AppendIfMissing(
                    failures,
                    !visualSource.Contains("LogEquippedProfileApplication(profile);")
                        || visualSource.Contains("debugEquipmentVisualProfileApplication"),
                    "Equipment visual controller must not always log equipped profile application.");
                AppendIfMissing(
                    failures,
                    visualSource.Contains("RevolverFitProfilePaths"),
                    "Equipment visual controller must reload fit profiles using RevolverFitProfilePaths.");
                AppendIfMissing(
                    failures,
                    !visualSource.Contains("0.11f, -0.04f, 0.05f"),
                    "Equipment visual controller must not hardcode holster seed/default fit values.");
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Revolver ownership and profile-driven visual bridge contracts are valid.");
        }

        public static CCS_SurvivalValidationResult ValidateWeaponAimResolverFoundation()
        {
            List<string> failures = new List<string>();
            string resolverPath = CCS_WeaponsConstants.ModuleRootPath + "/Runtime/Aiming/CCS_WeaponAimResolver.cs";
            string solutionPath = CCS_WeaponsConstants.ModuleRootPath + "/Runtime/Aiming/CCS_WeaponAimSolution.cs";
            string muzzleUtilityPath = CCS_WeaponsConstants.ModuleRootPath + "/Runtime/Aiming/CCS_WeaponMuzzlePointUtility.cs";

            AppendIfMissing(failures, File.Exists(resolverPath), "Missing CCS_WeaponAimResolver.");
            AppendIfMissing(failures, File.Exists(solutionPath), "Missing CCS_WeaponAimSolution.");
            AppendIfMissing(failures, File.Exists(muzzleUtilityPath), "Missing CCS_WeaponMuzzlePointUtility.");

            string controllerPath = CCS_WeaponsConstants.ModuleRootPath + "/Runtime/Components/CCS_RevolverController.cs";
            if (File.Exists(controllerPath))
            {
                string controllerSource = File.ReadAllText(controllerPath);
                AppendIfMissing(
                    failures,
                    controllerSource.Contains("CCS_WeaponAimResolver"),
                    "CCS_RevolverController must use CCS_WeaponAimResolver when firing.");
                AppendIfMissing(
                    failures,
                    controllerSource.Contains("GetReticleViewportPoint"),
                    "CCS_RevolverController must use HUD reticle viewport point for aim resolution.");
                AppendIfMissing(
                    failures,
                    controllerSource.Contains("CurrentEquippedMuzzlePoint"),
                    "CCS_RevolverController must use equipped visual muzzle when available.");
                AppendIfMissing(
                    failures,
                    controllerSource.Contains("debugAimAlignment"),
                    "CCS_RevolverController must expose debug aim alignment toggle.");
            }

            string hudPath = CCS_WeaponsConstants.ModuleRootPath + "/Runtime/Components/CCS_RevolverHudPresenter.cs";
            if (File.Exists(hudPath))
            {
                string hudSource = File.ReadAllText(hudPath);
                AppendIfMissing(
                    failures,
                    hudSource.Contains("GetReticleViewportPoint"),
                    "CCS_RevolverHudPresenter must expose GetReticleViewportPoint.");
            }

            string visualPath = CCS_WeaponsConstants.ModuleRootPath
                + "/Runtime/Components/CCS_PlayerEquipmentVisualController.cs";
            if (File.Exists(visualPath))
            {
                string visualSource = File.ReadAllText(visualPath);
                AppendIfMissing(
                    failures,
                    visualSource.Contains("CurrentEquippedMuzzlePoint"),
                    "Equipment visual controller must expose CurrentEquippedMuzzlePoint.");
                AppendIfMissing(
                    failures,
                    visualSource.Contains("HasEquippedMuzzlePoint"),
                    "Equipment visual controller must expose HasEquippedMuzzlePoint.");
                AppendIfMissing(
                    failures,
                    !visualSource.Contains("SetMuzzlePoint"),
                    "Equipment visual controller must not rebind gameplay muzzle.");
            }

            string feedbackPath = CCS_WeaponsConstants.ModuleRootPath + "/Runtime/Components/CCS_RevolverFireFeedback.cs";
            if (File.Exists(feedbackPath))
            {
                string feedbackSource = File.ReadAllText(feedbackPath);
                AppendIfMissing(
                    failures,
                    feedbackSource.Contains("HitscanResult.RayOrigin"),
                    "CCS_RevolverFireFeedback must start tracers from resolved hitscan muzzle origin.");
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Weapon aim resolver foundation validated.");
        }

        public static CCS_SurvivalValidationResult ValidateWeaponAimConvergenceFoundation()
        {
            List<string> failures = new List<string>();
            string convergencePath = CCS_WeaponsConstants.ModuleRootPath
                + "/Runtime/Aiming/CCS_RevolverVisualAimConvergence.cs";
            string settingsPath = CCS_WeaponsConstants.ModuleRootPath
                + "/Runtime/Aiming/CCS_RevolverVisualAimConvergenceSettings.cs";

            AppendIfMissing(failures, File.Exists(convergencePath), "Missing CCS_RevolverVisualAimConvergence.");
            AppendIfMissing(failures, File.Exists(settingsPath), "Missing CCS_RevolverVisualAimConvergenceSettings.");

            string visualPath = CCS_WeaponsConstants.ModuleRootPath
                + "/Runtime/Components/CCS_PlayerEquipmentVisualController.cs";
            if (File.Exists(visualPath))
            {
                string visualSource = File.ReadAllText(visualPath);
                AppendIfMissing(
                    failures,
                    visualSource.Contains("RuntimeEquippedAimConvergenceRootObjectName"),
                    "Equipment visual controller must create AimConvergenceRoot for equipped revolver.");
                AppendIfMissing(
                    failures,
                    visualSource.Contains("CurrentAimConvergenceRoot"),
                    "Equipment visual controller must expose CurrentAimConvergenceRoot.");
                AppendIfMissing(
                    failures,
                    visualSource.Contains("TickEquippedVisualAimConvergence"),
                    "Equipment visual controller must tick visual aim convergence.");
                AppendIfMissing(
                    failures,
                    visualSource.Contains("ZeroLocalTransform"),
                    "Equipped visual must remain zeroed under AimConvergenceRoot.");
                AppendIfMissing(
                    failures,
                    !visualSource.Contains("rightHandEquippedFitProfile.SocketLocal"),
                    "Equipment visual controller must not mutate fit profile assets at runtime.");
            }

            string controllerPath = CCS_WeaponsConstants.ModuleRootPath + "/Runtime/Components/CCS_RevolverController.cs";
            if (File.Exists(controllerPath))
            {
                string controllerSource = File.ReadAllText(controllerPath);
                AppendIfMissing(
                    failures,
                    controllerSource.Contains("TickVisualAimConvergence"),
                    "CCS_RevolverController must tick visual aim convergence while aiming.");
                AppendIfMissing(
                    failures,
                    controllerSource.Contains("enableVisualAimConvergence"),
                    "CCS_RevolverController must expose enableVisualAimConvergence.");
                AppendIfMissing(
                    failures,
                    controllerSource.Contains("debugVisualConvergence"),
                    "CCS_RevolverController must expose debugVisualConvergence.");
                AppendIfMissing(
                    failures,
                    controllerSource.Contains("debugAimCameraAlignment"),
                    "CCS_RevolverController must expose debugAimCameraAlignment.");
                AppendIfMissing(
                    failures,
                    !controllerSource.Contains("enableVisualAimConvergence = true"),
                    "enableVisualAimConvergence must default off in source.");
                AppendIfMissing(
                    failures,
                    !controllerSource.Contains("debugVisualConvergence = true"),
                    "debugVisualConvergence must default off in source.");
                AppendIfMissing(
                    failures,
                    !controllerSource.Contains("debugAimCameraAlignment = true"),
                    "debugAimCameraAlignment must default off in source.");
            }

            string settingsPathContent = File.Exists(settingsPath) ? File.ReadAllText(settingsPath) : string.Empty;
            AppendIfMissing(
                failures,
                settingsPathContent.Contains("false,") || settingsPathContent.Contains("false\n"),
                "CCS_RevolverVisualAimConvergenceSettings.Default must disable convergence.");

            string convergenceSource = File.Exists(convergencePath) ? File.ReadAllText(convergencePath) : string.Empty;
            AppendIfMissing(
                failures,
                convergenceSource.Contains("CCS_WeaponAimResolver"),
                "Visual aim convergence must use CCS_WeaponAimResolver for aim solution.");
            AppendIfMissing(
                failures,
                convergenceSource.Contains("ResetConvergenceRotation"),
                "Visual aim convergence must reset when holstering.");

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Weapon aim convergence foundation validated.");
        }

        public static CCS_SurvivalValidationResult ValidateRevolverArmReticleIKFoundation(GameObject prefabRoot)
        {
            List<string> failures = new List<string>();
            string armIkPath = CCS_WeaponsConstants.ModuleRootPath
                + "/Runtime/Aiming/CCS_RevolverArmReticleIK.cs";
            AppendIfMissing(failures, File.Exists(armIkPath), "Missing CCS_RevolverArmReticleIK runtime source.");
            AppendIfMissing(
                failures,
                !File.Exists(CCS_WeaponsConstants.ModuleRootPath + "/Runtime/Aiming/CCS_RevolverReticleAimRig.cs"),
                "v0.6.13 CCS_RevolverReticleAimRig must be removed.");
            AppendIfMissing(
                failures,
                !File.Exists(CCS_WeaponsConstants.ModuleRootPath + "/Runtime/Aiming/CCS_RevolverArmAimBias.cs"),
                "LateUpdate CCS_RevolverArmAimBias must be removed in favor of Animation Rigging arm IK.");
            AppendIfMissing(
                failures,
                !File.Exists(
                    "Assets/CCS/Modules/CharacterController/Editor/Equipment/CCS_RevolverReticleAimRigBuilder.cs"),
                "v0.6.13 CCS_RevolverReticleAimRigBuilder must be removed.");
            AppendIfMissing(
                failures,
                !File.Exists(
                    "Assets/CCS/Modules/CharacterController/Editor/Equipment/CCS_RevolverArmAimBiasBuilder.cs"),
                "CCS_RevolverArmAimBiasBuilder must be removed in favor of CCS_RevolverArmReticleIKBuilder.");

            if (File.Exists(armIkPath))
            {
                string armIkSource = File.ReadAllText(armIkPath);
                AppendIfMissing(
                    failures,
                    armIkSource.Contains("ShouldDriveArmReticleIk"),
                    "CCS_RevolverArmReticleIK must gate on revolver owned + RMB aim held.");
                AppendIfMissing(
                    failures,
                    armIkSource.Contains("RightHandReticleIKTarget"),
                    "CCS_RevolverArmReticleIK must derive hand IK target from animated hand with clamped correction.");
                AppendIfMissing(
                    failures,
                    armIkSource.Contains("twoBoneIkRotationWeight = 0f"),
                    "CCS_RevolverArmReticleIK must keep TwoBoneIK rotation weight at zero by default.");
                AppendIfMissing(
                    failures,
                    armIkSource.Contains("UnityEngine.Animations.Rigging"),
                    "CCS_RevolverArmReticleIK must use Unity Animation Rigging.");
                AppendIfMissing(
                    failures,
                    !armIkSource.Contains("equippedAttachmentRoot.localPosition ="),
                    "CCS_RevolverArmReticleIK must not mutate equipped attachment local transform.");
                AppendIfMissing(
                    failures,
                    armIkSource.Contains("enableArmToReticleIK"),
                    "CCS_RevolverArmReticleIK must expose enableArmToReticleIK toggle.");
                AppendIfMissing(
                    failures,
                    !armIkSource.Contains("enableArmToReticleIK = true"),
                    "enableArmToReticleIK must default off in source.");
                AppendIfMissing(
                    failures,
                    armIkSource.Contains("SetArmToReticleIkEnabled"),
                    "CCS_RevolverArmReticleIK must expose SetArmToReticleIkEnabled.");
                AppendIfMissing(
                    failures,
                    armIkSource.Contains("rigBlendSpeed"),
                    "CCS_RevolverArmReticleIK must blend rig weight while RMB aim is held.");
            }

            string muzzleReticlePath = CCS_WeaponsConstants.ModuleRootPath
                + "/Runtime/Aiming/CCS_MuzzleDrivenReticleController.cs";
            string reticleModePath = CCS_WeaponsConstants.ModuleRootPath
                + "/Runtime/Aiming/CCS_AimReticleMode.cs";
            AppendIfMissing(failures, File.Exists(muzzleReticlePath), "Missing CCS_MuzzleDrivenReticleController.");
            AppendIfMissing(failures, File.Exists(reticleModePath), "Missing CCS_AimReticleMode.");
            if (File.Exists(muzzleReticlePath))
            {
                string muzzleReticleSource = File.ReadAllText(muzzleReticlePath);
                AppendIfMissing(
                    failures,
                    muzzleReticleSource.Contains("HybridCameraCenterWithMuzzleDrift"),
                    "Reticle stabilizer must support hybrid camera center with muzzle drift mode.");
                AppendIfMissing(
                    failures,
                    muzzleReticleSource.Contains("ScreenPointToLocalPointInRectangle"),
                    "Reticle stabilizer must convert screen points to canvas local coordinates.");
                AppendIfMissing(
                    failures,
                    muzzleReticleSource.Contains("HandleAimStarted"),
                    "Reticle stabilizer must reset to center when aim starts.");
                AppendIfMissing(
                    failures,
                    muzzleReticleSource.Contains("ClampMagnitude"),
                    "Reticle stabilizer must clamp muzzle drift offset.");
                AppendIfMissing(
                    failures,
                    muzzleReticleSource.Contains("hasValidReticlePosition"),
                    "Reticle stabilizer must guard against stale/off-screen teleport positions.");
            }

            string bodyAimPath = CCS_WeaponsConstants.ModuleRootPath
                + "/Runtime/Aiming/CCS_RevolverBodyAimFollowController.cs";
            AppendIfMissing(failures, File.Exists(bodyAimPath), "Missing CCS_RevolverBodyAimFollowController.");

            string resolverPath = CCS_WeaponsConstants.ModuleRootPath
                + "/Runtime/Aiming/CCS_WeaponAimResolver.cs";
            if (File.Exists(resolverPath))
            {
                string resolverSource = File.ReadAllText(resolverPath);
                AppendIfMissing(
                    failures,
                    resolverSource.Contains("muzzleToAimDirection"),
                    "CCS_WeaponAimResolver must compute muzzle-to-aim direction for fire alignment.");
                AppendIfMissing(
                    failures,
                    resolverSource.Contains("ResolveMuzzleAuthoritativeHitscan"),
                    "CCS_WeaponAimResolver must support muzzle-authoritative hitscan.");
                AppendIfMissing(
                    failures,
                    resolverSource.Contains("ResolveMuzzleForward"),
                    "CCS_WeaponAimResolver must support muzzle-forward aim solutions.");
            }

            string controllerPath = CCS_WeaponsConstants.ModuleRootPath
                + "/Runtime/Components/CCS_RevolverController.cs";
            if (File.Exists(controllerPath))
            {
                string controllerSource = File.ReadAllText(controllerPath);
                AppendIfMissing(
                    failures,
                    controllerSource.Contains("CCS_WeaponShotResolver.ResolveShot"),
                    "CCS_RevolverController fire path must use CCS_WeaponShotResolver.");
                AppendIfMissing(
                    failures,
                    controllerSource.Contains("enableMuzzleAuthoritativeShots"),
                    "CCS_RevolverController must expose enableMuzzleAuthoritativeShots debug toggle.");
                AppendIfMissing(
                    failures,
                    controllerSource.Contains("ConfigureAimVisualTestSettings"),
                    "CCS_RevolverController must expose ConfigureAimVisualTestSettings for Master Test toggles.");
            }

            string weaponsAsmdefPath = CCS_WeaponsConstants.ModuleRootPath + "/Runtime/CCS.Modules.Weapons.Runtime.asmdef";
            if (File.Exists(weaponsAsmdefPath))
            {
                string asmdefText = File.ReadAllText(weaponsAsmdefPath);
                AppendIfMissing(
                    failures,
                    asmdefText.Contains("Unity.Animation.Rigging"),
                    "Weapons.Runtime must reference Unity.Animation.Rigging for arm reticle IK.");
            }

            AppendIfMissing(failures, prefabRoot != null, "Canonical test player prefab is missing for arm reticle IK validation.");
            if (prefabRoot == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            CCS_RevolverArmReticleIK armReticleIk = prefabRoot.GetComponentInChildren<CCS_RevolverArmReticleIK>(true);
            AppendIfMissing(
                failures,
                armReticleIk != null,
                "Test player must contain CCS_RevolverArmReticleIK on VisualRoot.");
            if (armReticleIk != null)
            {
                AppendIfMissing(
                    failures,
                    !armReticleIk.EnableArmToReticleIk,
                    "CCS_RevolverArmReticleIK.enableArmToReticleIK must default off on test player prefab.");
            }

            CCS_MuzzleDrivenReticleController muzzleReticle =
                prefabRoot.GetComponentInChildren<CCS_MuzzleDrivenReticleController>(true);
            AppendIfMissing(
                failures,
                muzzleReticle != null,
                "Test player must contain CCS_MuzzleDrivenReticleController on WeaponHudRoot.");
            if (muzzleReticle != null)
            {
                AppendIfMissing(
                    failures,
                    muzzleReticle.ReticleMode == CCS_AimReticleMode.HybridCameraCenterWithMuzzleDrift,
                    "CCS_MuzzleDrivenReticleController must default to HybridCameraCenterWithMuzzleDrift on test player prefab.");
                AppendIfMissing(
                    failures,
                    muzzleReticle.EnableReticleClamp,
                    "CCS_MuzzleDrivenReticleController.enableReticleClamp must default on for test player prefab.");
                AppendIfMissing(
                    failures,
                    Mathf.Approximately(
                        muzzleReticle.MaxMuzzleReticleOffsetPixels,
                        CCS_WeaponsConstants.MasterTestMaxReticleDriftPixelsDefault),
                    "CCS_MuzzleDrivenReticleController max drift must default to "
                    + CCS_WeaponsConstants.MasterTestMaxReticleDriftPixelsDefault.ToString("0.##")
                    + " pixels.");
            }

            CCS_RevolverBodyAimFollowController bodyAimFollow =
                prefabRoot.GetComponentInChildren<CCS_RevolverBodyAimFollowController>(true);
            AppendIfMissing(
                failures,
                bodyAimFollow != null,
                "Test player must contain CCS_RevolverBodyAimFollowController on VisualRoot.");
            if (bodyAimFollow != null)
            {
                AppendIfMissing(
                    failures,
                    bodyAimFollow.EnableBodyAimFollow,
                    "CCS_RevolverBodyAimFollowController must default enabled on test player prefab.");
            }

            CCS_FirstPersonAimCameraOverrideController fovOverride =
                prefabRoot.GetComponent<CCS_FirstPersonAimCameraOverrideController>();
            AppendIfMissing(
                failures,
                fovOverride == null,
                "CCS_FirstPersonAimCameraOverrideController must not be active on test player prefab.");

            AppendIfMissing(
                failures,
                !PrefabContainsComponentTypeName(prefabRoot, "CCS_RevolverUpperBodyAnimator"),
                "Test player must not contain CCS_RevolverUpperBodyAnimator after Phase 3B locomotion-only reset.");

            string muzzleReticleSourcePath = CCS_WeaponsConstants.ModuleRootPath
                + "/Runtime/Aiming/CCS_MuzzleDrivenReticleController.cs";
            if (File.Exists(muzzleReticleSourcePath))
            {
                string muzzleReticleSource = File.ReadAllText(muzzleReticleSourcePath);
                AppendIfMissing(
                    failures,
                    muzzleReticleSource.Contains("revolverController.IsAiming")
                        || muzzleReticleSource.Contains("IsAiming"),
                    "CCS_MuzzleDrivenReticleController must gate reticle on gameplay aim state.");
                AppendIfMissing(
                    failures,
                    muzzleReticleSource.Contains("IsAimPresentationReadyForReticle"),
                    "CCS_MuzzleDrivenReticleController must gate reticle on aim presentation readiness.");
                AppendIfMissing(
                    failures,
                    muzzleReticleSource.Contains("EnsureReticleHiddenAtStartup"),
                    "CCS_MuzzleDrivenReticleController must hide reticle at startup.");
                AppendIfMissing(
                    failures,
                    muzzleReticleSource.Contains("ForceRevolverHandSocketPreview"),
                    "CCS_MuzzleDrivenReticleController must block reticle during hand socket preview.");
            }

            string hudSourcePath = CCS_WeaponsConstants.ModuleRootPath
                + "/Runtime/Components/CCS_RevolverHudPresenter.cs";
            if (File.Exists(hudSourcePath))
            {
                string hudSource = File.ReadAllText(hudSourcePath);
                AppendIfMissing(
                    failures,
                    hudSource.Contains("WeaponReticleFillColor"),
                    "CCS_RevolverHudPresenter must use red WeaponReticleFillColor.");
                AppendIfMissing(
                    failures,
                    !hudSource.Contains("showReticleWhileAiming && stateChangedEvent.IsAiming"),
                    "CCS_RevolverHudPresenter must not show reticle during IdleToAim.");
            }

            CCS_RevolverController revolverController = prefabRoot.GetComponent<CCS_RevolverController>();
            if (revolverController != null)
            {
                AppendIfMissing(
                    failures,
                    !revolverController.EnableMuzzleAuthoritativeShots,
                    "CCS_RevolverController.enableMuzzleAuthoritativeShots must default off; camera-center aim is the default shot authority.");
            }

            MonoBehaviour[] behaviours = prefabRoot.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                string typeName = behaviour.GetType().Name;
                if (typeName == "CCS_RevolverReticleAimRig" || typeName == "CCS_RevolverArmAimBias")
                {
                    failures.Add("Test player must not contain legacy direct weapon/hand aim component: " + typeName + ".");
                }
            }

            Transform visualRoot = FindDeepChild(prefabRoot.transform, CCS_EquipmentConstants.VisualRootObjectName);
            AppendIfMissing(failures, visualRoot != null, "Test player must contain VisualRoot.");
            if (visualRoot != null)
            {
                Transform ikRoot = visualRoot.Find(CCS_WeaponsConstants.RevolverArmReticleIkRootObjectName);
                AppendIfMissing(
                    failures,
                    ikRoot != null,
                    "VisualRoot must contain " + CCS_WeaponsConstants.RevolverArmReticleIkRootObjectName + ".");
                if (ikRoot != null)
                {
                    AppendIfMissing(
                        failures,
                        ikRoot.Find(CCS_WeaponsConstants.ReticleAimWorldTargetObjectName) != null,
                        "Arm reticle IK root must contain ReticleAimWorldTarget.");
                    AppendIfMissing(
                        failures,
                        ikRoot.Find(CCS_WeaponsConstants.RightHandReticleIkTargetObjectName) != null,
                        "Arm reticle IK root must contain RightHandReticleIKTarget.");
                    AppendIfMissing(
                        failures,
                        ikRoot.Find(CCS_WeaponsConstants.RightElbowHintObjectName) != null,
                        "Arm reticle IK root must contain RightElbowHint.");
                }

                AppendIfMissing(
                    failures,
                    visualRoot.Find(CCS_WeaponsConstants.RevolverAimRigRootObjectName) == null,
                    "VisualRoot must not contain legacy " + CCS_WeaponsConstants.RevolverAimRigRootObjectName + ".");
            }

            Animator animator = visualRoot != null
                ? visualRoot.GetComponentInChildren<Animator>(true)
                : null;
            Transform armIkRigTransform = animator != null
                ? animator.transform.Find(CCS_WeaponsConstants.RevolverArmReticleIkRigObjectName)
                : null;
            AppendIfMissing(
                failures,
                armIkRigTransform != null,
                "Animator must contain " + CCS_WeaponsConstants.RevolverArmReticleIkRigObjectName + ".");
            if (armIkRigTransform != null)
            {
                AppendIfMissing(
                    failures,
                    armIkRigTransform.Find(CCS_WeaponsConstants.RightArmTwoBoneIkConstraintObjectName) != null,
                    "Rig_RevolverArmReticleIK must contain RightArmTwoBoneIK.");
                AppendIfMissing(
                    failures,
                    armIkRigTransform.Find(CCS_WeaponsConstants.ChestAimBiasConstraintObjectName) != null,
                    "Rig_RevolverArmReticleIK must contain ChestAimBias.");
                AppendIfMissing(
                    failures,
                    armIkRigTransform.Find(CCS_WeaponsConstants.RightShoulderAimBiasConstraintObjectName) != null,
                    "Rig_RevolverArmReticleIK must contain RightShoulderAimBias.");
                AppendIfMissing(
                    failures,
                    armIkRigTransform.Find(CCS_WeaponsConstants.RevolverRightHandAimConstraintObjectName) == null,
                    "RightHandAimConstraint direct reticle aim must not remain active.");
            }

            Transform legacyRigTransform = animator != null
                ? animator.transform.Find(CCS_WeaponsConstants.RevolverAimRigObjectName)
                : null;
            AppendIfMissing(
                failures,
                legacyRigTransform == null,
                "Animator must not contain legacy " + CCS_WeaponsConstants.RevolverAimRigObjectName + ".");

            CCS_PlayerEquipmentVisualController visualController =
                prefabRoot.GetComponent<CCS_PlayerEquipmentVisualController>();
            AppendIfMissing(
                failures,
                visualController != null,
                "Test player must contain CCS_PlayerEquipmentVisualController for fit profile validation.");
            if (visualController != null && visualController.RightHandEquippedFitProfile != null)
            {
                AppendIfMissing(
                    failures,
                    visualController.RightHandEquippedFitProfile.name
                        == "CCS_RevolverM1879_RightHandEquipped_Fit",
                    "Equipment visual controller must reference CCS_RevolverM1879_RightHandEquipped_Fit.asset.");
            }

            Transform fallbackMuzzle = FindDeepChild(prefabRoot.transform, CCS_WeaponsConstants.MuzzlePointObjectName);
            AppendIfMissing(
                failures,
                fallbackMuzzle != null,
                "Test player must contain fallback MuzzlePoint for fire/tracer alignment.");

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass(
                    "Revolver arm reticle IK foundation validated. TwoBoneIK rotation weight=0, fit profile unchanged.");
        }

        public static CCS_SurvivalValidationResult ValidateRevolverUpperBodyAimLayerContract()
        {
            List<string> failures = new List<string>();
            const string removedUpperBodyAnimatorPath =
                "Assets/CCS/Modules/CharacterController/Runtime/Animation/CCS_RevolverUpperBodyAnimator.cs";
            const string playerControllerPath =
                "Assets/CCS/Modules/CharacterController/Characters/Player/Animations/Controllers/AC_CCS_Player_Locomotion_StarterAssets.controller";
            const string aiAnimatorDriverPath =
                "Assets/CCS/Modules/AI/Runtime/Animation/CCS_AIAnimatorDriver.cs";
            const string aiWeaponControllerPath =
                "Assets/CCS/Modules/AI/Runtime/Combat/CCS_AIWeaponController.cs";

            AppendIfMissing(
                failures,
                !File.Exists(removedUpperBodyAnimatorPath),
                "CCS_RevolverUpperBodyAnimator must be removed for Phase 3B locomotion-only reset.");
            AppendIfMissing(
                failures,
                File.Exists(playerControllerPath),
                "Missing player locomotion Animator Controller for Phase 3B aim layer contract validation.");

            if (File.Exists(playerControllerPath))
            {
                string controllerSource = File.ReadAllText(playerControllerPath);
                AppendIfMissing(
                    failures,
                    !ContainsExactAnimatorLayerName(
                        controllerSource,
                        CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName),
                    "Player Animator Controller must not contain removed RevolverUpperBody layer.");
                AppendIfMissing(
                    failures,
                    ContainsExactAnimatorLayerName(
                        controllerSource,
                        CCS_CharacterControllerConstants.SingleRevolverUpperBodyLayerName),
                    "Player Animator Controller must contain SingleRevolverUpperBody layer.");
            }

            if (File.Exists(aiAnimatorDriverPath))
            {
                string aiDriverSource = File.ReadAllText(aiAnimatorDriverPath);
                AppendIfMissing(
                    failures,
                    !aiDriverSource.Contains("RevolverAimHeld"),
                    "CCS_AIAnimatorDriver must not write RevolverAimHeld to Base Layer locomotion.");
            }

            if (File.Exists(aiWeaponControllerPath))
            {
                string aiWeaponSource = File.ReadAllText(aiWeaponControllerPath);
                AppendIfMissing(
                    failures,
                    !aiWeaponSource.Contains("SetRevolverAimHeldExternal"),
                    "CCS_AIWeaponController must not route aim through removed RevolverUpperBody animator layer.");
                AppendIfMissing(
                    failures,
                    !aiWeaponSource.Contains("CCS_RevolverUpperBodyAnimator"),
                    "CCS_AIWeaponController must not reference CCS_RevolverUpperBodyAnimator.");
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass(
                    "Phase 3B aim layer contract validated: locomotion-only Animator Controller, no upper-body bridge.");
        }

        private static bool PrefabContainsComponentTypeName(GameObject prefabRoot, string typeName)
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

        public static CCS_SurvivalValidationResult ValidateFirstPersonRevolverArmPresentationRemoved(
            GameObject prefabRoot)
        {
            List<string> failures = new List<string>();
            string presentationPath = CCS_WeaponsConstants.ModuleRootPath
                + "/Runtime/Components/CCS_FirstPersonRevolverArmPresentationController.cs";
            string builderPath =
                "Assets/CCS/Modules/CharacterController/Editor/Equipment/CCS_FirstPersonRevolverArmPresentationBuilder.cs";

            AppendIfMissing(
                failures,
                !File.Exists(presentationPath),
                "Failed FP arm presentation experiment source must be removed: "
                + presentationPath);
            AppendIfMissing(
                failures,
                !File.Exists(builderPath),
                "Failed FP arm presentation builder must be removed: " + builderPath);

            string armReticlePath = CCS_WeaponsConstants.ModuleRootPath
                + "/Runtime/Aiming/CCS_RevolverArmReticleIK.cs";
            if (File.Exists(armReticlePath))
            {
                string armReticleSource = File.ReadAllText(armReticlePath);
                AppendIfMissing(
                    failures,
                    !armReticleSource.Contains("ShouldDeferToFirstPersonArmPresentation"),
                    "Revolver arm reticle IK must not defer to removed first-person arm presentation.");
                AppendIfMissing(
                    failures,
                    !armReticleSource.Contains("CCS_FirstPersonRevolverArmPresentationController"),
                    "Revolver arm reticle IK must not reference removed first-person arm presentation controller.");
            }

            if (prefabRoot != null)
            {
                MonoBehaviour[] behaviours = prefabRoot.GetComponentsInChildren<MonoBehaviour>(true);
                for (int i = 0; i < behaviours.Length; i++)
                {
                    MonoBehaviour behaviour = behaviours[i];
                    if (behaviour != null
                        && behaviour.GetType().Name == "CCS_FirstPersonRevolverArmPresentationController")
                    {
                        failures.Add(
                            "Test player must not contain active CCS_FirstPersonRevolverArmPresentationController.");
                        break;
                    }
                }

                Transform aimAnchor = FindDeepChild(
                    prefabRoot.transform,
                    CCS_CharacterControllerConstants.FirstPersonAimCameraAnchorObjectName);
                if (aimAnchor != null)
                {
                    AppendIfMissing(
                        failures,
                        aimAnchor.Find(CCS_WeaponsConstants.FirstPersonRevolverRightHandTargetObjectName) == null,
                        "FirstPersonAimCameraAnchor must not contain experimental CCS_FP_Revolver_RightHandTarget.");
                    AppendIfMissing(
                        failures,
                        aimAnchor.Find(CCS_WeaponsConstants.FirstPersonRevolverRightElbowHintObjectName) == null,
                        "FirstPersonAimCameraAnchor must not contain experimental CCS_FP_Revolver_RightElbowHint.");
                }
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass(
                    "Failed first-person revolver arm presentation experiment removed from runtime and test player.");
        }

        public static CCS_SurvivalValidationResult ValidateRuntimeRevolverVisualBehaviorFoundation(
            GameObject prefabRoot)
        {
            List<string> failures = new List<string>();
            string visualControllerPath = CCS_WeaponsConstants.ModuleRootPath
                + "/Runtime/Components/CCS_PlayerEquipmentVisualController.cs";
            AppendIfMissing(
                failures,
                File.Exists(visualControllerPath),
                "Missing CCS_PlayerEquipmentVisualController source.");

            if (File.Exists(visualControllerPath))
            {
                string visualSource = File.ReadAllText(visualControllerPath);
                AppendIfMissing(
                    failures,
                    visualSource.Contains("equippedAttachmentRoot")
                        && visualSource.Contains("EnsureVisualInstance")
                        && visualSource.Contains("equippedAttachmentRoot,"),
                    "Equipped visual must spawn under runtime equipped attachment root.");
                AppendIfMissing(
                    failures,
                    visualSource.Contains("SetConvergenceActive(false)"),
                    "Equipped visual aim convergence must remain disabled by default.");
                AppendIfMissing(
                    failures,
                    !visualSource.Contains("DestroyLegacyDirectEquippedVisual"),
                    "Equipped visual must not destroy attachment-root visual after spawning it.");
                AppendIfMissing(
                    failures,
                    visualSource.Contains("CleanupLegacyEquippedVisualLayouts"),
                    "Equipped visual must migrate legacy convergence-child visuals without deleting attachment-root visual.");
            }

            if (prefabRoot != null)
            {
                CCS_PlayerEquipmentVisualController visualController =
                    prefabRoot.GetComponentInChildren<CCS_PlayerEquipmentVisualController>(true);
                AppendIfMissing(
                    failures,
                    visualController != null,
                    "Test player must contain CCS_PlayerEquipmentVisualController.");
                if (visualController != null)
                {
                    AppendIfMissing(
                        failures,
                        visualController.RightHandEquippedFitProfile != null
                            && visualController.RightHandEquippedFitProfile.name
                            == "CCS_RevolverM1879_RightHandEquipped_Fit",
                        "Equipment visual controller must reference CCS_RevolverM1879_RightHandEquipped_Fit.asset.");
                }
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass(
                    "Runtime revolver equipped visual hierarchy restored on attachment root with convergence disabled.");
        }

        public static CCS_SurvivalValidationResult ValidateRevolverProductionAimClipFoundation()
        {
            List<string> failures = new List<string>();
            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.WildWestFulldrawIdleClipPath),
                "Missing Wild West aim hold clip at "
                + CCS_CharacterControllerConstants.WildWestFulldrawIdleClipPath);
            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.WildWestIdleFulldrawRevolverClipPath),
                "Missing Wild West draw clip at "
                + CCS_CharacterControllerConstants.WildWestIdleFulldrawRevolverClipPath);
            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.WildWestIdleFullHolsterRevolverClipPath),
                "Missing Wild West holster clip at "
                + CCS_CharacterControllerConstants.WildWestIdleFullHolsterRevolverClipPath);
            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.RevolverAimRightArmMaskPath)
                    || File.Exists(CCS_CharacterControllerConstants.RevolverAimRightArmMaskLegacyPath),
                "Missing revolver upper-body/right-arm aim Avatar Mask asset.");

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass(
                    "Production revolver aim clips and upper-body aim mask foundation validated.");
        }

        #endregion

        #region Private Methods

        private static void ValidatePrefabUsesCcsAssetsOnly(List<string> failures, string prefabPath)
        {
            if (!File.Exists(prefabPath))
            {
                return;
            }

            string prefabText = File.ReadAllText(prefabPath);
            if (prefabText.Contains(CCS_WeaponsConstants.LegacyReichsrevolverSourceRootPath))
            {
                failures.Add(prefabPath + " must not reference " + CCS_WeaponsConstants.LegacyReichsrevolverSourceRootPath + ".");
            }

            if (prefabText.Contains(CCS_WeaponsConstants.VendorSourceReichsrevolverRootPath))
            {
                failures.Add(prefabPath + " must not reference " + CCS_WeaponsConstants.VendorSourceReichsrevolverRootPath + ".");
            }

            if (prefabText.Contains("Reichsrevolver_M1879/Scripts"))
            {
                failures.Add(prefabPath + " must not reference imported Reichsrevolver vendor scripts.");
            }

            if (prefabText.Contains(CCS_WeaponsConstants.VendorSourceReichsrevolverPrefabGuid))
            {
                failures.Add(prefabPath + " must not contain nested vendor Reichsrevolver prefab instances.");
            }

            if (prefabText.Contains("m_Controller:") || prefabText.Contains("RevolverController"))
            {
                failures.Add(prefabPath + " must not reference imported revolver Animator Controller or RevolverController script.");
            }

            if (prefabText.Contains("UnityEngine.Animation"))
            {
                failures.Add(prefabPath + " must not contain imported Animation components.");
            }
        }

        private static void ValidateWorldPickupPrefabContent(List<string> failures, string prefabPath)
        {
            if (!File.Exists(prefabPath))
            {
                return;
            }

            string prefabText = File.ReadAllText(prefabPath);
            AppendIfMissing(
                failures,
                prefabText.Contains("m_Name: " + CCS_WeaponsConstants.RevolverModelRootObjectName),
                prefabPath + " must contain ModelRoot.");
            AppendIfMissing(
                failures,
                prefabText.Contains("m_Name: " + CCS_WeaponsConstants.RevolverMaterializedVisualChildName),
                prefabPath + " must contain ModelRoot/RevolverVisual.");
            AppendIfMissing(
                failures,
                prefabText.Contains("SkinnedMeshRenderer:") || prefabText.Contains("m_Name: Revolver_Mesh"),
                prefabPath + " must contain the materialized revolver mesh hierarchy under ModelRoot/RevolverVisual.");
            ValidateWorldPickupHasNoDuplicateVisualRoots(failures, prefabPath, prefabText);
            ValidatePrefabUsesCcsMaterialGuids(failures, prefabPath, prefabText);
            AppendIfMissing(
                failures,
                prefabText.Contains("CCS_WeaponPickupInteractable"),
                prefabPath + " must contain CCS_WeaponPickupInteractable.");
            AppendIfMissing(
                failures,
                prefabText.Contains("BoxCollider:"),
                prefabPath + " must contain an interaction collider.");
        }

        private static void ValidateWorldPickupHasNoDuplicateVisualRoots(
            List<string> failures,
            string prefabPath,
            string prefabText)
        {
            AppendIfMissing(
                failures,
                !ContainsTopLevelPrefabChildName(prefabText, "RevolverMesh"),
                prefabPath + " must not contain top-level RevolverMesh.");
            AppendIfMissing(
                failures,
                !ContainsTopLevelPrefabChildName(prefabText, "Body"),
                prefabPath + " must not contain top-level Body outside ModelRoot.");
            AppendIfMissing(
                failures,
                !ContainsTopLevelPrefabChildName(prefabText, "Revolver_Mesh"),
                prefabPath + " must not contain top-level Revolver_Mesh outside ModelRoot.");
        }

        private static bool ContainsTopLevelPrefabChildName(string prefabText, string childName)
        {
            int rootIndex = prefabText.IndexOf("m_Name: PF_CCS_RevolverM1879_WorldPickup");
            if (rootIndex < 0)
            {
                return prefabText.Contains("m_Name: " + childName);
            }

            int childIndex = prefabText.IndexOf("m_Name: " + childName, rootIndex);
            if (childIndex < 0)
            {
                return false;
            }

            int modelRootIndex = prefabText.IndexOf(
                "m_Name: " + CCS_WeaponsConstants.RevolverModelRootObjectName,
                rootIndex);
            if (modelRootIndex < 0)
            {
                return true;
            }

            int nextRootSibling = prefabText.IndexOf("\n--- !u!", modelRootIndex + 1);
            if (nextRootSibling < 0)
            {
                return childIndex > modelRootIndex;
            }

            return childIndex > rootIndex && childIndex < modelRootIndex;
        }

        private static bool PlayerPrefabContainsComponentName(string prefabPath, string componentTypeName)
        {
            if (!File.Exists(prefabPath))
            {
                return false;
            }

            return File.ReadAllText(prefabPath).Contains(componentTypeName);
        }

        private static void ValidateVisualOnlyPrefabContent(List<string> failures, string prefabPath)
        {
            ValidateRevolverVisualPrefabContent(failures, prefabPath, requireEquippedAnchors: false);
            if (!File.Exists(prefabPath))
            {
                return;
            }

            string prefabText = File.ReadAllText(prefabPath);
            AppendIfMissing(
                failures,
                prefabText.Contains("m_Name: PF_CCS_RevolverM1879_VisualOnly"),
                prefabPath + " must use PF_CCS_RevolverM1879_VisualOnly root name.");
            AppendIfMissing(
                failures,
                !prefabText.Contains("BoxCollider:"),
                prefabPath + " must not contain colliders.");
            AppendIfMissing(
                failures,
                prefabText.Contains("m_Name: " + CCS_WeaponsConstants.MuzzlePointObjectName),
                prefabPath + " must contain MuzzlePoint for equipped visual aim alignment.");
            AppendIfMissing(
                failures,
                prefabText.Contains("m_Name: " + CCS_WeaponsConstants.FitGuidesObjectName),
                prefabPath + " must contain FitGuides with weapon anchor points.");
            AppendIfMissing(
                failures,
                prefabText.Contains("m_Name: " + CCS_WeaponsConstants.CylinderPointObjectName),
                prefabPath + " must contain CylinderPoint fit guide.");
            AppendIfMissing(
                failures,
                prefabText.Contains("m_Name: " + CCS_WeaponsConstants.ShellEjectPointObjectName),
                prefabPath + " must contain ShellEjectPoint fit guide.");
            AppendIfMissing(
                failures,
                !prefabText.Contains("CCS_WeaponPickupInteractable"),
                prefabPath + " must not contain pickup interaction components.");
        }

        public static CCS_SurvivalValidationResult ValidateRevolverFireVisualsFoundation()
        {
            List<string> failures = new List<string>();
            string feedbackPath = CCS_WeaponsConstants.ModuleRootPath + "/Runtime/Components/CCS_RevolverFireFeedback.cs";
            string tracerPath = CCS_WeaponsConstants.ModuleRootPath + "/Runtime/Components/CCS_RevolverBulletTracerVisual.cs";
            string flashPath = CCS_WeaponsConstants.ModuleRootPath + "/Runtime/Components/CCS_RevolverMuzzleFlashVisual.cs";
            string smokePath = CCS_WeaponsConstants.ModuleRootPath + "/Runtime/Components/CCS_RevolverMuzzleSmokeVisual.cs";
            string shellPath = CCS_WeaponsConstants.ModuleRootPath + "/Runtime/Components/CCS_RevolverSpentShellVisual.cs";

            AppendIfMissing(failures, File.Exists(tracerPath), "Missing CCS_RevolverBulletTracerVisual.");
            AppendIfMissing(failures, File.Exists(flashPath), "Missing CCS_RevolverMuzzleFlashVisual.");
            AppendIfMissing(failures, File.Exists(smokePath), "Missing CCS_RevolverMuzzleSmokeVisual.");
            AppendIfMissing(failures, File.Exists(shellPath), "Missing CCS_RevolverSpentShellVisual.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_WeaponsConstants.RevolverM1879BulletTracerVisualPrefabPath),
                "Missing PF_CCS_RevolverM1879_BulletTracerVisual.prefab.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_WeaponsConstants.RevolverM1879MuzzleFlashPrefabPath),
                "Missing PF_CCS_RevolverM1879_MuzzleFlash.prefab.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_WeaponsConstants.RevolverM1879MuzzleSmokePrefabPath),
                "Missing PF_CCS_RevolverM1879_MuzzleSmoke.prefab.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_WeaponsConstants.RevolverM1879SpentShellVisualPrefabPath),
                "Missing PF_CCS_RevolverM1879_SpentShellVisual.prefab.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_WeaponsConstants.RevolverM1879BulletTrailMaterialAssetPath),
                "Missing MAT_CCS_Revolver_BulletTrail.mat.");

            if (File.Exists(feedbackPath))
            {
                string feedbackSource = File.ReadAllText(feedbackPath);
                AppendIfMissing(
                    failures,
                    feedbackSource.Contains("HitscanResult.RayOrigin"),
                    "CCS_RevolverFireFeedback must use HitscanResult.RayOrigin for fire visuals.");
                AppendIfMissing(
                    failures,
                    feedbackSource.Contains("RevolverReloadStarted"),
                    "CCS_RevolverFireFeedback must extract spent shells on reload only.");
                AppendIfMissing(
                    failures,
                    feedbackSource.Contains("debugFireVisuals"),
                    "CCS_RevolverFireFeedback must expose debugFireVisuals.");
                AppendIfMissing(
                    failures,
                    !feedbackSource.Contains("debugFireVisuals = true"),
                    "debugFireVisuals must default off in source.");
                AppendIfMissing(
                    failures,
                    feedbackSource.Contains("HandleReloadStarted")
                        && feedbackSource.Contains("SpawnSpentShell"),
                    "Spent shell visuals must spawn during reload extraction only.");
                AppendIfMissing(
                    failures,
                    feedbackSource.Contains("bulletVisualScaleMultiplier"),
                    "CCS_RevolverFireFeedback must expose bulletVisualScaleMultiplier.");
                AppendIfMissing(
                    failures,
                    feedbackSource.Contains("spentShellVisualScaleMultiplier"),
                    "CCS_RevolverFireFeedback must expose spentShellVisualScaleMultiplier.");
                AppendIfMissing(
                    failures,
                    feedbackSource.Contains("bulletTrailEnabled"),
                    "CCS_RevolverFireFeedback must expose bulletTrailEnabled.");
                AppendIfMissing(
                    failures,
                    feedbackSource.Contains("bulletTrailLifetime"),
                    "CCS_RevolverFireFeedback must expose bulletTrailLifetime.");
                AppendIfMissing(
                    failures,
                    feedbackSource.Contains("bulletTrailWidth"),
                    "CCS_RevolverFireFeedback must expose bulletTrailWidth.");
                AppendIfMissing(
                    failures,
                    feedbackSource.Contains("DefaultBulletVisualScaleMultiplier"),
                    "Bullet visual scale multiplier must use validated default constant.");
                AppendIfMissing(
                    failures,
                    feedbackSource.Contains("MinBulletVisualScaleMultiplier")
                        && feedbackSource.Contains("MaxBulletVisualScaleMultiplier"),
                    "Bullet visual scale multiplier must clamp to safe range.");
                AppendIfMissing(
                    failures,
                    feedbackSource.Contains("MinSpentShellVisualScaleMultiplier")
                        && feedbackSource.Contains("MaxSpentShellVisualScaleMultiplier"),
                    "Spent shell visual scale multiplier must clamp to safe range.");
                AppendIfMissing(
                    failures,
                    feedbackSource.Contains("localScale = Vector3.one * scale"),
                    "Visual scale multipliers must apply to spawned visual instances only.");
                AppendIfMissing(
                    failures,
                    !ContainsPerShotShellEjection(feedbackSource),
                    "Revolver fire feedback must not eject shells per shot.");
            }

            if (File.Exists(tracerPath))
            {
                string tracerSource = File.ReadAllText(tracerPath);
                AppendIfMissing(
                    failures,
                    !tracerSource.Contains("ApplyWeaponDamage") && !tracerSource.Contains("CCS_TestDamageTarget"),
                    "Bullet tracer visual must not apply damage.");
                AppendIfMissing(
                    failures,
                    tracerSource.Contains("TrailRenderer"),
                    "Bullet tracer visual must support a readable TrailRenderer streak.");
                AppendIfMissing(
                    failures,
                    tracerSource.Contains("ConfigureTrail"),
                    "Bullet tracer visual must configure trail lifetime and width.");
                AppendIfMissing(
                    failures,
                    tracerSource.Contains("MinBulletTrailLifetime")
                        && tracerSource.Contains("MaxBulletTrailWidth"),
                    "Bullet trail tuning must clamp to safe cosmetic ranges.");
            }

            string bulletTracerPrefabPath = CCS_WeaponsConstants.RevolverM1879BulletTracerVisualPrefabPath;
            if (File.Exists(bulletTracerPrefabPath))
            {
                string prefabText = File.ReadAllText(bulletTracerPrefabPath);
                AppendIfMissing(
                    failures,
                    prefabText.Contains("TrailRenderer:"),
                    "PF_CCS_RevolverM1879_BulletTracerVisual must include TrailRenderer for readability.");
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Revolver fire visuals foundation validated.");
        }

        private static bool ContainsPerShotShellEjection(string feedbackSource)
        {
            int fireHandlerStart = feedbackSource.IndexOf("HandleFireResolved", System.StringComparison.Ordinal);
            if (fireHandlerStart < 0)
            {
                return false;
            }

            int reloadHandlerStart = feedbackSource.IndexOf("HandleReloadStarted", fireHandlerStart, System.StringComparison.Ordinal);
            if (reloadHandlerStart < 0)
            {
                reloadHandlerStart = feedbackSource.Length;
            }

            string fireHandlerBlock = feedbackSource.Substring(fireHandlerStart, reloadHandlerStart - fireHandlerStart);
            return fireHandlerBlock.Contains("SpawnSpentShell");
        }

        private static void ValidateRevolverVisualPrefabContent(
            List<string> failures,
            string prefabPath,
            bool requireEquippedAnchors)
        {
            if (!File.Exists(prefabPath))
            {
                return;
            }

            string prefabText = File.ReadAllText(prefabPath);
            AppendIfMissing(
                failures,
                prefabText.Contains("m_Name: " + CCS_WeaponsConstants.RevolverModelRootObjectName),
                prefabPath + " must contain ModelRoot with full CCS materialized revolver visual.");
            AppendIfMissing(
                failures,
                prefabText.Contains("SkinnedMeshRenderer:") || prefabText.Contains("m_Name: Revolver_Mesh"),
                prefabPath + " must contain the full materialized revolver mesh hierarchy.");
            ValidatePrefabUsesCcsMaterialGuids(failures, prefabPath, prefabText);
            AppendIfMissing(
                failures,
                !prefabText.Contains("m_Name: RevolverMesh"),
                prefabPath + " must not use legacy RevolverMesh-only visual root.");

            if (requireEquippedAnchors)
            {
                AppendIfMissing(
                    failures,
                    prefabText.Contains("m_Name: " + CCS_WeaponsConstants.MuzzlePointObjectName),
                    prefabPath + " must contain MuzzlePoint.");
                AppendIfMissing(
                    failures,
                    prefabText.Contains("m_Name: " + CCS_WeaponsConstants.ShellEjectPointObjectName),
                    prefabPath + " must contain ShellEjectPoint.");
                AppendIfMissing(
                    failures,
                    prefabText.Contains("m_Name: " + CCS_WeaponsConstants.BulletVisualSpawnPointObjectName),
                    prefabPath + " must contain BulletVisualSpawnPoint.");
                AppendIfMissing(
                    failures,
                    prefabText.Contains("m_Name: " + CCS_WeaponsConstants.CylinderPointObjectName),
                    prefabPath + " must contain CylinderPoint.");
            }

            AppendIfMissing(
                failures,
                !prefabText.Contains("CCS_WeaponPickupInteractable"),
                prefabPath + " must not contain world pickup interaction components.");
        }

        private static void ValidatePrefabUsesCcsMaterialGuids(
            List<string> failures,
            string prefabPath,
            string prefabText)
        {
            bool usesCcsMaterial = false;
            string[] materialPaths =
            {
                CCS_WeaponsConstants.RevolverM1879MaterialAssetPath,
                CCS_WeaponsConstants.RevolverM1879MetalMaterialAssetPath,
                CCS_WeaponsConstants.RevolverM1879WoodGripMaterialAssetPath,
            };

            for (int i = 0; i < materialPaths.Length; i++)
            {
                string metaPath = materialPaths[i] + ".meta";
                if (!File.Exists(metaPath))
                {
                    continue;
                }

                string metaText = File.ReadAllText(metaPath);
                int guidIndex = metaText.IndexOf("guid: ", StringComparison.Ordinal);
                if (guidIndex < 0)
                {
                    continue;
                }

                int guidStart = guidIndex + 6;
                int guidEnd = metaText.IndexOf('\n', guidStart);
                if (guidEnd < 0)
                {
                    guidEnd = metaText.Length;
                }

                string guid = metaText.Substring(guidStart, guidEnd - guidStart).Trim();
                if (!string.IsNullOrEmpty(guid) && prefabText.Contains(guid))
                {
                    usesCcsMaterial = true;
                    break;
                }
            }

            AppendIfMissing(
                failures,
                usesCcsMaterial,
                prefabPath + " must assign CCS revolver materials from Content/RevolverM1879/Materials.");
        }

        private static void ValidateEquippedPrefabHasMuzzlePoint(List<string> failures)
        {
            if (!File.Exists(CCS_WeaponsConstants.RevolverM1879EquippedPrefabPath))
            {
                return;
            }

            string prefabText = File.ReadAllText(CCS_WeaponsConstants.RevolverM1879EquippedPrefabPath);
            AppendIfMissing(
                failures,
                prefabText.Contains("m_Name: " + CCS_WeaponsConstants.MuzzlePointObjectName),
                "Equipped revolver prefab must contain MuzzlePoint.");
            AppendIfMissing(
                failures,
                prefabText.Contains("m_Name: " + CCS_WeaponsConstants.ShellEjectPointObjectName),
                "Equipped revolver prefab must contain ShellEjectPoint.");
        }

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

        private static bool ContainsExactAnimatorLayerName(string source, string layerName)
        {
            return source.Contains("m_Name: " + layerName + "\r\n")
                || source.Contains("m_Name: " + layerName + "\n");
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

        #endregion
    }
}
