using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingPlacementTestHarness
// CATEGORY: Modules / Building / Runtime / Testing
// PURPOSE: Development-only harness that cycles test definitions and places pieces.
// PLACEMENT: Bootstrap verification scenes only. Disable for shipping builds.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Automatically places foundation, wall, and roof around the test area.
// =============================================================================

namespace CCS.Modules.Building
{
    [DefaultExecutionOrder(270)]
    public sealed class CCS_BuildingPlacementTestHarness : MonoBehaviour
    {
        private const string FoundationPieceId = "ccs.survival.building.test.foundation";
        private const string WallPieceId = "ccs.survival.building.test.wall";
        private const string RoofPieceId = "ccs.survival.building.test.roof";

        #region Variables

        [Header("Development Testing")]
        [Tooltip("When enabled, the harness cycles test definitions and places pieces.")]
        [SerializeField] private bool enableHarness = true;

        [Tooltip("Seconds between automated placement attempts.")]
        [SerializeField] private float placementIntervalSeconds = 4f;

        [Tooltip("World-space anchor for placement offsets.")]
        [SerializeField] private Transform testAreaAnchor;

        [Tooltip("Local offsets used for each automated placement.")]
        [SerializeField] private Vector3[] placementOffsets =
        {
            new Vector3(0f, 0.5f, 0f),
            new Vector3(2f, 0.5f, 0f),
            new Vector3(4f, 0.5f, 0f),
            new Vector3(0f, 0.5f, 2f),
            new Vector3(2f, 0.5f, 2f),
            new Vector3(4f, 0.5f, 2f)
        };

        private readonly string[] cyclePieceIds =
        {
            FoundationPieceId,
            WallPieceId,
            RoofPieceId
        };

        private float nextPlacementTime;
        private int cycleIndex;
        private int offsetIndex;

        #endregion

        #region Unity Callbacks

        private void Update()
        {
            if (!enableHarness)
            {
                return;
            }

            if (Time.time < nextPlacementTime)
            {
                return;
            }

            nextPlacementTime = Time.time + placementIntervalSeconds;
            TryPlaceNextPiece();
        }

        #endregion

        #region Private Methods

        private void TryPlaceNextPiece()
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

            string pieceId = cyclePieceIds[cycleIndex % cyclePieceIds.Length];
            cycleIndex++;

            if (!buildingService.TryGetDefinition(pieceId, out CCS_BuildingPieceDefinition definition))
            {
                return;
            }

            if (!placementService.SetActiveDefinition(definition))
            {
                return;
            }

            Vector3 placementPosition = ResolvePlacementPosition();
            if (!placementService.UpdatePreview(placementPosition, Quaternion.identity))
            {
                return;
            }

            if (!placementService.PlaceCurrentPiece())
            {
                return;
            }

            SpawnPlacedVisual(definition, placementPosition, Quaternion.identity, testAreaAnchor != null ? testAreaAnchor : transform);
            offsetIndex = (offsetIndex + 1) % Mathf.Max(1, placementOffsets.Length);
        }

        private Vector3 ResolvePlacementPosition()
        {
            Vector3 anchorPosition = testAreaAnchor != null ? testAreaAnchor.position : transform.position;
            if (placementOffsets == null || placementOffsets.Length == 0)
            {
                return anchorPosition;
            }

            return anchorPosition + placementOffsets[offsetIndex % placementOffsets.Length];
        }

        private static void SpawnPlacedVisual(
            CCS_BuildingPieceDefinition definition,
            Vector3 position,
            Quaternion rotation,
            Transform parent)
        {
            GameObject placedObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            placedObject.name = $"CCS_Placed_{definition.BuildingPieceType}";
            placedObject.transform.SetPositionAndRotation(position, rotation);

            if (parent != null)
            {
                placedObject.transform.SetParent(parent, true);
            }
        }

        #endregion
    }
}
