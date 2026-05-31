using System.Collections.Generic;
using CCS.Modules.Building;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingShelterIntegrationTestHarness
// CATEGORY: Modules / Shelter / Runtime / Testing
// PURPOSE: Development-only verification of building shelter contribution integration.
// PLACEMENT: Bootstrap verification scenes only. Disable for shipping builds.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Waits for snap placement, applies subject position, verifies shelter protection.
// =============================================================================

namespace CCS.Modules.Shelter
{
    [DefaultExecutionOrder(275)]
    public sealed class CCS_BuildingShelterIntegrationTestHarness : MonoBehaviour
    {
        private const string LogPrefix = "[CCS_BuildingShelterIntegrationTestHarness]";
        private const string RoofPieceId = "ccs.survival.building.test.roof";
        private const int MinimumShelterContributions = 2;

        #region Variables

        [Header("Development Testing")]
        [Tooltip("When enabled, the harness verifies building shelter integration.")]
        [SerializeField] private bool enableHarness = true;

        [Tooltip("Seconds between harness state checks.")]
        [SerializeField] private float checkIntervalSeconds = 2f;

        [Tooltip("Optional placement harness that must finish before shelter verification.")]
        [SerializeField] private CCS_BuildingPlacementTestHarness placementTestHarness;

        private float nextCheckTime;
        private bool integrationTestCompleted;

        #endregion

        #region Unity Callbacks

        private void Update()
        {
            if (!enableHarness || integrationTestCompleted)
            {
                return;
            }

            if (Time.time < nextCheckTime)
            {
                return;
            }

            nextCheckTime = Time.time + checkIntervalSeconds;
            TryAdvanceIntegrationTest();
        }

        #endregion

        #region Private Methods

        private void TryAdvanceIntegrationTest()
        {
            if (!TryResolveServices(
                    out CCS_BuildingService buildingService,
                    out CCS_ShelterService shelterService))
            {
                return;
            }

            if (placementTestHarness == null)
            {
                placementTestHarness = Object.FindAnyObjectByType<CCS_BuildingPlacementTestHarness>();
            }

            if (placementTestHarness != null && !placementTestHarness.IsSnapSequenceComplete)
            {
                return;
            }

            if (buildingService.PlacedInstanceCount < 3)
            {
                return;
            }

            buildingService.RecalculateShelterContributions();

            if (!TryResolveRoofPosition(buildingService, out Vector3 roofPosition))
            {
                Debug.LogWarning($"{LogPrefix} Could not resolve roof position for shelter subject.");
                return;
            }

            shelterService.SetSubjectPosition(roofPosition);

            int contributionCount = buildingService.ShelterContributionCount;
            CCS_ShelterSnapshot shelterSnapshot = shelterService.GetSnapshot();
            bool contributionsReady = contributionCount >= MinimumShelterContributions;
            bool buildingShelterActive = shelterSnapshot.IsBuildingShelterActive;
            bool protectionApplied = shelterSnapshot.WetnessProtection > 0f
                || shelterSnapshot.ExposureProtection > 0f
                || shelterSnapshot.TemperatureProtection > 0f;

            if (contributionsReady && buildingShelterActive && protectionApplied)
            {
                Debug.Log(
                    $"{LogPrefix} PASS — building shelter contributions={contributionCount}, " +
                    $"wet={shelterSnapshot.WetnessProtection:0.##}, " +
                    $"exp={shelterSnapshot.ExposureProtection:0.##}, " +
                    $"temp={shelterSnapshot.TemperatureProtection:0.##}.");
            }
            else
            {
                Debug.LogError(
                    $"{LogPrefix} FAIL — contributions={contributionCount}, " +
                    $"buildingActive={buildingShelterActive}, " +
                    $"wet={shelterSnapshot.WetnessProtection:0.##}, " +
                    $"exp={shelterSnapshot.ExposureProtection:0.##}, " +
                    $"temp={shelterSnapshot.TemperatureProtection:0.##}.");
            }

            integrationTestCompleted = true;
        }

        private static bool TryResolveRoofPosition(
            CCS_BuildingService buildingService,
            out Vector3 roofPosition)
        {
            roofPosition = Vector3.zero;

            IReadOnlyList<CCS_BuildingInstance> placedInstances = buildingService.GetPlacedInstances();
            for (int index = 0; index < placedInstances.Count; index++)
            {
                CCS_BuildingInstance instance = placedInstances[index];
                if (instance.PieceId == RoofPieceId)
                {
                    roofPosition = instance.Position;
                    return true;
                }
            }

            return false;
        }

        private static bool TryResolveServices(
            out CCS_BuildingService buildingService,
            out CCS_ShelterService shelterService)
        {
            buildingService = null;
            shelterService = null;

            if (!CCS_BuildingShelterRuntimeBridge.TryGetBuildingService(out buildingService)
                || buildingService == null
                || !buildingService.IsInitialized)
            {
                return false;
            }

            return CCS_ShelterRuntimeBridge.TryGetShelterService(out shelterService)
                && shelterService != null
                && shelterService.IsInitialized;
        }

        #endregion
    }
}
