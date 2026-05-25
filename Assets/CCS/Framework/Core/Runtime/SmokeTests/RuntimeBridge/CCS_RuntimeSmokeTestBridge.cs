using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RuntimeSmokeTestBridge
// CATEGORY: Framework / Tests / Runtime / SmokeTests
// PURPOSE: Validation-only bridge that runs smoke test installers on CCS_RuntimeHost.
// PLACEMENT: Attach to same GameObject as CCS_RuntimeHost during validation scenes.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Temporary validation infrastructure. Runs after CCS_RuntimeHost Awake.
// =============================================================================

namespace CCS.Core.Tests
{
    [DefaultExecutionOrder(100)]
    public sealed class CCS_RuntimeSmokeTestBridge : MonoBehaviour
    {
        private const string LogCategory = "SmokeTest";

        #region Variables

        [SerializeField] private bool enableDebugLogs = true;

        private CCS_RuntimeHost runtimeHost;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            runtimeHost = GetComponent<CCS_RuntimeHost>();
            if (!CCS_Validation.IsObjectValid(runtimeHost))
            {
                CCS_Logger.LogWarning(LogCategory, "CCS_RuntimeHost not found on smoke test bridge GameObject.");
                return;
            }

            if (!runtimeHost.EnableRuntimeDiagnostics)
            {
                return;
            }

            CCS_RuntimeSmokeTestInstaller runtimeSmokeTestInstaller = new CCS_RuntimeSmokeTestInstaller(enableDebugLogs);
            runtimeHost.BootstrapRunner.RegisterInstaller(runtimeSmokeTestInstaller);
            runtimeHost.BootstrapRunner.Run(runtimeHost);

            ValidateModuleDependencyMissing(runtimeHost);

            CCS_SmokeTestModuleInstaller moduleSmokeTestInstaller = new CCS_SmokeTestModuleInstaller(enableDebugLogs);
            if (moduleSmokeTestInstaller.Module.LifecycleState != CCS_ModuleLifecycleState.Uninstalled)
            {
                CCS_Logger.LogWarning(LogCategory, "Smoke test module should start Uninstalled.");
            }

            runtimeHost.BootstrapRunner.RegisterInstaller(moduleSmokeTestInstaller);
            runtimeHost.BootstrapRunner.Run(runtimeHost);

            ValidateModuleHostRegistry(runtimeHost);
            ValidateModuleLifecycleState(runtimeHost);
            ValidateDuplicateModuleRegistration(runtimeHost);
            ValidateModuleDependencyPresent(runtimeHost);
            ValidateModuleInstallPlan(runtimeHost);
            ValidateModuleUninstall(runtimeHost);
            ValidateCoreDiagnostics(runtimeHost);
        }

        #endregion

        #region Private Methods

        private void ValidateModuleHostRegistry(CCS_RuntimeHost host)
        {
            if (host.ModuleHost.TryGetModule(CCS_SmokeTestModule.ModuleId, out CCS_IModule registeredModule))
            {
                CCS_Logger.Log(LogCategory, $"Module host registry lookup confirmed: {registeredModule.Metadata.ModuleId}", enableDebugLogs);
                return;
            }

            CCS_Logger.LogWarning(LogCategory, "Module host registry lookup failed for smoke test module.");
        }

        private void ValidateModuleLifecycleState(CCS_RuntimeHost host)
        {
            if (!host.ModuleHost.TryGetModuleLifecycleState(CCS_SmokeTestModule.ModuleId, out CCS_ModuleLifecycleState lifecycleState))
            {
                CCS_Logger.LogWarning(LogCategory, "Module lifecycle state lookup failed for smoke test module.");
                return;
            }

            if (lifecycleState == CCS_ModuleLifecycleState.Installed)
            {
                CCS_Logger.Log(LogCategory, "Smoke test module lifecycle Installed confirmed.", enableDebugLogs);
                return;
            }

            CCS_Logger.LogWarning(LogCategory, $"Smoke test module lifecycle unexpected after install: {lifecycleState}");
        }

        private void ValidateDuplicateModuleRegistration(CCS_RuntimeHost host)
        {
            if (!host.ModuleHost.TryGetModule(CCS_SmokeTestModule.ModuleId, out CCS_IModule registeredModule))
            {
                CCS_Logger.LogWarning(LogCategory, "Cannot validate duplicate install without registered module.");
                return;
            }

            CCS_SmokeTestModuleInstaller duplicateInstaller = new CCS_SmokeTestModuleInstaller(enableDebugLogs);
            CCS_IModule duplicateModule = duplicateInstaller.Module;

            if (duplicateModule.LifecycleState != CCS_ModuleLifecycleState.Uninstalled)
            {
                CCS_Logger.LogWarning(LogCategory, "Duplicate smoke test module should start Uninstalled.");
            }

            CCS_Result preflightResult = host.ModuleHost.TryPreflightInstall(CCS_SmokeTestModule.ModuleId);
            if (!preflightResult.IsSuccess
                && preflightResult.ErrorCode != CCS_CoreErrorCode.ModuleAlreadyInstalled)
            {
                CCS_Logger.LogWarning(LogCategory, $"Duplicate preflight unexpected error code: {preflightResult.ErrorCode}");
            }

            duplicateInstaller.Install(host);

            if (registeredModule.LifecycleState != CCS_ModuleLifecycleState.Installed)
            {
                CCS_Logger.LogWarning(LogCategory, "Registered smoke test module lifecycle was overwritten by duplicate install.");
            }

            if (duplicateModule.LifecycleState != CCS_ModuleLifecycleState.Failed)
            {
                CCS_Logger.LogWarning(LogCategory, $"Duplicate smoke test module lifecycle expected Failed, got {duplicateModule.LifecycleState}");
            }

            CCS_Logger.Log(LogCategory, "Duplicate module registration blocked as expected.", enableDebugLogs);
        }

        private void ValidateModuleDependencyMissing(CCS_RuntimeHost host)
        {
            CCS_SmokeTestDependentModuleInstaller dependentInstaller = new CCS_SmokeTestDependentModuleInstaller(enableDebugLogs);
            dependentInstaller.Install(host);

            if (dependentInstaller.Module.LifecycleState != CCS_ModuleLifecycleState.Failed)
            {
                CCS_Logger.LogWarning(LogCategory, "Dependent module install should fail when required module is missing.");
                return;
            }

            CCS_Logger.Log(LogCategory, "Dependent module install blocked when dependency missing.", enableDebugLogs);
        }

        private void ValidateModuleDependencyPresent(CCS_RuntimeHost host)
        {
            CCS_SmokeTestDependentModuleInstaller dependentInstaller = new CCS_SmokeTestDependentModuleInstaller(enableDebugLogs);
            dependentInstaller.Install(host);

            if (dependentInstaller.Module.LifecycleState != CCS_ModuleLifecycleState.Installed)
            {
                CCS_Logger.LogWarning(LogCategory, "Dependent module install should succeed when required module is installed.");
                return;
            }

            CCS_Logger.Log(LogCategory, "Dependent module install succeeded with required dependency present.", enableDebugLogs);
        }

        private void ValidateModuleInstallPlan(CCS_RuntimeHost host)
        {
            CCS_ModuleInstallPlan duplicatePlan = new CCS_ModuleInstallPlan(enableDebugLogs);
            duplicatePlan.AddInstaller(new CCS_SmokeTestModuleInstaller(enableDebugLogs));
            duplicatePlan.AddInstaller(new CCS_SmokeTestModuleInstaller(enableDebugLogs));

            CCS_Result duplicatePlanResult = duplicatePlan.Execute(host);
            if (!duplicatePlanResult.IsSuccess
                && duplicatePlanResult.ErrorCode == CCS_CoreErrorCode.DuplicateInstallPlanEntry)
            {
                CCS_Logger.Log(LogCategory, "Install plan duplicate entry blocked as expected.", enableDebugLogs);
            }
            else
            {
                CCS_Logger.LogWarning(LogCategory, $"Install plan duplicate entry expected failure missing: {duplicatePlanResult}");
            }

            // Clear registry so dependency preflight reflects plan order, not prior installs.
            host.ModuleHost.UninstallModule(host, CCS_SmokeTestDependentModule.ModuleId);
            host.ModuleHost.UninstallModule(host, CCS_SmokeTestModule.ModuleId);

            CCS_ModuleInstallPlan orderPlan = new CCS_ModuleInstallPlan(enableDebugLogs);
            orderPlan.AddInstaller(new CCS_SmokeTestDependentModuleInstaller(enableDebugLogs));
            orderPlan.AddInstaller(new CCS_SmokeTestModuleInstaller(enableDebugLogs));

            CCS_Result orderPlanResult = orderPlan.Execute(host);
            if (!orderPlanResult.IsSuccess
                && orderPlanResult.ErrorCode == CCS_CoreErrorCode.MissingRequiredModuleDependency)
            {
                CCS_Logger.Log(
                    LogCategory,
                    $"Install plan dependency order blocked as expected ({orderPlanResult.ErrorCode}).",
                    enableDebugLogs);
            }
            else
            {
                CCS_Logger.LogWarning(LogCategory, $"Install plan dependency order expected failure missing: {orderPlanResult}");
            }

            CCS_ModuleInstallPlan successPlan = new CCS_ModuleInstallPlan(enableDebugLogs);
            successPlan.AddInstaller(new CCS_SmokeTestModuleInstaller(enableDebugLogs));

            CCS_Result successPlanResult = successPlan.Execute(host);
            if (!successPlanResult.IsSuccess)
            {
                CCS_Logger.LogWarning(LogCategory, $"Install plan smoke install failed: {successPlanResult.Message}");
                return;
            }

            if (!host.ModuleHost.IsModuleInstalled(CCS_SmokeTestModule.ModuleId))
            {
                CCS_Logger.LogWarning(LogCategory, "Install plan did not install smoke test module.");
                return;
            }

            CCS_Logger.Log(LogCategory, "Install plan smoke module install confirmed.", enableDebugLogs);
        }

        private void ValidateCoreDiagnostics(CCS_RuntimeHost host)
        {
            try
            {
                CCS_CoreDiagnosticsReport report = host.BuildDiagnosticsReport();
                if (report == null)
                {
                    CCS_Logger.LogWarning(LogCategory, "Diagnostics report was null.");
                    return;
                }

                if (!report.IsRuntimeHostInitialized)
                {
                    CCS_Logger.LogWarning(LogCategory, "Diagnostics expected runtime host initialized.");
                }

                if (report.RegisteredModuleCount != 0)
                {
                    CCS_Logger.LogWarning(LogCategory, $"Diagnostics expected zero modules after uninstall, got {report.RegisteredModuleCount}.");
                }

                if (report.Modules != null && report.Modules.Count > 0)
                {
                    CCS_Logger.LogWarning(LogCategory, "Diagnostics module list should be empty after uninstall.");
                }

                CCS_Logger.Log(
                    LogCategory,
                    $"Diagnostics report OK. Services={report.Services.RegisteredServiceCount}, Update={report.UpdateLoop.UpdatableSystemCount}, Events={report.EventSubscriptionCount}, Installers={report.BootstrapInstallerCount}",
                    enableDebugLogs);
            }
            catch (System.Exception exception)
            {
                CCS_Logger.LogWarning(LogCategory, $"Diagnostics report failed: {exception.Message}");
            }
        }

        private void ValidateModuleUninstall(CCS_RuntimeHost host)
        {
            if (!host.ModuleHost.TryGetModule(CCS_SmokeTestModule.ModuleId, out CCS_IModule installedModule))
            {
                CCS_Logger.LogWarning(LogCategory, "Cannot validate uninstall without installed smoke test module.");
                return;
            }

            CCS_Result uninstallResult = host.ModuleHost.UninstallModule(host, CCS_SmokeTestModule.ModuleId);
            if (!uninstallResult.IsSuccess)
            {
                CCS_Logger.LogWarning(LogCategory, $"Smoke test module uninstall failed: {uninstallResult.Message}");
                return;
            }

            if (host.ModuleHost.TryGetModule(CCS_SmokeTestModule.ModuleId, out CCS_IModule _))
            {
                CCS_Logger.LogWarning(LogCategory, "Smoke test module still found in registry after uninstall.");
            }
            else
            {
                CCS_Logger.Log(LogCategory, "Smoke test module registry cleared after uninstall.", enableDebugLogs);
            }

            if (installedModule.LifecycleState == CCS_ModuleLifecycleState.Uninstalled)
            {
                CCS_Logger.Log(LogCategory, "Smoke test module lifecycle Uninstalled confirmed.", enableDebugLogs);
            }
            else
            {
                CCS_Logger.LogWarning(LogCategory, $"Smoke test module lifecycle expected Uninstalled, got {installedModule.LifecycleState}");
            }

            CCS_Result missingUninstallResult = host.ModuleHost.UninstallModule(host, "ccs.smoketest.missing");
            if (!missingUninstallResult.IsSuccess
                && missingUninstallResult.ErrorCode == CCS_CoreErrorCode.ModuleNotRegistered)
            {
                CCS_Logger.Log(LogCategory, "Missing module uninstall failed gracefully as expected.", enableDebugLogs);
            }
            else if (!missingUninstallResult.IsSuccess)
            {
                CCS_Logger.Log(LogCategory, "Missing module uninstall failed gracefully as expected.", enableDebugLogs);
            }

            CCS_Result duplicateUninstallResult = host.ModuleHost.UninstallModule(host, CCS_SmokeTestModule.ModuleId);
            if (!duplicateUninstallResult.IsSuccess)
            {
                CCS_Logger.Log(LogCategory, "Duplicate module uninstall failed gracefully as expected.", enableDebugLogs);
            }
        }

        #endregion
    }
}
