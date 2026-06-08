using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalBootstrapProfileSlot
// CATEGORY: Survival / Runtime / Foundation / Bootstrap
// PURPOSE: Serializable placeholder for future profile-driven bootstrap setup slots on survival scenes.
// PLACEMENT: Serialized on CCS_SurvivalBootstrap. No dynamic asset loading or save IO.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Setup only. See CCS_SurvivalFrameworkFutureMarkers.ProfileLoadingAbstraction.
// =============================================================================

namespace CCS.Project
{
    [Serializable]
    public sealed class CCS_SurvivalBootstrapProfileSlot
    {
        #region Variables

        [Header("Bootstrap Profile Slot")]
        [Tooltip("Stable setup slot ID (ccs.survival.bootstrap.slot.*). Not a scene or save identity.")]
        [SerializeField] private string slotId = string.Empty;

        [Tooltip("Optional setup profile asset. Required only when Is Required is enabled.")]
        [SerializeField] private CCS_SurvivalProfileBase profile;

        [Tooltip("When enabled, a valid profile reference must be assigned for this slot.")]
        [SerializeField] private bool isRequired;

        #endregion

        #region Public Methods

        public CCS_SurvivalValidationResult ValidateSlot()
        {
            if (string.IsNullOrWhiteSpace(slotId))
            {
                if (!isRequired)
                {
                    return CCS_SurvivalValidationResult.Pass("Empty bootstrap profile slot ID ignored (slot not required).");
                }

                return CCS_SurvivalValidationResult.Fail("Bootstrap profile slot ID is required when the slot is marked required.");
            }

            CCS_SurvivalValidationResult slotIdValidation = CCS_SurvivalIdentityUtility.ValidateStableRuntimeId(
                slotId,
                CCS_SurvivalRuntimeConstants.BootstrapProfileSlotPrefix,
                "Bootstrap profile slot ID");

            if (!slotIdValidation.IsSuccess)
            {
                return slotIdValidation;
            }

            if (profile == null)
            {
                if (isRequired)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Bootstrap profile slot '{slotId}' is required but no profile asset is assigned.");
                }

                return CCS_SurvivalValidationResult.Pass();
            }

            return CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
        }

        #endregion

        #region Properties

        public string SlotId => slotId;

        public CCS_SurvivalProfileBase Profile => profile;

        public bool IsRequired => isRequired;

        #endregion
    }
}
