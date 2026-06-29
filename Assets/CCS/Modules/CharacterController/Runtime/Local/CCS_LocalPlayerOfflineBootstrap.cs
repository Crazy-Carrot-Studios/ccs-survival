using UnityEngine;

// =============================================================================
// SCRIPT: CCS_LocalPlayerOfflineBootstrap
// CATEGORY: Modules / CharacterController / Tests / Runtime
// PURPOSE: Safety bootstrap for offline solo use of the network-capable test player prefab.
// PLACEMENT: PF_CCS_CharacterController_Player_Networked root.
// AUTHOR: James Schilz
// CREATED: 2026-06-19
// NOTES: No-ops when Netcode is listening or the player is already configured.
// =============================================================================

using CCS.Modules.CharacterController;

namespace CCS.Modules.CharacterController.Local {
    [DefaultExecutionOrder(-150)]
    public sealed class CCS_LocalPlayerOfflineBootstrap : MonoBehaviour
    {
        #region Variables

        [SerializeField] private CCS_PlayerDisplayProfile displayProfile;

        private bool configured;

        #endregion

        #region Properties

        public CCS_PlayerDisplayProfile DisplayProfile => displayProfile;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            if (configured)
            {
                return;
            }

            configured = CCS_LocalPlayerSessionConfigurator.TryConfigureOfflinePlayer(
                gameObject,
                displayProfile,
                null);
        }

        #endregion
    }
}
