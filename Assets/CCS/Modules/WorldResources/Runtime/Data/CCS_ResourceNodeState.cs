// =============================================================================
// SCRIPT: CCS_ResourceNodeState
// CATEGORY: Modules / WorldResources / Runtime / Data
// PURPOSE: Mutable runtime state for a harvestable resource node instance.
// PLACEMENT: Owned by CCS_HarvestableResource and updated by harvest/respawn services.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Tracks remaining harvests, depletion, and respawn timer placeholder.
// =============================================================================

namespace CCS.Modules.WorldResources
{
    public sealed class CCS_ResourceNodeState
    {
        #region Public Methods

        public static CCS_ResourceNodeState CreateFromDefinition(CCS_ResourceDefinition resourceDefinition)
        {
            int maxHarvestCount = resourceDefinition != null ? resourceDefinition.MaxHarvestCount : 0;
            if (maxHarvestCount < 0)
            {
                maxHarvestCount = 0;
            }

            return new CCS_ResourceNodeState(maxHarvestCount);
        }

        public CCS_ResourceNodeState(int remainingHarvests)
        {
            RemainingHarvests = remainingHarvests < 0 ? 0 : remainingHarvests;
            RespawnRemainingSeconds = 0f;
        }

        public void ResetFromDefinition(CCS_ResourceDefinition resourceDefinition)
        {
            RemainingHarvests = resourceDefinition != null ? resourceDefinition.MaxHarvestCount : 0;
            if (RemainingHarvests < 0)
            {
                RemainingHarvests = 0;
            }

            RespawnRemainingSeconds = 0f;
        }

        public void ConsumeHarvest()
        {
            if (RemainingHarvests > 0)
            {
                RemainingHarvests--;
            }
        }

        #endregion

        #region Properties

        public int RemainingHarvests { get; private set; }

        public bool IsDepleted => RemainingHarvests <= 0;

        public float RespawnRemainingSeconds { get; set; }

        #endregion
    }
}
