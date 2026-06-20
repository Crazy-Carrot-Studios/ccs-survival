using System.IO;
using CCS.Modules.Attributes;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AttributesAssetBuilder
// CATEGORY: Modules / Attributes / Editor
// PURPOSE: Creates and repairs canonical Attributes test profile assets.
// PLACEMENT: Editor utility invoked from Attributes validation and netcode setup.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Keeps Health definition reproducible without hand-editing asset YAML.
// =============================================================================

namespace CCS.Modules.Attributes.Editor
{
    public static class CCS_AttributesAssetBuilder
    {
        #region Public Methods

        public static bool EnsureAttributesAssets()
        {
            bool changed = EnsureHealthDefinitionAsset();
            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return changed;
        }

        public static bool EnsureHealthDefinitionAsset()
        {
            CCS_AttributeDefinition healthDefinition = AssetDatabase.LoadAssetAtPath<CCS_AttributeDefinition>(
                CCS_AttributesConstants.HealthDefinitionPath);
            bool created = false;
            if (healthDefinition == null)
            {
                string directory = Path.GetDirectoryName(CCS_AttributesConstants.HealthDefinitionPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                healthDefinition = ScriptableObject.CreateInstance<CCS_AttributeDefinition>();
                healthDefinition.name = "CCS_AttributeDefinition_Health";
                AssetDatabase.CreateAsset(healthDefinition, CCS_AttributesConstants.HealthDefinitionPath);
                created = true;
            }

            SerializedObject serializedDefinition = new SerializedObject(healthDefinition);
            bool changed = created;
            changed |= SetString(serializedDefinition, "profileDisplayName", CCS_AttributesConstants.HealthDisplayName);
            changed |= SetString(serializedDefinition, "profileId", CCS_AttributesConstants.HealthAttributeId);
            changed |= SetString(
                serializedDefinition,
                "profileDescription",
                "Default player health attribute for v0.3.0 Attributes foundation.");
            changed |= SetString(serializedDefinition, "profileVersion", CCS_AttributesConstants.ModuleVersion);
            changed |= SetFloat(serializedDefinition, "defaultValue", 100f);
            changed |= SetFloat(serializedDefinition, "minValue", 0f);
            changed |= SetFloat(serializedDefinition, "maxValue", 100f);
            changed |= SetBool(serializedDefinition, "allowRegeneration", false);
            changed |= SetString(serializedDefinition, "uiLabel", CCS_AttributesConstants.HealthDisplayName);
            changed |= SetColor(
                serializedDefinition,
                "uiColor",
                new Color(0.85f, 0.2f, 0.2f, 1f));

            if (changed)
            {
                serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(healthDefinition);
            }

            return changed;
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

        private static bool SetFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || Mathf.Approximately(property.floatValue, value))
            {
                return false;
            }

            property.floatValue = value;
            return true;
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

        private static bool SetColor(SerializedObject serializedObject, string propertyName, Color value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.colorValue == value)
            {
                return false;
            }

            property.colorValue = value;
            return true;
        }

        #endregion
    }
}
