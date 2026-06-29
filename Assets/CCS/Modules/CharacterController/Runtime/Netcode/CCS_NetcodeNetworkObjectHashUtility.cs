using System.Reflection;
using Unity.Netcode;

// =============================================================================
// SCRIPT: CCS_NetcodeNetworkObjectHashUtility
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Reads NetworkObject global object id hashes for registry diagnostics.
// PLACEMENT: Runtime utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-19
// NOTES: NGO stores the hash as a serialized field, not a public runtime property.
// =============================================================================

namespace CCS.Modules.CharacterController.Netcode
{
    public static class CCS_NetcodeNetworkObjectHashUtility
    {
        #region Public Methods

        public static uint GetHash(NetworkObject networkObject)
        {
            if (networkObject == null)
            {
                return 0u;
            }

            FieldInfo hashField = typeof(NetworkObject).GetField(
                "GlobalObjectIdHash",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (hashField != null && hashField.FieldType == typeof(uint))
            {
                return (uint)hashField.GetValue(networkObject);
            }

            PropertyInfo hashPropertyInfo = typeof(NetworkObject).GetProperty(
                "GlobalObjectIdHash",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (hashPropertyInfo != null && hashPropertyInfo.PropertyType == typeof(uint))
            {
                return (uint)hashPropertyInfo.GetValue(networkObject);
            }

            return 0u;
        }

        #endregion
    }
}
