using UnityEngine;
using UnityEngine.Serialization;

// =============================================================================
// SCRIPT: CCS_TestDoorMarker
// CATEGORY: Modules / CharacterController / Tests / Runtime
// PURPOSE: Data-only marker for test door hinge pivot and swing settings.
// PLACEMENT: PF_CCS_TestDoor_Single root. Not attached elsewhere.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: No Update loop. Future hinge rotation applies to DoorHingePivot around local Y.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests
{
    public sealed class CCS_TestDoorMarker : MonoBehaviour
    {
        #region Serialized Fields

        [FormerlySerializedAs("doorPivot")]
        [SerializeField] private Transform doorHingePivot;
        [SerializeField] private float closedAngle = 0f;
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private bool opensInward = true;

        #endregion

        #region Public Properties

        public Transform DoorHingePivot => doorHingePivot;

        public float ClosedAngle => closedAngle;

        public float OpenAngle => openAngle;

        public bool OpensInward => opensInward;

        #endregion
    }
}
