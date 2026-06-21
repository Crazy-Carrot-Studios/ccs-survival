// =============================================================================
// SCRIPT: CCS_InteractionAnimationKeyUtility
// CATEGORY: Modules / Interaction / Runtime / Data
// PURPOSE: Maps interaction animation keys to Animator trigger parameter names.
// PLACEMENT: Runtime static utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Right-hand only. No left-hand or 90-degree variants yet.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public static class CCS_InteractionAnimationKeyUtility
    {
        #region Public Methods

        public static string ToAnimatorTriggerName(CCS_InteractionAnimationKey animationKey)
        {
            switch (animationKey)
            {
                case CCS_InteractionAnimationKey.WalkThroughDoor_RH:
                    return "WalkThroughDoor_RH";
                case CCS_InteractionAnimationKey.PickUp_RH:
                default:
                    return "PickUp_RH";
            }
        }

        public static float GetLockDuration(CCS_InteractionAnimationKey animationKey)
        {
            switch (animationKey)
            {
                case CCS_InteractionAnimationKey.WalkThroughDoor_RH:
                    return CCS_InteractionConstants.WalkThroughDoorRightHandLockDuration;
                case CCS_InteractionAnimationKey.PickUp_RH:
                default:
                    return CCS_InteractionConstants.PickUpRightHandLockDuration;
            }
        }

        #endregion
    }
}
