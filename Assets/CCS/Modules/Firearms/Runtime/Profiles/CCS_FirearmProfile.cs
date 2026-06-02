using System;
using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FirearmProfile
// CATEGORY: Modules / Firearms / Runtime / Profiles
// PURPOSE: Catalog of firearm and ammo definitions for the generic firearm service.
// PLACEMENT: Assets/CCS/Survival/Profiles/Firearms/
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Firearms
{
    [CreateAssetMenu(
        fileName = "CCS_FirearmProfile",
        menuName = "CCS/Survival/Firearms/Firearm Profile")]
    public sealed class CCS_FirearmProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_FirearmDefinition[] firearmDefinitions = Array.Empty<CCS_FirearmDefinition>();
        [SerializeField] private CCS_AmmoDefinition[] ammoDefinitions = Array.Empty<CCS_AmmoDefinition>();

        public IReadOnlyList<CCS_FirearmDefinition> FirearmDefinitions => firearmDefinitions;

        public IReadOnlyList<CCS_AmmoDefinition> AmmoDefinitions => ammoDefinitions;

        public bool TryGetFirearmByItemId(string itemId, out CCS_FirearmDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(itemId) || firearmDefinitions == null)
            {
                return false;
            }

            for (int index = 0; index < firearmDefinitions.Length; index++)
            {
                CCS_FirearmDefinition candidate = firearmDefinitions[index];
                if (candidate != null
                    && string.Equals(candidate.InventoryItemId, itemId, StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetFirearmById(string firearmId, out CCS_FirearmDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(firearmId) || firearmDefinitions == null)
            {
                return false;
            }

            for (int index = 0; index < firearmDefinitions.Length; index++)
            {
                CCS_FirearmDefinition candidate = firearmDefinitions[index];
                if (candidate != null
                    && string.Equals(candidate.FirearmId, firearmId, StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetAmmoByItemId(string itemId, out CCS_AmmoDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(itemId) || ammoDefinitions == null)
            {
                return false;
            }

            for (int index = 0; index < ammoDefinitions.Length; index++)
            {
                CCS_AmmoDefinition candidate = ammoDefinitions[index];
                if (candidate != null
                    && string.Equals(candidate.InventoryItemId, itemId, StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
