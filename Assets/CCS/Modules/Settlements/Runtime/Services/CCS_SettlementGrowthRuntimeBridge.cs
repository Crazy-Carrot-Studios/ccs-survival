using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementGrowthRuntimeBridge
// CATEGORY: Modules / Settlements / Runtime / Services
// PURPOSE: Forwards settlement growth stage changes to scene debug visuals.
// PLACEMENT: Used by settlement service and world simulation composition wiring.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.2.0 settlement growth foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_SettlementGrowthRuntimeBridge
    {
        public static event Action<CCS_SettlementGrowthSnapshot> GrowthStageChanged;

        public static void NotifyGrowthStageChanged(CCS_SettlementGrowthSnapshot snapshot)
        {
            if (snapshot == null || !snapshot.IsValid)
            {
                return;
            }

            GrowthStageChanged?.Invoke(snapshot);
            ApplyLocationVisual(snapshot);
        }

        private static void ApplyLocationVisual(CCS_SettlementGrowthSnapshot snapshot)
        {
            CCS_SettlementLocation[] locations =
                CCS_SurvivalSceneQueryUtility.FindActiveObjectsByType<CCS_SettlementLocation>();
            if (locations == null)
            {
                return;
            }

            for (int index = 0; index < locations.Length; index++)
            {
                CCS_SettlementLocation location = locations[index];
                if (location == null
                    || location.SettlementDefinition == null
                    || !string.Equals(
                        location.SettlementDefinition.SettlementId,
                        snapshot.SettlementId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                location.ApplyGrowthStageVisual(snapshot.CurrentGrowthStage);
            }
        }
    }
}
