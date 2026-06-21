using CCS.Modules.Interaction;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_InteractionBuildingDoorBuilder
// CATEGORY: Modules / Interaction / Editor
// PURPOSE: Wires PF_CCS_TestDoor_Single as a walk-through interactable building door.
// PLACEMENT: Editor utility invoked from Build Master Test Interactions menu.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Renames DoorSlab to CCS_BuildingDoor_Interactable and adds interaction components.
// =============================================================================

namespace CCS.Modules.Interaction.Editor
{
    public static class CCS_InteractionBuildingDoorBuilder
    {
        #region Public Methods

        public static bool EnsureTestDoorSinglePrefab()
        {
            int interactableLayer = LayerMask.NameToLayer(CCS_InteractionConstants.InteractableLayerName);
            if (interactableLayer < 0)
            {
                Debug.LogError("[Interaction Door Builder] Interactable layer was not found.");
                return false;
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(CCS_InteractionConstants.TestDoorSinglePrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError(
                    "[Interaction Door Builder] Missing prefab: "
                    + CCS_InteractionConstants.TestDoorSinglePrefabPath);
                return false;
            }

            try
            {
                bool changed = EnsureDoorInteractableOnRoot(prefabRoot, interactableLayer);
                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, CCS_InteractionConstants.TestDoorSinglePrefabPath);
                }

                return changed;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        public static bool EnsureSceneBuildingDoors(int interactableLayer)
        {
            bool changed = false;
            Transform[] allTransforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < allTransforms.Length; i++)
            {
                Transform candidate = allTransforms[i];
                if (candidate == null
                    || candidate.name != CCS_InteractionConstants.TestDoorSingleRootObjectName)
                {
                    continue;
                }

                changed |= EnsureDoorInteractableOnRoot(candidate.gameObject, interactableLayer);
            }

            return changed;
        }

        public static bool RemoveLegacyWalkThroughDoorSlab()
        {
            GameObject legacyDoor = FindSceneObjectByName(CCS_InteractionConstants.TestWalkThroughDoorObjectName);
            if (legacyDoor == null)
            {
                return false;
            }

            Undo.DestroyObjectImmediate(legacyDoor);
            return true;
        }

        #endregion

        #region Private Methods

        private static bool EnsureDoorInteractableOnRoot(GameObject doorRoot, int interactableLayer)
        {
            Transform hingePivot = doorRoot.transform.Find(CCS_InteractionConstants.TestDoorHingePivotObjectName);
            if (hingePivot == null)
            {
                Debug.LogWarning(
                    "[Interaction Door Builder] Door hinge pivot was not found on "
                    + doorRoot.name,
                    doorRoot);
                return false;
            }

            Transform interactableTransform = hingePivot.Find(CCS_InteractionConstants.BuildingDoorInteractableObjectName);
            if (interactableTransform == null)
            {
                interactableTransform = hingePivot.Find(CCS_InteractionConstants.TestDoorSlabObjectName);
            }

            if (interactableTransform == null)
            {
                Debug.LogWarning(
                    "[Interaction Door Builder] Door slab was not found under "
                    + hingePivot.name,
                    doorRoot);
                return false;
            }

            GameObject interactableObject = interactableTransform.gameObject;
            bool changed = false;

            if (interactableObject.name != CCS_InteractionConstants.BuildingDoorInteractableObjectName)
            {
                interactableObject.name = CCS_InteractionConstants.BuildingDoorInteractableObjectName;
                changed = true;
            }

            if (interactableObject.layer != interactableLayer)
            {
                interactableObject.layer = interactableLayer;
                changed = true;
            }

            if (!interactableObject.CompareTag(CCS_InteractionConstants.InteractableTagName))
            {
                interactableObject.tag = CCS_InteractionConstants.InteractableTagName;
                changed = true;
            }

            BoxCollider boxCollider = interactableObject.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = interactableObject.AddComponent<BoxCollider>();
                changed = true;
            }

            if (!boxCollider.enabled)
            {
                boxCollider.enabled = true;
                changed = true;
            }

            if (boxCollider.isTrigger)
            {
                boxCollider.isTrigger = false;
                changed = true;
            }

            CCS_InteractableLabelTarget labelTarget = interactableObject.GetComponent<CCS_InteractableLabelTarget>();
            if (labelTarget == null)
            {
                labelTarget = interactableObject.AddComponent<CCS_InteractableLabelTarget>();
                changed = true;
            }

            labelTarget.ConfigureWalkThroughDoor(
                CCS_InteractionConstants.BuildingDoorDisplayName,
                CCS_InteractionConstants.DefaultWalkThroughDoorStrictRange);

            if (interactableObject.GetComponent<CCS_InteractableExecutor>() == null)
            {
                interactableObject.AddComponent<CCS_InteractableExecutor>();
                changed = true;
            }

            CCS_InteractableDoor door = interactableObject.GetComponent<CCS_InteractableDoor>();
            if (door == null)
            {
                door = interactableObject.AddComponent<CCS_InteractableDoor>();
                changed = true;
            }

            changed |= ConfigureDoorComponent(door, hingePivot);

            if (changed)
            {
                EditorUtility.SetDirty(interactableObject);
                EditorUtility.SetDirty(doorRoot);
            }

            return changed;
        }

        private static bool ConfigureDoorComponent(CCS_InteractableDoor door, Transform hingePivot)
        {
            SerializedObject serializedDoor = new SerializedObject(door);
            bool changed = false;
            changed |= SetObjectReference(serializedDoor, "doorPivot", hingePivot);
            changed |= SetFloat(serializedDoor, "openAngle", 90f);
            changed |= SetFloat(serializedDoor, "openDuration", 0.35f);
            changed |= SetBool(serializedDoor, "opensInward", true);
            changed |= SetBool(serializedDoor, "startsOpen", false);

            if (changed)
            {
                serializedDoor.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static GameObject FindSceneObjectByName(string objectName)
        {
            Transform[] transforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform candidate = transforms[i];
                if (candidate != null && candidate.name == objectName)
                {
                    return candidate.gameObject;
                }
            }

            return null;
        }

        private static bool SetObjectReference(SerializedObject serializedObject, string propertyName, Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == value)
            {
                return false;
            }

            property.objectReferenceValue = value;
            return true;
        }

        private static bool SetFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || Mathf.Approximately(property.floatValue, value))
            {
                return false;
            }

            property.floatValue = value;
            return true;
        }

        private static bool SetBool(SerializedObject serializedObject, string propertyName, bool value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.boolValue == value)
            {
                return false;
            }

            property.boolValue = value;
            return true;
        }

        #endregion
    }
}
