using System;

// =============================================================================
// SCRIPT: CCS_SaveModuleDataEntry
// CATEGORY: Modules / SaveLoad / Runtime / Data
// PURPOSE: Serializable key/value entry for module-owned save payloads.
// PLACEMENT: Serialized inside CCS_SaveGameData for JsonUtility compatibility.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Replaces Dictionary serialization until a shared serializer is introduced.
// =============================================================================

namespace CCS.Modules.SaveLoad
{
    [Serializable]
    public sealed class CCS_SaveModuleDataEntry
    {
        #region Variables

        public string moduleId = string.Empty;

        public string payloadJson = string.Empty;

        #endregion
    }
}
