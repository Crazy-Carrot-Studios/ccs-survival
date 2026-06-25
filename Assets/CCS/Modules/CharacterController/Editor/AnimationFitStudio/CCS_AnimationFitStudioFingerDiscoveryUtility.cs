using System.Collections.Generic;
using System.Text;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AnimationFitStudioFingerDiscoveryUtility
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Discovers right-hand finger bones on the preview humanoid rig.
// PLACEMENT: Editor utility used by Animation Fit Studio pose and validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Falls back to name search under RightHand when HumanBodyBones are missing.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public sealed class CCS_AnimationFitStudioFingerSegmentBinding
    {
        public HumanBodyBones Bone { get; set; }

        public Transform Transform { get; set; }

        public Quaternion BaselineLocalRotation { get; set; } = Quaternion.identity;
    }

    public sealed class CCS_AnimationFitStudioFingerChainDiscovery
    {
        public string FingerId { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public int ExpectedSegmentCount { get; set; }

        public List<CCS_AnimationFitStudioFingerSegmentBinding> Segments { get; } =
            new List<CCS_AnimationFitStudioFingerSegmentBinding>();

        public int FoundSegmentCount => Segments.Count;

        public string StatusLabel => FoundSegmentCount + "/" + ExpectedSegmentCount;
    }

    public sealed class CCS_AnimationFitStudioFingerDiscoveryResult
    {
        public bool AnyFingerBonesFound { get; set; }

        public bool AllPrimaryFingersFound { get; set; }

        public List<CCS_AnimationFitStudioFingerChainDiscovery> Chains { get; } =
            new List<CCS_AnimationFitStudioFingerChainDiscovery>();

        public string GetSummaryLabel()
        {
            return AnyFingerBonesFound ? "Found" : "Missing";
        }

        public string GetDetailedStatusText()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Finger Bones: " + GetSummaryLabel());
            for (int i = 0; i < Chains.Count; i++)
            {
                CCS_AnimationFitStudioFingerChainDiscovery chain = Chains[i];
                builder.AppendLine(chain.Label + ": " + chain.StatusLabel);
            }

            return builder.ToString().TrimEnd();
        }
    }

    public static class CCS_AnimationFitStudioFingerDiscoveryUtility
    {
        public const string MissingFingerBonesWarning =
            "Finger bones not found on this preview rig. Grip sliders cannot affect the hand.";

        private static readonly FingerChainDefinition[] ChainDefinitions =
        {
            new FingerChainDefinition(
                "right_thumb",
                "Thumb",
                3,
                HumanBodyBones.RightThumbProximal,
                HumanBodyBones.RightThumbIntermediate,
                HumanBodyBones.RightThumbDistal),
            new FingerChainDefinition(
                "right_index",
                "Index",
                3,
                HumanBodyBones.RightIndexProximal,
                HumanBodyBones.RightIndexIntermediate,
                HumanBodyBones.RightIndexDistal),
            new FingerChainDefinition(
                "right_middle",
                "Middle",
                3,
                HumanBodyBones.RightMiddleProximal,
                HumanBodyBones.RightMiddleIntermediate,
                HumanBodyBones.RightMiddleDistal),
            new FingerChainDefinition(
                "right_ring",
                "Ring",
                3,
                HumanBodyBones.RightRingProximal,
                HumanBodyBones.RightRingIntermediate,
                HumanBodyBones.RightRingDistal),
            new FingerChainDefinition(
                "right_pinky",
                "Pinky",
                3,
                HumanBodyBones.RightLittleProximal,
                HumanBodyBones.RightLittleIntermediate,
                HumanBodyBones.RightLittleDistal),
        };

        private static readonly string[][] FallbackNameTokens =
        {
            new[] { "Thumb", "RightHandThumb", "Right Thumb" },
            new[] { "Index", "RightHandIndex", "Right Index" },
            new[] { "Middle", "RightHandMiddle", "Right Middle" },
            new[] { "Ring", "RightHandRing", "Right Ring" },
            new[] { "Little", "Pinky", "RightHandLittle", "Right Little" },
        };

        public static CCS_AnimationFitStudioFingerDiscoveryResult Discover(Animator animator)
        {
            CCS_AnimationFitStudioFingerDiscoveryResult result = new CCS_AnimationFitStudioFingerDiscoveryResult();
            if (animator == null)
            {
                return result;
            }

            Transform rightHand = animator.isHuman
                ? animator.GetBoneTransform(HumanBodyBones.RightHand)
                : null;

            for (int i = 0; i < ChainDefinitions.Length; i++)
            {
                FingerChainDefinition definition = ChainDefinitions[i];
                CCS_AnimationFitStudioFingerChainDiscovery chain = new CCS_AnimationFitStudioFingerChainDiscovery
                {
                    FingerId = definition.FingerId,
                    Label = definition.Label,
                    ExpectedSegmentCount = definition.ExpectedSegmentCount,
                };

                for (int s = 0; s < definition.Bones.Length; s++)
                {
                    Transform segmentTransform = animator.GetBoneTransform(definition.Bones[s]);
                    if (segmentTransform == null && rightHand != null)
                    {
                        segmentTransform = FindChildByNameTokens(rightHand, FallbackNameTokens[i], s);
                    }

                    if (segmentTransform == null)
                    {
                        continue;
                    }

                    chain.Segments.Add(new CCS_AnimationFitStudioFingerSegmentBinding
                    {
                        Bone = definition.Bones[s],
                        Transform = segmentTransform,
                        BaselineLocalRotation = segmentTransform.localRotation,
                    });
                }

                result.Chains.Add(chain);
            }

            int foundChains = 0;
            for (int i = 0; i < result.Chains.Count; i++)
            {
                if (result.Chains[i].FoundSegmentCount > 0)
                {
                    foundChains++;
                }
            }

            result.AnyFingerBonesFound = foundChains > 0;
            result.AllPrimaryFingersFound = foundChains >= 4;
            return result;
        }

        public static void CaptureBaselines(CCS_AnimationFitStudioFingerDiscoveryResult discovery)
        {
            if (discovery == null)
            {
                return;
            }

            for (int i = 0; i < discovery.Chains.Count; i++)
            {
                CCS_AnimationFitStudioFingerChainDiscovery chain = discovery.Chains[i];
                for (int s = 0; s < chain.Segments.Count; s++)
                {
                    CCS_AnimationFitStudioFingerSegmentBinding segment = chain.Segments[s];
                    if (segment.Transform != null)
                    {
                        segment.BaselineLocalRotation = segment.Transform.localRotation;
                    }
                }
            }
        }

        private static Transform FindChildByNameTokens(Transform root, string[] tokens, int segmentIndex)
        {
            if (root == null)
            {
                return null;
            }

            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            List<Transform> matches = new List<Transform>();
            for (int i = 0; i < children.Length; i++)
            {
                Transform child = children[i];
                if (child == root)
                {
                    continue;
                }

                string name = child.name;
                for (int t = 0; t < tokens.Length; t++)
                {
                    if (name.IndexOf(tokens[t], System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        matches.Add(child);
                        break;
                    }
                }
            }

            matches.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            return segmentIndex >= 0 && segmentIndex < matches.Count ? matches[segmentIndex] : null;
        }

        private sealed class FingerChainDefinition
        {
            public string FingerId { get; }

            public string Label { get; }

            public int ExpectedSegmentCount { get; }

            public HumanBodyBones[] Bones { get; }

            public FingerChainDefinition(
                string fingerId,
                string label,
                int expectedSegmentCount,
                params HumanBodyBones[] bones)
            {
                FingerId = fingerId;
                Label = label;
                ExpectedSegmentCount = expectedSegmentCount;
                Bones = bones;
            }
        }
    }
}
