using CCS.Project;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioWorkflowSessionUtility
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Active fit target state, workflow step resolution, and slot switching helpers.
// PLACEMENT: Editor utility used by Equipment Fit Studio window and validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Saved profiles do not lock the workflow. Target selection precedes the 9 steps.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public enum CCS_EquipmentFitStudioFitTargetState
    {
        ReadyToTune = 0,
        Editing = 1,
        Unsaved = 2,
        Saved = 3,
        Testing = 4,
    }

    public static class CCS_EquipmentFitStudioWorkflowSessionUtility
    {
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
            if (manualStepOverride.HasValue)
            {
                CCS_EquipmentFitStudioWorkflowStep overridden = manualStepOverride.Value;
                if (playerRoot == null && overridden != CCS_EquipmentFitStudioWorkflowStep.SelectPlayer)
                {
                    return CCS_EquipmentFitStudioWorkflowStep.SelectPlayer;
                }

                if (string.IsNullOrEmpty(weaponId) && overridden > CCS_EquipmentFitStudioWorkflowStep.SelectWeapon)
                {
                    return CCS_EquipmentFitStudioWorkflowStep.SelectWeapon;
                }

                if ((string.IsNullOrEmpty(socketId) || !socketCompatible)
                    && overridden > CCS_EquipmentFitStudioWorkflowStep.SelectSocket)
                {
                    return CCS_EquipmentFitStudioWorkflowStep.SelectSocket;
                }

                return overridden;
            }

            if (playerRoot == null)
            {
                return CCS_EquipmentFitStudioWorkflowStep.SelectPlayer;
            }

            if (string.IsNullOrEmpty(weaponId))
            {
                return CCS_EquipmentFitStudioWorkflowStep.SelectWeapon;
            }

            if (string.IsNullOrEmpty(socketId) || !socketCompatible)
            {
                return CCS_EquipmentFitStudioWorkflowStep.SelectSocket;
            }

            if (hasPendingSaveCapture)
            {
                return CCS_EquipmentFitStudioWorkflowStep.SaveProfile;
            }

            if (justSavedProfileThisSession)
            {
                return CCS_EquipmentFitStudioWorkflowStep.TestSavedFit;
            }

            if (testAttachmentsExist)
            {
                return CCS_EquipmentFitStudioWorkflowStep.ClearValidate;
            }

            if (!previewSpawned)
            {
                return CCS_EquipmentFitStudioWorkflowStep.SpawnPreview;
            }

            return CCS_EquipmentFitStudioWorkflowStep.TuneSocket;
        }

        public static CCS_EquipmentFitStudioFitTargetState ResolveFitTargetState(
            bool hasPendingSaveCapture,
            bool justSavedProfileThisSession,
            bool previewSpawned,
            bool testAttachmentsExist,
            CCS_WeaponAttachmentFitProfile activeProfile)
        {
            if (testAttachmentsExist)
            {
                return CCS_EquipmentFitStudioFitTargetState.Testing;
            }

            if (hasPendingSaveCapture)
            {
                return CCS_EquipmentFitStudioFitTargetState.Unsaved;
            }

            if (justSavedProfileThisSession)
            {
                return CCS_EquipmentFitStudioFitTargetState.Saved;
            }

            if (previewSpawned)
            {
                return CCS_EquipmentFitStudioFitTargetState.Editing;
            }

            if (activeProfile != null && !ProfileUsesSeedDefaults(activeProfile))
            {
                return CCS_EquipmentFitStudioFitTargetState.ReadyToTune;
            }

            return CCS_EquipmentFitStudioFitTargetState.ReadyToTune;
        }

        public static string GetFitTargetStateLabel(CCS_EquipmentFitStudioFitTargetState targetState)
        {
            switch (targetState)
            {
                case CCS_EquipmentFitStudioFitTargetState.Editing:
                    return "Editing";
                case CCS_EquipmentFitStudioFitTargetState.Unsaved:
                    return "Unsaved";
                case CCS_EquipmentFitStudioFitTargetState.Saved:
                    return "Saved";
                case CCS_EquipmentFitStudioFitTargetState.Testing:
                    return "Testing";
                default:
                    return "Ready to tune";
            }
        }

        public static string GetSlotDisplayLabel(string socketId)
        {
            switch (socketId)
            {
                case CCS_EquipmentConstants.HolsterSocketRightHipId:
                    return "Right Hip Holster";
                case CCS_EquipmentConstants.HandSocketRightId:
                    return "Right Hand Equipped";
                case CCS_EquipmentConstants.HolsterSocketLeftHipId:
                    return "Left Hip Holster";
                case CCS_EquipmentConstants.HandSocketLeftId:
                    return "Left Hand";
                case CCS_EquipmentConstants.BackSocketLongGunAId:
                    return "Back Long Gun A";
                case CCS_EquipmentConstants.BackSocketLongGunBId:
                    return "Back Long Gun B";
                default:
                    return socketId;
            }
        }

        public static string GetWeaponDisplayLabel(string weaponId)
        {
            if (weaponId == CCS_EquipmentConstants.RevolverM1879WeaponId)
            {
                return "Revolver M1879";
            }

            return weaponId;
        }

        public static bool IsRevolverQuickTargetMapped(string socketId)
        {
            return socketId == CCS_EquipmentConstants.HolsterSocketRightHipId
                || socketId == CCS_EquipmentConstants.HandSocketRightId;
        }

        public static bool ProfileUsesSeedDefaults(CCS_WeaponAttachmentFitProfile profile)
        {
            if (profile == null)
            {
                return false;
            }

            if (profile.SocketId == CCS_EquipmentConstants.HolsterSocketRightHipId)
            {
                return CCS_EquipmentFitProfilePersistenceUtility.ProfileMatchesSeedDefaults(profile);
            }

            if (profile.SocketId == CCS_EquipmentConstants.HandSocketRightId)
            {
                return CCS_EquipmentFitStudioPendingChange.VectorsApproximatelyEqual(
                        profile.SocketLocalPosition,
                        new Vector3(0.03f, 0.015f, 0.05f))
                    && CCS_EquipmentFitStudioPendingChange.VectorsApproximatelyEqual(
                        profile.SocketLocalEulerAngles,
                        new Vector3(-12f, 92f, 8f));
            }

            return false;
        }

        public static string GetActiveProfileFileName(string socketId)
        {
            string path = CCS_EquipmentFitStudioRevolverFitUtility.GetRevolverAttachmentFitProfilePath(socketId);
            if (string.IsNullOrEmpty(path))
            {
                return "Not mapped";
            }

            return System.IO.Path.GetFileName(path);
        }

        public static string GetRuntimeAttachmentRootName(string socketId)
        {
            if (socketId == CCS_EquipmentConstants.HolsterSocketRightHipId)
            {
                return CCS_EquipmentConstants.RuntimeHolsterAttachmentRootObjectName;
            }

            if (socketId == CCS_EquipmentConstants.HandSocketRightId)
            {
                return CCS_EquipmentConstants.RuntimeEquippedAttachmentRootObjectName;
            }

            return string.Empty;
        }

        public static CCS_SurvivalValidationResult ValidateActiveTargetWorkflowRouting()
        {
            if (CCS_EquipmentFitStudioRevolverFitUtility.GetRevolverAttachmentFitProfilePath(
                    CCS_EquipmentConstants.HolsterSocketRightHipId)
                != CCS_EquipmentConstants.RevolverM1879RightHipHolsterFitPath)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Right hip holster must map to CCS_RevolverM1879_RightHipHolster_Fit.asset.");
            }

            if (CCS_EquipmentFitStudioRevolverFitUtility.GetRevolverAttachmentFitProfilePath(
                    CCS_EquipmentConstants.HandSocketRightId)
                != CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Right hand equipped must map to CCS_RevolverM1879_RightHandEquipped_Fit.asset.");
            }

            CCS_EquipmentFitStudioSelectionState hipState = new CCS_EquipmentFitStudioSelectionState
            {
                SelectedSocketId = CCS_EquipmentConstants.HolsterSocketRightHipId,
                HasPendingSaveCapture = true,
            };
            hipState.SocketPendingChange.CaptureFromBaseline(
                CCS_EquipmentConstants.HolsterSocketRightHipId,
                "HipProfile",
                Vector3.zero,
                Vector3.zero,
                Vector3.one,
                new Vector3(0.19f, -0.08f, -0.02f),
                new Vector3(73.01f, 1f, 349f),
                Vector3.one);

            hipState.SelectedSocketId = CCS_EquipmentConstants.HandSocketRightId;
            hipState.ClearPendingChanges();

            if (hipState.HasPendingSaveCapture || hipState.SocketPendingChange.HasCaptured)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Switching active target from Right Hip to Right Hand must clear pending capture.");
            }

            CCS_EquipmentFitStudioWorkflowStep hipStep;
            GameObject validationPlayer = new GameObject("FitStudioValidationPlayer");
            try
            {
                hipStep = ResolveActiveStep(
                    validationPlayer,
                    CCS_EquipmentConstants.RevolverM1879WeaponId,
                    CCS_EquipmentConstants.HolsterSocketRightHipId,
                    previewSpawned: false,
                    hasPendingSaveCapture: false,
                    justSavedProfileThisSession: false,
                    testAttachmentsExist: false,
                    socketCompatible: true,
                    manualStepOverride: null);
            }
            finally
            {
                Object.DestroyImmediate(validationPlayer);
            }

            if (hipStep != CCS_EquipmentFitStudioWorkflowStep.SpawnPreview)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Existing saved profile must not lock workflow at Step 7/8. Expected Step 4 Spawn Preview.");
            }

            GameObject pendingPlayer = new GameObject("FitStudioValidationPlayerPending");
            CCS_EquipmentFitStudioWorkflowStep pendingStep;
            try
            {
                pendingStep = ResolveActiveStep(
                    pendingPlayer,
                    CCS_EquipmentConstants.RevolverM1879WeaponId,
                    CCS_EquipmentConstants.HandSocketRightId,
                    previewSpawned: true,
                    hasPendingSaveCapture: true,
                    justSavedProfileThisSession: false,
                    testAttachmentsExist: false,
                    socketCompatible: true,
                    manualStepOverride: null);
            }
            finally
            {
                Object.DestroyImmediate(pendingPlayer);
            }

            if (pendingStep != CCS_EquipmentFitStudioWorkflowStep.SaveProfile)
            {
                return CCS_SurvivalValidationResult.Fail("Pending capture must route workflow to Step 7 Save Profile.");
            }

            return CCS_SurvivalValidationResult.Pass("Active fit target workflow routing validated.");
        }
    }
}
