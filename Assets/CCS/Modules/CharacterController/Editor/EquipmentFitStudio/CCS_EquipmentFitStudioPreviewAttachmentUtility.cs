using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioPreviewAttachmentUtility
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Editor preview attachment-root apply/capture aligned with runtime applicator.
// PLACEMENT: Editor utility used by Fit Studio preview and save workflow.
// AUTHOR: James Schilz
// CREATED: 2026-06-24
// NOTES: Socket stays at definition; profile offset lives on attachment root.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public static class CCS_EquipmentFitStudioPreviewAttachmentUtility
    {
        #region Public Methods

        public static string GetAttachmentRootObjectName(CCS_EquipmentFitStudioFitTarget fitTarget)
        {
            return fitTarget == CCS_EquipmentFitStudioFitTarget.EquippedItem
                ? CCS_EquipmentConstants.RightHandRevolverAttachmentOffsetObjectName
                : CCS_EquipmentConstants.RuntimeHolsterAttachmentRootObjectName;
        }

        public static Transform FindRightHandAttachmentOffsetRoot(Transform socketTransform)
        {
            if (socketTransform == null)
            {
                return null;
            }

            Transform attachmentRoot = socketTransform.Find(
                CCS_EquipmentConstants.RightHandRevolverAttachmentOffsetObjectName);
            if (attachmentRoot != null)
            {
                return attachmentRoot;
            }

            return socketTransform.Find(CCS_EquipmentConstants.LegacyRuntimeEquippedAttachmentRootObjectName);
        }

        public static Transform EnsurePreviewAttachmentRoot(Transform socketTransform, string attachmentRootName)
        {
            return CCS_EquipmentFitStudioTestAttachmentUtility.EnsureEditorAttachmentRoot(
                socketTransform,
                attachmentRootName);
        }

        public static Transform FindPreviewAttachmentRoot(
            GameObject playerRoot,
            string socketId,
            string attachmentRootName)
        {
            if (playerRoot == null)
            {
                return null;
            }

            CCS_EquipmentSocketRegistry registry = playerRoot.GetComponent<CCS_EquipmentSocketRegistry>();
            if (registry == null || !registry.TryGetSocket(socketId, out Transform socketTransform))
            {
                return null;
            }

            return socketTransform.Find(attachmentRootName);
        }

        public static bool TryApplyProfileToPreviewAttachment(
            GameObject playerRoot,
            Transform socketTransform,
            string socketId,
            CCS_WeaponAttachmentFitProfile profile,
            string attachmentRootName)
        {
            if (playerRoot == null || socketTransform == null || profile == null)
            {
                return false;
            }

            if (!CCS_EquipmentFitStudioTestAttachmentUtility.TryGetSocketDefinition(
                    playerRoot.GetComponent<CCS_EquipmentSocketRegistry>(),
                    socketId,
                    out Vector3 definitionPosition,
                    out Vector3 definitionEuler,
                    out Vector3 definitionScale))
            {
                return false;
            }

            Transform attachmentRoot = EnsurePreviewAttachmentRoot(socketTransform, attachmentRootName);
            CCS_WeaponAttachmentFitProfileApplicator.ApplyProfileToAttachmentRoot(
                socketTransform,
                attachmentRoot,
                profile,
                definitionPosition,
                definitionEuler,
                definitionScale);
            return true;
        }

        public static bool TryCaptureProfileFromPreviewAttachment(
            GameObject playerRoot,
            string socketId,
            Transform attachmentRoot,
            out Vector3 profileSocketLocalPosition,
            out Vector3 profileSocketLocalEuler,
            out Vector3 profileSocketLocalScale)
        {
            profileSocketLocalPosition = Vector3.zero;
            profileSocketLocalEuler = Vector3.zero;
            profileSocketLocalScale = Vector3.one;

            if (playerRoot == null || attachmentRoot == null)
            {
                return false;
            }

            CCS_EquipmentSocketRegistry registry = playerRoot.GetComponent<CCS_EquipmentSocketRegistry>();
            if (!CCS_EquipmentFitStudioTestAttachmentUtility.TryGetSocketDefinition(
                    registry,
                    socketId,
                    out Vector3 definitionPosition,
                    out Vector3 definitionEuler,
                    out Vector3 definitionScale))
            {
                return false;
            }

            CCS_WeaponAttachmentFitProfileApplicator.ComputeProfileSocketLocalFromAttachmentRoot(
                definitionPosition,
                definitionEuler,
                definitionScale,
                attachmentRoot.localPosition,
                attachmentRoot.localEulerAngles,
                attachmentRoot.localScale,
                out profileSocketLocalPosition,
                out profileSocketLocalEuler,
                out profileSocketLocalScale);
            return true;
        }

        public static bool AttachmentRootMatchesProfile(
            GameObject playerRoot,
            string socketId,
            Transform attachmentRoot,
            CCS_WeaponAttachmentFitProfile profile)
        {
            if (playerRoot == null || attachmentRoot == null || profile == null)
            {
                return false;
            }

            CCS_EquipmentSocketRegistry registry = playerRoot.GetComponent<CCS_EquipmentSocketRegistry>();
            if (!CCS_EquipmentFitStudioTestAttachmentUtility.TryGetSocketDefinition(
                    registry,
                    socketId,
                    out Vector3 definitionPosition,
                    out Vector3 definitionEuler,
                    out Vector3 definitionScale))
            {
                return false;
            }

            return CCS_WeaponAttachmentFitProfileApplicator.AttachmentRootMatchesProfile(
                attachmentRoot,
                profile,
                definitionPosition,
                definitionEuler,
                definitionScale);
        }

        #endregion
    }
}
