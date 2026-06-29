using System.Text;
using CCS.Modules.CharacterController;
using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NetworkPlayerDebugLog
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Temporary diagnostics for network player ownership, visuals, and camera.
// PLACEMENT: Runtime debug utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Remove or gate behind diagnostics once multiplayer test flow is stable.
// =============================================================================

namespace CCS.Modules.CharacterController.Netcode
{
    public static class CCS_NetworkPlayerDebugLog
    {
        private const string LogPrefix = "[CCS Network Player Debug]";

        #region Public Methods

        public static void LogNetworkSpawn(
            NetworkBehaviour networkBehaviour,
            CCS_CharacterInputActionProvider inputProvider,
            CCS_CharacterMotor motor,
            CCS_CharacterCameraController playerCameraController,
            CCS_PlayerNameplateBillboard nameplateBillboard,
            Renderer bodyRenderer,
            Transform glassesTransform)
        {
            if (networkBehaviour == null)
            {
                return;
            }

            NetworkObject networkObject = networkBehaviour.NetworkObject;
            NetworkManager networkManager = networkBehaviour.NetworkManager;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("OnNetworkSpawn");
            builder.AppendLine($"  GameObject: {GetGameObjectPath(networkBehaviour.transform)}");
            builder.AppendLine($"  PrefabSource: {DescribePrefabSource(networkObject)}");
            builder.AppendLine($"  NetworkObjectId: {networkObject?.NetworkObjectId.ToString() ?? "n/a"}");
            builder.AppendLine($"  OwnerClientId: {networkObject?.OwnerClientId.ToString() ?? "n/a"}");
            builder.AppendLine($"  LocalClientId: {networkManager?.LocalClientId.ToString() ?? "n/a"}");
            builder.AppendLine($"  IsOwner: {networkBehaviour.IsOwner}");
            builder.AppendLine($"  IsLocalPlayer: {networkObject != null && networkObject.IsLocalPlayer}");
            builder.AppendLine($"  IsHost: {networkManager != null && networkManager.IsHost}");
            builder.AppendLine($"  IsClient: {networkManager != null && networkManager.IsClient}");
            builder.AppendLine($"  IsServer: {networkManager != null && networkManager.IsServer}");
            builder.AppendLine($"  Scene: {networkBehaviour.gameObject.scene.name}");
            builder.AppendLine($"  Position: {networkBehaviour.transform.position}");
            builder.AppendLine($"  InputProviderEnabled: {DescribeEnabled(inputProvider)}");
            builder.AppendLine($"  CharacterMotorEnabled: {DescribeEnabled(motor)}");
            builder.AppendLine($"  PlayerCameraControllerEnabled: {DescribeEnabled(playerCameraController)}");
            builder.AppendLine($"  NameplateVisible: {DescribeNameplateVisible(nameplateBillboard)}");
            builder.AppendLine($"  CapsuleVisualMaterial: {DescribeMaterial(bodyRenderer)}");
            builder.AppendLine($"  VisualGlassesTransform: {DescribeTransform(glassesTransform)}");
            Debug.Log(LogPrefix + " " + builder.ToString().TrimEnd(), networkBehaviour);
        }

        public static void LogCameraBind(
            NetworkObject ownerNetworkObject,
            CCS_CharacterCameraController sceneCameraRig,
            Transform followTarget,
            Transform lookAtTarget)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("CameraBind");
            builder.AppendLine($"  OwnerNetworkObjectId: {ownerNetworkObject?.NetworkObjectId.ToString() ?? "n/a"}");
            builder.AppendLine($"  OwnerIsLocalPlayer: {ownerNetworkObject != null && ownerNetworkObject.IsLocalPlayer}");
            builder.AppendLine($"  FollowTargetPath: {GetTransformPath(followTarget)}");
            builder.AppendLine($"  LookAtTargetPath: {GetTransformPath(lookAtTarget)}");
            builder.AppendLine($"  SceneCameraRig: {GetGameObjectPath(sceneCameraRig != null ? sceneCameraRig.transform : null)}");
            builder.AppendLine($"  CinemachineCamera_TP: {DescribeCinemachineCamera(sceneCameraRig)}");
            Debug.Log(LogPrefix + " " + builder.ToString().TrimEnd(), sceneCameraRig);
        }

        public static void LogInputState(
            NetworkObject ownerNetworkObject,
            bool enabled,
            CCS_CharacterInputActionProvider inputProvider)
        {
            string action = enabled ? "InputEnable" : "InputDisable";
            Debug.Log(
                LogPrefix
                + $" {action} NetworkObjectId={ownerNetworkObject?.NetworkObjectId.ToString() ?? "n/a"} "
                + $"ProviderId={inputProvider?.GetEntityId().ToString() ?? "n/a"} "
                + $"ProviderPath={GetGameObjectPath(inputProvider != null ? inputProvider.transform : null)}",
                inputProvider);
        }

        public static void LogSpawnAssignment(
            ulong clientId,
            ulong networkObjectId,
            int spawnIndex,
            string spawnTransformName,
            Vector3 spawnPosition)
        {
            Debug.Log(
                LogPrefix
                + " SpawnAssignment"
                + $" ClientId={clientId}"
                + $" NetworkObjectId={networkObjectId}"
                + $" SpawnIndex={spawnIndex}"
                + $" SpawnTransform={spawnTransformName}"
                + $" SpawnPosition={spawnPosition}");
        }

        #endregion

        #region Private Methods

        private static string DescribePrefabSource(NetworkObject networkObject)
        {
            if (networkObject == null)
            {
                return "missing NetworkObject";
            }

            if (networkObject.IsSceneObject.HasValue && networkObject.IsSceneObject.Value)
            {
                return networkObject.name + " (scene object)";
            }

            return networkObject.name + " (network prefab instance)";
        }

        private static string DescribeEnabled(Behaviour behaviour)
        {
            if (behaviour == null)
            {
                return "missing";
            }

            return behaviour.enabled ? "enabled" : "disabled";
        }

        private static string DescribeNameplateVisible(CCS_PlayerNameplateBillboard nameplateBillboard)
        {
            if (nameplateBillboard == null)
            {
                return "missing";
            }

            return nameplateBillboard.IsLocalNameplateVisible ? "visible" : "hidden";
        }

        private static string DescribeMaterial(Renderer renderer)
        {
            if (renderer == null)
            {
                return "missing renderer";
            }

            Material material = renderer.sharedMaterial;
            return material != null ? material.name : "null material";
        }

        private static string DescribeTransform(Transform transform)
        {
            if (transform == null)
            {
                return "missing";
            }

            Vector3 position = transform.localPosition;
            Vector3 euler = transform.localEulerAngles;
            Vector3 scale = transform.localScale;
            return $"pos=({Format(position.x)},{Format(position.y)},{Format(position.z)}) "
                + $"rot=({Format(euler.x)},{Format(euler.y)},{Format(euler.z)}) "
                + $"scale=({Format(scale.x)},{Format(scale.y)},{Format(scale.z)})";
        }

        private static string DescribeCinemachineCamera(CCS_CharacterCameraController sceneCameraRig)
        {
            if (sceneCameraRig == null)
            {
                return "missing scene camera rig";
            }

            Transform cinemachineTransform = sceneCameraRig.transform.Find("CinemachineCamera_TP");
            if (cinemachineTransform == null)
            {
                cinemachineTransform = sceneCameraRig.GetComponentInChildren<Transform>(true);
            }

            return cinemachineTransform != null
                ? GetGameObjectPath(cinemachineTransform)
                : "missing CinemachineCamera_TP";
        }

        private static string GetGameObjectPath(Transform transform)
        {
            if (transform == null)
            {
                return "null";
            }

            return GetTransformPath(transform);
        }

        private static string GetTransformPath(Transform transform)
        {
            if (transform == null)
            {
                return "null";
            }

            string path = transform.name;
            Transform current = transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }

        private static string Format(float value)
        {
            return value.ToString("0.###");
        }

        #endregion
    }
}
