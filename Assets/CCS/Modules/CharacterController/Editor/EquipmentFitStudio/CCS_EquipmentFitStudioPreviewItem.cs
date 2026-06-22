using CCS.Modules.Weapons;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioPreviewItem
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Spawns editor-only zeroed preview visuals under equipment sockets.
// PLACEMENT: Editor utility used by Equipment Fit Studio window.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Uses ModelRoot/RevolverVisual only. Never saved to scene or prefab.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public sealed class CCS_EquipmentFitStudioPreviewItem
    {
        #region Variables

        private GameObject previewRoot;

        #endregion

        #region Properties

        public GameObject PreviewRoot => previewRoot;

        public bool IsSpawned => previewRoot != null;

        public bool IsZeroed =>
            previewRoot != null
            && previewRoot.transform.localPosition == Vector3.zero
            && previewRoot.transform.localRotation == Quaternion.identity
            && previewRoot.transform.localScale == Vector3.one;

        #endregion

        #region Public Methods

        public bool SpawnUnderSocket(Transform socketTransform, GameObject previewSourcePrefab)
        {
            DestroyPreview();
            if (socketTransform == null)
            {
                return false;
            }

            GameObject visualSource = ResolvePreviewVisualSource(previewSourcePrefab);
            if (visualSource == null)
            {
                Debug.LogWarning("[Equipment Fit Studio] Could not resolve ModelRoot/RevolverVisual preview source.");
                return false;
            }

            previewRoot = Object.Instantiate(visualSource);
            previewRoot.name = CCS_EquipmentConstants.EditorPreviewItemObjectName;
            previewRoot.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
            previewRoot.transform.SetParent(socketTransform, false);
            StripGameplayComponents(previewRoot);
            ResetPreviewItemToZero();
            return true;
        }

        public void ResetPreviewItemToZero()
        {
            if (previewRoot == null)
            {
                return;
            }

            previewRoot.transform.localPosition = Vector3.zero;
            previewRoot.transform.localRotation = Quaternion.identity;
            previewRoot.transform.localScale = Vector3.one;
        }

        public void EnforceZeroedTransform()
        {
            if (previewRoot == null)
            {
                return;
            }

            if (!IsZeroed)
            {
                ResetPreviewItemToZero();
            }
        }

        public void DestroyPreview()
        {
            if (previewRoot != null)
            {
                Object.DestroyImmediate(previewRoot);
                previewRoot = null;
            }
        }

        #endregion

        #region Private Methods

        private static GameObject ResolvePreviewVisualSource(GameObject previewSourcePrefab)
        {
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

        private static void StripGameplayComponents(GameObject root)
        {
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
