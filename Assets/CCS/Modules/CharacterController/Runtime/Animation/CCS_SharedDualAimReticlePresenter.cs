using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_SharedDualAimReticlePresenter
// CATEGORY: Modules / CharacterController / Runtime / Animation
// PURPOSE: Projects SharedDualAimPoint to screen-space reticle during aim presentation.
// PLACEMENT: WeaponHudRoot on PF_CCS_CharacterController_Player_Networked.
// AUTHOR: James Schilz
// CREATED: 2026-07-02
// NOTES: Visual-only. Uses registry world point; no muzzle LOS or aim target resolver.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [DefaultExecutionOrder(200)]
    public sealed class CCS_SharedDualAimReticlePresenter : MonoBehaviour
    {
        [SerializeField] private bool showSharedDualAimReticle = true;
        [SerializeField] private RectTransform reticleRectTransform;
        [SerializeField] private Image reticleImage;
        [SerializeField] private Camera aimCamera;

        private void Awake()
        {
            if (aimCamera == null)
            {
                aimCamera = Camera.main;
            }

            SetReticleVisible(false);
        }

        private void LateUpdate()
        {
            if (!showSharedDualAimReticle
                || reticleRectTransform == null
                || !CCS_SharedDualAimPointRegistry.IsActive)
            {
                SetReticleVisible(false);
                return;
            }

            if (aimCamera == null)
            {
                aimCamera = Camera.main;
            }

            if (aimCamera == null)
            {
                SetReticleVisible(false);
                return;
            }

            Vector3 worldPoint = CCS_SharedDualAimPointRegistry.WorldPosition;
            Vector3 screenPoint = aimCamera.WorldToScreenPoint(worldPoint);
            if (screenPoint.z <= 0f)
            {
                SetReticleVisible(false);
                return;
            }

            reticleRectTransform.position = screenPoint;
            SetReticleVisible(true);
        }

        private void SetReticleVisible(bool visible)
        {
            if (reticleImage != null)
            {
                reticleImage.enabled = visible;
            }

            if (reticleRectTransform != null)
            {
                reticleRectTransform.gameObject.SetActive(visible);
            }
        }
    }
}
