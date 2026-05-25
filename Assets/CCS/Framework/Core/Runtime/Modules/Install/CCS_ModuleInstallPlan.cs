using System;
using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_ModuleInstallPlan
// CATEGORY: Core / Runtime / Modules / Install
// PURPOSE: Executes explicit ordered module installs with validation and stop-on-failure.
// PLACEMENT: Runtime utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: No auto-discovery, sorting, or singleton. Manual order is authoritative.
// =============================================================================

namespace CCS.Core
{
    public sealed class CCS_ModuleInstallPlan
    {
        private const string LogCategory = "Module Install Plan";

        #region Variables

        private readonly List<CCS_ModuleInstallPlanEntry> entries;
        private readonly bool enableDebugLogs;

        #endregion

        #region Public Methods

        public CCS_ModuleInstallPlan(bool enableDebugLogs = false)
        {
            entries = new List<CCS_ModuleInstallPlanEntry>();
            this.enableDebugLogs = enableDebugLogs;
        }

        public void AddInstaller(CCS_IModuleInstaller installer)
        {
            if (installer == null)
            {
                return;
            }

            entries.Add(new CCS_ModuleInstallPlanEntry(installer));
        }

        public CCS_Result Execute(CCS_RuntimeHost runtimeHost)
        {
            CCS_Result hostValidation = CCS_CoreValidation.ValidateRuntimeHost(runtimeHost);
            if (!hostValidation.IsSuccess)
            {
                return hostValidation;
            }

            CCS_Result duplicateValidation = ValidateDuplicateModuleIds();
            if (!duplicateValidation.IsSuccess)
            {
                return duplicateValidation;
            }

            CCS_Result dependencyValidation = ValidateDependencies(runtimeHost);
            if (!dependencyValidation.IsSuccess)
            {
                return dependencyValidation;
            }

            for (int index = 0; index < entries.Count; index++)
            {
                CCS_ModuleInstallPlanEntry entry = entries[index];
                CCS_Result entryValidation = ValidateEntry(entry, index);
                if (!entryValidation.IsSuccess)
                {
                    return entryValidation;
                }

                entry.Installer.Install(runtimeHost);

                if (entry.Installer.Module.LifecycleState == CCS_ModuleLifecycleState.Failed)
                {
                    return CCS_Result.Failure(
                        CCS_CoreErrorCode.ModuleInstallFailed,
                        $"Install plan stopped at index {index} for module ID: {entry.ModuleId}");
                }

                if (entry.Installer.Module.LifecycleState != CCS_ModuleLifecycleState.Installed)
                {
                    return CCS_Result.Failure(
                        CCS_CoreErrorCode.ModuleInstallFailed,
                        $"Install plan entry did not reach Installed at index {index} for module ID: {entry.ModuleId}");
                }
            }

            CCS_Logger.Log(LogCategory, "Module install plan completed.", enableDebugLogs);
            return CCS_Result.Success("Module install plan completed.");
        }

        #endregion

        #region Private Methods

        private CCS_Result ValidateDuplicateModuleIds()
        {
            HashSet<string> seenModuleIds = new HashSet<string>(StringComparer.Ordinal);

            for (int index = 0; index < entries.Count; index++)
            {
                CCS_ModuleInstallPlanEntry entry = entries[index];
                CCS_Result moduleIdValidation = CCS_CoreValidation.ValidateModuleId(entry.ModuleId);
                if (!moduleIdValidation.IsSuccess)
                {
                    return CCS_Result.Failure(
                        CCS_CoreErrorCode.InvalidModuleId,
                        $"Install plan entry {index} has invalid module ID.");
                }

                if (!seenModuleIds.Add(entry.ModuleId))
                {
                    return CCS_Result.Failure(
                        CCS_CoreErrorCode.DuplicateInstallPlanEntry,
                        $"Duplicate module ID in install plan: {entry.ModuleId}");
                }
            }

            return CCS_Result.Success();
        }

        private CCS_Result ValidateDependencies(CCS_RuntimeHost runtimeHost)
        {
            for (int index = 0; index < entries.Count; index++)
            {
                CCS_IModule module = entries[index].Installer.Module;
                CCS_Result dependencyResult = CCS_ModuleDependencyValidation.ValidateInstallDependencies(
                    runtimeHost,
                    module,
                    index);
                if (!dependencyResult.IsSuccess)
                {
                    return dependencyResult;
                }
            }

            return CCS_Result.Success();
        }

        private static CCS_Result ValidateEntry(CCS_ModuleInstallPlanEntry entry, int index)
        {
            if (entry == null || entry.Installer == null)
            {
                return CCS_Result.Failure(
                    CCS_CoreErrorCode.NullModule,
                    $"Install plan entry {index} installer is null.");
            }

            return CCS_CoreValidation.ValidateModule(entry.Installer.Module);
        }

        #endregion
    }
}
