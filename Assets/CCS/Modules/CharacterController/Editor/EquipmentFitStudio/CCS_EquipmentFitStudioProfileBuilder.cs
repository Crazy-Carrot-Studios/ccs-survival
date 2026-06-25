using System.IO;
using CCS.Modules.Weapons;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioProfileBuilder
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Creates default Equipment Fit Studio settings and profile folders.
// PLACEMENT: Editor builder invoked from master test setup and Fit Studio.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Does not overwrite user-tuned socket or fit profile values.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public static class CCS_EquipmentFitStudioProfileBuilder
    {
        #region Public Methods

        public static bool EnsureEquipmentFitStudioAssets()
        {
            EnsureFolders();
            bool changed = false;
            changed |= EnsureSettingsAsset();
            changed |= EnsureDefaultIkPoseProfile();
            changed |= EnsureAxisTestProfile();
            changed |= CCS_RevolverM1879FitProfileBuilder.EnsureRevolverM1879FitProfilePack();
            changed |= CCS_EquipmentFitStudioCleanupUtility.CleanupAllPreviewObjects();

            if (changed)
            {
                AssetDatabase.SaveAssets();
            }

            return changed;
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            EnsureFolder(CCS_EquipmentConstants.EquipmentFittingProfileRootPath);
            EnsureFolder(CCS_EquipmentConstants.EquipmentFittingIkProfileFolderPath);
            EnsureFolder(CCS_EquipmentConstants.EquipmentFittingHandPoseFolderPath);
        }

        private static bool EnsureSettingsAsset()
        {
            CCS_EquipmentFitStudioSettings settings = AssetDatabase.LoadAssetAtPath<CCS_EquipmentFitStudioSettings>(
                CCS_EquipmentConstants.EquipmentFitStudioSettingsPath);
            bool created = false;
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<CCS_EquipmentFitStudioSettings>();
                AssetDatabase.CreateAsset(settings, CCS_EquipmentConstants.EquipmentFitStudioSettingsPath);
                created = true;
            }

            CCS_EquipmentSocketProfile socketProfile = AssetDatabase.LoadAssetAtPath<CCS_EquipmentSocketProfile>(
                CCS_EquipmentConstants.DefaultEquipmentSocketProfilePath);
            GameObject previewPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_WeaponsConstants.RevolverM1879WorldPickupPrefabPath);

            SerializedObject serializedSettings = new SerializedObject(settings);
            bool changed = created;
            changed |= SetObjectReference(serializedSettings, "defaultSocketProfile", socketProfile);
            changed |= SetObjectReference(serializedSettings, "defaultPreviewWeaponPrefab", previewPrefab);
            changed |= SetString(serializedSettings, "defaultWeaponId", CCS_EquipmentConstants.DefaultPreviewWeaponId);
            changed |= SetString(
                serializedSettings,
                "defaultCharacterRigId",
                CCS_EquipmentConstants.DefaultCharacterRigId);

            if (changed)
            {
                serializedSettings.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(settings);
            }

            return changed;
        }

        private static bool EnsureDefaultIkPoseProfile()
        {
            CCS_WeaponIKPoseProfile profile = AssetDatabase.LoadAssetAtPath<CCS_WeaponIKPoseProfile>(
                CCS_EquipmentConstants.DefaultWeaponIkPoseProfilePath);
            bool created = false;
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_WeaponIKPoseProfile>();
                profile.name = "CCS_WeaponIKPoseProfile_DefaultRevolver";
                AssetDatabase.CreateAsset(profile, CCS_EquipmentConstants.DefaultWeaponIkPoseProfilePath);
                created = true;
            }

            if (created)
            {
                profile.ApplyIdentity(
                    "fit.revolver.default",
                    CCS_EquipmentConstants.RevolverM1879WeaponId,
                    CCS_EquipmentConstants.TestPlayerCc3BasePlusRigId,
                    "pose.default");
                EditorUtility.SetDirty(profile);
            }

            return created;
        }

        private static bool EnsureAxisTestProfile()
        {
            EnsureFolder(CCS_EquipmentConstants.EquipmentFitStudioAxisTestProfileFolderPath);
            CCS_WeaponAttachmentFitProfile profile = AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(
                CCS_EquipmentConstants.EquipmentFitStudioAxisTestProfilePath);
            bool created = false;
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_WeaponAttachmentFitProfile>();
                profile.name = "CCS_EquipmentFitStudio_AxisTest_DO_NOT_SHIP";
                AssetDatabase.CreateAsset(profile, CCS_EquipmentConstants.EquipmentFitStudioAxisTestProfilePath);
                created = true;
            }

            if (created)
            {
                profile.SetIdentity(
                    "CCS_EquipmentFitStudio_AxisTest_DO_NOT_SHIP",
                    CCS_EquipmentConstants.RevolverM1879WeaponId,
                    CCS_EquipmentConstants.TestPlayerCc3BasePlusRigId,
                    CCS_EquipmentConstants.HandSocketRightId);
                profile.ApplySocketTransform(
                    "CCS_EquipmentFitStudio_AxisTest_DO_NOT_SHIP",
                    Vector3.zero,
                    Vector3.zero,
                    Vector3.one);
                SerializedObject serializedProfile = new SerializedObject(profile);
                SerializedProperty notesProperty = serializedProfile.FindProperty("notes");
                if (notesProperty != null)
                {
                    notesProperty.stringValue =
                        "Editor-only axis test profile for Fit Studio validation. Do not ship or use in production.";
                    serializedProfile.ApplyModifiedPropertiesWithoutUndo();
                }

                EditorUtility.SetDirty(profile);
            }

            return created;
        }

        private static void EnsureFolder(string assetFolderPath)
        {
            assetFolderPath = assetFolderPath?.Replace('\\', '/');
            if (string.IsNullOrEmpty(assetFolderPath) || AssetDatabase.IsValidFolder(assetFolderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(assetFolderPath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent) && parent.StartsWith("Assets/"))
            {
                EnsureFolder(parent);
            }

            string folderName = Path.GetFileName(assetFolderPath);
            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(folderName))
            {
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        private static bool SetObjectReference(SerializedObject serializedObject, string propertyName, Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == value)
            {
                return false;
            }

            property.objectReferenceValue = value;
            return true;
        }

        private static bool SetString(SerializedObject serializedObject, string propertyName, string value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.stringValue == value)
            {
                return false;
            }

            property.stringValue = value;
            return true;
        }

        #endregion
    }
}
