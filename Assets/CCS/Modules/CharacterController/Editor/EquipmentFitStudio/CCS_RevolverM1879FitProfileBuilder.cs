using System.IO;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverM1879FitProfileBuilder
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Creates and seeds Revolver M1879 fit profile pack assets.
// PLACEMENT: Editor builder invoked from master test setup and Fit Studio rebuild.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.8 profile data only. Does not overwrite manually tuned assets.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public static class CCS_RevolverM1879FitProfileBuilder
    {
        #region Public Methods

        public static bool EnsureRevolverM1879FitProfilePack()
        {
            EnsureFolder(CCS_EquipmentConstants.RevolverM1879FitProfileFolderPath);
            bool changed = false;
            changed |= EnsureRightHipHolsterFitProfile();
            changed |= EnsureRightHandEquippedFitProfile();
            changed |= EnsureAimIkPoseProfile();
            changed |= EnsureRightHandGripPoseProfile();
            changed |= EnsureTuningNotesReadme();

            if (changed)
            {
                AssetDatabase.SaveAssets();
            }

            return changed;
        }

        public static bool ResetRevolverFitProfilesToSeedDefaults()
        {
            bool changed = false;
            changed |= ResetHolsterProfileToSeedDefaultsExplicit();
            changed |= ResetEquippedProfileToSeedDefaultsExplicit();
            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return changed;
        }

        private static bool ResetHolsterProfileToSeedDefaultsExplicit()
        {
            return CCS_EquipmentFitProfilePersistenceUtility.ResetHolsterProfileToSeedDefaults();
        }

        private static bool ResetEquippedProfileToSeedDefaultsExplicit()
        {
            return CCS_EquipmentFitProfilePersistenceUtility.ResetEquippedProfileToSeedDefaults();
        }

        #endregion

        #region Private Methods

        private static bool EnsureRightHipHolsterFitProfile()
        {
            return EnsureAttachmentFitProfile(
                CCS_EquipmentConstants.RevolverM1879RightHipHolsterFitPath,
                "CCS_RevolverM1879_RightHipHolster_Fit",
                CCS_EquipmentConstants.HolsterSocketRightHipId,
                new Vector3(0.11f, -0.04f, 0.05f),
                new Vector3(68f, 98f, -10f),
                Vector3.one,
                "Right hip holster preview fit. Sidearm outside hip/thigh, barrel downward, grip reachable.");
        }

        private static bool EnsureRightHandEquippedFitProfile()
        {
            return EnsureAttachmentFitProfile(
                CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath,
                "CCS_RevolverM1879_RightHandEquipped_Fit",
                CCS_EquipmentConstants.HandSocketRightId,
                new Vector3(0.03f, 0.015f, 0.05f),
                new Vector3(-12f, 92f, 8f),
                Vector3.one,
                "Right hand equipped preview fit. Grip in palm, barrel forward, trigger guard near index finger.");
        }

        private static bool EnsureAimIkPoseProfile()
        {
            string assetPath = CCS_EquipmentConstants.RevolverM1879AimIkPosePath;
            if (File.Exists(assetPath))
            {
                return false;
            }

            CCS_WeaponIKPoseProfile profile = LoadOrCreateAsset<CCS_WeaponIKPoseProfile>(
                assetPath,
                "CCS_RevolverM1879_AimIKPose");
            if (profile == null)
            {
                return false;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            SetString(serializedProfile, "profileId", "CCS_RevolverM1879_AimIKPose");
            SetString(serializedProfile, "weaponId", CCS_EquipmentConstants.RevolverM1879WeaponId);
            SetString(serializedProfile, "characterRigId", CCS_EquipmentConstants.TestPlayerCc3BasePlusRigId);
            SetString(serializedProfile, "poseId", CCS_EquipmentConstants.RevolverM1879AimPoseId);
            SetVector3(serializedProfile, "rightHandTargetLocalPosition", new Vector3(0.36f, 1.36f, 0.48f));
            SetVector3(serializedProfile, "rightHandTargetLocalEulerAngles", Vector3.zero);
            SetVector3(serializedProfile, "rightElbowHintLocalPosition", new Vector3(0.44f, 1.26f, 0.14f));
            SetVector3(serializedProfile, "weaponAimTargetLocalPosition", new Vector3(0f, 1.45f, 1.15f));
            SetVector3(serializedProfile, "weaponAimTargetLocalEulerAngles", Vector3.zero);
            SetFloat(serializedProfile, "rigWeight", 0f);
            SetFloat(serializedProfile, "rightHandIKWeight", 0f);
            SetFloat(serializedProfile, "leftHandIKWeight", 0f);
            SetFloat(serializedProfile, "aimWeight", 0f);
            SetString(
                serializedProfile,
                "notes",
                "v0.6.8 foundation revolver aim IK pose. Preview weights must remain 0 in scene/prefab.");
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return true;
        }

        private static bool EnsureRightHandGripPoseProfile()
        {
            string assetPath = CCS_EquipmentConstants.RevolverM1879RightHandGripPosePath;
            if (File.Exists(assetPath))
            {
                return false;
            }

            CCS_HandPoseDefinition profile = LoadOrCreateAsset<CCS_HandPoseDefinition>(
                assetPath,
                "CCS_RevolverM1879_RightHandGripPose");
            if (profile == null)
            {
                return false;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            SetString(serializedProfile, "poseId", CCS_EquipmentConstants.RevolverM1879RightHandGripPoseId);
            SetString(serializedProfile, "weaponId", CCS_EquipmentConstants.RevolverM1879WeaponId);
            SetString(serializedProfile, "characterRigId", CCS_EquipmentConstants.TestPlayerCc3BasePlusRigId);
            SetEnum(serializedProfile, "handSide", (int)CCS_HandPoseSide.Right);
            SetFloat(serializedProfile, "thumbCurl", 0.35f);
            SetFloat(serializedProfile, "indexCurl", 0.15f);
            SetFloat(serializedProfile, "middleCurl", 0.45f);
            SetFloat(serializedProfile, "ringCurl", 0.45f);
            SetFloat(serializedProfile, "littleCurl", 0.4f);
            SetFloat(serializedProfile, "fingerSpread", 0.05f);
            SetVector3(serializedProfile, "wristLocalEulerOffset", new Vector3(-8f, 5f, 0f));
            SetString(
                serializedProfile,
                "notes",
                "Foundation-only right-hand revolver grip (v0.6.8). "
                + "Thumb wrapped around grip. Index near trigger, not fully curled. "
                + "Middle/ring/little grip handle. Wrist slightly corrected for palm alignment. "
                + "Runtime finger posing is not wired yet.");
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return true;
        }

        private static bool EnsureAttachmentFitProfile(
            string assetPath,
            string profileId,
            string socketId,
            Vector3 localPosition,
            Vector3 localEulerAngles,
            Vector3 localScale,
            string notes)
        {
            if (File.Exists(assetPath))
            {
                Debug.Log(
                    "[Equipment Fit Studio] Preserved existing revolver fit profile values: "
                    + Path.GetFileNameWithoutExtension(assetPath));
                return false;
            }

            CCS_WeaponAttachmentFitProfile profile = LoadOrCreateAsset<CCS_WeaponAttachmentFitProfile>(
                assetPath,
                profileId);
            if (profile == null)
            {
                return false;
            }

            profile.SetIdentity(
                profileId,
                CCS_EquipmentConstants.RevolverM1879WeaponId,
                CCS_EquipmentConstants.TestPlayerCc3BasePlusRigId,
                socketId);
            profile.ApplySocketTransform(profileId, localPosition, localEulerAngles, localScale);

            SerializedObject serializedProfile = new SerializedObject(profile);
            SetString(serializedProfile, "notes", notes);
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return true;
        }

        private static bool EnsureTuningNotesReadme()
        {
            string readmePath = CCS_EquipmentConstants.RevolverM1879FitTuningNotesPath;
            if (File.Exists(readmePath))
            {
                return false;
            }

            string body = @"# Revolver M1879 Fit Profiles (v0.6.8)

Profile data only — runtime holstered/equipped visuals are intentionally deferred.

## IDs

| Field | Value |
|-------|-------|
| weaponId | `ccs.weapon.revolver.m1879` |
| characterRigId | `ccs.character.testplayer.cc3_base_plus` |

## Assets

| Asset | Socket / Purpose |
|-------|------------------|
| `CCS_RevolverM1879_RightHipHolster_Fit.asset` | `CCS_HolsterSocket_RightHip` — side holster preview |
| `CCS_RevolverM1879_RightHandEquipped_Fit.asset` | `CCS_HandSocket_Right` — equipped grip preview |
| `CCS_RevolverM1879_AimIKPose.asset` | Aim IK foundation (`revolver.aim.basic`) |
| `CCS_RevolverM1879_RightHandGripPose.asset` | Hand pose foundation (`revolver.right_hand.trigger_ready`) |

## Tuning rules

- Preview revolver stays **zeroed** under the socket (`0,0,0` / identity / `1,1,1`).
- Tune **socket/profile values only** — do not move the weapon prefab root.
- Clear preview after tuning. Do not save preview objects to scene or prefab.
- IK preview weights must return to **0** before closing Fit Studio.

## Tool

Open **CCS → Character Controller → Equipment → Equipment Fit Studio**.
";
            File.WriteAllText(readmePath, body);
            AssetDatabase.ImportAsset(readmePath);
            return true;
        }

        private static T LoadOrCreateAsset<T>(string assetPath, string assetName) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            asset.name = assetName;
            AssetDatabase.CreateAsset(asset, assetPath);
            return asset;
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

        private static void SetString(SerializedObject serializedObject, string propertyName, string value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.stringValue = value;
            }
        }

        private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.floatValue = value;
            }
        }

        private static void SetEnum(SerializedObject serializedObject, string propertyName, int value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.intValue = value;
            }
        }

        private static void SetVector3(SerializedObject serializedObject, string propertyName, Vector3 value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.vector3Value = value;
            }
        }

        #endregion
    }
}
