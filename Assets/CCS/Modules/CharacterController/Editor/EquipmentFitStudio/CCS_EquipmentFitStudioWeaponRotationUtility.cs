using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioWeaponRotationUtility
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Weapon-space pitch/yaw/roll quaternion helpers for Fit Studio preview tuning.
// PLACEMENT: Editor utility used by Equipment Fit Studio transform panel.
// AUTHOR: James Schilz
// CREATED: 2026-06-24
// NOTES: Roll rotates around configurable weapon-forward axis, not world Y.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public static class CCS_EquipmentFitStudioWeaponRotationUtility
    {
        public const float SmallNudgeDegrees = 1f;

        public const float MediumNudgeDegrees = 5f;

        public const float LargeNudgeDegrees = 15f;

        public const float AxisTestDegrees = 15f;

        public static readonly string[] WeaponForwardAxisLabels =
        {
            "Local +Z",
            "Local -Z",
            "Local +X",
            "Local -X",
        };

        public static Quaternion ApplyLocalAxisDelta(
            Quaternion current,
            Vector3 localAxis,
            float degrees)
        {
            if (localAxis.sqrMagnitude <= 0.000001f)
            {
                return current;
            }

            return current * Quaternion.AngleAxis(degrees, localAxis.normalized);
        }

        public static Vector3 NormalizeEuler(Vector3 euler)
        {
            return new Vector3(
                NormalizeAngle(euler.x),
                NormalizeAngle(euler.y),
                NormalizeAngle(euler.z));
        }

        public static Vector3 GetWeaponForwardLocalAxis(CCS_EquipmentFitStudioWeaponForwardAxis axis)
        {
            switch (axis)
            {
                case CCS_EquipmentFitStudioWeaponForwardAxis.LocalPositiveX:
                    return Vector3.right;
                case CCS_EquipmentFitStudioWeaponForwardAxis.LocalNegativeX:
                    return Vector3.left;
                case CCS_EquipmentFitStudioWeaponForwardAxis.LocalNegativeZ:
                    return Vector3.back;
                default:
                    return Vector3.forward;
            }
        }

        public static string GetWeaponForwardAxisLabel(CCS_EquipmentFitStudioWeaponForwardAxis axis)
        {
            int index = (int)axis;
            return index >= 0 && index < WeaponForwardAxisLabels.Length
                ? WeaponForwardAxisLabels[index]
                : axis.ToString();
        }

        public static Vector3 GetWorldWeaponForwardDirection(
            Quaternion attachmentLocalRotation,
            Transform attachmentRoot,
            CCS_EquipmentFitStudioWeaponForwardAxis axis)
        {
            if (attachmentRoot == null)
            {
                return Vector3.forward;
            }

            Vector3 localForward = GetWeaponForwardLocalAxis(axis);
            return attachmentRoot.TransformDirection(attachmentLocalRotation * localForward).normalized;
        }

        public static Quaternion ApplyPitchDelta(Quaternion current, float degrees)
        {
            return ApplyLocalAxisDelta(current, Vector3.right, degrees);
        }

        public static Quaternion ApplyYawDelta(Quaternion current, float degrees)
        {
            return ApplyLocalAxisDelta(current, Vector3.up, degrees);
        }

        public static Quaternion ApplyRollDelta(
            Quaternion current,
            float degrees,
            CCS_EquipmentFitStudioWeaponForwardAxis forwardAxis)
        {
            return ApplyLocalAxisDelta(current, GetWeaponForwardLocalAxis(forwardAxis), degrees);
        }

        public static bool QuaternionsApproximatelyEqual(Quaternion left, Quaternion right, float maxAngleDegrees = 0.25f)
        {
            float angle = Quaternion.Angle(left, right);
            return angle <= maxAngleDegrees;
        }

        public static bool RollAndYawProduceDistinctRotations(CCS_EquipmentFitStudioWeaponForwardAxis forwardAxis)
        {
            Quaternion identity = Quaternion.identity;
            Quaternion yaw = ApplyYawDelta(identity, AxisTestDegrees);
            Quaternion roll = ApplyRollDelta(identity, AxisTestDegrees, forwardAxis);
            return !QuaternionsApproximatelyEqual(yaw, roll, 0.5f);
        }

        public static bool PitchYawRollProduceDistinctRotations(CCS_EquipmentFitStudioWeaponForwardAxis forwardAxis)
        {
            Quaternion identity = Quaternion.identity;
            Quaternion pitch = ApplyPitchDelta(identity, AxisTestDegrees);
            Quaternion yaw = ApplyYawDelta(identity, AxisTestDegrees);
            Quaternion roll = ApplyRollDelta(identity, AxisTestDegrees, forwardAxis);
            return !QuaternionsApproximatelyEqual(pitch, yaw, 0.5f)
                && !QuaternionsApproximatelyEqual(pitch, roll, 0.5f)
                && !QuaternionsApproximatelyEqual(yaw, roll, 0.5f);
        }

        public static float ResolveNudgeStep(int stepIndex)
        {
            switch (stepIndex)
            {
                case 0:
                    return SmallNudgeDegrees;
                case 2:
                    return LargeNudgeDegrees;
                default:
                    return MediumNudgeDegrees;
            }
        }

        private static float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle > 180f)
            {
                angle -= 360f;
            }
            else if (angle < -180f)
            {
                angle += 360f;
            }

            return angle;
        }
    }
}
