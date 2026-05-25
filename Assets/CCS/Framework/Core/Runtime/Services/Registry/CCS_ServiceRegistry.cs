using System;
using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_ServiceRegistry
// CATEGORY: Core / Runtime / Services
// PURPOSE: Lightweight runtime registry for CCS service registration and lookup.
// PLACEMENT: Instantiated by bootstrap code. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: No static singleton. No lifecycle ownership. No UnityEditor references.
// =============================================================================

namespace CCS.Core
{
    public sealed class CCS_ServiceRegistry : CCS_IServiceRegistry
    {
        private const string LogCategory = "Service Registry";

        #region Variables

        private readonly Dictionary<Type, CCS_IService> registeredServices;
        private readonly bool enableDebugLogs;

        #endregion

        #region Public Methods

        public CCS_ServiceRegistry()
            : this(false)
        {
        }

        public CCS_ServiceRegistry(bool enableDebugLogs)
        {
            registeredServices = new Dictionary<Type, CCS_IService>();
            this.enableDebugLogs = enableDebugLogs;
        }

        public bool RegisterService<TService>(TService service) where TService : class, CCS_IService
        {
            if (!CCS_CoreValidation.IsObjectValid(service))
            {
                return false;
            }

            Type serviceType = typeof(TService);
            if (registeredServices.ContainsKey(serviceType))
            {
                return false;
            }

            registeredServices.Add(serviceType, service);
            CCS_Logger.Log(LogCategory, $"Registered service: {serviceType.Name}", enableDebugLogs);
            return true;
        }

        public bool UnregisterService<TService>() where TService : class, CCS_IService
        {
            Type serviceType = typeof(TService);
            if (!registeredServices.Remove(serviceType))
            {
                return false;
            }

            CCS_Logger.Log(LogCategory, $"Unregistered service: {serviceType.Name}", enableDebugLogs);
            return true;
        }

        public bool TryGetService<TService>(out TService service) where TService : class, CCS_IService
        {
            Type serviceType = typeof(TService);
            if (registeredServices.TryGetValue(serviceType, out CCS_IService registeredService))
            {
                service = registeredService as TService;
                return service != null;
            }

            service = null;
            return false;
        }

        public bool HasService<TService>() where TService : class, CCS_IService
        {
            return registeredServices.ContainsKey(typeof(TService));
        }

        public bool HasServiceType(Type serviceType)
        {
            return serviceType != null && registeredServices.ContainsKey(serviceType);
        }

        public void Clear()
        {
            registeredServices.Clear();
            CCS_Logger.Log(LogCategory, "Cleared all registered services.", enableDebugLogs);
        }

        public int GetRegisteredServiceCount()
        {
            return registeredServices.Count;
        }

        public CCS_ServiceDiagnosticsInfo BuildDiagnosticsSnapshot()
        {
            if (registeredServices.Count == 0)
            {
                return new CCS_ServiceDiagnosticsInfo(0, Array.Empty<string>());
            }

            string[] serviceTypeNames = new string[registeredServices.Count];
            int index = 0;

            foreach (KeyValuePair<Type, CCS_IService> entry in registeredServices)
            {
                serviceTypeNames[index] = entry.Key.Name;
                index++;
            }

            return new CCS_ServiceDiagnosticsInfo(registeredServices.Count, serviceTypeNames);
        }

        #endregion
    }
}
