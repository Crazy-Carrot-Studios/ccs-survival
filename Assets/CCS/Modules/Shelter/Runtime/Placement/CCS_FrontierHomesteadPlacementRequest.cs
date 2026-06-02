using UnityEngine;

namespace CCS.Modules.Shelter
{
    public sealed class CCS_FrontierHomesteadPlacementRequest
    {
        public CCS_FrontierHomesteadPlacementRequest(
            CCS_CampStructureKind structureKind,
            string definitionId,
            Vector3 useOrigin,
            Vector3 useDirection,
            bool confirmPlacement)
        {
            StructureKind = structureKind;
            DefinitionId = definitionId ?? string.Empty;
            UseOrigin = useOrigin;
            UseDirection = useDirection;
            ConfirmPlacement = confirmPlacement;
        }

        public CCS_CampStructureKind StructureKind { get; }

        public string DefinitionId { get; }

        public Vector3 UseOrigin { get; }

        public Vector3 UseDirection { get; }

        public bool ConfirmPlacement { get; }
    }
}
