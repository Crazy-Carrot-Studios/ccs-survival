using CCS.Modules.Attributes;
using CCS.Modules.Attributes.Tests;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_TestPlayerAttributeDebugInputRouter
// CATEGORY: Modules / CharacterController / Tests / Runtime / Diagnostics
// PURPOSE: Scene-level test damage input gated by CCS_CharacterControllerTestingManager.
// PLACEMENT: CCS_TestingManager on SCN_CCS_CharacterController_MasterTest only.
// AUTHOR: James Schilz
// CREATED: 2026-06-29
// NOTES: Replaces prefab-root CCS_TestPlayerAttributeDebugInput. No-op when EnableTestDamage is false.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests
{
    public sealed class CCS_TestPlayerAttributeDebugInputRouter : MonoBehaviour
    {
        #region Variables

        [SerializeField] private Key damageKey = Key.K;

        [SerializeField] private float damageAmount = CCS_AttributesTestConstants.TestDamageAmount;

        #endregion

        #region Unity Callbacks

        private void Update()
        {
            CCS_CharacterControllerTestingManager manager = CCS_CharacterControllerTestingManager.ActiveInstance;
            if (manager == null || !manager.EnableTestDamage)
            {
                return;
            }

            if (SceneManager.GetActiveScene().name != CCS_MasterTestUiConstants.MasterTestSceneName)
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
