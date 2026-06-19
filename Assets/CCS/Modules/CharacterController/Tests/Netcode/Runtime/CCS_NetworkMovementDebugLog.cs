using System.Text;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NetworkMovementDebugLog
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Temporary diagnostics for network movement authority and transform sync.
// PLACEMENT: Runtime debug utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Throttled movement samples. Remove or gate when test flow is stable.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode
{
    public static class CCS_NetworkMovementDebugLog
    {
        private const string LogPrefix = "[CCS Network Movement Debug]";

        #region Public Methods

        public static void LogTransformAuthority(
            NetworkObject networkObject,
            NetworkTransform networkTransform,
            bool motorEnabled,
            bool characterControllerEnabled)
        {
            if (networkObject == null)
            {
                return;
            }

            bool isServer = DescribeIsServer(networkObject);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("TransformAuthority");
            builder.AppendLine($"  NetworkObjectId: {networkObject.NetworkObjectId}");
            builder.AppendLine($"  OwnerClientId: {networkObject.OwnerClientId}");
            builder.AppendLine($"  LocalClientId: {networkObject.NetworkManager?.LocalClientId.ToString() ?? "n/a"}");
            builder.AppendLine($"  IsOwner: {networkObject.IsOwner}");
            builder.AppendLine($"  IsServer: {isServer}");
            builder.AppendLine($"  NetworkTransformType: {DescribeNetworkTransformType(networkTransform)}");
            builder.AppendLine($"  AuthorityMode: {DescribeAuthorityMode(networkTransform)}");
            builder.AppendLine($"  MotorEnabled: {motorEnabled}");
            builder.AppendLine($"  CharacterControllerEnabled: {characterControllerEnabled}");
            Debug.Log(LogPrefix + " " + builder.ToString().TrimEnd(), networkObject);
        }

        public static void LogMovementSample(
            NetworkObject networkObject,
            NetworkTransform networkTransform,
            bool motorEnabled,
            bool characterControllerEnabled,
            Vector3 positionBefore,
            Vector3 positionAfter)
        {
            if (networkObject == null)
            {
                return;
            }

            Vector3 delta = positionAfter - positionBefore;
            if (delta.sqrMagnitude < 0.000001f && !motorEnabled)
            {
                return;
            }

            bool isServer = DescribeIsServer(networkObject);
            Debug.Log(
                LogPrefix
                + " MovementSample"
                + $" NetworkObjectId={networkObject.NetworkObjectId}"
                + $" OwnerClientId={networkObject.OwnerClientId}"
                + $" LocalClientId={networkObject.NetworkManager?.LocalClientId.ToString() ?? "n/a"}"
                + $" IsOwner={networkObject.IsOwner}"
                + $" IsServer={isServer}"
                + $" MotorEnabled={motorEnabled}"
                + $" CharacterControllerEnabled={characterControllerEnabled}"
                + $" NetworkTransformType={DescribeNetworkTransformType(networkTransform)}"
                + $" AuthorityMode={DescribeAuthorityMode(networkTransform)}"
                + $" PositionBefore={Format(positionBefore)}"
                + $" PositionAfter={Format(positionAfter)}"
                + $" Delta={Format(delta)}",
                networkObject);
        }

        #endregion

        #region Private Methods

        private static string DescribeNetworkTransformType(NetworkTransform networkTransform)
        {
            if (networkTransform == null)
            {
                return "missing";
            }

            return networkTransform.GetType().Name;
        }

        private static string DescribeAuthorityMode(NetworkTransform networkTransform)
        {
            if (networkTransform == null)
            {
                return "missing";
            }

            return networkTransform.AuthorityMode.ToString();
        }

        private static bool DescribeIsServer(NetworkObject networkObject)
        {
            NetworkManager networkManager = networkObject != null ? networkObject.NetworkManager : null;
            return networkManager != null && networkManager.IsServer;
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
