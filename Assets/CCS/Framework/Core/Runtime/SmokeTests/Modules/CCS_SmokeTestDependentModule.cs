using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_SmokeTestDependentModule
// CATEGORY: Framework / Tests / Runtime / SmokeTests
// PURPOSE: Test-only module declaring a required dependency on the smoke test module.
// PLACEMENT: Installed by CCS_SmokeTestDependentModuleInstaller during diagnostics Play Mode.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Validates dependency preflight only. No gameplay logic.
// =============================================================================

namespace CCS.Core.Tests
{
    public sealed class CCS_SmokeTestDependentModule : CCS_ModuleBase
    {
        public const string ModuleId = "ccs.smoketest.dependent";

        private static readonly IReadOnlyCollection<CCS_ModuleDependency> ModuleDependencies =
            new[] { CCS_ModuleDependency.RequiredModule(CCS_SmokeTestModule.ModuleId) };

        #region Properties

        public override IReadOnlyCollection<CCS_ModuleDependency> Dependencies => ModuleDependencies;

        #endregion

        #region Public Methods

        public CCS_SmokeTestDependentModule(bool enableDebugLogs)
            : base(
                new CCS_ModuleMetadata(ModuleId, "CCS Smoke Test Dependent Module", "0.3.14", "Dependency validation only."),
                enableDebugLogs)
        {
        }

        #endregion
    }
}
