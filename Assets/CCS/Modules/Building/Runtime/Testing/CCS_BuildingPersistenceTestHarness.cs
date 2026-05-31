using System.Collections.Generic;
using CCS.Modules.SaveLoad;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingPersistenceTestHarness
// CATEGORY: Modules / Building / Runtime / Testing
// PURPOSE: Development-only save/load verification for placed building instances.
// PLACEMENT: Bootstrap verification scenes only. Disable for shipping builds.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Waits for placement harness, saves, clears, loads, and verifies restore.
// =============================================================================

namespace CCS.Modules.Building
{
    [DefaultExecutionOrder(280)]
    public sealed class CCS_BuildingPersistenceTestHarness : MonoBehaviour
    {
        private const string LogPrefix = "[CCS_BuildingPersistenceTestHarness]";
        private const string PersistenceTestSlotId = "building_persistence_test";
        private const string FoundationPieceId = "ccs.survival.building.test.foundation";
        private const string WallPieceId = "ccs.survival.building.test.wall";
        private const string FoundationOccupiedSnapPointId = "foundation_edge_top";
        private const string WallOccupiedSnapPointId = "wall_top";
        private const int ExpectedPlacedCount = 3;

        private enum PersistenceStep
        {
            WaitingForPlacement = 0,
            Saving = 1,
            Clearing = 2,
            Loading = 3,
            Verifying = 4,
            Complete = 5
        }

        #region Variables

        [Header("Development Testing")]
        [Tooltip("When enabled, the harness verifies building save/load restore.")]
        [SerializeField] private bool enableHarness = true;

        [Tooltip("Seconds between harness state checks.")]
        [SerializeField] private float checkIntervalSeconds = 2f;

        [Tooltip("Optional placement harness that must finish before persistence runs.")]
        [SerializeField] private CCS_BuildingPlacementTestHarness placementTestHarness;

        [Tooltip("Optional debug controller used for manual save/load calls.")]
        [SerializeField] private CCS_SaveLoadDebugController saveLoadDebugController;

        private float nextCheckTime;
        private PersistenceStep persistenceStep = PersistenceStep.WaitingForPlacement;
        private int expectedSavedCount;

        #endregion

        #region Unity Callbacks

        private void Update()
        {
            if (!enableHarness || persistenceStep == PersistenceStep.Complete)
            {
                return;
            }

            if (Time.time < nextCheckTime)
            {
                return;
            }

            nextCheckTime = Time.time + checkIntervalSeconds;
            TryAdvancePersistenceTest();
        }

        #endregion

        #region Private Methods

        private void TryAdvancePersistenceTest()
        {
            if (!TryResolveServices(
                    out CCS_BuildingService buildingService,
                    out CCS_SaveLoadService saveLoadService,
                    out CCS_SaveLoadDebugController debugController))
            {
                return;
            }

            switch (persistenceStep)
            {
                case PersistenceStep.WaitingForPlacement:
                    TryBeginAfterPlacement(buildingService);
                    break;
                case PersistenceStep.Saving:
                    TrySave(buildingService, debugController);
                    break;
                case PersistenceStep.Clearing:
                    TryClear(buildingService);
                    break;
                case PersistenceStep.Loading:
                    TryLoad(debugController);
                    break;
                case PersistenceStep.Verifying:
                    TryVerify(buildingService);
                    break;
            }
        }

        private void TryBeginAfterPlacement(CCS_BuildingService buildingService)
        {
            if (placementTestHarness == null)
            {
                placementTestHarness = Object.FindAnyObjectByType<CCS_BuildingPlacementTestHarness>();
            }

            if (placementTestHarness != null && !placementTestHarness.IsSnapSequenceComplete)
            {
                return;
            }

            if (buildingService.PlacedInstanceCount < ExpectedPlacedCount)
            {
                return;
            }

            expectedSavedCount = buildingService.PlacedInstanceCount;
            persistenceStep = PersistenceStep.Saving;
            Debug.Log($"{LogPrefix} Placement complete. Starting building persistence save/load cycle.");
        }

        private void TrySave(CCS_BuildingService buildingService, CCS_SaveLoadDebugController debugController)
        {
            debugController.SetSelectedSlotId(PersistenceTestSlotId);
            CCS_SaveLoadResult saveResult = debugController.ManualSaveSelectedSlot();
            if (!saveResult.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Persistence save failed: {saveResult.Message}");
                return;
            }

            persistenceStep = PersistenceStep.Clearing;
            Debug.Log($"{LogPrefix} Saved {expectedSavedCount} building records to slot '{PersistenceTestSlotId}'.");
        }

        private void TryClear(CCS_BuildingService buildingService)
        {
            buildingService.ClearPlacedInstances();

            if (buildingService.PlacedInstanceCount != 0)
            {
                Debug.LogWarning($"{LogPrefix} ClearPlacedInstances did not remove all runtime instances.");
                return;
            }

            persistenceStep = PersistenceStep.Loading;
            Debug.Log($"{LogPrefix} Cleared runtime placed instances before load.");
        }

        private void TryLoad(CCS_SaveLoadDebugController debugController)
        {
            debugController.SetSelectedSlotId(PersistenceTestSlotId);
            CCS_SaveLoadResult loadResult = debugController.ManualLoadSelectedSlot();
            if (!loadResult.IsSuccess)
            {
                Debug.LogError($"{LogPrefix} FAIL — persistence load failed: {loadResult.Message}");
                persistenceStep = PersistenceStep.Complete;
                return;
            }

            persistenceStep = PersistenceStep.Verifying;
            Debug.Log($"{LogPrefix} Load completed. Verifying restored building state.");
        }

        private void TryVerify(CCS_BuildingService buildingService)
        {
            bool countMatches = buildingService.PlacedInstanceCount == expectedSavedCount;
            bool savedRecordCountMatches = buildingService.SavedBuildingRecordCount == expectedSavedCount;
            bool restoredCountMatches = buildingService.RestoredBuildingCount == expectedSavedCount;
            bool snapOccupancyMatches = VerifySnapOccupancy(buildingService);

            if (countMatches && savedRecordCountMatches && restoredCountMatches && snapOccupancyMatches)
            {
                Debug.Log(
                    $"{LogPrefix} PASS — restored {buildingService.RestoredBuildingCount} building instances with snap occupancy.");
            }
            else
            {
                Debug.LogError(
                    $"{LogPrefix} FAIL — restored state mismatch. " +
                    $"Placed={buildingService.PlacedInstanceCount} expected {expectedSavedCount}, " +
                    $"SavedRecords={buildingService.SavedBuildingRecordCount}, " +
                    $"Restored={buildingService.RestoredBuildingCount}, " +
                    $"SnapOccupancy={snapOccupancyMatches}.");
            }

            persistenceStep = PersistenceStep.Complete;
        }

        private static bool VerifySnapOccupancy(CCS_BuildingService buildingService)
        {
            bool foundationOccupied = false;
            bool wallOccupied = false;

            IReadOnlyList<CCS_BuildingInstance> placedInstances = buildingService.GetPlacedInstances();
            for (int index = 0; index < placedInstances.Count; index++)
            {
                CCS_BuildingInstance instance = placedInstances[index];
                if (instance.PieceId == FoundationPieceId
                    && instance.HasOccupiedSnapPoint(FoundationOccupiedSnapPointId))
                {
                    foundationOccupied = true;
                }

                if (instance.PieceId == WallPieceId
                    && instance.HasOccupiedSnapPoint(WallOccupiedSnapPointId))
                {
                    wallOccupied = true;
                }
            }

            return foundationOccupied && wallOccupied;
        }

        private bool TryResolveServices(
            out CCS_BuildingService buildingService,
            out CCS_SaveLoadService saveLoadService,
            out CCS_SaveLoadDebugController debugController)
        {
            buildingService = null;
            saveLoadService = null;
            debugController = saveLoadDebugController;

            if (!CCS_BuildingRuntimeBridge.TryGetBuildingService(out buildingService)
                || buildingService == null
                || !buildingService.IsInitialized)
            {
                return false;
            }

            if (!CCS_SaveLoadRuntimeBridge.TryGetSaveLoadService(out saveLoadService)
                || saveLoadService == null
                || !saveLoadService.IsInitialized)
            {
                return false;
            }

            if (debugController == null)
            {
                debugController = Object.FindAnyObjectByType<CCS_SaveLoadDebugController>();
                saveLoadDebugController = debugController;
            }

            return debugController != null && debugController.EnableDebugControls;
        }

        #endregion
    }
}
