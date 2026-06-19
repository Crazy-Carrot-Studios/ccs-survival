using CCS.Modules.CharacterController.Tests;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterControllerMasterTestEnvironmentPrefabBuilder
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Rebuilds master test environment prefabs with exact layout specs.
// PLACEMENT: Editor utility. Invoked from master test setup only.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Produces modular building, ramp, and door prefabs as source of truth.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerMasterTestEnvironmentPrefabBuilder
    {
        #region Public Methods

        public static bool RebuildEnvironmentPrefabs()
        {
            bool changed = false;
            changed |= RebuildDoorPrefab();
            changed |= RebuildRampPrefab();
            changed |= RebuildBuildingPrefab();
            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return changed;
        }

        #endregion

        #region Private Methods

        private static bool RebuildDoorPrefab()
        {
            Material wood = LoadMaterial(CCS_CharacterControllerMasterTestLayoutConstants.DoorWoodMaterialPath);
            if (wood == null)
            {
                return false;
            }

            GameObject root = new GameObject(CCS_CharacterControllerMasterTestLayoutConstants.DoorInstanceName);

            GameObject pivotObject = new GameObject("DoorHingePivot");
            pivotObject.transform.SetParent(root.transform, false);
            pivotObject.transform.localPosition =
                CCS_CharacterControllerMasterTestLayoutConstants.DoorHingePivotLocalPosition;
            pivotObject.AddComponent<CCS_TestDoorHingeGizmo>();

            CreateBox(
                pivotObject.transform,
                "DoorSlab",
                CCS_CharacterControllerMasterTestLayoutConstants.DoorSlabLocalPositionFromPivot,
                new Vector3(
                    CCS_CharacterControllerMasterTestLayoutConstants.DoorSlabThicknessMeters,
                    CCS_CharacterControllerMasterTestLayoutConstants.DoorSlabHeightMeters,
                    CCS_CharacterControllerMasterTestLayoutConstants.DoorSlabWidthMeters),
                wood,
                includeCollider: true);

            CCS_TestDoorMarker marker = root.AddComponent<CCS_TestDoorMarker>();
            SerializedObject serializedMarker = new SerializedObject(marker);
            serializedMarker.FindProperty("doorHingePivot").objectReferenceValue = pivotObject.transform;
            serializedMarker.FindProperty("closedAngle").floatValue = 0f;
            serializedMarker.FindProperty("openAngle").floatValue = 90f;
            serializedMarker.FindProperty("opensInward").boolValue = true;
            serializedMarker.ApplyModifiedPropertiesWithoutUndo();

            bool saved = SavePrefab(root, CCS_CharacterControllerMasterTestLayoutConstants.DoorPrefabPath);
            Object.DestroyImmediate(root);
            return saved;
        }

        private static bool RebuildRampPrefab()
        {
            Material concrete = LoadMaterial(CCS_CharacterControllerMasterTestLayoutConstants.ConcreteMaterialPath);
            if (concrete == null)
            {
                return false;
            }

            GameObject root = new GameObject(CCS_CharacterControllerMasterTestLayoutConstants.RampInstanceName);

            float run = CCS_CharacterControllerMasterTestLayoutConstants.RampRunMeters;
            float rise = CCS_CharacterControllerMasterTestLayoutConstants.RampRiseMeters;
            Vector3 high = CCS_CharacterControllerMasterTestLayoutConstants.RampHighEndpointLocal;
            Vector3 low = CCS_CharacterControllerMasterTestLayoutConstants.RampLowEndpointLocal;
            Vector3 center = (high + low) * 0.5f;
            float slopeAngle = Mathf.Atan2(rise, run) * Mathf.Rad2Deg;

            CreateBox(
                root.transform,
                "RampSurface",
                center,
                new Vector3(3f, 0.2f, run),
                concrete,
                includeCollider: true,
                localRotation: Quaternion.Euler(-slopeAngle, 0f, 0f));

            bool saved = SavePrefab(root, CCS_CharacterControllerMasterTestLayoutConstants.RampPrefabPath);
            Object.DestroyImmediate(root);
            return saved;
        }

        private static bool RebuildBuildingPrefab()
        {
            Material brick = LoadMaterial(CCS_CharacterControllerMasterTestLayoutConstants.BrickMaterialPath);
            GameObject doorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_CharacterControllerMasterTestLayoutConstants.DoorPrefabPath);
            if (brick == null || doorPrefab == null)
            {
                return false;
            }

            GameObject root = new GameObject(CCS_CharacterControllerMasterTestLayoutConstants.BuildingInstanceName);

            float wallY = 1.45f;
            float wallHeight = 2.9f;
            float wallDepth = CCS_CharacterControllerMasterTestLayoutConstants.WallThicknessMeters;
            float frontZ = CCS_CharacterControllerMasterTestLayoutConstants.WallFrontCenterLocalZ;
            float backZ = CCS_CharacterControllerMasterTestLayoutConstants.WallBackCenterLocalZ;
            float leftX = CCS_CharacterControllerMasterTestLayoutConstants.WallLeftCenterLocalX;
            float rightX = CCS_CharacterControllerMasterTestLayoutConstants.WallRightCenterLocalX;

            CreateBox(root.transform, "Wall_Back", new Vector3(0f, wallY, backZ), new Vector3(8f, wallHeight, wallDepth), brick);
            CreateBox(root.transform, "Wall_Front", new Vector3(0f, wallY, frontZ), new Vector3(8f, wallHeight, wallDepth), brick);
            CreateBox(root.transform, "Wall_Left", new Vector3(leftX, wallY, 0f), new Vector3(wallDepth, wallHeight, 10f), brick);
            CreateBox(root.transform, "Wall_Right_Back", new Vector3(rightX, wallY, -3f), new Vector3(wallDepth, wallHeight, 4f), brick);
            CreateBox(root.transform, "Wall_Right_Front", new Vector3(rightX, wallY, 3f), new Vector3(wallDepth, wallHeight, 4f), brick);
            CreateBox(root.transform, "Wall_Right_DoorHeader", new Vector3(rightX, 2.7f, 0f), new Vector3(wallDepth, 0.4f, 2f), brick);
            CreateBox(
                root.transform,
                "RoofDeck",
                new Vector3(0f, CCS_CharacterControllerMasterTestLayoutConstants.RoofDeckCenterYMeters, 0f),
                new Vector3(8f, CCS_CharacterControllerMasterTestLayoutConstants.RoofDeckThicknessMeters, 10f),
                brick);

            GameObject doorInstance = PrefabUtility.InstantiatePrefab(doorPrefab, root.transform) as GameObject;
            if (doorInstance != null)
            {
                doorInstance.name = CCS_CharacterControllerMasterTestLayoutConstants.DoorInstanceName;
                doorInstance.transform.localPosition = CCS_CharacterControllerMasterTestLayoutConstants.DoorLocalPosition;
                doorInstance.transform.localRotation = Quaternion.Euler(
                    CCS_CharacterControllerMasterTestLayoutConstants.DoorLocalEuler);
            }

            bool saved = SavePrefab(root, CCS_CharacterControllerMasterTestLayoutConstants.BuildingPrefabPath);
            Object.DestroyImmediate(root);
            return saved;
        }

        private static Material LoadMaterial(string path)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Debug.LogError($"[Environment Prefab Builder] Missing material: {path}");
            }

            return material;
        }

        private static void CreateBox(
            Transform parent,
            string objectName,
            Vector3 localPosition,
            Vector3 size,
            Material material,
            bool includeCollider = true,
            Quaternion localRotation = default)
        {
            GameObject boxObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boxObject.name = objectName;
            boxObject.transform.SetParent(parent, false);
            boxObject.transform.localPosition = localPosition;
            boxObject.transform.localRotation = localRotation == default ? Quaternion.identity : localRotation;
            boxObject.transform.localScale = size;

            MeshRenderer renderer = boxObject.GetComponent<MeshRenderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            if (!includeCollider)
            {
                Object.DestroyImmediate(boxObject.GetComponent<Collider>());
            }
        }

        private static bool SavePrefab(GameObject root, string prefabPath)
        {
            bool success;
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath, out success);
            if (!success)
            {
                Debug.LogError($"[Environment Prefab Builder] Failed to save prefab: {prefabPath}");
            }

            return success;
        }

        #endregion
    }
}
