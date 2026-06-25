using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AnimationFitStudioBodyPartCatalog
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Defines the limited right-arm / grip body parts exposed by Animation Fit Studio.
// PLACEMENT: Editor utility used by Animation Fit Studio window and validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Test tool only — no full skeleton listing.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public enum CCS_AnimationFitStudioBodyPartKind
    {
        BoneRotation,
        FingerCurl,
        OptionalChestLean,
    }

    public sealed class CCS_AnimationFitStudioBodyPartDefinition
    {
        public string Id { get; }

        public string Label { get; }

        public CCS_AnimationFitStudioBodyPartKind Kind { get; }

        public HumanBodyBones Bone { get; }

        public string[] MuscleNames { get; }

        public CCS_AnimationFitStudioBodyPartDefinition(
            string id,
            string label,
            CCS_AnimationFitStudioBodyPartKind kind,
            HumanBodyBones bone = HumanBodyBones.LastBone,
            params string[] muscleNames)
        {
            Id = id;
            Label = label;
            Kind = kind;
            Bone = bone;
            MuscleNames = muscleNames ?? System.Array.Empty<string>();
        }
    }

    public static class CCS_AnimationFitStudioBodyPartCatalog
    {
        public const int AllowedBodyPartCount = 12;

        private static readonly IReadOnlyList<CCS_AnimationFitStudioBodyPartDefinition> Definitions =
            new List<CCS_AnimationFitStudioBodyPartDefinition>
            {
                new CCS_AnimationFitStudioBodyPartDefinition(
                    "spine",
                    "Spine",
                    CCS_AnimationFitStudioBodyPartKind.BoneRotation,
                    muscleNames: new[]
                    {
                        "Spine Front-Back",
                        "Spine Left-Right",
                        "Spine Twist Left-Right",
                    }),
                new CCS_AnimationFitStudioBodyPartDefinition(
                    "chest_aim_lean",
                    "Chest",
                    CCS_AnimationFitStudioBodyPartKind.BoneRotation,
                    muscleNames: new[]
                    {
                        "Chest Front-Back",
                        "Chest Left-Right",
                        "Chest Twist Left-Right",
                    }),
                new CCS_AnimationFitStudioBodyPartDefinition(
                    "upper_chest_aim_lean",
                    "Upper Chest",
                    CCS_AnimationFitStudioBodyPartKind.BoneRotation,
                    muscleNames: new[]
                    {
                        "UpperChest Front-Back",
                        "UpperChest Left-Right",
                        "UpperChest Twist Left-Right",
                    }),
                new CCS_AnimationFitStudioBodyPartDefinition(
                    "right_shoulder",
                    "Shoulder",
                    CCS_AnimationFitStudioBodyPartKind.BoneRotation,
                    HumanBodyBones.RightShoulder,
                    "Right Shoulder Down-Up",
                    "Right Shoulder Front-Back"),
                new CCS_AnimationFitStudioBodyPartDefinition(
                    "right_upper_arm",
                    "Upper Arm",
                    CCS_AnimationFitStudioBodyPartKind.BoneRotation,
                    HumanBodyBones.RightUpperArm,
                    "Right Arm Down-Up",
                    "Right Arm Front-Back",
                    "Right Arm Twist In-Out"),
                new CCS_AnimationFitStudioBodyPartDefinition(
                    "right_forearm",
                    "Forearm",
                    CCS_AnimationFitStudioBodyPartKind.BoneRotation,
                    HumanBodyBones.RightLowerArm,
                    muscleNames: new[]
                    {
                        string.Empty,
                        string.Empty,
                        "Right Forearm Twist In-Out",
                    }),
                new CCS_AnimationFitStudioBodyPartDefinition(
                    "right_hand",
                    "Wrist",
                    CCS_AnimationFitStudioBodyPartKind.BoneRotation,
                    HumanBodyBones.RightHand,
                    "Right Hand Down-Up",
                    "Right Hand In-Out"),
                new CCS_AnimationFitStudioBodyPartDefinition(
                    "right_thumb",
                    "Thumb Curl",
                    CCS_AnimationFitStudioBodyPartKind.FingerCurl,
                    muscleNames: new[]
                    {
                        "RightHand.Thumb.1 Stretched",
                        "RightHand.Thumb.2 Stretched",
                        "RightHand.Thumb.3 Stretched",
                    }),
                new CCS_AnimationFitStudioBodyPartDefinition(
                    "right_index",
                    "Index Curl",
                    CCS_AnimationFitStudioBodyPartKind.FingerCurl,
                    muscleNames: new[]
                    {
                        "RightHand.Index.1 Stretched",
                        "RightHand.Index.2 Stretched",
                        "RightHand.Index.3 Stretched",
                    }),
                new CCS_AnimationFitStudioBodyPartDefinition(
                    "right_middle",
                    "Middle Curl",
                    CCS_AnimationFitStudioBodyPartKind.FingerCurl,
                    muscleNames: new[]
                    {
                        "RightHand.Middle.1 Stretched",
                        "RightHand.Middle.2 Stretched",
                        "RightHand.Middle.3 Stretched",
                    }),
                new CCS_AnimationFitStudioBodyPartDefinition(
                    "right_ring",
                    "Ring Curl",
                    CCS_AnimationFitStudioBodyPartKind.FingerCurl,
                    muscleNames: new[]
                    {
                        "RightHand.Ring.1 Stretched",
                        "RightHand.Ring.2 Stretched",
                        "RightHand.Ring.3 Stretched",
                    }),
                new CCS_AnimationFitStudioBodyPartDefinition(
                    "right_pinky",
                    "Pinky Curl",
                    CCS_AnimationFitStudioBodyPartKind.FingerCurl,
                    muscleNames: new[]
                    {
                        "RightHand.Little.1 Stretched",
                        "RightHand.Little.2 Stretched",
                        "RightHand.Little.3 Stretched",
                    }),
            };

        public static IReadOnlyList<CCS_AnimationFitStudioBodyPartDefinition> AllDefinitions => Definitions;

        public static bool TryGetDefinition(string id, out CCS_AnimationFitStudioBodyPartDefinition definition)
        {
            for (int i = 0; i < Definitions.Count; i++)
            {
                definition = Definitions[i];
                if (definition.Id == id)
                {
                    return true;
                }
            }

            definition = null;
            return false;
        }
    }
}
