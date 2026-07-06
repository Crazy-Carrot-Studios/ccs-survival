using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Local;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PistolAimReferenceValidationUtility
// CATEGORY: Modules / CharacterController / Editor / Shooter
// PURPOSE: Validates v0.7.14 CCS pistol aim reference planning milestone.
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_PistolAimReferenceValidationUtility
    {
        private const string ForbiddenVendorNameToken = "Invector";

        private static readonly string[] RequiredDocumentationRelativePaths =
        {
            CCS_PistolAimReferencePaths.AdaptationPlanRelative,
            CCS_PistolAimReferencePaths.CameraReferenceRelative,
            CCS_PistolAimReferencePaths.AnimationReferenceRelative,
        };

        private static readonly string[] RequiredFolderRelativePaths =
        {
            "Assets/CCS/Modules/CharacterController/Documentation/Shooter",
            "Assets/CCS/Modules/CharacterController/Content/Animations/Pistol",
            "Assets/CCS/Modules/CharacterController/Content/Animations/Pistol/TwoHanded",
            "Assets/CCS/Modules/CharacterController/Content/Animations/Pistol/Masks",
            "Assets/CCS/Modules/CharacterController/Profiles/Shooter",
            "Assets/CCS/Modules/CharacterController/Editor/Shooter",
        };

        private static readonly string[] ForbiddenProductionComponentTypeNames =
        {
            "vShooterManager",
            "vShooterMeleeInput",
            "vThirdPersonCamera",
            "vThirdPersonController",
            "vShooterWeapon",
            "vControlAimCanvas",
            "vWeaponIKAdjust",
            "CCS_DualRevolverAimConvergenceRigPresenter",
            "CCS_DualRevolverArmAimBiasPresenter",
            "CCS_ManualArmRotationBiasPresenter",
        };

        public static CCS_SurvivalValidationResult ValidatePistolAimReferencePlanning()
        {
            List<string> errors = new List<string>();
            List<string> warnings = new List<string>();
            string projectRoot = CCS_PistolAimReferencePaths.GetProjectRoot();

            ValidateRequiredDocumentation(projectRoot, errors);
            ValidateRequiredFolders(projectRoot, errors);
            ValidateNoVendorNamingUnderCcsAssets(errors);
            ValidateNoVendorScriptsInCcs(errors);
            ValidateProductionPrefab(errors, warnings);
            ValidateTargetClipNotCopiedYet(projectRoot, warnings);

            if (errors.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join("\n", errors));
            }

            string message = "Pistol aim reference planning validation passed.";
            if (warnings.Count > 0)
            {
                message += " Warnings:\n" + string.Join("\n", warnings);
            }

            return CCS_SurvivalValidationResult.Pass(message);
        }

        public static bool TryFindExternalReferenceProject(out string projectPath, out string poseFbxPath)
        {
            projectPath = string.Empty;
            poseFbxPath = string.Empty;

            foreach (string candidate in CCS_PistolAimReferencePaths.ExternalReferenceProjectCandidates.Distinct())
            {
                if (!Directory.Exists(candidate))
                {
                    continue;
                }

                string assetsRoot = Path.Combine(candidate, "Assets");
                if (!Directory.Exists(assetsRoot))
                {
                    continue;
                }

                string[] matches = Directory.GetFiles(
                    assetsRoot,
                    "Shooter_UpperBodyPoses.fbx",
                    SearchOption.AllDirectories);

                if (matches.Length == 0)
                {
                    continue;
                }

                projectPath = candidate;
                poseFbxPath = matches[0];
                return true;
            }

            return false;
        }

        private static void ValidateRequiredDocumentation(string projectRoot, List<string> errors)
        {
            for (int i = 0; i < RequiredDocumentationRelativePaths.Length; i++)
            {
                string path = Path.Combine(projectRoot, RequiredDocumentationRelativePaths[i]);
                if (!File.Exists(path))
                {
                    errors.Add("Missing required documentation: " + path);
                }
            }
        }

        private static void ValidateRequiredFolders(string projectRoot, List<string> errors)
        {
            for (int i = 0; i < RequiredFolderRelativePaths.Length; i++)
            {
                string path = Path.Combine(projectRoot, RequiredFolderRelativePaths[i]);
                if (!Directory.Exists(path))
                {
                    errors.Add("Missing required folder: " + path);
                }
            }

            string testControllerPath = Path.Combine(
                projectRoot,
                CCS_CharacterControllerConstants.PistolAimTestControllerPath);
            if (!File.Exists(testControllerPath))
            {
                errors.Add("Missing test controller: " + testControllerPath);
            }
        }

        private static void ValidateNoVendorNamingUnderCcsAssets(List<string> errors)
        {
            const string ccsRoot = "Assets/CCS";
            if (!Directory.Exists(ccsRoot))
            {
                errors.Add("Assets/CCS folder not found.");
                return;
            }

            string[] allPaths = Directory.GetFiles(ccsRoot, "*.*", SearchOption.AllDirectories);
            for (int i = 0; i < allPaths.Length; i++)
            {
                string normalized = allPaths[i].Replace('\\', '/');
                if (normalized.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (ContainsVendorNameToken(Path.GetFileName(normalized))
                    || ContainsVendorNameToken(normalized))
                {
                    errors.Add("Vendor naming forbidden under Assets/CCS: " + normalized);
                }
            }

            string[] textAssetExtensions = { ".md", ".controller", ".asset", ".prefab", ".unity", ".anim" };
            for (int i = 0; i < allPaths.Length; i++)
            {
                string normalized = allPaths[i].Replace('\\', '/');
                string extension = Path.GetExtension(normalized);
                if (string.IsNullOrEmpty(extension))
                {
                    continue;
                }

                bool isTextAsset = false;
                for (int e = 0; e < textAssetExtensions.Length; e++)
                {
                    if (string.Equals(extension, textAssetExtensions[e], StringComparison.OrdinalIgnoreCase))
                    {
                        isTextAsset = true;
                        break;
                    }
                }

                if (!isTextAsset)
                {
                    continue;
                }

                string content = File.ReadAllText(normalized);
                if (ContainsVendorNameToken(content))
                {
                    errors.Add("Vendor naming forbidden in asset content: " + normalized);
                }
            }
        }

        private static void ValidateNoVendorScriptsInCcs(List<string> errors)
        {
            const string ccsRoot = "Assets/CCS";
            string[] csFiles = Directory.GetFiles(ccsRoot, "*.cs", SearchOption.AllDirectories);
            for (int i = 0; i < csFiles.Length; i++)
            {
                string fileName = Path.GetFileName(csFiles[i]);
                if (fileName.StartsWith("vShooter", StringComparison.Ordinal)
                    || fileName.StartsWith("vThirdPerson", StringComparison.Ordinal)
                    || fileName.StartsWith("vWeapon", StringComparison.Ordinal)
                    || fileName.StartsWith("vControlAim", StringComparison.Ordinal))
                {
                    errors.Add("External vendor script copied into Assets/CCS: " + csFiles[i]);
                }
            }

            CCS_SurvivalValidationResult legacyPackageResult =
                CCS_CharacterControllerAnimationValidationUtility.ValidateNoLegacyExternalShooterPackageInProject();
            if (!legacyPackageResult.IsSuccess)
            {
                errors.Add(legacyPackageResult.Message);
            }
        }

        private static void ValidateProductionPrefab(List<string> errors, List<string> warnings)
        {
            string prefabPath = CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath;
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                errors.Add("Production player prefab not found: " + prefabPath);
                return;
            }

            MonoBehaviour[] behaviours = prefab.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    warnings.Add("Production prefab contains missing script reference.");
                    continue;
                }

                string typeName = behaviour.GetType().Name;
                if (ForbiddenProductionComponentTypeNames.Contains(typeName))
                {
                    errors.Add("Production prefab contains forbidden component: " + typeName);
                }

                string namespaceName = behaviour.GetType().Namespace ?? string.Empty;
                if (IsExternalVendorNamespace(namespaceName))
                {
                    errors.Add("Production prefab references external vendor namespace: " + namespaceName);
                }
            }

            Transform[] transforms = prefab.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                string objectName = transforms[i].name;
                if (objectName.Contains("MultiAimConstraint")
                    || objectName.Contains("MultiRotationConstraint")
                    || objectName.Contains("TwoBoneIKConstraint")
                    || objectName.Contains("RigBuilder"))
                {
                    errors.Add("Procedural convergence object on production prefab: " + objectName);
                }
            }
        }

        private static void ValidateTargetClipNotCopiedYet(string projectRoot, List<string> warnings)
        {
            string clipPath = Path.Combine(
                projectRoot,
                CCS_CharacterControllerConstants.PistolTwoHandedAimHoldClipPath);
            if (File.Exists(clipPath))
            {
                warnings.Add("Target aim hold clip already exists; verify license approval before commit: " + clipPath);
                return;
            }

            warnings.Add("Reference two-handed aim clip not duplicated yet (awaiting license confirmation).");
        }

        private static bool ContainsVendorNameToken(string value)
        {
            return value.IndexOf(ForbiddenVendorNameToken, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsExternalVendorNamespace(string namespaceName)
        {
            if (string.IsNullOrEmpty(namespaceName))
            {
                return false;
            }

            return namespaceName.StartsWith(ForbiddenVendorNameToken, StringComparison.OrdinalIgnoreCase);
        }
    }
}
