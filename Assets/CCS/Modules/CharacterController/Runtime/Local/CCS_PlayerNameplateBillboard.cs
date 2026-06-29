using TMPro;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerNameplateBillboard
// CATEGORY: Modules / CharacterController / Tests / Runtime
// PURPOSE: Local-client nameplate visibility and camera-facing billboard rotation.
// PLACEMENT: PF_CCS_CharacterController_Player_Networked / NameplateRoot.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Hides local owner nameplates without disabling the player root.
//        Remote clients keep nameplate renderers enabled and billboard rotation active.
// =============================================================================

namespace CCS.Modules.CharacterController {
    [DefaultExecutionOrder(-100)]
    public sealed class CCS_PlayerNameplateBillboard : MonoBehaviour
    {
        #region Variables

        [SerializeField] private bool flipFacing = true;

        private Camera cachedCamera;
        private bool localNameplateVisible;
        private Renderer[] cachedRenderers;
        private TMP_Text cachedNameplateText;

        #endregion

        #region Properties

        public bool IsLocalNameplateVisible => localNameplateVisible;

        #endregion

        #region Public Methods

        public void SetLocalNameplateVisible(bool visible)
        {
            localNameplateVisible = visible;
            ApplyRendererVisibility(visible);
        }

        public void ApplyNameplateVisibility(bool isLocalOwner)
        {
            SetLocalNameplateVisible(!isLocalOwner);
        }

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            CacheNameplateRenderers();
        }

        private void LateUpdate()
        {
            if (!localNameplateVisible)
            {
                return;
            }

            Camera targetCamera = ResolveCamera();
            if (targetCamera == null)
            {
                return;
            }

            Vector3 lookDirection = transform.position - targetCamera.transform.position;
            if (lookDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(
                flipFacing ? lookDirection.normalized : -lookDirection.normalized,
                Vector3.up);
        }

        #endregion

        #region Private Methods

        private void CacheNameplateRenderers()
        {
            cachedRenderers = GetComponentsInChildren<Renderer>(true);
            cachedNameplateText = GetComponentInChildren<TMP_Text>(true);
        }

        private void ApplyRendererVisibility(bool visible)
        {
            if (cachedRenderers == null || cachedNameplateText == null)
            {
                CacheNameplateRenderers();
            }

            if (cachedRenderers != null)
            {
                for (int i = 0; i < cachedRenderers.Length; i++)
                {
                    Renderer renderer = cachedRenderers[i];
                    if (renderer != null)
                    {
                        renderer.enabled = visible;
                    }
                }
            }

            if (cachedNameplateText != null)
            {
                cachedNameplateText.enabled = visible;
            }
        }

        private Camera ResolveCamera()
        {
            if (cachedCamera != null && cachedCamera.isActiveAndEnabled)
            {
                return cachedCamera;
            }

            cachedCamera = Camera.main;
            return cachedCamera;
        }

        #endregion
    }
}
