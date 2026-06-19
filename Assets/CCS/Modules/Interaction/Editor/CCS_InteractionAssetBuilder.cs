using System.IO;
using CCS.Project;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_InteractionAssetBuilder
// CATEGORY: Modules / Interaction / Editor
// PURPOSE: Creates scanner profile and test toggle interactable prefab assets.
// PLACEMENT: Editor utility invoked from Interaction validation and master test setup.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Keeps Interaction assets reproducible without hand-editing prefab YAML.
// =============================================================================

namespace CCS.Modules.Interaction.Editor
{
    public static class CCS_InteractionAssetBuilder
    {
        #region Public Methods

        public static bool EnsureInteractionAssets()
        {
            bool changed = false;
            changed |= EnsureScannerProfileAsset();
            changed |= EnsureToggleInteractablePrefab();

            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return changed;
        }

        public static bool EnsureScannerProfileAsset()
        {
            CCS_InteractionScannerProfile profile = AssetDatabase.LoadAssetAtPath<CCS_InteractionScannerProfile>(
                CCS_InteractionConstants.ScannerProfilePath);
            bool created = false;
            if (profile == null)
            {
                string directory = Path.GetDirectoryName(CCS_InteractionConstants.ScannerProfilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                profile = ScriptableObject.CreateInstance<CCS_InteractionScannerProfile>();
                profile.name = "CCS_InteractionScannerProfile_Default";
                AssetDatabase.CreateAsset(profile, CCS_InteractionConstants.ScannerProfilePath);
                created = true;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            bool changed = created;
            changed |= SetString(serializedProfile, "profileDisplayName", "Interaction Scanner Default");
            changed |= SetString(serializedProfile, "profileId", CCS_InteractionConstants.ScannerProfileId);
            changed |= SetString(
                serializedProfile,
                "profileDescription",
                "Default interaction scanner profile for v0.4.0 Interaction foundation.");
            changed |= SetString(serializedProfile, "profileVersion", CCS_InteractionConstants.ModuleVersion);
            changed |= SetFloat(serializedProfile, "interactionRange", 3f);
            changed |= SetFloat(serializedProfile, "interactionCooldownSeconds", 0.25f);
            changed |= SetBool(serializedProfile, "useCameraForward", true);

            SerializedProperty layerMaskProperty = serializedProfile.FindProperty("interactionLayerMask");
            if (layerMaskProperty != null && layerMaskProperty.intValue != ~0)
            {
                layerMaskProperty.intValue = ~0;
                changed = true;
            }

            if (changed)
            {
                serializedProfile.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(profile);
            }

            return changed;
        }

        public static bool EnsureToggleInteractablePrefab()
        {
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_InteractionConstants.TestToggleInteractablePrefabPath);
            if (existingPrefab != null
                && existingPrefab.GetComponent<CCS_TestToggleInteractable>() != null
                && existingPrefab.GetComponent<NetworkObject>() != null
                && existingPrefab.GetComponentInChildren<Collider>() != null)
            {
                return false;
            }

            string directory = Path.GetDirectoryName(CCS_InteractionConstants.TestToggleInteractablePrefabPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            GameObject root = new GameObject("PF_CCS_TestInteractable_ToggleCube");
            try
            {
                root.AddComponent<NetworkObject>();
                CCS_TestToggleInteractable interactable = root.AddComponent<CCS_TestToggleInteractable>();

                GameObject visualRoot = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visualRoot.name = "VisualRoot";
                visualRoot.transform.SetParent(root.transform, false);
                visualRoot.transform.localPosition = new Vector3(0f, 0.5f, 0f);
                visualRoot.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

                MeshRenderer renderer = visualRoot.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    renderer.sharedMaterial.color = new Color(0.85f, 0.2f, 0.2f, 1f);
                }

                SerializedObject serializedInteractable = new SerializedObject(interactable);
                SerializedProperty visualRootProperty = serializedInteractable.FindProperty("visualRoot");
                SerializedProperty rendererProperty = serializedInteractable.FindProperty("visualRenderer");
                if (visualRootProperty != null)
                {
                    visualRootProperty.objectReferenceValue = visualRoot.transform;
                }

                if (rendererProperty != null)
                {
                    rendererProperty.objectReferenceValue = renderer;
                }

                serializedInteractable.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, CCS_InteractionConstants.TestToggleInteractablePrefabPath);
                return true;
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
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

        #endregion
    }
}
