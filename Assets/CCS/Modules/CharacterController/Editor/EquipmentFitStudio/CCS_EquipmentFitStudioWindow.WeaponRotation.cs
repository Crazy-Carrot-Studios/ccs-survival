using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioWindow.WeaponRotation
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Weapon-space pitch/yaw/roll controls and axis diagnostics for Fit Studio.
// PLACEMENT: Partial class extension for Equipment Fit Studio window.
// AUTHOR: James Schilz
// CREATED: 2026-06-24
// NOTES: Profile transforms apply to preview attachment root; visual child stays zeroed.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public sealed partial class CCS_EquipmentFitStudioWindow
    {
        #region Variables

        private Quaternion pendingLocalRotation = Quaternion.identity;

        private Vector3 pendingDisplayEuler = Vector3.zero;

        private Vector3 pendingLocalPosition = Vector3.zero;

        private Vector3 pendingLocalScale = Vector3.one;

        private bool pendingRotationInitialized;

        private CCS_EquipmentFitStudioWeaponForwardAxis weaponForwardAxis =
            CCS_EquipmentFitStudioWeaponForwardAxis.LocalNegativeZ;

        private string lastAxisAction = string.Empty;

        private bool rollAxisWarning;

        private bool showAxisDiagnostics;

        private bool showSocketLocalAxes;

        private bool showWeaponLocalAxes;

        private bool showWeaponForwardAxis;

        private Quaternion axisTestBaselineRotation = Quaternion.identity;

        private bool axisTestActive;

        private int weaponRotationStepIndex = 1;

        private static readonly string[] WeaponRotationStepLabels = { "1° (Small)", "5° (Medium)", "15° (Large)" };

        private static readonly float[] WeaponRotationStepValues =
        {
            CCS_EquipmentFitStudioWeaponRotationUtility.SmallNudgeDegrees,
            CCS_EquipmentFitStudioWeaponRotationUtility.MediumNudgeDegrees,
            CCS_EquipmentFitStudioWeaponRotationUtility.LargeNudgeDegrees,
        };

        #endregion

        #region Private Methods

        private Transform GetPreviewAttachmentRootTransform()
        {
            string attachmentRootName =
                CCS_EquipmentFitStudioPreviewAttachmentUtility.GetAttachmentRootObjectName(state.FitTarget);
            return CCS_EquipmentFitStudioPreviewAttachmentUtility.FindPreviewAttachmentRoot(
                state.PlayerRoot,
                state.SelectedSocketId,
                attachmentRootName);
        }

        private void SyncPendingFromAttachmentRoot(Transform attachmentRoot)
        {
            if (attachmentRoot == null)
            {
                pendingRotationInitialized = false;
                return;
            }

            pendingLocalPosition = attachmentRoot.localPosition;
            pendingLocalScale = attachmentRoot.localScale;
            pendingLocalRotation = attachmentRoot.localRotation;
            pendingDisplayEuler =
                CCS_EquipmentFitStudioWeaponRotationUtility.NormalizeEuler(pendingLocalRotation.eulerAngles);
            pendingRotationInitialized = true;
            rollAxisWarning = !CCS_EquipmentFitStudioWeaponRotationUtility.RollAndYawProduceDistinctRotations(
                weaponForwardAxis);
        }

        private void ApplyPendingToAttachmentRoot(Transform attachmentRoot)
        {
            if (attachmentRoot == null)
            {
                return;
            }

            attachmentRoot.localPosition = pendingLocalPosition;
            attachmentRoot.localScale = pendingLocalScale;
            attachmentRoot.localRotation = pendingLocalRotation;
            previewItem.EnforceZeroedTransform();
        }

        private void DrawWeaponRotationTransformPanel(Transform attachmentRoot)
        {
            if (attachmentRoot == null)
            {
                EditorGUILayout.HelpBox("Preview attachment root not found. Load preview first.", MessageType.Warning);
                return;
            }

            if (!pendingRotationInitialized)
            {
                SyncPendingFromAttachmentRoot(attachmentRoot);
            }

            EditorGUI.BeginChangeCheck();
            pendingLocalPosition = EditorGUILayout.Vector3Field("Local Position", pendingLocalPosition);
            pendingDisplayEuler = EditorGUILayout.Vector3Field("Profile Euler (Display)", pendingDisplayEuler);
            pendingLocalScale = EditorGUILayout.Vector3Field("Local Scale", pendingLocalScale);
            if (EditorGUI.EndChangeCheck())
            {
                pendingLocalRotation = Quaternion.Euler(pendingDisplayEuler);
                ApplyPendingToAttachmentRoot(attachmentRoot);
            }

            DrawPositionNudgeControls(attachmentRoot);
            DrawWeaponRotationControlsSection(attachmentRoot);
            DrawRawEulerNudgeControls(attachmentRoot);
            DrawAxisVisualizationToggles();
            DrawAxisDiagnosticPanel(attachmentRoot);
            DrawAxisTestButtons(attachmentRoot);
        }

        private void DrawPositionNudgeControls(Transform attachmentRoot)
        {
            EditorGUILayout.Space(4f);
            state.NudgeStepIndex = EditorGUILayout.Popup(
                "Step Size",
                state.NudgeStepIndex,
                NudgeStepLabels);
            float step = NudgeStepValues[Mathf.Clamp(state.NudgeStepIndex, 0, NudgeStepValues.Length - 1)];

            EditorGUILayout.LabelField("Position Nudge", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+X")) NudgePendingPosition(attachmentRoot, Vector3.right, step);
            if (GUILayout.Button("-X")) NudgePendingPosition(attachmentRoot, Vector3.right, -step);
            if (GUILayout.Button("+Y")) NudgePendingPosition(attachmentRoot, Vector3.up, step);
            if (GUILayout.Button("-Y")) NudgePendingPosition(attachmentRoot, Vector3.up, -step);
            if (GUILayout.Button("+Z")) NudgePendingPosition(attachmentRoot, Vector3.forward, step);
            if (GUILayout.Button("-Z")) NudgePendingPosition(attachmentRoot, Vector3.forward, -step);
            EditorGUILayout.EndHorizontal();
        }

        private void NudgePendingPosition(Transform attachmentRoot, Vector3 axis, float delta)
        {
            pendingLocalPosition += axis * delta;
            ApplyPendingToAttachmentRoot(attachmentRoot);
        }

        private void DrawWeaponRotationControlsSection(Transform attachmentRoot)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Weapon Rotation Controls", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Pitch = muzzle up/down. Yaw = muzzle left/right. Roll / Side Tilt = rotate around weapon forward axis.",
                MessageType.None);

            weaponForwardAxis = (CCS_EquipmentFitStudioWeaponForwardAxis)EditorGUILayout.Popup(
                "Weapon Forward Axis",
                (int)weaponForwardAxis,
                CCS_EquipmentFitStudioWeaponRotationUtility.WeaponForwardAxisLabels);

            weaponRotationStepIndex = EditorGUILayout.Popup(
                "Rotation Step",
                weaponRotationStepIndex,
                WeaponRotationStepLabels);
            float step = WeaponRotationStepValues[
                Mathf.Clamp(weaponRotationStepIndex, 0, WeaponRotationStepValues.Length - 1)];

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Pitch +")) ApplyWeaponAxisNudge(attachmentRoot, "Pitch", step, true);
            if (GUILayout.Button("Pitch -")) ApplyWeaponAxisNudge(attachmentRoot, "Pitch", -step, true);
            if (GUILayout.Button("Yaw +")) ApplyWeaponAxisNudge(attachmentRoot, "Yaw", step, false);
            if (GUILayout.Button("Yaw -")) ApplyWeaponAxisNudge(attachmentRoot, "Yaw", -step, false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Roll / Side Tilt +")) ApplyWeaponAxisNudge(attachmentRoot, "Roll", step, false, true);
            if (GUILayout.Button("Roll / Side Tilt -")) ApplyWeaponAxisNudge(attachmentRoot, "Roll", -step, false, true);
            EditorGUILayout.EndHorizontal();

            if (rollAxisWarning)
            {
                EditorGUILayout.HelpBox(
                    "Roll axis appears aligned with yaw. Check Weapon Forward Axis setting or model local axes.",
                    MessageType.Warning);
            }
        }

        private void ApplyWeaponAxisNudge(
            Transform attachmentRoot,
            string axisLabel,
            float degrees,
            bool isPitch,
            bool isRoll = false)
        {
            if (isPitch)
            {
                pendingLocalRotation = CCS_EquipmentFitStudioWeaponRotationUtility.ApplyPitchDelta(
                    pendingLocalRotation,
                    degrees);
            }
            else if (isRoll)
            {
                pendingLocalRotation = CCS_EquipmentFitStudioWeaponRotationUtility.ApplyRollDelta(
                    pendingLocalRotation,
                    degrees,
                    weaponForwardAxis);
            }
            else
            {
                pendingLocalRotation = CCS_EquipmentFitStudioWeaponRotationUtility.ApplyYawDelta(
                    pendingLocalRotation,
                    degrees);
            }

            pendingDisplayEuler =
                CCS_EquipmentFitStudioWeaponRotationUtility.NormalizeEuler(pendingLocalRotation.eulerAngles);
            lastAxisAction = axisLabel + (degrees >= 0f ? " +" : " ") + degrees.ToString("0.#");
            rollAxisWarning = !CCS_EquipmentFitStudioWeaponRotationUtility.RollAndYawProduceDistinctRotations(
                weaponForwardAxis);
            ApplyPendingToAttachmentRoot(attachmentRoot);
        }

        private void DrawRawEulerNudgeControls(Transform attachmentRoot)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Raw Euler Nudge (Advanced)", EditorStyles.miniBoldLabel);
            float step = WeaponRotationStepValues[
                Mathf.Clamp(weaponRotationStepIndex, 0, WeaponRotationStepValues.Length - 1)];

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Euler X +")) NudgeDisplayEuler(attachmentRoot, 0, step);
            if (GUILayout.Button("Euler X -")) NudgeDisplayEuler(attachmentRoot, 0, -step);
            if (GUILayout.Button("Euler Y +")) NudgeDisplayEuler(attachmentRoot, 1, step);
            if (GUILayout.Button("Euler Y -")) NudgeDisplayEuler(attachmentRoot, 1, -step);
            if (GUILayout.Button("Euler Z +")) NudgeDisplayEuler(attachmentRoot, 2, step);
            if (GUILayout.Button("Euler Z -")) NudgeDisplayEuler(attachmentRoot, 2, -step);
            EditorGUILayout.EndHorizontal();
        }

        private void NudgeDisplayEuler(Transform attachmentRoot, int axisIndex, float delta)
        {
            if (axisIndex == 0)
            {
                pendingDisplayEuler.x += delta;
            }
            else if (axisIndex == 1)
            {
                pendingDisplayEuler.y += delta;
            }
            else
            {
                pendingDisplayEuler.z += delta;
            }

            pendingDisplayEuler = CCS_EquipmentFitStudioWeaponRotationUtility.NormalizeEuler(pendingDisplayEuler);
            pendingLocalRotation = Quaternion.Euler(pendingDisplayEuler);
            ApplyPendingToAttachmentRoot(attachmentRoot);
        }

        private void DrawAxisVisualizationToggles()
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Axis Visualization", EditorStyles.miniBoldLabel);
            showSocketLocalAxes = EditorGUILayout.Toggle("Show Socket Local Axes", showSocketLocalAxes);
            showWeaponLocalAxes = EditorGUILayout.Toggle("Show Weapon Local Axes", showWeaponLocalAxes);
            showWeaponForwardAxis = EditorGUILayout.Toggle("Show Weapon Forward / Barrel Axis", showWeaponForwardAxis);
        }

        private void DrawAxisDiagnosticPanel(Transform attachmentRoot)
        {
            showAxisDiagnostics = EditorGUILayout.Foldout(showAxisDiagnostics, "Axis Diagnostics", true);
            if (!showAxisDiagnostics || attachmentRoot == null)
            {
                return;
            }

            EditorGUILayout.LabelField(
                "Attachment Root Local Rotation Euler: "
                + FormatVector(pendingDisplayEuler));
            EditorGUILayout.LabelField(
                "Attachment Root Local Rotation Quaternion: "
                + pendingLocalRotation.x.ToString("F3")
                + ", "
                + pendingLocalRotation.y.ToString("F3")
                + ", "
                + pendingLocalRotation.z.ToString("F3")
                + ", "
                + pendingLocalRotation.w.ToString("F3"));
            EditorGUILayout.LabelField(
                "Weapon Forward Axis Selected: "
                + CCS_EquipmentFitStudioWeaponRotationUtility.GetWeaponForwardAxisLabel(weaponForwardAxis));
            Vector3 worldForward = CCS_EquipmentFitStudioWeaponRotationUtility.GetWorldWeaponForwardDirection(
                pendingLocalRotation,
                attachmentRoot,
                weaponForwardAxis);
            EditorGUILayout.LabelField(
                "Weapon Forward World Direction: "
                + FormatVector(worldForward));
            EditorGUILayout.LabelField(
                "Last Axis Action: "
                + (string.IsNullOrEmpty(lastAxisAction) ? "(none)" : lastAxisAction));
        }

        private void DrawAxisTestButtons(Transform attachmentRoot)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Axis Hard Tests", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Test Pitch +15")) RunAxisHardTest(attachmentRoot, "Pitch");
            if (GUILayout.Button("Test Yaw +15")) RunAxisHardTest(attachmentRoot, "Yaw");
            if (GUILayout.Button("Test Roll +15")) RunAxisHardTest(attachmentRoot, "Roll");
            if (GUILayout.Button("Reset Axis Test")) ResetAxisHardTest(attachmentRoot);
            EditorGUILayout.EndHorizontal();
        }

        private void RunAxisHardTest(Transform attachmentRoot, string axisName)
        {
            if (!axisTestActive)
            {
                axisTestBaselineRotation = pendingLocalRotation;
                axisTestActive = true;
            }

            float degrees = CCS_EquipmentFitStudioWeaponRotationUtility.AxisTestDegrees;
            switch (axisName)
            {
                case "Pitch":
                    pendingLocalRotation = CCS_EquipmentFitStudioWeaponRotationUtility.ApplyPitchDelta(
                        axisTestBaselineRotation,
                        degrees);
                    break;
                case "Yaw":
                    pendingLocalRotation = CCS_EquipmentFitStudioWeaponRotationUtility.ApplyYawDelta(
                        axisTestBaselineRotation,
                        degrees);
                    break;
                default:
                    pendingLocalRotation = CCS_EquipmentFitStudioWeaponRotationUtility.ApplyRollDelta(
                        axisTestBaselineRotation,
                        degrees,
                        weaponForwardAxis);
                    break;
            }

            pendingDisplayEuler =
                CCS_EquipmentFitStudioWeaponRotationUtility.NormalizeEuler(pendingLocalRotation.eulerAngles);
            lastAxisAction = "Test " + axisName + " +" + degrees.ToString("0.#");
            rollAxisWarning = !CCS_EquipmentFitStudioWeaponRotationUtility.RollAndYawProduceDistinctRotations(
                weaponForwardAxis);
            ApplyPendingToAttachmentRoot(attachmentRoot);
        }

        private void ResetAxisHardTest(Transform attachmentRoot)
        {
            pendingLocalRotation = axisTestActive ? axisTestBaselineRotation : pendingLocalRotation;
            axisTestActive = false;
            pendingDisplayEuler =
                CCS_EquipmentFitStudioWeaponRotationUtility.NormalizeEuler(pendingLocalRotation.eulerAngles);
            lastAxisAction = "Reset Axis Test";
            ApplyPendingToAttachmentRoot(attachmentRoot);
        }

        private void DrawWeaponRotationSceneGizmos()
        {
            Transform socketTransform = GetSelectedSocketTransform();
            Transform attachmentRoot = GetPreviewAttachmentRootTransform();

            if (showSocketLocalAxes && socketTransform != null)
            {
                CCS_EquipmentFitStudioAxisVisualizationUtility.DrawLocalAxes(
                    socketTransform,
                    0.08f,
                    true);
            }

            if (attachmentRoot != null)
            {
                if (showWeaponLocalAxes)
                {
                    CCS_EquipmentFitStudioAxisVisualizationUtility.DrawLocalAxes(
                        attachmentRoot,
                        0.1f,
                        true);
                }

                if (showWeaponForwardAxis)
                {
                    CCS_EquipmentFitStudioAxisVisualizationUtility.DrawWeaponForwardAxis(
                        attachmentRoot,
                        pendingRotationInitialized ? pendingLocalRotation : attachmentRoot.localRotation,
                        weaponForwardAxis,
                        true);
                }
            }

            if (previewItem.IsSpawned && showWeaponLocalAxes)
            {
                CCS_EquipmentFitStudioAxisVisualizationUtility.DrawLocalAxes(
                    previewItem.PreviewRoot.transform,
                    0.06f,
                    false);
            }

            if (previewItem.IsSpawned)
            {
                Transform muzzle = previewItem.PreviewRoot.transform.Find("MuzzlePoint");
                if (muzzle == null)
                {
                    muzzle = FindDeepChild(previewItem.PreviewRoot.transform, "MuzzlePoint");
                }

                if (showWeaponForwardAxis && muzzle != null)
                {
                    CCS_EquipmentFitStudioAxisVisualizationUtility.DrawMuzzlePointAxis(muzzle);
                }
            }
        }

        #endregion
    }
}
