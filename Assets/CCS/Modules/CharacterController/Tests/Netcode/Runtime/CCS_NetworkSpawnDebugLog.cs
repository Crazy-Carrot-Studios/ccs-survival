using CCS.Modules.CharacterController;
using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NetworkSpawnDebugLog
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Diagnostics for network player spawn placement and owner control state.
// PLACEMENT: Runtime debug utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Temporary spawn authority diagnostics for local multiplayer test flow.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode
{
    public static class CCS_NetworkSpawnDebugLog
    {
        private const string LogPrefix = "[CCS Network Spawn Debug]";

        #region Public Methods

        public static void LogConnectionApprovalSpawn(
            ulong clientId,
            int spawnIndex,
            string spawnPointName,
            Vector3 spawnPosition,
            Quaternion spawnRotation)
        {
            Debug.Log(
                LogPrefix
                + " ConnectionApproval"
                + $" ClientId={clientId}"
                + $" SpawnPoint={spawnPointName}"
                + $" SpawnIndex={spawnIndex}"
                + $" Position={Format(spawnPosition)}"
                + $" Rotation={Format(spawnRotation.eulerAngles)}");
        }

        public static void LogSpawnPlacement(
            ulong clientId,
            ulong networkObjectId,
            string spawnPointName,
            Vector3 positionBefore,
            Vector3 positionAfter,
            bool wasNetworkObjectSpawned,
            string reason)
        {
            Debug.Log(
                LogPrefix
                + " SpawnPlacement"
                + $" Reason={reason}"
                + $" ClientId={clientId}"
                + $" NetworkObjectId={networkObjectId}"
                + $" SpawnPoint={spawnPointName}"
                + $" PositionBefore={Format(positionBefore)}"
                + $" PositionAfter={Format(positionAfter)}"
                + $" WasNetworkObjectAlreadySpawned={wasNetworkObjectSpawned}");
        }

        public static void LogOwnerControlState(
            NetworkObject networkObject,
            CCS_CharacterInputActionProvider inputProvider,
            CCS_CharacterMotor motor,
            UnityEngine.CharacterController characterController,
            string reason)
        {
            if (networkObject == null)
            {
                return;
            }

            Debug.Log(
                LogPrefix
                + " OwnerControl"
                + $" Reason={reason}"
                + $" ClientId={networkObject.OwnerClientId}"
                + $" NetworkObjectId={networkObject.NetworkObjectId}"
                + $" LocalClientId={networkObject.NetworkManager?.LocalClientId.ToString() ?? "n/a"}"
                + $" IsOwner={networkObject.IsOwner}"
                + $" IsServer={DescribeIsServer(networkObject)}"
                + $" InputProviderEnabled={DescribeInputProvider(inputProvider)}"
                + $" MotorEnabled={DescribeEnabled(motor)}"
                + $" CharacterControllerEnabled={DescribeCharacterControllerEnabled(characterController)}"
                + $" Position={Format(networkObject.transform.position)}",
                networkObject);
        }

        #endregion

        #region Private Methods

        private static bool DescribeIsServer(NetworkObject networkObject)
        {
            NetworkManager networkManager = networkObject != null ? networkObject.NetworkManager : null;
            return networkManager != null && networkManager.IsServer;
        }

        private static string DescribeInputProvider(CCS_CharacterInputActionProvider inputProvider)
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

        private static string DescribeEnabled(Behaviour behaviour)
        {
            if (behaviour == null)
            {
                return "missing";
            }

            return behaviour.enabled ? "enabled" : "disabled";
        }

        private static string DescribeCharacterControllerEnabled(UnityEngine.CharacterController characterController)
        {
            if (characterController == null)
            {
                return "missing";
            }

            return characterController.enabled ? "enabled" : "disabled";
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
