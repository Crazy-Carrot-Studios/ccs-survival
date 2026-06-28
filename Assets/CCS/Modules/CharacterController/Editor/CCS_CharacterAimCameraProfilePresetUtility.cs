using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterAimCameraProfilePresetUtility
// CATEGORY: Modules / CharacterController / Editor
// PURPOSE: Editor-only aim camera profile presets for manual revolver alignment tuning.
// PLACEMENT: Menu CCS/Character Controller/Camera/Aim Camera Presets.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Applies values to CCS_CharacterCameraProfile_AimOverShoulder only.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterAimCameraProfilePresetUtility
    {
        private const string MenuRoot = "CCS/Character Controller/Camera/Aim Camera Presets/";

        public static void ApplyRightShoulderBalanced()
        {
            ApplyPreset("Right Shoulder Balanced", 0.65f, 0.12f, 1.50f, 52f);
        }

        public static void ApplyRightShoulderTight()
        {
            ApplyPreset("Right Shoulder Tight", 0.75f, 0.10f, 1.35f, 50f);
        }

        public static void ApplyRightShoulderWide()
        {
            ApplyPreset("Right Shoulder Wide", 0.60f, 0.16f, 1.65f, 55f);
        }

        private static void ApplyPreset(
            string presetLabel,
            float shoulderX,
            float shoulderY,
            float distance,
            float fieldOfView)
        {
            CCS_CharacterCameraProfile profile = AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfile>(
                CCS_CharacterControllerConstants.AimCameraProfilePath);
            if (profile == null)
            {
                Debug.LogError(
                    "[Aim Camera Presets] Missing profile at "
                    + CCS_CharacterControllerConstants.AimCameraProfilePath);
                return;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.Update();
            SerializedProperty shoulderOffset = serializedProfile.FindProperty("thirdPersonShoulderOffset");
            if (shoulderOffset != null)
            {
                shoulderOffset.vector3Value = new Vector3(shoulderX, shoulderY, 0f);
            }

            serializedProfile.FindProperty("thirdPersonCameraDistance").floatValue = distance;
            serializedProfile.FindProperty("fieldOfView").floatValue = fieldOfView;
            serializedProfile.FindProperty("thirdPersonCameraSide").floatValue = 1f;
            serializedProfile.FindProperty("profileVersion").stringValue =
                "0.6.8-aim-camera-preset-" + presetLabel.ToLowerInvariant().Replace(' ', '-');
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            Debug.Log(
                "[Aim Camera Presets] Applied "
                + presetLabel
                + " to "
                + profile.name
                + " (Shoulder X="
                + shoulderX
                + ", Y="
                + shoulderY
                + ", Distance="
                + distance
                + ", FOV="
                + fieldOfView
                + ").");
        }
    }
}
