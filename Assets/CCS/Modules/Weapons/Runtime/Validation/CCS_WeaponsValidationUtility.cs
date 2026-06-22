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
                source.Contains("muzzlePoint.position") || source.Contains("MuzzlePointTransform"),
                "CCS_RevolverFireFeedback must start tracers from the active muzzle transform.");
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
                !File.Exists(CCS_WeaponsConstants.RevolverM1879HolsteredPrefabPath),
                "Holstered revolver prefab must be removed for world-pickup-only scope.");
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
            ValidatePrefabUsesCcsAssetsOnly(failures, CCS_WeaponsConstants.RevolverM1879WorldPickupPrefabPath);
            ValidateWorldPickupPrefabContent(failures, CCS_WeaponsConstants.RevolverM1879WorldPickupPrefabPath);

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Revolver M1879 world pickup assets are present and CCS-owned.");
        }

        public static CCS_SurvivalValidationResult ValidatePlayerWeaponLoadoutComponents(GameObject prefabRoot)
        {
            List<string> failures = new List<string>();
            AppendIfMissing(failures, prefabRoot != null, "Canonical test player prefab is missing.");
            if (prefabRoot == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            AppendIfMissing(
                failures,
                prefabRoot.GetComponent<CCS_PlayerWeaponLoadout>() != null,
                "Test player must contain CCS_PlayerWeaponLoadout.");
            AppendIfMissing(
                failures,
                !PlayerPrefabContainsComponentName(
                    CCS_WeaponsConstants.NetworkedTestPlayerPrefabPath,
                    "CCS_RevolverWeaponVisualFeedback"),
                "Test player must not contain CCS_RevolverWeaponVisualFeedback in world-pickup-only scope.");
            AppendIfMissing(
                failures,
                FindDeepChild(prefabRoot.transform, CCS_WeaponsConstants.RevolverHolsterSocketName) == null,
                "Test player must not contain holster socket "
                + CCS_WeaponsConstants.RevolverHolsterSocketName
                + ".");
            AppendIfMissing(
                failures,
                FindDeepChild(prefabRoot.transform, CCS_WeaponsConstants.RevolverHandSocketName) == null,
                "Test player must not contain hand socket " + CCS_WeaponsConstants.RevolverHandSocketName + ".");
            AppendIfMissing(
                failures,
                FindDeepChild(prefabRoot.transform, "PF_CCS_RevolverM1879_Holstered_Instance") == null,
                "Test player must not contain holstered revolver visual instance.");
            AppendIfMissing(
                failures,
                FindDeepChild(prefabRoot.transform, "PF_CCS_RevolverM1879_Equipped_Instance") == null,
                "Test player must not contain equipped revolver visual instance.");

            CCS_CharacterAimLocomotionController aimLocomotion =
                prefabRoot.GetComponent<CCS_CharacterAimLocomotionController>();
            AppendIfMissing(
                failures,
                aimLocomotion != null,
                "Test player must contain CCS_CharacterAimLocomotionController for weapon aim gate wiring.");

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Player weapon ownership loadout is valid without attached gun visuals.");
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
                    !loadoutSource.Contains("ShowEquippedVisual"),
                    "CCS_PlayerWeaponLoadout must not spawn equipped gun visuals in world-pickup-only scope.");
                AppendIfMissing(
                    failures,
                    !loadoutSource.Contains("ShowHolsteredVisual"),
                    "CCS_PlayerWeaponLoadout must not spawn holstered gun visuals in world-pickup-only scope.");
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Revolver ownership contract is valid for world-pickup-only scope.");
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
