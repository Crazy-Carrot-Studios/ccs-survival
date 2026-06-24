using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioWindow (Active Fit Target partial)
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Always-visible active fit target selector and workflow session controls.
// PLACEMENT: Partial class extension of CCS_EquipmentFitStudioWindow.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Target selection precedes the 9-step workflow and never locks after save.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public sealed partial class CCS_EquipmentFitStudioWindow
    {
        private void DrawActiveFitTargetPanel()
        {
            DrawFitStudioModeSelector();
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Active Fit Target", CCS_EquipmentFitStudioStyles.SectionLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            DrawPreviewPlayerControls();

            state.PlayerRoot = (GameObject)EditorGUILayout.ObjectField(
                "Player",
                state.PlayerRoot,
                typeof(GameObject),
                true);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Find Player"))
            {
                TryAssignDefaultPlayer(forceRefresh: true);
            }

            EditorGUILayout.EndHorizontal();

            if (state.FitStudioMode == CCS_EquipmentFitStudioFitMode.PlayModeRuntimeTest)
            {
                EditorGUILayout.HelpBox(
                    "Runtime test mode uses the live spawned player. Pick up the revolver and hold RMB to verify saved profiles.",
                    MessageType.Info);
            }
            else if (state.FitStudioMode == CCS_EquipmentFitStudioFitMode.PlayModeAimFit)
            {
                EditorGUILayout.HelpBox(
                    CCS_EquipmentFitStudioEquippedPoseGuidanceUtility.PlayModeAimFitPurpose,
                    MessageType.Info);
            }

            EditorGUILayout.LabelField(
                "Weapon / Item",
                CCS_EquipmentFitStudioWorkflowSessionUtility.GetWeaponDisplayLabel(state.SelectedWeaponId));
            EditorGUILayout.LabelField("weaponId", state.SelectedWeaponId);

            DrawActiveSlotSelector();
            DrawActiveFitTargetSummary();
            DrawEquippedPoseTypeAndGuidance();
            if (CanUseEditFitTuning())
            {
                DrawPosePreviewControls();
            }
            else
            {
                EditorGUILayout.Space(6f);
                EditorGUILayout.LabelField("Pose Preview", CCS_EquipmentFitStudioStyles.SectionLabel);
                EditorGUILayout.HelpBox(
                    "Pose preview tuning uses Edit Fit Preview mode. Exit Play Mode or use the editor preview player.",
                    MessageType.Info);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4f);
            DrawRevolverQuickTargetButtons();
            EditorGUILayout.Space(4f);
            DrawTargetSessionControls();
            EditorGUILayout.Space(6f);
        }

        private void DrawActiveSlotSelector()
        {
            int socketIndex = GetSelectedSocketIndex();
            string previousSocketId = state.SelectedSocketId;
            socketIndex = EditorGUILayout.Popup(
                "Slot / Fit Target",
                socketIndex,
                CCS_EquipmentFitStudioRevolverFitUtility.GetSocketDropdownLabels());
            string newSocketId = CCS_EquipmentConstants.RequiredSocketIds[socketIndex];
            if (previousSocketId != newSocketId)
            {
                CCS_EquipmentFitStudioImGuiUtility.EnqueueDeferredAction(
                    () => TryChangeActiveSocket(newSocketId));
            }
        }

        private void DrawActiveFitTargetSummary()
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Current Profile:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(CCS_EquipmentFitStudioWorkflowSessionUtility.GetActiveProfileFileName(state.SelectedSocketId));
            EditorGUILayout.LabelField(
                "State: "
                + CCS_EquipmentFitStudioWorkflowSessionUtility.GetFitTargetStateLabel(GetActiveFitTargetState()),
                GetActiveFitTargetStateStyle());
        }

        private void DrawRevolverQuickTargetButtons()
        {
            EditorGUILayout.LabelField("Revolver Fit Targets", CCS_EquipmentFitStudioStyles.SectionLabel);
            EditorGUILayout.BeginHorizontal();
            DrawQuickTargetButton(
                "Right Hip Holster",
                CCS_EquipmentConstants.HolsterSocketRightHipId,
                state.SelectedSocketId == CCS_EquipmentConstants.HolsterSocketRightHipId);
            DrawQuickTargetButton(
                "Right Hand Equipped",
                CCS_EquipmentConstants.HandSocketRightId,
                state.SelectedSocketId == CCS_EquipmentConstants.HandSocketRightId);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            GUILayout.Button("Left Hip (not mapped for revolver)");
            GUILayout.Button("Back (not mapped for revolver)");
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawQuickTargetButton(string label, string socketId, bool isActive)
        {
            GUIStyle style = isActive
                ? CCS_EquipmentFitStudioStyles.WorkflowStepActive
                : EditorStyles.miniButton;
            if (GUILayout.Button(label, style, GUILayout.ExpandWidth(true)))
            {
                SelectRevolverQuickTarget(socketId);
            }
        }

        private void DrawTargetSessionControls()
        {
            EditorGUILayout.LabelField("Target Controls", CCS_EquipmentFitStudioStyles.SectionLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Start New Fit", GUILayout.ExpandWidth(true)))
            {
                StartNewFitSession();
            }

            if (GUILayout.Button("Edit Saved Fit", GUILayout.ExpandWidth(true)))
            {
                EditSavedFitSession();
            }

            if (GUILayout.Button("Change Slot / Item", GUILayout.ExpandWidth(true)))
            {
                ChangeSlotOrItemSession();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawActiveFitTargetHeaderBar()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Active Fit Target:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Weapon: "
                + CCS_EquipmentFitStudioWorkflowSessionUtility.GetWeaponDisplayLabel(state.SelectedWeaponId));
            EditorGUILayout.LabelField(
                "Slot: "
                + CCS_EquipmentFitStudioWorkflowSessionUtility.GetSlotDisplayLabel(state.SelectedSocketId));
            EditorGUILayout.LabelField("Socket: " + state.SelectedSocketId);
            EditorGUILayout.LabelField(
                "Profile: "
                + CCS_EquipmentFitStudioWorkflowSessionUtility.GetActiveProfileFileName(state.SelectedSocketId));
            EditorGUILayout.LabelField(
                "State: "
                + CCS_EquipmentFitStudioWorkflowSessionUtility.GetFitTargetStateLabel(GetActiveFitTargetState()));
            EditorGUILayout.LabelField(
                "Pose: "
                + CCS_EquipmentFitStudioPosePreviewUtility.GetPosePreviewStateLabel(state.PosePreviewMode));
            EditorGUILayout.LabelField(
                "Mode: "
                + GetFitStudioModeLabel(state.FitStudioMode));
            EditorGUILayout.EndVertical();
        }

        private static string GetFitStudioModeLabel(CCS_EquipmentFitStudioFitMode fitMode)
        {
            switch (fitMode)
            {
                case CCS_EquipmentFitStudioFitMode.PlayModeAimFit:
                    return "Play Mode Aim Fit";
                case CCS_EquipmentFitStudioFitMode.PlayModeRuntimeTest:
                    return "Play Mode Runtime Test";
                default:
                    return "Edit Fit Preview";
            }
        }

        private void DrawFitStudioModeSelector()
        {
            EditorGUILayout.LabelField("Fit Studio Mode", CCS_EquipmentFitStudioStyles.SectionLabel);
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Toggle(
                        state.FitStudioMode == CCS_EquipmentFitStudioFitMode.EditFitPreview,
                        "Edit Fit Preview",
                        EditorStyles.miniButton))
                {
                    SetFitStudioMode(CCS_EquipmentFitStudioFitMode.EditFitPreview);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.HelpBox(
                    "Use an editor-only preview player/mannequin for socket, pose, and profile tuning.",
                    MessageType.None);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Toggle(
                    state.FitStudioMode == CCS_EquipmentFitStudioFitMode.PlayModeRuntimeTest,
                    "Play Mode Runtime Test",
                    EditorStyles.miniButtonLeft))
            {
                SetFitStudioMode(CCS_EquipmentFitStudioFitMode.PlayModeRuntimeTest);
            }

            if (GUILayout.Toggle(
                    state.FitStudioMode == CCS_EquipmentFitStudioFitMode.PlayModeAimFit,
                    "Play Mode Aim Fit",
                    EditorStyles.miniButtonRight))
            {
                SetFitStudioMode(CCS_EquipmentFitStudioFitMode.PlayModeAimFit);
            }

            EditorGUILayout.EndHorizontal();

            string modePurpose = state.FitStudioMode == CCS_EquipmentFitStudioFitMode.PlayModeAimFit
                ? CCS_EquipmentFitStudioEquippedPoseGuidanceUtility.PlayModeAimFitPurpose
                : "Use the real spawned player only to verify pickup/aim visuals after profiles are saved.";
            EditorGUILayout.HelpBox(modePurpose, MessageType.None);
        }

        private void SetFitStudioMode(CCS_EquipmentFitStudioFitMode fitMode)
        {
            if (state.FitStudioMode == fitMode)
            {
                return;
            }

            if (fitMode != CCS_EquipmentFitStudioFitMode.PlayModeAimFit)
            {
                CCS_EquipmentFitStudioPlayModeAimFitUtility.CleanupEditorOverrides(state.PlayerRoot);
                state.ForceAimPoseActive = false;
            }

            state.FitStudioMode = fitMode;
            TryAssignDefaultPlayer(forceRefresh: true);
        }

        private void DrawPreviewPlayerControls()
        {
            EditorGUILayout.LabelField("Preview Player", CCS_EquipmentFitStudioStyles.SectionLabel);

            if (state.PlayerRoot == null
                && !EditorApplication.isPlaying
                && state.FitStudioMode == CCS_EquipmentFitStudioFitMode.EditFitPreview)
            {
                EditorGUILayout.HelpBox(
                    "No scene player found. Create an editor preview player to tune fits.",
                    MessageType.Info);
                if (GUILayout.Button("Create Preview Player"))
                {
                    CreateOrRefreshPreviewPlayerSession();
                }
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
            if (GUILayout.Button("Create / Refresh Preview Player", GUILayout.ExpandWidth(true)))
            {
                CreateOrRefreshPreviewPlayerSession();
            }

            if (GUILayout.Button("Use Selected Scene Player", GUILayout.ExpandWidth(true)))
            {
                UseSelectedScenePlayerSession();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);
            if (GUILayout.Button("Find Runtime Player", GUILayout.ExpandWidth(true)))
            {
                FindRuntimePlayerSession();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
            if (GUILayout.Button("Clear Preview Player", GUILayout.ExpandWidth(true)))
            {
                ClearPreviewPlayerSession();
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            if (state.UsesEditorPreviewPlayer && state.PlayerRoot != null)
            {
                EditorGUILayout.LabelField(
                    "Active target: editor preview player (temporary, not saved).",
                    EditorStyles.miniLabel);
            }
        }

        private void CreateOrRefreshPreviewPlayerSession()
        {
            ClearTargetSessionArtifacts(clearPending: false);
            if (!CCS_EquipmentFitStudioPreviewPlayerUtility.CreateOrRefreshPreviewPlayer(
                    out GameObject previewPlayer,
                    out string errorMessage))
            {
                SetStatus(errorMessage, MessageType.Error);
                Repaint();
                return;
            }

            state.PlayerRoot = previewPlayer;
            state.UsesEditorPreviewPlayer = true;
            state.FitStudioMode = CCS_EquipmentFitStudioFitMode.EditFitPreview;
            state.UserManuallySelectedPosePreview = false;
            LoadRevolverProfileSelections();
            ApplyDefaultPosePreviewForActiveTarget();
            ResetWorkflowToSpawnPreview();
            SetStatus("Editor preview player ready for fit tuning.", MessageType.Info);
            Repaint();
            SceneView.RepaintAll();
        }

        private void UseSelectedScenePlayerSession()
        {
            if (!CCS_EquipmentFitStudioPreviewPlayerUtility.TryUseSelectedScenePlayer(
                    out GameObject playerRoot,
                    out string errorMessage))
            {
                SetStatus(errorMessage, MessageType.Error);
                Repaint();
                return;
            }

            state.PlayerRoot = playerRoot;
            state.UsesEditorPreviewPlayer =
                CCS_EquipmentFitStudioPreviewPlayerUtility.IsPreviewPlayer(playerRoot);
            state.FitStudioMode = CCS_EquipmentFitStudioFitMode.EditFitPreview;
            SetStatus("Using selected scene player for fit tuning.", MessageType.Info);
            Repaint();
        }

        private void FindRuntimePlayerSession()
        {
            if (!CCS_EquipmentFitStudioPreviewPlayerUtility.TryFindRuntimePlayer(
                    out GameObject runtimePlayer,
                    out string errorMessage))
            {
                SetStatus(errorMessage, MessageType.Error);
                Repaint();
                return;
            }

            state.PlayerRoot = runtimePlayer;
            state.UsesEditorPreviewPlayer = false;
            state.FitStudioMode = CCS_EquipmentFitStudioFitMode.PlayModeRuntimeTest;
            SetStatus("Using live spawned player for runtime verification.", MessageType.Info);
            Repaint();
        }

        private void ClearPreviewPlayerSession()
        {
            ClearPosePreviewSession();
            ClearTargetSessionArtifacts(clearPending: false);
            CCS_EquipmentFitStudioPreviewPlayerUtility.ClearPreviewPlayer();
            if (state.UsesEditorPreviewPlayer)
            {
                state.PlayerRoot = null;
            }

            state.UsesEditorPreviewPlayer = false;
            SetStatus("Editor preview player cleared.", MessageType.Info);
            Repaint();
            SceneView.RepaintAll();
        }

        private GUIStyle GetActiveFitTargetStateStyle()
        {
            switch (GetActiveFitTargetState())
            {
                case CCS_EquipmentFitStudioFitTargetState.Unsaved:
                    return CCS_EquipmentFitStudioStyles.StatusWarnLabel;
                case CCS_EquipmentFitStudioFitTargetState.Saved:
                    return CCS_EquipmentFitStudioStyles.StatusOkLabel;
                case CCS_EquipmentFitStudioFitTargetState.Testing:
                    return CCS_EquipmentFitStudioStyles.StatusWarnLabel;
                default:
                    return EditorStyles.label;
            }
        }

        private CCS_EquipmentFitStudioFitTargetState GetActiveFitTargetState()
        {
            return CCS_EquipmentFitStudioWorkflowSessionUtility.ResolveFitTargetState(
                state.HasPendingSaveCapture,
                state.JustSavedProfileThisSession,
                previewItem.IsSpawned,
                HasTestAttachments(),
                state.SelectedAttachmentFitProfile);
        }

        private bool TryChangeActiveSocket(string newSocketId, bool skipPendingConfirmation = false)
        {
            if (string.IsNullOrEmpty(newSocketId) || newSocketId == state.SelectedSocketId)
            {
                return false;
            }

            if (state.HasPendingSaveCapture && !skipPendingConfirmation)
            {
                bool discard = EditorUtility.DisplayDialog(
                    "Change Fit Target",
                    "You have captured values that are not saved. Change target and discard pending capture?",
                    "Change Target",
                    "Cancel");
                if (!discard)
                {
                    return false;
                }
            }

            ClearTargetSessionArtifacts(clearPending: true);
            state.SelectedSocketId = newSocketId;
            state.SelectedWeaponId = CCS_EquipmentConstants.RevolverM1879WeaponId;
            state.EquippedPoseType =
                CCS_EquipmentFitStudioEquippedPoseGuidanceUtility.GetDefaultPoseTypeForWeapon(
                    state.SelectedWeaponId);
            state.UserManuallySelectedPosePreview = false;
            LoadRevolverProfileSelections();
            ApplyDefaultPosePreviewForActiveTarget();
            ResetWorkflowToSpawnPreview();
            SetStatus(
                "Active target: "
                + CCS_EquipmentFitStudioWorkflowSessionUtility.GetSlotDisplayLabel(newSocketId),
                MessageType.Info);
            Repaint();
            return true;
        }

        private void SelectRevolverQuickTarget(string socketId)
        {
            state.SelectedWeaponId = CCS_EquipmentConstants.RevolverM1879WeaponId;
            if (!TryChangeActiveSocket(socketId))
            {
                if (state.SelectedSocketId == socketId)
                {
                    ClearTargetSessionArtifacts(clearPending: true);
                    state.UserManuallySelectedPosePreview = false;
                    LoadRevolverProfileSelections();
                    ApplyDefaultPosePreviewForActiveTarget();
                    ResetWorkflowToSpawnPreview();
                    Repaint();
                }
            }
        }

        private void StartNewFitSession()
        {
            ClearTargetSessionArtifacts(clearPending: true);
            LoadRevolverProfileSelections();
            ApplyDefaultPosePreviewForActiveTarget();
            ResetWorkflowToSpawnPreview();
            SetStatus("Started new fit session for the active target.", MessageType.Info);
            Repaint();
        }

        private void EditSavedFitSession()
        {
            ClearTargetSessionArtifacts(clearPending: false);
            state.ClearWorkflowSessionFlags();
            LoadRevolverProfileSelections();
            ApplyDefaultPosePreviewForActiveTarget();
            ApplyRevolverAttachmentFitProfile(GetSelectedSocketTransform());
            if (!previewItem.IsSpawned)
            {
                if (EditorUtility.DisplayDialog(
                        "Edit Saved Fit",
                        "Spawn preview now to edit the saved fit on the player?",
                        "Spawn Preview",
                        "Skip For Now"))
                {
                    SpawnPreviewItem();
                }
                else
                {
                    state.WorkflowStepOverride = CCS_EquipmentFitStudioWorkflowStep.SpawnPreview;
                }
            }
            else
            {
                previewItem.EnforceZeroedTransform();
                state.WorkflowStepOverride = CCS_EquipmentFitStudioWorkflowStep.TuneSocket;
            }

            SetStatus("Loaded saved fit for editing.", MessageType.Info);
            Repaint();
        }

        private void ChangeSlotOrItemSession()
        {
            ClearTargetSessionArtifacts(clearPending: true);
            state.WorkflowStepOverride = CCS_EquipmentFitStudioWorkflowStep.SelectSocket;
            SetStatus("Select another weapon slot or fit target above.", MessageType.Info);
            Repaint();
        }

        private void TuneAnotherSlotSession()
        {
            ChangeSlotOrItemSession();
        }

        private void ClearTargetSessionArtifacts(bool clearPending)
        {
            ClearPreviewAndWeights();
            CCS_EquipmentFitStudioTestAttachmentUtility.ClearTestAttachments(state.PlayerRoot);
            if (clearPending)
            {
                state.ClearPendingChanges();
            }

            state.JustSavedProfileThisSession = false;
            state.SavedProfileThisSession = false;
            state.WorkflowStepOverride = null;
            state.LastSaveConfirmationMessage = string.Empty;
        }

        private void DrawPosePreviewControls()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Pose Preview", CCS_EquipmentFitStudioStyles.SectionLabel);
            EditorGUILayout.HelpBox(
                CCS_EquipmentFitStudioPosePreviewUtility.GetPosePreviewHint(state.SelectedSocketId),
                MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            if (DrawPosePreviewButton(
                    "Neutral",
                    CCS_EquipmentFitStudioPosePreviewMode.Neutral,
                    state.PosePreviewMode == CCS_EquipmentFitStudioPosePreviewMode.Neutral))
            {
                SetPosePreview(CCS_EquipmentFitStudioPosePreviewMode.Neutral, userSelected: true);
            }

            if (DrawPosePreviewButton(
                    "Revolver Aim",
                    CCS_EquipmentFitStudioPosePreviewMode.RevolverAim,
                    state.PosePreviewMode == CCS_EquipmentFitStudioPosePreviewMode.RevolverAim))
            {
                SetPosePreview(CCS_EquipmentFitStudioPosePreviewMode.RevolverAim, userSelected: true);
            }

            EditorGUI.BeginDisabledGroup(!CCS_EquipmentFitStudioPosePreviewUtility.IsFireFramePreviewEnabled);
            if (DrawPosePreviewButton(
                    "Revolver Fire Frame",
                    CCS_EquipmentFitStudioPosePreviewMode.RevolverFireFrame,
                    state.PosePreviewMode == CCS_EquipmentFitStudioPosePreviewMode.RevolverFireFrame))
            {
                SetPosePreview(CCS_EquipmentFitStudioPosePreviewMode.RevolverFireFrame, userSelected: true);
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            if (!CCS_EquipmentFitStudioPosePreviewUtility.IsFireFramePreviewEnabled)
            {
                EditorGUILayout.LabelField(
                    CCS_EquipmentFitStudioPosePreviewUtility.FireFramePreviewDisabledTooltip,
                    EditorStyles.miniLabel);
            }

            if (GUILayout.Button("Clear Pose Preview"))
            {
                ClearPosePreviewSession();
            }

            EditorGUILayout.LabelField(
                "Current Pose: "
                + CCS_EquipmentFitStudioPosePreviewUtility.GetPosePreviewStateLabel(state.PosePreviewMode));
            if (!string.IsNullOrEmpty(state.LastPosePreviewError))
            {
                EditorGUILayout.LabelField(state.LastPosePreviewError, CCS_EquipmentFitStudioStyles.StatusErrorLabel);
            }
        }

        private bool DrawPosePreviewButton(
            string label,
            CCS_EquipmentFitStudioPosePreviewMode mode,
            bool isActive)
        {
            GUIStyle style = isActive
                ? CCS_EquipmentFitStudioStyles.WorkflowStepActive
                : EditorStyles.miniButton;
            return GUILayout.Button(label, style, GUILayout.ExpandWidth(true));
        }

        private void SetPosePreview(CCS_EquipmentFitStudioPosePreviewMode mode, bool userSelected)
        {
            if (!CanUseEditFitTuning())
            {
                state.LastPosePreviewError =
                    "Pose preview tuning uses Edit Fit Preview mode. Exit Play Mode or use the editor preview player.";
                SetStatus(state.LastPosePreviewError, MessageType.Warning);
                Repaint();
                return;
            }

            if (userSelected)
            {
                state.UserManuallySelectedPosePreview = true;
            }

            if (CCS_EquipmentFitStudioPosePreviewUtility.TryApplyPosePreview(
                    state.PlayerRoot,
                    mode,
                    out string errorMessage))
            {
                state.PosePreviewMode = mode;
                state.LastPosePreviewError = string.Empty;
                SetStatus(
                    "Pose preview: "
                    + CCS_EquipmentFitStudioPosePreviewUtility.GetPosePreviewStateLabel(mode),
                    MessageType.Info);
            }
            else
            {
                state.LastPosePreviewError = errorMessage;
                SetStatus(errorMessage, MessageType.Warning);
            }

            Repaint();
            SceneView.RepaintAll();
        }

        private void ApplyDefaultPosePreviewForActiveTarget()
        {
            if (!CanUseEditFitTuning())
            {
                return;
            }

            if (state.UserManuallySelectedPosePreview)
            {
                return;
            }

            CCS_EquipmentFitStudioPosePreviewMode defaultMode =
                CCS_EquipmentFitStudioPosePreviewUtility.GetDefaultPosePreviewForSocket(state.SelectedSocketId);
            SetPosePreview(defaultMode, userSelected: false);
        }

        private void ClearPosePreviewSession()
        {
            CCS_EquipmentFitStudioPosePreviewUtility.ClearPosePreview(state.PlayerRoot);
            state.PosePreviewMode = CCS_EquipmentFitStudioPosePreviewMode.Neutral;
            state.UserManuallySelectedPosePreview = false;
            state.LastPosePreviewError = string.Empty;
            SetStatus("Pose preview cleared.", MessageType.Info);
            Repaint();
            SceneView.RepaintAll();
        }

        private void ResetWorkflowToSpawnPreview()
        {
            state.JustSavedProfileThisSession = false;
            state.SavedProfileThisSession = false;
            state.WorkflowStepOverride = CCS_EquipmentFitStudioWorkflowStep.SpawnPreview;
        }

        private void TestSavedActiveTargetFit()
        {
            if (state.SelectedSocketId == CCS_EquipmentConstants.HandSocketRightId)
            {
                TestSavedEquippedFit();
                return;
            }

            TestSavedHolsterFit();
        }

        private string GetActiveTargetPrimaryTestButtonLabel()
        {
            if (state.SelectedSocketId == CCS_EquipmentConstants.HandSocketRightId)
            {
                return "Test Saved Right Hand Equipped Fit";
            }

            return "Test Saved Right Hip Holster Fit";
        }

        private Transform GetActiveTargetRuntimeAttachmentRoot()
        {
            string attachmentRootName =
                CCS_EquipmentFitStudioWorkflowSessionUtility.GetRuntimeAttachmentRootName(state.SelectedSocketId);
            if (string.IsNullOrEmpty(attachmentRootName))
            {
                return null;
            }

            CCS_EquipmentFitStudioTestAttachmentUtility.TryGetRuntimeAttachmentRoot(
                state.PlayerRoot,
                state.SelectedSocketId,
                attachmentRootName,
                out Transform attachmentRoot);
            return attachmentRoot;
        }

        private string GetSlotSpecificSaveStatusMessage()
        {
            string profileName = CCS_EquipmentFitStudioWorkflowSessionUtility.GetActiveProfileFileName(state.SelectedSocketId);
            if (state.SelectedSocketId == CCS_EquipmentConstants.HandSocketRightId)
            {
                return "Saved Right Hand Equipped fit profile.\nProfile: " + profileName;
            }

            return "Saved Right Hip Holster fit profile.\nProfile: " + profileName;
        }
    }
}
