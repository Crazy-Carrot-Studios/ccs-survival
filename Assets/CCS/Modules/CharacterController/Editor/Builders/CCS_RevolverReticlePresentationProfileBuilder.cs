using System.IO;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.Weapons;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverReticlePresentationProfileBuilder
// CATEGORY: Modules / CharacterController / Editor / Builders
// PURPOSE: Ensures revolver reticle presentation profile asset and player wiring.
// PLACEMENT: Editor builder invoked from validation batches and master test setup.
// AUTHOR: James Schilz
// CREATED: 2026-06-30
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_RevolverReticlePresentationProfileBuilder
    {
        public static bool EnsureRevolverReticlePresentationProfile()
        {
            bool changed = EnsureProfileAsset();
            changed |= EnsureProfileDefaults();
            changed |= EnsurePlayerPrefabWiring();
            if (changed)
            {
                AssetDatabase.SaveAssets();
            }

            return changed;
        }

        public static bool EnsureProfileAsset()
        {
            string profilePath = CCS_CharacterControllerConstants.RevolverReticlePresentationProfilePath;
            string directory = Path.GetDirectoryName(profilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            CCS_RevolverReticlePresentationProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_RevolverReticlePresentationProfile>(profilePath);
            if (profile != null)
            {
                return false;
            }

            profile = ScriptableObject.CreateInstance<CCS_RevolverReticlePresentationProfile>();
            AssetDatabase.CreateAsset(profile, profilePath);
            return true;
        }

        public static bool EnsureProfileDefaults()
        {
            CCS_RevolverReticlePresentationProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_RevolverReticlePresentationProfile>(
                    CCS_CharacterControllerConstants.RevolverReticlePresentationProfilePath);
            if (profile == null)
            {
                return false;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            bool changed = false;
            changed |= SetEnum(serializedProfile, "reticleRevealSource", (int)CCS_RevolverReticleRevealSource.AnimationEvent);
            changed |= SetBool(serializedProfile, "revealDuringDraw", false);
            if (changed)
            {
                serializedProfile.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(profile);
            }

            return changed;
        }

        public static bool EnsurePlayerPrefabWiring()
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError("[Revolver Reticle Presentation Profile Builder] Missing networked player prefab.");
                return false;
            }

            CCS_RevolverReticlePresentationProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_RevolverReticlePresentationProfile>(
                    CCS_CharacterControllerConstants.RevolverReticlePresentationProfilePath);
            if (profile == null)
            {
                return false;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefabRoot) as GameObject;
            if (instance == null)
            {
                return false;
            }

            bool changed = false;
            try
            {
                Transform modelRoot = CCS_PlayerModelRootUtility.FindModelRoot(instance.transform);
                CCS_SingleRevolverAimAnimator aimAnimator = modelRoot != null
                    ? modelRoot.GetComponent<CCS_SingleRevolverAimAnimator>()
                    : null;
                CCS_MuzzleDrivenReticleController reticleController =
                    instance.GetComponentInChildren<CCS_MuzzleDrivenReticleController>(true);

                if (aimAnimator != null)
                {
                    SerializedObject serializedAnimator = new SerializedObject(aimAnimator);
                    changed |= SetObjectReference(serializedAnimator, "reticlePresentationProfile", profile);
                }

                if (reticleController != null)
                {
                    SerializedObject serializedReticle = new SerializedObject(reticleController);
                    changed |= SetObjectReference(serializedReticle, "reticlePresentationProfile", profile);
                    if (aimAnimator != null)
                    {
                        changed |= SetObjectReference(
                            serializedReticle,
                            "aimPresentationReadinessSourceComponent",
                            aimAnimator);
                    }
                }

                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(instance, CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
                }
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }

            return changed;
        }

        private static bool SetObjectReference(SerializedObject serializedObject, string propertyName, Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == value)
            {
                return false;
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
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
    }
}
