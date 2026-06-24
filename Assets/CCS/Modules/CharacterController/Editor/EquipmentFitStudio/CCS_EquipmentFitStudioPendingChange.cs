using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioPendingChange
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Tracks pending socket/IK diffs before profile save.
// PLACEMENT: Editor-only data class.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Used by Equipment Fit Studio window capture/save flow.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    [Serializable]
    public sealed class CCS_EquipmentFitStudioPendingChange
    {
        public const float CompareTolerance = 0.001f;

        public string Label = string.Empty;

        public string ProfileAssetName = string.Empty;

        public Vector3 OldPosition;

        public Vector3 NewPosition;

        public Vector3 OldEulerAngles;

        public Vector3 NewEulerAngles;

        public Vector3 OldScale = Vector3.one;

        public Vector3 NewScale = Vector3.one;

        public bool HasCaptured;

        public bool HasTransformChanges =>
            !VectorsApproximatelyEqual(OldPosition, NewPosition)
            || !VectorsApproximatelyEqual(OldEulerAngles, NewEulerAngles)
            || !VectorsApproximatelyEqual(OldScale, NewScale);

        public bool HasChanges => HasCaptured;

        public void CaptureFromBaseline(
            string label,
            string profileAssetName,
            Vector3 oldPosition,
            Vector3 oldEulerAngles,
            Vector3 oldScale,
            Vector3 newPosition,
            Vector3 newEulerAngles,
            Vector3 newScale)
        {
            Label = label;
            ProfileAssetName = profileAssetName;
            OldPosition = oldPosition;
            OldEulerAngles = oldEulerAngles;
            OldScale = oldScale;
            NewPosition = newPosition;
            NewEulerAngles = newEulerAngles;
            NewScale = newScale;
            HasCaptured = true;
        }

        public void Capture(string label, Vector3 position, Vector3 eulerAngles, Vector3 scale)
        {
            CaptureFromBaseline(
                label,
                ProfileAssetName,
                OldPosition,
                OldEulerAngles,
                OldScale,
                position,
                eulerAngles,
                scale);
        }

        public void SetBaseline(Vector3 position, Vector3 eulerAngles, Vector3 scale)
        {
            OldPosition = position;
            OldEulerAngles = eulerAngles;
            OldScale = scale;
            NewPosition = position;
            NewEulerAngles = eulerAngles;
            NewScale = scale;
            HasCaptured = false;
        }

        public void ClearCapture()
        {
            HasCaptured = false;
        }

        public static bool VectorsApproximatelyEqual(Vector3 left, Vector3 right, float tolerance = CompareTolerance)
        {
            return Mathf.Abs(left.x - right.x) <= tolerance
                && Mathf.Abs(left.y - right.y) <= tolerance
                && Mathf.Abs(left.z - right.z) <= tolerance;
        }

        public static string FormatVector3(Vector3 value)
        {
            return "(" + value.x.ToString("0.00") + ", " + value.y.ToString("0.00") + ", " + value.z.ToString("0.00") + ")";
        }
    }
}
