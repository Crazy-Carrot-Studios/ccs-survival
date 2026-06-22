using CCS.Modules.Interaction;
using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WeaponPickupInteractable
// CATEGORY: Modules / Weapons / Runtime / Components
// PURPOSE: World revolver pickup that grants weapon ownership to the local player.
// PLACEMENT: PF_CCS_RevolverM1879_WorldPickup prefab root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Uses interaction scanner pickup flow. No shooting logic on pickup prefab.
// =============================================================================

namespace CCS.Modules.Weapons
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(CCS_InteractableLabelTarget))]
    public sealed class CCS_WeaponPickupInteractable : MonoBehaviour, CCS_IInteractable
    {
        #region Variables

        [SerializeField] private CCS_RevolverVisualDefinition visualDefinition;

        #endregion

        #region Public Methods

        public void Configure(CCS_RevolverVisualDefinition definition)
        {
            visualDefinition = definition;
            CCS_InteractableLabelTarget labelTarget = GetComponent<CCS_InteractableLabelTarget>();
            if (labelTarget != null)
            {
                labelTarget.ConfigureForKind(CCS_InteractionKind.Pickup, "Revolver");
            }
        }

        public bool CanInteract(CCS_InteractionRequest request)
        {
            if (!isActiveAndEnabled || visualDefinition == null)
            {
                return false;
            }

            float distance = Vector3.Distance(request.OriginPosition, transform.position);
            return distance <= request.MaxRange + 0.75f;
        }

        public bool Interact(CCS_InteractionRequest request, out CCS_InteractionResult result)
        {
            ulong targetId = request.TargetNetworkObjectId;
            if (!CanInteract(request))
            {
                result = CCS_InteractionResult.Failure(targetId, "Weapon pickup is out of range.");
                return false;
            }

            CCS_PlayerWeaponLoadout loadout = FindLocalPlayerLoadout();
            if (loadout == null)
            {
                result = CCS_InteractionResult.Failure(targetId, "No player weapon loadout found.");
                return false;
            }

            if (loadout.HasRevolver)
            {
                result = CCS_InteractionResult.Failure(targetId, "Player already owns a revolver.");
                return false;
            }

            loadout.GrantWeapon(visualDefinition);
            result = CCS_InteractionResult.Success(
                targetId,
                CCS_InteractionAnimationKey.PickUp_RH,
                "Revolver collected.");
            DestroyPickupInstance();
            return true;
        }

        #endregion

        #region Private Methods

        private static CCS_PlayerWeaponLoadout FindLocalPlayerLoadout()
        {
            NetworkManager networkManager = NetworkManager.Singleton;
            if (networkManager != null && networkManager.IsListening)
            {
                NetworkObject playerObject = networkManager.LocalClient != null
                    ? networkManager.LocalClient.PlayerObject
                    : null;
                if (playerObject != null)
                {
                    CCS_PlayerWeaponLoadout loadout = playerObject.GetComponent<CCS_PlayerWeaponLoadout>();
                    if (loadout != null)
                    {
                        return loadout;
                    }
                }
            }

            CCS_PlayerWeaponLoadout[] loadouts = Object.FindObjectsByType<CCS_PlayerWeaponLoadout>(FindObjectsSortMode.None);
            for (int i = 0; i < loadouts.Length; i++)
            {
                CCS_PlayerWeaponLoadout candidate = loadouts[i];
                if (candidate != null && candidate.isActiveAndEnabled)
                {
                    return candidate;
                }
            }

            return null;
        }

        private void DestroyPickupInstance()
        {
            Destroy(gameObject);
        }

        #endregion
    }
}
