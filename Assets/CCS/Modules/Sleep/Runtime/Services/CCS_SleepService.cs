using CCS.Core;
using CCS.Modules.Crafting;
using CCS.Modules.Equipment;
using CCS.Modules.Inventory;
using CCS.Modules.Shelter;
using CCS.Modules.SurvivalCore;
using CCS.Modules.TimeOfDay;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SleepService
// CATEGORY: Modules / Sleep / Runtime / Services
// PURPOSE: Validates and executes sleep requests, advancing time and restoring fatigue.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration from sleep profile.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No dreams, death, enemy interruption, or sleep UI in 0.9.6 foundation.
// =============================================================================

namespace CCS.Modules.Sleep
{
    public sealed class CCS_SleepService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_SleepService]";

        #region Variables

        private CCS_SleepProfile activeProfile;
        private CCS_SurvivalCoreService survivalCoreService;
        private CCS_TimeOfDayService timeOfDayService;
        private CCS_ShelterService shelterService;
        private CCS_PlayerInventoryService inventoryService;
        private CCS_PlayerEquipmentService equipmentService;
        private CCS_CraftingService craftingService;

        private float lastHoursSlept;
        private float lastFatigueRestored;
        private CCS_SleepFailureReason lastFailureReason = CCS_SleepFailureReason.None;
        private string lastMessage = string.Empty;
        private bool isInitialized;

        #endregion

        #region Events

        public event SleepCompletedHandler SleepCompleted;
        public event SleepFailedHandler SleepFailed;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_SleepProfile ActiveProfile => activeProfile;

        #endregion

        #region Public Methods

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_SleepProfile profile)
        {
            if (profile == null)
            {
                Debug.LogWarning($"{LogPrefix} InitializeFromProfile called with null profile.");
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_SleepValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            activeProfile = profile;
            isInitialized = true;
            RegisterBedrollRecipe();
        }

        public void BindSurvivalCoreService(CCS_SurvivalCoreService service)
        {
            survivalCoreService = service;
        }

        public void BindTimeOfDayService(CCS_TimeOfDayService service)
        {
            timeOfDayService = service;
        }

        public void BindShelterService(CCS_ShelterService service)
        {
            shelterService = service;
        }

        public void BindInventoryService(CCS_PlayerInventoryService service)
        {
            inventoryService = service;
        }

        public void BindEquipmentService(CCS_PlayerEquipmentService service)
        {
            equipmentService = service;
        }

        public void BindCraftingService(CCS_CraftingService service)
        {
            craftingService = service;
            RegisterBedrollRecipe();
        }

        public CCS_SleepSnapshot CreateSnapshot()
        {
            if (!EnsureInitialized() || activeProfile == null)
            {
                return CCS_SleepSnapshot.Empty;
            }

            bool hasBedroll = HasBedrollAvailable();
            bool isSheltered = ResolveIsSheltered();
            bool canSleep = CanAttemptSleep(out _);
            bool sleepReady = hasBedroll && canSleep;

            return new CCS_SleepSnapshot(
                hasBedroll,
                isSheltered,
                canSleep,
                sleepReady,
                lastHoursSlept,
                lastFatigueRestored,
                lastFailureReason,
                lastMessage);
        }

        public CCS_SleepResult TrySleep(CCS_SleepRequest request)
        {
            request ??= new CCS_SleepRequest();

            if (!EnsureInitialized() || activeProfile == null)
            {
                return FailSleep(CCS_SleepFailureReason.ProfileUnavailable, "Sleep profile is unavailable.");
            }

            if (timeOfDayService == null || !timeOfDayService.IsInitialized)
            {
                return FailSleep(
                    CCS_SleepFailureReason.TimeServiceUnavailable,
                    "Time of day service is unavailable.");
            }

            if (survivalCoreService == null || !survivalCoreService.IsInitialized)
            {
                return FailSleep(
                    CCS_SleepFailureReason.SurvivalCoreUnavailable,
                    "Survival core service is unavailable.");
            }

            if (activeProfile.RequireBedroll && !HasBedrollAvailable())
            {
                return FailSleep(CCS_SleepFailureReason.MissingBedroll, "Missing bedroll.");
            }

            if (IsAlreadyRested())
            {
                return FailSleep(CCS_SleepFailureReason.AlreadyRested, "Already rested.");
            }

            bool isSheltered = ResolveIsSheltered();
            if (activeProfile.RequireShelter && !isSheltered)
            {
                return FailSleep(CCS_SleepFailureReason.UnsafeConditions, "Unsafe conditions.");
            }

            float sleepHours = CCS_SleepValidationUtility.ClampSleepHours(
                activeProfile,
                request.RequestedSleepHours);

            float fatigueMultiplier = isSheltered
                ? 1f
                : activeProfile.UnshelteredFatigueRestoreMultiplier;
            float fatigueRestored = activeProfile.FatigueRestorePerHour * sleepHours * fatigueMultiplier;
            bool usedPoorShelterPenalty = !isSheltered && fatigueMultiplier < 1f;

            timeOfDayService.AdvanceTimeByHours(sleepHours);
            ApplySleepStatDrain(sleepHours);

            if (fatigueRestored > 0f)
            {
                survivalCoreService.TryApplyModifier(
                    CCS_SurvivalStatType.Fatigue,
                    CCS_SurvivalStatModifier.Add(-fatigueRestored));
            }

            lastHoursSlept = sleepHours;
            lastFatigueRestored = fatigueRestored;
            lastFailureReason = CCS_SleepFailureReason.None;
            lastMessage = BuildSuccessMessage(sleepHours, fatigueRestored, usedPoorShelterPenalty);

            CCS_SleepResult result = CCS_SleepResult.Success(
                sleepHours,
                fatigueRestored,
                usedPoorShelterPenalty,
                lastMessage);
            RaiseSleepCompleted(result);
            return result;
        }

        #endregion

        #region Private Methods

        private bool EnsureInitialized()
        {
            return isInitialized;
        }

        private void RegisterBedrollRecipe()
        {
            if (craftingService == null
                || !craftingService.IsInitialized
                || activeProfile?.BedrollRecipeDefinition == null)
            {
                return;
            }

            craftingService.RegisterDefaultUnlockedRecipe(activeProfile.BedrollRecipeDefinition);
        }

        private bool CanAttemptSleep(out CCS_SleepFailureReason failureReason)
        {
            failureReason = CCS_SleepFailureReason.None;

            if (activeProfile == null)
            {
                failureReason = CCS_SleepFailureReason.ProfileUnavailable;
                return false;
            }

            if (timeOfDayService == null || !timeOfDayService.IsInitialized)
            {
                failureReason = CCS_SleepFailureReason.TimeServiceUnavailable;
                return false;
            }

            if (survivalCoreService == null || !survivalCoreService.IsInitialized)
            {
                failureReason = CCS_SleepFailureReason.SurvivalCoreUnavailable;
                return false;
            }

            if (activeProfile.RequireBedroll && !HasBedrollAvailable())
            {
                failureReason = CCS_SleepFailureReason.MissingBedroll;
                return false;
            }

            if (IsAlreadyRested())
            {
                failureReason = CCS_SleepFailureReason.AlreadyRested;
                return false;
            }

            if (activeProfile.RequireShelter && !ResolveIsSheltered())
            {
                failureReason = CCS_SleepFailureReason.UnsafeConditions;
                return false;
            }

            return true;
        }

        private bool HasBedrollAvailable()
        {
            if (activeProfile == null)
            {
                return false;
            }

            if (!activeProfile.RequireBedroll)
            {
                return true;
            }

            if (activeProfile.BedrollItemDefinition == null)
            {
                return false;
            }

            if (inventoryService != null
                && inventoryService.IsInitialized
                && inventoryService.GetQuantity(activeProfile.BedrollItemDefinition) > 0)
            {
                return true;
            }

            if (equipmentService != null && equipmentService.IsInitialized)
            {
                CCS_EquippedItem equippedBedroll =
                    equipmentService.GetEquippedItem(CCS_EquipmentSlotType.Bedroll);
                if (equippedBedroll?.ItemDefinition == activeProfile.BedrollItemDefinition)
                {
                    return true;
                }

                if (activeProfile.BedrollEquipmentDefinition != null
                    && equippedBedroll?.EquipmentDefinition == activeProfile.BedrollEquipmentDefinition)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsAlreadyRested()
        {
            if (survivalCoreService == null
                || !survivalCoreService.TryGetSnapshot(CCS_SurvivalStatType.Fatigue, out CCS_SurvivalStatSnapshot fatigueSnapshot))
            {
                return false;
            }

            return fatigueSnapshot.CurrentValue
                <= fatigueSnapshot.MinValue + CCS_SurvivalStatUtility.DepletionEpsilon;
        }

        private bool ResolveIsSheltered()
        {
            if (shelterService == null || !shelterService.IsInitialized)
            {
                return false;
            }

            return shelterService.GetSnapshot().IsSheltered;
        }

        private void ApplySleepStatDrain(float sleepHours)
        {
            if (survivalCoreService?.ActiveProfile == null
                || timeOfDayService?.ActiveProfile == null
                || sleepHours <= 0f)
            {
                return;
            }

            float realSecondsPerGameHour = timeOfDayService.ActiveProfile.RealSecondsPerGameDay / 24f;
            float simulatedRealSeconds = sleepHours * realSecondsPerGameHour;

            float hungerDrain = survivalCoreService.ActiveProfile.HungerDrainPerSecond
                * simulatedRealSeconds
                * activeProfile.HungerDrainDuringSleepMultiplier;
            if (hungerDrain > CCS_SurvivalStatUtility.DepletionEpsilon)
            {
                survivalCoreService.TryApplyModifier(
                    CCS_SurvivalStatType.Hunger,
                    CCS_SurvivalStatModifier.Add(-hungerDrain));
            }

            float thirstDrainPerSecond = ResolveThirstDrainPerSecond(survivalCoreService.ActiveProfile);
            float thirstDrain = thirstDrainPerSecond
                * simulatedRealSeconds
                * activeProfile.ThirstDrainDuringSleepMultiplier;
            if (thirstDrain > CCS_SurvivalStatUtility.DepletionEpsilon)
            {
                survivalCoreService.TryApplyModifier(
                    CCS_SurvivalStatType.Thirst,
                    CCS_SurvivalStatModifier.Add(-thirstDrain));
            }
        }

        private static float ResolveThirstDrainPerSecond(CCS_SurvivalCoreProfile profile)
        {
            if (profile?.DecayDefinitions == null)
            {
                return 0f;
            }

            for (int index = 0; index < profile.DecayDefinitions.Count; index++)
            {
                CCS_SurvivalStatDecayDefinition decayDefinition = profile.DecayDefinitions[index];
                if (decayDefinition == null || decayDefinition.StatType != CCS_SurvivalStatType.Thirst)
                {
                    continue;
                }

                return decayDefinition.SubtractPerSecond
                    ? decayDefinition.ChangePerSecond
                    : 0f;
            }

            return 0f;
        }

        private static string BuildSuccessMessage(
            float sleepHours,
            float fatigueRestored,
            bool usedPoorShelterPenalty)
        {
            if (usedPoorShelterPenalty)
            {
                return $"Slept {sleepHours:0} hours. Rested, but shelter was poor. Fatigue restored {fatigueRestored:0}.";
            }

            return $"Slept {sleepHours:0} hours. Fatigue restored {fatigueRestored:0}.";
        }

        private CCS_SleepResult FailSleep(CCS_SleepFailureReason failureReason, string message)
        {
            lastFailureReason = failureReason;
            lastMessage = message ?? string.Empty;
            lastHoursSlept = 0f;
            lastFatigueRestored = 0f;

            CCS_SleepResult result = CCS_SleepResult.Failure(failureReason, lastMessage);
            RaiseSleepFailed(result);
            return result;
        }

        private void RaiseSleepCompleted(CCS_SleepResult result)
        {
            SleepCompleted?.Invoke(new CCS_SleepEventArgs(result, CreateSnapshot(), result.Message));
        }

        private void RaiseSleepFailed(CCS_SleepResult result)
        {
            SleepFailed?.Invoke(new CCS_SleepEventArgs(result, CreateSnapshot(), result.Message));
        }

        #endregion
    }
}
