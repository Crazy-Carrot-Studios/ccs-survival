using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Tests;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_ControllerTestNetworkPlayerBehaviour
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Network test player ownership, movement authority, and local camera/input.
// PLACEMENT: PF_CCS_CharacterController_TestPlayer_Networked root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Owner runs motor. Scene camera binds to world-stable follow anchor, not body-yaw pivot.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode
{
    [DefaultExecutionOrder(-200)]
    public sealed class CCS_ControllerTestNetworkPlayerBehaviour : NetworkBehaviour
    {
        #region Variables

        [Header("Visuals")]
        [SerializeField] private Material yellowBodyMaterial;
        [SerializeField] private Material blackGlassesMaterial;
        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private Renderer glassesRenderer;

        [Header("References")]
        [SerializeField] private CCS_CharacterInputActionProvider inputProvider;
        [SerializeField] private CCS_CharacterMotor motor;
        [SerializeField] private CCS_CharacterControllerService controllerService;
        [SerializeField] private CCS_CharacterControllerDebugHud debugHud;
        [SerializeField] private CCS_CharacterCameraController playerCameraController;
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Transform cameraLookTarget;

        private CCS_PlayerNameplateBillboard nameplateBillboard;
        private CCS_CharacterCameraFollowAnchor cameraFollowAnchor;
        private CCS_CharacterCameraController boundSceneCameraRig;
        private UnityEngine.CharacterController characterController;
        private NetworkTransform networkTransform;
        private Transform glassesTransform;
        private Vector3 lastLoggedPosition;
        private float nextMovementLogTime;
        private float nextInputAuditLogTime;
        private bool subscribedToSceneLoaded;

        private readonly NetworkVariable<float> replicatedBodyYaw = new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        private const float MovementLogIntervalSeconds = 0.25f;
        private const float InputAuditLogIntervalSeconds = 0.5f;

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            if (IsSpawned)
            {
                RefreshLocalConfiguration("OnEnable");
            }

            if (subscribedToSceneLoaded)
            {
                return;
            }

            SceneManager.sceneLoaded += HandleSceneLoaded;
            subscribedToSceneLoaded = true;
        }

        private void OnDisable()
        {
            if (!subscribedToSceneLoaded)
            {
                return;
            }

            SceneManager.sceneLoaded -= HandleSceneLoaded;
            subscribedToSceneLoaded = false;
        }

        private void LateUpdate()
        {
            if (!IsSpawned)
            {
                return;
            }

            SynchronizeBodyYawReplication();

            if (IsOwner && inputProvider != null && NetworkObject != null
                && Time.unscaledTime >= nextInputAuditLogTime)
            {
                nextInputAuditLogTime = Time.unscaledTime + InputAuditLogIntervalSeconds;
                CCS_NetworkControllerAuditDiagnostics.LogOwnedInputSample(
                    NetworkObject,
                    inputProvider,
                    inputProvider.MoveInput,
                    inputProvider.LookInput);
            }

            Vector3 currentPosition = transform.position;
            if (Time.unscaledTime < nextMovementLogTime)
            {
                return;
            }

            nextMovementLogTime = Time.unscaledTime + MovementLogIntervalSeconds;
            bool motorEnabled = motor != null && motor.enabled;
            bool characterControllerEnabled = characterController != null && characterController.enabled;
            CCS_NetworkMovementDebugLog.LogMovementSample(
                NetworkObject,
                networkTransform,
                motorEnabled,
                characterControllerEnabled,
                lastLoggedPosition,
                currentPosition);
            lastLoggedPosition = currentPosition;
        }

        #endregion

        #region Public Methods

        public static void RefreshAllLocalConfigurations(string reason)
        {
            CCS_ControllerTestNetworkPlayerBehaviour[] behaviours =
                Object.FindObjectsByType<CCS_ControllerTestNetworkPlayerBehaviour>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
            for (int i = 0; i < behaviours.Length; i++)
            {
                CCS_ControllerTestNetworkPlayerBehaviour behaviour = behaviours[i];
                if (behaviour != null)
                {
                    behaviour.RefreshLocalConfiguration(reason);
                }
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            ResolveReferences();
            lastLoggedPosition = transform.position;
            nextMovementLogTime = Time.unscaledTime;
            RefreshLocalConfiguration("OnNetworkSpawn");

            if (IsOwner)
            {
                replicatedBodyYaw.Value = transform.eulerAngles.y;
            }

            ApplyPlayerVisuals();
            ApplyDisplayProfileLayout();
            ApplyNameplateVisibility();
            LogNetworkSpawnState();
            LogTransformAuthorityState();
            CCS_NetworkControllerAuditDiagnostics.LogSceneComposition();
            CCS_SingleAudioListenerUtility.EnsureSingleActiveListener();
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner)
            {
                boundSceneCameraRig?.UnregisterMovementCamera();
                RestoreSceneCameraRig();
            }

            base.OnNetworkDespawn();
        }

        #endregion

        #region Private Methods

        private void ResolveReferences()
        {
            if (inputProvider == null)
            {
                inputProvider = GetComponent<CCS_CharacterInputActionProvider>();
            }

            if (motor == null)
            {
                motor = GetComponent<CCS_CharacterMotor>();
            }

            if (controllerService == null)
            {
                controllerService = GetComponent<CCS_CharacterControllerService>();
            }

            if (debugHud == null)
            {
                debugHud = GetComponent<CCS_CharacterControllerDebugHud>();
            }

            if (playerCameraController == null)
            {
                playerCameraController = GetComponent<CCS_CharacterCameraController>();
            }

            if (characterController == null)
            {
                characterController = GetComponent<UnityEngine.CharacterController>();
            }

            if (networkTransform == null)
            {
                networkTransform = GetComponent<NetworkTransform>();
            }

            if (cameraPivot == null)
            {
                Transform pivot = transform.Find("CameraPivot");
                cameraPivot = pivot != null ? pivot : cameraPivot;
            }

            if (cameraLookTarget == null)
            {
                Transform lookTarget = transform.Find("CameraPivot/CameraLookTarget");
                if (lookTarget == null)
                {
                    lookTarget = transform.Find("CameraLookTarget");
                }

                cameraLookTarget = lookTarget != null ? lookTarget : cameraLookTarget;
            }

            if (bodyRenderer == null)
            {
                Transform visual = transform.Find(CCS_NetcodeTestConstants.CapsuleVisualName);
                bodyRenderer = visual != null ? visual.GetComponent<Renderer>() : GetComponentInChildren<Renderer>();
            }

            glassesTransform = transform.Find(CCS_NetcodeTestConstants.GlassesVisualName);
            if (glassesTransform == null)
            {
                glassesTransform = transform.Find("GlassesVisual");
            }

            if (glassesRenderer == null && glassesTransform != null)
            {
                glassesRenderer = glassesTransform.GetComponent<Renderer>();
            }

            if (nameplateBillboard == null)
            {
                Transform nameplateRoot = transform.Find(CCS_NetcodeTestConstants.NameplateRootObjectName);
                nameplateBillboard = nameplateRoot != null
                    ? nameplateRoot.GetComponent<CCS_PlayerNameplateBillboard>()
                    : null;
            }

            if (cameraFollowAnchor == null)
            {
                cameraFollowAnchor = GetComponentInChildren<CCS_CharacterCameraFollowAnchor>(true);
            }

            if (cameraFollowAnchor != null)
            {
                cameraFollowAnchor.ResolveReferences();
            }
        }

        private void ConfigureOwnerPlayer()
        {
            bool gameplaySceneActive = CCS_MultiplayerTestSpawnUtility.IsMasterTestSceneActive();
            bool allowLocalSimulation = IsOwner && gameplaySceneActive;

            SetInputEnabled(allowLocalSimulation);
            SetComponentEnabled(motor, allowLocalSimulation);
            SetComponentEnabled(controllerService, allowLocalSimulation);
            SetComponentEnabled(debugHud, allowLocalSimulation);
            SetCharacterControllerEnabled(allowLocalSimulation);
            ApplyNetworkTransformAuditSettings();

            if (!gameplaySceneActive)
            {
                SetComponentEnabled(playerCameraController, false);
                DisableEmbeddedPlayerCamera();
                return;
            }

            EnsureCameraFollowAnchor();

            if (TryWireSceneCameraRig(out CCS_CharacterCameraController sceneCameraRig))
            {
                SetComponentEnabled(playerCameraController, false);
                DisableForeignCameraControllers(sceneCameraRig);
                ConfigureOwnerAudioListener(sceneCameraRig);
                return;
            }

            SetComponentEnabled(playerCameraController, true);
            DisableForeignCameraControllers(playerCameraController);
            ConfigureOwnerAudioListener(null);
        }

        private void ConfigureRemotePlayer()
        {
            SetInputEnabled(false);
            SetComponentEnabled(motor, false);
            SetComponentEnabled(controllerService, false);
            SetComponentEnabled(debugHud, false);
            SetComponentEnabled(playerCameraController, false);
            SetCharacterControllerEnabled(false);
            DisableEmbeddedPlayerCamera();
            ApplyNetworkTransformAuditSettings();
        }

        private void EnsureCameraFollowAnchor()
        {
            if (cameraFollowAnchor == null)
            {
                cameraFollowAnchor = GetComponentInChildren<CCS_CharacterCameraFollowAnchor>(true);
            }

            if (cameraFollowAnchor != null)
            {
                cameraFollowAnchor.ResolveReferences();
                return;
            }

            Transform legacyLookTarget = transform.Find("CameraPivot/CameraLookTarget");
            GameObject anchorObject = new GameObject("CameraFollowAnchor");
            anchorObject.transform.SetParent(transform, false);
            cameraFollowAnchor = anchorObject.AddComponent<CCS_CharacterCameraFollowAnchor>();

            if (legacyLookTarget != null)
            {
                legacyLookTarget.SetParent(anchorObject.transform, false);
            }

            cameraFollowAnchor.Configure(
                transform,
                legacyLookTarget != null ? legacyLookTarget : cameraLookTarget,
                1.05f);
        }

        private void ApplyNameplateVisibility()
        {
            if (nameplateBillboard != null)
            {
                nameplateBillboard.ApplyNameplateVisibility(IsOwner);
            }
        }

        private void ApplyPlayerVisuals()
        {
            ApplyBodyMaterial();
            ApplyGlassesMaterial();
        }

        private void ApplyDisplayProfileLayout()
        {
            CCS_TestPlayerOfflineBootstrap bootstrap = GetComponent<CCS_TestPlayerOfflineBootstrap>();
            if (bootstrap == null || bootstrap.DisplayProfile == null)
            {
                return;
            }

            CCS_TestPlayerDisplayProfileApplicator.ApplyVisualLayout(gameObject, bootstrap.DisplayProfile);
            if (IsOwner)
            {
                CCS_TestPlayerDisplayProfileApplicator.ApplyGameplayProfiles(gameObject, bootstrap.DisplayProfile);
            }
        }

        private void ApplyBodyMaterial()
        {
            if (bodyRenderer == null)
            {
                return;
            }

            Material selectedMaterial = yellowBodyMaterial != null
                ? yellowBodyMaterial
                : bodyRenderer.sharedMaterial;
            if (selectedMaterial == null)
            {
                return;
            }

            Material[] materials = bodyRenderer.sharedMaterials;
            if (materials == null || materials.Length == 0)
            {
                bodyRenderer.sharedMaterial = selectedMaterial;
                return;
            }

            materials[0] = selectedMaterial;
            bodyRenderer.sharedMaterials = materials;
        }

        private void ApplyGlassesMaterial()
        {
            if (glassesRenderer == null || blackGlassesMaterial == null)
            {
                return;
            }

            glassesRenderer.sharedMaterial = blackGlassesMaterial;
        }

        private void ConfigureOwnerAudioListener(CCS_CharacterCameraController sceneCameraRig)
        {
            AudioListener preferredListener = null;
            if (sceneCameraRig != null)
            {
                Camera sceneCamera = sceneCameraRig.GetComponentInChildren<Camera>(true);
                preferredListener = CCS_SingleAudioListenerUtility.FindListenerOnCamera(sceneCamera);
                DisableEmbeddedPlayerCamera();
            }
            else
            {
                Camera playerCamera = GetEmbeddedPlayerCamera();
                if (playerCamera != null)
                {
                    playerCamera.enabled = true;
                    preferredListener = CCS_SingleAudioListenerUtility.FindListenerOnCamera(playerCamera);
                }
            }

            CCS_SingleAudioListenerUtility.EnsureSingleActiveListener(preferredListener);
        }

        private void DisableEmbeddedPlayerCamera()
        {
            Camera playerCamera = GetEmbeddedPlayerCamera();
            if (playerCamera == null)
            {
                return;
            }

            AudioListener playerListener = playerCamera.GetComponent<AudioListener>();
            CCS_SingleAudioListenerUtility.SetListenerEnabled(playerListener, false);
            playerCamera.enabled = false;
        }

        private Camera GetEmbeddedPlayerCamera()
        {
            Transform embeddedCamera = transform.Find("Main Camera");
            return embeddedCamera != null ? embeddedCamera.GetComponent<Camera>() : null;
        }

        private bool TryWireSceneCameraRig(out CCS_CharacterCameraController sceneCameraRig)
        {
            sceneCameraRig = ResolveSceneCameraRig();
            if (sceneCameraRig == null)
            {
                boundSceneCameraRig = null;
                return false;
            }

            if (cameraFollowAnchor == null)
            {
                cameraFollowAnchor = GetComponentInChildren<CCS_CharacterCameraFollowAnchor>(true);
            }

            Transform followTarget = cameraFollowAnchor != null
                ? cameraFollowAnchor.FollowTransform
                : cameraPivot;
            Transform lookTarget = cameraFollowAnchor != null && cameraFollowAnchor.LookTarget != null
                ? cameraFollowAnchor.LookTarget
                : cameraLookTarget;

            if (followTarget == null || lookTarget == null)
            {
                boundSceneCameraRig = null;
                return false;
            }

            boundSceneCameraRig = sceneCameraRig;
            sceneCameraRig.enabled = true;
            sceneCameraRig.BindFollowTargets(followTarget, lookTarget);
            CCS_NetworkPlayerDebugLog.LogCameraBind(
                NetworkObject,
                sceneCameraRig,
                followTarget,
                lookTarget);
            CCS_TestPlayerSessionEvents.RaiseLocalPlayerReady(
                new CCS_TestPlayerSessionContext(
                    OwnerClientId,
                    gameObject,
                    isNetworkSession: true,
                    IsOwner));
            return true;
        }

        private static CCS_CharacterCameraController ResolveSceneCameraRig()
        {
            GameObject sceneCameraRigObject = GameObject.Find(CCS_NetcodeTestConstants.SceneCameraRigName);
            if (sceneCameraRigObject == null)
            {
                return null;
            }

            CCS_CharacterCameraController sceneCameraRig =
                sceneCameraRigObject.GetComponent<CCS_CharacterCameraController>();
            if (sceneCameraRig == null)
            {
                return null;
            }

            if (sceneCameraRig.transform.Find("CinemachineCamera_TP") == null)
            {
                return null;
            }

            return sceneCameraRig;
        }

        private void DisableForeignCameraControllers(CCS_CharacterCameraController activeController)
        {
            CCS_CharacterCameraController[] cameraControllers =
                FindObjectsByType<CCS_CharacterCameraController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < cameraControllers.Length; i++)
            {
                CCS_CharacterCameraController candidate = cameraControllers[i];
                if (candidate == null || candidate == activeController)
                {
                    continue;
                }

                candidate.enabled = false;
            }
        }

        private void RestoreSceneCameraRig()
        {
            CCS_CharacterCameraController sceneCameraRig = ResolveSceneCameraRig();
            if (sceneCameraRig != null)
            {
                sceneCameraRig.enabled = false;
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.path != CCS_NetcodeTestConstants.MasterTestScenePath
                && scene.name != CCS_NetcodeTestConstants.MasterTestSceneName)
            {
                return;
            }

            if (!IsSpawned)
            {
                return;
            }

            RefreshLocalConfiguration("MasterTestSceneLoaded");
            ApplyPlayerVisuals();
            ApplyNameplateVisibility();
            LogNetworkSpawnState();
            LogTransformAuthorityState();
            CCS_SingleAudioListenerUtility.EnsureSingleActiveListener();
        }

        public void RefreshLocalConfiguration(string reason)
        {
            if (!IsSpawned)
            {
                return;
            }

            ResolveReferences();
            ReapplyOwnershipConfiguration();

            CCS_NetworkControllerAuditDiagnostics.LogOwnershipRefresh(
                NetworkObject,
                reason,
                inputProvider,
                motor,
                characterController,
                cameraFollowAnchor,
                boundSceneCameraRig);

            if (IsOwner && NetworkObject != null)
            {
                CCS_NetworkSpawnDebugLog.LogOwnerControlState(
                    NetworkObject,
                    inputProvider,
                    motor,
                    characterController,
                    reason);
            }
        }

        private void ReapplyOwnershipConfiguration()
        {
            if (IsOwner)
            {
                ConfigureOwnerPlayer();
            }
            else
            {
                ConfigureRemotePlayer();
            }
        }

        private void SynchronizeBodyYawReplication()
        {
            if (IsOwner)
            {
                if (motor != null && motor.enabled)
                {
                    replicatedBodyYaw.Value = transform.eulerAngles.y;
                }

                return;
            }

            float targetYaw = replicatedBodyYaw.Value;
            Vector3 euler = transform.eulerAngles;
            if (Mathf.Abs(Mathf.DeltaAngle(euler.y, targetYaw)) < 0.01f)
            {
                return;
            }

            transform.rotation = Quaternion.Euler(0f, targetYaw, 0f);
        }

        private void ApplyNetworkTransformAuditSettings()
        {
            if (networkTransform == null)
            {
                return;
            }

            bool shouldEnableNetworkTransform = !CCS_NetworkControllerAuditDiagnostics.DisableNetworkTransformForAudit;
            if (networkTransform.enabled != shouldEnableNetworkTransform)
            {
                networkTransform.enabled = shouldEnableNetworkTransform;
            }
        }

        private void SetInputEnabled(bool enabled)
        {
            if (inputProvider != null)
            {
                if (!inputProvider.enabled)
                {
                    inputProvider.enabled = true;
                }

                inputProvider.SetInputAccepted(enabled);
            }

            CCS_NetworkPlayerDebugLog.LogInputState(NetworkObject, enabled, inputProvider);
        }

        private void SetCharacterControllerEnabled(bool enabled)
        {
            if (characterController != null)
            {
                characterController.enabled = enabled;
            }
        }

        private void LogNetworkSpawnState()
        {
            ResolveReferences();
            ApplyNameplateVisibility();
            CCS_NetworkPlayerDebugLog.LogNetworkSpawn(
                this,
                inputProvider,
                motor,
                playerCameraController,
                nameplateBillboard,
                bodyRenderer,
                glassesTransform);
        }

        private void LogTransformAuthorityState()
        {
            CCS_NetworkMovementDebugLog.LogTransformAuthority(
                NetworkObject,
                networkTransform,
                motor != null && motor.enabled,
                characterController != null && characterController.enabled);
        }

        private static void SetComponentEnabled(Behaviour behaviour, bool enabled)
        {
            if (behaviour != null)
            {
                behaviour.enabled = enabled;
            }
        }

        #endregion
    }
}
