using UnityEngine;

namespace CCS.Modules.Shelter
{
    public sealed class CCS_FrontierShelterInstanceSaveState
    {
        public string InstanceId = string.Empty;
        public string ShelterDefinitionId = string.Empty;
        public Vector3 Position;
        public float RotationY;
        public string CampOwnerId = string.Empty;
    }
}
