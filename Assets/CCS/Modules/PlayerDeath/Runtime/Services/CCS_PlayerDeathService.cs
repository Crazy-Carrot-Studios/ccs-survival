using CCS.Core;
using CCS.Modules.CharacterController;
using CCS.Modules.SurvivalCore;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerDeathService
// CATEGORY: Modules / PlayerDeath / Runtime / Services
// PURPOSE: Monitors hunger/thirst depletion and handles respawn at bootstrap spawn points.
// PLACEMENT: Registered as CCS_ISurvivalService by survival gameplay composition wiring.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: No UI in 1.0.1. Freezes movement during death handling only.
// =============================================================================

namespace CCS.Modules.PlayerDeath
{
    public sealed class CCS_PlayerDeathService : CCS_ISurvivalService, CCS_IUpdatable
    {
        private const string LogPrefix = "[CCS_PlayerDeathService]";

        #region Variables

        private CCS_PlayerDeathProfile activeProfile;
        private CCS_SurvivalCoreService survivalCoreService;
        private CCS_CharacterMovementService movementService;
        private Transform playerTransform;
        private bool isInitialized;
        private bool isPlayerDead;

        #endregion

        #region Events

        public event PlayerDiedHandler PlayerDied;
        public event PlayerRespawnedHandler PlayerRespawned;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public bool IsPlayerDead => isPlayerDead;

        public CCS_PlayerDeathProfile ActiveProfile => activeProfile;

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

        public void InitializeFromProfile(CCS_PlayerDeathProfile profile)
        {
            activeProfile = profile;
            isInitialized = profile != null;
        }

        public void BindGameplayServices(
            CCS_SurvivalCoreService survivalCore,
            CCS_CharacterMovementService movement,
            Transform playerRoot)
        {
            UnbindSurvivalCore();
            survivalCoreService = survivalCore;
            movementService = movement;
            playerTransform = playerRoot;
            BindSurvivalCore();
        }

        public void Tick(float deltaTime)
        {
            if (!isInitialized || isPlayerDead || survivalCoreService == null)
            {
                return;
            }

            if (IsHungerOrThirstDepleted())
            {
                HandlePlayerDeath(ResolveDeathCauseMessage());
            }
        }

        #endregion

        #region Private Methods

        private void BindSurvivalCore()
        {
            if (survivalCoreService == null)
            {
                return;
            }

            survivalCoreService.StatDepleted += HandleStatDepleted;
        }

        private void UnbindSurvivalCore()
        {
            if (survivalCoreService == null)
            {
                return;
            }

            survivalCoreService.StatDepleted -= HandleStatDepleted;
        }

        private void HandleStatDepleted(CCS_SurvivalStatType statType, CCS_SurvivalStatSnapshot snapshot)
        {
            if (isPlayerDead)
            {
                return;
            }

            if (statType != CCS_SurvivalStatType.Hunger && statType != CCS_SurvivalStatType.Thirst)
            {
                return;
            }

            string cause = statType == CCS_SurvivalStatType.Hunger
                ? "You starved."
                : "You died of dehydration.";

            HandlePlayerDeath(cause);
        }

        private bool IsHungerOrThirstDepleted()
        {
            if (survivalCoreService == null)
            {
                return false;
            }

            bool hungerDepleted = survivalCoreService.TryGetSnapshot(
                    CCS_SurvivalStatType.Hunger,
                    out CCS_SurvivalStatSnapshot hunger)
                && hunger.CurrentValue <= hunger.MinValue + CCS_SurvivalStatUtility.DepletionEpsilon;

            bool thirstDepleted = survivalCoreService.TryGetSnapshot(
                    CCS_SurvivalStatType.Thirst,
                    out CCS_SurvivalStatSnapshot thirst)
                && thirst.CurrentValue <= thirst.MinValue + CCS_SurvivalStatUtility.DepletionEpsilon;

            return hungerDepleted || thirstDepleted;
        }

        private string ResolveDeathCauseMessage()
        {
            if (survivalCoreService != null
                && survivalCoreService.TryGetSnapshot(CCS_SurvivalStatType.Hunger, out CCS_SurvivalStatSnapshot hunger)
                && hunger.CurrentValue <= hunger.MinValue + CCS_SurvivalStatUtility.DepletionEpsilon)
            {
                return "You starved.";
            }

            return "You died of dehydration.";
        }

        private void HandlePlayerDeath(string causeMessage)
        {
            if (isPlayerDead)
            {
                return;
            }

            isPlayerDead = true;
            SetMovementFrozen(true);
            Vector3 deathPosition = playerTransform != null ? playerTransform.position : Vector3.zero;
            Debug.Log($"{LogPrefix} {causeMessage}");
            PlayerDied?.Invoke(new CCS_PlayerDeathEventArgs(causeMessage, deathPosition));
            PerformRespawn(causeMessage);
        }

        private void PerformRespawn(string causeMessage)
        {
            string spawnId = activeProfile != null ? activeProfile.DefaultSpawnId : string.Empty;
            CCS_PlayerRespawnPoint respawnPoint = ResolveRespawnPoint(spawnId);
            Vector3 spawnPosition = respawnPoint != null
                ? respawnPoint.SpawnPosition
                : deathFallbackPosition();
            Quaternion spawnRotation = respawnPoint != null
                ? respawnPoint.SpawnRotation
                : Quaternion.identity;

            ApplyPlayerTransform(spawnPosition, spawnRotation);
            RestoreSafeNeeds();
            SetMovementFrozen(false);
            isPlayerDead = false;
            LogDebug($"Player respawned at '{spawnId}'.");
            PlayerRespawned?.Invoke(new CCS_PlayerDeathEventArgs(causeMessage, spawnPosition, spawnId));
        }

        private Vector3 deathFallbackPosition()
        {
            return playerTransform != null ? playerTransform.position : Vector3.zero;
        }

        private void RestoreSafeNeeds()
        {
            if (survivalCoreService == null || activeProfile == null)
            {
                return;
            }

            survivalCoreService.TryRestoreSavedNeeds(
                activeProfile.RespawnHunger,
                activeProfile.RespawnThirst,
                activeProfile.RespawnStamina);
        }

        private void ApplyPlayerTransform(Vector3 position, Quaternion rotation)
        {
            if (playerTransform == null)
            {
                return;
            }

            UnityEngine.CharacterController controller =
                playerTransform.GetComponent<UnityEngine.CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
            }

            playerTransform.SetPositionAndRotation(position, rotation);

            if (controller != null)
            {
                controller.enabled = true;
            }
        }

        private CCS_PlayerRespawnPoint ResolveRespawnPoint(string spawnId)
        {
            CCS_PlayerRespawnPoint[] points =
                CCS_SurvivalSceneQueryUtility.FindActiveObjectsByType<CCS_PlayerRespawnPoint>();
            if (points == null || points.Length == 0)
            {
                return null;
            }

            for (int index = 0; index < points.Length; index++)
            {
                CCS_PlayerRespawnPoint point = points[index];
                if (point != null && point.SpawnId == spawnId)
                {
                    return point;
                }
            }

            return points[0];
        }

        private void SetMovementFrozen(bool frozen)
        {
            movementService?.SetMovementLocked(frozen);
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
