using CCS.Core;
using CCS.Survival;

namespace CCS.Modules.Firearms
{
    public static class CCS_FirearmRuntimeBridge
    {
        private static CCS_FirearmService registeredService;

        public static void Register(CCS_FirearmService service)
        {
            registeredService = service;
        }

        public static bool TryGetFirearmService(out CCS_FirearmService service)
        {
            service = registeredService;
            if (service != null && service.IsInitialized)
            {
                return true;
            }

            CCS_RuntimeHost[] runtimeHosts = CCS_SurvivalSceneQueryUtility.FindActiveObjectsByType<CCS_RuntimeHost>();
            if (runtimeHosts != null
                && runtimeHosts.Length > 0
                && runtimeHosts[0]?.ServiceRegistry != null
                && runtimeHosts[0].ServiceRegistry.TryGetService(out service)
                && service != null
                && service.IsInitialized)
            {
                registeredService = service;
                return true;
            }

            service = null;
            return false;
        }
    }
}
