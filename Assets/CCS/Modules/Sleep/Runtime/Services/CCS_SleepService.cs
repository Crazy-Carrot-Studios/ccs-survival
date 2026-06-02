using System;
using System.Collections.Generic;
using CCS.Core;
using CCS.Modules.Crafting;
using CCS.Modules.Equipment;
using CCS.Modules.Inventory;
using CCS.Modules.Shelter;
using CCS.Modules.SurvivalCore;
using CCS.Modules.TimeOfDay;
using CCS.Survival;
using CCS.Survival.Player;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SleepService
// CATEGORY: Modules / Sleep / Runtime / Services
// PURPOSE: Validates and executes sleep requests, advancing time and restoring fatigue.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration from sleep profile.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Milestone 1.1.3 adds placeable bedroll sleep spots, save restore, and respawn assignment.
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
        private CCS_CampService campService;
        private CCS_PlayerInventoryService inventoryService;
        private CCS_PlayerEquipmentService equipmentService;
        private CCS_CraftingService craftingService;

        private readonly Dictionary<string, CCS_SleepSpot> registeredSleepSpots =
            new Dictionary<string, CCS_SleepSpot>(StringComparer.Ordinal);

        private readonly List<CCS_SleepSpot> dynamicallySpawnedSleepSpots = new List<CCS_SleepSpot>();

        private float lastHoursSlept;
        private float lastFatigueRestored;
        private CCS_SleepFailureReason lastFailureReason = CCS_SleepFailureReason.None;
        private string lastMessage = string.Empty;
        private string assignedRespawnSpawnId = string.Empty;
        private bool isInitialized;

        #endregion

        #region Events

        public event SleepStartedHandler SleepStarted;
        public event SleepCompletedHandler SleepCompleted;
        public event SleepFailedHandler SleepFailed;
        public event SleepRespawnPointAssignedHandler SleepRespawnPointAssigned;
        public event SleepStateRestoredHandler SleepStateRestored;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_SleepProfile ActiveProfile => activeProfile;

        public string AssignedRespawnSpawnId => assignedRespawnSpawnId ?? string.Empty;

        public int RegisteredSleepSpotCount => registeredSleepSpots.Count;

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

        public void BindCampService(CCS_CampService service)
        {
            campService = service;
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
            if (campService != null && campService.IsInitialized)
            {
                fatigueMultiplier *= campService.GetSleepBonusMultiplier();
            }

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

        public void RegisterSleepSpot(CCS_SleepSpot sleepSpot)
        {
            if (sleepSpot == null || string.IsNullOrWhiteSpace(sleepSpot.InstanceId))
            {
                return;
            }

            registeredSleepSpots[sleepSpot.InstanceId] = sleepSpot;
            LogDebug($"Registered sleep spot {sleepSpot.InstanceId}.");
        }

        public void UnregisterSleepSpot(CCS_SleepSpot sleepSpot)
        {
            if (sleepSpot == null || string.IsNullOrWhiteSpace(sleepSpot.InstanceId))
            {
                return;
            }

            registeredSleepSpots.Remove(sleepSpot.InstanceId);
            dynamicallySpawnedSleepSpots.Remove(sleepSpot);
            LogDebug($"Unregistered sleep spot {sleepSpot.InstanceId}.");
        }

        public bool TrySleep(CCS_SleepSpot sleepSpot)
        {
            if (sleepSpot == null)
            {
                return FailSpotSleep(null, "Sleep spot is missing.");
            }

            if (!EnsureInitialized() || activeProfile == null)
            {
                return FailSpotSleep(sleepSpot, "Sleep profile is unavailable.");
            }

            if (!sleepSpot.CanSleep())
            {
                return FailSpotSleep(sleepSpot, "Cannot sleep at this bedroll right now.");
            }

            if (survivalCoreService == null || !survivalCoreService.IsInitialized)
            {
                return FailSpotSleep(sleepSpot, "Survival core service is unavailable.");
            }

            RaiseSleepStarted(sleepSpot);
            sleepSpot.SetSleepingState(true);

            float sleepHours = activeProfile.DefaultSleepHours;
            if (timeOfDayService != null && timeOfDayService.IsInitialized && sleepHours > 0f)
            {
                timeOfDayService.AdvanceTimeByHours(sleepHours);
            }

            ApplySpotNeedsRecovery();
            ApplySpotFatigueRecovery(sleepHours);

            lastHoursSlept = sleepHours;
            lastFatigueRestored = activeProfile.FatigueRestorePerHour * sleepHours;
            lastFailureReason = CCS_SleepFailureReason.None;
            lastMessage = BuildSpotSuccessMessage(sleepSpot, sleepHours);

            bool respawnAssigned = false;
            if (activeProfile.AssignRespawnPointOnSleep)
            {
                respawnAssigned = TryAssignRespawnPoint(sleepSpot);
            }

            sleepSpot.SetSleepingState(false);
            RaiseSpotSleepCompleted(sleepSpot, respawnAssigned);
            LogDebug(
                $"Sleep completed at '{sleepSpot.DisplayName}' ({sleepSpot.InstanceId}) "
                + $"duration foundation {activeProfile.SleepDurationSeconds:0}s.");
            return true;
        }

        public CCS_SleepSpotSaveState[] CaptureWorldState()
        {
            if (registeredSleepSpots.Count == 0)
            {
                return Array.Empty<CCS_SleepSpotSaveState>();
            }

            List<CCS_SleepSpotSaveState> records = new List<CCS_SleepSpotSaveState>(registeredSleepSpots.Count);
            foreach (KeyValuePair<string, CCS_SleepSpot> entry in registeredSleepSpots)
            {
                CCS_SleepSpot sleepSpot = entry.Value;
                if (sleepSpot == null || !dynamicallySpawnedSleepSpots.Contains(sleepSpot))
                {
                    continue;
                }

                records.Add(sleepSpot.CaptureState());
            }

            return records.ToArray();
        }

        public void RestoreWorldState(CCS_SleepSpotSaveState[] saveStates)
        {
            ClearDynamicallySpawnedSleepSpots();

            if (saveStates == null || saveStates.Length == 0)
            {
                RaiseSleepStateRestored(null, true, "No sleep spots to restore.");
                return;
            }

            int restoredCount = 0;
            for (int index = 0; index < saveStates.Length; index++)
            {
                CCS_SleepSpotSaveState saveState = saveStates[index];
                if (saveState == null || string.IsNullOrWhiteSpace(saveState.instanceId))
                {
                    continue;
                }

                if (TryFindRegisteredSleepSpot(saveState.instanceId, out CCS_SleepSpot existingSpot))
                {
                    existingSpot.RestoreState(saveState);
                    if (saveState.isAssignedRespawn)
                    {
                        TryAssignRespawnPoint(existingSpot);
                    }

                    restoredCount++;
                    continue;
                }

                CCS_SleepSpotDefinition definition = ResolveSleepSpotDefinition(saveState.sleepSpotDefinitionId);
                if (definition == null || definition.PrefabReference == null)
                {
                    continue;
                }

                Vector3 position = new Vector3(saveState.positionX, saveState.positionY, saveState.positionZ);
                Quaternion rotation = new Quaternion(
                    saveState.rotationX,
                    saveState.rotationY,
                    saveState.rotationZ,
                    saveState.rotationW);

                CCS_SleepSpot spawnedSpot = SpawnSleepSpot(
                    definition,
                    position,
                    rotation,
                    saveState.instanceId,
                    markDynamicSpawn: true);
                if (spawnedSpot == null)
                {
                    continue;
                }

                spawnedSpot.RestoreState(saveState);
                if (saveState.isAssignedRespawn)
                {
                    TryAssignRespawnPoint(spawnedSpot);
                }

                restoredCount++;
            }

            RaiseSleepStateRestored(null, restoredCount > 0, $"Restored {restoredCount} sleep spot(s).");
        }

        public CCS_SleepSpot TryPlaceDefaultSleepSpotNearPlayer()
        {
            if (!EnsureInitialized() || activeProfile?.DefaultSleepSpotDefinition == null)
            {
                return null;
            }

            if (!TryResolvePlayerPosition(out Vector3 playerPosition, out Vector3 playerForward))
            {
                return null;
            }

            Vector3 spawnPosition = playerPosition + playerForward * 2f + Vector3.up * 0.1f;
            Quaternion spawnRotation = Quaternion.LookRotation(playerForward, Vector3.up);
            return SpawnSleepSpot(
                activeProfile.DefaultSleepSpotDefinition,
                spawnPosition,
                spawnRotation,
                null,
                markDynamicSpawn: true);
        }

        public bool TryGetNearestSleepSpotWithinRadius(Vector3 origin, float radius, out CCS_SleepSpot sleepSpot)
        {
            sleepSpot = null;
            float nearestDistance = float.MaxValue;
            foreach (KeyValuePair<string, CCS_SleepSpot> entry in registeredSleepSpots)
            {
                CCS_SleepSpot candidate = entry.Value;
                if (candidate == null)
                {
                    continue;
                }

                float distance = Vector3.Distance(origin, candidate.transform.position);
                if (distance <= radius && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    sleepSpot = candidate;
                }
            }

            return sleepSpot != null;
        }

        public bool TrySleepAtNearestSpot()
        {
            if (!TryResolvePlayerPosition(out Vector3 playerPosition, out _))
            {
                return false;
            }

            CCS_SleepSpot nearest = null;
            float nearestDistance = float.MaxValue;
            foreach (KeyValuePair<string, CCS_SleepSpot> entry in registeredSleepSpots)
            {
                CCS_SleepSpot sleepSpot = entry.Value;
                if (sleepSpot == null || !sleepSpot.CanSleep())
                {
                    continue;
                }

                float distance = Vector3.Distance(playerPosition, sleepSpot.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = sleepSpot;
                }
            }

            return nearest != null && TrySleep(nearest);
        }

        public void ApplySavedAssignedRespawn(string spawnId)
        {
            if (string.IsNullOrWhiteSpace(spawnId))
            {
                return;
            }

            assignedRespawnSpawnId = spawnId;
            foreach (KeyValuePair<string, CCS_SleepSpot> entry in registeredSleepSpots)
            {
                CCS_SleepSpot sleepSpot = entry.Value;
                if (sleepSpot != null && sleepSpot.RespawnSpawnId == spawnId)
                {
                    sleepSpot.SetAssignedRespawn(true);
                }
            }
        }

        public CCS_SleepSpot SpawnSleepSpot(
            CCS_SleepSpotDefinition definition,
            Vector3 position,
            Quaternion rotation,
            string instanceId,
            bool markDynamicSpawn)
        {
            if (!EnsureInitialized() || definition == null || definition.PrefabReference == null)
            {
                return null;
            }

            GameObject instance = UnityEngine.Object.Instantiate(definition.PrefabReference, position, rotation);
            if (instance == null)
            {
                return null;
            }

            CCS_SleepSpot sleepSpot = instance.GetComponent<CCS_SleepSpot>();
            if (sleepSpot == null)
            {
                sleepSpot = instance.AddComponent<CCS_SleepSpot>();
            }

            if (instance.GetComponent<CCS_SleepSpotInteractable>() == null)
            {
                instance.AddComponent<CCS_SleepSpotInteractable>();
            }

            sleepSpot.ConfigureFromDefinition(definition, instanceId);
            RegisterSleepSpot(sleepSpot);

            if (markDynamicSpawn && !dynamicallySpawnedSleepSpots.Contains(sleepSpot))
            {
                dynamicallySpawnedSleepSpots.Add(sleepSpot);
            }

            LogDebug($"Spawned sleep spot '{definition.DisplayName}' at {position}.");
            return sleepSpot;
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

        private bool FailSpotSleep(CCS_SleepSpot sleepSpot, string message)
        {
            lastFailureReason = CCS_SleepFailureReason.UnsafeConditions;
            lastMessage = message ?? string.Empty;
            SleepFailed?.Invoke(new CCS_SleepEventArgs(sleepSpot, false, lastMessage));
            LogDebug(lastMessage);
            return false;
        }

        private void ApplySpotNeedsRecovery()
        {
            if (survivalCoreService == null || activeProfile == null)
            {
                return;
            }

            float hunger = activeProfile.HungerRecoveryAmount;
            float thirst = activeProfile.ThirstRecoveryAmount;
            if (survivalCoreService.TryGetSnapshot(CCS_SurvivalStatType.Hunger, out CCS_SurvivalStatSnapshot hungerSnapshot))
            {
                hunger = Mathf.Min(
                    hungerSnapshot.MaxValue,
                    hungerSnapshot.CurrentValue + activeProfile.HungerRecoveryAmount);
            }

            if (survivalCoreService.TryGetSnapshot(CCS_SurvivalStatType.Thirst, out CCS_SurvivalStatSnapshot thirstSnapshot))
            {
                thirst = Mathf.Min(
                    thirstSnapshot.MaxValue,
                    thirstSnapshot.CurrentValue + activeProfile.ThirstRecoveryAmount);
            }

            survivalCoreService.TryRestoreSavedNeeds(
                hunger,
                thirst,
                activeProfile.StaminaRecoveryAmount);
        }

        private void ApplySpotFatigueRecovery(float sleepHours)
        {
            if (survivalCoreService == null || activeProfile == null || sleepHours <= 0f)
            {
                return;
            }

            float fatigueRestored = activeProfile.FatigueRestorePerHour * sleepHours;
            if (fatigueRestored > 0f)
            {
                survivalCoreService.TryApplyModifier(
                    CCS_SurvivalStatType.Fatigue,
                    CCS_SurvivalStatModifier.Add(-fatigueRestored));
            }
        }

        private bool TryAssignRespawnPoint(CCS_SleepSpot sleepSpot)
        {
            if (sleepSpot == null)
            {
                return false;
            }

            sleepSpot.SetAssignedRespawn(true);
            assignedRespawnSpawnId = sleepSpot.RespawnSpawnId;
            RaiseSleepRespawnPointAssigned(sleepSpot);
            LogDebug($"Assigned respawn point '{assignedRespawnSpawnId}'.");
            return true;
        }

        private void ClearDynamicallySpawnedSleepSpots()
        {
            for (int index = dynamicallySpawnedSleepSpots.Count - 1; index >= 0; index--)
            {
                CCS_SleepSpot sleepSpot = dynamicallySpawnedSleepSpots[index];
                if (sleepSpot == null)
                {
                    dynamicallySpawnedSleepSpots.RemoveAt(index);
                    continue;
                }

                UnregisterSleepSpot(sleepSpot);
                UnityEngine.Object.Destroy(sleepSpot.gameObject);
            }

            dynamicallySpawnedSleepSpots.Clear();
        }

        private bool TryFindRegisteredSleepSpot(string instanceId, out CCS_SleepSpot sleepSpot)
        {
            sleepSpot = null;
            if (string.IsNullOrWhiteSpace(instanceId))
            {
                return false;
            }

            return registeredSleepSpots.TryGetValue(instanceId, out sleepSpot) && sleepSpot != null;
        }

        private CCS_SleepSpotDefinition ResolveSleepSpotDefinition(string sleepSpotDefinitionId)
        {
            if (activeProfile?.DefaultSleepSpotDefinition != null
                && activeProfile.DefaultSleepSpotDefinition.SleepSpotId == sleepSpotDefinitionId)
            {
                return activeProfile.DefaultSleepSpotDefinition;
            }

            return activeProfile?.DefaultSleepSpotDefinition;
        }

        private static bool TryResolvePlayerPosition(out Vector3 position, out Vector3 forward)
        {
            position = Vector3.zero;
            forward = Vector3.forward;

            CCS_PlayerGameplayController[] players =
                CCS_SurvivalSceneQueryUtility.FindActiveObjectsByType<CCS_PlayerGameplayController>();
            if (players == null || players.Length == 0 || players[0] == null)
            {
                return false;
            }

            Transform playerTransform = players[0].transform;
            position = playerTransform.position;
            forward = playerTransform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f)
            {
                forward = Vector3.forward;
            }
            else
            {
                forward.Normalize();
            }

            return true;
        }

        private static string BuildSpotSuccessMessage(CCS_SleepSpot sleepSpot, float sleepHours)
        {
            string label = sleepSpot != null ? sleepSpot.DisplayName : "Bedroll";
            return $"Slept at {label} for {sleepHours:0} hours. Needs recovered.";
        }

        private void RaiseSleepStarted(CCS_SleepSpot sleepSpot)
        {
            SleepStarted?.Invoke(new CCS_SleepEventArgs(sleepSpot, true, "Sleep started."));
        }

        private void RaiseSpotSleepCompleted(CCS_SleepSpot sleepSpot, bool respawnAssigned)
        {
            CCS_SleepResult result = CCS_SleepResult.Success(
                lastHoursSlept,
                lastFatigueRestored,
                false,
                lastMessage);
            SleepCompleted?.Invoke(
                new CCS_SleepEventArgs(sleepSpot, true, lastMessage, result, CreateSnapshot()));
            if (respawnAssigned)
            {
                RaiseSleepRespawnPointAssigned(sleepSpot);
            }
        }

        private void RaiseSleepRespawnPointAssigned(CCS_SleepSpot sleepSpot)
        {
            SleepRespawnPointAssigned?.Invoke(
                new CCS_SleepEventArgs(sleepSpot, true, $"Respawn point assigned: {assignedRespawnSpawnId}."));
        }

        private void RaiseSleepStateRestored(CCS_SleepSpot sleepSpot, bool success, string message)
        {
            SleepStateRestored?.Invoke(new CCS_SleepEventArgs(sleepSpot, success, message));
        }

        private void LogDebug(string message)
        {
            if (activeProfile != null && activeProfile.EnableDebugLogging)
            {
                Debug.Log($"{LogPrefix} {message}");
            }
        }

        #endregion
    }
}
