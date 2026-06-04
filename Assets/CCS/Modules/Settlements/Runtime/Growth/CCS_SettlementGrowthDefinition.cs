using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementGrowthDefinition
// CATEGORY: Modules / Settlements / Runtime / Growth
// PURPOSE: ScriptableObject requirements for reaching a settlement growth stage.
// PLACEMENT: Assets/CCS/Survival/Content/Settlements/Growth/
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.6.0 — minimum population gates active growth stages.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [CreateAssetMenu(
        fileName = "CCS_SettlementGrowthDefinition",
        menuName = "CCS/Survival/Settlements/Settlement Growth Definition")]
    public sealed class CCS_SettlementGrowthDefinition : ScriptableObject
    {
        [SerializeField] private string growthDefinitionId = string.Empty;

        [SerializeField] private CCS_SettlementGrowthStage growthStage = CCS_SettlementGrowthStage.Outpost;

        [SerializeField] private bool isActive = true;

        [SerializeField] private float minimumProsperity;

        [SerializeField] private float minimumFoodSupplyPercent;

        [SerializeField] private float minimumIndustrialSupplyPercent;

        [SerializeField] private int minimumCompletedContracts;

        [Tooltip("Minimum total population required to reach this growth stage.")]
        [SerializeField] private int minimumPopulation;

        [SerializeField] private bool requiresRegionDiscovered;

        [SerializeField] private string requiredRegionId = string.Empty;

        public string GrowthDefinitionId => growthDefinitionId ?? string.Empty;

        public CCS_SettlementGrowthStage GrowthStage => growthStage;

        public bool IsActive => isActive;

        public float MinimumProsperity => minimumProsperity < 0f ? 0f : minimumProsperity;

        public float MinimumFoodSupplyPercent => Mathf.Clamp(minimumFoodSupplyPercent, 0f, 100f);

        public float MinimumIndustrialSupplyPercent => Mathf.Clamp(minimumIndustrialSupplyPercent, 0f, 100f);

        public int MinimumCompletedContracts => minimumCompletedContracts < 0 ? 0 : minimumCompletedContracts;

        public int MinimumPopulation => minimumPopulation < 0 ? 0 : minimumPopulation;

        public bool RequiresRegionDiscovered => requiresRegionDiscovered;

        public string RequiredRegionId => requiredRegionId ?? string.Empty;
    }
}
