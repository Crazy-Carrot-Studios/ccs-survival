using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AnimationFitStudioAimPoseScoreUtility
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Scores preview right-arm pose after clip sampling to detect idle vs aim.
// PLACEMENT: Editor utility used by pose utility, audition, and validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Prevents false Applied status when only minor bones change during idle sampling.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public enum CCS_AnimationFitStudioAimPoseResultKind
    {
        Failed,
        IdleLike,
        Aim,
    }

    public sealed class CCS_AnimationFitStudioAimPoseScore
    {
        public float Score { get; set; }

        public float RightHandForwardDistance { get; set; }

        public float RightHandHeightDelta { get; set; }

        public float RightArmExtension { get; set; }

        public int ChangedAimBones { get; set; }

        public CCS_AnimationFitStudioAimPoseResultKind ResultKind { get; set; }

        public string Notes { get; set; } = string.Empty;

        public static CCS_AnimationFitStudioAimPoseScore CreateFailed(string notes)
        {
            return new CCS_AnimationFitStudioAimPoseScore
            {
                Score = 0f,
                ResultKind = CCS_AnimationFitStudioAimPoseResultKind.Failed,
                Notes = notes,
            };
        }
    }

    public static class CCS_AnimationFitStudioAimPoseScoreUtility
    {
        public const float AimThreshold = 50f;

        public const float IdleLikeThreshold = 15f;

        public const string IdleLikeWarning =
            "Clip sampled, but right hand/arm did not reach an aim pose. Try another Pose Target or use Seed Pose.";

        public const string SaveBlockedInvalidAimWarning =
            "Cannot save FitTest pose because the current preview is not a valid aim pose.";

        private const float RotationChangeThresholdDegrees = 0.1f;

        private static readonly HumanBodyBones[] AimBones =
        {
            HumanBodyBones.RightShoulder,
            HumanBodyBones.RightUpperArm,
            HumanBodyBones.RightLowerArm,
            HumanBodyBones.RightHand,
            HumanBodyBones.Chest,
        };

        public static CCS_AnimationFitStudioAimPoseScore Evaluate(
            Animator animator,
            AimBoneSnapshot beforeSnapshot = null)
        {
            if (animator == null || !animator.isHuman)
            {
                return CCS_AnimationFitStudioAimPoseScore.CreateFailed("Preview animator is missing or not humanoid.");
            }

            Transform root = animator.transform;
            Transform chest = animator.GetBoneTransform(HumanBodyBones.Chest)
                ?? animator.GetBoneTransform(HumanBodyBones.UpperChest)
                ?? animator.GetBoneTransform(HumanBodyBones.Spine);
            Transform shoulder = animator.GetBoneTransform(HumanBodyBones.RightShoulder)
                ?? animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            Transform upperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            Transform lowerArm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            Transform hand = animator.GetBoneTransform(HumanBodyBones.RightHand);

            if (chest == null || shoulder == null || upperArm == null || lowerArm == null || hand == null)
            {
                return CCS_AnimationFitStudioAimPoseScore.CreateFailed("Right arm bones are missing on preview rig.");
            }

            Vector3 bodyForward = chest.forward;
            bodyForward.y = 0f;
            if (bodyForward.sqrMagnitude < 0.0001f)
            {
                bodyForward = root.forward;
                bodyForward.y = 0f;
            }

            bodyForward.Normalize();
            Vector3 chestPosition = chest.position;
            Vector3 shoulderPosition = shoulder.position;
            Vector3 handPosition = hand.position;

            float forwardDistance = Vector3.Dot(handPosition - chestPosition, bodyForward);
            float heightDelta = handPosition.y - shoulderPosition.y;
            float armExtension = Vector3.Distance(shoulderPosition, handPosition);

            Vector3 upperArmVector = (lowerArm.position - upperArm.position).normalized;
            Vector3 worldDown = Vector3.down;
            float forearmRaiseAngle = Vector3.Angle(upperArmVector, worldDown);

            float forwardScore = Mathf.Clamp01(forwardDistance / 0.35f) * 40f;
            float heightScore = Mathf.Clamp01((heightDelta + 0.05f) / 0.25f) * 25f;
            float extensionScore = Mathf.Clamp01((armExtension - 0.25f) / 0.35f) * 20f;
            float forearmScore = forearmRaiseAngle >= 35f && forearmRaiseAngle <= 130f ? 15f : 0f;

            float score = forwardScore + heightScore + extensionScore + forearmScore;
            score = Mathf.Clamp(score, 0f, 100f);

            AimBoneSnapshot afterSnapshot = CaptureAimBoneSnapshot(animator);
            int changedAimBones = beforeSnapshot != null
                ? CountChangedAimBones(beforeSnapshot, afterSnapshot)
                : afterSnapshot.Bones.Count;

            CCS_AnimationFitStudioAimPoseResultKind resultKind = ResolveResultKind(score);
            string notes = BuildNotes(forwardDistance, heightDelta, armExtension, forearmRaiseAngle, resultKind);

            return new CCS_AnimationFitStudioAimPoseScore
            {
                Score = score,
                RightHandForwardDistance = forwardDistance,
                RightHandHeightDelta = heightDelta,
                RightArmExtension = armExtension,
                ChangedAimBones = changedAimBones,
                ResultKind = resultKind,
                Notes = notes,
            };
        }

        public static CCS_AnimationFitStudioAimPoseResultKind ResolveResultKind(float score)
        {
            if (score >= AimThreshold)
            {
                return CCS_AnimationFitStudioAimPoseResultKind.Aim;
            }

            if (score >= IdleLikeThreshold)
            {
                return CCS_AnimationFitStudioAimPoseResultKind.IdleLike;
            }

            return CCS_AnimationFitStudioAimPoseResultKind.Failed;
        }

        public static AimBoneSnapshot CaptureAimBoneSnapshot(Animator animator)
        {
            AimBoneSnapshot snapshot = new AimBoneSnapshot();
            if (animator == null)
            {
                return snapshot;
            }

            for (int i = 0; i < AimBones.Length; i++)
            {
                Transform boneTransform = animator.GetBoneTransform(AimBones[i]);
                if (boneTransform == null)
                {
                    continue;
                }

                snapshot.Bones.Add(new AimBoneSample
                {
                    Bone = AimBones[i],
                    LocalRotation = boneTransform.localRotation,
                });
            }

            return snapshot;
        }

        public static int CountChangedAimBones(AimBoneSnapshot before, AimBoneSnapshot after)
        {
            int changedCount = 0;
            for (int i = 0; i < after.Bones.Count; i++)
            {
                AimBoneSample afterBone = after.Bones[i];
                if (!before.TryGetBone(afterBone.Bone, out AimBoneSample beforeBone))
                {
                    changedCount++;
                    continue;
                }

                if (Quaternion.Angle(beforeBone.LocalRotation, afterBone.LocalRotation) > RotationChangeThresholdDegrees)
                {
                    changedCount++;
                }
            }

            return changedCount;
        }

        private static string BuildNotes(
            float forwardDistance,
            float heightDelta,
            float armExtension,
            float forearmRaiseAngle,
            CCS_AnimationFitStudioAimPoseResultKind resultKind)
        {
            return "Forward "
                + forwardDistance.ToString("0.###")
                + "m, height "
                + heightDelta.ToString("0.###")
                + "m, extension "
                + armExtension.ToString("0.###")
                + "m, forearm angle "
                + forearmRaiseAngle.ToString("0.#")
                + " -> "
                + resultKind;
        }
    }

    public sealed class AimBoneSnapshot
    {
        public System.Collections.Generic.List<AimBoneSample> Bones { get; } =
            new System.Collections.Generic.List<AimBoneSample>();

        public bool TryGetBone(HumanBodyBones bone, out AimBoneSample sample)
        {
            for (int i = 0; i < Bones.Count; i++)
            {
                if (Bones[i].Bone == bone)
                {
                    sample = Bones[i];
                    return true;
                }
            }

            sample = default;
            return false;
        }
    }

    public struct AimBoneSample
    {
        public HumanBodyBones Bone;

        public Quaternion LocalRotation;
    }
}
