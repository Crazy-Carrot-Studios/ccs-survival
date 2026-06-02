using CCS.Core;
using CCS.Modules.CharacterController;
using CCS.Modules.Industry;
using CCS.Modules.Shelter;
using CCS.Modules.SurvivalCore;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerGameplayController
// CATEGORY: Survival / Runtime / Player
// PURPOSE: Composition glue wiring input, movement, camera, stamina, and cursor lock.
// PLACEMENT: PF_CCS_Player root alongside CharacterController and input provider.
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: No gameplay logic that belongs in modules. Animator root motion OFF.
// =============================================================================

namespace CCS.Survival.Player
{
    [DefaultExecutionOrder(210)]
    [RequireComponent(typeof(UnityEngine.CharacterController))]
    public sealed class CCS_PlayerGameplayController : MonoBehaviour
    {
        #region Variables

        [Header("Character Controller")]
        [Tooltip("Movement and camera tuning profile.")]
        [SerializeField] private CCS_CharacterControllerProfile characterControllerProfile;

        [Header("Camera")]
        [Tooltip("Pivot transform used for yaw rotation and movement facing.")]
        [SerializeField] private Transform cameraPivot;

        [Tooltip("Look target tracked by Cinemachine third-person follow.")]
        [SerializeField] private Transform cameraLookTarget;

        [Tooltip("Gameplay camera on the player prefab (CinemachineBrain output).")]
        [SerializeField] private Camera playerCamera;

        [Header("Cursor")]
        [Tooltip("Lock and hide cursor while gameplay input is active.")]
        [SerializeField] private bool lockCursorOnStart = true;

        private UnityEngine.CharacterController characterController;
        private CCS_CharacterInputActionProvider inputProvider;
        private CCS_CharacterMovementService movementService;
        private CCS_SurvivalCoreService survivalCoreService;
        private CCS_ShelterService shelterService;
        private CCS_CampService campService;
        private CCS_IndustryService industryService;
        private bool cursorLocked;
        private bool servicesBound;
        private bool gameplayFrozen;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            characterController = GetComponent<UnityEngine.CharacterController>();
            inputProvider = GetComponent<CCS_CharacterInputActionProvider>();

            if (cameraPivot == null)
            {
                Transform pivot = transform.Find("CameraPivot");
                cameraPivot = pivot != null ? pivot : transform;
            }

            if (cameraLookTarget == null && cameraPivot != null)
            {
                Transform look = cameraPivot.Find("CameraLookTarget");
                cameraLookTarget = look != null ? look : cameraPivot;
            }

            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
            }
        }

        private void Start()
        {
            TryBindServices();
            ApplyCursorLock(lockCursorOnStart);
        }

        private void Update()
        {
            if (!servicesBound)
            {
                TryBindServices();
            }

            UpdateStaminaSprintGate();
            HandlePauseCursorToggle();
            UpdateShelterSubjectPosition();
        }

        #endregion

        #region Public Methods

        public void ApplyCursorLock(bool locked)
        {
            cursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        public void SetGameplayFrozen(bool frozen)
        {
            gameplayFrozen = frozen;
            movementService?.SetMovementLocked(frozen);

            if (inputProvider != null)
            {
                inputProvider.InputEnabled = !frozen;
            }
        }

        #endregion

        #region Private Methods

        private void TryBindServices()
        {
            if (servicesBound || characterControllerProfile == null)
            {
                return;
            }

            if (!CCS_CharacterMovementRuntimeBridge.TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost)
                || runtimeHost.ServiceRegistry == null)
            {
                return;
            }

            if (!runtimeHost.ServiceRegistry.TryGetService(out movementService)
                || movementService == null)
            {
                return;
            }

            runtimeHost.ServiceRegistry.TryGetService(out survivalCoreService);
            runtimeHost.ServiceRegistry.TryGetService(out shelterService);
            runtimeHost.ServiceRegistry.TryGetService(out campService);
            runtimeHost.ServiceRegistry.TryGetService(out industryService);

            CCS_ICharacterInputProvider provider = inputProvider != null
                ? inputProvider
                : movementService.DefaultInputBridge;

            Transform lookTarget = cameraLookTarget != null
                ? cameraLookTarget
                : cameraPivot != null ? cameraPivot : transform;

            movementService.InitializeFromScene(
                characterController,
                characterControllerProfile,
                cameraPivot != null ? cameraPivot : transform,
                lookTarget,
                provider);

            servicesBound = movementService.IsInitialized;
        }

        private void UpdateStaminaSprintGate()
        {
            if (movementService == null || !movementService.IsInitialized)
            {
                return;
            }

            bool allowSprint = true;
            if (survivalCoreService != null
                && survivalCoreService.IsInitialized
                && survivalCoreService.TryGetSnapshot(CCS_SurvivalStatType.Stamina, out CCS_SurvivalStatSnapshot staminaSnapshot))
            {
                allowSprint = staminaSnapshot.CurrentValue > staminaSnapshot.MinValue + 0.01f;
            }

            movementService.SetSprintAllowed(allowSprint);

            if (inputProvider != null)
            {
                inputProvider.SprintAllowed = allowSprint;
            }
        }

        private void HandlePauseCursorToggle()
        {
            if (inputProvider == null || !inputProvider.PausePressedThisFrame)
            {
                return;
            }

            ApplyCursorLock(!cursorLocked);
        }

        private void UpdateShelterSubjectPosition()
        {
            if (shelterService == null || !shelterService.IsInitialized)
            {
                return;
            }

            Vector3 subjectPosition = transform.position;
            shelterService.SetSubjectPosition(subjectPosition);
            if (campService != null && campService.IsInitialized)
            {
                campService.SetSubjectPosition(subjectPosition);
            }

            if (industryService != null && industryService.IsInitialized)
            {
                industryService.SetSubjectPosition(subjectPosition);
            }
        }

        #endregion
    }
}
