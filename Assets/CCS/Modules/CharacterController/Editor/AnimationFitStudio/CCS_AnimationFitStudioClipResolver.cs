using System.IO;
using CCS.Modules.CharacterController;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AnimationFitStudioClipResolver
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Resolves allowed Animation Fit Studio pose target source clips.
// PLACEMENT: Editor utility used by Animation Fit Studio window and validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.15 exposes two pose targets only via dropdown. No clip picker.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public static class CCS_AnimationFitStudioClipResolver
    {
        public const string DefaultSourceClipName = CCS_AnimationFitStudioConstants.DefaultSourceClipName;

        public const string DefaultAimedMovementClipName =
            CCS_AnimationFitStudioConstants.DefaultAimedMovementClipName;

        public const string MissingDefaultSourceClipError =
            "Missing default stationary aim clip:\n"
            + DefaultSourceClipName
            + "\nExpected under Content/Animations/Revolver/WildWest/.";

        public static string DefaultSourceClipAssetPath =>
            CCS_CharacterControllerConstants.AnimationFitStudioDefaultSourceClipPath;

        public static string DefaultFitTestClipAssetPath =>
            CCS_CharacterControllerConstants.AnimationFitStudioDefaultFitTestClipPath;

        public static bool TryResolveClipForPoseSource(
            CCS_AnimationFitStudioBasePoseSourceKind sourceKind,
            out AnimationClip clip,
            out string assetPath,
            out string errorMessage)
        {
            clip = null;
            assetPath = string.Empty;
            errorMessage = string.Empty;

            if (!CCS_AnimationFitStudioPoseSourceCatalog.TryGetDefinition(
                    sourceKind,
                    out CCS_AnimationFitStudioPoseSourceDefinition definition))
            {
                errorMessage = "Unknown pose source.";
                return false;
            }

            if (definition.UsesDirectFitTestAssetPath
                || !string.IsNullOrEmpty(definition.SaveTargetClipAssetPath))
            {
                clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(definition.SaveTargetClipAssetPath);
                if (clip != null)
                {
                    assetPath = definition.SaveTargetClipAssetPath;
                    return true;
                }

                errorMessage = "Missing pose source clip:\n"
                    + definition.SaveTargetClipFileName
                    + "\nExpected at "
                    + definition.SaveTargetClipAssetPath
                    + ".";
                return false;
            }

            string root = CCS_CharacterControllerConstants.WildWestRevolverAnimationsPath;
            if (!Directory.Exists(root))
            {
                errorMessage = "Missing WildWest animation folder at " + root + ".";
                return false;
            }

            clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(definition.SaveTargetClipAssetPath);
            if (clip != null)
            {
                assetPath = definition.SaveTargetClipAssetPath;
                return true;
            }

            errorMessage = "Missing pose source clip:\n"
                + definition.SaveTargetClipFileName
                + "\nExpected at "
                + definition.SaveTargetClipAssetPath
                + ".";
            return false;
        }

        public static bool TryResolveClipForPoseTarget(
            CCS_AnimationFitStudioPoseTargetKind targetKind,
            out AnimationClip clip,
            out string assetPath,
            out string errorMessage)
        {
            clip = null;
            assetPath = string.Empty;
            errorMessage = string.Empty;

            if (targetKind == CCS_AnimationFitStudioPoseTargetKind.AimedWalkRh)
            {
                assetPath = CCS_CharacterControllerConstants.WildWestRevolverWalkAimedRhClipPath;
                clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
                if (clip != null)
                {
                    return true;
                }

                errorMessage = "Missing WalkAimed RH clip at " + assetPath + ".";
                return false;
            }

            return TryResolveClipForPoseSource(
                CCS_AnimationFitStudioBasePoseSourceKind.RuntimeAimIdleFullDraw,
                out clip,
                out assetPath,
                out errorMessage);
        }

        public static bool TryResolveDefaultRuntimeFitTestClip(
            out AnimationClip clip,
            out string assetPath,
            out string errorMessage)
        {
            return TryResolveClipForPoseSource(
                CCS_AnimationFitStudioPoseSourceCatalog.DefaultPoseSourceKind,
                out clip,
                out assetPath,
                out errorMessage);
        }

        public static string GetFitTestClipPathForPoseSource(CCS_AnimationFitStudioBasePoseSourceKind sourceKind)
        {
            if (CCS_AnimationFitStudioPoseSourceCatalog.TryGetDefinition(
                    sourceKind,
                    out CCS_AnimationFitStudioPoseSourceDefinition definition))
            {
                if (definition.UsesDirectFitTestAssetPath)
                {
                    return definition.SaveTargetClipAssetPath;
                }

                return GetFitTestClipPathForSource(definition.SaveTargetClipAssetPath);
            }

            return GetDefaultFitTestClipPath();
        }

        public static bool TryResolveDefaultSourceAimClip(
            out AnimationClip clip,
            out string assetPath,
            out string errorMessage)
        {
            return TryResolveClipForPoseSource(
                CCS_AnimationFitStudioPoseSourceCatalog.DefaultPoseSourceKind,
                out clip,
                out assetPath,
                out errorMessage);
        }

        public static bool TryResolveSourceAimClip(
            out AnimationClip clip,
            out string assetPath,
            out string errorMessage)
        {
            return TryResolveDefaultSourceAimClip(out clip, out assetPath, out errorMessage);
        }

        public static string GetFitTestOutputFolderPath()
        {
            return CCS_CharacterControllerConstants.RevolverAimAnimationsPath;
        }

        public static string GetDefaultFitTestClipFileName()
        {
            return CCS_AnimationFitStudioConstants.DefaultControllerFullDrawClipFileName;
        }

        public static string GetFitTestClipFileName(string sourceAssetPath)
        {
            if (string.IsNullOrEmpty(sourceAssetPath))
            {
                return GetDefaultFitTestClipFileName();
            }

            return Path.GetFileName(sourceAssetPath);
        }

        public static string GetFitTestClipPathForSource(string sourceAssetPath)
        {
            if (string.IsNullOrEmpty(sourceAssetPath))
            {
                return GetDefaultFitTestClipPath();
            }

            return sourceAssetPath;
        }

        public static string GetDefaultFitTestClipPath()
        {
            return DefaultFitTestClipAssetPath;
        }

        public static bool TryGetExpectedFitTestPaths(
            string sourceAssetPath,
            out string fitTestFileName,
            out string fitTestAssetPath,
            out string outputFolderPath)
        {
            fitTestFileName = GetFitTestClipFileName(sourceAssetPath);
            fitTestAssetPath = GetFitTestClipPathForSource(sourceAssetPath);
            outputFolderPath = GetFitTestOutputFolderPath();
            return !string.IsNullOrEmpty(fitTestFileName);
        }
    }
}
