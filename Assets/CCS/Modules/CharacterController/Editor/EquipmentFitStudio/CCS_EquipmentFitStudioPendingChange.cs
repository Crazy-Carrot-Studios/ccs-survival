using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioPendingChange
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Tracks pending socket/IK diffs before profile save.
// PLACEMENT: Editor-only data class.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Used by Equipment Fit Studio window save flow.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    [Serializable]
    public sealed class CCS_EquipmentFitStudioPendingChange
    {
        public string Label = string.Empty;

        public Vector3 OldPosition;

        public Vector3 NewPosition;

        public Vector3 OldEulerAngles;

        public Vector3 NewEulerAngles;

        public Vector3 OldScale = Vector3.one;

        public Vector3 NewScale = Vector3.one;

        public bool HasChanges =>
            OldPosition != NewPosition
            || OldEulerAngles != NewEulerAngles
            || OldScale != NewScale;

        public void Capture(string label, Vector3 position, Vector3 eulerAngles, Vector3 scale)
        {
            Label = label;
            NewPosition = position;
            NewEulerAngles = eulerAngles;
            NewScale = scale;
        }

        public void SetBaseline(Vector3 position, Vector3 eulerAngles, Vector3 scale)
        {
            OldPosition = position;
            OldEulerAngles = eulerAngles;
            OldScale = scale;
            NewPosition = position;
            NewEulerAngles = eulerAngles;
            NewScale = scale;
        }
    }
}
