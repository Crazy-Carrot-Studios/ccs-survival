// =============================================================================
// SCRIPT: CCS_WildlifeState
// CATEGORY: Modules / Wildlife / Runtime / Data
// PURPOSE: Mutable runtime state for a harvestable wildlife instance.
// PLACEMENT: Owned by CCS_HarvestableWildlife and updated by harvest service.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No AI state in 0.9.3 foundation.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    public sealed class CCS_WildlifeState
    {
        #region Public Methods

        public static CCS_WildlifeState CreateFromDefinition(CCS_WildlifeDefinition wildlifeDefinition)
        {
            int maxHarvestCount = wildlifeDefinition != null ? wildlifeDefinition.MaxHarvestCount : 0;
            if (maxHarvestCount < 0)
            {
                maxHarvestCount = 0;
            }

            return new CCS_WildlifeState(maxHarvestCount);
        }

        public CCS_WildlifeState(int remainingHarvests)
        {
            RemainingHarvests = remainingHarvests < 0 ? 0 : remainingHarvests;
            LastHarvestMessage = string.Empty;
        }

        public void ResetFromDefinition(CCS_WildlifeDefinition wildlifeDefinition)
        {
            RemainingHarvests = wildlifeDefinition != null ? wildlifeDefinition.MaxHarvestCount : 0;
            if (RemainingHarvests < 0)
            {
                RemainingHarvests = 0;
            }

            LastHarvestMessage = string.Empty;
        }

        public void ConsumeHarvest(string message)
        {
            if (RemainingHarvests > 0)
            {
                RemainingHarvests--;
            }

            LastHarvestMessage = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public int RemainingHarvests { get; private set; }

        public bool IsDepleted => RemainingHarvests <= 0;

        public string LastHarvestMessage { get; private set; }

        #endregion
    }
}
