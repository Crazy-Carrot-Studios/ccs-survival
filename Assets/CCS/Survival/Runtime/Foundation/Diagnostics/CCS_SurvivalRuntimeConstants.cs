// =============================================================================
// SCRIPT: CCS_SurvivalRuntimeConstants
// CATEGORY: Survival / Runtime / Foundation / Diagnostics
// PURPOSE: Central survival-owned constants for module IDs, log categories, and diagnostic expectations.
// PLACEMENT: Static constants. Not attached to GameObjects. No runtime mechanics.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Feature-specific diagnostics may alias these values. Identity prefixes are authoritative; see Framework_Architecture_Guide.md.
// =============================================================================

namespace CCS.Survival
{
    public static class CCS_SurvivalRuntimeConstants
    {
        public const string ModuleIdPrefix = "ccs.survival.";

        public const string DevelopmentDiagnosticsLogCategory = "Survival Development Diagnostics";

        public const string DevelopmentModuleId = ModuleIdPrefix + "development";

        public const string CharacterModuleId = ModuleIdPrefix + "character";

        public const string CharacterControllerModuleId = ModuleIdPrefix + "movement";

        public const string CharacterControllerLogCategory = "Character Controller";

        public const string InteractionModuleId = ModuleIdPrefix + "interaction";

        public const string InteractionLogCategory = "Interaction";

        public const string InventoryModuleId = ModuleIdPrefix + "inventory";

        public const string InventoryLogCategory = "Inventory";

        public const string EquipmentModuleId = ModuleIdPrefix + "equipment";

        public const string EquipmentLogCategory = "Equipment";

        public const string SurvivalCoreModuleId = ModuleIdPrefix + "core";

        public const string SurvivalCoreLogCategory = "Survival Core";

        public const string SurvivalDiagnosticsLogCategory = "Survival Diagnostics";

        public const string SurvivalInstallerLogCategory = "Survival Installer";

        public const string SurvivalBootstrapLogCategory = "Survival Bootstrap";

        public const string SurvivalContextLogCategory = "Survival Context";

        public const string CharacterLogCategory = "Survival Character";

        public const string CharacterInstallerLogCategory = "Survival Character Installer";

        public const int ExpectedSkeletonModuleCount = 1;

        public const int SkeletonExpectedServicesCount = 0;

        public const int SkeletonExpectedUpdateSystemsCount = 0;

        public const string DefaultProfileVersion = "0.0.1";

        public const string ProfileIdPrefix = "ccs.survival.profile.";

        public const string ValidationLogCategory = "Survival Validation";

        public const string ValidationPassedDefaultMessage = "Validation passed.";

        public const string ValidationPassedNoDetailMessage = "Validation passed with no detail message.";

        public const string ValidationWarningDefaultMessage = "Validation completed with a warning.";

        public const string ValidationFailedDefaultMessage = "Validation failed.";

        public const string InvalidProfileIdMessage = "Survival profile ID is null or empty.";

        public const string SaveStableIdGuidanceMessage =
            "Survival profile IDs follow the same save-stable rules as runtime identity. See StableRuntimeIdentityGuidanceMessage.";

        public const string AuthorityIdPrefix = "ccs.survival.authority.";

        public const string AvatarIdPrefix = "ccs.survival.avatar.";

        public const string BindingIdPrefix = "ccs.survival.binding.";

        public const string AuthorityLogCategory = "Survival Authority";

        public const string AvatarLogCategory = "Survival Avatar";

        public const string IdentityValidationLogCategory = "Survival Identity Validation";

        public const string StableRuntimeIdentityGuidanceMessage =
            "Survival runtime identity must use save-stable lowercase reverse-DNS characters (a-z, 0-9, '.', '-'). Do not use Unity instance IDs, GameObject names, scene paths, or asset paths.";

        public const string ForbiddenUnityIdentitySourceMessage =
            "Survival runtime identity must not use Unity asset paths, scene paths, slashes, spaces, or other scene-object-derived values.";

        public const string SceneValidationLogCategory = "Survival Scene Validation";

        public const string BootstrapRulesLogCategory = "Survival Bootstrap Rules";

        public const int ExpectedRuntimeHostCount = 1;

        public const int ExpectedSurvivalBootstrapCount = 1;

        public const string BootstrapProfileSlotPrefix = "ccs.survival.bootstrap.slot.";

        public const string SceneIdentityGuidanceMessage =
            "Scene names, hierarchy paths, and GameObject names are not save identity. Use authority, profile, and module IDs instead.";
    }
}
