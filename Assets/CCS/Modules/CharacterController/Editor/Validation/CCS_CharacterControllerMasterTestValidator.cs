using System.Collections.Generic;

using System.Globalization;

using System.IO;

using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Tests;
using CCS.Modules.CharacterController.Tests.Netcode;
using CCS.Project;

using Unity.Cinemachine;
using UnityEditor;

using UnityEditor.SceneManagement;

using Unity.Netcode;

using Unity.Netcode.Components;

using UnityEngine;

using UnityEngine.SceneManagement;

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

            ValidateJoinNotificationFeed(failures);

            ValidatePlayerPrefabAssets(failures);

            ValidateCameraProfile(failures);

            ValidateCcsRuntimeScriptsDoNotWriteOrbitalAxisValues(failures);

            ValidateCameraRig(failures);

            ValidateDirectionalLight(failures);

            ValidateSingleAudioListener(failures);

            ValidateNoNetworkManager(failures);

            ValidateNoNetworkedPlayerInMasterTestScene(failures);

            ValidateNoLegacyObjects(failures);

            ValidateNoDestroyedPrefabInstances(failures);



            if (failures.Count > 0)

            {

                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));

            }



            return CCS_SurvivalValidationResult.Pass("Character controller master test scene validated.");

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

            ValidateRendererMaterial(

                failures,

                door,

                "DoorSlab",

                CCS_CharacterControllerMasterTestLayoutConstants.DoorWoodMaterialPath,

                "Door panel");

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
            ValidateCinemachineLookInput(failures, tpCamera);

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

            ValidateNetworkPlayerUsesSharedYellowVisuals(failures);
            ValidateNetworkPlayerMovementAuthority(failures);
            ValidateOfflineBootstrapOnNetworkedPrefab(failures);
            ValidateTestPlayerDisplayProfileAsset(failures);
            ValidateNetworkedPrefabDisplayProfileAssignment(failures);
            ValidatePlayerJumpConfiguration(failures);
            ValidateNameplateOwnershipVisibility(failures);
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

            Transform cameraPivot = FindChildByName(prefab.transform, "CameraPivot");
            AppendIfMissing(
                failures,
                cameraPivot != null,
                $"{prefabPath} must contain CameraPivot.");

            Transform cameraLookTarget = cameraPivot != null
                ? FindChildByName(cameraPivot, "CameraLookTarget")
                : null;
            AppendIfMissing(
                failures,
                cameraLookTarget != null,
                $"{prefabPath} must contain CameraLookTarget under CameraPivot.");
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

            if (Mathf.Abs(profile.OrbitalRadius - CCS_CharacterControllerMasterTestLayoutConstants.ExpectedOrbitalRadius) > 0.01f)
            {
                failures.Add("Third-person camera profile orbitalRadius must be 5.75 for full-body framing.");
            }

            if (Mathf.Abs(profile.FollowTargetHeight - CCS_CharacterControllerMasterTestLayoutConstants.ExpectedFollowTargetHeight) > 0.01f)
            {
                failures.Add("Third-person camera profile followTargetHeight must be 0.92.");
            }

            if (Mathf.Abs(profile.CameraHeight - CCS_CharacterControllerMasterTestLayoutConstants.ExpectedCameraShoulderOffsetY) > 0.01f)
            {
                failures.Add("Third-person camera profile cameraHeight must be 0.08.");
            }

            if (Mathf.Abs(profile.VerticalArmLength - CCS_CharacterControllerMasterTestLayoutConstants.ExpectedVerticalArmLength) > 0.01f)
            {
                failures.Add("Third-person camera profile verticalArmLength must be 0.25.");
            }

            if (Mathf.Abs(profile.VerticalOrbitDefault - CCS_CharacterControllerMasterTestLayoutConstants.ExpectedVerticalOrbitDefault) > 0.01f)
            {
                failures.Add("Third-person camera profile verticalOrbitDefault must be -10 for full-body framing.");
            }

            if (Mathf.Abs(profile.VerticalOrbitMin - CCS_CharacterControllerMasterTestLayoutConstants.ExpectedVerticalOrbitMin) > 0.01f
                || Mathf.Abs(profile.VerticalOrbitMax - CCS_CharacterControllerMasterTestLayoutConstants.ExpectedVerticalOrbitMax) > 0.01f)
            {
                failures.Add("Third-person camera profile vertical orbit limits must be -30 to 55.");
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
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
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
            }

            CinemachineOrbitalFollow orbitalFollow = tpCamera != null
                ? tpCamera.GetComponent<CinemachineOrbitalFollow>()
                : null;
            if (orbitalFollow != null)
            {
                if (Mathf.Abs(orbitalFollow.Radius - CCS_CharacterControllerMasterTestLayoutConstants.ExpectedOrbitalRadius) > 0.15f)
                {
                    failures.Add("Camera rig orbital radius must be about 5.75 for full-body framing.");
                }

                if (Mathf.Abs(orbitalFollow.TargetOffset.y - CCS_CharacterControllerMasterTestLayoutConstants.ExpectedCameraShoulderOffsetY) > 0.05f)
                {
                    failures.Add("Camera rig orbital target offset Y must be about 0.08.");
                }
            }

            CinemachineRotationComposer rotationComposer = tpCamera != null
                ? tpCamera.GetComponent<CinemachineRotationComposer>()
                : null;
            AppendIfMissing(
                failures,
                rotationComposer != null && rotationComposer.enabled,
                "CinemachineCamera_TP must use Rotation Composer for LookAt framing.");

            ValidateCinemachineLookInput(failures, tpCamera);
            ValidateCameraRigPrefabAsset(failures);
        }



        private static void ValidateCameraRigPrefabAsset(List<string> failures)
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
            ValidateCinemachineLookInput(failures, tpCamera);
        }



        private static void ValidateCinemachineLookInput(List<string> failures, Transform tpCamera)
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
                tpCamera.GetComponent<CinemachineOrbitalFollow>() != null,
                "CinemachineCamera_TP must use Orbital Follow for position control.");

            AppendIfMissing(
                failures,
                tpCamera.GetComponent<CinemachineRotationComposer>() != null,
                "CinemachineCamera_TP must use Rotation Composer for LookAt framing.");

            AppendIfMissing(
                failures,
                tpCamera.GetComponent<CinemachineThirdPersonFollow>() == null,
                "CinemachineCamera_TP must not use Third Person Follow with Orbital Follow architecture.");

            AppendIfMissing(
                failures,
                tpCamera.GetComponent<CinemachinePanTilt>() == null,
                "CinemachineCamera_TP must not use Pan Tilt with Orbital Follow architecture.");

            AppendIfMissing(
                failures,
                !HasLegacyBoundPivotComponent(tpCamera),
                "CinemachineCamera_TP must not contain CCS_CinemachineBoundPivot.");

            CinemachineOrbitalFollow orbitalFollow = tpCamera.GetComponent<CinemachineOrbitalFollow>();
            AppendIfMissing(
                failures,
                orbitalFollow != null && orbitalFollow.enabled,
                "CinemachineCamera_TP must use an enabled Cinemachine Orbital Follow component.");

            CinemachineInputAxisController axisController = tpCamera.GetComponent<CinemachineInputAxisController>();
            AppendIfMissing(
                failures,
                axisController != null && axisController.enabled,
                "CinemachineCamera_TP must contain an enabled Cinemachine Input Axis Controller.");

            if (orbitalFollow != null)
            {
                if (Mathf.Abs(orbitalFollow.VerticalAxis.Range.x - CCS_CharacterControllerMasterTestLayoutConstants.ExpectedVerticalOrbitMin) > 0.01f
                    || Mathf.Abs(orbitalFollow.VerticalAxis.Range.y - CCS_CharacterControllerMasterTestLayoutConstants.ExpectedVerticalOrbitMax) > 0.01f)
                {
                    failures.Add("Cinemachine Orbital Follow vertical orbit clamp must be -30 to 55.");
                }

                if (Mathf.Abs(orbitalFollow.VerticalAxis.Value - CCS_CharacterControllerMasterTestLayoutConstants.ExpectedVerticalOrbitDefault) > 0.01f)
                {
                    failures.Add("Cinemachine Orbital Follow default vertical orbit must start near -10.");
                }
            }

            if (axisController == null)
            {
                return;
            }

            SerializedObject serializedAxisController = new SerializedObject(axisController);
            SerializedProperty controllers = GetAxisControllersProperty(serializedAxisController);
            AppendIfMissing(
                failures,
                controllers != null && controllers.arraySize >= 2,
                "Cinemachine Input Axis Controller must expose look orbit X/Y driven axes.");

            if (controllers == null)
            {
                return;
            }

            bool foundHorizontalOrbit = false;
            bool foundVerticalOrbit = false;
            for (int i = 0; i < controllers.arraySize; i++)
            {
                SerializedProperty controller = controllers.GetArrayElementAtIndex(i);
                SerializedProperty nameProperty = controller.FindPropertyRelative("Name");
                if (nameProperty == null)
                {
                    continue;
                }

                string axisName = nameProperty.stringValue;
                bool isHorizontalOrbit = axisName == CCS_CharacterControllerConstants.LookOrbitHorizontalAxisName;
                bool isVerticalOrbit = axisName == CCS_CharacterControllerConstants.LookOrbitVerticalAxisName;
                if (!isHorizontalOrbit && !isVerticalOrbit)
                {
                    continue;
                }

                SerializedProperty enabledProperty = controller.FindPropertyRelative("Enabled");
                AppendIfMissing(
                    failures,
                    enabledProperty == null || enabledProperty.boolValue,
                    $"Cinemachine look axis {axisName} must be enabled.");

                SerializedProperty ownerProperty = controller.FindPropertyRelative("Owner");
                if (orbitalFollow != null
                    && ownerProperty != null
                    && ownerProperty.objectReferenceValue != orbitalFollow)
                {
                    failures.Add($"Cinemachine look axis {axisName} must target Cinemachine Orbital Follow.");
                }

                SerializedProperty inputProperty = controller.FindPropertyRelative("Input");
                SerializedProperty inputActionProperty = inputProperty != null
                    ? inputProperty.FindPropertyRelative("InputAction")
                    : null;
                AppendIfMissing(
                    failures,
                    inputActionProperty != null && inputActionProperty.objectReferenceValue != null,
                    $"Cinemachine look axis {axisName} must reference the Gameplay/Look input action.");

                if (isHorizontalOrbit)
                {
                    foundHorizontalOrbit = true;
                    SerializedProperty gainProperty = inputProperty?.FindPropertyRelative("Gain");
                    if (gainProperty != null
                        && Mathf.Abs(gainProperty.floatValue - CCS_CharacterControllerMasterTestLayoutConstants.ExpectedMouseSensitivityX) > 0.001f)
                    {
                        failures.Add("Cinemachine look orbit X gain must be 0.12 for mouse X sensitivity.");
                    }
                }

                if (isVerticalOrbit)
                {
                    foundVerticalOrbit = true;
                    SerializedProperty gainProperty = inputProperty?.FindPropertyRelative("Gain");
                    if (gainProperty != null
                        && Mathf.Abs(gainProperty.floatValue + CCS_CharacterControllerMasterTestLayoutConstants.ExpectedMouseSensitivityY) > 0.001f)
                    {
                        failures.Add("Cinemachine look orbit Y gain must be -0.10 for mouse Y sensitivity.");
                    }

                    SerializedProperty cancelDeltaTimeProperty = inputProperty?.FindPropertyRelative("CancelDeltaTime");
                    AppendIfMissing(
                        failures,
                        cancelDeltaTimeProperty != null && cancelDeltaTimeProperty.boolValue,
                        $"Cinemachine look axis {axisName} must enable Cancel Delta Time for mouse look.");
                }
            }

            AppendIfMissing(
                failures,
                foundHorizontalOrbit,
                "Cinemachine Input Axis Controller must include Look Orbit X.");
            AppendIfMissing(
                failures,
                foundVerticalOrbit,
                "Cinemachine Input Axis Controller must include Look Orbit Y.");
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

                    FindObjectsInactive.Include,

                    FindObjectsSortMode.None);

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

                FindObjectsInactive.Include,

                FindObjectsSortMode.None);

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

                FindObjectsInactive.Include,

                FindObjectsSortMode.None);

            for (int i = 0; i < networkManagers.Length; i++)

            {

                if (networkManagers[i] != null && networkManagers[i].gameObject.scene == masterScene)

                {

                    failures.Add("Master test scene must not contain a scene NetworkManager.");

                    break;

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


