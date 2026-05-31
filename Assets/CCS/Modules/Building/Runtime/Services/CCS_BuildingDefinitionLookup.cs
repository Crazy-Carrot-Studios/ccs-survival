using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_BuildingDefinitionLookup
// CATEGORY: Modules / Building / Runtime / Services
// PURPOSE: Resolves building piece definition IDs for restore and validation.
// PLACEMENT: Used by CCS_BuildingService.RestoreState().
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Uses profile startup definitions and registered runtime catalog entries.
// =============================================================================

namespace CCS.Modules.Building
{
    public sealed class CCS_BuildingDefinitionLookup
    {
        #region Variables

        private readonly Dictionary<string, CCS_BuildingPieceDefinition> definitionsById =
            new Dictionary<string, CCS_BuildingPieceDefinition>();

        #endregion

        #region Public Methods

        public CCS_BuildingDefinitionLookup(
            CCS_BuildingProfile profile,
            IReadOnlyDictionary<string, CCS_BuildingPieceDefinition> registeredDefinitions)
        {
            if (profile?.StartupDefinitions != null)
            {
                IReadOnlyList<CCS_BuildingPieceDefinition> startupDefinitions = profile.StartupDefinitions;
                for (int index = 0; index < startupDefinitions.Count; index++)
                {
                    RegisterDefinition(startupDefinitions[index]);
                }
            }

            if (registeredDefinitions == null)
            {
                return;
            }

            foreach (KeyValuePair<string, CCS_BuildingPieceDefinition> entry in registeredDefinitions)
            {
                RegisterDefinition(entry.Value);
            }
        }

        public bool TryResolveDefinition(string pieceId, out CCS_BuildingPieceDefinition definition)
        {
            definition = null;

            if (string.IsNullOrWhiteSpace(pieceId))
            {
                return false;
            }

            return definitionsById.TryGetValue(pieceId, out definition) && definition != null;
        }

        #endregion

        #region Private Methods

        private void RegisterDefinition(CCS_BuildingPieceDefinition definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.PieceId))
            {
                return;
            }

            definitionsById[definition.PieceId] = definition;
        }

        #endregion
    }
}
