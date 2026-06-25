using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Editor.Common;
using CCS.Modules.CharacterController.Editor.EquipmentFitStudio;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AnimationFitStudioPoseUtility
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Samples, previews, and saves final-pose edits for Animation Fit Studio.
// PLACEMENT: Editor utility used by Animation Fit Studio window.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Writes only allowed body-part curves into the _FitTest.anim duplicate. Never wires Animator Controller.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public sealed class CCS_AnimationFitStudioApplyPoseResult
    {
        public bool Success { get; set; }

        public bool BonesChanged { get; set; }

        public int ChangedBoneCount { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;

        public string WarningMessage { get; set; } = string.Empty;

        public CCS_AnimationFitStudioClipDiagnostics Diagnostics { get; set; }
    }

    public static class CCS_AnimationFitStudioPoseUtility
    {
        public const string ClipDidNotAffectSkeletonWarning =
            "Animation clip did not affect the preview skeleton. Check rig type or clip bone paths.";

        public const string ClipSampledNoBoneChangeWarning =
            "Clip sampled but did not change preview bones. This usually means the clip curves do not match "
            + "the preview rig or the clip is not compatible with this avatar.";

        public const string SaveBlockedTposeWarning =
            CCS_AnimationFitStudioAimPoseScoreUtility.SaveBlockedInvalidAimWarning;

        public const string SeedPoseFitTestNotice =
            "This FitTest pose is based on CCS seed pose, not source animation.";

        private const float RotationChangeThresholdDegrees = 0.1f;
        private const float PositionChangeThreshold = 0.0001f;

        private static readonly HumanBodyBones[] MonitoredBones =
        {
            HumanBodyBones.RightUpperArm,
            HumanBodyBones.RightLowerArm,
            HumanBodyBones.RightHand,
            HumanBodyBones.Chest,
            HumanBodyBones.Spine,
        };

        private static readonly Vector3 SeedUpperArmEuler = new Vector3(-42f, -18f, 82f);
        private static readonly Vector3 SeedLowerArmEuler = new Vector3(28f, 0f, 0f);
        private static readonly Vector3 SeedHandEuler = new Vector3(-12f, 8f, 0f);
        private static readonly Vector3 SeedChestEuler = new Vector3(6f, 0f, 0f);

        private static readonly Dictionary<string, int> MuscleNameToIndex = BuildMuscleNameIndex();

        public static void EnsureAnimationMode()
        {
            if (!AnimationMode.InAnimationMode())
            {
                AnimationMode.StartAnimationMode();
            }
        }

        public static void StopAnimationModeIfNeeded()
        {
            if (AnimationMode.InAnimationMode())
            {
                AnimationMode.StopAnimationMode();
            }
        }

        public static float GetDefaultPoseTime(AnimationClip clip)
        {
            return clip != null ? clip.length : 0f;
        }

        public static float ClampPoseTime(AnimationClip clip, float poseTime)
        {
            if (clip == null)
            {
                return 0f;
            }

            return Mathf.Clamp(poseTime, 0f, clip.length);
        }

        public static CCS_AnimationFitStudioApplyPoseResult TryApplyPreviewPose(
            CCS_AnimationFitStudioPreviewState previewState,
            CCS_AnimationFitStudioBasePoseSourceKind basePoseSource,
            AnimationClip clip,
            float poseTime,
            CCS_AnimationFitStudioPoseEditData poseEditData,
            float gripTightness,
            CCS_AnimationFitStudioFingerCurlDirectionKind fingerCurlDirection,
            CCS_AnimationFitStudioPoseSourceKind runtimePoseKind = CCS_AnimationFitStudioPoseSourceKind.Source,
            bool useSeedPose = false)
        {
            CCS_AnimationFitStudioApplyPoseResult result = new CCS_AnimationFitStudioApplyPoseResult();

            if (previewState?.PreviewPlayer == null)
            {
                result.ErrorMessage = "Load preview player first.";
                return result;
            }

            Animator animator = previewState.PreviewAnimator
                ?? CCS_EquipmentFitStudioPosePreviewUtility.FindPlayerAnimator(previewState.PreviewPlayer);
            previewState.PreviewAnimator = animator;
            if (animator == null)
            {
                result.ErrorMessage = "Preview player is missing an Animator.";
                return result;
            }

            previewState.ActiveBasePoseSource = basePoseSource;
            previewState.ActivePoseSource = useSeedPose
                ? CCS_AnimationFitStudioPoseSourceKind.Seed
                : runtimePoseKind;
            previewState.FingerCurlDirection = fingerCurlDirection;
            previewState.ActivePoseTime = poseTime;
            previewState.SeedPoseBasedFitTest = useSeedPose;

            if (previewState.FingerDiscovery == null)
            {
                previewState.FingerDiscovery = CCS_AnimationFitStudioFingerDiscoveryUtility.Discover(animator);
                previewState.FingerBonesFound = previewState.FingerDiscovery.AnyFingerBonesFound;
            }

            if (useSeedPose)
            {
                ApplySeedPoseInternal(animator, previewState);
            }
            else if (!TryApplyBasePose(previewState, basePoseSource, clip, poseTime, out string basePoseError))
            {
                previewState.PosePreviewStatus = CCS_AnimationFitStudioPosePreviewStatus.Failed;
                previewState.PoseApplied = false;
                previewState.PoseWarningMessage = basePoseError;
                result.ErrorMessage = basePoseError;
                return result;
            }

            CCS_AnimationFitStudioFingerDiscoveryUtility.CaptureBaselines(previewState.FingerDiscovery);

            bool humanoidClip = clip != null
                && CCS_AnimationFitStudioClipCurveModeUtility.DetectClipCurveMode(clip)
                    == CCS_AnimationFitStudioClipCurveMode.HumanoidMuscleCurves;
            if (humanoidClip && !useSeedPose)
            {
                if (!previewState.HumanoidControl.IsInitialized)
                {
                    CCS_AnimationFitStudioHumanoidControlUtility.CaptureBaselineFromClip(
                        clip,
                        poseTime,
                        previewState.HumanoidControl);
                }

                CCS_AnimationFitStudioHumanoidControlUtility.ApplyCurrentMuscleValuesToAnimator(
                    animator,
                    previewState.HumanoidControl);
                CCS_AnimationFitStudioFingerManipulationUtility.ApplyFingerEdits(
                    previewState.FingerDiscovery,
                    poseEditData,
                    gripTightness,
                    fingerCurlDirection);
            }
            else
            {
                ApplyPartOffsets(
                    animator,
                    previewState.FingerDiscovery,
                    poseEditData,
                    gripTightness,
                    fingerCurlDirection);
            }

            previewState.PosePreviewStatus = useSeedPose
                ? CCS_AnimationFitStudioPosePreviewStatus.SeedPose
                : CCS_AnimationFitStudioPosePreviewStatus.AppliedAimPose;
            previewState.PoseApplied = true;
            previewState.PoseWarningMessage = useSeedPose ? SeedPoseFitTestNotice : string.Empty;
            previewState.AimPoseScore = CCS_AnimationFitStudioAimPoseScoreUtility.Evaluate(animator);
            previewState.ChangedBoneCount = previewState.AimPoseScore?.ChangedAimBones ?? 0;
            result.Diagnostics = CCS_AnimationFitStudioClipDiagnostics.Build(
                clip,
                previewState.ChangedBoneCount,
                previewState.AimPoseScore,
                previewState.LastSampleMethod);
            result.Success = true;
            result.BonesChanged = true;
            result.ChangedBoneCount = previewState.ChangedBoneCount;
            return result;
        }

        private static bool TryApplyBasePose(
            CCS_AnimationFitStudioPreviewState previewState,
            CCS_AnimationFitStudioBasePoseSourceKind basePoseSource,
            AnimationClip clip,
            float poseTime,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            Animator animator = previewState.PreviewAnimator;
            if (animator == null)
            {
                errorMessage = "Preview animator is missing.";
                return false;
            }

            if (basePoseSource == CCS_AnimationFitStudioBasePoseSourceKind.EquipmentFitStudioRevolverAim)
            {
                CCS_AnimationFitStudioPlayablePreviewSampler.RestoreAnimatorController(previewState);
                if (animator.runtimeAnimatorController == null
                    && previewState.StoredRuntimeAnimatorController != null)
                {
                    animator.runtimeAnimatorController = previewState.StoredRuntimeAnimatorController;
                }

                CCS_RevolverAimPreviewPoseUtility.ApplyRevolverAimPose(animator);
                previewState.LastSampleMethod = CCS_RevolverAimPreviewPoseUtility.DisplayLabel;
                return true;
            }

            if (clip == null)
            {
                errorMessage = "Clip is missing for clip-based pose source.";
                return false;
            }

            float sampleTime = poseTime > 0.001f
                ? ClampPoseTime(clip, poseTime)
                : clip.name.IndexOf("WalkAimed", System.StringComparison.OrdinalIgnoreCase) >= 0
                    ? clip.length * 0.5f
                    : clip.length * 0.65f;

            if (!CCS_AnimationFitStudioPlayablePreviewSampler.TrySampleClip(
                    animator,
                    previewState,
                    clip,
                    sampleTime,
                    out string sampleMethod))
            {
                errorMessage = ClipDidNotAffectSkeletonWarning;
                return false;
            }

            previewState.LastSampleMethod = sampleMethod;
            previewState.ActivePoseTime = sampleTime;
            return true;
        }

        public static void MaintainPreviewPose(
            CCS_AnimationFitStudioPreviewState previewState,
            CCS_AnimationFitStudioBasePoseSourceKind basePoseSource,
            AnimationClip clip,
            float poseTime,
            CCS_AnimationFitStudioPoseEditData poseEditData,
            float gripTightness,
            CCS_AnimationFitStudioFingerCurlDirectionKind fingerCurlDirection,
            CCS_AnimationFitStudioPoseSourceKind runtimePoseKind,
            bool useSeedPose)
        {
            if (previewState?.PreviewPlayer == null
                || !IsMaintainablePreviewStatus(previewState.PosePreviewStatus))
            {
                return;
            }

            Animator animator = previewState.PreviewAnimator;
            if (animator == null)
            {
                return;
            }

            if (useSeedPose)
            {
                ApplySeedPoseInternal(animator, previewState);
            }
            else
            {
                TryApplyBasePose(previewState, basePoseSource, clip, poseTime, out _);
            }

            CCS_AnimationFitStudioFingerDiscoveryUtility.CaptureBaselines(previewState.FingerDiscovery);
            ApplyPartOffsets(
                animator,
                previewState.FingerDiscovery,
                poseEditData,
                gripTightness,
                fingerCurlDirection);
        }

        public static void MaintainPreviewPose(
            CCS_AnimationFitStudioPreviewState previewState,
            AnimationClip clip,
            float poseTime,
            CCS_AnimationFitStudioPoseEditData poseEditData,
            float gripTightness,
            CCS_AnimationFitStudioPoseSourceKind poseSource)
        {
            MaintainPreviewPose(
                previewState,
                previewState.ActiveBasePoseSource,
                clip,
                poseTime,
                poseEditData,
                gripTightness,
                previewState.FingerCurlDirection,
                poseSource,
                poseSource == CCS_AnimationFitStudioPoseSourceKind.Seed);
        }

        public static CCS_AnimationFitStudioApplyPoseResult TryApplySeedPose(
            CCS_AnimationFitStudioPreviewState previewState,
            CCS_AnimationFitStudioPoseEditData poseEditData,
            float gripTightness,
            CCS_AnimationFitStudioFingerCurlDirectionKind fingerCurlDirection)
        {
            return TryApplyPreviewPose(
                previewState,
                previewState.ActiveBasePoseSource,
                null,
                0f,
                poseEditData,
                gripTightness,
                fingerCurlDirection,
                CCS_AnimationFitStudioPoseSourceKind.Seed,
                useSeedPose: true);
        }

        public static void SampleClipOnPreview(
            GameObject previewRoot,
            AnimationClip clip,
            float poseTime)
        {
            if (previewRoot == null || clip == null)
            {
                return;
            }

            Animator animator = CCS_EquipmentFitStudioPosePreviewUtility.FindPlayerAnimator(previewRoot);
            if (animator == null)
            {
                return;
            }

            SampleClipOnAnimator(animator, null, clip, ClampPoseTime(clip, poseTime));
        }

        public static void SampleClipOnAnimator(
            Animator animator,
            CCS_AnimationFitStudioPreviewState previewState,
            AnimationClip clip,
            float poseTime)
        {
            if (animator == null || clip == null)
            {
                return;
            }

            CCS_AnimationFitStudioPlayablePreviewSampler.TrySampleClip(
                animator,
                previewState,
                clip,
                poseTime,
                out _);
        }

        public static void SampleClipOnAnimator(Animator animator, AnimationClip clip, float poseTime)
        {
            SampleClipOnAnimator(animator, null, clip, poseTime);
        }

        public static void ApplyPartOffsets(
            Animator animator,
            CCS_AnimationFitStudioFingerDiscoveryResult fingerDiscovery,
            CCS_AnimationFitStudioPoseEditData poseEditData,
            float gripTightness,
            CCS_AnimationFitStudioFingerCurlDirectionKind fingerCurlDirection)
        {
            if (animator == null || poseEditData?.PartEdits == null)
            {
                return;
            }

            IReadOnlyDictionary<string, CCS_AnimationFitStudioPartEditState> partEdits = poseEditData.PartEdits;
            IReadOnlyList<CCS_AnimationFitStudioBodyPartDefinition> definitions =
                CCS_AnimationFitStudioBodyPartCatalog.AllDefinitions;
            for (int i = 0; i < definitions.Count; i++)
            {
                CCS_AnimationFitStudioBodyPartDefinition definition = definitions[i];
                if (definition.Kind == CCS_AnimationFitStudioBodyPartKind.FingerCurl)
                {
                    continue;
                }

                if (!partEdits.TryGetValue(definition.Id, out CCS_AnimationFitStudioPartEditState editState)
                    || editState == null)
                {
                    continue;
                }

                switch (definition.Kind)
                {
                    case CCS_AnimationFitStudioBodyPartKind.BoneRotation:
                        if (definition.MuscleNames.Length > 0)
                        {
                            ApplyMuscleOffsets(animator, definition, editState, gripTightness);
                        }
                        else
                        {
                            ApplyBoneRotationOffset(animator, definition.Bone, editState.EulerOffsetDegrees);
                        }

                        break;
                    case CCS_AnimationFitStudioBodyPartKind.OptionalChestLean:
                        ApplyMuscleOffsets(animator, definition, editState, gripTightness);
                        break;
                }
            }

            CCS_AnimationFitStudioFingerManipulationUtility.ApplyFingerEdits(
                fingerDiscovery,
                poseEditData,
                gripTightness,
                fingerCurlDirection);
        }

        public static void ApplyPartOffsets(
            Animator animator,
            CCS_AnimationFitStudioPoseEditData poseEditData,
            float gripTightness)
        {
            ApplyPartOffsets(
                animator,
                null,
                poseEditData,
                gripTightness,
                CCS_AnimationFitStudioFingerCurlDirectionKind.Normal);
        }

        public static bool HasFingerBones(Animator animator)
        {
            if (animator == null || !animator.isHuman)
            {
                return false;
            }

            return animator.GetBoneTransform(HumanBodyBones.RightHand) != null;
        }

        public static bool CanSaveFitTestPose(CCS_AnimationFitStudioPreviewState previewState)
        {
            if (previewState == null)
            {
                return false;
            }

            return previewState.PreviewPlayer != null
                && previewState.RightHandBonesFound
                && previewState.PoseApplied
                && (previewState.PosePreviewStatus == CCS_AnimationFitStudioPosePreviewStatus.AppliedAimPose
                    || previewState.PosePreviewStatus == CCS_AnimationFitStudioPosePreviewStatus.SeedPose);
        }

        public static bool CanEditPoseParts(CCS_AnimationFitStudioPreviewState previewState)
        {
            return previewState != null
                && previewState.PreviewPlayer != null
                && previewState.PoseApplied
                && IsMaintainablePreviewStatus(previewState.PosePreviewStatus);
        }

        public static bool IsMaintainablePreviewStatus(CCS_AnimationFitStudioPosePreviewStatus status)
        {
            return status == CCS_AnimationFitStudioPosePreviewStatus.AppliedAimPose
                || status == CCS_AnimationFitStudioPosePreviewStatus.SeedPose;
        }

        public static bool HasHumanoidPoseEdits(CCS_AnimationFitStudioHumanoidControlState humanoidControl)
        {
            if (humanoidControl == null || !humanoidControl.IsInitialized)
            {
                return false;
            }

            for (int i = 0; i < CCS_AnimationFitStudioHumanoidMuscleMapping.RightArmSaveMuscleNames.Length; i++)
            {
                string muscleName = CCS_AnimationFitStudioHumanoidMuscleMapping.RightArmSaveMuscleNames[i];
                if (Mathf.Abs(humanoidControl.GetDeltaFromBaseline(muscleName))
                    > CCS_AnimationFitStudioPoseEditData.EditEpsilon)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasPoseEdits(CCS_AnimationFitStudioPoseEditData poseEditData)
        {
            if (poseEditData == null)
            {
                return false;
            }

            foreach (System.Collections.Generic.KeyValuePair<string, CCS_AnimationFitStudioPartEditState> entry in poseEditData.PartEdits)
            {
                CCS_AnimationFitStudioPartEditState editState = entry.Value;
                if (editState == null)
                {
                    continue;
                }

                if (editState.EulerOffsetDegrees.sqrMagnitude > CCS_AnimationFitStudioPoseEditData.EditEpsilon
                    || editState.FingerCurl > CCS_AnimationFitStudioPoseEditData.EditEpsilon)
                {
                    return true;
                }
            }

            if (CountEditedFingerSegments(poseEditData) > 0)
            {
                return true;
            }

            return false;
        }

        public static int CountEditedFingerSegments(CCS_AnimationFitStudioPoseEditData poseEditData)
        {
            if (poseEditData?.FingerEdits == null)
            {
                return 0;
            }

            int count = 0;
            foreach (System.Collections.Generic.KeyValuePair<string, CCS_AnimationFitStudioFingerChainEditState> pair in poseEditData.FingerEdits)
            {
                CCS_AnimationFitStudioFingerChainEditState chainEdit = pair.Value;
                if (chainEdit == null || !chainEdit.WasEdited || chainEdit.Segments == null)
                {
                    continue;
                }

                for (int segmentIndex = 0; segmentIndex < chainEdit.Segments.Length; segmentIndex++)
                {
                    if (chainEdit.Segments[segmentIndex].WasEdited)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public static int CountFingerBonesFound(CCS_AnimationFitStudioFingerDiscoveryResult fingerDiscovery)
        {
            if (fingerDiscovery?.Chains == null)
            {
                return 0;
            }

            int count = 0;
            for (int chainIndex = 0; chainIndex < fingerDiscovery.Chains.Count; chainIndex++)
            {
                CCS_AnimationFitStudioFingerChainDiscovery chain = fingerDiscovery.Chains[chainIndex];
                for (int segmentIndex = 0; segmentIndex < chain.Segments.Count; segmentIndex++)
                {
                    if (chain.Segments[segmentIndex].Transform != null)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public static string GetPosePreviewStatusLabel(CCS_AnimationFitStudioPosePreviewStatus status)
        {
            switch (status)
            {
                case CCS_AnimationFitStudioPosePreviewStatus.AppliedAimPose:
                    return "Applied — Aim Pose";
                case CCS_AnimationFitStudioPosePreviewStatus.AppliedIdleLike:
                    return "Applied — Idle-Like";
                case CCS_AnimationFitStudioPosePreviewStatus.Failed:
                    return "Failed";
                case CCS_AnimationFitStudioPosePreviewStatus.SeedPose:
                    return "Seed Pose";
                default:
                    return "Not Loaded";
            }
        }

        public static string GetPoseSourceLabel(CCS_AnimationFitStudioPoseSourceKind source)
        {
            switch (source)
            {
                case CCS_AnimationFitStudioPoseSourceKind.Source:
                    return "Source";
                case CCS_AnimationFitStudioPoseSourceKind.FitTest:
                    return "FitTest";
                case CCS_AnimationFitStudioPoseSourceKind.Seed:
                    return "Seed Pose";
                default:
                    return "None";
            }
        }

        public static bool TryCreateOrLoadFitTestClip(
            AnimationClip sourceClip,
            string sourceAssetPath,
            out AnimationClip fitTestClip,
            out string fitTestAssetPath,
            out string errorMessage)
        {
            fitTestClip = null;
            fitTestAssetPath = string.Empty;
            errorMessage = string.Empty;

            if (sourceClip == null)
            {
                errorMessage = "Load source aim clip first.";
                return false;
            }

            if (IsVendorFbxSubAssetPath(sourceAssetPath))
            {
                errorMessage = "FitTest clip must be duplicated from a CCS-owned .anim, never vendor FBX.";
                return false;
            }

            fitTestAssetPath = CCS_AnimationFitStudioClipResolver.GetFitTestClipPathForSource(sourceAssetPath);
            string folder = Path.GetDirectoryName(fitTestAssetPath)?.Replace('\\', '/');
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                AssetDatabase.Refresh();
            }

            fitTestClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(fitTestAssetPath);
            if (fitTestClip == null)
            {
                if (!AssetDatabase.CopyAsset(sourceAssetPath, fitTestAssetPath))
                {
                    errorMessage = "Could not duplicate source clip to " + fitTestAssetPath;
                    return false;
                }

                AssetDatabase.Refresh();
                fitTestClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(fitTestAssetPath);
            }

            if (fitTestClip == null)
            {
                errorMessage = "Could not load FitTest clip at " + fitTestAssetPath;
                return false;
            }

            return true;
        }

        public static bool SavesAnimatorController => false;

        public static bool TrySaveRuntimeFitTestPose(
            GameObject previewRoot,
            Animator animator,
            AnimationClip sourceClip,
            AnimationClip fitTestClip,
            string fitTestAssetPath,
            float poseTime,
            CCS_AnimationFitStudioPoseEditData poseEditData,
            float gripTightness,
            CCS_AnimationFitStudioPreviewState previewState,
            CCS_AnimationFitStudioSaveMode saveMode,
            bool createBackupBeforeOverwrite,
            out CCS_AnimationFitStudioSaveResult saveResult)
        {
            saveResult = new CCS_AnimationFitStudioSaveResult
            {
                AnimatorControllerModified = false,
            };

            if (previewRoot == null || animator == null || fitTestClip == null)
            {
                saveResult.ErrorMessage = "Preview, animator, and FitTest clip are required before saving.";
                return false;
            }

            if (previewState != null && !CanSaveFitTestPose(previewState))
            {
                saveResult.ErrorMessage = SaveBlockedTposeWarning;
                return false;
            }

            fitTestAssetPath = fitTestAssetPath?.Replace('\\', '/');
            if (!CCS_AnimationFitStudioRuntimeControllerClipUtility.TryResolveControllerFullDrawSaveTarget(
                    out string controllerClipPath,
                    out AnimationClip controllerClip,
                    out string resolveError))
            {
                saveResult.ErrorMessage = resolveError;
                return false;
            }

            if (!string.Equals(fitTestAssetPath, controllerClipPath, System.StringComparison.OrdinalIgnoreCase))
            {
                saveResult.ErrorMessage =
                    "Save target "
                    + fitTestAssetPath
                    + " does not match controller FullDraw motion "
                    + controllerClipPath
                    + ".";
                return false;
            }

            fitTestClip = controllerClip;
            string guidBeforeSave = AssetDatabase.AssetPathToGUID(controllerClipPath);
            if (string.IsNullOrEmpty(guidBeforeSave))
            {
                saveResult.ErrorMessage = "Controller FullDraw clip GUID is missing.";
                return false;
            }

            saveResult.PoseEditsDetected = HasPoseEdits(poseEditData)
                || HasHumanoidPoseEdits(previewState?.HumanoidControl);
            CaptureHumanoidSaveMuscleSnapshots(
                fitTestClip,
                poseTime,
                previewState?.HumanoidControl,
                saveResult);
            saveResult.EditedFingerSegmentsDetected = CountEditedFingerSegments(poseEditData);
            saveResult.FingerBonesFound = CountFingerBonesFound(previewState?.FingerDiscovery);
            saveResult.CurveHashBefore = CCS_AnimationFitStudioCurveHashUtility.ComputeCurveHash(fitTestClip);

            if (saveMode != CCS_AnimationFitStudioSaveMode.OverwriteControllerClip)
            {
                saveResult.ErrorMessage = "Animation Fit Studio only supports overwrite of the controller FullDraw clip.";
                return false;
            }

            AnimationClip curveTemplateClip = fitTestClip;
            saveResult.ClipCurveMode = CCS_AnimationFitStudioClipCurveModeUtility.DetectClipCurveMode(fitTestClip);
            if (!TryWritePoseCurvesIntoClip(
                    previewRoot,
                    animator,
                    curveTemplateClip,
                    fitTestClip,
                    poseTime,
                    poseEditData,
                    gripTightness,
                    previewState,
                    out int armBonesWritten,
                    out int fingerBonesWritten,
                    out int fingerSegmentsWritten,
                    out int humanoidMuscleCurvesWritten,
                    out int transformCurvesWritten,
                    out CCS_AnimationFitStudioClipCurveMode savedClipCurveMode,
                    out string writeError))
            {
                saveResult.ErrorMessage = writeError;
                return false;
            }

            saveResult.ClipCurveMode = savedClipCurveMode;
            saveResult.ArmBonesWritten = armBonesWritten;
            saveResult.FingerBonesWritten = fingerBonesWritten;
            saveResult.FingerCurvesWritten = fingerBonesWritten;
            saveResult.HumanoidMuscleCurvesWritten = humanoidMuscleCurvesWritten;
            saveResult.TransformCurvesWritten = transformCurvesWritten;
            saveResult.SavedAssetPath = controllerClipPath;
            saveResult.GuidPreserved = guidBeforeSave == AssetDatabase.AssetPathToGUID(controllerClipPath);
            saveResult.ControllerAlreadyReferencesClip =
                CCS_AnimationFitStudioRuntimeControllerClipUtility.ControllerReferencesClip(controllerClipPath);
            saveResult.ControllerStillReferencesSavedClip = saveResult.ControllerAlreadyReferencesClip;

            CCS_AnimationFitStudioSaveUtility.FinalizeSavedClipImport(controllerClipPath);
            AnimationClip reloadedClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(controllerClipPath);
            saveResult.CurveHashAfter = CCS_AnimationFitStudioCurveHashUtility.ComputeCurveHash(reloadedClip);
            saveResult.CurveHashChanged = saveResult.CurveHashBefore != saveResult.CurveHashAfter;

            if (previewState?.HumanoidControl != null
                && previewState.HumanoidControl.IsInitialized
                && reloadedClip != null)
            {
                if (!CCS_AnimationFitStudioHumanoidControlUtility.ValidateReloadedClipMatchesState(
                        reloadedClip,
                        poseTime,
                        previewState.HumanoidControl,
                        out string reloadMismatch))
                {
                    saveResult.ErrorMessage = reloadMismatch;
                    saveResult.Success = false;
                    return false;
                }

                previewState.HumanoidControl.MarkSaved();
                CaptureHumanoidSaveMuscleSnapshots(
                    reloadedClip,
                    poseTime,
                    previewState.HumanoidControl,
                    saveResult,
                    afterSave: true);
            }

            if (saveResult.EditedFingerSegmentsDetected > 0 && saveResult.FingerCurvesWritten == 0)
            {
                saveResult.ErrorMessage = "Finger edits were detected but no finger curves were written.";
                saveResult.Success = false;
                return false;
            }

            if (saveResult.ClipCurveMode == CCS_AnimationFitStudioClipCurveMode.HumanoidMuscleCurves
                && saveResult.HumanoidMuscleCurvesWritten <= 0)
            {
                saveResult.ErrorMessage = "Humanoid FullDraw save wrote zero muscle curves.";
                saveResult.Success = false;
                return false;
            }

            if (saveResult.PoseEditsDetected && !saveResult.CurveHashChanged)
            {
                saveResult.ErrorMessage = "Save failed: visible pose edits were not written to the controller clip.";
                saveResult.Success = false;
                return false;
            }

            saveResult.Success = true;
            return true;
        }

        public static bool TrySaveFitTestPose(
            GameObject previewRoot,
            Animator animator,
            AnimationClip sourceClip,
            AnimationClip fitTestClip,
            float poseTime,
            CCS_AnimationFitStudioPoseEditData poseEditData,
            float gripTightness,
            CCS_AnimationFitStudioPreviewState previewState,
            out string errorMessage)
        {
            string fitTestAssetPath = AssetDatabase.GetAssetPath(fitTestClip);
            if (!TrySaveRuntimeFitTestPose(
                    previewRoot,
                    animator,
                    sourceClip,
                    fitTestClip,
                    fitTestAssetPath,
                    poseTime,
                    poseEditData,
                    gripTightness,
                    previewState,
                    CCS_AnimationFitStudioSaveMode.OverwriteControllerClip,
                    createBackupBeforeOverwrite: false,
                    out CCS_AnimationFitStudioSaveResult saveResult))
            {
                errorMessage = saveResult.ErrorMessage;
                return false;
            }

            CCS_AnimationFitStudioSaveUtility.LogOverwriteResult(saveResult);
            errorMessage = string.Empty;
            return true;
        }

        private static bool TryWritePoseCurvesIntoClip(
            GameObject previewRoot,
            Animator animator,
            AnimationClip curveTemplateClip,
            AnimationClip fitTestClip,
            float poseTime,
            CCS_AnimationFitStudioPoseEditData poseEditData,
            float gripTightness,
            CCS_AnimationFitStudioPreviewState previewState,
            out int armBonesWritten,
            out int fingerBonesWritten,
            out int fingerSegmentsWritten,
            out int humanoidMuscleCurvesWritten,
            out int transformCurvesWritten,
            out CCS_AnimationFitStudioClipCurveMode clipCurveMode,
            out string errorMessage)
        {
            armBonesWritten = 0;
            fingerBonesWritten = 0;
            fingerSegmentsWritten = 0;
            humanoidMuscleCurvesWritten = 0;
            transformCurvesWritten = 0;
            clipCurveMode = CCS_AnimationFitStudioClipCurveModeUtility.DetectClipCurveMode(fitTestClip);
            errorMessage = string.Empty;

            if (previewRoot == null || animator == null || curveTemplateClip == null || fitTestClip == null)
            {
                errorMessage = "Preview, animator, template clip, and FitTest clip are required before saving.";
                return false;
            }

            if (!PreparePreviewPoseForSave(
                    animator,
                    curveTemplateClip,
                    fitTestClip,
                    poseTime,
                    poseEditData,
                    gripTightness,
                    previewState))
            {
                errorMessage = "Failed to prepare preview pose before saving.";
                return false;
            }

            if (clipCurveMode == CCS_AnimationFitStudioClipCurveMode.HumanoidMuscleCurves)
            {
                return TryWriteHumanoidMuscleCurvesIntoClip(
                    animator,
                    fitTestClip,
                    poseEditData,
                    previewState,
                    out armBonesWritten,
                    out fingerBonesWritten,
                    out fingerSegmentsWritten,
                    out humanoidMuscleCurvesWritten,
                    out transformCurvesWritten,
                    out errorMessage);
            }

            return TryWriteTransformCurvesIntoClip(
                animator,
                curveTemplateClip,
                fitTestClip,
                poseTime,
                poseEditData,
                previewState,
                out armBonesWritten,
                out fingerBonesWritten,
                out fingerSegmentsWritten,
                out transformCurvesWritten,
                out errorMessage);
        }

        private static bool PreparePreviewPoseForSave(
            Animator animator,
            AnimationClip curveTemplateClip,
            AnimationClip fitTestClip,
            float poseTime,
            CCS_AnimationFitStudioPoseEditData poseEditData,
            float gripTightness,
            CCS_AnimationFitStudioPreviewState previewState)
        {
            if (previewState != null)
            {
                if (previewState.ActivePoseSource == CCS_AnimationFitStudioPoseSourceKind.Seed)
                {
                    ApplySeedPoseInternal(animator, previewState);
                }
                else
                {
                    TryApplyBasePose(
                        previewState,
                        previewState.ActiveBasePoseSource,
                        curveTemplateClip,
                        poseTime,
                        out _);
                }

                CCS_AnimationFitStudioFingerDiscoveryUtility.CaptureBaselines(previewState.FingerDiscovery);
            }
            else
            {
                SampleClipOnAnimator(animator, previewState, fitTestClip, poseTime);
            }

            if (previewState?.HumanoidControl != null && previewState.HumanoidControl.IsInitialized)
            {
                CCS_AnimationFitStudioHumanoidControlUtility.ApplyCurrentMuscleValuesToAnimator(
                    animator,
                    previewState.HumanoidControl);
                CCS_AnimationFitStudioFingerManipulationUtility.ApplyFingerEdits(
                    previewState.FingerDiscovery,
                    poseEditData,
                    gripTightness,
                    previewState.FingerCurlDirection);
            }
            else
            {
                ApplyPartOffsets(
                    animator,
                    previewState?.FingerDiscovery,
                    poseEditData,
                    gripTightness,
                    previewState?.FingerCurlDirection ?? CCS_AnimationFitStudioFingerCurlDirectionKind.Normal);
            }

            return true;
        }

        private static void CaptureHumanoidSaveMuscleSnapshots(
            AnimationClip clip,
            float poseTime,
            CCS_AnimationFitStudioHumanoidControlState humanoidControl,
            CCS_AnimationFitStudioSaveResult saveResult,
            bool afterSave = false)
        {
            if (humanoidControl == null || !humanoidControl.IsInitialized || saveResult == null)
            {
                return;
            }

            saveResult.RightArmDownUpBefore = CCS_AnimationFitStudioHumanoidControlUtility.ReadMuscleValueFromClip(
                clip,
                "Right Arm Down-Up",
                poseTime);
            saveResult.RightArmFrontBackBefore = CCS_AnimationFitStudioHumanoidControlUtility.ReadMuscleValueFromClip(
                clip,
                "Right Arm Front-Back",
                poseTime);
            saveResult.RightHandDownUpBefore = CCS_AnimationFitStudioHumanoidControlUtility.ReadMuscleValueFromClip(
                clip,
                "Right Hand Down-Up",
                poseTime);
            saveResult.RightHandInOutBefore = CCS_AnimationFitStudioHumanoidControlUtility.ReadMuscleValueFromClip(
                clip,
                "Right Hand In-Out",
                poseTime);

            if (afterSave)
            {
                saveResult.RightArmDownUpAfter = saveResult.RightArmDownUpBefore;
                saveResult.RightArmFrontBackAfter = saveResult.RightArmFrontBackBefore;
                saveResult.RightHandDownUpAfter = saveResult.RightHandDownUpBefore;
                saveResult.RightHandInOutAfter = saveResult.RightHandInOutBefore;
                return;
            }

            saveResult.RightArmDownUpAfter = humanoidControl.GetCurrentValue("Right Arm Down-Up");
            saveResult.RightArmFrontBackAfter = humanoidControl.GetCurrentValue("Right Arm Front-Back");
            saveResult.RightHandDownUpAfter = humanoidControl.GetCurrentValue("Right Hand Down-Up");
            saveResult.RightHandInOutAfter = humanoidControl.GetCurrentValue("Right Hand In-Out");
            saveResult.ClampedMuscleNames = humanoidControl.LastClampedMuscles.Count > 0
                ? string.Join(", ", humanoidControl.LastClampedMuscles)
                : string.Empty;
        }

        private static bool TryWriteHumanoidMuscleCurvesIntoClip(
            Animator animator,
            AnimationClip fitTestClip,
            CCS_AnimationFitStudioPoseEditData poseEditData,
            CCS_AnimationFitStudioPreviewState previewState,
            out int armBonesWritten,
            out int fingerBonesWritten,
            out int fingerSegmentsWritten,
            out int humanoidMuscleCurvesWritten,
            out int transformCurvesWritten,
            out string errorMessage)
        {
            armBonesWritten = 0;
            fingerBonesWritten = 0;
            fingerSegmentsWritten = 0;
            transformCurvesWritten = 0;
            errorMessage = string.Empty;

            bool includeFingerMuscles = HasPoseEdits(poseEditData)
                && (CountEditedFingerSegments(poseEditData) > 0
                    || HasFingerCurlEdits(poseEditData));
            HumanPose pose = CCS_AnimationFitStudioHumanoidMuscleWriteUtility.CaptureHumanPose(animator);

            if (previewState?.HumanoidControl != null && previewState.HumanoidControl.IsInitialized)
            {
                humanoidMuscleCurvesWritten = CCS_AnimationFitStudioHumanoidControlUtility.WriteCurrentValuesToClip(
                    fitTestClip,
                    previewState.HumanoidControl,
                    includeFingerMuscles,
                    pose);
            }
            else
            {
                CCS_AnimationFitStudioHumanoidMuscleWriteResult writeResult =
                    CCS_AnimationFitStudioHumanoidMuscleWriteUtility.WriteHumanPoseToClip(
                        fitTestClip,
                        pose,
                        includeFingerMuscles);
                humanoidMuscleCurvesWritten = writeResult.HumanoidMuscleCurvesWritten;
            }

            armBonesWritten = humanoidMuscleCurvesWritten;
            fingerSegmentsWritten = CCS_AnimationFitStudioFingerManipulationUtility.CountWrittenFingerSegments(
                previewState?.FingerDiscovery,
                previewState?.PoseEdits);
            fingerBonesWritten = includeFingerMuscles
                ? CCS_AnimationFitStudioHumanoidMuscleMapping.RightHandFingerMuscleNames.Length
                : 0;

            if (humanoidMuscleCurvesWritten <= 0)
            {
                errorMessage = "Humanoid FullDraw save wrote zero muscle curves.";
                return false;
            }

            EditorUtility.SetDirty(fitTestClip);
            return true;
        }

        private static bool HasFingerCurlEdits(CCS_AnimationFitStudioPoseEditData poseEditData)
        {
            if (poseEditData?.PartEdits == null)
            {
                return false;
            }

            foreach (System.Collections.Generic.KeyValuePair<string, CCS_AnimationFitStudioPartEditState> entry in poseEditData.PartEdits)
            {
                CCS_AnimationFitStudioPartEditState editState = entry.Value;
                if (editState != null && editState.FingerCurl > CCS_AnimationFitStudioPoseEditData.EditEpsilon)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryWriteTransformCurvesIntoClip(
            Animator animator,
            AnimationClip curveTemplateClip,
            AnimationClip fitTestClip,
            float poseTime,
            CCS_AnimationFitStudioPoseEditData poseEditData,
            CCS_AnimationFitStudioPreviewState previewState,
            out int armBonesWritten,
            out int fingerBonesWritten,
            out int fingerSegmentsWritten,
            out int transformCurvesWritten,
            out string errorMessage)
        {
            armBonesWritten = 0;
            fingerBonesWritten = 0;
            fingerSegmentsWritten = 0;
            transformCurvesWritten = 0;
            errorMessage = string.Empty;

            HumanPoseHandler poseHandler = new HumanPoseHandler(animator.avatar, animator.transform);
            HumanPose pose = new HumanPose();
            poseHandler.GetHumanPose(ref pose);

            HashSet<string> allowedAttributes = BuildAllowedCurveAttributes();
            EditorCurveBinding[] sourceBindings = AnimationUtility.GetCurveBindings(curveTemplateClip);
            for (int i = 0; i < sourceBindings.Length; i++)
            {
                EditorCurveBinding binding = sourceBindings[i];
                if (!allowedAttributes.Contains(binding.propertyName))
                {
                    continue;
                }

                AnimationCurve sourceCurve = AnimationUtility.GetEditorCurve(curveTemplateClip, binding);
                if (sourceCurve == null)
                {
                    continue;
                }

                float sampledValue = sourceCurve.Evaluate(poseTime);
                if (TryGetMuscleIndex(binding.propertyName, out int muscleIndex))
                {
                    sampledValue = pose.muscles[muscleIndex];
                }

                float endTime = Mathf.Max(fitTestClip.length, 0.01f);
                AnimationCurve savedCurve = new AnimationCurve(
                    new Keyframe(0f, sampledValue),
                    new Keyframe(endTime, sampledValue));
                AnimationUtility.SetEditorCurve(fitTestClip, binding, savedCurve);
                armBonesWritten++;
                transformCurvesWritten++;
            }

            fingerSegmentsWritten = CCS_AnimationFitStudioFingerManipulationUtility.CountWrittenFingerSegments(
                previewState?.FingerDiscovery,
                previewState?.PoseEdits);
            fingerBonesWritten = WriteFingerTransformCurves(
                animator,
                fitTestClip,
                previewState?.FingerDiscovery,
                previewState?.PoseEdits,
                poseTime);
            transformCurvesWritten += fingerBonesWritten * 4;

            EditorUtility.SetDirty(fitTestClip);
            return true;
        }

        private static int WriteFingerTransformCurves(
            Animator animator,
            AnimationClip fitTestClip,
            CCS_AnimationFitStudioFingerDiscoveryResult fingerDiscovery,
            CCS_AnimationFitStudioPoseEditData poseEditData,
            float poseTime)
        {
            if (animator == null || fitTestClip == null || fingerDiscovery == null)
            {
                return 0;
            }

            int writtenBones = 0;
            float keyTime = Mathf.Max(poseTime, 0.01f);
            for (int c = 0; c < fingerDiscovery.Chains.Count; c++)
            {
                CCS_AnimationFitStudioFingerChainDiscovery chain = fingerDiscovery.Chains[c];
                CCS_AnimationFitStudioFingerChainEditState chainEdit = null;
                bool chainEdited = poseEditData != null
                    && poseEditData.TryGetFingerChainEdit(chain.FingerId, out chainEdit)
                    && chainEdit != null
                    && chainEdit.WasEdited;
                bool quickCurlEdited = poseEditData != null
                    && poseEditData.PartEdits.TryGetValue(chain.FingerId, out CCS_AnimationFitStudioPartEditState quickEdit)
                    && quickEdit != null
                    && quickEdit.FingerCurl > CCS_AnimationFitStudioPoseEditData.EditEpsilon;

                for (int s = 0; s < chain.Segments.Count; s++)
                {
                    Transform segmentTransform = chain.Segments[s].Transform;
                    if (segmentTransform == null)
                    {
                        continue;
                    }

                    bool segmentEdited = chainEdit != null
                        && chainEdited
                        && s < chainEdit.Segments.Length
                        && chainEdit.Segments[s].WasEdited;
                    if (!segmentEdited && !quickCurlEdited)
                    {
                        continue;
                    }

                    string path = AnimationUtility.CalculateTransformPath(segmentTransform, animator.transform);
                    Quaternion localRotation = segmentTransform.localRotation;
                    WriteTransformRotationCurve(fitTestClip, path, "localRotation.x", localRotation.x, keyTime);
                    WriteTransformRotationCurve(fitTestClip, path, "localRotation.y", localRotation.y, keyTime);
                    WriteTransformRotationCurve(fitTestClip, path, "localRotation.z", localRotation.z, keyTime);
                    WriteTransformRotationCurve(fitTestClip, path, "localRotation.w", localRotation.w, keyTime);
                    writtenBones++;
                }
            }

            return writtenBones;
        }

        private static int WriteTransformRotationCurve(
            AnimationClip clip,
            string path,
            string propertyName,
            float value,
            float keyTime)
        {
            EditorCurveBinding binding = EditorCurveBinding.FloatCurve(path, typeof(Transform), propertyName);
            AnimationCurve curve = new AnimationCurve(
                new Keyframe(0f, value),
                new Keyframe(keyTime, value));
            AnimationUtility.SetEditorCurve(clip, binding, curve);
            return 1;
        }

        private static void ApplySeedPoseInternal(Animator animator, CCS_AnimationFitStudioPreviewState previewState)
        {
            if (animator == null)
            {
                return;
            }

            CCS_AnimationFitStudioPlayablePreviewSampler.PrepareAnimatorForPreviewSampling(
                animator,
                previewState);
            EnsureAnimationMode();

            ApplySeedBoneRotation(animator, HumanBodyBones.RightUpperArm, SeedUpperArmEuler);
            ApplySeedBoneRotation(animator, HumanBodyBones.RightLowerArm, SeedLowerArmEuler);
            ApplySeedBoneRotation(animator, HumanBodyBones.RightHand, SeedHandEuler);
            ApplySeedBoneRotation(animator, HumanBodyBones.Chest, SeedChestEuler);
            animator.Update(0f);
        }

        private static void ApplySeedBoneRotation(Animator animator, HumanBodyBones bone, Vector3 euler)
        {
            Transform boneTransform = animator.GetBoneTransform(bone);
            if (boneTransform == null)
            {
                return;
            }

            boneTransform.localRotation = Quaternion.Euler(euler);
        }

        private static BoneSnapshot CaptureBoneSnapshot(Animator animator)
        {
            BoneSnapshot snapshot = new BoneSnapshot();
            if (animator == null)
            {
                return snapshot;
            }

            for (int i = 0; i < MonitoredBones.Length; i++)
            {
                Transform boneTransform = animator.GetBoneTransform(MonitoredBones[i]);
                if (boneTransform == null)
                {
                    continue;
                }

                snapshot.Bones.Add(new BoneSample
                {
                    Bone = MonitoredBones[i],
                    LocalPosition = boneTransform.localPosition,
                    LocalRotation = boneTransform.localRotation,
                });
            }

            return snapshot;
        }

        private static int CountChangedBones(BoneSnapshot before, BoneSnapshot after)
        {
            int changedCount = 0;
            for (int i = 0; i < after.Bones.Count; i++)
            {
                BoneSample afterBone = after.Bones[i];
                if (!before.TryGetBone(afterBone.Bone, out BoneSample beforeBone))
                {
                    changedCount++;
                    continue;
                }

                if (Quaternion.Angle(beforeBone.LocalRotation, afterBone.LocalRotation) > RotationChangeThresholdDegrees
                    || Vector3.Distance(beforeBone.LocalPosition, afterBone.LocalPosition) > PositionChangeThreshold)
                {
                    changedCount++;
                }
            }

            return changedCount;
        }

        private static void ApplyBoneRotationOffset(Animator animator, HumanBodyBones bone, Vector3 eulerOffsetDegrees)
        {
            if (eulerOffsetDegrees == Vector3.zero)
            {
                return;
            }

            Transform boneTransform = animator.GetBoneTransform(bone);
            if (boneTransform == null)
            {
                return;
            }

            Quaternion offset = Quaternion.Euler(eulerOffsetDegrees);
            boneTransform.localRotation = boneTransform.localRotation * offset;
        }

        private static void ApplyMuscleOffsets(
            Animator animator,
            CCS_AnimationFitStudioBodyPartDefinition definition,
            CCS_AnimationFitStudioPartEditState editState,
            float gripTightness)
        {
            if (animator == null || definition.MuscleNames.Length == 0)
            {
                return;
            }

            HumanPoseHandler poseHandler = new HumanPoseHandler(animator.avatar, animator.transform);
            HumanPose pose = new HumanPose();
            poseHandler.GetHumanPose(ref pose);

            float curlAmount = definition.Kind == CCS_AnimationFitStudioBodyPartKind.FingerCurl
                ? Mathf.Clamp01(editState.FingerCurl * Mathf.Max(gripTightness, 0.01f))
                : 0f;

            for (int muscleAxis = 0; muscleAxis < definition.MuscleNames.Length; muscleAxis++)
            {
                string muscleName = definition.MuscleNames[muscleAxis];
                if (string.IsNullOrEmpty(muscleName))
                {
                    continue;
                }

                if (!TryGetMuscleIndex(muscleName, out int muscleIndex))
                {
                    continue;
                }

                if (definition.Kind == CCS_AnimationFitStudioBodyPartKind.FingerCurl)
                {
                    float axisWeight = muscleAxis == 0 ? 0.45f : muscleAxis == 1 ? 0.75f : 1f;
                    pose.muscles[muscleIndex] = Mathf.Lerp(pose.muscles[muscleIndex], 1f, curlAmount * axisWeight);
                }
                else
                {
                    float axisOffset = muscleAxis switch
                    {
                        0 => editState.EulerOffsetDegrees.x,
                        1 => editState.EulerOffsetDegrees.y,
                        _ => editState.EulerOffsetDegrees.z,
                    };
                    pose.muscles[muscleIndex] += axisOffset * CCS_AnimationFitStudioHumanoidMuscleMapping.DegreesToMuscleScale;
                    pose.muscles[muscleIndex] = Mathf.Clamp(pose.muscles[muscleIndex], -1f, 1f);
                }
            }

            poseHandler.SetHumanPose(ref pose);
        }

        private static HashSet<string> BuildAllowedCurveAttributes()
        {
            HashSet<string> allowed = new HashSet<string>();
            IReadOnlyList<CCS_AnimationFitStudioBodyPartDefinition> definitions =
                CCS_AnimationFitStudioBodyPartCatalog.AllDefinitions;
            for (int i = 0; i < definitions.Count; i++)
            {
                CCS_AnimationFitStudioBodyPartDefinition definition = definitions[i];
                for (int m = 0; m < definition.MuscleNames.Length; m++)
                {
                    allowed.Add(definition.MuscleNames[m]);
                }
            }

            allowed.Add("RightUpperArmQ.x");
            allowed.Add("RightUpperArmQ.y");
            allowed.Add("RightUpperArmQ.z");
            allowed.Add("RightUpperArmQ.w");
            allowed.Add("RightLowerArmQ.x");
            allowed.Add("RightLowerArmQ.y");
            allowed.Add("RightLowerArmQ.z");
            allowed.Add("RightLowerArmQ.w");
            allowed.Add("RightHandQ.x");
            allowed.Add("RightHandQ.y");
            allowed.Add("RightHandQ.z");
            allowed.Add("RightHandQ.w");
            return allowed;
        }

        private static Dictionary<string, int> BuildMuscleNameIndex()
        {
            Dictionary<string, int> map = new Dictionary<string, int>();
            for (int i = 0; i < HumanTrait.MuscleCount; i++)
            {
                map[HumanTrait.MuscleName[i]] = i;
            }

            return map;
        }

        private static bool TryGetMuscleIndex(string muscleName, out int muscleIndex)
        {
            return MuscleNameToIndex.TryGetValue(muscleName, out muscleIndex);
        }

        private static bool IsVendorFbxSubAssetPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return false;
            }

            string normalized = assetPath.Replace('\\', '/');
            return normalized.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)
                || normalized.Contains("/YashMakesGames/");
        }

        private sealed class BoneSnapshot
        {
            public List<BoneSample> Bones { get; } = new List<BoneSample>();

            public bool TryGetBone(HumanBodyBones bone, out BoneSample sample)
            {
                for (int i = 0; i < Bones.Count; i++)
                {
                    if (Bones[i].Bone == bone)
                    {
                        sample = Bones[i];
                        return true;
                    }
                }

                sample = default;
                return false;
            }
        }

        private struct BoneSample
        {
            public HumanBodyBones Bone;

            public Vector3 LocalPosition;

            public Quaternion LocalRotation;
        }
    }
}
