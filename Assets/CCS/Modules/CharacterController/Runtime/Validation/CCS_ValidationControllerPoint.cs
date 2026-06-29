using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ValidationControllerPoint
// CATEGORY: Modules / CharacterController / Tests / Runtime
// PURPOSE: Marks master test layout points for validation and future automated tests.
// PLACEMENT: Test point transforms under TestPoints in master test scene.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Test-only metadata. Does not affect gameplay simulation.
// =============================================================================

namespace CCS.Modules.CharacterController.Validation {
    public enum CCS_ValidationControllerPointType
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

    public sealed class CCS_ValidationControllerPoint : MonoBehaviour
    {
        #region Variables

        [SerializeField] private string testPointId;
        [SerializeField] private string label;
        [SerializeField] private CCS_ValidationControllerPointType testPointType;
        [SerializeField] private float radius = 0.6f;

        #endregion

        #region Properties

        public string TestPointId => testPointId;

        public string Label => label;

        public CCS_ValidationControllerPointType TestPointType => testPointType;

        public float Radius => radius;

        #endregion
    }
}
