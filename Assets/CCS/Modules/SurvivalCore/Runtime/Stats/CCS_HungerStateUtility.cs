// =============================================================================
// SCRIPT: CCS_HungerStateUtility
// CATEGORY: Modules / SurvivalCore / Runtime / Stats
// PURPOSE: Resolves hunger warning states and fullness checks from snapshots and profile thresholds.
// PLACEMENT: Used by consumable food service, HUD presenters, and validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Threshold order: critical < low. No health damage at Empty in 0.9.5.
// =============================================================================

namespace CCS.Modules.SurvivalCore
{
    public static class CCS_HungerStateUtility
    {
        #region Public Methods

        public static CCS_HungerState ResolveState(
            CCS_SurvivalStatSnapshot hungerSnapshot,
            CCS_SurvivalCoreProfile profile)
        {
            return ResolveState(
                hungerSnapshot.CurrentValue,
                hungerSnapshot.MinValue,
                hungerSnapshot.MaxValue,
                profile);
        }

        public static CCS_HungerState ResolveState(
            float currentHunger,
            float minHunger,
            float maxHunger,
            CCS_SurvivalCoreProfile profile)
        {
            float lowThreshold = profile != null ? profile.HungerLowThreshold : 30f;
            float criticalThreshold = profile != null ? profile.HungerCriticalThreshold : 10f;

            if (currentHunger <= minHunger + CCS_SurvivalStatUtility.DepletionEpsilon)
            {
                return CCS_HungerState.Empty;
            }

            if (currentHunger <= criticalThreshold)
            {
                return CCS_HungerState.Critical;
            }

            if (currentHunger <= lowThreshold)
            {
                return CCS_HungerState.Low;
            }

            return CCS_HungerState.Normal;
        }

        public static bool IsHungerFull(CCS_SurvivalStatSnapshot hungerSnapshot)
        {
            return hungerSnapshot.CurrentValue
                >= hungerSnapshot.MaxValue - CCS_SurvivalStatUtility.DepletionEpsilon;
        }

        public static bool HasRoomForRestore(
            CCS_SurvivalStatSnapshot hungerSnapshot,
            float restoreAmount)
        {
            if (restoreAmount <= 0f || IsHungerFull(hungerSnapshot))
            {
                return false;
            }

            return true;
        }

        public static string GetDisplayLabel(CCS_HungerState hungerState)
        {
            switch (hungerState)
            {
                case CCS_HungerState.Low:
                    return "Low";
                case CCS_HungerState.Critical:
                    return "Critical";
                case CCS_HungerState.Empty:
                    return "Empty";
                default:
                    return "Normal";
            }
        }

        public static string GetThresholdNotification(CCS_HungerState hungerState)
        {
            switch (hungerState)
            {
                case CCS_HungerState.Low:
                    return "You are hungry";
                case CCS_HungerState.Critical:
                    return "You are starving soon";
                default:
                    return string.Empty;
            }
        }

        #endregion
    }
}
