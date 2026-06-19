using System.Collections;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Tests.Netcode;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

// =============================================================================
// SCRIPT: CCS_MasterTestSpawnController
// CATEGORY: Modules / CharacterController / Tests / Runtime
// PURPOSE: Spawns the shared network-capable test player for solo Master Test sessions.
// PLACEMENT: CCS_MasterTestSpawnController scene object in master test scene.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Network sessions rely on NetworkManager player spawning instead.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests
{
    [DefaultExecutionOrder(100)]
    public sealed class CCS_MasterTestSpawnController : MonoBehaviour
    {
        #region Serialized Fields

        [FormerlySerializedAs("soloPlayerPrefab")]
        [SerializeField] private GameObject testPlayerPrefab;

        [SerializeField] private Transform soloSpawnPoint;

        [SerializeField] private CCS_CharacterCameraController cameraController;

        [SerializeField] private Material defaultBodyMaterial;

        [SerializeField] private CCS_TestPlayerDisplayProfile displayProfile;

        #endregion

        #region Variables

        private GameObject spawnedSoloPlayer;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            if (CCS_MasterTestNetworkSessionUtility.IsNetworkSessionActive())
            {
                return;
            }

            SpawnSoloPlayer();
        }

        #endregion

        #region Private Methods

        private void SpawnSoloPlayer()
        {
            ResolveReferences();
            if (testPlayerPrefab == null || soloSpawnPoint == null)
            {
                Debug.LogError("[Master Test Spawn] Test player prefab or TP_Spawn_Host is not assigned.");
                return;
            }

            if (spawnedSoloPlayer != null)
            {
                return;
            }

            spawnedSoloPlayer = Instantiate(
                testPlayerPrefab,
                soloSpawnPoint.position,
                soloSpawnPoint.rotation);
            spawnedSoloPlayer.name = testPlayerPrefab.name;

            ApplyDefaultBodyVisual(spawnedSoloPlayer);
            CCS_TestPlayerLocalSessionConfigurator.TryConfigureOfflinePlayer(
                spawnedSoloPlayer,
                displayProfile,
                cameraController);
            StartCoroutine(RebindCameraAfterFrame(spawnedSoloPlayer));
        }

        private void ResolveReferences()
        {
            if (soloSpawnPoint == null)
            {
                Transform testPoints = GameObject.Find("TestPoints")?.transform;
                soloSpawnPoint = testPoints != null ? testPoints.Find("TP_Spawn_Host") : null;
            }

            if (cameraController == null)
            {
                cameraController = ResolveSceneCameraController();
            }
        }

        private static CCS_CharacterCameraController ResolveSceneCameraController()
        {
            CCS_CharacterCameraController[] cameraControllers =
                FindObjectsByType<CCS_CharacterCameraController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < cameraControllers.Length; i++)
            {
                CCS_CharacterCameraController candidate = cameraControllers[i];
                if (candidate == null)
                {
                    continue;
                }

                if (candidate.GetComponentInParent<CCS_CharacterMotor>() != null)
                {
                    continue;
                }

                if (candidate.GetComponentInChildren<CinemachineCamera>(true) != null)
                {
                    return candidate;
                }
            }

            for (int i = 0; i < cameraControllers.Length; i++)
            {
                CCS_CharacterCameraController candidate = cameraControllers[i];
                if (candidate != null && candidate.GetComponentInParent<CCS_CharacterMotor>() == null)
                {
                    return candidate;
                }
            }

            return null;
        }

        private void ApplyDefaultBodyVisual(GameObject player)
        {
            if (player == null)
            {
                return;
            }

            Transform bodyVisual = player.transform.Find(CCS_NetcodeTestConstants.CapsuleVisualName);
            if (bodyVisual == null)
            {
                return;
            }

            MeshRenderer bodyRenderer = bodyVisual.GetComponent<MeshRenderer>();
            if (bodyRenderer == null || defaultBodyMaterial == null)
            {
                return;
            }

            Material currentMaterial = bodyRenderer.sharedMaterial;
            if (currentMaterial == null || currentMaterial.name.Contains("Default"))
            {
                bodyRenderer.sharedMaterial = defaultBodyMaterial;
            }
        }

        private IEnumerator RebindCameraAfterFrame(GameObject player)
        {
            yield return null;

            if (player == null || !player.activeInHierarchy)
            {
                yield break;
            }

            CCS_TestPlayerLocalSessionConfigurator.BindSceneCamera(player, cameraController, displayProfile);
        }

        #endregion
    }
}
