using UnityEngine;

// =============================================================================
// SCRIPT: CCS_InteractionScanner
// CATEGORY: Modules / Interaction / Runtime / Detection
// PURPOSE: Profile-driven forward raycast to find CCS_IInteractable targets.
// PLACEMENT: Owned by CCS_InteractionService. No inventory or UI references.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Spherecast, focus assist, and gamepad aim deferred to future milestones.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public sealed class CCS_InteractionScanner
    {
        #region Public Methods

        public CCS_InteractionDetectionResult ScanForward(
            Vector3 origin,
            Vector3 forward,
            CCS_InteractionProfile profile)
        {
            if (profile == null || forward.sqrMagnitude <= 0.0001f)
            {
                return CCS_InteractionDetectionResult.None;
            }

            Vector3 direction = forward.normalized;
            float maxDistance = profile.InteractionDistance;
            LayerMask layerMask = profile.InteractionLayers;

            if (Physics.Raycast(
                    origin,
                    direction,
                    out RaycastHit hit,
                    maxDistance,
                    layerMask,
                    QueryTriggerInteraction.Collide))
            {
                CCS_IInteractable interactable = hit.collider.GetComponentInParent<CCS_IInteractable>();
                if (interactable != null)
                {
                    float allowedDistance = interactable.GetInteractionDistance();
                    if (hit.distance <= allowedDistance)
                    {
                        return new CCS_InteractionDetectionResult(true, interactable, hit.distance, hit);
                    }
                }
            }

            return CCS_InteractionDetectionResult.None;
        }

        #endregion
    }
}
