using System;
using System.Collections.Generic;
using CCS.Modules.Economy;
using CCS.Modules.Regions;
using CCS.Modules.Settlements;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WorldSimulationService
// CATEGORY: Modules / WorldSimulation / Runtime / Services
// PURPOSE: Tracks settlement supply, demand, production, and region metadata state.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.0.0 frontier world simulation foundation.
// =============================================================================

namespace CCS.Modules.WorldSimulation
{
    public sealed class CCS_WorldSimulationService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_WorldSimulationService]";

        private readonly Dictionary<string, CCS_SettlementSimulationState> settlementLookup =
            new Dictionary<string, CCS_SettlementSimulationState>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, CCS_RegionSimulationState> regionLookup =
            new Dictionary<string, CCS_RegionSimulationState>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, string> vendorRouteLookup =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private CCS_WorldSimulationProfile activeProfile;
        private CCS_SettlementGrowthProfile settlementGrowthProfile;
        private CCS_SettlementService settlementService;
        private CCS_RegionService regionService;
        private bool isInitialized;

        public event Action<CCS_SettlementSimulationState> SettlementSupplyChanged;
        public event Action<CCS_SettlementSimulationState> SettlementProsperityChanged;
        public event Action<CCS_SettlementGrowthChangedEventArgs> SettlementGrowthChanged;

        public bool IsInitialized => isInitialized;

        public CCS_WorldSimulationProfile ActiveProfile => activeProfile;

        public CCS_SettlementGrowthProfile SettlementGrowthProfile => settlementGrowthProfile;

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_WorldSimulationProfile profile)
        {
            activeProfile = profile;
            settlementGrowthProfile = profile?.SettlementGrowthProfile;
            settlementLookup.Clear();
            regionLookup.Clear();
            vendorRouteLookup.Clear();

            if (profile == null)
            {
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_WorldSimulationValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            CCS_WorldSimulationSettlementProfileEntry[] settlementEntries = profile.SettlementEntries;
            for (int index = 0; index < settlementEntries.Length; index++)
            {
                CCS_WorldSimulationSettlementProfileEntry entry = settlementEntries[index];
                if (entry == null || string.IsNullOrWhiteSpace(entry.settlementId))
                {
                    continue;
                }

                settlementLookup[entry.settlementId] = CreateSettlementStateFromProfile(entry);
            }

            CCS_WorldSimulationRegionProfileEntry[] regionEntries = profile.RegionEntries;
            for (int index = 0; index < regionEntries.Length; index++)
            {
                CCS_WorldSimulationRegionProfileEntry entry = regionEntries[index];
                if (entry == null || string.IsNullOrWhiteSpace(entry.regionId))
                {
                    continue;
                }

                regionLookup[entry.regionId] = CreateRegionStateFromProfile(entry);
            }

            CCS_WorldSimulationVendorRouteEntry[] vendorRoutes = profile.VendorRoutes;
            for (int index = 0; index < vendorRoutes.Length; index++)
            {
                CCS_WorldSimulationVendorRouteEntry route = vendorRoutes[index];
                if (route == null
                    || string.IsNullOrWhiteSpace(route.vendorId)
                    || string.IsNullOrWhiteSpace(route.settlementId))
                {
                    continue;
                }

                vendorRouteLookup[route.vendorId] = route.settlementId;
            }

            SyncDiscoveryFromGameplayServices();
            SyncRegionalEconomyMetadata();
            RecalculateAllProsperity();
            EvaluateAllSettlementGrowth();
            isInitialized = true;
        }

        public void BindGameplayServices(CCS_SettlementService settlements, CCS_RegionService regions)
        {
            UnbindGameplayServices();
            settlementService = settlements;
            regionService = regions;

            if (settlementService != null)
            {
                settlementService.SettlementDiscovered += HandleSettlementDiscovered;
            }

            if (regionService != null)
            {
                regionService.RegionDiscovered += HandleRegionDiscovered;
            }

            SyncDiscoveryFromGameplayServices();
        }

        public void UnbindGameplayServices()
        {
            if (settlementService != null)
            {
                settlementService.SettlementDiscovered -= HandleSettlementDiscovered;
            }

            if (regionService != null)
            {
                regionService.RegionDiscovered -= HandleRegionDiscovered;
            }

            settlementService = null;
            regionService = null;
        }

        public void HandleVendorTransactionCompleted(CCS_VendorTransactionResult transaction)
        {
            if (!isInitialized || transaction == null || !transaction.IsSuccess)
            {
                return;
            }

            if (!TryResolveSettlementForVendor(transaction.VendorId, out string settlementId))
            {
                return;
            }

            if (!TryGetSettlementState(settlementId, out CCS_SettlementSimulationState settlementState)
                || settlementState == null
                || !settlementState.isDiscovered)
            {
                return;
            }

            if (!CCS_WorldSimulationValidationUtility.TryResolveSupplyImpact(
                    transaction,
                    out CCS_SettlementSupplyType supplyType,
                    out float amountDelta))
            {
                return;
            }

            ApplySupplyDelta(settlementState, supplyType, amountDelta);

            if (transaction.WasSell
                && IsFuelOrBuildingItem(transaction.ItemId)
                && supplyType == CCS_SettlementSupplyType.BuildingMaterials)
            {
                ApplySupplyDelta(settlementState, CCS_SettlementSupplyType.Fuel, amountDelta * 0.5f);
            }

            RecalculateProsperity(settlementState);
            EvaluateSettlementGrowth(settlementState);
            SettlementSupplyChanged?.Invoke(settlementState);
        }

        public void HandleContractCompleted(
            string settlementId,
            CCS_SettlementSupplyType supplyType,
            float supplyAmount,
            float prosperityBonus)
        {
            if (!isInitialized || string.IsNullOrWhiteSpace(settlementId) || supplyAmount <= 0f)
            {
                return;
            }

            if (!TryGetSettlementState(settlementId, out CCS_SettlementSimulationState settlementState)
                || settlementState == null
                || !settlementState.isDiscovered)
            {
                return;
            }

            ApplySupplyDelta(settlementState, supplyType, supplyAmount);
            settlementState.completedContractsCount++;
            RecalculateProsperity(settlementState);
            if (prosperityBonus > 0f)
            {
                settlementState.prosperity = Mathf.Clamp(settlementState.prosperity + prosperityBonus, 0f, 100f);
                SettlementProsperityChanged?.Invoke(settlementState);
            }

            EvaluateSettlementGrowth(settlementState);
            SettlementSupplyChanged?.Invoke(settlementState);
        }

        public bool TryGetGrowthSnapshot(string settlementId, out CCS_SettlementGrowthSnapshot snapshot)
        {
            snapshot = CCS_SettlementGrowthSnapshot.Empty;
            if (!TryGetSettlementState(settlementId, out CCS_SettlementSimulationState settlementState)
                || settlementState == null)
            {
                return false;
            }

            snapshot = BuildGrowthSnapshot(settlementState);
            return snapshot.IsValid;
        }

        public bool TryGetSnapshot(out CCS_WorldSimulationSnapshot snapshot)
        {
            snapshot = BuildSnapshot();
            return snapshot != null;
        }

        public bool TryGetSettlementState(string settlementId, out CCS_SettlementSimulationState state)
        {
            state = null;
            if (string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            return settlementLookup.TryGetValue(settlementId, out state) && state != null;
        }

        public bool TryGetRegionState(string regionId, out CCS_RegionSimulationState state)
        {
            state = null;
            if (string.IsNullOrWhiteSpace(regionId))
            {
                return false;
            }

            return regionLookup.TryGetValue(regionId, out state) && state != null;
        }

        public float GetSupplyAmount(string settlementId, CCS_SettlementSupplyType supplyType)
        {
            if (!TryGetSettlementState(settlementId, out CCS_SettlementSimulationState state))
            {
                return 0f;
            }

            return GetSupplyEntry(state, supplyType)?.currentAmount ?? 0f;
        }

        public CCS_SettlementSimulationState[] CaptureState()
        {
            if (settlementLookup.Count == 0 && regionLookup.Count == 0)
            {
                return Array.Empty<CCS_SettlementSimulationState>();
            }

            CCS_SettlementSimulationState[] settlements = new CCS_SettlementSimulationState[settlementLookup.Count];
            int writeIndex = 0;
            foreach (KeyValuePair<string, CCS_SettlementSimulationState> pair in settlementLookup)
            {
                settlements[writeIndex++] = CloneSettlementState(pair.Value);
            }

            return settlements;
        }

        public CCS_RegionSimulationState[] CaptureRegionState()
        {
            if (regionLookup.Count == 0)
            {
                return Array.Empty<CCS_RegionSimulationState>();
            }

            CCS_RegionSimulationState[] regions = new CCS_RegionSimulationState[regionLookup.Count];
            int writeIndex = 0;
            foreach (KeyValuePair<string, CCS_RegionSimulationState> pair in regionLookup)
            {
                regions[writeIndex++] = CloneRegionState(pair.Value);
            }

            return regions;
        }

        public void RestoreState(CCS_SettlementSimulationState[] settlementStates, CCS_RegionSimulationState[] regionStates)
        {
            if (settlementStates != null)
            {
                for (int index = 0; index < settlementStates.Length; index++)
                {
                    CCS_SettlementSimulationState source = settlementStates[index];
                    if (source == null || string.IsNullOrWhiteSpace(source.settlementId))
                    {
                        continue;
                    }

                    settlementLookup[source.settlementId] = CloneSettlementState(source);
                }
            }

            if (regionStates != null)
            {
                for (int index = 0; index < regionStates.Length; index++)
                {
                    CCS_RegionSimulationState source = regionStates[index];
                    if (source == null || string.IsNullOrWhiteSpace(source.regionId))
                    {
                        continue;
                    }

                    regionLookup[source.regionId] = CloneRegionState(source);
                }
            }

            SyncDiscoveryFromGameplayServices();
            SyncRegionalEconomyMetadata();
            RecalculateAllProsperity();
            EvaluateAllSettlementGrowth();
        }

        private void SyncRegionalEconomyMetadata()
        {
            foreach (KeyValuePair<string, CCS_RegionSimulationState> pair in regionLookup)
            {
                CCS_RegionSimulationState state = pair.Value;
                if (state == null || string.IsNullOrWhiteSpace(state.regionId))
                {
                    continue;
                }

                if (regionService != null && regionService.IsInitialized)
                {
                    regionService.ApplyRegionalEconomyMetadata(
                        state.regionId,
                        (CCS_RegionSpecializationType)state.dominantIndustry,
                        state.foodSupplyStrength,
                        state.industrialSupplyStrength,
                        state.buildingSupplyStrength,
                        state.tradeSupplyStrength);
                }
            }
        }

        private void HandleSettlementDiscovered(CCS_SettlementSnapshot snapshot)
        {
            if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.SettlementId))
            {
                return;
            }

            if (TryGetSettlementState(snapshot.SettlementId, out CCS_SettlementSimulationState state))
            {
                state.isDiscovered = true;
            }
        }

        private void HandleRegionDiscovered(CCS_RegionSnapshot snapshot)
        {
            if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.RegionId))
            {
                return;
            }

            if (TryGetRegionState(snapshot.RegionId, out CCS_RegionSimulationState state))
            {
                state.isDiscovered = true;
                if (regionService != null && regionService.IsInitialized)
                {
                    regionService.ApplyRegionalEconomyMetadata(
                        snapshot.RegionId,
                        (CCS_RegionSpecializationType)state.dominantIndustry,
                        state.foodSupplyStrength,
                        state.industrialSupplyStrength,
                        state.buildingSupplyStrength,
                        state.tradeSupplyStrength);
                }
            }
        }

        private void SyncDiscoveryFromGameplayServices()
        {
            foreach (KeyValuePair<string, CCS_SettlementSimulationState> pair in settlementLookup)
            {
                CCS_SettlementSimulationState state = pair.Value;
                if (state == null)
                {
                    continue;
                }

                state.isDiscovered = settlementService != null
                    && settlementService.IsInitialized
                    && settlementService.IsDiscovered(state.settlementId);
            }

            foreach (KeyValuePair<string, CCS_RegionSimulationState> pair in regionLookup)
            {
                CCS_RegionSimulationState state = pair.Value;
                if (state == null)
                {
                    continue;
                }

                state.isDiscovered = regionService != null
                    && regionService.IsInitialized
                    && regionService.IsDiscovered(state.regionId);
            }
        }

        private bool TryResolveSettlementForVendor(string vendorId, out string settlementId)
        {
            settlementId = string.Empty;
            if (string.IsNullOrWhiteSpace(vendorId))
            {
                return false;
            }

            if (vendorRouteLookup.TryGetValue(vendorId, out settlementId))
            {
                return !string.IsNullOrWhiteSpace(settlementId);
            }

            return false;
        }

        private static void ApplySupplyDelta(
            CCS_SettlementSimulationState settlementState,
            CCS_SettlementSupplyType supplyType,
            float amountDelta)
        {
            CCS_SettlementSupplyEntry entry = GetOrCreateSupplyEntry(settlementState, supplyType);
            entry.currentAmount = Mathf.Max(0f, entry.currentAmount + amountDelta);
        }

        private void RecalculateAllProsperity()
        {
            foreach (KeyValuePair<string, CCS_SettlementSimulationState> pair in settlementLookup)
            {
                RecalculateProsperity(pair.Value);
            }
        }

        private void RecalculateProsperity(CCS_SettlementSimulationState settlementState)
        {
            if (settlementState == null)
            {
                return;
            }

            float foodPercent = GetFillPercent(settlementState, CCS_SettlementSupplyType.Food);
            float supplyPercent = GetAverageSupplyFillPercent(settlementState);
            float productionPercent = GetProductionFillPercent(settlementState);
            ResolveRegionalProductionBonus(settlementState, out float productionBonus, out float prosperityModifier);
            productionPercent = Mathf.Clamp(productionPercent * productionBonus, 0f, 100f);
            float prosperity = (foodPercent + supplyPercent + productionPercent) / 3f;
            settlementState.prosperity = CCS_RegionEconomyUtility.ApplyProsperityModifier(prosperity, prosperityModifier);
            SettlementProsperityChanged?.Invoke(settlementState);
            EvaluateSettlementGrowth(settlementState);
        }

        private void EvaluateAllSettlementGrowth()
        {
            foreach (KeyValuePair<string, CCS_SettlementSimulationState> pair in settlementLookup)
            {
                EvaluateSettlementGrowth(pair.Value);
            }
        }

        private void EvaluateSettlementGrowth(CCS_SettlementSimulationState settlementState)
        {
            if (settlementState == null || settlementGrowthProfile == null)
            {
                return;
            }

            CCS_SettlementGrowthStage startingStage = ResolveStartingGrowthStage(settlementState.settlementId);
            CCS_SettlementGrowthStage previousStage = ResolveGrowthStage(settlementState.currentGrowthStage, startingStage);
            float foodPercent = GetFillPercent(settlementState, CCS_SettlementSupplyType.Food);
            float industrialPercent = GetFillPercent(settlementState, CCS_SettlementSupplyType.IndustrialMaterials);
            bool regionDiscovered = IsSettlementRegionDiscovered(settlementState);
            string regionId = settlementState.regionId ?? string.Empty;

            CCS_SettlementGrowthStage resolvedStage = CCS_SettlementGrowthUtility.ResolveHighestAchievedStage(
                settlementGrowthProfile,
                startingStage,
                settlementState.prosperity,
                foodPercent,
                industrialPercent,
                settlementState.completedContractsCount,
                regionDiscovered,
                regionId);

            CCS_SettlementGrowthStage nextStage = CCS_SettlementGrowthUtility.ResolveNextActiveStage(resolvedStage);
            float progressPercent = 100f;
            if (nextStage != CCS_SettlementGrowthStage.Unknown
                && settlementGrowthProfile.TryGetDefinition(nextStage, out CCS_SettlementGrowthDefinition nextDefinition))
            {
                progressPercent = CCS_SettlementGrowthUtility.CalculateProgressToStage(
                    nextDefinition,
                    settlementState.prosperity,
                    foodPercent,
                    industrialPercent,
                    settlementState.completedContractsCount,
                    regionDiscovered,
                    regionId);
            }

            bool stageChanged = resolvedStage != previousStage;
            settlementState.previousGrowthStage = (int)previousStage;
            settlementState.currentGrowthStage = (int)resolvedStage;
            settlementState.growthProgressPercent = progressPercent;

            if (!stageChanged)
            {
                return;
            }

            CCS_SettlementGrowthChangedEventArgs eventArgs = new CCS_SettlementGrowthChangedEventArgs
            {
                Snapshot = BuildGrowthSnapshot(settlementState),
                StageChanged = true
            };
            SettlementGrowthChanged?.Invoke(eventArgs);

            if (activeProfile != null && activeProfile.EnableDebugLogging)
            {
                Debug.Log(
                    $"{LogPrefix} Settlement '{settlementState.settlementId}' growth stage changed "
                    + $"from {CCS_SettlementGrowthUtility.GetDisplayName(previousStage)} to "
                    + $"{CCS_SettlementGrowthUtility.GetDisplayName(resolvedStage)}.");
            }
        }

        private CCS_SettlementGrowthSnapshot BuildGrowthSnapshot(CCS_SettlementSimulationState settlementState)
        {
            CCS_SettlementGrowthStage currentStage = ResolveGrowthStage(
                settlementState.currentGrowthStage,
                ResolveStartingGrowthStage(settlementState.settlementId));
            CCS_SettlementGrowthStage previousStage = ResolveGrowthStage(
                settlementState.previousGrowthStage,
                currentStage);
            CCS_SettlementGrowthStage nextStage = CCS_SettlementGrowthUtility.ResolveNextActiveStage(currentStage);
            return CCS_SettlementGrowthUtility.BuildSnapshot(
                settlementState.settlementId,
                currentStage,
                previousStage,
                nextStage,
                settlementState.growthProgressPercent,
                settlementState.completedContractsCount,
                settlementState.prosperity,
                GetFillPercent(settlementState, CCS_SettlementSupplyType.Food),
                GetFillPercent(settlementState, CCS_SettlementSupplyType.IndustrialMaterials));
        }

        private CCS_SettlementGrowthStage ResolveStartingGrowthStage(string settlementId)
        {
            if (settlementGrowthProfile != null
                && settlementGrowthProfile.TryGetStartingStage(settlementId, out CCS_SettlementGrowthStage startingStage))
            {
                return startingStage;
            }

            return CCS_SettlementGrowthStage.Outpost;
        }

        private static CCS_SettlementGrowthStage ResolveGrowthStage(int rawStage, CCS_SettlementGrowthStage fallback)
        {
            return Enum.IsDefined(typeof(CCS_SettlementGrowthStage), rawStage)
                ? (CCS_SettlementGrowthStage)rawStage
                : fallback;
        }

        private bool IsSettlementRegionDiscovered(CCS_SettlementSimulationState settlementState)
        {
            if (settlementState == null)
            {
                return false;
            }

            string regionId = settlementState.regionId;
            if (string.IsNullOrWhiteSpace(regionId)
                && regionService != null
                && regionService.TryGetOwningRegionForSettlement(
                    settlementState.settlementId,
                    out CCS_RegionDefinition regionDefinition)
                && regionDefinition != null)
            {
                regionId = regionDefinition.RegionId;
            }

            return !string.IsNullOrWhiteSpace(regionId)
                && regionService != null
                && regionService.IsInitialized
                && regionService.IsDiscovered(regionId);
        }

        private void ResolveRegionalProductionBonus(
            CCS_SettlementSimulationState settlementState,
            out float productionBonus,
            out float prosperityModifier)
        {
            productionBonus = 1f;
            prosperityModifier = 1f;
            if (settlementState == null)
            {
                return;
            }

            string regionId = settlementState.regionId;
            if (string.IsNullOrWhiteSpace(regionId)
                && regionService != null
                && regionService.TryGetOwningRegionForSettlement(
                    settlementState.settlementId,
                    out CCS_RegionDefinition regionDefinition)
                && regionDefinition != null)
            {
                regionId = regionDefinition.RegionId;
            }

            if (string.IsNullOrWhiteSpace(regionId))
            {
                return;
            }

            if (TryGetRegionState(regionId, out CCS_RegionSimulationState regionState) && regionState != null)
            {
                productionBonus = regionState.productionBonus > 0f ? regionState.productionBonus : 1f;
                prosperityModifier = regionState.prosperityModifier > 0f ? regionState.prosperityModifier : 1f;
                return;
            }

            if (regionService != null
                && regionService.TryGetDefinition(regionId, out CCS_RegionDefinition definition)
                && definition?.ProductionModifier != null)
            {
                productionBonus = definition.ProductionModifier.ProductionBonus;
                prosperityModifier = definition.ProductionModifier.ProsperityModifier;
            }
        }

        private static float GetFillPercent(CCS_SettlementSimulationState settlementState, CCS_SettlementSupplyType supplyType)
        {
            CCS_SettlementSupplyEntry entry = GetSupplyEntry(settlementState, supplyType);
            if (entry == null)
            {
                return 0f;
            }

            return Mathf.Clamp01(entry.FillRatio) * 100f;
        }

        private static float GetAverageSupplyFillPercent(CCS_SettlementSimulationState settlementState)
        {
            CCS_SettlementSupplyEntry[] supplies = settlementState.supplies;
            if (supplies == null || supplies.Length == 0)
            {
                return 0f;
            }

            float total = 0f;
            int count = 0;
            for (int index = 0; index < supplies.Length; index++)
            {
                CCS_SettlementSupplyEntry supply = supplies[index];
                if (supply == null)
                {
                    continue;
                }

                total += Mathf.Clamp01(supply.FillRatio);
                count++;
            }

            return count == 0 ? 0f : (total / count) * 100f;
        }

        private static float GetProductionFillPercent(CCS_SettlementSimulationState settlementState)
        {
            CCS_SettlementProductionEntry[] productions = settlementState.productions;
            CCS_SettlementDemandEntry[] demands = settlementState.demands;
            if (productions == null || productions.Length == 0)
            {
                return 0f;
            }

            float totalProduction = 0f;
            float totalDemand = 0f;
            for (int index = 0; index < productions.Length; index++)
            {
                CCS_SettlementProductionEntry production = productions[index];
                if (production == null)
                {
                    continue;
                }

                totalProduction += Mathf.Max(0f, production.currentProduction);
                totalDemand += Mathf.Max(0f, GetDemandAmount(demands, production.SupplyType));
            }

            if (totalDemand <= 0f)
            {
                return totalProduction > 0f ? 100f : 0f;
            }

            return Mathf.Clamp01(totalProduction / totalDemand) * 100f;
        }

        private static float GetDemandAmount(CCS_SettlementDemandEntry[] demands, CCS_SettlementSupplyType supplyType)
        {
            if (demands == null)
            {
                return 0f;
            }

            for (int index = 0; index < demands.Length; index++)
            {
                CCS_SettlementDemandEntry demand = demands[index];
                if (demand != null && demand.SupplyType == supplyType)
                {
                    return demand.currentDemand;
                }
            }

            return 0f;
        }

        private CCS_WorldSimulationSnapshot BuildSnapshot()
        {
            CCS_SettlementSimulationState[] settlements = CaptureState();
            CCS_RegionSimulationState[] regions = CaptureRegionState();
            return new CCS_WorldSimulationSnapshot
            {
                SettlementStates = settlements,
                RegionStates = regions
            };
        }

        private CCS_SettlementSimulationState CreateSettlementStateFromProfile(
            CCS_WorldSimulationSettlementProfileEntry entry)
        {
            CCS_SettlementGrowthStage startingStage = CCS_SettlementGrowthStage.Outpost;
            if (settlementGrowthProfile != null)
            {
                settlementGrowthProfile.TryGetStartingStage(entry.settlementId, out startingStage);
            }

            return new CCS_SettlementSimulationState
            {
                settlementId = entry.settlementId,
                regionId = entry.regionId ?? string.Empty,
                population = entry.population,
                prosperity = 0f,
                isDiscovered = false,
                supplies = CloneSupplyEntries(entry.supplies),
                demands = CloneDemandEntries(entry.demands),
                productions = CloneProductionEntries(entry.productions),
                currentGrowthStage = (int)startingStage,
                previousGrowthStage = (int)startingStage,
                growthProgressPercent = 0f,
                completedContractsCount = 0
            };
        }

        private static CCS_RegionSimulationState CreateRegionStateFromProfile(CCS_WorldSimulationRegionProfileEntry entry)
        {
            CCS_RegionSpecializationType specialization = Enum.IsDefined(
                typeof(CCS_RegionSpecializationType),
                entry.specializationType)
                ? (CCS_RegionSpecializationType)entry.specializationType
                : CCS_RegionSpecializationType.Unknown;

            CCS_RegionSpecializationType dominantIndustry = CCS_RegionEconomyUtility.ResolveDominantIndustry(
                specialization,
                entry.foodPotential,
                entry.wildlifePotential,
                entry.miningPotential,
                entry.industryPotential);

            CCS_RegionEconomyUtility.ResolveRegionalSupplyStrengths(
                specialization,
                entry.foodPotential,
                entry.wildlifePotential,
                entry.miningPotential,
                entry.industryPotential,
                out float foodStrength,
                out float industrialStrength,
                out float buildingStrength,
                out float tradeStrength);

            return new CCS_RegionSimulationState
            {
                regionId = entry.regionId,
                isDiscovered = false,
                specializationType = (int)specialization,
                dominantIndustry = (int)dominantIndustry,
                foodPotential = entry.foodPotential,
                wildlifePotential = entry.wildlifePotential,
                miningPotential = entry.miningPotential,
                industryPotential = entry.industryPotential,
                productionBonus = entry.productionBonus > 0f ? entry.productionBonus : 1f,
                prosperityModifier = entry.prosperityModifier > 0f ? entry.prosperityModifier : 1f,
                foodSupplyStrength = foodStrength,
                industrialSupplyStrength = industrialStrength,
                buildingSupplyStrength = buildingStrength,
                tradeSupplyStrength = tradeStrength
            };
        }

        private static CCS_SettlementSupplyEntry GetSupplyEntry(
            CCS_SettlementSimulationState settlementState,
            CCS_SettlementSupplyType supplyType)
        {
            CCS_SettlementSupplyEntry[] supplies = settlementState?.supplies;
            if (supplies == null)
            {
                return null;
            }

            for (int index = 0; index < supplies.Length; index++)
            {
                CCS_SettlementSupplyEntry supply = supplies[index];
                if (supply != null && supply.SupplyType == supplyType)
                {
                    return supply;
                }
            }

            return null;
        }

        private static CCS_SettlementSupplyEntry GetOrCreateSupplyEntry(
            CCS_SettlementSimulationState settlementState,
            CCS_SettlementSupplyType supplyType)
        {
            CCS_SettlementSupplyEntry existing = GetSupplyEntry(settlementState, supplyType);
            if (existing != null)
            {
                return existing;
            }

            CCS_SettlementSupplyEntry[] supplies = settlementState.supplies ?? Array.Empty<CCS_SettlementSupplyEntry>();
            CCS_SettlementSupplyEntry[] expanded = new CCS_SettlementSupplyEntry[supplies.Length + 1];
            Array.Copy(supplies, expanded, supplies.Length);
            existing = new CCS_SettlementSupplyEntry
            {
                supplyType = (int)supplyType,
                currentAmount = 0f,
                desiredAmount = 0f
            };
            expanded[supplies.Length] = existing;
            settlementState.supplies = expanded;
            return existing;
        }

        private static bool IsFuelOrBuildingItem(string itemId)
        {
            return !string.IsNullOrWhiteSpace(itemId)
                && (itemId.Contains("wood", StringComparison.OrdinalIgnoreCase)
                    || itemId.Contains("lumber", StringComparison.OrdinalIgnoreCase));
        }

        private static CCS_SettlementSimulationState CloneSettlementState(CCS_SettlementSimulationState source)
        {
            if (source == null)
            {
                return null;
            }

            return new CCS_SettlementSimulationState
            {
                settlementId = source.settlementId,
                regionId = source.regionId ?? string.Empty,
                population = source.population,
                prosperity = source.prosperity,
                isDiscovered = source.isDiscovered,
                supplies = CloneSupplyEntries(source.supplies),
                demands = CloneDemandEntries(source.demands),
                productions = CloneProductionEntries(source.productions),
                currentGrowthStage = source.currentGrowthStage,
                previousGrowthStage = source.previousGrowthStage,
                growthProgressPercent = source.growthProgressPercent,
                completedContractsCount = source.completedContractsCount
            };
        }

        private static CCS_RegionSimulationState CloneRegionState(CCS_RegionSimulationState source)
        {
            if (source == null)
            {
                return null;
            }

            return new CCS_RegionSimulationState
            {
                regionId = source.regionId,
                isDiscovered = source.isDiscovered,
                specializationType = source.specializationType,
                dominantIndustry = source.dominantIndustry,
                foodPotential = source.foodPotential,
                wildlifePotential = source.wildlifePotential,
                miningPotential = source.miningPotential,
                industryPotential = source.industryPotential,
                productionBonus = source.productionBonus,
                prosperityModifier = source.prosperityModifier,
                foodSupplyStrength = source.foodSupplyStrength,
                industrialSupplyStrength = source.industrialSupplyStrength,
                buildingSupplyStrength = source.buildingSupplyStrength,
                tradeSupplyStrength = source.tradeSupplyStrength
            };
        }

        private static CCS_SettlementSupplyEntry[] CloneSupplyEntries(CCS_SettlementSupplyEntry[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<CCS_SettlementSupplyEntry>();
            }

            CCS_SettlementSupplyEntry[] clone = new CCS_SettlementSupplyEntry[source.Length];
            for (int index = 0; index < source.Length; index++)
            {
                CCS_SettlementSupplyEntry entry = source[index];
                clone[index] = entry == null
                    ? new CCS_SettlementSupplyEntry()
                    : new CCS_SettlementSupplyEntry
                    {
                        supplyType = entry.supplyType,
                        currentAmount = entry.currentAmount,
                        desiredAmount = entry.desiredAmount
                    };
            }

            return clone;
        }

        private static CCS_SettlementDemandEntry[] CloneDemandEntries(CCS_SettlementDemandEntry[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<CCS_SettlementDemandEntry>();
            }

            CCS_SettlementDemandEntry[] clone = new CCS_SettlementDemandEntry[source.Length];
            for (int index = 0; index < source.Length; index++)
            {
                CCS_SettlementDemandEntry entry = source[index];
                clone[index] = entry == null
                    ? new CCS_SettlementDemandEntry()
                    : new CCS_SettlementDemandEntry
                    {
                        supplyType = entry.supplyType,
                        currentDemand = entry.currentDemand
                    };
            }

            return clone;
        }

        private static CCS_SettlementProductionEntry[] CloneProductionEntries(CCS_SettlementProductionEntry[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<CCS_SettlementProductionEntry>();
            }

            CCS_SettlementProductionEntry[] clone = new CCS_SettlementProductionEntry[source.Length];
            for (int index = 0; index < source.Length; index++)
            {
                CCS_SettlementProductionEntry entry = source[index];
                clone[index] = entry == null
                    ? new CCS_SettlementProductionEntry()
                    : new CCS_SettlementProductionEntry
                    {
                        supplyType = entry.supplyType,
                        currentProduction = entry.currentProduction
                    };
            }

            return clone;
        }
    }
}
