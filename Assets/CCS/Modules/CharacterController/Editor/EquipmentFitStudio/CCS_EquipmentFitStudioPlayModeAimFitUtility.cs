using CCS.Modules.CharacterController;
using CCS.Modules.Weapons;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioPlayModeAimFitUtility
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Editor-safe Play Mode Aim Fit controls for runtime player tuning.
// PLACEMENT: Editor utility used by Equipment Fit Studio window.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Forces aim/equipped visual without RMB; clears overrides on cleanup.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public static class CCS_EquipmentFitStudioPlayModeAimFitUtility
    {
        public static bool TryGetRuntimeFitComponents(
            GameObject playerRoot,
            out CCS_CharacterAimLocomotionController aimController,
            out CCS_PlayerEquipmentVisualController visualController)
        {
            aimController = null;
            visualController = null;
            if (playerRoot == null)
            {
                return false;
            }

            aimController = playerRoot.GetComponent<CCS_CharacterAimLocomotionController>();
            visualController = playerRoot.GetComponent<CCS_PlayerEquipmentVisualController>();
            return aimController != null && visualController != null;
        }

        public static bool ForceAimPoseOn(GameObject playerRoot, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (!EditorApplication.isPlaying)
            {
                errorMessage = "Play Mode Aim Fit requires Play Mode.";
                return false;
            }

            if (!TryGetRuntimeFitComponents(playerRoot, out CCS_CharacterAimLocomotionController aimController, out CCS_PlayerEquipmentVisualController visualController))
            {
                errorMessage = "Runtime player is missing aim locomotion or equipment visual components.";
                return false;
            }

            aimController.SetEditorAimFitOverride(true);
            visualController.SetEditorAimFitOverride(true);
            return true;
        }

        public static void ForceAimPoseOff(GameObject playerRoot)
        {
            if (playerRoot == null)
            {
                return;
            }

            CCS_CharacterAimLocomotionController aimController =
                playerRoot.GetComponent<CCS_CharacterAimLocomotionController>();
            if (aimController != null)
            {
                aimController.SetEditorAimFitOverride(false);
            }

            CCS_PlayerEquipmentVisualController visualController =
                playerRoot.GetComponent<CCS_PlayerEquipmentVisualController>();
            if (visualController != null)
            {
                visualController.SetEditorAimFitOverride(false);
            }
        }

        public static bool ShowEquippedVisual(GameObject playerRoot, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (!EditorApplication.isPlaying)
            {
                errorMessage = "Show Equipped Visual requires Play Mode.";
                return false;
            }

            if (!TryGetRuntimeFitComponents(playerRoot, out _, out CCS_PlayerEquipmentVisualController visualController))
            {
                errorMessage = "Runtime player is missing CCS_PlayerEquipmentVisualController.";
                return false;
            }

            visualController.SetEditorForceEquippedVisual(true);
            return true;
        }

        public static void HideEquippedVisual(GameObject playerRoot)
        {
            if (playerRoot == null)
            {
                return;
            }

            CCS_PlayerEquipmentVisualController visualController =
                playerRoot.GetComponent<CCS_PlayerEquipmentVisualController>();
            if (visualController != null)
            {
                visualController.SetEditorForceEquippedVisual(false);
            }
        }

        public static void CleanupEditorOverrides(GameObject playerRoot)
        {
            ForceAimPoseOff(playerRoot);
            HideEquippedVisual(playerRoot);
        }

        public static Transform GetRuntimeEquippedAttachmentRoot(GameObject playerRoot)
        {
            if (playerRoot == null)
            {
                return null;
            }

            CCS_EquipmentFitStudioTestAttachmentUtility.TryGetRuntimeAttachmentRoot(
                playerRoot,
                CCS_EquipmentConstants.HandSocketRightId,
                CCS_EquipmentConstants.RuntimeEquippedAttachmentRootObjectName,
                out Transform attachmentRoot);
            return attachmentRoot;
        }

        public static bool TryCaptureFromRuntimeAttachmentRoot(
            GameObject playerRoot,
            CCS_EquipmentFitStudioPendingChange pending,
            out string message,
            out MessageType messageType)
        {
            message = string.Empty;
            messageType = MessageType.Info;

            if (playerRoot == null)
            {
                message = "Select a runtime player before capture.";
                messageType = MessageType.Error;
                return false;
            }

            CCS_EquipmentSocketRegistry registry = playerRoot.GetComponent<CCS_EquipmentSocketRegistry>();
            if (registry == null || !registry.TryGetSocket(CCS_EquipmentConstants.HandSocketRightId, out Transform socketTransform))
            {
                message = "Missing right hand socket on runtime player.";
                messageType = MessageType.Error;
                return false;
            }

            Transform attachmentRoot = GetRuntimeEquippedAttachmentRoot(playerRoot);
            if (attachmentRoot == null)
            {
                message = "Show Equipped Visual first, then capture from the runtime attachment root.";
                messageType = MessageType.Error;
                return false;
            }

            if (!CCS_EquipmentFitStudioTestAttachmentUtility.TryGetSocketDefinition(
                    registry,
                    CCS_EquipmentConstants.HandSocketRightId,
                    out Vector3 defPos,
                    out Vector3 defEuler,
                    out Vector3 defScale))
            {
                message = "Missing right hand socket definition.";
                messageType = MessageType.Error;
                return false;
            }

            string profilePath =
                CCS_EquipmentFitStudioRevolverFitUtility.GetRevolverAttachmentFitProfilePath(
                    CCS_EquipmentConstants.HandSocketRightId);
            CCS_WeaponAttachmentFitProfile profile =
                CCS_EquipmentFitProfilePersistenceUtility.LoadProfileFromDisk(profilePath);

            CCS_WeaponAttachmentFitProfileApplicator.ComputeProfileSocketLocalFromAttachmentRoot(
                defPos,
                defEuler,
                defScale,
                attachmentRoot.localPosition,
                attachmentRoot.localEulerAngles,
                attachmentRoot.localScale,
                out Vector3 profilePos,
                out Vector3 profileEuler,
                out Vector3 profileScale);

            pending.CaptureFromBaseline(
                CCS_EquipmentConstants.HandSocketRightId,
                profile != null ? profile.name : "CCS_RevolverM1879_RightHandEquipped_Fit",
                profile != null ? profile.SocketLocalPosition : Vector3.zero,
                profile != null ? profile.SocketLocalEulerAngles : Vector3.zero,
                profile != null ? profile.SocketLocalScale : Vector3.one,
                profilePos,
                profileEuler,
                profileScale);

            message = "Captured runtime equipped attachment values for Right Hand Equipped profile.";
            messageType = MessageType.Info;
            return true;
        }
    }
}
