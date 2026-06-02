using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_MountSnapshot
// CATEGORY: Modules / Mounts / Runtime / Data
// PURPOSE: Serializable ownership and world state for a single owned mount.
// PLACEMENT: Captured by CCS_MountService and persisted through CCS_SaveService.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Mounts
{
    [Serializable]
    public sealed class CCS_MountSnapshot
    {
        public bool ownsMount;
        public string mountDefinitionId = string.Empty;
        public string instanceId = string.Empty;
        public string displayName = "Frontier Horse";
        public int mountState;
        public float positionX;
        public float positionY;
        public float positionZ;
        public float rotationY;
        public string saddlebagInstanceId = string.Empty;

        public Vector3 Position => new Vector3(positionX, positionY, positionZ);

        public CCS_MountState MountState =>
            Enum.IsDefined(typeof(CCS_MountState), mountState)
                ? (CCS_MountState)mountState
                : CCS_MountState.Idle;

        public static CCS_MountSnapshot Empty => new CCS_MountSnapshot();
    }
}
