using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_InteractionLayerUtility
// CATEGORY: Modules / Interaction / Editor
// PURPOSE: Ensures the Interactable physics layer exists in project settings.
// PLACEMENT: Editor utility invoked from Interaction master test builders.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Uses the first available user layer slot when Interactable is missing.
// =============================================================================

namespace CCS.Modules.Interaction.Editor
{
    public static class CCS_InteractionLayerUtility
    {
        private const string TagManagerAssetPath = "ProjectSettings/TagManager.asset";

        #region Public Methods

        public static bool EnsureInteractableLayer()
        {
            if (LayerMask.NameToLayer(CCS_InteractionConstants.InteractableLayerName) >= 0)
            {
                return false;
            }

            Object[] tagManagerAssets = AssetDatabase.LoadAllAssetsAtPath(TagManagerAssetPath);
            if (tagManagerAssets == null || tagManagerAssets.Length == 0)
            {
                Debug.LogError("[Interaction Layer Utility] Could not load TagManager.asset.");
                return false;
            }

            SerializedObject tagManager = new SerializedObject(tagManagerAssets[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");
            if (layers == null || !layers.isArray)
            {
                Debug.LogError("[Interaction Layer Utility] TagManager layers property was not found.");
                return false;
            }

            for (int i = 6; i < layers.arraySize; i++)
            {
                SerializedProperty layerSlot = layers.GetArrayElementAtIndex(i);
                if (layerSlot == null || !string.IsNullOrEmpty(layerSlot.stringValue))
                {
                    continue;
                }

                layerSlot.stringValue = CCS_InteractionConstants.InteractableLayerName;
                tagManager.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssets();
                Debug.Log($"[Interaction Layer Utility] Added layer '{CCS_InteractionConstants.InteractableLayerName}' at index {i}.");
                return true;
            }

            Debug.LogError("[Interaction Layer Utility] No free user layer slot available for Interactable.");
            return false;
        }

        public static LayerMask GetInteractableLayerMask()
        {
            EnsureInteractableLayer();
            int layer = LayerMask.NameToLayer(CCS_InteractionConstants.InteractableLayerName);
            return layer >= 0 ? 1 << layer : default;
        }

        #endregion
    }
}
