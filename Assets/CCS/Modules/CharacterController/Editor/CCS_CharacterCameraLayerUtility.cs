using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterCameraLayerUtility
// CATEGORY: Modules / CharacterController / Editor
// PURPOSE: Ensures Player layer/tag and builds camera obstruction layer masks.
// PLACEMENT: Editor utility invoked from camera rig and player prefab builders.
// AUTHOR: James Schilz
// CREATED: 2026-06-21
// NOTES: Camera obstruction includes world geometry only, not player/interaction/UI layers.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterCameraLayerUtility
    {
        private const string TagManagerAssetPath = "ProjectSettings/TagManager.asset";

        private static readonly string[] ExcludedObstructionLayerNames =
        {
            "TransparentFX",
            "Ignore Raycast",
            "Water",
            "UI",
            CCS_CharacterControllerConstants.InteractableLayerName,
            CCS_CharacterControllerConstants.PlayerLayerName,
        };

        #region Public Methods

        public static bool EnsurePlayerLayerAndTag()
        {
            bool changed = EnsurePlayerTag();
            changed |= EnsurePlayerLayer();
            changed |= EnsureLocalSelfHeadHiddenLayer();
            changed |= EnsureLocalFirstPersonBodyLayer();
            return changed;
        }

        public static bool EnsureLocalFirstPersonBodyLayer()
        {
            if (LayerMask.NameToLayer(CCS_CharacterControllerConstants.LocalFirstPersonBodyLayerName) >= 0)
            {
                return false;
            }

            return EnsureUserLayer(CCS_CharacterControllerConstants.LocalFirstPersonBodyLayerName);
        }

        public static bool EnsureLocalSelfHeadHiddenLayer()
        {
            if (LayerMask.NameToLayer(CCS_CharacterControllerConstants.LocalSelfHeadHiddenLayerName) >= 0)
            {
                return false;
            }

            return EnsureUserLayer(CCS_CharacterControllerConstants.LocalSelfHeadHiddenLayerName);
        }

        public static LayerMask GetCameraObstructionLayerMask()
        {
            EnsurePlayerLayerAndTag();

            int mask = 0;
            int defaultLayer = LayerMask.NameToLayer("Default");
            if (defaultLayer >= 0)
            {
                mask |= 1 << defaultLayer;
            }

            return mask;
        }

        public static bool IsEverythingLayerMask(LayerMask layerMask)
        {
            return layerMask.value == -1;
        }

        public static bool LayerMaskIncludesExcludedLayers(LayerMask layerMask)
        {
            for (int i = 0; i < ExcludedObstructionLayerNames.Length; i++)
            {
                int layer = LayerMask.NameToLayer(ExcludedObstructionLayerNames[i]);
                if (layer >= 0 && (layerMask.value & (1 << layer)) != 0)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Private Methods

        private static bool EnsurePlayerTag()
        {
            Object[] tagManagerAssets = AssetDatabase.LoadAllAssetsAtPath(TagManagerAssetPath);
            if (tagManagerAssets == null || tagManagerAssets.Length == 0)
            {
                return false;
            }

            SerializedObject tagManager = new SerializedObject(tagManagerAssets[0]);
            SerializedProperty tags = tagManager.FindProperty("tags");
            if (tags == null || !tags.isArray)
            {
                return false;
            }

            for (int i = 0; i < tags.arraySize; i++)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == CCS_CharacterControllerConstants.PlayerTag)
                {
                    return false;
                }
            }

            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue =
                CCS_CharacterControllerConstants.PlayerTag;
            tagManager.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            return true;
        }

        private static bool EnsurePlayerLayer()
        {
            if (LayerMask.NameToLayer(CCS_CharacterControllerConstants.PlayerLayerName) >= 0)
            {
                return false;
            }

            return EnsureUserLayer(CCS_CharacterControllerConstants.PlayerLayerName);
        }

        private static bool EnsureUserLayer(string layerName)
        {
            Object[] tagManagerAssets = AssetDatabase.LoadAllAssetsAtPath(TagManagerAssetPath);
            if (tagManagerAssets == null || tagManagerAssets.Length == 0)
            {
                return false;
            }

            SerializedObject tagManager = new SerializedObject(tagManagerAssets[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");
            if (layers == null || !layers.isArray)
            {
                return false;
            }

            for (int i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty layerSlot = layers.GetArrayElementAtIndex(i);
                if (layerSlot == null || !string.IsNullOrEmpty(layerSlot.stringValue))
                {
                    continue;
                }

                layerSlot.stringValue = layerName;
                tagManager.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssets();
                return true;
            }

            Debug.LogError("[Camera Layer Utility] No free user layer slot available for " + layerName + ".");
            return false;
        }

        #endregion
    }
}
