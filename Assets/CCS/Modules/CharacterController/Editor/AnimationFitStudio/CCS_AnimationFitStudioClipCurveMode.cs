// =============================================================================
// SCRIPT: CCS_AnimationFitStudioClipCurveMode
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Identifies whether a clip is saved via Humanoid muscles or Transform curves.
// PLACEMENT: Used by Animation Fit Studio save workflow and validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: FullDraw controller clip uses Humanoid muscle curves (Animator classID 95).
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public enum CCS_AnimationFitStudioClipCurveMode
    {
        TransformCurves = 0,
        HumanoidMuscleCurves = 1,
    }
}
