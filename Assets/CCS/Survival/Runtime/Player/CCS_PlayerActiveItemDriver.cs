using CCS.Modules.CharacterController;
using CCS.Modules.Hotbar;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerActiveItemDriver
// CATEGORY: Survival / Runtime / Player
// PURPOSE: Routes primary use input through CCS_ActiveItemService.
// PLACEMENT: PF_CCS_Player alongside CCS_PlayerCombatDriver.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Alpha1 cycles equipped active items. Combat driver remains for legacy fallback.
// =============================================================================

namespace CCS.Survival.Player
{
    [DefaultExecutionOrder(224)]
    public sealed class CCS_PlayerActiveItemDriver : MonoBehaviour
    {
        #region Variables

        [Header("Active Item Use")]
        [Tooltip("Camera used for active item use direction. Defaults to child camera.")]
        [SerializeField] private Camera useCamera;

        [Header("Selection")]
        [Tooltip("When enabled, Alpha1 cycles the active item across occupied equipment slots.")]
        [SerializeField] private bool enableCycleHotkey = true;

        private CCS_CharacterInputActionProvider inputProvider;
        private CCS_ActiveItemService activeItemService;
        private bool serviceResolved;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            inputProvider = GetComponent<CCS_CharacterInputActionProvider>();
            if (useCamera == null)
            {
                useCamera = GetComponentInChildren<Camera>();
            }
        }

        private void Update()
        {
            if (!serviceResolved)
            {
                serviceResolved = CCS_ActiveItemRuntimeBridge.TryGetActiveItemService(out activeItemService)
                    && activeItemService != null
                    && activeItemService.IsInitialized;
            }

            if (!serviceResolved || inputProvider == null)
            {
                return;
            }

            if (enableCycleHotkey && CCS_KeyboardInputUtility.WasKeyPressedThisFrame(KeyCode.Alpha1))
            {
                activeItemService.CycleActiveEquippedItem();
            }

            if (CCS_KeyboardInputUtility.WasKeyPressedThisFrame(KeyCode.R)
                && activeItemService.ActiveState.BehaviorType == CCS_ActiveItemBehaviorType.Firearm)
            {
                activeItemService.TryReloadActiveFirearm();
            }

            if (!inputProvider.PrimaryActionPressedThisFrame || useCamera == null)
            {
                return;
            }

            Transform cameraTransform = useCamera.transform;
            CCS_ActiveItemUseRequest request = new CCS_ActiveItemUseRequest(
                cameraTransform.position,
                cameraTransform.forward);
            activeItemService.TryUseActiveItem(request);
        }

        #endregion
    }
}
