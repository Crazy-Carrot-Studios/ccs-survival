using System;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioWorkflowAccordion
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Step-by-step accordion workflow UI for Equipment Fit Studio.
// PLACEMENT: Drawn in left workflow panel by Fit Studio window.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Only active step expands; completed steps show compact summaries.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public enum CCS_EquipmentFitStudioWorkflowStep
    {
        SelectPlayer = 1,
        SelectWeapon = 2,
        SelectSocket = 3,
        SpawnPreview = 4,
        TuneSocket = 5,
        CaptureValues = 6,
        SaveProfile = 7,
        TestSavedFit = 8,
        ClearValidate = 9,
    }

    public sealed class CCS_EquipmentFitStudioWorkflowAccordionCallbacks
    {
        public Action DrawSelectPlayerStep;

        public Action DrawSelectWeaponStep;

        public Action DrawSelectSocketStep;

        public Action DrawSpawnPreviewStep;

        public Action DrawTuneSocketStep;

        public Action DrawCaptureValuesStep;

        public Action DrawSaveProfileStep;

        public Action DrawTestSavedFitStep;

        public Action DrawClearValidateStep;

        public Func<CCS_EquipmentFitStudioWorkflowStep, string> GetStepSummary;
    }

    public static class CCS_EquipmentFitStudioWorkflowAccordion
    {
        public static void DrawDecisionHelper(string selectedSocketId)
        {
            EditorGUILayout.LabelField("Should I edit socket or IK?", CCS_EquipmentFitStudioStyles.SectionLabel);
            EditorGUILayout.LabelField(
                "Edit SOCKET when:\n"
                + "• weapon is in the wrong place on the body\n"
                + "• holster/back carry is wrong\n"
                + "• gun grip is not near the hand\n"
                + "• barrel/weapon rotation is wrong at attachment\n\n"
                + "Edit IK when:\n"
                + "• weapon/socket is close but arm does not reach naturally\n"
                + "• elbow bends wrong\n"
                + "• wrist/aim line needs pose correction\n\n"
                + "Edit HAND/FINGER when weapon and hand are close and fingers need grip shaping.",
                CCS_EquipmentFitStudioStyles.ModeGuideBody);

            if (CCS_EquipmentFitStudioWorkflowGuide.IsPassiveHolsterOrBackSocket(selectedSocketId))
            {
                EditorGUILayout.HelpBox(
                    "Hip holster / back carry: Socket only for now. IK is not usually needed.",
                    MessageType.Info);
            }
            else if (CCS_EquipmentFitStudioWorkflowGuide.IsHandSocket(selectedSocketId))
            {
                EditorGUILayout.HelpBox(
                    "Right/left hand: Socket first, IK second.",
                    MessageType.Info);
            }
        }

        public static void DrawAccordion(
            CCS_EquipmentFitStudioWorkflowStep activeStep,
            CCS_EquipmentFitStudioWorkflowAccordionCallbacks callbacks)
        {
            EditorGUILayout.LabelField("Workflow Steps", CCS_EquipmentFitStudioStyles.SectionLabel);
            DrawStep(activeStep, CCS_EquipmentFitStudioWorkflowStep.SelectPlayer, "1. Select Player", callbacks);
            DrawStep(activeStep, CCS_EquipmentFitStudioWorkflowStep.SelectWeapon, "2. Select Weapon", callbacks);
            DrawStep(activeStep, CCS_EquipmentFitStudioWorkflowStep.SelectSocket, "3. Select Socket", callbacks);
            DrawStep(activeStep, CCS_EquipmentFitStudioWorkflowStep.SpawnPreview, "4. Spawn Preview", callbacks);
            DrawStep(activeStep, CCS_EquipmentFitStudioWorkflowStep.TuneSocket, "5. Tune Socket", callbacks);
            DrawStep(activeStep, CCS_EquipmentFitStudioWorkflowStep.CaptureValues, "6. Capture Values", callbacks);
            DrawStep(activeStep, CCS_EquipmentFitStudioWorkflowStep.SaveProfile, "7. Save Profile", callbacks);
            DrawStep(activeStep, CCS_EquipmentFitStudioWorkflowStep.TestSavedFit, "8. Test Saved Fit", callbacks);
            DrawStep(activeStep, CCS_EquipmentFitStudioWorkflowStep.ClearValidate, "9. Clear / Validate", callbacks);
        }

        public static CCS_EquipmentFitStudioWorkflowStep ResolveActiveStep(
            GameObject playerRoot,
            string weaponId,
            string socketId,
            bool previewSpawned,
            bool hasPendingSaveCapture,
            bool justSavedProfileThisSession,
            bool testAttachmentsExist,
            bool socketCompatible,
            CCS_EquipmentFitStudioWorkflowStep? manualStepOverride)
        {
            return CCS_EquipmentFitStudioWorkflowSessionUtility.ResolveActiveStep(
                playerRoot,
                weaponId,
                socketId,
                previewSpawned,
                hasPendingSaveCapture,
                justSavedProfileThisSession,
                testAttachmentsExist,
                socketCompatible,
                manualStepOverride);
        }

        private static void DrawStep(
            CCS_EquipmentFitStudioWorkflowStep activeStep,
            CCS_EquipmentFitStudioWorkflowStep step,
            string title,
            CCS_EquipmentFitStudioWorkflowAccordionCallbacks callbacks)
        {
            int stepIndex = (int)step;
            int activeIndex = (int)activeStep;
            bool isComplete = stepIndex < activeIndex;
            bool isActive = step == activeStep;
            bool isFuture = stepIndex > activeIndex;

            string summary = callbacks.GetStepSummary != null ? callbacks.GetStepSummary(step) : string.Empty;
            string header = title;
            if (isComplete)
            {
                header += " ✅ " + summary;
            }
            else if (isActive)
            {
                header += " ⬅ current";
            }

            GUIStyle headerStyle = isActive
                ? CCS_EquipmentFitStudioStyles.WorkflowStepActive
                : isComplete
                    ? CCS_EquipmentFitStudioStyles.StatusOkLabel
                    : CCS_EquipmentFitStudioStyles.WorkflowStepInactive;

            EditorGUILayout.LabelField(header, headerStyle);

            if (!isActive)
            {
                return;
            }

            EditorGUI.BeginDisabledGroup(isFuture);
            switch (step)
            {
                case CCS_EquipmentFitStudioWorkflowStep.SelectPlayer:
                    callbacks.DrawSelectPlayerStep?.Invoke();
                    break;
                case CCS_EquipmentFitStudioWorkflowStep.SelectWeapon:
                    callbacks.DrawSelectWeaponStep?.Invoke();
                    break;
                case CCS_EquipmentFitStudioWorkflowStep.SelectSocket:
                    callbacks.DrawSelectSocketStep?.Invoke();
                    break;
                case CCS_EquipmentFitStudioWorkflowStep.SpawnPreview:
                    callbacks.DrawSpawnPreviewStep?.Invoke();
                    break;
                case CCS_EquipmentFitStudioWorkflowStep.TuneSocket:
                    callbacks.DrawTuneSocketStep?.Invoke();
                    break;
                case CCS_EquipmentFitStudioWorkflowStep.CaptureValues:
                    callbacks.DrawCaptureValuesStep?.Invoke();
                    break;
                case CCS_EquipmentFitStudioWorkflowStep.SaveProfile:
                    callbacks.DrawSaveProfileStep?.Invoke();
                    break;
                case CCS_EquipmentFitStudioWorkflowStep.TestSavedFit:
                    callbacks.DrawTestSavedFitStep?.Invoke();
                    break;
                case CCS_EquipmentFitStudioWorkflowStep.ClearValidate:
                    callbacks.DrawClearValidateStep?.Invoke();
                    break;
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space(4f);
        }
    }
}
