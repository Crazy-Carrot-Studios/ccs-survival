using CCS.Modules.Attributes;
using CCS.Modules.Interaction;
using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerRuntimeFacade
// CATEGORY: Modules / CharacterController / Runtime
// PURPOSE: Central reference hub for major player subsystems without gameplay logic.
// PLACEMENT: PF_CCS_Player_Networked_Runtime root.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: v0.8.0 — composition root only. Validates references in Awake/OnValidate.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-500)]
    public sealed class CCS_PlayerRuntimeFacade : MonoBehaviour
    {
        #region Variables

        [Header("Network")]
        [SerializeField] private NetworkObject networkObject;

        [Header("Presentation")]
        [SerializeField] private Animator animator;
        [SerializeField] private CCS_PlayerInteractionAnimator playerInteractionAnimator;

        [Header("Runtime Systems")]
        [SerializeField] private CCS_CharacterInputActionProvider inputProvider;
        [SerializeField] private CCS_CharacterMotor motor;
        [SerializeField] private CCS_CharacterCameraController cameraController;
        [SerializeField] private CCS_NetworkInteractionScanner interactionScanner;
        [SerializeField] private CCS_AttributeContainer attributeContainer;
        [SerializeField] private CCS_NetworkHealth networkHealth;
        [SerializeField] private CCS_StaminaController staminaController;
        [SerializeField] private CCS_HealthRegenController healthRegenController;
        [SerializeField] private Component revolverController;
        [SerializeField] private Component weaponLoadout;
        [SerializeField] private Component equipmentVisualController;

        [Header("Transitional Local UI")]
        [SerializeField] private MonoBehaviour localUiBridge;

        #endregion

        #region Properties

        public NetworkObject NetworkObject => networkObject;

        public Animator Animator => animator;

        public CCS_PlayerInteractionAnimator PlayerInteractionAnimator => playerInteractionAnimator;

        public CCS_CharacterInputActionProvider InputProvider => inputProvider;

        public CCS_CharacterMotor Motor => motor;

        public CCS_CharacterCameraController CameraController => cameraController;

        public CCS_NetworkInteractionScanner InteractionScanner => interactionScanner;

        public CCS_AttributeContainer AttributeContainer => attributeContainer;

        public CCS_NetworkHealth NetworkHealth => networkHealth;

        public CCS_StaminaController StaminaController => staminaController;

        public CCS_HealthRegenController HealthRegenController => healthRegenController;

        public Component RevolverController => revolverController;

        public Component WeaponLoadout => weaponLoadout;

        public Component EquipmentVisualController => equipmentVisualController;

        public MonoBehaviour LocalUiBridge => localUiBridge;

        public bool IsLocalOwner =>
            networkObject == null || !networkObject.IsSpawned || networkObject.IsOwner;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveMissingReferences(logErrors: Application.isPlaying);
        }

        private void OnValidate()
        {
            ResolveMissingReferences(logErrors: false);
        }

        #endregion

        #region Public Methods

        public bool HasRequiredProductionReferences()
        {
            return networkObject != null
                && motor != null
                && inputProvider != null
                && animator != null
                && playerInteractionAnimator != null;
        }

        public T GetRevolverController<T>() where T : Component
        {
            return revolverController as T;
        }

        public T GetWeaponLoadout<T>() where T : Component
        {
            return weaponLoadout as T;
        }

        public T GetEquipmentVisualController<T>() where T : Component
        {
            return equipmentVisualController as T;
        }

        #endregion

        #region Private Methods

        private void ResolveMissingReferences(bool logErrors)
        {
            if (networkObject == null)
            {
                networkObject = GetComponent<NetworkObject>();
            }

            if (motor == null)
            {
                motor = GetComponent<CCS_CharacterMotor>()
                    ?? GetComponentInChildren<CCS_CharacterMotor>(true);
            }

            if (inputProvider == null)
            {
                inputProvider = GetComponent<CCS_CharacterInputActionProvider>()
                    ?? GetComponentInChildren<CCS_CharacterInputActionProvider>(true);
            }

            if (cameraController == null)
            {
                cameraController = GetComponent<CCS_CharacterCameraController>()
                    ?? GetComponentInChildren<CCS_CharacterCameraController>(true);
            }

            if (interactionScanner == null)
            {
                interactionScanner = GetComponent<CCS_NetworkInteractionScanner>()
                    ?? GetComponentInChildren<CCS_NetworkInteractionScanner>(true);
            }

            if (attributeContainer == null)
            {
                attributeContainer = GetComponent<CCS_AttributeContainer>()
                    ?? GetComponentInChildren<CCS_AttributeContainer>(true);
            }

            if (networkHealth == null)
            {
                networkHealth = GetComponent<CCS_NetworkHealth>()
                    ?? GetComponentInChildren<CCS_NetworkHealth>(true);
            }

            if (staminaController == null)
            {
                staminaController = GetComponent<CCS_StaminaController>()
                    ?? GetComponentInChildren<CCS_StaminaController>(true);
            }

            if (healthRegenController == null)
            {
                healthRegenController = GetComponent<CCS_HealthRegenController>()
                    ?? GetComponentInChildren<CCS_HealthRegenController>(true);
            }

            if (revolverController == null)
            {
                revolverController = FindComponentByTypeName("CCS_RevolverController");
            }

            if (weaponLoadout == null)
            {
                weaponLoadout = FindComponentByTypeName("CCS_PlayerWeaponLoadout");
            }

            if (equipmentVisualController == null)
            {
                equipmentVisualController = FindComponentByTypeName("CCS_PlayerEquipmentVisualController");
            }

            if (animator == null || !CCS_PlayerAnimatorResolver.IsAuthoritativeGameplayAnimator(animator))
            {
                if (CCS_PlayerAnimatorResolver.TryResolveAuthoritativeAnimator(
                        transform,
                        out Animator resolvedAnimator,
                        out bool usedFallback))
                {
                    animator = resolvedAnimator;
                    if (usedFallback && logErrors)
                    {
                        Debug.LogWarning(
                            "[CCS_PlayerRuntimeFacade] Used fallback authoritative Animator resolution on "
                            + name
                            + ".",
                            this);
                    }
                }
            }

            if (playerInteractionAnimator == null)
            {
                playerInteractionAnimator = GetComponentInChildren<CCS_PlayerInteractionAnimator>(true);
            }

            if (logErrors && !HasRequiredProductionReferences())
            {
                Debug.LogError(
                    "[CCS_PlayerRuntimeFacade] Missing required references on "
                    + name
                    + ". Run CCS Player Prefab Architecture builder.",
                    this);
            }
        }

        private Component FindComponentByTypeName(string typeName)
        {
            MonoBehaviour[] behaviours = GetComponentsInChildren<MonoBehaviour>(true);
            for (int behaviourIndex = 0; behaviourIndex < behaviours.Length; behaviourIndex++)
            {
                MonoBehaviour behaviour = behaviours[behaviourIndex];
                if (behaviour != null && behaviour.GetType().Name == typeName)
                {
                    return behaviour;
                }
            }

            return null;
        }

        #endregion
    }
}
