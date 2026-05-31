using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SaveLoadDebugController
// CATEGORY: Modules / SaveLoad / Runtime / Testing
// PURPOSE: Developer-facing manual save/load/delete hooks for bootstrap verification.
// PLACEMENT: Bootstrap verification scenes only. Disable for shipping builds.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Wraps CCS_SaveLoadService. No final player-facing save menu in 0.6.1.
// =============================================================================

namespace CCS.Modules.SaveLoad
{
    [DefaultExecutionOrder(280)]
    public sealed class CCS_SaveLoadDebugController : MonoBehaviour
    {
        private const string LogPrefix = "[CCS_SaveLoadDebugController]";

        #region Variables

        [Header("Development Testing")]
        [Tooltip("When enabled, manual save/load/delete hooks are active.")]
        [SerializeField] private bool enableDebugControls = true;

        [Tooltip("Default slot id used by manual save/load/delete requests.")]
        [SerializeField] private string selectedSlotId = "slot_01";

        private readonly List<CCS_SaveSlotData> listedSaveSlots = new List<CCS_SaveSlotData>();

        private CCS_SaveLoadService saveLoadService;
        private CCS_SaveLoadResult lastResult;
        private CCS_SaveLoadDebugState cachedDebugState;
        private string lastOperationSummary = "Ready.";

        #endregion

        #region Events

        public event Action DebugStateChanged;

        #endregion

        #region Properties

        public bool EnableDebugControls => enableDebugControls;

        public string SelectedSlotId => selectedSlotId;

        public CCS_SaveLoadResult LastResult => lastResult;

        public IReadOnlyList<CCS_SaveSlotData> ListedSaveSlots => listedSaveSlots;

        public CCS_SaveLoadDebugState CachedDebugState => cachedDebugState;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            if (!enableDebugControls)
            {
                UpdateCachedDebugState("Debug controls disabled.");
                return;
            }

            if (!TryResolveSaveLoadService())
            {
                UpdateCachedDebugState("Save/load service unavailable.");
                return;
            }

            BindServiceEvents();
            RefreshSaveSlotListing();
            UpdateCachedDebugState("Debug controller initialized.");
            Debug.Log($"{LogPrefix} Manual save/load hooks ready for slot '{selectedSlotId}'.");
        }

        private void OnDestroy()
        {
            UnbindServiceEvents();
            saveLoadService = null;
        }

        #endregion

        #region Public Methods

        public CCS_SaveLoadDebugState GetDebugState()
        {
            return cachedDebugState ?? BuildDebugState(lastOperationSummary);
        }

        public void SetSelectedSlotId(string slotId)
        {
            if (string.IsNullOrWhiteSpace(slotId))
            {
                return;
            }

            selectedSlotId = CCS_SavePathUtility.SanitizeSlotId(slotId);
            UpdateCachedDebugState($"Selected slot set to '{selectedSlotId}'.");
        }

        public CCS_SaveLoadResult ManualSaveSelectedSlot()
        {
            return ExecuteManualOperation("Save", () => saveLoadService.TryCreateSave(selectedSlotId));
        }

        public CCS_SaveLoadResult ManualLoadSelectedSlot()
        {
            return ExecuteManualOperation("Load", () => saveLoadService.TryLoadSave(selectedSlotId));
        }

        public CCS_SaveLoadResult ManualDeleteSelectedSlot()
        {
            return ExecuteManualOperation("Delete", () => saveLoadService.TryDeleteSaveSlot(selectedSlotId));
        }

        public void RefreshSaveSlotListing()
        {
            listedSaveSlots.Clear();

            if (!TryResolveSaveLoadService())
            {
                UpdateCachedDebugState("Save/load service unavailable.");
                return;
            }

            IReadOnlyList<CCS_SaveSlotData> slots = saveLoadService.EnumerateSaveSlots();
            for (int index = 0; index < slots.Count; index++)
            {
                listedSaveSlots.Add(slots[index]);
            }

            UpdateCachedDebugState($"Listed {listedSaveSlots.Count} save slot(s).");
        }

        public void SelectNextSlot()
        {
            CycleSelectedSlot(1);
        }

        public void SelectPreviousSlot()
        {
            CycleSelectedSlot(-1);
        }

        #endregion

        #region Private Methods

        private CCS_SaveLoadResult ExecuteManualOperation(
            string operationName,
            Func<CCS_SaveLoadResult> operation)
        {
            if (!enableDebugControls)
            {
                return RecordResult(CCS_SaveLoadResult.Failure("Debug controls are disabled.", selectedSlotId));
            }

            if (!TryResolveSaveLoadService())
            {
                return RecordResult(CCS_SaveLoadResult.Failure("Save/load service unavailable.", selectedSlotId));
            }

            CCS_SaveLoadResult result = operation != null
                ? operation.Invoke()
                : CCS_SaveLoadResult.Failure("Manual operation delegate was null.", selectedSlotId);

            RefreshSaveSlotListing();

            string summaryPrefix = result.IsSuccess ? "Succeeded" : "Failed";
            string message = string.IsNullOrWhiteSpace(result.Message) ? operationName : result.Message;
            UpdateCachedDebugState($"{operationName} {summaryPrefix}: {message}");
            Debug.Log(result.IsSuccess
                ? $"{LogPrefix} {operationName} succeeded for slot '{result.SlotId}': {result.Message}"
                : $"{LogPrefix} {operationName} failed for slot '{result.SlotId}': {result.Message}");

            return RecordResult(result);
        }

        private CCS_SaveLoadResult RecordResult(CCS_SaveLoadResult result)
        {
            lastResult = result ?? CCS_SaveLoadResult.Failure("Result was null.", selectedSlotId);
            return lastResult;
        }

        private void CycleSelectedSlot(int direction)
        {
            if (direction == 0)
            {
                return;
            }

            RefreshSaveSlotListing();

            if (listedSaveSlots.Count == 0)
            {
                UpdateCachedDebugState("No save slots available to cycle.");
                return;
            }

            int currentIndex = FindSelectedSlotIndex();
            if (currentIndex < 0)
            {
                currentIndex = 0;
            }

            int nextIndex = (currentIndex + direction + listedSaveSlots.Count) % listedSaveSlots.Count;
            selectedSlotId = listedSaveSlots[nextIndex].SlotId;
            UpdateCachedDebugState($"Selected slot cycled to '{selectedSlotId}'.");
        }

        private int FindSelectedSlotIndex()
        {
            for (int index = 0; index < listedSaveSlots.Count; index++)
            {
                if (string.Equals(listedSaveSlots[index].SlotId, selectedSlotId, StringComparison.Ordinal))
                {
                    return index;
                }
            }

            return -1;
        }

        private bool TryResolveSaveLoadService()
        {
            if (saveLoadService != null && saveLoadService.IsInitialized)
            {
                return true;
            }

            if (!CCS_SaveLoadRuntimeBridge.TryGetSaveLoadService(out saveLoadService)
                || saveLoadService == null
                || !saveLoadService.IsInitialized)
            {
                saveLoadService = null;
                return false;
            }

            return true;
        }

        private void BindServiceEvents()
        {
            if (saveLoadService == null)
            {
                return;
            }

            saveLoadService.SaveCompleted += HandleSaveLoadServiceEvent;
            saveLoadService.SaveFailed += HandleSaveLoadServiceEventFailure;
            saveLoadService.LoadCompleted += HandleSaveLoadServiceEvent;
            saveLoadService.LoadFailed += HandleSaveLoadServiceEventFailure;
        }

        private void UnbindServiceEvents()
        {
            if (saveLoadService == null)
            {
                return;
            }

            saveLoadService.SaveCompleted -= HandleSaveLoadServiceEvent;
            saveLoadService.SaveFailed -= HandleSaveLoadServiceEventFailure;
            saveLoadService.LoadCompleted -= HandleSaveLoadServiceEvent;
            saveLoadService.LoadFailed -= HandleSaveLoadServiceEventFailure;
        }

        private void HandleSaveLoadServiceEvent(CCS_SaveLoadEventArgs eventArgs)
        {
            if (eventArgs == null)
            {
                return;
            }

            UpdateCachedDebugState(eventArgs.Message);
        }

        private void HandleSaveLoadServiceEventFailure(CCS_SaveLoadEventArgs eventArgs)
        {
            if (eventArgs == null)
            {
                UpdateCachedDebugState("Save/load operation failed.");
                return;
            }

            UpdateCachedDebugState($"Failed: {eventArgs.Message}");
        }

        private void UpdateCachedDebugState(string operationSummary)
        {
            lastOperationSummary = string.IsNullOrWhiteSpace(operationSummary)
                ? "Ready."
                : operationSummary;

            cachedDebugState = BuildDebugState(lastOperationSummary);
            DebugStateChanged?.Invoke();
        }

        private CCS_SaveLoadDebugState BuildDebugState(string operationSummary)
        {
            return new CCS_SaveLoadDebugState(
                selectedSlotId,
                BuildListedSaveSlotsSummary(),
                operationSummary,
                CCS_SavePathUtility.GetShortDisplayPath(),
                saveLoadService != null && saveLoadService.IsInitialized,
                saveLoadService != null
                    && saveLoadService.IsSaveableRegistered(CCS_SaveLoadSaveableIds.PlayerInventory),
                saveLoadService != null
                    && saveLoadService.IsSaveableRegistered(CCS_SaveLoadSaveableIds.PlayerEquipment));
        }

        private string BuildListedSaveSlotsSummary()
        {
            if (listedSaveSlots.Count == 0)
            {
                return "None";
            }

            StringBuilder builder = new StringBuilder(listedSaveSlots.Count * 12);
            for (int index = 0; index < listedSaveSlots.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(listedSaveSlots[index].SlotId);
            }

            return builder.ToString();
        }

        #endregion
    }
}
