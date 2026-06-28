using CCS.Modules.CharacterController.Tests;
using CCS.Project;
using UnityEngine;

// SCRIPT: CCS_CharacterControllerMasterTestLayoutConstants

// CATEGORY: Modules / CharacterController / Editor / Validation

// PURPOSE: Expected layout for SCN_CCS_CharacterController_MasterTest.

// PLACEMENT: Editor layout constants. Not attached to GameObjects.

// AUTHOR: James Schilz

// CREATED: 2026-06-07

// NOTES: Source-of-truth positions for builder setup and validator checks.

// =============================================================================



namespace CCS.Modules.CharacterController.Editor

{

    public static class CCS_CharacterControllerMasterTestLayoutConstants

    {

        public const string ScenesRootPath = "Assets/CCS/Scenes";



        public const string MasterTestScenePath =

            ScenesRootPath + "/CharacterController/SCN_CCS_CharacterController_MasterTest.unity";



        public const string BootstrapPrefabPath =

            "Assets/CCS/Project/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";



        public const string EnvironmentParentName = "Environment";



        public const string TestPointsParentName = "TestPoints";



        public const string GroundPrefabPath =

            "Assets/CCS/Modules/CharacterController/Prefabs/Environment/PF_CCS_TestGround_OneMeterGrid.prefab";



        public const string BuildingPrefabPath =

            "Assets/CCS/Modules/CharacterController/Prefabs/Environment/PF_CCS_TestBuilding_RoofPlatform.prefab";



        public const string StairsPrefabPath =

            "Assets/CCS/Modules/CharacterController/Prefabs/Environment/PF_CCS_TestStairs_RoofAccess.prefab";



        public const string RampPrefabPath =

            "Assets/CCS/Modules/CharacterController/Prefabs/Environment/PF_CCS_TestRamp_RoofAccess.prefab";



        public const string DoorPrefabPath =

            "Assets/CCS/Modules/CharacterController/Prefabs/Environment/PF_CCS_TestDoor_Single.prefab";



        public const string NetworkedPlayerPrefabPath =
            CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath;

        public const string TestPlayerDisplayProfilePath =
            CCS_TestPlayerPrefabConstants.DefaultDisplayProfilePath;

        public const string NameplateRootObjectName = "NameplateRoot";

        public const string PlayerNameTextObjectName = "PlayerNameText";

        public const string MasterTestSpawnControllerObjectName = "CCS_MasterTestSpawnController";

        public const string MasterTestTestingManagerObjectName = CCS_ProjectAudioConstants.MasterTestTestingManagerObjectName;

        public const string MasterTestAmbientAudioObjectName = CCS_ProjectAudioConstants.MasterTestAmbientAudioObjectName;

        public const string DefaultPlayerDisplayName = "Player";

        public static readonly Vector3 NameplateRootLocalPosition = new Vector3(0f, 2.12f, 0f);

        public const string NpcPrefabPath =

            "Assets/CCS/Modules/CharacterController/Prefabs/Player/PF_CCS_CharacterController_TestNPC.prefab";



        public const string CameraRigPrefabPath =

            "Assets/CCS/Modules/CharacterController/Prefabs/Camera/PF_CCS_CharacterCameraRig.prefab";

        public const string CameraProfilePath =

            "Assets/CCS/Modules/CharacterController/Profiles/Camera/CCS_CharacterCameraProfile_ThirdPersonSurvival.asset";

        public const string AimCameraProfilePath =
            CCS_CharacterControllerConstants.AimCameraProfilePath;

        public const float ExpectedThirdPersonCameraDistance = CCS_CharacterControllerConstants.ThirdPersonCameraDistanceTuned;

        public const float ExpectedAimCameraDistance = CCS_CharacterControllerConstants.AimCameraDistanceTuned;

        public const float ExpectedTrackingTargetLocalHeight = CCS_CharacterControllerConstants.CameraPitchTargetLocalHeight;

        public const float ExpectedThirdPersonShoulderOffsetX = 0.20f;

        public const float ExpectedThirdPersonShoulderOffsetY = 0.20f;

        public const float ExpectedThirdPersonShoulderOffsetZ = 0f;

        public const float ExpectedThirdPersonVerticalArmLength = 0.45f;

        public const float ExpectedThirdPersonFieldOfView = 62f;

        public const float ExpectedAimShoulderOffsetX =
            CCS_CharacterControllerConstants.AimCameraShoulderOffsetXTuned;

        public const float ExpectedAimShoulderOffsetY = 0.15f;

        public const float ExpectedAimShoulderOffsetZ = 0f;

        public const float ExpectedAimTrackingTargetLocalHeight =
            CCS_CharacterControllerConstants.AimCameraTrackingHeightTuned;

        public const float ExpectedAimVerticalArmLength = 0.28f;

        public const float ExpectedAimCameraSide = 1f;

        public const float ExpectedAimBlendDurationSeconds = 0.45f;

        public const float ExpectedAimBlendDurationMaxSeconds = 0.5f;

        public const float ExpectedAimFieldOfView =
            CCS_CharacterControllerConstants.AimCameraFieldOfViewTuned;

        public static readonly Vector3 ExpectedCameraLookTargetLocalPosition =
            CCS_CharacterControllerConstants.CameraLookTargetLocalPosition;

        public static readonly Vector3 GlassesVisualLocalPosition = new Vector3(0f, 1.6f, 0.222f);

        public static readonly Vector3 GlassesVisualLocalEuler = new Vector3(180f, 180f, 90f);

        public static readonly Vector3 GlassesVisualLocalScale = new Vector3(0.3f, 0.3f, 0.3f);

        public static readonly Vector3 CapsuleVisualLocalPosition = new Vector3(0f, 1f, 0f);

        public static readonly Vector3 CapsuleVisualLocalScale = new Vector3(0.7f, 1f, 0.7f);

        public const float CameraPitchTargetLocalHeight = CCS_CharacterControllerConstants.CameraPitchTargetLocalHeight;

        public const float ExpectedVerticalArmLength = ExpectedThirdPersonVerticalArmLength;

        public const float ExpectedVerticalOrbitDefault = 0f;

        public const float ExpectedVerticalOrbitMin = -45f;

        public const float ExpectedVerticalOrbitMax = 70f;

        public const float ExpectedAimVerticalOrbitMin =
            CCS_CharacterControllerConstants.ThirdPersonAimPitchDownLimitDegrees;

        public const float ExpectedAimVerticalOrbitMax =
            CCS_CharacterControllerConstants.ThirdPersonAimPitchUpLimitDegrees;

        public const float MinimumAimVerticalOrbitMax = 55f;

        public const float ExpectedMouseSensitivityX = 0.12f;

        public const float ExpectedMouseSensitivityY = 0.10f;

        public const float ExpectedGamepadSensitivityX = 90f;

        public const float ExpectedGamepadSensitivityY = 70f;



        public const string GroundGridMaterialPath =

            "Assets/CCS/Modules/CharacterController/Materials/Environment/M_CCS_TestGround_1mGrid.mat";



        public const string BrickMaterialPath =

            "Assets/CCS/Modules/CharacterController/Materials/Environment/M_CCS_TestBrick.mat";



        public const string ConcreteMaterialPath =

            "Assets/CCS/Modules/CharacterController/Materials/Environment/M_CCS_TestConcrete.mat";



        public const string DoorWoodMaterialPath =

            "Assets/CCS/Modules/CharacterController/Materials/Environment/M_CCS_TestDoorWood.mat";



        public const string PlayerYellowMaterialPath =

            "Assets/CCS/Modules/CharacterController/Materials/Player/M_CCS_TestPlayerYellow.mat";



        public const string PlayerGreenMaterialPath =

            "Assets/CCS/Modules/CharacterController/Materials/Player/M_CCS_TestPlayerGreen.mat";



        public const string PlayerRedMaterialPath =

            "Assets/CCS/Modules/CharacterController/Materials/Player/M_CCS_TestPlayerRed.mat";



        public const string PlayerBlackMaterialPath =

            "Assets/CCS/Modules/CharacterController/Materials/Player/M_CCS_TestPlayerBlack.mat";



        public const string GroundInstanceName = "PF_CCS_TestGround_OneMeterGrid";



        public const string BuildingInstanceName = "PF_CCS_TestBuilding_RoofPlatform";



        public const string StairsInstanceName = "PF_CCS_TestStairs_RoofAccess";



        public const string RampInstanceName = "PF_CCS_TestRamp_RoofAccess";



        public const string DoorInstanceName = "PF_CCS_TestDoor_Single";



        public const string BootstrapInstanceName = "PF_CCS_Survival_BootstrapRoot";



        public const string PlayerInstanceName = CCS_TestPlayerPrefabConstants.DeprecatedOfflinePlayerInstanceName;

        public const string NetworkedPlayerInstanceName = CCS_TestPlayerPrefabConstants.NetworkedPlayerInstanceName;



        public const string NpcInstanceName = "PF_CCS_CharacterController_TestNPC";



        public const string CameraRigInstanceName = "PF_CCS_CharacterCameraRig";



        public const string DirectionalLightName = "Directional Light";



        public const string GlassesVisualName = "VisualGlasses";



        public const string CapsuleVisualName = "CapsuleVisual";



        public const float PositionTolerance = 0.2f;

        public const float RampEndpointPositionTolerance = 0.5f;



        public const float RoofHeightMeters = 3f;



        public const float RampSlopeDegrees = 26.565f;



        public static readonly Vector3 CourseOrigin = new Vector3(30f, 0f, 30f);



        public static readonly Vector3 GroundPosition = Vector3.zero;



        public static readonly Vector3 BuildingPosition = CourseOrigin;



        public const float BuildingHalfWidthMeters = 4f;



        public const float BuildingHalfDepthMeters = 5f;



        public const float BuildingFrontEdgeLocalZ = 5f;



        public const float BuildingRearEdgeLocalZ = -5f;



        public const float StairsOuterEdgeLocalZ = -9.2f;



        public const float RampOuterEdgeLocalZ = 11f;



        public const float WallTopHeightMeters = 2.9f;

        public const float WallThicknessMeters = 0.2f;

        public const float WallHalfThicknessMeters = WallThicknessMeters * 0.5f;

        public const float WallFrontCenterLocalZ = BuildingFrontEdgeLocalZ - WallHalfThicknessMeters;

        public const float WallBackCenterLocalZ = BuildingRearEdgeLocalZ + WallHalfThicknessMeters;

        public const float WallLeftCenterLocalX = -BuildingHalfWidthMeters + WallHalfThicknessMeters;

        public const float WallRightCenterLocalX = BuildingHalfWidthMeters - WallHalfThicknessMeters;

        public const float RoofDeckCenterYMeters = 2.945f;

        public const float RoofDeckThicknessMeters = 0.11f;

        public const float DoorSlabThicknessMeters = 0.08f;

        public const float DoorSlabWidthMeters = 1.9f;

        public const float DoorSlabHeightMeters = 2.45f;

        public const float DoorWallPlaneXMeters = 4f;

        public const float DoorRootLocalXMeters = DoorWallPlaneXMeters - (DoorSlabThicknessMeters * 0.5f);

        public const float DoorHingeLocalZMeters = -0.95f;

        public const float DoorSlabHalfWidthMeters = DoorSlabWidthMeters * 0.5f;

        public static readonly Vector3 DoorHingePivotLocalPosition = new Vector3(0f, 0f, DoorHingeLocalZMeters);

        public static readonly Vector3 DoorSlabLocalPositionFromPivot = new Vector3(0f, 1.225f, DoorSlabHalfWidthMeters);

        public static readonly Vector3 DoorSlabCenterBuildingLocal = new Vector3(DoorRootLocalXMeters, 1.225f, 0f);

        public const float RampRunMeters = 6f;

        public const float RampRiseMeters = 3f;

        public const float RampSlopeLengthMeters = 6.708203932499369f;

        public static readonly Vector3 RampHighEndpointLocal = new Vector3(0f, 3f, 0f);

        public static readonly Vector3 RampLowEndpointLocal = new Vector3(0f, 0f, 6f);

        public static readonly Vector3 RampHighEndpointWorld = new Vector3(30f, 3f, 35.35f);

        public static readonly Vector3 RampLowEndpointWorld = new Vector3(30f, 0.25f, 40.65f);

        public static readonly Vector3 StairsPosition = new Vector3(30f, 0f, 20.8f);



        public static readonly Vector3 RampPosition = new Vector3(29.84f, 0f, 40.6f);



        public static readonly Vector3 RampRotationEuler = new Vector3(0f, 180f, 0f);



        public static readonly Vector3 DoorLocalPosition = new Vector3(DoorRootLocalXMeters, 0f, 0f);

        public static readonly Vector3 DoorLocalEuler = Vector3.zero;

        public static readonly Vector3 DoorHingePivotBuildingLocal = new Vector3(DoorRootLocalXMeters, 0f, DoorHingeLocalZMeters);



        public static readonly Vector3 CameraRigPosition = new Vector3(22f, 0f, 18f);



        public static readonly Vector3 NpcPosition = new Vector3(24f, 0.25f, 22f);



        public static readonly Vector3 DirectionalLightEuler = new Vector3(50f, -30f, 0f);



        public static readonly string[] RequiredRootObjectNames =

        {

            BootstrapInstanceName,

            EnvironmentParentName,

            TestPointsParentName,

            MasterTestSpawnControllerObjectName,

            MasterTestTestingManagerObjectName,

            CCS_MasterTestUiConstants.MasterTestUiCanvasObjectName,

            CameraRigInstanceName,

            DirectionalLightName,

        };



        public static readonly string[] EnvironmentInstanceNames =

        {

            GroundInstanceName,

            BuildingInstanceName,

            StairsInstanceName,

            RampInstanceName,

        };



        public static readonly string[] SpawnPointNames =

        {

            "TP_Spawn_Host",

            "TP_Spawn_Client_01",

            "TP_Spawn_Client_02",

        };



        public static readonly Vector3[] SpawnPointPositions =

        {

            new Vector3(22f, 0.25f, 18f),

            new Vector3(25f, 0.25f, 18f),

            new Vector3(28f, 0.25f, 18f),

        };



        public static readonly string[] TraversalPointNames =

        {

            "TP_StairsBottom",

            "TP_StairsTop",

            "TP_RoofCenter",

            "TP_RampTop",

            "TP_RampBottom",

            "TP_DoorOutside",

            "TP_DoorInside",

            "TP_CoverInside",

            "TP_LoopComplete",

        };



        public static readonly Vector3[] TraversalPointPositions =

        {

            new Vector3(30f, 0.25f, 21.15f),

            new Vector3(30f, 3f, 25.35f),

            new Vector3(30f, 3f, 30f),

            new Vector3(30f, 3f, 35.35f),

            new Vector3(30f, 0.25f, 40.65f),

            new Vector3(35.2f, 0.25f, 30f),

            new Vector3(33.6f, 0.25f, 30f),

            new Vector3(30f, 0.25f, 30f),

            new Vector3(24f, 0f, 19.3f),

        };



        public static readonly string[] BuildingWallPieceNames =
        {
            "Wall_Back",
            "Wall_Front",
            "Wall_Left",
            "Wall_Right_Back",
            "Wall_Right_Front",
            "Wall_Right_DoorHeader",
            "RoofDeck",
        };

        public static readonly string[] LegacyObjectNamesToRemove =

        {

            "CCS_TestSceneLabel",

            "Future",

            "ControllerTraversalCourse",

            "PF_CCS_ControllerTraversalCourse",

            "CCS_TestGround_OneMeterGrid",

            "PF_CCS_TestBuilding",

            "PF_CCS_TestStairs",

            "PF_CCS_TestRamp",

            "TP_Spawn",

        };

        public static readonly string[] ForbiddenDebugHudObjectNames =

        {

            "DebugHud",

            "CharacterDebugHud",

            "DebugPanel",

            "DebugText",

        };

        public const string CharacterControllerDebugHudTitle = "CCS Character Controller Debug HUD";



        public static Quaternion GetSpawnFacingRotation(Vector3 spawnPosition)

        {

            Vector3 lookDirection = CourseOrigin - spawnPosition;

            lookDirection.y = 0f;

            if (lookDirection.sqrMagnitude < 0.001f)

            {

                return Quaternion.identity;

            }



            return Quaternion.LookRotation(lookDirection.normalized, Vector3.up);

        }

    }

}


