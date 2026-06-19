using CCS.Modules.CharacterController.Tests;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using TMPro;

// =============================================================================
// SCRIPT: CCS_NetworkPlayerNameplate
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Synchronizes a test-only overhead display name for networked players.
// PLACEMENT: PF_CCS_CharacterController_TestPlayer_Networked root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Uses NetworkVariable plus owner ServerRpc. No account or lobby services.
//        Local owner hides their nameplate on this client only.
//        Session events decouple join notifications from spawn timing.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode
{
    public sealed class CCS_NetworkPlayerNameplate : NetworkBehaviour
    {
        #region Variables

        private readonly NetworkVariable<FixedString32Bytes> displayName =
            new NetworkVariable<FixedString32Bytes>(
                default,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        [SerializeField] private TMP_Text nameplateText;
        [SerializeField] private Transform nameplateRoot;
        [SerializeField] private CCS_PlayerNameplateBillboard nameplateBillboard;

        #endregion

        #region Public Methods

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            ResolveReferences();
            displayName.OnValueChanged += HandleDisplayNameChanged;
            ApplyDisplayName(displayName.Value.ToString());
            ApplyLocalOwnershipVisibility();

            if (IsOwner)
            {
                SubmitDisplayNameServerRpc(CCS_LocalMultiplayerPlayerNameCache.PendingLocalDisplayName);
            }

            CCS_TestPlayerSessionEvents.RaisePlayerSpawned(CreateSessionContext());
        }

        public override void OnNetworkDespawn()
        {
            displayName.OnValueChanged -= HandleDisplayNameChanged;
            base.OnNetworkDespawn();
        }

        public string GetDisplayNameForJoinAnnouncement()
        {
            return displayName.Value.ToString();
        }

        public void BroadcastJoinNotification(string playerName, ulong ownerClientId)
        {
            NotifyPlayerJoinedClientRpc(playerName, ownerClientId);
        }

        #endregion

        #region Private Methods

        [ServerRpc]
        private void SubmitDisplayNameServerRpc(string requestedName)
        {
            displayName.Value = CCS_MultiplayerPlayerNameUtility.Sanitize(requestedName);
            string submittedName = displayName.Value.ToString();
            CCS_TestPlayerSessionEvents.RaisePlayerNameChanged(
                new CCS_TestPlayerNameChangedContext(OwnerClientId, submittedName, gameObject));
        }

        [ClientRpc]
        private void NotifyPlayerJoinedClientRpc(string playerName, ulong ownerClientId)
        {
            CCS_NetworkPlayerJoinAnnouncer.ClientReceiveJoinAnnouncement(ownerClientId, playerName);
        }

        private CCS_TestPlayerSessionContext CreateSessionContext()
        {
            return new CCS_TestPlayerSessionContext(
                OwnerClientId,
                gameObject,
                isNetworkSession: true,
                IsOwner);
        }

        private void ResolveReferences()
        {
            if (nameplateRoot == null)
            {
                Transform root = transform.Find(CCS_NetcodeTestConstants.NameplateRootObjectName);
                nameplateRoot = root != null ? root : nameplateRoot;
            }

            if (nameplateBillboard == null && nameplateRoot != null)
            {
                nameplateBillboard = nameplateRoot.GetComponent<CCS_PlayerNameplateBillboard>();
            }

            if (nameplateText == null && nameplateRoot != null)
            {
                Transform textTransform = nameplateRoot.Find(CCS_NetcodeTestConstants.NameplateTextObjectName);
                if (textTransform == null)
                {
                    textTransform = nameplateRoot.Find(CCS_NetcodeTestConstants.LegacyNameplateTextObjectName);
                }

                if (textTransform != null)
                {
                    nameplateText = textTransform.GetComponent<TMP_Text>();
                }

                if (nameplateText == null)
                {
                    nameplateText = nameplateRoot.GetComponentInChildren<TMP_Text>(true);
                }
            }
        }

        private void ApplyLocalOwnershipVisibility()
        {
            if (nameplateBillboard == null && nameplateRoot != null)
            {
                nameplateBillboard = nameplateRoot.GetComponent<CCS_PlayerNameplateBillboard>();
            }

            if (nameplateBillboard != null)
            {
                nameplateBillboard.ApplyNameplateVisibility(IsOwner);
            }
        }

        private void HandleDisplayNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
        {
            ApplyDisplayName(newValue.ToString());
        }

        private void ApplyDisplayName(string value)
        {
            if (nameplateText == null)
            {
                return;
            }

            nameplateText.text = string.IsNullOrWhiteSpace(value)
                ? CCS_NetcodeTestConstants.DefaultDisplayName
                : value;
        }

        #endregion
    }
}

