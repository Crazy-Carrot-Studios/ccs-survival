// =============================================================================
// SCRIPT: CCS_SurvivalSceneBootstrapRules
// CATEGORY: Survival / Runtime / Foundation / Scene
// PURPOSE: Survival-owned scene bootstrap standards and rule constants for AAA-safe composition roots.
// PLACEMENT: Static rules reference. Not attached to GameObjects. No scene scanning.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Scene identity is not save identity. Profiles configure setup; runtime state stays separate.
// =============================================================================

namespace CCS.Survival
{
    public static class CCS_SurvivalSceneBootstrapRules
    {
        public const string SingleRuntimeHostRule =
            "Each survival scene should contain exactly one CCS_RuntimeHost on the composition root.";

        public const string SingleSurvivalBootstrapRule =
            "Each survival scene should contain exactly one CCS_SurvivalBootstrap on the same composition root as the runtime host.";

        public const string SurvivalDiagnosticsOwnershipRule =
            "Survival scenes own survival diagnostics. Keep Core runtime diagnostics disabled unless explicitly testing Core.";

        public const string CompositionRootRule =
            "CCS_SurvivalBootstrap is the survival composition root: host initialization, survival context, installer pipeline, and diagnostics.";

        public const string SceneIdentityRule =
            "Scene names, hierarchy paths, and GameObject names must not be used as save or authority identity.";

        public const string ProfileSetupRule =
            "ScriptableObject profiles configure setup only. Runtime simulation state and save data remain separate from profile assets.";

        public const string SkeletonRegistrationRule =
            "During skeleton phase, do not register survival services or updatables unless a milestone explicitly allows it.";

        public const string BootstrapInstallerRule =
            "Skeleton bootstrap should register exactly one survival bootstrap installer per startup pipeline run.";
    }
}
