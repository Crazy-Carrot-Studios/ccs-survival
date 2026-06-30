using System.IO;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioCaptureUtility
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Capture pending socket values and save/verify revolver fit profiles.
// PLACEMENT: Editor utility used by Equipment Fit Studio window.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Capture copies live socket values; save writes pending buffer to assets.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public static class CCS_EquipmentFitStudioCaptureUtility
    {
        #region Public Methods

        public static bool TryCaptureAttachmentRootValues(
            GameObject playerRoot,
            Transform attachmentRoot,
            string socketId,
            bool previewSpawned,
            CCS_EquipmentFitStudioPendingChange pending,
            out string message,
            out MessageType messageType)
        {
            message = string.Empty;
            messageType = MessageType.Info;

            if (playerRoot == null)
            {
                message = "Select a player before capturing values.";
                messageType = MessageType.Error;
                return false;
            }

            if (attachmentRoot == null || string.IsNullOrEmpty(socketId))
            {
                message = "Preview attachment root not found. Load preview first.";
                messageType = MessageType.Error;
                return false;
            }

            string profilePath = CCS_EquipmentFitStudioRevolverFitUtility.GetRevolverAttachmentFitProfilePath(socketId);
            if (string.IsNullOrEmpty(profilePath))
            {
                message = "No revolver fit profile is mapped for this socket. Select Right Hip Holster or Right Hand.";
                messageType = MessageType.Error;
                return false;
            }

            CCS_WeaponAttachmentFitProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(profilePath);
            string profileAssetName = profile != null ? profile.name : Path.GetFileNameWithoutExtension(profilePath);

            Vector3 oldPosition = profile != null ? profile.SocketLocalPosition : Vector3.zero;
            Vector3 oldEuler = profile != null ? profile.SocketLocalEulerAngles : Vector3.zero;
            Vector3 oldScale = profile != null ? profile.SocketLocalScale : Vector3.one;

            if (!CCS_EquipmentFitStudioPreviewAttachmentUtility.TryCaptureProfileFromPreviewAttachment(
                    playerRoot,
                    socketId,
                    attachmentRoot,
                    out Vector3 newPosition,
                    out Vector3 newEuler,
                    out Vector3 newScale))
            {
                message = "Failed to capture profile values from preview attachment root.";
                messageType = MessageType.Error;
                return false;
            }

            pending.CaptureFromBaseline(
                socketId,
                profileAssetName,
                oldPosition,
                oldEuler,
                oldScale,
                newPosition,
                newEuler,
                newScale);

            if (!previewSpawned)
            {
                message = "Captured attachment root values without preview. Spawn preview first if you want visual confirmation.";
                messageType = MessageType.Warning;
                return true;
            }

            if (pending.HasTransformChanges)
            {
                message = "Captured preview attachment root values. Review the pending diff, then save.";
                messageType = MessageType.Info;
            }
            else
            {
                message = "No transform changes detected. You can still save, but the profile already matches this attachment root.";
                messageType = MessageType.Warning;
            }

            return true;
        }

        public static bool TryCaptureSocketValues(
            GameObject playerRoot,
            Transform socketTransform,
            string socketId,
            bool previewSpawned,
            CCS_EquipmentFitStudioPendingChange pending,
            out string message,
            out MessageType messageType)
        {
            if (playerRoot == null || socketTransform == null)
            {
                message = "Select a player and socket before capturing values.";
                messageType = MessageType.Error;
                return false;
            }

            string attachmentRootName =
                socketId == CCS_EquipmentConstants.HandSocketRightId
                    ? CCS_EquipmentConstants.RightHandRevolverAttachmentOffsetObjectName
                    : CCS_EquipmentConstants.RuntimeHolsterAttachmentRootObjectName;
            Transform attachmentRoot = socketTransform.Find(attachmentRootName);
            if (attachmentRoot == null && socketId == CCS_EquipmentConstants.HandSocketRightId)
            {
                attachmentRoot = socketTransform.Find(
                    CCS_EquipmentConstants.LegacyRuntimeEquippedAttachmentRootObjectName);
            }
            if (attachmentRoot == null)
            {
                attachmentRoot = CCS_EquipmentFitStudioPreviewAttachmentUtility.EnsurePreviewAttachmentRoot(
                    socketTransform,
                    attachmentRootName);
            }

            return TryCaptureAttachmentRootValues(
                playerRoot,
                attachmentRoot,
                socketId,
                previewSpawned,
                pending,
                out message,
                out messageType);
        }

        public static bool TrySavePendingSocketCapture(
            CCS_EquipmentFitStudioPendingChange pending,
            string socketId,
            out string successMessage,
            out string errorMessage)
        {
            successMessage = string.Empty;
            errorMessage = string.Empty;

            if (pending == null || !pending.HasCaptured)
            {
                errorMessage = "Capture values first. Save needs a pending captured transform.";
                return false;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
            {
                errorMessage = "Cannot save fit profile while entering/exiting Play Mode.";
                return false;
            }

            string profilePath = CCS_EquipmentFitStudioRevolverFitUtility.GetRevolverAttachmentFitProfilePath(socketId);
            if (string.IsNullOrEmpty(profilePath))
            {
                errorMessage = "No revolver fit profile is mapped for this socket. Select Right Hip Holster or Right Hand.";
                return false;
            }

            CCS_WeaponAttachmentFitProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(profilePath);
            if (profile == null)
            {
                errorMessage = "Missing revolver fit profile asset at " + profilePath + ".";
                return false;
            }

            profile.SetIdentity(
                profile.ProfileId,
                CCS_EquipmentConstants.RevolverM1879WeaponId,
                CCS_EquipmentConstants.TestPlayerCc3BasePlusRigId,
                socketId);
            profile.ApplySocketTransform(
                profile.ProfileId,
                pending.NewPosition,
                pending.NewEulerAngles,
                pending.NewScale);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            CCS_WeaponAttachmentFitProfile reloaded =
                AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(profilePath);
            if (!VerifyProfileValues(
                    reloaded,
                    pending.NewPosition,
                    pending.NewEulerAngles,
                    pending.NewScale))
            {
                errorMessage = "Save verification failed. Profile asset did not contain the captured values after save.";
                return false;
            }

            pending.SetBaseline(pending.NewPosition, pending.NewEulerAngles, pending.NewScale);
            successMessage = BuildSaveSuccessMessage(socketId, profileAssetName: reloaded.name);

            Debug.Log(
                "[Equipment Fit Studio] Saved profile "
                + reloaded.name
                + " Position="
                + CCS_EquipmentFitStudioPendingChange.FormatVector3(reloaded.SocketLocalPosition)
                + " Rotation="
                + CCS_EquipmentFitStudioPendingChange.FormatVector3(reloaded.SocketLocalEulerAngles)
                + " Scale="
                + CCS_EquipmentFitStudioPendingChange.FormatVector3(reloaded.SocketLocalScale)
                + " Path="
                + profilePath);

            return true;
        }

        public static bool VerifyProfileValues(
            CCS_WeaponAttachmentFitProfile profile,
            Vector3 position,
            Vector3 eulerAngles,
            Vector3 scale)
        {
            if (profile == null)
            {
                return false;
            }

            return CCS_EquipmentFitStudioPendingChange.VectorsApproximatelyEqual(profile.SocketLocalPosition, position)
                && CCS_EquipmentFitStudioPendingChange.VectorsApproximatelyEqual(profile.SocketLocalEulerAngles, eulerAngles)
                && CCS_EquipmentFitStudioPendingChange.VectorsApproximatelyEqual(profile.SocketLocalScale, scale);
        }

        public static CCS_SurvivalValidationResult ValidateCaptureSaveWorkflowRouting()
        {
            if (CCS_EquipmentFitStudioRevolverFitUtility.GetRevolverAttachmentFitProfilePath(
                    CCS_EquipmentConstants.HolsterSocketRightHipId)
                != CCS_EquipmentConstants.RevolverM1879RightHipHolsterFitPath)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Right hip holster socket must map to CCS_RevolverM1879_RightHipHolster_Fit.asset.");
            }

            if (CCS_EquipmentFitStudioRevolverFitUtility.GetRevolverAttachmentFitProfilePath(
                    CCS_EquipmentConstants.HandSocketRightId)
                != CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Right hand socket must map to CCS_RevolverM1879_RightHandEquipped_Fit.asset.");
            }

            CCS_EquipmentFitStudioPendingChange pending = new CCS_EquipmentFitStudioPendingChange();
            string errorMessage;
            if (TrySavePendingSocketCapture(pending, CCS_EquipmentConstants.HolsterSocketRightHipId, out _, out errorMessage))
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Save must be blocked when no pending capture exists.");
            }

            pending.CaptureFromBaseline(
                CCS_EquipmentConstants.HolsterSocketRightHipId,
                "TestProfile",
                Vector3.zero,
                Vector3.zero,
                Vector3.one,
                new Vector3(0.19f, -0.08f, -0.02f),
                new Vector3(73.01f, 1f, 349f),
                Vector3.one);

            if (!pending.HasCaptured || !pending.HasTransformChanges)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Capture must create a pending change with transform differences.");
            }

            return CCS_SurvivalValidationResult.Pass("Capture/save workflow routing validated.");
        }

        #endregion

        #region Private Methods

        private static string BuildSaveSuccessMessage(string socketId, string profileAssetName)
        {
            switch (socketId)
            {
                case CCS_EquipmentConstants.HolsterSocketRightHipId:
                    return "Saved Right Hip Holster fit profile (" + profileAssetName + ").";
                case CCS_EquipmentConstants.HandSocketRightId:
                    return "Saved Right Hand Equipped fit profile (" + profileAssetName + ").";
                default:
                    return "Saved revolver fit profile (" + profileAssetName + ").";
            }
        }

        #endregion
    }
}
