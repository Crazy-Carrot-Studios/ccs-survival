using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AnimationFitStudioFingerManipulationUtility
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Applies detailed finger segment offsets and quick grip curls with axis correction.
// PLACEMENT: Editor utility used by Animation Fit Studio pose apply and save.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Applies local rotation offsets from baselines after base pose sampling.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public enum CCS_AnimationFitStudioFingerCurlDirectionKind
    {
        Normal,
        Inverted,
    }

    public static class CCS_AnimationFitStudioFingerManipulationUtility
    {
        private const float QuickCurlMaxDegrees = 55f;
        private const float QuickCurlGripScale = 0.35f;

        public static void ApplyFingerEdits(
            CCS_AnimationFitStudioFingerDiscoveryResult discovery,
            CCS_AnimationFitStudioPoseEditData poseEditData,
            float gripTightness,
            CCS_AnimationFitStudioFingerCurlDirectionKind directionKind)
        {
            if (discovery == null || poseEditData == null)
            {
                return;
            }

            float directionSign = directionKind == CCS_AnimationFitStudioFingerCurlDirectionKind.Inverted ? -1f : 1f;
            for (int i = 0; i < discovery.Chains.Count; i++)
            {
                CCS_AnimationFitStudioFingerChainDiscovery chain = discovery.Chains[i];
                RestoreFingerBaseline(chain);

                if (!poseEditData.TryGetFingerChainEdit(chain.FingerId, out CCS_AnimationFitStudioFingerChainEditState chainEdit)
                    && !poseEditData.PartEdits.TryGetValue(chain.FingerId, out CCS_AnimationFitStudioPartEditState quickEdit))
                {
                    continue;
                }

                chainEdit ??= poseEditData.GetOrCreateFingerChainEdit(chain.FingerId);
                poseEditData.PartEdits.TryGetValue(chain.FingerId, out CCS_AnimationFitStudioPartEditState quickCurlEdit);

                CCS_AnimationFitStudioFingerAxisSettings axisSettings =
                    ResolveAxisSettings(poseEditData, chainEdit);

                float quickCurlDegrees = 0f;
                if (quickCurlEdit != null)
                {
                    quickCurlDegrees = directionSign
                        * QuickCurlMaxDegrees
                        * Mathf.Clamp01(quickCurlEdit.FingerCurl + gripTightness * QuickCurlGripScale);
                }

                for (int s = 0; s < chain.Segments.Count; s++)
                {
                    CCS_AnimationFitStudioFingerSegmentBinding segment = chain.Segments[s];
                    if (segment.Transform == null)
                    {
                        continue;
                    }

                    CCS_AnimationFitStudioFingerSegmentEditState segmentEdit =
                        s < chainEdit.Segments.Length ? chainEdit.Segments[s] : null;
                    if (segmentEdit == null)
                    {
                        continue;
                    }

                    float curlDegrees = segmentEdit.CurlDegrees + quickCurlDegrees;
                    float spreadDegrees = segmentEdit.SpreadDegrees;
                    Vector3 eulerOffset = new Vector3(
                        segmentEdit.PitchDegrees,
                        segmentEdit.YawDegrees,
                        segmentEdit.RollDegrees);

                    segment.Transform.localRotation = ApplySegmentRotation(
                        segment.BaselineLocalRotation,
                        eulerOffset,
                        curlDegrees,
                        spreadDegrees,
                        axisSettings);
                }
            }
        }

        public static int CountFingerSegments(CCS_AnimationFitStudioFingerDiscoveryResult discovery)
        {
            int count = 0;
            if (discovery == null)
            {
                return count;
            }

            for (int i = 0; i < discovery.Chains.Count; i++)
            {
                count += discovery.Chains[i].FoundSegmentCount;
            }

            return count;
        }

        public static int CountWrittenFingerSegments(
            CCS_AnimationFitStudioFingerDiscoveryResult discovery,
            CCS_AnimationFitStudioPoseEditData poseEditData)
        {
            if (discovery == null || poseEditData == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < discovery.Chains.Count; i++)
            {
                CCS_AnimationFitStudioFingerChainDiscovery chain = discovery.Chains[i];
                if (!poseEditData.TryGetFingerChainEdit(chain.FingerId, out CCS_AnimationFitStudioFingerChainEditState chainEdit)
                    || chainEdit == null)
                {
                    continue;
                }

                for (int s = 0; s < chain.Segments.Count && s < chainEdit.Segments.Length; s++)
                {
                    if (chain.Segments[s].Transform != null && chainEdit.Segments[s].WasEdited)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private static CCS_AnimationFitStudioFingerAxisSettings ResolveAxisSettings(
            CCS_AnimationFitStudioPoseEditData poseEditData,
            CCS_AnimationFitStudioFingerChainEditState chainEdit)
        {
            if (poseEditData.UseSharedFingerAxisSettings || chainEdit == null || !chainEdit.OverrideAxis)
            {
                return poseEditData.SharedFingerAxisSettings;
            }

            return chainEdit.AxisOverride;
        }

        private static Quaternion ApplySegmentRotation(
            Quaternion baseline,
            Vector3 eulerOffsetDegrees,
            float curlDegrees,
            float spreadDegrees,
            CCS_AnimationFitStudioFingerAxisSettings axisSettings)
        {
            Quaternion result = baseline;
            if (eulerOffsetDegrees != Vector3.zero)
            {
                result *= Quaternion.Euler(eulerOffsetDegrees);
            }

            if (Mathf.Abs(curlDegrees) > CCS_AnimationFitStudioPoseEditData.EditEpsilon)
            {
                Vector3 curlAxis = CCS_AnimationFitStudioLocalAxisUtility.ToAxisVector(axisSettings.CurlAxis);
                result *= Quaternion.AngleAxis(curlDegrees, curlAxis);
            }

            if (Mathf.Abs(spreadDegrees) > CCS_AnimationFitStudioPoseEditData.EditEpsilon)
            {
                Vector3 spreadAxis = CCS_AnimationFitStudioLocalAxisUtility.ToAxisVector(axisSettings.SpreadAxis);
                result *= Quaternion.AngleAxis(spreadDegrees, spreadAxis);
            }

            return result;
        }

        private static void RestoreFingerBaseline(CCS_AnimationFitStudioFingerChainDiscovery chain)
        {
            for (int s = 0; s < chain.Segments.Count; s++)
            {
                CCS_AnimationFitStudioFingerSegmentBinding segment = chain.Segments[s];
                if (segment.Transform != null)
                {
                    segment.Transform.localRotation = segment.BaselineLocalRotation;
                }
            }
        }
    }
}
