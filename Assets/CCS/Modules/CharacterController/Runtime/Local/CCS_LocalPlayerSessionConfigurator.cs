using CCS.Modules.CharacterController;
using CCS.Modules.Interaction;
using CCS.Modules.Weapons;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_LocalPlayerSessionConfigurator
// CATEGORY: Modules / CharacterController / Tests / Runtime
// PURPOSE: Configures offline solo test players spawned from the networked prefab.
// PLACEMENT: Runtime static utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-19
// NOTES: Enables local gameplay without an active Netcode session.
// =============================================================================

namespace CCS.Modules.CharacterController.Local {
    public static class CCS_LocalPlayerSessionConfigurator
    {
        #region Variables

        private static readonly HashSet<GameObject> ConfiguredOfflinePlayers = new HashSet<GameObject>();

        #endregion

        #region Public Methods

        public static bool TryConfigureOfflinePlayer(
            GameObject playerRoot,
            CCS_PlayerDisplayProfile displayProfile,
            CCS_CharacterCameraController sceneCameraController)
        {
            if (playerRoot == null || CCS_NetworkSessionUtility.IsNetworkSessionActive())
            {
                return false;
            }

            NetworkObject networkObject = playerRoot.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsSpawned)
            {
                return false;
            }

            if (ConfiguredOfflinePlayers.Contains(playerRoot))
            {
                return false;
            }

            ApplyProfileTuning(playerRoot, displayProfile);
            ApplyOfflineNameplate(playerRoot, displayProfile);
            EnableOfflineGameplay(playerRoot);
            BindSceneCamera(playerRoot, sceneCameraController, displayProfile);

            ConfiguredOfflinePlayers.Add(playerRoot);

            CCS_PlayerSessionContext context = new CCS_PlayerSessionContext(
                ownerClientId: 0,
                playerRoot: playerRoot,
                isNetworkSession: false,
                isLocalOwner: true);
            CCS_PlayerSessionEvents.RaisePlayerSpawned(context);
            CCS_PlayerSessionEvents.RaiseLocalPlayerReady(context);
            return true;
        }

        public static void BindSceneCamera(
            GameObject playerRoot,
            CCS_CharacterCameraController sceneCameraController,
            CCS_PlayerDisplayProfile displayProfile)
        {
            if (playerRoot == null)
            {
                return;
            }

            CCS_CharacterCameraController cameraController = sceneCameraController ?? ResolveSceneCameraController();
            if (cameraController == null)
            {
                Debug.LogError("[Test Player Local Session] Scene camera rig controller was not found.");
                return;
            }

            CCS_CharacterCameraFollowAnchor followAnchor =
                playerRoot.GetComponentInChildren<CCS_CharacterCameraFollowAnchor>(true);
            if (followAnchor != null)
            {
                followAnchor.ResolveReferences();
            }

            Transform followTarget = followAnchor != null ? followAnchor.FollowTransform : null;
            Transform lookTarget = followAnchor != null && followAnchor.LookTarget != null
                ? followAnchor.LookTarget
                : null;

            if (followTarget == null || lookTarget == null)
            {
                Transform cameraPivot = playerRoot.transform.Find("CameraPivot");
                Transform cameraLookTarget = cameraPivot != null ? cameraPivot.Find("CameraLookTarget") : null;
                followTarget = cameraPivot;
                lookTarget = cameraLookTarget;
            }

            if (followTarget == null || lookTarget == null)
            {
                Debug.LogError("[Test Player Local Session] Spawned player is missing camera follow/look targets.");
                return;
            }

            CCS_CharacterCameraController playerCameraController =
                playerRoot.GetComponent<CCS_CharacterCameraController>();
            if (playerCameraController != null)
            {
                playerCameraController.enabled = false;
            }

            CCS_CharacterCameraProfile lookProfile = cameraController.CameraProfileSet != null
                ? cameraController.CameraProfileSet.DefaultProfile
                : displayProfile != null ? displayProfile.CameraProfile : null;

            if (followAnchor != null)
            {
                followAnchor.Configure(playerRoot.transform, lookTarget, lookProfile);
                followAnchor.InitializeSpawnOrientation(force: true);
            }

            cameraController.enabled = true;
            cameraController.BindFollowTargets(followTarget, lookTarget);
            EnsureMainCameraTagged(cameraController);
            ConfigureAimLocomotion(playerRoot, cameraController);
            ConfigureRevolverWeapon(playerRoot, cameraController);
        }

        private static void ConfigureAimLocomotion(
            GameObject playerRoot,
            CCS_CharacterCameraController cameraController)
        {
            if (playerRoot == null)
            {
                return;
            }

            CCS_CharacterAimLocomotionController aimLocomotion =
                playerRoot.GetComponent<CCS_CharacterAimLocomotionController>();
            if (aimLocomotion == null)
            {
                return;
            }

            aimLocomotion.ConfigureSceneCamera(cameraController);
        }

        private static void ConfigureRevolverWeapon(
            GameObject playerRoot,
            CCS_CharacterCameraController cameraController)
        {
            if (playerRoot == null)
            {
                return;
            }

            CCS_RevolverController revolverController = playerRoot.GetComponent<CCS_RevolverController>();
            if (revolverController == null)
            {
                return;
            }

            Camera outputCamera = cameraController != null
                ? cameraController.GetOutputCamera()
                : null;
            revolverController.ConfigureSceneWeaponCamera(cameraController, outputCamera);
        }

        #endregion

        #region Private Methods

        private static void EnsureMainCameraTagged(CCS_CharacterCameraController cameraController)
        {
            if (Camera.main != null)
            {
                return;
            }

            Camera sceneCamera = cameraController != null
                ? cameraController.GetComponentInChildren<Camera>(true)
                : null;
            if (sceneCamera != null)
            {
                sceneCamera.tag = "MainCamera";
            }
        }

        private static void ApplyProfileTuning(GameObject playerRoot, CCS_PlayerDisplayProfile displayProfile)
        {
            if (displayProfile == null)
            {
                return;
            }

            CCS_PlayerDisplayProfileApplicator.ApplyGameplayProfiles(playerRoot, displayProfile);
            CCS_PlayerDisplayProfileApplicator.ApplyVisualLayout(playerRoot, displayProfile);
        }

        private static void EnableOfflineGameplay(GameObject playerRoot)
        {
            CCS_CharacterInputActionProvider inputProvider =
                playerRoot.GetComponent<CCS_CharacterInputActionProvider>();
            CCS_CharacterMotor motor = playerRoot.GetComponent<CCS_CharacterMotor>();
            CCS_CharacterControllerService controllerService =
                playerRoot.GetComponent<CCS_CharacterControllerService>();
            UnityEngine.CharacterController characterController =
                playerRoot.GetComponent<UnityEngine.CharacterController>();
            NetworkTransform networkTransform = playerRoot.GetComponent<NetworkTransform>();

            if (inputProvider != null)
            {
                inputProvider.enabled = true;
                inputProvider.SetInputAccepted(true);
            }

            if (motor != null)
            {
                motor.enabled = true;
            }

            if (controllerService != null)
            {
                controllerService.enabled = true;
            }

            if (characterController != null)
            {
                characterController.enabled = true;
            }

            if (networkTransform != null)
            {
                networkTransform.enabled = false;
            }

            CCS_CharacterCameraController embeddedCamera =
                playerRoot.GetComponent<CCS_CharacterCameraController>();
            if (embeddedCamera != null)
            {
                embeddedCamera.enabled = false;
            }

            CCS_NetworkInteractionScanner interactionScanner =
                playerRoot.GetComponent<CCS_NetworkInteractionScanner>();
            if (interactionScanner != null)
            {
                interactionScanner.enabled = true;
                interactionScanner.EnsureOfflineInteractionSession();
            }
        }

        private static void ApplyOfflineNameplate(GameObject playerRoot, CCS_PlayerDisplayProfile displayProfile)
        {
            Transform nameplateRoot = playerRoot.transform.Find(CCS_PlayerPrefabConstants.NameplateRootObjectName);
            if (nameplateRoot == null)
            {
                return;
            }

            string displayName = displayProfile != null
                ? displayProfile.DefaultDisplayName
                : CCS_PlayerPrefabConstants.DefaultDisplayName;

            Transform nameplateTextTransform = nameplateRoot.Find(CCS_PlayerPrefabConstants.NameplateTextObjectName);
            if (nameplateTextTransform != null)
            {
                TMP_Text nameplateText = nameplateTextTransform.GetComponent<TMP_Text>();
                if (nameplateText != null)
                {
                    nameplateText.text = displayName;
                }
            }

            CCS_PlayerNameplateBillboard nameplateBillboard = nameplateRoot.GetComponent<CCS_PlayerNameplateBillboard>();
            if (nameplateBillboard != null)
            {
                nameplateBillboard.ApplyNameplateVisibility(isLocalOwner: true);
            }
        }

        private static CCS_CharacterCameraController ResolveSceneCameraController()
        {
            CCS_CharacterCameraController[] cameraControllers =
                Object.FindObjectsByType<CCS_CharacterCameraController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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

        #endregion
    }
}
