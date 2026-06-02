using System;
using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_MountProfile
// CATEGORY: Modules / Mounts / Runtime / Profiles
// PURPOSE: Catalog of mount definitions for the generic mount service.
// PLACEMENT: Assets/CCS/Survival/Profiles/Mounts/
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Mounts
{
    [CreateAssetMenu(
        fileName = "CCS_MountProfile",
        menuName = "CCS/Survival/Mounts/Mount Profile")]
    public sealed class CCS_MountProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_MountDefinition[] mountDefinitions = Array.Empty<CCS_MountDefinition>();
        [SerializeField] private CCS_MountDefinition defaultHorseDefinition;

        public IReadOnlyList<CCS_MountDefinition> MountDefinitions => mountDefinitions;

        public CCS_MountDefinition DefaultHorseDefinition => defaultHorseDefinition;

        public bool TryGetMountById(string mountId, out CCS_MountDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(mountId) || mountDefinitions == null)
            {
                return false;
            }

            for (int index = 0; index < mountDefinitions.Length; index++)
            {
                CCS_MountDefinition candidate = mountDefinitions[index];
                if (candidate != null
                    && string.Equals(candidate.MountId, mountId, StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
