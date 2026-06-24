using System;
using CCS.Modules.CharacterController;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerWeaponLoadout
// CATEGORY: Modules / Weapons / Runtime / Components
// PURPOSE: Owns revolver pickup ownership until Inventory/Equipment module exists.
// PLACEMENT: PF_CCS_CharacterController_TestPlayer_Networked root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.5 world-pickup-only scope. Holster/equipped visuals deferred.
// =============================================================================

namespace CCS.Modules.Weapons
{
    [DefaultExecutionOrder(115)]
    public sealed class CCS_PlayerWeaponLoadout : MonoBehaviour
    {
        #region Variables

        [SerializeField] private CCS_RevolverVisualDefinition revolverVisualDefinition;
        [SerializeField] private CCS_RevolverController revolverController;

        private bool hasRevolver;
        private string currentWeaponId = string.Empty;

        #endregion

        #region Properties

        public bool HasRevolver => hasRevolver;

        public string CurrentWeaponId => currentWeaponId;

        public bool HasWeapon(string weaponId)
        {
            return hasRevolver && !string.IsNullOrEmpty(weaponId) && currentWeaponId == weaponId;
        }

        #endregion

        #region Events

        public event Action WeaponGranted;

        public event Action RevolverGranted;

        public event Action<string> WeaponGrantedWithId;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveReferences();
        }

        #endregion

        #region Public Methods

        public void GrantWeapon(CCS_RevolverVisualDefinition visualDefinition)
        {
            if (visualDefinition == null)
            {
                return;
            }

            revolverVisualDefinition = visualDefinition;
            hasRevolver = true;
            currentWeaponId = visualDefinition.WeaponId;

            if (revolverController != null)
            {
                revolverController.ResetAmmoToFull();
                revolverController.SetWeaponOwnershipActive(true);
            }

            WeaponGranted?.Invoke();
            RevolverGranted?.Invoke();
            WeaponGrantedWithId?.Invoke(currentWeaponId);
        }

        #endregion

        #region Private Methods

        private void ResolveReferences()
        {
            if (revolverController == null)
            {
                revolverController = GetComponent<CCS_RevolverController>();
            }
        }

        #endregion
    }
}
