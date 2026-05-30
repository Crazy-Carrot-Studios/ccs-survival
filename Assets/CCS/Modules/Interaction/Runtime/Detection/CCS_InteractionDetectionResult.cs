using UnityEngine;

// =============================================================================
// SCRIPT: CCS_InteractionDetectionResult
// CATEGORY: Modules / Interaction / Runtime / Detection
// PURPOSE: Forward-raycast detection output for the interaction scanner.
// PLACEMENT: Returned by CCS_InteractionScanner.ScanForward.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Read-only snapshot. No spherecast or aim assist in 0.3.9.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public readonly struct CCS_InteractionDetectionResult
    {
        #region Public Methods

        public CCS_InteractionDetectionResult(
            bool hasTarget,
            CCS_IInteractable interactable,
            float distance,
            RaycastHit hit)
        {
            HasTarget = hasTarget;
            Interactable = interactable;
            Distance = distance;
            Hit = hit;
        }

        public static CCS_InteractionDetectionResult None =>
            new CCS_InteractionDetectionResult(false, null, 0f, default);

        #endregion

        #region Properties

        public bool HasTarget { get; }

        public CCS_IInteractable Interactable { get; }

        public float Distance { get; }

        public RaycastHit Hit { get; }

        #endregion
    }
}
