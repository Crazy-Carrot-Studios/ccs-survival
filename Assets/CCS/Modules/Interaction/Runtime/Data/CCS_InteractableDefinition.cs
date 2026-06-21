using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_InteractableDefinition
// CATEGORY: Modules / Interaction / Runtime / Data
// PURPOSE: Data definition for interactable kind, prompt, animation, and strict range.
// PLACEMENT: Serialized on CCS_InteractableLabelTarget and future interactable assets.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Pickup and WalkThroughDoor defaults only. No door movement yet.
// =============================================================================

namespace CCS.Modules.Interaction
{
    [Serializable]
    public sealed class CCS_InteractableDefinition
    {
        #region Variables

        [SerializeField] private CCS_InteractionKind interactionKind = CCS_InteractionKind.Pickup;
        [SerializeField] private string displayName = "Interactable";
        [SerializeField] private string promptText = CCS_InteractionConstants.DefaultInteractionPromptText;
        [SerializeField] private CCS_InteractionAnimationKey animationKey = CCS_InteractionAnimationKey.PickUp_RH;
        [SerializeField] private bool destroyOnSuccess = true;
        [SerializeField] private float strictRange = CCS_InteractionConstants.DefaultStrictPickupDistance;

        #endregion

        #region Properties

        public CCS_InteractionKind InteractionKind => interactionKind;

        public string DisplayName => displayName;

        public string PromptText => string.IsNullOrWhiteSpace(promptText)
            ? CCS_InteractionConstants.DefaultInteractionPromptText
            : promptText;

        public CCS_InteractionAnimationKey AnimationKey => animationKey;

        public bool DestroyOnSuccess => destroyOnSuccess;

        public float StrictRange => ResolveStrictRange();

        #endregion

        #region Public Methods

        public void Configure(CCS_InteractionKind kind, string configuredDisplayName)
        {
            interactionKind = kind;
            displayName = configuredDisplayName;
            ApplyKindDefaults();
        }

        public void ConfigureWalkThroughDoor(string configuredDisplayName, float configuredStrictRange)
        {
            interactionKind = CCS_InteractionKind.WalkThroughDoor;
            displayName = configuredDisplayName;
            promptText = CCS_InteractionConstants.DefaultInteractionPromptText;
            animationKey = CCS_InteractionAnimationKey.WalkThroughDoor_RH;
            destroyOnSuccess = false;
            strictRange = configuredStrictRange;
        }

        public void ApplyKindDefaults()
        {
            switch (interactionKind)
            {
                case CCS_InteractionKind.WalkThroughDoor:
                    animationKey = CCS_InteractionAnimationKey.WalkThroughDoor_RH;
                    destroyOnSuccess = false;
                    strictRange = CCS_InteractionConstants.DefaultWalkThroughDoorStrictRange;
                    break;
                case CCS_InteractionKind.Pickup:
                default:
                    animationKey = CCS_InteractionAnimationKey.PickUp_RH;
                    destroyOnSuccess = true;
                    strictRange = CCS_InteractionConstants.DefaultStrictPickupDistance;
                    break;
            }

            if (string.IsNullOrWhiteSpace(promptText))
            {
                promptText = CCS_InteractionConstants.DefaultInteractionPromptText;
            }
        }

        #endregion

        #region Private Methods

        private float ResolveStrictRange()
        {
            switch (interactionKind)
            {
                case CCS_InteractionKind.WalkThroughDoor:
                    return Mathf.Clamp(
                        strictRange,
                        CCS_InteractionConstants.WalkThroughDoorStrictRangeMin,
                        CCS_InteractionConstants.WalkThroughDoorStrictRangeMax);
                case CCS_InteractionKind.Pickup:
                default:
                    return CCS_InteractionConstants.DefaultStrictPickupDistance;
            }
        }

        #endregion
    }
}
