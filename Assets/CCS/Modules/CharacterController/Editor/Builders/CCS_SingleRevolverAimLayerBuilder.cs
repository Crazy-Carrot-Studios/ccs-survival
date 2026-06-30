using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Local;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SingleRevolverAimLayerBuilder
// CATEGORY: Modules / CharacterController / Editor / Builders
// PURPOSE: Adds SingleRevolverUpperBody masked layer to the player Animator Controller.
// PLACEMENT: Editor builder invoked before aim layer validation and prefab wiring.
// AUTHOR: James Schilz
// CREATED: 2026-06-29
// NOTES: Preserves Base Layer locomotion. Uses Wild West FBX sub-asset clips read-only.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_SingleRevolverAimLayerBuilder
    {
        private const float AimTransitionDuration = 0.08f;

        private const float DrawToHoldExitTime = 0.95f;

        private const float HolsterToEmptyExitTime = 0.95f;

        public static bool EnsureSingleRevolverAimLayer()
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            if (controller == null)
            {
                Debug.LogError(
                    "[Single Revolver Aim Layer Builder] Missing Animator Controller at "
                    + CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
                return false;
            }

            AvatarMask mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(
                CCS_CharacterControllerConstants.RevolverAimRightArmMaskPath);
            if (mask == null)
            {
                Debug.LogError("[Single Revolver Aim Layer Builder] Missing revolver aim Avatar Mask.");
                return false;
            }

            AnimationClip drawClip = LoadWildWestClip(
                CCS_CharacterControllerConstants.WildWestIdleFulldrawRevolverClipPath,
                CCS_CharacterControllerConstants.WildWestIdleFulldrawRevolverClipName);
            AnimationClip holdClip = LoadWildWestClip(
                CCS_CharacterControllerConstants.WildWestFulldrawIdleClipPath,
                CCS_CharacterControllerConstants.WildWestFulldrawIdleClipName);
            AnimationClip holsterClip = LoadWildWestClip(
                CCS_CharacterControllerConstants.WildWestIdleFullHolsterRevolverClipPath,
                CCS_CharacterControllerConstants.WildWestIdleFullHolsterRevolverClipName);

            if (drawClip == null || holdClip == null || holsterClip == null)
            {
                Debug.LogError("[Single Revolver Aim Layer Builder] Missing required Wild West aim clips.");
                return false;
            }

            bool changed = false;
            changed |= EnsurePresentationParameters(controller);
            changed |= EnsureSingleRevolverUpperBodyLayer(controller, mask, drawClip, holdClip, holsterClip);

            if (changed)
            {
                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssets();
            }

            return true;
        }

        public static bool EnsureSingleRevolverAimAnimatorOnNetworkedPlayerPrefab()
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError("[Single Revolver Aim Layer Builder] Missing networked player prefab.");
                return false;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefabRoot) as GameObject;
            if (instance == null)
            {
                return false;
            }

            bool changed = false;
            try
            {
                Transform modelRoot = CCS_PlayerModelRootUtility.FindModelRoot(instance.transform);
                if (modelRoot == null)
                {
                    Debug.LogError("[Single Revolver Aim Layer Builder] Missing Model root on player prefab.");
                    return false;
                }

                CCS_SingleRevolverAimAnimator aimAnimator = modelRoot.GetComponent<CCS_SingleRevolverAimAnimator>();
                if (aimAnimator == null)
                {
                    aimAnimator = modelRoot.gameObject.AddComponent<CCS_SingleRevolverAimAnimator>();
                    changed = true;
                }

                changed |= WireSingleRevolverAimAnimator(modelRoot, aimAnimator);
                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(instance, CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
                }
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }

            return changed;
        }

        private static bool WireSingleRevolverAimAnimator(Transform modelRoot, CCS_SingleRevolverAimAnimator aimAnimator)
        {
            SerializedObject serializedAnimator = new SerializedObject(aimAnimator);
            SerializedProperty animatorProperty = serializedAnimator.FindProperty("animator");
            SerializedProperty revolverStateProperty = serializedAnimator.FindProperty("revolverAnimationStateComponent");
            SerializedProperty layerNameProperty = serializedAnimator.FindProperty("upperBodyLayerName");

            Animator resolvedAnimator = modelRoot.GetComponentInChildren<Animator>(true);
            Component revolverState = null;
            MonoBehaviour[] behaviours = modelRoot.root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is CCS_IRevolverAnimationState)
                {
                    revolverState = behaviours[i];
                    break;
                }
            }

            bool changed = false;
            if (animatorProperty != null && animatorProperty.objectReferenceValue != resolvedAnimator)
            {
                animatorProperty.objectReferenceValue = resolvedAnimator;
                changed = true;
            }

            if (revolverStateProperty != null && revolverStateProperty.objectReferenceValue != revolverState)
            {
                revolverStateProperty.objectReferenceValue = revolverState;
                changed = true;
            }

            if (layerNameProperty != null
                && layerNameProperty.stringValue != CCS_CharacterControllerConstants.SingleRevolverUpperBodyLayerName)
            {
                layerNameProperty.stringValue = CCS_CharacterControllerConstants.SingleRevolverUpperBodyLayerName;
                changed = true;
            }

            if (changed)
            {
                serializedAnimator.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool EnsurePresentationParameters(AnimatorController controller)
        {
            bool changed = false;
            changed |= EnsureAnimatorParameter(
                controller,
                CCS_CharacterAnimationParameterIds.Active.IsAiming,
                AnimatorControllerParameterType.Bool);
            changed |= EnsureAnimatorParameter(
                controller,
                CCS_CharacterAnimationParameterIds.Active.RevolverDrawTrigger,
                AnimatorControllerParameterType.Trigger);
            changed |= EnsureAnimatorParameter(
                controller,
                CCS_CharacterAnimationParameterIds.Active.RevolverHolsterTrigger,
                AnimatorControllerParameterType.Trigger);
            return changed;
        }

        private static bool EnsureSingleRevolverUpperBodyLayer(
            AnimatorController controller,
            AvatarMask mask,
            AnimationClip drawClip,
            AnimationClip holdClip,
            AnimationClip holsterClip)
        {
            bool changed = false;
            int layerIndex = FindLayerIndex(controller, CCS_CharacterControllerConstants.SingleRevolverUpperBodyLayerName);
            if (layerIndex < 0)
            {
                controller.AddLayer(CCS_CharacterControllerConstants.SingleRevolverUpperBodyLayerName);
                layerIndex = controller.layers.Length - 1;
                changed = true;
            }

            AnimatorControllerLayer layer = controller.layers[layerIndex];
            layer.defaultWeight = 0f;
            layer.blendingMode = AnimatorLayerBlendingMode.Override;
            layer.iKPass = false;
            controller.layers[layerIndex] = layer;

            SerializedObject serializedController = new SerializedObject(controller);
            SerializedProperty layersProperty = serializedController.FindProperty("m_AnimatorLayers");
            if (layersProperty != null && layerIndex < layersProperty.arraySize)
            {
                SerializedProperty layerProperty = layersProperty.GetArrayElementAtIndex(layerIndex);
                SerializedProperty maskProperty = layerProperty.FindPropertyRelative("m_Mask");
                if (maskProperty != null && maskProperty.objectReferenceValue != mask)
                {
                    maskProperty.objectReferenceValue = mask;
                    changed = true;
                }
            }

            serializedController.ApplyModifiedPropertiesWithoutUndo();
            layer = controller.layers[layerIndex];

            AnimatorStateMachine stateMachine = layer.stateMachine;
            if (stateMachine == null)
            {
                return changed;
            }

            changed |= RebuildSingleRevolverStateMachine(stateMachine, drawClip, holdClip, holsterClip);
            if (changed)
            {
                EditorUtility.SetDirty(stateMachine);
                EditorUtility.SetDirty(controller);
            }

            return changed;
        }

        private static bool RebuildSingleRevolverStateMachine(
            AnimatorStateMachine stateMachine,
            AnimationClip drawClip,
            AnimationClip holdClip,
            AnimationClip holsterClip)
        {
            ClearStateMachine(stateMachine);

            bool changed = true;
            AnimatorState emptyState = CreateState(
                stateMachine,
                CCS_CharacterControllerConstants.SingleRevolverUpperBodyEmptyStateName,
                null,
                new Vector3(250f, 0f, 0f));
            AnimatorState drawState = CreateState(
                stateMachine,
                CCS_CharacterControllerConstants.SingleRevolverDrawStateName,
                drawClip,
                new Vector3(250f, 120f, 0f));
            AnimatorState holdState = CreateState(
                stateMachine,
                CCS_CharacterControllerConstants.SingleRevolverAimHoldStateName,
                holdClip,
                new Vector3(250f, 240f, 0f));
            AnimatorState holsterState = CreateState(
                stateMachine,
                CCS_CharacterControllerConstants.SingleRevolverHolsterStateName,
                holsterClip,
                new Vector3(250f, 360f, 0f));

            drawState.writeDefaultValues = true;
            holdState.writeDefaultValues = true;
            holsterState.writeDefaultValues = true;

            stateMachine.defaultState = emptyState;

            AddTriggerTransition(
                emptyState,
                drawState,
                CCS_CharacterAnimationParameterIds.Active.RevolverDrawTrigger,
                AimTransitionDuration);
            AddExitTimeTransition(
                drawState,
                holdState,
                AimTransitionDuration,
                DrawToHoldExitTime,
                CCS_CharacterAnimationParameterIds.Active.IsAiming,
                expectedTrue: true);
            AddBoolTransition(
                drawState,
                holsterState,
                CCS_CharacterAnimationParameterIds.Active.IsAiming,
                expectedTrue: false,
                AimTransitionDuration);
            AddBoolOrTriggerTransition(
                holdState,
                holsterState,
                CCS_CharacterAnimationParameterIds.Active.IsAiming,
                CCS_CharacterAnimationParameterIds.Active.RevolverHolsterTrigger,
                AimTransitionDuration);
            AddExitTimeTransition(
                holsterState,
                emptyState,
                AimTransitionDuration,
                HolsterToEmptyExitTime);

            return changed;
        }

        private static AnimationClip LoadWildWestClip(string assetPath, string clipName)
        {
            if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath))
            {
                return null;
            }

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is AnimationClip clip && clip.name == clipName && !clip.name.StartsWith("__preview__"))
                {
                    return clip;
                }
            }

            return null;
        }

        private static bool EnsureAnimatorParameter(
            AnimatorController controller,
            string parameterName,
            AnimatorControllerParameterType parameterType)
        {
            for (int i = 0; i < controller.parameters.Length; i++)
            {
                if (controller.parameters[i].name == parameterName)
                {
                    return false;
                }
            }

            controller.AddParameter(parameterName, parameterType);
            return true;
        }

        private static AnimatorState CreateState(
            AnimatorStateMachine stateMachine,
            string stateName,
            Motion motion,
            Vector3 position)
        {
            AnimatorState state = stateMachine.AddState(stateName, position);
            state.motion = motion;
            return state;
        }

        private static void ClearStateMachine(AnimatorStateMachine stateMachine)
        {
            ChildAnimatorState[] childStates = stateMachine.states;
            for (int i = childStates.Length - 1; i >= 0; i--)
            {
                stateMachine.RemoveState(childStates[i].state);
            }

            AnimatorStateTransition[] anyStateTransitions = stateMachine.anyStateTransitions;
            for (int i = anyStateTransitions.Length - 1; i >= 0; i--)
            {
                stateMachine.RemoveAnyStateTransition(anyStateTransitions[i]);
            }
        }

        private static void AddTriggerTransition(
            AnimatorState fromState,
            AnimatorState toState,
            string triggerName,
            float duration)
        {
            AnimatorStateTransition transition = fromState.AddTransition(toState);
            transition.hasExitTime = false;
            transition.duration = duration;
            transition.AddCondition(AnimatorConditionMode.If, 0f, triggerName);
        }

        private static void AddBoolTransition(
            AnimatorState fromState,
            AnimatorState toState,
            string boolName,
            bool expectedTrue,
            float duration)
        {
            AnimatorStateTransition transition = fromState.AddTransition(toState);
            transition.hasExitTime = false;
            transition.duration = duration;
            transition.AddCondition(
                expectedTrue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
                0f,
                boolName);
        }

        private static void AddBoolOrTriggerTransition(
            AnimatorState fromState,
            AnimatorState toState,
            string boolName,
            string triggerName,
            float duration)
        {
            AnimatorStateTransition boolTransition = fromState.AddTransition(toState);
            boolTransition.hasExitTime = false;
            boolTransition.duration = duration;
            boolTransition.AddCondition(AnimatorConditionMode.IfNot, 0f, boolName);

            AnimatorStateTransition triggerTransition = fromState.AddTransition(toState);
            triggerTransition.hasExitTime = false;
            triggerTransition.duration = duration;
            triggerTransition.AddCondition(AnimatorConditionMode.If, 0f, triggerName);
        }

        private static void AddExitTimeTransition(
            AnimatorState fromState,
            AnimatorState toState,
            float duration,
            float exitTime,
            string boolName = null,
            bool expectedTrue = true)
        {
            AnimatorStateTransition transition = fromState.AddTransition(toState);
            transition.hasExitTime = true;
            transition.exitTime = exitTime;
            transition.duration = duration;
            if (!string.IsNullOrEmpty(boolName))
            {
                transition.AddCondition(
                    expectedTrue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
                    0f,
                    boolName);
            }
        }

        private static int FindLayerIndex(AnimatorController controller, string layerName)
        {
            AnimatorControllerLayer[] layers = controller.layers;
            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i].name == layerName)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
