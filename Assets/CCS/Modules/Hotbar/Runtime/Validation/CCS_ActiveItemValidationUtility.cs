using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_ActiveItemValidationUtility
// CATEGORY: Modules / Hotbar / Runtime / Validation
// PURPOSE: Runtime validation helpers for active item module foundation.
// PLACEMENT: Used by CCS_HotbarValidationValidator.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Editor asset checks remain in the editor validator.
// =============================================================================

namespace CCS.Modules.Hotbar
{
    public static class CCS_ActiveItemValidationUtility
    {
        private const string ModuleRoot = "Assets/CCS/Modules/Hotbar";
        private const string ActiveItemRoot = ModuleRoot + "/Runtime/ActiveItem";
        private const string DefaultProfilePath = "Assets/CCS/Survival/Profiles/Hotbar/CCS_DefaultActiveItemProfile.asset";

        public static CCS_SurvivalValidationResult ValidateModuleFolders()
        {
            if (!System.IO.Directory.Exists(ActiveItemRoot))
            {
                return CCS_SurvivalValidationResult.Fail($"Missing active item folder: {ActiveItemRoot}");
            }

            if (!System.IO.Directory.Exists(ModuleRoot + "/Runtime/Profiles"))
            {
                return CCS_SurvivalValidationResult.Fail("Missing Hotbar Runtime/Profiles folder.");
            }

            return CCS_SurvivalValidationResult.Pass("Active item module folders are present.");
        }

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_ActiveItemProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail($"Missing active item profile: {DefaultProfilePath}");
            }

            if (profile.UseCooldownSeconds < 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Active item use cooldown cannot be negative.");
            }

            return CCS_SurvivalValidationResult.Pass("Active item profile validated.");
        }

        public static string DefaultProfileAssetPath => DefaultProfilePath;
    }
}
