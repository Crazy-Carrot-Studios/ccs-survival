using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController.Editor;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.Weapons.Editor;
using CCS.Project;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations.Rigging;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioWindow
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Editor-only socket and IK tuning tool with live preview viewport.
// PLACEMENT: Open via CCS/Character Controller/Equipment/Equipment Fit Studio.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Preview objects are temporary and must never be saved to assets.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public sealed partial class CCS_EquipmentFitStudioWindow : EditorWindow
    {
        #region Variables

        private CCS_EquipmentFitStudioSettings settings;

        private CCS_EquipmentFitStudioSelectionState state = new CCS_EquipmentFitStudioSelectionState();

        private CCS_EquipmentFitStudioPreviewCamera previewCamera = new CCS_EquipmentFitStudioPreviewCamera();

        private CCS_EquipmentFitStudioPreviewItem previewItem = new CCS_EquipmentFitStudioPreviewItem();

        private Vector2 scrollPosition;

        private Vector2 presetScrollPosition;

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

        private const float PresetOverlayHeight = 72f;

        private const float WrappedButtonMinWidth = 108f;

        private Rect lastPreviewRect;

        private string statusMessage = "Ready.";

        private MessageType statusType = MessageType.Info;

        private float previewRigWeight;

        private float previewRightHandIkWeight;

        private float previewLeftHandIkWeight;

        private float previewAimWeight;

        private readonly string[] ikTargetNames =
        {
            CCS_EquipmentConstants.RightHandIkTargetObjectName,
            CCS_EquipmentConstants.RightElbowHintObjectName,
            CCS_EquipmentConstants.LeftHandIkTargetObjectName,
            CCS_EquipmentConstants.LeftElbowHintObjectName,
            CCS_EquipmentConstants.WeaponAimTargetObjectName,
        };

        #endregion

        #region Unity Callbacks

        [MenuItem(CCS_EquipmentConstants.EquipmentFitStudioMenuPath)]
        public static void OpenWindow()
        {
            CCS_EquipmentFitStudioWindow window = GetWindow<CCS_EquipmentFitStudioWindow>("Equipment Fit Studio");
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
            CCS_EquipmentFitStudioImGuiUtility.EnsureLogSubscription();
            EditorApplication.delayCall += DeferredEnsureEquipmentFitStudioAssets;
            settings = AssetDatabase.LoadAssetAtPath<CCS_EquipmentFitStudioSettings>(
                CCS_EquipmentConstants.EquipmentFitStudioSettingsPath);
            previewCamera.EnsureCamera(settings);
            SceneView.duringSceneGui += OnSceneGui;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            LoadFitTargetFromEditorPrefs();
            SyncFitTargetRoutingToState();
            if (!EditorApplication.isPlaying)
            {
                RequestAutoLoadPreview(deferUntilAfterGui: true);
            }
        }

        private void DeferredEnsureEquipmentFitStudioAssets()
        {
            EditorApplication.delayCall -= DeferredEnsureEquipmentFitStudioAssets;
            if (this == null)
            {
                return;
            }

            CCS_EquipmentFitStudioProfileBuilder.EnsureEquipmentFitStudioAssets();
        }

        private void OnDisable()
        {
            EditorApplication.delayCall -= DeferredEnsureEquipmentFitStudioAssets;
            SceneView.duringSceneGui -= OnSceneGui;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            CCS_EquipmentFitStudioPlayModeAimFitUtility.CleanupEditorOverrides(state.PlayerRoot);
            CCS_EquipmentFitStudioIkDiagnosticsUtility.ResetIkPreviewToZero(state.PlayerRoot);
            CCS_EquipmentFitStudioPosePreviewUtility.ClearAllPosePreview();
            CleanupTemporaryObjects(resetIkPreviewWeights: true, clearTestAttachments: true);
            CCS_EquipmentFitStudioPreviewPlayerUtility.ClearPreviewPlayer();
            state.PlayerRoot = null;
            state.UsesEditorPreviewPlayer = false;
            state.ForceAimPoseActive = false;
            CCS_EquipmentFitStudioImGuiUtility.ResetForWindowClose();
        }

        private void OnGUI()
        {
            CCS_EquipmentFitStudioStyles.EnsureInitialized();
            minSize = new Vector2(MinWindowWidth, MinWindowHeight);
            CCS_EquipmentFitStudioImGuiUtility.DrawVerticalScope(DrawRevampedLayout);
            ProcessRevampedDeferredActions();
        }

        #endregion

        #region Private Methods

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("CCS Equipment Fit Studio", CCS_EquipmentFitStudioStyles.TitleLabel, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField(
                CCS_EquipmentConstants.EquipmentFitStudioVersionLabel,
                CCS_EquipmentFitStudioStyles.HeaderMetaLabel,
                GUILayout.Width(60f));
            DrawStatusBadge();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatusBadge()
        {
            GUIStyle badgeStyle = CCS_EquipmentFitStudioStyles.StatusOkLabel;
            string badge = "GREEN: Preview Zeroed";
            if (RequiresCleanup())
            {
                badgeStyle = CCS_EquipmentFitStudioStyles.StatusErrorLabel;
                badge = "RED: Cleanup Required";
            }
            else if (state.HasPendingSaveCapture)
            {
                badgeStyle = CCS_EquipmentFitStudioStyles.StatusWarnLabel;
                badge = "YELLOW: Unsaved Captured Values";
            }
            else if (state.IkPendingChange.HasCaptured)
            {
                badgeStyle = CCS_EquipmentFitStudioStyles.StatusWarnLabel;
                badge = "YELLOW: Unsaved Changes";
            }

            EditorGUILayout.LabelField(badge, badgeStyle, GUILayout.Width(220f));
        }

        private void DrawSetupSummaryBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(
                "Player: " + (state.PlayerRoot != null ? state.PlayerRoot.name : "None")
                + "  |  Weapon: " + state.SelectedWeaponId
                + "  |  Socket: " + CCS_EquipmentFitStudioRevolverFitUtility.GetSocketDisplayLabel(state.SelectedSocketId),
                EditorStyles.wordWrappedLabel,
                GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawWorkflowPanel()
        {
            float leftPanelWidth = GetLeftPanelWidth();
            scrollPosition = EditorGUILayout.BeginScrollView(
                scrollPosition,
                GUILayout.Width(leftPanelWidth),
                GUILayout.MinWidth(LeftPanelMinWidth),
                GUILayout.MaxWidth(LeftPanelMaxWidth),
                GUILayout.ExpandHeight(true));

            DrawActiveFitTargetPanel();
            EditorGUILayout.Space(4f);
            CCS_EquipmentFitStudioWorkflowGuide.DrawProfileStatusPanel(
                state.HasUnsavedChanges,
                previewItem.IsSpawned,
                HasTestAttachments(),
                HasNonZeroIkPreviewWeights());
            EditorGUILayout.Space(6f);
            CCS_EquipmentFitStudioWorkflowAccordion.DrawDecisionHelper(state.SelectedSocketId);
            EditorGUILayout.Space(6f);
            CCS_EquipmentFitStudioWorkflowAccordion.DrawAccordion(GetActiveWorkflowStep(), BuildWorkflowCallbacks());

            EditorGUILayout.EndScrollView();
        }

        private void DrawPreviewPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            Rect previewRect = GUILayoutUtility.GetRect(
                GUIContent.none,
                GUIStyle.none,
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true),
                GUILayout.MinHeight(400f));
            lastPreviewRect = previewRect;

            if (previewRect.width > 1f && previewRect.height > 1f)
            {
                EditorGUI.DrawRect(previewRect, new Color(0.06f, 0.07f, 0.09f));
                Rect innerRect = new Rect(
                    previewRect.x + 3f,
                    previewRect.y + 3f,
                    previewRect.width - 6f,
                    previewRect.height - 6f);
                previewCamera.EnsureRenderTexture(
                    Mathf.Max(64, (int)innerRect.width),
                    Mathf.Max(64, (int)innerRect.height));
                previewCamera.HandleInput(innerRect, Event.current);
                previewCamera.RenderNow();
                if (previewCamera.RenderTexture != null)
                {
                    GUI.DrawTexture(innerRect, previewCamera.RenderTexture, ScaleMode.StretchToFill, false);
                }

                DrawPreviewOverlay(innerRect);
                DrawPreviewOverlayStatus(innerRect);
                DrawPreviewPresetOverlay(innerRect);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawPreviewOverlayStatus(Rect previewRect)
        {
            string zeroedText = previewItem.IsSpawned
                ? (previewItem.IsZeroed ? "GREEN: Preview Zeroed" : "RED: Preview Not Zeroed")
                : "Preview: not spawned";
            GUIStyle style = previewItem.IsSpawned && previewItem.IsZeroed
                ? CCS_EquipmentFitStudioStyles.StatusOkLabel
                : previewItem.IsSpawned
                    ? CCS_EquipmentFitStudioStyles.StatusErrorLabel
                    : CCS_EquipmentFitStudioStyles.StatusWarnLabel;
            Rect statusRect = new Rect(previewRect.x + 8f, previewRect.yMax - 28f, previewRect.width - 16f, 22f);
            GUI.Label(statusRect, zeroedText, style);
        }

        private void DrawPreviewPresetOverlay(Rect previewRect)
        {
            Rect presetRect = new Rect(
                previewRect.x + 8f,
                previewRect.yMax - PresetOverlayHeight - 32f,
                previewRect.width - 16f,
                PresetOverlayHeight);
            DrawPreviewPresetButtonsAtRect(presetRect);
        }

        private void DrawPreviewOverlay(Rect previewRect)
        {
            Rect overlayRect = new Rect(previewRect.x + 8f, previewRect.y + 8f, previewRect.width - 16f, 72f);
            GUI.Label(
                overlayRect,
                "Socket: " + state.SelectedSocketId
                + "\nMode: " + state.Mode
                + "\nPreview: " + (previewItem.IsSpawned ? "Spawned" : "None")
                + " | Zeroed: " + (previewItem.IsSpawned && previewItem.IsZeroed ? "Yes" : "No"),
                CCS_EquipmentFitStudioStyles.PreviewOverlayLabel);
        }

        private void DrawBottomActionBar()
        {
            EditorGUILayout.BeginVertical(
                CCS_EquipmentFitStudioStyles.BottomBarBackground,
                GUILayout.MinHeight(BottomActionBarHeight),
                GUILayout.ExpandWidth(true));

            EditorGUILayout.BeginHorizontal();
            if (CCS_EquipmentFitStudioStyles.DrawBottomActionButton(
                    "Spawn Preview",
                    CCS_EquipmentFitStudioButtonKind.SpawnPreview,
                    "Creates a temporary editor-only visual under the selected socket.",
                    CanSpawnPreview(out _),
                    GUILayout.MinWidth(120f),
                    GUILayout.ExpandWidth(true)))
            {
                SpawnPreviewItem();
            }

            if (CCS_EquipmentFitStudioStyles.DrawBottomActionButton(
                    "Clear Preview",
                    CCS_EquipmentFitStudioButtonKind.ClearPreview,
                    "Removes preview item and resets IK preview weights.",
                    previewItem.IsSpawned,
                    GUILayout.MinWidth(110f),
                    GUILayout.ExpandWidth(true)))
            {
                ClearPreviewAndWeights();
            }

            if (CCS_EquipmentFitStudioStyles.DrawBottomActionButton(
                    "Capture",
                    CCS_EquipmentFitStudioButtonKind.Capture,
                    CanCaptureLiveValues()
                        ? "Copies the current socket values into a pending save buffer."
                        : "Select a player and socket before capturing values.",
                    CanCaptureLiveValues(),
                    GUILayout.MinWidth(90f),
                    GUILayout.ExpandWidth(true)))
            {
                CaptureLiveValues();
            }

            if (CCS_EquipmentFitStudioStyles.DrawBottomActionButton(
                    CanUsePlayModeAimFit() ? "Save Hand Profile" : "Save Profile",
                    CCS_EquipmentFitStudioButtonKind.SaveProfile,
                    CanSavePendingCapture()
                        ? "Saves the captured socket values into the selected revolver fit profile asset."
                        : "Capture live values before saving.",
                    CanSavePendingCapture(),
                    GUILayout.MinWidth(100f),
                    GUILayout.ExpandWidth(true)))
            {
                SaveActiveProfile();
            }

            if (CCS_EquipmentFitStudioStyles.DrawBottomActionButton(
                    "Apply Saved",
                    CCS_EquipmentFitStudioButtonKind.ApplyProfile,
                    "Applies saved profile values to the selected player socket.",
                    HasRevolverAttachmentFitProfileForSelectedSocket() && CanUseEditFitTuning(),
                    GUILayout.MinWidth(100f),
                    GUILayout.ExpandWidth(true)))
            {
                ApplyActiveSavedProfile();
            }

            if (CCS_EquipmentFitStudioStyles.DrawBottomActionButton(
                    GetActiveTargetPrimaryTestButtonLabel(),
                    CCS_EquipmentFitStudioButtonKind.TestFit,
                    "Tests the saved fit profile for the active slot.",
                    HasRevolverAttachmentFitProfileForSelectedSocket() && CanUseEditFitTuning(),
                    GUILayout.MinWidth(140f),
                    GUILayout.ExpandWidth(true)))
            {
                TestSavedActiveTargetFit();
            }

            if (CCS_EquipmentFitStudioStyles.DrawBottomActionButton(
                    "Test Hip",
                    CCS_EquipmentFitStudioButtonKind.TestFit,
                    "Temporary editor holster test from saved right hip profile.",
                    File.Exists(CCS_EquipmentConstants.RevolverM1879RightHipHolsterFitPath) && CanUseEditFitTuning(),
                    GUILayout.MinWidth(100f),
                    GUILayout.ExpandWidth(true)))
            {
                TestSavedHolsterFit();
            }

            if (CCS_EquipmentFitStudioStyles.DrawBottomActionButton(
                    "Test Hand",
                    CCS_EquipmentFitStudioButtonKind.TestFit,
                    "Temporary editor equipped test from saved right hand profile.",
                    File.Exists(CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath) && CanUseEditFitTuning(),
                    GUILayout.MinWidth(105f),
                    GUILayout.ExpandWidth(true)))
            {
                TestSavedEquippedFit();
            }

            if (CCS_EquipmentFitStudioStyles.DrawBottomActionButton(
                    "Validate",
                    CCS_EquipmentFitStudioButtonKind.Validate,
                    "Runs Fit Studio and revolver profile validation.",
                    true,
                    GUILayout.MinWidth(80f),
                    GUILayout.ExpandWidth(true)))
            {
                RunValidation();
            }

            if (CCS_EquipmentFitStudioStyles.DrawBottomActionButton(
                    "Cleanup",
                    CCS_EquipmentFitStudioButtonKind.Warning,
                    "Removes preview, test attachments, and preview camera objects.",
                    RequiresCleanup(),
                    GUILayout.MinWidth(80f),
                    GUILayout.ExpandWidth(true)))
            {
                CleanupTemporaryObjects(resetIkPreviewWeights: true, clearTestAttachments: true);
                SetStatus("Cleanup complete.", MessageType.Info);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawSocketTunerPanel()
        {
            Transform socketTransform = GetSelectedSocketTransform();
            CCS_EquipmentSocketAnchor anchor = socketTransform != null
                ? socketTransform.GetComponent<CCS_EquipmentSocketAnchor>()
                : null;
            DrawHint(CCS_EquipmentFitStudioRevolverFitUtility.GetSocketTuningHint(state.SelectedSocketId));
            DrawHint("Keep the preview item zeroed. Only the socket should move.");

            if (socketTransform == null)
            {
                EditorGUILayout.HelpBox("Select a player with equipment sockets.", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField("Socket ID", anchor != null ? anchor.SocketId : state.SelectedSocketId);
            EditorGUILayout.LabelField("Parent Bone", anchor != null ? anchor.ParentBone.ToString() : "Unknown");
            if (anchor != null && anchor.AllowedItemTypes != null)
            {
                EditorGUILayout.LabelField("Allowed Types", string.Join(", ", anchor.AllowedItemTypes));
            }

            Vector3 position = socketTransform.localPosition;
            Vector3 euler = socketTransform.localEulerAngles;
            Vector3 scale = socketTransform.localScale;
            position = EditorGUILayout.Vector3Field("Local Position", position);
            euler = EditorGUILayout.Vector3Field("Local Rotation", euler);
            scale = EditorGUILayout.Vector3Field("Local Scale", scale);
            socketTransform.localPosition = position;
            socketTransform.localRotation = Quaternion.Euler(euler);
            socketTransform.localScale = scale;
            previewItem.EnforceZeroedTransform();

            DrawNudgeControls(settings, ref position, ref euler, ref scale, socketTransform);
            DrawPendingDiff(state.SocketPendingChange, socketTransform);

            DrawWrappedButtons(
                GetLeftPanelWidth(),
                new FitStudioButtonSpec("Capture Live Socket", CaptureLiveValues),
                new FitStudioButtonSpec("Reset To Profile", () => ResetSocketToProfile(socketTransform)),
                new FitStudioButtonSpec("Mirror Right ↔ Left", () => MirrorSelectedSocket(socketTransform)));
        }

        private void DrawIkTargetTunerPanel()
        {
            Transform ikTarget = GetSelectedIkTargetTransform();
            DrawHint("Use the elbow hint to make the arm bend naturally.");
            if (!HasActiveIkConstraints())
            {
                EditorGUILayout.HelpBox(
                    "IK constraints are not active on this rig, but target positions can still be authored.",
                    MessageType.Info);
            }

            int ikIndex = System.Array.IndexOf(ikTargetNames, state.SelectedIkTargetName);
            if (ikIndex < 0)
            {
                ikIndex = 0;
            }

            ikIndex = EditorGUILayout.Popup("IK Target", ikIndex, ikTargetNames);
            state.SelectedIkTargetName = ikTargetNames[ikIndex];
            ikTarget = GetSelectedIkTargetTransform();
            if (ikTarget == null)
            {
                EditorGUILayout.HelpBox("Select a player with CCS_WeaponIKTargets.", MessageType.Warning);
                return;
            }

            Vector3 position = ikTarget.localPosition;
            Vector3 euler = ikTarget.localEulerAngles;
            position = EditorGUILayout.Vector3Field("Local Position", position);
            euler = EditorGUILayout.Vector3Field("Local Rotation", euler);
            ikTarget.localPosition = position;
            ikTarget.localRotation = Quaternion.Euler(euler);
            DrawNudgeControls(settings, ref position, ref euler, ref position, ikTarget, allowScale: false);

            EditorGUILayout.Space();
            previewRigWeight = EditorGUILayout.Slider("Preview Rig Weight", previewRigWeight, 0f, 1f);
            previewRightHandIkWeight = EditorGUILayout.Slider("Preview Right Hand IK", previewRightHandIkWeight, 0f, 1f);
            previewLeftHandIkWeight = EditorGUILayout.Slider("Preview Left Hand IK", previewLeftHandIkWeight, 0f, 1f);
            previewAimWeight = EditorGUILayout.Slider("Preview Aim Weight", previewAimWeight, 0f, 1f);
            ApplyIkPreviewWeights();
            if (GUILayout.Button("Reset Weights To Zero"))
            {
                previewRigWeight = 0f;
                previewRightHandIkWeight = 0f;
                previewLeftHandIkWeight = 0f;
                previewAimWeight = 0f;
                ApplyIkPreviewWeights();
            }

            if (GUILayout.Button("Capture Live Target"))
            {
                state.IkPendingChange.Capture(
                    state.SelectedIkTargetName,
                    ikTarget.localPosition,
                    ikTarget.localEulerAngles,
                    Vector3.one);
            }
        }

        private void DrawPreviewControlsPanel()
        {
            DrawHint("Left drag = orbit. Middle drag or Shift+drag = pan. Mouse wheel = zoom.");
            DrawPreviewPresetButtons(GetLeftPanelWidth());
        }

        private void DrawHandPoseFoundationPanel()
        {
            DrawHint("Finger pose controls are foundation-only in v0.6.8.");
            DrawHint("Runtime finger posing is not wired yet — save notes and curl values only.");
            Animator animator = GetPlayerAnimator();
            DrawFingerBoneDetection("Right Hand", animator, true);
            DrawFingerBoneDetection("Left Hand", animator, false);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Slider("Thumb Curl (future)", 0f, 0f, 1f);
            EditorGUILayout.Slider("Index Curl (future)", 0f, 0f, 1f);
            EditorGUILayout.Slider("Middle Curl (future)", 0f, 0f, 1f);
            EditorGUILayout.Slider("Ring Curl (future)", 0f, 0f, 1f);
            EditorGUILayout.Slider("Little Curl (future)", 0f, 0f, 1f);
            EditorGUI.EndDisabledGroup();

            if (state.SelectedHandPoseDefinition == null)
            {
                LoadRevolverProfileSelections();
            }

            state.SelectedHandPoseDefinition = (CCS_HandPoseDefinition)EditorGUILayout.ObjectField(
                "Hand Pose Asset",
                state.SelectedHandPoseDefinition,
                typeof(CCS_HandPoseDefinition),
                false);
            if (state.SelectedHandPoseDefinition != null
                && !string.IsNullOrEmpty(state.SelectedHandPoseDefinition.Notes))
            {
                EditorGUILayout.HelpBox(state.SelectedHandPoseDefinition.Notes, MessageType.Info);
            }

            if (GUILayout.Button("Load Revolver Right-Hand Grip Pose"))
            {
                state.SelectedHandPoseDefinition =
                    CCS_EquipmentFitStudioRevolverFitUtility.LoadRevolverRightHandGripPose();
            }

            if (GUILayout.Button("Create Hand Pose Asset"))
            {
                CreateHandPoseAsset();
            }
        }

        private void DrawSaveValidatePanel()
        {
            DrawPendingDiff(state.SocketPendingChange, GetSelectedSocketTransform());
            DrawPendingDiff(state.IkPendingChange, GetSelectedIkTargetTransform());
            DrawHint("Use the bottom action bar for Capture, Save, Apply, Test, Validate, and Cleanup.");
        }

        private void DrawStatusBar()
        {
            EditorGUILayout.Space();
            if (settings != null && settings.ShowHints)
            {
                DrawHint("Green means safe. Yellow means check it. Red means fix before saving.");
            }

            EditorGUILayout.HelpBox(statusMessage, statusType);
            if (state.HasUnsavedChanges)
            {
                EditorGUILayout.LabelField("Pending unsaved changes", CCS_EquipmentFitStudioStyles.StatusWarnLabel);
            }
        }

        private void DrawPreviewItemStatus()
        {
            if (!previewItem.IsSpawned)
            {
                EditorGUILayout.LabelField("Preview item: not spawned", CCS_EquipmentFitStudioStyles.StatusWarnLabel);
                return;
            }

            GUIStyle style = previewItem.IsZeroed
                ? CCS_EquipmentFitStudioStyles.StatusOkLabel
                : CCS_EquipmentFitStudioStyles.StatusErrorLabel;
            EditorGUILayout.LabelField(
                "Preview item local transform — Position: 0,0,0  Rotation: identity  Scale: 1,1,1 "
                + (previewItem.IsZeroed ? "✅" : "❌"),
                style);
            if (!previewItem.IsZeroed && GUILayout.Button("Reset Preview Item To Zero"))
            {
                previewItem.ResetPreviewItemToZero();
            }
        }

        private void DrawPreviewPresetButtons(float containerWidth)
        {
            DrawWrappedButtons(
                containerWidth,
                new FitStudioButtonSpec("Frame", () => previewCamera.ApplyPreset(CCS_EquipmentFitStudioCameraPreset.Frame, state.PlayerRoot, state.SelectedSocketId)),
                new FitStudioButtonSpec("Full Body", () => previewCamera.ApplyPreset(CCS_EquipmentFitStudioCameraPreset.FullBody, state.PlayerRoot, state.SelectedSocketId)),
                new FitStudioButtonSpec("Right Hand", () => previewCamera.ApplyPreset(CCS_EquipmentFitStudioCameraPreset.RightHand, state.PlayerRoot, state.SelectedSocketId)),
                new FitStudioButtonSpec("Left Hand", () => previewCamera.ApplyPreset(CCS_EquipmentFitStudioCameraPreset.LeftHand, state.PlayerRoot, state.SelectedSocketId)),
                new FitStudioButtonSpec("Right Hip", () => previewCamera.ApplyPreset(CCS_EquipmentFitStudioCameraPreset.RightHip, state.PlayerRoot, state.SelectedSocketId)),
                new FitStudioButtonSpec("Left Hip", () => previewCamera.ApplyPreset(CCS_EquipmentFitStudioCameraPreset.LeftHip, state.PlayerRoot, state.SelectedSocketId)),
                new FitStudioButtonSpec("Back", () => previewCamera.ApplyPreset(CCS_EquipmentFitStudioCameraPreset.Back, state.PlayerRoot, state.SelectedSocketId)),
                new FitStudioButtonSpec("Trigger Close-Up", () => previewCamera.ApplyPreset(CCS_EquipmentFitStudioCameraPreset.TriggerCloseUp, state.PlayerRoot, state.SelectedSocketId)),
                new FitStudioButtonSpec("Muzzle View", () => previewCamera.ApplyPreset(CCS_EquipmentFitStudioCameraPreset.MuzzleView, state.PlayerRoot, state.SelectedSocketId)));
        }

        private void DrawPreviewPresetButtonsAtRect(Rect area)
        {
            FitStudioButtonSpec[] buttons =
            {
                new FitStudioButtonSpec("Frame", () => previewCamera.ApplyPreset(CCS_EquipmentFitStudioCameraPreset.Frame, state.PlayerRoot, state.SelectedSocketId)),
                new FitStudioButtonSpec("Full Body", () => previewCamera.ApplyPreset(CCS_EquipmentFitStudioCameraPreset.FullBody, state.PlayerRoot, state.SelectedSocketId)),
                new FitStudioButtonSpec("Right Hand", () => previewCamera.ApplyPreset(CCS_EquipmentFitStudioCameraPreset.RightHand, state.PlayerRoot, state.SelectedSocketId)),
                new FitStudioButtonSpec("Left Hand", () => previewCamera.ApplyPreset(CCS_EquipmentFitStudioCameraPreset.LeftHand, state.PlayerRoot, state.SelectedSocketId)),
                new FitStudioButtonSpec("Right Hip", () => previewCamera.ApplyPreset(CCS_EquipmentFitStudioCameraPreset.RightHip, state.PlayerRoot, state.SelectedSocketId)),
                new FitStudioButtonSpec("Left Hip", () => previewCamera.ApplyPreset(CCS_EquipmentFitStudioCameraPreset.LeftHip, state.PlayerRoot, state.SelectedSocketId)),
                new FitStudioButtonSpec("Back", () => previewCamera.ApplyPreset(CCS_EquipmentFitStudioCameraPreset.Back, state.PlayerRoot, state.SelectedSocketId)),
                new FitStudioButtonSpec("Trigger Close-Up", () => previewCamera.ApplyPreset(CCS_EquipmentFitStudioCameraPreset.TriggerCloseUp, state.PlayerRoot, state.SelectedSocketId)),
                new FitStudioButtonSpec("Muzzle View", () => previewCamera.ApplyPreset(CCS_EquipmentFitStudioCameraPreset.MuzzleView, state.PlayerRoot, state.SelectedSocketId)),
            };

            float buttonWidth = WrappedButtonMinWidth;
            float buttonHeight = 20f;
            float x = area.x;
            float y = area.y;
            float maxX = area.xMax;
            for (int i = 0; i < buttons.Length; i++)
            {
                FitStudioButtonSpec button = buttons[i];
                if (!button.Visible || button.OnClick == null)
                {
                    continue;
                }

                if (x + buttonWidth > maxX)
                {
                    x = area.x;
                    y += buttonHeight + 4f;
                }

                Rect buttonRect = new Rect(x, y, buttonWidth, buttonHeight);
                if (GUI.Button(buttonRect, button.Label))
                {
                    button.OnClick.Invoke();
                }

                x += buttonWidth + 4f;
            }
        }

        private readonly struct FitStudioButtonSpec
        {
            public FitStudioButtonSpec(string label, System.Action onClick, bool visible = true)
            {
                Label = label;
                OnClick = onClick;
                Visible = visible;
            }

            public string Label { get; }

            public System.Action OnClick { get; }

            public bool Visible { get; }
        }

        private static void DrawWrappedButtons(float containerWidth, params FitStudioButtonSpec[] buttons)
        {
            float safeWidth = Mathf.Max(WrappedButtonMinWidth, containerWidth);
            int buttonsPerRow = Mathf.Max(1, Mathf.FloorToInt(safeWidth / (WrappedButtonMinWidth + 4f)));
            int visibleCount = 0;

            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < buttons.Length; i++)
            {
                FitStudioButtonSpec button = buttons[i];
                if (!button.Visible || button.OnClick == null)
                {
                    continue;
                }

                if (visibleCount > 0 && visibleCount % buttonsPerRow == 0)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }

                if (GUILayout.Button(button.Label, GUILayout.MinWidth(WrappedButtonMinWidth), GUILayout.ExpandWidth(true)))
                {
                    button.OnClick.Invoke();
                }

                visibleCount++;
            }

            EditorGUILayout.EndHorizontal();
        }

        private float GetLeftPanelWidth()
        {
            float availableWidth = Mathf.Max(MinWindowWidth, position.width) - RightPanelMinWidth - 24f;
            float leftWidth = Mathf.Clamp(LeftPanelPreferredWidth, LeftPanelMinWidth, LeftPanelMaxWidth);
            if (availableWidth - leftWidth < RightPanelMinWidth)
            {
                leftWidth = Mathf.Max(LeftPanelMinWidth, availableWidth - RightPanelMinWidth);
            }

            return leftWidth;
        }

        private float GetRightPanelWidth()
        {
            float availableWidth = Mathf.Max(MinWindowWidth, position.width) - LeftPanelMinWidth - 24f;
            return Mathf.Clamp(RightPanelMinWidth, RightPanelMinWidth, RightPanelMaxWidth);
        }

        private float GetPreviewPanelContentWidth()
        {
            return Mathf.Max(RightPanelMinWidth, position.width - GetLeftPanelWidth() - 24f);
        }

        private static void DrawHint(string message)
        {
            EditorGUILayout.LabelField(message, CCS_EquipmentFitStudioStyles.HintLabel);
        }

        private static void DrawPendingDiff(CCS_EquipmentFitStudioPendingChange pending, Transform target)
        {
            if (!pending.HasCaptured)
            {
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("PENDING PROFILE SAVE", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Socket: " + pending.Label);
            if (!string.IsNullOrEmpty(pending.ProfileAssetName))
            {
                EditorGUILayout.LabelField("Profile: " + pending.ProfileAssetName + ".asset");
            }

            if (pending.HasTransformChanges)
            {
                EditorGUILayout.LabelField(
                    "Position:\nOld: " + CCS_EquipmentFitStudioPendingChange.FormatVector3(pending.OldPosition)
                    + "\nNew: " + CCS_EquipmentFitStudioPendingChange.FormatVector3(pending.NewPosition));
                EditorGUILayout.LabelField(
                    "Rotation:\nOld: " + CCS_EquipmentFitStudioPendingChange.FormatVector3(pending.OldEulerAngles)
                    + "\nNew: " + CCS_EquipmentFitStudioPendingChange.FormatVector3(pending.NewEulerAngles));
                EditorGUILayout.LabelField(
                    "Scale:\nOld: " + CCS_EquipmentFitStudioPendingChange.FormatVector3(pending.OldScale)
                    + "\nNew: " + CCS_EquipmentFitStudioPendingChange.FormatVector3(pending.NewScale));
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "No transform changes detected. You can still save, but the profile already matches this socket.",
                    MessageType.Info);
            }

            if (target == null)
            {
                EditorGUILayout.HelpBox("Live socket transform is unavailable. Re-select the player.", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }

        private static void DrawNudgeControls(
            CCS_EquipmentFitStudioSettings studioSettings,
            ref Vector3 position,
            ref Vector3 euler,
            ref Vector3 scale,
            Transform target,
            bool allowScale = true)
        {
            float posSmall = studioSettings != null ? studioSettings.NudgePositionSmall : 0.01f;
            float posLarge = studioSettings != null ? studioSettings.NudgePositionLarge : 0.05f;
            float rotSmall = studioSettings != null ? studioSettings.NudgeRotationSmall : 1f;
            float rotLarge = studioSettings != null ? studioSettings.NudgeRotationLarge : 5f;

            EditorGUILayout.LabelField("Position Nudge");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+X", GUILayout.ExpandWidth(true)))
            {
                position.x += posSmall;
            }

            if (GUILayout.Button("-X", GUILayout.ExpandWidth(true)))
            {
                position.x -= posSmall;
            }

            if (GUILayout.Button("+Y", GUILayout.ExpandWidth(true)))
            {
                position.y += posSmall;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("-Y", GUILayout.ExpandWidth(true)))
            {
                position.y -= posSmall;
            }

            if (GUILayout.Button("+Z", GUILayout.ExpandWidth(true)))
            {
                position.z += posSmall;
            }

            if (GUILayout.Button("-Z", GUILayout.ExpandWidth(true)))
            {
                position.z -= posSmall;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Rotation Nudge");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Pitch +", GUILayout.ExpandWidth(true)))
            {
                euler.x += rotSmall;
            }

            if (GUILayout.Button("Pitch -", GUILayout.ExpandWidth(true)))
            {
                euler.x -= rotSmall;
            }

            if (GUILayout.Button("Yaw +", GUILayout.ExpandWidth(true)))
            {
                euler.y += rotSmall;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Yaw -", GUILayout.ExpandWidth(true)))
            {
                euler.y -= rotSmall;
            }

            if (GUILayout.Button("Roll +", GUILayout.ExpandWidth(true)))
            {
                euler.z += rotSmall;
            }

            if (GUILayout.Button("Roll -", GUILayout.ExpandWidth(true)))
            {
                euler.z -= rotSmall;
            }

            EditorGUILayout.EndHorizontal();

            if (allowScale)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Scale +", GUILayout.ExpandWidth(true)))
                {
                    scale += Vector3.one * 0.01f;
                }

                if (GUILayout.Button("Scale -", GUILayout.ExpandWidth(true)))
                {
                    scale -= Vector3.one * 0.01f;
                }

                EditorGUILayout.EndHorizontal();
            }

            target.localPosition = position;
            target.localRotation = Quaternion.Euler(euler);
            if (allowScale)
            {
                target.localScale = scale;
            }
        }

        private void SpawnPreviewItem()
        {
            state.LastPreviewError = string.Empty;
            if (!CanSpawnPreview(out string blockingReason))
            {
                state.LastPreviewError = blockingReason;
                Debug.LogWarning("[Equipment Fit Studio] " + blockingReason);
                SetStatus(blockingReason, MessageType.Error);
                return;
            }

            Transform socketTransform = GetSelectedSocketTransform();
            GameObject source = settings != null ? settings.DefaultPreviewWeaponPrefab : null;
            string attachmentRootName =
                CCS_EquipmentFitStudioPreviewAttachmentUtility.GetAttachmentRootObjectName(state.FitTarget);
            Transform attachmentRoot = CCS_EquipmentFitStudioPreviewAttachmentUtility.EnsurePreviewAttachmentRoot(
                socketTransform,
                attachmentRootName);
            if (!previewItem.TrySpawnUnderAttachmentRoot(attachmentRoot, source, out string spawnError))
            {
                state.LastPreviewError = spawnError;
                Debug.LogWarning("[Equipment Fit Studio] " + spawnError);
                SetStatus(spawnError, MessageType.Error);
                return;
            }

            state.PreviewItemSpawned = true;
            state.LastPreviewError = string.Empty;
            state.WorkflowStepOverride = null;
            state.JustSavedProfileThisSession = false;
            Transform frameTarget = previewItem.PreviewRoot != null
                ? previewItem.PreviewRoot.transform
                : socketTransform;
            previewCamera.FrameTransform(frameTarget, 1.35f);
            previewCamera.ApplyPreset(
                state.SelectedSocketId == CCS_EquipmentConstants.HandSocketRightId
                    ? CCS_EquipmentFitStudioCameraPreset.RightHand
                    : state.SelectedSocketId == CCS_EquipmentConstants.HolsterSocketRightHipId
                        ? CCS_EquipmentFitStudioCameraPreset.RightHip
                        : CCS_EquipmentFitStudioCameraPreset.Frame,
                state.PlayerRoot,
                state.SelectedSocketId);
            SetStatus("Preview spawned under " + state.SelectedSocketId + " (zeroed).", MessageType.Info);
            Repaint();
        }

        private void SaveSocketToDefinition(bool showDialog)
        {
            Transform socketTransform = GetSelectedSocketTransform();
            CCS_EquipmentSocketDefinition definition = FindSocketDefinition(state.SelectedSocketId);
            if (socketTransform == null || definition == null)
            {
                SetStatus("Missing socket transform or definition.", MessageType.Error);
                return;
            }

            if (showDialog && !EditorUtility.DisplayDialog(
                    "Save Socket Definition",
                    "Save live socket values to " + definition.name + "?",
                    "Save",
                    "Cancel"))
            {
                return;
            }

            SerializedObject serializedDefinition = new SerializedObject(definition);
            serializedDefinition.FindProperty("localPosition").vector3Value = socketTransform.localPosition;
            serializedDefinition.FindProperty("localEulerAngles").vector3Value = socketTransform.localEulerAngles;
            serializedDefinition.FindProperty("localScale").vector3Value = socketTransform.localScale;
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
            state.SocketPendingChange.SetBaseline(
                socketTransform.localPosition,
                socketTransform.localEulerAngles,
                socketTransform.localScale);
            SetStatus("Saved socket definition for " + state.SelectedSocketId + ".", MessageType.Info);
        }

        private void SaveAttachmentFitProfile(bool showDialog)
        {
            Transform socketTransform = GetSelectedSocketTransform();
            if (socketTransform == null)
            {
                return;
            }

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Attachment Fit Profile",
                "CCS_WeaponAttachmentFitProfile_" + state.SelectedSocketId,
                "asset",
                "Choose save location",
                CCS_EquipmentConstants.EquipmentFittingProfileRootPath);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            CCS_WeaponAttachmentFitProfile profile = AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_WeaponAttachmentFitProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            profile.SetIdentity(
                System.IO.Path.GetFileNameWithoutExtension(path),
                settings != null ? settings.DefaultWeaponId : CCS_EquipmentConstants.DefaultPreviewWeaponId,
                settings != null ? settings.DefaultCharacterRigId : CCS_EquipmentConstants.DefaultCharacterRigId,
                state.SelectedSocketId);
            profile.ApplySocketTransform(
                profile.ProfileId,
                socketTransform.localPosition,
                socketTransform.localEulerAngles,
                socketTransform.localScale);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            state.SelectedAttachmentFitProfile = profile;
            SetStatus("Saved attachment fit profile.", MessageType.Info);
        }

        private void SaveIkPoseProfile(bool showDialog)
        {
            Transform ikRoot = GetIkTargetsRoot();
            if (ikRoot == null)
            {
                return;
            }

            CCS_WeaponIKPoseProfile profile = state.SelectedIkPoseProfile;
            if (profile == null)
            {
                profile = AssetDatabase.LoadAssetAtPath<CCS_WeaponIKPoseProfile>(
                    CCS_EquipmentConstants.DefaultWeaponIkPoseProfilePath);
            }

            if (profile == null)
            {
                SetStatus("Missing IK pose profile asset.", MessageType.Error);
                return;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            WriteVector3(serializedProfile, "rightHandTargetLocalPosition", GetIkLocalPosition(ikRoot, CCS_EquipmentConstants.RightHandIkTargetObjectName));
            WriteVector3(serializedProfile, "rightElbowHintLocalPosition", GetIkLocalPosition(ikRoot, CCS_EquipmentConstants.RightElbowHintObjectName));
            WriteVector3(serializedProfile, "leftHandTargetLocalPosition", GetIkLocalPosition(ikRoot, CCS_EquipmentConstants.LeftHandIkTargetObjectName));
            WriteVector3(serializedProfile, "leftElbowHintLocalPosition", GetIkLocalPosition(ikRoot, CCS_EquipmentConstants.LeftElbowHintObjectName));
            WriteVector3(serializedProfile, "weaponAimTargetLocalPosition", GetIkLocalPosition(ikRoot, CCS_EquipmentConstants.WeaponAimTargetObjectName));
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            SetStatus("Saved IK pose profile.", MessageType.Info);
        }

        private void ApplySocketProfileToPlayer()
        {
            if (state.PlayerRoot == null)
            {
                return;
            }

            CCS_EquipmentSocketPlayerBuilder.EnsurePlayerEquipmentSocketFoundation(state.PlayerRoot);
            SetStatus("Applied socket profile to selected player.", MessageType.Info);
        }

        private void RebuildAndApply()
        {
            CleanupTemporaryObjects(resetIkPreviewWeights: true, clearTestAttachments: true);
            CCS_EquipmentSocketProfileBuilder.EnsureDefaultEquipmentSocketProfile();
            CCS_EquipmentFitStudioProfileBuilder.EnsureEquipmentFitStudioAssets();
            CCS_CharacterControllerPlayerPrefabBuilder.EnsurePlayerPrefabs();
            CCS_WeaponsAssetBuilder.EnsureWeaponsAssets();
            CCS_EquipmentFitStudioCleanupUtility.CleanupAllPreviewObjects();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            SetStatus("Rebuild complete. Preview objects cleaned up.", MessageType.Info);
        }

        private void RunValidation()
        {
            CleanupTemporaryObjects(resetIkPreviewWeights: true, clearTestAttachments: true);
            CCS_EquipmentFitStudioPreviewPlayerUtility.ClearPreviewPlayer();
            if (state.UsesEditorPreviewPlayer)
            {
                state.PlayerRoot = null;
                state.UsesEditorPreviewPlayer = false;
            }

            CCS_SurvivalValidationResult result = CCS_EquipmentFitStudioValidationUtility.ValidateEquipmentFitStudioFoundation();
            statusMessage = result.Message;
            statusType = result.IsSuccess ? MessageType.Info : MessageType.Error;
        }

        private void ResetSocketToProfile(Transform socketTransform)
        {
            if (state.PlayerRoot != null
                && CCS_EquipmentFitStudioRevolverFitUtility.ApplyRevolverAttachmentFitProfile(
                    state.SelectedSocketId,
                    state.PlayerRoot,
                    state.FitTarget))
            {
                previewItem.EnforceZeroedTransform();
                SyncPendingFromAttachmentRoot(GetPreviewAttachmentRootTransform());
                return;
            }

            CCS_EquipmentSocketDefinition definition = FindSocketDefinition(state.SelectedSocketId);
            if (definition == null || socketTransform == null)
            {
                return;
            }

            socketTransform.localPosition = definition.LocalPosition;
            socketTransform.localRotation = Quaternion.Euler(definition.LocalEulerAngles);
            socketTransform.localScale = definition.LocalScale;
            previewItem.EnforceZeroedTransform();
            SyncPendingFromAttachmentRoot(GetPreviewAttachmentRootTransform());
        }

        private void LoadRevolverProfileSelections()
        {
            state.SelectedAttachmentFitProfile =
                CCS_EquipmentFitStudioRevolverFitUtility.LoadRevolverAttachmentFitProfile(state.SelectedSocketId);
            state.SelectedIkPoseProfile = CCS_EquipmentFitStudioRevolverFitUtility.LoadRevolverAimIkPoseProfile();
            if (state.SelectedHandPoseDefinition == null)
            {
                state.SelectedHandPoseDefinition =
                    CCS_EquipmentFitStudioRevolverFitUtility.LoadRevolverRightHandGripPose();
            }

            InitializePendingBaselineFromProfile();
        }

        private void InitializePendingBaselineFromProfile()
        {
            CCS_WeaponAttachmentFitProfile profile = state.SelectedAttachmentFitProfile;
            if (profile != null)
            {
                state.SocketPendingChange.SetBaseline(
                    profile.SocketLocalPosition,
                    profile.SocketLocalEulerAngles,
                    profile.SocketLocalScale);
                state.SocketPendingChange.ProfileAssetName = profile.name;
            }
            else
            {
                state.SocketPendingChange.SetBaseline(Vector3.zero, Vector3.zero, Vector3.one);
                state.SocketPendingChange.ProfileAssetName = string.Empty;
            }

            state.HasPendingSaveCapture = false;
            state.SavedProfileThisSession = false;
            state.JustSavedProfileThisSession = false;
        }

        private bool HasRevolverAttachmentFitProfileForSelectedSocket()
        {
            return !string.IsNullOrEmpty(
                CCS_EquipmentFitStudioRevolverFitUtility.GetRevolverAttachmentFitProfilePath(state.SelectedSocketId));
        }

        private void SaveRevolverAttachmentFitProfile(Transform socketTransform)
        {
            if (socketTransform == null)
            {
                SetStatus("Select a socket before saving revolver fit profile.", MessageType.Error);
                return;
            }

            if (!CCS_EquipmentFitStudioRevolverFitUtility.SaveRevolverAttachmentFitProfile(
                    state.SelectedSocketId,
                    socketTransform))
            {
                SetStatus("Could not save revolver fit profile for " + state.SelectedSocketId + ".", MessageType.Error);
                return;
            }

            state.SelectedAttachmentFitProfile =
                CCS_EquipmentFitStudioRevolverFitUtility.LoadRevolverAttachmentFitProfile(state.SelectedSocketId);
            state.SocketPendingChange.SetBaseline(
                socketTransform.localPosition,
                socketTransform.localEulerAngles,
                socketTransform.localScale);
            SetStatus(
                "Saved revolver fit profile for " + state.SelectedSocketId + ".",
                MessageType.Info);
        }

        private void ApplyRevolverAttachmentFitProfile(Transform socketTransform)
        {
            if (socketTransform == null)
            {
                SetStatus("Select a socket before applying revolver fit profile.", MessageType.Error);
                return;
            }

            if (!CCS_EquipmentFitStudioRevolverFitUtility.ApplyRevolverAttachmentFitProfile(
                    state.SelectedSocketId,
                    socketTransform))
            {
                SetStatus("Could not apply revolver fit profile for " + state.SelectedSocketId + ".", MessageType.Error);
                return;
            }

            previewItem.EnforceZeroedTransform();
            SetStatus("Applied revolver fit profile to " + state.SelectedSocketId + ".", MessageType.Info);
        }

        private void SaveRevolverAimIkPoseProfile()
        {
            Transform ikRoot = GetIkTargetsRoot();
            if (ikRoot == null)
            {
                SetStatus("Missing CCS_WeaponIKTargets on selected player.", MessageType.Error);
                return;
            }

            previewRigWeight = 0f;
            previewRightHandIkWeight = 0f;
            previewLeftHandIkWeight = 0f;
            previewAimWeight = 0f;
            ApplyIkPreviewWeights();

            if (!CCS_EquipmentFitStudioRevolverFitUtility.SaveRevolverAimIkPoseProfile(ikRoot))
            {
                SetStatus("Could not save revolver aim IK profile.", MessageType.Error);
                return;
            }

            state.SelectedIkPoseProfile = CCS_EquipmentFitStudioRevolverFitUtility.LoadRevolverAimIkPoseProfile();
            SetStatus("Saved revolver aim IK profile with zero weights.", MessageType.Info);
        }

        private void MirrorSelectedSocket(Transform socketTransform)
        {
            if (socketTransform == null)
            {
                return;
            }

            Vector3 position = socketTransform.localPosition;
            Vector3 euler = socketTransform.localEulerAngles;
            position.x *= -1f;
            euler.y *= -1f;
            euler.z *= -1f;
            socketTransform.localPosition = position;
            socketTransform.localRotation = Quaternion.Euler(euler);
        }

        private void CreateHandPoseAsset()
        {
            string path = AssetDatabase.GenerateUniqueAssetPath(
                CCS_EquipmentConstants.EquipmentFittingHandPoseFolderPath + "/CCS_HandPoseDefinition_New.asset");
            CCS_HandPoseDefinition pose = ScriptableObject.CreateInstance<CCS_HandPoseDefinition>();
            AssetDatabase.CreateAsset(pose, path);
            AssetDatabase.SaveAssets();
            state.SelectedHandPoseDefinition = pose;
        }

        private void TryAssignDefaultPlayer(bool forceRefresh = false)
        {
            if (EditorApplication.isPlaying)
            {
                state.FitStudioMode = CCS_EquipmentFitStudioFitMode.PlayModeRuntimeTest;
                state.UsesEditorPreviewPlayer = false;
                if (forceRefresh
                    || state.PlayerRoot == null
                    || !IsValidRuntimePlayerTarget(state.PlayerRoot))
                {
                    if (CCS_EquipmentFitStudioPreviewPlayerUtility.TryFindRuntimePlayer(
                            out GameObject runtimePlayer,
                            out _))
                    {
                        state.PlayerRoot = runtimePlayer;
                    }
                    else
                    {
                        state.PlayerRoot = null;
                    }
                }

                return;
            }

            state.FitStudioMode = CCS_EquipmentFitStudioFitMode.EditFitPreview;
            if (!forceRefresh
                && state.PlayerRoot != null
                && CCS_EquipmentFitStudioPreviewPlayerUtility.IsValidEditFitPlayerTarget(state.PlayerRoot))
            {
                state.UsesEditorPreviewPlayer =
                    CCS_EquipmentFitStudioPreviewPlayerUtility.IsPreviewPlayer(state.PlayerRoot);
                return;
            }

            GameObject existingPreview = CCS_EquipmentFitStudioPreviewPlayerUtility.FindExistingPreviewPlayer();
            if (existingPreview != null)
            {
                state.PlayerRoot = existingPreview;
                state.UsesEditorPreviewPlayer = true;
                return;
            }

            CCS_EquipmentSocketRegistry registry = Object.FindFirstObjectByType<CCS_EquipmentSocketRegistry>();
            if (registry != null
                && !CCS_EquipmentFitStudioPreviewPlayerUtility.IsPreviewPlayer(registry.gameObject))
            {
                state.PlayerRoot = registry.gameObject;
                state.UsesEditorPreviewPlayer = false;
                return;
            }

            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null
                && prefabStage.prefabContentsRoot.GetComponent<CCS_EquipmentSocketRegistry>() != null)
            {
                state.PlayerRoot = prefabStage.prefabContentsRoot;
                state.UsesEditorPreviewPlayer = false;
                return;
            }

            state.PlayerRoot = null;
            state.UsesEditorPreviewPlayer = false;
        }

        private static bool IsValidRuntimePlayerTarget(GameObject playerRoot)
        {
            return playerRoot != null
                && !CCS_EquipmentFitStudioPreviewPlayerUtility.IsPreviewPlayer(playerRoot)
                && playerRoot.GetComponent<CCS_EquipmentSocketRegistry>() != null;
        }

        private bool CanUseEditFitTuning()
        {
            return state.FitStudioMode == CCS_EquipmentFitStudioFitMode.EditFitPreview
                && !EditorApplication.isPlaying;
        }

        private bool CanUsePlayModeAimFit()
        {
            return state.FitStudioMode == CCS_EquipmentFitStudioFitMode.PlayModeAimFit
                && EditorApplication.isPlaying
                && state.SelectedSocketId == CCS_EquipmentConstants.HandSocketRightId;
        }

        private bool CanUseFitCaptureAndSave()
        {
            return CanUseEditFitTuning() || CanUsePlayModeAimFit();
        }

        private Transform GetSelectedSocketTransform()
        {
            if (state.PlayerRoot == null)
            {
                return null;
            }

            CCS_EquipmentSocketRegistry registry = state.PlayerRoot.GetComponent<CCS_EquipmentSocketRegistry>();
            if (registry != null && registry.TryGetSocket(state.SelectedSocketId, out Transform socketTransform))
            {
                return socketTransform;
            }

            return null;
        }

        private Transform GetSelectedIkTargetTransform()
        {
            Transform ikRoot = GetIkTargetsRoot();
            return ikRoot != null ? ikRoot.Find(state.SelectedIkTargetName) : null;
        }

        private Transform GetIkTargetsRoot()
        {
            if (state.PlayerRoot == null)
            {
                return null;
            }

            Transform visualRoot = FindDeepChild(state.PlayerRoot.transform, CCS_EquipmentConstants.VisualRootObjectName);
            return visualRoot != null ? visualRoot.Find(CCS_EquipmentConstants.WeaponIkTargetsObjectName) : null;
        }

        private CCS_EquipmentSocketDefinition FindSocketDefinition(string socketId)
        {
            CCS_EquipmentSocketProfile profile = settings != null
                ? settings.DefaultSocketProfile
                : AssetDatabase.LoadAssetAtPath<CCS_EquipmentSocketProfile>(
                    CCS_EquipmentConstants.DefaultEquipmentSocketProfilePath);
            if (profile == null)
            {
                return null;
            }

            IReadOnlyList<CCS_EquipmentSocketDefinition> definitions = profile.SocketDefinitions;
            for (int i = 0; i < definitions.Count; i++)
            {
                if (definitions[i] != null && definitions[i].SocketId == socketId)
                {
                    return definitions[i];
                }
            }

            return null;
        }

        private Animator GetPlayerAnimator()
        {
            if (state.PlayerRoot == null)
            {
                return null;
            }

            Transform visualRoot = FindDeepChild(state.PlayerRoot.transform, CCS_EquipmentConstants.VisualRootObjectName);
            return visualRoot != null ? visualRoot.GetComponentInChildren<Animator>(true) : null;
        }

        private bool HasActiveIkConstraints()
        {
            Animator animator = GetPlayerAnimator();
            if (animator == null)
            {
                return false;
            }

            return animator.GetComponentsInChildren<TwoBoneIKConstraint>(true).Length > 0;
        }

        private void ApplyIkPreviewWeights()
        {
            Animator animator = GetPlayerAnimator();
            if (animator == null)
            {
                return;
            }

            Rig rig = FindDeepChild(animator.transform, CCS_EquipmentConstants.WeaponIkRigObjectName)?.GetComponent<Rig>();
            if (rig != null)
            {
                rig.weight = previewRigWeight;
            }

            TwoBoneIKConstraint[] twoBone = animator.GetComponentsInChildren<TwoBoneIKConstraint>(true);
            for (int i = 0; i < twoBone.Length; i++)
            {
                TwoBoneIKConstraint constraint = twoBone[i];
                if (constraint == null)
                {
                    continue;
                }

                if (constraint.name.Contains("Right"))
                {
                    constraint.weight = previewRightHandIkWeight;
                }
                else if (constraint.name.Contains("Left"))
                {
                    constraint.weight = previewLeftHandIkWeight;
                }
            }

            MultiAimConstraint[] aimConstraints = animator.GetComponentsInChildren<MultiAimConstraint>(true);
            for (int i = 0; i < aimConstraints.Length; i++)
            {
                if (aimConstraints[i] != null)
                {
                    aimConstraints[i].weight = previewAimWeight;
                }
            }
        }

        private void ResetIkPreviewWeights()
        {
            previewRigWeight = 0f;
            previewRightHandIkWeight = 0f;
            previewLeftHandIkWeight = 0f;
            previewAimWeight = 0f;
            ApplyIkPreviewWeights();
        }

        private void DrawFingerBoneDetection(string label, Animator animator, bool rightHand)
        {
            if (animator == null || !animator.isHuman)
            {
                EditorGUILayout.LabelField(label + ": humanoid rig missing");
                return;
            }

            HumanBodyBones[] bones =
            {
                rightHand ? HumanBodyBones.RightThumbProximal : HumanBodyBones.LeftThumbProximal,
                rightHand ? HumanBodyBones.RightIndexProximal : HumanBodyBones.LeftIndexProximal,
                rightHand ? HumanBodyBones.RightMiddleProximal : HumanBodyBones.LeftMiddleProximal,
            };
            int found = 0;
            for (int i = 0; i < bones.Length; i++)
            {
                if (animator.GetBoneTransform(bones[i]) != null)
                {
                    found++;
                }
            }

            EditorGUILayout.LabelField(label + " finger bones detected: " + found + " / " + bones.Length);
        }

        private void OnSceneGui(SceneView sceneView)
        {
            if (settings == null || !settings.ShowGizmos)
            {
                return;
            }

            Transform socket = GetSelectedSocketTransform();
            if (socket != null)
            {
                Handles.color = Color.blue;
                Handles.SphereHandleCap(0, socket.position, Quaternion.identity, 0.03f, EventType.Repaint);
                Handles.Label(socket.position, "Socket: " + state.SelectedSocketId);
            }

            Transform ikTarget = GetSelectedIkTargetTransform();
            if (ikTarget != null)
            {
                Handles.color = new Color(0.7f, 0.3f, 1f);
                Handles.SphereHandleCap(0, ikTarget.position, Quaternion.identity, 0.025f, EventType.Repaint);
                Handles.Label(ikTarget.position, "IK: " + state.SelectedIkTargetName);
            }

            if (previewItem.IsSpawned)
            {
                Handles.color = previewItem.IsZeroed ? Color.green : Color.red;
                Handles.Label(previewItem.PreviewRoot.transform.position, "Preview zeroed: " + previewItem.IsZeroed);
            }

            DrawWeaponRotationSceneGizmos();

            if (HasTestAttachments())
            {
                DrawTestFitSceneLabels();
            }
        }

        private void DrawTestFitSceneLabels()
        {
            if (state.PlayerRoot == null)
            {
                return;
            }

            Transform[] transforms = state.PlayerRoot.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform candidate = transforms[i];
                if (candidate.name == CCS_EquipmentConstants.EditorTestHolsterFitObjectName
                    || candidate.name == CCS_EquipmentConstants.EditorTestEquippedFitObjectName)
                {
                    Handles.color = Color.cyan;
                    Handles.Label(candidate.position, "EDITOR TEST FIT — NOT SAVED");
                }
            }
        }

        private void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (state.HasPendingSaveCapture
                && (change == PlayModeStateChange.ExitingEditMode || change == PlayModeStateChange.ExitingPlayMode))
            {
                bool continueAnyway = EditorUtility.DisplayDialog(
                    "Unsaved Fit Values",
                    "You have captured fit values that are not saved to the profile asset. Save before testing in Play Mode.",
                    "Continue Anyway",
                    "Cancel");
                if (!continueAnyway && change == PlayModeStateChange.ExitingEditMode)
                {
                    EditorApplication.isPlaying = false;
                    return;
                }
            }

            if (change == PlayModeStateChange.ExitingEditMode)
            {
                CCS_EquipmentFitStudioPreviewPlayerUtility.ClearPreviewPlayer();
                CCS_EquipmentFitStudioPosePreviewUtility.ClearAllPosePreview();
                state.PosePreviewMode = CCS_EquipmentFitStudioPosePreviewMode.Neutral;
                state.UserManuallySelectedPosePreview = false;
                state.UsesEditorPreviewPlayer = false;
                state.PlayerRoot = null;
                state.FitStudioMode = CCS_EquipmentFitStudioFitMode.PlayModeAimFit;
            }

            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                state.FitStudioMode = CCS_EquipmentFitStudioFitMode.PlayModeAimFit;
                TryAssignDefaultPlayer(forceRefresh: true);
            }

            if (change == PlayModeStateChange.ExitingPlayMode)
            {
                CCS_EquipmentFitStudioPlayModeAimFitUtility.CleanupEditorOverrides(state.PlayerRoot);
                CCS_EquipmentFitStudioIkDiagnosticsUtility.ResetIkPreviewToZero(state.PlayerRoot);
                state.ForceAimPoseActive = false;
                CCS_EquipmentFitStudioPosePreviewUtility.ClearAllPosePreview();
                state.PosePreviewMode = CCS_EquipmentFitStudioPosePreviewMode.Neutral;
                state.UserManuallySelectedPosePreview = false;
                CleanupTemporaryObjects(resetIkPreviewWeights: true, clearTestAttachments: true);
            }

            if (change == PlayModeStateChange.EnteredEditMode)
            {
                CCS_EquipmentFitStudioPreviewPlayerUtility.ClearPreviewPlayer();
                state.UsesEditorPreviewPlayer = false;
                state.PlayerRoot = null;
                state.FitStudioMode = CCS_EquipmentFitStudioFitMode.EditFitPreview;
                SyncFitTargetRoutingToState();
                RequestAutoLoadPreview(deferUntilAfterGui: true);
            }
        }

        private void CaptureLiveValues()
        {
            switch (state.Mode)
            {
                case CCS_EquipmentFitStudioMode.IkTargetTuner:
                    Transform ikTarget = GetSelectedIkTargetTransform();
                    if (ikTarget == null)
                    {
                        SetStatus("Select a player with IK targets before capture.", MessageType.Error);
                        return;
                    }

                    state.IkPendingChange.Capture(
                        state.SelectedIkTargetName,
                        ikTarget.localPosition,
                        ikTarget.localEulerAngles,
                        Vector3.one);
                    SetStatus("Captured IK target values.", MessageType.Info);
                    break;
                default:
                    if (CanUsePlayModeAimFit())
                    {
                        if (!CCS_EquipmentFitStudioPlayModeAimFitUtility.TryCaptureFromRuntimeAttachmentRoot(
                                state.PlayerRoot,
                                state.SocketPendingChange,
                                out string runtimeMessage,
                                out MessageType runtimeMessageType))
                        {
                            SetStatus(runtimeMessage, runtimeMessageType);
                            Repaint();
                            return;
                        }

                        state.HasPendingSaveCapture = true;
                        state.SavedProfileThisSession = false;
                        state.JustSavedProfileThisSession = false;
                        state.WorkflowStepOverride = CCS_EquipmentFitStudioWorkflowStep.SaveProfile;
                        SetStatus(runtimeMessage, runtimeMessageType);
                        Repaint();
                        break;
                    }

                    if (!CCS_EquipmentFitStudioCaptureUtility.TryCaptureSocketValues(
                            state.PlayerRoot,
                            GetSelectedSocketTransform(),
                            state.SelectedSocketId,
                            previewItem.IsSpawned,
                            state.SocketPendingChange,
                            out string message,
                            out MessageType messageType))
                    {
                        SetStatus(message, messageType);
                        Repaint();
                        return;
                    }

                    state.HasPendingSaveCapture = true;
                    state.SavedProfileThisSession = false;
                    state.JustSavedProfileThisSession = false;
                    state.WorkflowStepOverride = CCS_EquipmentFitStudioWorkflowStep.SaveProfile;
                    SetStatus(message, messageType);
                    Repaint();
                    break;
            }
        }

        private void SaveActiveProfile()
        {
            switch (state.Mode)
            {
                case CCS_EquipmentFitStudioMode.IkTargetTuner:
                    SaveRevolverAimIkPoseProfile();
                    break;
                default:
                    if (!CanSavePendingCapture())
                    {
                        SetStatus("Capture values first. Save needs a pending captured transform.", MessageType.Warning);
                        return;
                    }

                    if (!HasRevolverAttachmentFitProfileForSelectedSocket())
                    {
                        SetStatus(
                            "No revolver fit profile is mapped for this socket. Select Right Hip Holster or Right Hand.",
                            MessageType.Error);
                        return;
                    }

                    if (!CCS_EquipmentFitProfilePersistenceUtility.TrySavePendingCaptureDetailed(
                            state.SocketPendingChange,
                            state.SelectedSocketId,
                            out CCS_EquipmentFitProfileSaveResult saveResult))
                    {
                        SetStatus(saveResult.Message, MessageType.Error);
                        Repaint();
                        return;
                    }

                    state.LastSaveConfirmationMessage = saveResult.Message;
                    state.SelectedAttachmentFitProfile =
                        CCS_EquipmentFitProfilePersistenceUtility.LoadProfileFromDisk(saveResult.ProfilePath);
                    state.HasPendingSaveCapture = false;
                    state.SavedProfileThisSession = true;
                    state.JustSavedProfileThisSession = true;
                    state.WorkflowStepOverride = CCS_EquipmentFitStudioWorkflowStep.TestSavedFit;
                    SetStatus(GetSlotSpecificSaveStatusMessage(), MessageType.Info);
                    Repaint();
                    break;
            }
        }

        private bool CanCaptureLiveValues()
        {
            if (!CanUseFitCaptureAndSave())
            {
                return false;
            }

            if (state.Mode == CCS_EquipmentFitStudioMode.IkTargetTuner)
            {
                return GetSelectedIkTargetTransform() != null;
            }

            return state.PlayerRoot != null
                && GetSelectedSocketTransform() != null
                && HasRevolverAttachmentFitProfileForSelectedSocket();
        }

        private bool CanSavePendingCapture()
        {
            if (!CanUseFitCaptureAndSave())
            {
                return false;
            }

            if (state.Mode == CCS_EquipmentFitStudioMode.IkTargetTuner)
            {
                return GetIkTargetsRoot() != null;
            }

            return state.HasPendingSaveCapture
                && state.SocketPendingChange.HasCaptured
                && HasRevolverAttachmentFitProfileForSelectedSocket();
        }

        private void ApplyActiveSavedProfile()
        {
            switch (state.Mode)
            {
                case CCS_EquipmentFitStudioMode.IkTargetTuner:
                    Transform ikRoot = GetIkTargetsRoot();
                    if (CCS_EquipmentFitStudioRevolverFitUtility.ApplyRevolverAimIkPoseProfile(ikRoot))
                    {
                        SetStatus("Applied saved revolver aim IK profile.", MessageType.Info);
                    }
                    else
                    {
                        SetStatus("Could not apply saved aim IK profile.", MessageType.Error);
                    }

                    break;
                default:
                    ApplyRevolverAttachmentFitProfile(GetSelectedSocketTransform());
                    ApplySocketProfileToPlayer();
                    break;
            }
        }

        private void TestSavedHolsterFit()
        {
            GameObject source = settings != null ? settings.DefaultPreviewWeaponPrefab : null;
            if (CCS_EquipmentFitStudioTestAttachmentUtility.TestSavedHolsterFit(state.PlayerRoot, source))
            {
                SetStatus("Test holster fit attached from reloaded profile (editor-only).", MessageType.Info);
                Repaint();
            }
            else
            {
                SetStatus("Could not test saved holster fit.", MessageType.Error);
            }
        }

        private void TestSavedEquippedFit()
        {
            GameObject source = settings != null ? settings.DefaultPreviewWeaponPrefab : null;
            if (CCS_EquipmentFitStudioTestAttachmentUtility.TestSavedEquippedFit(state.PlayerRoot, source))
            {
                SetStatus("Test equipped fit attached (editor-only).", MessageType.Info);
            }
            else
            {
                SetStatus("Could not test saved equipped fit.", MessageType.Error);
            }
        }

        private void ClearTestAttachments()
        {
            CCS_EquipmentFitStudioTestAttachmentUtility.ClearTestAttachments(state.PlayerRoot);
            SetStatus("Cleared editor test attachments.", MessageType.Info);
        }

        private void ClearPreviewAndWeights()
        {
            previewItem.DestroyPreview();
            state.PreviewItemSpawned = false;
            previewRigWeight = 0f;
            previewRightHandIkWeight = 0f;
            previewLeftHandIkWeight = 0f;
            previewAimWeight = 0f;
            ApplyIkPreviewWeights();
            SetStatus("Preview cleared and IK preview weights reset.", MessageType.Info);
        }

        private bool HasTestAttachments()
        {
            return CCS_EquipmentFitStudioTestAttachmentUtility.HasAnyTestAttachment(state.PlayerRoot);
        }

        private bool HasNonZeroIkPreviewWeights()
        {
            return previewRigWeight != 0f
                || previewRightHandIkWeight != 0f
                || previewLeftHandIkWeight != 0f
                || previewAimWeight != 0f;
        }

        private bool RequiresCleanup()
        {
            return previewItem.IsSpawned
                || HasTestAttachments()
                || HasNonZeroIkPreviewWeights()
                || CCS_EquipmentFitStudioPosePreviewUtility.IsPosePreviewActive
                || CCS_EquipmentFitStudioPreviewPlayerUtility.FindExistingPreviewPlayer() != null;
        }

        private CCS_EquipmentFitStudioWorkflowStep GetActiveWorkflowStep()
        {
            return CCS_EquipmentFitStudioWorkflowAccordion.ResolveActiveStep(
                state.PlayerRoot,
                state.SelectedWeaponId,
                state.SelectedSocketId,
                previewItem.IsSpawned,
                state.HasPendingSaveCapture,
                state.JustSavedProfileThisSession,
                HasTestAttachments(),
                IsSelectedSocketCompatible(),
                state.WorkflowStepOverride);
        }

        private void CleanupTemporaryObjects(bool resetIkPreviewWeights, bool clearTestAttachments)
        {
            previewItem.DestroyPreview();
            previewCamera.DestroyCamera();
            state.PreviewItemSpawned = false;
            if (clearTestAttachments)
            {
                CCS_EquipmentFitStudioTestAttachmentUtility.ClearTestAttachments(state.PlayerRoot);
            }

            if (resetIkPreviewWeights)
            {
                ResetIkPreviewWeights();
            }

            CCS_EquipmentFitStudioPosePreviewUtility.ClearAllPosePreview();
            state.PosePreviewMode = CCS_EquipmentFitStudioPosePreviewMode.Neutral;
            state.UserManuallySelectedPosePreview = false;

            if (state.UsesEditorPreviewPlayer)
            {
                CCS_EquipmentFitStudioPreviewPlayerUtility.ClearPreviewPlayer();
                state.PlayerRoot = null;
                state.UsesEditorPreviewPlayer = false;
            }

            CCS_EquipmentFitStudioCleanupUtility.CleanupEditorTemporaryObjectsInOpenScenes();
        }

        private int GetSelectedSocketIndex()
        {
            for (int i = 0; i < CCS_EquipmentConstants.RequiredSocketIds.Length; i++)
            {
                if (CCS_EquipmentConstants.RequiredSocketIds[i] == state.SelectedSocketId)
                {
                    return i;
                }
            }

            return 0;
        }

        private static Vector3 GetIkLocalPosition(Transform ikRoot, string childName)
        {
            Transform child = ikRoot.Find(childName);
            return child != null ? child.localPosition : Vector3.zero;
        }

        private static void WriteVector3(SerializedObject serializedObject, string propertyName, Vector3 value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.vector3Value = value;
            }
        }

        private static Transform FindDeepChild(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform match = FindDeepChild(root.GetChild(i), childName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private void SetStatus(string message, MessageType type)
        {
            statusMessage = message;
            statusType = type;
        }

        #endregion
    }
}
