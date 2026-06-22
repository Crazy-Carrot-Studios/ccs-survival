using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterCameraRigInputBuilder
// CATEGORY: Modules / CharacterController / Editor
// PURPOSE: Ensures CM3 Third Person Follow + Third Person Aim camera rig wiring.
// PLACEMENT: Editor utility. Invoked from master test camera rig setup.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Shared player rig target owns yaw/pitch. Cameras only differ by profile tuning.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterCameraRigInputBuilder
    {
        #region Public Methods

        public static bool EnsureCinemachineLookInput(Transform tpCameraTransform)
        {
            if (tpCameraTransform == null)
            {
                return false;
            }

            bool changed = RemoveCustomBoundPivot(tpCameraTransform);
            changed |= RemoveLegacyOrbitalComponents(tpCameraTransform);
            changed |= EnsureThirdPersonFollow(tpCameraTransform, useAimProfile: false);
            changed |= EnsureRotateWithFollowTarget(tpCameraTransform);
            changed |= EnsureCameraTargetSettings(tpCameraTransform);
            changed |= EnsureThirdPersonCameraPriority(tpCameraTransform);
            return changed;
        }

        public static bool EnsureAimCamera(GameObject cameraRigRoot)
        {
            if (cameraRigRoot == null)
            {
                return false;
            }

            CCS_CharacterCameraAssetBuilder.EnsureCameraProfileAssets();
            CCS_CharacterCameraLayerUtility.EnsurePlayerLayerAndTag();

            bool changed = false;
            Transform aimCameraTransform = cameraRigRoot.transform.Find(
                CCS_CharacterControllerConstants.AimCinemachineCameraName);
            if (aimCameraTransform == null)
            {
                GameObject aimCameraObject = new GameObject(CCS_CharacterControllerConstants.AimCinemachineCameraName);
                aimCameraTransform = aimCameraObject.transform;
                aimCameraTransform.SetParent(cameraRigRoot.transform, false);

                Transform mainCameraTransform = cameraRigRoot.transform.Find("Main Camera");
                if (mainCameraTransform != null)
                {
                    aimCameraTransform.SetSiblingIndex(mainCameraTransform.GetSiblingIndex());
                }

                aimCameraObject.AddComponent<CinemachineCamera>();
                changed = true;
            }

            changed |= RemoveCustomBoundPivot(aimCameraTransform);
            changed |= RemoveLegacyOrbitalComponents(aimCameraTransform);
            changed |= EnsureThirdPersonFollow(aimCameraTransform, useAimProfile: true);
            changed |= EnsureRotateWithFollowTarget(aimCameraTransform);
            changed |= EnsureCameraTargetSettings(aimCameraTransform);
            changed |= EnsureThirdPersonAimExtension(aimCameraTransform);
            changed |= EnsureAimCameraPriority(aimCameraTransform);
            changed |= WireSceneCameraControllerAimReference(cameraRigRoot, aimCameraTransform);
            changed |= EnsureCinemachineBrainBlend(cameraRigRoot);

            return changed;
        }

        public static bool EnsureCinemachineBrainBlend(GameObject cameraRigRoot)
        {
            if (cameraRigRoot == null)
            {
                return false;
            }

            Transform mainCameraTransform = cameraRigRoot.transform.Find("Main Camera");
            if (mainCameraTransform == null)
            {
                return false;
            }

            CinemachineBrain brain = mainCameraTransform.GetComponent<CinemachineBrain>();
            if (brain == null)
            {
                return false;
            }

            CCS_CharacterCameraProfile aimProfile = AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfile>(
                CCS_CharacterControllerConstants.AimCameraProfilePath);
            float targetBlendSeconds = aimProfile != null ? aimProfile.AimBlendDurationSeconds : 0.5f;

            bool changed = false;
            CinemachineBlendDefinition defaultBlend = brain.DefaultBlend;
            if (!Mathf.Approximately(defaultBlend.Time, targetBlendSeconds))
            {
                defaultBlend.Time = targetBlendSeconds;
                changed = true;
            }

            if (defaultBlend.Style != CinemachineBlendDefinition.Styles.EaseInOut)
            {
                defaultBlend.Style = CinemachineBlendDefinition.Styles.EaseInOut;
                changed = true;
            }

            if (changed)
            {
                brain.DefaultBlend = defaultBlend;
                EditorUtility.SetDirty(brain);
            }

            return changed;
        }

        public static bool EnsureThirdPersonCameraPriority(Transform tpCameraTransform)
        {
            if (tpCameraTransform == null)
            {
                return false;
            }

            CinemachineCamera cinemachineCamera = tpCameraTransform.GetComponent<CinemachineCamera>();
            if (cinemachineCamera == null)
            {
                return false;
            }

            PrioritySettings prioritySettings = cinemachineCamera.Priority;
            bool changed = false;
            if (!prioritySettings.Enabled)
            {
                prioritySettings.Enabled = true;
                changed = true;
            }

            if (prioritySettings.Value != CCS_CharacterControllerConstants.ThirdPersonCameraActivePriority)
            {
                prioritySettings.Value = CCS_CharacterControllerConstants.ThirdPersonCameraActivePriority;
                changed = true;
            }

            if (changed)
            {
                cinemachineCamera.Priority = prioritySettings;
                EditorUtility.SetDirty(cinemachineCamera);
            }

            return changed;
        }

        public static bool RefreshCameraRigObstacleSettingsFromProfiles()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_CharacterControllerConstants.CameraRigPrefabPath);
            if (prefab == null)
            {
                return false;
            }

            bool changed = false;
            Transform tpCamera = prefab.transform.Find(CCS_CharacterControllerConstants.ThirdPersonCinemachineCameraName);
            Transform aimCamera = prefab.transform.Find(CCS_CharacterControllerConstants.AimCinemachineCameraName);
            if (tpCamera != null)
            {
                changed |= EnsureThirdPersonFollow(tpCamera, useAimProfile: false);
                changed |= EnsureRotateWithFollowTarget(tpCamera);
                changed |= EnsureCameraTargetSettings(tpCamera);
            }

            if (aimCamera != null)
            {
                changed |= EnsureThirdPersonFollow(aimCamera, useAimProfile: true);
                changed |= EnsureRotateWithFollowTarget(aimCamera);
                changed |= EnsureCameraTargetSettings(aimCamera);
                changed |= EnsureThirdPersonAimExtension(aimCamera);
            }

            if (changed)
            {
                EditorUtility.SetDirty(prefab);
                AssetDatabase.SaveAssets();
            }

            return changed;
        }

        public static bool ApplyValidationBaselineObstacleAvoidance(bool enabled)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_CharacterControllerConstants.CameraRigPrefabPath);
            if (prefab == null)
            {
                return false;
            }

            bool changed = ApplyObstacleAvoidanceEnabled(prefab.transform, enabled);
            if (changed)
            {
                EditorUtility.SetDirty(prefab);
                AssetDatabase.SaveAssets();
            }

            return changed;
        }

        #endregion

        #region Private Methods

        private static bool ApplyObstacleAvoidanceEnabled(Transform rigRoot, bool enabled)
        {
            bool changed = false;
            Transform tpCamera = rigRoot.Find(CCS_CharacterControllerConstants.ThirdPersonCinemachineCameraName);
            Transform aimCamera = rigRoot.Find(CCS_CharacterControllerConstants.AimCinemachineCameraName);
            changed |= SetObstacleAvoidanceEnabled(tpCamera, enabled);
            changed |= SetObstacleAvoidanceEnabled(aimCamera, enabled);
            return changed;
        }

#if CINEMACHINE_PHYSICS
        private static bool SetObstacleAvoidanceEnabled(Transform cameraTransform, bool enabled)
        {
            if (cameraTransform == null)
            {
                return false;
            }

            CinemachineThirdPersonFollow thirdPersonFollow =
                cameraTransform.GetComponent<CinemachineThirdPersonFollow>();
            if (thirdPersonFollow == null)
            {
                return false;
            }

            var obstacleSettings = thirdPersonFollow.AvoidObstacles;
            if (obstacleSettings.Enabled == enabled)
            {
                return false;
            }

            obstacleSettings.Enabled = enabled;
            thirdPersonFollow.AvoidObstacles = obstacleSettings;
            EditorUtility.SetDirty(thirdPersonFollow);
            return true;
        }
#else
        private static bool SetObstacleAvoidanceEnabled(Transform cameraTransform, bool enabled) => false;
#endif

        private static bool EnsureRotateWithFollowTarget(Transform cameraTransform)
        {
            if (cameraTransform == null)
            {
                return false;
            }

            CinemachineRotateWithFollowTarget rotateWithFollowTarget =
                cameraTransform.GetComponent<CinemachineRotateWithFollowTarget>();
            if (rotateWithFollowTarget == null)
            {
                rotateWithFollowTarget =
                    cameraTransform.gameObject.AddComponent<CinemachineRotateWithFollowTarget>();
                EditorUtility.SetDirty(rotateWithFollowTarget);
                return true;
            }

            bool changed = false;
            if (!Mathf.Approximately(rotateWithFollowTarget.Damping, 0f))
            {
                rotateWithFollowTarget.Damping = 0f;
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(rotateWithFollowTarget);
            }

            return changed;
        }

        private static bool EnsureCameraTargetSettings(Transform cameraTransform)
        {
            CinemachineCamera cinemachineCamera = cameraTransform.GetComponent<CinemachineCamera>();
            if (cinemachineCamera == null)
            {
                return false;
            }

            CameraTarget target = cinemachineCamera.Target;
            bool changed = false;
            if (target.CustomLookAtTarget)
            {
                target.CustomLookAtTarget = false;
                changed = true;
            }

            if (target.LookAtTarget != null)
            {
                target.LookAtTarget = null;
                changed = true;
            }

            if (changed)
            {
                cinemachineCamera.Target = target;
                EditorUtility.SetDirty(cinemachineCamera);
            }

            return changed;
        }

        private static bool RemoveCustomBoundPivot(Transform cameraTransform)
        {
            bool changed = false;
            Component[] components = cameraTransform.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component != null && component.GetType().Name == "CCS_CinemachineBoundPivot")
                {
                    Object.DestroyImmediate(component, true);
                    changed = true;
                }
            }

            return changed;
        }

        private static bool RemoveLegacyOrbitalComponents(Transform cameraTransform)
        {
            bool changed = false;

            CinemachineOrbitalFollow orbitalFollow = cameraTransform.GetComponent<CinemachineOrbitalFollow>();
            if (orbitalFollow != null)
            {
                Object.DestroyImmediate(orbitalFollow, true);
                changed = true;
            }

            CinemachineRotationComposer rotationComposer =
                cameraTransform.GetComponent<CinemachineRotationComposer>();
            if (rotationComposer != null)
            {
                Object.DestroyImmediate(rotationComposer, true);
                changed = true;
            }

            CinemachineInputAxisController axisController =
                cameraTransform.GetComponent<CinemachineInputAxisController>();
            if (axisController != null)
            {
                Object.DestroyImmediate(axisController, true);
                changed = true;
            }

            CinemachinePanTilt panTilt = cameraTransform.GetComponent<CinemachinePanTilt>();
            if (panTilt != null)
            {
                Object.DestroyImmediate(panTilt, true);
                changed = true;
            }

            return changed;
        }

        private static bool EnsureThirdPersonFollow(Transform cameraTransform, bool useAimProfile)
        {
            CinemachineThirdPersonFollow thirdPersonFollow =
                cameraTransform.GetComponent<CinemachineThirdPersonFollow>();
            if (thirdPersonFollow == null)
            {
                thirdPersonFollow = cameraTransform.gameObject.AddComponent<CinemachineThirdPersonFollow>();
            }

            string profilePath = useAimProfile
                ? CCS_CharacterControllerConstants.AimCameraProfilePath
                : CCS_CharacterControllerMasterTestLayoutConstants.CameraProfilePath;
            CCS_CharacterCameraProfile profile = AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfile>(profilePath);

            bool changed = WireThirdPersonFollowComponent(thirdPersonFollow, profile);
            if (changed)
            {
                EditorUtility.SetDirty(thirdPersonFollow);
            }

            return changed;
        }

        private static bool WireThirdPersonFollowComponent(
            CinemachineThirdPersonFollow thirdPersonFollow,
            CCS_CharacterCameraProfile profile)
        {
            if (thirdPersonFollow == null)
            {
                return false;
            }

            Vector3 shoulderOffset = profile != null
                ? profile.ThirdPersonShoulderOffset
                : new Vector3(0.35f, 0.25f, -0.25f);

            bool changed = false;
            Vector3 damping = profile != null
                ? new Vector3(profile.FollowDampingX, profile.FollowDampingY, profile.FollowDampingZ)
                : new Vector3(0.10f, 0.12f, 0.10f);
            if (thirdPersonFollow.Damping != damping)
            {
                thirdPersonFollow.Damping = damping;
                changed = true;
            }

            if (thirdPersonFollow.ShoulderOffset != shoulderOffset)
            {
                thirdPersonFollow.ShoulderOffset = shoulderOffset;
                changed = true;
            }

            float armLength = profile != null ? profile.ThirdPersonVerticalArmLength : 0.45f;
            if (!Mathf.Approximately(thirdPersonFollow.VerticalArmLength, armLength))
            {
                thirdPersonFollow.VerticalArmLength = armLength;
                changed = true;
            }

            float cameraSide = profile != null ? Mathf.Clamp01(Mathf.Abs(profile.ThirdPersonCameraSide)) : 0f;
            if (!Mathf.Approximately(thirdPersonFollow.CameraSide, cameraSide))
            {
                thirdPersonFollow.CameraSide = cameraSide;
                changed = true;
            }

            float cameraDistance = profile != null ? profile.ThirdPersonCameraDistance : 3.0f;
            if (!Mathf.Approximately(thirdPersonFollow.CameraDistance, cameraDistance))
            {
                thirdPersonFollow.CameraDistance = cameraDistance;
                changed = true;
            }

#if CINEMACHINE_PHYSICS
            var obstacleSettings = thirdPersonFollow.AvoidObstacles;
            bool collisionEnabled = profile == null || profile.ObstacleAvoidanceEnabled;
            LayerMask collisionFilter = profile != null
                ? profile.CollisionLayerMask
                : CCS_CharacterCameraLayerUtility.GetCameraObstructionLayerMask();
            if (CCS_CharacterCameraLayerUtility.IsEverythingLayerMask(collisionFilter))
            {
                collisionFilter = CCS_CharacterCameraLayerUtility.GetCameraObstructionLayerMask();
            }

            string ignoreTag = profile != null ? profile.CollisionIgnoreTag : CCS_CharacterControllerConstants.PlayerTag;
            float cameraRadius = profile != null ? profile.ObstacleAvoidanceRadius : 0.25f;
            float dampingInto = profile != null ? profile.CollisionDampingInto : 0.1f;
            float dampingFrom = profile != null ? profile.CollisionDampingFrom : 0.5f;

            if (obstacleSettings.Enabled != collisionEnabled
                || obstacleSettings.CollisionFilter != collisionFilter
                || obstacleSettings.IgnoreTag != ignoreTag
                || !Mathf.Approximately(obstacleSettings.CameraRadius, cameraRadius)
                || !Mathf.Approximately(obstacleSettings.DampingIntoCollision, dampingInto)
                || !Mathf.Approximately(obstacleSettings.DampingFromCollision, dampingFrom))
            {
                obstacleSettings.Enabled = collisionEnabled;
                obstacleSettings.CollisionFilter = collisionFilter;
                obstacleSettings.IgnoreTag = ignoreTag;
                obstacleSettings.CameraRadius = cameraRadius;
                obstacleSettings.DampingIntoCollision = dampingInto;
                obstacleSettings.DampingFromCollision = dampingFrom;
                thirdPersonFollow.AvoidObstacles = obstacleSettings;
                changed = true;
            }
#endif

            return changed;
        }

        private static bool EnsureThirdPersonAimExtension(Transform aimCameraTransform)
        {
#if CINEMACHINE_PHYSICS
            CinemachineThirdPersonAim thirdPersonAim =
                aimCameraTransform.GetComponent<CinemachineThirdPersonAim>();
            if (thirdPersonAim == null)
            {
                thirdPersonAim = aimCameraTransform.gameObject.AddComponent<CinemachineThirdPersonAim>();
            }

            CCS_CharacterCameraProfile aimProfile = AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfile>(
                CCS_CharacterControllerConstants.AimCameraProfilePath);

            bool changed = false;
            LayerMask collisionFilter = aimProfile != null
                ? aimProfile.CollisionLayerMask
                : CCS_CharacterCameraLayerUtility.GetCameraObstructionLayerMask();
            if (CCS_CharacterCameraLayerUtility.IsEverythingLayerMask(collisionFilter))
            {
                collisionFilter = CCS_CharacterCameraLayerUtility.GetCameraObstructionLayerMask();
            }

            string ignoreTag = aimProfile != null ? aimProfile.CollisionIgnoreTag : CCS_CharacterControllerConstants.PlayerTag;

            if (thirdPersonAim.AimCollisionFilter != collisionFilter)
            {
                thirdPersonAim.AimCollisionFilter = collisionFilter;
                changed = true;
            }

            if (thirdPersonAim.IgnoreTag != ignoreTag)
            {
                thirdPersonAim.IgnoreTag = ignoreTag;
                changed = true;
            }

            if (!Mathf.Approximately(thirdPersonAim.AimDistance, 200f))
            {
                thirdPersonAim.AimDistance = 200f;
                changed = true;
            }

            if (!thirdPersonAim.NoiseCancellation)
            {
                thirdPersonAim.NoiseCancellation = true;
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(thirdPersonAim);
            }

            return changed;
#else
            return false;
#endif
        }

        private static bool EnsureAimCameraPriority(Transform aimCameraTransform)
        {
            CinemachineCamera cinemachineCamera = aimCameraTransform.GetComponent<CinemachineCamera>();
            if (cinemachineCamera == null)
            {
                return false;
            }

            bool changed = false;
            LensSettings lens = cinemachineCamera.Lens;
            CCS_CharacterCameraProfile aimProfile = AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfile>(
                CCS_CharacterControllerConstants.AimCameraProfilePath);
            if (aimProfile != null && !Mathf.Approximately(lens.FieldOfView, aimProfile.FieldOfView))
            {
                lens.FieldOfView = aimProfile.FieldOfView;
                cinemachineCamera.Lens = lens;
                changed = true;
            }

            PrioritySettings prioritySettings = cinemachineCamera.Priority;
            if (!prioritySettings.Enabled)
            {
                prioritySettings.Enabled = true;
                changed = true;
            }

            if (prioritySettings.Value != CCS_CharacterControllerConstants.CinemachineCameraInactivePriority)
            {
                prioritySettings.Value = CCS_CharacterControllerConstants.CinemachineCameraInactivePriority;
                changed = true;
            }

            if (changed)
            {
                cinemachineCamera.Priority = prioritySettings;
                EditorUtility.SetDirty(cinemachineCamera);
            }

            return changed;
        }

        private static bool WireSceneCameraControllerAimReference(
            GameObject cameraRigRoot,
            Transform aimCameraTransform)
        {
            CCS_CharacterCameraController cameraController = cameraRigRoot.GetComponent<CCS_CharacterCameraController>();
            CinemachineCamera aimCamera = aimCameraTransform.GetComponent<CinemachineCamera>();
            if (cameraController == null || aimCamera == null)
            {
                return false;
            }

            SerializedObject serializedCamera = new SerializedObject(cameraController);
            SerializedProperty aimProperty = serializedCamera.FindProperty("aimCinemachineCamera");
            if (aimProperty == null || aimProperty.objectReferenceValue == aimCamera)
            {
                return false;
            }

            aimProperty.objectReferenceValue = aimCamera;
            serializedCamera.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(cameraController);
            return true;
        }

        #endregion
    }
}
