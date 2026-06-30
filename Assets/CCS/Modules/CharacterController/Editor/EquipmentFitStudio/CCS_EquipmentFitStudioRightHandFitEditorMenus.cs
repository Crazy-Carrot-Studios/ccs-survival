using CCS.Modules.CharacterController.Editor.EquipmentFitStudio;
using CCS.Modules.Weapons;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioRightHandFitEditorMenus
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Editor-only helpers for right-hand revolver fit profile capture/apply.
// PLACEMENT: Editor menu utilities. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-30
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public static class CCS_EquipmentFitStudioRightHandFitEditorMenus
    {
        private const string MenuRoot = "CCS/Character Controller/Equipment/Right Hand Revolver Fit/";

        [MenuItem(MenuRoot + "Capture Current Right-Hand Preview Offset To Fit Profile", false, 210)]
        public static void CaptureCurrentRightHandPreviewOffsetToFitProfile()
        {
            CCS_PlayerEquipmentVisualController equipmentVisual =
                Object.FindAnyObjectByType<CCS_PlayerEquipmentVisualController>();
            if (equipmentVisual == null)
            {
                Debug.LogWarning(
                    "[Right Hand Fit] Missing CCS_PlayerEquipmentVisualController. Spawn the validation player first.");
                return;
            }

            CCS_EquipmentSocketRegistry registry = equipmentVisual.GetComponent<CCS_EquipmentSocketRegistry>();
            if (registry == null
                || !registry.TryGetSocket(CCS_EquipmentConstants.HandSocketRightId, out Transform socketTransform))
            {
                Debug.LogWarning("[Right Hand Fit] Missing CCS_HandSocket_Right on player.");
                return;
            }

            Transform attachmentRoot =
                CCS_EquipmentFitStudioPreviewAttachmentUtility.FindRightHandAttachmentOffsetRoot(socketTransform);
            if (attachmentRoot == null)
            {
                Debug.LogWarning(
                    "[Right Hand Fit] Missing "
                    + CCS_EquipmentConstants.RightHandRevolverAttachmentOffsetObjectName
                    + ". Enable Force Revolver Hand Socket Preview first.");
                return;
            }

            CCS_EquipmentFitStudioPendingChange pending = new CCS_EquipmentFitStudioPendingChange();
            if (!CCS_EquipmentFitStudioCaptureUtility.TryCaptureAttachmentRootValues(
                    equipmentVisual.gameObject,
                    attachmentRoot,
                    CCS_EquipmentConstants.HandSocketRightId,
                    true,
                    pending,
                    out string message,
                    out MessageType messageType))
            {
                Debug.LogWarning("[Right Hand Fit] Capture failed: " + message);
                return;
            }

            if (!CCS_EquipmentFitStudioCaptureUtility.TrySavePendingSocketCapture(
                    pending,
                    CCS_EquipmentConstants.HandSocketRightId,
                    out string successMessage,
                    out string errorMessage))
            {
                Debug.LogWarning("[Right Hand Fit] Save failed: " + errorMessage);
                return;
            }

            Debug.Log(
                "[Right Hand Fit] Saved "
                + CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath
                + ". "
                + successMessage
                + " "
                + message);
        }

        [MenuItem(MenuRoot + "Apply Fit Profile To Current Right-Hand Preview", false, 211)]
        public static void ApplyFitProfileToCurrentRightHandPreview()
        {
            CCS_PlayerEquipmentVisualController equipmentVisual =
                Object.FindAnyObjectByType<CCS_PlayerEquipmentVisualController>();
            if (equipmentVisual == null)
            {
                Debug.LogWarning(
                    "[Right Hand Fit] Missing CCS_PlayerEquipmentVisualController. Spawn the validation player first.");
                return;
            }

            equipmentVisual.SetDiagnosticsRevolverHandSocketPreviewActive(false);
            equipmentVisual.SetDiagnosticsRevolverHandSocketPreviewActive(true);
            Debug.Log(
                "[Right Hand Fit] Applied "
                + CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath
                + " to diagnostics right-hand preview.");
        }

        [MenuItem(MenuRoot + "Reset Right-Hand Preview Child Visual To Identity", false, 212)]
        public static void ResetRightHandPreviewChildVisualToIdentity()
        {
            CCS_PlayerEquipmentVisualController equipmentVisual =
                Object.FindAnyObjectByType<CCS_PlayerEquipmentVisualController>();
            if (equipmentVisual == null)
            {
                Debug.LogWarning(
                    "[Right Hand Fit] Missing CCS_PlayerEquipmentVisualController. Spawn the validation player first.");
                return;
            }

            CCS_EquipmentSocketRegistry registry = equipmentVisual.GetComponent<CCS_EquipmentSocketRegistry>();
            if (registry == null
                || !registry.TryGetSocket(CCS_EquipmentConstants.HandSocketRightId, out Transform socketTransform))
            {
                Debug.LogWarning("[Right Hand Fit] Missing CCS_HandSocket_Right on player.");
                return;
            }

            Transform attachmentRoot =
                CCS_EquipmentFitStudioPreviewAttachmentUtility.FindRightHandAttachmentOffsetRoot(socketTransform);
            if (attachmentRoot == null)
            {
                Debug.LogWarning(
                    "[Right Hand Fit] Missing "
                    + CCS_EquipmentConstants.RightHandRevolverAttachmentOffsetObjectName
                    + ".");
                return;
            }

            CCS_WeaponAttachmentFitProfileApplicator.ResetDirectVisualChildToIdentity(attachmentRoot);
            Debug.Log(
                "[Right Hand Fit] Reset direct visual children under "
                + BuildTransformPath(attachmentRoot)
                + " to local identity.");
        }

        private static string BuildTransformPath(Transform transform)
        {
            string path = transform.name;
            Transform current = transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }
    }
}
