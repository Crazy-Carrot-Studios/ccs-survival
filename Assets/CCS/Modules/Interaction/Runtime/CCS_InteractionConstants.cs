// =============================================================================
// SCRIPT: CCS_InteractionConstants
// CATEGORY: Modules / Interaction / Runtime
// PURPOSE: Canonical paths, IDs, and version metadata for the Interaction module.
// PLACEMENT: Runtime constants. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.5.4 — pickup/door interaction flow, forward volume, closest-point LOS.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public static class CCS_InteractionConstants
    {
        public const string ModuleVersion = "0.5.4";

        public const string ModuleLogCategory = "Interaction";

        public const string ModuleRootPath = "Assets/CCS/Modules/Interaction";

        public const string InteractableLayerName = "Interactable";

        public const string InteractableTagName = "Interactable";

        public const string ScannerProfilePath =
            ModuleRootPath + "/Tests/Profiles/CCS_InteractionScannerProfile_Default.asset";

        public const string ScannerProfileId = "ccs.survival.profile.interaction.scanner.default";

        public const string TestPickupInteractablePrefabPath =
            ModuleRootPath + "/Tests/Prefabs/PF_CCS_TestInteractable_PickupItem.prefab";

        public const string TestPickupInteractableInstanceName = "PF_CCS_TestInteractable_PickupItem";

        public const string TestDetectionCubeObjectName = "CCS_TestDetectionCube";

        public const string TestDetectionCubeBootstrapObjectName = "CCS_TestDetectionCubeSceneBootstrap";

        public const string PickupItemSpawnerObjectName = "CCS_TestPickupItemSpawner";

        public const string MasterTestSpawnOriginObjectPath = "TestPoints/TP_Spawn_Host";

        public const float TestDetectionCubeForwardDistance = 3f;

        public const float TestDetectionCubeHeightAboveGround = 1f;

        public const float TestPickupSpawnForwardDistance = 2.5f;

        public const float DefaultDetectionRange = 5f;

        public const float DefaultBroadDetectionRadius = 3f;

        public const float DefaultStrictPickupDistance = 1.5f;

        public const float DefaultInteractionHalfWidth = 0.65f;

        public const float DefaultInteractionHalfHeight = 1.25f;

        public const float DefaultInteractionVolumeWidth = DefaultInteractionHalfWidth * 2f;

        public const float DefaultPickupMaxCameraAngleAssist = 35f;

        public const float DefaultPickupMaxReachDistance = 1.5f;

        public const float DefaultLineOfSightSphereRadius = 0.1f;

        public const float DefaultLineOfSightDistancePadding = 0.05f;

        public const string PlayerLayerName = "Player";

        public const string InteractionScanOriginObjectName = "InteractionScanOrigin";

        public const float InteractionScanOriginLocalHeight = 1f;

        public const string PlayerRightHandSocketName = "RightHand";

        public const string PlayerPickupRightHandAnimationName = "PickUp_RH";

        public const string PlayerWalkThroughDoorRightHandAnimationName = "WalkThroughDoor_RH";

        public const float WalkThroughDoorStrictRangeMin = 1.25f;

        public const float WalkThroughDoorStrictRangeMax = 1.75f;

        public const float DefaultWalkThroughDoorStrictRange = 1.75f;

        public const string TestWalkThroughDoorObjectName = "CCS_TestWalkThroughDoor";

        public const float TestWalkThroughDoorForwardDistance = 4.5f;

        public const float TestWalkThroughDoorLateralOffset = 2.5f;

        public const string TestWalkThroughDoorDisplayName = "Test Door";

        public const string BuildingDoorInteractableObjectName = "CCS_BuildingDoor_Interactable";

        public const string BuildingDoorDisplayName = "Door";

        public const string TestDoorSinglePrefabPath =
            "Assets/CCS/Modules/CharacterController/Prototyping/Prefabs/Environment/PF_CCS_TestDoor_Single.prefab";

        public const string TestBuildingRoofPlatformPrefabPath =
            "Assets/CCS/Modules/CharacterController/Prototyping/Prefabs/Environment/PF_CCS_TestBuilding_RoofPlatform.prefab";

        public const string TestDoorSingleRootObjectName = "PF_CCS_TestDoor_Single";

        public const string TestDoorSlabObjectName = "DoorSlab";

        public const string TestDoorHingePivotObjectName = "DoorHingePivot";

        public const string NetworkedTestPlayerPrefabPath =
            "Assets/CCS/Modules/CharacterController/Prefabs/Player/PF_CCS_CharacterController_Player_Networked.prefab";

        public const string MasterTestScenePath =
            "Assets/CCS/Modules/CharacterController/Scenes/Validation/SCN_CCS_CharacterController_Validation.unity";

        public const string InteractionPromptHudRootName = "InteractionPromptHudRoot";

        public const string InteractionPromptPanelObjectName = "InteractionPromptPanel";

        public const string InteractionPromptTextObjectName = "InteractionPromptText";

        public const string DefaultInteractionPromptText = "Press [E]";

        public const float InteractionPromptFontSize = 36f;

        public const float PickUpRightHandLockDuration = 2f;

        public const float WalkThroughDoorRightHandLockDuration = 1.3f;

        public const string TestDetectionCubeDisplayName = "Test Cube";
    }
}
