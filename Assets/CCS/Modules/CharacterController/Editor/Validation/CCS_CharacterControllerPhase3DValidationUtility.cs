using System.Collections.Generic;
using System.IO;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterControllerPhase3DValidationUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Validates v0.7.5 Phase 3D player prefab hierarchy architecture (planning only).
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Does not enforce hierarchy migration; confirms architecture artifacts exist.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerPhase3DValidationUtility
    {
        private const string ArchitectureDocPath =
            "Assets/CCS/Modules/CharacterController/Documentation/CCS_PlayerPrefab_Hierarchy_Architecture.md";

        private const string CompositionInterfacePath =
            "Assets/CCS/Modules/CharacterController/Runtime/Composition/CCS_IPlayerCompositionRoot.cs";

        private const string EquipmentFitStudioWindowPath =
            "Assets/CCS/Modules/CharacterController/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioWindow.cs";

        private const string CharacterControllerTestsRoot =
            "Assets/CCS/Modules/CharacterController/Tests";

        private const string AnimationFitStudioRoot =
            "Assets/CCS/Modules/CharacterController/Editor/AnimationFitStudio";

        private const string TempHierarchyAuditPath =
            "Logs/CharacterController/PrefabAudit/TEMP_PF_CCS_CharacterController_Player_Networked_Hierarchy.md";

        private const string TempHierarchyParserPath =
            "Logs/CharacterController/PrefabAudit/_temp_prefab_hierarchy_parser.py";

        public static CCS_SurvivalValidationResult ValidatePhase3DPlayerPrefabHierarchyArchitecture()
        {
            List<string> failures = new List<string>();
            List<string> warnings = new List<string>();

            CCS_SurvivalValidationResult phase3CResult =
                CCS_CharacterControllerPhase3CValidationUtility.ValidatePhase3CAnimationRebuildArchitecture();
            if (!phase3CResult.IsSuccess)
            {
                failures.Add(phase3CResult.Message);
            }
            else if (!string.IsNullOrEmpty(phase3CResult.Message) && phase3CResult.Message.Contains("Warnings:"))
            {
                warnings.Add(phase3CResult.Message);
            }

            ValidateArchitectureArtifactsExist(failures);
            ValidateCompositionInterfaceExists(failures);
            ValidateTestsFolderRemoved(failures);
            ValidateAnimationFitStudioNotPresent(failures);
            ValidateEquipmentFitStudioPresent(failures);
            ValidateTemporaryAuditArtifactsRemoved(warnings);
            CollectPlanningWarnings(warnings);

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            string message =
                "Phase 3D player prefab hierarchy architecture validated. No hierarchy migration enforced in v0.7.5.";
            if (warnings.Count > 0)
            {
                message += " Warnings: " + string.Join(" ", warnings);
            }

            return CCS_SurvivalValidationResult.Pass(message);
        }

        private static void ValidateArchitectureArtifactsExist(List<string> failures)
        {
            AppendIfMissing(failures, File.Exists(ArchitectureDocPath), "Missing architecture doc at " + ArchitectureDocPath);
            if (!File.Exists(ArchitectureDocPath))
            {
                return;
            }

            string source = File.ReadAllText(ArchitectureDocPath);
            AppendIfMissing(failures, source.Contains("Target A"), "Architecture doc must define Target A root budget.");
            AppendIfMissing(failures, source.Contains("Target B"), "Architecture doc must define Target B Netcode-safe root budget.");
            AppendIfMissing(failures, source.Contains("VisualRoot"), "Architecture doc must document VisualRoot nesting issue.");
            AppendIfMissing(failures, source.Contains("LocalOnly"), "Architecture doc must document LocalOnly UI separation.");
            AppendIfMissing(
                failures,
                source.Contains("CCS_PlayerPrefabHierarchyMigrationBuilder"),
                "Architecture doc must reference future hierarchy migration builder.");
            AppendIfMissing(
                failures,
                source.Contains("CCS_PlayerPrefabHierarchyValidationUtility"),
                "Architecture doc must reference future hierarchy validation utility.");
        }

        private static void ValidateCompositionInterfaceExists(List<string> failures)
        {
            AppendIfMissing(
                failures,
                File.Exists(CompositionInterfacePath),
                "Missing composition interface at " + CompositionInterfacePath);
            if (!File.Exists(CompositionInterfacePath))
            {
                return;
            }

            string source = File.ReadAllText(CompositionInterfacePath);
            AppendIfMissing(
                failures,
                source.Contains("interface CCS_IPlayerCompositionRoot"),
                "CCS_IPlayerCompositionRoot interface must be defined for v0.7.5.");
        }

        private static void ValidateTestsFolderRemoved(List<string> failures)
        {
            if (Directory.Exists(CharacterControllerTestsRoot))
            {
                failures.Add("CharacterController Tests folder must not return: " + CharacterControllerTestsRoot);
            }
        }

        private static void ValidateAnimationFitStudioNotPresent(List<string> failures)
        {
            if (Directory.Exists(AnimationFitStudioRoot))
            {
                failures.Add("Animation Fit Studio must remain removed: " + AnimationFitStudioRoot);
            }
        }

        private static void ValidateEquipmentFitStudioPresent(List<string> failures)
        {
            AppendIfMissing(
                failures,
                File.Exists(EquipmentFitStudioWindowPath),
                "Equipment Fit Studio window missing at " + EquipmentFitStudioWindowPath);
        }

        private static void ValidateTemporaryAuditArtifactsRemoved(List<string> warnings)
        {
            if (File.Exists(TempHierarchyAuditPath))
            {
                warnings.Add("Temporary hierarchy audit still present (delete before commit): " + TempHierarchyAuditPath);
            }

            if (File.Exists(TempHierarchyParserPath))
            {
                warnings.Add("Temporary hierarchy parser still present (delete before commit): " + TempHierarchyParserPath);
            }
        }

        private static void CollectPlanningWarnings(List<string> warnings)
        {
            string migrationBuilderPath =
                "Assets/CCS/Modules/CharacterController/Editor/Builders/CCS_PlayerPrefabHierarchyMigrationBuilder.cs";
            string hierarchyValidatorPath =
                "Assets/CCS/Modules/CharacterController/Editor/Validation/CCS_PlayerPrefabHierarchyValidationUtility.cs";

            if (File.Exists(migrationBuilderPath))
            {
                warnings.Add("Migration builder exists early — expected v0.7.6+ only.");
            }

            if (File.Exists(hierarchyValidatorPath))
            {
                warnings.Add("Hierarchy validation utility exists early — expected v0.7.6+ only.");
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
