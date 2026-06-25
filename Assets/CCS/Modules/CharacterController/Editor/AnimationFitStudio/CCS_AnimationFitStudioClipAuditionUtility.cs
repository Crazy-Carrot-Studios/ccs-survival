using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AnimationFitStudioClipAuditionUtility
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Auditions allowed aim clips at preset times and selects best aim frame.
// PLACEMENT: Editor utility used by Animation Fit Studio window.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Tests only FullDraw and WalkAimed RH. No full animation inventory.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public sealed class CCS_AnimationFitStudioClipAuditionRow
    {
        public string ClipName { get; set; } = string.Empty;

        public CCS_AnimationFitStudioPoseTargetKind TargetKind { get; set; }

        public float NormalizedTime { get; set; }

        public float TimeSeconds { get; set; }

        public float AimPoseScore { get; set; }

        public CCS_AnimationFitStudioAimPoseResultKind Result { get; set; }

        public int ChangedBones { get; set; }

        public string Notes { get; set; } = string.Empty;

        public string SampleMethod { get; set; } = string.Empty;

        public string TimeLabel =>
            NormalizedTime <= 0.001f
                ? "Start"
                : NormalizedTime >= 0.999f
                    ? "End"
                    : (NormalizedTime * 100f).ToString("0") + "%";
    }

    public static class CCS_AnimationFitStudioClipAuditionUtility
    {
        public static readonly float[] AuditionNormalizedTimes =
        {
            0f,
            0.25f,
            0.5f,
            0.65f,
            0.75f,
            0.85f,
            1f,
        };

        public static List<CCS_AnimationFitStudioClipAuditionRow> RunAudition(
            Animator animator,
            CCS_AnimationFitStudioPreviewState previewState)
        {
            List<CCS_AnimationFitStudioClipAuditionRow> rows = new List<CCS_AnimationFitStudioClipAuditionRow>();
            if (animator == null || previewState == null)
            {
                return rows;
            }

            AimBoneSnapshot idleSnapshot = CCS_AnimationFitStudioAimPoseScoreUtility.CaptureAimBoneSnapshot(animator);

            IReadOnlyList<CCS_AnimationFitStudioPoseTargetDefinition> targets =
                CCS_AnimationFitStudioPoseTargetCatalog.AllDefinitions;
            for (int t = 0; t < targets.Count; t++)
            {
                CCS_AnimationFitStudioPoseTargetDefinition definition = targets[t];
                if (!CCS_AnimationFitStudioClipResolver.TryResolveClipForPoseTarget(
                        definition.Kind,
                        out AnimationClip clip,
                        out _,
                        out _))
                {
                    continue;
                }

                for (int i = 0; i < AuditionNormalizedTimes.Length; i++)
                {
                    float normalizedTime = AuditionNormalizedTimes[i];
                    float timeSeconds = clip.length * normalizedTime;
                    rows.Add(AuditionSingleSample(
                        animator,
                        previewState,
                        definition.Kind,
                        clip,
                        normalizedTime,
                        timeSeconds,
                        idleSnapshot));
                }
            }

            return rows;
        }

        public static bool TryGetBestRowForTarget(
            IReadOnlyList<CCS_AnimationFitStudioClipAuditionRow> rows,
            CCS_AnimationFitStudioPoseTargetKind targetKind,
            out CCS_AnimationFitStudioClipAuditionRow bestRow)
        {
            bestRow = null;
            if (rows == null || rows.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < rows.Count; i++)
            {
                CCS_AnimationFitStudioClipAuditionRow row = rows[i];
                if (row.TargetKind != targetKind)
                {
                    continue;
                }

                if (bestRow == null || row.AimPoseScore > bestRow.AimPoseScore)
                {
                    bestRow = row;
                }
            }

            return bestRow != null;
        }

        public static bool TargetHasUsableAimPose(
            IReadOnlyList<CCS_AnimationFitStudioClipAuditionRow> rows,
            CCS_AnimationFitStudioPoseTargetKind targetKind)
        {
            if (!TryGetBestRowForTarget(rows, targetKind, out CCS_AnimationFitStudioClipAuditionRow bestRow))
            {
                return false;
            }

            return bestRow.Result == CCS_AnimationFitStudioAimPoseResultKind.Aim;
        }

        public static bool TryGetBestOverallRow(
            IReadOnlyList<CCS_AnimationFitStudioClipAuditionRow> rows,
            out CCS_AnimationFitStudioClipAuditionRow bestRow)
        {
            bestRow = null;
            if (rows == null || rows.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < rows.Count; i++)
            {
                CCS_AnimationFitStudioClipAuditionRow row = rows[i];
                if (bestRow == null || row.AimPoseScore > bestRow.AimPoseScore)
                {
                    bestRow = row;
                }
            }

            return bestRow != null;
        }

        private static CCS_AnimationFitStudioClipAuditionRow AuditionSingleSample(
            Animator animator,
            CCS_AnimationFitStudioPreviewState previewState,
            CCS_AnimationFitStudioPoseTargetKind targetKind,
            AnimationClip clip,
            float normalizedTime,
            float timeSeconds,
            AimBoneSnapshot idleSnapshot)
        {
            AimBoneSnapshot beforeSnapshot = CCS_AnimationFitStudioAimPoseScoreUtility.CaptureAimBoneSnapshot(animator);
            bool sampled = CCS_AnimationFitStudioPlayablePreviewSampler.TrySampleClip(
                animator,
                previewState,
                clip,
                timeSeconds,
                out string methodUsed);

            CCS_AnimationFitStudioAimPoseScore score = sampled
                ? CCS_AnimationFitStudioAimPoseScoreUtility.Evaluate(animator, beforeSnapshot)
                : CCS_AnimationFitStudioAimPoseScore.CreateFailed("Sampling failed.");

            int changedFromIdle = CCS_AnimationFitStudioAimPoseScoreUtility.CountChangedAimBones(
                idleSnapshot,
                CCS_AnimationFitStudioAimPoseScoreUtility.CaptureAimBoneSnapshot(animator));

            return new CCS_AnimationFitStudioClipAuditionRow
            {
                ClipName = clip.name,
                TargetKind = targetKind,
                NormalizedTime = normalizedTime,
                TimeSeconds = timeSeconds,
                AimPoseScore = score.Score,
                Result = score.ResultKind,
                ChangedBones = changedFromIdle,
                Notes = sampled ? score.Notes : "Sampling failed.",
                SampleMethod = methodUsed,
            };
        }
    }
}
