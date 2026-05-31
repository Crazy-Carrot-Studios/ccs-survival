using System.Collections.Generic;
using CCS.Modules.SaveLoad;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingService
// CATEGORY: Modules / Building / Runtime / Services
// PURPOSE: Authoritative building definition catalog with events and save/load.
// PLACEMENT: Registered as CCS_ISurvivalService by survival gameplay composition wiring.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: No placement, spawning, construction, or destruction in 0.8.0.
// =============================================================================

namespace CCS.Modules.Building
{
    public sealed class CCS_BuildingService : CCS_ISurvivalService, CCS_ISaveable
    {
        private const string LogPrefix = "[CCS_BuildingService]";

        #region Variables

        private readonly Dictionary<string, CCS_BuildingPieceDefinition> definitionsById =
            new Dictionary<string, CCS_BuildingPieceDefinition>();

        private readonly CCS_BuildingState buildingState = new CCS_BuildingState();

        private CCS_BuildingProfile activeProfile;
        private bool isInitialized;

        #endregion

        #region Events

        public event BuildingDefinitionRegisteredHandler BuildingDefinitionRegistered;
        public event BuildingStateChangedHandler BuildingStateChanged;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_BuildingProfile ActiveProfile => activeProfile;

        public string SaveableId => CCS_SaveLoadSaveableIds.GlobalBuilding;

        public int RegisteredDefinitionCount => buildingState.RegisteredDefinitionCount;

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
                registeredPieceIds = new List<string>(buildingState.RegisteredPieceIds)
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

            if (string.IsNullOrWhiteSpace(stateJson))
            {
                buildingState.Clear();
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
            RaiseBuildingStateChanged("Building catalog restored from save.");
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
