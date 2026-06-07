using CCS.Modules.Interaction;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcDialogueStubInteractable
// CATEGORY: Modules / NPCs / Runtime / Components
// PURPOSE: Workforce NPC interaction entry point for profile-driven dialogue stubs.
// PLACEMENT: Added to population placeholder actors with identity.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.9.0 — representatives use CCS_NpcServiceRepresentativeInteractable.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public sealed class CCS_NpcDialogueStubInteractable : MonoBehaviour, CCS_IInteractableResultProvider
    {
        [SerializeField] private string settlementId = string.Empty;

        [SerializeField] private string npcIdentityId = string.Empty;

        [SerializeField] private float interactionDistance = 3f;

        public void Configure(string anchorSettlementId, string identityId)
        {
            settlementId = anchorSettlementId ?? string.Empty;
            npcIdentityId = identityId ?? string.Empty;
        }

        public string GetInteractionDisplayName()
        {
            if (transform.GetComponent<CCS_IPopulationPlaceholderIdentityHost>() is CCS_IPopulationPlaceholderIdentityHost host
                && host.HasIdentity)
            {
                return host.DisplayName;
            }

            return "NPC";
        }

        public bool CanInteract()
        {
            return isActiveAndEnabled
                && !string.IsNullOrWhiteSpace(npcIdentityId)
                && !string.IsNullOrWhiteSpace(settlementId);
        }

        public void Interact()
        {
            TryInteract();
        }

        public float GetInteractionDistance()
        {
            return interactionDistance < 0.5f ? 3f : interactionDistance;
        }

        public bool TryInteract()
        {
            if (!CanInteract())
            {
                CCS_NpcDialogueStubDebugHud.NotifyDialogueResult(new CCS_NpcDialogueStubResult
                {
                    ResultType = CCS_NpcDialogueStubResultType.InvalidTarget,
                    Message = "Workforce NPC dialogue target unavailable."
                });
                return false;
            }

            CCS_INpcMovementHost host = transform.GetComponent<CCS_INpcMovementHost>();
            if (host == null || !host.HasIdentity)
            {
                CCS_NpcDialogueStubDebugHud.NotifyDialogueResult(new CCS_NpcDialogueStubResult
                {
                    ResultType = CCS_NpcDialogueStubResultType.NoIdentity,
                    Message = "NPC identity missing for dialogue stub."
                });
                return false;
            }

            CCS_NpcDialogueStubResult result = CCS_NpcDialogueStubRuntimeBridge.ResolveDialogueForHost?.Invoke(host)
                ?? new CCS_NpcDialogueStubResult
                {
                    ResultType = CCS_NpcDialogueStubResultType.Failed,
                    Message = "Dialogue stub service unavailable."
                };

            if (!result.IsSuccess)
            {
                CCS_NpcDialogueStubDebugHud.NotifyDialogueResult(result);
            }

            return result.IsSuccess;
        }
    }
}
