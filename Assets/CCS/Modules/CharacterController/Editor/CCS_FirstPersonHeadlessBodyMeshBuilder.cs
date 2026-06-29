using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FirstPersonHeadlessBodyMeshBuilder
// CATEGORY: Modules / CharacterController / Editor
// PURPOSE: Bakes a CCS-owned headless first-person body mesh from combined CC3 body sources.
// PLACEMENT: Editor utility invoked from player prefab builder and validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-24
// NOTES: Never modifies vendor meshes. Removes head/face triangles by bone weight and height trim.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_FirstPersonHeadlessBodyMeshBuilder
    {
        public const float DefaultHeadBoneWeightThreshold = 0.45f;
        public const float DefaultProtectedLimbWeightThreshold = 0.35f;
        public const float DefaultNeckTrimLocalHeight = 0.02f;

        #region Public Methods

        public static bool EnsureHeadlessBodyMeshAsset()
        {
            CCS_CharacterCameraLayerUtility.EnsurePlayerLayerAndTag();
            CCS_CharacterCameraLayerUtility.EnsureLocalFirstPersonBodyLayer();

            GameObject bakeInstance = null;
            SkinnedMeshRenderer sourceRenderer = FindCombinedBodyRendererForBake(out bakeInstance);
            if (sourceRenderer == null)
            {
                Debug.LogWarning("[Headless Body Mesh] No combined CC_Game_Body SkinnedMeshRenderer found to bake.");
                return false;
            }

            try
            {
                CCS_FirstPersonHeadlessMeshStats stats = BakeHeadlessMesh(
                    sourceRenderer,
                    DefaultHeadBoneWeightThreshold,
                    DefaultProtectedLimbWeightThreshold,
                    DefaultNeckTrimLocalHeight);

                if (stats.RemainingTriangleCount <= 0)
                {
                    Debug.LogError("[Headless Body Mesh] Bake produced zero triangles. Aborting asset write.");
                    return false;
                }

                Debug.Log(
                    "[Headless Body Mesh] Baked "
                    + stats.RemainingTriangleCount
                    + "/"
                    + stats.OriginalTriangleCount
                    + " triangles (removed "
                    + stats.RemovedTriangleCountComputed
                    + ") from "
                    + stats.SourceRendererName
                    + " -> "
                    + stats.MeshAssetPath);

                return true;
            }
            finally
            {
                if (bakeInstance != null)
                {
                    Object.DestroyImmediate(bakeInstance);
                }
            }
        }

        public static SkinnedMeshRenderer FindCombinedBodyRendererForBake(out GameObject temporaryInstance)
        {
            temporaryInstance = null;

            string[] candidateAssetPaths =
            {
                CCS_CharacterControllerConstants.PlayerModelKevinPrefabPath,
                CCS_CharacterControllerConstants.KevinImportPrefabPath,
                CCS_CharacterControllerConstants.KevinFbxPath,
                CCS_CharacterControllerConstants.Cc3BasePlusPrefabPath,
                CCS_CharacterControllerConstants.Cc3BasePlusBodyFbxPath,
                CCS_CharacterControllerConstants.PlayerVisualPrefabPath,
                CCS_CharacterControllerConstants.TestPrefabPath,
            };

            for (int pathIndex = 0; pathIndex < candidateAssetPaths.Length; pathIndex++)
            {
                string assetPath = candidateAssetPaths[pathIndex];
                if (string.IsNullOrEmpty(assetPath))
                {
                    continue;
                }

                GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefabAsset == null)
                {
                    continue;
                }

                GameObject bakeRoot = PrefabUtility.InstantiatePrefab(prefabAsset) as GameObject;
                if (bakeRoot == null)
                {
                    bakeRoot = Object.Instantiate(prefabAsset);
                }

                bakeRoot.name = "CCS_HeadlessMeshBake_Temp";
                bakeRoot.hideFlags = HideFlags.HideAndDontSave;

                SkinnedMeshRenderer renderer = FindCombinedBodyRendererInHierarchy(bakeRoot.transform);
                if (renderer == null)
                {
                    UnpackNestedPrefabInstances(bakeRoot);
                    renderer = FindCombinedBodyRendererInHierarchy(bakeRoot.transform);
                }
                if (renderer != null)
                {
                    temporaryInstance = bakeRoot;
                    return renderer;
                }

                Object.DestroyImmediate(bakeRoot);
            }

            return null;
        }

        private static void UnpackNestedPrefabInstances(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            List<GameObject> prefabInstanceRoots = new List<GameObject>();
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform current = transforms[i];
                if (current == null || !PrefabUtility.IsPartOfPrefabInstance(current.gameObject))
                {
                    continue;
                }

                GameObject instanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot(current.gameObject);
                if (instanceRoot == null || prefabInstanceRoots.Contains(instanceRoot))
                {
                    continue;
                }

                prefabInstanceRoots.Add(instanceRoot);
            }

            for (int i = 0; i < prefabInstanceRoots.Count; i++)
            {
                GameObject instanceRoot = prefabInstanceRoots[i];
                if (instanceRoot == null || !PrefabUtility.IsPartOfPrefabInstance(instanceRoot))
                {
                    continue;
                }

                try
                {
                    PrefabUtility.UnpackPrefabInstance(
                        instanceRoot,
                        PrefabUnpackMode.Completely,
                        InteractionMode.AutomatedAction);
                }
                catch (System.ArgumentException)
                {
                    // Some Reallusion import hierarchies cannot be completely unpacked during temp bake instances.
                }
            }
        }

        public static SkinnedMeshRenderer FindCombinedBodyRendererInProject()
        {
            GameObject temporaryInstance;
            return FindCombinedBodyRendererForBake(out temporaryInstance);
        }

        public static CCS_FirstPersonHeadlessMeshStats BakeHeadlessMesh(
            SkinnedMeshRenderer sourceRenderer,
            float headBoneWeightThreshold,
            float protectedLimbWeightThreshold,
            float neckTrimLocalHeight)
        {
            CCS_FirstPersonHeadlessMeshStats stats = new CCS_FirstPersonHeadlessMeshStats
            {
                MeshAssetPath = CCS_CharacterControllerConstants.FirstPersonHeadlessBodyMeshAssetPath,
                SourceRendererName = sourceRenderer != null ? sourceRenderer.gameObject.name : string.Empty,
            };

            if (sourceRenderer == null || sourceRenderer.sharedMesh == null)
            {
                return stats;
            }

            Mesh sourceMesh = sourceRenderer.sharedMesh;
            Transform[] bones = sourceRenderer.bones;
            Transform headBone = ResolveHeadBone(bones);

            HashSet<int> headBoneIndices = BuildHeadBoneIndices(bones);
            HashSet<int> protectedBoneIndices = BuildProtectedLimbBoneIndices(bones);

            Mesh headlessMesh = BuildHeadlessMeshCopy(
                sourceMesh,
                bones,
                headBone,
                headBoneIndices,
                protectedBoneIndices,
                headBoneWeightThreshold,
                protectedLimbWeightThreshold,
                neckTrimLocalHeight,
                ref stats);

            if (headlessMesh == null)
            {
                return stats;
            }

            EnsureMeshFolderExists();
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(stats.MeshAssetPath);
            if (existing != null)
            {
                EditorUtility.CopySerialized(headlessMesh, existing);
                EditorUtility.SetDirty(existing);
                Object.DestroyImmediate(headlessMesh);
            }
            else
            {
                AssetDatabase.CreateAsset(headlessMesh, stats.MeshAssetPath);
            }

            AssetDatabase.SaveAssets();
            return stats;
        }

        public static SkinnedMeshRenderer FindCombinedBodyRendererInHierarchy(Transform root)
        {
            if (root == null)
            {
                return null;
            }

            SkinnedMeshRenderer[] renderers = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                SkinnedMeshRenderer renderer = renderers[i];
                if (renderer == null || IsHeadlessBodyRenderer(renderer))
                {
                    continue;
                }

                if (IsCombinedBodyRendererCandidate(renderer))
                {
                    return renderer;
                }
            }

            return null;
        }

        private static bool IsHeadlessBodyRenderer(SkinnedMeshRenderer renderer)
        {
            if (renderer == null)
            {
                return true;
            }

            string objectName = NormalizeToken(renderer.gameObject.name);
            if (objectName.Contains("firstpersonheadlessbody"))
            {
                return true;
            }

            Mesh mesh = renderer.sharedMesh;
            return mesh != null
                && NormalizeToken(mesh.name).Contains("firstpersonheadlessbody");
        }

        private static bool IsCombinedBodyRendererCandidate(SkinnedMeshRenderer renderer)
        {
            if (renderer == null)
            {
                return false;
            }

            if (IsCombinedBodyRendererName(renderer.gameObject.name))
            {
                return true;
            }

            Mesh mesh = renderer.sharedMesh;
            return mesh != null && IsCombinedBodyRendererName(mesh.name);
        }

        public static bool IsCombinedBodyRendererName(string rendererName)
        {
            string normalized = NormalizeToken(rendererName);
            return normalized.Contains("ccgamebody")
                || (normalized.Contains("cc3") && normalized.Contains("body"))
                || normalized == "body"
                || normalized.Contains("capsulevisual");
        }

        #endregion

        #region Private Methods

        private static void EnsureMeshFolderExists()
        {
            string folder = CCS_CharacterControllerConstants.FirstPersonHeadlessBodyMeshFolderPath;
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            string leaf = Path.GetFileName(folder);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                string grandParent = Path.GetDirectoryName(parent)?.Replace('\\', '/');
                string parentLeaf = Path.GetFileName(parent);
                if (!string.IsNullOrEmpty(grandParent))
                {
                    AssetDatabase.CreateFolder(grandParent, parentLeaf);
                }
            }

            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(leaf))
            {
                AssetDatabase.CreateFolder(parent, leaf);
            }
        }

        private static Mesh BuildHeadlessMeshCopy(
            Mesh sourceMesh,
            Transform[] bones,
            Transform headBone,
            HashSet<int> headBoneIndices,
            HashSet<int> protectedBoneIndices,
            float headBoneWeightThreshold,
            float protectedLimbWeightThreshold,
            float neckTrimLocalHeight,
            ref CCS_FirstPersonHeadlessMeshStats stats)
        {
            Mesh result = new Mesh
            {
                name = CCS_CharacterControllerConstants.FirstPersonHeadlessBodyMeshAssetName,
            };

            result.vertices = sourceMesh.vertices;
            result.normals = sourceMesh.normals;
            result.tangents = sourceMesh.tangents;
            result.uv = sourceMesh.uv;
            result.uv2 = sourceMesh.uv2;
            result.uv3 = sourceMesh.uv3;
            result.uv4 = sourceMesh.uv4;
            result.colors = sourceMesh.colors;
            result.boneWeights = sourceMesh.boneWeights;
            result.bindposes = sourceMesh.bindposes;
            result.indexFormat = sourceMesh.indexFormat;

            int originalTriangleCount = 0;
            int remainingTriangleCount = 0;
            int subMeshCount = sourceMesh.subMeshCount;
            result.subMeshCount = subMeshCount;

            for (int subMesh = 0; subMesh < subMeshCount; subMesh++)
            {
                int[] sourceTriangles = sourceMesh.GetTriangles(subMesh);
                originalTriangleCount += sourceTriangles.Length;

                List<int> keptTriangles = new List<int>(sourceTriangles.Length);
                for (int t = 0; t < sourceTriangles.Length; t += 3)
                {
                    int i0 = sourceTriangles[t];
                    int i1 = sourceTriangles[t + 1];
                    int i2 = sourceTriangles[t + 2];
                    if (ShouldRemoveTriangle(
                            i0,
                            i1,
                            i2,
                            sourceMesh.boneWeights,
                            sourceMesh.vertices,
                            bones,
                            headBone,
                            headBoneIndices,
                            protectedBoneIndices,
                            headBoneWeightThreshold,
                            protectedLimbWeightThreshold,
                            neckTrimLocalHeight))
                    {
                        continue;
                    }

                    keptTriangles.Add(i0);
                    keptTriangles.Add(i1);
                    keptTriangles.Add(i2);
                }

                remainingTriangleCount += keptTriangles.Count;
                result.SetTriangles(keptTriangles, subMesh, false);
            }

            result.RecalculateBounds();
            stats.OriginalTriangleCount = originalTriangleCount;
            stats.RemainingTriangleCount = remainingTriangleCount;
            stats.RemovedTriangleCount = originalTriangleCount - remainingTriangleCount;
            return result;
        }

        private static bool ShouldRemoveTriangle(
            int i0,
            int i1,
            int i2,
            BoneWeight[] boneWeights,
            Vector3[] vertices,
            Transform[] bones,
            Transform headBone,
            HashSet<int> headBoneIndices,
            HashSet<int> protectedBoneIndices,
            float headBoneWeightThreshold,
            float protectedLimbWeightThreshold,
            float neckTrimLocalHeight)
        {
            float protectedWeight = (
                GetBoneSetWeight(boneWeights[i0], protectedBoneIndices)
                + GetBoneSetWeight(boneWeights[i1], protectedBoneIndices)
                + GetBoneSetWeight(boneWeights[i2], protectedBoneIndices))
                / 3f;
            if (protectedWeight >= protectedLimbWeightThreshold)
            {
                return false;
            }

            float headWeight = (
                GetBoneSetWeight(boneWeights[i0], headBoneIndices)
                + GetBoneSetWeight(boneWeights[i1], headBoneIndices)
                + GetBoneSetWeight(boneWeights[i2], headBoneIndices))
                / 3f;
            if (headWeight >= headBoneWeightThreshold)
            {
                return true;
            }

            if (headBone == null)
            {
                return false;
            }

            float highestLocalY = float.NegativeInfinity;
            AccumulateHighestHeadLocalY(vertices[i0], boneWeights[i0], bones, headBone, ref highestLocalY);
            AccumulateHighestHeadLocalY(vertices[i1], boneWeights[i1], bones, headBone, ref highestLocalY);
            AccumulateHighestHeadLocalY(vertices[i2], boneWeights[i2], bones, headBone, ref highestLocalY);

            return highestLocalY >= neckTrimLocalHeight;
        }

        private static void AccumulateHighestHeadLocalY(
            Vector3 vertex,
            BoneWeight boneWeight,
            Transform[] bones,
            Transform headBone,
            ref float highestLocalY)
        {
            SampleWeightedHeadLocalY(vertex, boneWeight.boneIndex0, boneWeight.weight0, bones, headBone, ref highestLocalY);
            SampleWeightedHeadLocalY(vertex, boneWeight.boneIndex1, boneWeight.weight1, bones, headBone, ref highestLocalY);
            SampleWeightedHeadLocalY(vertex, boneWeight.boneIndex2, boneWeight.weight2, bones, headBone, ref highestLocalY);
            SampleWeightedHeadLocalY(vertex, boneWeight.boneIndex3, boneWeight.weight3, bones, headBone, ref highestLocalY);
        }

        private static void SampleWeightedHeadLocalY(
            Vector3 vertex,
            int boneIndex,
            float weight,
            Transform[] bones,
            Transform headBone,
            ref float highestLocalY)
        {
            if (weight <= 0.001f || boneIndex < 0 || boneIndex >= bones.Length || bones[boneIndex] == null)
            {
                return;
            }

            Vector3 world = bones[boneIndex].TransformPoint(vertex);
            float localY = headBone.InverseTransformPoint(world).y;
            if (localY > highestLocalY)
            {
                highestLocalY = localY;
            }
        }

        private static float GetBoneSetWeight(BoneWeight boneWeight, HashSet<int> boneIndices)
        {
            float total = 0f;
            if (boneIndices.Contains(boneWeight.boneIndex0))
            {
                total += boneWeight.weight0;
            }

            if (boneIndices.Contains(boneWeight.boneIndex1))
            {
                total += boneWeight.weight1;
            }

            if (boneIndices.Contains(boneWeight.boneIndex2))
            {
                total += boneWeight.weight2;
            }

            if (boneIndices.Contains(boneWeight.boneIndex3))
            {
                total += boneWeight.weight3;
            }

            return total;
        }

        private static HashSet<int> BuildHeadBoneIndices(Transform[] bones)
        {
            HashSet<int> indices = new HashSet<int>();
            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i] == null)
                {
                    continue;
                }

                if (IsHeadRemovalBoneName(bones[i].name))
                {
                    indices.Add(i);
                }
            }

            return indices;
        }

        private static HashSet<int> BuildProtectedLimbBoneIndices(Transform[] bones)
        {
            HashSet<int> indices = new HashSet<int>();
            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i] == null)
                {
                    continue;
                }

                if (IsProtectedLimbBoneName(bones[i].name))
                {
                    indices.Add(i);
                }
            }

            return indices;
        }

        private static Transform ResolveHeadBone(Transform[] bones)
        {
            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i] != null && NormalizeToken(bones[i].name) == "head")
                {
                    return bones[i];
                }
            }

            return null;
        }

        private static bool IsHeadRemovalBoneName(string boneName)
        {
            string normalized = NormalizeToken(boneName);
            if (string.IsNullOrEmpty(normalized))
            {
                return false;
            }

            if (IsProtectedLimbBoneName(boneName))
            {
                return false;
            }

            return ContainsAny(normalized, HeadRemovalBoneTokens);
        }

        private static bool IsProtectedLimbBoneName(string boneName)
        {
            string normalized = NormalizeToken(boneName);
            return ContainsAny(normalized, ProtectedLimbBoneTokens);
        }

        private static string NormalizeToken(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.ToLowerInvariant().Replace("_", string.Empty).Replace(" ", string.Empty);
        }

        private static bool ContainsAny(string normalizedValue, string[] tokens)
        {
            for (int i = 0; i < tokens.Length; i++)
            {
                if (normalizedValue.Contains(tokens[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static readonly string[] HeadRemovalBoneTokens =
        {
            "head",
            "neck",
            "jaw",
            "eye",
            "tongue",
            "teeth",
            "tooth",
            "facial",
            "upperteeth",
            "lowerteeth",
            "ear",
            "scalp",
            "brow",
            "eyelash",
            "ccbasehead",
        };

        private static readonly string[] ProtectedLimbBoneTokens =
        {
            "upperarm",
            "lowerarm",
            "hand",
            "clavicle",
            "shoulder",
            "finger",
            "thumb",
            "index",
            "middle",
            "ring",
            "pinky",
        };

        #endregion
    }
}
