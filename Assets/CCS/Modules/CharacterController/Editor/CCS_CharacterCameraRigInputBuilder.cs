using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

// =============================================================================
// SCRIPT: CCS_CharacterCameraRigInputBuilder
// CATEGORY: Modules / CharacterController / Editor
// PURPOSE: Ensures CM3 Orbital Follow, Rotation Composer, and Input Axis look wiring.
// PLACEMENT: Editor utility. Invoked from master test camera rig setup.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Orbital Follow implements IInputAxisOwner (Look Orbit X/Y). Rotation Composer
//        frames LookAt only and is not input-driven (confirmed CM 3.1 API/docs).
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
            changed |= RemoveLegacyFollowComponents(tpCameraTransform);
            changed |= EnsureOrbitalFollow(tpCameraTransform, out CinemachineOrbitalFollow orbitalFollow);
            changed |= EnsureRotationComposer(tpCameraTransform);

            CinemachineInputAxisController axisController =
                tpCameraTransform.GetComponent<CinemachineInputAxisController>();
            if (axisController == null)
            {
                axisController = tpCameraTransform.gameObject.AddComponent<CinemachineInputAxisController>();
                changed = true;
            }

            changed |= WireInputAxisController(axisController, orbitalFollow);
            if (changed && axisController != null)
            {
                ForceSynchronizeAxisController(axisController);
            }

            return changed;
        }

        #endregion

        #region Private Methods

        private static bool RemoveCustomBoundPivot(Transform tpCameraTransform)
        {
            bool changed = false;
            Component[] components = tpCameraTransform.GetComponents<Component>();
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

        private static bool RemoveLegacyFollowComponents(Transform tpCameraTransform)
        {
            bool changed = false;

            CinemachineThirdPersonFollow thirdPersonFollow =
                tpCameraTransform.GetComponent<CinemachineThirdPersonFollow>();
            if (thirdPersonFollow != null)
            {
                Object.DestroyImmediate(thirdPersonFollow, true);
                changed = true;
            }

            CinemachinePanTilt panTilt = tpCameraTransform.GetComponent<CinemachinePanTilt>();
            if (panTilt != null)
            {
                Object.DestroyImmediate(panTilt, true);
                changed = true;
            }

            return changed;
        }

        private static bool EnsureOrbitalFollow(
            Transform tpCameraTransform,
            out CinemachineOrbitalFollow orbitalFollow)
        {
            orbitalFollow = tpCameraTransform.GetComponent<CinemachineOrbitalFollow>();
            if (orbitalFollow == null)
            {
                orbitalFollow = tpCameraTransform.gameObject.AddComponent<CinemachineOrbitalFollow>();
            }

            return WireOrbitalFollowComponent(orbitalFollow);
        }

        private static bool WireOrbitalFollowComponent(CinemachineOrbitalFollow orbitalFollow)
        {
            if (orbitalFollow == null)
            {
                return false;
            }

            CCS_CharacterCameraProfile profile = AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfile>(
                CCS_CharacterControllerMasterTestLayoutConstants.CameraProfilePath);
            float verticalOrbitMin = profile != null ? profile.VerticalOrbitMin : -35f;
            float verticalOrbitMax = profile != null ? profile.VerticalOrbitMax : 60f;
            float orbitalRadius = profile != null ? profile.OrbitalRadius : 4.5f;
            Vector3 shoulderOffset = profile != null
                ? profile.CameraShoulderOffset
                : new Vector3(0.45f, 0.12f, 0f);
            if (profile != null)
            {
                shoulderOffset.x *= profile.CameraSide >= 0f ? 1f : -1f;
                shoulderOffset.y = profile.CameraHeight;
            }

            bool changed = false;
            if (orbitalFollow.OrbitStyle != CinemachineOrbitalFollow.OrbitStyles.Sphere)
            {
                orbitalFollow.OrbitStyle = CinemachineOrbitalFollow.OrbitStyles.Sphere;
                changed = true;
            }

            if (!Mathf.Approximately(orbitalFollow.Radius, orbitalRadius))
            {
                orbitalFollow.Radius = orbitalRadius;
                changed = true;
            }

            if (orbitalFollow.TargetOffset != shoulderOffset)
            {
                orbitalFollow.TargetOffset = shoulderOffset;
                changed = true;
            }

            if (orbitalFollow.RecenteringTarget != CinemachineOrbitalFollow.ReferenceFrames.TrackingTarget)
            {
                orbitalFollow.RecenteringTarget = CinemachineOrbitalFollow.ReferenceFrames.TrackingTarget;
                changed = true;
            }

            TrackerSettings trackerSettings = orbitalFollow.TrackerSettings;
            if (trackerSettings.BindingMode != BindingMode.LockToTargetWithWorldUp)
            {
                trackerSettings.BindingMode = BindingMode.LockToTargetWithWorldUp;
                changed = true;
            }

            if (profile != null)
            {
                Vector3 targetDamping = new Vector3(
                    profile.FollowDampingX,
                    profile.FollowDampingY,
                    profile.FollowDampingZ);
                if (trackerSettings.PositionDamping != targetDamping)
                {
                    trackerSettings.PositionDamping = targetDamping;
                    changed = true;
                }
            }

            trackerSettings.Validate();
            orbitalFollow.TrackerSettings = trackerSettings;

            changed |= ResetAxisDefaults(
                orbitalFollow.HorizontalAxis,
                0f,
                new Vector2(-180f, 180f),
                wrap: true,
                ref orbitalFollow.HorizontalAxis);
            changed |= ResetAxisDefaults(
                orbitalFollow.VerticalAxis,
                0f,
                new Vector2(verticalOrbitMin, verticalOrbitMax),
                wrap: false,
                ref orbitalFollow.VerticalAxis);
            changed |= ResetAxisDefaults(
                orbitalFollow.RadialAxis,
                1f,
                new Vector2(1f, 1f),
                wrap: false,
                ref orbitalFollow.RadialAxis);

            if (changed)
            {
                EditorUtility.SetDirty(orbitalFollow);
            }

            return changed;
        }

        private static bool ResetAxisDefaults(
            InputAxis currentAxis,
            float targetValue,
            Vector2 targetRange,
            bool wrap,
            ref InputAxis axisField)
        {
            bool changed = false;
            InputAxis axis = currentAxis;
            if (!Mathf.Approximately(axis.Value, targetValue))
            {
                axis.Value = targetValue;
                changed = true;
            }

            if (!Mathf.Approximately(axis.Center, targetValue))
            {
                axis.Center = targetValue;
                changed = true;
            }

            if (axis.Range != targetRange)
            {
                axis.Range = targetRange;
                changed = true;
            }

            if (axis.Wrap != wrap)
            {
                axis.Wrap = wrap;
                changed = true;
            }

            axis.Recentering.Enabled = false;
            axis.Validate();
            axisField = axis;
            return changed;
        }

        private static bool EnsureRotationComposer(Transform tpCameraTransform)
        {
            CinemachineRotationComposer rotationComposer =
                tpCameraTransform.GetComponent<CinemachineRotationComposer>();
            if (rotationComposer == null)
            {
                rotationComposer = tpCameraTransform.gameObject.AddComponent<CinemachineRotationComposer>();
            }

            CCS_CharacterCameraProfile profile = AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfile>(
                CCS_CharacterControllerMasterTestLayoutConstants.CameraProfilePath);
            bool changed = false;
            Vector2 targetDamping = profile != null
                ? new Vector2(profile.FollowDampingX, profile.FollowDampingY)
                : new Vector2(0.25f, 0.3f);

            if (rotationComposer.Damping != targetDamping)
            {
                rotationComposer.Damping = targetDamping;
                changed = true;
            }

            if (!rotationComposer.CenterOnActivate)
            {
                rotationComposer.CenterOnActivate = true;
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(rotationComposer);
            }

            return changed;
        }

        private static bool WireInputAxisController(
            CinemachineInputAxisController axisController,
            CinemachineOrbitalFollow orbitalFollow)
        {
            if (axisController == null)
            {
                return false;
            }

            CCS_CharacterCameraProfile profile = AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfile>(
                CCS_CharacterControllerMasterTestLayoutConstants.CameraProfilePath);
            InputActionReference lookReference = GetOrCreateLookActionReference();
            if (lookReference == null)
            {
                Debug.LogError("[Camera Rig Input Builder] Could not create Look InputActionReference.");
                return false;
            }

            bool changed = false;
            SerializedObject serializedAxisController = new SerializedObject(axisController);
            SerializedProperty scanRecursively = serializedAxisController.FindProperty("ScanRecursively");
            SerializedProperty autoEnableInputs = serializedAxisController.FindProperty("AutoEnableInputs");
            SerializedProperty playerIndexProperty = serializedAxisController.FindProperty("PlayerIndex");

            if (scanRecursively != null && scanRecursively.boolValue)
            {
                scanRecursively.boolValue = false;
                changed = true;
            }

            if (autoEnableInputs != null && !autoEnableInputs.boolValue)
            {
                autoEnableInputs.boolValue = true;
                changed = true;
            }

            if (playerIndexProperty != null && playerIndexProperty.intValue != -1)
            {
                playerIndexProperty.intValue = -1;
                changed = true;
            }

            if (changed)
            {
                serializedAxisController.ApplyModifiedPropertiesWithoutUndo();
            }

            ForceSynchronizeAxisController(axisController);

            serializedAxisController.Update();
            SerializedProperty controllers = GetControllersProperty(serializedAxisController);
            if (controllers == null || controllers.arraySize == 0)
            {
                ForceSynchronizeAxisController(axisController);
                serializedAxisController.Update();
                controllers = GetControllersProperty(serializedAxisController);
            }

            if (controllers == null)
            {
                Debug.LogError("[Camera Rig Input Builder] CinemachineInputAxisController has no Controllers property.");
                return changed;
            }

            float mouseGainX = profile != null ? profile.MouseSensitivityX : 0.12f;
            float mouseGainY = profile != null ? -profile.MouseSensitivityY : -0.10f;
            float gamepadGainX = profile != null ? profile.GamepadSensitivityX : 90f;
            float gamepadGainY = profile != null ? -profile.GamepadSensitivityY : -70f;

            for (int i = 0; i < controllers.arraySize; i++)
            {
                SerializedProperty controller = controllers.GetArrayElementAtIndex(i);
                SerializedProperty nameProperty = controller.FindPropertyRelative("Name");
                if (nameProperty == null)
                {
                    continue;
                }

                string axisName = nameProperty.stringValue;
                bool isHorizontalOrbit = axisName == CCS_CharacterControllerConstants.LookOrbitHorizontalAxisName;
                bool isVerticalOrbit = axisName == CCS_CharacterControllerConstants.LookOrbitVerticalAxisName;
                if (!isHorizontalOrbit && !isVerticalOrbit)
                {
                    continue;
                }

                SerializedProperty ownerProperty = controller.FindPropertyRelative("Owner");
                if (ownerProperty != null && orbitalFollow != null && ownerProperty.objectReferenceValue != orbitalFollow)
                {
                    ownerProperty.objectReferenceValue = orbitalFollow;
                    changed = true;
                }

                SerializedProperty enabledProperty = controller.FindPropertyRelative("Enabled");
                if (enabledProperty != null && !enabledProperty.boolValue)
                {
                    enabledProperty.boolValue = true;
                    changed = true;
                }

                SerializedProperty inputProperty = controller.FindPropertyRelative("Input");
                if (inputProperty == null)
                {
                    continue;
                }

                SerializedProperty inputActionProperty = inputProperty.FindPropertyRelative("InputAction");
                if (inputActionProperty != null && inputActionProperty.objectReferenceValue != lookReference)
                {
                    inputActionProperty.objectReferenceValue = lookReference;
                    changed = true;
                }

                SerializedProperty gainProperty = inputProperty.FindPropertyRelative("Gain");
                if (gainProperty != null)
                {
                    float targetGain = isHorizontalOrbit ? mouseGainX : mouseGainY;
                    if (!Mathf.Approximately(gainProperty.floatValue, targetGain))
                    {
                        gainProperty.floatValue = targetGain;
                        changed = true;
                    }
                }

                SerializedProperty legacyGainProperty = inputProperty.FindPropertyRelative("LegacyGain");
                if (legacyGainProperty != null)
                {
                    float targetLegacyGain = isHorizontalOrbit ? gamepadGainX : gamepadGainY;
                    if (!Mathf.Approximately(legacyGainProperty.floatValue, targetLegacyGain))
                    {
                        legacyGainProperty.floatValue = targetLegacyGain;
                        changed = true;
                    }
                }

                SerializedProperty cancelDeltaTimeProperty = inputProperty.FindPropertyRelative("CancelDeltaTime");
                if (cancelDeltaTimeProperty != null && !cancelDeltaTimeProperty.boolValue)
                {
                    cancelDeltaTimeProperty.boolValue = true;
                    changed = true;
                }

                SerializedProperty driverProperty = controller.FindPropertyRelative("Driver");
                if (driverProperty != null)
                {
                    SerializedProperty accelTimeProperty = driverProperty.FindPropertyRelative("AccelTime");
                    SerializedProperty decelTimeProperty = driverProperty.FindPropertyRelative("DecelTime");
                    if (accelTimeProperty != null && !Mathf.Approximately(accelTimeProperty.floatValue, 0f))
                    {
                        accelTimeProperty.floatValue = 0f;
                        changed = true;
                    }

                    if (decelTimeProperty != null && !Mathf.Approximately(decelTimeProperty.floatValue, 0f))
                    {
                        decelTimeProperty.floatValue = 0f;
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                serializedAxisController.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static InputActionReference GetOrCreateLookActionReference()
        {
            InputActionReference existingReference = AssetDatabase.LoadAssetAtPath<InputActionReference>(
                CCS_CharacterControllerConstants.LookActionReferencePath);
            if (existingReference != null)
            {
                return existingReference;
            }

            InputActionAsset inputActionsAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                CCS_CharacterControllerConstants.InputActionsAssetPath);
            if (inputActionsAsset == null)
            {
                return null;
            }

            InputActionMap gameplayMap = inputActionsAsset.FindActionMap(
                CCS_CharacterControllerConstants.InputActionMapName,
                false);
            InputAction lookAction = gameplayMap != null
                ? gameplayMap.FindAction(CCS_CharacterControllerConstants.LookActionName, false)
                : null;
            if (lookAction == null)
            {
                return null;
            }

            InputActionReference lookReference = InputActionReference.Create(lookAction);
            AssetDatabase.CreateAsset(lookReference, CCS_CharacterControllerConstants.LookActionReferencePath);
            AssetDatabase.SaveAssets();
            return lookReference;
        }

        private static SerializedProperty GetControllersProperty(SerializedObject serializedAxisController)
        {
            SerializedProperty controllerManager = serializedAxisController.FindProperty("m_ControllerManager");
            return controllerManager != null
                ? controllerManager.FindPropertyRelative("Controllers")
                : serializedAxisController.FindProperty("Controllers");
        }

        private static void ForceSynchronizeAxisController(CinemachineInputAxisController axisController)
        {
            if (axisController == null)
            {
                return;
            }

            bool wasEnabled = axisController.enabled;
            axisController.enabled = false;
            axisController.enabled = true;
            if (!wasEnabled)
            {
                axisController.enabled = false;
            }
        }

        #endregion
    }
}
