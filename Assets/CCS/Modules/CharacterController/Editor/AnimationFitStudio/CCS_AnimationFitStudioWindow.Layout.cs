using System.Collections.Generic;
using CCS.Modules.CharacterController.Editor.EquipmentFitStudio;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AnimationFitStudioWindow.Layout
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Equipment Fit Studio-style layout for Animation Fit Studio final-pose editor.
// PLACEMENT: Partial class for Animation Fit Studio window.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Left guide, center viewport, right pose controls, simplified bottom action bar.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public sealed partial class CCS_AnimationFitStudioWindow
    {
        private static readonly string[] CameraPresetLabels =
        {
            "Fit Target Default",
            "Full Body",
            "Right Arm",
            "Right Hand",
            "Grip Close-Up",
            "Weapon Sightline",
        };

        private static readonly CCS_EquipmentFitStudioCameraPreset[] CameraPresetValues =
        {
            CCS_EquipmentFitStudioCameraPreset.Frame,
            CCS_EquipmentFitStudioCameraPreset.FullBody,
            CCS_EquipmentFitStudioCameraPreset.UpperBody,
            CCS_EquipmentFitStudioCameraPreset.RightHand,
            CCS_EquipmentFitStudioCameraPreset.WeaponCloseUp,
            CCS_EquipmentFitStudioCameraPreset.MuzzleView,
        };

        private static readonly string[] FingerCurlDirectionLabels =
        {
            "Normal",
            "Inverted",
        };

        private static readonly string[] FingerSegmentLabels =
        {
            "Whole Finger",
            "Proximal / Base",
            "Intermediate / Middle",
            "Distal / Tip",
        };

        private static readonly string[] NudgeStepLabels =
        {
            "1°",
            "5°",
            "15°",
        };

        private void DrawMainLayout()
        {
            DrawHeaderBar();

            if (EditorApplication.isPlaying)
            {
                DrawPlayModePanel();
                DrawStatusBar();
                return;
            }

            DrawTopStatusChips();

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            try
            {
                DrawLeftGuidePanel();
                DrawCenterPreviewViewport();
                DrawRightPosePanel();
            }
            finally
            {
                EditorGUILayout.EndHorizontal();
            }

            DrawBottomActionBar();
            DrawAdvancedDiagnosticsFoldout();
            DrawStatusBar();
        }

        private void DrawHeaderBar()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                "Animation Fit Studio",
                CCS_EquipmentFitStudioStyles.TitleLabel,
                GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField(
                CCS_AnimationFitStudioRuntimePolicy.PreviewOnlyStatusLabel
                + " / "
                + CCS_AnimationFitStudioRuntimePolicy.RuntimeCandidateStatusLabel,
                CCS_EquipmentFitStudioStyles.StatusWarnLabel,
                GUILayout.Width(220f));
            EditorGUILayout.LabelField(
                CCS_AnimationFitStudioConstants.VersionLabel,
                CCS_EquipmentFitStudioStyles.HeaderMetaLabel,
                GUILayout.Width(52f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Pose Target", GUILayout.Width(82f));
            EditorGUI.BeginChangeCheck();
            selectedPoseSourceIndex = EditorGUILayout.Popup(
                selectedPoseSourceIndex,
                CCS_AnimationFitStudioPoseSourceCatalog.PoseSourceLabels,
                GUILayout.MinWidth(240f));
            if (EditorGUI.EndChangeCheck())
            {
                int capturedIndex = selectedPoseSourceIndex;
                QueueGuiAction(() => OnPoseSourceChanged(capturedIndex));
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(
                "Clip Used By Controller: " + runtimeControllerClipInfo.ClipUsedByControllerLabel,
                CCS_EquipmentFitStudioStyles.HeaderMetaLabel,
                GUILayout.MinWidth(220f));
            EditorGUILayout.LabelField(
                "Clip Curve Mode: "
                + CCS_AnimationFitStudioClipCurveModeUtility.GetDisplayLabel(
                    CCS_AnimationFitStudioClipCurveModeUtility.DetectClipCurveMode(sourceClip)),
                CCS_EquipmentFitStudioStyles.HeaderMetaLabel,
                GUILayout.MinWidth(220f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Save Mode", GUILayout.Width(82f));
            EditorGUILayout.LabelField(
                CCS_AnimationFitStudioRuntimePolicy.OverwriteSaveModeLabel,
                CCS_EquipmentFitStudioStyles.HeaderMetaLabel,
                GUILayout.MinWidth(240f));
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(
                "Curve Hash Changed On Last Save: "
                + (lastCurveHashChangedOnSave ? "true" : "false"),
                CCS_EquipmentFitStudioStyles.HeaderMetaLabel,
                GUILayout.MinWidth(220f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(string.Empty, GUILayout.Width(82f));
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(
                "Save Target: " + GetSaveTargetClipFileName(),
                CCS_EquipmentFitStudioStyles.HeaderMetaLabel,
                GUILayout.MinWidth(360f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawPlayModePanel()
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(
                "Animation Fit Studio works in Editor Mode only.",
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField(
                "Exit Play Mode to preview fitted revolver aim pose edits.",
                EditorStyles.centeredGreyMiniLabel);
            GUILayout.FlexibleSpace();
        }

        private void DrawTopStatusChips()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            DrawChip(
                "Pose Source",
                CCS_AnimationFitStudioPoseSourceCatalog.GetPoseSourceDisplayLabel(selectedPoseSource),
                selectedPoseSource == CCS_AnimationFitStudioPoseSourceCatalog.DefaultPoseSourceKind);
            DrawChip(
                "Pose Preview",
                CCS_AnimationFitStudioPoseUtility.GetPosePreviewStatusLabel(previewState.PosePreviewStatus),
                previewState.PosePreviewStatus == CCS_AnimationFitStudioPosePreviewStatus.AppliedAimPose
                    || previewState.PosePreviewStatus == CCS_AnimationFitStudioPosePreviewStatus.SeedPose);
            DrawChip(
                "Finger Bones",
                previewState.FingerDiscovery != null
                    ? previewState.FingerDiscovery.GetSummaryLabel()
                    : "Unknown",
                previewState.FingerBonesFound);
            DrawChip(
                "Weapon Visual",
                previewState.WeaponVisualLoaded ? "Loaded" : "Missing",
                previewState.WeaponVisualLoaded);
            DrawChip(
                "Equipped Fit Profile",
                previewState.EquippedFitProfileApplied ? "Applied" : "Missing",
                previewState.EquippedFitProfileApplied);
            DrawChip(
                "Clip Used By Controller",
                runtimeControllerClipInfo.ClipUsedByControllerLabel,
                runtimeControllerClipInfo.SelectedClipUsedByController);
            DrawChip(
                "Save Mode",
                SaveModeLabels[Mathf.Clamp(selectedSaveModeIndex, 0, SaveModeLabels.Length - 1)],
                saveMode == CCS_AnimationFitStudioSaveMode.OverwriteControllerClip);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (runtimeControllerClipInfo.SelectedClipUsedByController)
            {
                EditorGUILayout.HelpBox(
                    CCS_AnimationFitStudioRuntimePolicy.RuntimeFullDrawEditNotice,
                    MessageType.Info);
            }
            else if (runtimeControllerClipInfo.HasBlockingWarning)
            {
                EditorGUILayout.HelpBox(runtimeControllerClipInfo.WarningMessage, MessageType.Warning);
            }
            else if (!string.IsNullOrEmpty(runtimeControllerClipInfo.PrimaryAimLoopClipPath))
            {
                EditorGUILayout.HelpBox(
                    "Primary controller aim loop: "
                    + runtimeControllerClipInfo.PrimaryAimLoopStateName
                    + " -> "
                    + runtimeControllerClipInfo.PrimaryAimLoopClipPath,
                    MessageType.None);
            }

            EditorGUILayout.HelpBox(
                "Workflow: Open studio -> select Runtime Aim Idle — FullDraw -> Load Preview / Weapon -> adjust pose -> Save Runtime FullDraw + Reimport -> Play Mode.",
                MessageType.None);

            if (previewState.PreviewPlayer != null && !previewState.FingerBonesFound)
            {
                EditorGUILayout.HelpBox(
                    CCS_AnimationFitStudioFingerDiscoveryUtility.MissingFingerBonesWarning,
                    MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }

        private static void DrawChip(string label, string value, bool ok)
        {
            GUIStyle style = ok
                ? CCS_EquipmentFitStudioStyles.StatusOkLabel
                : CCS_EquipmentFitStudioStyles.StatusWarnLabel;
            EditorGUILayout.LabelField(label + ": " + value, style, GUILayout.Width(190f));
        }

        private void DrawLeftGuidePanel()
        {
            float leftWidth = GetLeftPanelWidth();
            leftScrollPosition = EditorGUILayout.BeginScrollView(
                leftScrollPosition,
                GUILayout.Width(leftWidth),
                GUILayout.MinWidth(LeftPanelMinWidth),
                GUILayout.MaxWidth(LeftPanelMaxWidth),
                GUILayout.ExpandHeight(true));

            DrawGuideCard(
                "1. Pose Target",
                "Default: Runtime Aim Idle — FullDraw.\n"
                + "This is the same clip referenced by Revolver_AimIdle_FullDraw on the player Animator Controller.\n"
                + "Primary aim loop state: "
                + (string.IsNullOrEmpty(runtimeControllerClipInfo.PrimaryAimLoopStateName)
                    ? "(unknown)"
                    : runtimeControllerClipInfo.PrimaryAimLoopStateName));
            DrawGuideCard(
                "2. Load Preview / Weapon",
                "Loads the player, fitted M1879 revolver, and the controller FullDraw clip.\n"
                + "Clip Used By Controller: "
                + runtimeControllerClipInfo.ClipUsedByControllerLabel);
            DrawGuideCard(
                "3. Adjust Final Pose",
                "Tune only right arm, wrist, and grip.");
            DrawGuideCard(
                "4. Save Runtime FullDraw + Reimport",
                "Overwrites the controller FullDraw clip in place.\n"
                + "Does not modify Animator Controller.\n"
                + "Target: "
                + GetSaveTargetClipFileName());

            if (!string.IsNullOrEmpty(previewState.ProfileWarningMessage))
            {
                EditorGUILayout.HelpBox(previewState.ProfileWarningMessage, MessageType.Warning);
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

        private void DrawCenterPreviewViewport()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            DrawCameraControlStrip();

            float previewMinHeight = Mathf.Max(220f, position.height - 320f);
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

                previewCamera.EnsureCamera(settings);
                previewCamera.EnsureRenderTexture(
                    Mathf.Max(64, (int)innerRect.width),
                    Mathf.Max(64, (int)innerRect.height));
                previewCamera.SetFrameContext(
                    previewState.PreviewPlayer,
                    CCS_EquipmentConstants.HandSocketRightId);
                previewCamera.HandleInput(innerRect, Event.current);
                previewCamera.RenderNow();

                if (previewCamera.RenderTexture != null)
                {
                    GUI.DrawTexture(innerRect, previewCamera.RenderTexture, ScaleMode.StretchToFill, false);
                }

                DrawPreviewOverlay(innerRect);
            }

            EditorGUILayout.LabelField(
                "Orbit: Left Drag | Pan: Middle Drag | Zoom: Mouse Wheel | Frame: F",
                EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawPreviewOverlay(Rect previewRect)
        {
            string profileLabel = previewState.EquippedFitProfile != null
                ? previewState.EquippedFitProfile.name
                : CCS_AnimationFitStudioConstants.DefaultEquippedFitProfileName + " (missing)";
            Rect overlayRect = new Rect(previewRect.x + 6f, previewRect.y + 6f, previewRect.width - 12f, 88f);
            GUI.Label(
                overlayRect,
                "Pose Source: "
                + CCS_AnimationFitStudioPoseSourceCatalog.GetPoseSourceDisplayLabel(selectedPoseSource)
                + "\nSave Target: "
                + GetSaveTargetClipFileName()
                + "\nProfile: "
                + profileLabel
                + "\nPreview Weapon: "
                + (previewState.PreviewWeaponZeroed ? "Zeroed" : "Not Ready"),
                CCS_EquipmentFitStudioStyles.PreviewOverlayLabel);
        }

        private void DrawCameraControlStrip()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Camera", GUILayout.Width(52f));

            if (GUILayout.Button("Fit Target Default", EditorStyles.toolbarButton, GUILayout.Width(118f)))
            {
                FrameDefaultHandAndWeapon();
            }

            if (GUILayout.Button("Frame", EditorStyles.toolbarButton, GUILayout.Width(52f)))
            {
                previewCamera.FrameCurrentFocus();
            }

            if (GUILayout.Button("Reset Camera", EditorStyles.toolbarButton, GUILayout.Width(92f)))
            {
                previewCamera.ResetCameraOrientation();
                FrameDefaultHandAndWeapon();
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
            if (previewState.PreviewPlayer == null)
            {
                return;
            }

            if (selectedCameraPresetIndex <= 0)
            {
                FrameDefaultHandAndWeapon();
                return;
            }

            int presetIndex = Mathf.Clamp(selectedCameraPresetIndex, 0, CameraPresetValues.Length - 1);
            previewCamera.ApplyPreset(
                CameraPresetValues[presetIndex],
                previewState.PreviewPlayer,
                CCS_EquipmentConstants.HandSocketRightId);

            if (CameraPresetValues[presetIndex] == CCS_EquipmentFitStudioCameraPreset.WeaponCloseUp
                || CameraPresetValues[presetIndex] == CCS_EquipmentFitStudioCameraPreset.MuzzleView)
            {
                Transform weaponTarget =
                    CCS_AnimationFitStudioPreviewUtility.GetDefaultCameraFrameTarget(previewState);
                if (weaponTarget != null)
                {
                    previewCamera.FrameTransform(weaponTarget, 0.95f);
                }
            }
        }

        private void DrawRightPosePanel()
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

            EditorGUILayout.LabelField("Right Arm / Grip", EditorStyles.boldLabel);
            DrawFingerBoneStatus();
            DrawHumanoidMuscleReadoutPanel();
            DrawSelectedPartEditor();
            DrawQuickGripSection();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawSelectedPartEditor()
        {
            EditorGUILayout.Space(8f);
            EditorGUI.BeginDisabledGroup(!CanEditPoseParts());

            EditorGUILayout.LabelField("Edit Part", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            selectedEditPartIndex = EditorGUILayout.Popup(
                "Edit Part",
                selectedEditPartIndex,
                CCS_AnimationFitStudioEditPartCatalog.EditPartDisplayLabels);
            if (EditorGUI.EndChangeCheck())
            {
                selectedFingerSegmentIndex = 0;
            }

            EditorGUI.BeginChangeCheck();
            int nudgeIndex = EditorGUILayout.Popup("Nudge Step", GetNudgeIndex(nudgeDegrees), NudgeStepLabels);
            float nextNudge = nudgeIndex switch
            {
                0 => 1f,
                2 => 15f,
                _ => 5f,
            };
            if (!Mathf.Approximately(nextNudge, nudgeDegrees))
            {
                nudgeDegrees = nextNudge;
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Selected Part Controls", EditorStyles.miniBoldLabel);

            if (!TryGetSelectedEditPart(out CCS_AnimationFitStudioEditPartDefinition editPart))
            {
                EditorGUI.EndDisabledGroup();
                return;
            }

            if (IsSelectedPartFinger())
            {
                DrawFingerSegmentSelector();
            }

            DrawUnifiedPitchYawRollControls(editPart.PartId);

            if (IsSelectedPartFinger())
            {
                DrawFingerCurlSpreadControls();
            }

            EditorGUI.EndDisabledGroup();
        }

        private void DrawFingerSegmentSelector()
        {
            bool gripReady = previewState.PreviewPlayer != null && previewState.FingerBonesFound;
            if (!gripReady)
            {
                EditorGUILayout.HelpBox(
                    CCS_AnimationFitStudioFingerDiscoveryUtility.MissingFingerBonesWarning,
                    MessageType.Warning);
                return;
            }

            if (!previewState.WeaponVisualLoaded)
            {
                EditorGUILayout.HelpBox(
                    CCS_AnimationFitStudioPreviewUtility.MissingWeaponVisualWarning,
                    MessageType.Warning);
            }

            EditorGUI.BeginChangeCheck();
            selectedFingerSegmentIndex = EditorGUILayout.Popup(
                "Finger Segment",
                selectedFingerSegmentIndex,
                FingerSegmentLabels);
            if (EditorGUI.EndChangeCheck())
            {
                Repaint();
            }
        }

        private void DrawUnifiedPitchYawRollControls(string partId)
        {
            DrawAxisControlRow(partId, "Pitch", CCS_AnimationFitStudioHumanoidControlAxis.Pitch, nudgeDegrees);
            DrawAxisControlRow(partId, "Yaw", CCS_AnimationFitStudioHumanoidControlAxis.Yaw, nudgeDegrees);
            DrawAxisControlRow(partId, "Roll", CCS_AnimationFitStudioHumanoidControlAxis.Roll, nudgeDegrees);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Reset Selected Part"))
                {
                    ResetSelectedPart();
                }
            }

            DrawPartAxisMappingHints(partId);
        }

        private void DrawAxisControlRow(
            string partId,
            string axisLabel,
            CCS_AnimationFitStudioHumanoidControlAxis axis,
            float stepDegrees)
        {
            bool axisEnabled = IsPartAxisEnabled(partId, axis);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginDisabledGroup(!axisEnabled);
                if (GUILayout.Button(axisLabel + " -"))
                {
                    NudgePartAxis(partId, axis, -stepDegrees);
                }

                if (GUILayout.Button(axisLabel + " +"))
                {
                    NudgePartAxis(partId, axis, stepDegrees);
                }

                EditorGUI.EndDisabledGroup();
            }
        }

        private void NudgePartAxis(
            string partId,
            CCS_AnimationFitStudioHumanoidControlAxis axis,
            float signedDegrees)
        {
            if (IsSelectedPartFinger())
            {
                Vector3 deltaEuler = axis switch
                {
                    CCS_AnimationFitStudioHumanoidControlAxis.Pitch => new Vector3(signedDegrees, 0f, 0f),
                    CCS_AnimationFitStudioHumanoidControlAxis.Yaw => new Vector3(0f, signedDegrees, 0f),
                    _ => new Vector3(0f, 0f, signedDegrees),
                };
                NudgeSelectedFingerEuler(deltaEuler);
                return;
            }

            Vector3 bodyDelta = axis switch
            {
                CCS_AnimationFitStudioHumanoidControlAxis.Pitch => new Vector3(signedDegrees, 0f, 0f),
                CCS_AnimationFitStudioHumanoidControlAxis.Yaw => new Vector3(0f, signedDegrees, 0f),
                _ => new Vector3(0f, 0f, signedDegrees),
            };
            NudgePart(partId, bodyDelta);
        }

        private void DrawPartAxisMappingHints(string partId)
        {
            if (!IsHumanoidClipMode() || IsSelectedPartFinger())
            {
                return;
            }

            List<string> unmappedAxes = new List<string>(3);
            if (!IsPartAxisEnabled(partId, CCS_AnimationFitStudioHumanoidControlAxis.Pitch))
            {
                unmappedAxes.Add("Pitch");
            }

            if (!IsPartAxisEnabled(partId, CCS_AnimationFitStudioHumanoidControlAxis.Yaw))
            {
                unmappedAxes.Add("Yaw");
            }

            if (!IsPartAxisEnabled(partId, CCS_AnimationFitStudioHumanoidControlAxis.Roll))
            {
                unmappedAxes.Add("Roll");
            }

            if (unmappedAxes.Count == 0)
            {
                return;
            }

            EditorGUILayout.HelpBox(
                "No Humanoid muscle mapping for "
                + string.Join(", ", unmappedAxes)
                + ".",
                MessageType.None);
        }

        private void DrawFingerCurlSpreadControls()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Curl -")) NudgeSelectedFingerCurl(-nudgeDegrees);
                if (GUILayout.Button("Curl +")) NudgeSelectedFingerCurl(nudgeDegrees);
                if (GUILayout.Button("Spread -")) NudgeSelectedFingerSpread(-nudgeDegrees);
                if (GUILayout.Button("Spread +")) NudgeSelectedFingerSpread(nudgeDegrees);
            }
        }

        private void DrawFingerAxisControls(string fingerId)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Finger Axis", EditorStyles.miniBoldLabel);

            CCS_AnimationFitStudioPoseEditData poseEdits = previewState.PoseEdits;
            EditorGUI.BeginChangeCheck();
            poseEdits.UseSharedFingerAxisSettings = EditorGUILayout.Toggle(
                "Use Shared Finger Axis Settings",
                poseEdits.UseSharedFingerAxisSettings);
            if (EditorGUI.EndChangeCheck())
            {
                QueueGuiAction(ApplyCurrentPosePreview);
            }

            CCS_AnimationFitStudioFingerChainEditState chainEdit =
                poseEdits.GetOrCreateFingerChainEdit(fingerId);
            if (!poseEdits.UseSharedFingerAxisSettings)
            {
                EditorGUI.BeginChangeCheck();
                chainEdit.OverrideAxis = EditorGUILayout.Toggle(
                    "Override Axis For Selected Finger",
                    chainEdit.OverrideAxis);
                if (EditorGUI.EndChangeCheck())
                {
                    QueueGuiAction(ApplyCurrentPosePreview);
                }
            }

            CCS_AnimationFitStudioFingerAxisSettings axisSettings =
                !poseEdits.UseSharedFingerAxisSettings && chainEdit.OverrideAxis
                    ? chainEdit.AxisOverride
                    : poseEdits.SharedFingerAxisSettings;

            EditorGUI.BeginChangeCheck();
            int curlAxisIndex = CCS_AnimationFitStudioLocalAxisUtility.AxisKindToIndex(axisSettings.CurlAxis);
            curlAxisIndex = EditorGUILayout.Popup(
                "Finger Curl Axis",
                curlAxisIndex,
                CCS_AnimationFitStudioLocalAxisUtility.AxisLabels);
            int spreadAxisIndex = CCS_AnimationFitStudioLocalAxisUtility.AxisKindToIndex(axisSettings.SpreadAxis);
            spreadAxisIndex = EditorGUILayout.Popup(
                "Finger Spread Axis",
                spreadAxisIndex,
                CCS_AnimationFitStudioLocalAxisUtility.AxisLabels);
            if (EditorGUI.EndChangeCheck())
            {
                axisSettings.CurlAxis = CCS_AnimationFitStudioLocalAxisUtility.IndexToAxisKind(curlAxisIndex);
                axisSettings.SpreadAxis = CCS_AnimationFitStudioLocalAxisUtility.IndexToAxisKind(spreadAxisIndex);
                QueueGuiAction(ApplyCurrentPosePreview);
            }
        }

        private void DrawQuickGripSection()
        {
            EditorGUILayout.Space(10f);
            showQuickGrip = EditorGUILayout.Foldout(showQuickGrip, "Quick Grip", true);
            if (!showQuickGrip)
            {
                return;
            }

            EditorGUI.BeginDisabledGroup(!CanEditPoseParts());
            float nextGrip = EditorGUILayout.Slider("Grip Tightness", gripTightness, 0f, 1f);
            if (!Mathf.Approximately(nextGrip, gripTightness))
            {
                gripTightness = nextGrip;
                MarkDirtyAndRefreshPreview();
            }

            DrawQuickFingerSlider("right_thumb", "Thumb Curl");
            DrawQuickFingerSlider("right_index", "Index Curl");
            DrawQuickFingerSlider("right_middle", "Middle Curl");
            DrawQuickFingerSlider("right_ring", "Ring Curl");
            DrawQuickFingerSlider("right_pinky", "Pinky Curl");

            EditorGUI.BeginChangeCheck();
            fingerCurlDirectionIndex = EditorGUILayout.Popup(
                "Finger Curl Direction",
                fingerCurlDirectionIndex,
                FingerCurlDirectionLabels);
            if (EditorGUI.EndChangeCheck())
            {
                QueueGuiAction(ApplyCurrentPosePreview);
            }

            EditorGUI.EndDisabledGroup();
        }

        private void DrawQuickFingerSlider(string fingerId, string label)
        {
            if (!previewState.PoseEdits.PartEdits.TryGetValue(fingerId, out CCS_AnimationFitStudioPartEditState editState))
            {
                return;
            }

            float nextCurl = EditorGUILayout.Slider(label, editState.FingerCurl, 0f, 1f);
            if (!Mathf.Approximately(nextCurl, editState.FingerCurl))
            {
                editState.FingerCurl = nextCurl;
                MarkDirtyAndRefreshPreview();
            }
        }

        private void DrawHumanoidMuscleReadoutPanel()
        {
            if (!IsHumanoidClipMode())
            {
                return;
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Humanoid Muscle Values", EditorStyles.boldLabel);

            CCS_AnimationFitStudioHumanoidControlState control = previewState.HumanoidControl;
            if (!control.IsInitialized)
            {
                EditorGUILayout.HelpBox(
                    "Load preview to populate Humanoid muscle values from the controller FullDraw clip.",
                    MessageType.Info);
                return;
            }

            for (int i = 0; i < CCS_AnimationFitStudioHumanoidMuscleMapping.FullDrawSaveMuscleNames.Length; i++)
            {
                string muscleName = CCS_AnimationFitStudioHumanoidMuscleMapping.FullDrawSaveMuscleNames[i];
                if (!CCS_AnimationFitStudioHumanoidMuscleMapping.IsMuscleAvailableOnAvatar(muscleName))
                {
                    continue;
                }
                float current = control.GetCurrentValue(muscleName);
                float saved = control.GetLastSavedValue(muscleName);
                float delta = control.GetDeltaFromBaseline(muscleName);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(muscleName, EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField("Current: " + current.ToString("F4"), EditorStyles.miniLabel);
                EditorGUILayout.LabelField(
                    "Last saved: " + (float.IsNaN(saved) ? "(none)" : saved.ToString("F4")),
                    EditorStyles.miniLabel);
                EditorGUILayout.LabelField(
                    "Delta from loaded pose: " + delta.ToString("F4"),
                    EditorStyles.miniLabel);

                if (control.IsNearClamp(muscleName))
                {
                    EditorGUILayout.HelpBox(
                        "Warning: muscle is near Humanoid clamp limit. Further edits may not visibly move this part.",
                        MessageType.Warning);
                }

                EditorGUILayout.EndVertical();
            }

            if (!string.IsNullOrEmpty(control.LastEditFeedback))
            {
                EditorGUILayout.LabelField("Last edit: " + control.LastEditFeedback, EditorStyles.miniLabel);
            }

            if (control.LastClampedMuscles.Count > 0)
            {
                EditorGUILayout.HelpBox(
                    "Edit reached Humanoid muscle limit.\nClamped values: "
                    + string.Join(", ", control.LastClampedMuscles),
                    MessageType.Warning);
            }
        }

        private void DrawHumanoidAxisCalibrationControls()
        {
            if (!IsHumanoidClipMode())
            {
                return;
            }

            CCS_AnimationFitStudioHumanoidControlState control = previewState.HumanoidControl;
            EditorGUI.BeginChangeCheck();
            control.InvertShoulderPitch = EditorGUILayout.Toggle(
                "Invert Shoulder Pitch",
                control.InvertShoulderPitch);
            control.InvertShoulderYaw = EditorGUILayout.Toggle(
                "Invert Shoulder Yaw",
                control.InvertShoulderYaw);
            control.InvertUpperArmPitch = EditorGUILayout.Toggle(
                "Invert Upper Arm Pitch",
                control.InvertUpperArmPitch);
            control.InvertUpperArmYaw = EditorGUILayout.Toggle(
                "Invert Upper Arm Yaw",
                control.InvertUpperArmYaw);
            control.InvertWristPitch = EditorGUILayout.Toggle(
                "Invert Wrist Pitch",
                control.InvertWristPitch);
            control.InvertWristYaw = EditorGUILayout.Toggle(
                "Invert Wrist Yaw",
                control.InvertWristYaw);
            if (EditorGUI.EndChangeCheck())
            {
                Repaint();
            }
        }

        private void DrawFingerBoneStatus()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Finger Bones", EditorStyles.boldLabel);

            if (previewState.FingerDiscovery == null)
            {
                EditorGUILayout.LabelField("Finger Bones: Unknown", EditorStyles.miniLabel);
                return;
            }

            EditorGUILayout.LabelField(
                "Finger Bones: " + previewState.FingerDiscovery.GetSummaryLabel(),
                previewState.FingerBonesFound
                    ? CCS_EquipmentFitStudioStyles.StatusOkLabel
                    : CCS_EquipmentFitStudioStyles.StatusWarnLabel);

            for (int i = 0; i < previewState.FingerDiscovery.Chains.Count; i++)
            {
                CCS_AnimationFitStudioFingerChainDiscovery chain = previewState.FingerDiscovery.Chains[i];
                EditorGUILayout.LabelField(
                    chain.Label + ": " + chain.StatusLabel,
                    EditorStyles.miniLabel);
            }

            if (!previewState.FingerBonesFound && previewState.PreviewPlayer != null)
            {
                EditorGUILayout.HelpBox(
                    CCS_AnimationFitStudioFingerDiscoveryUtility.MissingFingerBonesWarning,
                    MessageType.Warning);
            }
        }

        private void DrawBottomActionBar()
        {
            EditorGUILayout.BeginVertical(
                CCS_EquipmentFitStudioStyles.BottomBarBackground,
                GUILayout.MinHeight(BottomActionBarHeight),
                GUILayout.ExpandWidth(true));

            EditorGUILayout.BeginHorizontal();

            if (DrawBottomButton("Load Preview / Weapon", true))
            {
                QueueGuiAction(LoadPreviewOrWeapon);
            }

            if (DrawBottomButton("Reset Pose", previewState.PreviewPlayer != null))
            {
                QueueGuiAction(ResetPose);
            }

            if (DrawBottomButton("Save Runtime FullDraw + Reimport", CanSaveFitTestPose()))
            {
                QueueGuiAction(() => SaveRuntimeFitTest(reimportAfterSave: true));
            }

            if (DrawBottomButton("Validate", true))
            {
                QueueGuiAction(RunValidation);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawAdvancedDiagnosticsFoldout()
        {
            showAdvancedDiagnostics = EditorGUILayout.Foldout(
                showAdvancedDiagnostics,
                "Advanced / Diagnostics",
                true);

            if (!showAdvancedDiagnostics)
            {
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Preview Source"))
                {
                    QueueGuiAction(() =>
                    {
                        previewingEditedClip = false;
                        PreviewClip(sourceClip);
                    });
                }

                if (GUILayout.Button("Preview Edited"))
                {
                    QueueGuiAction(PreviewEditedPose);
                }

                if (GUILayout.Button("Audition Aim Clips"))
                {
                    QueueGuiAction(() => RunAuditionAimClips(silent: false));
                }

                if (GUILayout.Button("Use Seed Pose"))
                {
                    QueueGuiAction(UseDefaultOneHandAimSeedPose);
                }

                if (GUILayout.Button("Re-resolve Clip"))
                {
                    QueueGuiAction(ReResolveClip);
                }

                if (GUILayout.Button("Apply Test Aim Offset"))
                {
                    QueueGuiAction(ApplyTestAimOffset);
                }
            }

            DrawAdvancedClipControls();
            DrawAxisCalibrationFoldout();
            DrawClipDiagnosticsPanel();
            DrawAuditionResultsPanel();

            EditorGUILayout.EndVertical();
        }

        private void DrawAxisCalibrationFoldout()
        {
            showAxisCalibration = EditorGUILayout.Foldout(
                showAxisCalibration,
                "Axis Calibration",
                true);

            if (!showAxisCalibration)
            {
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawHumanoidAxisCalibrationControls();

            if (TryGetSelectedEditPart(out CCS_AnimationFitStudioEditPartDefinition editPart)
                && IsSelectedPartFinger())
            {
                DrawFingerAxisControls(editPart.PartId);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawAdvancedClipControls()
        {
            if (!CCS_AnimationFitStudioPoseSourceCatalog.UsesClipSampling(selectedPoseSource))
            {
                EditorGUILayout.HelpBox(
                    "Clip pose time controls apply only to FullDraw or WalkAimed RH clip sources.",
                    MessageType.None);
                return;
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Clip Pose Time", EditorStyles.boldLabel);

            if (sourceClip != null)
            {
                EditorGUI.BeginChangeCheck();
                float nextPoseTime = EditorGUILayout.Slider("Custom Pose Time", poseTime, 0f, sourceClip.length);
                if (EditorGUI.EndChangeCheck())
                {
                    poseTime = nextPoseTime;
                    QueueGuiAction(RefreshPreview);
                }
            }
        }

        private void DrawClipDiagnosticsPanel()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Clip Diagnostics", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(clipDiagnostics.ToDisplayText(), MessageType.None);
        }

        private void DrawAuditionResultsPanel()
        {
            if (auditionResults.Count == 0)
            {
                return;
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Audition Results", EditorStyles.boldLabel);
            auditionScrollPosition = EditorGUILayout.BeginScrollView(
                auditionScrollPosition,
                GUILayout.MaxHeight(120f));
            for (int i = 0; i < auditionResults.Count; i++)
            {
                CCS_AnimationFitStudioClipAuditionRow row = auditionResults[i];
                EditorGUILayout.LabelField(
                    row.ClipName
                    + " | "
                    + row.TimeLabel
                    + " | Score "
                    + row.AimPoseScore.ToString("0.#")
                    + " | "
                    + row.Result
                    + " | Bones "
                    + row.ChangedBones,
                    EditorStyles.miniLabel);
            }

            EditorGUILayout.EndScrollView();
        }

        private static bool DrawBottomButton(string label, bool enabled)
        {
            return CCS_EquipmentFitStudioStyles.DrawBottomActionButton(
                label,
                CCS_EquipmentFitStudioButtonKind.Neutral,
                label,
                enabled,
                GUILayout.MinWidth(100f),
                GUILayout.ExpandWidth(true));
        }

        private void DrawStatusBar()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Save Target:", GetSaveTargetClipFileName());
            EditorGUILayout.LabelField("Source Clip:", GetSourceClipLabel());
            EditorGUILayout.LabelField("Controller FullDraw Clip:", GetFitTestClipLabel());
            EditorGUILayout.LabelField("Output Path:", GetOutputPathLabel());
            EditorGUILayout.LabelField(
                "Clip Used By Controller:",
                runtimeControllerClipInfo.ClipUsedByControllerLabel);
            EditorGUILayout.LabelField(
                "Controller Wiring:",
                CCS_AnimationFitStudioRuntimePolicy.SaveDoesNotWireControllerNotice);
            EditorGUILayout.LabelField(
                "Pose Target:",
                CCS_AnimationFitStudioPoseSourceCatalog.GetPoseSourceDisplayLabel(selectedPoseSource));
            EditorGUILayout.LabelField(
                "Pose Preview:",
                CCS_AnimationFitStudioPoseUtility.GetPosePreviewStatusLabel(previewState.PosePreviewStatus));
            EditorGUILayout.LabelField("Edited parts:", GetEditedPartsLabel());
            EditorGUILayout.LabelField("Dirty:", isDirty ? "Yes" : "No");

            if (!string.IsNullOrEmpty(statusMessage))
            {
                EditorGUILayout.HelpBox(statusMessage, statusType);
            }

            EditorGUILayout.EndVertical();
        }

        private string GetSourceClipLabel()
        {
            if (sourceClip == null)
            {
                return "(missing — resolve failed)";
            }

            return sourceClipDisplayName + "  (resolved)";
        }

        private string GetFitTestClipLabel()
        {
            if (string.IsNullOrEmpty(fitTestClipDisplayName) || fitTestClipDisplayName == "(not available)")
            {
                return "(not created)";
            }

            string loadedState = fitTestClip != null ? "loaded" : "not created";
            return fitTestClipDisplayName + "  (" + loadedState + ")";
        }

        private string GetOutputPathLabel()
        {
            return string.IsNullOrEmpty(fitTestOutputFolderPath)
                ? CCS_AnimationFitStudioClipResolver.GetFitTestOutputFolderPath()
                : fitTestOutputFolderPath;
        }
    }
}
