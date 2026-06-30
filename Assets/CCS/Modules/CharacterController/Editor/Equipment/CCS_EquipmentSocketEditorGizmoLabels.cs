using CCS.Modules.CharacterController.Diagnostics;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentSocketEditorGizmoLabels
// CATEGORY: Modules / CharacterController / Editor / Equipment
// PURPOSE: Editor-only gizmo labels clarifying socket vs IK attachment points.
// PLACEMENT: Editor utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-30
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_EquipmentSocketEditorGizmoLabels
    {
        private static readonly (string objectName, string label, Color color)[] LabeledTargets =
        {
            ("CCS_HandSocket_Right", "RIGHT HAND SOCKET / WEAPON ATTACH", new Color(0.2f, 0.85f, 0.35f)),
            ("CCS_RightHandIKTarget", "RIGHT HAND IK TARGET / DO NOT ATTACH WEAPON", new Color(0.95f, 0.35f, 0.2f)),
            ("CCS_WeaponAimTarget", "AIM IK TARGET", new Color(0.95f, 0.75f, 0.2f)),
            ("MuzzlePoint", "FIRE ORIGIN", new Color(0.35f, 0.65f, 0.95f)),
        };

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawEquipmentSocketLabels(Transform transform, GizmoType gizmoType)
        {
            if (!ShouldDrawLabels() || transform == null)
            {
                return;
            }

            for (int i = 0; i < LabeledTargets.Length; i++)
            {
                (string objectName, string label, Color color) entry = LabeledTargets[i];
                if (transform.name != entry.objectName)
                {
                    continue;
                }

                Gizmos.color = entry.color;
                Gizmos.DrawWireSphere(transform.position, 0.03f);
                Handles.color = entry.color;
                Handles.Label(transform.position + Vector3.up * 0.05f, entry.label);
                return;
            }
        }

        private static bool ShouldDrawLabels()
        {
            if (!Application.isPlaying)
            {
                return true;
            }

            CCS_CharacterControllerDiagnosticsManager diagnosticsManager =
                CCS_CharacterControllerDiagnosticsManager.ActiveInstance;
            return diagnosticsManager != null && diagnosticsManager.EnableVisualDebugHelpers;
        }
    }
}
