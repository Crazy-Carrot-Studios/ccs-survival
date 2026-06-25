// =============================================================================
// SCRIPT: CCS_AnimationFitStudioRuntimePolicy
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Controller FullDraw clip overwrite policy for Animation Fit Studio.
// PLACEMENT: Editor utility used by Animation Fit Studio window and validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Save overwrites the controller-used FullDraw clip in place. Controller is not modified.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public static class CCS_AnimationFitStudioRuntimePolicy
    {
        public const string RuntimeAimIdleFullDrawLabel = "Runtime Aim Idle — FullDraw";

        public const string ControllerFullDrawClipFileName = "CCS_WW_Revolver_AimIdle_FullDraw.anim";

        public const string ControllerFullDrawClipAssetPath =
            CCS_CharacterControllerConstants.RevolverAimIdleFullDrawClipPath;

        public const string RuntimeFullDrawEditNotice =
            "You are editing the same FullDraw clip used by the runtime Animator Controller.\n"
            + "Saving overwrites the existing controller clip in place.\n"
            + "Animator Controller is not modified because it already references this clip.";

        public const string NonControllerBackedWarningPrefix =
            "Warning: selected clip is not the current controller runtime FullDraw clip.";

        public const string SaveDoesNotWireControllerNotice =
            "Save Runtime FullDraw does not modify the Animator Controller.";

        public const string RuntimeCandidateStatusLabel = "Runtime Candidate";

        public const string PreviewOnlyStatusLabel = "Preview Only";

        public const string OverwriteSaveModeLabel = "Overwrite Controller Clip";

        public const string ControllerFullDrawTargetStatusLabel =
            "Controller FullDraw clip: " + ControllerFullDrawClipFileName;

        public const string RuntimePreviewOnlyStatusLabel =
            ControllerFullDrawTargetStatusLabel
            + " Save overwrites the existing asset in place; controller wiring is unchanged.";
    }
}
