using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BusinessProfile
// CATEGORY: Modules / Settlements / Runtime / Businesses
// PURPOSE: Business definitions and per-settlement business catalogs.
// PLACEMENT: Assets/CCS/Survival/Profiles/Settlements/Businesses/
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.7.0 — wired on CCS_WorldSimulationProfile.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [CreateAssetMenu(
        fileName = "CCS_BusinessProfile",
        menuName = "CCS/Survival/Settlements/Business Profile")]
    public sealed class CCS_BusinessProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_BusinessDefinition[] businessDefinitions = Array.Empty<CCS_BusinessDefinition>();

        [SerializeField] private CCS_BusinessSettlementCatalogEntry[] settlementCatalog =
            Array.Empty<CCS_BusinessSettlementCatalogEntry>();

        public CCS_BusinessDefinition[] BusinessDefinitions =>
            businessDefinitions ?? Array.Empty<CCS_BusinessDefinition>();

        public CCS_BusinessSettlementCatalogEntry[] SettlementCatalog =>
            settlementCatalog ?? Array.Empty<CCS_BusinessSettlementCatalogEntry>();

        public bool TryGetDefinition(
            CCS_BusinessType businessType,
            out CCS_BusinessDefinition definition)
        {
            definition = null;
            CCS_BusinessDefinition[] definitions = BusinessDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_BusinessDefinition candidate = definitions[index];
                if (candidate != null && candidate.businessType == businessType)
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetSettlementCatalog(
            string settlementId,
            out CCS_BusinessSettlementCatalogEntry entry)
        {
            entry = null;
            if (string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            CCS_BusinessSettlementCatalogEntry[] entries = SettlementCatalog;
            for (int index = 0; index < entries.Length; index++)
            {
                CCS_BusinessSettlementCatalogEntry candidate = entries[index];
                if (candidate != null
                    && string.Equals(candidate.settlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    entry = candidate;
                    return true;
                }
            }

            return false;
        }
    }

    [Serializable]
    public sealed class CCS_BusinessSettlementCatalogEntry
    {
        public string settlementId = string.Empty;

        public CCS_BusinessType[] businessTypes = Array.Empty<CCS_BusinessType>();
    }
}
