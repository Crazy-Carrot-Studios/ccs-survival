using System.Collections.Generic;
using CCS.Modules.CharacterController;

// =============================================================================
// SCRIPT: CCS_AnimationFitStudioPoseTargetCatalog
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Allowed Pose Target and Pose Frame presets for Animation Fit Studio v0.6.15.
// PLACEMENT: Editor catalog used by window, clip resolver, and validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Only Final Aim FullDraw and Aimed Walk RH. No full animation inventory.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public enum CCS_AnimationFitStudioPoseTargetKind
    {
        FinalAimFullDraw,
        AimedWalkRh,
    }

    public enum CCS_AnimationFitStudioPoseFrameKind
    {
        BestAimFrame,
        Start,
        Middle,
        End,
        Custom,
    }

    public sealed class CCS_AnimationFitStudioPoseTargetDefinition
    {
        public CCS_AnimationFitStudioPoseTargetKind Kind { get; }

        public string DisplayLabel { get; }

        public string SourceClipFileName { get; }

        public string SourceClipAssetPath { get; }

        public string FitTestClipFileName { get; }

        public CCS_AnimationFitStudioPoseFrameKind DefaultPoseFrame { get; }

        public CCS_AnimationFitStudioPoseTargetDefinition(
            CCS_AnimationFitStudioPoseTargetKind kind,
            string displayLabel,
            string sourceClipFileName,
            string sourceClipAssetPath,
            string fitTestClipFileName,
            CCS_AnimationFitStudioPoseFrameKind defaultPoseFrame)
        {
            Kind = kind;
            DisplayLabel = displayLabel;
            SourceClipFileName = sourceClipFileName;
            SourceClipAssetPath = sourceClipAssetPath;
            FitTestClipFileName = fitTestClipFileName;
            DefaultPoseFrame = defaultPoseFrame;
        }
    }

    public static class CCS_AnimationFitStudioPoseTargetCatalog
    {
        public const float DefaultBestAimFrameNormalizedTime = 0.65f;

        public const float MinBestAimFrameNormalizedTime = 0.10f;

        public const float MaxBestAimFrameNormalizedTime = 0.95f;

        public const string FullDrawIdleLikeWarning =
            "FullDraw did not produce a usable aim pose on this preview rig. Try Audition Aim Clips or Seed Pose.";

        public const string ClipIdleLikeWarning =
            CCS_AnimationFitStudioAimPoseScoreUtility.IdleLikeWarning;

        private static readonly IReadOnlyList<CCS_AnimationFitStudioPoseTargetDefinition> Definitions =
            new List<CCS_AnimationFitStudioPoseTargetDefinition>
            {
                new CCS_AnimationFitStudioPoseTargetDefinition(
                    CCS_AnimationFitStudioPoseTargetKind.FinalAimFullDraw,
                    "Final Aim — FullDraw",
                    "CCS_WW_Revolver_AimIdle_FullDraw",
                    CCS_CharacterControllerConstants.AnimationFitStudioDefaultSourceClipPath,
                    "CCS_WW_Revolver_AimIdle_FullDraw_FitTest.anim",
                    CCS_AnimationFitStudioPoseFrameKind.BestAimFrame),
                new CCS_AnimationFitStudioPoseTargetDefinition(
                    CCS_AnimationFitStudioPoseTargetKind.AimedWalkRh,
                    "Aimed Walk — RH",
                    "CCS_WW_Revolver_WalkAimed_RH",
                    CCS_CharacterControllerConstants.WildWestRevolverWalkAimedRhClipPath,
                    "CCS_WW_Revolver_WalkAimed_RH_FitTest.anim",
                    CCS_AnimationFitStudioPoseFrameKind.BestAimFrame),
            };

        private static readonly string[] PoseTargetDisplayLabels =
        {
            "Final Aim — FullDraw",
            "Aimed Walk — RH",
        };

        private static readonly string[] PoseFrameDisplayLabels =
        {
            "Best Aim Frame",
            "Start",
            "Middle",
            "End",
            "Custom",
        };

        public static IReadOnlyList<CCS_AnimationFitStudioPoseTargetDefinition> AllDefinitions => Definitions;

        public static string[] PoseTargetLabels => PoseTargetDisplayLabels;

        public static string[] PoseFrameLabels => PoseFrameDisplayLabels;

        public static int AllowedPoseTargetCount => Definitions.Count;

        public static CCS_AnimationFitStudioPoseTargetKind DefaultPoseTargetKind =>
            CCS_AnimationFitStudioPoseTargetKind.FinalAimFullDraw;

        public static bool TryGetDefinition(
            CCS_AnimationFitStudioPoseTargetKind kind,
            out CCS_AnimationFitStudioPoseTargetDefinition definition)
        {
            for (int i = 0; i < Definitions.Count; i++)
            {
                if (Definitions[i].Kind == kind)
                {
                    definition = Definitions[i];
                    return true;
                }
            }

            definition = null;
            return false;
        }

        public static int GetPoseTargetIndex(CCS_AnimationFitStudioPoseTargetKind kind)
        {
            for (int i = 0; i < Definitions.Count; i++)
            {
                if (Definitions[i].Kind == kind)
                {
                    return i;
                }
            }

            return 0;
        }

        public static CCS_AnimationFitStudioPoseTargetKind GetPoseTargetKindFromIndex(int index)
        {
            int clamped = UnityEngine.Mathf.Clamp(index, 0, Definitions.Count - 1);
            return Definitions[clamped].Kind;
        }

        public static int GetPoseFrameIndex(CCS_AnimationFitStudioPoseFrameKind frame)
        {
            return (int)frame;
        }

        public static CCS_AnimationFitStudioPoseFrameKind GetPoseFrameKindFromIndex(int index)
        {
            return (CCS_AnimationFitStudioPoseFrameKind)UnityEngine.Mathf.Clamp(
                index,
                0,
                PoseFrameDisplayLabels.Length - 1);
        }

        public static string GetPoseTargetDisplayLabel(CCS_AnimationFitStudioPoseTargetKind kind)
        {
            return TryGetDefinition(kind, out CCS_AnimationFitStudioPoseTargetDefinition definition)
                ? definition.DisplayLabel
                : kind.ToString();
        }

        public static string GetPoseFrameDisplayLabel(CCS_AnimationFitStudioPoseFrameKind frame)
        {
            int index = GetPoseFrameIndex(frame);
            return index >= 0 && index < PoseFrameDisplayLabels.Length
                ? PoseFrameDisplayLabels[index]
                : frame.ToString();
        }

        public static float ResolvePoseTimeSeconds(
            UnityEngine.AnimationClip clip,
            CCS_AnimationFitStudioPoseFrameKind frameKind,
            float bestAimFrameNormalizedTime,
            float customPoseTimeSeconds)
        {
            if (clip == null)
            {
                return 0f;
            }

            switch (frameKind)
            {
                case CCS_AnimationFitStudioPoseFrameKind.BestAimFrame:
                    float normalized = UnityEngine.Mathf.Clamp(
                        bestAimFrameNormalizedTime,
                        MinBestAimFrameNormalizedTime,
                        MaxBestAimFrameNormalizedTime);
                    return clip.length * normalized;
                case CCS_AnimationFitStudioPoseFrameKind.Start:
                    return 0f;
                case CCS_AnimationFitStudioPoseFrameKind.Middle:
                    return clip.length * 0.5f;
                case CCS_AnimationFitStudioPoseFrameKind.End:
                    return clip.length;
                default:
                    return UnityEngine.Mathf.Clamp(customPoseTimeSeconds, 0f, clip.length);
            }
        }
    }
}
