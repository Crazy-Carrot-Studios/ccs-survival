using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_Validation
// CATEGORY: Core / Runtime / Utilities
// PURPOSE: Backward-compatible validation helpers delegating to CCS_CoreValidation.
// PLACEMENT: Static runtime utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Prefer CCS_CoreValidation for new Core code.
// =============================================================================

namespace CCS.Core
{
    public static class CCS_Validation
    {
        #region Public Methods

        public static bool IsObjectValid(object targetObject)
        {
            return CCS_CoreValidation.IsObjectValid(targetObject);
        }

        public static bool IsStringValid(string value)
        {
            return CCS_CoreValidation.IsStringValid(value);
        }

        public static bool IsCollectionValid<T>(ICollection<T> collection)
        {
            return CCS_CoreValidation.IsCollectionValid(collection);
        }

        public static CCS_Result ValidateObject(object targetObject, string objectName)
        {
            return CCS_CoreValidation.ValidateObject(targetObject, objectName);
        }

        public static CCS_Result ValidateString(string value, string fieldName)
        {
            return CCS_CoreValidation.ValidateString(value, fieldName);
        }

        #endregion
    }
}
