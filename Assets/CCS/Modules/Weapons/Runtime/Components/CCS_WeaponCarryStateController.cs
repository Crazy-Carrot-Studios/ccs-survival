using System;
using CCS.Modules.CharacterController;
using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WeaponCarryStateController
// CATEGORY: Modules / Weapons / Runtime / Components
// PURPOSE: Owns weapon carry state for locomotion, visuals, and local firearm aim camera routing.
// PLACEMENT: PF_CCS_CharacterController_TestPlayer_Networked root.
// AUTHOR: James Schilz
// CREATED: 2026-06-23
// NOTES: Local owner drives state. NetworkVariable replicates carry state for remote visuals.
// =============================================================================

namespace CCS.Modules.Weapons
{
    [DefaultExecutionOrder(35)]
    public sealed class CCS_WeaponCarryStateController : NetworkBehaviour, CCS_IWeaponAimGate, CCS_IWeaponCarryStateCameraSource
    {
        #region Variables

        [SerializeField] private CCS_CharacterInputActionProvider inputProvider;
        [SerializeField] private CCS_PlayerWeaponLoadout playerWeaponLoadout;

        private readonly NetworkVariable<byte> networkedCarryState = new(
            (byte)CCS_WeaponCarryState.None,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        private CCS_WeaponCarryState localCarryState = CCS_WeaponCarryState.None;

        #endregion

        #region Properties

        public CCS_WeaponCarryState CarryState => ResolveCarryState();

        public bool UsesCombatLocomotion =>
            CarryState == CCS_WeaponCarryState.EquippedInHands
            || CarryState == CCS_WeaponCarryState.Aiming;

        public bool CanUseAimMovement => UsesCombatLocomotion;

        public bool CanUseFirearmAimCamera =>
            CarryState == CCS_WeaponCarryState.Aiming
            && playerWeaponLoadout != null
            && playerWeaponLoadout.HasRevolver;

        public bool ShouldDriveLocalCamera => ShouldDriveLocalCameraInternal();

        public bool WantsAimOverShoulderCamera => CanUseFirearmAimCamera;

        public byte CarryStateValue => (byte)CarryState;

        #endregion

        #region Events

        public event Action<CCS_WeaponCarryState> CarryStateChanged;

        event Action CCS_IWeaponCarryStateCameraSource.CarryStateChanged
        {
            add => carryStateCameraChanged += value;
            remove => carryStateCameraChanged -= value;
        }

        private event Action carryStateCameraChanged;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            if (playerWeaponLoadout != null)
            {
                playerWeaponLoadout.RevolverGranted += HandleRevolverGranted;
            }

            RefreshLocalCarryState(forceNotify: true);
        }

        private void OnDisable()
        {
            if (playerWeaponLoadout != null)
            {
                playerWeaponLoadout.RevolverGranted -= HandleRevolverGranted;
            }
        }

        private void Update()
        {
            if (!IsLocalAuthority())
            {
                return;
            }

            RefreshLocalCarryState(forceNotify: false);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            networkedCarryState.OnValueChanged += HandleNetworkedCarryStateChanged;
            CarryStateChanged?.Invoke(CarryState);
        }

        public override void OnNetworkDespawn()
        {
            networkedCarryState.OnValueChanged -= HandleNetworkedCarryStateChanged;
            base.OnNetworkDespawn();
        }

        #endregion

        #region Public Methods

        public void SetEquippedInHandsForCombat(bool equippedInHands)
        {
            if (!IsLocalAuthority())
            {
                return;
            }

            if (playerWeaponLoadout == null || !playerWeaponLoadout.HasRevolver)
            {
                ApplyCarryState(CCS_WeaponCarryState.None);
                return;
            }

            if (equippedInHands)
            {
                ApplyCarryState(CCS_WeaponCarryState.EquippedInHands);
            }
            else if (inputProvider != null && inputProvider.AimHeld)
            {
                ApplyCarryState(CCS_WeaponCarryState.Aiming);
            }
            else
            {
                ApplyCarryState(CCS_WeaponCarryState.Holstered);
            }
        }

        #endregion

        #region Private Methods

        private void ResolveReferences()
        {
            if (inputProvider == null)
            {
                inputProvider = GetComponent<CCS_CharacterInputActionProvider>();
            }

            if (playerWeaponLoadout == null)
            {
                playerWeaponLoadout = GetComponent<CCS_PlayerWeaponLoadout>();
            }
        }

        private void HandleRevolverGranted()
        {
            ApplyCarryState(CCS_WeaponCarryState.Holstered);
        }

        private void HandleNetworkedCarryStateChanged(byte previousValue, byte newValue)
        {
            if (IsLocalAuthority())
            {
                return;
            }

            CarryStateChanged?.Invoke((CCS_WeaponCarryState)newValue);
        }

        private void RefreshLocalCarryState(bool forceNotify)
        {
            CCS_WeaponCarryState desiredState = EvaluateDesiredCarryState();
            if (!forceNotify && desiredState == localCarryState)
            {
                return;
            }

            ApplyCarryState(desiredState);
        }

        private CCS_WeaponCarryState EvaluateDesiredCarryState()
        {
            if (playerWeaponLoadout == null || !playerWeaponLoadout.HasRevolver)
            {
                return CCS_WeaponCarryState.None;
            }

            if (localCarryState == CCS_WeaponCarryState.EquippedInHands
                && inputProvider != null
                && !inputProvider.AimHeld)
            {
                return CCS_WeaponCarryState.EquippedInHands;
            }

            if (inputProvider != null && inputProvider.AimHeld && inputProvider.InputAccepted)
            {
                return CCS_WeaponCarryState.Aiming;
            }

            return CCS_WeaponCarryState.Holstered;
        }

        private void ApplyCarryState(CCS_WeaponCarryState nextState)
        {
            if (localCarryState == nextState && !IsLocalAuthority())
            {
                return;
            }

            localCarryState = nextState;
            if (IsSpawned && IsOwner)
            {
                networkedCarryState.Value = (byte)nextState;
            }

            CarryStateChanged?.Invoke(nextState);
            carryStateCameraChanged?.Invoke();
        }

        private bool ShouldDriveLocalCameraInternal()
        {
            if (!Application.isPlaying)
            {
                return false;
            }

            if (!IsSpawned)
            {
                return true;
            }

            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
            {
                return true;
            }

            return IsOwner;
        }

        private CCS_WeaponCarryState ResolveCarryState()
        {
            if (IsSpawned && !IsOwner)
            {
                return (CCS_WeaponCarryState)networkedCarryState.Value;
            }

            return localCarryState;
        }

        private bool IsLocalAuthority()
        {
            if (!IsSpawned)
            {
                return true;
            }

            return IsOwner;
        }

        #endregion
    }
}
