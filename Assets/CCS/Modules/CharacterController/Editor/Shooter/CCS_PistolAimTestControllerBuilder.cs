using System;
using System.IO;
using System.Linq;
using CCS.Modules.CharacterController;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PistolAimTestControllerBuilder
// CATEGORY: Modules / CharacterController / Editor / Shooter
// PURPOSE: Creates test-only CCS pistol aim Animator Controller scaffold.
// PLACEMENT: Editor builder. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_PistolAimTestControllerBuilder
    {
        public static string BuildOrUpdateTestController()
        {
            string controllerPath = CCS_CharacterControllerConstants.PistolAimTestControllerPath;
            string directory = Path.GetDirectoryName(controllerPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            }

            EnsureParameters(controller);
            EnsureLayers(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "[Pistol Aim Reference] Test controller ready at "
                + controllerPath
                + ". Assign "
                + CCS_CharacterControllerConstants.PistolTwoHandedAimHoldClipPath
                + " after licensed clip duplicate.");
            return controllerPath;
        }

        private static void EnsureParameters(AnimatorController controller)
        {
            AddParameterIfMissing(controller, "IsAiming", AnimatorControllerParameterType.Bool);
            AddParameterIfMissing(controller, "PistolDrawTrigger", AnimatorControllerParameterType.Trigger);
            AddParameterIfMissing(controller, "PistolHolsterTrigger", AnimatorControllerParameterType.Trigger);
        }

        private static void EnsureLayers(AnimatorController controller)
        {
            AnimatorControllerLayer baseLayer = controller.layers[0];
            if (baseLayer.name != "Base Locomotion")
            {
                baseLayer.name = "Base Locomotion";
                controller.layers[0] = baseLayer;
            }

            AnimatorControllerLayer upperBodyLayer = FindOrCreateLayer(controller, "PistolUpperBodyTest");
            AvatarMask upperBodyMask = AssetDatabase.LoadAssetAtPath<AvatarMask>(
                "Assets/CCS/Modules/CharacterController/Content/Animations/Masks/AM_CCS_Revolver_UpperBodyLeftArm_Aim.mask");

            upperBodyLayer.defaultWeight = 1f;
            upperBodyLayer.avatarMask = upperBodyMask;
            upperBodyLayer.blendingMode = AnimatorLayerBlendingMode.Override;

            AnimatorStateMachine stateMachine = upperBodyLayer.stateMachine;
            if (stateMachine == null)
            {
                stateMachine = new AnimatorStateMachine();
                AssetDatabase.AddObjectToAsset(stateMachine, controller);
                upperBodyLayer.stateMachine = stateMachine;
            }

            AnimatorState emptyState = FindOrCreateState(stateMachine, "Empty", new Vector3(300f, 0f, 0f));
            AnimatorState aimHoldState = FindOrCreateState(stateMachine, "AimHold_Test", new Vector3(300f, 120f, 0f));
            AnimatorState drawState = FindOrCreateState(stateMachine, "Draw_Test", new Vector3(100f, 120f, 0f));
            AnimatorState holsterState = FindOrCreateState(stateMachine, "Holster_Test", new Vector3(500f, 120f, 0f));

            AnimationClip aimHoldClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                CCS_CharacterControllerConstants.PistolTwoHandedAimHoldClipPath);
            if (aimHoldClip != null)
            {
                aimHoldState.motion = aimHoldClip;
            }

            stateMachine.defaultState = emptyState;

            AddBoolTransition(emptyState, aimHoldState, "IsAiming", true);
            AddBoolTransition(aimHoldState, emptyState, "IsAiming", false);
            AddTriggerTransition(emptyState, drawState, "PistolDrawTrigger");
            AddBoolTransition(drawState, aimHoldState, "IsAiming", true);
            AddTriggerTransition(aimHoldState, holsterState, "PistolHolsterTrigger");
            AddBoolTransition(holsterState, emptyState, "IsAiming", false);

            SetLayer(controller, upperBodyLayer);
        }

        private static AnimatorControllerLayer FindOrCreateLayer(AnimatorController controller, string layerName)
        {
            for (int i = 0; i < controller.layers.Length; i++)
            {
                if (controller.layers[i].name == layerName)
                {
                    return controller.layers[i];
                }
            }

            AnimatorControllerLayer[] layers = controller.layers;
            Array.Resize(ref layers, layers.Length + 1);
            layers[layers.Length - 1] = new AnimatorControllerLayer
            {
                name = layerName,
                stateMachine = new AnimatorStateMachine(),
            };
            controller.layers = layers;
            AssetDatabase.AddObjectToAsset(layers[layers.Length - 1].stateMachine, controller);
            return layers[layers.Length - 1];
        }

        private static void SetLayer(AnimatorController controller, AnimatorControllerLayer layer)
        {
            for (int i = 0; i < controller.layers.Length; i++)
            {
                if (controller.layers[i].name == layer.name)
                {
                    controller.layers[i] = layer;
                    return;
                }
            }
        }

        private static AnimatorState FindOrCreateState(
            AnimatorStateMachine stateMachine,
            string stateName,
            Vector3 position)
        {
            ChildAnimatorState[] states = stateMachine.states;
            for (int i = 0; i < states.Length; i++)
            {
                if (states[i].state.name == stateName)
                {
                    return states[i].state;
                }
            }

            return stateMachine.AddState(stateName, position);
        }

        private static void AddParameterIfMissing(
            AnimatorController controller,
            string name,
            AnimatorControllerParameterType type)
        {
            for (int i = 0; i < controller.parameters.Length; i++)
            {
                if (controller.parameters[i].name == name)
                {
                    return;
                }
            }

            controller.AddParameter(name, type);
        }

        private static void AddBoolTransition(
            AnimatorState source,
            AnimatorState destination,
            string boolParameter,
            bool expectedValue)
        {
            RemoveExistingTransitions(source, destination);
            AnimatorStateTransition transition = source.AddTransition(destination);
            transition.hasExitTime = false;
            transition.duration = 0.15f;
            transition.AddCondition(
                expectedValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
                0f,
                boolParameter);
        }

        private static void AddTriggerTransition(
            AnimatorState source,
            AnimatorState destination,
            string triggerParameter)
        {
            RemoveExistingTransitions(source, destination);
            AnimatorStateTransition transition = source.AddTransition(destination);
            transition.hasExitTime = false;
            transition.duration = 0.1f;
            transition.AddCondition(AnimatorConditionMode.If, 0f, triggerParameter);
        }

        private static void RemoveExistingTransitions(AnimatorState source, AnimatorState destination)
        {
            AnimatorStateTransition[] transitions = source.transitions;
            for (int i = transitions.Length - 1; i >= 0; i--)
            {
                if (transitions[i].destinationState == destination)
                {
                    source.RemoveTransition(transitions[i]);
                }
            }
        }
    }
}
