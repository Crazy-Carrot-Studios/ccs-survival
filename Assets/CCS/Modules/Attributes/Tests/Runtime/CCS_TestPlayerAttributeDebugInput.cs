using UnityEngine;
using UnityEngine.InputSystem;

// =============================================================================
// SCRIPT: CCS_TestPlayerAttributeDebugInput
// CATEGORY: Modules / Attributes / Tests / Runtime
// PURPOSE: Test-only self-damage input for local owner health validation.
// PLACEMENT: Networked test player root in master/hosting test scenes.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Press K to apply 10 self-damage. Multiplayer routes through server authority.
// =============================================================================

namespace CCS.Modules.Attributes.Tests
{
    public sealed class CCS_TestPlayerAttributeDebugInput : MonoBehaviour
    {
        #region Variables

        [SerializeField] private CCS_NetworkAttributeReplicator attributeReplicator;

        [SerializeField] private Key damageKey = Key.K;

        [SerializeField] private float damageAmount = CCS_AttributesTestConstants.TestDamageAmount;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            if (attributeReplicator == null)
            {
                attributeReplicator = GetComponent<CCS_NetworkAttributeReplicator>();
            }
        }

        private void Update()
        {
            if (!IsLocalOwner() || attributeReplicator == null)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard[damageKey].wasPressedThisFrame)
            {
                return;
            }

            attributeReplicator.RequestSelfDamage(damageAmount, "TestDebugInput");
        }

        #endregion

        #region Private Methods

        private bool IsLocalOwner()
        {
            Unity.Netcode.NetworkObject networkObject = GetComponent<Unity.Netcode.NetworkObject>();
            if (networkObject == null || !networkObject.IsSpawned)
            {
                return true;
            }

            return networkObject.IsOwner;
        }

        #endregion
    }
}
