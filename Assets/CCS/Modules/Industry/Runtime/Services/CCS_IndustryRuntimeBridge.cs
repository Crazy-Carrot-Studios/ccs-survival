using CCS.Core;

namespace CCS.Modules.Industry
{
    public static class CCS_IndustryRuntimeBridge
    {
        private static CCS_IndustryService cachedService;

        public static void Register(CCS_IndustryService service)
        {
            cachedService = service;
        }

        public static void Unregister(CCS_IndustryService service)
        {
            if (cachedService == service)
            {
                cachedService = null;
            }
        }

        public static CCS_IndustryService ResolveService(CCS_RuntimeHost runtimeHost)
        {
            if (cachedService != null && cachedService.IsInitialized)
            {
                return cachedService;
            }

            if (runtimeHost == null)
            {
                return null;
            }

            return runtimeHost.ServiceRegistry != null
                && runtimeHost.ServiceRegistry.TryGetService(out CCS_IndustryService service)
                ? service
                : null;
        }
    }
}
