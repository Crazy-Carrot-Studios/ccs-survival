using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TestPickupInteractable
// CATEGORY: Modules / Interaction / Runtime / Components
// PURPOSE: Test pickup item that destroys itself after a successful interaction.
// PLACEMENT: PF_CCS_TestInteractable_PickupItem prefab root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Server-authoritative when Netcode is listening. Scanner raises InteractionCompleted.
// =============================================================================

namespace CCS.Modules.Interaction
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class CCS_TestPickupInteractable : NetworkBehaviour, CCS_IInteractable
    {
        #region Public Methods

        public bool CanInteract(CCS_InteractionRequest request)
        {
            Vector3 targetPosition = transform.position;
            float distance = Vector3.Distance(request.OriginPosition, targetPosition);
            return distance <= request.MaxRange + 0.75f;
        }

        public bool Interact(CCS_InteractionRequest request, out CCS_InteractionResult result)
        {
            ulong targetId = request.TargetNetworkObjectId;
            if (!CanInteract(request))
            {
                result = CCS_InteractionResult.Failure(targetId, "Pickup is out of range.");
                return false;
            }

            if (!IsSpawned || NetworkManager == null || !NetworkManager.IsListening)
            {
                Debug.Log($"[Interaction Test] Pickup interacted: {name}", this);
                result = CCS_InteractionResult.Success(
                    targetId,
                    CCS_InteractionAnimationKey.PickUp_RH,
                    "Pickup collected locally.");
                DestroyPickupInstance();
                return true;
            }

            if (!IsServer)
            {
                result = CCS_InteractionResult.Failure(targetId, "Only the server can collect networked pickups.");
                return false;
            }

            Debug.Log($"[Interaction Test] Pickup interacted: {name}", this);
            result = CCS_InteractionResult.Success(
                NetworkObject != null ? NetworkObject.NetworkObjectId : targetId,
                CCS_InteractionAnimationKey.PickUp_RH,
                "Pickup collected.");
            DestroyPickupInstance();
            return true;
        }

        #endregion

        #region Private Methods

        private void DestroyPickupInstance()
        {
            if (NetworkObject != null && NetworkObject.IsSpawned && IsServer)
            {
                NetworkObject.Despawn(true);
                return;
            }

            Destroy(gameObject);
        }

        #endregion
    }
}
