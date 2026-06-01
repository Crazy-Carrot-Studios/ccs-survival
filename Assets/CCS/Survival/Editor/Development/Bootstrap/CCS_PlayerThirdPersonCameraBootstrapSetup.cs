using System.IO;
using CCS.Modules.CharacterController;
using CCS.Survival.Player;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerThirdPersonCameraBootstrapSetup
// CATEGORY: Survival / Editor / Development / Bootstrap
// PURPOSE: Rebuilds player prefab third-person Cinemachine camera hierarchy and tuning.
// PLACEMENT: Batch entry for 1.1.4 third-person camera milestone.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Keeps CharacterController locomotion. Main Camera is not parented to yaw pivot.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public static class CCS_PlayerThirdPersonCameraBootstrapSetup
    {
        private const string PlayerPrefabPath = "Assets/CCS/Survival/Prefabs/Player/PF_CCS_Player.prefab";
        private const string CharacterControllerProfilePath =
            "Assets/CCS/Survival/Profiles/CharacterController/CCS_DefaultCharacterControllerProfile.asset";
        private const string LogPrefix = "[CCS_PlayerThirdPersonCameraBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            CCS_CharacterControllerProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_CharacterControllerProfile>(CharacterControllerProfilePath);
            if (profile == null)
            {
                Debug.LogError($"{LogPrefix} Missing profile: {CharacterControllerProfilePath}");
                EditorApplication.Exit(1);
                return;
            }

            UpdateDefaultProfile(profile);

            if (!File.Exists(PlayerPrefabPath))
            {
                Debug.LogError($"{LogPrefix} Missing player prefab: {PlayerPrefabPath}");
                EditorApplication.Exit(1);
                return;
            }

            string prefabPath = PlayerPrefabPath;
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
            ConfigurePlayerCameraHierarchy(prefabContents, profile);
            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Third-person Cinemachine player camera setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void UpdateDefaultProfile(CCS_CharacterControllerProfile profile)
        {
            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileVersion").stringValue = "1.1.4";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Default third-person survival camera tuning for 1.1.4 Cinemachine integration.";
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();

            SerializedProperty cameraProperty = serializedProfile.FindProperty("camera");
            SetCameraProperty(cameraProperty, "mouseSensitivityX", 0.12f);
            SetCameraProperty(cameraProperty, "mouseSensitivityY", 0.1f);
            SetCameraProperty(cameraProperty, "gamepadSensitivityX", 90f);
            SetCameraProperty(cameraProperty, "gamepadSensitivityY", 70f);
            SetCameraProperty(cameraProperty, "minPitch", -35f);
            SetCameraProperty(cameraProperty, "maxPitch", 60f);
            SetCameraProperty(cameraProperty, "pivotHeight", 1.35f);
            SetCameraProperty(cameraProperty, "lookTargetHeight", 0.25f);
            SetCameraProperty(cameraProperty, "cameraDistance", 4.5f);
            SetCameraProperty(cameraProperty, "shoulderOffset", new Vector3(0.55f, 0.1f, 0f));
            SetCameraProperty(cameraProperty, "verticalArmLength", 0.4f);
            SetCameraProperty(cameraProperty, "cameraSide", 1f);
            SetCameraProperty(cameraProperty, "followDamping", new Vector3(0.2f, 0.25f, 0.2f));
            SetCameraProperty(cameraProperty, "pointerLookThreshold", 1f);
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void SetCameraProperty(
            SerializedProperty cameraProperty,
            string propertyName,
            float value)
        {
            SerializedProperty property = cameraProperty.FindPropertyRelative(propertyName);
            if (property != null)
            {
                property.floatValue = value;
            }
        }

        private static void SetCameraProperty(
            SerializedProperty cameraProperty,
            string propertyName,
            Vector3 value)
        {
            SerializedProperty property = cameraProperty.FindPropertyRelative(propertyName);
            if (property != null)
            {
                property.vector3Value = value;
            }
        }

        private static void ConfigurePlayerCameraHierarchy(
            GameObject playerRoot,
            CCS_CharacterControllerProfile profile)
        {
            CCS_CharacterCameraProfile cameraProfile = profile.Camera;

            RemoveLegacyCameraHierarchy(playerRoot);

            GameObject cameraPivot = new GameObject("CameraPivot");
            cameraPivot.transform.SetParent(playerRoot.transform, false);
            cameraPivot.transform.localPosition = new Vector3(0f, cameraProfile.PivotHeight, 0f);

            GameObject cameraLookTarget = new GameObject("CameraLookTarget");
            cameraLookTarget.transform.SetParent(cameraPivot.transform, false);
            cameraLookTarget.transform.localPosition = new Vector3(0f, cameraProfile.LookTargetHeight, 0f);

            GameObject mainCameraObject = new GameObject("Main Camera");
            mainCameraObject.tag = "MainCamera";
            mainCameraObject.transform.SetParent(playerRoot.transform, false);
            mainCameraObject.transform.localPosition = Vector3.zero;
            mainCameraObject.transform.localRotation = Quaternion.identity;

            Camera camera = mainCameraObject.AddComponent<Camera>();
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000f;
            camera.fieldOfView = 60f;
            mainCameraObject.AddComponent<AudioListener>();
            mainCameraObject.AddComponent<CinemachineBrain>();

            GameObject cinemachineObject = new GameObject("CM_GameplayCamera");
            cinemachineObject.transform.SetParent(playerRoot.transform, false);
            cinemachineObject.transform.localPosition = Vector3.zero;
            cinemachineObject.transform.localRotation = Quaternion.identity;

            CinemachineCamera cinemachineCamera = cinemachineObject.AddComponent<CinemachineCamera>();
            cinemachineCamera.Priority = 10;
            cinemachineCamera.Target.TrackingTarget = cameraLookTarget.transform;
            cinemachineCamera.Target.LookAtTarget = cameraLookTarget.transform;

            CinemachineThirdPersonFollow thirdPersonFollow =
                cinemachineObject.AddComponent<CinemachineThirdPersonFollow>();
            thirdPersonFollow.CameraDistance = cameraProfile.CameraDistance;
            thirdPersonFollow.ShoulderOffset = cameraProfile.ShoulderOffset;
            thirdPersonFollow.VerticalArmLength = cameraProfile.VerticalArmLength;
            thirdPersonFollow.CameraSide = cameraProfile.CameraSide;
            thirdPersonFollow.Damping = cameraProfile.FollowDamping;

            WirePlayerComponents(playerRoot, profile, cameraPivot.transform, cameraLookTarget.transform, camera, cinemachineCamera);
        }

        private static void RemoveLegacyCameraHierarchy(GameObject playerRoot)
        {
            Transform existingPivot = playerRoot.transform.Find("CameraPivot");
            if (existingPivot != null)
            {
                Object.DestroyImmediate(existingPivot.gameObject);
            }

            Transform existingCmCamera = playerRoot.transform.Find("CM_GameplayCamera");
            if (existingCmCamera != null)
            {
                Object.DestroyImmediate(existingCmCamera.gameObject);
            }

            Camera[] cameras = playerRoot.GetComponentsInChildren<Camera>(true);
            for (int index = 0; index < cameras.Length; index++)
            {
                Object.DestroyImmediate(cameras[index].gameObject);
            }
        }

        private static void WirePlayerComponents(
            GameObject playerRoot,
            CCS_CharacterControllerProfile profile,
            Transform cameraPivot,
            Transform cameraLookTarget,
            Camera playerCamera,
            CinemachineCamera cinemachineCamera)
        {
            CCS_PlayerGameplayController gameplayController =
                playerRoot.GetComponent<CCS_PlayerGameplayController>();
            if (gameplayController != null)
            {
                SerializedObject serializedGameplay = new SerializedObject(gameplayController);
                serializedGameplay.FindProperty("characterControllerProfile").objectReferenceValue = profile;
                serializedGameplay.FindProperty("cameraPivot").objectReferenceValue = cameraPivot;
                serializedGameplay.FindProperty("cameraLookTarget").objectReferenceValue = cameraLookTarget;
                serializedGameplay.FindProperty("playerCamera").objectReferenceValue = playerCamera;
                serializedGameplay.ApplyModifiedPropertiesWithoutUndo();
            }

            CCS_PlayerCinemachineCameraDriver cinemachineDriver =
                playerRoot.GetComponent<CCS_PlayerCinemachineCameraDriver>();
            if (cinemachineDriver == null)
            {
                cinemachineDriver = playerRoot.AddComponent<CCS_PlayerCinemachineCameraDriver>();
            }

            SerializedObject serializedCinemachine = new SerializedObject(cinemachineDriver);
            serializedCinemachine.FindProperty("gameplayCamera").objectReferenceValue = cinemachineCamera;
            serializedCinemachine.FindProperty("cameraYawPivot").objectReferenceValue = cameraPivot;
            serializedCinemachine.FindProperty("cameraLookTarget").objectReferenceValue = cameraLookTarget;
            serializedCinemachine.FindProperty("characterControllerProfile").objectReferenceValue = profile;
            serializedCinemachine.ApplyModifiedPropertiesWithoutUndo();

            CCS_InteractionPlayerDriver interactionDriver = playerRoot.GetComponent<CCS_InteractionPlayerDriver>();
            if (interactionDriver != null)
            {
                SerializedObject serializedInteraction = new SerializedObject(interactionDriver);
                serializedInteraction.FindProperty("interactionCamera").objectReferenceValue = playerCamera;
                serializedInteraction.ApplyModifiedPropertiesWithoutUndo();
            }

            CCS_CampfireBuildingPlayerDriver buildingDriver =
                playerRoot.GetComponent<CCS_CampfireBuildingPlayerDriver>();
            if (buildingDriver != null)
            {
                SerializedObject serializedBuilding = new SerializedObject(buildingDriver);
                serializedBuilding.FindProperty("placementCamera").objectReferenceValue = playerCamera;
                serializedBuilding.ApplyModifiedPropertiesWithoutUndo();
            }

            CCS_PlayerCombatDriver combatDriver = playerRoot.GetComponent<CCS_PlayerCombatDriver>();
            if (combatDriver != null)
            {
                SerializedObject serializedCombat = new SerializedObject(combatDriver);
                serializedCombat.FindProperty("combatCamera").objectReferenceValue = playerCamera;
                serializedCombat.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        #endregion
    }
}
