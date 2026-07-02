using System.IO;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.Weapons;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverAimTargetResolverBuilder
// CATEGORY: Modules / CharacterController / Editor / Aiming
// PURPOSE: Ensures aim target profile asset and player prefab resolver wiring.
// PLACEMENT: Editor builder invoked from validation batches.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_RevolverAimTargetResolverBuilder
    {
        public static bool EnsureRevolverAimTargetResolver()
        {
            bool changed = EnsureProfileAsset();
            changed |= EnsurePlayerPrefabWiring();
            if (changed)
            {
                AssetDatabase.SaveAssets();
            }

            return changed;
        }

        public static bool EnsureProfileAsset()
        {
            string profilePath = CCS_CharacterControllerConstants.RevolverAimTargetProfilePath;
            string directory = Path.GetDirectoryName(profilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            CCS_RevolverAimTargetProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_RevolverAimTargetProfile>(profilePath);
            if (profile != null)
            {
                return false;
            }

            profile = ScriptableObject.CreateInstance<CCS_RevolverAimTargetProfile>();
            AssetDatabase.CreateAsset(profile, profilePath);
            return true;
        }

        public static bool EnsurePlayerPrefabWiring()
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError("[Revolver Aim Target Resolver Builder] Missing networked player prefab.");
                return false;
            }

            CCS_RevolverAimTargetProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_RevolverAimTargetProfile>(
                    CCS_CharacterControllerConstants.RevolverAimTargetProfilePath);
            if (profile == null)
            {
                return false;
            }

            GameObject instance = PrefabUtility.LoadPrefabContents(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (instance == null)
            {
                return false;
            }

            bool changed = false;
            try
            {
                Transform modelRoot = CCS_PlayerModelRootUtility.FindModelRoot(instance.transform);
                if (modelRoot == null)
                {
                    Debug.LogError("[Revolver Aim Target Resolver Builder] Missing Model root.");
                    return false;
                }

                Transform aimingRoot = EnsureAimingBranch(modelRoot);
                if (aimingRoot == null)
                {
                    return false;
                }

                CCS_RevolverAimTargetResolver[] allResolvers =
                    instance.GetComponentsInChildren<CCS_RevolverAimTargetResolver>(true);
                CCS_RevolverAimTargetResolver resolver = null;
                for (int i = 0; i < allResolvers.Length; i++)
                {
                    if (allResolvers[i] != null && allResolvers[i].gameObject == aimingRoot.gameObject)
                    {
                        resolver = allResolvers[i];
                        break;
                    }
                }

                if (resolver == null)
                {
                    resolver = aimingRoot.gameObject.AddComponent<CCS_RevolverAimTargetResolver>();
                    changed = true;
                }

                for (int i = 0; i < allResolvers.Length; i++)
                {
                    if (allResolvers[i] != null && allResolvers[i] != resolver)
                    {
                        Object.DestroyImmediate(allResolvers[i], true);
                        changed = true;
                    }
                }

                SerializedObject serializedResolver = new SerializedObject(resolver);
                changed |= SetObjectReference(serializedResolver, "aimTargetProfile", profile);
                changed |= WireAimCameraReference(serializedResolver, instance.transform);
                if (changed)
                {
                    serializedResolver.ApplyModifiedPropertiesWithoutUndo();
                }

                changed |= WireReticleAimTargetSource(instance.transform, resolver);

                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(instance, CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(instance);
            }

            return changed;
        }

        private static Transform EnsureAimingBranch(Transform modelRoot)
        {
            Transform aimingRoot = modelRoot.Find(CCS_CharacterControllerConstants.RevolverAimTargetResolverObjectName);
            if (aimingRoot != null)
            {
                return aimingRoot;
            }

            GameObject aimingObject = new GameObject(CCS_CharacterControllerConstants.RevolverAimTargetResolverObjectName);
            aimingRoot = aimingObject.transform;
            aimingRoot.SetParent(modelRoot, false);
            aimingRoot.localPosition = Vector3.zero;
            aimingRoot.localRotation = Quaternion.identity;
            aimingRoot.localScale = Vector3.one;
            return aimingRoot;
        }

        private static bool WireAimCameraReference(SerializedObject serializedResolver, Transform playerRoot)
        {
            SerializedProperty cameraProperty = serializedResolver.FindProperty("aimCamera");
            if (cameraProperty == null)
            {
                return false;
            }

            Camera[] cameras = playerRoot.GetComponentsInChildren<Camera>(true);
            Camera preferredCamera = null;
            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i] != null && cameras[i].name.Contains("FirstPerson"))
                {
                    preferredCamera = cameras[i];
                    break;
                }
            }

            if (preferredCamera == null && cameras.Length > 0)
            {
                preferredCamera = cameras[0];
            }

            if (cameraProperty.objectReferenceValue == preferredCamera)
            {
                return false;
            }

            cameraProperty.objectReferenceValue = preferredCamera;
            return true;
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

        private static bool WireReticleAimTargetSource(Transform playerRoot, CCS_RevolverAimTargetResolver resolver)
        {
            if (resolver == null)
            {
                return false;
            }

            CCS_MuzzleDrivenReticleController reticleController =
                playerRoot.GetComponentInChildren<CCS_MuzzleDrivenReticleController>(true);
            if (reticleController == null)
            {
                return false;
            }

            SerializedObject serializedReticle = new SerializedObject(reticleController);
            SerializedProperty sourceProperty = serializedReticle.FindProperty("aimTargetSourceComponent");
            if (sourceProperty == null)
            {
                return false;
            }

            if (sourceProperty.objectReferenceValue == resolver)
            {
                return false;
            }

            sourceProperty.objectReferenceValue = resolver;
            serializedReticle.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }
    }
}
