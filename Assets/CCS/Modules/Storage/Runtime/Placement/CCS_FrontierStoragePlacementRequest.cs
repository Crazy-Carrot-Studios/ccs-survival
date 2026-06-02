using UnityEngine;

namespace CCS.Modules.Storage
{
    public sealed class CCS_FrontierStoragePlacementRequest
    {
        public CCS_FrontierStoragePlacementRequest(
            string containerDefinitionId,
            Vector3 useOrigin,
            Vector3 useDirection,
            bool confirmPlacement)
        {
            ContainerDefinitionId = containerDefinitionId ?? string.Empty;
            UseOrigin = useOrigin;
            UseDirection = useDirection;
            ConfirmPlacement = confirmPlacement;
        }

        public string ContainerDefinitionId { get; }

        public Vector3 UseOrigin { get; }

        public Vector3 UseDirection { get; }

        public bool ConfirmPlacement { get; }
    }
}
