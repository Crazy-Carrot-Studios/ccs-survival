using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AnimationFitStudioPoseEditData
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Stores arm, chest, finger segment, and axis settings for pose edits.
// PLACEMENT: Used by Animation Fit Studio window, pose utility, and save.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Detailed finger edits are applied after base pose sampling every update.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public enum CCS_AnimationFitStudioLocalAxisKind
    {
        LocalX,
        LocalNegX,
        LocalY,
        LocalNegY,
        LocalZ,
        LocalNegZ,
    }

    public enum CCS_AnimationFitStudioFingerSegmentSelectionKind
    {
        WholeFinger = -1,
        Proximal = 0,
        Intermediate = 1,
        Distal = 2,
    }

    public sealed class CCS_AnimationFitStudioPartEditState
    {
        public Vector3 EulerOffsetDegrees;

        public float FingerCurl;

        public bool WasEdited =>
            EulerOffsetDegrees != Vector3.zero || FingerCurl > 0.0001f;
    }

    public sealed class CCS_AnimationFitStudioFingerSegmentEditState
    {
        public float PitchDegrees;

        public float YawDegrees;

        public float RollDegrees;

        public float CurlDegrees;

        public float SpreadDegrees;

        public bool WasEdited =>
            Mathf.Abs(PitchDegrees) > 0.0001f
            || Mathf.Abs(YawDegrees) > 0.0001f
            || Mathf.Abs(RollDegrees) > 0.0001f
            || Mathf.Abs(CurlDegrees) > 0.0001f
            || Mathf.Abs(SpreadDegrees) > 0.0001f;

        public void Reset()
        {
            PitchDegrees = 0f;
            YawDegrees = 0f;
            RollDegrees = 0f;
            CurlDegrees = 0f;
            SpreadDegrees = 0f;
        }
    }

    public sealed class CCS_AnimationFitStudioFingerAxisSettings
    {
        public CCS_AnimationFitStudioLocalAxisKind CurlAxis =
            CCS_AnimationFitStudioLocalAxisKind.LocalNegX;

        public CCS_AnimationFitStudioLocalAxisKind SpreadAxis =
            CCS_AnimationFitStudioLocalAxisKind.LocalY;
    }

    public sealed class CCS_AnimationFitStudioFingerChainEditState
    {
        public const int SegmentCount = 3;

        public readonly CCS_AnimationFitStudioFingerSegmentEditState[] Segments =
        {
            new CCS_AnimationFitStudioFingerSegmentEditState(),
            new CCS_AnimationFitStudioFingerSegmentEditState(),
            new CCS_AnimationFitStudioFingerSegmentEditState(),
        };

        public bool OverrideAxis;

        public CCS_AnimationFitStudioFingerAxisSettings AxisOverride =
            new CCS_AnimationFitStudioFingerAxisSettings();

        public bool WasEdited
        {
            get
            {
                for (int i = 0; i < Segments.Length; i++)
                {
                    if (Segments[i].WasEdited)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void ResetSegments()
        {
            for (int i = 0; i < Segments.Length; i++)
            {
                Segments[i].Reset();
            }
        }
    }

    public sealed class CCS_AnimationFitStudioPoseEditData
    {
        public const float EditEpsilon = 0.0001f;

        public readonly Dictionary<string, CCS_AnimationFitStudioPartEditState> PartEdits =
            new Dictionary<string, CCS_AnimationFitStudioPartEditState>();

        public readonly Dictionary<string, CCS_AnimationFitStudioFingerChainEditState> FingerEdits =
            new Dictionary<string, CCS_AnimationFitStudioFingerChainEditState>();

        public CCS_AnimationFitStudioFingerAxisSettings SharedFingerAxisSettings =
            new CCS_AnimationFitStudioFingerAxisSettings();

        public bool UseSharedFingerAxisSettings = true;

        public void InitializeDefaults()
        {
            PartEdits.Clear();
            FingerEdits.Clear();
            UseSharedFingerAxisSettings = true;
            SharedFingerAxisSettings = new CCS_AnimationFitStudioFingerAxisSettings();

            IReadOnlyList<CCS_AnimationFitStudioBodyPartDefinition> definitions =
                CCS_AnimationFitStudioBodyPartCatalog.AllDefinitions;
            for (int i = 0; i < definitions.Count; i++)
            {
                PartEdits[definitions[i].Id] = new CCS_AnimationFitStudioPartEditState();
            }

            IReadOnlyList<CCS_AnimationFitStudioEditPartDefinition> editParts =
                CCS_AnimationFitStudioEditPartCatalog.AllDefinitions;
            for (int i = 0; i < editParts.Count; i++)
            {
                if (editParts[i].Kind != CCS_AnimationFitStudioEditPartKind.Finger)
                {
                    continue;
                }

                FingerEdits[editParts[i].PartId] = new CCS_AnimationFitStudioFingerChainEditState();
            }
        }

        public bool TryGetFingerChainEdit(
            string fingerId,
            out CCS_AnimationFitStudioFingerChainEditState chainEdit)
        {
            return FingerEdits.TryGetValue(fingerId, out chainEdit);
        }

        public CCS_AnimationFitStudioFingerChainEditState GetOrCreateFingerChainEdit(string fingerId)
        {
            if (!FingerEdits.TryGetValue(fingerId, out CCS_AnimationFitStudioFingerChainEditState chainEdit))
            {
                chainEdit = new CCS_AnimationFitStudioFingerChainEditState();
                FingerEdits[fingerId] = chainEdit;
            }

            return chainEdit;
        }

        public int CountEditedFingerSegments()
        {
            int count = 0;
            foreach (KeyValuePair<string, CCS_AnimationFitStudioFingerChainEditState> pair in FingerEdits)
            {
                CCS_AnimationFitStudioFingerChainEditState chain = pair.Value;
                if (chain == null)
                {
                    continue;
                }

                for (int i = 0; i < chain.Segments.Length; i++)
                {
                    if (chain.Segments[i].WasEdited)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public bool HasAnyEdits()
        {
            foreach (KeyValuePair<string, CCS_AnimationFitStudioPartEditState> pair in PartEdits)
            {
                if (pair.Value != null && pair.Value.WasEdited)
                {
                    return true;
                }
            }

            foreach (KeyValuePair<string, CCS_AnimationFitStudioFingerChainEditState> pair in FingerEdits)
            {
                if (pair.Value != null && pair.Value.WasEdited)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public static class CCS_AnimationFitStudioLocalAxisUtility
    {
        public static readonly string[] AxisLabels =
        {
            "Local X",
            "Local -X",
            "Local Y",
            "Local -Y",
            "Local Z",
            "Local -Z",
        };

        public static Vector3 ToAxisVector(CCS_AnimationFitStudioLocalAxisKind axisKind)
        {
            switch (axisKind)
            {
                case CCS_AnimationFitStudioLocalAxisKind.LocalNegX:
                    return Vector3.left;
                case CCS_AnimationFitStudioLocalAxisKind.LocalY:
                    return Vector3.up;
                case CCS_AnimationFitStudioLocalAxisKind.LocalNegY:
                    return Vector3.down;
                case CCS_AnimationFitStudioLocalAxisKind.LocalZ:
                    return Vector3.forward;
                case CCS_AnimationFitStudioLocalAxisKind.LocalNegZ:
                    return Vector3.back;
                default:
                    return Vector3.right;
            }
        }

        public static int AxisKindToIndex(CCS_AnimationFitStudioLocalAxisKind axisKind)
        {
            return (int)axisKind;
        }

        public static CCS_AnimationFitStudioLocalAxisKind IndexToAxisKind(int index)
        {
            return (CCS_AnimationFitStudioLocalAxisKind)Mathf.Clamp(index, 0, AxisLabels.Length - 1);
        }
    }
}
