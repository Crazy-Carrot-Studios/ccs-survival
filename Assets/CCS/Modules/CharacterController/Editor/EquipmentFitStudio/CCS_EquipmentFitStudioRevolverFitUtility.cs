using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioRevolverFitUtility
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Revolver M1879 fit profile paths, labels, apply/save helpers for Fit Studio.
// PLACEMENT: Editor utility used by Equipment Fit Studio window.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.8 profile tuning workflow only. No runtime weapon attachment.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public static class CCS_EquipmentFitStudioRevolverFitUtility
    {
        #region Public Methods

        public static string GetSocketDisplayLabel(string socketId)
        {
            switch (socketId)
            {
                case CCS_EquipmentConstants.HolsterSocketRightHipId:
                    return "Right Hip Holster (CCS_HolsterSocket_RightHip)";
                case CCS_EquipmentConstants.HandSocketRightId:
                    return "Right Hand Equipped (CCS_HandSocket_Right)";
                case CCS_EquipmentConstants.HolsterSocketLeftHipId:
                    return "Left Hip Holster (CCS_HolsterSocket_LeftHip)";
                case CCS_EquipmentConstants.HandSocketLeftId:
                    return "Left Hand (CCS_HandSocket_Left)";
                case CCS_EquipmentConstants.BackSocketLongGunAId:
                    return "Back Long Gun A (CCS_BackSocket_LongGun_A)";
                case CCS_EquipmentConstants.BackSocketLongGunBId:
                    return "Back Long Gun B (CCS_BackSocket_LongGun_B)";
                default:
                    return socketId;
            }
        }

        public static string GetSocketTuningHint(string socketId)
        {
            switch (socketId)
            {
                case CCS_EquipmentConstants.HolsterSocketRightHipId:
                    return "Tune the right hip holster: outside hip/thigh, barrel down, grip reachable. Keep preview item zeroed.";
                case CCS_EquipmentConstants.HandSocketRightId:
                    return "Tune the right hand grip: palm on grip, barrel forward, trigger guard near index finger. Keep preview item zeroed.";
                default:
                    return "Move the socket until the preview item sits where it should attach. Keep the preview item zeroed.";
            }
        }

        public static string GetRevolverAttachmentFitProfilePath(string socketId)
        {
            if (socketId == CCS_EquipmentConstants.HolsterSocketRightHipId)
            {
                return CCS_EquipmentConstants.RevolverM1879RightHipHolsterFitPath;
            }

            if (socketId == CCS_EquipmentConstants.HandSocketRightId)
            {
                return CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath;
            }

            return string.Empty;
        }

        public static CCS_WeaponAttachmentFitProfile LoadRevolverAttachmentFitProfile(string socketId)
        {
            string path = GetRevolverAttachmentFitProfilePath(socketId);
            return string.IsNullOrEmpty(path)
                ? null
                : CCS_EquipmentFitProfilePersistenceUtility.LoadProfileFromDisk(path);
        }

        public static CCS_WeaponIKPoseProfile LoadRevolverAimIkPoseProfile()
        {
            return AssetDatabase.LoadAssetAtPath<CCS_WeaponIKPoseProfile>(
                CCS_EquipmentConstants.RevolverM1879AimIkPosePath);
        }

        public static CCS_HandPoseDefinition LoadRevolverRightHandGripPose()
        {
            return AssetDatabase.LoadAssetAtPath<CCS_HandPoseDefinition>(
                CCS_EquipmentConstants.RevolverM1879RightHandGripPosePath);
        }

        public static bool SaveRevolverAttachmentFitProfile(string socketId, Transform socketTransform)
        {
            string path = GetRevolverAttachmentFitProfilePath(socketId);
            if (string.IsNullOrEmpty(path) || socketTransform == null)
            {
                return false;
            }

            CCS_WeaponAttachmentFitProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(path);
            if (profile == null)
            {
                return false;
            }

            profile.SetIdentity(
                profile.ProfileId,
                CCS_EquipmentConstants.RevolverM1879WeaponId,
                CCS_EquipmentConstants.TestPlayerCc3BasePlusRigId,
                socketId);
            profile.ApplySocketTransform(
                profile.ProfileId,
                socketTransform.localPosition,
                socketTransform.localEulerAngles,
                socketTransform.localScale);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return true;
        }

        public static bool ApplyRevolverAttachmentFitProfile(string socketId, Transform socketTransform)
        {
            CCS_WeaponAttachmentFitProfile profile = LoadRevolverAttachmentFitProfile(socketId);
            if (profile == null || socketTransform == null)
            {
                return false;
            }

            socketTransform.localPosition = profile.SocketLocalPosition;
            socketTransform.localRotation = Quaternion.Euler(profile.SocketLocalEulerAngles);
            socketTransform.localScale = profile.SocketLocalScale;
            return true;
        }

        public static bool SaveRevolverAimIkPoseProfile(Transform ikTargetsRoot)
        {
            if (ikTargetsRoot == null)
            {
                return false;
            }

            CCS_WeaponIKPoseProfile profile = LoadRevolverAimIkPoseProfile();
            if (profile == null)
            {
                return false;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            WriteIkTarget(serializedProfile, ikTargetsRoot, CCS_EquipmentConstants.RightHandIkTargetObjectName, "rightHandTargetLocalPosition", "rightHandTargetLocalEulerAngles");
            WriteIkTargetPosition(serializedProfile, ikTargetsRoot, CCS_EquipmentConstants.RightElbowHintObjectName, "rightElbowHintLocalPosition");
            WriteIkTarget(serializedProfile, ikTargetsRoot, CCS_EquipmentConstants.WeaponAimTargetObjectName, "weaponAimTargetLocalPosition", "weaponAimTargetLocalEulerAngles");
            SetFloat(serializedProfile, "rigWeight", 0f);
            SetFloat(serializedProfile, "rightHandIKWeight", 0f);
            SetFloat(serializedProfile, "leftHandIKWeight", 0f);
            SetFloat(serializedProfile, "aimWeight", 0f);
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return true;
        }

        public static bool ApplyRevolverAimIkPoseProfile(Transform ikTargetsRoot)
        {
            CCS_WeaponIKPoseProfile profile = LoadRevolverAimIkPoseProfile();
            if (profile == null || ikTargetsRoot == null)
            {
                return false;
            }

            ApplyIkTarget(ikTargetsRoot, CCS_EquipmentConstants.RightHandIkTargetObjectName, profile.RightHandTargetLocalPosition, profile.RightHandTargetLocalEulerAngles);
            ApplyIkTargetPosition(ikTargetsRoot, CCS_EquipmentConstants.RightElbowHintObjectName, profile.RightElbowHintLocalPosition);
            ApplyIkTarget(ikTargetsRoot, CCS_EquipmentConstants.WeaponAimTargetObjectName, profile.WeaponAimTargetLocalPosition, profile.WeaponAimTargetLocalEulerAngles);
            return true;
        }

        public static string[] GetSocketDropdownLabels()
        {
            string[] labels = new string[CCS_EquipmentConstants.RequiredSocketIds.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i] = GetSocketDisplayLabel(CCS_EquipmentConstants.RequiredSocketIds[i]);
            }

            return labels;
        }

        #endregion

        #region Private Methods

        private static void WriteIkTarget(
            SerializedObject serializedProfile,
            Transform ikTargetsRoot,
            string targetName,
            string positionProperty,
            string eulerProperty)
        {
            Transform target = ikTargetsRoot.Find(targetName);
            if (target == null)
            {
                return;
            }

            SetVector3(serializedProfile, positionProperty, target.localPosition);
            SetVector3(serializedProfile, eulerProperty, target.localEulerAngles);
        }

        private static void WriteIkTargetPosition(
            SerializedObject serializedProfile,
            Transform ikTargetsRoot,
            string targetName,
            string positionProperty)
        {
            Transform target = ikTargetsRoot.Find(targetName);
            if (target == null)
            {
                return;
            }

            SetVector3(serializedProfile, positionProperty, target.localPosition);
        }

        private static void ApplyIkTarget(
            Transform ikTargetsRoot,
            string targetName,
            Vector3 localPosition,
            Vector3 localEulerAngles)
        {
            Transform target = ikTargetsRoot.Find(targetName);
            if (target == null)
            {
                return;
            }

            target.localPosition = localPosition;
            target.localRotation = Quaternion.Euler(localEulerAngles);
        }

        private static void ApplyIkTargetPosition(Transform ikTargetsRoot, string targetName, Vector3 localPosition)
        {
            Transform target = ikTargetsRoot.Find(targetName);
            if (target != null)
            {
                target.localPosition = localPosition;
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

        private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.floatValue = value;
            }
        }

        #endregion
    }
}
