using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AnimationFitStudioRuntimeControllerClipUtility
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Resolves aim clips referenced by the player Animator Controller for Fit Studio.
// PLACEMENT: Editor utility used by Animation Fit Studio window and validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Read-only controller inspection. Save never modifies the Animator Controller.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public sealed class CCS_AnimationFitStudioRuntimeControllerClipInfo
    {
        public bool ControllerFound { get; set; }

        public string ControllerAssetPath { get; set; } = string.Empty;

        public bool FullDrawStateFound { get; set; }

        public string FullDrawStateClipPath { get; set; } = string.Empty;

        public string FullDrawStateClipName { get; set; } = string.Empty;

        public bool AimPitchBlendStateFound { get; set; }

        public string AimPitchCenterClipPath { get; set; } = string.Empty;

        public string AimPitchCenterClipName { get; set; } = string.Empty;

        public string PrimaryAimLoopStateName { get; set; } = string.Empty;

        public string PrimaryAimLoopClipPath { get; set; } = string.Empty;

        public bool SelectedClipUsedByController { get; set; }

        public string SelectedClipAssetPath { get; set; } = string.Empty;

        public string WarningMessage { get; set; } = string.Empty;

        public bool HasBlockingWarning => !string.IsNullOrEmpty(WarningMessage);

        public string ClipUsedByControllerLabel =>
            SelectedClipUsedByController ? "Yes" : HasBlockingWarning ? "No — See Warning" : "No";
    }

    public static class CCS_AnimationFitStudioRuntimeControllerClipUtility
    {
        public static bool TryQueryRuntimeAimClips(
            string selectedFitTestClipPath,
            out CCS_AnimationFitStudioRuntimeControllerClipInfo info)
        {
            info = new CCS_AnimationFitStudioRuntimeControllerClipInfo
            {
                SelectedClipAssetPath = NormalizeAssetPath(selectedFitTestClipPath),
            };

            string controllerPath = NormalizeAssetPath(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
            {
                info.WarningMessage =
                    "Missing player Animator Controller at "
                    + controllerPath
                    + ".";
                return false;
            }

            info.ControllerFound = true;
            info.ControllerAssetPath = controllerPath;

            int layerIndex = FindLayerIndex(
                controller,
                CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName);

            if (layerIndex < 0)
            {
                info.WarningMessage = "Player Animator Controller is missing RevolverUpperBody layer.";
                return false;
            }

            AnimatorStateMachine stateMachine = controller.layers[layerIndex].stateMachine;
            if (stateMachine == null)
            {
                info.WarningMessage = "RevolverUpperBody layer has no state machine.";
                return false;
            }

            AnimatorState fullDrawState = FindState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorRevolverAimIdleFullDrawStateName);

            if (fullDrawState != null)
            {
                info.FullDrawStateFound = true;
                if (TryGetMotionClipPath(fullDrawState.motion, out string fullDrawClipPath, out string fullDrawClipName))
                {
                    info.FullDrawStateClipPath = fullDrawClipPath;
                    info.FullDrawStateClipName = fullDrawClipName;
                    info.PrimaryAimLoopStateName =
                        CCS_CharacterControllerConstants.AnimatorRevolverAimIdleFullDrawStateName;
                    info.PrimaryAimLoopClipPath = fullDrawClipPath;
                }
            }

            string expectedRuntimePath = NormalizeAssetPath(
                CCS_CharacterControllerConstants.RevolverAimIdleFullDrawClipPath);
            bool matchesExpectedFullDraw = PathsEqual(info.FullDrawStateClipPath, expectedRuntimePath);
            bool matchesSelected = PathsEqual(info.SelectedClipAssetPath, info.FullDrawStateClipPath);
            info.SelectedClipUsedByController = matchesSelected;

            if (!string.IsNullOrEmpty(info.SelectedClipAssetPath)
                && PathsEqual(info.SelectedClipAssetPath, expectedRuntimePath)
                && !matchesExpectedFullDraw
                && info.FullDrawStateFound)
            {
                info.WarningMessage =
                    "Runtime controller is not currently using "
                    + Path.GetFileName(expectedRuntimePath)
                    + " on "
                    + CCS_CharacterControllerConstants.AnimatorRevolverAimIdleFullDrawStateName
                    + ".\nCurrent motion: "
                    + (string.IsNullOrEmpty(info.FullDrawStateClipName)
                        ? "(none)"
                        : info.FullDrawStateClipName)
                    + (string.IsNullOrEmpty(info.FullDrawStateClipPath)
                        ? string.Empty
                        : " (" + info.FullDrawStateClipPath + ")");
            }
            else if (!string.IsNullOrEmpty(info.SelectedClipAssetPath) && !matchesSelected)
            {
                info.WarningMessage =
                    "Warning: selected clip is not the current controller runtime aim-idle clip.\n"
                    + "Primary aim loop: "
                    + (string.IsNullOrEmpty(info.PrimaryAimLoopClipPath)
                        ? "(unknown)"
                        : info.PrimaryAimLoopClipPath);
            }

            return true;
        }

        public static bool TryResolveControllerFullDrawSaveTarget(
            out string clipAssetPath,
            out AnimationClip clip,
            out string errorMessage)
        {
            clipAssetPath = string.Empty;
            clip = null;
            errorMessage = string.Empty;

            if (!TryQueryRuntimeAimClips(
                    CCS_CharacterControllerConstants.RevolverAimIdleFullDrawClipPath,
                    out CCS_AnimationFitStudioRuntimeControllerClipInfo info))
            {
                errorMessage = info.WarningMessage;
                return false;
            }

            if (!info.FullDrawStateFound || string.IsNullOrEmpty(info.FullDrawStateClipPath))
            {
                errorMessage = "Animator Controller Revolver_AimIdle_FullDraw state has no motion clip.";
                return false;
            }

            clipAssetPath = NormalizeAssetPath(info.FullDrawStateClipPath);
            string expectedPath = NormalizeAssetPath(
                CCS_CharacterControllerConstants.RevolverAimIdleFullDrawClipPath);
            if (!PathsEqual(clipAssetPath, expectedPath))
            {
                errorMessage =
                    "Controller FullDraw motion path "
                    + clipAssetPath
                    + " does not match expected "
                    + expectedPath
                    + ".";
                return false;
            }

            clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipAssetPath);
            if (clip == null)
            {
                errorMessage = "Missing controller FullDraw clip at " + clipAssetPath + ".";
                return false;
            }

            return true;
        }

        public static bool ControllerReferencesClip(string fitTestClipPath)
        {
            if (!TryQueryRuntimeAimClips(fitTestClipPath, out CCS_AnimationFitStudioRuntimeControllerClipInfo info))
            {
                return false;
            }

            return info.SelectedClipUsedByController;
        }

        private static bool TryGetBlendTreeCenterClipPath(
            BlendTree blendTree,
            out string clipPath,
            out string clipName)
        {
            clipPath = string.Empty;
            clipName = string.Empty;
            if (blendTree == null || blendTree.children == null)
            {
                return false;
            }

            ChildMotion? centerChild = null;
            float bestThresholdDistance = float.MaxValue;
            for (int i = 0; i < blendTree.children.Length; i++)
            {
                ChildMotion child = blendTree.children[i];
                float distance = Mathf.Abs(child.threshold);
                if (distance < bestThresholdDistance)
                {
                    bestThresholdDistance = distance;
                    centerChild = child;
                }
            }

            if (centerChild == null)
            {
                return false;
            }

            return TryGetMotionClipPath(centerChild.Value.motion, out clipPath, out clipName);
        }

        private static bool TryGetMotionClipPath(Motion motion, out string clipPath, out string clipName)
        {
            clipPath = string.Empty;
            clipName = string.Empty;
            if (motion is not AnimationClip clip)
            {
                return false;
            }

            clipPath = NormalizeAssetPath(AssetDatabase.GetAssetPath(clip));
            clipName = clip.name;
            return !string.IsNullOrEmpty(clipPath);
        }

        private static int FindLayerIndex(AnimatorController controller, string layerName)
        {
            for (int i = 0; i < controller.layers.Length; i++)
            {
                if (controller.layers[i].name == layerName)
                {
                    return i;
                }
            }

            return -1;
        }

        private static AnimatorState FindState(AnimatorStateMachine stateMachine, string stateName)
        {
            if (stateMachine == null)
            {
                return null;
            }

            for (int i = 0; i < stateMachine.states.Length; i++)
            {
                ChildAnimatorState childState = stateMachine.states[i];
                if (childState.state != null && childState.state.name == stateName)
                {
                    return childState.state;
                }
            }

            return null;
        }

        private static string NormalizeAssetPath(string assetPath)
        {
            return string.IsNullOrEmpty(assetPath) ? string.Empty : assetPath.Replace('\\', '/');
        }

        private static bool PathsEqual(string left, string right)
        {
            return string.Equals(
                NormalizeAssetPath(left),
                NormalizeAssetPath(right),
                System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
