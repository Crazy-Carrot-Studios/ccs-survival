using System.Text;
using CCS.Modules.CharacterController;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

// =============================================================================
// SCRIPT: CCS_NetworkControllerAuditDiagnostics
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Structured audit logs for network controller authority conflicts.
// PLACEMENT: Runtime debug utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Toggle via CCS_NetcodeConstants.EnableControllerAuditLogs.
// =============================================================================

namespace CCS.Modules.CharacterController.Netcode
{
    public static class CCS_NetworkControllerAuditDiagnostics
    {
        private const string LogPrefix = "[CCS Network Controller Audit]";

        #region Properties

        public static bool IsEnabled => CCS_NetcodeConstants.EnableControllerAuditLogs;

        public static bool MotorMoveLogsEnabled => CCS_NetcodeConstants.EnableControllerAuditLogs
            && CCS_NetcodeConstants.EnableMotorMoveAuditLogs;

        public static bool InputLogsEnabled => CCS_NetcodeConstants.EnableControllerAuditLogs
            && CCS_NetcodeConstants.EnableInputAuditLogs;

        public static bool JumpLogsEnabled => CCS_NetcodeConstants.EnableControllerAuditLogs
            && (CCS_NetcodeConstants.EnableJumpAuditLogs
                || CCS_CharacterControllerConstants.EnableJumpDebugLogs);

        public static bool DisableNetworkTransformForAudit => CCS_NetcodeConstants.DisableNetworkTransformForAudit;

        #endregion

        #region Public Methods

        public static void LogSceneComposition()
        {
            if (!IsEnabled)
            {
                return;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("SceneComposition");
            builder.AppendLine($"  ApplicationIsFocused: {Application.isFocused}");
            builder.AppendLine($"  OfflinePlayerActive: {DescribeActiveObject(CCS_NetcodeConstants.OfflineTestPlayerSceneName)}");
            builder.AppendLine($"  SceneCameraRigCount: {CountNamedRoots(CCS_NetcodeConstants.SceneCameraRigName)}");
            builder.AppendLine($"  EnabledCameras: {CountEnabledCameras()}");
            builder.AppendLine($"  EnabledAudioListeners: {CountEnabledAudioListeners()}");
            builder.AppendLine($"  MovementCameraRegistered: {CCS_CharacterMovementCameraContext.HasActiveCamera}");
            builder.AppendLine($"  MovementCameraName: {DescribeMovementCamera()}");
            builder.AppendLine($"  DisableNetworkTransformForAudit: {DisableNetworkTransformForAudit}");
            Debug.Log(LogPrefix + " " + builder.ToString().TrimEnd());
        }

        public static void LogOwnershipRefresh(
            NetworkObject networkObject,
            string reason,
            CCS_CharacterInputActionProvider inputProvider,
            CCS_CharacterMotor motor,
            UnityEngine.CharacterController characterController,
            CCS_CharacterCameraFollowAnchor followAnchor,
            CCS_CharacterCameraController boundSceneRig)
        {
            if (!IsEnabled || networkObject == null)
            {
                return;
            }

            Debug.Log(
                LogPrefix
                + " OwnershipRefresh"
                + $" Reason={reason}"
                + $" NetworkObjectId={networkObject.NetworkObjectId}"
                + $" OwnerClientId={networkObject.OwnerClientId}"
                + $" LocalClientId={networkObject.NetworkManager?.LocalClientId.ToString() ?? "n/a"}"
                + $" IsOwner={networkObject.IsOwner}"
                + $" InputAccepted={DescribeInputAccepted(inputProvider)}"
                + $" MotorEnabled={DescribeEnabled(motor)}"
                + $" CharacterControllerEnabled={DescribeEnabled(characterController)}"
                + $" NetworkTransformEnabled={DescribeNetworkTransform(networkObject)}"
                + $" FollowAnchor={DescribeTransform(followAnchor != null ? followAnchor.FollowTransform : null)}"
                + $" BoundSceneRig={DescribeTransform(boundSceneRig != null ? boundSceneRig.transform : null)}"
                + $" MovementCamera={DescribeMovementCamera()}",
                networkObject);
        }

        public static void LogOwnedInputSample(
            NetworkObject networkObject,
            CCS_CharacterInputActionProvider inputProvider,
            Vector2 moveInput,
            Vector2 lookInput)
        {
            if (!InputLogsEnabled || networkObject == null || !networkObject.IsOwner)
            {
                return;
            }

            Debug.Log(
                LogPrefix
                + " OwnedInput"
                + $" NetworkObjectId={networkObject.NetworkObjectId}"
                + $" ApplicationIsFocused={Application.isFocused}"
                + $" InputAccepted={DescribeInputAccepted(inputProvider)}"
                + $" SharedMapEnabled={DescribeSharedMapEnabled(inputProvider)}"
                + $" MoveInput={Format(moveInput)}"
                + $" LookInput={Format(lookInput)}",
                networkObject);
        }

        public static void LogMotorMove(
            NetworkObject networkObject,
            bool isOwner,
            float deltaTime,
            Vector2 moveInput,
            Vector3 velocity,
            Vector3 positionBefore,
            Vector3 positionAfter,
            Vector3 cameraForward,
            Camera movementCamera)
        {
            if (!MotorMoveLogsEnabled)
            {
                return;
            }

            Debug.Log(
                LogPrefix
                + " MotorMove"
                + $" NetworkObjectId={networkObject?.NetworkObjectId.ToString() ?? "n/a"}"
                + $" IsOwner={isOwner}"
                + $" OwnerClientId={networkObject?.OwnerClientId.ToString() ?? "n/a"}"
                + $" LocalClientId={networkObject?.NetworkManager?.LocalClientId.ToString() ?? "n/a"}"
                + $" DeltaTime={Format(deltaTime)}"
                + $" MoveInput={Format(moveInput)}"
                + $" Velocity={Format(velocity)}"
                + $" CameraForward={Format(cameraForward)}"
                + $" MovementCamera={DescribeCamera(movementCamera)}"
                + $" PositionBefore={Format(positionBefore)}"
                + $" PositionAfter={Format(positionAfter)}",
                networkObject);
        }

        #endregion

        #region Private Methods

        private static string DescribeActiveObject(string objectName)
        {
            GameObject target = GameObject.Find(objectName);
            return target != null && target.activeInHierarchy ? "active" : "inactive-or-missing";
        }

        private static int CountNamedRoots(string objectName)
        {
            GameObject target = GameObject.Find(objectName);
            return target != null ? 1 : 0;
        }

        private static int CountEnabledCameras()
        {
            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            int count = 0;
            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i] != null && cameras[i].enabled)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountEnabledAudioListeners()
        {
            AudioListener[] listeners = Object.FindObjectsByType<AudioListener>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            int count = 0;
            for (int i = 0; i < listeners.Length; i++)
            {
                if (listeners[i] != null && listeners[i].enabled)
                {
                    count++;
                }
            }

            return count;
        }

        private static string DescribeMovementCamera()
        {
            Camera camera = CCS_CharacterMovementCameraContext.ActiveCamera;
            return DescribeCamera(camera);
        }

        private static string DescribeCamera(Camera camera)
        {
            if (camera == null)
            {
                return "missing";
            }

            return camera.name + (camera.enabled ? " (enabled)" : " (disabled)");
        }

        private static string DescribeInputAccepted(CCS_CharacterInputActionProvider inputProvider)
        {
            if (inputProvider == null)
            {
                return "missing";
            }

            if (!inputProvider.enabled)
            {
                return "component-disabled";
            }

            return inputProvider.InputAccepted ? "accepted" : "blocked";
        }

        private static string DescribeSharedMapEnabled(CCS_CharacterInputActionProvider inputProvider)
        {
            if (inputProvider == null || inputProvider.InputActionsAsset == null)
            {
                return "missing";
            }

            InputActionMap gameplayMap = inputProvider.InputActionsAsset.FindActionMap(
                CCS_CharacterControllerConstants.InputActionMapName,
                false);
            return gameplayMap != null && gameplayMap.enabled ? "enabled" : "disabled";
        }

        private static string DescribeEnabled(Behaviour behaviour)
        {
            if (behaviour == null)
            {
                return "missing";
            }

            return behaviour.enabled ? "enabled" : "disabled";
        }

        private static string DescribeEnabled(UnityEngine.CharacterController characterController)
        {
            if (characterController == null)
            {
                return "missing";
            }

            return characterController.enabled ? "enabled" : "disabled";
        }

        private static string DescribeNetworkTransform(NetworkObject networkObject)
        {
            NetworkTransform networkTransform = networkObject.GetComponent<NetworkTransform>();
            if (networkTransform == null)
            {
                return "missing";
            }

            return networkTransform.enabled ? "enabled" : "disabled";
        }

        private static string DescribeTransform(Transform transform)
        {
            return transform != null ? transform.name : "missing";
        }

        private static string Format(Vector2 value)
        {
            return $"({Format(value.x)},{Format(value.y)})";
        }

        private static string Format(Vector3 value)
        {
            return $"({Format(value.x)},{Format(value.y)},{Format(value.z)})";
        }

        private static string Format(float value)
        {
            return value.ToString("0.###");
        }

        #endregion
    }
}
