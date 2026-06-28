using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerLocalOwnerUiBootstrap
// CATEGORY: Modules / CharacterController / Runtime / UI
// PURPOSE: Owner-only gate for transitional PlayerLocalUI canvases on the network player.
// PLACEMENT: PlayerLocalUI child on production/test player prefabs.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: v0.8.0 transitional until HUD is scene-owned. Disables UI for remote players.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-400)]
    public sealed class CCS_PlayerLocalOwnerUiBootstrap : MonoBehaviour
    {
        #region Variables

        [SerializeField] private CCS_PlayerRuntimeFacade runtimeFacade;
        [SerializeField] private Canvas[] ownerOnlyCanvases;
        [SerializeField] private Behaviour[] ownerOnlyBehaviours;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveReferences();
            ApplyOwnerVisibility();
        }

        private void OnEnable()
        {
            ApplyOwnerVisibility();
        }

        #endregion

        #region Private Methods

        private void ResolveReferences()
        {
            if (runtimeFacade == null)
            {
                runtimeFacade = GetComponentInParent<CCS_PlayerRuntimeFacade>(true);
            }

            if (ownerOnlyCanvases == null || ownerOnlyCanvases.Length == 0)
            {
                ownerOnlyCanvases = GetComponentsInChildren<Canvas>(true);
            }
        }

        private void ApplyOwnerVisibility()
        {
            bool allowLocalUi = runtimeFacade == null || runtimeFacade.IsLocalOwner;

            if (ownerOnlyCanvases != null)
            {
                for (int canvasIndex = 0; canvasIndex < ownerOnlyCanvases.Length; canvasIndex++)
                {
                    Canvas canvas = ownerOnlyCanvases[canvasIndex];
                    if (canvas == null)
                    {
                        continue;
                    }

                    canvas.enabled = allowLocalUi;
                }
            }

            if (ownerOnlyBehaviours != null)
            {
                for (int behaviourIndex = 0; behaviourIndex < ownerOnlyBehaviours.Length; behaviourIndex++)
                {
                    Behaviour behaviour = ownerOnlyBehaviours[behaviourIndex];
                    if (behaviour == null || behaviour == this)
                    {
                        continue;
                    }

                    behaviour.enabled = allowLocalUi;
                }
            }

            if (!allowLocalUi)
            {
                gameObject.SetActive(false);
            }
        }

        #endregion
    }
}
