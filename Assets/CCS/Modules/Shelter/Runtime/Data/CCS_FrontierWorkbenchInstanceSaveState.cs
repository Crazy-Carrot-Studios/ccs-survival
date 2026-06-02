using UnityEngine;

namespace CCS.Modules.Shelter
{
    public sealed class CCS_FrontierWorkbenchInstanceSaveState
    {
        public string InstanceId { get; set; } = string.Empty;

        public string WorkbenchDefinitionId { get; set; } = string.Empty;

        public Vector3 Position { get; set; }

        public float RotationY { get; set; }

        public string CampOwnerId { get; set; } = string.Empty;
    }
}
