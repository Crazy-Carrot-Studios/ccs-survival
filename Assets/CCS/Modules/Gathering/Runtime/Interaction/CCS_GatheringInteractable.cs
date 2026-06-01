using CCS.Modules.Interaction;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_GatheringInteractable
// CATEGORY: Modules / Gathering / Runtime / Interaction
// PURPOSE: Interaction entry point that gathers through CCS_GatheringService.
// PLACEMENT: Same GameObject as CCS_GatheringNode on bootstrap gathering placeholders.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Uses existing interaction scan and Interact input for bootstrap verification.
// =============================================================================

namespace CCS.Modules.Gathering
{
    [RequireComponent(typeof(CCS_GatheringNode))]
    public sealed class CCS_GatheringInteractable : MonoBehaviour, CCS_IInteractableResultProvider
    {
        #region Variables

        [Header("Dependencies")]
        [Tooltip("Gathering node executed when the player interacts.")]
        [SerializeField] private CCS_GatheringNode gatheringNode;

        [Tooltip("Optional profile override for interaction distance. Falls back to active profile.")]
        [SerializeField] private CCS_GatheringProfile gatheringProfileOverride;

        private CCS_GatheringProfile resolvedProfile;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            if (gatheringNode == null)
            {
                gatheringNode = GetComponent<CCS_GatheringNode>();
            }
        }

        #endregion

        #region Public Methods

        public void ConfigureRuntime(CCS_GatheringProfile profile)
        {
            gatheringProfileOverride = profile;
            resolvedProfile = profile;
        }

        public string GetInteractionDisplayName()
        {
            if (gatheringNode == null)
            {
                return "Gather";
            }

            return gatheringNode.NodeType switch
            {
                CCS_GatheringNodeType.SmallTree => "Gather Tree",
                CCS_GatheringNodeType.Rock => "Gather Rock",
                CCS_GatheringNodeType.Bush => "Gather Bush",
                _ => "Gather"
            };
        }

        public bool CanInteract()
        {
            return gatheringNode != null && gatheringNode.CanGather();
        }

        public void Interact()
        {
            TryInteract();
        }

        public bool TryInteract()
        {
            return gatheringNode != null && gatheringNode.Gather();
        }

        public float GetInteractionDistance()
        {
            ResolveProfile();
            return resolvedProfile != null ? resolvedProfile.NodeInteractionDistance : 3f;
        }

        #endregion

        #region Private Methods

        private void ResolveProfile()
        {
            if (gatheringProfileOverride != null)
            {
                resolvedProfile = gatheringProfileOverride;
                return;
            }

            if (resolvedProfile != null)
            {
                return;
            }

            if (CCS_GatheringRuntimeBridge.TryGetGatheringService(out CCS_GatheringService gatheringService)
                && gatheringService.ActiveProfile != null)
            {
                resolvedProfile = gatheringService.ActiveProfile;
            }
        }

        #endregion
    }
}
