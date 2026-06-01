using CCS.Modules.Interaction;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_StorageContainerInteractable
// CATEGORY: Modules / Storage / Runtime / Interactables
// PURPOSE: Interaction handoff that opens or closes a storage container through the service.
// PLACEMENT: PF_CCS_PrimitiveStorageCrate alongside CCS_StorageContainer.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 1.1.2 foundation. No polished storage UI.
// =============================================================================

namespace CCS.Modules.Storage
{
    public sealed class CCS_StorageContainerInteractable : MonoBehaviour, CCS_IInteractableResultProvider
    {
        #region Variables

        [Header("Interaction")]
        [Tooltip("Maximum distance at which this container accepts interaction requests.")]
        [SerializeField] private float interactionDistance = 3f;

        [Tooltip("Optional override label. Empty uses container display name.")]
        [SerializeField] private string interactionDisplayNameOverride = string.Empty;

        private CCS_StorageContainer storageContainer;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            storageContainer = GetComponent<CCS_StorageContainer>();
            if (storageContainer == null)
            {
                storageContainer = GetComponentInParent<CCS_StorageContainer>();
            }
        }

        #endregion

        #region Public Methods

        public string GetInteractionDisplayName()
        {
            if (!string.IsNullOrWhiteSpace(interactionDisplayNameOverride))
            {
                return interactionDisplayNameOverride;
            }

            return storageContainer != null && !string.IsNullOrWhiteSpace(storageContainer.DisplayName)
                ? storageContainer.DisplayName
                : "Storage Crate";
        }

        public float GetInteractionDistance()
        {
            return interactionDistance < 0.1f ? 3f : interactionDistance;
        }

        public bool CanInteract()
        {
            return isActiveAndEnabled
                && storageContainer != null
                && storageContainer.CanOpen();
        }

        public void Interact()
        {
            TryInteract();
        }

        public bool TryInteract()
        {
            if (storageContainer == null
                || !CCS_StorageRuntimeBridge.TryGetStorageService(out CCS_StorageService storageService)
                || !storageService.IsInitialized)
            {
                return false;
            }

            if (storageService.ActiveContainer == storageContainer)
            {
                storageService.CloseContainer();
                return true;
            }

            return storageService.OpenContainer(storageContainer);
        }

        #endregion
    }
}
