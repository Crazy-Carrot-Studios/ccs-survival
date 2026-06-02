using CCS.Modules.WorldResources;

// =============================================================================
// SCRIPT: CCS_WildlifeHarvestRequest
// CATEGORY: Modules / Wildlife / Runtime / Data
// PURPOSE: Request payload for wildlife harvest validation and execution.
// PLACEMENT: Built by CCS_HarvestableWildlife and passed to CCS_WildlifeHarvestService.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No combat kill state required in 0.9.3 foundation.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    public sealed class CCS_WildlifeHarvestRequest
    {
        #region Public Methods

        public CCS_WildlifeHarvestRequest(
            CCS_WildlifeDefinition wildlifeDefinition,
            CCS_WildlifeState wildlifeState,
            CCS_RequiredToolType equippedToolType,
            string instanceKey = "",
            bool isDeadCarcass = true)
        {
            WildlifeDefinition = wildlifeDefinition;
            WildlifeState = wildlifeState;
            EquippedToolType = equippedToolType;
            InstanceKey = instanceKey ?? string.Empty;
            IsDeadCarcass = isDeadCarcass;
        }

        #endregion

        #region Properties

        public CCS_WildlifeDefinition WildlifeDefinition { get; }

        public CCS_WildlifeState WildlifeState { get; }

        public CCS_RequiredToolType EquippedToolType { get; }

        public string InstanceKey { get; }

        public bool IsDeadCarcass { get; }

        #endregion
    }
}
