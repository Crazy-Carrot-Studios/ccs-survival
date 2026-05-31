using System;
using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_SaveGameData
// CATEGORY: Modules / SaveLoad / Runtime / Data
// PURPOSE: Root JSON save document for a single save slot.
// PLACEMENT: Serialized to Application.persistentDataPath by CCS_SaveLoadService.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Module payloads stored in moduleDataEntries for JsonUtility compatibility.
// =============================================================================

namespace CCS.Modules.SaveLoad
{
    [Serializable]
    public sealed class CCS_SaveGameData
    {
        #region Variables

        public string saveId = string.Empty;

        public string slotId = string.Empty;

        public string timestampUtc = string.Empty;

        public string version = string.Empty;

        public string profileVersion = string.Empty;

        public string playerDataJson = string.Empty;

        public List<CCS_SaveModuleDataEntry> moduleDataEntries = new List<CCS_SaveModuleDataEntry>();

        #endregion

        #region Properties

        public string SaveId
        {
            get => saveId;
            set => saveId = value ?? string.Empty;
        }

        public string SlotId
        {
            get => slotId;
            set => slotId = value ?? string.Empty;
        }

        public string TimestampUtc
        {
            get => timestampUtc;
            set => timestampUtc = value ?? string.Empty;
        }

        public string Version
        {
            get => version;
            set => version = value ?? string.Empty;
        }

        public string ProfileVersion
        {
            get => profileVersion;
            set => profileVersion = value ?? string.Empty;
        }

        public string PlayerDataJson
        {
            get => playerDataJson;
            set => playerDataJson = value ?? string.Empty;
        }

        #endregion

        #region Public Methods

        public Dictionary<string, string> GetModuleDataDictionary()
        {
            Dictionary<string, string> moduleData = new Dictionary<string, string>();
            if (moduleDataEntries == null)
            {
                return moduleData;
            }

            for (int i = 0; i < moduleDataEntries.Count; i++)
            {
                CCS_SaveModuleDataEntry entry = moduleDataEntries[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.moduleId))
                {
                    continue;
                }

                moduleData[entry.moduleId] = entry.payloadJson ?? string.Empty;
            }

            return moduleData;
        }

        public void SetModuleDataDictionary(IReadOnlyDictionary<string, string> moduleData)
        {
            moduleDataEntries = new List<CCS_SaveModuleDataEntry>();
            if (moduleData == null)
            {
                return;
            }

            foreach (KeyValuePair<string, string> pair in moduleData)
            {
                if (string.IsNullOrWhiteSpace(pair.Key))
                {
                    continue;
                }

                moduleDataEntries.Add(new CCS_SaveModuleDataEntry
                {
                    moduleId = pair.Key,
                    payloadJson = pair.Value ?? string.Empty
                });
            }
        }

        #endregion
    }
}
