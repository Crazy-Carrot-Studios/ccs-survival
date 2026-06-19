using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ControllerTestPoint
// CATEGORY: Modules / CharacterController / Tests / Runtime
// PURPOSE: Marks master test layout points for validation and future automated tests.
// PLACEMENT: Test point transforms under TestPoints in master test scene.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Test-only metadata. Does not affect gameplay simulation.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests
{
    public enum CCS_ControllerTestPointType
    {
        Unspecified = 0,
        StairsBottom = 1,
        StairsTop = 2,
        RoofCenter = 3,
        RampTop = 4,
        RampBottom = 5,
        DoorOutside = 8,
        DoorInside = 9,
        CoverInside = 10
    }

    public sealed class CCS_ControllerTestPoint : MonoBehaviour
    {
        #region Variables

        [SerializeField] private string testPointId;
        [SerializeField] private string label;
        [SerializeField] private CCS_ControllerTestPointType testPointType;
        [SerializeField] private float radius = 0.6f;

        #endregion

        #region Properties

        public string TestPointId => testPointId;

        public string Label => label;

        public CCS_ControllerTestPointType TestPointType => testPointType;

        public float Radius => radius;

        #endregion
    }
}
