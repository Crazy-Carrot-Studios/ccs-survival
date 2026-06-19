using System.Collections.Generic;

using Unity.Netcode;

using Unity.Netcode.Components;

using UnityEngine;

using UnityEngine.SceneManagement;



// =============================================================================

// SCRIPT: CCS_MultiplayerTestSpawnUtility

// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime

// PURPOSE: Shared spawn placement helpers for local multiplayer test sessions.

// PLACEMENT: Runtime utility. Not attached to GameObjects.

// AUTHOR: James Schilz

// CREATED: 2026-06-07

// NOTES: Uses dedicated spawn points in SCN_CCS_CharacterController_MasterTest.

// =============================================================================



namespace CCS.Modules.CharacterController.Tests.Netcode

{

    public static class CCS_MultiplayerTestSpawnUtility

    {

        #region Public Methods



        public static bool IsMasterTestSceneActive()

        {

            Scene activeScene = SceneManager.GetActiveScene();

            return activeScene.path == CCS_NetcodeTestConstants.MasterTestScenePath

                   || activeScene.name == CCS_NetcodeTestConstants.MasterTestSceneName;

        }



        public static Vector3 GetSpawnPosition(int spawnIndex)

        {

            Transform spawnPoint = FindSpawnPoint(spawnIndex);

            return spawnPoint != null

                ? spawnPoint.position

                : CCS_NetcodeTestConstants.MasterTestFallbackSpawnPosition;

        }



        public static Quaternion GetSpawnRotation(int spawnIndex)

        {

            Transform spawnPoint = FindSpawnPoint(spawnIndex);

            if (spawnPoint != null)

            {

                return spawnPoint.rotation;

            }



            return GetFallbackSpawnRotation(CCS_NetcodeTestConstants.MasterTestFallbackSpawnPosition);

        }



        public static string GetSpawnTransformName(int spawnIndex)

        {

            string[] spawnPointNames = CCS_NetcodeTestConstants.MasterTestSpawnPointObjectNames;

            if (spawnPointNames == null || spawnPointNames.Length == 0)

            {

                return "FallbackSpawn";

            }



            int clampedIndex = Mathf.Clamp(spawnIndex, 0, spawnPointNames.Length - 1);

            return spawnPointNames[clampedIndex];

        }



        public static int GetSpawnIndexForClient(NetworkManager networkManager, ulong clientId)

        {

            if (networkManager == null)

            {

                return 0;

            }



            IReadOnlyList<ulong> connectedClientIds = networkManager.ConnectedClientsIds;

            for (int i = 0; i < connectedClientIds.Count; i++)

            {

                if (connectedClientIds[i] == clientId)

                {

                    return i;

                }

            }



            return 0;

        }



        public static bool IsNearWorldOrigin(Vector3 position)

        {

            return position.sqrMagnitude < 0.01f;

        }



        public static void ApplyApprovedSpawnPose(

            NetworkObject playerObject,

            int spawnIndex,

            ulong clientId,

            string reason)

        {

            if (playerObject == null)

            {

                return;

            }



            Vector3 spawnPosition = GetSpawnPosition(spawnIndex);

            Quaternion spawnRotation = GetSpawnRotation(spawnIndex);

            string spawnPointName = GetSpawnTransformName(spawnIndex);

            Vector3 positionBefore = playerObject.transform.position;

            bool wasSpawned = playerObject.IsSpawned;



            SetPlayerWorldPose(playerObject, spawnPosition, spawnRotation);



            CCS_NetworkSpawnDebugLog.LogSpawnPlacement(

                clientId,

                wasSpawned ? playerObject.NetworkObjectId : 0UL,

                spawnPointName,

                positionBefore,

                playerObject.transform.position,

                wasSpawned,

                reason);

        }



        public static void SetPlayerWorldPose(NetworkObject playerObject, Vector3 position, Quaternion rotation)

        {

            if (playerObject == null)

            {

                return;

            }



            UnityEngine.CharacterController characterController =

                playerObject.GetComponent<UnityEngine.CharacterController>();

            bool restoreCharacterController = ShouldSimulateCharacterController(playerObject);

            if (characterController != null)
            {
                characterController.enabled = false;
            }

            playerObject.transform.SetPositionAndRotation(position, rotation);

            NetworkTransform networkTransform = playerObject.GetComponent<NetworkTransform>();
            if (networkTransform != null && networkTransform.CanCommitToTransform)
            {
                networkTransform.Teleport(position, rotation, playerObject.transform.localScale);
            }

            if (characterController != null)
            {
                characterController.enabled = restoreCharacterController;
            }

        }



        #endregion



        #region Private Methods



        private static bool ShouldSimulateCharacterController(NetworkObject playerObject)

        {

            if (playerObject == null)

            {

                return false;

            }



            if (!playerObject.IsSpawned)

            {

                return true;

            }



            return playerObject.IsOwner;

        }



        private static Transform FindSpawnPoint(int spawnIndex)

        {

            string[] spawnPointNames = CCS_NetcodeTestConstants.MasterTestSpawnPointObjectNames;

            if (spawnPointNames == null || spawnPointNames.Length == 0)

            {

                return null;

            }



            int clampedIndex = Mathf.Clamp(spawnIndex, 0, spawnPointNames.Length - 1);

            GameObject spawnPointObject = GameObject.Find(spawnPointNames[clampedIndex]);

            return spawnPointObject != null ? spawnPointObject.transform : null;

        }



        private static Quaternion GetFallbackSpawnRotation(Vector3 spawnPosition)

        {

            Vector3 lookDirection = Vector3.zero - spawnPosition;

            lookDirection.y = 0f;

            if (lookDirection.sqrMagnitude < 0.001f)

            {

                return Quaternion.identity;

            }



            return Quaternion.LookRotation(lookDirection.normalized, Vector3.up);

        }



        #endregion

    }

}


