using UnityEngine;

// =============================================================================
// SCRIPT: CCS_InteractableLabelTarget
// CATEGORY: Modules / Interaction / Runtime / Components
// PURPOSE: Interaction target metadata for awareness, prompts, and definition data.
// PLACEMENT: Interactable objects such as pickup cubes and door test targets.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Supplies CCS_InteractableDefinition for scanner validation and prompts.
// =============================================================================

namespace CCS.Modules.Interaction
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class CCS_InteractableLabelTarget : MonoBehaviour
    {
        #region Variables

        [SerializeField] private CCS_InteractableDefinition definition = new CCS_InteractableDefinition();

        #endregion

        #region Properties

        public CCS_InteractableDefinition Definition => definition;

        public string DisplayName => definition.DisplayName;

        public float StrictRange => definition.StrictRange;

        public Vector3 BoundsCenter
        {
            get
            {
                BoxCollider boxCollider = GetComponent<BoxCollider>();
                return boxCollider != null ? boxCollider.bounds.center : transform.position;
            }
        }

        #endregion

        #region Public Methods

        public void ConfigureForKind(CCS_InteractionKind interactionKind, string displayName)
        {
            definition.Configure(interactionKind, displayName);
        }

        public void ConfigureWalkThroughDoor(string displayName, float strictRange)
        {
            definition.ConfigureWalkThroughDoor(displayName, strictRange);
        }

        public string GetPromptText()
        {
            return definition.PromptText;
        }

        #endregion
    }
}
