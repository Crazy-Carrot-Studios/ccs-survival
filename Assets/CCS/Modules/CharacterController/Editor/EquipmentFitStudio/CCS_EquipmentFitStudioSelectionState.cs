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

        public string SelectedSocketId = CCS_EquipmentConstants.HandSocketRightId;

        public string SelectedIkTargetName = CCS_EquipmentConstants.RightHandIkTargetObjectName;

        public CCS_EquipmentFitStudioMode Mode = CCS_EquipmentFitStudioMode.SocketTuner;

        public CCS_EquipmentSocketDefinition SelectedSocketDefinition;

        public CCS_WeaponAttachmentFitProfile SelectedAttachmentFitProfile;

        public CCS_WeaponIKPoseProfile SelectedIkPoseProfile;

        public CCS_HandPoseDefinition SelectedHandPoseDefinition;

        public CCS_EquipmentFitStudioPendingChange SocketPendingChange = new CCS_EquipmentFitStudioPendingChange();

        public CCS_EquipmentFitStudioPendingChange IkPendingChange = new CCS_EquipmentFitStudioPendingChange();

        public bool PreviewItemSpawned;

        public bool HasUnsavedChanges =>
            SocketPendingChange.HasChanges || IkPendingChange.HasChanges;

        public void ClearPendingChanges()
        {
            SocketPendingChange = new CCS_EquipmentFitStudioPendingChange();
            IkPendingChange = new CCS_EquipmentFitStudioPendingChange();
        }
    }
}
