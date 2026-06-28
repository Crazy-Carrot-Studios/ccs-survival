using System.Collections.Generic;

using System.Globalization;

using System.IO;

using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Tests;
using CCS.Modules.CharacterController.Tests.Netcode;
using CCS.Modules.Interaction;
using CCS.Modules.Weapons;
using CCS.Project;

using Unity.Cinemachine;
using UnityEditor;

using UnityEditor.SceneManagement;

using Unity.Netcode;

using Unity.Netcode.Components;

using UnityEngine;

using UnityEngine.SceneManagement;

using UnityEngine.EventSystems;

using UnityEngine.InputSystem.UI;

using TMPro;



// =============================================================================

// SCRIPT: CCS_CharacterControllerMasterTestValidator

// CATEGORY: Modules / CharacterController / Editor / Validation

// PURPOSE: Reports layout problems in SCN_CCS_CharacterController_MasterTest.

// PLACEMENT: Editor validation utility. Not attached to GameObjects.

// AUTHOR: James Schilz

// CREATED: 2026-06-07

// NOTES: Report-only. Does not rebuild or auto-fix the scene.

// =============================================================================



namespace CCS.Modules.CharacterController.Editor

{

    public static class CCS_CharacterControllerMasterTestValidator

    {

        #region Public Methods



        public static CCS_SurvivalValidationResult ValidateMasterTestScene()

        {

            List<string> failures = new List<string>();



            if (!System.IO.File.Exists(CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath))

            {

                return CCS_SurvivalValidationResult.Fail(

                    $"Missing asset: {CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath}");

            }



            Scene scene = EditorSceneManager.OpenScene(

                CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath,

                OpenSceneMode.Single);

            if (!scene.IsValid())

            {

                return CCS_SurvivalValidationResult.Fail(

                    $"Could not open {CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath}.");

            }



            ValidateRootHierarchy(failures);

            ValidateParents(failures);

            ValidateEnvironment(failures);

            ValidateBuildingDoor(failures);

            ValidateSpawnPoints(failures);

            ValidateTraversalPoints(failures);

            ValidateBootstrap(failures);

            ValidateMasterTestSpawnSetup(failures);

            ValidateTestingManagerAndRecordingAmbience(failures);

            AppendValidationResult(
                failures,
                CCS_CharacterControllerPhase2BValidationUtility.ValidatePhase2BFoundation());

            ValidateJoinNotificationFeed(failures);

            ValidatePlayerPrefabAssets(failures);

            ValidateMasterTestDeathUiInputContracts(failures);

            AppendValidationResult(
                failures,
                CCS_EquipmentSocketValidationUtility.ValidateAnimationRiggingPackageInstalled());
            AppendValidationResult(
                failures,
                CCS_EquipmentSocketValidationUtility.ValidateDefaultEquipmentSocketProfile());

            GameObject networkedPrefabForEquipment = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (networkedPrefabForEquipment != null)
            {
                AppendValidationResult(
                    failures,
                    CCS_EquipmentSocketValidationUtility.ValidatePlayerEquipmentSocketFoundation(
                        networkedPrefabForEquipment));
                AppendValidationResult(
                    failures,
                    CCS_EquipmentSocketValidationUtility.ValidatePlayerWeaponIkFoundation(
                        networkedPrefabForEquipment));
            }

            AppendValidationResult(
                failures,
                CCS.Modules.CharacterController.Editor.EquipmentFitStudio.CCS_EquipmentFitStudioValidationUtility.ValidateEquipmentFitStudioFoundation());

            AppendValidationResult(
                failures,
                CCS_CharacterControllerAnimationValidationUtility.ValidatePlayerAnimatorControllerAnimationIsolation());

            AppendValidationResult(
                failures,
                CCS_CharacterControllerAnimationValidationUtility.ValidateAimLocomotionAnimatorParameters());

            AppendValidationResult(
                failures,
                CCS_CharacterControllerAnimationValidationUtility.ValidateAimStrafeAnimationIsolation());

            AppendValidationResult(
                failures,
                CCS_CharacterControllerAnimationValidationUtility.ValidateRevolverUpperBodyAnimationIsolation());

            AppendValidationResult(
                failures,
                CCS_CharacterControllerAnimationValidationUtility.ValidateRevolverWildWestHardReplaceAimRuntime());

            AppendValidationResult(
                failures,
                CCS_CharacterControllerAnimationValidationUtility.ValidateNoInvectorRuntimeReferences());

            GameObject networkedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (networkedPrefab != null)
            {
                AppendValidationResult(
                    failures,
                    CCS_CharacterControllerAnimationValidationUtility.ValidatePlayerAnimatorRootMotionDisabled(
                        networkedPrefab));
                AppendValidationResult(
                    failures,
                    CCS_CharacterControllerAnimationValidationUtility.ValidatePlayerRevolverUpperBodyAnimator(
                        networkedPrefab));
            }

            ValidateCameraProfile(failures);

            ValidateCcsRuntimeScriptsDoNotWriteOrbitalAxisValues(failures);

            ValidateCameraRig(failures);

            ValidateDirectionalLight(failures);

            ValidateSingleAudioListener(failures);

            ValidateNoNetworkManager(failures);

            ValidateNoNetworkedPlayerInMasterTestScene(failures);

            ValidateNoLegacyObjects(failures);

            ValidateNoCharacterDebugHud(failures);

            ValidateNoDestroyedPrefabInstances(failures);



            if (failures.Count > 0)

            {

                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));

            }



            return CCS_SurvivalValidationResult.Pass("Character controller master test scene validated.");

        }



        public static CCS_SurvivalValidationResult ValidateMasterTestCameraBaseline()

        {

            List<string> failures = new List<string>();



            if (!System.IO.File.Exists(CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath))

            {

                return CCS_SurvivalValidationResult.Fail(

                    $"Missing asset: {CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath}");

            }



            Scene scene = EditorSceneManager.OpenScene(

                CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath,

                OpenSceneMode.Single);

            if (!scene.IsValid())

            {

                return CCS_SurvivalValidationResult.Fail(

                    $"Could not open {CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath}.");

            }



            ValidateCameraProfile(failures);

            ValidatePlayerCameraTargetsOnPrefab(

                failures,

                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);

            ValidatePlayerCameraCollisionExclusion(

                failures,

                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);

            ValidateCameraFollowAnchorRotationChain(

                failures,

                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);

            ValidateCameraRigPrefabAsset(failures, baselinePass: true);

            ValidateSceneCameraRigBaseline(failures);



            if (failures.Count > 0)

            {

                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));

            }



            return CCS_SurvivalValidationResult.Pass(

                "Character controller master test camera baseline validated without obstacle avoidance.");

        }



        #endregion



        #region Private Methods



        private static void ValidateRootHierarchy(List<string> failures)

        {

            for (int i = 0; i < CCS_CharacterControllerMasterTestLayoutConstants.RequiredRootObjectNames.Length; i++)

            {

                string objectName = CCS_CharacterControllerMasterTestLayoutConstants.RequiredRootObjectNames[i];

                Transform rootObject = FindRootTransform(objectName);

                AppendIfMissing(

                    failures,

                    rootObject != null,

                    $"Scene root is missing {objectName}.");

                if (rootObject != null && rootObject.parent != null)

                {

                    failures.Add($"{objectName} must be a scene root object.");

                }

            }

        }



        private static void ValidateParents(List<string> failures)

        {

            AppendIfMissing(

                failures,

                GameObject.Find(CCS_CharacterControllerMasterTestLayoutConstants.EnvironmentParentName) != null,

                $"Missing parent object {CCS_CharacterControllerMasterTestLayoutConstants.EnvironmentParentName}.");

            AppendIfMissing(

                failures,

                GameObject.Find(CCS_CharacterControllerMasterTestLayoutConstants.TestPointsParentName) != null,

                $"Missing parent object {CCS_CharacterControllerMasterTestLayoutConstants.TestPointsParentName}.");

        }



        private static void ValidateEnvironment(List<string> failures)

        {

            Transform environment = GameObject.Find(CCS_CharacterControllerMasterTestLayoutConstants.EnvironmentParentName)?.transform;

            if (environment == null)

            {

                return;

            }



            ValidateEnvironmentEntry(

                failures,

                environment,

                CCS_CharacterControllerMasterTestLayoutConstants.GroundInstanceName,

                CCS_CharacterControllerMasterTestLayoutConstants.GroundPosition,

                CCS_CharacterControllerMasterTestLayoutConstants.GroundGridMaterialPath);

            ValidateEnvironmentEntry(

                failures,

                environment,

                CCS_CharacterControllerMasterTestLayoutConstants.BuildingInstanceName,

                CCS_CharacterControllerMasterTestLayoutConstants.BuildingPosition,

                null);

            Transform building = FindDirectChild(environment, CCS_CharacterControllerMasterTestLayoutConstants.BuildingInstanceName);

            if (building != null)
            {
                ValidateBuildingStructure(failures, building);
            }

            ValidateEnvironmentEntry(

                failures,

                environment,

                CCS_CharacterControllerMasterTestLayoutConstants.StairsInstanceName,

                CCS_CharacterControllerMasterTestLayoutConstants.StairsPosition,

                CCS_CharacterControllerMasterTestLayoutConstants.ConcreteMaterialPath);

            ValidateEnvironmentEntry(

                failures,

                environment,

                CCS_CharacterControllerMasterTestLayoutConstants.RampInstanceName,

                CCS_CharacterControllerMasterTestLayoutConstants.RampPosition,

                CCS_CharacterControllerMasterTestLayoutConstants.ConcreteMaterialPath);



            ValidateUniqueEnvironmentCount(failures, environment);

            ValidateRampOrientation(failures, environment);

            ValidateRampPlacement(failures, environment);

            ValidateStairsAttachment(failures, environment);

        }



        private static void ValidateUniqueEnvironmentCount(List<string> failures, Transform environment)

        {

            for (int i = 0; i < CCS_CharacterControllerMasterTestLayoutConstants.EnvironmentInstanceNames.Length; i++)

            {

                string instanceName = CCS_CharacterControllerMasterTestLayoutConstants.EnvironmentInstanceNames[i];

                int count = CountDirectChildrenNamed(environment, instanceName);

                if (count != 1)

                {

                    failures.Add($"Environment must contain exactly one {instanceName} (found {count}).");

                }

            }

        }



        private static void ValidateEnvironmentEntry(

            List<string> failures,

            Transform environment,

            string objectName,

            Vector3 expectedPosition,

            string expectedMaterialPath)

        {

            Transform entry = FindDirectChild(environment, objectName);

            AppendIfMissing(

                failures,

                entry != null,

                $"Environment is missing {objectName}.");

            if (entry == null)

            {

                return;

            }



            ValidatePosition(failures, objectName, entry.position, expectedPosition);

            if (!string.IsNullOrEmpty(expectedMaterialPath))

            {

                ValidateRenderersUseMaterial(failures, entry, expectedMaterialPath, objectName);

            }

        }



        private static void ValidateBuildingStructure(List<string> failures, Transform building)
        {
            if (FindChildByName(building, "Body") != null)
            {
                failures.Add("Building must not use a solid Body cube. Use wall pieces instead.");
            }

            if (FindChildByName(building, "Wall_Right") != null)
            {
                failures.Add("Right wall must be split into Wall_Right_Back, Wall_Right_Front, and Wall_Right_DoorHeader.");
            }

            for (int i = 0; i < CCS_CharacterControllerMasterTestLayoutConstants.BuildingWallPieceNames.Length; i++)
            {
                string wallName = CCS_CharacterControllerMasterTestLayoutConstants.BuildingWallPieceNames[i];
                AppendIfMissing(
                    failures,
                    FindChildByName(building, wallName) != null,
                    $"Building prefab is missing {wallName}.");
            }

            ValidateRendererMaterial(
                failures,
                building,
                "RoofDeck",
                CCS_CharacterControllerMasterTestLayoutConstants.BrickMaterialPath,
                "Building roof");

            for (int i = 0; i < CCS_CharacterControllerMasterTestLayoutConstants.BuildingWallPieceNames.Length - 1; i++)
            {
                string wallName = CCS_CharacterControllerMasterTestLayoutConstants.BuildingWallPieceNames[i];
                ValidateRendererMaterial(
                    failures,
                    building,
                    wallName,
                    CCS_CharacterControllerMasterTestLayoutConstants.BrickMaterialPath,
                    $"Building {wallName}");
            }

            Transform roofDeck = FindChildByName(building, "RoofDeck");
            if (roofDeck != null)
            {
                float roofBottom = roofDeck.localPosition.y - (roofDeck.localScale.y * 0.5f);
                float roofTop = roofDeck.localPosition.y + (roofDeck.localScale.y * 0.5f);
                if (Mathf.Abs(roofBottom - CCS_CharacterControllerMasterTestLayoutConstants.WallTopHeightMeters) > 0.03f)
                {
                    failures.Add(
                        $"Roof deck bottom must meet wall tops at Y={CCS_CharacterControllerMasterTestLayoutConstants.WallTopHeightMeters:0.##} (found {roofBottom:0.##}).");
                }

                if (Mathf.Abs(roofTop - CCS_CharacterControllerMasterTestLayoutConstants.RoofHeightMeters) > 0.03f)
                {
                    failures.Add(
                        $"Roof walk surface must remain at Y={CCS_CharacterControllerMasterTestLayoutConstants.RoofHeightMeters:0.##} (found {roofTop:0.##}).");
                }

                float halfWidth = CCS_CharacterControllerMasterTestLayoutConstants.BuildingHalfWidthMeters;
                float halfDepth = CCS_CharacterControllerMasterTestLayoutConstants.BuildingHalfDepthMeters;
                if (Mathf.Abs(roofDeck.localScale.x - (halfWidth * 2f)) > 0.05f
                    || Mathf.Abs(roofDeck.localScale.z - (halfDepth * 2f)) > 0.05f)
                {
                    failures.Add("Roof deck footprint must cover building X -4 to +4 and Z -5 to +5.");
                }
            }

            ValidateBuildingExteriorBounds(failures, building);
        }

        private static void ValidateBuildingExteriorBounds(List<string> failures, Transform building)
        {
            float exteriorX = CCS_CharacterControllerMasterTestLayoutConstants.BuildingHalfWidthMeters;
            float exteriorZ = CCS_CharacterControllerMasterTestLayoutConstants.BuildingHalfDepthMeters;
            float halfWall = CCS_CharacterControllerMasterTestLayoutConstants.WallHalfThicknessMeters;

            Transform wallFront = FindChildByName(building, "Wall_Front");
            if (wallFront != null)
            {
                float outsideZ = wallFront.localPosition.z + halfWall;
                if (Mathf.Abs(outsideZ - exteriorZ) > 0.02f)
                {
                    failures.Add($"Wall_Front outside face must align with Z=+{exteriorZ:0.#} (found {outsideZ:0.##}).");
                }
            }

            Transform wallBack = FindChildByName(building, "Wall_Back");
            if (wallBack != null)
            {
                float outsideZ = wallBack.localPosition.z - halfWall;
                if (Mathf.Abs(outsideZ + exteriorZ) > 0.02f)
                {
                    failures.Add($"Wall_Back outside face must align with Z=-{exteriorZ:0.#} (found {outsideZ:0.##}).");
                }
            }

            Transform wallLeft = FindChildByName(building, "Wall_Left");
            if (wallLeft != null)
            {
                float outsideX = wallLeft.localPosition.x - halfWall;
                if (Mathf.Abs(outsideX + exteriorX) > 0.02f)
                {
                    failures.Add($"Wall_Left outside face must align with X=-{exteriorX:0.#} (found {outsideX:0.##}).");
                }
            }

            Transform wallRightBack = FindChildByName(building, "Wall_Right_Back");
            if (wallRightBack != null)
            {
                float outsideX = wallRightBack.localPosition.x + halfWall;
                if (Mathf.Abs(outsideX - exteriorX) > 0.02f)
                {
                    failures.Add($"Right wall outside face must align with X=+{exteriorX:0.#} (found {outsideX:0.##}).");
                }
            }

            Transform roofDeck = FindChildByName(building, "RoofDeck");
            if (roofDeck != null)
            {
                float roofOutsideMaxX = roofDeck.localPosition.x + (roofDeck.localScale.x * 0.5f);
                float roofOutsideMinX = roofDeck.localPosition.x - (roofDeck.localScale.x * 0.5f);
                float roofOutsideMaxZ = roofDeck.localPosition.z + (roofDeck.localScale.z * 0.5f);
                float roofOutsideMinZ = roofDeck.localPosition.z - (roofDeck.localScale.z * 0.5f);
                if (Mathf.Abs(roofOutsideMaxX - exteriorX) > 0.02f
                    || Mathf.Abs(roofOutsideMinX + exteriorX) > 0.02f
                    || Mathf.Abs(roofOutsideMaxZ - exteriorZ) > 0.02f
                    || Mathf.Abs(roofOutsideMinZ + exteriorZ) > 0.02f)
                {
                    failures.Add("Roof outside edges must align with the exterior wall footprint (X +/-4, Z +/-5).");
                }
            }
        }



        private static void ValidateBuildingDoor(List<string> failures)

        {

            Transform environment = GameObject.Find(CCS_CharacterControllerMasterTestLayoutConstants.EnvironmentParentName)?.transform;

            Transform building = FindDirectChild(environment, CCS_CharacterControllerMasterTestLayoutConstants.BuildingInstanceName);

            if (building == null)

            {

                return;

            }



            Transform looseEnvironmentDoor = FindDirectChild(environment, CCS_CharacterControllerMasterTestLayoutConstants.DoorInstanceName);

            if (looseEnvironmentDoor != null)

            {

                failures.Add(

                    $"{CCS_CharacterControllerMasterTestLayoutConstants.DoorInstanceName} must not be a loose Environment object.");

            }



            Transform looseRootDoor = FindRootTransform(CCS_CharacterControllerMasterTestLayoutConstants.DoorInstanceName);

            if (looseRootDoor != null)

            {

                failures.Add(

                    $"{CCS_CharacterControllerMasterTestLayoutConstants.DoorInstanceName} must not be a scene root object.");

            }



            Transform door = FindChildByName(building, CCS_CharacterControllerMasterTestLayoutConstants.DoorInstanceName);

            AppendIfMissing(

                failures,

                door != null,

                $"Building prefab instance is missing nested door {CCS_CharacterControllerMasterTestLayoutConstants.DoorInstanceName}.");

            if (door == null)

            {

                return;

            }



            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(door.gameObject) > 0)
            {
                failures.Add($"{CCS_CharacterControllerMasterTestLayoutConstants.DoorInstanceName} has missing script references.");
            }

            CCS_TestDoorMarker doorMarker = door.GetComponent<CCS_TestDoorMarker>();
            AppendIfMissing(
                failures,
                doorMarker != null,
                $"{CCS_CharacterControllerMasterTestLayoutConstants.DoorInstanceName} is missing CCS_TestDoorMarker.");
            if (doorMarker != null && doorMarker.DoorHingePivot == null)
            {
                failures.Add($"{CCS_CharacterControllerMasterTestLayoutConstants.DoorInstanceName} marker DoorHingePivot is not assigned.");
            }

            Transform doorHingePivot = FindChildByName(door, "DoorHingePivot");
            AppendIfMissing(
                failures,
                doorHingePivot != null,
                $"{CCS_CharacterControllerMasterTestLayoutConstants.DoorInstanceName} is missing DoorHingePivot child.");

            Transform legacyDoorPivot = FindChildByName(door, "DoorPivot");
            if (legacyDoorPivot != null)
            {
                failures.Add("DoorPivot is deprecated. Use DoorHingePivot as the rotating hinge transform.");
            }

            if (doorMarker != null && doorHingePivot != null && doorMarker.DoorHingePivot != doorHingePivot)
            {
                failures.Add("CCS_TestDoorMarker must reference the DoorHingePivot transform for hinge rotation.");
            }

            Transform doorSlab = FindChildByName(door, "DoorSlab");
            bool usesInteractionDoor = FindChildByName(door, CCS_InteractionConstants.BuildingDoorInteractableObjectName) != null;
            if (doorMarker != null && doorSlab != null && doorMarker.DoorHingePivot == doorSlab)
            {
                failures.Add("CCS_TestDoorMarker must not reference DoorSlab. It must reference DoorHingePivot.");
            }

            if (!door.IsChildOf(building))

            {

                failures.Add("Door must be nested inside PF_CCS_TestBuilding_RoofPlatform.");

            }



            Vector3 expectedDoorWorldPosition = building.position + building.rotation * CCS_CharacterControllerMasterTestLayoutConstants.DoorLocalPosition;

            ValidatePosition(

                failures,

                CCS_CharacterControllerMasterTestLayoutConstants.DoorInstanceName,

                door.position,

                expectedDoorWorldPosition);

            Quaternion expectedDoorRotation = Quaternion.Euler(
                CCS_CharacterControllerMasterTestLayoutConstants.DoorLocalEuler);
            if (Quaternion.Angle(door.localRotation, expectedDoorRotation) > 5f)
            {
                failures.Add(
                    $"{CCS_CharacterControllerMasterTestLayoutConstants.DoorInstanceName} must sit flush in the right-wall opening without a sideways Y rotation.");
            }

            if (doorHingePivot != null)
            {
                ValidatePosition(
                    failures,
                    "DoorHingePivot (door local)",
                    doorHingePivot.localPosition,
                    CCS_CharacterControllerMasterTestLayoutConstants.DoorHingePivotLocalPosition);

                Vector3 pivotBuildingLocal = building.InverseTransformPoint(doorHingePivot.position);
                ValidatePosition(
                    failures,
                    "DoorHingePivot",
                    pivotBuildingLocal,
                    CCS_CharacterControllerMasterTestLayoutConstants.DoorHingePivotBuildingLocal);

                if (doorHingePivot.localPosition.sqrMagnitude < 0.04f)
                {
                    failures.Add("DoorHingePivot must not sit at the door root origin. It must be on the hinge edge.");
                }

                if (doorHingePivot.GetComponent<MeshRenderer>() != null
                    || doorHingePivot.GetComponent<MeshFilter>() != null)
                {
                    failures.Add("DoorHingePivot must be an empty hinge GameObject without render geometry.");
                }
            }

            if (doorSlab != null && doorHingePivot != null)
            {
                if (doorSlab.parent != doorHingePivot)
                {
                    failures.Add("DoorSlab must be parented under DoorHingePivot so the door swings from the hinge edge.");
                }

                Vector3 slabOffsetFromPivot = doorSlab.localPosition;
                float halfWidth = CCS_CharacterControllerMasterTestLayoutConstants.DoorSlabHalfWidthMeters;
                if (Mathf.Abs(slabOffsetFromPivot.z) < halfWidth * 0.5f)
                {
                    failures.Add("DoorHingePivot must not be near DoorSlab center. It must sit on a vertical hinge edge.");
                }

                if (Mathf.Abs(slabOffsetFromPivot.z - halfWidth) > 0.1f)
                {
                    failures.Add("DoorSlab must be offset from DoorHingePivot by half door width on local Z.");
                }

                ValidatePosition(
                    failures,
                    "DoorSlab (from hinge)",
                    slabOffsetFromPivot,
                    CCS_CharacterControllerMasterTestLayoutConstants.DoorSlabLocalPositionFromPivot);

                ValidateDoorHingeSwing(failures, doorHingePivot, doorSlab, doorMarker);
            }
            else if (usesInteractionDoor && doorHingePivot != null && doorMarker != null)
            {
                ValidateDoorHingeSwingFromInteractionTarget(failures, door, doorHingePivot, doorMarker);
            }

            if (doorSlab != null)
            {
                Vector3 slabScale = doorSlab.lossyScale;
                if (slabScale.x > slabScale.y || slabScale.x > slabScale.z)
                {
                    failures.Add("Door slab thickness must run along local X in the right-wall plane.");
                }

                if (Mathf.Abs(slabScale.z - CCS_CharacterControllerMasterTestLayoutConstants.DoorSlabWidthMeters) > 0.15f
                    || Mathf.Abs(slabScale.y - CCS_CharacterControllerMasterTestLayoutConstants.DoorSlabHeightMeters) > 0.15f)
                {
                    failures.Add("Door slab must span about 1.9m along Z and 2.45m along Y.");
                }

                Vector3 slabCenterBuildingLocal = building.InverseTransformPoint(doorSlab.position);
                if (slabCenterBuildingLocal.z < -1.05f || slabCenterBuildingLocal.z > 1.05f)
                {
                    failures.Add("Door slab must cover the right-wall opening from local Z -1 to +1.");
                }

                if (slabCenterBuildingLocal.y < 1.1f || slabCenterBuildingLocal.y > 1.35f)
                {
                    failures.Add("Door slab must vertically cover the doorway opening up to Y = 2.5.");
                }

                if (Vector3.Distance(
                        slabCenterBuildingLocal,
                        CCS_CharacterControllerMasterTestLayoutConstants.DoorSlabCenterBuildingLocal) > 0.1f)
                {
                    failures.Add("Door slab center must align with the doorway opening in the right wall plane.");
                }

                float slabMinZ = slabCenterBuildingLocal.z - (CCS_CharacterControllerMasterTestLayoutConstants.DoorSlabWidthMeters * 0.5f);
                float slabMaxZ = slabCenterBuildingLocal.z + (CCS_CharacterControllerMasterTestLayoutConstants.DoorSlabWidthMeters * 0.5f);
                float expectedSlabMinZ = CCS_CharacterControllerMasterTestLayoutConstants.DoorHingeLocalZMeters;
                float expectedSlabMaxZ = expectedSlabMinZ + CCS_CharacterControllerMasterTestLayoutConstants.DoorSlabWidthMeters;
                if (Mathf.Abs(slabMinZ - expectedSlabMinZ) > 0.1f || Mathf.Abs(slabMaxZ - expectedSlabMaxZ) > 0.1f)
                {
                    failures.Add("Closed door slab must span the doorway from hinge edge Z = -0.95 to free edge Z = +0.95.");
                }
            }

            ValidateDoorwayClearOfWallColliders(failures, building, doorSlab);

            if (!usesInteractionDoor)
            {
                ValidateRendererMaterial(
                    failures,
                    door,
                    "DoorSlab",
                    CCS_CharacterControllerMasterTestLayoutConstants.DoorWoodMaterialPath,
                    "Door panel");
            }
            else
            {
                Transform interactionTarget = FindChildByName(door, CCS_InteractionConstants.BuildingDoorInteractableObjectName);
                AppendIfMissing(
                    failures,
                    interactionTarget != null && interactionTarget.GetComponent<CCS_InteractableLabelTarget>() != null,
                    $"{CCS_InteractionConstants.BuildingDoorInteractableObjectName} must include {nameof(CCS_InteractableLabelTarget)}.");
            }

        }

        private static void ValidateDoorHingeSwingFromInteractionTarget(
            List<string> failures,
            Transform doorRoot,
            Transform doorHingePivot,
            CCS_TestDoorMarker doorMarker)
        {
            Transform interactionTarget = FindChildByName(doorRoot, CCS_InteractionConstants.BuildingDoorInteractableObjectName);
            if (interactionTarget == null)
            {
                return;
            }

            ValidateDoorHingeSwing(failures, doorHingePivot, interactionTarget, doorMarker);
        }



        private static void ValidateDoorHingeSwing(
            List<string> failures,
            Transform doorHingePivot,
            Transform doorSlab,
            CCS_TestDoorMarker doorMarker)
        {
            float halfWidth = CCS_CharacterControllerMasterTestLayoutConstants.DoorSlabHalfWidthMeters;
            Vector3 freeEdgeOffsetFromHinge = doorSlab.localPosition + new Vector3(0f, 0f, halfWidth);
            Vector3 hingeWorld = doorHingePivot.position;
            Vector3 closedFreeEdgeWorld = doorHingePivot.TransformPoint(freeEdgeOffsetFromHinge);

            float testAngle = doorMarker != null ? doorMarker.OpenAngle : 90f;
            if (doorMarker != null && !doorMarker.OpensInward)
            {
                testAngle = -testAngle;
            }

            Quaternion openRotation = Quaternion.Euler(0f, testAngle, 0f);
            Vector3 openFreeEdgeWorld = doorHingePivot.parent.TransformPoint(
                doorHingePivot.localPosition + (openRotation * freeEdgeOffsetFromHinge));

            if ((openFreeEdgeWorld - closedFreeEdgeWorld).sqrMagnitude < 0.05f)
            {
                failures.Add("Rotating DoorHingePivot must swing the free door edge instead of spinning around slab center.");
            }

            if ((openFreeEdgeWorld - hingeWorld).sqrMagnitude < 0.02f)
            {
                failures.Add("DoorHingePivot rotation must move the free edge away from the hinge while the hinge edge stays fixed.");
            }
        }



        private static void ValidateDoorwayClearOfWallColliders(
            List<string> failures,
            Transform building,
            Transform doorSlab)
        {
            Vector3 clearVolumeCenterBuilding = new Vector3(
                CCS_CharacterControllerMasterTestLayoutConstants.WallRightCenterLocalX,
                CCS_CharacterControllerMasterTestLayoutConstants.DoorSlabHeightMeters * 0.5f,
                0f);
            Vector3 clearVolumeSize = new Vector3(
                CCS_CharacterControllerMasterTestLayoutConstants.WallThicknessMeters * 0.75f,
                CCS_CharacterControllerMasterTestLayoutConstants.DoorSlabHeightMeters,
                CCS_CharacterControllerMasterTestLayoutConstants.DoorSlabWidthMeters);

            Bounds clearVolume = new Bounds(
                building.TransformPoint(clearVolumeCenterBuilding),
                Vector3.Scale(clearVolumeSize, building.lossyScale));

            string[] wallNames =
            {
                "Wall_Right_Back",
                "Wall_Right_Front",
            };

            for (int i = 0; i < wallNames.Length; i++)
            {
                Transform wall = FindChildByName(building, wallNames[i]);
                if (wall == null)
                {
                    continue;
                }

                Collider wallCollider = wall.GetComponent<Collider>();
                if (wallCollider != null && wallCollider.bounds.Intersects(clearVolume))
                {
                    failures.Add($"Door opening clear volume is blocked by {wallNames[i]} collider.");
                }
            }

            if (doorSlab != null)
            {
                Collider doorCollider = doorSlab.GetComponent<Collider>();
                if (doorCollider == null)
                {
                    failures.Add("DoorSlab must include a collider for doorway coverage validation.");
                }
            }
        }



        private static void ValidateRampOrientation(List<string> failures, Transform environment)

        {

            Transform ramp = FindDirectChild(environment, CCS_CharacterControllerMasterTestLayoutConstants.RampInstanceName);

            if (ramp == null)

            {

                return;

            }



            Transform rampSurface = FindChildByName(ramp, "RampSurface");

            if (rampSurface == null)

            {

                failures.Add("Ramp prefab is missing RampSurface.");

                return;

            }

            if (rampSurface.localScale.y <= 0f)
            {
                failures.Add("RampSurface must use positive Y scale. Negative scale is not allowed.");
            }

            if (!TryGetRampWalkSurfaceEndpoints(rampSurface, out Vector3 rampHighPoint, out Vector3 rampLowPoint))
            {
                failures.Add("Ramp walk surface endpoints could not be sampled from RampSurface.");
                return;
            }

            if (rampHighPoint.y <= rampLowPoint.y + 1f)
            {
                failures.Add("Ramp orientation is reversed. High end must be at the roof edge above the low end.");
            }

            if (rampLowPoint.z <= rampHighPoint.z + 0.1f)
            {
                failures.Add("Ramp must extend outward on +Z from the building front edge.");
            }

            if (rampSurface.up.y <= 0f)
            {
                failures.Add("Ramp walk surface normal must point upward.");
            }

            float slopeAngle = Vector3.Angle(rampSurface.up, Vector3.up);

            if (Mathf.Abs(slopeAngle - CCS_CharacterControllerMasterTestLayoutConstants.RampSlopeDegrees) > 5f)

            {

                failures.Add(

                    $"Ramp slope is {slopeAngle:0.#} degrees but expected about "

                    + $"{CCS_CharacterControllerMasterTestLayoutConstants.RampSlopeDegrees:0.#} degrees.");

            }

        }



        private static void ValidateRampPlacement(List<string> failures, Transform environment)

        {

            Transform ramp = FindDirectChild(environment, CCS_CharacterControllerMasterTestLayoutConstants.RampInstanceName);

            if (ramp == null)

            {

                return;

            }



            ValidatePosition(
                failures,
                CCS_CharacterControllerMasterTestLayoutConstants.RampInstanceName,
                ramp.position,
                CCS_CharacterControllerMasterTestLayoutConstants.RampPosition);

            Quaternion expectedRampRotation = Quaternion.Euler(
                CCS_CharacterControllerMasterTestLayoutConstants.RampRotationEuler);
            if (Quaternion.Angle(ramp.rotation, expectedRampRotation) > 1f)
            {
                failures.Add(
                    $"{CCS_CharacterControllerMasterTestLayoutConstants.RampInstanceName} rotation must be "
                    + FormatVector(CCS_CharacterControllerMasterTestLayoutConstants.RampRotationEuler)
                    + " degrees.");
            }



            float buildingFrontEdgeWorldZ = CCS_CharacterControllerMasterTestLayoutConstants.BuildingPosition.z

                + CCS_CharacterControllerMasterTestLayoutConstants.BuildingFrontEdgeLocalZ;

            float buildingMinX = CCS_CharacterControllerMasterTestLayoutConstants.BuildingPosition.x

                - CCS_CharacterControllerMasterTestLayoutConstants.BuildingHalfWidthMeters;

            float buildingMaxX = CCS_CharacterControllerMasterTestLayoutConstants.BuildingPosition.x

                + CCS_CharacterControllerMasterTestLayoutConstants.BuildingHalfWidthMeters;



            Transform rampSurface = FindChildByName(ramp, "RampSurface");
            if (rampSurface == null)
            {
                failures.Add("Ramp prefab is missing RampSurface.");
                return;
            }

            if (!TryGetRampWalkSurfaceEndpoints(rampSurface, out Vector3 rampHighPoint, out Vector3 rampLowPoint))
            {
                failures.Add("Ramp walk surface endpoints could not be sampled from RampSurface.");
                return;
            }

            ValidateRampEndpointPosition(
                failures,
                "Ramp high walk edge",
                rampHighPoint,
                CCS_CharacterControllerMasterTestLayoutConstants.RampHighEndpointWorld);

            ValidateRampEndpointPosition(
                failures,
                "Ramp low walk edge",
                rampLowPoint,
                CCS_CharacterControllerMasterTestLayoutConstants.RampLowEndpointWorld);

            if (rampLowPoint.z <= rampHighPoint.z + 0.1f)
            {
                failures.Add("Ramp low walk edge must be farther +Z than the high walk edge.");
            }

            if (rampHighPoint.y <= rampLowPoint.y + 1f)
            {
                failures.Add("Ramp must climb from low ground up to the roof edge, not slope down toward the roof.");
            }



            if (rampHighPoint.z < buildingFrontEdgeWorldZ - CCS_CharacterControllerMasterTestLayoutConstants.PositionTolerance)

            {

                failures.Add("Ramp high end is inside the building footprint. It must connect to the front roof edge.");

            }



            if (rampHighPoint.z > buildingFrontEdgeWorldZ + CCS_CharacterControllerMasterTestLayoutConstants.PositionTolerance)

            {

                failures.Add("Ramp high end must connect to the building front roof edge at local Z = +5.");

            }



            if (rampLowPoint.z < buildingFrontEdgeWorldZ + 5f - CCS_CharacterControllerMasterTestLayoutConstants.PositionTolerance)

            {

                failures.Add("Ramp must extend outward from the building front edge over a 6m run.");

            }



            if (rampLowPoint.y > 0.5f)

            {

                failures.Add("Ramp low end must sit on ground level outside the building.");

            }



            if (rampHighPoint.x < buildingMinX || rampHighPoint.x > buildingMaxX)

            {

                failures.Add("Ramp high end must align with the building centerline on the front edge.");

            }



            float buildingMinInteriorZ = CCS_CharacterControllerMasterTestLayoutConstants.BuildingPosition.z

                + CCS_CharacterControllerMasterTestLayoutConstants.BuildingRearEdgeLocalZ;

            float buildingMaxInteriorZ = CCS_CharacterControllerMasterTestLayoutConstants.BuildingPosition.z

                + CCS_CharacterControllerMasterTestLayoutConstants.BuildingFrontEdgeLocalZ;

            if (rampHighPoint.z > buildingMinInteriorZ + CCS_CharacterControllerMasterTestLayoutConstants.PositionTolerance

                && rampHighPoint.z < buildingMaxInteriorZ - CCS_CharacterControllerMasterTestLayoutConstants.PositionTolerance)

            {

                failures.Add("Ramp top is inside the building footprint. It must connect to the front exterior roof edge.");

            }

        }



        private static void ValidateStairsAttachment(List<string> failures, Transform environment)

        {

            Transform stairs = FindDirectChild(environment, CCS_CharacterControllerMasterTestLayoutConstants.StairsInstanceName);

            if (stairs == null)

            {

                return;

            }



            Transform topStep = FindChildByName(stairs, "Step_12");

            if (topStep == null)

            {

                failures.Add("Stairs prefab is missing Step_12.");

                return;

            }



            float topY = topStep.position.y + 0.125f;

            if (Mathf.Abs(topY - CCS_CharacterControllerMasterTestLayoutConstants.RoofHeightMeters) > 0.15f)

            {

                failures.Add(

                    $"Stairs top is at Y={topY:0.##} but roof walk surface must be Y="

                    + $"{CCS_CharacterControllerMasterTestLayoutConstants.RoofHeightMeters:0.##}.");

            }



            float buildingRearEdgeWorldZ = CCS_CharacterControllerMasterTestLayoutConstants.BuildingPosition.z

                + CCS_CharacterControllerMasterTestLayoutConstants.BuildingRearEdgeLocalZ;

            if (Mathf.Abs(topStep.position.z - buildingRearEdgeWorldZ) > 0.35f)

            {

                failures.Add("Stairs top must meet the building rear roof edge at local Z = -5.");

            }



            Transform bottomStep = FindChildByName(stairs, "Step_01");

            if (bottomStep != null)

            {

                float expectedOuterZ = CCS_CharacterControllerMasterTestLayoutConstants.BuildingPosition.z

                    + CCS_CharacterControllerMasterTestLayoutConstants.StairsOuterEdgeLocalZ;

                if (Mathf.Abs(bottomStep.position.z - expectedOuterZ) > 0.35f)

                {

                    failures.Add("Stairs must extend outward from the building rear edge toward local Z = -9.2.");

                }

            }

        }



        private static void ValidateSpawnPoints(List<string> failures)

        {

            Transform testPoints = GameObject.Find(CCS_CharacterControllerMasterTestLayoutConstants.TestPointsParentName)?.transform;

            if (testPoints == null)

            {

                return;

            }



            for (int i = 0; i < CCS_CharacterControllerMasterTestLayoutConstants.SpawnPointNames.Length; i++)

            {

                string pointName = CCS_CharacterControllerMasterTestLayoutConstants.SpawnPointNames[i];

                Vector3 expectedPosition = CCS_CharacterControllerMasterTestLayoutConstants.SpawnPointPositions[i];

                ValidateTestPoint(failures, testPoints, pointName, expectedPosition);

            }

        }



        private static void ValidateTraversalPoints(List<string> failures)

        {

            Transform testPoints = GameObject.Find(CCS_CharacterControllerMasterTestLayoutConstants.TestPointsParentName)?.transform;

            if (testPoints == null)

            {

                return;

            }



            for (int i = 0; i < CCS_CharacterControllerMasterTestLayoutConstants.TraversalPointNames.Length; i++)

            {

                string pointName = CCS_CharacterControllerMasterTestLayoutConstants.TraversalPointNames[i];

                Vector3 expectedPosition = CCS_CharacterControllerMasterTestLayoutConstants.TraversalPointPositions[i];

                ValidateTestPoint(failures, testPoints, pointName, expectedPosition);

            }

        }



        private static void ValidateBootstrap(List<string> failures)

        {

            Transform bootstrap = FindRootTransform(CCS_CharacterControllerMasterTestLayoutConstants.BootstrapInstanceName);

            AppendIfMissing(

                failures,

                bootstrap != null,

                $"Scene is missing {CCS_CharacterControllerMasterTestLayoutConstants.BootstrapInstanceName}.");

        }



        private static void ValidateMasterTestSpawnSetup(List<string> failures)
        {
            if (FindRootTransform(CCS_CharacterControllerMasterTestLayoutConstants.PlayerInstanceName) != null)
            {
                failures.Add(
                    $"{CCS_CharacterControllerMasterTestLayoutConstants.PlayerInstanceName} must not be placed in the master test scene.");
            }

            if (FindRootTransform(CCS_CharacterControllerMasterTestLayoutConstants.NetworkedPlayerInstanceName) != null)
            {
                failures.Add(
                    $"{CCS_CharacterControllerMasterTestLayoutConstants.NetworkedPlayerInstanceName} must not be placed in the master test scene.");
            }

            ValidateNpcRemoved(failures);

            Transform spawnControllerTransform = FindRootTransform(
                CCS_CharacterControllerMasterTestLayoutConstants.MasterTestSpawnControllerObjectName);
            AppendIfMissing(
                failures,
                spawnControllerTransform != null,
                $"Scene is missing {CCS_CharacterControllerMasterTestLayoutConstants.MasterTestSpawnControllerObjectName}.");

            if (spawnControllerTransform == null)
            {
                return;
            }

            CCS_MasterTestSpawnController spawnController =
                spawnControllerTransform.GetComponent<CCS_MasterTestSpawnController>();
            AppendIfMissing(
                failures,
                spawnController != null,
                $"{CCS_CharacterControllerMasterTestLayoutConstants.MasterTestSpawnControllerObjectName} is missing CCS_MasterTestSpawnController.");

            if (spawnController == null)
            {
                return;
            }

            SerializedObject serializedSpawn = new SerializedObject(spawnController);
            SerializedProperty prefabProperty = serializedSpawn.FindProperty("testPlayerPrefab");
            if (prefabProperty == null)
            {
                prefabProperty = serializedSpawn.FindProperty("soloPlayerPrefab");
            }

            GameObject assignedPrefab = prefabProperty != null
                ? prefabProperty.objectReferenceValue as GameObject
                : null;
            Transform assignedSpawn = serializedSpawn.FindProperty("soloSpawnPoint").objectReferenceValue as Transform;
            CCS_CharacterCameraController assignedCamera =
                serializedSpawn.FindProperty("cameraController").objectReferenceValue as CCS_CharacterCameraController;

            AppendIfMissing(
                failures,
                assignedPrefab != null,
                "CCS_MasterTestSpawnController must reference PF_CCS_CharacterController_TestPlayer_Networked.");

            if (assignedPrefab != null)
            {
                string prefabPath = AssetDatabase.GetAssetPath(assignedPrefab);
                AppendIfMissing(
                    failures,
                    prefabPath == CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath,
                    "CCS_MasterTestSpawnController testPlayerPrefab must use the shared network-capable test player prefab.");
            }

            AppendIfMissing(
                failures,
                assignedSpawn != null && assignedSpawn.name == CCS_CharacterControllerMasterTestLayoutConstants.SpawnPointNames[0],
                "CCS_MasterTestSpawnController must reference TP_Spawn_Host.");

            AppendIfMissing(
                failures,
                assignedCamera != null,
                "CCS_MasterTestSpawnController must reference PF_CCS_CharacterCameraRig camera controller.");

            SerializedProperty assignedBodyMaterial = serializedSpawn.FindProperty("defaultBodyMaterial");
            AppendIfMissing(
                failures,
                assignedBodyMaterial != null && assignedBodyMaterial.objectReferenceValue != null,
                "CCS_MasterTestSpawnController must reference the default yellow body material.");

            ValidateSceneCameraRigRuntimeBinding(failures, assignedCamera);
            ValidatePlayerCameraPivotPrefab(
                failures,
                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);
            ValidateSoloSpawnUsesSharedPrefabContracts(failures);
        }

        private static void ValidateTestingManagerAndRecordingAmbience(List<string> failures)
        {
            Transform testingManagerTransform = FindRootTransform(
                CCS_CharacterControllerMasterTestLayoutConstants.MasterTestTestingManagerObjectName);
            AppendIfMissing(
                failures,
                testingManagerTransform != null,
                "Scene is missing "
                + CCS_CharacterControllerMasterTestLayoutConstants.MasterTestTestingManagerObjectName
                + ".");

            Transform ambientAudioTransform = FindRootTransform(
                CCS_CharacterControllerMasterTestLayoutConstants.MasterTestAmbientAudioObjectName);
            AppendIfMissing(
                failures,
                ambientAudioTransform == null,
                "Master Test gameplay scene must not contain "
                + CCS_CharacterControllerMasterTestLayoutConstants.MasterTestAmbientAudioObjectName
                + " (ambient music belongs on hosting scene only).");

            if (testingManagerTransform == null)
            {
                return;
            }

            CCS_CharacterControllerTestingManager testingManager =
                testingManagerTransform.GetComponent<CCS_CharacterControllerTestingManager>();
            AppendIfMissing(
                failures,
                testingManager != null,
                CCS_CharacterControllerMasterTestLayoutConstants.MasterTestTestingManagerObjectName
                + " must contain CCS_CharacterControllerTestingManager.");

            if (testingManager == null)
            {
                return;
            }

            SerializedObject serializedManager = new SerializedObject(testingManager);
            SerializedProperty ambienceEnabledProperty = serializedManager.FindProperty("enableRecordingAmbience");
            SerializedProperty playlistReferenceProperty = serializedManager.FindProperty("ambientAudioPlaylist");
            AppendIfMissing(
                failures,
                ambienceEnabledProperty != null && !ambienceEnabledProperty.boolValue,
                "CCS_MasterTestSceneTestingManager.enableRecordingAmbience must default to false in gameplay Master Test.");
            AppendIfMissing(
                failures,
                playlistReferenceProperty == null || playlistReferenceProperty.objectReferenceValue == null,
                "CCS_MasterTestSceneTestingManager must not reference a Master Test ambient playlist.");

            string testingManagerSourcePath =
                "Assets/CCS/Modules/CharacterController/Tests/Runtime/Managers/CCS_CharacterControllerTestingManager.cs";
            if (File.Exists(testingManagerSourcePath))
            {
                string testingManagerSource = File.ReadAllText(testingManagerSourcePath);
                AppendIfMissing(
                    failures,
                    testingManagerSource.Contains("SetRecordingAmbienceEnabled")
                        && testingManagerSource.Contains("ApplyTestingSettings")
                        && testingManagerSource.Contains("WriteOneShotReport"),
                    "CCS_CharacterControllerTestingManager must expose ambience toggle methods and WriteOneShotReport.");
            }

            if (testingManager != null)
            {
                SerializedProperty armIkProperty = serializedManager.FindProperty("enableArmToReticleIK");
                SerializedProperty convergenceProperty = serializedManager.FindProperty("enableVisualAimConvergence");
                SerializedProperty reticleModeProperty = serializedManager.FindProperty("reticleMode");
                SerializedProperty reticleClampProperty = serializedManager.FindProperty("enableReticleClamp");
                SerializedProperty maxDriftProperty = serializedManager.FindProperty("maxReticleDriftPixels");
                SerializedProperty aimDebugRaysProperty = serializedManager.FindProperty("enableAimDebugRays");

                AppendIfMissing(
                    failures,
                    armIkProperty == null || !armIkProperty.boolValue,
                    "CCS_MasterTestSceneTestingManager.enableArmToReticleIK must default to false.");
                AppendIfMissing(
                    failures,
                    convergenceProperty == null || !convergenceProperty.boolValue,
                    "CCS_MasterTestSceneTestingManager.enableVisualAimConvergence must default to false.");
                AppendIfMissing(
                    failures,
                    reticleModeProperty == null
                        || reticleModeProperty.enumValueIndex == (int)CCS_AimReticleMode.HybridCameraCenterWithMuzzleDrift,
                    "CCS_MasterTestSceneTestingManager.reticleMode must default to HybridCameraCenterWithMuzzleDrift.");
                AppendIfMissing(
                    failures,
                    reticleClampProperty == null || reticleClampProperty.boolValue,
                    "CCS_MasterTestSceneTestingManager.enableReticleClamp must default to true.");
                AppendIfMissing(
                    failures,
                    maxDriftProperty == null
                        || Mathf.Approximately(
                            maxDriftProperty.floatValue,
                            CCS_WeaponsConstants.MasterTestMaxReticleDriftPixelsDefault),
                    "CCS_MasterTestSceneTestingManager.maxReticleDriftPixels must default to "
                    + CCS_WeaponsConstants.MasterTestMaxReticleDriftPixelsDefault.ToString("0.##")
                    + ".");
                AppendIfMissing(
                    failures,
                    aimDebugRaysProperty == null || !aimDebugRaysProperty.boolValue,
                    "CCS_MasterTestSceneTestingManager.enableAimDebugRays must default to false.");
            }

            if (File.Exists(testingManagerSourcePath))
            {
                string testingManagerSource = File.ReadAllText(testingManagerSourcePath);
                AppendIfMissing(
                    failures,
                    testingManagerSource.Contains("enableArmToReticleIK")
                        && testingManagerSource.Contains("reticleMode"),
                    "CCS_MasterTestSceneTestingManager must expose Master Test aim visual toggles.");
                AppendIfMissing(
                    failures,
                    !testingManagerSource.Contains("ConfigureThirdPersonAimPitchBlend"),
                    "CCS_MasterTestSceneTestingManager must not configure removed RevolverAimPitch flow.");
            }
        }



        private static void ValidatePlayerCameraPivotPrefab(List<string> failures, string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                return;
            }

            CCS_CharacterCameraController cameraController = prefab.GetComponent<CCS_CharacterCameraController>();
            AppendIfMissing(
                failures,
                cameraController != null,
                $"{prefabPath} must contain CCS_CharacterCameraController for camera pivot references.");

            if (cameraController == null)
            {
                return;
            }

            SerializedObject serializedCamera = new SerializedObject(cameraController);
            SerializedProperty cinemachineProperty = serializedCamera.FindProperty("cinemachineCamera");
            AppendIfMissing(
                failures,
                cinemachineProperty == null || cinemachineProperty.objectReferenceValue == null,
                $"{prefabPath} player camera controller must not embed Cinemachine; scene rig owns camera components.");

            AppendIfMissing(
                failures,
                cameraController.CameraPivot != null && cameraController.CameraLookTarget != null,
                $"{prefabPath} camera controller must reference CameraPivot and CameraLookTarget for runtime binding.");
        }



        private static void ValidateSceneCameraRigRuntimeBinding(
            List<string> failures,
            CCS_CharacterCameraController sceneCameraController)
        {
            if (sceneCameraController == null)
            {
                return;
            }

            SerializedObject serializedCamera = new SerializedObject(sceneCameraController);
            SerializedProperty cinemachineProperty = serializedCamera.FindProperty("cinemachineCamera");
            SerializedProperty pivotProperty = serializedCamera.FindProperty("cameraPivot");
            SerializedProperty lookTargetProperty = serializedCamera.FindProperty("cameraLookTarget");

            AppendIfMissing(
                failures,
                cinemachineProperty != null && cinemachineProperty.objectReferenceValue != null,
                "Scene camera rig must reference CinemachineCamera_TP so solo spawn can bind follow targets.");

            Transform tpCamera = sceneCameraController.transform.Find("CinemachineCamera_TP");
            ValidateCinemachineLookInput(failures, tpCamera, requireObstacleAvoidanceEnabled: true);

            if (pivotProperty != null && pivotProperty.objectReferenceValue != null)
            {
                Object pivotReference = pivotProperty.objectReferenceValue;
                if (EditorUtility.IsPersistent(pivotReference))
                {
                    failures.Add(
                        "Scene camera rig follow pivot must not reference prefab assets; solo spawn binds spawned player pivots at runtime.");
                }
                else
                {
                    failures.Add(
                        "Scene camera rig follow pivot must be unassigned in edit mode; solo spawn binds spawned player pivots at runtime.");
                }
            }

            if (lookTargetProperty != null && lookTargetProperty.objectReferenceValue != null)
            {
                Object lookReference = lookTargetProperty.objectReferenceValue;
                if (EditorUtility.IsPersistent(lookReference))
                {
                    failures.Add(
                        "Scene camera rig look target must not reference prefab assets; solo spawn binds spawned player pivots at runtime.");
                }
                else
                {
                    failures.Add(
                        "Scene camera rig look target must be unassigned in edit mode; solo spawn binds spawned player pivots at runtime.");
                }
            }
        }



        private static void ValidateNpcRemoved(List<string> failures)
        {
            if (FindRootTransform(CCS_CharacterControllerMasterTestLayoutConstants.NpcInstanceName) != null)
            {
                failures.Add(
                    $"{CCS_CharacterControllerMasterTestLayoutConstants.NpcInstanceName} must not be present in the master test scene.");
            }
        }



        private static void ValidateDeprecatedPlayerPrefabsRemoved(List<string> failures)
        {
            AppendIfMissing(
                failures,
                !File.Exists(CCS_TestPlayerPrefabConstants.DeprecatedOfflinePlayerPrefabPath),
                $"Deprecated solo test player prefab must be removed: {CCS_TestPlayerPrefabConstants.DeprecatedOfflinePlayerPrefabPath}");

            AppendIfMissing(
                failures,
                !File.Exists(CCS_TestPlayerPrefabConstants.DeprecatedNetworkedPlayerDuplicatePrefabPath),
                $"Duplicate networked test player prefab must be removed: {CCS_TestPlayerPrefabConstants.DeprecatedNetworkedPlayerDuplicatePrefabPath}");
        }

        private static void ValidatePlayerPrefabAssets(List<string> failures)
        {
            ValidateDeprecatedPlayerPrefabsRemoved(failures);

            ValidatePlayerNameplatePrefab(
                failures,
                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath,
                requireNetworkNameplate: true);

            ValidatePlayerGlassesPrefab(
                failures,
                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);

            ValidatePlayerBodyMaterialPrefab(
                failures,
                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);

            ValidatePlayerCapsuleVisualLayout(failures, CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);

            ValidatePlayerCameraTargetsOnPrefab(
                failures,
                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);

            ValidatePlayerCameraCollisionExclusion(
                failures,
                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);

            ValidateCameraFollowAnchorRotationChain(
                failures,
                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);

            ValidateNetworkPlayerUsesSharedYellowVisuals(failures);
            ValidateNetworkPlayerMovementAuthority(failures);
            ValidateOfflineBootstrapOnNetworkedPrefab(failures);
            ValidateTestPlayerDisplayProfileAsset(failures);
            ValidateNetworkedPrefabDisplayProfileAssignment(failures);
            ValidatePlayerJumpConfiguration(failures);
            ValidateAimLocomotionSetup(failures);
            ValidateNameplateOwnershipVisibility(failures);
            ValidateAttributeBarsHudOnPlayerPrefab(failures);
            ValidateNoCharacterDebugHudOnPlayerPrefab(failures);
            ValidatePlayerWeaponAimAlignment(failures);
        }

        private static void ValidatePlayerWeaponAimAlignment(List<string> failures)
        {
            GameObject networkedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (networkedPrefab == null)
            {
                return;
            }

            AppendValidationResult(
                failures,
                CCS_WeaponsValidationUtility.ValidatePlayerRevolverComponents(networkedPrefab));
            AppendValidationResult(
                failures,
                CCS_WeaponsValidationUtility.ValidateRevolverFireFeedbackSourceContract());
            AppendValidationResult(
                failures,
                CCS_WeaponsValidationUtility.ValidateRevolverFireVisualsFoundation());
            AppendValidationResult(
                failures,
                CCS_WeaponsValidationUtility.ValidateHitscanUsesCameraCenterAim());
            AppendValidationResult(
                failures,
                CCS_WeaponsValidationUtility.ValidateWeaponAimConvergenceFoundation());
            AppendValidationResult(
                failures,
                CCS_WeaponsValidationUtility.ValidateRevolverArmReticleIKFoundation(networkedPrefab));
            AppendValidationResult(
                failures,
                CCS_CharacterControllerValidationUtility.ValidateFirstPersonBodyAwareCameraFoundation());

            Transform followAnchor = FindChildByName(networkedPrefab.transform, CCS_CharacterControllerConstants.CameraFollowAnchorObjectName);
            Transform pitchTarget = followAnchor != null
                ? followAnchor.Find(CCS_CharacterControllerConstants.CameraPitchTargetObjectName)
                : null;
            if (pitchTarget != null
                && pitchTarget.localPosition.y < CCS_CharacterControllerConstants.CameraPitchTargetMinimumLocalHeight)
            {
                failures.Add("CameraPitchTarget local height must be between 1.40m and 1.60m from player root for third-person framing.");
            }
        }

        private static void ValidateMasterTestDeathUiInputContracts(List<string> failures)
        {
            EventSystem[] eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            for (int i = 0; i < eventSystems.Length; i++)
            {
                EventSystem eventSystem = eventSystems[i];
                if (eventSystem == null)
                {
                    continue;
                }

                if (eventSystem.GetComponent<StandaloneInputModule>() != null)
                {
                    failures.Add("Master Test scene EventSystem must not use StandaloneInputModule.");
                }

                if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
                {
                    failures.Add("Master Test scene EventSystem must use InputSystemUIInputModule when present.");
                }
            }
        }

        private static void ValidateAttributeBarsHudOnPlayerPrefab(List<string> failures)
        {
            GameObject networkedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (networkedPrefab == null)
            {
                return;
            }

            CCS_SurvivalValidationResult attributeValidation =
                CCS.Modules.Attributes.CCS_AttributesValidationUtility.ValidateTestPlayerComponents(networkedPrefab);
            if (!attributeValidation.IsSuccess)
            {
                failures.Add(attributeValidation.Message);
            }
        }

        private static void ValidateNoCharacterDebugHudOnPlayerPrefab(List<string> failures)
        {
            GameObject networkedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (networkedPrefab == null)
            {
                return;
            }

            AppendIfMissing(
                failures,
                !HasBehaviourNamed(networkedPrefab, "CCS_CharacterControllerDebugHud"),
                "Networked test player prefab must not contain CCS_CharacterControllerDebugHud.");
            AppendIfMissing(
                failures,
                !HasBehaviourNamed(networkedPrefab, "CCS_PlayerAttributeHud"),
                "Networked test player prefab must not contain legacy CCS_PlayerAttributeHud.");
        }

        private static bool HasBehaviourNamed(GameObject root, string typeName)
        {
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null && behaviour.GetType().Name == typeName)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidateOfflineBootstrapOnNetworkedPrefab(List<string> failures)
        {
            GameObject networkedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);
            AppendIfMissing(
                failures,
                networkedPrefab != null
                && networkedPrefab.GetComponent<CCS_TestPlayerOfflineBootstrap>() != null,
                $"{CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath} must contain CCS_TestPlayerOfflineBootstrap for solo compatibility.");
        }

        private static void ValidateTestPlayerDisplayProfileAsset(List<string> failures)
        {
            AppendIfMissing(
                failures,
                File.Exists(CCS_TestPlayerPrefabConstants.DefaultDisplayProfilePath),
                $"Missing test player display profile: {CCS_TestPlayerPrefabConstants.DefaultDisplayProfilePath}.");

            CCS_TestPlayerDisplayProfile displayProfile = AssetDatabase.LoadAssetAtPath<CCS_TestPlayerDisplayProfile>(
                CCS_TestPlayerPrefabConstants.DefaultDisplayProfilePath);
            if (displayProfile == null)
            {
                return;
            }

            AppendIfMissing(
                failures,
                displayProfile.GlassesLocalScale == new Vector3(0.3f, 0.3f, 0.3f),
                $"{CCS_TestPlayerPrefabConstants.DefaultDisplayProfilePath} VisualGlasses scale must be (0.3, 0.3, 0.3).");
            AppendIfMissing(
                failures,
                displayProfile.GlassesLocalEuler == new Vector3(180f, 180f, 90f),
                $"{CCS_TestPlayerPrefabConstants.DefaultDisplayProfilePath} VisualGlasses rotation must be (180, 180, 90).");
        }

        private static void ValidateNetworkedPrefabDisplayProfileAssignment(List<string> failures)
        {
            GameObject networkedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (networkedPrefab == null)
            {
                return;
            }

            CCS_TestPlayerOfflineBootstrap bootstrap = networkedPrefab.GetComponent<CCS_TestPlayerOfflineBootstrap>();
            if (bootstrap == null)
            {
                return;
            }

            CCS_TestPlayerDisplayProfile expectedProfile = AssetDatabase.LoadAssetAtPath<CCS_TestPlayerDisplayProfile>(
                CCS_TestPlayerPrefabConstants.DefaultDisplayProfilePath);
            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            SerializedProperty displayProfileProperty = serializedBootstrap.FindProperty("displayProfile");
            AppendIfMissing(
                failures,
                displayProfileProperty != null
                && displayProfileProperty.objectReferenceValue == expectedProfile,
                $"{CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath} must assign CCS_TestPlayerDisplayProfile_Default on CCS_TestPlayerOfflineBootstrap.");

            Transform followAnchor = FindChildByName(networkedPrefab.transform, "CameraFollowAnchor");
            AppendIfMissing(
                failures,
                followAnchor != null && followAnchor.GetComponent<CCS_CharacterCameraFollowAnchor>() != null,
                $"{CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath} must contain CameraFollowAnchor with CCS_CharacterCameraFollowAnchor.");
        }

        private static void ValidatePlayerCapsuleVisualLayout(List<string> failures, string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                return;
            }

            Transform capsuleVisual = FindChildByName(
                prefab.transform,
                CCS_CharacterControllerMasterTestLayoutConstants.CapsuleVisualName);
            if (capsuleVisual == null)
            {
                return;
            }

            if (Vector3.Distance(
                    capsuleVisual.localPosition,
                    CCS_CharacterControllerMasterTestLayoutConstants.CapsuleVisualLocalPosition) > 0.01f)
            {
                failures.Add($"{prefabPath} CapsuleVisual local position must be (0, 1, 0).");
            }

            if (Vector3.Distance(
                    capsuleVisual.localScale,
                    CCS_CharacterControllerMasterTestLayoutConstants.CapsuleVisualLocalScale) > 0.01f)
            {
                failures.Add($"{prefabPath} CapsuleVisual local scale must be (0.7, 1, 0.7).");
            }
        }

        private static void ValidateSoloSpawnUsesSharedPrefabContracts(List<string> failures)
        {
            const string spawnControllerPath =
                "Assets/CCS/Modules/CharacterController/Tests/Runtime/CCS_MasterTestSpawnController.cs";
            const string sessionEventsPath =
                "Assets/CCS/Modules/CharacterController/Tests/Runtime/CCS_TestPlayerSessionEvents.cs";
            const string localConfiguratorPath =
                "Assets/CCS/Modules/CharacterController/Tests/Runtime/CCS_TestPlayerLocalSessionConfigurator.cs";

            if (File.Exists(spawnControllerPath))
            {
                string spawnSource = File.ReadAllText(spawnControllerPath);
                AppendIfMissing(
                    failures,
                    spawnSource.Contains("CCS_TestPlayerLocalSessionConfigurator.TryConfigureOfflinePlayer"),
                    "CCS_MasterTestSpawnController must configure offline players through CCS_TestPlayerLocalSessionConfigurator.");
                AppendIfMissing(
                    failures,
                    spawnSource.Contains("CCS_MasterTestNetworkSessionUtility.IsNetworkSessionActive"),
                    "CCS_MasterTestSpawnController must skip solo spawn during active Netcode sessions.");
            }

            if (File.Exists(sessionEventsPath))
            {
                string eventsSource = File.ReadAllText(sessionEventsPath);
                AppendIfMissing(
                    failures,
                    eventsSource.Contains("PlayerSpawned"),
                    "CCS_TestPlayerSessionEvents must expose PlayerSpawned.");
                AppendIfMissing(
                    failures,
                    eventsSource.Contains("LocalPlayerReady"),
                    "CCS_TestPlayerSessionEvents must expose LocalPlayerReady.");
                AppendIfMissing(
                    failures,
                    eventsSource.Contains("PlayerNameChanged"),
                    "CCS_TestPlayerSessionEvents must expose PlayerNameChanged.");
                AppendIfMissing(
                    failures,
                    eventsSource.Contains("JoinNotificationQueued"),
                    "CCS_TestPlayerSessionEvents must expose JoinNotificationQueued.");
            }

            if (File.Exists(localConfiguratorPath))
            {
                string configuratorSource = File.ReadAllText(localConfiguratorPath);
                AppendIfMissing(
                    failures,
                    configuratorSource.Contains("CCS_TestPlayerDisplayProfileApplicator"),
                    "CCS_TestPlayerLocalSessionConfigurator must apply layout through CCS_TestPlayerDisplayProfileApplicator.");
                AppendIfMissing(
                    failures,
                    configuratorSource.Contains("networkTransform.enabled = false"),
                    "CCS_TestPlayerLocalSessionConfigurator must disable NetworkTransform during offline solo play.");
            }
        }

        private static void ValidateNetworkPlayerUsesSharedYellowVisuals(List<string> failures)
        {
            GameObject networkedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_CharacterControllerMasterTestLayoutConstants.NetworkedPlayerPrefabPath);
            if (networkedPrefab == null)
            {
                return;
            }

            CCS_ControllerTestNetworkPlayerBehaviour behaviour =
                networkedPrefab.GetComponent<CCS_ControllerTestNetworkPlayerBehaviour>();
            AppendIfMissing(
                failures,
                behaviour != null,
                $"{CCS_CharacterControllerMasterTestLayoutConstants.NetworkedPlayerPrefabPath} must contain CCS_ControllerTestNetworkPlayerBehaviour.");

            Material yellowMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                CCS_CharacterControllerMasterTestLayoutConstants.PlayerYellowMaterialPath);
            Material greenMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/CCS/Modules/CharacterController/Materials/Player/M_CCS_TestPlayerGreen.mat");
            if (behaviour == null || yellowMaterial == null)
            {
                return;
            }

            SerializedObject serializedBehaviour = new SerializedObject(behaviour);
            SerializedProperty yellowProperty = serializedBehaviour.FindProperty("yellowBodyMaterial");
            AppendIfMissing(
                failures,
                yellowProperty != null && yellowProperty.objectReferenceValue == yellowMaterial,
                $"{CCS_CharacterControllerMasterTestLayoutConstants.NetworkedPlayerPrefabPath} must wire yellowBodyMaterial to M_CCS_TestPlayerYellow.");

            SerializedProperty greenProperty = serializedBehaviour.FindProperty("greenBodyMaterial");
            AppendIfMissing(
                failures,
                greenProperty == null || greenProperty.objectReferenceValue == null,
                $"{CCS_CharacterControllerMasterTestLayoutConstants.NetworkedPlayerPrefabPath} must not assign a green remote-player body material.");

            if (greenMaterial != null)
            {
                Transform bodyVisual = FindChildByName(
                    networkedPrefab.transform,
                    CCS_CharacterControllerMasterTestLayoutConstants.CapsuleVisualName);
                Renderer bodyRenderer = bodyVisual != null ? bodyVisual.GetComponent<Renderer>() : null;
                AppendIfMissing(
                    failures,
                    bodyRenderer != null && bodyRenderer.sharedMaterial == yellowMaterial,
                    $"{CCS_CharacterControllerMasterTestLayoutConstants.NetworkedPlayerPrefabPath} CapsuleVisual must use M_CCS_TestPlayerYellow.");
            }
        }

        private static void ValidateNetworkPlayerMovementAuthority(List<string> failures)
        {
            GameObject networkedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_CharacterControllerMasterTestLayoutConstants.NetworkedPlayerPrefabPath);
            if (networkedPrefab == null)
            {
                return;
            }

            AppendIfMissing(
                failures,
                networkedPrefab.GetComponent<CCS_ClientOwnerNetworkTransform>() != null,
                $"{CCS_CharacterControllerMasterTestLayoutConstants.NetworkedPlayerPrefabPath} must contain CCS_ClientOwnerNetworkTransform.");

            NetworkTransform networkTransform = networkedPrefab.GetComponent<NetworkTransform>();
            AppendIfMissing(
                failures,
                networkTransform != null
                && networkTransform.AuthorityMode == NetworkTransform.AuthorityModes.Owner,
                $"{CCS_CharacterControllerMasterTestLayoutConstants.NetworkedPlayerPrefabPath} NetworkTransform must use Owner authority mode.");

            NetworkObject networkObject = networkedPrefab.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                return;
            }

            SerializedObject serializedNetworkObject = new SerializedObject(networkObject);
            SerializedProperty synchronizeTransform = serializedNetworkObject.FindProperty("SynchronizeTransform");
            AppendIfMissing(
                failures,
                synchronizeTransform == null || !synchronizeTransform.boolValue,
                $"{CCS_CharacterControllerMasterTestLayoutConstants.NetworkedPlayerPrefabPath} NetworkObject.SynchronizeTransform must be disabled when using NetworkTransform.");

            if (networkTransform == null)
            {
                return;
            }

            SerializedObject serializedTransform = new SerializedObject(networkTransform);
            SerializedProperty syncRotY = serializedTransform.FindProperty("SyncRotAngleY");
            AppendIfMissing(
                failures,
                syncRotY != null && !syncRotY.boolValue,
                $"{CCS_CharacterControllerMasterTestLayoutConstants.NetworkedPlayerPrefabPath} NetworkTransform must not sync rotation (motor owns local yaw).");
        }

        private static void ValidatePlayerJumpConfiguration(List<string> failures)
        {
            GameObject networkedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);

            if (networkedPrefab != null)
            {
                CCS_CharacterMotor networkedMotor = networkedPrefab.GetComponent<CCS_CharacterMotor>();
                CCS_SurvivalValidationResult networkedValidation =
                    CCS_CharacterControllerValidationUtility.ValidatePlayerJumpConfiguration(
                        networkedMotor,
                        CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);
                if (!networkedValidation.IsSuccess)
                {
                    failures.Add(networkedValidation.Message);
                }
            }
        }

        private static void ValidateAimLocomotionSetup(List<string> failures)
        {
            GameObject networkedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (networkedPrefab == null)
            {
                return;
            }

            AppendValidationResult(
                failures,
                CCS_CharacterControllerValidationUtility.ValidateAimLocomotionPlayerComponents(networkedPrefab));

            CCS_CharacterMovementProfile movementProfile = AssetDatabase.LoadAssetAtPath<CCS_CharacterMovementProfile>(
                CCS_CharacterControllerConstants.DefaultMovementProfilePath);
            if (movementProfile != null)
            {
                AppendValidationResult(
                    failures,
                    CCS_CharacterControllerValidationUtility.ValidateMovementProfile(movementProfile));
            }

            string revolverSourcePath =
                "Assets/CCS/Modules/Weapons/Runtime/Components/CCS_RevolverController.cs";
            if (File.Exists(revolverSourcePath))
            {
                string revolverSource = File.ReadAllText(revolverSourcePath);
                AppendIfMissing(
                    failures,
                    !revolverSource.Contains("Input.GetMouseButton"),
                    "Revolver controller must not poll mouse input directly for aim movement.");
                AppendIfMissing(
                    failures,
                    !revolverSource.Contains("CCS_CharacterMotor"),
                    "Revolver controller must not own character movement logic.");
                AppendIfMissing(
                    failures,
                    revolverSource.Contains("CCS_CharacterAimLocomotionController"),
                    "Revolver controller must read aim state from CCS_CharacterAimLocomotionController.");
            }

            string motorSourcePath =
                "Assets/CCS/Modules/CharacterController/Runtime/Components/CCS_CharacterMotor.cs";
            if (File.Exists(motorSourcePath))
            {
                string motorSource = File.ReadAllText(motorSourcePath);
                AppendIfMissing(
                    failures,
                    !motorSource.Contains("Input.GetMouseButton"),
                    "Character motor must not poll mouse input directly for aim movement.");
            }
        }



        private static void ValidatePlayerBodyMaterialPrefab(List<string> failures, string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                return;
            }

            Transform bodyVisual = FindChildByName(
                prefab.transform,
                CCS_CharacterControllerMasterTestLayoutConstants.CapsuleVisualName);
            AppendIfMissing(
                failures,
                bodyVisual != null,
                $"{prefabPath} must contain {CCS_CharacterControllerMasterTestLayoutConstants.CapsuleVisualName}.");

            ValidateRendererMaterial(
                failures,
                prefab.transform,
                CCS_CharacterControllerMasterTestLayoutConstants.CapsuleVisualName,
                CCS_CharacterControllerMasterTestLayoutConstants.PlayerYellowMaterialPath,
                "Player body");
        }



        private static void ValidatePlayerCameraTargetsOnPrefab(List<string> failures, string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                return;
            }

            Transform followAnchor = FindChildByName(prefab.transform, CCS_CharacterControllerConstants.CameraFollowAnchorObjectName);
            AppendIfMissing(
                failures,
                followAnchor != null,
                $"{prefabPath} must contain {CCS_CharacterControllerConstants.CameraFollowAnchorObjectName}.");

            if (followAnchor != null && followAnchor.localPosition != Vector3.zero)
            {
                failures.Add($"{prefabPath} CameraFollowAnchor local position must be (0,0,0).");
            }

            Transform pitchTarget = followAnchor != null
                ? followAnchor.Find(CCS_CharacterControllerConstants.CameraPitchTargetObjectName)
                : null;
            AppendIfMissing(
                failures,
                pitchTarget != null,
                $"{prefabPath} must contain {CCS_CharacterControllerConstants.CameraPitchTargetObjectName} under CameraFollowAnchor.");

            if (pitchTarget != null)
            {
                if (pitchTarget.localPosition.y < CCS_CharacterControllerConstants.CameraPitchTargetMinimumLocalHeight
                    || pitchTarget.localPosition.y > CCS_CharacterControllerConstants.CameraPitchTargetMaximumLocalHeight)
                {
                    failures.Add(
                        $"{prefabPath} CameraPitchTarget local Y must be between "
                        + $"{CCS_CharacterControllerConstants.CameraPitchTargetMinimumLocalHeight} and "
                        + $"{CCS_CharacterControllerConstants.CameraPitchTargetMaximumLocalHeight} for third-person framing.");
                }

                if (Mathf.Abs(
                        pitchTarget.localPosition.y - CCS_CharacterControllerConstants.CameraPitchTargetLocalHeight) > 0.02f)
                {
                    failures.Add(
                        $"{prefabPath} CameraPitchTarget local Y must apply as "
                        + $"{CCS_CharacterControllerConstants.CameraPitchTargetLocalHeight:0.00}.");
                }
            }

            Transform cameraLookTarget = pitchTarget != null
                ? pitchTarget.Find(CCS_CharacterControllerConstants.CameraLookTargetObjectName)
                : null;
            AppendIfMissing(
                failures,
                cameraLookTarget != null,
                $"{prefabPath} must contain {CCS_CharacterControllerConstants.CameraLookTargetObjectName} under CameraPitchTarget.");

            if (cameraLookTarget != null
                && cameraLookTarget.localPosition != CCS_CharacterControllerConstants.CameraLookTargetLocalPosition)
            {
                failures.Add($"{prefabPath} CameraLookTarget local position must match the profile-driven chest/head reference.");
            }

            CCS_CharacterCameraController cameraController = prefab.GetComponent<CCS_CharacterCameraController>();
            if (cameraController != null && pitchTarget != null && cameraController.CameraPivot != pitchTarget)
            {
                failures.Add($"{prefabPath} camera controller tracking pivot must reference CameraPitchTarget.");
            }
        }



        private static void ValidatePlayerGlassesPrefab(List<string> failures, string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                return;
            }

            CCS_TestPlayerDisplayProfile displayProfile = AssetDatabase.LoadAssetAtPath<CCS_TestPlayerDisplayProfile>(
                CCS_TestPlayerPrefabConstants.DefaultDisplayProfilePath);

            Transform glasses = FindChildByName(
                prefab.transform,
                CCS_CharacterControllerMasterTestLayoutConstants.GlassesVisualName);
            AppendIfMissing(
                failures,
                glasses != null,
                $"{prefabPath} must contain {CCS_CharacterControllerMasterTestLayoutConstants.GlassesVisualName}.");

            if (glasses == null)
            {
                return;
            }

            MeshFilter meshFilter = glasses.GetComponent<MeshFilter>();
            AppendIfMissing(
                failures,
                meshFilter != null && meshFilter.sharedMesh != null && meshFilter.sharedMesh.name == "Capsule",
                $"{prefabPath} VisualGlasses must use a Capsule mesh.");

            AppendIfMissing(
                failures,
                glasses.GetComponent<Collider>() == null,
                $"{prefabPath} VisualGlasses must not include a collider.");

            ValidateRendererMaterial(
                failures,
                prefab.transform,
                CCS_CharacterControllerMasterTestLayoutConstants.GlassesVisualName,
                CCS_CharacterControllerMasterTestLayoutConstants.PlayerBlackMaterialPath,
                "Player glasses");

            if (displayProfile != null)
            {
                ValidatePosition(
                    failures,
                    $"{prefabPath} VisualGlasses",
                    glasses.localPosition,
                    displayProfile.GlassesLocalPosition);

                if (Quaternion.Angle(
                        glasses.localRotation,
                        Quaternion.Euler(displayProfile.GlassesLocalEuler)) > 1f)
                {
                    failures.Add(
                        $"{prefabPath} VisualGlasses rotation must match {CCS_TestPlayerPrefabConstants.DefaultDisplayProfilePath}.");
                }

                if (Vector3.Distance(glasses.localScale, displayProfile.GlassesLocalScale) > 0.01f)
                {
                    failures.Add(
                        $"{prefabPath} VisualGlasses scale must match {CCS_TestPlayerPrefabConstants.DefaultDisplayProfilePath}.");
                }
            }

            if (glasses.parent != prefab.transform)
            {
                failures.Add($"{prefabPath} VisualGlasses must be a direct child of the player root visual hierarchy.");
            }

            Transform cinemachineChild = FindChildByName(prefab.transform, "CM_ThirdPersonSurvival");
            if (cinemachineChild != null)
            {
                failures.Add($"{prefabPath} must not contain an embedded Cinemachine child.");
            }
        }



        private static void ValidateCameraProfile(List<string> failures)
        {
            CCS_CharacterCameraProfile profile = AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfile>(
                CCS_CharacterControllerMasterTestLayoutConstants.CameraProfilePath);
            AppendIfMissing(
                failures,
                profile != null,
                $"Missing camera profile: {CCS_CharacterControllerMasterTestLayoutConstants.CameraProfilePath}.");

            if (profile == null)
            {
                return;
            }

            ValidateThirdPersonSurvivalCameraProfile(failures, profile);
            ValidateAimCameraProfile(failures);
        }

        private static void ValidateThirdPersonSurvivalCameraProfile(List<string> failures, CCS_CharacterCameraProfile profile)
        {
            if (profile.ThirdPersonCameraDistance < CCS_CharacterControllerConstants.ThirdPersonCameraDistanceMinimum
                || profile.ThirdPersonCameraDistance > CCS_CharacterControllerConstants.ThirdPersonCameraDistanceMaximum)
            {
                failures.Add("Third-person camera profile distance must be between 2.85 and 3.25.");
            }

            if (profile.TrackingTargetLocalHeight < CCS_CharacterControllerConstants.CameraPitchTargetMinimumLocalHeight
                || profile.TrackingTargetLocalHeight > CCS_CharacterControllerConstants.CameraPitchTargetMaximumLocalHeight)
            {
                failures.Add("Third-person camera profile tracking target height must be between 1.40 and 1.60.");
            }

            if (profile.ThirdPersonVerticalArmLength < CCS_CharacterControllerConstants.ThirdPersonVerticalArmLengthMinimum
                || profile.ThirdPersonVerticalArmLength > CCS_CharacterControllerConstants.ThirdPersonVerticalArmLengthMaximum)
            {
                failures.Add("Third-person camera profile vertical arm length must be between 0.35 and 0.55.");
            }

            if (profile.ThirdPersonShoulderOffset.x < -0.10f || profile.ThirdPersonShoulderOffset.x > 0.35f)
            {
                failures.Add("Third-person camera profile shoulder offset X must be between -0.10 and 0.35.");
            }

            if (profile.ThirdPersonShoulderOffset.y < 0.10f || profile.ThirdPersonShoulderOffset.y > 0.35f)
            {
                failures.Add("Third-person camera profile shoulder offset Y must be between 0.10 and 0.35.");
            }

            if (profile.ThirdPersonCameraSide < -0.15f || profile.ThirdPersonCameraSide > 0.15f)
            {
                failures.Add("Third-person camera profile camera side must be between -0.15 and 0.15.");
            }

            if (profile.FieldOfView < 58f || profile.FieldOfView > 66f)
            {
                failures.Add("Third-person camera profile FOV must be between 58 and 66.");
            }

            if (!profile.ObstacleAvoidanceEnabled)
            {
                failures.Add("Third-person camera profile obstacle avoidance must be enabled.");
            }

            if (CCS_CharacterCameraLayerUtility.IsEverythingLayerMask(profile.CollisionLayerMask))
            {
                failures.Add("Third-person camera profile collision layer mask must not be Everything.");
            }

            if (CCS_CharacterCameraLayerUtility.LayerMaskIncludesExcludedLayers(profile.CollisionLayerMask))
            {
                failures.Add(
                    "Third-person camera profile collision layer mask must exclude Player, UI, and interaction layers.");
            }

            if (profile.CollisionIgnoreTag != CCS_CharacterControllerConstants.PlayerTag)
            {
                failures.Add("Third-person camera profile collision ignore tag must be Player.");
            }

            if (!profile.ValidationDisableObstacleAvoidanceForBaselinePass)
            {
                failures.Add(
                    "Third-person camera profile must enable validationDisableObstacleAvoidanceForBaselinePass.");
            }

            if (Mathf.Abs(profile.VerticalOrbitDefault - CCS_CharacterControllerMasterTestLayoutConstants.ExpectedVerticalOrbitDefault) > 0.01f)
            {
                failures.Add("Third-person camera profile default pitch must be 0 for neutral spawn framing.");
            }

            if (Mathf.Abs(profile.VerticalOrbitMin - CCS_CharacterControllerMasterTestLayoutConstants.ExpectedVerticalOrbitMin) > 0.01f
                || Mathf.Abs(profile.VerticalOrbitMax - CCS_CharacterControllerMasterTestLayoutConstants.ExpectedVerticalOrbitMax) > 0.01f)
            {
                failures.Add("Third-person camera profile pitch limits must be -45 to 70.");
            }

            if (Mathf.Abs(profile.MouseSensitivityX - CCS_CharacterControllerMasterTestLayoutConstants.ExpectedMouseSensitivityX) > 0.001f
                || Mathf.Abs(profile.MouseSensitivityY - CCS_CharacterControllerMasterTestLayoutConstants.ExpectedMouseSensitivityY) > 0.001f)
            {
                failures.Add("Third-person camera profile mouse look sensitivity must be X=0.12 and Y=0.10.");
            }

            if (Mathf.Abs(profile.GamepadSensitivityX - CCS_CharacterControllerMasterTestLayoutConstants.ExpectedGamepadSensitivityX) > 0.01f
                || Mathf.Abs(profile.GamepadSensitivityY - CCS_CharacterControllerMasterTestLayoutConstants.ExpectedGamepadSensitivityY) > 0.01f)
            {
                failures.Add("Third-person camera profile gamepad look sensitivity must be X=90 and Y=70.");
            }
        }

        private static void ValidateAimCameraProfile(List<string> failures)
        {
            CCS_CharacterCameraProfile aimProfile = AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfile>(
                CCS_CharacterControllerMasterTestLayoutConstants.AimCameraProfilePath);
            AppendIfMissing(
                failures,
                aimProfile != null,
                $"Missing aim camera profile: {CCS_CharacterControllerMasterTestLayoutConstants.AimCameraProfilePath}.");

            if (aimProfile == null)
            {
                return;
            }

            if (aimProfile.ThirdPersonCameraDistance < CCS_CharacterControllerConstants.AimCameraDistanceMinimum
                || aimProfile.ThirdPersonCameraDistance > CCS_CharacterControllerConstants.AimCameraDistanceMaximum)
            {
                failures.Add(
                    "Aim camera profile distance must be between "
                    + CCS_CharacterControllerConstants.AimCameraDistanceMinimum
                    + " and "
                    + CCS_CharacterControllerConstants.AimCameraDistanceMaximum
                    + ".");
            }

            if (Mathf.Abs(
                    aimProfile.ThirdPersonCameraDistance
                        - CCS_CharacterControllerConstants.AimCameraDistanceLegacyLoose)
                < 0.05f)
            {
                failures.Add(
                    "Aim camera profile distance must not revert to legacy ~2.5 preset; use tightened AimOverShoulder tuning.");
            }

            ValidateTunedCameraFloat(
                failures,
                "AimOverShoulder profile",
                aimProfile.ThirdPersonCameraDistance,
                CCS_CharacterControllerConstants.AimCameraDistanceTuned,
                CCS_CharacterControllerConstants.AimCameraDistanceMinimum,
                CCS_CharacterControllerConstants.AimCameraDistanceMaximum);

            if (aimProfile.TrackingTargetLocalHeight < CCS_CharacterControllerConstants.AimCameraTrackingHeightMinimum
                || aimProfile.TrackingTargetLocalHeight > CCS_CharacterControllerConstants.AimCameraTrackingHeightMaximum)
            {
                failures.Add(
                    "Aim camera profile tracking target height must be between "
                    + CCS_CharacterControllerConstants.AimCameraTrackingHeightMinimum
                    + " and "
                    + CCS_CharacterControllerConstants.AimCameraTrackingHeightMaximum
                    + ".");
            }

            ValidateTunedCameraFloat(
                failures,
                "AimOverShoulder profile height",
                aimProfile.TrackingTargetLocalHeight,
                CCS_CharacterControllerConstants.AimCameraTrackingHeightTuned,
                CCS_CharacterControllerConstants.AimCameraTrackingHeightMinimum,
                CCS_CharacterControllerConstants.AimCameraTrackingHeightMaximum);

            if (aimProfile.ThirdPersonVerticalArmLength < CCS_CharacterControllerConstants.AimVerticalArmLengthMinimum
                || aimProfile.ThirdPersonVerticalArmLength > CCS_CharacterControllerConstants.AimVerticalArmLengthMaximum)
            {
                failures.Add("Aim camera profile vertical arm length must be between 0.18 and 0.32.");
            }

            if (aimProfile.ThirdPersonShoulderOffset.x < CCS_CharacterControllerConstants.AimCameraShoulderOffsetXMinimum
                || aimProfile.ThirdPersonShoulderOffset.x > CCS_CharacterControllerConstants.AimCameraShoulderOffsetXMaximum)
            {
                failures.Add(
                    "Aim camera profile shoulder offset X must be between "
                    + CCS_CharacterControllerConstants.AimCameraShoulderOffsetXMinimum
                    + " and "
                    + CCS_CharacterControllerConstants.AimCameraShoulderOffsetXMaximum
                    + ".");
            }

            if (aimProfile.ThirdPersonShoulderOffset.y < CCS_CharacterControllerConstants.AimCameraShoulderOffsetYMinimum
                || aimProfile.ThirdPersonShoulderOffset.y > CCS_CharacterControllerConstants.AimCameraShoulderOffsetYMaximum)
            {
                failures.Add(
                    "Aim camera profile shoulder offset Y must be between "
                    + CCS_CharacterControllerConstants.AimCameraShoulderOffsetYMinimum
                    + " and "
                    + CCS_CharacterControllerConstants.AimCameraShoulderOffsetYMaximum
                    + ".");
            }

            if (aimProfile.ThirdPersonCameraSide < 0.90f || aimProfile.ThirdPersonCameraSide > 1.00f)
            {
                failures.Add("Aim camera profile camera side must be between 0.90 and 1.00.");
            }

            if (aimProfile.FieldOfView < CCS_CharacterControllerConstants.AimCameraFieldOfViewMinimum
                || aimProfile.FieldOfView > CCS_CharacterControllerConstants.AimCameraFieldOfViewMaximum)
            {
                failures.Add(
                    "Aim camera profile FOV must be between "
                    + CCS_CharacterControllerConstants.AimCameraFieldOfViewMinimum
                    + " and "
                    + CCS_CharacterControllerConstants.AimCameraFieldOfViewMaximum
                    + ".");
            }

            ValidateTunedCameraFloat(
                failures,
                "AimOverShoulder profile FOV",
                aimProfile.FieldOfView,
                CCS_CharacterControllerConstants.AimCameraFieldOfViewTuned,
                CCS_CharacterControllerConstants.AimCameraFieldOfViewMinimum,
                CCS_CharacterControllerConstants.AimCameraFieldOfViewMaximum);

            ValidateTunedCameraFloat(
                failures,
                "AimOverShoulder profile shoulder X",
                aimProfile.ThirdPersonShoulderOffset.x,
                CCS_CharacterControllerConstants.AimCameraShoulderOffsetXTuned,
                CCS_CharacterControllerConstants.AimCameraShoulderOffsetXMinimum,
                CCS_CharacterControllerConstants.AimCameraShoulderOffsetXMaximum);

            if (aimProfile.AimBlendDurationSeconds < CCS_CharacterControllerConstants.AimCameraBlendMinimumSeconds
                || aimProfile.AimBlendDurationSeconds > CCS_CharacterControllerConstants.AimCameraBlendMaximumSeconds)
            {
                failures.Add(
                    "Aim camera profile blend duration must be between "
                    + CCS_CharacterControllerConstants.AimCameraBlendMinimumSeconds
                    + " and "
                    + CCS_CharacterControllerConstants.AimCameraBlendMaximumSeconds
                    + ".");
            }

            if (!aimProfile.ObstacleAvoidanceEnabled)
            {
                failures.Add("Aim camera profile obstacle avoidance must be enabled.");
            }

            if (CCS_CharacterCameraLayerUtility.IsEverythingLayerMask(aimProfile.CollisionLayerMask))
            {
                failures.Add("Aim camera profile collision layer mask must not be Everything.");
            }

            if (CCS_CharacterCameraLayerUtility.LayerMaskIncludesExcludedLayers(aimProfile.CollisionLayerMask))
            {
                failures.Add(
                    "Aim camera profile collision layer mask must exclude Player, UI, and interaction layers.");
            }

            if (aimProfile.CollisionIgnoreTag != CCS_CharacterControllerConstants.PlayerTag)
            {
                failures.Add("Aim camera profile collision ignore tag must be Player.");
            }

            if (!aimProfile.ValidationDisableObstacleAvoidanceForBaselinePass)
            {
                failures.Add(
                    "Aim camera profile must enable validationDisableObstacleAvoidanceForBaselinePass.");
            }

            if (aimProfile.AimBlendDurationSeconds > CCS_CharacterControllerMasterTestLayoutConstants.ExpectedAimBlendDurationMaxSeconds)
            {
                failures.Add("Aim camera profile aimBlendDurationSeconds must be <= 0.5 for responsive aim transitions.");
            }

            if (Mathf.Abs(aimProfile.VerticalOrbitDefault - CCS_CharacterControllerMasterTestLayoutConstants.ExpectedVerticalOrbitDefault) > 0.01f)
            {
                failures.Add("Aim camera profile default pitch must be 0 for neutral spawn framing.");
            }

            if (Mathf.Abs(aimProfile.VerticalOrbitMin - CCS_CharacterControllerMasterTestLayoutConstants.ExpectedAimVerticalOrbitMin) > 0.01f
                || Mathf.Abs(aimProfile.VerticalOrbitMax - CCS_CharacterControllerMasterTestLayoutConstants.ExpectedAimVerticalOrbitMax) > 0.01f)
            {
                failures.Add("Aim camera profile pitch limits must be -35 to 55.");
            }
        }



        private static void ValidatePlayerNameplatePrefab(
            List<string> failures,
            string prefabPath,
            bool requireNetworkNameplate)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            AppendIfMissing(failures, prefab != null, $"Missing player prefab asset: {prefabPath}.");
            if (prefab == null)
            {
                return;
            }

            Transform nameplateRoot = FindChildByName(
                prefab.transform,
                CCS_CharacterControllerMasterTestLayoutConstants.NameplateRootObjectName);
            AppendIfMissing(
                failures,
                nameplateRoot != null,
                $"{prefabPath} must contain {CCS_CharacterControllerMasterTestLayoutConstants.NameplateRootObjectName}.");

            Transform playerNameText = nameplateRoot != null
                ? FindChildByName(nameplateRoot, CCS_CharacterControllerMasterTestLayoutConstants.PlayerNameTextObjectName)
                : null;
            AppendIfMissing(
                failures,
                playerNameText != null,
                $"{prefabPath} must contain {CCS_CharacterControllerMasterTestLayoutConstants.PlayerNameTextObjectName} under NameplateRoot.");

            if (nameplateRoot != null)
            {
                ValidatePosition(
                    failures,
                    $"{prefabPath} NameplateRoot",
                    nameplateRoot.localPosition,
                    CCS_CharacterControllerMasterTestLayoutConstants.NameplateRootLocalPosition);

                AppendIfMissing(
                    failures,
                    nameplateRoot.GetComponent<CCS_PlayerNameplateBillboard>() != null,
                    $"{prefabPath} NameplateRoot must include CCS_PlayerNameplateBillboard.");

                CCS_PlayerNameplateBillboard nameplateBillboard =
                    nameplateRoot.GetComponent<CCS_PlayerNameplateBillboard>();
                AppendIfMissing(
                    failures,
                    nameplateBillboard != null
                        && nameplateBillboard.GetType().GetMethod("SetLocalNameplateVisible") != null
                        && nameplateBillboard.GetType().GetMethod("ApplyNameplateVisibility") != null,
                    $"{prefabPath} nameplate billboard must support local ownership visibility.");

                if (nameplateRoot.GetComponent<Collider>() != null
                    || (playerNameText != null && playerNameText.GetComponent<Collider>() != null))
                {
                    failures.Add($"{prefabPath} nameplate must not include colliders.");
                }
            }

            if (playerNameText != null)
            {
                TMPro.TextMeshPro textMesh = playerNameText.GetComponent<TMPro.TextMeshPro>();
                AppendIfMissing(failures, textMesh != null, $"{prefabPath} PlayerNameText must use TextMeshPro.");
                if (textMesh != null
                    && textMesh.text != CCS_CharacterControllerMasterTestLayoutConstants.DefaultPlayerDisplayName)
                {
                    failures.Add($"{prefabPath} PlayerNameText default must be \"Player\".");
                }
            }

            if (requireNetworkNameplate)
            {
                CCS_NetworkPlayerNameplate networkNameplate = prefab.GetComponent<CCS_NetworkPlayerNameplate>();
                AppendIfMissing(
                    failures,
                    networkNameplate != null,
                    $"{prefabPath} must contain CCS_NetworkPlayerNameplate.");

                if (networkNameplate != null && playerNameText != null && nameplateRoot != null)
                {
                    SerializedObject serializedNameplate = new SerializedObject(networkNameplate);
                    Object referencedText = serializedNameplate.FindProperty("nameplateText").objectReferenceValue;
                    if (referencedText != playerNameText.GetComponent<TMPro.TextMeshPro>())
                    {
                        failures.Add($"{prefabPath} CCS_NetworkPlayerNameplate must reference PlayerNameText.");
                    }

                    CCS_PlayerNameplateBillboard billboard = nameplateRoot.GetComponent<CCS_PlayerNameplateBillboard>();
                    SerializedProperty billboardProperty = serializedNameplate.FindProperty("nameplateBillboard");
                    if (billboardProperty != null
                        && billboard != null
                        && billboardProperty.objectReferenceValue != billboard)
                    {
                        failures.Add($"{prefabPath} CCS_NetworkPlayerNameplate must reference NameplateRoot billboard.");
                    }
                }
            }
        }



        private static void ValidateJoinNotificationFeed(List<string> failures)
        {
            Transform canvasTransform = FindRootTransform(CCS_MasterTestUiConstants.MasterTestUiCanvasObjectName);
            AppendIfMissing(
                failures,
                canvasTransform != null,
                $"Scene is missing {CCS_MasterTestUiConstants.MasterTestUiCanvasObjectName}.");

            if (canvasTransform == null)
            {
                return;
            }

            Transform feedTransform = canvasTransform.Find(CCS_MasterTestUiConstants.JoinNotificationFeedObjectName);
            AppendIfMissing(
                failures,
                feedTransform != null,
                $"Scene is missing {CCS_MasterTestUiConstants.JoinNotificationFeedObjectName} under MasterTestUiCanvas.");

            if (feedTransform == null)
            {
                return;
            }

            CCS_PlayerJoinNotificationFeed feed = feedTransform.GetComponent<CCS_PlayerJoinNotificationFeed>();
            AppendIfMissing(
                failures,
                feed != null,
                $"{CCS_MasterTestUiConstants.JoinNotificationFeedObjectName} must include CCS_PlayerJoinNotificationFeed.");

            if (feed == null)
            {
                return;
            }

            CCS_PlayerJoinNotificationFeed[] allFeeds =
                Object.FindObjectsByType<CCS_PlayerJoinNotificationFeed>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (allFeeds.Length != 1)
            {
                failures.Add("Master test scene must contain exactly one CCS_PlayerJoinNotificationFeed.");
            }

            AppendIfMissing(
                failures,
                feed.MaxEntries == CCS_MasterTestUiConstants.JoinNotificationMaxEntries,
                $"Join notification feed maxEntries must be {CCS_MasterTestUiConstants.JoinNotificationMaxEntries}.");

            AppendIfMissing(
                failures,
                Mathf.Approximately(
                    feed.EntryLifetimeSeconds,
                    CCS_MasterTestUiConstants.JoinNotificationEntryLifetimeSeconds),
                $"Join notification feed entryLifetimeSeconds must be {CCS_MasterTestUiConstants.JoinNotificationEntryLifetimeSeconds:0.#}.");

            SerializedObject serializedFeed = new SerializedObject(feed);
            AppendIfMissing(
                failures,
                serializedFeed.FindProperty("entriesContainer")?.objectReferenceValue != null,
                "Join notification feed must reference JoinNotificationEntries container.");

            AppendIfMissing(
                failures,
                serializedFeed.FindProperty("panelRoot")?.objectReferenceValue != null,
                "Join notification feed must reference JoinNotificationPanel.");

            Transform panelTransform = feedTransform.Find(CCS_MasterTestUiConstants.JoinNotificationPanelObjectName);
            if (panelTransform != null)
            {
                AppendIfMissing(
                    failures,
                    !panelTransform.gameObject.activeSelf,
                    "Join notification panel must be hidden by default in the master test scene.");

                RectTransform panelRect = panelTransform as RectTransform;
                if (panelRect != null)
                {
                    AppendIfMissing(
                        failures,
                        panelRect.anchorMin == new Vector2(1f, 1f) && panelRect.anchorMax == new Vector2(1f, 1f),
                        "Join notification panel must be anchored to the top-right.");

                    AppendIfMissing(
                        failures,
                        Mathf.Approximately(
                            panelRect.sizeDelta.x,
                            CCS_MasterTestUiConstants.JoinNotificationPanelWidth),
                        $"Join notification panel width must be {CCS_MasterTestUiConstants.JoinNotificationPanelWidth:0.#}.");

                    AppendIfMissing(
                        failures,
                        Mathf.Approximately(
                            -panelRect.anchoredPosition.x,
                            CCS_MasterTestUiConstants.JoinNotificationPanelMargin),
                        $"Join notification panel right margin must be {CCS_MasterTestUiConstants.JoinNotificationPanelMargin:0.#}.");
                }

                Transform titleTransform = panelTransform.Find(CCS_MasterTestUiConstants.JoinNotificationTitleObjectName);
                TMP_Text titleText = titleTransform != null ? titleTransform.GetComponent<TMP_Text>() : null;
                AppendIfMissing(
                    failures,
                    titleText != null && titleText.text == CCS_MasterTestUiConstants.JoinNotificationHeaderText,
                    $"Join notification header must read \"{CCS_MasterTestUiConstants.JoinNotificationHeaderText}\".");
            }

            RectTransform feedRect = feedTransform as RectTransform;
            if (feedRect != null)
            {
                AppendIfMissing(
                    failures,
                    feedRect.anchorMin == new Vector2(1f, 1f) && feedRect.anchorMax == new Vector2(1f, 1f),
                    "Join notification feed root must be anchored to the top-right.");
            }

            ValidateJoinNotificationSourceContracts(failures);
        }

        private static void ValidateJoinNotificationSourceContracts(List<string> failures)
        {
            const string spawnControllerPath =
                "Assets/CCS/Modules/CharacterController/Tests/Runtime/CCS_MasterTestSpawnController.cs";
            const string feedPath =
                "Assets/CCS/Modules/CharacterController/Tests/Runtime/CCS_PlayerJoinNotificationFeed.cs";
            const string networkNameplatePath =
                "Assets/CCS/Modules/CharacterController/Tests/Netcode/Runtime/CCS_NetworkPlayerNameplate.cs";
            const string joinAnnouncerPath =
                "Assets/CCS/Modules/CharacterController/Tests/Netcode/Runtime/CCS_NetworkPlayerJoinAnnouncer.cs";

            if (File.Exists(spawnControllerPath))
            {
                string spawnSource = File.ReadAllText(spawnControllerPath);
                AppendIfMissing(
                    failures,
                    !spawnSource.Contains("CCS_PlayerJoinNotificationFeedRegistry.ShowPlayerJoined"),
                    "CCS_MasterTestSpawnController must not notify the join feed during solo spawn.");
            }

            if (File.Exists(feedPath))
            {
                string feedSource = File.ReadAllText(feedPath);
                AppendIfMissing(
                    failures,
                    feedSource.Contains("CCS_MasterTestNetworkSessionUtility.IsNetworkSessionActive"),
                    "CCS_PlayerJoinNotificationFeed must gate notifications on active Netcode sessions.");
                AppendIfMissing(
                    failures,
                    feedSource.Contains("UpdatePanelVisibility"),
                    "CCS_PlayerJoinNotificationFeed must hide the panel when there are no active entries.");
            }

            if (File.Exists(networkNameplatePath))
            {
                string networkSource = File.ReadAllText(networkNameplatePath);
                AppendIfMissing(
                    failures,
                    networkSource.Contains("NotifyPlayerJoinedClientRpc"),
                    "CCS_NetworkPlayerNameplate must broadcast join notifications through ClientRpc.");
                AppendIfMissing(
                    failures,
                    networkSource.Contains("CCS_TestPlayerSessionEvents.RaisePlayerNameChanged"),
                    "CCS_NetworkPlayerNameplate must raise PlayerNameChanged session events.");
                AppendIfMissing(
                    failures,
                    networkSource.Contains("CCS_TestPlayerSessionEvents.RaisePlayerSpawned"),
                    "CCS_NetworkPlayerNameplate must raise PlayerSpawned session events.");
            }

            if (File.Exists(joinAnnouncerPath))
            {
                string announcerSource = File.ReadAllText(joinAnnouncerPath);
                AppendIfMissing(
                    failures,
                    announcerSource.Contains("ServerRpcSentOwnerClientIds"),
                    "CCS_NetworkPlayerJoinAnnouncer must deduplicate server join announcements by OwnerClientId.");
                AppendIfMissing(
                    failures,
                    announcerSource.Contains("LocalDisplayedOwnerClientIds"),
                    "CCS_NetworkPlayerJoinAnnouncer must deduplicate local join feed entries by OwnerClientId.");
                AppendIfMissing(
                    failures,
                    announcerSource.Contains("FlushClientPendingAnnouncements"),
                    "CCS_NetworkPlayerJoinAnnouncer must flush client join announcements when the feed registers.");
                AppendIfMissing(
                    failures,
                    announcerSource.Contains("FlushAllServerPendingAnnouncements"),
                    "CCS_NetworkPlayerJoinAnnouncer must flush server join announcements when Master Test is ready.");
                AppendIfMissing(
                    failures,
                    announcerSource.Contains("AnnounceAllConnectedPlayersIfReady"),
                    "CCS_NetworkPlayerJoinAnnouncer must announce persisted players when Master Test becomes ready.");
                AppendIfMissing(
                    failures,
                    announcerSource.Contains("CCS_TestPlayerSessionEvents.PlayerSpawned"),
                    "CCS_NetworkPlayerJoinAnnouncer must subscribe to PlayerSpawned session events.");
                AppendIfMissing(
                    failures,
                    announcerSource.Contains("CCS_TestPlayerSessionEvents.PlayerNameChanged"),
                    "CCS_NetworkPlayerJoinAnnouncer must subscribe to PlayerNameChanged session events.");
            }
        }



        private static void ValidateNameplateOwnershipVisibility(List<string> failures)
        {
            const string spawnControllerPath =
                "Assets/CCS/Modules/CharacterController/Tests/Runtime/CCS_MasterTestSpawnController.cs";
            const string localConfiguratorPath =
                "Assets/CCS/Modules/CharacterController/Tests/Runtime/CCS_TestPlayerLocalSessionConfigurator.cs";
            const string networkNameplatePath =
                "Assets/CCS/Modules/CharacterController/Tests/Netcode/Runtime/CCS_NetworkPlayerNameplate.cs";
            const string nameplateBillboardPath =
                "Assets/CCS/Modules/CharacterController/Tests/Runtime/CCS_PlayerNameplateBillboard.cs";

            if (File.Exists(spawnControllerPath))
            {
                string spawnSource = File.ReadAllText(spawnControllerPath);
                AppendIfMissing(
                    failures,
                    spawnSource.Contains("TryConfigureOfflinePlayer"),
                    "CCS_MasterTestSpawnController must configure solo players through CCS_TestPlayerLocalSessionConfigurator.");
            }

            if (File.Exists(localConfiguratorPath))
            {
                string configuratorSource = File.ReadAllText(localConfiguratorPath);
                AppendIfMissing(
                    failures,
                    configuratorSource.Contains("ApplyNameplateVisibility(isLocalOwner: true)"),
                    "CCS_TestPlayerLocalSessionConfigurator must hide the solo/local owner nameplate.");
            }

            if (File.Exists(networkNameplatePath))
            {
                string networkSource = File.ReadAllText(networkNameplatePath);
                AppendIfMissing(
                    failures,
                    networkSource.Contains("ApplyNameplateVisibility")
                        || networkSource.Contains("ApplyLocalOwnershipVisibility"),
                    "CCS_NetworkPlayerNameplate must apply local ownership nameplate visibility.");
                AppendIfMissing(
                    failures,
                    networkSource.Contains("IsOwner"),
                    "CCS_NetworkPlayerNameplate must use IsOwner to hide only the local owner's nameplate.");
            }

            if (File.Exists(nameplateBillboardPath))
            {
                string billboardSource = File.ReadAllText(nameplateBillboardPath);
                AppendIfMissing(
                    failures,
                    billboardSource.Contains("SetLocalNameplateVisible")
                        && billboardSource.Contains("ApplyNameplateVisibility"),
                    "CCS_PlayerNameplateBillboard must support local-owner self-hide visibility.");
            }
        }



        private static void ValidateCameraRig(List<string> failures)

        {

            Transform cameraRig = FindRootTransform(CCS_CharacterControllerMasterTestLayoutConstants.CameraRigInstanceName);

            AppendIfMissing(

                failures,

                cameraRig != null,

                $"Scene is missing {CCS_CharacterControllerMasterTestLayoutConstants.CameraRigInstanceName}.");

            if (cameraRig == null)

            {

                return;

            }



            if (CountNamedRootObjects(CCS_CharacterControllerMasterTestLayoutConstants.CameraRigInstanceName) != 1)

            {

                failures.Add("Scene must contain exactly one camera rig.");

            }



            ValidatePosition(

                failures,

                CCS_CharacterControllerMasterTestLayoutConstants.CameraRigInstanceName,

                cameraRig.position,

                CCS_CharacterControllerMasterTestLayoutConstants.CameraRigPosition);



            Transform tpCamera = FindChildByName(cameraRig, "CinemachineCamera_TP");
            AppendIfMissing(
                failures,
                tpCamera != null,
                "Camera rig is missing a third-person Cinemachine camera.");

            Transform aimCamera = FindChildByName(
                cameraRig,
                CCS_CharacterControllerConstants.AimCinemachineCameraName);
            AppendIfMissing(
                failures,
                aimCamera != null,
                "Camera rig is missing CinemachineCamera_Aim.");

            Camera[] sceneCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            int mainCameraCount = 0;
            for (int i = 0; i < sceneCameras.Length; i++)
            {
                Camera sceneCamera = sceneCameras[i];
                if (sceneCamera != null
                    && sceneCamera.CompareTag("MainCamera")
                    && sceneCamera.gameObject.scene == cameraRig.gameObject.scene)
                {
                    mainCameraCount++;
                }
            }

            AppendIfMissing(
                failures,
                mainCameraCount == 1,
                "Master test scene must contain exactly one Main Camera.");

            CCS_CharacterCameraProfileSet profileSet = AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfileSet>(
                CCS_CharacterControllerConstants.DefaultCameraProfileSetPath);
            AppendIfMissing(
                failures,
                profileSet != null && profileSet.AimOverShoulderProfile != null,
                "Default camera profile set must assign an aim-over-shoulder profile.");

            CCS_CharacterCameraController sceneCameraController =
                cameraRig.GetComponent<CCS_CharacterCameraController>();
            if (sceneCameraController != null)
            {
                SerializedObject serializedCamera = new SerializedObject(sceneCameraController);
                SerializedProperty cinemachineProperty = serializedCamera.FindProperty("cinemachineCamera");
                AppendIfMissing(
                    failures,
                    cinemachineProperty != null && cinemachineProperty.objectReferenceValue != null,
                    "Scene camera rig controller must reference CinemachineCamera_TP.");

                SerializedProperty aimCinemachineProperty = serializedCamera.FindProperty("aimCinemachineCamera");
                AppendIfMissing(
                    failures,
                    aimCinemachineProperty != null && aimCinemachineProperty.objectReferenceValue != null,
                    "Scene camera rig controller must reference CinemachineCamera_Aim.");
            }

            CinemachineThirdPersonFollow thirdPersonFollow = tpCamera != null
                ? tpCamera.GetComponent<CinemachineThirdPersonFollow>()
                : null;
            if (thirdPersonFollow != null)
            {
                ValidateTunedCameraFloat(
                    failures,
                    "CinemachineCamera_TP",
                    thirdPersonFollow.CameraDistance,
                    CCS_CharacterControllerConstants.ThirdPersonCameraDistanceTuned,
                    CCS_CharacterControllerConstants.ThirdPersonCameraDistanceMinimum,
                    CCS_CharacterControllerConstants.ThirdPersonCameraDistanceMaximum);

#if CINEMACHINE_PHYSICS
                ValidateCinemachineCollisionFilter(failures, thirdPersonFollow, "CinemachineCamera_TP");
                if (!thirdPersonFollow.AvoidObstacles.Enabled)
                {
                    failures.Add("Third-person camera collision avoidance must be enabled.");
                }
#endif
            }

            ValidateCinemachineLookInput(failures, tpCamera, requireObstacleAvoidanceEnabled: true);
            if (aimCamera != null)
            {
                AppendIfMissing(
                    failures,
                    aimCamera.GetComponent<CinemachineThirdPersonFollow>() != null,
                    "CinemachineCamera_Aim must use Third Person Follow.");

                AppendIfMissing(
                    failures,
                    aimCamera.GetComponent<CinemachineThirdPersonAim>() != null,
                    "CinemachineCamera_Aim must use Third Person Aim extension.");

                ValidateCinemachineRotationControl(failures, aimCamera, "CinemachineCamera_Aim");

                AppendIfMissing(
                    failures,
                    tpCamera.GetComponent<CinemachineThirdPersonAim>() == null,
                    "CinemachineCamera_TP must not use Third Person Aim.");

                CinemachineThirdPersonFollow aimThirdPersonFollow =
                    aimCamera.GetComponent<CinemachineThirdPersonFollow>();
                if (aimThirdPersonFollow != null)
                {
                    ValidateTunedCameraFloat(
                        failures,
                        "CinemachineCamera_Aim",
                        aimThirdPersonFollow.CameraDistance,
                        CCS_CharacterControllerConstants.AimCameraDistanceTuned,
                        CCS_CharacterControllerConstants.AimCameraDistanceMinimum,
                        CCS_CharacterControllerConstants.AimCameraDistanceMaximum);

                    if (aimThirdPersonFollow.ShoulderOffset.x < CCS_CharacterControllerConstants.AimCameraShoulderOffsetXMinimum
                        || aimThirdPersonFollow.ShoulderOffset.x > CCS_CharacterControllerConstants.AimCameraShoulderOffsetXMaximum)
                    {
                        failures.Add(
                            "Aim camera shoulder offset X must be between "
                            + CCS_CharacterControllerConstants.AimCameraShoulderOffsetXMinimum
                            + " and "
                            + CCS_CharacterControllerConstants.AimCameraShoulderOffsetXMaximum
                            + " for right-shoulder framing.");
                    }

                    ValidateTunedCameraFloat(
                        failures,
                        "CinemachineCamera_Aim shoulder X",
                        aimThirdPersonFollow.ShoulderOffset.x,
                        CCS_CharacterControllerConstants.AimCameraShoulderOffsetXTuned,
                        CCS_CharacterControllerConstants.AimCameraShoulderOffsetXMinimum,
                        CCS_CharacterControllerConstants.AimCameraShoulderOffsetXMaximum);

#if CINEMACHINE_PHYSICS
                    ValidateCinemachineCollisionFilter(failures, aimThirdPersonFollow, "CinemachineCamera_Aim");
                    if (!aimThirdPersonFollow.AvoidObstacles.Enabled)
                    {
                        failures.Add("Aim camera collision avoidance must be enabled.");
                    }
#endif
                }

                Transform mainCameraTransform = cameraRig.Find("Main Camera");
                if (mainCameraTransform != null)
                {
                    CinemachineBrain brain = mainCameraTransform.GetComponent<CinemachineBrain>();
                    if (brain != null
                        && brain.DefaultBlend.Time > CCS_CharacterControllerMasterTestLayoutConstants.ExpectedAimBlendDurationMaxSeconds)
                    {
                        failures.Add("Cinemachine Brain default blend must be <= 0.5 seconds for aim transitions.");
                    }
                }
            }

            ValidateCameraRigPrefabAsset(failures, baselinePass: false);
        }



        private static void ValidateSceneCameraRigBaseline(List<string> failures)
        {
            Transform cameraRig = GameObject.Find(CCS_CharacterControllerMasterTestLayoutConstants.CameraRigInstanceName)?.transform;
            if (cameraRig == null)
            {
                return;
            }

            Transform tpCamera = FindChildByName(cameraRig, "CinemachineCamera_TP");
            Transform aimCamera = FindChildByName(
                cameraRig,
                CCS_CharacterControllerConstants.AimCinemachineCameraName);

            ValidateCinemachineLookInput(failures, tpCamera, requireObstacleAvoidanceEnabled: false);
            if (aimCamera != null)
            {
                ValidateCinemachineRotationControl(failures, aimCamera, "CinemachineCamera_Aim");
            }

            CinemachineThirdPersonFollow thirdPersonFollow = tpCamera != null
                ? tpCamera.GetComponent<CinemachineThirdPersonFollow>()
                : null;
            CinemachineThirdPersonFollow aimThirdPersonFollow = aimCamera != null
                ? aimCamera.GetComponent<CinemachineThirdPersonFollow>()
                : null;

#if CINEMACHINE_PHYSICS
            if (thirdPersonFollow != null && thirdPersonFollow.AvoidObstacles.Enabled)
            {
                failures.Add(
                    "Baseline camera validation requires third-person obstacle avoidance to be disabled.");
            }

            if (aimThirdPersonFollow != null && aimThirdPersonFollow.AvoidObstacles.Enabled)
            {
                failures.Add("Baseline camera validation requires aim obstacle avoidance to be disabled.");
            }
#endif
        }



        private static void ValidateCameraRigPrefabAsset(List<string> failures, bool baselinePass)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_CharacterControllerMasterTestLayoutConstants.CameraRigPrefabPath);
            AppendIfMissing(
                failures,
                prefab != null,
                $"Missing camera rig prefab at {CCS_CharacterControllerMasterTestLayoutConstants.CameraRigPrefabPath}.");

            if (prefab == null)
            {
                return;
            }

            Transform tpCamera = prefab.transform.Find("CinemachineCamera_TP");
            ValidateCinemachineLookInput(
                failures,
                tpCamera,
                requireObstacleAvoidanceEnabled: !baselinePass);

            Transform aimCamera = prefab.transform.Find(CCS_CharacterControllerConstants.AimCinemachineCameraName);
            AppendIfMissing(
                failures,
                aimCamera != null,
                "Camera rig prefab must contain CinemachineCamera_Aim.");

            if (aimCamera != null)
            {
                AppendIfMissing(
                    failures,
                    aimCamera.GetComponent<CinemachineThirdPersonFollow>() != null,
                    "Camera rig prefab aim camera must use Third Person Follow.");

                AppendIfMissing(
                    failures,
                    aimCamera.GetComponent<CinemachineThirdPersonAim>() != null,
                    "Camera rig prefab aim camera must use Third Person Aim.");

                ValidateCinemachineRotationControl(failures, aimCamera, "CinemachineCamera_Aim");

                CinemachineThirdPersonFollow aimThirdPersonFollow =
                    aimCamera.GetComponent<CinemachineThirdPersonFollow>();
                if (aimThirdPersonFollow != null)
                {
                    ValidateTunedCameraFloat(
                        failures,
                        "CinemachineCamera_Aim",
                        aimThirdPersonFollow.CameraDistance,
                        CCS_CharacterControllerConstants.AimCameraDistanceTuned,
                        CCS_CharacterControllerConstants.AimCameraDistanceMinimum,
                        CCS_CharacterControllerConstants.AimCameraDistanceMaximum);

#if CINEMACHINE_PHYSICS
                    if (baselinePass)
                    {
                        if (aimThirdPersonFollow.AvoidObstacles.Enabled)
                        {
                            failures.Add(
                                "Baseline validation requires aim camera obstacle avoidance to be disabled.");
                        }
                    }
                    else
                    {
                        ValidateCinemachineCollisionFilter(failures, aimThirdPersonFollow, "CinemachineCamera_Aim");
                        if (!aimThirdPersonFollow.AvoidObstacles.Enabled)
                        {
                            failures.Add("Aim camera collision avoidance must be enabled.");
                        }
                    }
#endif
                }
            }
        }



        private static void ValidateCinemachineRotationControl(
            List<string> failures,
            Transform cameraTransform,
            string cameraName)
        {
            if (cameraTransform == null)
            {
                return;
            }

            CinemachineRotateWithFollowTarget rotateWithFollowTarget =
                cameraTransform.GetComponent<CinemachineRotateWithFollowTarget>();
            AppendIfMissing(
                failures,
                rotateWithFollowTarget != null && rotateWithFollowTarget.enabled,
                $"{cameraName} must use Rotate With Follow Target rotation control.");

            AppendIfMissing(
                failures,
                cameraTransform.GetComponent<CinemachineRotationComposer>() == null,
                $"{cameraName} must not use Rotation Composer.");

            CinemachineCamera cinemachineCamera = cameraTransform.GetComponent<CinemachineCamera>();
            if (cinemachineCamera != null)
            {
                CameraTarget target = cinemachineCamera.Target;
                if (target.CustomLookAtTarget || target.LookAtTarget != null)
                {
                    failures.Add($"{cameraName} Look At target must remain unset for survival/aim cameras.");
                }
            }
        }



#if CINEMACHINE_PHYSICS
        private static void ValidateCinemachineCollisionFilter(
            List<string> failures,
            CinemachineThirdPersonFollow thirdPersonFollow,
            string cameraName)
        {
            if (thirdPersonFollow == null)
            {
                return;
            }

            LayerMask collisionFilter = thirdPersonFollow.AvoidObstacles.CollisionFilter;
            if (CCS_CharacterCameraLayerUtility.IsEverythingLayerMask(collisionFilter))
            {
                failures.Add($"{cameraName} collision filter must not be Everything.");
            }

            if (CCS_CharacterCameraLayerUtility.LayerMaskIncludesExcludedLayers(collisionFilter))
            {
                failures.Add(
                    $"{cameraName} collision filter must exclude Player, UI, and interaction layers.");
            }
        }
#else
        private static void ValidateCinemachineCollisionFilter(
            List<string> failures,
            CinemachineThirdPersonFollow thirdPersonFollow,
            string cameraName)
        {
        }
#endif



        private static void ValidatePlayerCameraCollisionExclusion(List<string> failures, string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                return;
            }

            if (!prefab.CompareTag(CCS_CharacterControllerConstants.PlayerTag))
            {
                failures.Add($"{prefabPath} player root must use the Player tag for camera collision exclusion.");
            }

            int playerLayer = LayerMask.NameToLayer(CCS_CharacterControllerConstants.PlayerLayerName);
            if (playerLayer < 0)
            {
                failures.Add("Project must define a Player layer for camera collision exclusion.");
                return;
            }

            if (prefab.layer != playerLayer)
            {
                failures.Add($"{prefabPath} player root must be on the Player layer.");
            }

            Collider[] colliders = prefab.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider == null)
                {
                    continue;
                }

                if (collider.gameObject.layer != playerLayer)
                {
                    failures.Add(
                        $"{prefabPath} collider '{collider.name}' must be on the Player layer for camera collision exclusion.");
                }
            }
        }



        private static void ValidateCameraFollowAnchorRotationChain(List<string> failures, string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                return;
            }

            Transform followAnchor = FindChildByName(
                prefab.transform,
                CCS_CharacterControllerConstants.CameraFollowAnchorObjectName);
            AppendIfMissing(
                failures,
                followAnchor != null && followAnchor.GetComponent<CCS_CharacterCameraFollowAnchor>() != null,
                $"{prefabPath} must contain CameraFollowAnchor with CCS_CharacterCameraFollowAnchor for yaw control.");

            Transform pitchTarget = followAnchor != null
                ? followAnchor.Find(CCS_CharacterControllerConstants.CameraPitchTargetObjectName)
                : null;
            AppendIfMissing(
                failures,
                pitchTarget != null,
                $"{prefabPath} CameraPitchTarget must exist under CameraFollowAnchor for pitch and Cinemachine tracking.");

            if (pitchTarget != null)
            {
                if (pitchTarget.localPosition.y < CCS_CharacterControllerConstants.CameraPitchTargetMinimumLocalHeight
                    || pitchTarget.localPosition.y > CCS_CharacterControllerConstants.CameraPitchTargetMaximumLocalHeight)
                {
                    failures.Add(
                        $"{prefabPath} CameraPitchTarget local Y must be between "
                        + $"{CCS_CharacterControllerConstants.CameraPitchTargetMinimumLocalHeight} and "
                        + $"{CCS_CharacterControllerConstants.CameraPitchTargetMaximumLocalHeight} for third-person framing.");
                }

                if (Mathf.Abs(
                        pitchTarget.localPosition.y - CCS_CharacterControllerConstants.CameraPitchTargetLocalHeight) > 0.02f)
                {
                    failures.Add(
                        $"{prefabPath} CameraPitchTarget local Y must apply as "
                        + $"{CCS_CharacterControllerConstants.CameraPitchTargetLocalHeight:0.00}.");
                }
            }

            CCS_CharacterCameraController cameraController = prefab.GetComponent<CCS_CharacterCameraController>();
            if (cameraController != null && pitchTarget != null && cameraController.CameraPivot != pitchTarget)
            {
                failures.Add(
                    $"{prefabPath} camera controller tracking pivot must reference CameraPitchTarget.");
            }
        }



        private static void ValidateCinemachineLookInput(
            List<string> failures,
            Transform tpCamera,
            bool requireObstacleAvoidanceEnabled)
        {
            AppendIfMissing(
                failures,
                tpCamera != null,
                "Camera rig is missing CinemachineCamera_TP.");

            if (tpCamera == null)
            {
                return;
            }

            AppendIfMissing(
                failures,
                tpCamera.GetComponent<CinemachineThirdPersonFollow>() != null,
                "CinemachineCamera_TP must use Third Person Follow for position control.");

            AppendIfMissing(
                failures,
                tpCamera.GetComponent<CinemachineOrbitalFollow>() == null,
                "CinemachineCamera_TP must not use Orbital Follow with Third Person Follow architecture.");

            AppendIfMissing(
                failures,
                tpCamera.GetComponent<CinemachineRotationComposer>() == null,
                "CinemachineCamera_TP must not use Rotation Composer with Third Person Follow architecture.");

            AppendIfMissing(
                failures,
                tpCamera.GetComponent<CinemachineInputAxisController>() == null,
                "CinemachineCamera_TP must not use Input Axis Controller; shared rig target owns look.");

            AppendIfMissing(
                failures,
                !HasLegacyBoundPivotComponent(tpCamera),
                "CinemachineCamera_TP must not contain CCS_CinemachineBoundPivot.");

            ValidateCinemachineRotationControl(failures, tpCamera, "CinemachineCamera_TP");

            CinemachineThirdPersonFollow thirdPersonFollow = tpCamera.GetComponent<CinemachineThirdPersonFollow>();
            AppendIfMissing(
                failures,
                thirdPersonFollow != null && thirdPersonFollow.enabled,
                "CinemachineCamera_TP must use an enabled Third Person Follow component.");

            if (thirdPersonFollow != null)
            {
                ValidateTunedCameraFloat(
                    failures,
                    "CinemachineCamera_TP",
                    thirdPersonFollow.CameraDistance,
                    CCS_CharacterControllerConstants.ThirdPersonCameraDistanceTuned,
                    CCS_CharacterControllerConstants.ThirdPersonCameraDistanceMinimum,
                    CCS_CharacterControllerConstants.ThirdPersonCameraDistanceMaximum);

                if (tpCamera.GetComponent<CinemachineThirdPersonAim>() != null)
                {
                    failures.Add("CinemachineCamera_TP must not use Third Person Aim.");
                }

#if CINEMACHINE_PHYSICS
                if (requireObstacleAvoidanceEnabled)
                {
                    ValidateCinemachineCollisionFilter(failures, thirdPersonFollow, "CinemachineCamera_TP");
                    if (!thirdPersonFollow.AvoidObstacles.Enabled)
                    {
                        failures.Add("Third-person camera collision avoidance must be enabled.");
                    }
                }
                else if (thirdPersonFollow.AvoidObstacles.Enabled)
                {
                    failures.Add(
                        "Baseline camera validation requires third-person obstacle avoidance to be disabled.");
                }

                if (thirdPersonFollow.AvoidObstacles.CameraRadius <= 0.01f)
                {
                    failures.Add("Third-person camera collision radius must be greater than zero.");
                }
#endif
            }
        }



        private static void ValidateCcsRuntimeScriptsDoNotWriteOrbitalAxisValues(List<string> failures)
        {
            const string runtimeRoot = "Assets/CCS/Modules/CharacterController/Runtime";
            if (!Directory.Exists(runtimeRoot))
            {
                return;
            }

            string[] files = Directory.GetFiles(runtimeRoot, "*.cs", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                string normalizedPath = files[i].Replace('\\', '/');
                string source = File.ReadAllText(normalizedPath);
                if (ContainsOrbitalAxisValueWrite(source))
                {
                    failures.Add(
                        $"CCS runtime script must not write Cinemachine orbital axis values: {normalizedPath}.");
                }
            }
        }



        private static bool ContainsOrbitalAxisValueWrite(string source)
        {
            return source.Contains("HorizontalAxis.Value")
                || source.Contains("VerticalAxis.Value")
                || source.Contains("RadialAxis.Value");
        }



        private static SerializedProperty GetAxisControllersProperty(SerializedObject serializedAxisController)
        {
            SerializedProperty controllerManager = serializedAxisController.FindProperty("m_ControllerManager");
            return controllerManager != null
                ? controllerManager.FindPropertyRelative("Controllers")
                : serializedAxisController.FindProperty("Controllers");
        }



        private static bool HasLegacyBoundPivotComponent(Transform tpCamera)
        {
            if (tpCamera == null)
            {
                return false;
            }

            Component[] components = tpCamera.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component != null && component.GetType().Name == "CCS_CinemachineBoundPivot")
                {
                    return true;
                }
            }

            return false;
        }



        private static void ValidateDirectionalLight(List<string> failures)

        {

            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);

            int directionalCount = 0;

            for (int i = 0; i < lights.Length; i++)

            {

                if (lights[i] != null && lights[i].type == LightType.Directional)

                {

                    directionalCount++;

                }

            }



            AppendIfMissing(

                failures,

                directionalCount == 1,

                $"Scene must contain exactly one {CCS_CharacterControllerMasterTestLayoutConstants.DirectionalLightName}.");

        }



        private static void ValidateSingleAudioListener(List<string> failures)

        {

            AudioListener[] listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);

            int enabledCount = 0;

            for (int i = 0; i < listeners.Length; i++)

            {

                if (listeners[i] != null && listeners[i].enabled)

                {

                    enabledCount++;

                }

            }



            AppendIfMissing(

                failures,

                enabledCount == 1,

                $"Scene must contain exactly one enabled AudioListener (found {enabledCount}).");

        }



        private static void ValidateNoNetworkedPlayerInMasterTestScene(List<string> failures)

        {

            Scene masterScene = SceneManager.GetActiveScene();

            CCS_ControllerTestNetworkPlayerBehaviour[] networkedPlayers =

                Object.FindObjectsByType<CCS_ControllerTestNetworkPlayerBehaviour>(

                    FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < networkedPlayers.Length; i++)

            {

                CCS_ControllerTestNetworkPlayerBehaviour networkedPlayer = networkedPlayers[i];

                if (networkedPlayer != null && networkedPlayer.gameObject.scene == masterScene)

                {

                    failures.Add(

                        "Master test scene must not contain a placed networked player prefab instance. "

                        + "Network players must spawn at runtime only.");

                    break;

                }

            }

            NetworkObject[] sceneNetworkObjects = Object.FindObjectsByType<NetworkObject>(

                FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < sceneNetworkObjects.Length; i++)

            {

                NetworkObject sceneNetworkObject = sceneNetworkObjects[i];

                if (sceneNetworkObject != null

                    && sceneNetworkObject.gameObject.scene == masterScene

                    && sceneNetworkObject.IsSceneObject.HasValue

                    && sceneNetworkObject.IsSceneObject.Value)

                {

                    failures.Add(

                        "Master test scene must not contain scene-placed NetworkObject instances for player spawning.");

                    break;

                }

            }

        }



        private static void ValidateNoNetworkManager(List<string> failures)

        {

            Scene masterScene = SceneManager.GetActiveScene();

            NetworkManager[] networkManagers = Object.FindObjectsByType<NetworkManager>(

                FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < networkManagers.Length; i++)

            {

                if (networkManagers[i] != null && networkManagers[i].gameObject.scene == masterScene)

                {

                    failures.Add("Master test scene must not contain a scene NetworkManager.");

                    break;

                }

            }

        }



        private static void ValidateNoCharacterDebugHud(List<string> failures)

        {

            string debugHudSourcePath =
                "Assets/CCS/Modules/CharacterController/Runtime/Components/CCS_CharacterControllerDebugHud.cs";

            AppendIfMissing(
                failures,
                !File.Exists(debugHudSourcePath),
                "CCS_CharacterControllerDebugHud source must be removed from the project.");

            for (int i = 0; i < CCS_CharacterControllerMasterTestLayoutConstants.ForbiddenDebugHudObjectNames.Length; i++)

            {

                string objectName = CCS_CharacterControllerMasterTestLayoutConstants.ForbiddenDebugHudObjectNames[i];

                if (GameObject.Find(objectName) != null)

                {

                    failures.Add($"Master test scene must not contain debug HUD object {objectName}.");

                }

            }

            TMP_Text[] sceneTexts = Object.FindObjectsByType<TMP_Text>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < sceneTexts.Length; i++)

            {

                TMP_Text text = sceneTexts[i];

                if (text != null
                    && !string.IsNullOrEmpty(text.text)
                    && text.text.Contains(
                        CCS_CharacterControllerMasterTestLayoutConstants.CharacterControllerDebugHudTitle,
                        System.StringComparison.Ordinal))

                {

                    failures.Add(
                        "Master test scene must not contain visible Character Controller Debug HUD text.");

                    break;

                }

            }

            MonoBehaviour[] sceneBehaviours = Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < sceneBehaviours.Length; i++)

            {

                MonoBehaviour behaviour = sceneBehaviours[i];

                if (behaviour == null)

                {

                    continue;

                }

                string behaviourName = behaviour.GetType().Name;

                if (behaviourName == "CCS_CharacterControllerDebugHud"
                    || behaviourName == "CCS_PlayerAttributeHud")

                {

                    failures.Add(
                        $"Master test scene must not contain legacy debug HUD component {behaviourName}.");

                }

            }

        }



        private static void ValidateNoLegacyObjects(List<string> failures)

        {

            for (int i = 0; i < CCS_CharacterControllerMasterTestLayoutConstants.LegacyObjectNamesToRemove.Length; i++)

            {

                string legacyName = CCS_CharacterControllerMasterTestLayoutConstants.LegacyObjectNamesToRemove[i];

                if (GameObject.Find(legacyName) != null)

                {

                    failures.Add($"Legacy object {legacyName} must be removed from the master test scene.");

                }

            }

        }



        private static void ValidateNoDestroyedPrefabInstances(List<string> failures)

        {

            Transform[] transforms = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);

            for (int i = 0; i < transforms.Length; i++)

            {

                if (transforms[i] != null && PrefabUtility.IsPrefabAssetMissing(transforms[i].gameObject))

                {

                    failures.Add($"Scene contains destroyed prefab reference on {transforms[i].name}.");

                    return;

                }

            }

        }



        private static void ValidateTestPoint(

            List<string> failures,

            Transform testPoints,

            string pointName,

            Vector3 expectedPosition)

        {

            Transform point = FindDirectChild(testPoints, pointName);

            AppendIfMissing(

                failures,

                point != null,

                $"TestPoints is missing {pointName}.");

            if (point == null)

            {

                return;

            }



            ValidatePosition(failures, pointName, point.position, expectedPosition);

        }



        private static bool TryGetRampWalkSurfaceEndpoints(
            Transform rampSurface,
            out Vector3 highWorld,
            out Vector3 lowWorld)
        {
            highWorld = default;
            lowWorld = default;
            if (rampSurface == null)
            {
                return false;
            }

            // Sample both walk-surface edges on the top face. Root yaw can flip local +/- Z in world space.
            Vector3 edgeA = rampSurface.TransformPoint(new Vector3(0f, 0.5f, -0.5f));
            Vector3 edgeB = rampSurface.TransformPoint(new Vector3(0f, 0.5f, 0.5f));
            if (edgeA.y >= edgeB.y)
            {
                highWorld = edgeA;
                lowWorld = edgeB;
            }
            else
            {
                highWorld = edgeB;
                lowWorld = edgeA;
            }

            return true;
        }



        private static void ValidateRampEndpointPosition(
            List<string> failures,
            string objectName,
            Vector3 actualPosition,
            Vector3 expectedPosition)
        {
            if (Vector3.Distance(actualPosition, expectedPosition)
                > CCS_CharacterControllerMasterTestLayoutConstants.RampEndpointPositionTolerance)
            {
                failures.Add(
                    $"{objectName} position is {FormatVector(actualPosition)} but expected {FormatVector(expectedPosition)}.");
            }
        }



        private static void ValidateTunedCameraFloat(
            List<string> failures,
            string label,
            float actualValue,
            float expectedValue,
            float minimumValue,
            float maximumValue,
            float tolerance = 0.02f)
        {
            if (actualValue < minimumValue || actualValue > maximumValue)
            {
                failures.Add($"{label} distance must be between {minimumValue:0.00} and {maximumValue:0.00}.");
                return;
            }

            if (Mathf.Abs(actualValue - expectedValue) > tolerance)
            {
                failures.Add($"{label} distance must apply as {expectedValue:0.00} but was {actualValue:0.00}.");
            }
        }



        private static void ValidatePosition(

            List<string> failures,

            string objectName,

            Vector3 actualPosition,

            Vector3 expectedPosition)

        {

            if (Vector3.Distance(actualPosition, expectedPosition)

                > CCS_CharacterControllerMasterTestLayoutConstants.PositionTolerance)

            {

                failures.Add(

                    $"{objectName} position is {FormatVector(actualPosition)} but expected {FormatVector(expectedPosition)}.");

            }

        }



        private static void ValidateRenderersUseMaterial(

            List<string> failures,

            Transform root,

            string expectedMaterialPath,

            string label)

        {

            Material expectedMaterial = AssetDatabase.LoadAssetAtPath<Material>(expectedMaterialPath);

            if (expectedMaterial == null)

            {

                failures.Add($"Missing material asset {expectedMaterialPath}.");

                return;

            }



            MeshRenderer[] renderers = root.GetComponentsInChildren<MeshRenderer>(true);

            if (renderers.Length == 0)

            {

                failures.Add($"{label} has no renderers to validate materials.");

                return;

            }



            for (int i = 0; i < renderers.Length; i++)

            {

                if (renderers[i] == null || renderers[i].sharedMaterial == expectedMaterial)

                {

                    continue;

                }



                failures.Add($"{label} renderer {renderers[i].gameObject.name} is not using {expectedMaterial.name}.");

                return;

            }

        }



        private static void ValidateRendererMaterial(

            List<string> failures,

            Transform root,

            string childName,

            string expectedMaterialPath,

            string label)

        {

            Transform target = FindChildByName(root, childName);

            if (target == null)

            {

                failures.Add($"{label} is missing {childName}.");

                return;

            }



            Material expectedMaterial = AssetDatabase.LoadAssetAtPath<Material>(expectedMaterialPath);

            MeshRenderer renderer = target.GetComponent<MeshRenderer>();

            if (expectedMaterial == null || renderer == null)

            {

                failures.Add($"{label} material validation failed for {childName}.");

                return;

            }



            if (renderer.sharedMaterial != expectedMaterial)

            {

                failures.Add($"{label} must use {expectedMaterial.name}.");

            }

        }



        private static int CountDirectChildrenNamed(Transform parent, string objectName)

        {

            int count = 0;

            if (parent == null)

            {

                return count;

            }



            for (int i = 0; i < parent.childCount; i++)

            {

                if (parent.GetChild(i).name == objectName)

                {

                    count++;

                }

            }



            return count;

        }



        private static int CountNamedRootObjects(string objectName)

        {

            int count = 0;

            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();

            for (int i = 0; i < roots.Length; i++)

            {

                if (roots[i] != null && roots[i].name == objectName)

                {

                    count++;

                }

            }



            return count;

        }



        private static Transform FindRootTransform(string objectName)

        {

            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();

            for (int i = 0; i < roots.Length; i++)

            {

                if (roots[i] != null && roots[i].name == objectName)

                {

                    return roots[i].transform;

                }

            }



            return null;

        }



        private static Transform FindDirectChild(Transform parent, string objectName)

        {

            if (parent == null)

            {

                return null;

            }



            for (int i = 0; i < parent.childCount; i++)

            {

                Transform child = parent.GetChild(i);

                if (child.name == objectName)

                {

                    return child;

                }

            }



            return null;

        }



        private static Transform FindChildByName(Transform parent, string objectName)

        {

            if (parent == null)

            {

                return null;

            }



            Transform[] children = parent.GetComponentsInChildren<Transform>(true);

            for (int i = 0; i < children.Length; i++)

            {

                if (children[i].name == objectName)

                {

                    return children[i];

                }

            }



            return null;

        }



        private static string FormatVector(Vector3 value)

        {

            return string.Format(

                CultureInfo.InvariantCulture,

                "({0:0.##},{1:0.##},{2:0.##})",

                value.x,

                value.y,

                value.z);

        }



        private static void AppendValidationResult(List<string> failures, CCS_SurvivalValidationResult result)
        {
            if (!result.IsSuccess)
            {
                failures.Add(result.Message);
            }
        }

        private static void AppendIfMissing(List<string> failures, bool condition, string message)

        {

            if (!condition)

            {

                failures.Add(message);

            }

        }



        #endregion

    }

}


