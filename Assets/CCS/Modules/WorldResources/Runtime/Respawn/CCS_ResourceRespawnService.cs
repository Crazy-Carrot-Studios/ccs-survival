using System;
using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ResourceRespawnService
// CATEGORY: Modules / WorldResources / Runtime / Respawn
// PURPOSE: Tracks depleted nodes and restores node state when timers complete.
// PLACEMENT: Used by CCS_HarvestableResource and future world resource managers.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No world streaming or save integration in 0.5.1 foundation.
// =============================================================================

namespace CCS.Modules.WorldResources
{
    public sealed class CCS_ResourceRespawnService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_ResourceRespawnService]";

        #region Variables

        private readonly Dictionary<string, CCS_ResourceRespawnState> activeRespawnStates =
            new Dictionary<string, CCS_ResourceRespawnState>();

        private CCS_WorldResourceProfile activeProfile;
        private bool isInitialized;

        #endregion

        #region Events

        public event ResourceRespawnedHandler ResourceRespawned;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_WorldResourceProfile ActiveProfile => activeProfile;

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

        public void InitializeFromProfile(CCS_WorldResourceProfile profile)
        {
            if (profile == null)
            {
                Debug.LogWarning($"{LogPrefix} InitializeFromProfile called with null profile.");
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_WorldResourceValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            activeProfile = profile;
            isInitialized = true;
        }

        public bool IsTrackingNode(string nodeKey)
        {
            return !string.IsNullOrWhiteSpace(nodeKey) && activeRespawnStates.ContainsKey(nodeKey);
        }

        public void RegisterDepletedNode(
            string nodeKey,
            CCS_ResourceDefinition resourceDefinition,
            CCS_ResourceNodeState nodeState,
            Action<CCS_ResourceNodeState> onRespawnReady)
        {
            if (!EnsureInitialized() || string.IsNullOrWhiteSpace(nodeKey) || resourceDefinition == null || nodeState == null)
            {
                return;
            }

            if (activeProfile != null && !activeProfile.EnableRespawn)
            {
                return;
            }

            float multiplier = activeProfile != null ? activeProfile.GlobalRespawnMultiplier : 1f;
            if (multiplier < 0f)
            {
                multiplier = 0f;
            }

            float respawnDuration = resourceDefinition.RespawnTimeSeconds * multiplier;
            nodeState.RespawnRemainingSeconds = respawnDuration;

            CCS_ResourceRespawnState respawnState = new CCS_ResourceRespawnState(
                nodeKey,
                resourceDefinition,
                respawnDuration,
                completedState =>
                {
                    onRespawnReady?.Invoke(nodeState);
                    RaiseResourceRespawned(resourceDefinition, nodeState, nodeKey);
                });

            activeRespawnStates[nodeKey] = respawnState;
        }

        public float GetRemainingSeconds(string nodeKey)
        {
            if (string.IsNullOrWhiteSpace(nodeKey))
            {
                return 0f;
            }

            return activeRespawnStates.TryGetValue(nodeKey, out CCS_ResourceRespawnState respawnState)
                ? respawnState.RemainingSeconds
                : 0f;
        }

        public void TickNode(string nodeKey, float deltaSeconds)
        {
            if (!EnsureInitialized() || string.IsNullOrWhiteSpace(nodeKey))
            {
                return;
            }

            if (!activeRespawnStates.TryGetValue(nodeKey, out CCS_ResourceRespawnState respawnState))
            {
                return;
            }

            if (deltaSeconds <= 0f)
            {
                return;
            }

            respawnState.RemainingSeconds -= deltaSeconds;
            if (respawnState.RemainingSeconds > 0f)
            {
                return;
            }

            activeRespawnStates.Remove(nodeKey);
            respawnState.OnRespawnReady?.Invoke(respawnState);
        }

        public void ClearNode(string nodeKey)
        {
            if (string.IsNullOrWhiteSpace(nodeKey))
            {
                return;
            }

            activeRespawnStates.Remove(nodeKey);
        }

        #endregion

        #region Private Methods

        private bool EnsureInitialized()
        {
            return isInitialized;
        }

        private void RaiseResourceRespawned(
            CCS_ResourceDefinition resourceDefinition,
            CCS_ResourceNodeState nodeState,
            string nodeKey)
        {
            ResourceRespawned?.Invoke(
                new CCS_ResourceEventArgs(resourceDefinition, nodeState, nodeKey, message: "Resource respawned."));
        }

        #endregion
    }
}
