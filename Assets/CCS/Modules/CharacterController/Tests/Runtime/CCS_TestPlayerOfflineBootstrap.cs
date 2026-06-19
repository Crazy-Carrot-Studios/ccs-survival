using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TestPlayerOfflineBootstrap
// CATEGORY: Modules / CharacterController / Tests / Runtime
// PURPOSE: Safety bootstrap for offline solo use of the network-capable test player prefab.
// PLACEMENT: PF_CCS_CharacterController_TestPlayer_Networked root.
// AUTHOR: James Schilz
// CREATED: 2026-06-19
// NOTES: No-ops when Netcode is listening or the player is already configured.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests
{
    [DefaultExecutionOrder(-150)]
    public sealed class CCS_TestPlayerOfflineBootstrap : MonoBehaviour
    {
        #region Variables

        [SerializeField] private CCS_TestPlayerDisplayProfile displayProfile;

        private bool configured;

        #endregion

        #region Properties

        public CCS_TestPlayerDisplayProfile DisplayProfile => displayProfile;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            if (configured)
            {
                return;
            }

            configured = CCS_TestPlayerLocalSessionConfigurator.TryConfigureOfflinePlayer(
                gameObject,
                displayProfile,
                null);
        }

        #endregion
    }
}
