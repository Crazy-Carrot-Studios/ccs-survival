using CCS.Modules.Weapons;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioWindow.Revamp
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: v0.6.8 editor-only Fit Studio layout — Fit Target first, compact preview UI.
// PLACEMENT: Partial class for Equipment Fit Studio window.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Play Mode shows one minimal message. Camera controls live outside viewport.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public sealed partial class CCS_EquipmentFitStudioWindow
    {
        private const string FitTargetEditorPrefKey = "CCS.EquipmentFitStudio.FitTarget";

        private static readonly string[] NudgeStepLabels = { "0.001", "0.005", "0.01", "0.05" };

        private static readonly float[] NudgeStepValues = { 0.001f, 0.005f, 0.01f, 0.05f };

        private static readonly string[] CameraPresetLabels =
        {
            "Target Default",
            "Full Body",
            "Upper Body",
            "Right Hand",
            "Right Hip",
            "Weapon Close-Up",
        };

        private static readonly CCS_EquipmentFitStudioCameraPreset[] CameraPresetValues =
        {
            CCS_EquipmentFitStudioCameraPreset.Frame,
            CCS_EquipmentFitStudioCameraPreset.FullBody,
            CCS_EquipmentFitStudioCameraPreset.UpperBody,
            CCS_EquipmentFitStudioCameraPreset.RightHand,
            CCS_EquipmentFitStudioCameraPreset.RightHip,
            CCS_EquipmentFitStudioCameraPreset.WeaponCloseUp,
        };

        private Vector2 rightScrollPosition;

        private int selectedCameraPresetIndex;

        private bool pendingAutoLoadPreview;

        private bool fitTargetDialogQueued;

        private void DrawRevampedLayout()
        {
            DrawRevampedHeader();
            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
                DrawPlayModeReadOnlyPanel();
                EditorGUILayout.EndVertical();
                DrawRevampedStatusBar();
                return;
            }

            DrawRevampedTopBar();
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            DrawRevampedGuidePanel();
            DrawRevampedPreviewViewport();
            DrawRevampedTransformPanel();
            EditorGUILayout.EndHorizontal();
            DrawRevampedBottomActionBar();
            DrawRevampedStatusBar();
        }

        private void DrawRevampedHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(
                "CCS Equipment Fit Studio",
                CCS_EquipmentFitStudioStyles.TitleLabel,
                GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField(
                "Editor Mode Only • Profile Tuning",
                CCS_EquipmentFitStudioStyles.HeaderMetaLabel,
                GUILayout.Width(220f));
            EditorGUILayout.LabelField(
                CCS_EquipmentConstants.EquipmentFitStudioVersionLabel,
                CCS_EquipmentFitStudioStyles.HeaderMetaLabel,
                GUILayout.Width(52f));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPlayModeReadOnlyPanel()
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField(
                "Equipment Fit Studio works in Editor Mode only.",
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Exit Play Mode to edit equipment fit profiles.",
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "Use Play Mode only to test saved profiles in-game.",
                EditorStyles.miniLabel);
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField(
                "Runtime profile verification is active through gameplay systems.",
                EditorStyles.centeredGreyMiniLabel);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
        }

        private void DrawRevampedTopBar()
        {
            CCS_EquipmentFitStudioFitTargetRoute route = GetActiveFitTargetRoute();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            int fitTargetIndex = (int)state.FitTarget;
            EditorGUI.BeginChangeCheck();
            fitTargetIndex = EditorGUILayout.Popup(
                "Fit Target",
                fitTargetIndex,
                CCS_EquipmentFitStudioFitTargetRoutingUtility.FitTargetLabels,
                GUILayout.MinWidth(180f),
                GUILayout.MaxWidth(240f));
            if (EditorGUI.EndChangeCheck())
            {
                RequestFitTargetChange((CCS_EquipmentFitStudioFitTarget)fitTargetIndex);
            }

            EditorGUILayout.LabelField("Weapon / Item", GUILayout.Width(88f));
            EditorGUILayout.LabelField(state.SelectedWeaponId, GUILayout.MinWidth(120f), GUILayout.MaxWidth(180f));

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(
                "Profile Asset",
                state.SelectedAttachmentFitProfile,
                typeof(CCS_WeaponAttachmentFitProfile),
                false,
                GUILayout.MinWidth(160f),
                GUILayout.MaxWidth(260f));
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Open Profile Asset", GUILayout.Width(128f)))
            {
                OpenActiveProfileAsset();
            }

            EditorGUILayout.EndHorizontal();

            DrawRevampedStatusChips(route);
            EditorGUILayout.EndVertical();

            if (!CCS_EquipmentFitStudioFitTargetRoutingUtility.RouteMatchesSelection(
                    state.FitTarget,
                    state.SelectedSocketId,
                    route.ProfilePath))
            {
                EditorGUILayout.HelpBox(
                    "Fit Target routing mismatch. Click Load Preview to sync socket and profile.",
                    MessageType.Error);
            }
        }

        private void DrawRevampedStatusChips(CCS_EquipmentFitStudioFitTargetRoute route)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(4f);

            string poseChip = route.PoseMode == CCS_EquipmentFitStudioPosePreviewMode.Neutral
                ? "Auto Pose: Neutral"
                : "Auto Pose: Revolver Aim";
            EditorGUILayout.LabelField(poseChip, CCS_EquipmentFitStudioStyles.StatusOkLabel, GUILayout.Width(150f));

            string loadedChip = state.ProfileLoadedFromSo ? "Preview Loaded from SO" : "Preview Not Loaded";
            GUIStyle loadedStyle = state.ProfileLoadedFromSo
                ? CCS_EquipmentFitStudioStyles.StatusOkLabel
                : CCS_EquipmentFitStudioStyles.StatusWarnLabel;
            EditorGUILayout.LabelField(loadedChip, loadedStyle, GUILayout.Width(170f));

            string dirtyChip = IsRevampedProfileDirty() ? "Profile Dirty" : "Profile Clean";
            GUIStyle dirtyStyle = IsRevampedProfileDirty()
                ? CCS_EquipmentFitStudioStyles.StatusWarnLabel
                : CCS_EquipmentFitStudioStyles.StatusOkLabel;
            EditorGUILayout.LabelField(dirtyChip, dirtyStyle, GUILayout.Width(110f));

            string zeroedChip = !previewItem.IsSpawned
                ? "Preview Not Spawned"
                : previewItem.IsZeroed ? "Preview Zeroed" : "Preview Not Zeroed";
            GUIStyle zeroedStyle = previewItem.IsSpawned && previewItem.IsZeroed
                ? CCS_EquipmentFitStudioStyles.StatusOkLabel
                : previewItem.IsSpawned
                    ? CCS_EquipmentFitStudioStyles.StatusErrorLabel
                    : CCS_EquipmentFitStudioStyles.StatusWarnLabel;
            EditorGUILayout.LabelField(zeroedChip, zeroedStyle, GUILayout.Width(130f));

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRevampedGuidePanel()
        {
            float leftPanelWidth = GetLeftPanelWidth();
            scrollPosition = EditorGUILayout.BeginScrollView(
                scrollPosition,
                GUILayout.Width(leftPanelWidth),
                GUILayout.MinWidth(LeftPanelMinWidth),
                GUILayout.MaxWidth(LeftPanelMaxWidth),
                GUILayout.ExpandHeight(true));

            CCS_EquipmentFitStudioFitTargetRoute route = GetActiveFitTargetRoute();
            string poseLabel = route.PoseMode == CCS_EquipmentFitStudioPosePreviewMode.Neutral ? "Neutral" : "Revolver Aim";
            DrawGuideCard(
                "1. Select Fit Target",
                "Active Target: " + CCS_EquipmentFitStudioFitTargetRoutingUtility.FitTargetLabels[(int)state.FitTarget]
                + "\nWeapon: " + route.WeaponId
                + "\nSocket: " + route.SocketId
                + "\nProfile: " + route.ProfileFileName);
            DrawGuideCard(
                "2. Auto Load Preview",
                "Scene: " + route.SceneLabel
                + "\nPose: " + poseLabel
                + "\nFocus: " + route.FocusLabel
                + "\nPreview: " + (previewItem.IsSpawned ? "Loaded" : "Not Loaded"));
            DrawGuideCard(
                "3. Adjust Transform",
                "Adjust only the attachment/profile offset.\nThe preview visual remains zeroed.");
            DrawGuideCard(
                "4. Save Profile",
                "Save writes the current profile offset to the selected SO.\n"
                + "Finger and palm IK are deferred for v0.6.8.");

            if (state.FitTarget == CCS_EquipmentFitStudioFitTarget.EquippedItem
                && CCS_EquipmentFitStudioEquippedPoseGuidanceUtility.ShouldShowTwoHandedSourceWarning(state.EquippedPoseType))
            {
                EditorGUILayout.HelpBox(
                    CCS_EquipmentFitStudioEquippedPoseGuidanceUtility.TwoHandedSourcePoseWarning
                    + "\nTune the weapon as close as possible for now.",
                    MessageType.Warning);
            }

            EditorGUILayout.EndScrollView();
        }

        private static void DrawGuideCard(string title, string body)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(body, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(3f);
        }

        private void DrawRevampedPreviewViewport()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            DrawRevampedCameraControlStrip();

            float previewMinHeight = Mathf.Max(220f, position.height - 280f);
            Rect previewRect = GUILayoutUtility.GetRect(
                GUIContent.none,
                GUIStyle.none,
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true),
                GUILayout.MinHeight(previewMinHeight));
            lastPreviewRect = previewRect;

            if (previewRect.width > 1f && previewRect.height > 1f)
            {
                EditorGUI.DrawRect(previewRect, new Color(0.06f, 0.07f, 0.09f));
                Rect innerRect = new Rect(
                    previewRect.x + 2f,
                    previewRect.y + 2f,
                    previewRect.width - 4f,
                    previewRect.height - 4f);
                previewCamera.EnsureRenderTexture(
                    Mathf.Max(64, (int)innerRect.width),
                    Mathf.Max(64, (int)innerRect.height));
                previewCamera.SetFrameContext(state.PlayerRoot, state.SelectedSocketId);
                previewCamera.HandleInput(innerRect, Event.current);
                previewCamera.RenderNow();
                if (previewCamera.RenderTexture != null)
                {
                    GUI.DrawTexture(innerRect, previewCamera.RenderTexture, ScaleMode.StretchToFill, false);
                }

                DrawRevampedPreviewOverlay(innerRect);
            }

            EditorGUILayout.LabelField(
                "Orbit: Left Drag | Pan: Middle Drag | Zoom: Mouse Wheel | Frame: F",
                EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawRevampedCameraControlStrip()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Camera", GUILayout.Width(52f));

            if (GUILayout.Button("Fit Target Default", EditorStyles.toolbarButton, GUILayout.Width(118f)))
            {
                ResetRevampedCamera();
            }

            if (GUILayout.Button("Frame", EditorStyles.toolbarButton, GUILayout.Width(52f)))
            {
                previewCamera.FrameCurrentFocus();
            }

            if (GUILayout.Button("Reset Camera", EditorStyles.toolbarButton, GUILayout.Width(92f)))
            {
                previewCamera.ResetCameraOrientation();
                ResetRevampedCamera();
            }

            GUILayout.Space(8f);
            EditorGUILayout.LabelField("Preset", GUILayout.Width(46f));
            EditorGUI.BeginChangeCheck();
            selectedCameraPresetIndex = EditorGUILayout.Popup(
                selectedCameraPresetIndex,
                CameraPresetLabels,
                GUILayout.MinWidth(120f),
                GUILayout.MaxWidth(180f));
            if (EditorGUI.EndChangeCheck())
            {
                ApplySelectedCameraPreset();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void ApplySelectedCameraPreset()
        {
            if (state.PlayerRoot == null)
            {
                return;
            }

            if (selectedCameraPresetIndex <= 0)
            {
                ResetRevampedCamera();
                return;
            }

            int presetIndex = Mathf.Clamp(selectedCameraPresetIndex, 0, CameraPresetValues.Length - 1);
            previewCamera.ApplyPreset(
                CameraPresetValues[presetIndex],
                state.PlayerRoot,
                state.SelectedSocketId);
        }

        private void DrawRevampedPreviewOverlay(Rect previewRect)
        {
            CCS_EquipmentFitStudioFitTargetRoute route = GetActiveFitTargetRoute();
            string poseLabel = route.PoseMode == CCS_EquipmentFitStudioPosePreviewMode.Neutral ? "Neutral" : "Revolver Aim";
            string modeLabel = CCS_EquipmentFitStudioFitTargetRoutingUtility.FitTargetLabels[(int)state.FitTarget];
            string profileLabel = route.ProfileFileName.Replace(".asset", string.Empty);
            Rect overlayRect = new Rect(previewRect.x + 6f, previewRect.y + 6f, previewRect.width - 12f, 72f);
            GUI.Label(
                overlayRect,
                "Socket: " + route.SocketId
                + "\nMode: " + modeLabel
                + "\nPose: " + poseLabel
                + "\nPreview: " + (previewItem.IsSpawned && previewItem.IsZeroed ? "Zeroed" : "Not Ready")
                + "\nProfile: " + profileLabel,
                CCS_EquipmentFitStudioStyles.PreviewOverlayLabel);
        }

        private void DrawRevampedTransformPanel()
        {
            float rightWidth = GetRightPanelWidth();
            EditorGUILayout.BeginVertical(
                GUILayout.Width(rightWidth),
                GUILayout.MinWidth(RightPanelMinWidth),
                GUILayout.MaxWidth(RightPanelMaxWidth),
                GUILayout.ExpandHeight(true));

            rightScrollPosition = EditorGUILayout.BeginScrollView(
                rightScrollPosition,
                GUILayout.ExpandHeight(true));

            EditorGUILayout.LabelField("Attachment / Profile Offset", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Editing the attachment/profile offset only.\nPreview visual stays zeroed.",
                MessageType.Info);

            Transform socketTransform = GetSelectedSocketTransform();
            if (socketTransform == null)
            {
                EditorGUILayout.HelpBox("Load preview to edit profile offset.", MessageType.Warning);
            }
            else
            {
                Transform attachmentRoot = GetPreviewAttachmentRootTransform();
                if (attachmentRoot == null)
                {
                    string attachmentRootName =
                        CCS_EquipmentFitStudioPreviewAttachmentUtility.GetAttachmentRootObjectName(state.FitTarget);
                    attachmentRoot = CCS_EquipmentFitStudioPreviewAttachmentUtility.EnsurePreviewAttachmentRoot(
                        socketTransform,
                        attachmentRootName);
                }

                DrawWeaponRotationTransformPanel(attachmentRoot);
                DrawRevampedProfileInfo(socketTransform);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawRevampedProfileInfo(Transform socketTransform)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Profile Info", EditorStyles.boldLabel);
            CCS_EquipmentSocketAnchor anchor = socketTransform.GetComponent<CCS_EquipmentSocketAnchor>();
            EditorGUILayout.LabelField("Socket", anchor != null ? anchor.SocketId : state.SelectedSocketId);
            EditorGUILayout.LabelField(
                "Parent Bone",
                anchor != null ? anchor.ParentBone.ToString() : "Unknown");
            if (anchor != null && anchor.AllowedItemTypes != null && anchor.AllowedItemTypes.Count > 0)
            {
                EditorGUILayout.LabelField("Allowed Types", string.Join(", ", anchor.AllowedItemTypes));
            }

            EditorGUILayout.LabelField("Profile", IsRevampedProfileDirty() ? "Dirty" : "Clean");
            EditorGUILayout.LabelField("Loaded From SO", state.ProfileLoadedFromSo ? "Yes" : "No");
            EditorGUILayout.LabelField(
                "Last Saved",
                string.IsNullOrEmpty(state.LastSavedDisplay) ? "Not saved" : state.LastSavedDisplay);
        }

        private void DrawRevampedBottomActionBar()
        {
            EditorGUILayout.BeginVertical(
                CCS_EquipmentFitStudioStyles.BottomBarBackground,
                GUILayout.MinHeight(BottomActionBarHeight),
                GUILayout.MaxHeight(BottomActionBarHeight + 8f),
                GUILayout.ExpandWidth(true));

            EditorGUILayout.BeginHorizontal();
            if (CCS_EquipmentFitStudioStyles.DrawBottomActionButton(
                    "Load Preview",
                    CCS_EquipmentFitStudioButtonKind.SpawnPreview,
                    "Reload preview player, pose, weapon, camera, and selected SO values.",
                    true,
                    GUILayout.MinWidth(100f),
                    GUILayout.ExpandWidth(true)))
            {
                LoadRevampedPreview();
            }

            if (CCS_EquipmentFitStudioStyles.DrawBottomActionButton(
                    "Reset to Profile",
                    CCS_EquipmentFitStudioButtonKind.ApplyProfile,
                    "Revert current edit fields to saved SO values.",
                    state.PlayerRoot != null,
                    GUILayout.MinWidth(110f),
                    GUILayout.ExpandWidth(true)))
            {
                Transform socketTransform = GetSelectedSocketTransform();
                if (socketTransform != null)
                {
                    ResetSocketToProfile(socketTransform);
                    state.ClearPendingChanges();
                    SetStatus("Reverted to saved profile values.", MessageType.Info);
                }
            }

            if (CCS_EquipmentFitStudioStyles.DrawBottomActionButton(
                    "Reset to Default",
                    CCS_EquipmentFitStudioButtonKind.Warning,
                    "Set profile offset to zero/default seed after confirmation.",
                    state.PlayerRoot != null,
                    GUILayout.MinWidth(110f),
                    GUILayout.ExpandWidth(true)))
            {
                QueueResetToDefaultDialog();
            }

            if (CCS_EquipmentFitStudioStyles.DrawBottomActionButton(
                    "Validate",
                    CCS_EquipmentFitStudioButtonKind.Validate,
                    "Run Fit Studio validation and cleanup checks.",
                    true,
                    GUILayout.MinWidth(80f),
                    GUILayout.ExpandWidth(true)))
            {
                RunValidation();
            }

            GUILayout.FlexibleSpace();
            if (CCS_EquipmentFitStudioStyles.DrawBottomActionButton(
                    "Save Profile",
                    CCS_EquipmentFitStudioButtonKind.SaveProfile,
                    "Save current transform values to selected profile SO.",
                    CanSaveRevampedProfile(),
                    GUILayout.MinWidth(140f),
                    GUILayout.Height(32f)))
            {
                SaveRevampedProfile();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawRevampedStatusBar()
        {
            if (!string.IsNullOrEmpty(statusMessage) && statusMessage != "Ready.")
            {
                EditorGUILayout.HelpBox(statusMessage, statusType);
            }
        }

        private void LoadFitTargetFromEditorPrefs()
        {
            int saved = EditorPrefs.GetInt(FitTargetEditorPrefKey, (int)CCS_EquipmentFitStudioFitTarget.HolsteredItem);
            saved = Mathf.Clamp(saved, 0, 1);
            state.FitTarget = (CCS_EquipmentFitStudioFitTarget)saved;
        }

        private void SaveFitTargetToEditorPrefs()
        {
            EditorPrefs.SetInt(FitTargetEditorPrefKey, (int)state.FitTarget);
        }

        private void SyncFitTargetRoutingToState()
        {
            CCS_EquipmentFitStudioFitTargetRoute route =
                CCS_EquipmentFitStudioFitTargetRoutingUtility.Resolve(state.FitTarget);
            state.SelectedSocketId = route.SocketId;
            state.SelectedWeaponId = route.WeaponId;
            state.PosePreviewMode = route.PoseMode;
            state.FitStudioMode = CCS_EquipmentFitStudioFitMode.EditFitPreview;
            state.Mode = CCS_EquipmentFitStudioMode.SocketTuner;
            state.SelectedAttachmentFitProfile =
                CCS_EquipmentFitProfilePersistenceUtility.LoadProfileFromDisk(route.ProfilePath);
            state.ProfileLoadedFromSo = state.SelectedAttachmentFitProfile != null;
            InitializePendingBaselineFromProfile();
        }

        private CCS_EquipmentFitStudioFitTargetRoute GetActiveFitTargetRoute()
        {
            return CCS_EquipmentFitStudioFitTargetRoutingUtility.Resolve(state.FitTarget);
        }

        private void RequestFitTargetChange(CCS_EquipmentFitStudioFitTarget newTarget)
        {
            if (newTarget == state.FitTarget)
            {
                return;
            }

            if (IsRevampedProfileDirty())
            {
                QueueFitTargetChangeDialog(newTarget);
                return;
            }

            ApplyFitTargetChange(newTarget);
        }

        private void QueueFitTargetChangeDialog(CCS_EquipmentFitStudioFitTarget newTarget)
        {
            if (fitTargetDialogQueued)
            {
                return;
            }

            fitTargetDialogQueued = true;
            CCS_EquipmentFitStudioImGuiUtility.EnqueueDeferredAction(() =>
            {
                fitTargetDialogQueued = false;
                bool discard = EditorUtility.DisplayDialog(
                    "Unsaved Profile Changes",
                    "Changing Fit Target will discard unsaved offset edits. Continue?",
                    "Discard Changes",
                    "Cancel");
                if (discard)
                {
                    ApplyFitTargetChange(newTarget);
                }

                Repaint();
            });
        }

        private void QueueResetToDefaultDialog()
        {
            CCS_EquipmentFitStudioImGuiUtility.EnqueueDeferredAction(() =>
            {
                bool confirm = EditorUtility.DisplayDialog(
                    "Reset to Default",
                    "Set the profile offset to the default seed values?",
                    "Reset",
                    "Cancel");
                if (confirm)
                {
                    ResetProfileOffsetToDefaultSeed();
                }
            });
        }

        private void ApplyFitTargetChange(CCS_EquipmentFitStudioFitTarget newTarget)
        {
            CleanupTemporaryObjects(resetIkPreviewWeights: true, clearTestAttachments: true);
            previewItem.DestroyPreview();
            state.FitTarget = newTarget;
            SaveFitTargetToEditorPrefs();
            state.ClearPendingChanges();
            SyncFitTargetRoutingToState();
            selectedCameraPresetIndex = 0;
            LoadRevampedPreview();
        }

        private void RequestAutoLoadPreview(bool deferUntilAfterGui)
        {
            if (deferUntilAfterGui)
            {
                pendingAutoLoadPreview = true;
                return;
            }

            LoadRevampedPreview();
        }

        private void LoadRevampedPreview()
        {
            CleanupTemporaryObjects(resetIkPreviewWeights: true, clearTestAttachments: true);
            previewItem.DestroyPreview();
            SyncFitTargetRoutingToState();

            if (!CCS_EquipmentFitStudioAutoLoadUtility.TryAutoLoadPreviewSetup(
                    state,
                    settings,
                    previewItem,
                    previewCamera,
                    out string errorMessage))
            {
                state.ProfileLoadedFromSo = false;
                SetStatus(errorMessage, MessageType.Error);
                return;
            }

            state.ProfileLoadedFromSo = true;
            previewCamera.SetFrameContext(state.PlayerRoot, state.SelectedSocketId);
            selectedCameraPresetIndex = 0;
            InitializePendingBaselineFromProfile();
            SyncPendingFromAttachmentRoot(GetPreviewAttachmentRootTransform());
            SetStatus(
                "Preview loaded for "
                + CCS_EquipmentFitStudioFitTargetRoutingUtility.FitTargetLabels[(int)state.FitTarget]
                + ".",
                MessageType.Info);
        }

        private void ResetRevampedCamera()
        {
            CCS_EquipmentFitStudioFitTargetRoute route = GetActiveFitTargetRoute();
            previewCamera.ResetCameraOrientation();
            previewCamera.ApplyPreset(route.DefaultCameraPreset, state.PlayerRoot, route.SocketId);
        }

        private void OpenActiveProfileAsset()
        {
            CCS_EquipmentFitStudioFitTargetRoute route = GetActiveFitTargetRoute();
            CCS_WeaponAttachmentFitProfile profile =
                CCS_EquipmentFitProfilePersistenceUtility.LoadProfileFromDisk(route.ProfilePath);
            if (profile != null)
            {
                Selection.activeObject = profile;
                EditorGUIUtility.PingObject(profile);
            }
        }

        private bool IsRevampedProfileDirty()
        {
            Transform attachmentRoot = GetPreviewAttachmentRootTransform();
            CCS_WeaponAttachmentFitProfile profile = state.SelectedAttachmentFitProfile;
            if (attachmentRoot == null || profile == null)
            {
                return state.HasPendingSaveCapture;
            }

            return !CCS_EquipmentFitStudioPreviewAttachmentUtility.AttachmentRootMatchesProfile(
                state.PlayerRoot,
                state.SelectedSocketId,
                attachmentRoot,
                profile);
        }

        private bool CanSaveRevampedProfile()
        {
            return !EditorApplication.isPlaying
                && state.PlayerRoot != null
                && previewItem.IsSpawned
                && previewItem.IsZeroed
                && CCS_EquipmentFitStudioFitTargetRoutingUtility.RouteMatchesSelection(
                    state.FitTarget,
                    state.SelectedSocketId,
                    GetActiveFitTargetRoute().ProfilePath);
        }

        private void SaveRevampedProfile()
        {
            if (!CanSaveRevampedProfile())
            {
                SetStatus(
                    "Cannot save. Load preview, keep visual zeroed, and verify Fit Target routing.",
                    MessageType.Error);
                return;
            }

            Transform socketTransform = GetSelectedSocketTransform();
            if (!CCS_EquipmentFitStudioCaptureUtility.TryCaptureSocketValues(
                    state.PlayerRoot,
                    socketTransform,
                    state.SelectedSocketId,
                    previewItem.IsSpawned,
                    state.SocketPendingChange,
                    out string captureMessage,
                    out MessageType captureType))
            {
                SetStatus(captureMessage, captureType);
                return;
            }

            state.HasPendingSaveCapture = true;

            if (!CCS_EquipmentFitProfilePersistenceUtility.TrySavePendingCaptureDetailed(
                    state.SocketPendingChange,
                    state.SelectedSocketId,
                    out CCS_EquipmentFitProfileSaveResult saveResult))
            {
                SetStatus(saveResult.Message, MessageType.Error);
                return;
            }

            state.SelectedAttachmentFitProfile =
                CCS_EquipmentFitProfilePersistenceUtility.LoadProfileFromDisk(saveResult.ProfilePath);
            state.ProfileLoadedFromSo = state.SelectedAttachmentFitProfile != null;
            state.HasPendingSaveCapture = false;
            state.LastSavedDisplay = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            state.LastSaveConfirmationMessage =
                "Saved " + saveResult.ProfileName
                + " | Pos " + FormatVector(saveResult.Position)
                + " | Rot " + FormatVector(saveResult.Rotation)
                + " | Scale " + FormatVector(saveResult.Scale);
            InitializePendingBaselineFromProfile();
            SyncPendingFromAttachmentRoot(GetPreviewAttachmentRootTransform());
            SetStatus(state.LastSaveConfirmationMessage, MessageType.Info);
        }

        private static string FormatVector(Vector3 value)
        {
            return "(" + value.x.ToString("F3") + ", " + value.y.ToString("F3") + ", " + value.z.ToString("F3") + ")";
        }

        private void ResetProfileOffsetToDefaultSeed()
        {
            Transform socketTransform = GetSelectedSocketTransform();
            Transform attachmentRoot = GetPreviewAttachmentRootTransform();
            if (socketTransform == null || attachmentRoot == null || state.PlayerRoot == null)
            {
                return;
            }

            if (state.FitTarget == CCS_EquipmentFitStudioFitTarget.HolsteredItem)
            {
                CCS_WeaponAttachmentFitProfile seedProfile = ScriptableObject.CreateInstance<CCS_WeaponAttachmentFitProfile>();
                seedProfile.ApplySocketTransform(
                    state.SelectedSocketId,
                    CCS_EquipmentFitProfilePersistenceUtility.RightHipHolsterSeedPosition,
                    CCS_EquipmentFitProfilePersistenceUtility.RightHipHolsterSeedEuler,
                    Vector3.one);
                CCS_EquipmentFitStudioPreviewAttachmentUtility.TryApplyProfileToPreviewAttachment(
                    state.PlayerRoot,
                    socketTransform,
                    state.SelectedSocketId,
                    seedProfile,
                    CCS_EquipmentFitStudioPreviewAttachmentUtility.GetAttachmentRootObjectName(state.FitTarget));
                Object.DestroyImmediate(seedProfile);
            }
            else
            {
                CCS_EquipmentFitStudioRevolverFitUtility.ApplyRevolverAttachmentFitProfile(
                    state.SelectedSocketId,
                    state.PlayerRoot,
                    state.FitTarget);
                attachmentRoot.localPosition = Vector3.zero;
                attachmentRoot.localRotation = Quaternion.identity;
                attachmentRoot.localScale = Vector3.one;
            }

            previewItem.EnforceZeroedTransform();
            SyncPendingFromAttachmentRoot(attachmentRoot);
            SetStatus("Reset profile offset to default seed. Save Profile to persist.", MessageType.Warning);
        }

        private void ProcessRevampedDeferredActions()
        {
            if (!pendingAutoLoadPreview)
            {
                return;
            }

            pendingAutoLoadPreview = false;
            CCS_EquipmentFitStudioImGuiUtility.EnqueueDeferredAction(LoadRevampedPreview);
        }
    }
}
