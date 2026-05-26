using System;
using System.Collections.Generic;
using CCS.Core;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalBootstrap
// CATEGORY: Survival / Bootstrap
// PURPOSE: Gameplay startup pipeline for survival layer install sequencing on CCS_RuntimeHost.
// PLACEMENT: Attach to PF_CCS_Survival_BootstrapRoot with CCS_RuntimeHost (same GameObject).
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Runs after host Awake. Core diagnostics off; survival diagnostics owned here. Character skeleton at 0.3.0.
// =============================================================================

namespace CCS.Survival
{
    [DefaultExecutionOrder(100)]
    public sealed class CCS_SurvivalBootstrap : MonoBehaviour
    {
        private const string LogCategory = "Survival Bootstrap";

        #region Variables

        [Header("Survival Bootstrap")]
        [Tooltip("When enabled, survival diagnostics validate Core health without invoking Core smoke tests.")]
        [SerializeField] private bool enableSurvivalDiagnostics = true;

        [SerializeField] private bool enableDebugLogs = true;

        [Header("Profile Slots (Optional)")]
        [Tooltip("Future profile-driven setup slots. Leave empty during skeleton phase; validated only when assigned.")]
        [SerializeField] private CCS_SurvivalBootstrapProfileSlot[] bootstrapProfileSlots = Array.Empty<CCS_SurvivalBootstrapProfileSlot>();

        private CCS_RuntimeHost runtimeHost;
        private CCS_SurvivalRuntimeContext survivalContext;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            runtimeHost = GetComponent<CCS_RuntimeHost>();
            if (!CCS_Validation.IsObjectValid(runtimeHost))
            {
                CCS_Logger.LogWarning(LogCategory, "CCS_RuntimeHost not found on survival bootstrap root.");
                return;
            }

            survivalContext = new CCS_SurvivalRuntimeContext(runtimeHost, enableDebugLogs);
            CCS_Result initializeResult = survivalContext.Initialize();
            if (!initializeResult.IsSuccess)
            {
                CCS_Logger.LogWarning(LogCategory, initializeResult.Message);
                return;
            }

            RunSurvivalInstallPipeline();

            if (enableSurvivalDiagnostics)
            {
                CCS_Result diagnosticsResult = CCS_SurvivalDiagnostics.RunCoreHealthValidation(survivalContext, enableDebugLogs);
                if (!diagnosticsResult.IsSuccess)
                {
                    CCS_Logger.LogWarning(LogCategory, diagnosticsResult.Message);
                }
            }

            CCS_Logger.Log(LogCategory, "Survival bootstrap completed.", enableDebugLogs);
        }

        private void OnDestroy()
        {
            if (survivalContext != null)
            {
                survivalContext.Shutdown();
            }
        }

        #endregion

        #region Properties

        public CCS_SurvivalRuntimeContext SurvivalContext => survivalContext;

        public bool EnableSurvivalDiagnostics => enableSurvivalDiagnostics;

        public IReadOnlyList<CCS_SurvivalBootstrapProfileSlot> BootstrapProfileSlots => bootstrapProfileSlots;

        #endregion

        #region Private Methods

        private void RunSurvivalInstallPipeline()
        {
            CCS_SurvivalInstaller survivalInstaller = new CCS_SurvivalInstaller(survivalContext, enableDebugLogs);
            runtimeHost.BootstrapRunner.RegisterInstaller(survivalInstaller);
            runtimeHost.BootstrapRunner.Run(runtimeHost);
        }

        #endregion
    }
}
