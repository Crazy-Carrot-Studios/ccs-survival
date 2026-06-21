using System.Collections.Generic;

using CCS.Modules.CharacterController;

using CCS.Modules.Interaction;

using Unity.Netcode;

using UnityEngine;

using UnityEngine.SceneManagement;



// =============================================================================

// SCRIPT: CCS_LocalMultiplayerTestLauncher

// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime

// PURPOSE: Connection approval and master-test scene cleanup for local netcode tests.

// PLACEMENT: PF_CCS_TestNetworkManager root.

// AUTHOR: James Schilz

// CREATED: 2026-06-07

// NOTES: Test-only launcher. Does not implement gameplay or account services.

// =============================================================================



namespace CCS.Modules.CharacterController.Tests.Netcode

{

    [DefaultExecutionOrder(-100)]

    public sealed class CCS_LocalMultiplayerTestLauncher : MonoBehaviour

    {

        #region Variables



        [Header("Master Test")]

        [SerializeField] private bool disableSceneTestActorsOnNetworkStart = true;



        private NetworkManager networkManager;



        #endregion



        #region Unity Callbacks



        private void Awake()

        {

            networkManager = GetComponent<NetworkManager>();

            if (networkManager == null)

            {

                networkManager = NetworkManager.Singleton;

            }

        }



        private void OnEnable()

        {

            RegisterNetworkCallbacks();

        }



        private void OnDisable()

        {

            UnregisterNetworkCallbacks();

        }



        #endregion



        #region Private Methods



        private void RegisterNetworkCallbacks()

        {

            if (networkManager == null)

            {

                return;

            }



            UnregisterNetworkCallbacks();

            networkManager.ConnectionApprovalCallback = HandleConnectionApproval;

            networkManager.OnServerStarted += HandleServerStarted;

            networkManager.OnClientConnectedCallback += HandleClientConnected;

            networkManager.OnClientDisconnectCallback += HandleClientDisconnected;

            SceneManager.sceneLoaded += HandleSceneLoaded;

        }



        private void UnregisterNetworkCallbacks()

        {

            if (networkManager == null)

            {

                return;

            }



            if (networkManager.ConnectionApprovalCallback == HandleConnectionApproval)

            {

                networkManager.ConnectionApprovalCallback = null;

            }



            networkManager.OnServerStarted -= HandleServerStarted;

            networkManager.OnClientConnectedCallback -= HandleClientConnected;

            networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;

            SceneManager.sceneLoaded -= HandleSceneLoaded;

        }



        private void HandleConnectionApproval(

            NetworkManager.ConnectionApprovalRequest request,

            NetworkManager.ConnectionApprovalResponse response)

        {

            bool hasCapacity = networkManager.ConnectedClientsIds.Count < CCS_NetcodeTestConstants.DefaultMaxPlayers;

            response.Approved = hasCapacity;

            response.CreatePlayerObject = true;

            response.Pending = false;

            response.Reason = hasCapacity ? string.Empty : "Session is full.";



            if (!hasCapacity)

            {

                return;

            }



            int spawnIndex = networkManager.ConnectedClientsIds.Count;
            Vector3 spawnPosition = CCS_MultiplayerTestSpawnUtility.GetSpawnPosition(spawnIndex);
            Quaternion spawnRotation = CCS_MultiplayerTestSpawnUtility.GetSpawnRotation(spawnIndex);
            response.Position = spawnPosition;
            response.Rotation = spawnRotation;

            CCS_NetworkSpawnDebugLog.LogConnectionApprovalSpawn(
                request.ClientNetworkId,
                spawnIndex,
                CCS_MultiplayerTestSpawnUtility.GetSpawnTransformName(spawnIndex),
                spawnPosition,
                spawnRotation);

        }



        private void HandleServerStarted()

        {

            if (!disableSceneTestActorsOnNetworkStart)

            {

                return;

            }



            DisableOfflineSceneTestActors();



            if (CCS_MultiplayerTestSpawnUtility.IsMasterTestSceneActive())

            {

                RepositionAllConnectedPlayers("ServerStarted");

            }



            RefreshAllNetworkPlayerConfigurations("ServerStarted");

        }



        private void HandleClientConnected(ulong clientId)

        {

            if (!disableSceneTestActorsOnNetworkStart || networkManager == null || !networkManager.IsServer)

            {

                return;

            }



            DisableOfflineSceneTestActors();



            if (CCS_MultiplayerTestSpawnUtility.IsMasterTestSceneActive())

            {

                RepositionConnectedPlayer(clientId, "ClientConnected");

            }



            RefreshAllNetworkPlayerConfigurations("ClientConnected");

        }



        private void HandleClientDisconnected(ulong clientId)

        {

            if (!disableSceneTestActorsOnNetworkStart || networkManager == null || !networkManager.IsListening)

            {

                return;

            }



            RefreshAllNetworkPlayerConfigurations("ClientDisconnected");

        }



        private void HandleSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)

        {

            if (!disableSceneTestActorsOnNetworkStart || networkManager == null || !networkManager.IsListening)

            {

                return;

            }



            if (scene.path != CCS_NetcodeTestConstants.MasterTestScenePath

                && scene.name != CCS_NetcodeTestConstants.MasterTestSceneName)

            {

                return;

            }



            DisableOfflineSceneTestActors();



            if (networkManager.IsServer)
            {
                RepositionAllConnectedPlayers("MasterTestSceneLoaded");
                CCS_TestPickupItemSpawner.EnsureNetworkInstanceIfServer();
            }

            RefreshAllNetworkPlayerConfigurations("MasterTestSceneLoaded");

            CCS_SingleAudioListenerUtility.EnsureSingleActiveListener();

        }



        private void RepositionAllConnectedPlayers(string reason)

        {

            if (networkManager == null || !networkManager.IsServer)

            {

                return;

            }



            IReadOnlyList<NetworkClient> connectedClients = networkManager.ConnectedClientsList;

            for (int i = 0; i < connectedClients.Count; i++)

            {

                RepositionConnectedPlayer(connectedClients[i].ClientId, reason);

            }

        }



        private void RepositionConnectedPlayer(ulong clientId, string reason)

        {

            if (networkManager == null || !networkManager.IsServer)

            {

                return;

            }



            if (!networkManager.ConnectedClients.TryGetValue(clientId, out NetworkClient client))

            {

                return;

            }



            NetworkObject playerObject = client.PlayerObject;

            if (playerObject == null)

            {

                return;

            }



            int spawnIndex = CCS_MultiplayerTestSpawnUtility.GetSpawnIndexForClient(networkManager, clientId);

            CCS_MultiplayerTestSpawnUtility.ApplyApprovedSpawnPose(

                playerObject,

                spawnIndex,

                clientId,

                reason);

        }



        private static void RefreshAllNetworkPlayerConfigurations(string reason)

        {

            CCS_ControllerTestNetworkPlayerBehaviour.RefreshAllLocalConfigurations(reason);

        }



        private static void DisableOfflineSceneTestActors()

        {

            GameObject offlinePlayer = GameObject.Find(CCS_NetcodeTestConstants.OfflineTestPlayerSceneName);

            if (offlinePlayer != null)

            {

                DisableLocalControlComponents(offlinePlayer);

                offlinePlayer.SetActive(false);

            }



            CCS_SingleAudioListenerUtility.EnsureSingleActiveListener();

        }



        private static void DisableLocalControlComponents(GameObject playerObject)

        {

            CCS_CharacterInputActionProvider inputProvider =

                playerObject.GetComponent<CCS_CharacterInputActionProvider>();

            if (inputProvider != null)

            {

                inputProvider.enabled = false;

            }



            CCS_CharacterMotor motor = playerObject.GetComponent<CCS_CharacterMotor>();

            if (motor != null)

            {

                motor.enabled = false;

            }



            CCS_CharacterControllerService controllerService =

                playerObject.GetComponent<CCS_CharacterControllerService>();

            if (controllerService != null)

            {

                controllerService.enabled = false;

            }



            CCS_CharacterCameraController cameraController =

                playerObject.GetComponent<CCS_CharacterCameraController>();

            if (cameraController != null)

            {

                cameraController.enabled = false;

            }

        }



        #endregion

    }

}


