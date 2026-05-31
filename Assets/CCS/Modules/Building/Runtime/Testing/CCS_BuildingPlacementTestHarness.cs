using CCS.Core;
using CCS.Modules.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingPlacementTestHarness
// CATEGORY: Modules / Building / Runtime / Testing
// PURPOSE: Development-only harness that places foundation, wall, and roof with snapping.
// PLACEMENT: Bootstrap verification scenes only. Disable for shipping builds.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Foundation free-place, wall snaps to foundation, roof snaps to wall.
// =============================================================================

namespace CCS.Modules.Building
{
    [DefaultExecutionOrder(270)]
    public sealed class CCS_BuildingPlacementTestHarness : MonoBehaviour
    {
        private const string LogPrefix = "[CCS_BuildingPlacementTestHarness]";
        private const string FoundationPieceId = "ccs.survival.building.test.foundation";
        private const string WallPieceId = "ccs.survival.building.test.wall";
        private const string RoofPieceId = "ccs.survival.building.test.roof";

        private enum HarnessSequenceStep
        {
            Foundation = 0,
            Wall = 1,
            Roof = 2,
            Complete = 3
        }

        #region Variables

        [Header("Development Testing")]
        [Tooltip("When enabled, the harness cycles test definitions and places pieces.")]
        [SerializeField] private bool enableHarness = true;

        [Tooltip("Seconds between automated placement attempts.")]
        [SerializeField] private float placementIntervalSeconds = 4f;

        [Tooltip("World-space anchor for placement offsets.")]
        [SerializeField] private Transform testAreaAnchor;

        [Tooltip("Local offset used for free foundation placement.")]
        [SerializeField] private Vector3 foundationPlacementOffset = new Vector3(0f, 0.5f, 0f);

        [Header("Resource Seeding")]
        [Tooltip("Wood item used to seed the player inventory for automated placement.")]
        [SerializeField] private CCS_ItemDefinition seedWoodItem;

        [Tooltip("Stone item used to seed the player inventory for automated placement.")]
        [SerializeField] private CCS_ItemDefinition seedStoneItem;

        [Tooltip("Fiber item used to seed the player inventory for automated placement.")]
        [SerializeField] private CCS_ItemDefinition seedFiberItem;

        [Tooltip("Initial wood quantity granted to inventory before cycling placements.")]
        [SerializeField] private int seedWoodQuantity = 50;

        [Tooltip("Initial stone quantity granted to inventory before cycling placements.")]
        [SerializeField] private int seedStoneQuantity = 20;

        [Tooltip("Initial fiber quantity granted to inventory before cycling placements.")]
        [SerializeField] private int seedFiberQuantity = 20;

        private float nextPlacementTime;
        private HarnessSequenceStep sequenceStep = HarnessSequenceStep.Foundation;
        private bool hasSeededResources;
        private Vector3 lastFoundationPosition = Vector3.zero;
        private Vector3 lastWallPosition = Vector3.zero;

        #endregion

        #region Unity Callbacks

        private void Update()
        {
            if (!enableHarness || sequenceStep == HarnessSequenceStep.Complete)
            {
                return;
            }

            if (Time.time < nextPlacementTime)
            {
                return;
            }

            nextPlacementTime = Time.time + placementIntervalSeconds;
            TryAdvanceSnapSequence();
        }

        #endregion

        #region Private Methods

        private void TryAdvanceSnapSequence()
        {
            if (!CCS_BuildingRuntimeBridge.TryGetBuildingPlacementService(out CCS_BuildingPlacementService placementService)
                || placementService == null
                || !placementService.IsInitialized)
            {
                return;
            }

            if (!CCS_BuildingRuntimeBridge.TryGetBuildingService(out CCS_BuildingService buildingService)
                || buildingService == null
                || !buildingService.IsInitialized)
            {
                return;
            }

            if (!TrySeedTestResources())
            {
                return;
            }

            switch (sequenceStep)
            {
                case HarnessSequenceStep.Foundation:
                    TryPlaceFoundation(placementService, buildingService);
                    break;
                case HarnessSequenceStep.Wall:
                    TryPlaceSnappedPiece(placementService, buildingService, WallPieceId, lastFoundationPosition);
                    break;
                case HarnessSequenceStep.Roof:
                    TryPlaceSnappedPiece(placementService, buildingService, RoofPieceId, lastWallPosition);
                    break;
            }
        }

        private void TryPlaceFoundation(
            CCS_BuildingPlacementService placementService,
            CCS_BuildingService buildingService)
        {
            if (!buildingService.TryGetDefinition(FoundationPieceId, out CCS_BuildingPieceDefinition definition)
                || !placementService.SetActiveDefinition(definition))
            {
                return;
            }

            Vector3 placementPosition = ResolveFoundationPosition();
            if (!placementService.UpdatePreviewWithSnap(placementPosition, Quaternion.identity))
            {
                Debug.Log($"{LogPrefix} Foundation preview invalid at {placementPosition}.");
                return;
            }

            CCS_BuildingPlacementValidationResult result = placementService.PlaceCurrentPieceUsingSnap();
            if (!result.Success)
            {
                Debug.Log($"{LogPrefix} Foundation placement failed: {result.FailureReason}");
                return;
            }

            lastFoundationPosition = placementService.GetSnapshot().PreviewPosition;
            SpawnPlacedVisual(definition, lastFoundationPosition, Quaternion.identity);
            Debug.Log($"{LogPrefix} Foundation placed free at {lastFoundationPosition}.");
            sequenceStep = HarnessSequenceStep.Wall;
        }

        private void TryPlaceSnappedPiece(
            CCS_BuildingPlacementService placementService,
            CCS_BuildingService buildingService,
            string pieceId,
            Vector3 snapHintPosition)
        {
            if (!buildingService.TryGetDefinition(pieceId, out CCS_BuildingPieceDefinition definition)
                || !placementService.SetActiveDefinition(definition))
            {
                return;
            }

            if (!placementService.UpdatePreviewWithSnap(snapHintPosition, Quaternion.identity))
            {
                Debug.Log($"{LogPrefix} Snap preview invalid for '{pieceId}' near {snapHintPosition}.");
                return;
            }

            if (!placementService.FindBestSnapMatch(snapHintPosition, out CCS_BuildingSnapMatch snapMatch))
            {
                Debug.Log($"{LogPrefix} No snap match found for '{pieceId}' near {snapHintPosition}.");
                return;
            }

            Debug.Log(
                $"{LogPrefix} Snap match for '{pieceId}': target={snapMatch.TargetSnapPointType}, source={snapMatch.SourceSnapPointType}, position={snapMatch.SnappedPosition}.");

            Vector3 placedPosition = snapMatch.SnappedPosition;
            CCS_BuildingPlacementValidationResult result = placementService.PlaceCurrentPieceUsingSnap();
            if (!result.Success)
            {
                Debug.Log($"{LogPrefix} Snapped placement failed for '{pieceId}': {result.FailureReason}");
                return;
            }

            SpawnPlacedVisual(definition, placedPosition, snapMatch.SnappedRotation);

            if (pieceId == WallPieceId)
            {
                lastWallPosition = placedPosition;
                sequenceStep = HarnessSequenceStep.Roof;
                Debug.Log($"{LogPrefix} Wall placed snapped at {lastWallPosition}.");
                return;
            }

            Debug.Log($"{LogPrefix} Roof placed snapped at {placedPosition}. Snap sequence complete.");
            sequenceStep = HarnessSequenceStep.Complete;
        }

        private bool TrySeedTestResources()
        {
            if (hasSeededResources)
            {
                return true;
            }

            if (!CCS_BuildingRuntimeBridge.TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost)
                || runtimeHost == null
                || runtimeHost.ServiceRegistry == null)
            {
                return false;
            }

            if (!runtimeHost.ServiceRegistry.TryGetService(out CCS_PlayerInventoryService inventoryService)
                || inventoryService == null
                || !inventoryService.IsInitialized)
            {
                return false;
            }

            AddSeedItem(inventoryService, seedWoodItem, seedWoodQuantity);
            AddSeedItem(inventoryService, seedStoneItem, seedStoneQuantity);
            AddSeedItem(inventoryService, seedFiberItem, seedFiberQuantity);
            hasSeededResources = true;
            return true;
        }

        private static void AddSeedItem(
            CCS_PlayerInventoryService inventoryService,
            CCS_ItemDefinition itemDefinition,
            int quantity)
        {
            if (inventoryService == null || itemDefinition == null || quantity <= 0)
            {
                return;
            }

            inventoryService.AddItem(itemDefinition, quantity);
        }

        private Vector3 ResolveFoundationPosition()
        {
            Vector3 anchorPosition = testAreaAnchor != null ? testAreaAnchor.position : transform.position;
            return anchorPosition + foundationPlacementOffset;
        }

        private static void SpawnPlacedVisual(
            CCS_BuildingPieceDefinition definition,
            Vector3 position,
            Quaternion rotation)
        {
            GameObject placedObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            placedObject.name = $"CCS_Placed_{definition.BuildingPieceType}";
            placedObject.transform.SetPositionAndRotation(position, rotation);
        }

        #endregion
    }
}
