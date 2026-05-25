using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_CoreValidation
// CATEGORY: Core / Runtime / Utilities / Validation
// PURPOSE: Centralized validation helpers for CCS Core runtime systems.
// PLACEMENT: Static runtime utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: No exceptions or logging. Returns CCS_Result for expected validation failures.
// =============================================================================

namespace CCS.Core
{
    public static class CCS_CoreValidation
    {
        #region Public Methods

        public static bool IsValidId(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        public static CCS_Result ValidateId(string value, string label)
        {
            if (IsValidId(value))
            {
                return CCS_Result.Success();
            }

            return CCS_Result.Failure(CCS_CoreErrorCode.ValidationFailed, $"{label} is null or empty.");
        }

        public static CCS_Result ValidateNotNull(object value, string label)
        {
            if (value != null)
            {
                return CCS_Result.Success();
            }

            return CCS_Result.Failure(CCS_CoreErrorCode.ValidationFailed, $"{label} is null.");
        }

        public static CCS_Result ValidateRuntimeHost(CCS_RuntimeHost runtimeHost)
        {
            if (runtimeHost != null)
            {
                return CCS_Result.Success();
            }

            return CCS_Result.Failure(CCS_CoreErrorCode.NullRuntimeHost, "runtimeHost is null.");
        }

        public static CCS_Result ValidateModule(CCS_IModule module)
        {
            if (module == null)
            {
                return CCS_Result.Failure(CCS_CoreErrorCode.NullModule, "module is null.");
            }

            return ValidateModuleId(module.Metadata.ModuleId);
        }

        public static CCS_Result ValidateModuleId(string moduleId)
        {
            if (!IsValidId(moduleId))
            {
                return CCS_Result.Failure(CCS_CoreErrorCode.InvalidModuleId, "moduleId is null or empty.");
            }

            return CCS_Result.Success();
        }

        public static bool IsObjectValid(object targetObject)
        {
            return targetObject != null;
        }

        public static bool IsStringValid(string value)
        {
            return IsValidId(value);
        }

        public static bool IsCollectionValid<T>(ICollection<T> collection)
        {
            return collection != null && collection.Count > 0;
        }

        public static CCS_Result ValidateObject(object targetObject, string objectName)
        {
            return ValidateNotNull(targetObject, objectName);
        }

        public static CCS_Result ValidateString(string value, string fieldName)
        {
            return ValidateId(value, fieldName);
        }

        #endregion
    }
}
