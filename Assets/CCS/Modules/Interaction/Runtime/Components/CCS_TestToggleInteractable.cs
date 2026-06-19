using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TestToggleInteractable
// CATEGORY: Modules / Interaction / Runtime / Components
// PURPOSE: Networked test cube that toggles height and color when interacted with.
// PLACEMENT: PF_CCS_TestInteractable_ToggleCube prefab root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Server-authoritative state. Works locally when Netcode is not listening.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public sealed class CCS_TestToggleInteractable : NetworkBehaviour, CCS_IInteractable
    {
        #region Variables

        [SerializeField] private Transform visualRoot;

        [SerializeField] private MeshRenderer visualRenderer;

        [SerializeField] private float closedLocalY = 0.5f;

        [SerializeField] private float openLocalY = 1.1f;

        [SerializeField] private Color closedColor = new Color(0.85f, 0.2f, 0.2f, 1f);

        [SerializeField] private Color openColor = new Color(0.2f, 0.8f, 0.3f, 1f);

        private readonly NetworkVariable<bool> isOpen =
            new NetworkVariable<bool>(
                false,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        private bool localOfflineState;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            if (visualRoot == null)
            {
                visualRoot = transform;
            }

            if (visualRenderer == null)
            {
                visualRenderer = GetComponentInChildren<MeshRenderer>();
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            isOpen.OnValueChanged += HandleOpenStateChanged;
            ApplyVisualState(isOpen.Value);
        }

        public override void OnNetworkDespawn()
        {
            isOpen.OnValueChanged -= HandleOpenStateChanged;
            base.OnNetworkDespawn();
        }

        private void Start()
        {
            if (!IsSpawned)
            {
                ApplyVisualState(localOfflineState);
            }
        }

        #endregion

        #region Public Methods

        public bool CanInteract(CCS_InteractionRequest request)
        {
            Vector3 targetPosition = visualRoot != null ? visualRoot.position : transform.position;
            float distance = Vector3.Distance(request.OriginPosition, targetPosition);
            return distance <= request.MaxRange + 0.75f;
        }

        public bool Interact(CCS_InteractionRequest request, out CCS_InteractionResult result)
        {
            ulong targetId = request.TargetNetworkObjectId;
            if (!CanInteract(request))
            {
                result = CCS_InteractionResult.Failure(targetId, "Target is out of range.");
                return false;
            }

            if (!IsSpawned || NetworkManager == null || !NetworkManager.IsListening)
            {
                localOfflineState = !localOfflineState;
                ApplyVisualState(localOfflineState);
                result = CCS_InteractionResult.Success(targetId, "Toggle cube switched locally.");
                return true;
            }

            if (!IsServer)
            {
                result = CCS_InteractionResult.Failure(targetId, "Only the server can apply networked interactions.");
                return false;
            }

            isOpen.Value = !isOpen.Value;
            result = CCS_InteractionResult.Success(
                NetworkObject != null ? NetworkObject.NetworkObjectId : targetId,
                isOpen.Value ? "Toggle cube opened." : "Toggle cube closed.");
            return true;
        }

        #endregion

        #region Private Methods

        private void HandleOpenStateChanged(bool previousValue, bool newValue)
        {
            ApplyVisualState(newValue);
        }

        private void ApplyVisualState(bool open)
        {
            if (visualRoot != null)
            {
                Vector3 localPosition = visualRoot.localPosition;
                localPosition.y = open ? openLocalY : closedLocalY;
                visualRoot.localPosition = localPosition;
            }

            if (visualRenderer != null)
            {
                visualRenderer.material.color = open ? openColor : closedColor;
            }
        }

        #endregion
    }
}
