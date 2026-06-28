using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AINavigationProbePoint
// CATEGORY: Modules / AI / Runtime / Navigation
// PURPOSE: Marks canonical NavMesh validation probe locations in Master Test scenes.
// PLACEMENT: Child of CCS_AINavigationRoot/NavMeshProbes_MasterTest.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Editor builders place probes; validation uses them for path-complete checks.
// =============================================================================

namespace CCS.Modules.AI
{
    public enum CCS_AINavigationProbeId
    {
        OutsideSpawn = 0,
        BuildingDoor = 1,
        InsideBuilding = 2,
        TopOfStairs = 3,
        RampTop = 4,
    }

    [DisallowMultipleComponent]
    public sealed class CCS_AINavigationProbePoint : MonoBehaviour
    {
        [SerializeField] private CCS_AINavigationProbeId probeId = CCS_AINavigationProbeId.OutsideSpawn;

        public CCS_AINavigationProbeId ProbeId => probeId;

        public void Configure(CCS_AINavigationProbeId configuredProbeId)
        {
            probeId = configuredProbeId;
        }
    }
}
