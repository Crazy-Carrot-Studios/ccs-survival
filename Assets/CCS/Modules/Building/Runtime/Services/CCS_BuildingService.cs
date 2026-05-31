using System.Collections.Generic;
using CCS.Modules.SaveLoad;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingService
// CATEGORY: Modules / Building / Runtime / Services
// PURPOSE: Authoritative building definition catalog and placed instance tracking.
// PLACEMENT: Registered as CCS_ISurvivalService by survival gameplay composition wiring.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Placement orchestration delegated to CCS_BuildingPlacementService in 0.8.1. Shelter contributions in 0.8.5.
// =============================================================================

namespace CCS.Modules.Building
{
    public sealed class CCS_BuildingService : CCS_ISurvivalService, CCS_ISaveable
    {
        private const string LogPrefix = "[CCS_BuildingService]";

        #region Variables

        private readonly Dictionary<string, CCS_BuildingPieceDefinition> definitionsById =
            new Dictionary<string, CCS_BuildingPieceDefinition>();

        private readonly List<CCS_BuildingInstance> placedInstances = new List<CCS_BuildingInstance>();

        private readonly List<CCS_BuildingShelterContribution> shelterContributions =
            new List<CCS_BuildingShelterContribution>();

        private readonly CCS_BuildingState buildingState = new CCS_BuildingState();

        private CCS_BuildingProfile activeProfile;
        private CCS_BuildingPlacementService placementService;
        private bool isInitialized;

        #endregion

        #region Events

        public event BuildingDefinitionRegisteredHandler BuildingDefinitionRegistered;
        public event BuildingStateChangedHandler BuildingStateChanged;
        public event BuildingShelterContributionsChangedHandler BuildingShelterContributionsChanged;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_BuildingProfile ActiveProfile => activeProfile;

        public string SaveableId => CCS_SaveLoadSaveableIds.GlobalBuilding;

        public int RegisteredDefinitionCount => buildingState.RegisteredDefinitionCount;

        public int PlacedInstanceCount => placedInstances.Count;

        public int SavedBuildingRecordCount { get; private set; }

        public int RestoredBuildingCount { get; private set; }

        public int ShelterContributionCount => shelterContributions.Count;

        #endregion

        #region Public Methods

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            // Profile binding via InitializeFromProfile sets isInitialized when ready.
        }

        public void InitializeFromProfile(CCS_BuildingProfile profile)
        {
            if (profile == null)
            {
                Debug.LogWarning($"{LogPrefix} InitializeFromProfile called with null profile.");
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_BuildingValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            activeProfile = profile;
            definitionsById.Clear();
            buildingState.Clear();
            placedInstances.Clear();
            shelterContributions.Clear();
            isInitialized = true;

            IReadOnlyList<CCS_BuildingPieceDefinition> startupDefinitions = profile.StartupDefinitions;
            if (startupDefinitions != null)
            {
                for (int index = 0; index < startupDefinitions.Count; index++)
                {
                    RegisterDefinition(startupDefinitions[index], "Startup definition registration.");
                }
            }

            RaiseBuildingStateChanged("Building service initialized.");
        }

        public void BindPlacementService(CCS_BuildingPlacementService service)
        {
            placementService = service;
        }

        public bool RegisterDefinition(CCS_BuildingPieceDefinition definition, string message = null)
        {
            if (!EnsureInitialized() || definition == null)
            {
                return false;
            }

            CCS_SurvivalValidationResult validation = CCS_BuildingValidationUtility.ValidateDefinition(definition);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} RegisterDefinition rejected: {validation.Message}");
                return false;
            }

            string pieceId = definition.PieceId;
            bool isNewDefinition = !definitionsById.ContainsKey(pieceId);
            definitionsById[pieceId] = definition;
            buildingState.RegisterPieceId(pieceId);

            CCS_BuildingEventArgs eventArgs = CreateEventArgs(
                message ?? $"Registered building definition '{pieceId}'.");

            if (isNewDefinition)
            {
                BuildingDefinitionRegistered?.Invoke(eventArgs);
            }

            BuildingStateChanged?.Invoke(eventArgs);
            return true;
        }

        public bool UnregisterDefinition(string pieceId, string message = null)
        {
            if (!EnsureInitialized() || string.IsNullOrWhiteSpace(pieceId))
            {
                return false;
            }

            if (!definitionsById.Remove(pieceId))
            {
                return false;
            }

            buildingState.UnregisterPieceId(pieceId);
            BuildingStateChanged?.Invoke(
                CreateEventArgs(message ?? $"Unregistered building definition '{pieceId}'."));
            return true;
        }

        public bool TryGetDefinition(string pieceId, out CCS_BuildingPieceDefinition definition)
        {
            definition = null;

            if (!EnsureInitialized() || string.IsNullOrWhiteSpace(pieceId))
            {
                return false;
            }

            return definitionsById.TryGetValue(pieceId, out definition);
        }

        public IReadOnlyList<CCS_BuildingPieceDefinition> GetRegisteredDefinitions()
        {
            if (!EnsureInitialized())
            {
                return System.Array.Empty<CCS_BuildingPieceDefinition>();
            }

            CCS_BuildingPieceDefinition[] definitions = new CCS_BuildingPieceDefinition[definitionsById.Count];
            definitionsById.Values.CopyTo(definitions, 0);
            return definitions;
        }

        public IReadOnlyList<CCS_BuildingInstance> GetPlacedInstances()
        {
            if (!EnsureInitialized())
            {
                return System.Array.Empty<CCS_BuildingInstance>();
            }

            return placedInstances;
        }

        public CCS_BuildingPlacementSnapshot GetPlacementSnapshot()
        {
            if (placementService != null && placementService.IsInitialized)
            {
                return placementService.GetSnapshot();
            }

            return CCS_BuildingPlacementSnapshot.Empty;
        }

        public bool TryAddPlacedInstance(
            CCS_BuildingInstance instance,
            CCS_BuildingSnapMatch snapMatch = default)
        {
            if (!EnsureInitialized() || instance == null)
            {
                return false;
            }

            if (!TryGetDefinition(instance.PieceId, out CCS_BuildingPieceDefinition definition))
            {
                Debug.LogWarning($"{LogPrefix} TryAddPlacedInstance rejected unknown piece '{instance.PieceId}'.");
                return false;
            }

            instance.InitializeRuntimeSnapPoints(definition);

            if (snapMatch.HasMatch)
            {
                instance.SetTargetSnapConnection(
                    snapMatch.TargetInstanceId,
                    snapMatch.TargetSnapPointId);
            }

            placedInstances.Add(instance);
            CCS_BuildingInstanceVisualFactory.SpawnInstanceVisual(definition, instance);
            RecalculateShelterContributions();
            BuildingStateChanged?.Invoke(
                CreateEventArgs($"Placed building instance '{instance.InstanceId}'."));
            return true;
        }

        public IReadOnlyList<CCS_BuildingShelterContribution> GetShelterContributions()
        {
            if (!EnsureInitialized())
            {
                return System.Array.Empty<CCS_BuildingShelterContribution>();
            }

            return shelterContributions;
        }

        public void RecalculateShelterContributions()
        {
            if (!EnsureInitialized())
            {
                return;
            }

            shelterContributions.Clear();

            for (int index = 0; index < placedInstances.Count; index++)
            {
                CCS_BuildingInstance instance = placedInstances[index];
                if (instance == null
                    || !TryGetDefinition(instance.PieceId, out CCS_BuildingPieceDefinition definition)
                    || !definition.ContributesToShelter)
                {
                    continue;
                }

                shelterContributions.Add(new CCS_BuildingShelterContribution(
                    instance.InstanceId,
                    definition.PieceId,
                    instance.Position,
                    definition.ShelterCoverageRadius,
                    definition.WetnessProtectionContribution,
                    definition.ExposureProtectionContribution,
                    definition.TemperatureProtectionContribution));
            }

            BuildingShelterContributionsChanged?.Invoke(
                new CCS_BuildingShelterContributionsChangedEventArgs(
                    shelterContributions.Count,
                    $"Recalculated {shelterContributions.Count} building shelter contributions."));
        }

        public void ClearPlacedInstances()
        {
            if (!EnsureInitialized())
            {
                return;
            }

            placedInstances.Clear();
            CCS_BuildingInstanceVisualFactory.DestroyAllVisuals();
            RestoredBuildingCount = 0;
            RecalculateShelterContributions();
            RaiseBuildingStateChanged("Cleared placed building instances.");
        }

        public bool TryMarkSnapPointOccupied(string instanceId, string snapPointId, bool occupied)
        {
            if (!EnsureInitialized()
                || string.IsNullOrWhiteSpace(instanceId)
                || string.IsNullOrWhiteSpace(snapPointId))
            {
                return false;
            }

            for (int index = 0; index < placedInstances.Count; index++)
            {
                CCS_BuildingInstance instance = placedInstances[index];
                if (instance.InstanceId != instanceId)
                {
                    continue;
                }

                if (!instance.TrySetSnapPointOccupied(snapPointId, occupied))
                {
                    return false;
                }

                BuildingStateChanged?.Invoke(
                    CreateEventArgs($"Updated snap point occupancy on '{instanceId}'."));
                return true;
            }

            return false;
        }

        public CCS_BuildingPieceSnapshot GetPieceSnapshot(string pieceId)
        {
            if (!TryGetDefinition(pieceId, out CCS_BuildingPieceDefinition definition))
            {
                return CCS_BuildingPieceSnapshot.Empty;
            }

            return new CCS_BuildingPieceSnapshot(
                definition.PieceId,
                definition.BuildingPieceType,
                Vector3.zero,
                Quaternion.identity);
        }

        public string CaptureState()
        {
            if (!EnsureInitialized())
            {
                return JsonUtility.ToJson(new CCS_BuildingSaveData());
            }

            CCS_BuildingSaveData saveData = new CCS_BuildingSaveData
            {
                saveDataVersion = CCS_BuildingSaveData.CurrentSaveDataVersion,
                registeredPieceIds = new List<string>(buildingState.RegisteredPieceIds),
                placedInstanceRecords = BuildPlacedInstanceRecords()
            };

            return JsonUtility.ToJson(saveData);
        }

        public void RestoreState(string stateJson)
        {
            if (!EnsureInitialized())
            {
                Debug.LogWarning($"{LogPrefix} RestoreState skipped because service is not initialized.");
                return;
            }

            placedInstances.Clear();
            CCS_BuildingInstanceVisualFactory.DestroyAllVisuals();
            RestoredBuildingCount = 0;
            SavedBuildingRecordCount = 0;

            if (string.IsNullOrWhiteSpace(stateJson))
            {
                buildingState.Clear();
                RecalculateShelterContributions();
                RaiseBuildingStateChanged("Building restore cleared catalog state.");
                return;
            }

            CCS_BuildingSaveData saveData = JsonUtility.FromJson<CCS_BuildingSaveData>(stateJson);
            if (saveData == null)
            {
                Debug.LogWarning($"{LogPrefix} RestoreState skipped because payload could not be parsed.");
                return;
            }

            if (saveData.saveDataVersion <= 0)
            {
                Debug.LogWarning($"{LogPrefix} RestoreState skipped because saveDataVersion is missing.");
                return;
            }

            buildingState.ReplaceRegisteredPieceIds(saveData.registeredPieceIds);
            PruneDefinitionsMissingFromState();
            RestorePlacedInstances(saveData);
            RecalculateShelterContributions();

            RaiseBuildingStateChanged(
                $"Building restore completed with {RestoredBuildingCount} placed instances.");
        }

        #endregion

        #region Private Methods

        private bool EnsureInitialized()
        {
            if (isInitialized && activeProfile != null)
            {
                return true;
            }

            Debug.LogWarning($"{LogPrefix} Service is not initialized.");
            return false;
        }

        private List<CCS_BuildingInstanceSaveRecord> BuildPlacedInstanceRecords()
        {
            List<CCS_BuildingInstanceSaveRecord> records = new List<CCS_BuildingInstanceSaveRecord>(placedInstances.Count);

            for (int index = 0; index < placedInstances.Count; index++)
            {
                CCS_BuildingInstance instance = placedInstances[index];
                List<string> occupiedSnapPointIds = instance.CollectOccupiedSnapPointIds();
                records.Add(new CCS_BuildingInstanceSaveRecord
                {
                    instanceId = instance.InstanceId,
                    pieceId = instance.PieceId,
                    positionX = instance.Position.x,
                    positionY = instance.Position.y,
                    positionZ = instance.Position.z,
                    rotationX = instance.Rotation.x,
                    rotationY = instance.Rotation.y,
                    rotationZ = instance.Rotation.z,
                    rotationW = instance.Rotation.w,
                    creationTime = instance.CreationTime,
                    placedOrderIndex = index,
                    occupiedSnapPointIds = occupiedSnapPointIds,
                    targetSnapInstanceId = instance.TargetSnapInstanceId,
                    targetSnapPointId = instance.TargetSnapPointId
                });
            }

            return records;
        }

        private void RestorePlacedInstances(CCS_BuildingSaveData saveData)
        {
            if (saveData.placedInstanceRecords == null || saveData.placedInstanceRecords.Count == 0)
            {
                SavedBuildingRecordCount = 0;
                return;
            }

            SavedBuildingRecordCount = saveData.placedInstanceRecords.Count;
            List<CCS_BuildingInstanceSaveRecord> records =
                new List<CCS_BuildingInstanceSaveRecord>(saveData.placedInstanceRecords);
            records.Sort(ComparePlacedInstanceRecords);

            CCS_BuildingDefinitionLookup definitionLookup =
                new CCS_BuildingDefinitionLookup(activeProfile, definitionsById);

            for (int index = 0; index < records.Count; index++)
            {
                CCS_BuildingInstanceSaveRecord record = records[index];
                if (record == null)
                {
                    Debug.LogWarning($"{LogPrefix} RestoreState skipped null placed instance record.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(record.instanceId) || string.IsNullOrWhiteSpace(record.pieceId))
                {
                    Debug.LogWarning($"{LogPrefix} RestoreState skipped record with missing identity.");
                    continue;
                }

                if (!definitionLookup.TryResolveDefinition(record.pieceId, out CCS_BuildingPieceDefinition definition))
                {
                    Debug.LogWarning(
                        $"{LogPrefix} RestoreState skipped unknown piece '{record.pieceId}' for instance '{record.instanceId}'.");
                    continue;
                }

                Vector3 position = new Vector3(record.positionX, record.positionY, record.positionZ);
                Quaternion rotation = new Quaternion(
                    record.rotationX,
                    record.rotationY,
                    record.rotationZ,
                    record.rotationW);

                if (rotation == default)
                {
                    rotation = Quaternion.identity;
                }

                CCS_BuildingInstance instance = new CCS_BuildingInstance(
                    record.instanceId,
                    record.pieceId,
                    position,
                    rotation,
                    record.creationTime);

                instance.InitializeRuntimeSnapPoints(definition);

                if (!string.IsNullOrWhiteSpace(record.targetSnapInstanceId)
                    && !string.IsNullOrWhiteSpace(record.targetSnapPointId))
                {
                    instance.SetTargetSnapConnection(record.targetSnapInstanceId, record.targetSnapPointId);
                }

                if (record.occupiedSnapPointIds != null && record.occupiedSnapPointIds.Count > 0)
                {
                    instance.ApplyOccupiedSnapPoints(record.occupiedSnapPointIds);
                }

                placedInstances.Add(instance);
                CCS_BuildingInstanceVisualFactory.SpawnInstanceVisual(definition, instance);
                RestoredBuildingCount++;
            }

            placementService?.SyncNextInstanceSequenceFromRestoredInstances(placedInstances);
        }

        private static int ComparePlacedInstanceRecords(
            CCS_BuildingInstanceSaveRecord left,
            CCS_BuildingInstanceSaveRecord right)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            return left.placedOrderIndex.CompareTo(right.placedOrderIndex);
        }

        private void PruneDefinitionsMissingFromState()
        {
            List<string> staleIds = new List<string>();

            foreach (KeyValuePair<string, CCS_BuildingPieceDefinition> entry in definitionsById)
            {
                if (!buildingState.ContainsPieceId(entry.Key))
                {
                    staleIds.Add(entry.Key);
                }
            }

            for (int index = 0; index < staleIds.Count; index++)
            {
                definitionsById.Remove(staleIds[index]);
            }
        }

        private CCS_BuildingEventArgs CreateEventArgs(string message)
        {
            return new CCS_BuildingEventArgs(buildingState.RegisteredDefinitionCount, message);
        }

        private void RaiseBuildingStateChanged(string message)
        {
            BuildingStateChanged?.Invoke(CreateEventArgs(message));
        }

        #endregion
    }
}
