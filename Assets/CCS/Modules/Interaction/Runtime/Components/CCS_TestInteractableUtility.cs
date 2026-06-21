using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TestInteractableUtility
// CATEGORY: Modules / Interaction / Runtime / Components
// PURPOSE: Shared helpers for placing and configuring Master Test interactables.
// PLACEMENT: Runtime static utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Configures pickup cube and walk-through door test objects.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public static class CCS_TestInteractableUtility
    {
        #region Public Methods

        public static Transform FindSpawnOrigin()
        {
            GameObject testPoints = GameObject.Find("TestPoints");
            if (testPoints == null)
            {
                return null;
            }

            return testPoints.transform.Find("TP_Spawn_Host");
        }

        public static Vector3 GetDetectionCubeWorldPosition(Transform spawnOrigin)
        {
            Vector3 position = spawnOrigin.position
                + spawnOrigin.forward * CCS_InteractionConstants.TestDetectionCubeForwardDistance;
            position.y = Mathf.Max(
                CCS_InteractionConstants.TestDetectionCubeHeightAboveGround,
                spawnOrigin.position.y + 0.75f);
            return position;
        }

        public static Vector3 GetWalkThroughDoorWorldPosition(Transform spawnOrigin)
        {
            Vector3 position = spawnOrigin.position
                + spawnOrigin.forward * CCS_InteractionConstants.TestWalkThroughDoorForwardDistance
                + spawnOrigin.right * CCS_InteractionConstants.TestWalkThroughDoorLateralOffset;
            position.y = Mathf.Max(
                CCS_InteractionConstants.TestDetectionCubeHeightAboveGround,
                spawnOrigin.position.y + 1f);
            return position;
        }

        public static bool TryConfigurePickupCube(GameObject cubeObject, int interactableLayer)
        {
            return TryConfigureInteractable(
                cubeObject,
                interactableLayer,
                CCS_InteractionConstants.TestDetectionCubeObjectName,
                CCS_InteractionKind.Pickup,
                CCS_InteractionConstants.TestDetectionCubeDisplayName,
                new Vector3(1f, 1f, 1f),
                new Color(0.2f, 0.55f, 0.95f, 1f));
        }

        public static bool TryConfigureWalkThroughDoor(GameObject doorObject, int interactableLayer)
        {
            return TryConfigureInteractable(
                doorObject,
                interactableLayer,
                CCS_InteractionConstants.TestWalkThroughDoorObjectName,
                CCS_InteractionKind.WalkThroughDoor,
                CCS_InteractionConstants.TestWalkThroughDoorDisplayName,
                new Vector3(0.35f, 2.2f, 0.15f),
                new Color(0.55f, 0.4f, 0.25f, 1f));
        }

        #endregion

        #region Private Methods

        private static bool TryConfigureInteractable(
            GameObject targetObject,
            int interactableLayer,
            string objectName,
            CCS_InteractionKind interactionKind,
            string displayName,
            Vector3 localScale,
            Color materialColor)
        {
            if (targetObject == null)
            {
                return false;
            }

            targetObject.name = objectName;
            targetObject.tag = CCS_InteractionConstants.InteractableTagName;
            targetObject.layer = interactableLayer;
            targetObject.transform.localScale = localScale;

            BoxCollider boxCollider = targetObject.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = targetObject.AddComponent<BoxCollider>();
            }

            boxCollider.enabled = true;
            boxCollider.isTrigger = false;

            CCS_InteractableLabelTarget labelTarget = targetObject.GetComponent<CCS_InteractableLabelTarget>();
            if (labelTarget == null)
            {
                labelTarget = targetObject.AddComponent<CCS_InteractableLabelTarget>();
            }

            labelTarget.ConfigureForKind(interactionKind, displayName);

            if (targetObject.GetComponent<CCS_InteractableExecutor>() == null)
            {
                targetObject.AddComponent<CCS_InteractableExecutor>();
            }

            ApplyMaterial(targetObject, materialColor);
            return true;
        }

        private static void ApplyMaterial(GameObject targetObject, Color color)
        {
            MeshRenderer renderer = targetObject.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                return;
            }

            Material material = renderer.sharedMaterial;
            if (material != null && !material.name.Contains("Default"))
            {
                return;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                return;
            }

            renderer.sharedMaterial = new Material(shader)
            {
                color = color
            };
        }

        #endregion
    }
}
