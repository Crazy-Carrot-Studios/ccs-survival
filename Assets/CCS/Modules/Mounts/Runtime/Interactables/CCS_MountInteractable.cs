using CCS.Modules.Interaction;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_MountInteractable
// CATEGORY: Modules / Mounts / Runtime / Interactables
// PURPOSE: Mount, dismount, call, and wait interaction for owned horses.
// PLACEMENT: PF_CCS_Horse root.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Mounts
{
    public sealed class CCS_MountInteractable : MonoBehaviour, CCS_IInteractableResultProvider
    {
        [SerializeField] private float interactionDistance = 3.5f;
        [SerializeField] private string mountInstanceId = string.Empty;
        [SerializeField] private CCS_MountWorldActor worldActor;

        public void BindMountInstanceId(string instanceId)
        {
            mountInstanceId = instanceId ?? string.Empty;
        }

        private void Awake()
        {
            if (worldActor == null)
            {
                worldActor = GetComponent<CCS_MountWorldActor>();
            }
        }

        public string GetInteractionDisplayName()
        {
            if (CCS_MountRuntimeBridge.TryGetMountService(out CCS_MountService mountService)
                && mountService.IsInitialized
                && mountService.IsMounted
                && mountService.ActiveMountInstanceId == mountInstanceId)
            {
                return "Dismount Horse";
            }

            return "Mount Horse";
        }

        public float GetInteractionDistance()
        {
            return interactionDistance < 0.5f ? 3.5f : interactionDistance;
        }

        public bool CanInteract()
        {
            return isActiveAndEnabled
                && !string.IsNullOrWhiteSpace(mountInstanceId)
                && CCS_MountRuntimeBridge.TryGetMountService(out CCS_MountService mountService)
                && mountService.IsInitialized
                && mountService.OwnsHorse;
        }

        public void Interact()
        {
            TryInteract();
        }

        public bool TryInteract()
        {
            if (!CanInteract()
                || !CCS_MountRuntimeBridge.TryGetMountService(out CCS_MountService mountService))
            {
                return false;
            }

            if (mountService.IsMounted && mountService.ActiveMountInstanceId == mountInstanceId)
            {
                return mountService.TryDismount();
            }

            return mountService.TryMount(mountInstanceId);
        }
    }
}
