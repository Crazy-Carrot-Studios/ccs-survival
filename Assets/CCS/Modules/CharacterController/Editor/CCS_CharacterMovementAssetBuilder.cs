using System.IO;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterMovementAssetBuilder
// CATEGORY: Modules / CharacterController / Editor
// PURPOSE: Ensures default movement profile assets include aim strafe tuning.
// PLACEMENT: Editor utility invoked from master test setup and validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.3 — profile-driven aim locomotion multipliers and rotation speed.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterMovementAssetBuilder
    {
        #region Public Methods

        public static bool EnsureMovementProfileAssets()
        {
            bool changed = EnsureDefaultMovementProfileAsset();

            if (changed)
            {
                AssetDatabase.SaveAssets();
            }

            return changed;
        }

        #endregion

        #region Private Methods

        private static bool EnsureDefaultMovementProfileAsset()
        {
            CCS_CharacterMovementProfile profile = LoadOrCreateProfile(
                CCS_CharacterControllerConstants.DefaultMovementProfilePath,
                "CCS_CharacterMovementProfile_Default");
            if (profile == null)
            {
                return false;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            bool changed = false;
            changed |= ForceSetString(
                serializedProfile,
                "profileId",
                CCS_CharacterControllerConstants.MovementProfileId);
            changed |= ForceSetString(
                serializedProfile,
                "profileDisplayName",
                "Default Movement");
            changed |= ForceSetString(serializedProfile, "profileVersion", "0.6.3-aim-strafe-movement");
            changed |= ForceSetFloat(
                serializedProfile,
                "airControl",
                CCS_CharacterControllerConstants.DefaultAirControl);
            changed |= ForceSetFloat(
                serializedProfile,
                "aimMovementSpeedMultiplier",
                CCS_CharacterControllerConstants.DefaultAimMovementSpeedMultiplier);
            changed |= ForceSetFloat(
                serializedProfile,
                "aimRotationSpeedDegrees",
                CCS_CharacterControllerConstants.DefaultAimRotationSpeedDegrees);
            changed |= ForceSetBool(serializedProfile, "aimDisableSprint", true);
            changed |= ForceSetFloat(
                serializedProfile,
                "aimStrafeDeadZone",
                CCS_CharacterControllerConstants.DefaultAimStrafeDeadZone);
            changed |= ForceSetFloat(
                serializedProfile,
                "aimBackpedalMultiplier",
                CCS_CharacterControllerConstants.DefaultAimBackpedalMultiplier);
            changed |= ForceSetFloat(
                serializedProfile,
                "aimSideStrafeMultiplier",
                CCS_CharacterControllerConstants.DefaultAimSideStrafeMultiplier);

            if (changed)
            {
                serializedProfile.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(profile);
            }

            return changed;
        }

        private static CCS_CharacterMovementProfile LoadOrCreateProfile(string assetPath, string assetName)
        {
            assetPath = assetPath.Replace('\\', '/');
            CCS_CharacterMovementProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_CharacterMovementProfile>(assetPath);
            if (profile != null)
            {
                return profile;
            }

            string directory = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(directory) && !AssetDatabase.IsValidFolder(directory))
            {
                string parent = Path.GetDirectoryName(directory)?.Replace('\\', '/');
                string folderName = Path.GetFileName(directory);
                if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(folderName))
                {
                    AssetDatabase.CreateFolder(parent, folderName);
                }
            }

            profile = ScriptableObject.CreateInstance<CCS_CharacterMovementProfile>();
            profile.name = assetName;
            AssetDatabase.CreateAsset(profile, assetPath);
            return profile;
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

        private static bool ForceSetFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || Mathf.Approximately(property.floatValue, value))
            {
                return false;
            }

            property.floatValue = value;
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
