using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_AnimationFitStudioEditPartCatalog
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Edit Part dropdown entries for Animation Fit Studio final-pose editor.
// PLACEMENT: Editor catalog used by window layout and validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Unified Pitch/Yaw/Roll controls for torso, right arm, wrist, and fingers.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public enum CCS_AnimationFitStudioEditPartKind
    {
        BoneRotation,
        Finger,
    }

    public sealed class CCS_AnimationFitStudioEditPartDefinition
    {
        public string PartId { get; }

        public string DisplayLabel { get; }

        public CCS_AnimationFitStudioEditPartKind Kind { get; }

        public int DefaultFingerSegmentIndex { get; }

        public CCS_AnimationFitStudioEditPartDefinition(
            string partId,
            string displayLabel,
            CCS_AnimationFitStudioEditPartKind kind,
            int defaultFingerSegmentIndex = 0)
        {
            PartId = partId;
            DisplayLabel = displayLabel;
            Kind = kind;
            DefaultFingerSegmentIndex = defaultFingerSegmentIndex;
        }
    }

    public static class CCS_AnimationFitStudioEditPartCatalog
    {
        public const int DefaultEditPartIndex = 4;

        private static readonly IReadOnlyList<CCS_AnimationFitStudioEditPartDefinition> Definitions =
            new List<CCS_AnimationFitStudioEditPartDefinition>
            {
                new CCS_AnimationFitStudioEditPartDefinition(
                    "spine",
                    "Spine",
                    CCS_AnimationFitStudioEditPartKind.BoneRotation),
                new CCS_AnimationFitStudioEditPartDefinition(
                    "chest_aim_lean",
                    "Chest",
                    CCS_AnimationFitStudioEditPartKind.BoneRotation),
                new CCS_AnimationFitStudioEditPartDefinition(
                    "upper_chest_aim_lean",
                    "Upper Chest",
                    CCS_AnimationFitStudioEditPartKind.BoneRotation),
                new CCS_AnimationFitStudioEditPartDefinition(
                    "right_shoulder",
                    "Shoulder / Clavicle",
                    CCS_AnimationFitStudioEditPartKind.BoneRotation),
                new CCS_AnimationFitStudioEditPartDefinition(
                    "right_upper_arm",
                    "Upper Arm",
                    CCS_AnimationFitStudioEditPartKind.BoneRotation),
                new CCS_AnimationFitStudioEditPartDefinition(
                    "right_forearm",
                    "Forearm",
                    CCS_AnimationFitStudioEditPartKind.BoneRotation),
                new CCS_AnimationFitStudioEditPartDefinition(
                    "right_hand",
                    "Wrist / Hand",
                    CCS_AnimationFitStudioEditPartKind.BoneRotation),
                new CCS_AnimationFitStudioEditPartDefinition(
                    "right_thumb",
                    "Thumb",
                    CCS_AnimationFitStudioEditPartKind.Finger),
                new CCS_AnimationFitStudioEditPartDefinition(
                    "right_index",
                    "Index Finger",
                    CCS_AnimationFitStudioEditPartKind.Finger),
                new CCS_AnimationFitStudioEditPartDefinition(
                    "right_middle",
                    "Middle Finger",
                    CCS_AnimationFitStudioEditPartKind.Finger),
                new CCS_AnimationFitStudioEditPartDefinition(
                    "right_ring",
                    "Ring Finger",
                    CCS_AnimationFitStudioEditPartKind.Finger),
                new CCS_AnimationFitStudioEditPartDefinition(
                    "right_pinky",
                    "Pinky Finger",
                    CCS_AnimationFitStudioEditPartKind.Finger),
            };

        private static readonly string[] EditPartLabels = BuildEditPartLabels();

        public static IReadOnlyList<CCS_AnimationFitStudioEditPartDefinition> AllDefinitions => Definitions;

        public static string[] EditPartDisplayLabels => EditPartLabels;

        public static int EditPartCount => Definitions.Count;

        public static CCS_AnimationFitStudioEditPartDefinition GetDefaultEditPart()
        {
            return Definitions[DefaultEditPartIndex];
        }

        public static bool TryGetDefinition(int index, out CCS_AnimationFitStudioEditPartDefinition definition)
        {
            if (index >= 0 && index < Definitions.Count)
            {
                definition = Definitions[index];
                return true;
            }

            definition = null;
            return false;
        }

        public static bool TryGetDefinitionByPartId(
            string partId,
            out CCS_AnimationFitStudioEditPartDefinition definition)
        {
            for (int i = 0; i < Definitions.Count; i++)
            {
                if (Definitions[i].PartId == partId)
                {
                    definition = Definitions[i];
                    return true;
                }
            }

            definition = null;
            return false;
        }

        public static int GetEditPartIndex(string partId)
        {
            for (int i = 0; i < Definitions.Count; i++)
            {
                if (Definitions[i].PartId == partId)
                {
                    return i;
                }
            }

            return DefaultEditPartIndex;
        }

        public static bool IsFingerPart(CCS_AnimationFitStudioEditPartDefinition definition)
        {
            return definition != null && definition.Kind == CCS_AnimationFitStudioEditPartKind.Finger;
        }

        public static string GetDisplayLabel(string partId)
        {
            return TryGetDefinitionByPartId(partId, out CCS_AnimationFitStudioEditPartDefinition definition)
                ? definition.DisplayLabel
                : partId;
        }

        private static string[] BuildEditPartLabels()
        {
            string[] labels = new string[Definitions.Count];
            for (int i = 0; i < Definitions.Count; i++)
            {
                labels[i] = Definitions[i].DisplayLabel;
            }

            return labels;
        }
    }
}
