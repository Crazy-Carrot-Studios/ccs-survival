using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using CCS.Modules.Attributes.Editor;
using CCS.Modules.CharacterController.Editor;
using CCS.Modules.Interaction;
using CCS.Modules.Interaction.Editor;
using CCS.Modules.Weapons;
using CCS.Modules.Weapons.Editor;

using CCS.Project;

using UnityEditor;

// =============================================================================
// SCRIPT: CCS_ProjectAuditValidator
// CATEGORY: Project / Editor
// PURPOSE: Report-first project audit for docs, versions, asmdefs, and legacy leftovers.
// PLACEMENT: Editor validator invoked from CCS/Project/Run Project Audit.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Does not repair assets or regenerate scenes.
// =============================================================================

namespace CCS.Project.Editor
{
    public static class CCS_ProjectAuditValidator
    {
        private const string ReadmePath = "README.md";
        private const string BundleVersionAssetPath = "ProjectSettings/ProjectSettings.asset";
        private const string VersionPolicyPath = "Assets/CCS/Project/Documentation/CCS_Versioning_Policy.md";

        private static readonly string[] RequiredRuntimeAssemblies =
        {
            "Assets/CCS/Framework/Core/Runtime/CCS.Core.Runtime.asmdef",
            "Assets/CCS/Project/Runtime/CCS.Project.Runtime.asmdef",
            "Assets/CCS/Modules/CharacterController/Runtime/CCS.Modules.CharacterController.Runtime.asmdef",
            "Assets/CCS/Modules/Attributes/Runtime/CCS.Modules.Attributes.Runtime.asmdef",
            "Assets/CCS/Modules/Interaction/Runtime/CCS.Modules.Interaction.Runtime.asmdef",
            "Assets/CCS/Modules/Weapons/Runtime/CCS.Modules.Weapons.Runtime.asmdef",
        };

        private static readonly string[] LegacyAssetPaths =
        {
            "Assets/CCS/Modules/Interaction/Tests/Prefabs/PF_CCS_TestInteractable_ToggleCube.prefab",
            "Assets/CCS/Modules/Interaction/Runtime/Components/CCS_TestToggleInteractable.cs",
        };

        private static readonly string[] RequiredScenes =
        {
            "Assets/CCS/Scenes/Bootstrap/SCN_CCS_Survival_Bootstrap.unity",
            "Assets/CCS/Scenes/CharacterController/SCN_CCS_CharacterController_MasterTest.unity",
        };

        public static CCS_SurvivalValidationResult RunProjectAudit(bool includeModuleValidators)
        {
            List<string> failures = new List<string>();
            List<string> notes = new List<string>();

            ValidateVersionConsistency(failures, notes);
            ValidateRequiredAssemblies(failures);
            ValidateLegacyLeftovers(failures);
            ValidateRequiredScenes(failures);
            ValidateActiveModuleDocs(failures, notes);

            if (includeModuleValidators)
            {
                AppendModuleValidatorResults(failures, notes);
            }

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join("\n", failures));
            }

            string summary = notes.Count > 0
                ? "Project audit passed. " + string.Join(" ", notes)
                : "Project audit passed.";
            return CCS_SurvivalValidationResult.Pass(summary);
        }

        private static void ValidateVersionConsistency(List<string> failures, List<string> notes)
        {
            string readme = ReadTextIfExists(ReadmePath);
            string bundleVersion = ReadBundleVersion();
            string policy = ReadTextIfExists(VersionPolicyPath);

            if (string.IsNullOrEmpty(bundleVersion))
            {
                failures.Add("Project audit: could not read ProjectSettings.bundleVersion.");
                return;
            }

            if (!readme.Contains(bundleVersion))
            {
                failures.Add($"Project audit: README.md does not mention bundleVersion {bundleVersion}.");
            }

            if (!policy.Contains($"**Current version:** `{bundleVersion}`"))
            {
                failures.Add($"Project audit: versioning policy current version does not match {bundleVersion}.");
            }

            notes.Add($"Version {bundleVersion} aligned across README and policy.");
        }

        private static void ValidateRequiredAssemblies(List<string> failures)
        {
            for (int i = 0; i < RequiredRuntimeAssemblies.Length; i++)
            {
                if (!File.Exists(RequiredRuntimeAssemblies[i]))
                {
                    failures.Add("Project audit: missing asmdef: " + RequiredRuntimeAssemblies[i]);
                }
            }
        }

        private static void ValidateLegacyLeftovers(List<string> failures)
        {
            for (int i = 0; i < LegacyAssetPaths.Length; i++)
            {
                if (File.Exists(LegacyAssetPaths[i]))
                {
                    failures.Add("Project audit: legacy asset still present: " + LegacyAssetPaths[i]);
                }
            }

            string interactionDoc = ReadTextIfExists(CCS_InteractionConstants.ModuleRootPath + "/Documentation/CCS_Interaction_Module.md");
            if (interactionDoc.Contains("PF_CCS_TestInteractable_ToggleCube")
                && !interactionDoc.Contains("Retired"))
            {
                failures.Add("Project audit: Interaction module doc still references toggle cube without retired context.");
            }
        }

        private static void ValidateRequiredScenes(List<string> failures)
        {
            for (int i = 0; i < RequiredScenes.Length; i++)
            {
                if (!File.Exists(RequiredScenes[i]))
                {
                    failures.Add("Project audit: missing scene: " + RequiredScenes[i]);
                }
            }
        }

        private static void ValidateActiveModuleDocs(List<string> failures, List<string> notes)
        {
            string modulesReadme = ReadTextIfExists("Assets/CCS/Modules/README.md");
            if (!modulesReadme.Contains("CharacterController")
                || !modulesReadme.Contains("Attributes")
                || !modulesReadme.Contains("Interaction")
                || !modulesReadme.Contains("Weapons"))
            {
                failures.Add("Project audit: Modules README does not list all active modules.");
            }

            notes.Add("Active modules documented.");
        }

        private static void AppendModuleValidatorResults(List<string> failures, List<string> notes)
        {
            AppendValidatorResult(
                failures,
                CCS_InteractionModuleValidator.ValidateInteractionModule(),
                "Interaction");

            AppendValidatorResult(
                failures,
                CCS_CharacterControllerMasterTestValidator.ValidateMasterTestScene(),
                "Character Controller Master Test");

            AppendValidatorResult(
                failures,
                CCS_AttributesModuleValidator.ValidateAttributesModule(),
                "Attributes");

            AppendValidatorResult(
                failures,
                CCS_WeaponsModuleValidator.ValidateWeaponsModule(),
                "Weapons");

            notes.Add("Module validators executed.");
        }

        private static void AppendValidatorResult(
            List<string> failures,
            CCS_SurvivalValidationResult result,
            string label)
        {
            if (result.IsSuccess)
            {
                return;
            }

            failures.Add($"[{label}] {result.Message}");
        }

        private static string ReadTextIfExists(string relativePath)
        {
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
            return File.Exists(fullPath) ? File.ReadAllText(fullPath) : string.Empty;
        }

        private static string ReadBundleVersion()
        {
            string text = ReadTextIfExists(BundleVersionAssetPath);
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            Match match = Regex.Match(text, @"bundleVersion:\s*(\S+)");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }
    }
}
