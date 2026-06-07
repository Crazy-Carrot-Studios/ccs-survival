using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcDialogueStubService
// CATEGORY: Modules / NPCs / Runtime / Dialogue
// PURPOSE: Resolves profile-driven dialogue stub lines from identity and affiliation.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.9.0 — static profile data; no dialogue persistence.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public sealed class CCS_NpcDialogueStubService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_NpcDialogueStubService]";

        private CCS_NpcDialogueStubProfile activeProfile;
        private bool isInitialized;

        public bool IsInitialized => isInitialized;

        public CCS_NpcDialogueStubProfile ActiveProfile => activeProfile;

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_NpcDialogueStubProfile profile)
        {
            activeProfile = profile;
            isInitialized = profile != null;
            if (profile == null)
            {
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_NpcDialogueStubValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            BindRuntimeBridge();
        }

        public CCS_NpcDialogueStubResult TryResolveDialogue(CCS_NpcDialogueStubRequest request)
        {
            CCS_NpcDialogueStubResult result =
                CCS_NpcDialogueStubValidationUtility.ResolveDialogue(activeProfile, request);
            CCS_NpcDialogueStubRuntimeBridge.LastDialogueResult = result;
            if (result.IsSuccess)
            {
                CCS_NpcDialogueStubDebugHud.NotifyDialogueResult(result);
            }

            return result;
        }

        public CCS_NpcDialogueStubResult TryResolveDialogueForHost(CCS_INpcMovementHost host)
        {
            if (host == null)
            {
                return TryResolveDialogue(new CCS_NpcDialogueStubRequest());
            }

            CCS_NpcDialogueStubRequest request =
                CCS_NpcDialogueStubValidationUtility.BuildRequestFromHost(host);
            return TryResolveDialogue(request);
        }

        public bool TryResolveAndDisplayForHost(CCS_INpcMovementHost host)
        {
            CCS_NpcDialogueStubResult result = TryResolveDialogueForHost(host);
            return result.IsSuccess;
        }

        public void RefreshDialogueHosts()
        {
            CCS_PopulationPlaceholderIdentityBridge.ForEachMovementHost(host =>
            {
                if (host != null && host.HasIdentity && !host.IsServiceRepresentative)
                {
                    EnsureWorkforceInteractable(host);
                }
            });
        }

        private void BindRuntimeBridge()
        {
            CCS_NpcDialogueStubRuntimeBridge.ResolveDialogue = request => TryResolveDialogue(request);
            CCS_NpcDialogueStubRuntimeBridge.ResolveDialogueForHost = TryResolveDialogueForHost;
            CCS_NpcDialogueStubRuntimeBridge.ResolveAndDisplayForHost = TryResolveAndDisplayForHost;
            CCS_NpcDialogueStubRuntimeBridge.RefreshDialogueHosts = RefreshDialogueHosts;
        }

        private static void EnsureWorkforceInteractable(CCS_INpcMovementHost host)
        {
            if (host?.MovementTransform == null)
            {
                return;
            }

            CCS_NpcDialogueStubInteractable interactable =
                host.MovementTransform.GetComponent<CCS_NpcDialogueStubInteractable>();
            if (interactable == null)
            {
                interactable = host.MovementTransform.gameObject.AddComponent<CCS_NpcDialogueStubInteractable>();
            }

            interactable.Configure(host.SettlementId, host.NpcIdentityId);
        }
    }
}
