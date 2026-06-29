using CCS.Modules.Attributes;
using CCS.Modules.Attributes.Tests;
using CCS.Modules.CharacterController.Validation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting.APIUpdating;

// =============================================================================
// SCRIPT: CCS_PlayerDiagnosticsInputRouter
// CATEGORY: Modules / CharacterController / Tests / Runtime / Diagnostics
// PURPOSE: Scene-level test damage input gated by CCS_CharacterControllerDiagnosticsManager.
// PLACEMENT: CCS_DiagnosticsManager on SCN_CCS_CharacterController_Validation only.
// AUTHOR: James Schilz
// CREATED: 2026-06-29
// NOTES: Replaces prefab-root CCS_TestPlayerAttributeDebugInput. No-op when EnableDamageDiagnostics is false.
// =============================================================================

namespace CCS.Modules.CharacterController.Diagnostics {
    [MovedFrom(true, "CCS.Modules.CharacterController.Tests", "CCS.Modules.CharacterController.Tests.Runtime", "CCS_TestPlayerAttributeDebugInputRouter")]
    public sealed class CCS_PlayerDiagnosticsInputRouter : MonoBehaviour
    {
        #region Variables

        [SerializeField] private Key damageKey = Key.K;

        [SerializeField] private float damageAmount = CCS_AttributesTestConstants.TestDamageAmount;

        #endregion

        #region Unity Callbacks

        private void Update()
        {
            CCS_CharacterControllerDiagnosticsManager manager = CCS_CharacterControllerDiagnosticsManager.ActiveInstance;
            if (manager == null || !manager.EnableDamageDiagnostics)
            {
                return;
            }

            if (SceneManager.GetActiveScene().name != CCS_ValidationUiConstants.MasterTestSceneName)
            {
                return;
            }

            CCS_NetworkAttributeReplicator replicator = ResolveLocalOwnerReplicator();
            if (replicator == null)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard[damageKey].wasPressedThisFrame)
            {
                return;
            }

            replicator.RequestSelfDamage(damageAmount, "TestDebugInputRouter");
        }

        #endregion

        #region Private Methods

        private static CCS_NetworkAttributeReplicator ResolveLocalOwnerReplicator()
        {
            CCS_NetworkAttributeReplicator[] replicators =
                FindObjectsByType<CCS_NetworkAttributeReplicator>(FindObjectsSortMode.None);
            for (int i = 0; i < replicators.Length; i++)
            {
                CCS_NetworkAttributeReplicator candidate = replicators[i];
                if (candidate == null || !IsLocalOwner(candidate.gameObject))
                {
                    continue;
                }

                return candidate;
            }

            return null;
        }

        private static bool IsLocalOwner(GameObject playerRoot)
        {
            Unity.Netcode.NetworkObject networkObject = playerRoot.GetComponent<Unity.Netcode.NetworkObject>();
            if (networkObject == null || !networkObject.IsSpawned)
            {
                return true;
            }

            return networkObject.IsOwner;
        }

        #endregion
    }
}
