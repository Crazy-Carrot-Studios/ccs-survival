using System.Collections.Generic;
using System.IO;
using CCS.Modules.Inventory;
using CCS.Survival.Composition;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingProgressionBootstrapSetup
// CATEGORY: Modules / Building / Editor / Validation
// PURPOSE: Creates tier-1 primitive building definitions, recipes, and bootstrap wiring.
// PLACEMENT: Batch entry for milestone 1.1.0 building progression foundation.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Uses primitive wood/stick inventory items from survival content.
// =============================================================================

namespace CCS.Modules.Building.Editor
{
    public static class CCS_BuildingProgressionBootstrapSetup
    {
        private const string ProfilesRoot = "Assets/CCS/Survival/Profiles/Building";
        private const string ProgressionProfilePath = ProfilesRoot + "/CCS_DefaultBuildingProgressionProfile.asset";
        private const string BuildingProfilePath = ProfilesRoot + "/CCS_DefaultBuildingProfile.asset";
        private const string PrimitiveDefinitionsRoot = "Assets/CCS/Survival/Content/Building/Primitive";
        private const string PrimitivePrefabsRoot = PrimitiveDefinitionsRoot + "/Prefabs";
        private const string WoodItemPath = "Assets/CCS/Survival/Content/Items/Resources/Primitive/CCS_Item_Wood.asset";
        private const string StickItemPath = "Assets/CCS/Survival/Content/Items/Resources/Primitive/CCS_Item_Stick.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string LogPrefix = "[CCS_BuildingProgressionBootstrapSetup]";

        private const string FoundationPieceId = "ccs.survival.building.primitive.foundation";
        private const string WallPieceId = "ccs.survival.building.primitive.wall";
        private const string DoorwayPieceId = "ccs.survival.building.primitive.doorway";
        private const string FloorPieceId = "ccs.survival.building.primitive.floor";
        private const string RoofPieceId = "ccs.survival.building.primitive.roof";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureFolders();

            CCS_ItemDefinition woodItem = LoadItem(WoodItemPath);
            CCS_ItemDefinition stickItem = LoadItem(StickItemPath);

            CCS_BuildingPieceDefinition foundation = EnsurePrimitiveDefinition(
                PrimitiveDefinitionsRoot + "/CCS_PrimitiveFoundation.asset",
                PrimitivePrefabsRoot + "/PF_CCS_PrimitiveFoundation.prefab",
                FoundationPieceId,
                "Primitive Foundation",
                CCS_BuildingPieceCategory.Foundation,
                CCS_BuildingPieceType.Foundation,
                new Vector3(2f, 0.35f, 2f),
                (woodItem, 4),
                (stickItem, 2));
            CCS_BuildingPieceDefinition wall = EnsurePrimitiveDefinition(
                PrimitiveDefinitionsRoot + "/CCS_PrimitiveWall.asset",
                PrimitivePrefabsRoot + "/PF_CCS_PrimitiveWall.prefab",
                WallPieceId,
                "Primitive Wall",
                CCS_BuildingPieceCategory.Wall,
                CCS_BuildingPieceType.Wall,
                new Vector3(2f, 1.5f, 0.25f),
                (woodItem, 3));
            CCS_BuildingPieceDefinition doorway = EnsurePrimitiveDefinition(
                PrimitiveDefinitionsRoot + "/CCS_PrimitiveDoorway.asset",
                PrimitivePrefabsRoot + "/PF_CCS_PrimitiveDoorway.prefab",
                DoorwayPieceId,
                "Primitive Doorway Wall",
                CCS_BuildingPieceCategory.Doorway,
                CCS_BuildingPieceType.Doorway,
                new Vector3(2f, 1.5f, 0.25f),
                (woodItem, 4));
            CCS_BuildingPieceDefinition floor = EnsurePrimitiveDefinition(
                PrimitiveDefinitionsRoot + "/CCS_PrimitiveFloor.asset",
                PrimitivePrefabsRoot + "/PF_CCS_PrimitiveFloor.prefab",
                FloorPieceId,
                "Primitive Floor",
                CCS_BuildingPieceCategory.Floor,
                CCS_BuildingPieceType.Floor,
                new Vector3(2f, 0.2f, 2f),
                (woodItem, 2));
            CCS_BuildingPieceDefinition roof = EnsurePrimitiveDefinition(
                PrimitiveDefinitionsRoot + "/CCS_PrimitiveRoof.asset",
                PrimitivePrefabsRoot + "/PF_CCS_PrimitiveRoof.prefab",
                RoofPieceId,
                "Primitive Roof",
                CCS_BuildingPieceCategory.Roof,
                CCS_BuildingPieceType.Roof,
                new Vector3(2f, 0.25f, 2f),
                (woodItem, 4),
                (stickItem, 2));

            ConfigureFoundationPlacement(foundation);
            ConfigureWallPlacement(wall);
            ConfigureDoorwayPlacement(doorway);
            ConfigureFloorPlacement(floor);
            ConfigureRoofPlacement(roof);

            List<CCS_BuildingPieceDefinition> enabledPieces = new List<CCS_BuildingPieceDefinition>
            {
                foundation,
                wall,
                doorway,
                floor,
                roof
            };

            CCS_BuildingProgressionProfile progressionProfile = EnsureProgressionProfile(
                enabledPieces,
                foundation,
                wall,
                doorway,
                floor,
                roof,
                woodItem,
                stickItem);

            MergeBuildingProfileStartupDefinitions(enabledPieces);
            EnsureBootstrapGameplayServiceHost(progressionProfile);
            UpdateProjectVersion();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Building progression bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Content/Building");
            EnsureFolder(PrimitiveDefinitionsRoot);
            EnsureFolder(PrimitivePrefabsRoot);
            EnsureFolder(ProfilesRoot);
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/') ?? "Assets";
            string folderName = Path.GetFileName(folderPath);
            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static CCS_ItemDefinition LoadItem(string assetPath)
        {
            CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(assetPath);
            if (item == null)
            {
                Debug.LogError($"{LogPrefix} Missing item asset: {assetPath}");
                EditorApplication.Exit(1);
            }

            return item;
        }

        private static CCS_BuildingPieceDefinition EnsurePrimitiveDefinition(
            string assetPath,
            string prefabPath,
            string pieceId,
            string displayName,
            CCS_BuildingPieceCategory pieceCategory,
            CCS_BuildingPieceType pieceType,
            Vector3 prefabScale,
            params (CCS_ItemDefinition itemDefinition, int quantity)[] buildCosts)
        {
            GameObject prefabReference = EnsurePrimitivePrefab(prefabPath, displayName, prefabScale);

            CCS_BuildingPieceDefinition definition =
                AssetDatabase.LoadAssetAtPath<CCS_BuildingPieceDefinition>(assetPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_BuildingPieceDefinition>();
                AssetDatabase.CreateAsset(definition, assetPath);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("pieceId").stringValue = pieceId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("description").stringValue =
                $"Tier-1 primitive {displayName.ToLowerInvariant()} for milestone 1.1.0.";
            serialized.FindProperty("buildingPieceType").enumValueIndex = (int)pieceType;
            serialized.FindProperty("pieceCategory").enumValueIndex = (int)pieceCategory;
            serialized.FindProperty("prefabReference").objectReferenceValue = prefabReference;
            SetBuildCostEntries(serialized, buildCosts);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static GameObject EnsurePrimitivePrefab(string prefabPath, string prefabName, Vector3 scale)
        {
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                return existingPrefab;
            }

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = prefabName;
            cube.transform.localScale = scale;

            Collider collider = cube.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            PrefabUtility.SaveAsPrefabAsset(cube, prefabPath);
            Object.DestroyImmediate(cube);
            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        private static void SetBuildCostEntries(
            SerializedObject serializedDefinition,
            (CCS_ItemDefinition itemDefinition, int quantity)[] buildCosts)
        {
            SerializedProperty costList = serializedDefinition.FindProperty("buildCostEntries");
            costList.ClearArray();
            if (buildCosts == null)
            {
                return;
            }

            for (int index = 0; index < buildCosts.Length; index++)
            {
                costList.InsertArrayElementAtIndex(index);
                SerializedProperty entry = costList.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("itemDefinition").objectReferenceValue = buildCosts[index].itemDefinition;
                entry.FindPropertyRelative("quantity").intValue = buildCosts[index].quantity;
            }
        }

        private static void ConfigureFoundationPlacement(CCS_BuildingPieceDefinition definition)
        {
            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("allowsFreePlacement").boolValue = true;
            serialized.FindProperty("requiresSnapPoint").boolValue = false;
            SetSnapPoints(serialized, ("foundation_edge_top", CCS_BuildingSnapPointType.FoundationEdge, new Vector3(0f, 0.5f, 0f)));
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
        }

        private static void ConfigureWallPlacement(CCS_BuildingPieceDefinition definition)
        {
            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("allowsFreePlacement").boolValue = false;
            serialized.FindProperty("requiresSnapPoint").boolValue = true;
            SetSnapPoints(
                serialized,
                ("wall_bottom", CCS_BuildingSnapPointType.WallBottom, new Vector3(0f, -0.5f, 0f)),
                ("wall_top", CCS_BuildingSnapPointType.WallTop, new Vector3(0f, 0.5f, 0f)));
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
        }

        private static void ConfigureDoorwayPlacement(CCS_BuildingPieceDefinition definition)
        {
            ConfigureWallPlacement(definition);
        }

        private static void ConfigureFloorPlacement(CCS_BuildingPieceDefinition definition)
        {
            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("allowsFreePlacement").boolValue = false;
            serialized.FindProperty("requiresSnapPoint").boolValue = true;
            SetSnapPoints(serialized, ("floor_edge", CCS_BuildingSnapPointType.FoundationEdge, new Vector3(0f, 0f, 0f)));
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
        }

        private static void ConfigureRoofPlacement(CCS_BuildingPieceDefinition definition)
        {
            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("allowsFreePlacement").boolValue = false;
            serialized.FindProperty("requiresSnapPoint").boolValue = true;
            SetSnapPoints(serialized, ("roof_edge", CCS_BuildingSnapPointType.RoofEdge, new Vector3(0f, -0.5f, 0f)));
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
        }

        private static void SetSnapPoints(
            SerializedObject serializedDefinition,
            params (string snapPointId, CCS_BuildingSnapPointType snapPointType, Vector3 localPosition)[] snapPoints)
        {
            SerializedProperty snapPointList = serializedDefinition.FindProperty("snapPoints");
            snapPointList.ClearArray();
            for (int index = 0; index < snapPoints.Length; index++)
            {
                snapPointList.InsertArrayElementAtIndex(index);
                SerializedProperty entry = snapPointList.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("snapPointId").stringValue = snapPoints[index].snapPointId;
                entry.FindPropertyRelative("snapPointType").enumValueIndex = (int)snapPoints[index].snapPointType;
                entry.FindPropertyRelative("localPosition").vector3Value = snapPoints[index].localPosition;
                entry.FindPropertyRelative("localEulerAngles").vector3Value = Vector3.zero;
            }
        }

        private static CCS_BuildingProgressionProfile EnsureProgressionProfile(
            List<CCS_BuildingPieceDefinition> enabledPieces,
            CCS_BuildingPieceDefinition foundation,
            CCS_BuildingPieceDefinition wall,
            CCS_BuildingPieceDefinition doorway,
            CCS_BuildingPieceDefinition floor,
            CCS_BuildingPieceDefinition roof,
            CCS_ItemDefinition woodItem,
            CCS_ItemDefinition stickItem)
        {
            CCS_BuildingProgressionProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_BuildingProgressionProfile>(ProgressionProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_BuildingProgressionProfile>();
                AssetDatabase.CreateAsset(profile, ProgressionProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileDisplayName").stringValue = "Default Building Progression";
            serialized.FindProperty("profileId").stringValue = "ccs.survival.profile.building.progression.default";
            serialized.FindProperty("profileDescription").stringValue =
                "Tier-1 primitive shelter building progression for milestone 1.1.0.";
            serialized.FindProperty("profileVersion").stringValue = "1.1.0";
            serialized.FindProperty("progressionEnabled").boolValue = true;
            serialized.FindProperty("enableDebugLogging").boolValue = true;
            serialized.FindProperty("minimumFoundationCount").intValue = 1;
            serialized.FindProperty("minimumWallCount").intValue = 1;
            serialized.FindProperty("minimumRoofCount").intValue = 1;

            SerializedProperty enabledList = serialized.FindProperty("enabledPieceDefinitions");
            enabledList.ClearArray();
            for (int index = 0; index < enabledPieces.Count; index++)
            {
                enabledList.InsertArrayElementAtIndex(index);
                enabledList.GetArrayElementAtIndex(index).objectReferenceValue = enabledPieces[index];
            }

            SerializedProperty recipeList = serialized.FindProperty("recipeDefinitions");
            recipeList.ClearArray();
            AddRecipe(recipeList, 0, "ccs.survival.building.recipe.primitive.foundation", "Primitive Foundation", CCS_BuildingPieceCategory.Foundation, foundation, woodItem, 4, stickItem, 2, freePlace: true);
            AddRecipe(recipeList, 1, "ccs.survival.building.recipe.primitive.wall", "Primitive Wall", CCS_BuildingPieceCategory.Wall, wall, woodItem, 3, requiresFoundation: true, requiresSnap: true);
            AddRecipe(recipeList, 2, "ccs.survival.building.recipe.primitive.doorway", "Primitive Doorway", CCS_BuildingPieceCategory.Doorway, doorway, woodItem, 4, requiresFoundation: true, requiresSnap: true);
            AddRecipe(recipeList, 3, "ccs.survival.building.recipe.primitive.floor", "Primitive Floor", CCS_BuildingPieceCategory.Floor, floor, woodItem, 2, requiresFoundation: true, requiresSnap: true);
            AddRecipe(recipeList, 4, "ccs.survival.building.recipe.primitive.roof", "Primitive Roof", CCS_BuildingPieceCategory.Roof, roof, woodItem, 4, stickItem, 2, requiresWallSupport: true, requiresSnap: true);

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void AddRecipe(
            SerializedProperty recipeList,
            int index,
            string recipeId,
            string displayName,
            CCS_BuildingPieceCategory category,
            CCS_BuildingPieceDefinition definition,
            CCS_ItemDefinition primaryItem,
            int primaryQuantity,
            CCS_ItemDefinition secondaryItem = null,
            int secondaryQuantity = 0,
            bool freePlace = false,
            bool requiresFoundation = false,
            bool requiresSnap = false,
            bool requiresWallSupport = false)
        {
            recipeList.InsertArrayElementAtIndex(index);
            SerializedProperty recipe = recipeList.GetArrayElementAtIndex(index);
            recipe.FindPropertyRelative("recipeId").stringValue = recipeId;
            recipe.FindPropertyRelative("displayName").stringValue = displayName;
            recipe.FindPropertyRelative("pieceCategory").enumValueIndex = (int)category;
            recipe.FindPropertyRelative("pieceDefinitionId").stringValue = definition.PieceId;

            SerializedProperty requiredItems = recipe.FindPropertyRelative("requiredItems");
            requiredItems.ClearArray();
            AddRequiredItem(requiredItems, 0, primaryItem, primaryQuantity);
            if (secondaryItem != null && secondaryQuantity > 0)
            {
                AddRequiredItem(requiredItems, 1, secondaryItem, secondaryQuantity);
            }

            SerializedProperty rules = recipe.FindPropertyRelative("placementRules");
            rules.FindPropertyRelative("allowsFreePlacement").boolValue = freePlace;
            rules.FindPropertyRelative("requiresSnapPoint").boolValue = requiresSnap;
            rules.FindPropertyRelative("requiresFoundationNearby").boolValue = requiresFoundation;
            rules.FindPropertyRelative("requiresWallOrDoorwaySupport").boolValue = requiresWallSupport;
            rules.FindPropertyRelative("foundationSearchRadius").floatValue = 12f;
        }

        private static void AddRequiredItem(
            SerializedProperty requiredItems,
            int index,
            CCS_ItemDefinition itemDefinition,
            int quantity)
        {
            requiredItems.InsertArrayElementAtIndex(index);
            SerializedProperty entry = requiredItems.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("itemDefinitionId").stringValue = itemDefinition.ItemId;
            entry.FindPropertyRelative("itemDefinition").objectReferenceValue = itemDefinition;
            entry.FindPropertyRelative("quantity").intValue = quantity;
        }

        private static void MergeBuildingProfileStartupDefinitions(List<CCS_BuildingPieceDefinition> primitivePieces)
        {
            CCS_BuildingProfile buildingProfile = AssetDatabase.LoadAssetAtPath<CCS_BuildingProfile>(BuildingProfilePath);
            if (buildingProfile == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(buildingProfile);
            serialized.FindProperty("profileVersion").stringValue = "1.1.0";
            SerializedProperty startupList = serialized.FindProperty("startupDefinitions");
            HashSet<CCS_BuildingPieceDefinition> merged = new HashSet<CCS_BuildingPieceDefinition>();
            for (int index = 0; index < startupList.arraySize; index++)
            {
                CCS_BuildingPieceDefinition existing = startupList.GetArrayElementAtIndex(index).objectReferenceValue as CCS_BuildingPieceDefinition;
                if (existing != null)
                {
                    merged.Add(existing);
                }
            }

            for (int index = 0; index < primitivePieces.Count; index++)
            {
                merged.Add(primitivePieces[index]);
            }

            startupList.ClearArray();
            int writeIndex = 0;
            foreach (CCS_BuildingPieceDefinition definition in merged)
            {
                startupList.InsertArrayElementAtIndex(writeIndex);
                startupList.GetArrayElementAtIndex(writeIndex).objectReferenceValue = definition;
                writeIndex++;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(buildingProfile);
        }

        private static void EnsureBootstrapGameplayServiceHost(CCS_BuildingProgressionProfile progressionProfile)
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefabRoot == null)
            {
                return;
            }

            string prefabPath = AssetDatabase.GetAssetPath(prefabRoot);
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
            CCS_SurvivalGameplayServiceHost host = prefabContents.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (host == null)
            {
                PrefabUtility.UnloadPrefabContents(prefabContents);
                return;
            }

            SerializedObject serializedHost = new SerializedObject(host);
            serializedHost.FindProperty("buildingProgressionProfile").objectReferenceValue = progressionProfile;
            serializedHost.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);
        }

        private static void UpdateProjectVersion()
        {
            string projectSettingsPath = "ProjectSettings/ProjectSettings.asset";
            string text = File.ReadAllText(projectSettingsPath);
            text = System.Text.RegularExpressions.Regex.Replace(
                text,
                @"bundleVersion: [0-9]+\.[0-9]+\.[0-9]+",
                "bundleVersion: 1.1.0");
            File.WriteAllText(projectSettingsPath, text);
        }

        #endregion
    }
}
