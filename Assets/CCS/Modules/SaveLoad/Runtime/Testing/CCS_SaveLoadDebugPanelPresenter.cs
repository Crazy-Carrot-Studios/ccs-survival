using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_SaveLoadDebugPanelPresenter
// CATEGORY: Modules / SaveLoad / Runtime / Testing
// PURPOSE: Minimal developer save/load panel bound to CCS_SaveLoadDebugController.
// PLACEMENT: Child of PF_CCS_HUD_Root canvas in bootstrap verification scenes.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Read-only display plus manual trigger buttons. Not final player save UI.
// =============================================================================

namespace CCS.Modules.SaveLoad
{
    public sealed class CCS_SaveLoadDebugPanelPresenter : MonoBehaviour
    {
        #region Variables

        [Header("Controller")]
        [Tooltip("Debug controller that owns manual save/load/delete operations.")]
        [SerializeField] private CCS_SaveLoadDebugController debugController;

        [Header("Display")]
        [Tooltip("Displays selected slot, listed slots, status, and save path.")]
        [SerializeField] private Text statusText;

        [Header("Manual Controls")]
        [Tooltip("Triggers ManualSaveSelectedSlot on the debug controller.")]
        [SerializeField] private Button saveButton;

        [Tooltip("Triggers ManualLoadSelectedSlot on the debug controller.")]
        [SerializeField] private Button loadButton;

        [Tooltip("Triggers ManualDeleteSelectedSlot on the debug controller.")]
        [SerializeField] private Button deleteButton;

        [Tooltip("Triggers RefreshSaveSlotListing on the debug controller.")]
        [SerializeField] private Button refreshButton;

        [Tooltip("Triggers SelectPreviousSlot on the debug controller.")]
        [SerializeField] private Button previousSlotButton;

        [Tooltip("Triggers SelectNextSlot on the debug controller.")]
        [SerializeField] private Button nextSlotButton;

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            BindControls();
            RefreshDisplay();
        }

        private void OnDisable()
        {
            UnbindControls();
        }

        private void OnDestroy()
        {
            UnbindControls();
        }

        #endregion

        #region Public Methods

        public void BindController(CCS_SaveLoadDebugController controller)
        {
            UnbindControllerEvents();
            debugController = controller;
            BindControllerEvents();
            BindControls();
            RefreshDisplay();
        }

        public void RefreshDisplay()
        {
            if (statusText == null)
            {
                return;
            }

            if (debugController == null || !debugController.EnableDebugControls)
            {
                statusText.text = "Save Debug\nDisabled";
                SetButtonsInteractable(false);
                return;
            }

            CCS_SaveLoadDebugState debugState = debugController.GetDebugState();
            SetButtonsInteractable(debugState.IsServiceReady);

            statusText.text =
                "Save Debug\n" +
                $"Slot: {debugState.SelectedSlotId}\n" +
                $"Slots: {debugState.ListedSaveSlotsSummary}\n" +
                $"Status: {debugState.LastOperationSummary}\n" +
                $"Inv Save: {(debugState.IsInventorySaveRegistered ? "Yes" : "No")}\n" +
                $"Eq Save: {(debugState.IsEquipmentSaveRegistered ? "Yes" : "No")}\n" +
                $"Path: {debugState.ShortenedSavePath}";
        }

        #endregion

        #region Private Methods

        private void BindControls()
        {
            UnbindButtonHandlers();

            if (saveButton != null)
            {
                saveButton.onClick.AddListener(HandleSaveButtonClicked);
            }

            if (loadButton != null)
            {
                loadButton.onClick.AddListener(HandleLoadButtonClicked);
            }

            if (deleteButton != null)
            {
                deleteButton.onClick.AddListener(HandleDeleteButtonClicked);
            }

            if (refreshButton != null)
            {
                refreshButton.onClick.AddListener(HandleRefreshButtonClicked);
            }

            if (previousSlotButton != null)
            {
                previousSlotButton.onClick.AddListener(HandlePreviousSlotButtonClicked);
            }

            if (nextSlotButton != null)
            {
                nextSlotButton.onClick.AddListener(HandleNextSlotButtonClicked);
            }

            BindControllerEvents();
        }

        private void UnbindControls()
        {
            UnbindButtonHandlers();
            UnbindControllerEvents();
        }

        private void BindControllerEvents()
        {
            if (debugController == null)
            {
                return;
            }

            debugController.DebugStateChanged += HandleDebugStateChanged;
        }

        private void UnbindControllerEvents()
        {
            if (debugController == null)
            {
                return;
            }

            debugController.DebugStateChanged -= HandleDebugStateChanged;
        }

        private void UnbindButtonHandlers()
        {
            if (saveButton != null)
            {
                saveButton.onClick.RemoveListener(HandleSaveButtonClicked);
            }

            if (loadButton != null)
            {
                loadButton.onClick.RemoveListener(HandleLoadButtonClicked);
            }

            if (deleteButton != null)
            {
                deleteButton.onClick.RemoveListener(HandleDeleteButtonClicked);
            }

            if (refreshButton != null)
            {
                refreshButton.onClick.RemoveListener(HandleRefreshButtonClicked);
            }

            if (previousSlotButton != null)
            {
                previousSlotButton.onClick.RemoveListener(HandlePreviousSlotButtonClicked);
            }

            if (nextSlotButton != null)
            {
                nextSlotButton.onClick.RemoveListener(HandleNextSlotButtonClicked);
            }
        }

        private void HandleSaveButtonClicked()
        {
            debugController?.ManualSaveSelectedSlot();
            RefreshDisplay();
        }

        private void HandleLoadButtonClicked()
        {
            debugController?.ManualLoadSelectedSlot();
            RefreshDisplay();
        }

        private void HandleDeleteButtonClicked()
        {
            debugController?.ManualDeleteSelectedSlot();
            RefreshDisplay();
        }

        private void HandleRefreshButtonClicked()
        {
            debugController?.RefreshSaveSlotListing();
            RefreshDisplay();
        }

        private void HandlePreviousSlotButtonClicked()
        {
            debugController?.SelectPreviousSlot();
            RefreshDisplay();
        }

        private void HandleNextSlotButtonClicked()
        {
            debugController?.SelectNextSlot();
            RefreshDisplay();
        }

        private void HandleDebugStateChanged()
        {
            RefreshDisplay();
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (saveButton != null)
            {
                saveButton.interactable = interactable;
            }

            if (loadButton != null)
            {
                loadButton.interactable = interactable;
            }

            if (deleteButton != null)
            {
                deleteButton.interactable = interactable;
            }

            if (refreshButton != null)
            {
                refreshButton.interactable = interactable;
            }

            if (previousSlotButton != null)
            {
                previousSlotButton.interactable = interactable;
            }

            if (nextSlotButton != null)
            {
                nextSlotButton.interactable = interactable;
            }
        }

        #endregion
    }
}
