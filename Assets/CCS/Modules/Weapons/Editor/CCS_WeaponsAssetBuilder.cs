using System.IO;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WeaponsAssetBuilder
// CATEGORY: Modules / Weapons / Editor
// PURPOSE: Creates revolver definition and test damage target prefab assets.
// PLACEMENT: Editor utility invoked from Weapons validation and master test setup.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Keeps Weapons test assets reproducible without hand-editing prefab YAML.
// =============================================================================

namespace CCS.Modules.Weapons.Editor
{
    public static class CCS_WeaponsAssetBuilder
    {
        #region Public Methods

        public static bool EnsureWeaponsAssets()
        {
            bool changed = false;
            changed |= EnsureRevolverDefinitionAsset();
            changed |= EnsureTestDamageTargetPrefab();
            changed |= CCS_WeaponsVisualAssetBuilder.EnsureRevolverM1879VisualAssets();

            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return changed;
        }

        public static bool EnsureRevolverDefinitionAsset()
        {
            CCS_RevolverDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_RevolverDefinition>(
                CCS_WeaponsConstants.RevolverDefinitionProfilePath);
            bool created = false;
            if (definition == null)
            {
                string directory = Path.GetDirectoryName(CCS_WeaponsConstants.RevolverDefinitionProfilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                definition = ScriptableObject.CreateInstance<CCS_RevolverDefinition>();
                definition.name = "CCS_RevolverDefinition_Test";
                AssetDatabase.CreateAsset(definition, CCS_WeaponsConstants.RevolverDefinitionProfilePath);
                created = true;
            }

            SerializedObject serializedDefinition = new SerializedObject(definition);
            bool changed = created;
            changed |= SetString(serializedDefinition, "profileDisplayName", CCS_WeaponsConstants.DefaultRevolverDisplayName);
            changed |= SetString(serializedDefinition, "profileId", CCS_WeaponsConstants.RevolverDefinitionProfileId);
            changed |= SetString(
                serializedDefinition,
                "profileDescription",
                "Test revolver definition for v0.6.1 hitscan weapon manual playtest.");
            changed |= SetString(serializedDefinition, "profileVersion", CCS_WeaponsConstants.ModuleVersion);
            changed |= SetString(serializedDefinition, "displayName", CCS_WeaponsConstants.DefaultRevolverDisplayName);
            changed |= SetInt(serializedDefinition, "cylinderCapacity", 6);
            changed |= SetFloat(serializedDefinition, "damage", 25f);
            changed |= SetFloat(serializedDefinition, "fireCooldownSeconds", 0.35f);
            changed |= SetFloat(serializedDefinition, "reloadSeconds", 1.6f);
            changed |= SetFloat(serializedDefinition, "maxRange", 60f);
            changed |= SetFloat(serializedDefinition, "aimSpreadDegrees", 1.5f);
            changed |= SetFloat(serializedDefinition, "hipSpreadDegrees", 4f);
            changed |= SetBool(serializedDefinition, "allowFireWhileReloading", false);

            SerializedProperty hitMaskProperty = serializedDefinition.FindProperty("hitMask");
            if (hitMaskProperty != null && hitMaskProperty.intValue != ~0)
            {
                hitMaskProperty.intValue = ~0;
                changed = true;
            }

            if (changed)
            {
                serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(definition);
            }

            return changed;
        }

        public static bool EnsureTestDamageTargetPrefab()
        {
            return CCS_WeaponsTestDamageTargetPrefabBuilder.EnsureTestDamageTargetPrefab();
        }

        #endregion

        #region Private Methods

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

        private static bool SetFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || !Mathf.Approximately(property.floatValue, value))
            {
                if (property == null)
                {
                    return false;
                }

                property.floatValue = value;
                return true;
            }

            return false;
        }

        private static bool SetBool(SerializedObject serializedObject, string propertyName, bool value)
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
