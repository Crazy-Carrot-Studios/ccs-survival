using UnityEngine;

namespace CCS.Modules.Shelter
{
    public sealed class CCS_FrontierShelterPlacementRequest
    {
        public CCS_FrontierShelterPlacementRequest(
            CCS_ShelterDefinition shelterDefinition,
            Vector3 useOrigin,
            Vector3 useDirection,
            bool confirmPlacement)
        {
            ShelterDefinition = shelterDefinition;
            UseOrigin = useOrigin;
            UseDirection = useDirection.sqrMagnitude > 0.0001f ? useDirection.normalized : Vector3.forward;
            ConfirmPlacement = confirmPlacement;
        }

        public CCS_ShelterDefinition ShelterDefinition { get; }

        public Vector3 UseOrigin { get; }

        public Vector3 UseDirection { get; }

        public bool ConfirmPlacement { get; }
    }
}
