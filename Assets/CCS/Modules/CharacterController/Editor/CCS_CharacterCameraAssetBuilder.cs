using System.IO;

using CCS.Project;

using UnityEditor;

using UnityEngine;



// =============================================================================

// SCRIPT: CCS_CharacterCameraAssetBuilder

// CATEGORY: Modules / CharacterController / Editor

// PURPOSE: Ensures camera profile assets and profile-set wiring for runtime rigs.

// PLACEMENT: Editor utility invoked from master test setup and validation.

// AUTHOR: James Schilz

// CREATED: 2026-06-07

// NOTES: Rewrites normalized Third Person Follow profile values on every setup pass.

// =============================================================================



namespace CCS.Modules.CharacterController.Editor

{

    public static class CCS_CharacterCameraAssetBuilder

    {

        #region Public Methods



        public static bool EnsureCameraProfileAssets()

        {

            bool changed = false;

            changed |= EnsureThirdPersonSurvivalProfileAsset();

            changed |= EnsureFirstPersonBodyAwareProfileAsset();

            changed |= EnsureFirstPersonAimProfileAsset();

            changed |= EnsureAimOverShoulderProfileAsset();

            changed |= EnsureDefaultProfileSetWiring();



            if (changed)

            {

                AssetDatabase.SaveAssets();

            }



            return changed;

        }

        public static bool ApplyFirstPersonBodyAwareDefaults()
        {
            bool changed = EnsureCameraProfileAssets();
            changed |= CCS_CharacterControllerPlayerPrefabBuilder.EnsurePlayerPrefabs();
            string cameraRigPath = CCS_CharacterControllerConstants.CameraRigPrefabPath;
            GameObject cameraRigPrefabRoot = PrefabUtility.LoadPrefabContents(cameraRigPath);
            if (cameraRigPrefabRoot != null)
            {
                changed |= CCS_CharacterCameraRigInputBuilder.EnsureFirstPersonBodyAwareCameras(cameraRigPrefabRoot);
                PrefabUtility.SaveAsPrefabAsset(cameraRigPrefabRoot, cameraRigPath);
                PrefabUtility.UnloadPrefabContents(cameraRigPrefabRoot);
            }
            if (changed)
            {
                AssetDatabase.SaveAssets();
            }

            return changed;
        }



        #endregion



        #region Private Methods



        private static bool EnsureThirdPersonSurvivalProfileAsset()

        {

            CCS_CharacterCameraProfile profile = LoadOrCreateProfile(

                CCS_CharacterControllerConstants.DefaultCameraProfilePath,

                "CCS_CharacterCameraProfile_ThirdPersonSurvival");

            if (profile == null)

            {

                return false;

            }



            SerializedObject serializedProfile = new SerializedObject(profile);

            bool changed = ApplyThirdPersonSurvivalProfileValues(serializedProfile);

            if (changed)

            {

                serializedProfile.ApplyModifiedPropertiesWithoutUndo();

                EditorUtility.SetDirty(profile);

            }



            return changed;

        }



        private static bool EnsureAimOverShoulderProfileAsset()

        {

            CCS_CharacterCameraProfile profile = LoadOrCreateProfile(

                CCS_CharacterControllerConstants.AimCameraProfilePath,

                "CCS_CharacterCameraProfile_AimOverShoulder");

            if (profile == null)

            {

                return false;

            }



            SerializedObject serializedProfile = new SerializedObject(profile);

            bool changed = ApplyAimOverShoulderProfileValues(serializedProfile);

            if (changed)

            {

                serializedProfile.ApplyModifiedPropertiesWithoutUndo();

                EditorUtility.SetDirty(profile);

            }



            return changed;

        }



        private static bool EnsureFirstPersonBodyAwareProfileAsset()

        {

            CCS_CharacterCameraProfile profile = LoadOrCreateProfile(

                CCS_CharacterControllerConstants.FirstPersonBodyAwareCameraProfilePath,

                "CCS_CharacterCameraProfile_FirstPersonBodyAware");

            if (profile == null)

            {

                return false;

            }



            SerializedObject serializedProfile = new SerializedObject(profile);

            bool changed = ApplyFirstPersonBodyAwareProfileValues(serializedProfile);

            if (changed)

            {

                serializedProfile.ApplyModifiedPropertiesWithoutUndo();

                EditorUtility.SetDirty(profile);

            }



            return changed;

        }



        private static bool EnsureFirstPersonAimProfileAsset()

        {

            CCS_CharacterCameraProfile profile = LoadOrCreateProfile(

                CCS_CharacterControllerConstants.FirstPersonAimCameraProfilePath,

                "CCS_CharacterCameraProfile_FirstPersonAim");

            if (profile == null)

            {

                return false;

            }



            SerializedObject serializedProfile = new SerializedObject(profile);

            bool changed = ApplyFirstPersonAimProfileValues(serializedProfile);

            if (changed)

            {

                serializedProfile.ApplyModifiedPropertiesWithoutUndo();

                EditorUtility.SetDirty(profile);

            }



            return changed;

        }



        private static CCS_CharacterCameraProfile LoadOrCreateProfile(string assetPath, string assetName)

        {

            CCS_CharacterCameraProfile profile = AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfile>(assetPath);

            if (profile != null)

            {

                return profile;

            }



            string directory = Path.GetDirectoryName(assetPath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))

            {

                Directory.CreateDirectory(directory);

            }



            profile = ScriptableObject.CreateInstance<CCS_CharacterCameraProfile>();

            profile.name = assetName;

            AssetDatabase.CreateAsset(profile, assetPath);

            return profile;

        }



        private static bool ApplyThirdPersonSurvivalProfileValues(SerializedObject serializedProfile)

        {

            bool changed = false;

            changed |= ForceSetString(serializedProfile, "profileDisplayName", "Third Person Survival");

            changed |= ForceSetString(

                serializedProfile,

                "profileId",

                "ccs.survival.profile.character.camera.thirdperson");

            changed |= ForceSetString(

                serializedProfile,

                "profileDescription",

                "Centered third-person survival camera using Cinemachine Third Person Follow.");

            changed |= ForceSetString(serializedProfile, "profileVersion", "0.6.2-close-camera-tune");

            changed |= ForceSetEnum(

                serializedProfile,

                "cameraMode",

                (int)CCS_CharacterCameraMode.ThirdPersonSurvival);

            changed |= ForceSetFloat(serializedProfile, "trackingTargetLocalHeight", 1.48f);

            changed |= ForceSetVector3(

                serializedProfile,

                "thirdPersonShoulderOffset",

                new Vector3(0.20f, 0.20f, 0f));

            changed |= ForceSetFloat(serializedProfile, "thirdPersonVerticalArmLength", 0.45f);

            changed |= ForceSetFloat(serializedProfile, "thirdPersonCameraSide", 0f);

            changed |= ForceSetFloat(serializedProfile, "thirdPersonCameraDistance", 3.0f);

            changed |= ForceSetFloat(serializedProfile, "fieldOfView", 62f);

            changed |= ForceSetFloat(serializedProfile, "verticalOrbitDefault", 0f);

            changed |= ForceSetFloat(serializedProfile, "verticalOrbitMin", -45f);

            changed |= ForceSetFloat(serializedProfile, "verticalOrbitMax", 70f);

            changed |= ForceSetEnum(

                serializedProfile,

                "defaultYawMode",

                (int)CCS_CharacterCameraDefaultYawMode.PlayerForward);

            changed |= ForceSetFloat(serializedProfile, "mouseSensitivityX", 0.12f);

            changed |= ForceSetFloat(serializedProfile, "mouseSensitivityY", 0.10f);

            changed |= ForceSetFloat(serializedProfile, "gamepadSensitivityX", 90f);

            changed |= ForceSetFloat(serializedProfile, "gamepadSensitivityY", 70f);

            changed |= ForceSetFloat(serializedProfile, "lookSmoothing", 12f);

            changed |= ForceSetFloat(serializedProfile, "followDampingX", 0.10f);

            changed |= ForceSetFloat(serializedProfile, "followDampingY", 0.12f);

            changed |= ForceSetFloat(serializedProfile, "followDampingZ", 0.10f);

            changed |= ForceSetBool(serializedProfile, "obstacleAvoidanceEnabled", true);

            changed |= ForceSetInt(
                serializedProfile,
                "collisionLayerMask",
                CCS_CharacterCameraLayerUtility.GetCameraObstructionLayerMask().value);

            changed |= ForceSetString(serializedProfile, "collisionIgnoreTag", "Player");

            changed |= ForceSetFloat(serializedProfile, "obstacleAvoidanceRadius", 0.25f);

            changed |= ForceSetFloat(serializedProfile, "collisionDampingInto", 0.08f);

            changed |= ForceSetFloat(serializedProfile, "collisionDampingFrom", 0.35f);

            changed |= ForceSetBool(serializedProfile, "validationDisableObstacleAvoidanceForBaselinePass", true);

            return changed;

        }



        private static bool ApplyAimOverShoulderProfileValues(SerializedObject serializedProfile)

        {

            bool changed = false;

            changed |= ForceSetString(serializedProfile, "profileDisplayName", "Aim Over Shoulder");

            changed |= ForceSetString(

                serializedProfile,

                "profileId",

                "ccs.survival.profile.character.camera.aimovershoulder");

            changed |= ForceSetString(

                serializedProfile,

                "profileDescription",

                "Right-shoulder aim camera tuned for v0.6.8 revolver reticle alignment.");

            changed |= ForceSetString(serializedProfile, "profileVersion", "0.6.8-aim-camera-alignment");

            changed |= ForceSetEnum(

                serializedProfile,

                "cameraMode",

                (int)CCS_CharacterCameraMode.AimOverShoulder);

            changed |= ForceSetFloat(serializedProfile, "trackingTargetLocalHeight", 1.48f);

            changed |= ForceSetVector3(

                serializedProfile,

                "thirdPersonShoulderOffset",

                new Vector3(0.65f, 0.12f, 0f));

            changed |= ForceSetFloat(serializedProfile, "thirdPersonVerticalArmLength", 0.24f);

            changed |= ForceSetFloat(serializedProfile, "thirdPersonCameraSide", 1f);

            changed |= ForceSetFloat(serializedProfile, "thirdPersonCameraDistance", 1.5f);

            changed |= ForceSetFloat(serializedProfile, "fieldOfView", 52f);

            changed |= ForceSetFloat(serializedProfile, "verticalOrbitDefault", 0f);

            changed |= ForceSetFloat(serializedProfile, "verticalOrbitMin", -45f);

            changed |= ForceSetFloat(serializedProfile, "verticalOrbitMax", 70f);

            changed |= ForceSetEnum(

                serializedProfile,

                "defaultYawMode",

                (int)CCS_CharacterCameraDefaultYawMode.PlayerForward);

            changed |= ForceSetFloat(serializedProfile, "mouseSensitivityX", 0.095f);

            changed |= ForceSetFloat(serializedProfile, "mouseSensitivityY", 0.08f);

            changed |= ForceSetFloat(serializedProfile, "gamepadSensitivityX", 72f);

            changed |= ForceSetFloat(serializedProfile, "gamepadSensitivityY", 55f);

            changed |= ForceSetFloat(serializedProfile, "lookSmoothing", 12f);

            changed |= ForceSetFloat(serializedProfile, "followDampingX", 0.06f);

            changed |= ForceSetFloat(serializedProfile, "followDampingY", 0.08f);

            changed |= ForceSetFloat(serializedProfile, "followDampingZ", 0.06f);

            changed |= ForceSetBool(serializedProfile, "obstacleAvoidanceEnabled", true);

            changed |= ForceSetInt(
                serializedProfile,
                "collisionLayerMask",
                CCS_CharacterCameraLayerUtility.GetCameraObstructionLayerMask().value);

            changed |= ForceSetString(serializedProfile, "collisionIgnoreTag", "Player");

            changed |= ForceSetFloat(serializedProfile, "obstacleAvoidanceRadius", 0.22f);

            changed |= ForceSetFloat(serializedProfile, "collisionDampingInto", 0.06f);

            changed |= ForceSetFloat(serializedProfile, "collisionDampingFrom", 0.30f);

            changed |= ForceSetFloat(serializedProfile, "aimBlendDurationSeconds", 0.45f);

            changed |= ForceSetFloat(serializedProfile, "aimLookSensitivityMultiplier", 0.85f);

            changed |= ForceSetBool(serializedProfile, "validationDisableObstacleAvoidanceForBaselinePass", true);

            return changed;

        }



        private static bool ApplyFirstPersonBodyAwareProfileValues(SerializedObject serializedProfile)

        {

            bool changed = false;

            changed |= ForceSetString(serializedProfile, "profileDisplayName", "First Person Body Aware");

            changed |= ForceSetString(

                serializedProfile,

                "profileId",

                "ccs.survival.profile.character.camera.firstpersonbodyaware");

            changed |= ForceSetString(

                serializedProfile,

                "profileDescription",

                "Body-aware first-person survival camera. Eye-forward offset keeps the view out of the skull while preserving torso/limb visibility.");

            changed |= ForceSetString(serializedProfile, "profileVersion", "0.6.14-local-self-head-mask");

            changed |= ForceSetEnum(

                serializedProfile,

                "cameraMode",

                (int)CCS_CharacterCameraMode.FirstPersonBodyAware);

            changed |= ForceSetFloat(serializedProfile, "trackingTargetLocalHeight", 1.48f);

            changed |= ForceSetFloat(serializedProfile, "fieldOfView", CCS_CharacterControllerConstants.FirstPersonFieldOfViewDefault);

            changed |= ForceSetFloat(serializedProfile, "nearClipPlane", CCS_CharacterControllerConstants.FirstPersonNearClipDefault);

            changed |= ForceSetFloat(

                serializedProfile,

                "firstPersonForwardEyeOffset",

                CCS_CharacterControllerConstants.FirstPersonForwardEyeOffsetDefault);

            changed |= ForceSetFloat(

                serializedProfile,

                "firstPersonVerticalEyeOffset",

                CCS_CharacterControllerConstants.FirstPersonVerticalEyeOffsetDefault);

            changed |= ForceSetFloat(serializedProfile, "verticalOrbitDefault", 0f);

            changed |= ForceSetFloat(serializedProfile, "verticalOrbitMin", CCS_CharacterControllerConstants.FirstPersonPitchMinimum);

            changed |= ForceSetFloat(serializedProfile, "verticalOrbitMax", CCS_CharacterControllerConstants.FirstPersonPitchMaximum);

            changed |= ForceSetFloat(serializedProfile, "mouseSensitivityX", 0.12f);

            changed |= ForceSetFloat(serializedProfile, "mouseSensitivityY", 0.10f);

            changed |= ForceSetFloat(serializedProfile, "gamepadSensitivityX", 90f);

            changed |= ForceSetFloat(serializedProfile, "gamepadSensitivityY", 70f);

            changed |= ForceSetFloat(serializedProfile, "lookSmoothing", 0f);

            changed |= ForceSetFloat(serializedProfile, "followDampingX", 0f);

            changed |= ForceSetFloat(serializedProfile, "followDampingY", 0f);

            changed |= ForceSetFloat(serializedProfile, "followDampingZ", 0f);

            changed |= ForceSetFloat(serializedProfile, "aimBlendDurationSeconds", 0.1f);

            changed |= ApplyFirstPersonHeadTrackingProfileValues(serializedProfile);

            changed |= ForceSetBool(serializedProfile, "obstacleAvoidanceEnabled", false);

            changed |= ForceSetBool(serializedProfile, "validationDisableObstacleAvoidanceForBaselinePass", true);

            return changed;

        }



        private static bool ApplyFirstPersonAimProfileValues(SerializedObject serializedProfile)

        {

            bool changed = false;

            changed |= ForceSetString(serializedProfile, "profileDisplayName", "First Person Aim");

            changed |= ForceSetString(

                serializedProfile,

                "profileId",

                "ccs.survival.profile.character.camera.firstpersonaim");

            changed |= ForceSetString(

                serializedProfile,

                "profileDescription",

                "First-person aim profile with fixed FirstPersonAimCameraAnchor above the gun hand while retaining zero damping and tightened pitch clamp.");

            changed |= ForceSetString(serializedProfile, "profileVersion", "0.6.9-fixed-first-person-aim-anchor");

            changed |= ForceSetEnum(

                serializedProfile,

                "cameraMode",

                (int)CCS_CharacterCameraMode.FirstPersonAim);

            changed |= ForceSetFloat(serializedProfile, "trackingTargetLocalHeight", 1.48f);

            changed |= ForceSetFloat(serializedProfile, "fieldOfView", CCS_CharacterControllerConstants.FirstPersonAimFieldOfViewDefault);

            changed |= ForceSetFloat(serializedProfile, "nearClipPlane", CCS_CharacterControllerConstants.FirstPersonNearClipDefault);

            changed |= ForceSetFloat(

                serializedProfile,

                "firstPersonForwardEyeOffset",

                CCS_CharacterControllerConstants.FirstPersonAimForwardEyeOffsetDefault);

            changed |= ForceSetFloat(

                serializedProfile,

                "firstPersonVerticalEyeOffset",

                CCS_CharacterControllerConstants.FirstPersonAimVerticalEyeOffsetDefault);

            changed |= ForceSetFloat(serializedProfile, "verticalOrbitDefault", 0f);

            changed |= ForceSetFloat(serializedProfile, "verticalOrbitMin", CCS_CharacterControllerConstants.FirstPersonAimPitchMinimum);

            changed |= ForceSetFloat(serializedProfile, "verticalOrbitMax", CCS_CharacterControllerConstants.FirstPersonPitchMaximum);

            changed |= ForceSetFloat(serializedProfile, "mouseSensitivityX", 0.10f);

            changed |= ForceSetFloat(serializedProfile, "mouseSensitivityY", 0.085f);

            changed |= ForceSetFloat(serializedProfile, "gamepadSensitivityX", 80f);

            changed |= ForceSetFloat(serializedProfile, "gamepadSensitivityY", 62f);

            changed |= ForceSetFloat(serializedProfile, "lookSmoothing", 0f);

            changed |= ForceSetFloat(serializedProfile, "followDampingX", 0f);

            changed |= ForceSetFloat(serializedProfile, "followDampingY", 0f);

            changed |= ForceSetFloat(serializedProfile, "followDampingZ", 0f);

            changed |= ForceSetFloat(serializedProfile, "aimBlendDurationSeconds", 0.1f);

            changed |= ForceSetFloat(serializedProfile, "aimLookSensitivityMultiplier", 0.85f);

            changed |= ApplyFirstPersonAimFixedAnchorProfileValues(serializedProfile);

            changed |= ForceSetBool(serializedProfile, "obstacleAvoidanceEnabled", false);

            changed |= ForceSetBool(serializedProfile, "validationDisableObstacleAvoidanceForBaselinePass", true);

            return changed;

        }



        private static bool ApplyFirstPersonHeadTrackingProfileValues(SerializedObject serializedProfile)

        {

            bool changed = false;

            changed |= ForceSetBool(serializedProfile, "useHeadTrackedAnchor", true);

            changed |= ForceSetVector3(

                serializedProfile,

                "headTrackedLocalOffset",

                CCS_CharacterControllerConstants.FirstPersonBodyAwareHeadTrackedLocalOffsetDefault);

            changed |= ForceSetFloat(

                serializedProfile,

                "headTrackingPositionLerpSpeed",

                CCS_CharacterControllerConstants.FirstPersonHeadTrackingPositionLerpSpeedDefault);

            changed |= ForceSetBool(serializedProfile, "inheritHeadBoneRotation", false);

            return changed;

        }

        private static bool ApplyFirstPersonAimFixedAnchorProfileValues(SerializedObject serializedProfile)

        {

            bool changed = false;

            changed |= ForceSetBool(serializedProfile, "useHeadTrackedAnchor", false);

            changed |= ForceSetVector3(

                serializedProfile,

                "fixedFirstPersonAimAnchorLocalOffset",

                CCS_CharacterControllerConstants.FirstPersonAimFixedAnchorLocalOffsetDefault);

            changed |= ForceSetBool(serializedProfile, "inheritHeadBoneRotation", false);

            return changed;

        }



        private static bool EnsureDefaultProfileSetWiring()

        {

            CCS_CharacterCameraProfileSet profileSet = AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfileSet>(

                CCS_CharacterControllerConstants.DefaultCameraProfileSetPath);

            CCS_CharacterCameraProfile firstPersonProfile = AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfile>(

                CCS_CharacterControllerConstants.FirstPersonBodyAwareCameraProfilePath);

            CCS_CharacterCameraProfile firstPersonAimProfile = AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfile>(

                CCS_CharacterControllerConstants.FirstPersonAimCameraProfilePath);

            CCS_CharacterCameraProfile thirdPersonProfile = AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfile>(

                CCS_CharacterControllerConstants.ThirdPersonSurvivalCameraProfilePath);

            CCS_CharacterCameraProfile aimProfile = AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfile>(

                CCS_CharacterControllerConstants.AimCameraProfilePath);

            if (profileSet == null || firstPersonProfile == null || thirdPersonProfile == null || aimProfile == null)

            {

                return false;

            }



            SerializedObject serializedProfileSet = new SerializedObject(profileSet);

            bool changed = false;

            changed |= SetProfileReference(serializedProfileSet, "defaultProfile", thirdPersonProfile);

            changed |= SetProfileReference(serializedProfileSet, "firstPersonProfile", firstPersonProfile);

            changed |= SetProfileReference(serializedProfileSet, "thirdPersonSurvivalProfile", thirdPersonProfile);

            changed |= SetProfileReference(serializedProfileSet, "aimOverShoulderProfile", aimProfile);

            if (firstPersonAimProfile != null)

            {

                changed |= SetProfileReference(serializedProfileSet, "firstPersonAimProfile", firstPersonAimProfile);

            }



            if (changed)

            {

                serializedProfileSet.ApplyModifiedPropertiesWithoutUndo();

                EditorUtility.SetDirty(profileSet);

            }



            return changed;

        }



        private static bool SetProfileReference(

            SerializedObject serializedProfileSet,

            string propertyName,

            CCS_CharacterCameraProfile profile)

        {

            SerializedProperty property = serializedProfileSet.FindProperty(propertyName);

            if (property == null || property.objectReferenceValue == profile)

            {

                return false;

            }

            property.objectReferenceValue = profile;

            return true;

        }



        private static bool ForceSetString(SerializedObject serializedObject, string propertyName, string value)

        {

            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null || property.stringValue == value)
            {
                return false;
            }

            property.stringValue = value;
            return true;
        }

        private static bool ForceSetInt(SerializedObject serializedObject, string propertyName, int value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.intValue == value)
            {
                return false;
            }

            property.intValue = value;
            return true;
        }

        private static bool ForceSetFloat(SerializedObject serializedObject, string propertyName, float value)

        {

            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)

            {

                return false;

            }



            if (Mathf.Approximately(property.floatValue, value))

            {

                return false;

            }



            property.floatValue = value;

            return true;

        }



        private static bool ForceSetEnum(SerializedObject serializedObject, string propertyName, int value)

        {

            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null || property.intValue == value)
            {
                return false;
            }

            property.intValue = value;
            return true;
        }

        private static bool ForceSetVector3(SerializedObject serializedObject, string propertyName, Vector3 value)

        {

            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null || property.vector3Value == value)
            {
                return false;
            }

            property.vector3Value = value;
            return true;
        }

        private static bool ForceSetBool(SerializedObject serializedObject, string propertyName, bool value)

        {

            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null || property.boolValue == value)
            {
                return false;
            }

            property.boolValue = value;
            return true;
        }



        #endregion

    }

}


