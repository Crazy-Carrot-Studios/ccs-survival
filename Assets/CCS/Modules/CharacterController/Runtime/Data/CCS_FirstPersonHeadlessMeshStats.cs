// =============================================================================
// SCRIPT: CCS_FirstPersonHeadlessMeshStats
// CATEGORY: Modules / CharacterController / Runtime / Data
// PURPOSE: Stores headless mesh bake statistics for debug and validation.
// PLACEMENT: Serialized on CCS_LocalFirstPersonHeadVisibility after editor bake.
// AUTHOR: James Schilz
// CREATED: 2026-06-24
// NOTES: Triangle counts are totals across all submeshes.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public struct CCS_FirstPersonHeadlessMeshStats
    {
        public string MeshAssetPath;
        public string SourceRendererName;
        public int OriginalTriangleCount;
        public int RemainingTriangleCount;
        public int RemovedTriangleCount;

        public int RemovedTriangleCountComputed =>
            OriginalTriangleCount > 0 && RemainingTriangleCount >= 0
                ? OriginalTriangleCount - RemainingTriangleCount
                : RemovedTriangleCount;
    }
}
