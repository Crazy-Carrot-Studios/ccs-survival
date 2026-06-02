using CCS.Modules.Interaction;
using CCS.Modules.Inventory;
using CCS.Modules.WorldResources;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TrapInstance
// CATEGORY: Modules / Trapping / Runtime / Components
// PURPOSE: Scene trap instance with state, capture data, and interactable harvest.
// PLACEMENT: Spawned by CCS_TrapService when a placeable trap is confirmed.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Trapping
{
    public sealed class CCS_TrapInstance : MonoBehaviour, CCS_IInteractableResultProvider
    {
        #region Variables

        [SerializeField] private CCS_TrapDefinition trapDefinition;
        [SerializeField] private string instanceId = string.Empty;
        [SerializeField] private CCS_TrapState trapState = CCS_TrapState.Unarmed;
        [SerializeField] private float remainingTimerSeconds;
        [SerializeField] private string capturedWildlifeId = string.Empty;
        [SerializeField] private string capturedInstanceKey = string.Empty;
        [SerializeField] private float interactionDistance = 3f;

        private Renderer trapRenderer;
        private CCS_TrapService trapService;

        #endregion

        #region Properties

        public CCS_TrapDefinition TrapDefinition => trapDefinition;

        public string InstanceId => instanceId;

        public CCS_TrapState TrapState => trapState;

        public float RemainingTimerSeconds => remainingTimerSeconds;

        public bool HasCaptureData =>
            !string.IsNullOrWhiteSpace(capturedWildlifeId) || trapState == CCS_TrapState.Triggered;

        #endregion

        #region Public Methods

        public void Initialize(
            CCS_TrapService service,
            CCS_TrapDefinition definition,
            string assignedInstanceId,
            CCS_TrapState initialState,
            float timerSeconds)
        {
            trapService = service;
            trapDefinition = definition;
            instanceId = assignedInstanceId;
            trapState = initialState;
            remainingTimerSeconds = timerSeconds;
            ApplyStateVisual();
        }

        public void SetTrapState(CCS_TrapState newState, float timerSeconds = 0f)
        {
            trapState = newState;
            remainingTimerSeconds = timerSeconds < 0f ? 0f : timerSeconds;
            ApplyStateVisual();
        }

        public void SetCaptureData(string wildlifeId, string wildlifeInstanceKey)
        {
            capturedWildlifeId = wildlifeId ?? string.Empty;
            capturedInstanceKey = wildlifeInstanceKey ?? string.Empty;
        }

        public void TickTimer(float deltaTime)
        {
            if (trapState != CCS_TrapState.Armed || remainingTimerSeconds <= 0f)
            {
                return;
            }

            remainingTimerSeconds -= deltaTime;
            if (remainingTimerSeconds < 0f)
            {
                remainingTimerSeconds = 0f;
            }
        }

        public bool IsTimerReady => trapState == CCS_TrapState.Armed && remainingTimerSeconds <= 0f;

        public CCS_TrapInstanceSaveState CaptureState()
        {
            Vector3 position = transform.position;
            return new CCS_TrapInstanceSaveState
            {
                instanceId = instanceId,
                trapDefinitionId = trapDefinition != null ? trapDefinition.TrapDefinitionId : string.Empty,
                trapState = (int)trapState,
                positionX = position.x,
                positionY = position.y,
                positionZ = position.z,
                rotationY = transform.eulerAngles.y,
                capturedWildlifeId = capturedWildlifeId,
                capturedInstanceKey = capturedInstanceKey,
                remainingTimerSeconds = remainingTimerSeconds,
                hasCaptureData = HasCaptureData
            };
        }

        public void RestoreState(CCS_TrapInstanceSaveState saveState)
        {
            if (saveState == null)
            {
                return;
            }

            instanceId = saveState.instanceId;
            trapState = (CCS_TrapState)saveState.trapState;
            remainingTimerSeconds = saveState.remainingTimerSeconds;
            capturedWildlifeId = saveState.capturedWildlifeId ?? string.Empty;
            capturedInstanceKey = saveState.capturedInstanceKey ?? string.Empty;
            transform.SetPositionAndRotation(
                new Vector3(saveState.positionX, saveState.positionY, saveState.positionZ),
                Quaternion.Euler(0f, saveState.rotationY, 0f));
            ApplyStateVisual();
        }

        public string GetInteractionDisplayName()
        {
            if (trapDefinition == null)
            {
                return "Trap";
            }

            return trapState == CCS_TrapState.Triggered
                ? $"{trapDefinition.DisplayName} (Caught)"
                : trapDefinition.DisplayName;
        }

        public bool CanInteract()
        {
            return trapState == CCS_TrapState.Triggered;
        }

        public void Interact()
        {
            TryInteract();
        }

        public bool TryInteract()
        {
            if (trapService == null)
            {
                CCS_TrapRuntimeBridge.TryGetTrapService(out trapService);
            }

            if (trapService == null)
            {
                return false;
            }

            CCS_TrapResult result = trapService.TryHarvestTrap(this, CCS_RequiredToolType.Knife);
            return result.IsSuccess;
        }

        public float GetInteractionDistance()
        {
            return interactionDistance;
        }

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            trapRenderer = GetComponent<Renderer>();
            if (trapRenderer == null)
            {
                trapRenderer = GetComponentInChildren<Renderer>();
            }
        }

        private void OnDestroy()
        {
            if (trapService != null)
            {
                trapService.UnregisterTrapInstance(this);
            }
        }

        #endregion

        #region Private Methods

        private void ApplyStateVisual()
        {
            if (trapRenderer == null)
            {
                return;
            }

            switch (trapState)
            {
                case CCS_TrapState.Armed:
                    trapRenderer.material.color = new Color(0.85f, 0.65f, 0.2f, 1f);
                    break;
                case CCS_TrapState.Triggered:
                    trapRenderer.material.color = new Color(0.2f, 0.75f, 0.35f, 1f);
                    break;
                case CCS_TrapState.Harvested:
                    trapRenderer.material.color = new Color(0.35f, 0.35f, 0.35f, 0.65f);
                    break;
                case CCS_TrapState.Broken:
                    trapRenderer.material.color = new Color(0.5f, 0.15f, 0.15f, 0.75f);
                    break;
                default:
                    trapRenderer.material.color = new Color(0.55f, 0.45f, 0.35f, 1f);
                    break;
            }
        }

        #endregion
    }
}
