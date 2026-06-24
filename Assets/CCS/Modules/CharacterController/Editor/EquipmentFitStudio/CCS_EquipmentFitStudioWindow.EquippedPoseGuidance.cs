using UnityEditor;
using UnityEngine;
using CCS.Modules.Weapons;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioWindow (Equipped Pose Guidance partial)
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Equipped pose type readout, deferred IK notes, Play Mode Aim Fit controls.
// PLACEMENT: Partial class extension of CCS_EquipmentFitStudioWindow.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.8 one-hand revolver default; two-hand preview is future/experimental.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public sealed partial class CCS_EquipmentFitStudioWindow
    {
        private void DrawEquippedPoseTypeAndGuidance()
        {
            if (state.SelectedSocketId != CCS_EquipmentConstants.HandSocketRightId)
            {
                return;
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("RIGHT HAND EQUIPPED FIT", CCS_EquipmentFitStudioStyles.SectionLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Equipped Pose Type", EditorStyles.boldLabel);
            int poseTypeIndex = (int)state.EquippedPoseType;
            string[] poseTypeLabels =
            {
                "One-Hand Revolver",
                "Two-Hand Weapon Preview (future/experimental)",
            };
            EditorGUI.BeginChangeCheck();
            poseTypeIndex = EditorGUILayout.Popup("Equipped Pose Type", poseTypeIndex, poseTypeLabels);
            if (EditorGUI.EndChangeCheck())
            {
                state.EquippedPoseType = (CCS_EquipmentFitStudioEquippedPoseType)poseTypeIndex;
            }

            if (state.EquippedPoseType == CCS_EquipmentFitStudioEquippedPoseType.OneHandRevolver)
            {
                EditorGUILayout.HelpBox(
                    CCS_EquipmentFitStudioEquippedPoseGuidanceUtility.OneHandRightHandGuidance,
                    MessageType.Info);
                EditorGUILayout.HelpBox(
                    CCS_EquipmentFitStudioEquippedPoseGuidanceUtility.TwoHandedSourcePoseWarning,
                    MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    CCS_EquipmentFitStudioEquippedPoseGuidanceUtility.TwoHandPreviewGuidance,
                    MessageType.Info);
            }

            EditorGUILayout.HelpBox(
                CCS_EquipmentFitStudioEquippedPoseGuidanceUtility.FingerIkDeferredNote,
                MessageType.None);

            DrawRightHandFitStepsPanel();
            DrawPlayModeAimFitControls();
            DrawIkDiagnosticsPanel();

            EditorGUILayout.EndVertical();
        }

        private void DrawRightHandFitStepsPanel()
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Fit Steps (v0.6.8)", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("A. Weapon Fit — use first", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("B. Hand / Finger Pose — later polish (deferred)", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("C. IK — optional arm correction after weapon placement", EditorStyles.miniLabel);
        }

        private void DrawPlayModeAimFitControls()
        {
            if (state.FitStudioMode != CCS_EquipmentFitStudioFitMode.PlayModeAimFit)
            {
                if (EditorApplication.isPlaying)
                {
                    EditorGUILayout.Space(4f);
                    EditorGUILayout.HelpBox(
                        "Select Play Mode Aim Fit to tune Right Hand Equipped on the live player.",
                        MessageType.Info);
                }
                else
                {
                    EditorGUILayout.Space(4f);
                    EditorGUILayout.HelpBox(
                        "Play Mode Aim Fit requires Play Mode because it uses the real runtime animator and equipped visual bridge.",
                        MessageType.Info);
                }

                return;
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Play Mode Aim Fit", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                CCS_EquipmentFitStudioEquippedPoseGuidanceUtility.PlayModeAimFitPurpose,
                MessageType.None);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Find Runtime Player", GUILayout.ExpandWidth(true)))
            {
                FindRuntimePlayerSession();
            }

            if (GUILayout.Button("Force Aim Pose ON", GUILayout.ExpandWidth(true)))
            {
                if (CCS_EquipmentFitStudioPlayModeAimFitUtility.ForceAimPoseOn(
                        state.PlayerRoot,
                        out string errorMessage))
                {
                    state.ForceAimPoseActive = true;
                    SetStatus("Forced aim pose ON for fit tuning.", MessageType.Info);
                }
                else
                {
                    SetStatus(errorMessage, MessageType.Error);
                }
            }

            if (GUILayout.Button("Force Aim Pose OFF", GUILayout.ExpandWidth(true)))
            {
                CCS_EquipmentFitStudioPlayModeAimFitUtility.ForceAimPoseOff(state.PlayerRoot);
                state.ForceAimPoseActive = false;
                SetStatus("Forced aim pose OFF. Normal input resumed.", MessageType.Info);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Show Equipped Visual", GUILayout.ExpandWidth(true)))
            {
                if (CCS_EquipmentFitStudioPlayModeAimFitUtility.ShowEquippedVisual(
                        state.PlayerRoot,
                        out string errorMessage))
                {
                    SetStatus("Equipped visual shown for fit tuning.", MessageType.Info);
                }
                else
                {
                    SetStatus(errorMessage, MessageType.Error);
                }
            }

            if (GUILayout.Button("Hide Equipped Visual", GUILayout.ExpandWidth(true)))
            {
                CCS_EquipmentFitStudioPlayModeAimFitUtility.HideEquippedVisual(state.PlayerRoot);
                SetStatus("Equipped visual hidden.", MessageType.Info);
            }

            if (GUILayout.Button("Reload / Reapply Saved", GUILayout.ExpandWidth(true)))
            {
                ReloadAndReapplySavedProfile();
            }

            EditorGUILayout.EndHorizontal();

            DrawPlayModeWeaponFitTuner();
        }

        private void DrawPlayModeWeaponFitTuner()
        {
            if (!CanUsePlayModeAimFit())
            {
                return;
            }

            Transform attachmentRoot =
                CCS_EquipmentFitStudioPlayModeAimFitUtility.GetRuntimeEquippedAttachmentRoot(state.PlayerRoot);
            if (attachmentRoot == null)
            {
                EditorGUILayout.HelpBox(
                    "Show Equipped Visual to create the runtime attachment root before tuning.",
                    MessageType.Warning);
                return;
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Weapon Attachment Root (runtime)", EditorStyles.boldLabel);
            Vector3 position = attachmentRoot.localPosition;
            Vector3 euler = attachmentRoot.localEulerAngles;
            Vector3 scale = attachmentRoot.localScale;
            position = EditorGUILayout.Vector3Field("Local Position", position);
            euler = EditorGUILayout.Vector3Field("Local Rotation", euler);
            scale = EditorGUILayout.Vector3Field("Local Scale", scale);
            attachmentRoot.localPosition = position;
            attachmentRoot.localRotation = Quaternion.Euler(euler);
            attachmentRoot.localScale = scale;
            DrawNudgeControls(settings, ref position, ref euler, ref scale, attachmentRoot);
        }

        private void DrawIkDiagnosticsPanel()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("IK Status (diagnostic only)", EditorStyles.boldLabel);
            CCS_EquipmentFitStudioIkDiagnosticsSnapshot snapshot =
                CCS_EquipmentFitStudioIkDiagnosticsUtility.CaptureSnapshot(state.PlayerRoot);

            EditorGUILayout.LabelField("RigBuilder: " + (snapshot.RigBuilderFound ? "Found" : "Missing"));
            EditorGUILayout.LabelField("Rig: " + (snapshot.RigFound ? "Found" : "Missing"));
            EditorGUILayout.LabelField(
                "Right Hand IK Constraint: "
                + (snapshot.RightHandConstraintFound ? "Found" : "Missing"));
            EditorGUILayout.LabelField("Target: " + (snapshot.TargetFound ? "Found" : "Missing"));
            EditorGUILayout.LabelField("Hint: " + (snapshot.HintFound ? "Found" : "Missing"));
            EditorGUILayout.LabelField("Rig Weight: " + snapshot.RigWeight.ToString("F2"));
            EditorGUILayout.LabelField("Constraint Weight: " + snapshot.ConstraintWeight.ToString("F2"));
            EditorGUILayout.LabelField("Target Position Weight: " + snapshot.TargetPositionWeight.ToString("F2"));
            EditorGUILayout.LabelField("Target Rotation Weight: " + snapshot.TargetRotationWeight.ToString("F2"));
            EditorGUILayout.LabelField("Hint Weight: " + snapshot.HintWeight.ToString("F2"));

            if (!snapshot.IsPreviewEnabled)
            {
                EditorGUILayout.HelpBox(
                    CCS_EquipmentFitStudioEquippedPoseGuidanceUtility.IkOffNote,
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    CCS_EquipmentFitStudioEquippedPoseGuidanceUtility.IkPreviewTemporaryWarning,
                    MessageType.Warning);
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(state.PlayerRoot == null);
            if (GUILayout.Button("Enable IK Preview", GUILayout.ExpandWidth(true)))
            {
                CCS_EquipmentFitStudioIkDiagnosticsUtility.EnableIkPreview(state.PlayerRoot);
                SetStatus("IK preview enabled temporarily.", MessageType.Info);
            }

            if (GUILayout.Button("Reset IK Preview To 0", GUILayout.ExpandWidth(true)))
            {
                CCS_EquipmentFitStudioIkDiagnosticsUtility.ResetIkPreviewToZero(state.PlayerRoot);
                ResetIkPreviewWeights();
                SetStatus("IK preview weights reset to 0.", MessageType.Info);
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private void ReloadAndReapplySavedProfile()
        {
            if (state.PlayerRoot == null)
            {
                SetStatus("Select a runtime player first.", MessageType.Error);
                return;
            }

            LoadRevolverProfileSelections();
            ApplyRevolverAttachmentFitProfile(GetSelectedSocketTransform());
            CCS_PlayerEquipmentVisualController visualController =
                state.PlayerRoot.GetComponent<CCS_PlayerEquipmentVisualController>();
            if (visualController != null)
            {
                visualController.RefreshVisualState();
            }

            SetStatus("Reloaded and reapplied saved profile to runtime equipped visual.", MessageType.Info);
        }
    }
}
