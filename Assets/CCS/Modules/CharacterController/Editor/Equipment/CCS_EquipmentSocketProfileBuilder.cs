using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentSocketProfileBuilder
// CATEGORY: Modules / CharacterController / Editor / Equipment
// PURPOSE: Creates default equipment socket definition assets and profile.
// PLACEMENT: Editor utility invoked from player prefab and master test setup.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.6 creates exactly six socket definitions for the default profile.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_EquipmentSocketProfileBuilder
    {
        #region Public Methods

        public static bool EnsureDefaultEquipmentSocketProfile()
        {
            EnsureFolders();
            bool changed = false;
            changed |= EnsureSocketDefinition(
                CCS_EquipmentConstants.HolsterSocketRightHipId,
                HumanBodyBones.Hips,
                HumanBodyBones.LastBone,
                new List<string>
                {
                    CCS_EquipmentItemTypes.WeaponRevolver,
                    CCS_EquipmentItemTypes.WeaponPistol,
                },
                new Vector3(0.14f, 0.02f, -0.08f),
                new Vector3(0f, 90f, -15f),
                Vector3.one,
                100,
                new List<string>());
            changed |= EnsureSocketDefinition(
                CCS_EquipmentConstants.HolsterSocketLeftHipId,
                HumanBodyBones.Hips,
                HumanBodyBones.LastBone,
                new List<string>
                {
                    CCS_EquipmentItemTypes.WeaponPistol,
                    CCS_EquipmentItemTypes.ToolKnife,
                    CCS_EquipmentItemTypes.ToolHand,
                },
                new Vector3(-0.14f, 0.02f, -0.08f),
                new Vector3(0f, -90f, 15f),
                Vector3.one,
                90,
                new List<string>());
            changed |= EnsureSocketDefinition(
                CCS_EquipmentConstants.HandSocketRightId,
                HumanBodyBones.RightHand,
                HumanBodyBones.LastBone,
                new List<string>
                {
                    CCS_EquipmentItemTypes.WeaponRevolver,
                    CCS_EquipmentItemTypes.WeaponPistol,
                    CCS_EquipmentItemTypes.WeaponRifle,
                    CCS_EquipmentItemTypes.WeaponShotgun,
                    CCS_EquipmentItemTypes.WeaponBow,
                    CCS_EquipmentItemTypes.ToolKnife,
                    CCS_EquipmentItemTypes.ToolHand,
                },
                Vector3.zero,
                Vector3.zero,
                Vector3.one,
                200,
                new List<string>());
            changed |= EnsureSocketDefinition(
                CCS_EquipmentConstants.HandSocketLeftId,
                HumanBodyBones.LeftHand,
                HumanBodyBones.LastBone,
                new List<string>
                {
                    CCS_EquipmentItemTypes.ToolLantern,
                    CCS_EquipmentItemTypes.ToolOffhand,
                    CCS_EquipmentItemTypes.WeaponRifle,
                    CCS_EquipmentItemTypes.WeaponShotgun,
                    CCS_EquipmentItemTypes.WeaponBow,
                },
                Vector3.zero,
                Vector3.zero,
                Vector3.one,
                180,
                new List<string>());
            changed |= EnsureSocketDefinition(
                CCS_EquipmentConstants.BackSocketLongGunAId,
                HumanBodyBones.Chest,
                HumanBodyBones.Spine,
                new List<string>
                {
                    CCS_EquipmentItemTypes.WeaponRifle,
                    CCS_EquipmentItemTypes.WeaponShotgun,
                    CCS_EquipmentItemTypes.WeaponBow,
                },
                new Vector3(-0.08f, 0.05f, -0.18f),
                new Vector3(0f, 0f, 45f),
                Vector3.one,
                70,
                new List<string> { CCS_EquipmentConstants.BackSocketLongGunBId });
            changed |= EnsureSocketDefinition(
                CCS_EquipmentConstants.BackSocketLongGunBId,
                HumanBodyBones.Chest,
                HumanBodyBones.Spine,
                new List<string>
                {
                    CCS_EquipmentItemTypes.WeaponRifle,
                    CCS_EquipmentItemTypes.WeaponShotgun,
                    CCS_EquipmentItemTypes.WeaponBow,
                },
                new Vector3(0.08f, 0.05f, -0.18f),
                new Vector3(0f, 0f, -45f),
                Vector3.one,
                60,
                new List<string> { CCS_EquipmentConstants.BackSocketLongGunAId });
            changed |= EnsureProfileAsset();

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
            EnsureFolder(CCS_EquipmentConstants.EquipmentSocketsProfileRootPath);
            EnsureFolder(CCS_EquipmentConstants.EquipmentSocketDefinitionsFolderPath);
        }

        private static bool EnsureProfileAsset()
        {
            CCS_EquipmentSocketProfile profile = AssetDatabase.LoadAssetAtPath<CCS_EquipmentSocketProfile>(
                CCS_EquipmentConstants.DefaultEquipmentSocketProfilePath);
            bool created = false;
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_EquipmentSocketProfile>();
                AssetDatabase.CreateAsset(profile, CCS_EquipmentConstants.DefaultEquipmentSocketProfilePath);
                created = true;
            }

            List<CCS_EquipmentSocketDefinition> definitions = new List<CCS_EquipmentSocketDefinition>();
            for (int i = 0; i < CCS_EquipmentConstants.RequiredSocketIds.Length; i++)
            {
                string assetPath = GetSocketDefinitionAssetPath(CCS_EquipmentConstants.RequiredSocketIds[i]);
                CCS_EquipmentSocketDefinition definition =
                    AssetDatabase.LoadAssetAtPath<CCS_EquipmentSocketDefinition>(assetPath);
                if (definition != null)
                {
                    definitions.Add(definition);
                }
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            SerializedProperty socketsProperty = serializedProfile.FindProperty("socketDefinitions");
            bool changed = created || ApplySocketDefinitionList(socketsProperty, definitions);
            if (changed)
            {
                serializedProfile.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(profile);
            }

            return changed;
        }

        private static bool ApplySocketDefinitionList(
            SerializedProperty socketsProperty,
            List<CCS_EquipmentSocketDefinition> definitions)
        {
            if (socketsProperty == null)
            {
                return false;
            }

            bool changed = socketsProperty.arraySize != definitions.Count;
            socketsProperty.arraySize = definitions.Count;
            for (int i = 0; i < definitions.Count; i++)
            {
                SerializedProperty element = socketsProperty.GetArrayElementAtIndex(i);
                if (element.objectReferenceValue != definitions[i])
                {
                    element.objectReferenceValue = definitions[i];
                    changed = true;
                }
            }

            return changed;
        }

        private static bool EnsureSocketDefinition(
            string socketId,
            HumanBodyBones parentBone,
            HumanBodyBones fallbackParentBone,
            List<string> allowedItemTypes,
            Vector3 localPosition,
            Vector3 localEulerAngles,
            Vector3 localScale,
            int priority,
            List<string> blocksOtherSockets)
        {
            string assetPath = GetSocketDefinitionAssetPath(socketId);
            CCS_EquipmentSocketDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_EquipmentSocketDefinition>(assetPath);
            bool created = false;
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_EquipmentSocketDefinition>();
                definition.name = socketId;
                AssetDatabase.CreateAsset(definition, assetPath);
                created = true;
            }

            SerializedObject serializedDefinition = new SerializedObject(definition);
            bool changed = created;
            changed |= SetString(serializedDefinition, "socketId", socketId);
            changed |= SetEnum(serializedDefinition, "parentBone", (int)parentBone);
            changed |= SetEnum(serializedDefinition, "fallbackParentBone", (int)fallbackParentBone);
            changed |= SetStringList(serializedDefinition, "allowedItemTypes", allowedItemTypes);
            changed |= SetVector3(serializedDefinition, "localPosition", localPosition);
            changed |= SetVector3(serializedDefinition, "localEulerAngles", localEulerAngles);
            changed |= SetVector3(serializedDefinition, "localScale", localScale);
            changed |= SetInt(serializedDefinition, "priority", priority);
            changed |= SetStringList(serializedDefinition, "blocksOtherSockets", blocksOtherSockets);
            if (changed)
            {
                serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(definition);
            }

            return changed;
        }

        private static string GetSocketDefinitionAssetPath(string socketId)
        {
            return CCS_EquipmentConstants.EquipmentSocketDefinitionsFolderPath + "/" + socketId + ".asset";
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

        private static bool SetInt(SerializedObject serializedObject, string propertyName, int value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.intValue == value)
            {
                return false;
            }

            property.intValue = value;
            return true;
        }

        private static bool SetEnum(SerializedObject serializedObject, string propertyName, int value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.intValue == value)
            {
                return false;
            }

            property.intValue = value;
            return true;
        }

        private static bool SetVector3(SerializedObject serializedObject, string propertyName, Vector3 value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.vector3Value == value)
            {
                return false;
            }

            property.vector3Value = value;
            return true;
        }

        private static bool SetStringList(
            SerializedObject serializedObject,
            string propertyName,
            List<string> values)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                return false;
            }

            bool changed = property.arraySize != values.Count;
            property.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                if (element.stringValue != values[i])
                {
                    element.stringValue = values[i];
                    changed = true;
                }
            }

            return changed;
        }

        #endregion
    }
}
