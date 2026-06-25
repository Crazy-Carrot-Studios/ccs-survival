using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioAutoLoadUtility
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Auto-loads editor preview player, pose, profile, weapon, and camera.
// PLACEMENT: Editor utility invoked by revamped Fit Studio window.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Editor Mode only. User should not click multiple setup buttons before editing.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public static class CCS_EquipmentFitStudioAutoLoadUtility
    {
        public static bool TryAutoLoadPreviewSetup(
            CCS_EquipmentFitStudioSelectionState state,
            CCS_EquipmentFitStudioSettings settings,
            CCS_EquipmentFitStudioPreviewItem previewItem,
            CCS_EquipmentFitStudioPreviewCamera previewCamera,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            if (EditorApplication.isPlaying)
            {
                errorMessage = "Auto-load preview is Editor Mode only.";
                return false;
            }

            if (state == null || previewItem == null || previewCamera == null)
            {
                errorMessage = "Fit Studio preview state is not initialized.";
                return false;
            }

            CCS_EquipmentFitStudioFitTargetRoute route =
                CCS_EquipmentFitStudioFitTargetRoutingUtility.Resolve(state.FitTarget);
            state.SelectedSocketId = route.SocketId;
            state.SelectedWeaponId = route.WeaponId;
            state.PosePreviewMode = route.PoseMode;
            state.UserManuallySelectedPosePreview = false;

            if (!CCS_EquipmentFitStudioPreviewPlayerUtility.CreateOrRefreshPreviewPlayer(
                    out GameObject previewPlayer,
                    out string previewError))
            {
                errorMessage = previewError;
                return false;
            }

            state.PlayerRoot = previewPlayer;
            state.UsesEditorPreviewPlayer = true;

            state.SelectedAttachmentFitProfile =
                CCS_EquipmentFitProfilePersistenceUtility.LoadProfileFromDisk(route.ProfilePath);
            if (state.SelectedAttachmentFitProfile == null)
            {
                errorMessage = "Missing profile asset at " + route.ProfilePath;
                return false;
            }

            Transform socketTransform = GetSocketTransform(state.PlayerRoot, route.SocketId);
            if (socketTransform == null)
            {
                errorMessage = "Preview player is missing socket " + route.SocketId;
                return false;
            }

            CCS_EquipmentFitStudioRevolverFitUtility.ApplyRevolverAttachmentFitProfile(
                route.SocketId,
                state.PlayerRoot,
                route.FitTarget);

            CCS_EquipmentFitStudioPosePreviewUtility.TryApplyPosePreview(
                state.PlayerRoot,
                route.PoseMode,
                out string poseError);
            state.LastPosePreviewError = poseError;

            string attachmentRootName =
                CCS_EquipmentFitStudioPreviewAttachmentUtility.GetAttachmentRootObjectName(route.FitTarget);
            Transform attachmentRoot = CCS_EquipmentFitStudioPreviewAttachmentUtility.EnsurePreviewAttachmentRoot(
                socketTransform,
                attachmentRootName);

            previewItem.DestroyPreview();
            GameObject source = settings != null ? settings.DefaultPreviewWeaponPrefab : null;
            if (!previewItem.TrySpawnUnderAttachmentRoot(attachmentRoot, source, out string spawnError))
            {
                errorMessage = spawnError;
                return false;
            }

            state.PreviewItemSpawned = true;
            state.LastPreviewError = string.Empty;
            previewItem.EnforceZeroedTransform();

            Transform frameTarget = previewItem.PreviewRoot != null
                ? previewItem.PreviewRoot.transform
                : socketTransform;
            previewCamera.EnsureCamera(settings);
            previewCamera.ApplyPreset(route.DefaultCameraPreset, state.PlayerRoot, route.SocketId);
            previewCamera.FrameTransform(frameTarget, route.FitTarget == CCS_EquipmentFitStudioFitTarget.EquippedItem ? 1.2f : 1.35f);

            state.ClearPendingChanges();
            state.ClearWorkflowSessionFlags();
            return true;
        }

        private static Transform GetSocketTransform(GameObject playerRoot, string socketId)
        {
            if (playerRoot == null)
            {
                return null;
            }

            CCS_EquipmentSocketRegistry registry = playerRoot.GetComponent<CCS_EquipmentSocketRegistry>();
            if (registry != null && registry.TryGetSocket(socketId, out Transform socketTransform))
            {
                return socketTransform;
            }

            return null;
        }
    }
}
