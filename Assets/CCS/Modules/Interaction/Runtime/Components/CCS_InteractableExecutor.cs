using UnityEngine;

// =============================================================================
// SCRIPT: CCS_InteractableExecutor
// CATEGORY: Modules / Interaction / Runtime / Components
// PURPOSE: Executes interactable definition behavior on successful interaction.
// PLACEMENT: Interactable objects alongside CCS_InteractableLabelTarget.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Pickup destroys on success. WalkThroughDoor opens via CCS_InteractableDoor.
// =============================================================================

namespace CCS.Modules.Interaction
{
    [RequireComponent(typeof(CCS_InteractableLabelTarget))]
    public sealed class CCS_InteractableExecutor : MonoBehaviour, CCS_IInteractable
    {
        #region Variables

        private CCS_InteractableLabelTarget labelTarget;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            labelTarget = GetComponent<CCS_InteractableLabelTarget>();
        }

        #endregion

        #region Public Methods

        public bool CanInteract(CCS_InteractionRequest request)
        {
            if (!isActiveAndEnabled || labelTarget == null)
            {
                return false;
            }

            if (labelTarget.Definition.InteractionKind == CCS_InteractionKind.WalkThroughDoor)
            {
                CCS_InteractableDoor door = GetComponent<CCS_InteractableDoor>();
                return door == null || !door.IsOpen;
            }

            return true;
        }

        public bool Interact(CCS_InteractionRequest request, out CCS_InteractionResult result)
        {
            if (!CanInteract(request))
            {
                result = CCS_InteractionResult.Failure(0, "Interactable is unavailable.");
                return false;
            }

            CCS_InteractableDefinition definition = labelTarget.Definition;
            ulong targetNetworkObjectId = request.TargetNetworkObjectId;
            string message = BuildSuccessMessage(definition);
            result = CCS_InteractionResult.Success(targetNetworkObjectId, definition.AnimationKey, message);

            switch (definition.InteractionKind)
            {
                case CCS_InteractionKind.WalkThroughDoor:
                    CCS_InteractableDoor door = GetComponent<CCS_InteractableDoor>();
                    if (door != null)
                    {
                        door.Open();
                    }
                    break;
                case CCS_InteractionKind.Pickup:
                default:
                    if (definition.DestroyOnSuccess)
                    {
                        Destroy(gameObject);
                    }
                    break;
            }

            return true;
        }

        #endregion

        #region Private Methods

        private static string BuildSuccessMessage(CCS_InteractableDefinition definition)
        {
            switch (definition.InteractionKind)
            {
                case CCS_InteractionKind.WalkThroughDoor:
                    return "Walk-through door interaction completed.";
                case CCS_InteractionKind.Pickup:
                default:
                    return "Pickup interaction completed.";
            }
        }

        #endregion
    }
}
