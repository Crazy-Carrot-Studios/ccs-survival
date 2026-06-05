using System;
using CCS.Modules.NPCs;
using CCS.Modules.Regions;
using CCS.Modules.Settlements;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WorldSimulationProfile
// CATEGORY: Modules / WorldSimulation / Runtime / Profiles
// PURPOSE: Profile catalog for settlement and region simulation defaults.
// PLACEMENT: Assets/CCS/Survival/Profiles/WorldSimulation/
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Registered on CCS_SurvivalGameplayServiceHost.
// =============================================================================

namespace CCS.Modules.WorldSimulation
{
    [CreateAssetMenu(
        fileName = "CCS_WorldSimulationProfile",
        menuName = "CCS/Survival/WorldSimulation/World Simulation Profile")]
    public sealed class CCS_WorldSimulationProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_WorldSimulationSettlementProfileEntry[] settlementEntries =
            Array.Empty<CCS_WorldSimulationSettlementProfileEntry>();

        [SerializeField] private CCS_WorldSimulationRegionProfileEntry[] regionEntries =
            Array.Empty<CCS_WorldSimulationRegionProfileEntry>();

        [SerializeField] private CCS_WorldSimulationVendorRouteEntry[] vendorRoutes =
            Array.Empty<CCS_WorldSimulationVendorRouteEntry>();

        [SerializeField] private bool enableDebugLogging = true;

        [SerializeField] private CCS_SettlementGrowthProfile settlementGrowthProfile;

        [SerializeField] private CCS_SettlementPopulationProfile settlementPopulationProfile;

        [SerializeField] private CCS_BusinessProfile settlementBusinessProfile;

        [SerializeField] private CCS_BusinessPresenceProfile settlementBusinessPresenceProfile;

        [SerializeField] private CCS_SettlementVisualGrowthProfile settlementVisualGrowthProfile;

        [SerializeField] private CCS_PopulationPresenceProfile settlementPopulationPresenceProfile;

        [SerializeField] private CCS_NpcIdentityProfile settlementNpcIdentityProfile;

        public CCS_SettlementGrowthProfile SettlementGrowthProfile => settlementGrowthProfile;

        public CCS_SettlementPopulationProfile SettlementPopulationProfile => settlementPopulationProfile;

        public CCS_BusinessProfile SettlementBusinessProfile => settlementBusinessProfile;

        public CCS_BusinessPresenceProfile SettlementBusinessPresenceProfile => settlementBusinessPresenceProfile;

        public CCS_SettlementVisualGrowthProfile SettlementVisualGrowthProfile => settlementVisualGrowthProfile;

        public CCS_PopulationPresenceProfile SettlementPopulationPresenceProfile =>
            settlementPopulationPresenceProfile;

        public CCS_NpcIdentityProfile SettlementNpcIdentityProfile => settlementNpcIdentityProfile;

        public CCS_WorldSimulationSettlementProfileEntry[] SettlementEntries =>
            settlementEntries ?? Array.Empty<CCS_WorldSimulationSettlementProfileEntry>();

        public CCS_WorldSimulationRegionProfileEntry[] RegionEntries =>
            regionEntries ?? Array.Empty<CCS_WorldSimulationRegionProfileEntry>();

        public CCS_WorldSimulationVendorRouteEntry[] VendorRoutes =>
            vendorRoutes ?? Array.Empty<CCS_WorldSimulationVendorRouteEntry>();

        public bool EnableDebugLogging => enableDebugLogging;
    }

    [Serializable]
    public sealed class CCS_WorldSimulationSettlementProfileEntry
    {
        public string settlementId = string.Empty;
        public string regionId = string.Empty;
        public int population = 25;
        public CCS_SettlementSupplyEntry[] supplies = Array.Empty<CCS_SettlementSupplyEntry>();
        public CCS_SettlementDemandEntry[] demands = Array.Empty<CCS_SettlementDemandEntry>();
        public CCS_SettlementProductionEntry[] productions = Array.Empty<CCS_SettlementProductionEntry>();
    }

    [Serializable]
    public sealed class CCS_WorldSimulationRegionProfileEntry
    {
        public string regionId = string.Empty;
        public int specializationType;
        public float foodPotential;
        public float wildlifePotential;
        public float miningPotential;
        public float industryPotential;
        public float productionBonus = 1f;
        public float prosperityModifier = 1f;
        public int[] preferredContractCategories = Array.Empty<int>();
    }

    [Serializable]
    public sealed class CCS_WorldSimulationVendorRouteEntry
    {
        public string vendorId = string.Empty;
        public string settlementId = string.Empty;
    }
}
