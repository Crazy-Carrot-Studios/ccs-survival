using System.Collections.Generic;



using CCS.Modules.CharacterController;



using CCS.Modules.CharacterController.Editor.Common;







// =============================================================================



// SCRIPT: CCS_AnimationFitStudioPoseSourceCatalog



// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio



// PURPOSE: Allowed base pose sources for Animation Fit Studio v0.6.15 final-pose editor.



// PLACEMENT: Editor catalog used by window, clip resolver, and validation.



// AUTHOR: James Schilz



// CREATED: 2026-06-07



// NOTES: Default is Runtime Aim Idle FitTest — the clip referenced by the Animator Controller.



// =============================================================================







namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio



{



    public enum CCS_AnimationFitStudioBasePoseSourceKind
    {
        RuntimeAimIdleFullDraw,
        EquipmentFitStudioRevolverAim,
    }







    public sealed class CCS_AnimationFitStudioPoseSourceDefinition



    {



        public CCS_AnimationFitStudioBasePoseSourceKind Kind { get; }







        public string DisplayLabel { get; }







        public string SaveTargetClipFileName { get; }







        public string SaveTargetClipAssetPath { get; }







        public string FitTestClipFileName { get; }







        public bool UsesDirectFitTestAssetPath { get; }







        public CCS_AnimationFitStudioPoseSourceDefinition(



            CCS_AnimationFitStudioBasePoseSourceKind kind,



            string displayLabel,



            string saveTargetClipFileName,



            string saveTargetClipAssetPath,



            string fitTestClipFileName,



            bool usesDirectFitTestAssetPath = false)



        {



            Kind = kind;



            DisplayLabel = displayLabel;



            SaveTargetClipFileName = saveTargetClipFileName;



            SaveTargetClipAssetPath = saveTargetClipAssetPath;



            FitTestClipFileName = fitTestClipFileName;



            UsesDirectFitTestAssetPath = usesDirectFitTestAssetPath;



        }



    }







    public static class CCS_AnimationFitStudioPoseSourceCatalog



    {



        private static readonly IReadOnlyList<CCS_AnimationFitStudioPoseSourceDefinition> Definitions =
            new List<CCS_AnimationFitStudioPoseSourceDefinition>
            {
                new CCS_AnimationFitStudioPoseSourceDefinition(
                    CCS_AnimationFitStudioBasePoseSourceKind.RuntimeAimIdleFullDraw,
                    "Runtime Aim Idle — FullDraw",
                    "CCS_WW_Revolver_AimIdle_FullDraw",
                    CCS_CharacterControllerConstants.RevolverAimIdleFullDrawClipPath,
                    "CCS_WW_Revolver_AimIdle_FullDraw.anim",
                    usesDirectFitTestAssetPath: true),
                new CCS_AnimationFitStudioPoseSourceDefinition(
                    CCS_AnimationFitStudioBasePoseSourceKind.EquipmentFitStudioRevolverAim,
                    CCS_RevolverAimPreviewPoseUtility.DisplayLabel,
                    "CCS_WW_Revolver_AimIdle_FullDraw",
                    CCS_CharacterControllerConstants.AnimationFitStudioDefaultSourceClipPath,
                    "CCS_WW_Revolver_AimIdle_FullDraw.anim",
                    usesDirectFitTestAssetPath: true),
            };

        private static readonly string[] PoseSourceDisplayLabels =
        {
            "Runtime Aim Idle — FullDraw",
            CCS_RevolverAimPreviewPoseUtility.DisplayLabel,
        };







        public static IReadOnlyList<CCS_AnimationFitStudioPoseSourceDefinition> AllDefinitions => Definitions;







        public static string[] PoseSourceLabels => PoseSourceDisplayLabels;







        public static int AllowedPoseSourceCount => Definitions.Count;







        public static CCS_AnimationFitStudioBasePoseSourceKind DefaultPoseSourceKind =>
            CCS_AnimationFitStudioBasePoseSourceKind.RuntimeAimIdleFullDraw;







        public static bool TryGetDefinition(



            CCS_AnimationFitStudioBasePoseSourceKind kind,



            out CCS_AnimationFitStudioPoseSourceDefinition definition)



        {



            for (int i = 0; i < Definitions.Count; i++)



            {



                if (Definitions[i].Kind == kind)



                {



                    definition = Definitions[i];



                    return true;



                }



            }







            definition = null;



            return false;



        }







        public static int GetPoseSourceIndex(CCS_AnimationFitStudioBasePoseSourceKind kind)



        {



            for (int i = 0; i < Definitions.Count; i++)



            {



                if (Definitions[i].Kind == kind)



                {



                    return i;



                }



            }







            return 0;



        }







        public static CCS_AnimationFitStudioBasePoseSourceKind GetPoseSourceKindFromIndex(int index)



        {



            int clamped = UnityEngine.Mathf.Clamp(index, 0, Definitions.Count - 1);



            return Definitions[clamped].Kind;



        }







        public static string GetPoseSourceDisplayLabel(CCS_AnimationFitStudioBasePoseSourceKind kind)



        {



            return TryGetDefinition(kind, out CCS_AnimationFitStudioPoseSourceDefinition definition)



                ? definition.DisplayLabel



                : kind.ToString();



        }







        public static bool UsesClipSampling(CCS_AnimationFitStudioBasePoseSourceKind kind)
        {
            return kind != CCS_AnimationFitStudioBasePoseSourceKind.EquipmentFitStudioRevolverAim
                && kind != CCS_AnimationFitStudioBasePoseSourceKind.RuntimeAimIdleFullDraw;
        }







        public static bool UsesDirectFitTestAssetPath(CCS_AnimationFitStudioBasePoseSourceKind kind)



        {



            return TryGetDefinition(kind, out CCS_AnimationFitStudioPoseSourceDefinition definition)



                && definition.UsesDirectFitTestAssetPath;



        }



    }



}



