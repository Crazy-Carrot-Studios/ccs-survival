using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalProfileBase
// CATEGORY: Survival / Runtime / Foundation / Profiles
// PURPOSE: Abstract ScriptableObject base for future survival setup profiles (configuration only).
// PLACEMENT: Create asset under Assets/CCS/Survival/... No runtime mechanics at foundation layer.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Configuration only. FUTURE: derived gameplay profiles inherit from this base; see Framework_Architecture_Guide.md.
// =============================================================================

namespace CCS.Survival
{
    public abstract class CCS_SurvivalProfileBase : ScriptableObject
    {
        #region Variables

        [Header("Profile Identity")]
        [Tooltip("Human-readable profile label shown in editors and tools.")]
        [SerializeField] private string profileDisplayName = string.Empty;

        [Tooltip("Stable reverse-DNS profile ID for save and runtime identity (not an asset path).")]
        [SerializeField] private string profileId = string.Empty;

        [Tooltip("Short description of what this profile configures.")]
        [SerializeField] private string profileDescription = string.Empty;

        [Header("Profile Version")]
        [Tooltip("Semantic version string for profile schema migrations.")]
        [SerializeField] private string profileVersion = CCS_SurvivalRuntimeConstants.DefaultProfileVersion;

        [Header("Diagnostics")]
        [Tooltip("When enabled, profile-driven systems may emit additional debug logs.")]
        [SerializeField] private bool enableDebugLogs;

        #endregion

        #region Properties

        public string ProfileDisplayName => profileDisplayName;

        public string ProfileId => profileId;

        public string ProfileDescription => profileDescription;

        public string ProfileVersion => profileVersion;

        public bool EnableDebugLogs => enableDebugLogs;

        #endregion

        #region Public Methods

        public bool ValidateProfile(out string message)
        {
            CCS_SurvivalValidationResult validation = CCS_SurvivalProfileValidationUtility.ValidateProfile(this);
            message = validation.Message;
            return validation.IsSuccess;
        }

        #endregion
    }
}
