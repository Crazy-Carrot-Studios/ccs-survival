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

            changed |= EnsureAimOverShoulderProfileAsset();

            changed |= EnsureDefaultProfileSetWiring();



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

                "Right-shoulder aim camera tuned for v0.6.2 manual playtest.");

            changed |= ForceSetString(serializedProfile, "profileVersion", "0.6.2-close-camera-tune");

            changed |= ForceSetEnum(

                serializedProfile,

                "cameraMode",

                (int)CCS_CharacterCameraMode.AimOverShoulder);

            changed |= ForceSetFloat(serializedProfile, "trackingTargetLocalHeight", 1.48f);

            changed |= ForceSetVector3(

                serializedProfile,

                "thirdPersonShoulderOffset",

                new Vector3(0.58f, 0.10f, 0f));

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



        private static bool EnsureDefaultProfileSetWiring()

        {

            CCS_CharacterCameraProfileSet profileSet = AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfileSet>(

                CCS_CharacterControllerConstants.DefaultCameraProfileSetPath);

            CCS_CharacterCameraProfile defaultProfile = AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfile>(

                CCS_CharacterControllerConstants.DefaultCameraProfilePath);

            CCS_CharacterCameraProfile aimProfile = AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfile>(

                CCS_CharacterControllerConstants.AimCameraProfilePath);

            if (profileSet == null || defaultProfile == null || aimProfile == null)

            {

                return false;

            }



            SerializedObject serializedProfileSet = new SerializedObject(profileSet);

            bool changed = false;

            SerializedProperty defaultProperty = serializedProfileSet.FindProperty("defaultProfile");

            if (defaultProperty != null && defaultProperty.objectReferenceValue != defaultProfile)

            {

                defaultProperty.objectReferenceValue = defaultProfile;

                changed = true;

            }



            SerializedProperty aimProperty = serializedProfileSet.FindProperty("aimOverShoulderProfile");

            if (aimProperty != null && aimProperty.objectReferenceValue != aimProfile)

            {

                aimProperty.objectReferenceValue = aimProfile;

                changed = true;

            }



            if (changed)

            {

                serializedProfileSet.ApplyModifiedPropertiesWithoutUndo();

                EditorUtility.SetDirty(profileSet);

            }



            return changed;

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


