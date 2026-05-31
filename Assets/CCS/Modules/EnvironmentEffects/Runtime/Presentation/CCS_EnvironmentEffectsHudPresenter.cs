using CCS.Modules.Building;
using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_EnvironmentEffectsHudPresenter
// CATEGORY: Modules / EnvironmentEffects / Runtime / Presentation
// PURPOSE: Read-only HUD display for ambient temperature, wetness, and exposure.
// PLACEMENT: Child of PF_CCS_HUD_Root canvas beneath time and weather panels.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Development/read-only display. No icons or final art.
// =============================================================================

namespace CCS.Modules.EnvironmentEffects
{
    public sealed class CCS_EnvironmentEffectsHudPresenter : MonoBehaviour
    {
        #region Variables

        [Header("Display")]
        [Tooltip("Text element showing ambient temperature, wetness, and exposure.")]
        [SerializeField] private Text statusText;

        private CCS_EnvironmentEffectsService environmentService;
        private CCS_BuildingService buildingService;
        private CCS_BuildingPlacementService placementService;

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            TryBindServices();
            RefreshDisplay();
        }

        private void OnDisable()
        {
            UnbindServiceEvents();
        }

        private void OnDestroy()
        {
            UnbindServiceEvents();
            environmentService = null;
            buildingService = null;
            placementService = null;
        }

        #endregion

        #region Public Methods

        public void BindStatusText(Text textComponent)
        {
            statusText = textComponent;
            RefreshDisplay();
        }

        public void RefreshDisplay()
        {
            if (statusText == null)
            {
                return;
            }

            if (!TryBindServices())
            {
                statusText.text = "Environment\nUnavailable";
                return;
            }

            CCS_EnvironmentSnapshot snapshot = environmentService.GetSnapshot();
            string displayText = CCS_EnvironmentEffectsValidationUtility.FormatEnvironmentDisplay(snapshot);
            displayText = AppendBuildingHudLines(displayText);
            statusText.text = displayText;
        }

        #endregion

        #region Private Methods

        private bool TryBindServices()
        {
            bool environmentReady = TryBindEnvironmentService();
            TryBindBuildingServices();
            return environmentReady;
        }

        private bool TryBindEnvironmentService()
        {
            if (environmentService != null && environmentService.IsInitialized)
            {
                return true;
            }

            UnbindEnvironmentServiceEvents();

            if (!CCS_EnvironmentEffectsRuntimeBridge.TryGetEnvironmentEffectsService(out environmentService)
                || environmentService == null
                || !environmentService.IsInitialized)
            {
                environmentService = null;
                return false;
            }

            environmentService.EnvironmentChanged += HandleEnvironmentChanged;
            environmentService.TemperatureChanged += HandleEnvironmentChanged;
            environmentService.WetnessChanged += HandleEnvironmentChanged;
            environmentService.ExposureChanged += HandleEnvironmentChanged;
            return true;
        }

        private void TryBindBuildingServices()
        {
            TryBindBuildingService();
            TryBindPlacementService();
        }

        private void TryBindBuildingService()
        {
            if (buildingService != null && buildingService.IsInitialized)
            {
                return;
            }

            UnbindBuildingServiceEvents();

            if (!CCS_BuildingRuntimeBridge.TryGetBuildingService(out buildingService)
                || buildingService == null
                || !buildingService.IsInitialized)
            {
                buildingService = null;
                return;
            }

            buildingService.BuildingDefinitionRegistered += HandleBuildingChanged;
            buildingService.BuildingStateChanged += HandleBuildingChanged;
        }

        private void TryBindPlacementService()
        {
            if (placementService != null && placementService.IsInitialized)
            {
                return;
            }

            UnbindPlacementServiceEvents();

            if (!CCS_BuildingRuntimeBridge.TryGetBuildingPlacementService(out placementService)
                || placementService == null
                || !placementService.IsInitialized)
            {
                placementService = null;
                return;
            }

            placementService.PlacementStarted += HandlePlacementChanged;
            placementService.PlacementCancelled += HandlePlacementChanged;
            placementService.BuildingPlaced += HandlePlacementChanged;
        }

        private void UnbindServiceEvents()
        {
            UnbindEnvironmentServiceEvents();
            UnbindBuildingServiceEvents();
            UnbindPlacementServiceEvents();
        }

        private void UnbindEnvironmentServiceEvents()
        {
            if (environmentService == null)
            {
                return;
            }

            environmentService.EnvironmentChanged -= HandleEnvironmentChanged;
            environmentService.TemperatureChanged -= HandleEnvironmentChanged;
            environmentService.WetnessChanged -= HandleEnvironmentChanged;
            environmentService.ExposureChanged -= HandleEnvironmentChanged;
        }

        private void UnbindBuildingServiceEvents()
        {
            if (buildingService == null)
            {
                return;
            }

            buildingService.BuildingDefinitionRegistered -= HandleBuildingChanged;
            buildingService.BuildingStateChanged -= HandleBuildingChanged;
        }

        private void UnbindPlacementServiceEvents()
        {
            if (placementService == null)
            {
                return;
            }

            placementService.PlacementStarted -= HandlePlacementChanged;
            placementService.PlacementCancelled -= HandlePlacementChanged;
            placementService.BuildingPlaced -= HandlePlacementChanged;
        }

        private string AppendBuildingHudLines(string displayText)
        {
            int definitionCount = 0;
            int placedCount = 0;
            CCS_BuildingPlacementSnapshot placementSnapshot = CCS_BuildingPlacementSnapshot.Empty;

            if (buildingService != null && buildingService.IsInitialized)
            {
                definitionCount = buildingService.RegisteredDefinitionCount;
                placedCount = buildingService.PlacedInstanceCount;
                placementSnapshot = buildingService.GetPlacementSnapshot();
            }

            return displayText + "\n" +
                   CCS_BuildingValidationUtility.FormatBuildingDefinitionCountLine(definitionCount) + "\n" +
                   CCS_BuildingValidationUtility.FormatPlacementHudLines(placementSnapshot, placedCount);
        }

        private void HandleEnvironmentChanged(CCS_EnvironmentEffectsEventArgs eventArgs)
        {
            RefreshDisplay();
        }

        private void HandleBuildingChanged(CCS_BuildingEventArgs eventArgs)
        {
            RefreshDisplay();
        }

        private void HandlePlacementChanged(CCS_BuildingPlacementEventArgs eventArgs)
        {
            RefreshDisplay();
        }

        #endregion
    }
}
