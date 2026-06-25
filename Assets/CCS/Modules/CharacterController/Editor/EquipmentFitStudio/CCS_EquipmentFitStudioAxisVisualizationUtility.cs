using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioAxisVisualizationUtility
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Scene-view axis gizmos for Fit Studio socket and weapon preview tuning.
// PLACEMENT: Editor utility invoked from Fit Studio OnSceneGui.
// AUTHOR: James Schilz
// CREATED: 2026-06-24
// NOTES: Red=X, Green=Y, Blue=Z, Yellow=weapon forward/barrel axis.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public static class CCS_EquipmentFitStudioAxisVisualizationUtility
    {
        private const float AxisLength = 0.08f;

        private const float ForwardAxisLength = 0.12f;

        public static void DrawLocalAxes(
            Transform target,
            float length,
            bool drawLabels)
        {
            if (target == null)
            {
                return;
            }

            DrawAxis(target.position, target.right, Color.red, length, drawLabels ? "X" : null);
            DrawAxis(target.position, target.up, Color.green, length, drawLabels ? "Y" : null);
            DrawAxis(target.position, target.forward, Color.blue, length, drawLabels ? "Z" : null);
        }

        public static void DrawWeaponForwardAxis(
            Transform attachmentRoot,
            Quaternion attachmentLocalRotation,
            CCS_EquipmentFitStudioWeaponForwardAxis forwardAxis,
            bool drawLabel)
        {
            if (attachmentRoot == null)
            {
                return;
            }

            Vector3 localForward = CCS_EquipmentFitStudioWeaponRotationUtility.GetWeaponForwardLocalAxis(forwardAxis);
            Vector3 worldForward = attachmentRoot.TransformDirection(attachmentLocalRotation * localForward).normalized;
            DrawAxis(
                attachmentRoot.position,
                worldForward,
                Color.yellow,
                ForwardAxisLength,
                drawLabel ? "Forward" : null);
        }

        public static void DrawMuzzlePointAxis(Transform muzzleTransform)
        {
            if (muzzleTransform == null)
            {
                return;
            }

            DrawAxis(muzzleTransform.position, muzzleTransform.forward, Color.white, ForwardAxisLength, "Muzzle");
        }

        private static void DrawAxis(
            Vector3 origin,
            Vector3 direction,
            Color color,
            float length,
            string label)
        {
            if (direction.sqrMagnitude <= 0.000001f)
            {
                return;
            }

            Handles.color = color;
            Vector3 end = origin + direction.normalized * length;
            Handles.DrawLine(origin, end);
            Handles.ConeHandleCap(0, end, Quaternion.LookRotation(direction.normalized), length * 0.18f, EventType.Repaint);
            if (!string.IsNullOrEmpty(label))
            {
                Handles.Label(end, label);
            }
        }
    }
}
