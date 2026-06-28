using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterFirstPersonCameraDefaultsUtility
// CATEGORY: Modules / CharacterController / Editor
// PURPOSE: Applies v0.6.9 first-person body-aware camera defaults from editor menu.
// PLACEMENT: Menu CCS/Character Controller/Camera/Apply First Person Body Aware Defaults.
// AUTHOR: James Schilz
// CREATED: 2026-06-23
// NOTES: Repairs profiles, profile set default, player anchors, and camera rig FP cameras.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterFirstPersonCameraDefaultsUtility
    {
        public static void ApplyFirstPersonBodyAwareDefaults()
        {
            bool changed = CCS_CharacterCameraAssetBuilder.ApplyFirstPersonBodyAwareDefaults();
            Debug.Log(
                changed
                    ? "[First Person Camera] Applied first-person body-aware defaults."
                    : "[First Person Camera] First-person body-aware defaults already up to date.");
        }
    }
}
