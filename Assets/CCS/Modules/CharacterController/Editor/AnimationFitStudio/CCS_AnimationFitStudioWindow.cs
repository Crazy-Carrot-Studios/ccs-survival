using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Editor.EquipmentFitStudio;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AnimationFitStudioWindow
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Editor tool to preview and edit the final right-hand revolver aim pose with fitted weapon.
// PLACEMENT: Open via CCS/Character Controller/Animations/Animation Fit Studio.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Matches Equipment Fit Studio layout. Edits only _FitTest.anim clips. Preview-only runtime.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public sealed partial class CCS_AnimationFitStudioWindow : EditorWindow
    {
        #region Variables

        private AnimationClip sourceClip;
        private string sourceClipPath = string.Empty;
        private string sourceClipDisplayName = string.Empty;
        private AnimationClip fitTestClip;
        private string fitTestClipPath = string.Empty;
        private string fitTestClipDisplayName = string.Empty;
        private string fitTestOutputFolderPath = string.Empty;

        private readonly CCS_AnimationFitStudioPreviewState previewState = new CCS_AnimationFitStudioPreviewState();
        private readonly CCS_EquipmentFitStudioPreviewCamera previewCamera =
            new CCS_EquipmentFitStudioPreviewCamera(CCS_AnimationFitStudioConstants.PreviewCameraObjectName);

        private CCS_EquipmentFitStudioSettings settings;
        private CCS_AnimationFitStudioBasePoseSourceKind selectedPoseSource =
            CCS_AnimationFitStudioPoseSourceCatalog.DefaultPoseSourceKind;
        private int selectedPoseSourceIndex;
        private int fingerCurlDirectionIndex;
        private bool showAdvancedDiagnostics;
        private bool showAxisCalibration;
        private int selectedEditPartIndex = CCS_AnimationFitStudioEditPartCatalog.DefaultEditPartIndex;
        private int selectedFingerSegmentIndex;
        private bool showQuickGrip;
        private float poseTime;
        private float gripTightness = 0.35f;
        private float nudgeDegrees = 5f;
        private bool isDirty;
        private bool previewingEditedClip;
        private AnimationClip activePreviewClip;
        private CCS_AnimationFitStudioPoseSourceKind activePoseSource =
            CCS_AnimationFitStudioPoseSourceKind.None;
        private readonly List<CCS_AnimationFitStudioClipAuditionRow> auditionResults =
            new List<CCS_AnimationFitStudioClipAuditionRow>();
        private Vector2 auditionScrollPosition;
        private CCS_AnimationFitStudioClipDiagnostics clipDiagnostics =
            new CCS_AnimationFitStudioClipDiagnostics();
        private string statusMessage = "Ready.";
        private MessageType statusType = MessageType.Info;

        private Vector2 leftScrollPosition;
        private Vector2 rightScrollPosition;
        private Rect lastPreviewRect;
        private int selectedCameraPresetIndex = 2;
        private CCS_AnimationFitStudioSaveMode saveMode = CCS_AnimationFitStudioSaveMode.OverwriteControllerClip;

        private int selectedSaveModeIndex;
        private CCS_AnimationFitStudioRuntimeControllerClipInfo runtimeControllerClipInfo =
            new CCS_AnimationFitStudioRuntimeControllerClipInfo();

        private bool lastCurveHashChangedOnSave;

        private static readonly string[] SaveModeLabels =
        {
            CCS_AnimationFitStudioRuntimePolicy.OverwriteSaveModeLabel,
        };

        private const float LeftPanelMinWidth = 280f;
        private const float LeftPanelMaxWidth = 340f;
        private const float LeftPanelPreferredWidth = 310f;
        private const float RightPanelMinWidth = 300f;
        private const float RightPanelMaxWidth = 360f;
        private const float MinWindowWidth = 1200f;
        private const float MinWindowHeight = 700f;
        private const float DefaultWindowWidth = 1450f;
        private const float DefaultWindowHeight = 820f;
        private const float BottomActionBarHeight = 80f;

        #endregion

        #region Unity Callbacks

        [MenuItem(CCS_CharacterControllerConstants.AnimationFitStudioMenuPath)]
        public static void OpenWindow()
        {
            CCS_AnimationFitStudioWindow window = GetWindow<CCS_AnimationFitStudioWindow>("Animation Fit Studio");
            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);
            Rect current = window.position;
            float width = Mathf.Max(MinWindowWidth, DefaultWindowWidth);
            float height = Mathf.Max(MinWindowHeight, DefaultWindowHeight);
            window.position = new Rect(
                current.x > 0f ? current.x : (Screen.currentResolution.width - width) * 0.5f,
                current.y > 0f ? current.y : (Screen.currentResolution.height - height) * 0.5f,
                width,
                height);
            window.Show();
        }

        private void OnEnable()
        {
            previewState.PoseEdits.InitializeDefaults();
            selectedEditPartIndex = CCS_AnimationFitStudioEditPartCatalog.DefaultEditPartIndex;
            settings = AssetDatabase.LoadAssetAtPath<CCS_EquipmentFitStudioSettings>(
                CCS_EquipmentConstants.EquipmentFitStudioSettingsPath);
            selectedPoseSource = CCS_AnimationFitStudioPoseSourceCatalog.DefaultPoseSourceKind;
            selectedPoseSourceIndex =
                CCS_AnimationFitStudioPoseSourceCatalog.GetPoseSourceIndex(selectedPoseSource);
            ResolveSourceClipForSelectedPoseSource();
            RefreshRuntimeControllerClipInfo();
            clipDiagnostics = CCS_AnimationFitStudioClipDiagnostics.Build(sourceClip);
            UpdateWindowTitle();
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.delayCall += TryAutoLoadPreviewOnOpen;
        }

        private void TryAutoLoadPreviewOnOpen()
        {
            EditorApplication.delayCall -= TryAutoLoadPreviewOnOpen;
            if (this == null || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (previewState.PreviewPlayer == null)
            {
                LoadPreviewOrWeapon();
            }
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            CCS_AnimationFitStudioPreviewUtility.DestroyPreviewArtifacts(previewState, previewCamera);
        }

        private void OnEditorUpdate()
        {
            if (previewState.PreviewPlayer == null
                || !CCS_AnimationFitStudioPoseUtility.IsMaintainablePreviewStatus(previewState.PosePreviewStatus))
            {
                return;
            }

            CCS_AnimationFitStudioPoseUtility.MaintainPreviewPose(
                previewState,
                selectedPoseSource,
                activePreviewClip ?? sourceClip,
                poseTime,
                previewState.PoseEdits,
                gripTightness,
                GetFingerCurlDirection(),
                activePoseSource,
                activePoseSource == CCS_AnimationFitStudioPoseSourceKind.Seed);
        }

        private CCS_AnimationFitStudioFingerCurlDirectionKind GetFingerCurlDirection()
        {
            return fingerCurlDirectionIndex == 1
                ? CCS_AnimationFitStudioFingerCurlDirectionKind.Inverted
                : CCS_AnimationFitStudioFingerCurlDirectionKind.Normal;
        }

        private void OnGUI()
        {
            CCS_EquipmentFitStudioStyles.EnsureInitialized();
            minSize = new Vector2(MinWindowWidth, MinWindowHeight);
            EditorGUILayout.BeginVertical();
            DrawMainLayout();
            EditorGUILayout.EndVertical();
        }

        private void QueueGuiAction(System.Action action)
        {
            if (action == null)
            {
                return;
            }

            EditorApplication.delayCall += ExecuteDeferredGuiAction;

            void ExecuteDeferredGuiAction()
            {
                EditorApplication.delayCall -= ExecuteDeferredGuiAction;
                if (this == null)
                {
                    return;
                }

                action();
                Repaint();
            }
        }

        #endregion

        #region Private Methods

        private void InitializePartEdits()
        {
            previewState.PoseEdits.InitializeDefaults();
        }

        private void ResolveSourceClipForSelectedPoseSource()
        {
            if (CCS_AnimationFitStudioClipResolver.TryResolveClipForPoseSource(
                    selectedPoseSource,
                    out AnimationClip resolvedClip,
                    out string resolvedPath,
                    out string errorMessage))
            {
                sourceClip = resolvedClip;
                sourceClipPath = resolvedPath;
                sourceClipDisplayName = sourceClip.name;
                RefreshFitTestClipPaths(loadExistingFitTestOnly: true);
                RefreshRuntimeControllerClipInfo();
                statusMessage = "Pose target resolved: " + fitTestClipDisplayName + ".";
                statusType = MessageType.Info;
            }
            else
            {
                sourceClip = null;
                sourceClipPath = string.Empty;
                fitTestClip = null;
                sourceClipDisplayName = GetSaveTargetClipFileName();
                statusMessage = errorMessage;
                statusType = MessageType.Error;
            }

            UpdateWindowTitle();
        }

        private void UpdateWindowTitle()
        {
            titleContent = new GUIContent("Animation Fit Studio — " + GetSaveTargetClipFileName());
        }

        private string GetSaveTargetClipFileName()
        {
            return CCS_AnimationFitStudioPoseSourceCatalog.TryGetDefinition(
                selectedPoseSource,
                out CCS_AnimationFitStudioPoseSourceDefinition definition)
                ? definition.FitTestClipFileName
                : CCS_AnimationFitStudioConstants.DefaultFitTestClipFileName;
        }

        private void RefreshFitTestClipPaths(bool loadExistingFitTestOnly)
        {
            fitTestOutputFolderPath = CCS_AnimationFitStudioClipResolver.GetFitTestOutputFolderPath();
            fitTestClipPath = CCS_AnimationFitStudioClipResolver.GetFitTestClipPathForPoseSource(selectedPoseSource);
            fitTestClipDisplayName = Path.GetFileName(fitTestClipPath);

            if (loadExistingFitTestOnly)
            {
                fitTestClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(fitTestClipPath);
            }
        }

        private void RefreshRuntimeControllerClipInfo()
        {
            CCS_AnimationFitStudioRuntimeControllerClipUtility.TryQueryRuntimeAimClips(
                fitTestClipPath,
                out runtimeControllerClipInfo);
        }

        private void OnPoseSourceChanged(int newSourceIndex)
        {
            CCS_AnimationFitStudioBasePoseSourceKind newSource =
                CCS_AnimationFitStudioPoseSourceCatalog.GetPoseSourceKindFromIndex(newSourceIndex);
            if (newSource == selectedPoseSource)
            {
                return;
            }

            selectedPoseSource = newSource;
            selectedPoseSourceIndex = newSourceIndex;
            previewingEditedClip = false;
            isDirty = false;
            InitializePartEdits();
            gripTightness = 0.35f;
            ResolveSourceClipForSelectedPoseSource();
            clipDiagnostics = CCS_AnimationFitStudioClipDiagnostics.Build(sourceClip);
            RefreshRuntimeControllerClipInfo();

            if (previewState.PreviewPlayer != null)
            {
                if (CCS_AnimationFitStudioPoseSourceCatalog.UsesDirectFitTestAssetPath(selectedPoseSource)
                    && fitTestClip != null)
                {
                    previewingEditedClip = true;
                }

                ApplyCurrentPosePreview();
            }
        }

        private void LoadPreviewOrWeapon()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                statusMessage = "Exit Play Mode before loading the editor preview.";
                statusType = MessageType.Warning;
                return;
            }

            previewCamera.EnsureCamera(settings);
            if (!CCS_AnimationFitStudioPreviewUtility.TryLoadPreview(
                    previewState,
                    sourceClip,
                    poseTime,
                    out string errorMessage))
            {
                statusMessage = errorMessage;
                statusType = MessageType.Error;
                return;
            }

            previewingEditedClip = false;
            FrameDefaultHandAndWeapon();
            if (CCS_AnimationFitStudioPoseSourceCatalog.UsesDirectFitTestAssetPath(selectedPoseSource)
                && fitTestClip != null)
            {
                previewingEditedClip = true;
            }

            ApplyCurrentPosePreview();

            if (!string.IsNullOrEmpty(previewState.ProfileWarningMessage))
            {
                statusMessage = previewState.ProfileWarningMessage;
                statusType = MessageType.Warning;
            }
            else
            {
                statusMessage = BuildPoseStatusMessage("Preview player and fitted revolver loaded.");
                statusType = MessageType.Info;
            }
        }

        private void ResetPose()
        {
            selectedEditPartIndex = CCS_AnimationFitStudioEditPartCatalog.DefaultEditPartIndex;
            selectedFingerSegmentIndex = 0;
            showQuickGrip = false;
            gripTightness = 0.35f;
            fingerCurlDirectionIndex = 0;
            isDirty = false;
            previewingEditedClip = false;
            previewState.HumanoidControl.Clear();
            InitializePartEdits();
            ApplyCurrentPosePreview();
            statusMessage = "Pose reset to "
                + CCS_AnimationFitStudioPoseSourceCatalog.GetPoseSourceDisplayLabel(selectedPoseSource)
                + ".";
            statusType = MessageType.Info;
        }

        private bool IsHumanoidClipMode()
        {
            AnimationClip clip = fitTestClip != null ? fitTestClip : sourceClip;
            return clip != null
                && CCS_AnimationFitStudioClipCurveModeUtility.DetectClipCurveMode(clip)
                    == CCS_AnimationFitStudioClipCurveMode.HumanoidMuscleCurves;
        }

        private void ApplyTestAimOffset()
        {
            if (!IsHumanoidClipMode())
            {
                statusMessage = "Test aim offset requires Humanoid Muscle Curves mode.";
                statusType = MessageType.Warning;
                return;
            }

            if (!previewState.HumanoidControl.IsInitialized)
            {
                statusMessage = "Load and preview pose before applying test aim offset.";
                statusType = MessageType.Warning;
                return;
            }

            if (CCS_AnimationFitStudioHumanoidControlUtility.ApplyTestAimOffset(previewState.HumanoidControl))
            {
                MarkDirtyAndRefreshPreview();
                statusMessage = previewState.HumanoidControl.LastEditFeedback;
                statusType = MessageType.Info;
                return;
            }

            statusMessage = string.IsNullOrEmpty(previewState.HumanoidControl.LastEditFeedback)
                ? "Test aim offset made no changes."
                : previewState.HumanoidControl.LastEditFeedback;
            statusType = MessageType.Warning;
        }

        private void ApplyCurrentPosePreview()
        {
            AnimationClip previewClip = previewingEditedClip && fitTestClip != null ? fitTestClip : sourceClip;
            ApplyPreviewPose(
                previewClip,
                poseTime,
                previewingEditedClip && fitTestClip != null
                    ? CCS_AnimationFitStudioPoseSourceKind.FitTest
                    : CCS_AnimationFitStudioPoseSourceKind.Source,
                useSeedPose: activePoseSource == CCS_AnimationFitStudioPoseSourceKind.Seed);
        }

        private void ApplyPreviewPose(
            AnimationClip clip,
            float time,
            CCS_AnimationFitStudioPoseSourceKind runtimePoseKind,
            bool useSeedPose = false)
        {
            if (previewState.PreviewPlayer == null)
            {
                statusMessage = "Load preview first.";
                statusType = MessageType.Warning;
                return;
            }

            if (!useSeedPose
                && CCS_AnimationFitStudioPoseSourceCatalog.UsesClipSampling(selectedPoseSource)
                && clip == null)
            {
                statusMessage = "Clip is missing for clip-based pose source.";
                statusType = MessageType.Error;
                return;
            }

            activePreviewClip = clip;
            activePoseSource = useSeedPose
                ? CCS_AnimationFitStudioPoseSourceKind.Seed
                : runtimePoseKind;

            CCS_AnimationFitStudioApplyPoseResult result = CCS_AnimationFitStudioPoseUtility.TryApplyPreviewPose(
                previewState,
                selectedPoseSource,
                clip,
                time,
                previewState.PoseEdits,
                gripTightness,
                GetFingerCurlDirection(),
                runtimePoseKind,
                useSeedPose);

            CCS_AnimationFitStudioPreviewUtility.RefreshWeaponAttachmentAfterPoseSample(previewState);
            clipDiagnostics = result.Diagnostics
                ?? CCS_AnimationFitStudioClipDiagnostics.Build(
                    clip,
                    previewState.ChangedBoneCount,
                    previewState.AimPoseScore,
                    previewState.LastSampleMethod);

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                statusMessage = result.ErrorMessage;
                statusType = MessageType.Error;
                Repaint();
                return;
            }

            if (!result.Success)
            {
                statusMessage = string.IsNullOrEmpty(result.WarningMessage)
                    ? CCS_AnimationFitStudioPoseUtility.ClipDidNotAffectSkeletonWarning
                    : result.WarningMessage;
                statusType = MessageType.Error;
            }
            else
            {
                statusMessage = BuildPoseStatusMessage("Final pose preview updated.");
                statusType = MessageType.Info;
            }

            previewCamera.RenderNow();
            Repaint();
        }

        private string BuildPoseStatusMessage(string prefix)
        {
            return prefix
                + "\nPose Source: "
                + CCS_AnimationFitStudioPoseSourceCatalog.GetPoseSourceDisplayLabel(selectedPoseSource)
                + "\nPose Preview: "
                + CCS_AnimationFitStudioPoseUtility.GetPosePreviewStatusLabel(previewState.PosePreviewStatus)
                + "\nFinger Bones: "
                + (previewState.FingerDiscovery != null
                    ? previewState.FingerDiscovery.GetSummaryLabel()
                    : "Unknown");
        }

        private void RunAuditionAimClips(bool silent)
        {
            if (previewState.PreviewAnimator == null)
            {
                if (!silent)
                {
                    statusMessage = "Load preview first.";
                    statusType = MessageType.Warning;
                }

                return;
            }

            auditionResults.Clear();
            auditionResults.AddRange(CCS_AnimationFitStudioClipAuditionUtility.RunAudition(
                previewState.PreviewAnimator,
                previewState));

            if (!silent)
            {
                statusMessage = "Audition complete. Review scores in Advanced / Diagnostics.";
                statusType = MessageType.Info;
            }
        }

        private void UseDefaultOneHandAimSeedPose()
        {
            if (previewState.PreviewPlayer == null)
            {
                statusMessage = "Load preview first.";
                statusType = MessageType.Warning;
                return;
            }

            previewingEditedClip = false;
            activePreviewClip = null;
            ApplyPreviewPose(
                null,
                0f,
                CCS_AnimationFitStudioPoseSourceKind.Seed,
                useSeedPose: true);
            statusMessage = CCS_AnimationFitStudioPoseUtility.SeedPoseFitTestNotice;
            statusType = MessageType.Warning;
        }

        private void ReResolveClip()
        {
            ResolveSourceClipForSelectedPoseSource();
            clipDiagnostics = CCS_AnimationFitStudioClipDiagnostics.Build(sourceClip);
            statusMessage = sourceClip != null
                ? "Re-resolved clip: " + sourceClipDisplayName + "."
                : "Clip re-resolve failed.";
            statusType = sourceClip != null ? MessageType.Info : MessageType.Error;
        }

        private bool CanEditPoseParts()
        {
            return CCS_AnimationFitStudioPoseUtility.CanEditPoseParts(previewState);
        }

        private bool CanSaveFitTestPose()
        {
            return sourceClip != null
                && CCS_AnimationFitStudioPoseUtility.CanSaveFitTestPose(previewState);
        }

        private void CreateOrLoadFitTestClip()
        {
            if (sourceClip == null)
            {
                statusMessage = "Save target source clip is missing.";
                statusType = MessageType.Error;
                return;
            }

            bool existedBefore = File.Exists(
                CCS_AnimationFitStudioClipResolver.GetFitTestClipPathForSource(sourceClipPath));

            if (!CCS_AnimationFitStudioPoseUtility.TryCreateOrLoadFitTestClip(
                    sourceClip,
                    sourceClipPath,
                    out fitTestClip,
                    out fitTestClipPath,
                    out string errorMessage))
            {
                statusMessage = errorMessage;
                statusType = MessageType.Error;
                return;
            }

            RefreshFitTestClipPaths(loadExistingFitTestOnly: false);
            statusMessage = existedBefore
                ? "Loaded existing FitTest clip. Source clip was not modified."
                : "Created FitTest clip from source. Source clip was not modified.";
            statusType = MessageType.Info;

            if (previewState.PreviewPlayer != null)
            {
                ApplyCurrentPosePreview();
            }
        }

        private void PreviewClip(AnimationClip clip)
        {
            if (previewState.PreviewPlayer == null)
            {
                statusMessage = "Load preview first.";
                statusType = MessageType.Warning;
                return;
            }

            if (clip == null)
            {
                statusMessage = "Clip is missing.";
                statusType = MessageType.Error;
                return;
            }

            previewingEditedClip = false;
            ApplyPreviewPose(clip, poseTime, CCS_AnimationFitStudioPoseSourceKind.Source);
        }

        private void PreviewEditedPose()
        {
            if (previewState.PreviewPlayer == null || previewState.PreviewAnimator == null)
            {
                statusMessage = "Load preview first.";
                statusType = MessageType.Warning;
                return;
            }

            AnimationClip previewClip = fitTestClip ?? sourceClip;
            if (previewClip == null)
            {
                statusMessage = "Resolve source clip or create FitTest clip first.";
                statusType = MessageType.Error;
                return;
            }

            previewingEditedClip = true;
            ApplyPreviewPose(
                previewClip,
                poseTime,
                fitTestClip != null
                    ? CCS_AnimationFitStudioPoseSourceKind.FitTest
                    : CCS_AnimationFitStudioPoseSourceKind.Source);
        }

        private void RefreshPreview()
        {
            if (previewState.PreviewPlayer == null)
            {
                return;
            }

            if (previewingEditedClip)
            {
                PreviewEditedPose();
            }
            else
            {
                ApplyCurrentPosePreview();
            }
        }

        private void SaveRuntimeFitTest(bool reimportAfterSave)
        {
            if (!CanSaveFitTestPose())
            {
                statusMessage = CCS_AnimationFitStudioPoseUtility.SaveBlockedTposeWarning;
                statusType = MessageType.Error;
                return;
            }

            if (!CCS_AnimationFitStudioRuntimeControllerClipUtility.TryResolveControllerFullDrawSaveTarget(
                    out string controllerClipPath,
                    out AnimationClip controllerClip,
                    out string resolveError))
            {
                statusMessage = resolveError;
                statusType = MessageType.Error;
                return;
            }

            fitTestClipPath = controllerClipPath;
            fitTestClip = controllerClip;

            AnimationClip curveTemplateClip = sourceClip != null ? sourceClip : fitTestClip;
            if (!CCS_AnimationFitStudioPoseUtility.TrySaveRuntimeFitTestPose(
                    previewState.PreviewPlayer,
                    previewState.PreviewAnimator,
                    curveTemplateClip,
                    fitTestClip,
                    fitTestClipPath,
                    poseTime,
                    previewState.PoseEdits,
                    gripTightness,
                    previewState,
                    saveMode,
                    createBackupBeforeOverwrite: false,
                    out CCS_AnimationFitStudioSaveResult saveResult))
            {
                statusMessage = saveResult.ErrorMessage;
                statusType = MessageType.Error;
                return;
            }

            lastCurveHashChangedOnSave = saveResult.CurveHashChanged;
            CCS_AnimationFitStudioSaveUtility.LogOverwriteResult(saveResult);
            RefreshRuntimeControllerClipInfo();
            previewingEditedClip = true;
            isDirty = false;
            statusMessage = saveResult.PoseEditsDetected
                ? "Saved Runtime FullDraw to "
                  + Path.GetFileName(saveResult.SavedAssetPath)
                  + ". Curve hash changed: "
                  + (saveResult.CurveHashChanged ? "true" : "false")
                : "No pose changes detected; curve hash unchanged.";
            statusType = MessageType.Info;
        }

        private void SaveFitTestPose()
        {
            SaveRuntimeFitTest(reimportAfterSave: true);
        }

        private void ResetUnsavedEdits()
        {
            ResetPose();
        }

        private void NudgePart(string partId, Vector3 deltaEuler)
        {
            if (IsHumanoidClipMode() && previewState.HumanoidControl.IsInitialized)
            {
                bool changed = false;
                if (!Mathf.Approximately(deltaEuler.x, 0f))
                {
                    changed |= TryHumanoidNudge(
                        partId,
                        CCS_AnimationFitStudioHumanoidControlAxis.Pitch,
                        Mathf.Sign(deltaEuler.x),
                        Mathf.Abs(deltaEuler.x));
                }

                if (!Mathf.Approximately(deltaEuler.y, 0f))
                {
                    changed |= TryHumanoidNudge(
                        partId,
                        CCS_AnimationFitStudioHumanoidControlAxis.Yaw,
                        Mathf.Sign(deltaEuler.y),
                        Mathf.Abs(deltaEuler.y));
                }

                if (!Mathf.Approximately(deltaEuler.z, 0f))
                {
                    changed |= TryHumanoidNudge(
                        partId,
                        CCS_AnimationFitStudioHumanoidControlAxis.Roll,
                        Mathf.Sign(deltaEuler.z),
                        Mathf.Abs(deltaEuler.z));
                }

                if (changed)
                {
                    MarkDirtyAndRefreshPreview();
                }
                else
                {
                    statusMessage = previewState.HumanoidControl.LastEditFeedback;
                    statusType = previewState.HumanoidControl.LastClampedMuscles.Count > 0
                        ? MessageType.Warning
                        : MessageType.Info;
                    Repaint();
                }

                return;
            }

            if (!previewState.PoseEdits.PartEdits.TryGetValue(partId, out CCS_AnimationFitStudioPartEditState editState))
            {
                return;
            }

            editState.EulerOffsetDegrees += deltaEuler;
            MarkDirtyAndRefreshPreview();
        }

        private bool TryHumanoidNudge(
            string partId,
            CCS_AnimationFitStudioHumanoidControlAxis axis,
            float sign,
            float nudgeDegrees)
        {
            return CCS_AnimationFitStudioHumanoidControlUtility.TryNudgePart(
                previewState.HumanoidControl,
                partId,
                axis,
                sign,
                nudgeDegrees,
                out _);
        }

        private bool IsPartAxisEnabled(string partId, CCS_AnimationFitStudioHumanoidControlAxis axis)
        {
            if (CCS_AnimationFitStudioEditPartCatalog.TryGetDefinitionByPartId(
                    partId,
                    out CCS_AnimationFitStudioEditPartDefinition definition)
                && definition.Kind == CCS_AnimationFitStudioEditPartKind.Finger)
            {
                return previewState.PreviewPlayer != null && previewState.FingerBonesFound;
            }

            if (IsHumanoidClipMode())
            {
                return CCS_AnimationFitStudioHumanoidControlUtility.CanNudgePartAxis(partId, axis);
            }

            return true;
        }

        private void ResetPart(string partId)
        {
            if (IsHumanoidClipMode() && previewState.HumanoidControl.IsInitialized)
            {
                CCS_AnimationFitStudioHumanoidControlUtility.ResetPartMusclesToBaseline(
                    previewState.HumanoidControl,
                    partId);
                MarkDirtyAndRefreshPreview();
                return;
            }

            if (!previewState.PoseEdits.PartEdits.TryGetValue(partId, out CCS_AnimationFitStudioPartEditState editState))
            {
                return;
            }

            editState.EulerOffsetDegrees = Vector3.zero;
            editState.FingerCurl = 0f;
            MarkDirtyAndRefreshPreview();
        }

        private void NudgeSelectedFingerEuler(Vector3 deltaEuler)
        {
            ModifySelectedFingerSegments(segmentEdit =>
            {
                segmentEdit.PitchDegrees += deltaEuler.x;
                segmentEdit.YawDegrees += deltaEuler.y;
                segmentEdit.RollDegrees += deltaEuler.z;
            });
        }

        private void NudgeSelectedFingerCurl(float deltaDegrees)
        {
            ModifySelectedFingerSegments(segmentEdit => segmentEdit.CurlDegrees += deltaDegrees);
        }

        private void NudgeSelectedFingerSpread(float deltaDegrees)
        {
            ModifySelectedFingerSegments(segmentEdit => segmentEdit.SpreadDegrees += deltaDegrees);
        }

        private void ResetSelectedFinger()
        {
            if (!TryGetSelectedEditPart(out CCS_AnimationFitStudioEditPartDefinition editPart)
                || editPart.Kind != CCS_AnimationFitStudioEditPartKind.Finger)
            {
                return;
            }

            CCS_AnimationFitStudioFingerChainEditState chainEdit =
                previewState.PoseEdits.GetOrCreateFingerChainEdit(editPart.PartId);
            chainEdit.ResetSegments();
            if (previewState.PoseEdits.PartEdits.TryGetValue(editPart.PartId, out CCS_AnimationFitStudioPartEditState quickEdit))
            {
                quickEdit.FingerCurl = 0f;
            }

            MarkDirtyAndRefreshPreview();
        }

        private void ResetSelectedPart()
        {
            if (!TryGetSelectedEditPart(out CCS_AnimationFitStudioEditPartDefinition editPart))
            {
                return;
            }

            if (editPart.Kind == CCS_AnimationFitStudioEditPartKind.Finger)
            {
                ResetSelectedFinger();
                return;
            }

            ResetPart(editPart.PartId);
        }

        private void ModifySelectedFingerSegments(
            System.Action<CCS_AnimationFitStudioFingerSegmentEditState> modifySegment)
        {
            if (modifySegment == null
                || !TryGetSelectedEditPart(out CCS_AnimationFitStudioEditPartDefinition editPart)
                || editPart.Kind != CCS_AnimationFitStudioEditPartKind.Finger)
            {
                return;
            }

            CCS_AnimationFitStudioFingerChainEditState chainEdit =
                previewState.PoseEdits.GetOrCreateFingerChainEdit(editPart.PartId);
            if (selectedFingerSegmentIndex <= 0)
            {
                for (int i = 0; i < chainEdit.Segments.Length; i++)
                {
                    modifySegment(chainEdit.Segments[i]);
                }
            }
            else
            {
                int segmentIndex = selectedFingerSegmentIndex - 1;
                if (segmentIndex >= 0 && segmentIndex < chainEdit.Segments.Length)
                {
                    modifySegment(chainEdit.Segments[segmentIndex]);
                }
            }

            MarkDirtyAndRefreshPreview();
        }

        private bool TryGetSelectedEditPart(out CCS_AnimationFitStudioEditPartDefinition editPart)
        {
            return CCS_AnimationFitStudioEditPartCatalog.TryGetDefinition(selectedEditPartIndex, out editPart);
        }

        private bool IsSelectedPartFinger()
        {
            return TryGetSelectedEditPart(out CCS_AnimationFitStudioEditPartDefinition editPart)
                && editPart.Kind == CCS_AnimationFitStudioEditPartKind.Finger;
        }

        private void MarkDirtyAndRefreshPreview()
        {
            MarkDirty();
            ApplyCurrentPosePreview();
            previewingEditedClip = true;
        }

        private void MarkDirty()
        {
            isDirty = true;
        }

        private void RunValidation()
        {
            CCS_SurvivalValidationResult result =
                CCS_AnimationFitStudioValidationUtility.ValidateAnimationFitStudioFoundation();
            statusMessage = result.Message;
            statusType = result.IsSuccess ? MessageType.Info : MessageType.Error;
        }

        private string GetEditedPartsLabel()
        {
            List<string> edited = new List<string>();
            foreach (KeyValuePair<string, CCS_AnimationFitStudioPartEditState> pair in previewState.PoseEdits.PartEdits)
            {
                if (pair.Value != null && pair.Value.WasEdited)
                {
                    edited.Add(pair.Key);
                }
            }

            foreach (KeyValuePair<string, CCS_AnimationFitStudioFingerChainEditState> pair in previewState.PoseEdits.FingerEdits)
            {
                if (pair.Value != null && pair.Value.WasEdited)
                {
                    edited.Add(pair.Key + "_segments");
                }
            }

            return edited.Count == 0 ? "(none)" : string.Join(", ", edited);
        }

        private static int GetNudgeIndex(float degrees)
        {
            if (degrees <= 1.5f)
            {
                return 0;
            }

            return degrees >= 10f ? 2 : 1;
        }

        private float GetLeftPanelWidth()
        {
            return Mathf.Clamp(position.width * 0.22f, LeftPanelMinWidth, LeftPanelMaxWidth);
        }

        private float GetRightPanelWidth()
        {
            return Mathf.Clamp(position.width * 0.24f, RightPanelMinWidth, RightPanelMaxWidth);
        }

        private void FrameDefaultHandAndWeapon()
        {
            previewCamera.EnsureCamera(settings);
            previewCamera.SetFrameContext(
                previewState.PreviewPlayer,
                CCS_EquipmentConstants.HandSocketRightId);
            Transform frameTarget = CCS_AnimationFitStudioPreviewUtility.GetDefaultCameraFrameTarget(previewState);
            if (frameTarget != null)
            {
                previewCamera.FrameTransform(frameTarget, 1.15f);
            }
            else
            {
                previewCamera.ApplyPreset(
                    CCS_EquipmentFitStudioCameraPreset.RightHand,
                    previewState.PreviewPlayer,
                    CCS_EquipmentConstants.HandSocketRightId);
            }
        }

        #endregion
    }
}
