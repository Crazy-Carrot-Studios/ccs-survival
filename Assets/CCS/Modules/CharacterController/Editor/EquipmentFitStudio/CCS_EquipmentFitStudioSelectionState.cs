using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioSelectionState
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Holds current Fit Studio selection, mode, and pending edits.
// PLACEMENT: Editor-only state object owned by Fit Studio window.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Cleared when the window closes.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public enum CCS_EquipmentFitStudioMode
    {
        SocketTuner = 0,
        IkTargetTuner = 1,
        PreviewView = 2,
        HandPoseFoundation = 3,
        SaveValidate = 4,
    }

    public sealed class CCS_EquipmentFitStudioSelectionState
    {
        public GameObject PlayerRoot;

        public string SelectedSocketId = CCS_EquipmentConstants.HolsterSocketRightHipId;

        public string SelectedWeaponId = CCS_EquipmentConstants.RevolverM1879WeaponId;

        public bool AllowIncompatibleSocketOverride;

        public string LastPreviewError = string.Empty;

        public string SelectedIkTargetName = CCS_EquipmentConstants.RightHandIkTargetObjectName;

        public CCS_EquipmentFitStudioMode Mode = CCS_EquipmentFitStudioMode.SocketTuner;

        public CCS_EquipmentSocketDefinition SelectedSocketDefinition;

        public CCS_WeaponAttachmentFitProfile SelectedAttachmentFitProfile;

        public CCS_WeaponIKPoseProfile SelectedIkPoseProfile;

        public CCS_HandPoseDefinition SelectedHandPoseDefinition;

        public CCS_EquipmentFitStudioPendingChange SocketPendingChange = new CCS_EquipmentFitStudioPendingChange();

        public CCS_EquipmentFitStudioPendingChange IkPendingChange = new CCS_EquipmentFitStudioPendingChange();

        public bool PreviewItemSpawned;

        public bool HasPendingSaveCapture;

        public bool SavedProfileThisSession;

        public bool JustSavedProfileThisSession;

        public CCS_EquipmentFitStudioWorkflowStep? WorkflowStepOverride;

        public string LastSaveConfirmationMessage = string.Empty;

        public CCS_EquipmentFitStudioPosePreviewMode PosePreviewMode =
            CCS_EquipmentFitStudioPosePreviewMode.Neutral;

        public bool UserManuallySelectedPosePreview;

        public string LastPosePreviewError = string.Empty;

        public CCS_EquipmentFitStudioFitMode FitStudioMode = CCS_EquipmentFitStudioFitMode.EditFitPreview;

        public CCS_EquipmentFitStudioFitTarget FitTarget = CCS_EquipmentFitStudioFitTarget.HolsteredItem;

        public int NudgeStepIndex;

        public bool ProfileLoadedFromSo;

        public string LastSavedDisplay = string.Empty;

        public bool UsesEditorPreviewPlayer;

        public CCS_EquipmentFitStudioEquippedPoseType EquippedPoseType =
            CCS_EquipmentFitStudioEquippedPoseType.OneHandRevolver;

        public bool ForceAimPoseActive;

        public bool HasUnsavedChanges =>
            HasPendingSaveCapture || IkPendingChange.HasCaptured;

        public void ClearPendingChanges()
        {
            SocketPendingChange = new CCS_EquipmentFitStudioPendingChange();
            IkPendingChange = new CCS_EquipmentFitStudioPendingChange();
            HasPendingSaveCapture = false;
            JustSavedProfileThisSession = false;
            SavedProfileThisSession = false;
        }

        public void ClearWorkflowSessionFlags()
        {
            HasPendingSaveCapture = false;
            JustSavedProfileThisSession = false;
            SavedProfileThisSession = false;
            WorkflowStepOverride = null;
            LastSaveConfirmationMessage = string.Empty;
        }
    }
}
