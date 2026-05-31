using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingInstanceVisualFactory
// CATEGORY: Modules / Building / Runtime / Placement
// PURPOSE: Spawns primitive visuals for placed and restored building instances.
// PLACEMENT: Called by CCS_BuildingService and CCS_BuildingPlacementService flows.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Cube placeholders only. Creates runtime root when test area is missing.
// =============================================================================

namespace CCS.Modules.Building
{
    public static class CCS_BuildingInstanceVisualFactory
    {
        private const string TestAreaObjectName = "CCS_BuildingTestArea";
        private const string RuntimeVisualRootName = "CCS_BuildingRuntimeVisualRoot";

        #region Variables

        private static readonly Dictionary<string, GameObject> visualsByInstanceId =
            new Dictionary<string, GameObject>();

        #endregion

        #region Public Methods

        public static GameObject SpawnInstanceVisual(
            CCS_BuildingPieceDefinition definition,
            CCS_BuildingInstance instance)
        {
            if (definition == null || instance == null)
            {
                return null;
            }

            if (visualsByInstanceId.TryGetValue(instance.InstanceId, out GameObject existingVisual)
                && existingVisual != null)
            {
                existingVisual.transform.SetPositionAndRotation(instance.Position, instance.Rotation);
                return existingVisual;
            }

            Transform parent = ResolveVisualParent();
            GameObject placedObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            placedObject.name = $"CCS_Placed_{definition.BuildingPieceType}_{instance.InstanceId}";
            placedObject.transform.SetPositionAndRotation(instance.Position, instance.Rotation);

            if (parent != null)
            {
                placedObject.transform.SetParent(parent, true);
            }

            Collider collider = placedObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            visualsByInstanceId[instance.InstanceId] = placedObject;
            return placedObject;
        }

        public static void DestroyAllVisuals()
        {
            foreach (KeyValuePair<string, GameObject> entry in visualsByInstanceId)
            {
                if (entry.Value != null)
                {
                    Object.Destroy(entry.Value);
                }
            }

            visualsByInstanceId.Clear();
        }

        public static void DestroyVisual(string instanceId)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
            {
                return;
            }

            if (visualsByInstanceId.TryGetValue(instanceId, out GameObject visual) && visual != null)
            {
                Object.Destroy(visual);
            }

            visualsByInstanceId.Remove(instanceId);
        }

        #endregion

        #region Private Methods

        private static Transform ResolveVisualParent()
        {
            GameObject testArea = GameObject.Find(TestAreaObjectName);
            if (testArea != null)
            {
                return testArea.transform;
            }

            GameObject runtimeRoot = GameObject.Find(RuntimeVisualRootName);
            if (runtimeRoot == null)
            {
                runtimeRoot = new GameObject(RuntimeVisualRootName);
            }

            return runtimeRoot.transform;
        }

        #endregion
    }
}
