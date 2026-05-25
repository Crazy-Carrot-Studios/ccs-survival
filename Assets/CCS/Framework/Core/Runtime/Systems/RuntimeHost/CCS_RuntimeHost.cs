using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RuntimeHost
// CATEGORY: Core / Runtime / Systems
// PURPOSE: Thin Unity MonoBehaviour bridge into CCS runtime architecture.
// PLACEMENT: Attach to a root runtime GameObject in bootstrap scenes.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Orchestration only. No singleton. No DontDestroyOnLoad. No gameplay logic.
// =============================================================================

namespace CCS.Core
{
    public sealed class CCS_RuntimeHost : MonoBehaviour
    {
        private const string LogCategory = "Runtime Host";

        #region Variables

        [SerializeField] private bool enableDebugLogs = false;

        [Header("Diagnostics")]
        [Tooltip("When enabled, diagnostic bridge components may run validation logic during Play Mode.")]
        [SerializeField] private bool enableRuntimeDiagnostics = false;

        private CCS_RuntimeUpdateLoop runtimeUpdateLoop;
        private CCS_ServiceRegistry serviceRegistry;
        private CCS_EventDispatcher eventDispatcher;
        private CCS_BootstrapRunner bootstrapRunner;
        private CCS_ModuleHost moduleHost;
        private bool isRuntimeInitialized;
        private bool isRuntimeShutdown;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            isRuntimeShutdown = false;
            isRuntimeInitialized = true;
            runtimeUpdateLoop = new CCS_RuntimeUpdateLoop(enableDebugLogs);
            serviceRegistry = new CCS_ServiceRegistry(enableDebugLogs);
            eventDispatcher = new CCS_EventDispatcher(enableDebugLogs);
            bootstrapRunner = new CCS_BootstrapRunner(enableDebugLogs);
            moduleHost = new CCS_ModuleHost(enableDebugLogs);

            CCS_Logger.Log(LogCategory, "Runtime host initialized.", enableDebugLogs);
        }

        private void Update()
        {
            runtimeUpdateLoop.Tick(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            runtimeUpdateLoop.FixedTick(Time.fixedDeltaTime);
        }

        private void LateUpdate()
        {
            runtimeUpdateLoop.LateTick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            isRuntimeShutdown = true;

            if (runtimeUpdateLoop != null)
            {
                runtimeUpdateLoop.Clear();
            }

            if (serviceRegistry != null)
            {
                serviceRegistry.Clear();
            }

            if (eventDispatcher != null)
            {
                eventDispatcher.Clear();
            }

            if (bootstrapRunner != null)
            {
                bootstrapRunner.Clear();
            }

            if (moduleHost != null)
            {
                moduleHost.Clear();
            }

            CCS_Logger.Log(LogCategory, "Runtime host shutdown.", enableDebugLogs);
        }

        #endregion

        #region Properties

        public CCS_RuntimeUpdateLoop RuntimeUpdateLoop => runtimeUpdateLoop;

        public CCS_ServiceRegistry ServiceRegistry => serviceRegistry;

        public CCS_EventDispatcher EventDispatcher => eventDispatcher;

        public CCS_BootstrapRunner BootstrapRunner => bootstrapRunner;

        public CCS_ModuleHost ModuleHost => moduleHost;

        public bool EnableRuntimeDiagnostics => enableRuntimeDiagnostics;

        public bool IsRuntimeInitialized => isRuntimeInitialized;

        public bool IsRuntimeShutdown => isRuntimeShutdown;

        #endregion

        #region Diagnostics

        public CCS_CoreDiagnosticsReport BuildDiagnosticsReport()
        {
            CCS_ModuleDiagnosticsInfo[] moduleDiagnostics = moduleHost != null
                ? moduleHost.BuildModuleDiagnosticsSnapshot()
                : System.Array.Empty<CCS_ModuleDiagnosticsInfo>();

            CCS_ServiceDiagnosticsInfo serviceDiagnostics = serviceRegistry != null
                ? serviceRegistry.BuildDiagnosticsSnapshot()
                : new CCS_ServiceDiagnosticsInfo(0, System.Array.Empty<string>());

            CCS_UpdateLoopDiagnosticsInfo updateLoopDiagnostics = runtimeUpdateLoop != null
                ? runtimeUpdateLoop.BuildDiagnosticsSnapshot()
                : new CCS_UpdateLoopDiagnosticsInfo(0, 0, 0);

            int eventSubscriptionCount = eventDispatcher != null
                ? eventDispatcher.GetSubscriptionCount()
                : 0;

            int bootstrapInstallerCount = bootstrapRunner != null
                ? bootstrapRunner.GetInstallerCount()
                : 0;

            return new CCS_CoreDiagnosticsReport(
                isRuntimeInitialized,
                isRuntimeShutdown,
                moduleDiagnostics.Length,
                moduleDiagnostics,
                serviceDiagnostics,
                updateLoopDiagnostics,
                eventSubscriptionCount,
                bootstrapInstallerCount);
        }

        #endregion
    }
}
