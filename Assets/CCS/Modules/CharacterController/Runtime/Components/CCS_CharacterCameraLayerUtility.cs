using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterCameraLayerUtility
// CATEGORY: Modules / CharacterController / Runtime / Components
// PURPOSE: Runtime helpers for local FP head/body layers and camera culling masks.
// PLACEMENT: Shared runtime utility used by camera and head visibility components.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Editor builders ensure TagManager layer slots exist.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_CharacterCameraLayerUtility
    {
        #region Public Methods

        public static int GetLocalSelfHeadHiddenLayer()
        {
            return LayerMask.NameToLayer(CCS_CharacterControllerConstants.LocalSelfHeadHiddenLayerName);
        }

        public static int GetLocalFirstPersonBodyLayer()
        {
            return LayerMask.NameToLayer(CCS_CharacterControllerConstants.LocalFirstPersonBodyLayerName);
        }

        public static int GetLocalSelfHeadHiddenLayerMask()
        {
            int layer = GetLocalSelfHeadHiddenLayer();
            return layer >= 0 ? 1 << layer : 0;
        }

        public static int GetLocalFirstPersonBodyLayerMask()
        {
            int layer = GetLocalFirstPersonBodyLayer();
            return layer >= 0 ? 1 << layer : 0;
        }

        public static LayerMask BuildDefaultOutputCameraCullingMask()
        {
            return ~0;
        }

        public static LayerMask BuildFirstPersonBodyAwareCullingMask(LayerMask baseMask)
        {
            int hiddenLayerMask = GetLocalSelfHeadHiddenLayerMask();
            if (hiddenLayerMask == 0)
            {
                return baseMask;
            }

            LayerMask result = baseMask;
            result.value &= ~hiddenLayerMask;
            return result;
        }

        public static LayerMask BuildCameraCullingMaskExcludingLocalSelfHead(LayerMask baseMask)
        {
            return BuildFirstPersonBodyAwareCullingMask(baseMask);
        }

        public static bool DoesMaskIncludeLocalFirstPersonBody(LayerMask layerMask)
        {
            int firstPersonBodyMask = GetLocalFirstPersonBodyLayerMask();
            if (firstPersonBodyMask == 0)
            {
                return true;
            }

            return (layerMask.value & firstPersonBodyMask) != 0;
        }

        #endregion
    }
}
