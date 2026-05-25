using System;

// =============================================================================
// SCRIPT: CCS_ModuleMetadata
// CATEGORY: Core / Runtime / Modules / Data
// PURPOSE: Standard lightweight identity information for future framework modules.
// PLACEMENT: Runtime assembly contract. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Immutable readonly struct. No gameplay coupling.
// =============================================================================

namespace CCS.Core
{
    [Serializable]
    public readonly struct CCS_ModuleMetadata
    {
        #region Properties

        public string ModuleId { get; }

        public string DisplayName { get; }

        public string Version { get; }

        public string Description { get; }

        #endregion

        #region Public Methods

        public CCS_ModuleMetadata(string moduleId, string displayName, string version, string description)
        {
            ModuleId = moduleId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            Version = version ?? string.Empty;
            Description = description ?? string.Empty;
        }

        public override string ToString()
        {
            return $"[CCS_Module] {ModuleId} ({DisplayName}) v{Version}";
        }

        #endregion
    }
}
