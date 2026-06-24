using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_LocalFirstPersonHeadVisibility
// CATEGORY: Modules / CharacterController / Runtime / Components
// PURPOSE: Local-only FP head hiding via layer mask or combined-body headless mesh fallback.
// PLACEMENT: Player prefab root. Auto-discovers separated head renderers and combined body sources.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.15 adds CCS_FirstPersonHeadlessBody local substitution for CC_Game_Body. Never disables renderers globally.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public sealed class CCS_LocalFirstPersonHeadVisibility : MonoBehaviour
    {
        #region Variables

        [SerializeField] private Animator characterAnimator;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Renderer[] additionalHeadRenderers;
        [SerializeField] private Renderer[] excludedRenderers;
        [SerializeField] private SkinnedMeshRenderer combinedBodyRenderer;
        [SerializeField] private SkinnedMeshRenderer headlessBodyRenderer;
        [SerializeField] private Mesh headlessBodyMesh;
        [SerializeField] private string headlessMeshAssetPath = CCS_CharacterControllerConstants.FirstPersonHeadlessBodyMeshAssetPath;
        [SerializeField] private int originalTriangleCount;
        [SerializeField] private int headlessTriangleCount;

        private readonly List<Renderer> discoveredHeadRenderers = new List<Renderer>();
        private readonly List<string> hiddenRendererNames = new List<string>();
        private readonly List<int> cachedOriginalLayers = new List<int>();
        private readonly List<string> incorrectHiddenRendererNames = new List<string>();

        private Transform headBone;
        private NetworkObject networkObject;
        private CCS_LocalFirstPersonHeadMaskMode resolvedMaskMode = CCS_LocalFirstPersonHeadMaskMode.None;
        private CCS_LocalFirstPersonHeadMaskMode activeMaskMode = CCS_LocalFirstPersonHeadMaskMode.None;
        private bool maskActive;
        private int combinedBodyOriginalLayer = -1;
        private bool combinedBodyCulledFromSelfCamera;
        private string combinedBodyRendererName = string.Empty;

        #endregion

        #region Properties

        public bool IsMaskActive => maskActive;

        public bool IsLocalOwner => ResolveIsLocalOwner();

        public CCS_LocalFirstPersonHeadMaskMode ResolvedMaskMode => resolvedMaskMode;

        public CCS_LocalFirstPersonHeadMaskMode ActiveMaskMode => activeMaskMode;

        public int HeadRendererCount => discoveredHeadRenderers.Count;

        public bool HasCombinedBodyFallback => resolvedMaskMode == CCS_LocalFirstPersonHeadMaskMode.CombinedBodyHeadlessFallback;

        public bool IsHeadlessBodyRendererActive =>
            headlessBodyRenderer != null && headlessBodyRenderer.enabled;

        public string CombinedBodyRendererName => combinedBodyRendererName;

        public string HeadlessMeshAssetPath => headlessMeshAssetPath;

        public int OriginalTriangleCount => originalTriangleCount;

        public int HeadlessTriangleCount => headlessTriangleCount;

        public bool HasIncorrectWeaponOrHandHidden => incorrectHiddenRendererNames.Count > 0;

        public bool OriginalFullBodyCulledFromSelfCamera => combinedBodyCulledFromSelfCamera;

        public bool BodyAwareCullingIncludesLocalFirstPersonBody =>
            CCS_CharacterCameraLayerUtility.DoesMaskIncludeLocalFirstPersonBody(
                CCS_CharacterCameraLayerUtility.BuildFirstPersonBodyAwareCullingMask(
                    CCS_CharacterCameraLayerUtility.BuildDefaultOutputCameraCullingMask()));

        public IReadOnlyList<string> HiddenRendererNames => hiddenRendererNames;

        public IReadOnlyList<string> IncorrectHiddenRendererNames => incorrectHiddenRendererNames;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            networkObject = GetComponent<NetworkObject>();
            ResolveReferences();
            DiscoverHeadRenderers();
        }

        private void OnDisable()
        {
            RestoreAllLocalMasking();
        }

        #endregion

        #region Public Methods

        public void ResolveReferences()
        {
            if (characterAnimator == null)
            {
                characterAnimator = GetComponentInChildren<Animator>(true);
            }

            if (visualRoot == null)
            {
                Transform visualRootTransform = transform.Find("VisualRoot");
                visualRoot = visualRootTransform != null ? visualRootTransform : transform;
            }

            if (combinedBodyRenderer == null && visualRoot != null)
            {
                combinedBodyRenderer = FindCombinedBodyRenderer(visualRoot);
            }

            if (headlessBodyRenderer == null && visualRoot != null)
            {
                Transform headlessTransform = visualRoot.Find(CCS_CharacterControllerConstants.FirstPersonHeadlessBodyObjectName);
                headlessBodyRenderer = headlessTransform != null
                    ? headlessTransform.GetComponent<SkinnedMeshRenderer>()
                    : null;
            }

            if (headlessBodyRenderer != null && headlessBodyMesh == null)
            {
                headlessBodyMesh = headlessBodyRenderer.sharedMesh;
            }
        }

        public void ApplyHeadlessMeshStats(CCS_FirstPersonHeadlessMeshStats stats)
        {
            headlessMeshAssetPath = stats.MeshAssetPath;
            combinedBodyRendererName = stats.SourceRendererName;
            originalTriangleCount = stats.OriginalTriangleCount;
            headlessTriangleCount = stats.RemainingTriangleCount;
        }

        public void DiscoverHeadRenderers()
        {
            ResolveReferences();
            ResolveHeadBone();

            discoveredHeadRenderers.Clear();
            hiddenRendererNames.Clear();
            incorrectHiddenRendererNames.Clear();
            resolvedMaskMode = CCS_LocalFirstPersonHeadMaskMode.None;
            combinedBodyRendererName = combinedBodyRenderer != null ? combinedBodyRenderer.gameObject.name : string.Empty;

            HashSet<Renderer> excluded = BuildExcludedRendererSet();
            if (headlessBodyRenderer != null)
            {
                excluded.Add(headlessBodyRenderer);
            }

            if (combinedBodyRenderer != null)
            {
                excluded.Add(combinedBodyRenderer);
            }

            HashSet<Renderer> candidates = new HashSet<Renderer>();

            if (additionalHeadRenderers != null)
            {
                for (int i = 0; i < additionalHeadRenderers.Length; i++)
                {
                    Renderer renderer = additionalHeadRenderers[i];
                    if (renderer != null && !excluded.Contains(renderer))
                    {
                        candidates.Add(renderer);
                    }
                }
            }

            Transform searchRoot = visualRoot != null ? visualRoot : transform;
            Renderer[] renderers = searchRoot.GetComponentsInChildren<Renderer>(true);

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || excluded.Contains(renderer))
                {
                    continue;
                }

                if (IsProtectedRenderer(renderer))
                {
                    continue;
                }

                string rendererName = renderer.gameObject.name;
                if (IsCombinedBodyRendererName(rendererName))
                {
                    if (combinedBodyRenderer == null && renderer is SkinnedMeshRenderer skinnedMeshRenderer)
                    {
                        combinedBodyRenderer = skinnedMeshRenderer;
                        combinedBodyRendererName = rendererName;
                    }

                    continue;
                }

                if (IsHeadRendererByName(rendererName) || IsUnderHeadBone(renderer))
                {
                    candidates.Add(renderer);
                }
            }

            discoveredHeadRenderers.AddRange(candidates);
            ValidateDiscoveredRenderers();

            if (combinedBodyRenderer != null && headlessBodyRenderer != null && headlessBodyMesh != null)
            {
                resolvedMaskMode = CCS_LocalFirstPersonHeadMaskMode.CombinedBodyHeadlessFallback;
            }
            else if (discoveredHeadRenderers.Count > 0)
            {
                resolvedMaskMode = CCS_LocalFirstPersonHeadMaskMode.SeparateRendererMask;
            }
        }

        public void SetFirstPersonHeadMaskActive(
            bool active,
            CCS_CharacterCameraMode cameraMode,
            string activeCinemachineCameraName)
        {
            bool shouldMask = active
                && ShouldApplyMaskForLocalOwner()
                && IsSupportedFirstPersonCameraMode(cameraMode, activeCinemachineCameraName);

            if (maskActive == shouldMask && (!shouldMask || activeMaskMode == resolvedMaskMode))
            {
                return;
            }

            if (shouldMask)
            {
                switch (resolvedMaskMode)
                {
                    case CCS_LocalFirstPersonHeadMaskMode.SeparateRendererMask:
                        ApplySeparateRendererMask();
                        break;
                    case CCS_LocalFirstPersonHeadMaskMode.CombinedBodyHeadlessFallback:
                        ApplyCombinedBodyHeadlessFallback();
                        break;
                    default:
                        Debug.LogWarning(
                            "[Local FP Head Mask] No separated head renderers or headless body fallback configured.",
                            this);
                        return;
                }
            }
            else
            {
                RestoreAllLocalMasking();
            }

            maskActive = shouldMask;
            activeMaskMode = shouldMask ? resolvedMaskMode : CCS_LocalFirstPersonHeadMaskMode.None;
        }

        public string BuildDebugReport(
            CCS_CharacterCameraMode cameraMode,
            string activeCinemachineCameraName,
            Vector3 bodyAwareAnchorLocalPosition,
            float pitchClampMin,
            float pitchClampMax,
            LayerMask activeCameraCullingMask)
        {
            StringBuilder builder = new StringBuilder(768);
            builder.AppendLine("Active Camera: " + (activeCinemachineCameraName ?? "null"));
            builder.AppendLine("Active Camera Mode: " + cameraMode);
            builder.AppendLine("Is Local Owner: " + IsLocalOwner);
            builder.AppendLine("Head Mask Mode (Resolved): " + resolvedMaskMode);
            builder.AppendLine("Head Mask Mode (Active): " + activeMaskMode);
            builder.AppendLine("Head Self-Mask Active: " + maskActive);
            builder.AppendLine("Head Renderer Count: " + discoveredHeadRenderers.Count);
            builder.AppendLine("BodyAware Anchor Local Position: " + FormatVector3(bodyAwareAnchorLocalPosition));
            builder.AppendLine("Pitch Clamp Min / Max: " + pitchClampMin.ToString("0.0") + " / " + pitchClampMax.ToString("0.0"));
            builder.AppendLine("Original Combined Body Renderer: " + (string.IsNullOrEmpty(combinedBodyRendererName) ? "(none)" : combinedBodyRendererName));
            builder.AppendLine("Headless Body Renderer Active: " + IsHeadlessBodyRendererActive);
            builder.AppendLine("Headless Mesh Asset Path: " + (headlessMeshAssetPath ?? "(none)"));
            builder.AppendLine("Original Triangle Count: " + originalTriangleCount);
            builder.AppendLine("Headless Triangle Count: " + headlessTriangleCount);
            builder.AppendLine("Original Full Body Culled From Self Camera: " + combinedBodyCulledFromSelfCamera);
            builder.AppendLine(
                "BodyAware Culling Includes "
                + CCS_CharacterControllerConstants.LocalFirstPersonBodyLayerName
                + ": "
                + BodyAwareCullingIncludesLocalFirstPersonBody);
            builder.AppendLine("Active Camera Culling Mask: " + activeCameraCullingMask.value);
            builder.AppendLine(
                "Arms/Hands/Revolver Visible: "
                + (HasIncorrectWeaponOrHandHidden ? "No" : "Yes"));

            builder.AppendLine("Hidden Head Renderer Names:");
            if (hiddenRendererNames.Count == 0)
            {
                builder.AppendLine("  (none)");
            }
            else
            {
                for (int i = 0; i < hiddenRendererNames.Count; i++)
                {
                    int originalLayer = i < cachedOriginalLayers.Count ? cachedOriginalLayers[i] : -1;
                    int activeLayer = i < discoveredHeadRenderers.Count && discoveredHeadRenderers[i] != null
                        ? discoveredHeadRenderers[i].gameObject.layer
                        : -1;
                    builder.AppendLine(
                        "  "
                        + hiddenRendererNames[i]
                        + " | original="
                        + LayerMask.LayerToName(originalLayer)
                        + "("
                        + originalLayer
                        + ") active="
                        + LayerMask.LayerToName(activeLayer)
                        + "("
                        + activeLayer
                        + ")");
                }
            }

            if (incorrectHiddenRendererNames.Count > 0)
            {
                builder.AppendLine("Weapon/Hand Incorrectly Hidden:");
                for (int i = 0; i < incorrectHiddenRendererNames.Count; i++)
                {
                    builder.AppendLine("  " + incorrectHiddenRendererNames[i]);
                }
            }

            return builder.ToString();
        }

        #endregion

        #region Private Methods

        private void ApplySeparateRendererMask()
        {
            if (discoveredHeadRenderers.Count == 0)
            {
                return;
            }

            ApplyHeadRendererHiddenLayer();
        }

        private void ApplyCombinedBodyHeadlessFallback()
        {
            ApplySeparateRendererMask();

            int hiddenLayer = CCS_CharacterCameraLayerUtility.GetLocalSelfHeadHiddenLayer();
            int firstPersonBodyLayer = CCS_CharacterCameraLayerUtility.GetLocalFirstPersonBodyLayer();
            if (hiddenLayer < 0 || firstPersonBodyLayer < 0)
            {
                Debug.LogError("[Local FP Head Mask] Missing user layers for combined body fallback.", this);
                return;
            }

            if (combinedBodyRenderer != null)
            {
                if (combinedBodyOriginalLayer < 0)
                {
                    combinedBodyOriginalLayer = combinedBodyRenderer.gameObject.layer;
                }

                combinedBodyRenderer.gameObject.layer = hiddenLayer;
                combinedBodyCulledFromSelfCamera = true;
            }

            if (headlessBodyRenderer != null)
            {
                if (headlessBodyMesh != null)
                {
                    headlessBodyRenderer.sharedMesh = headlessBodyMesh;
                }

                headlessBodyRenderer.gameObject.layer = firstPersonBodyLayer;
                headlessBodyRenderer.enabled = true;
            }
        }

        private void RestoreAllLocalMasking()
        {
            RestoreHeadRendererLayers();
            RestoreCombinedBodyFallback();
            maskActive = false;
            activeMaskMode = CCS_LocalFirstPersonHeadMaskMode.None;
            combinedBodyCulledFromSelfCamera = false;
        }

        private void RestoreCombinedBodyFallback()
        {
            if (combinedBodyRenderer != null && combinedBodyOriginalLayer >= 0)
            {
                combinedBodyRenderer.gameObject.layer = combinedBodyOriginalLayer;
            }

            combinedBodyOriginalLayer = -1;

            if (headlessBodyRenderer != null)
            {
                headlessBodyRenderer.enabled = false;
            }
        }

        private static SkinnedMeshRenderer FindCombinedBodyRenderer(Transform searchRoot)
        {
            SkinnedMeshRenderer[] renderers = searchRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                SkinnedMeshRenderer renderer = renderers[i];
                if (renderer == null || IsHeadlessBodyRendererName(renderer))
                {
                    continue;
                }

                if (IsCombinedBodyRendererName(renderer.gameObject.name))
                {
                    return renderer;
                }

                Mesh mesh = renderer.sharedMesh;
                if (mesh != null && IsCombinedBodyRendererName(mesh.name))
                {
                    return renderer;
                }
            }

            return null;
        }

        private static bool IsHeadlessBodyRendererName(SkinnedMeshRenderer renderer)
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

        private HashSet<Renderer> BuildExcludedRendererSet()
        {
            HashSet<Renderer> excluded = new HashSet<Renderer>();
            if (excludedRenderers == null)
            {
                return excluded;
            }

            for (int i = 0; i < excludedRenderers.Length; i++)
            {
                if (excludedRenderers[i] != null)
                {
                    excluded.Add(excludedRenderers[i]);
                }
            }

            return excluded;
        }

        private void ResolveHeadBone()
        {
            headBone = null;
            if (characterAnimator == null || !characterAnimator.isHuman)
            {
                return;
            }

            headBone = characterAnimator.GetBoneTransform(HumanBodyBones.Head);
        }

        private bool ResolveIsLocalOwner()
        {
            if (networkObject == null || !networkObject.IsSpawned)
            {
                return true;
            }

            return networkObject.IsOwner;
        }

        private bool ShouldApplyMaskForLocalOwner()
        {
            return ResolveIsLocalOwner();
        }

        private static bool IsSupportedFirstPersonCameraMode(
            CCS_CharacterCameraMode cameraMode,
            string activeCinemachineCameraName)
        {
            if (cameraMode != CCS_CharacterCameraMode.FirstPersonAim
                && cameraMode != CCS_CharacterCameraMode.FirstPersonBodyAware)
            {
                return false;
            }

            return activeCinemachineCameraName
                == CCS_CharacterControllerConstants.FirstPersonBodyAwareCinemachineCameraName;
        }

        private static bool IsCombinedBodyRendererName(string rendererName)
        {
            string normalized = NormalizeToken(rendererName);
            return normalized.Contains("ccgamebody")
                || (normalized.Contains("cc3") && normalized.Contains("body"))
                || normalized == "body"
                || normalized.Contains("capsulevisual");
        }

        private static bool IsHeadRendererByName(string rendererName)
        {
            string normalized = NormalizeToken(rendererName);
            if (string.IsNullOrEmpty(normalized))
            {
                return false;
            }

            if (ContainsAny(normalized, ProtectedNameTokens))
            {
                return false;
            }

            return ContainsAny(normalized, HeadNameTokens);
        }

        private bool IsUnderHeadBone(Renderer renderer)
        {
            if (headBone == null || renderer == null)
            {
                return false;
            }

            Transform current = renderer.transform;
            if (!current.IsChildOf(headBone))
            {
                return false;
            }

            string normalized = NormalizeToken(renderer.gameObject.name);
            return !ContainsAny(normalized, ProtectedNameTokens) && !ContainsAny(normalized, BodyPartNameTokens);
        }

        private static bool IsProtectedRenderer(Renderer renderer)
        {
            if (renderer == null)
            {
                return true;
            }

            Transform current = renderer.transform;
            while (current != null)
            {
                string normalized = NormalizeToken(current.name);
                if (ContainsAny(normalized, ProtectedNameTokens))
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private void ValidateDiscoveredRenderers()
        {
            incorrectHiddenRendererNames.Clear();
            for (int i = discoveredHeadRenderers.Count - 1; i >= 0; i--)
            {
                Renderer renderer = discoveredHeadRenderers[i];
                if (renderer == null)
                {
                    discoveredHeadRenderers.RemoveAt(i);
                    continue;
                }

                if (renderer == combinedBodyRenderer || renderer == headlessBodyRenderer)
                {
                    discoveredHeadRenderers.RemoveAt(i);
                    continue;
                }

                if (IsProtectedRenderer(renderer))
                {
                    incorrectHiddenRendererNames.Add(renderer.gameObject.name);
                    discoveredHeadRenderers.RemoveAt(i);
                }
            }
        }

        private void ApplyHeadRendererHiddenLayer()
        {
            int hiddenLayer = CCS_CharacterCameraLayerUtility.GetLocalSelfHeadHiddenLayer();
            if (hiddenLayer < 0)
            {
                Debug.LogError(
                    "[Local FP Head Mask] Missing user layer "
                    + CCS_CharacterControllerConstants.LocalSelfHeadHiddenLayerName
                    + ".",
                    this);
                return;
            }

            cachedOriginalLayers.Clear();
            hiddenRendererNames.Clear();

            for (int i = 0; i < discoveredHeadRenderers.Count; i++)
            {
                Renderer renderer = discoveredHeadRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                cachedOriginalLayers.Add(renderer.gameObject.layer);
                hiddenRendererNames.Add(renderer.gameObject.name);
                renderer.gameObject.layer = hiddenLayer;
            }
        }

        private void RestoreHeadRendererLayers()
        {
            for (int i = 0; i < discoveredHeadRenderers.Count; i++)
            {
                Renderer renderer = discoveredHeadRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                int originalLayer = i < cachedOriginalLayers.Count ? cachedOriginalLayers[i] : 0;
                renderer.gameObject.layer = originalLayer;
            }

            cachedOriginalLayers.Clear();
            hiddenRendererNames.Clear();
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

        private static string FormatVector3(Vector3 value)
        {
            return "("
                + value.x.ToString("0.000")
                + ", "
                + value.y.ToString("0.000")
                + ", "
                + value.z.ToString("0.000")
                + ")";
        }

        private static readonly string[] HeadNameTokens =
        {
            "head",
            "hair",
            "hat",
            "beard",
            "mustache",
            "brow",
            "eyelash",
            "eyebrow",
            "eye",
            "cornea",
            "teeth",
            "tongue",
            "face",
            "ear",
            "scalp",
            "glasses",
            "facial",
        };

        private static readonly string[] BodyPartNameTokens =
        {
            "arm",
            "hand",
            "finger",
            "shoulder",
            "clavicle",
            "torso",
            "chest",
            "body",
            "leg",
            "foot",
            "hip",
            "thigh",
            "calf",
            "spine",
            "pelvis",
            "shirt",
            "jean",
            "sneaker",
            "boxer",
            "nail",
        };

        private static readonly string[] ProtectedNameTokens =
        {
            "revolver",
            "weapon",
            "holster",
            "socket",
            "equipped",
            "equipment",
            "muzzle",
            "rifle",
            "shotgun",
            "bow",
            "lantern",
            "backpack",
            "righthand",
            "lefthand",
            "handweapon",
            "firstpersonheadlessbody",
        };

        #endregion
    }
}
