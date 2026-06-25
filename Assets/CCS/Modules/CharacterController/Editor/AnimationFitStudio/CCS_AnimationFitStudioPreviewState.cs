using CCS.Modules.Weapons;

using UnityEngine;



// =============================================================================

// SCRIPT: CCS_AnimationFitStudioPreviewState

// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio

// PURPOSE: Tracks Animation Fit Studio editor preview player, weapon, and pose state.

// PLACEMENT: Used by Animation Fit Studio window and preview utilities.

// AUTHOR: James Schilz

// CREATED: 2026-06-07

// NOTES: Preview-only state. Never written to scene or prefab assets.

// =============================================================================



namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio

{

    public enum CCS_AnimationFitStudioPosePreviewStatus

    {

        NotLoaded,

        AppliedAimPose,

        AppliedIdleLike,

        Failed,

        SeedPose,

    }



    public enum CCS_AnimationFitStudioPoseSourceKind

    {

        None,

        Source,

        FitTest,

        Seed,

    }



    public sealed class CCS_AnimationFitStudioPreviewState

    {

        public GameObject PreviewPlayer { get; set; }



        public Animator PreviewAnimator { get; set; }



        public RuntimeAnimatorController StoredRuntimeAnimatorController { get; set; }



        public bool AnimatorControllerClearedForPreview { get; set; }



        public Transform WeaponAttachmentRoot { get; set; }



        public GameObject PreviewWeaponVisual { get; set; }



        public CCS_WeaponAttachmentFitProfile EquippedFitProfile { get; set; }



        public bool EquippedFitProfileApplied { get; set; }



        public bool WeaponVisualLoaded { get; set; }



        public bool PreviewWeaponZeroed { get; set; }



        public bool RightHandBonesFound { get; set; }

        public bool FingerBonesFound { get; set; }

        public CCS_AnimationFitStudioFingerDiscoveryResult FingerDiscovery { get; set; }

        public CCS_AnimationFitStudioFingerCurlDirectionKind FingerCurlDirection { get; set; } =
            CCS_AnimationFitStudioFingerCurlDirectionKind.Normal;

        public CCS_AnimationFitStudioPoseEditData PoseEdits { get; } = new CCS_AnimationFitStudioPoseEditData();

        public CCS_AnimationFitStudioHumanoidControlState HumanoidControl { get; } =
            new CCS_AnimationFitStudioHumanoidControlState();

        public CCS_AnimationFitStudioBasePoseSourceKind ActiveBasePoseSource { get; set; } =
            CCS_AnimationFitStudioPoseSourceCatalog.DefaultPoseSourceKind;



        public string ProfileWarningMessage { get; set; } = string.Empty;



        public CCS_AnimationFitStudioPosePreviewStatus PosePreviewStatus { get; set; } =

            CCS_AnimationFitStudioPosePreviewStatus.NotLoaded;



        public CCS_AnimationFitStudioPoseSourceKind ActivePoseSource { get; set; } =

            CCS_AnimationFitStudioPoseSourceKind.None;



        public bool PoseApplied { get; set; }



        public int ChangedBoneCount { get; set; }



        public float ActivePoseTime { get; set; }



        public string PoseWarningMessage { get; set; } = string.Empty;



        public CCS_AnimationFitStudioAimPoseScore AimPoseScore { get; set; }



        public string LastSampleMethod { get; set; } = string.Empty;



        public bool SeedPoseBasedFitTest { get; set; }



        public void Clear()

        {

            PreviewPlayer = null;

            PreviewAnimator = null;

            StoredRuntimeAnimatorController = null;

            AnimatorControllerClearedForPreview = false;

            WeaponAttachmentRoot = null;

            PreviewWeaponVisual = null;

            EquippedFitProfile = null;

            EquippedFitProfileApplied = false;

            WeaponVisualLoaded = false;

            PreviewWeaponZeroed = false;

            RightHandBonesFound = false;
            FingerBonesFound = false;
            FingerDiscovery = null;
            FingerCurlDirection = CCS_AnimationFitStudioFingerCurlDirectionKind.Normal;
            PoseEdits.InitializeDefaults();
            HumanoidControl.Clear();
            ActiveBasePoseSource = CCS_AnimationFitStudioPoseSourceCatalog.DefaultPoseSourceKind;

            ProfileWarningMessage = string.Empty;

            PosePreviewStatus = CCS_AnimationFitStudioPosePreviewStatus.NotLoaded;

            ActivePoseSource = CCS_AnimationFitStudioPoseSourceKind.None;

            PoseApplied = false;

            ChangedBoneCount = 0;

            ActivePoseTime = 0f;

            PoseWarningMessage = string.Empty;

            AimPoseScore = null;

            LastSampleMethod = string.Empty;

            SeedPoseBasedFitTest = false;

        }

    }

}


