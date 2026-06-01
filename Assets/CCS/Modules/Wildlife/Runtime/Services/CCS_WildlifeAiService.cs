using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WildlifeAiService
// CATEGORY: Modules / Wildlife / Runtime / Services
// PURPOSE: Registers passive wildlife agents and exposes debug snapshots for HUD wiring.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration from wildlife AI profile.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No singletons. Agents register explicitly on enable.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    public sealed class CCS_WildlifeAiService : CCS_ISurvivalService
    {
        #region Variables

        private readonly List<CCS_WildlifeAgent> registeredAgents = new List<CCS_WildlifeAgent>(8);
        private CCS_WildlifeAiProfile activeProfile;
        private bool isInitialized;

        #endregion

        #region Events

        public event System.Action WildlifeAiStateChanged;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_WildlifeAiProfile ActiveProfile => activeProfile;

        public int RegisteredAgentCount => registeredAgents.Count;

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

        public void InitializeFromProfile(CCS_WildlifeAiProfile profile)
        {
            if (profile == null)
            {
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_WildlifeAiValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"[CCS_WildlifeAiService] Profile validation warning: {validation.Message}");
            }

            activeProfile = profile;
            isInitialized = true;
        }

        public void RegisterAgent(CCS_WildlifeAgent agent)
        {
            if (agent == null || registeredAgents.Contains(agent))
            {
                return;
            }

            registeredAgents.Add(agent);
            WildlifeAiStateChanged?.Invoke();
        }

        public void UnregisterAgent(CCS_WildlifeAgent agent)
        {
            if (agent == null)
            {
                return;
            }

            if (registeredAgents.Remove(agent))
            {
                WildlifeAiStateChanged?.Invoke();
            }
        }

        public void NotifyAgentStateChanged(CCS_WildlifeAgent agent)
        {
            if (agent == null || !registeredAgents.Contains(agent))
            {
                return;
            }

            WildlifeAiStateChanged?.Invoke();
        }

        public IReadOnlyList<CCS_WildlifeAiSnapshot> CreateSnapshots()
        {
            List<CCS_WildlifeAiSnapshot> snapshots = new List<CCS_WildlifeAiSnapshot>(registeredAgents.Count);
            for (int index = 0; index < registeredAgents.Count; index++)
            {
                CCS_WildlifeAgent agent = registeredAgents[index];
                if (agent == null)
                {
                    continue;
                }

                snapshots.Add(agent.CreateSnapshot());
            }

            snapshots.Sort(CompareSnapshots);
            return snapshots;
        }

        public string BuildDebugLabel()
        {
            IReadOnlyList<CCS_WildlifeAiSnapshot> snapshots = CreateSnapshots();
            if (snapshots.Count == 0)
            {
                return "Wildlife:\n--";
            }

            System.Text.StringBuilder builder = new System.Text.StringBuilder(128);
            builder.AppendLine("Wildlife:");
            for (int index = 0; index < snapshots.Count; index++)
            {
                builder.AppendLine(snapshots[index].DebugLine);
            }

            return builder.ToString().TrimEnd();
        }

        #endregion

        #region Private Methods

        private static int CompareSnapshots(CCS_WildlifeAiSnapshot left, CCS_WildlifeAiSnapshot right)
        {
            return string.Compare(left.AgentDisplayName, right.AgentDisplayName, System.StringComparison.Ordinal);
        }

        #endregion
    }
}
