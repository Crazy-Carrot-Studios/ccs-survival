using System.Collections.Generic;
using CCS.Modules.Attributes.Editor;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Tests;
using CCS.Modules.Interaction.Editor;
using CCS.Modules.Weapons.Editor;
using Unity.Cinemachine;
using UnityEditor;

using UnityEditor.SceneManagement;

using UnityEngine;

using UnityEngine.SceneManagement;



// =============================================================================

// SCRIPT: CCS_CharacterControllerMasterTestBuilder

// CATEGORY: Modules / CharacterController / Editor / Validation

// PURPOSE: Sets up SCN_CCS_CharacterController_MasterTest from layout constants.

// PLACEMENT: Editor builder utility. Not attached to GameObjects.

// AUTHOR: James Schilz

// CREATED: 2026-06-07

// NOTES: Invoked only from Setup menu. Cleans duplicates and restores expected hierarchy.

// =============================================================================



namespace CCS.Modules.CharacterController.Editor

{

    public static class CCS_CharacterControllerMasterTestBuilder

    {

        #region Public Methods



        public static bool SetupMasterTestScene()

        {

            Scene scene = EditorSceneManager.OpenScene(

                CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath,

                OpenSceneMode.Single);

            if (!scene.IsValid())

            {

                Debug.LogError(

                    "[Master Test Builder] Could not open "

                    + CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath);

                return false;

            }



            CCS_CharacterControllerMasterTestEnvironmentPrefabBuilder.RebuildEnvironmentPrefabs();
            CCS_CharacterControllerPlayerPrefabBuilder.EnsurePlayerPrefabs();
            CCS_CharacterControllerAnimationIsolationBuilder.EnsurePlayerAnimationIsolation();
            CCS_AttributesTestPlayerPrefabBuilder.EnsureTestPlayerAttributes();
            CCS_InteractionAssetBuilder.EnsureInteractionAssets();
            CCS_InteractionTestPlayerPrefabBuilder.EnsureTestPlayerInteractionScanner();
            CCS_InteractionPromptHudPrefabBuilder.EnsureTestPlayerInteractionPromptHud();
            CCS_WeaponsAssetBuilder.EnsureWeaponsAssets();
            CCS_WeaponsTestPlayerPrefabBuilder.EnsureTestPlayerWeaponWiring();

            EnsurePrefabAssetMaterials();



            bool changed = false;

            changed |= RemoveLegacyAndDuplicateObjects();

            changed |= RemoveDestroyedPrefabInstances();



            Transform environment = EnsureParent(CCS_CharacterControllerMasterTestLayoutConstants.EnvironmentParentName, ref changed);

            Transform testPoints = EnsureParent(CCS_CharacterControllerMasterTestLayoutConstants.TestPointsParentName, ref changed);



            changed |= RevertEnvironmentPrefabInstances(environment);

            changed |= EnsureEnvironmentPrefab(

                environment,

                CCS_CharacterControllerMasterTestLayoutConstants.GroundPrefabPath,

                CCS_CharacterControllerMasterTestLayoutConstants.GroundInstanceName,

                CCS_CharacterControllerMasterTestLayoutConstants.GroundPosition,

                Quaternion.identity);

            changed |= EnsureEnvironmentPrefab(

                environment,

                CCS_CharacterControllerMasterTestLayoutConstants.BuildingPrefabPath,

                CCS_CharacterControllerMasterTestLayoutConstants.BuildingInstanceName,

                CCS_CharacterControllerMasterTestLayoutConstants.BuildingPosition,

                Quaternion.identity);

            changed |= EnsureEnvironmentPrefab(

                environment,

                CCS_CharacterControllerMasterTestLayoutConstants.StairsPrefabPath,

                CCS_CharacterControllerMasterTestLayoutConstants.StairsInstanceName,

                CCS_CharacterControllerMasterTestLayoutConstants.StairsPosition,

                Quaternion.identity);

            changed |= EnsureEnvironmentPrefab(

                environment,

                CCS_CharacterControllerMasterTestLayoutConstants.RampPrefabPath,

                CCS_CharacterControllerMasterTestLayoutConstants.RampInstanceName,

                CCS_CharacterControllerMasterTestLayoutConstants.RampPosition,

                Quaternion.Euler(CCS_CharacterControllerMasterTestLayoutConstants.RampRotationEuler));



            changed |= RemoveLooseDoorInstances(environment);

            changed |= EnsureSpawnPoints(testPoints);

            changed |= EnsureTraversalPoints(testPoints);

            changed |= CCS_InteractionMasterTestBuilder.EnsureMasterTestPickupInteraction();

            changed |= CCS_WeaponsMasterTestBuilder.EnsureMasterTestWeaponTarget();

            changed |= EnsureBootstrapRoot();

            changed |= EnsureMasterTestSpawnController(testPoints);

            changed |= CCS_MasterTestJoinNotificationUiBuilder.EnsureJoinNotificationFeed();

            changed |= RemoveNpcFromScene();

            changed |= EnsureCameraRigPrefab();

            changed |= EnsureCameraRig();

            changed |= EnsureDirectionalLight();

            changed |= EnsureSingleAudioListener();



            if (changed)

            {

                EditorSceneManager.MarkSceneDirty(scene);

                EditorSceneManager.SaveScene(scene);

                Debug.Log("[Master Test Builder] Master test scene setup complete and saved.");

            }

            else

            {

                Debug.Log("[Master Test Builder] Master test scene already matched expected layout.");

            }



            return changed;

        }



        #endregion



        #region Private Methods



        private static void EnsurePrefabAssetMaterials()
        {
            ApplyMaterialToPrefabRenderers(
                CCS_CharacterControllerMasterTestLayoutConstants.GroundPrefabPath,
                CCS_CharacterControllerMasterTestLayoutConstants.GroundGridMaterialPath);
        }



        private static void ApplyMaterialToPrefabRenderers(

            string prefabPath,

            string materialPath,

            string rendererNameFilter = null)

        {

            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

            if (prefabRoot == null || material == null)

            {

                return;

            }



            string prefabAssetPath = prefabPath;

            GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabAssetPath);

            if (prefabContents == null)

            {

                return;

            }



            bool changed = false;

            MeshRenderer[] renderers = prefabContents.GetComponentsInChildren<MeshRenderer>(true);

            for (int i = 0; i < renderers.Length; i++)

            {

                if (renderers[i] == null)

                {

                    continue;

                }



                if (!string.IsNullOrEmpty(rendererNameFilter)

                    && renderers[i].gameObject.name != rendererNameFilter

                    && !renderers[i].gameObject.name.StartsWith(rendererNameFilter))

                {

                    continue;

                }



                if (renderers[i].sharedMaterial != material)

                {

                    renderers[i].sharedMaterial = material;

                    changed = true;

                }

            }



            if (changed)

            {

                try

                {

                    PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabAssetPath);

                }

                catch (System.SystemException exception)

                {

                    Debug.LogWarning(

                        $"[Master Test Builder] Could not save prefab '{prefabAssetPath}': {exception.Message}");

                }

            }



            PrefabUtility.UnloadPrefabContents(prefabContents);

        }



        private static bool RemoveLegacyAndDuplicateObjects()

        {

            bool changed = false;

            for (int i = 0; i < CCS_CharacterControllerMasterTestLayoutConstants.LegacyObjectNamesToRemove.Length; i++)

            {

                changed |= DestroyAllByName(CCS_CharacterControllerMasterTestLayoutConstants.LegacyObjectNamesToRemove[i]);

            }



            changed |= RemoveDuplicateEnvironmentInstances();

            return changed;

        }



        private static bool RemoveDuplicateEnvironmentInstances()

        {

            bool changed = false;

            Transform environment = GameObject.Find(CCS_CharacterControllerMasterTestLayoutConstants.EnvironmentParentName)?.transform;

            for (int i = 0; i < CCS_CharacterControllerMasterTestLayoutConstants.EnvironmentInstanceNames.Length; i++)

            {

                string instanceName = CCS_CharacterControllerMasterTestLayoutConstants.EnvironmentInstanceNames[i];

                changed |= KeepFirstDestroyRest(FindAllByName(instanceName, environment));

            }



            return changed;

        }



        private static bool RemoveDestroyedPrefabInstances()

        {

            bool changed = false;

            Transform[] allTransforms = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);

            for (int i = 0; i < allTransforms.Length; i++)

            {

                Transform current = allTransforms[i];

                if (current == null || current.gameObject == null)

                {

                    continue;

                }



                if (PrefabUtility.IsPrefabAssetMissing(current.gameObject))

                {

                    Object.DestroyImmediate(current.gameObject);

                    changed = true;

                }

            }



            return changed;

        }



        private static Transform EnsureParent(string parentName, ref bool changed)

        {

            GameObject parentObject = GameObject.Find(parentName);

            if (parentObject == null)

            {

                parentObject = new GameObject(parentName);

                changed = true;

            }



            return parentObject.transform;

        }



        private static bool RevertEnvironmentPrefabInstances(Transform environmentParent)
        {
            if (environmentParent == null)
            {
                return false;
            }

            bool changed = false;
            for (int i = 0; i < CCS_CharacterControllerMasterTestLayoutConstants.EnvironmentInstanceNames.Length; i++)
            {
                string instanceName = CCS_CharacterControllerMasterTestLayoutConstants.EnvironmentInstanceNames[i];
                Transform instance = FindDirectChild(environmentParent, instanceName);
                if (instance == null || !PrefabUtility.IsPartOfPrefabInstance(instance))
                {
                    continue;
                }

                PrefabUtility.RevertPrefabInstance(instance.gameObject, InteractionMode.AutomatedAction);
                changed = true;
            }

            return changed;
        }



        private static bool EnsureEnvironmentPrefab(

            Transform environmentParent,

            string prefabPath,

            string instanceName,

            Vector3 worldPosition,

            Quaternion worldRotation)

        {

            List<Transform> matches = FindAllByName(instanceName, environmentParent);

            Transform existing = matches.Count > 0 ? matches[0] : null;

            if (matches.Count > 1)

            {

                KeepFirstDestroyRest(matches);

            }



            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)

            {

                Debug.LogError($"[Master Test Builder] Missing prefab asset: {prefabPath}");

                return false;

            }



            bool changed = false;

            if (existing == null)

            {

                GameObject instance = PrefabUtility.InstantiatePrefab(prefab, environmentParent) as GameObject;

                if (instance == null)

                {

                    return false;

                }



                instance.name = instanceName;

                instance.transform.SetPositionAndRotation(worldPosition, worldRotation);

                return true;

            }



            if (existing.parent != environmentParent)

            {

                existing.SetParent(environmentParent, true);

                changed = true;

            }



            if (existing.name != instanceName)

            {

                existing.name = instanceName;

                changed = true;

            }



            if (!Approximately(existing.position, worldPosition))

            {

                existing.position = worldPosition;

                changed = true;

            }



            if (Quaternion.Angle(existing.rotation, worldRotation) > 1f)

            {

                existing.rotation = worldRotation;

                changed = true;

            }



            return changed;

        }



        private static bool RemoveLooseDoorInstances(Transform environmentParent)
        {
            bool changed = false;

            Transform building = FindChildByName(
                environmentParent,
                CCS_CharacterControllerMasterTestLayoutConstants.BuildingInstanceName);

            if (building != null)
            {
                List<Transform> nestedDoors = FindAllByName(
                    CCS_CharacterControllerMasterTestLayoutConstants.DoorInstanceName,
                    building);
                if (nestedDoors.Count > 1)
                {
                    changed |= KeepFirstDestroyRest(nestedDoors);
                }
            }

            List<Transform> doorMatches = FindAllByName(
                CCS_CharacterControllerMasterTestLayoutConstants.DoorInstanceName,
                null);
            for (int i = 0; i < doorMatches.Count; i++)
            {
                Transform door = doorMatches[i];
                if (door == null)
                {
                    continue;
                }

                if (building != null && door.IsChildOf(building))
                {
                    continue;
                }

                Object.DestroyImmediate(door.gameObject);
                changed = true;
            }

            return changed;
        }



        private static bool EnsureSpawnPoints(Transform testPointsParent)

        {

            bool changed = false;

            for (int i = 0; i < CCS_CharacterControllerMasterTestLayoutConstants.SpawnPointNames.Length; i++)

            {

                Vector3 position = CCS_CharacterControllerMasterTestLayoutConstants.SpawnPointPositions[i];

                Quaternion rotation = CCS_CharacterControllerMasterTestLayoutConstants.GetSpawnFacingRotation(position);

                changed |= EnsureTestPoint(

                    testPointsParent,

                    CCS_CharacterControllerMasterTestLayoutConstants.SpawnPointNames[i],

                    position,

                    rotation);

            }



            return changed;

        }



        private static bool EnsureTraversalPoints(Transform testPointsParent)

        {

            bool changed = false;

            for (int i = 0; i < CCS_CharacterControllerMasterTestLayoutConstants.TraversalPointNames.Length; i++)

            {

                changed |= EnsureTestPoint(

                    testPointsParent,

                    CCS_CharacterControllerMasterTestLayoutConstants.TraversalPointNames[i],

                    CCS_CharacterControllerMasterTestLayoutConstants.TraversalPointPositions[i],

                    Quaternion.identity);

            }



            return changed;

        }



        private static bool EnsureTestPoint(

            Transform testPointsParent,

            string pointName,

            Vector3 worldPosition,

            Quaternion worldRotation)

        {

            Transform point = FindChildByName(testPointsParent, pointName);

            bool changed = false;

            if (point == null)

            {

                GameObject pointObject = new GameObject(pointName);

                pointObject.transform.SetParent(testPointsParent, true);

                pointObject.transform.SetPositionAndRotation(worldPosition, worldRotation);

                return true;

            }



            if (point.parent != testPointsParent)

            {

                point.SetParent(testPointsParent, true);

                changed = true;

            }



            if (point.name != pointName)

            {

                point.name = pointName;

                changed = true;

            }



            if (!Approximately(point.position, worldPosition))

            {

                point.position = worldPosition;

                changed = true;

            }



            if (Quaternion.Angle(point.rotation, worldRotation) > 1f)

            {

                point.rotation = worldRotation;

                changed = true;

            }



            return changed;

        }



        private static bool EnsureBootstrapRoot()

        {

            Transform existing = FindLooseSceneObject(CCS_CharacterControllerMasterTestLayoutConstants.BootstrapInstanceName);

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(

                CCS_CharacterControllerMasterTestLayoutConstants.BootstrapPrefabPath);

            if (prefab == null)

            {

                Debug.LogError("[Master Test Builder] Missing bootstrap prefab asset.");

                return false;

            }



            if (existing == null)

            {

                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

                if (instance == null)

                {

                    return false;

                }



                instance.name = CCS_CharacterControllerMasterTestLayoutConstants.BootstrapInstanceName;

                return true;

            }



            return false;

        }



        private static bool EnsureMasterTestSpawnController(Transform testPointsParent)
        {
            bool changed = false;
            changed |= RemovePlacedPlayerFromScene();

            Transform existing = FindLooseSceneObject(
                CCS_CharacterControllerMasterTestLayoutConstants.MasterTestSpawnControllerObjectName);
            GameObject spawnObject;
            if (existing == null)
            {
                spawnObject = new GameObject(
                    CCS_CharacterControllerMasterTestLayoutConstants.MasterTestSpawnControllerObjectName);
                changed = true;
            }
            else
            {
                spawnObject = existing.gameObject;
            }

            CCS_MasterTestSpawnController spawnController = spawnObject.GetComponent<CCS_MasterTestSpawnController>();
            if (spawnController == null)
            {
                spawnController = spawnObject.AddComponent<CCS_MasterTestSpawnController>();
                changed = true;
            }

            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);
            CCS_TestPlayerDisplayProfile displayProfile = AssetDatabase.LoadAssetAtPath<CCS_TestPlayerDisplayProfile>(
                CCS_TestPlayerPrefabConstants.DefaultDisplayProfilePath);
            Transform spawnHost = testPointsParent != null
                ? testPointsParent.Find(CCS_CharacterControllerMasterTestLayoutConstants.SpawnPointNames[0])
                : null;
            Transform cameraRig = FindLooseSceneObject(
                CCS_CharacterControllerMasterTestLayoutConstants.CameraRigInstanceName);
            CCS_CharacterCameraController cameraController = cameraRig != null
                ? cameraRig.GetComponent<CCS_CharacterCameraController>()
                : null;

            SerializedObject serializedSpawn = new SerializedObject(spawnController);
            SerializedProperty prefabProperty = serializedSpawn.FindProperty("testPlayerPrefab");
            if (prefabProperty == null)
            {
                prefabProperty = serializedSpawn.FindProperty("soloPlayerPrefab");
            }

            SerializedProperty spawnProperty = serializedSpawn.FindProperty("soloSpawnPoint");
            SerializedProperty cameraProperty = serializedSpawn.FindProperty("cameraController");
            SerializedProperty bodyMaterialProperty = serializedSpawn.FindProperty("defaultBodyMaterial");
            SerializedProperty displayProfileProperty = serializedSpawn.FindProperty("displayProfile");
            Material yellowBodyMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                CCS_CharacterControllerMasterTestLayoutConstants.PlayerYellowMaterialPath);

            if (prefabProperty.objectReferenceValue != playerPrefab)
            {
                prefabProperty.objectReferenceValue = playerPrefab;
                changed = true;
            }

            if (spawnProperty.objectReferenceValue != spawnHost)
            {
                spawnProperty.objectReferenceValue = spawnHost;
                changed = true;
            }

            if (cameraProperty.objectReferenceValue != cameraController)
            {
                cameraProperty.objectReferenceValue = cameraController;
                changed = true;
            }

            if (bodyMaterialProperty != null && bodyMaterialProperty.objectReferenceValue != yellowBodyMaterial)
            {
                bodyMaterialProperty.objectReferenceValue = yellowBodyMaterial;
                changed = true;
            }

            if (displayProfileProperty != null && displayProfileProperty.objectReferenceValue != displayProfile)
            {
                displayProfileProperty.objectReferenceValue = displayProfile;
                changed = true;
            }

            if (changed)
            {
                serializedSpawn.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool RemovePlacedPlayerFromScene()
        {
            bool changed = false;
            GameObject[] sceneRoots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < sceneRoots.Length; i++)
            {
                GameObject root = sceneRoots[i];
                if (root == null)
                {
                    continue;
                }

                if (root.name == CCS_CharacterControllerMasterTestLayoutConstants.PlayerInstanceName
                    || root.name == CCS_CharacterControllerMasterTestLayoutConstants.NetworkedPlayerInstanceName)
                {
                    Object.DestroyImmediate(root);
                    changed = true;
                }
            }

            return changed;
        }

        private static bool RemoveNpcFromScene()
        {
            bool changed = false;
            GameObject[] sceneRoots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < sceneRoots.Length; i++)
            {
                if (sceneRoots[i] != null
                    && sceneRoots[i].name == CCS_CharacterControllerMasterTestLayoutConstants.NpcInstanceName)
                {
                    Object.DestroyImmediate(sceneRoots[i]);
                    changed = true;
                }
            }

            return changed;
        }



        private static bool SetRendererMaterial(Transform root, string childName, string materialPath)

        {

            Transform target = root.name == childName ? root : root.Find(childName);

            if (target == null)

            {

                return false;

            }



            MeshRenderer renderer = target.GetComponent<MeshRenderer>();

            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

            if (renderer == null || material == null || renderer.sharedMaterial == material)

            {

                return false;

            }



            renderer.sharedMaterial = material;

            return true;

        }



        private static bool EnsureCameraRigPrefab()
        {
            string prefabPath = CCS_CharacterControllerMasterTestLayoutConstants.CameraRigPrefabPath;
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError("[Master Test Builder] Missing camera rig prefab asset.");
                return false;
            }

            bool changed = ConfigureSceneCameraControllerForRuntimeBinding(prefabRoot);
            Transform tpCameraTransform = prefabRoot.transform.Find("CinemachineCamera_TP");
            changed |= CCS_CharacterCameraRigInputBuilder.EnsureCinemachineLookInput(tpCameraTransform);
            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return changed;
        }

        private static bool EnsureCameraRig()

        {

            Transform existing = FindLooseSceneObject(CCS_CharacterControllerMasterTestLayoutConstants.CameraRigInstanceName);

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(

                CCS_CharacterControllerMasterTestLayoutConstants.CameraRigPrefabPath);

            if (prefab == null)

            {

                Debug.LogError("[Master Test Builder] Missing camera rig prefab asset.");

                return false;

            }



            bool changed = false;

            if (existing == null)

            {

                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

                if (instance == null)

                {

                    return false;

                }



                instance.name = CCS_CharacterControllerMasterTestLayoutConstants.CameraRigInstanceName;

                instance.transform.position = CCS_CharacterControllerMasterTestLayoutConstants.CameraRigPosition;

                return true;

            }



            if (!Approximately(existing.position, CCS_CharacterControllerMasterTestLayoutConstants.CameraRigPosition))
            {
                existing.position = CCS_CharacterControllerMasterTestLayoutConstants.CameraRigPosition;
                changed = true;
            }

            changed |= ConfigureSceneCameraControllerForRuntimeBinding(existing.gameObject);
            Transform tpCameraTransform = existing.Find("CinemachineCamera_TP");
            changed |= CCS_CharacterCameraRigInputBuilder.EnsureCinemachineLookInput(tpCameraTransform);

            return changed;
        }

        private static bool ConfigureSceneCameraControllerForRuntimeBinding(GameObject cameraRigRoot)
        {
            if (cameraRigRoot == null)
            {
                return false;
            }

            CCS_CharacterCameraController cameraController = cameraRigRoot.GetComponent<CCS_CharacterCameraController>();
            if (cameraController == null)
            {
                return false;
            }

            Transform tpCameraTransform = cameraRigRoot.transform.Find("CinemachineCamera_TP");
            CinemachineCamera cinemachineCamera = tpCameraTransform != null
                ? tpCameraTransform.GetComponent<CinemachineCamera>()
                : null;

            bool changed = false;
            SerializedObject serializedCamera = new SerializedObject(cameraController);
            SerializedProperty cinemachineProperty = serializedCamera.FindProperty("cinemachineCamera");
            SerializedProperty pivotProperty = serializedCamera.FindProperty("cameraPivot");
            SerializedProperty lookTargetProperty = serializedCamera.FindProperty("cameraLookTarget");

            if (cinemachineProperty != null && cinemachineProperty.objectReferenceValue != cinemachineCamera)
            {
                cinemachineProperty.objectReferenceValue = cinemachineCamera;
                changed = true;
            }

            if (pivotProperty != null && pivotProperty.objectReferenceValue != null)
            {
                pivotProperty.objectReferenceValue = null;
                changed = true;
            }

            if (lookTargetProperty != null && lookTargetProperty.objectReferenceValue != null)
            {
                lookTargetProperty.objectReferenceValue = null;
                changed = true;
            }

            if (changed)
            {
                serializedCamera.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }



        private static bool EnsureDirectionalLight()

        {

            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);

            Light directionalLight = null;

            for (int i = 0; i < lights.Length; i++)

            {

                if (lights[i] != null && lights[i].type == LightType.Directional)

                {

                    directionalLight = lights[i];

                    break;

                }

            }



            bool changed = false;

            if (directionalLight == null)

            {

                GameObject lightObject = new GameObject(CCS_CharacterControllerMasterTestLayoutConstants.DirectionalLightName);

                directionalLight = lightObject.AddComponent<Light>();

                directionalLight.type = LightType.Directional;

                changed = true;

            }

            else if (directionalLight.gameObject.name != CCS_CharacterControllerMasterTestLayoutConstants.DirectionalLightName)

            {

                directionalLight.gameObject.name = CCS_CharacterControllerMasterTestLayoutConstants.DirectionalLightName;

                changed = true;

            }



            Vector3 expectedEuler = CCS_CharacterControllerMasterTestLayoutConstants.DirectionalLightEuler;

            if (Quaternion.Angle(directionalLight.transform.rotation, Quaternion.Euler(expectedEuler)) > 1f)

            {

                directionalLight.transform.rotation = Quaternion.Euler(expectedEuler);

                changed = true;

            }



            return changed;

        }



        private static bool EnsureSingleAudioListener()

        {

            AudioListener[] listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);

            AudioListener keepListener = null;

            Transform cameraRig = FindLooseSceneObject(CCS_CharacterControllerMasterTestLayoutConstants.CameraRigInstanceName);

            if (cameraRig != null)

            {

                keepListener = cameraRig.GetComponentInChildren<AudioListener>(true);

            }



            if (keepListener == null && listeners.Length > 0)

            {

                keepListener = listeners[0];

            }



            bool changed = false;

            for (int i = 0; i < listeners.Length; i++)

            {

                if (listeners[i] == null)

                {

                    continue;

                }



                bool shouldEnable = listeners[i] == keepListener;

                if (listeners[i].enabled != shouldEnable)

                {

                    listeners[i].enabled = shouldEnable;

                    changed = true;

                }

            }



            return changed;

        }



        private static bool DestroyAllByName(string objectName)

        {

            bool changed = false;

            List<Transform> matches = FindAllByName(objectName, null);

            for (int i = 0; i < matches.Count; i++)

            {

                if (matches[i] != null)

                {

                    Object.DestroyImmediate(matches[i].gameObject);

                    changed = true;

                }

            }



            return changed;

        }



        private static bool KeepFirstDestroyRest(List<Transform> matches)

        {

            if (matches == null || matches.Count <= 1)

            {

                return false;

            }



            for (int i = 1; i < matches.Count; i++)

            {

                if (matches[i] != null)

                {

                    Object.DestroyImmediate(matches[i].gameObject);

                }

            }



            return true;

        }



        private static List<Transform> FindAllByName(string objectName, Transform scope)

        {

            List<Transform> results = new List<Transform>();

            Transform[] transforms = scope != null

                ? scope.GetComponentsInChildren<Transform>(true)

                : Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);

            for (int i = 0; i < transforms.Length; i++)

            {

                if (transforms[i] != null && transforms[i].name == objectName)

                {

                    results.Add(transforms[i]);

                }

            }



            return results;

        }



        private static Transform FindDirectChild(Transform parent, string objectName)
        {
            if (parent == null)
            {
                return null;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == objectName)
                {
                    return child;
                }
            }

            return null;
        }



        private static Transform FindChildByName(Transform parent, string objectName)

        {

            if (parent == null)

            {

                return null;

            }



            Transform[] children = parent.GetComponentsInChildren<Transform>(true);

            for (int i = 0; i < children.Length; i++)

            {

                if (children[i].name == objectName)

                {

                    return children[i];

                }

            }



            return null;

        }



        private static Transform FindLooseSceneObject(string objectName)

        {

            List<Transform> matches = FindAllByName(objectName, null);

            return matches.Count > 0 ? matches[0] : null;

        }



        private static bool Approximately(Vector3 left, Vector3 right)

        {

            return Vector3.Distance(left, right) <= CCS_CharacterControllerMasterTestLayoutConstants.PositionTolerance;

        }



        #endregion

    }

}


