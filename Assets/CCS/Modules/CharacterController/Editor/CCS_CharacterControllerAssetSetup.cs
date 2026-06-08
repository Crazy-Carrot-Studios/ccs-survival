using System.IO;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

// =============================================================================
// SCRIPT: CCS_CharacterControllerAssetSetup
// CATEGORY: Modules / CharacterController / Editor
// PURPOSE: Creates default profiles and test prefab for v0.2.0 foundation assets.
// PLACEMENT: Editor-only setup utility. Invoked via menu or batch executeMethod.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Idempotent. Does not create a test scene.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerAssetSetup
    {
        private const string MovementFolder = "Assets/CCS/Modules/CharacterController/Profiles/Movement";
        private const string CameraFolder = "Assets/CCS/Modules/CharacterController/Profiles/Camera";
        private const string PrefabFolder = "Assets/CCS/Modules/CharacterController/Prefabs";

        #region Public Methods

        [MenuItem("CCS/Project/Setup/Create Character Controller Assets")]
        public static void CreateDefaultAssetsMenu()
        {
            CreateDefaultAssets();
        }

        public static void CreateDefaultAssets()
        {
            EnsureFolders();

            CCS_CharacterMovementProfile movementProfile = CreateOrLoadMovementProfile();
            CCS_CharacterCameraProfile cameraProfile = CreateOrLoadCameraProfile();
            CCS_CharacterCameraProfileSet cameraProfileSet = CreateOrLoadCameraProfileSet(cameraProfile);
            InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                CCS_CharacterControllerConstants.InputActionsAssetPath);

            CreateOrUpdateTestPrefab(movementProfile, cameraProfileSet, inputActions);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Character Controller Setup] Default assets and test prefab created or updated.");
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            CreateFolderIfMissing(MovementFolder);
            CreateFolderIfMissing(CameraFolder);
            CreateFolderIfMissing(PrefabFolder);
        }

        private static void CreateFolderIfMissing(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string child = Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(child))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static CCS_CharacterMovementProfile CreateOrLoadMovementProfile()
        {
            CCS_CharacterMovementProfile profile = AssetDatabase.LoadAssetAtPath<CCS_CharacterMovementProfile>(
                CCS_CharacterControllerConstants.DefaultMovementProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_CharacterMovementProfile>();
                AssetDatabase.CreateAsset(profile, CCS_CharacterControllerConstants.DefaultMovementProfilePath);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileDisplayName").stringValue = "Default Movement";
            serializedProfile.FindProperty("profileId").stringValue = CCS_CharacterControllerConstants.MovementProfileId;
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Default third-person grounded movement tuning for v0.2.0.";
            serializedProfile.FindProperty("profileVersion").stringValue = "0.2.0";
            serializedProfile.FindProperty("walkSpeed").floatValue = 4f;
            serializedProfile.FindProperty("sprintSpeed").floatValue = 6f;
            serializedProfile.FindProperty("acceleration").floatValue = 12f;
            serializedProfile.FindProperty("deceleration").floatValue = 16f;
            serializedProfile.FindProperty("gravity").floatValue = -20f;
            serializedProfile.FindProperty("jumpEnabled").boolValue = false;
            serializedProfile.FindProperty("jumpHeight").floatValue = 1.2f;
            serializedProfile.FindProperty("rotationSmoothing").floatValue = 540f;
            serializedProfile.FindProperty("airControl").floatValue = 0.25f;
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static CCS_CharacterCameraProfile CreateOrLoadCameraProfile()
        {
            CCS_CharacterCameraProfile profile = AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfile>(
                CCS_CharacterControllerConstants.DefaultCameraProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_CharacterCameraProfile>();
                AssetDatabase.CreateAsset(profile, CCS_CharacterControllerConstants.DefaultCameraProfilePath);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileDisplayName").stringValue = "Third Person Survival";
            serializedProfile.FindProperty("profileId").stringValue = CCS_CharacterControllerConstants.CameraProfileId;
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Default third-person survival camera profile for Cinemachine 3.1.";
            serializedProfile.FindProperty("profileVersion").stringValue = "0.2.0";
            serializedProfile.FindProperty("cameraMode").enumValueIndex = (int)CCS_CharacterCameraMode.ThirdPersonSurvival;
            serializedProfile.FindProperty("cameraDistance").floatValue = 4.5f;
            serializedProfile.FindProperty("cameraHeight").floatValue = 1.4f;
            serializedProfile.FindProperty("cameraShoulderOffset").vector3Value = new Vector3(0.45f, 0f, 0f);
            serializedProfile.FindProperty("verticalArmLength").floatValue = 0.35f;
            serializedProfile.FindProperty("cameraSide").floatValue = 1f;
            serializedProfile.FindProperty("minPitch").floatValue = -35f;
            serializedProfile.FindProperty("maxPitch").floatValue = 55f;
            serializedProfile.FindProperty("mouseSensitivityX").floatValue = 0.1f;
            serializedProfile.FindProperty("mouseSensitivityY").floatValue = 0.085f;
            serializedProfile.FindProperty("gamepadSensitivityX").floatValue = 85f;
            serializedProfile.FindProperty("gamepadSensitivityY").floatValue = 65f;
            serializedProfile.FindProperty("lookSmoothing").floatValue = 12f;
            serializedProfile.FindProperty("followDampingX").floatValue = 0.25f;
            serializedProfile.FindProperty("followDampingY").floatValue = 0.3f;
            serializedProfile.FindProperty("followDampingZ").floatValue = 0.25f;
            serializedProfile.FindProperty("obstacleAvoidanceEnabled").boolValue = true;
            serializedProfile.FindProperty("obstacleAvoidanceRadius").floatValue = 0.25f;
            serializedProfile.FindProperty("zoomDistanceMin").floatValue = 2.5f;
            serializedProfile.FindProperty("zoomDistanceMax").floatValue = 6f;
            serializedProfile.FindProperty("aimTransitionSpeed").floatValue = 8f;
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static CCS_CharacterCameraProfileSet CreateOrLoadCameraProfileSet(
            CCS_CharacterCameraProfile defaultProfile)
        {
            CCS_CharacterCameraProfileSet profileSet = AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfileSet>(
                CCS_CharacterControllerConstants.DefaultCameraProfileSetPath);
            if (profileSet == null)
            {
                profileSet = ScriptableObject.CreateInstance<CCS_CharacterCameraProfileSet>();
                AssetDatabase.CreateAsset(profileSet, CCS_CharacterControllerConstants.DefaultCameraProfileSetPath);
            }

            SerializedObject serializedSet = new SerializedObject(profileSet);
            serializedSet.FindProperty("defaultProfile").objectReferenceValue = defaultProfile;
            serializedSet.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profileSet);
            return profileSet;
        }

        private static void CreateOrUpdateTestPrefab(
            CCS_CharacterMovementProfile movementProfile,
            CCS_CharacterCameraProfileSet cameraProfileSet,
            InputActionAsset inputActions)
        {
            GameObject root = new GameObject("PF_CCS_CharacterController_TestPlayer");

            UnityEngine.CharacterController characterController = root.AddComponent<UnityEngine.CharacterController>();
            characterController.height = 2f;
            characterController.radius = 0.35f;
            characterController.center = new Vector3(0f, 1f, 0f);

            GameObject capsuleVisual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsuleVisual.name = "CapsuleVisual";
            Object.DestroyImmediate(capsuleVisual.GetComponent<CapsuleCollider>());
            capsuleVisual.transform.SetParent(root.transform, false);
            capsuleVisual.transform.localPosition = new Vector3(0f, 1f, 0f);
            capsuleVisual.transform.localScale = new Vector3(0.7f, 1f, 0.7f);

            Transform cameraPivot = new GameObject("CameraPivot").transform;
            cameraPivot.SetParent(root.transform, false);
            cameraPivot.localPosition = new Vector3(0f, 1.4f, 0f);

            Transform cameraLookTarget = new GameObject("CameraLookTarget").transform;
            cameraLookTarget.SetParent(cameraPivot, false);
            cameraLookTarget.localPosition = Vector3.zero;

            GameObject cinemachineObject = new GameObject("CM_ThirdPersonSurvival");
            cinemachineObject.transform.SetParent(root.transform, false);
            CinemachineCamera cinemachineCamera = cinemachineObject.AddComponent<CinemachineCamera>();
            cinemachineObject.AddComponent<CinemachineThirdPersonFollow>();
            cinemachineCamera.Target.TrackingTarget = cameraPivot;
            cinemachineCamera.Target.LookAtTarget = cameraLookTarget;

            GameObject mainCameraObject = new GameObject("Main Camera");
            mainCameraObject.tag = "MainCamera";
            mainCameraObject.transform.SetParent(root.transform, false);
            mainCameraObject.transform.localPosition = new Vector3(0f, 1.6f, -4.5f);
            Camera camera = mainCameraObject.AddComponent<Camera>();
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 500f;
            mainCameraObject.AddComponent<AudioListener>();
            mainCameraObject.AddComponent<CinemachineBrain>();

            CCS_CharacterInputActionProvider inputProvider = root.AddComponent<CCS_CharacterInputActionProvider>();
            CCS_CharacterMotor motor = root.AddComponent<CCS_CharacterMotor>();
            CCS_CharacterCameraController cameraController = root.AddComponent<CCS_CharacterCameraController>();
            CCS_CharacterControllerService service = root.AddComponent<CCS_CharacterControllerService>();
            CCS_CharacterControllerDebugHud debugHud = root.AddComponent<CCS_CharacterControllerDebugHud>();

            SerializedObject inputSerialized = new SerializedObject(inputProvider);
            inputSerialized.FindProperty("inputActionsAsset").objectReferenceValue = inputActions;
            inputSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject motorSerialized = new SerializedObject(motor);
            motorSerialized.FindProperty("movementProfile").objectReferenceValue = movementProfile;
            motorSerialized.FindProperty("inputProvider").objectReferenceValue = inputProvider;
            motorSerialized.FindProperty("cameraController").objectReferenceValue = cameraController;
            motorSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject cameraSerialized = new SerializedObject(cameraController);
            cameraSerialized.FindProperty("cameraProfileSet").objectReferenceValue = cameraProfileSet;
            cameraSerialized.FindProperty("cameraPivot").objectReferenceValue = cameraPivot;
            cameraSerialized.FindProperty("cameraLookTarget").objectReferenceValue = cameraLookTarget;
            cameraSerialized.FindProperty("cinemachineCamera").objectReferenceValue = cinemachineCamera;
            cameraSerialized.FindProperty("inputProvider").objectReferenceValue = inputProvider;
            cameraSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serviceSerialized = new SerializedObject(service);
            serviceSerialized.FindProperty("motor").objectReferenceValue = motor;
            serviceSerialized.FindProperty("inputProvider").objectReferenceValue = inputProvider;
            serviceSerialized.FindProperty("cameraController").objectReferenceValue = cameraController;
            serviceSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject hudSerialized = new SerializedObject(debugHud);
            hudSerialized.FindProperty("controllerService").objectReferenceValue = service;
            hudSerialized.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, CCS_CharacterControllerConstants.TestPrefabPath);
            Object.DestroyImmediate(root);
        }

        #endregion
    }
}
