using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Local;
using CCS.Project;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SingleRevolverAimLayerValidationUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Validates v0.7.8 single-revolver upper-body aim Animator layer and wiring.
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-29
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_SingleRevolverAimLayerValidationUtility
    {
        private const string EquipmentFitStudioWindowPath =
            "Assets/CCS/Modules/CharacterController/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioWindow.cs";

        private const string CharacterControllerTestsRoot =
            "Assets/CCS/Modules/CharacterController/Tests";

        private const string AnimationFitStudioRoot =
            "Assets/CCS/Modules/CharacterController/Editor/AnimationFitStudio";

        private static readonly string[] ForbiddenPresentationParameters =
        {
            CCS_CharacterAnimationParameterIds.FutureDesignOnly.FireTrigger,
            CCS_CharacterAnimationParameterIds.FutureDesignOnly.ReloadTrigger,
            CCS_CharacterAnimationParameterIds.FutureDesignOnly.InteractionTrigger,
            CCS_CharacterAnimationParameterIds.FutureDesignOnly.InteractionType,
            CCS_CharacterControllerConstants.AnimatorRevolverFireTriggerParameter,
            CCS_CharacterControllerConstants.AnimatorRevolverReloadTriggerParameter,
            CCS_CharacterControllerConstants.AnimatorRevolverAimHeldParameter,
            CCS_CharacterControllerConstants.AnimatorRevolverAimPitchParameter,
            "WeaponMode",
            "AimPitch",
            "AimYaw",
        };

        public static CCS_SurvivalValidationResult ValidateSingleRevolverAimLayer()
        {
            List<string> failures = new List<string>();
            List<string> warnings = new List<string>();

            ValidateRequiredAssets(failures);
            ValidateAnimatorController(failures);
            ValidatePlayerPrefab(failures);
            ValidateTestsFolderRemoved(failures);
            ValidateAnimationFitStudioNotPresent(failures);
            ValidateEquipmentFitStudioPresent(failures);
            CollectDeferredWarnings(warnings);

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            string message = "Single revolver aim upper-body layer validated.";
            if (warnings.Count > 0)
            {
                message += " Warnings: " + string.Join(" ", warnings);
            }

            return CCS_SurvivalValidationResult.Pass(message);
        }

        private static void ValidateRequiredAssets(List<string> failures)
        {
            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.RevolverAimRightArmMaskPath),
                "Missing Avatar mask at " + CCS_CharacterControllerConstants.RevolverAimRightArmMaskPath);
            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.WildWestIdleFulldrawRevolverClipPath),
                "Missing draw clip at " + CCS_CharacterControllerConstants.WildWestIdleFulldrawRevolverClipPath);
            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.WildWestFulldrawIdleClipPath),
                "Missing hold clip at " + CCS_CharacterControllerConstants.WildWestFulldrawIdleClipPath);
            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.WildWestIdleFullHolsterRevolverClipPath),
                "Missing holster clip at " + CCS_CharacterControllerConstants.WildWestIdleFullHolsterRevolverClipPath);
        }

        private static void ValidateAnimatorController(List<string> failures)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            AppendIfMissing(failures, controller != null, "Missing player Animator Controller.");

            if (controller == null)
            {
                return;
            }

            int aimLayerIndex = FindLayerIndex(controller, CCS_CharacterControllerConstants.SingleRevolverUpperBodyLayerName);
            AppendIfMissing(
                failures,
                aimLayerIndex == 1,
                "Animator Controller must define SingleRevolverUpperBody at layer index 1.");
            AppendIfMissing(
                failures,
                controller.layers.Length == 2,
                "Animator Controller must contain exactly two layers: Base Layer and SingleRevolverUpperBody.");

            AppendValidationFailures(
                failures,
                CCS_CharacterControllerPhase3BValidationUtility.ValidatePhase3BLocomotionOnlyAnimatorReset());

            for (int i = 0; i < ForbiddenPresentationParameters.Length; i++)
            {
                AppendIfMissing(
                    failures,
                    !HasAnimatorParameter(controller, ForbiddenPresentationParameters[i]),
                    "Animator Controller must not define unauthorized parameter "
                    + ForbiddenPresentationParameters[i]
                    + ".");
            }

            for (int i = 0; i < CCS_CharacterControllerConstants.SingleRevolverAimPresentationParameterNames.Length; i++)
            {
                string parameterName = CCS_CharacterControllerConstants.SingleRevolverAimPresentationParameterNames[i];
                AppendIfMissing(
                    failures,
                    HasAnimatorParameter(controller, parameterName),
                    "Animator Controller missing required parameter " + parameterName + ".");
            }

            if (aimLayerIndex < 0)
            {
                return;
            }

            AnimatorControllerLayer aimLayer = controller.layers[aimLayerIndex];
            AvatarMask expectedMask = AssetDatabase.LoadAssetAtPath<AvatarMask>(
                CCS_CharacterControllerConstants.RevolverAimRightArmMaskPath);
            AppendIfMissing(
                failures,
                aimLayer.avatarMask == expectedMask,
                "SingleRevolverUpperBody must use AM_CCS_Revolver_UpperBodyRightArm_Aim mask.");

            AnimatorStateMachine stateMachine = aimLayer.stateMachine;
            AppendIfMissing(
                failures,
                stateMachine != null && stateMachine.states.Length == 4,
                "SingleRevolverUpperBody must contain exactly four states.");

            if (stateMachine == null)
            {
                return;
            }

            HashSet<string> allowedStates = new HashSet<string>(CCS_CharacterControllerConstants.SingleRevolverUpperBodyAllowedStateNames);
            for (int stateIndex = 0; stateIndex < stateMachine.states.Length; stateIndex++)
            {
                string stateName = stateMachine.states[stateIndex].state.name;
                if (!allowedStates.Contains(stateName))
                {
                    failures.Add("SingleRevolverUpperBody contains unauthorized state " + stateName + ".");
                }
            }

            AppendIfMissing(
                failures,
                stateMachine.defaultState != null
                && stateMachine.defaultState.name == CCS_CharacterControllerConstants.SingleRevolverUpperBodyEmptyStateName,
                "SingleRevolverUpperBody default state must be UpperBody_Empty.");

            ValidateStateClip(
                failures,
                stateMachine,
                CCS_CharacterControllerConstants.SingleRevolverDrawStateName,
                CCS_CharacterControllerConstants.WildWestIdleFulldrawRevolverClipName);
            ValidateStateClip(
                failures,
                stateMachine,
                CCS_CharacterControllerConstants.SingleRevolverAimHoldStateName,
                CCS_CharacterControllerConstants.WildWestFulldrawIdleClipName);
            ValidateStateClip(
                failures,
                stateMachine,
                CCS_CharacterControllerConstants.SingleRevolverHolsterStateName,
                CCS_CharacterControllerConstants.WildWestIdleFullHolsterRevolverClipName);
        }

        private static void ValidatePlayerPrefab(List<string> failures)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            AppendIfMissing(failures, prefab != null, "Missing networked player prefab.");

            if (prefab == null)
            {
                return;
            }

            Transform modelRoot = CCS_PlayerModelRootUtility.FindModelRoot(prefab.transform);
            AppendIfMissing(failures, modelRoot != null, "Player prefab missing Model root.");

            CCS_SingleRevolverAimAnimator aimAnimator = prefab.GetComponentInChildren<CCS_SingleRevolverAimAnimator>(true);
            AppendIfMissing(
                failures,
                aimAnimator != null,
                "Player prefab missing CCS_SingleRevolverAimAnimator.");
            AppendIfMissing(
                failures,
                aimAnimator == null || aimAnimator.transform == modelRoot,
                "CCS_SingleRevolverAimAnimator must be on Model root, not player root.");

            Animator animator = prefab.GetComponentInChildren<Animator>(true);
            AppendIfMissing(failures, animator != null, "Player prefab missing Kevin Animator.");

            AppendValidationFailures(
                failures,
                CCS_CharacterControllerAnimationValidationUtility.ValidatePlayerRevolverUpperBodyAnimator(prefab));
        }

        private static void ValidateStateClip(
            List<string> failures,
            AnimatorStateMachine stateMachine,
            string stateName,
            string expectedClipName)
        {
            AnimatorState state = FindState(stateMachine, stateName);
            AppendIfMissing(failures, state != null, "Missing state " + stateName + " on SingleRevolverUpperBody.");

            if (state == null || state.motion == null)
            {
                if (stateName != CCS_CharacterControllerConstants.SingleRevolverUpperBodyEmptyStateName)
                {
                    failures.Add("State " + stateName + " must reference clip " + expectedClipName + ".");
                }

                return;
            }

            AnimationClip clip = state.motion as AnimationClip;
            AppendIfMissing(
                failures,
                clip != null && clip.name == expectedClipName,
                "State " + stateName + " must use clip " + expectedClipName + ".");
        }

        private static void CollectDeferredWarnings(List<string> warnings)
        {
            warnings.Add("Remote player aim presentation is not implemented in v0.7.8; local owner drives presentation only.");
            warnings.Add("Humanoid generic clip binding warnings may remain until animation rebuild milestone.");
            warnings.Add("EnemyAI does not use the single-revolver aim layer in v0.7.8.");
        }

        private static void ValidateTestsFolderRemoved(List<string> failures)
        {
            AppendIfMissing(
                failures,
                !Directory.Exists(CharacterControllerTestsRoot),
                "CharacterController/Tests folder must remain removed.");
        }

        private static void ValidateAnimationFitStudioNotPresent(List<string> failures)
        {
            AppendIfMissing(
                failures,
                !Directory.Exists(AnimationFitStudioRoot),
                "Animation Fit Studio editor folder must not be reintroduced.");
        }

        private static void ValidateEquipmentFitStudioPresent(List<string> failures)
        {
            AppendIfMissing(
                failures,
                File.Exists(EquipmentFitStudioWindowPath),
                "Equipment Fit Studio window must remain present.");
        }

        private static void AppendValidationFailures(List<string> failures, CCS_SurvivalValidationResult result)
        {
            if (!result.IsSuccess)
            {
                failures.Add(result.Message);
            }
        }

        private static void AppendIfMissing(List<string> failures, bool condition, string message)
        {
            if (!condition)
            {
                failures.Add(message);
            }
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
    }
}
