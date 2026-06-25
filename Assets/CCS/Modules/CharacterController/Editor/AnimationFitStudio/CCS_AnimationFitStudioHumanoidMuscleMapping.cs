using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AnimationFitStudioHumanoidMuscleMapping
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Maps Fit Studio body-part edits to Humanoid muscle curve names.
// PLACEMENT: Used by pose apply, save, and validation for FullDraw Humanoid clips.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Pitch/Yaw/Roll map to Down-Up, Front-Back, and Twist muscle axes.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public static class CCS_AnimationFitStudioHumanoidMuscleMapping
    {
        public const float DegreesToMuscleScale = 1f / 125f;

        public static readonly string[] TorsoSaveMuscleNames =
        {
            "Spine Front-Back",
            "Spine Left-Right",
            "Spine Twist Left-Right",
            "Chest Front-Back",
            "Chest Left-Right",
            "Chest Twist Left-Right",
            "UpperChest Front-Back",
            "UpperChest Left-Right",
            "UpperChest Twist Left-Right",
        };

        public static readonly string[] RightArmSaveMuscleNames =
        {
            "Right Shoulder Down-Up",
            "Right Shoulder Front-Back",
            "Right Arm Down-Up",
            "Right Arm Front-Back",
            "Right Arm Twist In-Out",
            "Right Forearm Twist In-Out",
            "Right Hand Down-Up",
            "Right Hand In-Out",
        };

        public static readonly string[] FullDrawSaveMuscleNames = BuildFullDrawSaveMuscleNames();

        public static readonly string[] RightHandFingerMuscleNames =
        {
            "RightHand.Thumb.1 Stretched",
            "RightHand.Thumb.2 Stretched",
            "RightHand.Thumb.3 Stretched",
            "RightHand.Thumb.Spread",
            "RightHand.Index.1 Stretched",
            "RightHand.Index.2 Stretched",
            "RightHand.Index.3 Stretched",
            "RightHand.Index.Spread",
            "RightHand.Middle.1 Stretched",
            "RightHand.Middle.2 Stretched",
            "RightHand.Middle.3 Stretched",
            "RightHand.Middle.Spread",
            "RightHand.Ring.1 Stretched",
            "RightHand.Ring.2 Stretched",
            "RightHand.Ring.3 Stretched",
            "RightHand.Ring.Spread",
            "RightHand.Little.1 Stretched",
            "RightHand.Little.2 Stretched",
            "RightHand.Little.3 Stretched",
            "RightHand.Little.Spread",
        };

        private static readonly Dictionary<string, int> MuscleNameToIndex = BuildMuscleNameIndex();

        public static bool TryGetMuscleIndex(string muscleName, out int muscleIndex)
        {
            if (string.IsNullOrEmpty(muscleName))
            {
                muscleIndex = -1;
                return false;
            }

            return MuscleNameToIndex.TryGetValue(muscleName, out muscleIndex);
        }

        public static bool IsMuscleAvailableOnAvatar(string muscleName)
        {
            return TryGetMuscleIndex(muscleName, out _);
        }

        public static IReadOnlyList<string> GetSaveMuscleNames(
            bool includeFingerMuscles,
            bool includeExistingClipBindings,
            AnimationClip clip)
        {
            HashSet<string> names = new HashSet<string>();
            for (int i = 0; i < FullDrawSaveMuscleNames.Length; i++)
            {
                if (IsMuscleAvailableOnAvatar(FullDrawSaveMuscleNames[i]))
                {
                    names.Add(FullDrawSaveMuscleNames[i]);
                }
            }

            if (includeFingerMuscles)
            {
                for (int i = 0; i < RightHandFingerMuscleNames.Length; i++)
                {
                    if (IsMuscleAvailableOnAvatar(RightHandFingerMuscleNames[i]))
                    {
                        names.Add(RightHandFingerMuscleNames[i]);
                    }
                }
            }

            if (includeExistingClipBindings && clip != null)
            {
                UnityEditor.EditorCurveBinding[] bindings = UnityEditor.AnimationUtility.GetCurveBindings(clip);
                for (int i = 0; i < bindings.Length; i++)
                {
                    if (CCS_AnimationFitStudioClipCurveModeUtility.IsHumanoidMuscleBinding(bindings[i]))
                    {
                        names.Add(bindings[i].propertyName);
                    }
                }
            }

            return new List<string>(names);
        }

        private static string[] BuildFullDrawSaveMuscleNames()
        {
            List<string> names = new List<string>(TorsoSaveMuscleNames.Length + RightArmSaveMuscleNames.Length);
            names.AddRange(TorsoSaveMuscleNames);
            names.AddRange(RightArmSaveMuscleNames);
            return names.ToArray();
        }

        private static Dictionary<string, int> BuildMuscleNameIndex()
        {
            Dictionary<string, int> map = new Dictionary<string, int>();
            for (int i = 0; i < UnityEngine.HumanTrait.MuscleCount; i++)
            {
                map[UnityEngine.HumanTrait.MuscleName[i]] = i;
            }

            return map;
        }
    }
}
