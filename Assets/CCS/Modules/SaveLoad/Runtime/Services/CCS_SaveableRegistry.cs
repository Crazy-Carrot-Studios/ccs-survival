using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_SaveableRegistry
// CATEGORY: Modules / SaveLoad / Runtime / Services
// PURPOSE: Tracks registered saveable systems for capture and restore passes.
// PLACEMENT: Owned by CCS_SaveLoadService. Future modules register at runtime.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Instance-owned registry. No static global service locator.
// =============================================================================

namespace CCS.Modules.SaveLoad
{
    public sealed class CCS_SaveableRegistry
    {
        #region Variables

        private readonly Dictionary<string, CCS_ISaveable> saveablesById =
            new Dictionary<string, CCS_ISaveable>();

        #endregion

        #region Public Methods

        public bool RegisterSaveable(CCS_ISaveable saveable)
        {
            if (saveable == null || string.IsNullOrWhiteSpace(saveable.SaveableId))
            {
                return false;
            }

            saveablesById[saveable.SaveableId] = saveable;
            return true;
        }

        public bool UnregisterSaveable(CCS_ISaveable saveable)
        {
            if (saveable == null || string.IsNullOrWhiteSpace(saveable.SaveableId))
            {
                return false;
            }

            return saveablesById.Remove(saveable.SaveableId);
        }

        public bool TryGetSaveable(string saveableId, out CCS_ISaveable saveable)
        {
            saveable = null;
            if (string.IsNullOrWhiteSpace(saveableId))
            {
                return false;
            }

            return saveablesById.TryGetValue(saveableId, out saveable) && saveable != null;
        }

        public Dictionary<string, string> CaptureAllModuleStates()
        {
            Dictionary<string, string> moduleData = new Dictionary<string, string>();
            foreach (KeyValuePair<string, CCS_ISaveable> pair in saveablesById)
            {
                CCS_ISaveable saveable = pair.Value;
                if (saveable == null)
                {
                    continue;
                }

                moduleData[pair.Key] = saveable.CaptureState() ?? string.Empty;
            }

            return moduleData;
        }

        public void RestoreAllModuleStates(IReadOnlyDictionary<string, string> moduleData)
        {
            if (moduleData == null)
            {
                return;
            }

            foreach (KeyValuePair<string, string> pair in moduleData)
            {
                if (!TryGetSaveable(pair.Key, out CCS_ISaveable saveable))
                {
                    continue;
                }

                saveable.RestoreState(pair.Value ?? string.Empty);
            }
        }

        public int RegisteredSaveableCount => saveablesById.Count;

        #endregion
    }
}
