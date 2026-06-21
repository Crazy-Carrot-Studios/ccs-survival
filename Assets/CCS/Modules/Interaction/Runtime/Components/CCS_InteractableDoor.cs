using System.Collections;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_InteractableDoor
// CATEGORY: Modules / Interaction / Runtime / Components
// PURPOSE: Rotates a door pivot open after a successful walk-through interaction.
// PLACEMENT: CCS_BuildingDoor_Interactable on PF_CCS_TestDoor_Single.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Open-only for now. Collider rotates with the door pivot child hierarchy.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public sealed class CCS_InteractableDoor : MonoBehaviour
    {
        #region Variables

        [SerializeField] private Transform doorPivot;
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float openDuration = 0.35f;
        [SerializeField] private bool opensInward = true;
        [SerializeField] private bool startsOpen = false;

        private bool isOpen;
        private Coroutine openCoroutine;
        private Quaternion closedLocalRotation;
        private bool hasCachedClosedRotation;

        #endregion

        #region Properties

        public bool IsOpen => isOpen;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveDoorPivot();
            CacheClosedRotation();

            if (startsOpen)
            {
                ApplyImmediateOpenRotation();
                isOpen = true;
            }
        }

        #endregion

        #region Public Methods

        public void Open()
        {
            if (isOpen || doorPivot == null)
            {
                return;
            }

            CacheClosedRotation();
            if (openCoroutine != null)
            {
                StopCoroutine(openCoroutine);
            }

            openCoroutine = StartCoroutine(OpenDoorRoutine());
        }

        #endregion

        #region Private Methods

        private void ResolveDoorPivot()
        {
            if (doorPivot != null)
            {
                return;
            }

            Transform hinge = transform.parent;
            if (hinge != null && hinge.name.Contains("Hinge"))
            {
                doorPivot = hinge;
            }
        }

        private void CacheClosedRotation()
        {
            if (doorPivot == null || hasCachedClosedRotation)
            {
                return;
            }

            closedLocalRotation = doorPivot.localRotation;
            hasCachedClosedRotation = true;
        }

        private void ApplyImmediateOpenRotation()
        {
            if (doorPivot == null)
            {
                return;
            }

            doorPivot.localRotation = closedLocalRotation * Quaternion.Euler(0f, GetSignedOpenAngle(), 0f);
        }

        private float GetSignedOpenAngle()
        {
            return opensInward ? -openAngle : openAngle;
        }

        private IEnumerator OpenDoorRoutine()
        {
            Quaternion startRotation = doorPivot.localRotation;
            Quaternion endRotation = closedLocalRotation * Quaternion.Euler(0f, GetSignedOpenAngle(), 0f);
            float elapsed = 0f;

            while (elapsed < openDuration)
            {
                elapsed += Time.deltaTime;
                float t = openDuration > 0f ? Mathf.Clamp01(elapsed / openDuration) : 1f;
                doorPivot.localRotation = Quaternion.Slerp(startRotation, endRotation, t);
                yield return null;
            }

            doorPivot.localRotation = endRotation;
            isOpen = true;
            openCoroutine = null;
        }

        #endregion
    }
}
