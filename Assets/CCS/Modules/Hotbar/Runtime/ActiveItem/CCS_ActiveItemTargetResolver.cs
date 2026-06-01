using CCS.Modules.Gathering;
using CCS.Modules.Interaction;
using CCS.Modules.WorldResources;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ActiveItemTargetResolver
// CATEGORY: Modules / Hotbar / Runtime / ActiveItem
// PURPOSE: Resolves harvest targets from CCS_InteractionService current target.
// PLACEMENT: Called by CCS_ActiveItemService during tool use routing.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Does not add a second forward-ray targeting system.
// =============================================================================

namespace CCS.Modules.Hotbar
{
    public static class CCS_ActiveItemTargetResolver
    {
        public static bool TryResolveFromInteraction(
            CCS_InteractionService interactionService,
            Vector3 useOrigin,
            out CCS_ActiveItemTargetContext targetContext)
        {
            targetContext = CCS_ActiveItemTargetContext.None;

            if (interactionService == null || !interactionService.IsInitialized || !interactionService.HasCurrentTarget)
            {
                return false;
            }

            return TryResolveFromInteractable(interactionService.CurrentTarget, useOrigin, out targetContext);
        }

        public static bool TryResolveFromInteractable(
            CCS_IInteractable interactable,
            Vector3 useOrigin,
            out CCS_ActiveItemTargetContext targetContext)
        {
            targetContext = CCS_ActiveItemTargetContext.None;
            if (interactable == null)
            {
                return false;
            }

            if (!(interactable is Component component) || component == null)
            {
                targetContext = new CCS_ActiveItemTargetContext(
                    CCS_ActiveItemTargetKind.UnsupportedInteractable,
                    interactable.GetInteractionDisplayName(),
                    "Interactable",
                    false,
                    null,
                    null);
                return true;
            }

            float maxDistance = Mathf.Max(0.1f, interactable.GetInteractionDistance());
            float distance = Vector3.Distance(useOrigin, component.transform.position);
            if (distance > maxDistance)
            {
                targetContext = new CCS_ActiveItemTargetContext(
                    CCS_ActiveItemTargetKind.UnsupportedInteractable,
                    interactable.GetInteractionDisplayName(),
                    "Interactable",
                    true,
                    null,
                    null);
                return true;
            }

            CCS_GatheringNode gatheringNode = component.GetComponent<CCS_GatheringNode>();
            if (gatheringNode != null)
            {
                targetContext = new CCS_ActiveItemTargetContext(
                    CCS_ActiveItemTargetKind.GatheringNode,
                    interactable.GetInteractionDisplayName(),
                    gatheringNode.NodeType.ToString(),
                    false,
                    gatheringNode,
                    null);
                return true;
            }

            CCS_HarvestableResource harvestableResource = component.GetComponent<CCS_HarvestableResource>();
            if (harvestableResource != null)
            {
                string resourceLabel = harvestableResource.ResourceDefinition != null
                    ? harvestableResource.ResourceDefinition.DisplayName
                    : interactable.GetInteractionDisplayName();

                targetContext = new CCS_ActiveItemTargetContext(
                    CCS_ActiveItemTargetKind.HarvestableResource,
                    resourceLabel,
                    "HarvestableResource",
                    false,
                    null,
                    harvestableResource);
                return true;
            }

            targetContext = new CCS_ActiveItemTargetContext(
                CCS_ActiveItemTargetKind.UnsupportedInteractable,
                interactable.GetInteractionDisplayName(),
                "Interactable",
                false,
                null,
                null);
            return true;
        }
    }
}
