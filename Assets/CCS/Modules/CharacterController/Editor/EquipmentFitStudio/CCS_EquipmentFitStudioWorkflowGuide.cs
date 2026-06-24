using System.IO;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioWorkflowGuide
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Shared workflow helpers for Fit Studio (socket/IK guidance, profile status).
// PLACEMENT: Editor-only helper used by Equipment Fit Studio window.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Accordion UI lives in CCS_EquipmentFitStudioWorkflowAccordion.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public static class CCS_EquipmentFitStudioWorkflowGuide
    {
        public static bool IsPassiveHolsterOrBackSocket(string socketId)
        {
            return socketId == CCS_EquipmentConstants.HolsterSocketRightHipId
                || socketId == CCS_EquipmentConstants.HolsterSocketLeftHipId
                || socketId == CCS_EquipmentConstants.BackSocketLongGunAId
                || socketId == CCS_EquipmentConstants.BackSocketLongGunBId;
        }

        public static bool IsHandSocket(string socketId)
        {
            return socketId == CCS_EquipmentConstants.HandSocketRightId
                || socketId == CCS_EquipmentConstants.HandSocketLeftId;
        }

        public static bool ShouldDeemphasizeIk(string socketId)
        {
            return IsPassiveHolsterOrBackSocket(socketId);
        }

        public static void DrawProfileStatusPanel(
            bool hasUnsavedChanges,
            bool previewSpawned,
            bool testAttachmentsExist,
            bool ikPreviewNonZero)
        {
            EditorGUILayout.LabelField("Profile / Test Status", CCS_EquipmentFitStudioStyles.SectionLabel);
            DrawProfileLine(
                "Right Hip Fit Profile",
                File.Exists(CCS_EquipmentConstants.RevolverM1879RightHipHolsterFitPath),
                hasUnsavedChanges);
            DrawProfileLine(
                "Right Hand Fit Profile",
                File.Exists(CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath),
                hasUnsavedChanges);
            DrawIkProfileLine();
            DrawHandPoseProfileLine();

            if (previewSpawned)
            {
                EditorGUILayout.LabelField("Warning: preview object exists.", CCS_EquipmentFitStudioStyles.StatusWarnLabel);
            }

            if (testAttachmentsExist)
            {
                EditorGUILayout.LabelField("Warning: test attachment exists.", CCS_EquipmentFitStudioStyles.StatusWarnLabel);
            }

            if (hasUnsavedChanges)
            {
                EditorGUILayout.HelpBox(
                    "You have captured fit values that are not saved to the profile asset. Save before testing in Play Mode.",
                    MessageType.Warning);
            }

            if (ikPreviewNonZero)
            {
                EditorGUILayout.LabelField("Warning: IK preview weights are non-zero.", CCS_EquipmentFitStudioStyles.StatusErrorLabel);
            }
        }

        private static void DrawProfileLine(string label, bool found, bool unsavedChanges)
        {
            string status = found ? "Found" : "Missing";
            if (found && unsavedChanges)
            {
                status = "Unsaved changes";
            }

            EditorGUILayout.LabelField(label + ": " + status);
        }

        private static void DrawIkProfileLine()
        {
            CCS_WeaponIKPoseProfile profile = AssetDatabase.LoadAssetAtPath<CCS_WeaponIKPoseProfile>(
                CCS_EquipmentConstants.RevolverM1879AimIkPosePath);
            if (profile == null)
            {
                EditorGUILayout.LabelField("Aim IK Profile: Missing");
                return;
            }

            bool weightsZero = profile.RigWeight == 0f
                && profile.RightHandIKWeight == 0f
                && profile.LeftHandIKWeight == 0f
                && profile.AimWeight == 0f;
            EditorGUILayout.LabelField(
                "Aim IK Profile: Found / Weights " + (weightsZero ? "0" : "Non-Zero"));
        }

        private static void DrawHandPoseProfileLine()
        {
            bool found = File.Exists(CCS_EquipmentConstants.RevolverM1879RightHandGripPosePath);
            EditorGUILayout.LabelField("Hand Pose Profile: " + (found ? "Found" : "Missing"));
        }
    }
}
