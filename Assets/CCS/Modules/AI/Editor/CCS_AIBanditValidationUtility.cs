using System.Collections.Generic;
using System.IO;
using CCS.Project;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AIBanditValidationUtility
// CATEGORY: Modules / AI / Editor
// PURPOSE: Validates AI module foundation, prefab contracts, and netcode registration.
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Milestone B13 validation set for v0.7.0.
// =============================================================================

namespace CCS.Modules.AI.Editor
{
    public static class CCS_AIBanditValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateMilestoneB13Foundation()
        {
            List<string> failures = new List<string>();

            AppendIfMissing(failures, Directory.Exists(CCS_AIConstants.ModuleRootPath + "/Runtime"), "Missing AI Runtime folder.");
            AppendIfMissing(failures, Directory.Exists(CCS_AIConstants.ModuleRootPath + "/Editor"), "Missing AI Editor folder.");
            AppendIfMissing(failures, Directory.Exists(CCS_AIConstants.ModuleRootPath + "/Documentation"), "Missing AI Documentation folder.");
            AppendIfMissing(failures, Directory.Exists(CCS_AIConstants.ModuleRootPath + "/Content/Prefabs"), "Missing AI Content/Prefabs folder.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_AIConstants.ModuleRootPath + "/Runtime/CCS.Modules.AI.Runtime.asmdef"),
                "Missing CCS.Modules.AI.Runtime.asmdef.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_AIConstants.ModuleRootPath + "/Editor/CCS.Modules.AI.Editor.asmdef"),
                "Missing CCS.Modules.AI.Editor.asmdef.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_AIConstants.ModuleRootPath + "/Runtime/Combat/CCS_AIBanditBrain.cs"),
                "Missing CCS_AIBanditBrain.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_AIConstants.ModuleRootPath + "/Runtime/Components/CCS_AIBanditController.cs"),
                "Missing CCS_AIBanditController.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_AIConstants.ModuleRootPath + "/Runtime/Spawning/CCS_AIBanditSpawner.cs"),
                "Missing CCS_AIBanditSpawner.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_AIConstants.ModuleRootPath + "/Runtime/UI/CCS_AIBanditNameplate.cs"),
                "Missing CCS_AIBanditNameplate.");
            AppendIfMissing(
                failures,
                File.Exists("Assets/CCS/Modules/Attributes/Runtime/Components/CCS_NetworkHealth.cs"),
                "Missing CCS_NetworkHealth shared damage implementation.");
            AppendIfMissing(
                failures,
                File.Exists("Assets/CCS/Modules/Attributes/Runtime/Data/CCS_IDamageable.cs"),
                "Missing CCS_IDamageable shared contract.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_AIConstants.AIBanditPrefabPath),
                "Missing PF_CCS_AI_Bandit_Networked prefab.");

            string netcodeConstantsPath =
                "Assets/CCS/Modules/CharacterController/Tests/Netcode/Runtime/CCS_NetcodeTestConstants.cs";
            bool netcodePathRegistered = File.Exists(netcodeConstantsPath)
                && File.ReadAllText(netcodeConstantsPath).Contains(CCS_AIConstants.AIBanditPrefabPath);

            AppendIfMissing(
                failures,
                netcodePathRegistered,
                "AI bandit prefab must be registered in CCS_NetcodeTestConstants.RequiredNetworkPrefabPaths.");

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("AI Milestone B13 validation passed.");
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
