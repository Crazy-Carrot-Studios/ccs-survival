using System;
using System.Collections.Generic;

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
// NOTES: Awareness overlap plus forward interaction volume validation on E only.
// =============================================================================

namespace CCS.Modules.Interaction
{
    [DefaultExecutionOrder(-100)]
    public sealed class CCS_NetworkInteractionScanner : NetworkBehaviour, CCS_IInteractionAnimationSource, CCS_IInteractionTargetSource
    {
        #region Variables

        private const int OverlapBufferSize = 16;
        private const int RaycastHitBufferSize = 8;

        [SerializeField] private CCS_InteractionScannerProfile scannerProfile;
        [SerializeField] private Transform scanOriginTransform;
        [SerializeField] private LayerMask interactableLayerMask;
        [SerializeField] private float broadDetectionRadius = CCS_InteractionConstants.DefaultBroadDetectionRadius;
        [SerializeField] private float interactionHalfWidth = CCS_InteractionConstants.DefaultInteractionHalfWidth;
        [SerializeField] private float interactionHalfHeight = CCS_InteractionConstants.DefaultInteractionHalfHeight;
        [SerializeField] private float lineOfSightSphereRadius = CCS_InteractionConstants.DefaultLineOfSightSphereRadius;
        [SerializeField] private float lineOfSightDistancePadding = CCS_InteractionConstants.DefaultLineOfSightDistancePadding;
        [SerializeField] private Component interactionBusySourceComponent;
        [SerializeField] private Component interactionLockControllerComponent;

        private readonly Collider[] overlapColliderBuffer = new Collider[OverlapBufferSize];
        private readonly RaycastHit[] lineOfSightHitBuffer = new RaycastHit[RaycastHitBufferSize];
        private readonly HashSet<CCS_InteractableLabelTarget> processedLabelTargets = new HashSet<CCS_InteractableLabelTarget>();

        private float lastInteractionTime;
        private CCS_InteractableLabelTarget awarenessLabelTarget;
        private CCS_InteractableLabelTarget pickupReadyLabelTarget;
        private float pickupReadyStrictRange;
        private bool hasPickupReadyTarget;
        private CCS_IInteractionBusySource interactionBusySource;
        private CCS_IInteractionLockController interactionLockController;
        private UnityEngine.CharacterController playerCharacterController;
        private int playerLayer = -1;

        #endregion

        #region Properties

        public event Action<CCS_InteractionCompletedEvent> InteractionCompleted;

        public CCS_InteractionScannerProfile ScannerProfile => scannerProfile;

        public bool HasPickupReadyTarget => hasPickupReadyTarget;

        public string CurrentPromptText
        {
            get
            {
                if (!hasPickupReadyTarget || pickupReadyLabelTarget == null)
                {
                    return string.Empty;
                }

                return pickupReadyLabelTarget.GetPromptText();
            }
        }

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            if (scanOriginTransform == null)
            {
                scanOriginTransform = transform;
            }

            EnsureInteractableLayerMaskConfigured();
            ResolveInteractionBusySource();
            ResolveInteractionLockController();
            playerCharacterController = GetComponent<UnityEngine.CharacterController>();
            playerLayer = LayerMask.NameToLayer(CCS_InteractionConstants.PlayerLayerName);
        }

        private void OnEnable()
        {
            EnsureScannerEnabledForSession();
        }

        private void Start()
        {
            EnsureScannerEnabledForSession();
        }

        private void Update()
        {
            if (!ShouldRunScanner())
            {
                return;
            }

            RefreshAwarenessTarget();
            UpdatePickupReadyState();

            if (TryGetInteractKeyPressed() && !IsInteractionBusy())
            {
                TryExecutePickup();
            }
        }

        #endregion

        #region Public Methods

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            EnsureScannerEnabledForSession();
        }

        public void ConfigureScanner(CCS_InteractionScannerProfile profile)
        {
            scannerProfile = profile;
        }

        public void EnsureOfflineInteractionSession()
        {
            enabled = true;
            EnsureInteractableLayerMaskConfigured();
            ResolveInteractionBusySource();
            ResolveInteractionLockController();
        }

        #endregion

        #region Private Methods

        private void EnsureInteractableLayerMaskConfigured()
        {
            if (interactableLayerMask.value != 0)
            {
                return;
            }

            int interactableLayer = LayerMask.NameToLayer(CCS_InteractionConstants.InteractableLayerName);
            if (interactableLayer >= 0)
            {
                interactableLayerMask = 1 << interactableLayer;
            }
        }

        private void ResolveInteractionBusySource()
        {
            if (interactionBusySource != null)
            {
                return;
            }

            if (interactionBusySourceComponent is CCS_IInteractionBusySource fromComponent)
            {
                interactionBusySource = fromComponent;
                return;
            }

            interactionBusySource = GetComponentInChildren<CCS_IInteractionBusySource>();
        }

        private void ResolveInteractionLockController()
        {
            if (interactionLockController != null)
            {
                return;
            }

            if (interactionLockControllerComponent is CCS_IInteractionLockController fromComponent)
            {
                interactionLockController = fromComponent;
                return;
            }

            interactionLockController = GetComponentInChildren<CCS_IInteractionLockController>();
        }

        private bool IsInteractionBusy()
        {
            ResolveInteractionBusySource();
            return interactionBusySource != null && interactionBusySource.IsInteractionBusy;
        }

        private void RefreshAwarenessTarget()
        {
            awarenessLabelTarget = null;

            Vector3 playerPosition = scanOriginTransform.position;
            int overlapCount = Physics.OverlapSphereNonAlloc(
                playerPosition,
                broadDetectionRadius,
                overlapColliderBuffer,
                interactableLayerMask,
                QueryTriggerInteraction.Ignore);

            processedLabelTargets.Clear();

            float closestDistance = float.MaxValue;
            CCS_InteractableLabelTarget closestTarget = null;

            for (int i = 0; i < overlapCount; i++)
            {
                Collider collider = overlapColliderBuffer[i];
                if (collider == null)
                {
                    continue;
                }

                CCS_InteractableLabelTarget labelTarget = collider.GetComponentInParent<CCS_InteractableLabelTarget>();
                if (labelTarget == null || !processedLabelTargets.Add(labelTarget))
                {
                    continue;
                }

                float distance = Vector3.Distance(playerPosition, labelTarget.BoundsCenter);
                if (distance >= closestDistance)
                {
                    continue;
                }

                closestDistance = distance;
                closestTarget = labelTarget;
            }

            awarenessLabelTarget = closestTarget;
        }

        private void UpdatePickupReadyState()
        {
            hasPickupReadyTarget = false;
            pickupReadyLabelTarget = null;
            pickupReadyStrictRange = CCS_InteractionConstants.DefaultStrictPickupDistance;

            if (IsInteractionBusy())
            {
                return;
            }

            if (TryFindBestReadyTarget(out CCS_InteractableLabelTarget bestTarget, out float strictRange))
            {
                hasPickupReadyTarget = true;
                pickupReadyLabelTarget = bestTarget;
                pickupReadyStrictRange = strictRange;
            }
        }

        private bool TryFindBestReadyTarget(out CCS_InteractableLabelTarget bestTarget, out float strictRange)
        {
            bestTarget = null;
            strictRange = CCS_InteractionConstants.DefaultStrictPickupDistance;

            Vector3 playerPosition = scanOriginTransform.position;
            int overlapCount = Physics.OverlapSphereNonAlloc(
                playerPosition,
                broadDetectionRadius,
                overlapColliderBuffer,
                interactableLayerMask,
                QueryTriggerInteraction.Ignore);

            processedLabelTargets.Clear();

            float closestDepth = float.MaxValue;

            for (int i = 0; i < overlapCount; i++)
            {
                Collider collider = overlapColliderBuffer[i];
                if (collider == null)
                {
                    continue;
                }

                CCS_InteractableLabelTarget labelTarget = collider.GetComponentInParent<CCS_InteractableLabelTarget>();
                if (labelTarget == null || !processedLabelTargets.Add(labelTarget))
                {
                    continue;
                }

                if (!EvaluatePickupReadiness(labelTarget, out float candidateStrictRange, out Vector3 localTarget))
                {
                    continue;
                }

                if (localTarget.z >= closestDepth)
                {
                    continue;
                }

                closestDepth = localTarget.z;
                bestTarget = labelTarget;
                strictRange = candidateStrictRange;
            }

            return bestTarget != null;
        }

        private void TryExecutePickup()
        {
            if (!hasPickupReadyTarget || pickupReadyLabelTarget == null || IsInteractionBusy())
            {
                return;
            }

            bool validationPassed = EvaluatePickupReadinessDetailed(
                pickupReadyLabelTarget,
                out float strictRange,
                out Vector3 localTarget,
                out bool inVolume,
                out bool lineOfSight);

            if (!validationPassed)
            {
                Debug.Log(
                    $"[Interaction] Interaction blocked: target={pickupReadyLabelTarget.name}, inVolume={inVolume}, los={lineOfSight}",
                    this);
                return;
            }

            CCS_InteractionAnimationKey animationKey = pickupReadyLabelTarget.Definition.AnimationKey;
            BeginInteractionLock(animationKey);

            if (!TrySubmitInteraction())
            {
                CancelInteractionLock();
            }
        }

        private void BeginInteractionLock(CCS_InteractionAnimationKey animationKey)
        {
            ResolveInteractionLockController();
            interactionLockController?.BeginInteractionLock(animationKey);
        }

        private void CancelInteractionLock()
        {
            ResolveInteractionLockController();
            interactionLockController?.CancelInteractionLock();
        }

        private bool EvaluatePickupReadiness(
            CCS_InteractableLabelTarget labelTarget,
            out float strictRange,
            out Vector3 localTarget)
        {
            return EvaluatePickupReadinessDetailed(
                labelTarget,
                out strictRange,
                out localTarget,
                out _,
                out _);
        }

        private bool EvaluatePickupReadinessDetailed(
            CCS_InteractableLabelTarget labelTarget,
            out float strictRange,
            out Vector3 localTarget,
            out bool inVolume,
            out bool lineOfSight)
        {
            strictRange = CCS_InteractionConstants.DefaultStrictPickupDistance;
            localTarget = Vector3.zero;
            inVolume = false;
            lineOfSight = false;

            if (labelTarget == null)
            {
                return false;
            }

            strictRange = labelTarget.StrictRange;
            localTarget = transform.InverseTransformPoint(labelTarget.BoundsCenter);
            inVolume = IsInsideInteractionVolume(localTarget, strictRange);
            lineOfSight = HasLineOfSightFromScanOrigin(labelTarget);

            return inVolume && lineOfSight;
        }

        private bool HasLineOfSightFromScanOrigin(CCS_InteractableLabelTarget labelTarget)
        {
            if (labelTarget == null)
            {
                return false;
            }

            Vector3 origin = scanOriginTransform != null ? scanOriginTransform.position : transform.position;
            Collider targetCollider = GetTargetCollider(labelTarget);
            Vector3 targetPoint = targetCollider != null
                ? targetCollider.ClosestPoint(origin)
                : labelTarget.BoundsCenter;

            return EvaluateLineOfSight(origin, targetPoint, labelTarget);
        }

        private bool EvaluateLineOfSight(
            Vector3 origin,
            Vector3 targetPoint,
            CCS_InteractableLabelTarget labelTarget)
        {
            Vector3 offset = targetPoint - origin;
            float distance = offset.magnitude;
            if (distance <= Mathf.Epsilon)
            {
                return true;
            }

            Vector3 direction = offset / distance;
            float castDistance = distance + lineOfSightDistancePadding;
            int hitCount = Physics.SphereCastNonAlloc(
                origin,
                lineOfSightSphereRadius,
                direction,
                lineOfSightHitBuffer,
                castDistance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);

            SortRaycastHitsByDistance(lineOfSightHitBuffer, hitCount);

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = lineOfSightHitBuffer[i];
                if (hit.collider == null || IsPlayerCollider(hit.collider))
                {
                    continue;
                }

                return IsColliderPartOfInteractable(hit.collider, labelTarget);
            }

            return true;
        }

        private static Collider GetTargetCollider(CCS_InteractableLabelTarget labelTarget)
        {
            if (labelTarget == null)
            {
                return null;
            }

            Collider collider = labelTarget.GetComponent<Collider>();
            return collider != null ? collider : labelTarget.GetComponentInChildren<Collider>();
        }

        private bool IsPlayerCollider(Collider hitCollider)
        {
            if (hitCollider == null)
            {
                return true;
            }

            Transform hitTransform = hitCollider.transform;
            if (hitTransform == transform || hitTransform.IsChildOf(transform))
            {
                return true;
            }

            if (playerLayer >= 0 && hitCollider.gameObject.layer == playerLayer)
            {
                return true;
            }

            UnityEngine.CharacterController hitCharacterController =
                hitCollider.GetComponent<UnityEngine.CharacterController>();
            if (hitCharacterController == null)
            {
                hitCharacterController = hitCollider.GetComponentInParent<UnityEngine.CharacterController>();
            }

            if (hitCharacterController != null
                && (hitCharacterController == playerCharacterController
                    || hitCharacterController.transform == transform
                    || hitCharacterController.transform.IsChildOf(transform)))
            {
                return true;
            }

            return false;
        }

        private static void SortRaycastHitsByDistance(RaycastHit[] hitBuffer, int hitCount)
        {
            for (int i = 1; i < hitCount; i++)
            {
                RaycastHit current = hitBuffer[i];
                int insertIndex = i;
                while (insertIndex > 0 && hitBuffer[insertIndex - 1].distance > current.distance)
                {
                    hitBuffer[insertIndex] = hitBuffer[insertIndex - 1];
                    insertIndex--;
                }

                hitBuffer[insertIndex] = current;
            }
        }

        private bool IsInsideInteractionVolume(Vector3 localTarget, float strictRange)
        {
            return localTarget.z > 0f
                && localTarget.z <= strictRange
                && Mathf.Abs(localTarget.x) <= interactionHalfWidth
                && Mathf.Abs(localTarget.y) <= interactionHalfHeight;
        }

        private static bool IsColliderPartOfInteractable(Collider hitCollider, CCS_InteractableLabelTarget labelTarget)
        {
            if (hitCollider == null || labelTarget == null)
            {
                return false;
            }

            Collider targetCollider = GetTargetCollider(labelTarget);
            if (targetCollider != null && hitCollider == targetCollider)
            {
                return true;
            }

            Transform hitTransform = hitCollider.transform;
            Transform interactableRoot = labelTarget.transform;
            if (hitTransform == interactableRoot || hitTransform.IsChildOf(interactableRoot))
            {
                return true;
            }

            CCS_InteractableLabelTarget hitLabelTarget = hitCollider.GetComponentInParent<CCS_InteractableLabelTarget>();
            return hitLabelTarget == labelTarget;
        }

        private static bool TryGetInteractKeyPressed()
        {
            Keyboard keyboard = Keyboard.current;
            return keyboard != null && keyboard.eKey.wasPressedThisFrame;
        }

        private bool TrySubmitInteraction()
        {
            if (!TryFindInteractable(
                    out CCS_IInteractable interactable,
                    out ulong targetNetworkObjectId,
                    out Vector3 origin,
                    out Vector3 hitPoint))
            {
                return false;
            }

            lastInteractionTime = Time.time;
            CCS_InteractionRequest request = BuildRequest(targetNetworkObjectId, origin, hitPoint);

            if (!IsSpawned || NetworkManager == null || !NetworkManager.IsListening)
            {
                return ApplyInteractionLocally(interactable, request);
            }

            if (IsServer && IsOwner)
            {
                return ApplyInteractionOnServer(request);
            }

            if (IsOwner)
            {
                SubmitInteractionServerRpc(targetNetworkObjectId, origin, hitPoint);
                return true;
            }

            return false;
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

        private bool ApplyInteractionOnServer(CCS_InteractionRequest request)
        {
            if (!IsServer || !TryResolveInteractable(request.TargetNetworkObjectId, out CCS_IInteractable interactable))
            {
                return false;
            }

            if (!ValidateServerInteraction(request, interactable))
            {
                RaiseCompleted(request, CCS_InteractionResult.Failure(request.TargetNetworkObjectId, "Server rejected interaction."));
                return false;
            }

            if (!interactable.Interact(request, out CCS_InteractionResult result))
            {
                RaiseCompleted(request, CCS_InteractionResult.Failure(request.TargetNetworkObjectId, "Interaction failed on server."));
                return false;
            }

            if (result.Succeeded)
            {
                RaiseCompleted(request, result);
            }
            else
            {
                RaiseCompleted(request, result);
                return false;
            }

            NotifyInteractionCompletedClientRpc(
                request.TargetNetworkObjectId,
                result.Succeeded,
                result.Message,
                (int)result.AnimationKey);
            return true;
        }

        [ClientRpc]
        private void NotifyInteractionCompletedClientRpc(
            ulong targetNetworkObjectId,
            bool succeeded,
            string message,
            int animationKeyValue)
        {
            if (IsServer)
            {
                return;
            }

            if (!succeeded)
            {
                return;
            }

            CCS_InteractionRequest request = BuildRequest(targetNetworkObjectId, transform.position, transform.position);
            CCS_InteractionAnimationKey animationKey = (CCS_InteractionAnimationKey)animationKeyValue;
            RaiseCompleted(
                request,
                CCS_InteractionResult.Success(targetNetworkObjectId, animationKey, message));
        }

        private bool ApplyInteractionLocally(CCS_IInteractable interactable, CCS_InteractionRequest request)
        {
            if (!interactable.CanInteract(request))
            {
                return false;
            }

            if (!interactable.Interact(request, out CCS_InteractionResult result))
            {
                return false;
            }

            if (result.Succeeded)
            {
                RaiseCompleted(request, result);
                return true;
            }

            RaiseCompleted(request, result);
            return false;
        }

        private bool ValidateServerInteraction(CCS_InteractionRequest request, CCS_IInteractable interactable)
        {
            if (!interactable.CanInteract(request))
            {
                return false;
            }

            float hitDistance = Vector3.Distance(request.OriginPosition, request.HitPoint);
            if (hitDistance > request.MaxRange + 0.1f)
            {
                return false;
            }

            float actorDistance = Vector3.Distance(GetInteractionOriginPosition(), request.HitPoint);
            return actorDistance <= request.MaxRange + 0.5f;
        }

        private Vector3 GetInteractionOriginPosition()
        {
            return scanOriginTransform != null ? scanOriginTransform.position : transform.position;
        }

        private void EnsureScannerEnabledForSession()
        {
            if (ShouldRunScanner())
            {
                enabled = true;
                return;
            }

            NetworkObject networkObject = NetworkObject;
            if (networkObject != null && networkObject.IsSpawned)
            {
                enabled = false;
            }
        }

        private bool ShouldRunScanner()
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

            if (!hasPickupReadyTarget || pickupReadyLabelTarget == null)
            {
                return false;
            }

            origin = scanOriginTransform.position;
            Collider targetCollider = GetTargetCollider(pickupReadyLabelTarget);
            hitPoint = targetCollider != null
                ? targetCollider.ClosestPoint(origin)
                : pickupReadyLabelTarget.BoundsCenter;
            interactable = ResolveInteractableFromTransform(pickupReadyLabelTarget.transform);
            if (interactable == null)
            {
                return false;
            }

            NetworkObject targetNetworkObject = pickupReadyLabelTarget.GetComponentInParent<NetworkObject>();
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

        private CCS_InteractionRequest BuildRequest(ulong targetNetworkObjectId, Vector3 origin, Vector3 hitPoint)
        {
            ulong requesterClientId = NetworkManager != null && NetworkManager.IsListening
                ? OwnerClientId
                : 0;

            return new CCS_InteractionRequest(
                requesterClientId,
                targetNetworkObjectId,
                origin,
                hitPoint,
                hasPickupReadyTarget ? pickupReadyStrictRange : CCS_InteractionConstants.DefaultStrictPickupDistance,
                "Interact");
        }

        private void RaiseCompleted(CCS_InteractionRequest request, CCS_InteractionResult result)
        {
            string animationLabel = CCS_InteractionAnimationKeyUtility.ToAnimatorTriggerName(result.AnimationKey);
            Debug.Log(
                $"[Interaction] InteractionCompleted {(result.Succeeded ? "succeeded" : "failed")}: {result.Message} ({animationLabel})",
                this);

            InteractionCompleted?.Invoke(new CCS_InteractionCompletedEvent(request, result));
            if (scannerProfile != null && scannerProfile.EnableDebugLogs)
            {
                CCS_Logger.Log(
                    CCS_InteractionConstants.ModuleLogCategory,
                    $"{name} interaction {(result.Succeeded ? "succeeded" : "failed")}: {result.Message}",
                    true);
            }
        }

        #endregion
    }
}
