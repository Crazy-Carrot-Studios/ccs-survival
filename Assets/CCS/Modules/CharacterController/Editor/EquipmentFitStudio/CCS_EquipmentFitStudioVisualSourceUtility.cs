using CCS.Modules.Weapons;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioVisualSourceUtility
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Resolves revolver preview visual source and strips gameplay components.
// PLACEMENT: Shared editor utility for preview and test attachment spawning.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Uses ModelRoot/RevolverVisual only. Editor-only visuals.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public static class CCS_EquipmentFitStudioVisualSourceUtility
    {
        #region Public Methods

        public static GameObject ResolveRevolverPreviewVisualSource(GameObject previewSourcePrefab)
        {
            if (previewSourcePrefab == null)
            {
                previewSourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    CCS_WeaponsConstants.RevolverM1879VisualOnlyPrefabPath);
            }

            if (previewSourcePrefab == null)
            {
                previewSourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    CCS_WeaponsConstants.RevolverM1879WorldPickupPrefabPath);
            }

            if (previewSourcePrefab == null)
            {
                return null;
            }

            Transform modelRoot = previewSourcePrefab.transform.Find(CCS_WeaponsConstants.RevolverModelRootObjectName);
            if (modelRoot == null)
            {
                return null;
            }

            Transform revolverVisual = modelRoot.Find(CCS_WeaponsConstants.RevolverMaterializedVisualChildName);
            return revolverVisual != null ? revolverVisual.gameObject : null;
        }

        public static GameObject SpawnEditorVisualUnderSocket(
            Transform socketTransform,
            GameObject previewSourcePrefab,
            string objectName,
            bool hideInHierarchy)
        {
            if (socketTransform == null)
            {
                return null;
            }

            GameObject visualSource = ResolveRevolverPreviewVisualSource(previewSourcePrefab);
            if (visualSource == null)
            {
                return null;
            }

            GameObject instance = Object.Instantiate(visualSource);
            instance.name = objectName;
            instance.hideFlags = hideInHierarchy
                ? HideFlags.DontSave | HideFlags.HideInHierarchy
                : HideFlags.DontSave;
            instance.transform.SetParent(socketTransform, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            StripGameplayComponents(instance);
            return instance;
        }

        public static void StripGameplayComponents(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Object.DestroyImmediate(colliders[i], true);
            }

            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null)
                {
                    Object.DestroyImmediate(behaviour, true);
                }
            }

            Animator[] animators = root.GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animators.Length; i++)
            {
                Object.DestroyImmediate(animators[i], true);
            }

            Animation[] animations = root.GetComponentsInChildren<Animation>(true);
            for (int i = 0; i < animations.Length; i++)
            {
                Object.DestroyImmediate(animations[i], true);
            }

            AudioSource[] audioSources = root.GetComponentsInChildren<AudioSource>(true);
            for (int i = 0; i < audioSources.Length; i++)
            {
                Object.DestroyImmediate(audioSources[i], true);
            }

            ParticleSystem[] particles = root.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particles.Length; i++)
            {
                Object.DestroyImmediate(particles[i], true);
            }
        }

        #endregion
    }
}
