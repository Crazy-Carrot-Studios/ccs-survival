using System.IO;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WeaponsFireVisualAssetBuilder
// CATEGORY: Modules / Weapons / Editor
// PURPOSE: Builds v0.6.8 cosmetic revolver fire visual prefabs and FitGuides anchors.
// PLACEMENT: Invoked from CCS_WeaponsVisualAssetBuilder.EnsureRevolverM1879VisualAssets.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Visual-only prefabs. No gameplay authority or per-shot shell ejection.
// =============================================================================

namespace CCS.Modules.Weapons.Editor
{
    public static class CCS_WeaponsFireVisualAssetBuilder
    {
        public static bool EnsureFireVisualPrefabs()
        {
            bool changed = false;
            changed |= EnsureBulletTrailMaterial();
            changed |= EnsureBulletTracerVisualPrefab();
            changed |= EnsureMuzzleFlashPrefab();
            changed |= EnsureMuzzleSmokePrefab();
            changed |= EnsureSpentShellVisualPrefab();
            return changed;
        }

        public static bool EnsureVisualFitGuideAnchors(Transform gunVisualRoot, CCS_RevolverVisualDefinition visualDefinition)
        {
            if (gunVisualRoot == null || visualDefinition == null)
            {
                return false;
            }

            bool changed = false;
            Transform fitGuides = gunVisualRoot.Find(CCS_WeaponsConstants.FitGuidesObjectName);
            if (fitGuides == null)
            {
                GameObject fitGuidesObject = new GameObject(CCS_WeaponsConstants.FitGuidesObjectName);
                fitGuides = fitGuidesObject.transform;
                fitGuides.SetParent(gunVisualRoot, false);
                changed = true;
            }

            changed |= EnsureChildTransform(
                fitGuides,
                CCS_WeaponsConstants.MuzzlePointObjectName,
                visualDefinition.MuzzleLocalPosition,
                CCS_WeaponMuzzlePointUtility.ComputeBarrelAlignedLocalEuler(visualDefinition.MuzzleLocalPosition));
            changed |= EnsureChildTransform(
                fitGuides,
                CCS_WeaponsConstants.CylinderPointObjectName,
                CCS_WeaponsConstants.DefaultCylinderLocalPosition,
                Vector3.zero);
            changed |= EnsureChildTransform(
                fitGuides,
                CCS_WeaponsConstants.ShellEjectPointObjectName,
                visualDefinition.ShellEjectLocalPosition,
                new Vector3(0f, 0f, 45f));
            return changed;
        }

        private static bool EnsureBulletTracerVisualPrefab()
        {
            GameObject prefabRoot = LoadOrCreatePrefabRoot(CCS_WeaponsConstants.RevolverM1879BulletTracerVisualPrefabPath);
            if (prefabRoot == null)
            {
                return false;
            }

            bool changed = prefabRoot.name != "PF_CCS_RevolverM1879_BulletTracerVisual";
            if (changed)
            {
                prefabRoot.name = "PF_CCS_RevolverM1879_BulletTracerVisual";
            }

            changed |= EnsureModelChild(
                prefabRoot.transform,
                "BulletVisualMesh",
                CCS_WeaponsConstants.RevolverM1879BulletModelAssetPath,
                CCS_WeaponsConstants.RevolverM1879BulletMaterialAssetPath,
                new Vector3(0.35f, 0.35f, 0.35f));
            changed |= EnsureComponent<CCS_RevolverBulletTracerVisual>(prefabRoot);
            changed |= EnsureBulletTrailRenderer(prefabRoot);
            changed |= StripColliders(prefabRoot);
            return SavePrefabIfChanged(prefabRoot, CCS_WeaponsConstants.RevolverM1879BulletTracerVisualPrefabPath, changed);
        }

        private static bool EnsureBulletTrailMaterial()
        {
            string materialPath = CCS_WeaponsConstants.RevolverM1879BulletTrailMaterialAssetPath;
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (existing != null)
            {
                return false;
            }

            string directory = Path.GetDirectoryName(materialPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                ?? Shader.Find("Particles/Standard Unlit")
                ?? Shader.Find("Sprites/Default");
            Material material = new Material(shader);
            material.name = "MAT_CCS_Revolver_BulletTrail";
            material.color = new Color(1f, 0.88f, 0.42f, 1f);
            AssetDatabase.CreateAsset(material, materialPath);
            return true;
        }

        private static bool EnsureBulletTrailRenderer(GameObject prefabRoot)
        {
            TrailRenderer trailRenderer = prefabRoot.GetComponent<TrailRenderer>();
            bool changed = false;
            if (trailRenderer == null)
            {
                trailRenderer = prefabRoot.AddComponent<TrailRenderer>();
                changed = true;
            }

            Material trailMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                CCS_WeaponsConstants.RevolverM1879BulletTrailMaterialAssetPath);
            if (trailMaterial != null && trailRenderer.sharedMaterial != trailMaterial)
            {
                trailRenderer.sharedMaterial = trailMaterial;
                changed = true;
            }

            if (trailRenderer.time != CCS_WeaponsConstants.DefaultBulletTrailLifetime)
            {
                trailRenderer.time = CCS_WeaponsConstants.DefaultBulletTrailLifetime;
                changed = true;
            }

            if (Mathf.Abs(trailRenderer.widthMultiplier - 1f) > 0.0001f)
            {
                trailRenderer.widthMultiplier = 1f;
                changed = true;
            }

            return changed;
        }

        private static bool EnsureMuzzleFlashPrefab()
        {
            GameObject prefabRoot = LoadOrCreatePrefabRoot(CCS_WeaponsConstants.RevolverM1879MuzzleFlashPrefabPath);
            if (prefabRoot == null)
            {
                return false;
            }

            bool changed = prefabRoot.name != "PF_CCS_RevolverM1879_MuzzleFlash";
            if (changed)
            {
                prefabRoot.name = "PF_CCS_RevolverM1879_MuzzleFlash";
            }

            changed |= EnsurePrimitiveChild(
                prefabRoot.transform,
                "FlashMesh",
                PrimitiveType.Sphere,
                new Color(1f, 0.85f, 0.35f, 1f),
                new Vector3(0.04f, 0.04f, 0.04f));
            changed |= EnsureComponent<CCS_RevolverMuzzleFlashVisual>(prefabRoot);
            changed |= StripColliders(prefabRoot);
            return SavePrefabIfChanged(prefabRoot, CCS_WeaponsConstants.RevolverM1879MuzzleFlashPrefabPath, changed);
        }

        private static bool EnsureMuzzleSmokePrefab()
        {
            GameObject prefabRoot = LoadOrCreatePrefabRoot(CCS_WeaponsConstants.RevolverM1879MuzzleSmokePrefabPath);
            if (prefabRoot == null)
            {
                return false;
            }

            bool changed = prefabRoot.name != "PF_CCS_RevolverM1879_MuzzleSmoke";
            if (changed)
            {
                prefabRoot.name = "PF_CCS_RevolverM1879_MuzzleSmoke";
            }

            changed |= EnsurePrimitiveChild(
                prefabRoot.transform,
                "SmokeMesh",
                PrimitiveType.Sphere,
                new Color(0.65f, 0.65f, 0.65f, 0.55f),
                new Vector3(0.03f, 0.03f, 0.03f));
            changed |= EnsureComponent<CCS_RevolverMuzzleSmokeVisual>(prefabRoot);
            changed |= StripColliders(prefabRoot);
            return SavePrefabIfChanged(prefabRoot, CCS_WeaponsConstants.RevolverM1879MuzzleSmokePrefabPath, changed);
        }

        private static bool EnsureSpentShellVisualPrefab()
        {
            GameObject prefabRoot = LoadOrCreatePrefabRoot(CCS_WeaponsConstants.RevolverM1879SpentShellVisualPrefabPath);
            if (prefabRoot == null)
            {
                return false;
            }

            bool changed = prefabRoot.name != "PF_CCS_RevolverM1879_SpentShellVisual";
            if (changed)
            {
                prefabRoot.name = "PF_CCS_RevolverM1879_SpentShellVisual";
            }

            changed |= EnsureModelChild(
                prefabRoot.transform,
                "ShellVisualMesh",
                CCS_WeaponsConstants.RevolverM1879ShellModelAssetPath,
                CCS_WeaponsConstants.RevolverM1879ShellMaterialAssetPath,
                new Vector3(1f, 1f, 1f));
            changed |= EnsureComponent<CCS_RevolverSpentShellVisual>(prefabRoot);
            changed |= StripColliders(prefabRoot);
            return SavePrefabIfChanged(prefabRoot, CCS_WeaponsConstants.RevolverM1879SpentShellVisualPrefabPath, changed);
        }

        private static bool EnsureModelChild(
            Transform parent,
            string childName,
            string modelPath,
            string materialPath,
            Vector3 localScale)
        {
            Transform existing = parent.Find(childName);
            bool changed = false;
            if (existing == null)
            {
                GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                if (modelAsset == null)
                {
                    Debug.LogError("[Weapons Fire Visual Builder] Missing model at " + modelPath);
                    return false;
                }

                GameObject instance = Object.Instantiate(modelAsset);
                instance.name = childName;
                existing = instance.transform;
                existing.SetParent(parent, false);
                changed = true;
            }

            if (existing.localScale != localScale)
            {
                existing.localScale = localScale;
                changed = true;
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material != null)
            {
                Renderer[] renderers = existing.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] != null && renderers[i].sharedMaterial != material)
                    {
                        renderers[i].sharedMaterial = material;
                        changed = true;
                    }
                }
            }

            return changed;
        }

        private static bool EnsurePrimitiveChild(
            Transform parent,
            string childName,
            PrimitiveType primitiveType,
            Color color,
            Vector3 localScale)
        {
            Transform existing = parent.Find(childName);
            bool changed = false;
            if (existing == null)
            {
                GameObject primitive = GameObject.CreatePrimitive(primitiveType);
                primitive.name = childName;
                existing = primitive.transform;
                existing.SetParent(parent, false);
                changed = true;
            }

            if (existing.localScale != localScale)
            {
                existing.localScale = localScale;
                changed = true;
            }

            Renderer renderer = existing.GetComponent<Renderer>();
            if (renderer != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                Material material = new Material(shader);
                material.color = color;
                renderer.sharedMaterial = material;
                changed = true;
            }

            Collider collider = existing.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider, true);
                changed = true;
            }

            return changed;
        }

        private static bool EnsureChildTransform(
            Transform parent,
            string childName,
            Vector3 localPosition,
            Vector3 localEuler)
        {
            Transform child = parent.Find(childName);
            bool changed = false;
            if (child == null)
            {
                GameObject childObject = new GameObject(childName);
                child = childObject.transform;
                child.SetParent(parent, false);
                changed = true;
            }

            if (child.localPosition != localPosition)
            {
                child.localPosition = localPosition;
                changed = true;
            }

            Quaternion expectedRotation = Quaternion.Euler(localEuler);
            if (child.localRotation != expectedRotation)
            {
                child.localRotation = expectedRotation;
                changed = true;
            }

            return changed;
        }

        private static bool EnsureComponent<T>(GameObject target) where T : Component
        {
            if (target.GetComponent<T>() != null)
            {
                return false;
            }

            target.AddComponent<T>();
            return true;
        }

        private static bool StripColliders(GameObject root)
        {
            bool changed = false;
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    Object.DestroyImmediate(colliders[i], true);
                    changed = true;
                }
            }

            return changed;
        }

        private static GameObject LoadOrCreatePrefabRoot(string prefabPath)
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null)
            {
                return PrefabUtility.LoadPrefabContents(prefabPath);
            }

            string directory = Path.GetDirectoryName(prefabPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            GameObject root = new GameObject(Path.GetFileNameWithoutExtension(prefabPath));
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            return PrefabUtility.LoadPrefabContents(prefabPath);
        }

        private static bool SavePrefabIfChanged(GameObject prefabRoot, string prefabPath, bool changed)
        {
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            if (changed)
            {
                AssetDatabase.ImportAsset(prefabPath);
            }

            return changed;
        }
    }
}
