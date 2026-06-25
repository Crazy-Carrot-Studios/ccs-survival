using CCS.Modules.Weapons;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioPreviewItem
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Spawns editor-only zeroed preview visuals under equipment sockets.
// PLACEMENT: Editor utility used by Equipment Fit Studio window.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Uses ModelRoot/RevolverVisual only. Never saved to scene or prefab.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public sealed class CCS_EquipmentFitStudioPreviewItem
    {
        #region Variables

        private GameObject previewRoot;

        #endregion

        #region Properties

        public GameObject PreviewRoot => previewRoot;

        public bool IsSpawned => previewRoot != null;

        public bool IsZeroed =>
            previewRoot != null
            && previewRoot.transform.localPosition == Vector3.zero
            && previewRoot.transform.localRotation == Quaternion.identity
            && previewRoot.transform.localScale == Vector3.one;

        #endregion

        #region Public Methods

        public bool TrySpawnUnderAttachmentRoot(
            Transform attachmentRoot,
            GameObject previewSourcePrefab,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            DestroyPreview();
            if (attachmentRoot == null)
            {
                errorMessage = "Preview failed: preview attachment root not found.";
                return false;
            }

            GameObject visualSource = CCS_EquipmentFitStudioVisualSourceUtility.ResolveRevolverPreviewVisualSource(previewSourcePrefab);
            if (visualSource == null)
            {
                errorMessage = "Preview failed: ModelRoot/RevolverVisual not found on source prefab.";
                return false;
            }

            previewRoot = Object.Instantiate(visualSource);
            previewRoot.name = CCS_EquipmentConstants.EditorPreviewItemObjectName;
            previewRoot.hideFlags = HideFlags.DontSave;
            previewRoot.transform.SetParent(attachmentRoot, false);
            CCS_EquipmentFitStudioVisualSourceUtility.StripGameplayComponents(previewRoot);
            ResetPreviewItemToZero();
            return true;
        }

        public bool TrySpawnUnderSocket(
            Transform socketTransform,
            GameObject previewSourcePrefab,
            out string errorMessage)
        {
            errorMessage = "Spawn preview under attachment root. Resolve attachment root before calling TrySpawnUnderAttachmentRoot.";
            if (socketTransform == null)
            {
                errorMessage = "Preview failed: socket transform not found.";
                return false;
            }

            return false;
        }

        public bool SpawnUnderAttachmentRoot(Transform attachmentRoot, GameObject previewSourcePrefab)
        {
            return TrySpawnUnderAttachmentRoot(attachmentRoot, previewSourcePrefab, out _);
        }

        public bool SpawnUnderSocket(Transform socketTransform, GameObject previewSourcePrefab)
        {
            return TrySpawnUnderSocket(socketTransform, previewSourcePrefab, out _);
        }

        public void ResetPreviewItemToZero()
        {
            if (previewRoot == null)
            {
                return;
            }

            previewRoot.transform.localPosition = Vector3.zero;
            previewRoot.transform.localRotation = Quaternion.identity;
            previewRoot.transform.localScale = Vector3.one;
        }

        public void EnforceZeroedTransform()
        {
            if (previewRoot == null)
            {
                return;
            }

            if (!IsZeroed)
            {
                ResetPreviewItemToZero();
            }
        }

        public void DestroyPreview()
        {
            if (previewRoot != null)
            {
                Object.DestroyImmediate(previewRoot);
                previewRoot = null;
            }
        }

        #endregion
    }
}
