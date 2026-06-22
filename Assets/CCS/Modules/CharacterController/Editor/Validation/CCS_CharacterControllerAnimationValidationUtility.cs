using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController;
using CCS.Project;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterControllerAnimationValidationUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Validates player Animator Controller uses only CCS-owned animation clips.
// PLACEMENT: Called from master test validator and animation isolation menu.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.4 — validates revolver upper-body isolation and RevolverUpperBody layer wiring.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerAnimationValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidatePlayerAnimatorControllerAnimationIsolation()
        {
            List<string> failures = new List<string>();

            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath),
                "Missing player Animator Controller at "
                + CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath
                + ".");

            AppendIfMissing(
                failures,
                Directory.Exists(CCS_CharacterControllerConstants.ContentAnimationsRootPath),
                "Missing Content/Animations folder at "
                + CCS_CharacterControllerConstants.ContentAnimationsRootPath
                + ".");

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            if (controller == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            HashSet<Motion> visitedMotions = new HashSet<Motion>();
            List<AnimationClip> clips = new List<AnimationClip>();
            for (int layerIndex = 0; layerIndex < controller.layers.Length; layerIndex++)
            {
                CollectAnimationClips(controller.layers[layerIndex].stateMachine, visitedMotions, clips);
            }

            AppendIfMissing(
                failures,
                clips.Count > 0,
                "Player Animator Controller resolved zero animation clips.");

            string allowedRoot = NormalizeAssetPath(CCS_CharacterControllerConstants.ContentAnimationsRootPath);
            for (int i = 0; i < clips.Count; i++)
            {
                AnimationClip clip = clips[i];
                if (clip == null)
                {
                    continue;
                }

                string clipPath = AssetDatabase.GetAssetPath(clip);
                if (string.IsNullOrEmpty(clipPath))
                {
                    failures.Add("Player Animator Controller references unresolved animation clip.");
                    continue;
                }

                string normalizedClipPath = NormalizeAssetPath(clipPath);
                if (!normalizedClipPath.StartsWith(allowedRoot))
                {
                    failures.Add(
                        "Player Animator Controller references non-CCS animation clip: "
                        + normalizedClipPath);
                }

                if (IsVendorFbxSubAssetPath(normalizedClipPath))
                {
                    failures.Add(
                        "Player Animator Controller references vendor FBX sub-asset: "
                        + normalizedClipPath);
                }
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass(
                    "Player Animator Controller uses CCS-owned animation clips only.");
        }

        public static CCS_SurvivalValidationResult ValidateAimLocomotionAnimatorParameters()
        {
            List<string> failures = new List<string>();

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            AppendIfMissing(failures, controller != null, "Player Animator Controller is missing.");

            if (controller == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            AppendIfMissing(
                failures,
                HasAnimatorParameter(controller, CCS_CharacterControllerConstants.AnimatorIsAimingMovementModeParameter),
                "Player Animator Controller must define IsAimingMovementMode.");
            AppendIfMissing(
                failures,
                HasAnimatorParameter(controller, CCS_CharacterControllerConstants.AnimatorAimMoveXParameter),
                "Player Animator Controller must define AimMoveX.");
            AppendIfMissing(
                failures,
                HasAnimatorParameter(controller, CCS_CharacterControllerConstants.AnimatorAimMoveYParameter),
                "Player Animator Controller must define AimMoveY.");

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Player Animator Controller aim strafe parameters validated.");
        }

        public static CCS_SurvivalValidationResult ValidateAimStrafeAnimationIsolation()
        {
            List<string> failures = new List<string>();

            AppendIfMissing(
                failures,
                Directory.Exists(CCS_CharacterControllerConstants.AimStrafeAnimationsPath),
                "Missing AimStrafe animation folder at "
                + CCS_CharacterControllerConstants.AimStrafeAnimationsPath
                + ".");

            ValidateRequiredAimStrafeClipAsset(
                failures,
                CCS_CharacterControllerConstants.AimStrafeWalkFwdClipPath,
                requireLoopTime: true);
            ValidateRequiredAimStrafeClipAsset(
                failures,
                CCS_CharacterControllerConstants.AimStrafeWalkBwdClipPath,
                requireLoopTime: true);
            ValidateRequiredAimStrafeClipAsset(
                failures,
                CCS_CharacterControllerConstants.AimStrafeStrafeLeftClipPath,
                requireLoopTime: true);
            ValidateRequiredAimStrafeClipAsset(
                failures,
                CCS_CharacterControllerConstants.AimStrafeStrafeRightClipPath,
                requireLoopTime: true);

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            AppendIfMissing(failures, controller != null, "Player Animator Controller is missing.");
            if (controller == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            AnimatorState aimState = FindState(
                controller.layers[0].stateMachine,
                CCS_CharacterControllerConstants.AnimatorAimStrafeLocomotionStateName);
            AppendIfMissing(
                failures,
                aimState != null,
                "Player Animator Controller must define AimStrafe_Locomotion state.");

            if (aimState == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            BlendTree blendTree = aimState.motion as BlendTree;
            AppendIfMissing(
                failures,
                blendTree != null,
                "AimStrafe_Locomotion must use a 2D blend tree.");
            if (blendTree == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            AppendIfMissing(
                failures,
                blendTree.blendType == BlendTreeType.FreeformDirectional2D
                    || blendTree.blendType == BlendTreeType.SimpleDirectional2D,
                "AimStrafe_Locomotion blend tree must be 2D directional.");
            AppendIfMissing(
                failures,
                blendTree.blendParameter == CCS_CharacterControllerConstants.AnimatorAimMoveXParameter,
                "AimStrafe blend tree must use AimMoveX.");
            AppendIfMissing(
                failures,
                blendTree.blendParameterY == CCS_CharacterControllerConstants.AnimatorAimMoveYParameter,
                "AimStrafe blend tree must use AimMoveY.");

            ValidateBlendTreeMotionAt(
                failures,
                blendTree,
                new Vector2(0f, 1f),
                CCS_CharacterControllerConstants.AimStrafeWalkFwdClipPath);
            ValidateBlendTreeMotionAt(
                failures,
                blendTree,
                new Vector2(0f, -1f),
                CCS_CharacterControllerConstants.AimStrafeWalkBwdClipPath);
            ValidateBlendTreeMotionAt(
                failures,
                blendTree,
                new Vector2(-1f, 0f),
                CCS_CharacterControllerConstants.AimStrafeStrafeLeftClipPath);
            ValidateBlendTreeMotionAt(
                failures,
                blendTree,
                new Vector2(1f, 0f),
                CCS_CharacterControllerConstants.AimStrafeStrafeRightClipPath);

            ChildMotion[] children = blendTree.children;
            bool hasIdleCenter = false;
            string idlePath = CCS_CharacterControllerConstants.LocomotionAnimationsPath + "/CCS_Locomotion_Idle.anim";
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].motion is AnimationClip clip
                    && Vector2.Distance(children[i].position, Vector2.zero) <= 0.001f
                    && NormalizeAssetPath(AssetDatabase.GetAssetPath(clip)) == NormalizeAssetPath(idlePath))
                {
                    hasIdleCenter = true;
                    break;
                }
            }

            AppendIfMissing(
                failures,
                hasIdleCenter,
                "AimStrafe blend tree must include CCS idle at (0,0).");

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Aim strafe animation isolation validated.");
        }

        public static CCS_SurvivalValidationResult ValidateRevolverUpperBodyAnimationIsolation()
        {
            List<string> failures = new List<string>();

            AppendIfMissing(
                failures,
                Directory.Exists(CCS_CharacterControllerConstants.CombatRevolverAnimationsPath),
                "Missing Revolver animation folder at "
                + CCS_CharacterControllerConstants.CombatRevolverAnimationsPath
                + ".");

            ValidateRequiredRevolverClipAsset(
                failures,
                CCS_CharacterControllerConstants.RevolverAimIdleClipPath,
                requireLoopTime: true);
            ValidateRequiredRevolverClipAsset(
                failures,
                CCS_CharacterControllerConstants.RevolverIdlePistolClipPath,
                requireLoopTime: true);
            ValidateRequiredRevolverClipAsset(
                failures,
                CCS_CharacterControllerConstants.RevolverFireClipPath,
                requireLoopTime: false);
            ValidateRequiredRevolverClipAsset(
                failures,
                CCS_CharacterControllerConstants.RevolverReloadClipPath,
                requireLoopTime: false);

            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.RevolverUpperBodyMaskPath),
                "Missing revolver upper-body mask at "
                + CCS_CharacterControllerConstants.RevolverUpperBodyMaskPath
                + ".");

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            AppendIfMissing(failures, controller != null, "Player Animator Controller is missing.");
            if (controller == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            AppendIfMissing(
                failures,
                HasAnimatorParameter(controller, CCS_CharacterControllerConstants.AnimatorRevolverAimHeldParameter),
                "Player Animator Controller must define RevolverAimHeld.");
            AppendIfMissing(
                failures,
                HasAnimatorParameter(controller, CCS_CharacterControllerConstants.AnimatorRevolverFireTriggerParameter),
                "Player Animator Controller must define RevolverFireTrigger.");
            AppendIfMissing(
                failures,
                HasAnimatorParameter(controller, CCS_CharacterControllerConstants.AnimatorRevolverReloadTriggerParameter),
                "Player Animator Controller must define RevolverReloadTrigger.");
            AppendIfMissing(
                failures,
                HasAnimatorParameter(controller, CCS_CharacterControllerConstants.AnimatorRevolverIsReloadingParameter),
                "Player Animator Controller must define RevolverIsReloading.");

            int layerIndex = FindLayerIndex(controller, CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName);
            AppendIfMissing(
                failures,
                layerIndex >= 0,
                "Player Animator Controller must define RevolverUpperBody layer.");

            if (layerIndex < 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            AnimatorControllerLayer layer = controller.layers[layerIndex];
            AvatarMask expectedMask = AssetDatabase.LoadAssetAtPath<AvatarMask>(
                CCS_CharacterControllerConstants.RevolverUpperBodyMaskPath);
            AppendIfMissing(
                failures,
                layer.avatarMask == expectedMask,
                "RevolverUpperBody layer must use CCS_Revolver_UpperBody.mask.");

            AnimatorStateMachine stateMachine = layer.stateMachine;
            ValidateRevolverStateClip(
                failures,
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorRevolverEmptyStateName,
                expectedClipPath: null);
            ValidateRevolverStateClip(
                failures,
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorRevolverAimIdleStateName,
                CCS_CharacterControllerConstants.RevolverAimIdleClipPath);
            ValidateRevolverStateClip(
                failures,
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorRevolverFireStateName,
                CCS_CharacterControllerConstants.RevolverFireClipPath);
            ValidateRevolverStateClip(
                failures,
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorRevolverReloadStateName,
                CCS_CharacterControllerConstants.RevolverReloadClipPath);

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Revolver upper-body animation isolation validated.");
        }

        public static CCS_SurvivalValidationResult ValidateNoInvectorRuntimeReferences()
        {
            List<string> failures = new List<string>();

            AppendIfMissing(
                failures,
                !Directory.Exists("Assets/Invector-3rdPersonController"),
                "Legacy Invector import folder must be removed from Assets/Invector-3rdPersonController.");

            string[] runtimeAssetGuids = AssetDatabase.FindAssets(
                "t:AnimatorController t:Prefab t:Scene",
                new[] { "Assets/CCS" });
            for (int i = 0; i < runtimeAssetGuids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(runtimeAssetGuids[i]);
                if (string.IsNullOrEmpty(assetPath))
                {
                    continue;
                }

                if (assetPath.StartsWith("Assets/VendorSource/"))
                {
                    continue;
                }

                string assetText = File.ReadAllText(assetPath);
                if (assetText.Contains("Invector-3rdPersonController"))
                {
                    failures.Add("Runtime CCS asset references legacy Invector path: " + assetPath);
                }
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("No runtime references to legacy Invector assets.");
        }

        public static CCS_SurvivalValidationResult ValidatePlayerRevolverUpperBodyAnimator(GameObject prefabRoot)
        {
            if (prefabRoot == null)
            {
                return CCS_SurvivalValidationResult.Fail("Player prefab reference is null.");
            }

            CCS_RevolverUpperBodyAnimator upperBodyAnimator =
                prefabRoot.GetComponentInChildren<CCS_RevolverUpperBodyAnimator>(true);
            if (upperBodyAnimator == null)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Player prefab must contain CCS_RevolverUpperBodyAnimator on VisualRoot.");
            }

            return CCS_SurvivalValidationResult.Pass("Player prefab revolver upper-body animator validated.");
        }

        public static CCS_SurvivalValidationResult ValidatePlayerAnimatorRootMotionDisabled(GameObject prefabRoot)
        {
            if (prefabRoot == null)
            {
                return CCS_SurvivalValidationResult.Fail("Player prefab reference is null.");
            }

            Animator[] animators = prefabRoot.GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animators.Length; i++)
            {
                Animator animator = animators[i];
                if (animator == null || !animator.isActiveAndEnabled && animator.runtimeAnimatorController == null)
                {
                    continue;
                }

                if (animator.applyRootMotion)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        "Player Animator must have Apply Root Motion disabled on "
                        + animator.gameObject.name
                        + ".");
                }
            }

            return CCS_SurvivalValidationResult.Pass("Player Animator root motion is disabled.");
        }

        #endregion

        #region Private Methods

        private static void ValidateRequiredRevolverClipAsset(
            List<string> failures,
            string clipAssetPath,
            bool requireLoopTime)
        {
            AppendIfMissing(
                failures,
                File.Exists(clipAssetPath),
                "Missing required Revolver clip asset: " + clipAssetPath + ".");

            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipAssetPath);
            if (clip == null)
            {
                return;
            }

            string normalizedPath = NormalizeAssetPath(AssetDatabase.GetAssetPath(clip));
            AppendIfMissing(
                failures,
                normalizedPath.StartsWith(NormalizeAssetPath(CCS_CharacterControllerConstants.ContentAnimationsRootPath)),
                "Revolver clip must live under Assets/CCS/: " + normalizedPath);
            AppendIfMissing(
                failures,
                !IsVendorFbxSubAssetPath(normalizedPath),
                "Revolver clip must not be a vendor FBX sub-asset: " + normalizedPath);

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            AppendIfMissing(
                failures,
                settings.loopTime == requireLoopTime,
                "Revolver clip loop setting invalid for " + clipAssetPath + ".");
        }

        private static void ValidateRevolverStateClip(
            List<string> failures,
            AnimatorStateMachine stateMachine,
            string stateName,
            string expectedClipPath)
        {
            AnimatorState state = FindState(stateMachine, stateName);
            AppendIfMissing(
                failures,
                state != null,
                "RevolverUpperBody layer must define state " + stateName + ".");

            if (state == null)
            {
                return;
            }

            if (expectedClipPath == null)
            {
                AppendIfMissing(
                    failures,
                    state.motion == null,
                    stateName + " must have no motion clip assigned.");
                return;
            }

            AppendIfMissing(
                failures,
                state.motion is AnimationClip,
                stateName + " must reference a CCS-owned animation clip.");

            if (state.motion is AnimationClip clip)
            {
                string clipPath = NormalizeAssetPath(AssetDatabase.GetAssetPath(clip));
                AppendIfMissing(
                    failures,
                    clipPath == NormalizeAssetPath(expectedClipPath),
                    stateName + " must reference " + expectedClipPath + " (found " + clipPath + ").");
                AppendIfMissing(
                    failures,
                    !IsVendorFbxSubAssetPath(clipPath),
                    stateName + " must not reference vendor FBX sub-asset: " + clipPath);
            }
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

        private static void ValidateRequiredAimStrafeClipAsset(
            List<string> failures,
            string clipAssetPath,
            bool requireLoopTime)
        {
            AppendIfMissing(
                failures,
                File.Exists(clipAssetPath),
                "Missing required AimStrafe clip asset: " + clipAssetPath + ".");

            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipAssetPath);
            if (clip == null)
            {
                return;
            }

            string normalizedPath = NormalizeAssetPath(AssetDatabase.GetAssetPath(clip));
            AppendIfMissing(
                failures,
                normalizedPath.StartsWith(NormalizeAssetPath(CCS_CharacterControllerConstants.ContentAnimationsRootPath)),
                "AimStrafe clip must live under Assets/CCS/: " + normalizedPath);
            AppendIfMissing(
                failures,
                !IsVendorFbxSubAssetPath(normalizedPath),
                "AimStrafe clip must not be a vendor FBX sub-asset: " + normalizedPath);

            if (requireLoopTime)
            {
                AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                AppendIfMissing(
                    failures,
                    settings.loopTime,
                    "AimStrafe clip must be loopable: " + clipAssetPath);
            }
        }

        private static void ValidateBlendTreeMotionAt(
            List<string> failures,
            BlendTree blendTree,
            Vector2 expectedPosition,
            string expectedClipPath)
        {
            ChildMotion[] children = blendTree.children;
            for (int i = 0; i < children.Length; i++)
            {
                if (Vector2.Distance(children[i].position, expectedPosition) > 0.001f)
                {
                    continue;
                }

                if (children[i].motion is AnimationClip clip)
                {
                    string clipPath = NormalizeAssetPath(AssetDatabase.GetAssetPath(clip));
                    if (clipPath == NormalizeAssetPath(expectedClipPath))
                    {
                        return;
                    }

                    failures.Add(
                        "AimStrafe blend point "
                        + expectedPosition
                        + " must reference "
                        + expectedClipPath
                        + " (found "
                        + clipPath
                        + ").");
                    return;
                }
            }

            failures.Add(
                "AimStrafe blend tree missing motion at "
                + expectedPosition
                + " for "
                + expectedClipPath
                + ".");
        }

        private static bool IsVendorFbxSubAssetPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return false;
            }

            string normalized = NormalizeAssetPath(assetPath);
            if (!normalized.StartsWith("Assets/CCS/"))
            {
                return normalized.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)
                    || normalized.Contains(".fbx/");
            }

            return false;
        }

        private static AnimatorState FindState(AnimatorStateMachine stateMachine, string stateName)
        {
            ChildAnimatorState[] states = stateMachine.states;
            for (int i = 0; i < states.Length; i++)
            {
                if (states[i].state != null && states[i].state.name == stateName)
                {
                    return states[i].state;
                }
            }

            return null;
        }

        private static bool HasAnimatorParameter(AnimatorController controller, string parameterName)
        {
            for (int i = 0; i < controller.parameters.Length; i++)
            {
                if (controller.parameters[i].name == parameterName)
                {
                    return true;
                }
            }

            return false;
        }

        private static void CollectAnimationClips(
            AnimatorStateMachine stateMachine,
            HashSet<Motion> visitedMotions,
            List<AnimationClip> clips)
        {
            ChildAnimatorState[] states = stateMachine.states;
            for (int i = 0; i < states.Length; i++)
            {
                CollectMotionClips(states[i].state != null ? states[i].state.motion : null, visitedMotions, clips);
            }

            ChildAnimatorStateMachine[] childStateMachines = stateMachine.stateMachines;
            for (int i = 0; i < childStateMachines.Length; i++)
            {
                if (childStateMachines[i].stateMachine != null)
                {
                    CollectAnimationClips(childStateMachines[i].stateMachine, visitedMotions, clips);
                }
            }
        }

        private static void CollectMotionClips(Motion motion, HashSet<Motion> visitedMotions, List<AnimationClip> clips)
        {
            if (motion == null || !visitedMotions.Add(motion))
            {
                return;
            }

            if (motion is AnimationClip clip)
            {
                clips.Add(clip);
                return;
            }

            if (motion is BlendTree blendTree)
            {
                ChildMotion[] children = blendTree.children;
                for (int i = 0; i < children.Length; i++)
                {
                    CollectMotionClips(children[i].motion, visitedMotions, clips);
                }
            }
        }

        private static string NormalizeAssetPath(string assetPath)
        {
            return assetPath.Replace('\\', '/');
        }

        private static void AppendIfMissing(List<string> failures, bool condition, string message)
        {
            if (!condition)
            {
                failures.Add(message);
            }
        }

        #endregion
    }
}
