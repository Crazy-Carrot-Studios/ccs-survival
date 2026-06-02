using System;
using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementService
// CATEGORY: Modules / Settlements / Runtime / Services
// PURPOSE: Tracks discovered settlements and service point activation events.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: No NPC AI, dialogue, or quest systems in 1.8.0.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_SettlementService]";

        private readonly Dictionary<string, CCS_SettlementSnapshot> discoveryLookup =
            new Dictionary<string, CCS_SettlementSnapshot>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, CCS_SettlementDefinition> definitionLookup =
            new Dictionary<string, CCS_SettlementDefinition>(StringComparer.OrdinalIgnoreCase);

        private CCS_SettlementProfile activeProfile;
        private bool isInitialized;
        private string lastActivatedServicePointId = string.Empty;
        private CCS_SettlementServicePointType lastActivatedServicePointType = CCS_SettlementServicePointType.Other;
        private string lastActivatedVendorId = string.Empty;

        public event Action<CCS_SettlementSnapshot> SettlementDiscovered;
        public event Action<CCS_SettlementServicePointActivationArgs> ServicePointActivated;

        public bool IsInitialized => isInitialized;

        public CCS_SettlementProfile ActiveProfile => activeProfile;

        public string LastActivatedServicePointId => lastActivatedServicePointId;

        public CCS_SettlementServicePointType LastActivatedServicePointType => lastActivatedServicePointType;

        public string LastActivatedVendorId => lastActivatedVendorId;

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_SettlementProfile profile)
        {
            activeProfile = profile;
            definitionLookup.Clear();

            if (profile == null)
            {
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_SettlementValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            CCS_SettlementDefinition[] definitions = profile.SettlementDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_SettlementDefinition definition = definitions[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.SettlementId))
                {
                    continue;
                }

                definitionLookup[definition.SettlementId] = definition;
            }

            isInitialized = true;
        }

        public bool TryGetDefinition(string settlementId, out CCS_SettlementDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            return definitionLookup.TryGetValue(settlementId, out definition) && definition != null;
        }

        public bool IsDiscovered(string settlementId)
        {
            return !string.IsNullOrWhiteSpace(settlementId)
                && discoveryLookup.TryGetValue(settlementId, out CCS_SettlementSnapshot snapshot)
                && snapshot != null
                && snapshot.Discovered;
        }

        public bool TryGetSnapshot(string settlementId, out CCS_SettlementSnapshot snapshot)
        {
            snapshot = null;
            if (string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            return discoveryLookup.TryGetValue(settlementId, out snapshot) && snapshot != null;
        }

        public IReadOnlyCollection<CCS_SettlementSnapshot> GetDiscoveredSnapshots()
        {
            return discoveryLookup.Values;
        }

        public bool DiscoverSettlement(CCS_SettlementDefinition definition, Vector3 worldPosition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.SettlementId))
            {
                return false;
            }

            Vector3 resolvedPosition = worldPosition;
            if (resolvedPosition == Vector3.zero)
            {
                resolvedPosition = definition.DefaultWorldPosition;
            }

            if (discoveryLookup.TryGetValue(definition.SettlementId, out CCS_SettlementSnapshot existing)
                && existing != null
                && existing.Discovered)
            {
                existing.DisplayName = definition.DisplayName;
                existing.SettlementType = definition.SettlementType;
                existing.Position = resolvedPosition;
                return false;
            }

            CCS_SettlementSnapshot snapshot = new CCS_SettlementSnapshot
            {
                SettlementId = definition.SettlementId,
                DisplayName = definition.DisplayName,
                SettlementType = definition.SettlementType,
                Discovered = true,
                Position = resolvedPosition
            };

            discoveryLookup[definition.SettlementId] = snapshot;

            if (activeProfile != null && activeProfile.EnableDebugLogging)
            {
                Debug.Log($"{LogPrefix} Discovered settlement '{snapshot.DisplayName}' ({snapshot.SettlementId}).");
            }

            SettlementDiscovered?.Invoke(snapshot);
            return true;
        }

        public void NotifyServicePointActivated(CCS_SettlementServicePointActivationArgs activationArgs)
        {
            if (activationArgs == null)
            {
                return;
            }

            lastActivatedServicePointId = activationArgs.ServicePointId ?? string.Empty;
            lastActivatedServicePointType = activationArgs.ServicePointType;
            lastActivatedVendorId = activationArgs.VendorId ?? string.Empty;

            if (activeProfile != null && activeProfile.EnableDebugLogging)
            {
                Debug.Log(
                    $"{LogPrefix} Service point activated: {activationArgs.ServicePointType} "
                    + $"(settlement={activationArgs.SettlementId}, vendor={activationArgs.VendorId}).");
            }

            ServicePointActivated?.Invoke(activationArgs);
        }

        public CCS_SettlementSaveState[] CaptureState()
        {
            if (discoveryLookup.Count == 0)
            {
                return Array.Empty<CCS_SettlementSaveState>();
            }

            CCS_SettlementSaveState[] records = new CCS_SettlementSaveState[discoveryLookup.Count];
            int writeIndex = 0;
            foreach (KeyValuePair<string, CCS_SettlementSnapshot> entry in discoveryLookup)
            {
                CCS_SettlementSnapshot snapshot = entry.Value;
                if (snapshot == null || !snapshot.Discovered)
                {
                    continue;
                }

                records[writeIndex++] = new CCS_SettlementSaveState
                {
                    settlementId = snapshot.SettlementId ?? string.Empty,
                    displayName = snapshot.DisplayName ?? string.Empty,
                    settlementType = (int)snapshot.SettlementType,
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

            CCS_SettlementSaveState[] trimmed = new CCS_SettlementSaveState[writeIndex];
            Array.Copy(records, trimmed, writeIndex);
            return trimmed;
        }

        public void RestoreState(CCS_SettlementSaveState[] records)
        {
            discoveryLookup.Clear();
            if (records == null || records.Length == 0)
            {
                return;
            }

            for (int index = 0; index < records.Length; index++)
            {
                CCS_SettlementSaveState record = records[index];
                if (record == null || !record.discovered || string.IsNullOrWhiteSpace(record.settlementId))
                {
                    continue;
                }

                CCS_SettlementType settlementType = Enum.IsDefined(typeof(CCS_SettlementType), record.settlementType)
                    ? (CCS_SettlementType)record.settlementType
                    : CCS_SettlementType.Other;

                discoveryLookup[record.settlementId] = new CCS_SettlementSnapshot
                {
                    SettlementId = record.settlementId,
                    DisplayName = record.displayName ?? string.Empty,
                    SettlementType = settlementType,
                    Discovered = true,
                    Position = new Vector3(record.positionX, record.positionY, record.positionZ)
                };
            }
        }
    }

    [Serializable]
    public sealed class CCS_SettlementSaveState
    {
        public string settlementId = string.Empty;
        public string displayName = string.Empty;
        public int settlementType;
        public bool discovered;
        public float positionX;
        public float positionY;
        public float positionZ;
    }

    public sealed class CCS_SettlementServicePointActivationArgs
    {
        public string SettlementId { get; set; } = string.Empty;

        public string ServicePointId { get; set; } = string.Empty;

        public CCS_SettlementServicePointType ServicePointType { get; set; }

        public string VendorId { get; set; } = string.Empty;

        public bool HasVendor { get; set; }
    }
}
