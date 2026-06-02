using System;
using System.Collections.Generic;
using CCS.Core;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RegionService
// CATEGORY: Modules / Regions / Runtime / Services
// PURPOSE: Tracks discovered regions, current region, and region enter/exit events.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: No procedural generation, factions, or final map UI in 1.9.0.
// =============================================================================

namespace CCS.Modules.Regions
{
    public sealed class CCS_RegionService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_RegionService]";

        private readonly Dictionary<string, CCS_RegionSnapshot> discoveryLookup =
            new Dictionary<string, CCS_RegionSnapshot>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, CCS_RegionDefinition> definitionLookup =
            new Dictionary<string, CCS_RegionDefinition>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, string> settlementOwnerLookup =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private CCS_RegionProfile activeProfile;
        private string currentRegionId = string.Empty;
        private bool isInitialized;

        public event Action<CCS_RegionSnapshot> RegionDiscovered;
        public event Action<CCS_RegionEventArgs> RegionEntered;
        public event Action<CCS_RegionEventArgs> RegionExited;

        public bool IsInitialized => isInitialized;

        public CCS_RegionProfile ActiveProfile => activeProfile;

        public string CurrentRegionId => currentRegionId ?? string.Empty;

        public bool HasCurrentRegion => !string.IsNullOrWhiteSpace(currentRegionId);

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_RegionProfile profile)
        {
            activeProfile = profile;
            definitionLookup.Clear();
            settlementOwnerLookup.Clear();

            if (profile == null)
            {
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_RegionValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            CCS_RegionDefinition[] definitions = profile.RegionDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_RegionDefinition definition = definitions[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.RegionId))
                {
                    continue;
                }

                definitionLookup[definition.RegionId] = definition;
                RegisterSettlementOwnership(definition);
            }

            isInitialized = true;
        }

        public bool TryGetDefinition(string regionId, out CCS_RegionDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(regionId))
            {
                return false;
            }

            return definitionLookup.TryGetValue(regionId, out definition) && definition != null;
        }

        public bool IsDiscovered(string regionId)
        {
            return !string.IsNullOrWhiteSpace(regionId)
                && discoveryLookup.TryGetValue(regionId, out CCS_RegionSnapshot snapshot)
                && snapshot != null
                && snapshot.Discovered;
        }

        public bool TryGetSnapshot(string regionId, out CCS_RegionSnapshot snapshot)
        {
            snapshot = null;
            if (string.IsNullOrWhiteSpace(regionId))
            {
                return false;
            }

            return discoveryLookup.TryGetValue(regionId, out snapshot) && snapshot != null;
        }

        public IReadOnlyCollection<CCS_RegionSnapshot> GetDiscoveredSnapshots()
        {
            return discoveryLookup.Values;
        }

        public int GetDiscoveredCount()
        {
            int count = 0;
            foreach (KeyValuePair<string, CCS_RegionSnapshot> entry in discoveryLookup)
            {
                if (entry.Value != null && entry.Value.Discovered)
                {
                    count++;
                }
            }

            return count;
        }

        public bool DiscoverRegion(CCS_RegionDefinition definition, Vector3 worldPosition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.RegionId))
            {
                return false;
            }

            Vector3 resolvedPosition = worldPosition;
            if (resolvedPosition == Vector3.zero)
            {
                resolvedPosition = definition.DefaultWorldPosition;
            }

            if (discoveryLookup.TryGetValue(definition.RegionId, out CCS_RegionSnapshot existing)
                && existing != null
                && existing.Discovered)
            {
                existing.DisplayName = definition.DisplayName;
                existing.RegionType = definition.RegionType;
                existing.Position = resolvedPosition;
                return false;
            }

            CCS_RegionSnapshot snapshot = new CCS_RegionSnapshot
            {
                RegionId = definition.RegionId,
                DisplayName = definition.DisplayName,
                RegionType = definition.RegionType,
                Discovered = true,
                Position = resolvedPosition
            };

            discoveryLookup[definition.RegionId] = snapshot;

            if (activeProfile != null && activeProfile.EnableDebugLogging)
            {
                Debug.Log($"{LogPrefix} Discovered region '{snapshot.DisplayName}' ({snapshot.RegionId}).");
            }

            RegionDiscovered?.Invoke(snapshot);
            return true;
        }

        public void NotifyRegionEntered(CCS_RegionDefinition definition, Vector3 worldPosition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.RegionId))
            {
                return;
            }

            DiscoverRegion(definition, worldPosition);

            if (string.Equals(currentRegionId, definition.RegionId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            currentRegionId = definition.RegionId;

            if (activeProfile != null && activeProfile.EnableDebugLogging)
            {
                Debug.Log($"{LogPrefix} Entered region '{definition.DisplayName}' ({definition.RegionId}).");
            }

            RegionEntered?.Invoke(CreateEventArgs(definition, worldPosition));
        }

        public void NotifyRegionExited(CCS_RegionDefinition definition, Vector3 worldPosition)
        {
            if (definition == null
                || string.IsNullOrWhiteSpace(definition.RegionId)
                || !string.Equals(currentRegionId, definition.RegionId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            currentRegionId = string.Empty;

            if (activeProfile != null && activeProfile.EnableDebugLogging)
            {
                Debug.Log($"{LogPrefix} Exited region '{definition.DisplayName}' ({definition.RegionId}).");
            }

            RegionExited?.Invoke(CreateEventArgs(definition, worldPosition));
        }

        public bool TryGetOwningRegionForSettlement(string settlementId, out CCS_RegionDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(settlementId)
                || !settlementOwnerLookup.TryGetValue(settlementId, out string regionId)
                || string.IsNullOrWhiteSpace(regionId))
            {
                return false;
            }

            return TryGetDefinition(regionId, out definition);
        }

        public bool TryGetResourceMetadataForRegion(string regionId, out string[] resourceMetadataTags)
        {
            resourceMetadataTags = Array.Empty<string>();
            if (!TryGetDefinition(regionId, out CCS_RegionDefinition definition) || definition == null)
            {
                return false;
            }

            resourceMetadataTags = definition.ResourceMetadataTags;
            return resourceMetadataTags != null && resourceMetadataTags.Length > 0;
        }

        public CCS_RegionSaveState[] CaptureState()
        {
            if (discoveryLookup.Count == 0)
            {
                return Array.Empty<CCS_RegionSaveState>();
            }

            CCS_RegionSaveState[] records = new CCS_RegionSaveState[discoveryLookup.Count];
            int writeIndex = 0;
            foreach (KeyValuePair<string, CCS_RegionSnapshot> entry in discoveryLookup)
            {
                CCS_RegionSnapshot snapshot = entry.Value;
                if (snapshot == null || !snapshot.Discovered)
                {
                    continue;
                }

                records[writeIndex++] = new CCS_RegionSaveState
                {
                    regionId = snapshot.RegionId ?? string.Empty,
                    displayName = snapshot.DisplayName ?? string.Empty,
                    regionType = (int)snapshot.RegionType,
                    discovered = snapshot.Discovered,
                    positionX = snapshot.Position.x,
                    positionY = snapshot.Position.y,
                    positionZ = snapshot.Position.z
                };
            }

            if (writeIndex == records.Length)
            {
                return records;
            }

            CCS_RegionSaveState[] trimmed = new CCS_RegionSaveState[writeIndex];
            Array.Copy(records, trimmed, writeIndex);
            return trimmed;
        }

        public void RestoreState(CCS_RegionSaveState[] records)
        {
            discoveryLookup.Clear();
            currentRegionId = string.Empty;

            if (records == null || records.Length == 0)
            {
                return;
            }

            for (int index = 0; index < records.Length; index++)
            {
                CCS_RegionSaveState record = records[index];
                if (record == null || !record.discovered || string.IsNullOrWhiteSpace(record.regionId))
                {
                    continue;
                }

                CCS_RegionType regionType = Enum.IsDefined(typeof(CCS_RegionType), record.regionType)
                    ? (CCS_RegionType)record.regionType
                    : CCS_RegionType.Other;

                discoveryLookup[record.regionId] = new CCS_RegionSnapshot
                {
                    RegionId = record.regionId,
                    DisplayName = record.displayName ?? string.Empty,
                    RegionType = regionType,
                    Discovered = true,
                    Position = new Vector3(record.positionX, record.positionY, record.positionZ)
                };
            }
        }

        private void RegisterSettlementOwnership(CCS_RegionDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            string[] settlementIds = definition.SettlementIds;
            for (int index = 0; index < settlementIds.Length; index++)
            {
                string settlementId = settlementIds[index];
                if (string.IsNullOrWhiteSpace(settlementId))
                {
                    continue;
                }

                settlementOwnerLookup[settlementId] = definition.RegionId;
            }
        }

        private static CCS_RegionEventArgs CreateEventArgs(CCS_RegionDefinition definition, Vector3 worldPosition)
        {
            return new CCS_RegionEventArgs
            {
                RegionId = definition.RegionId,
                DisplayName = definition.DisplayName,
                RegionType = definition.RegionType,
                WorldPosition = worldPosition
            };
        }
    }

    [Serializable]
    public sealed class CCS_RegionSaveState
    {
        public string regionId = string.Empty;
        public string displayName = string.Empty;
        public int regionType;
        public bool discovered;
        public float positionX;
        public float positionY;
        public float positionZ;
    }

    public sealed class CCS_RegionEventArgs
    {
        public string RegionId { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public CCS_RegionType RegionType { get; set; }

        public Vector3 WorldPosition { get; set; }
    }
}
