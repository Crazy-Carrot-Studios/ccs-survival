using System;
using CCS.Core;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

// =============================================================================
// SCRIPT: CCS_NetworkInteractionScanner
// CATEGORY: Modules / Interaction / Runtime / Components
// PURPOSE: Local-owner interaction scanner with server-authoritative multiplayer requests.
// PLACEMENT: Canonical networked test player prefab root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Uses E key directly. Solo/offline applies locally without NetworkManager.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public sealed class CCS_NetworkInteractionScanner : NetworkBehaviour
    {
        #region Variables

        [SerializeField] private CCS_InteractionScannerProfile scannerProfile;

        [SerializeField] private Key interactKey = Key.E;

        [SerializeField] private Transform scanOriginTransform;

        private float lastInteractionTime;

        #endregion

        #region Properties

        public event Action<CCS_InteractionCompletedEvent> InteractionCompleted;

        public CCS_InteractionScannerProfile ScannerProfile => scannerProfile;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            if (scanOriginTransform == null)
            {
                scanOriginTransform = transform;
            }
        }

        private void Update()
        {
            if (!IsLocalScannerOwner() || scannerProfile == null)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard[interactKey].wasPressedThisFrame)
            {
                return;
            }

            if (Time.time - lastInteractionTime < scannerProfile.InteractionCooldownSeconds)
            {
                return;
            }

            TrySubmitInteraction();
        }

        #endregion

        #region Public Methods

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            enabled = IsOwner;
        }

        public void ConfigureScanner(CCS_InteractionScannerProfile profile)
        {
            scannerProfile = profile;
        }

        #endregion

        #region Private Methods

        private void TrySubmitInteraction()
        {
            if (!TryFindInteractable(out CCS_IInteractable interactable, out ulong targetNetworkObjectId, out Vector3 origin, out Vector3 hitPoint))
            {
                return;
            }

            lastInteractionTime = Time.time;
            CCS_InteractionRequest request = BuildRequest(targetNetworkObjectId, origin, hitPoint);

            if (!IsSpawned || NetworkManager == null || !NetworkManager.IsListening)
            {
                ApplyInteractionLocally(interactable, request);
                return;
            }

            if (IsServer && IsOwner)
            {
                ApplyInteractionOnServer(request);
                return;
            }

            if (IsOwner)
            {
                SubmitInteractionServerRpc(targetNetworkObjectId, origin, hitPoint);
            }
        }

        [ServerRpc]
        private void SubmitInteractionServerRpc(
            ulong targetNetworkObjectId,
            Vector3 originPosition,
            Vector3 hitPoint,
            ServerRpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerClientId)
            {
                return;
            }

            CCS_InteractionRequest request = BuildRequest(targetNetworkObjectId, originPosition, hitPoint);
            ApplyInteractionOnServer(request);
        }

        private void ApplyInteractionOnServer(CCS_InteractionRequest request)
        {
            if (!IsServer || !TryResolveInteractable(request.TargetNetworkObjectId, out CCS_IInteractable interactable))
            {
                return;
            }

            if (!ValidateServerInteraction(request, interactable))
            {
                RaiseCompleted(request, CCS_InteractionResult.Failure(request.TargetNetworkObjectId, "Server rejected interaction."));
                return;
            }

            if (!interactable.Interact(request, out CCS_InteractionResult result))
            {
                RaiseCompleted(request, CCS_InteractionResult.Failure(request.TargetNetworkObjectId, "Interaction failed on server."));
                return;
            }

            RaiseCompleted(request, result);
            NotifyInteractionCompletedClientRpc(request.TargetNetworkObjectId, result.Succeeded, result.Message);
        }

        [ClientRpc]
        private void NotifyInteractionCompletedClientRpc(ulong targetNetworkObjectId, bool succeeded, string message)
        {
            if (IsServer)
            {
                return;
            }

            CCS_InteractionRequest request = BuildRequest(targetNetworkObjectId, transform.position, transform.position);
            RaiseCompleted(request, succeeded
                ? CCS_InteractionResult.Success(targetNetworkObjectId, message)
                : CCS_InteractionResult.Failure(targetNetworkObjectId, message));
        }

        private void ApplyInteractionLocally(CCS_IInteractable interactable, CCS_InteractionRequest request)
        {
            if (!interactable.CanInteract(request))
            {
                RaiseCompleted(request, CCS_InteractionResult.Failure(request.TargetNetworkObjectId, "Interaction rejected locally."));
                return;
            }

            if (!interactable.Interact(request, out CCS_InteractionResult result))
            {
                RaiseCompleted(request, CCS_InteractionResult.Failure(request.TargetNetworkObjectId, "Interaction failed locally."));
                return;
            }

            RaiseCompleted(request, result);
        }

        private bool ValidateServerInteraction(CCS_InteractionRequest request, CCS_IInteractable interactable)
        {
            if (!interactable.CanInteract(request))
            {
                return false;
            }

            float maxRange = scannerProfile != null ? scannerProfile.InteractionRange : 3f;
            float hitDistance = Vector3.Distance(request.OriginPosition, request.HitPoint);
            if (hitDistance > maxRange + 0.1f)
            {
                return false;
            }

            float actorDistance = Vector3.Distance(scanOriginTransform.position, request.HitPoint);
            return actorDistance <= maxRange + 0.5f;
        }

        private bool TryFindInteractable(
            out CCS_IInteractable interactable,
            out ulong targetNetworkObjectId,
            out Vector3 origin,
            out Vector3 hitPoint)
        {
            interactable = null;
            targetNetworkObjectId = 0;
            origin = Vector3.zero;
            hitPoint = Vector3.zero;

            if (scannerProfile == null)
            {
                return false;
            }

            Ray ray = BuildInteractionRay();
            origin = ray.origin;

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    scannerProfile.InteractionRange,
                    scannerProfile.InteractionLayerMask,
                    QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            hitPoint = hit.point;
            interactable = ResolveInteractableFromCollider(hit.collider);
            if (interactable == null)
            {
                return false;
            }

            NetworkObject targetNetworkObject = hit.collider.GetComponentInParent<NetworkObject>();
            targetNetworkObjectId = targetNetworkObject != null ? targetNetworkObject.NetworkObjectId : 0;
            return true;
        }

        private bool TryResolveInteractable(ulong targetNetworkObjectId, out CCS_IInteractable interactable)
        {
            interactable = null;
            if (targetNetworkObjectId == 0)
            {
                return false;
            }

            if (NetworkManager == null
                || !NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out NetworkObject networkObject))
            {
                return false;
            }

            interactable = ResolveInteractableFromTransform(networkObject.transform);
            return interactable != null;
        }

        private static CCS_IInteractable ResolveInteractableFromCollider(Collider collider)
        {
            if (collider == null)
            {
                return null;
            }

            return ResolveInteractableFromTransform(collider.transform);
        }

        private static CCS_IInteractable ResolveInteractableFromTransform(Transform targetTransform)
        {
            if (targetTransform == null)
            {
                return null;
            }

            MonoBehaviour[] behaviours = targetTransform.GetComponentsInParent<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is CCS_IInteractable interactable)
                {
                    return interactable;
                }
            }

            return null;
        }

        private Ray BuildInteractionRay()
        {
            if (scannerProfile.UseCameraForward)
            {
                Camera activeCamera = Camera.main;
                if (activeCamera != null)
                {
                    return new Ray(activeCamera.transform.position, activeCamera.transform.forward);
                }
            }

            Vector3 origin = scanOriginTransform.position + Vector3.up * 1.4f;
            Vector3 direction = scanOriginTransform.forward;
            return new Ray(origin, direction);
        }

        private CCS_InteractionRequest BuildRequest(ulong targetNetworkObjectId, Vector3 origin, Vector3 hitPoint)
        {
            float maxRange = scannerProfile != null ? scannerProfile.InteractionRange : 3f;
            ulong requesterClientId = NetworkManager != null && NetworkManager.IsListening
                ? OwnerClientId
                : 0;
            return new CCS_InteractionRequest(
                requesterClientId,
                targetNetworkObjectId,
                origin,
                hitPoint,
                maxRange,
                "Interact");
        }

        private void RaiseCompleted(CCS_InteractionRequest request, CCS_InteractionResult result)
        {
            InteractionCompleted?.Invoke(new CCS_InteractionCompletedEvent(request, result));
            if (scannerProfile != null && scannerProfile.EnableDebugLogs)
            {
                CCS_Logger.Log(
                    CCS_InteractionConstants.ModuleLogCategory,
                    $"{name} interaction {(result.Succeeded ? "succeeded" : "failed")}: {result.Message}",
                    true);
            }
        }

        private bool IsLocalScannerOwner()
        {
            if (!isActiveAndEnabled)
            {
                return false;
            }

            NetworkObject networkObject = NetworkObject;
            if (networkObject == null || !networkObject.IsSpawned)
            {
                return true;
            }

            return networkObject.IsOwner;
        }

        #endregion
    }
}
