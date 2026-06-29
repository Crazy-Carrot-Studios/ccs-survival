using System.Collections.Generic;
using System.IO;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterControllerPhase3CValidationUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Validates v0.7.4 Phase 3C animation rebuild architecture (planning only).
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Preserves v0.7.3 locomotion-only baseline; confirms architecture contracts exist.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerPhase3CValidationUtility
    {
        private const string ArchitectureDocPath =
            "Assets/CCS/Modules/CharacterController/Documentation/CCS_CharacterController_Animation_Rebuild_Architecture.md";

        private const string ParameterIdsPath =
            "Assets/CCS/Modules/CharacterController/Runtime/Animation/CCS_CharacterAnimationParameterIds.cs";

        private const string WeaponModePath =
            "Assets/CCS/Modules/CharacterController/Runtime/Animation/CCS_CharacterWeaponAnimationMode.cs";

        private const string PresenterInterfacePath =
            "Assets/CCS/Modules/CharacterController/Runtime/Animation/CCS_ICharacterAnimationPresenter.cs";

        private const string LocomotionAnimatorPath =
            "Assets/CCS/Modules/CharacterController/Runtime/Visuals/CCS_PlayerLocomotionAnimator.cs";

        private const string EquipmentFitStudioWindowPath =
            "Assets/CCS/Modules/CharacterController/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioWindow.cs";

        private const string CharacterControllerTestsRoot =
            "Assets/CCS/Modules/CharacterController/Tests";

        private const string AnimationFitStudioRoot =
            "Assets/CCS/Modules/CharacterController/Editor/AnimationFitStudio";

        public static CCS_SurvivalValidationResult ValidatePhase3CAnimationRebuildArchitecture()
        {
            List<string> failures = new List<string>();
            List<string> warnings = new List<string>();

            CCS_SurvivalValidationResult phase3BResult =
                CCS_CharacterControllerPhase3BValidationUtility.ValidatePhase3BLocomotionOnlyAnimatorReset();
            if (!phase3BResult.IsSuccess)
            {
                failures.Add(phase3BResult.Message);
            }
            else if (!string.IsNullOrEmpty(phase3BResult.Message) && phase3BResult.Message.Contains("Warnings:"))
            {
                warnings.Add(phase3BResult.Message);
            }

            ValidateArchitectureArtifactsExist(failures);
            ValidateLocomotionAnimatorUsesParameterIds(failures);
            ValidateTestsFolderRemoved(failures);
            ValidateAnimationFitStudioNotPresent(failures);
            ValidateEquipmentFitStudioPresent(failures);
            CollectArchitecturePlanningWarnings(warnings);

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            string message = "Phase 3C animation rebuild architecture validated. v0.7.3 locomotion-only baseline preserved.";
            if (warnings.Count > 0)
            {
                message += " Warnings: " + string.Join(" ", warnings);
            }

            return CCS_SurvivalValidationResult.Pass(message);
        }

        private static void ValidateArchitectureArtifactsExist(List<string> failures)
        {
            AppendIfMissing(failures, File.Exists(ArchitectureDocPath), "Missing architecture doc at " + ArchitectureDocPath);
            AppendIfMissing(failures, File.Exists(ParameterIdsPath), "Missing parameter IDs at " + ParameterIdsPath);
            AppendIfMissing(failures, File.Exists(WeaponModePath), "Missing weapon animation mode enum at " + WeaponModePath);
            AppendIfMissing(
                failures,
                File.Exists(PresenterInterfacePath),
                "Missing presentation interface at " + PresenterInterfacePath);

            if (!File.Exists(ParameterIdsPath))
            {
                return;
            }

            string parameterSource = File.ReadAllText(ParameterIdsPath);
            AppendIfMissing(
                failures,
                parameterSource.Contains("class Active"),
                "CCS_CharacterAnimationParameterIds must define Active locomotion parameter hashes.");
            AppendIfMissing(
                failures,
                parameterSource.Contains("class FutureDesignOnly"),
                "CCS_CharacterAnimationParameterIds must define FutureDesignOnly planned parameter names.");
            AppendIfMissing(
                failures,
                !parameterSource.Contains("FutureDesignOnly") || !ContainsFutureDesignOnlyRuntimeHash(parameterSource),
                "FutureDesignOnly parameters must not expose runtime Animator hashes in v0.7.4.");
        }

        private static void ValidateLocomotionAnimatorUsesParameterIds(List<string> failures)
        {
            AppendIfMissing(failures, File.Exists(LocomotionAnimatorPath), "Missing locomotion animator at " + LocomotionAnimatorPath);
            if (!File.Exists(LocomotionAnimatorPath))
            {
                return;
            }

            string source = File.ReadAllText(LocomotionAnimatorPath);
            AppendIfMissing(
                failures,
                source.Contains("CCS_CharacterAnimationParameterIds.Active"),
                "CCS_PlayerLocomotionAnimator must use CCS_CharacterAnimationParameterIds.Active hashes.");
            AppendIfMissing(
                failures,
                !source.Contains("Animator.StringToHash"),
                "CCS_PlayerLocomotionAnimator must not define local Animator.StringToHash calls after Phase 3C.");
        }

        private static void ValidateTestsFolderRemoved(List<string> failures)
        {
            AppendIfMissing(
                failures,
                !Directory.Exists(CharacterControllerTestsRoot),
                "CharacterController/Tests folder must remain removed (production architecture).");
        }

        private static void ValidateAnimationFitStudioNotPresent(List<string> failures)
        {
            AppendIfMissing(
                failures,
                !Directory.Exists(AnimationFitStudioRoot),
                "Animation Fit Studio editor folder must not be reintroduced.");
        }

        private static void ValidateEquipmentFitStudioPresent(List<string> failures)
        {
            AppendIfMissing(
                failures,
                File.Exists(EquipmentFitStudioWindowPath),
                "Equipment Fit Studio window must remain at " + EquipmentFitStudioWindowPath);
            AppendIfMissing(
                failures,
                Directory.Exists("Assets/CCS/Modules/CharacterController/Editor/EquipmentFitStudio"),
                "Equipment Fit Studio editor folder must remain.");
        }

        private static void CollectArchitecturePlanningWarnings(List<string> warnings)
        {
            string revolverRoot = CCS_CharacterControllerConstants.RevolverAimAnimationsPath;
            if (Directory.Exists(revolverRoot))
            {
                int clipCount = Directory.GetFiles(revolverRoot, "*.anim", SearchOption.AllDirectories).Length;
                if (clipCount > 0)
                {
                    warnings.Add(
                        "Non-locomotion revolver animation clips remain on disk (" + clipCount + ") for future rebuild review.");
                }
            }

            string prototypingRoot = CCS_CharacterControllerConstants.ModuleRootPath + "/Prototyping";
            if (Directory.Exists(prototypingRoot))
            {
                string[] testNamedAssets = Directory.GetFiles(prototypingRoot, "*Test*", SearchOption.AllDirectories);
                if (testNamedAssets.Length > 0)
                {
                    warnings.Add(
                        "Prototyping assets contain Test naming (" + testNamedAssets.Length + " paths) — expected for blockout assets.");
                }
            }

            if (File.Exists(ArchitectureDocPath))
            {
                warnings.Add("Architecture documentation references future layers and parameters (planning only).");
            }
        }

        private static bool ContainsFutureDesignOnlyRuntimeHash(string parameterSource)
        {
            int futureIndex = parameterSource.IndexOf("class FutureDesignOnly");
            if (futureIndex < 0)
            {
                return false;
            }

            string futureSection = parameterSource.Substring(futureIndex);
            return futureSection.Contains("StringToHash");
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
