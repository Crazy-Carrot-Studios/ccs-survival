using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerAnimationPoseDeltaDiagnostic
// CATEGORY: Modules / CharacterController / Runtime / Visuals
// PURPOSE: Detects whether active Animator clips physically move humanoid bones over time.
// PLACEMENT: Player VisualRoot. Enabled only for manual diagnostics.
// AUTHOR: James Schilz
// CREATED: 2026-06-28
// NOTES: v0.8.1b — writes Logs/player-animation-pose-delta-v0.8.1.md, no UI overlay.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public sealed class CCS_PlayerAnimationPoseDeltaDiagnostic : MonoBehaviour
    {
        #region Variables

        private const string ReportRelativePath = "Logs/player-animation-pose-delta-v0.8.1.md";
        private const float SampleIntervalSeconds = 0.5f;
        private const float MinimumPassDelta = 0.001f;

        private static readonly HumanBodyBones[] SampleBones =
        {
            HumanBodyBones.Hips,
            HumanBodyBones.Spine,
            HumanBodyBones.Chest,
            HumanBodyBones.UpperChest,
            HumanBodyBones.LeftUpperLeg,
            HumanBodyBones.RightUpperLeg,
            HumanBodyBones.LeftUpperArm,
            HumanBodyBones.RightUpperArm,
            HumanBodyBones.RightHand,
        };

        [SerializeField] private bool enablePoseDeltaReport;
        [SerializeField] private bool logSampleResultsToConsole;

        private readonly List<PoseDeltaSampleResult> sampleResults = new List<PoseDeltaSampleResult>();
        private Coroutine samplingCoroutine;
        private bool wroteReport;

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            if (!enablePoseDeltaReport || samplingCoroutine != null)
            {
                return;
            }

            samplingCoroutine = StartCoroutine(SamplePoseDeltasLoop());
        }

        private void OnDisable()
        {
            if (samplingCoroutine != null)
            {
                StopCoroutine(samplingCoroutine);
                samplingCoroutine = null;
            }
        }

        #endregion

        #region Private Methods

        private IEnumerator SamplePoseDeltasLoop()
        {
            yield return new WaitForSeconds(1f);

            while (enablePoseDeltaReport && !wroteReport)
            {
                if (CCS_PlayerAnimatorResolver.TryResolveAuthoritativeAnimator(
                        transform,
                        out Animator animator,
                        out _))
                {
                    yield return StartCoroutine(SampleCurrentPoseDelta(animator));
                }

                yield return new WaitForSeconds(SampleIntervalSeconds);

                if (sampleResults.Count >= 8)
                {
                    WriteReport();
                    wroteReport = true;
                }
            }
        }

        private IEnumerator SampleCurrentPoseDelta(Animator animator)
        {
            PoseDeltaSampleResult result = new PoseDeltaSampleResult();
            Dictionary<HumanBodyBones, Quaternion> firstRotations = CaptureBoneRotations(animator);
            Dictionary<HumanBodyBones, Vector3> firstPositions = CaptureBonePositions(animator);

            AnimatorStateInfo baseState = animator.GetCurrentAnimatorStateInfo(0);
            result.StateLabel = "BaseLayer:" + baseState.shortNameHash;
            result.NormalizedTime = baseState.normalizedTime;
            AnimatorClipInfo[] clipInfos = animator.GetCurrentAnimatorClipInfo(0);
            result.ClipName = clipInfos.Length > 0 && clipInfos[0].clip != null
                ? clipInfos[0].clip.name
                : "None";

            yield return new WaitForSeconds(SampleIntervalSeconds);

            float totalDelta = 0f;
            for (int boneIndex = 0; boneIndex < SampleBones.Length; boneIndex++)
            {
                HumanBodyBones bone = SampleBones[boneIndex];
                Transform boneTransform = animator.GetBoneTransform(bone);
                if (boneTransform == null)
                {
                    continue;
                }

                if (firstRotations.TryGetValue(bone, out Quaternion firstRotation))
                {
                    totalDelta += Quaternion.Angle(firstRotation, boneTransform.localRotation);
                }

                if (firstPositions.TryGetValue(bone, out Vector3 firstPosition))
                {
                    totalDelta += Vector3.Distance(firstPosition, boneTransform.localPosition);
                }
            }

            result.BoneDeltaMagnitude = totalDelta;
            result.Passed = totalDelta >= MinimumPassDelta;
            sampleResults.Add(result);

            if (logSampleResultsToConsole)
            {
                Debug.Log(
                    "[Player Pose Delta] "
                    + result.StateLabel
                    + " clip="
                    + result.ClipName
                    + " delta="
                    + result.BoneDeltaMagnitude.ToString("0.000000")
                    + " pass="
                    + result.Passed,
                    this);
            }

            yield break;
        }

        private static Dictionary<HumanBodyBones, Quaternion> CaptureBoneRotations(Animator animator)
        {
            Dictionary<HumanBodyBones, Quaternion> rotations = new Dictionary<HumanBodyBones, Quaternion>();
            for (int boneIndex = 0; boneIndex < SampleBones.Length; boneIndex++)
            {
                HumanBodyBones bone = SampleBones[boneIndex];
                Transform boneTransform = animator.GetBoneTransform(bone);
                if (boneTransform != null)
                {
                    rotations[bone] = boneTransform.localRotation;
                }
            }

            return rotations;
        }

        private static Dictionary<HumanBodyBones, Vector3> CaptureBonePositions(Animator animator)
        {
            Dictionary<HumanBodyBones, Vector3> positions = new Dictionary<HumanBodyBones, Vector3>();
            for (int boneIndex = 0; boneIndex < SampleBones.Length; boneIndex++)
            {
                HumanBodyBones bone = SampleBones[boneIndex];
                Transform boneTransform = animator.GetBoneTransform(bone);
                if (boneTransform != null)
                {
                    positions[bone] = boneTransform.localPosition;
                }
            }

            return positions;
        }

        private void WriteReport()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string reportPath = Path.Combine(projectRoot, ReportRelativePath);
            string directory = Path.GetDirectoryName(reportPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            StringBuilder builder = new StringBuilder(4096);
            builder.AppendLine("# Player Animation Pose Delta Report");
            builder.AppendLine();
            builder.AppendLine("| State | Clip | Normalized Time | Bone Delta | Pass |");
            builder.AppendLine("|---|---|---:|---:|---|");
            for (int resultIndex = 0; resultIndex < sampleResults.Count; resultIndex++)
            {
                PoseDeltaSampleResult result = sampleResults[resultIndex];
                builder.Append("| ");
                builder.Append(result.StateLabel);
                builder.Append(" | ");
                builder.Append(result.ClipName);
                builder.Append(" | ");
                builder.Append(result.NormalizedTime.ToString("0.000"));
                builder.Append(" | ");
                builder.Append(result.BoneDeltaMagnitude.ToString("0.000000"));
                builder.Append(" | ");
                builder.Append(result.Passed ? "pass" : "fail");
                builder.AppendLine(" |");
            }

            File.WriteAllText(reportPath, builder.ToString());
            Debug.Log("[Player Pose Delta] Wrote report: " + reportPath, this);
        }

        private sealed class PoseDeltaSampleResult
        {
            public string StateLabel = string.Empty;
            public string ClipName = string.Empty;
            public float NormalizedTime;
            public float BoneDeltaMagnitude;
            public bool Passed;
        }

        #endregion
    }
}
