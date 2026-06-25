using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_AnimationFitStudioHumanoidControlState
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Tracks baseline, current, and saved Humanoid muscle values for Fit Studio.
// PLACEMENT: Owned by preview state; drives readout, nudge, preview, and save.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Values are normalized Humanoid muscle space (-1 to +1).
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public sealed class CCS_AnimationFitStudioHumanoidControlState
    {
        public const float ClampWarningThreshold = 0.95f;
        public const float MuscleMin = -1f;
        public const float MuscleMax = 1f;

        public readonly Dictionary<string, float> BaselineValues = new Dictionary<string, float>();

        public readonly Dictionary<string, float> CurrentValues = new Dictionary<string, float>();

        public readonly Dictionary<string, float> LastSavedValues = new Dictionary<string, float>();

        public readonly List<string> LastClampedMuscles = new List<string>();

        public string LastEditFeedback = string.Empty;

        public bool InvertShoulderPitch;

        public bool InvertShoulderYaw;

        public bool InvertUpperArmPitch;

        public bool InvertUpperArmYaw;

        public bool InvertWristPitch = true;

        public bool InvertWristYaw;

        public bool IsInitialized { get; set; }

        public void Clear()
        {
            BaselineValues.Clear();
            CurrentValues.Clear();
            LastSavedValues.Clear();
            LastClampedMuscles.Clear();
            LastEditFeedback = string.Empty;
            IsInitialized = false;
        }

        public void ResetCurrentToBaseline()
        {
            CurrentValues.Clear();
            foreach (KeyValuePair<string, float> entry in BaselineValues)
            {
                CurrentValues[entry.Key] = entry.Value;
            }

            LastClampedMuscles.Clear();
            LastEditFeedback = string.Empty;
        }

        public void MarkSaved()
        {
            LastSavedValues.Clear();
            foreach (KeyValuePair<string, float> entry in CurrentValues)
            {
                LastSavedValues[entry.Key] = entry.Value;
            }
        }

        public bool IsNearClamp(string muscleName)
        {
            if (!CurrentValues.TryGetValue(muscleName, out float value))
            {
                return false;
            }

            return System.Math.Abs(value) >= ClampWarningThreshold;
        }

        public float GetDeltaFromBaseline(string muscleName)
        {
            if (!CurrentValues.TryGetValue(muscleName, out float current)
                || !BaselineValues.TryGetValue(muscleName, out float baseline))
            {
                return 0f;
            }

            return current - baseline;
        }

        public float GetCurrentValue(string muscleName)
        {
            return CurrentValues.TryGetValue(muscleName, out float value) ? value : 0f;
        }

        public float GetLastSavedValue(string muscleName)
        {
            return LastSavedValues.TryGetValue(muscleName, out float value) ? value : float.NaN;
        }
    }
}
