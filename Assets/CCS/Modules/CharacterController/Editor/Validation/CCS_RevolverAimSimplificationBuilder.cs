using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverAimSimplificationBuilder
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Migrates revolver aim clips, builds simplified right-arm aim layer, removes AimPitch flow.
// PLACEMENT: Invoked from animation isolation builder and batch validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.15 — IdleToAim → FullDraw hold → IdleToAim reverse. Fit Studio overwrites FullDraw in place.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_RevolverAimSimplificationBuilder
    {
        private const float AimInDuration = 0.05f;
        private const float AimOutDuration = 0.08f;
        private const float AimEnterExitTime = 0.95f;
        private const float AimReturnExitTime = 0.95f;
        private const float ReverseAimReturnStateSpeed = -1f;
        private const int CanonicalRevolverUpperBodyLayerIndex = 1;
        private const int CanonicalInteractionLayerIndex = 2;

        private static readonly string[] RevolverAimLayerNamesToRemove =
        {
            CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName,
            CCS_CharacterControllerConstants.AnimatorRevolverAimUpperBodyLayerNameObsolete,
            CCS_CharacterControllerConstants.AnimatorRevolverRightHandPreviewLayerName,
        };

        private static readonly string[] ObsoleteEditedClipPaths =
        {
            "Assets/CCS/Modules/CharacterController/Content/Animations/Revolver/WildWest/Edited/CCS_WW_Revolver_AimIdle_FullDraw_FitTest.anim",
            "Assets/CCS/Modules/CharacterController/Content/Animations/Revolver/WildWest/Edited/CCS_WW_Revolver_AimPitch_Down_FitTest.anim",
            "Assets/CCS/Modules/CharacterController/Content/Animations/Revolver/WildWest/Edited/CCS_WW_Revolver_AimPitch_Center_FitTest.anim",
            "Assets/CCS/Modules/CharacterController/Content/Animations/Revolver/WildWest/Edited/CCS_WW_Revolver_AimPitch_Up_FitTest.anim",
            "Assets/CCS/Modules/CharacterController/Content/Animations/Revolver/WildWest/Edited/CCS_WW_Revolver_AimIdle_RH_FitTest.anim",
        };

        public static bool EnsureRevolverAimSimplificationPass()
        {
            return CCS_LocomotionOnlyAnimatorResetBuilder.MaintainLocomotionOnlyController();
        }

        public static bool EnsureRevolverAimFolders()
        {
            bool changed = false;
            changed |= EnsureFolder(CCS_CharacterControllerConstants.RevolverAimAnimationsPath);
            changed |= EnsureFolder(CCS_CharacterControllerConstants.RevolverAimMasksPath);
            return changed;
        }

        public static bool EnsureRevolverAimClipMigration()
        {
            bool changed = false;
            changed |= MoveClipIfNeeded(
                "Assets/CCS/Modules/CharacterController/Content/Animations/Revolver/WildWest/CCS_WW_Revolver_IdleToAim.anim",
                CCS_CharacterControllerConstants.RevolverIdleToAimClipPath);
            changed |= MoveClipIfNeeded(
                "Assets/CCS/Modules/CharacterController/Content/Animations/Revolver/WildWest/CCS_WW_Revolver_AimIdle_FullDraw.anim",
                CCS_CharacterControllerConstants.RevolverAimIdleFullDrawClipPath);

            string editedFullDrawPath =
                "Assets/CCS/Modules/CharacterController/Content/Animations/Revolver/WildWest/Edited/CCS_WW_Revolver_AimIdle_FullDraw_FitTest.anim";
            if (File.Exists(editedFullDrawPath)
                && !File.Exists(CCS_CharacterControllerConstants.RevolverAimIdleFullDrawClipPath))
            {
                changed |= MoveClipIfNeeded(editedFullDrawPath, CCS_CharacterControllerConstants.RevolverAimIdleFullDrawClipPath);
            }

            return changed;
        }

        public static bool EnsureRevolverAimRightArmMask()
        {
            return CCS_RevolverUpperBodyRightArmAimMaskUtility.EnsureMaskAsset();
        }

        public static bool EnsureInteractionReservedLayer()
        {
            AnimatorController controller = LoadPlayerController();
            if (controller == null)
            {
                return false;
            }

            bool changed = false;
            int layerIndex = FindLayerIndex(controller, CCS_CharacterControllerConstants.AnimatorInteractionReservedLayerName);
            if (layerIndex < 0)
            {
                controller.AddLayer(CCS_CharacterControllerConstants.AnimatorInteractionReservedLayerName);
                layerIndex = controller.layers.Length - 1;
                changed = true;
            }

            AnimatorControllerLayer layer = controller.layers[layerIndex];
            if (layer.defaultWeight > 0.001f)
            {
                layer.defaultWeight = 0f;
                changed = true;
            }

            if (layer.blendingMode != AnimatorLayerBlendingMode.Override)
            {
                layer.blendingMode = AnimatorLayerBlendingMode.Override;
                changed = true;
            }

            controller.layers[layerIndex] = layer;
            if (changed)
            {
                EditorUtility.SetDirty(controller);
            }

            return changed;
        }

        public static bool EnsureSimplifiedRevolverAimLayer()
        {
            AnimatorController controller = LoadPlayerController();
            if (controller == null)
            {
                return false;
            }

            AnimationClip idleToAimClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                CCS_CharacterControllerConstants.RevolverIdleToAimClipPath);
            AnimationClip fullDrawClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                CCS_CharacterControllerConstants.RevolverAimIdleFullDrawClipPath);
            AvatarMask mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(
                CCS_CharacterControllerConstants.RevolverAimRightArmMaskPath);

            if (idleToAimClip == null || fullDrawClip == null || mask == null)
            {
                Debug.LogError(
                    "[Revolver Aim Simplification] Missing IdleToAim, FullDraw, or right-arm aim mask.");
                return false;
            }

            bool changed = false;
            changed |= RemoveAllRevolverAimLayers(controller);
            changed |= EnsureCanonicalLayerStack(controller);
            changed |= EnsureAnimatorBoolParameter(
                controller,
                CCS_CharacterControllerConstants.AnimatorRevolverAimHeldParameter,
                ref changed);

            int layerIndex = FindLayerIndex(controller, CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName);
            if (layerIndex < 0)
            {
                Debug.LogError("[Revolver Aim Simplification] Failed to create canonical RevolverUpperBody layer.");
                return false;
            }

            changed |= AssignLayerMask(controller, layerIndex, mask);

            AnimatorControllerLayer layer = controller.layers[layerIndex];
            layer.defaultWeight = 0f;
            layer.blendingMode = AnimatorLayerBlendingMode.Override;
            layer.iKPass = false;
            controller.layers[layerIndex] = layer;

            AnimatorStateMachine stateMachine = layer.stateMachine;
            if (stateMachine == null)
            {
                return changed;
            }

            changed |= ResetStateMachine(stateMachine);

            AnimatorState noAimState = EnsureState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorRevolverNoAimStateName,
                null,
                new Vector3(300f, 0f, 0f),
                1f,
                ref changed);
            if (noAimState.writeDefaultValues)
            {
                noAimState.writeDefaultValues = false;
                changed = true;
            }

            AnimatorState idleToAimState = EnsureState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorRevolverIdleToAimStateName,
                idleToAimClip,
                new Vector3(300f, 120f, 0f),
                1f,
                ref changed);
            AnimatorState fullDrawState = EnsureState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorRevolverAimIdleFullDrawStateName,
                fullDrawClip,
                new Vector3(300f, 240f, 0f),
                1f,
                ref changed);
            AnimatorState aimToIdleReturnState = EnsureState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorRevolverAimToIdleReturnStateName,
                idleToAimClip,
                new Vector3(300f, 360f, 0f),
                ReverseAimReturnStateSpeed,
                ref changed);

            if (stateMachine.defaultState != noAimState)
            {
                stateMachine.defaultState = noAimState;
                changed = true;
            }

            string aimHeld = CCS_CharacterControllerConstants.AnimatorRevolverAimHeldParameter;

            changed |= EnsureBoolTransition(noAimState, idleToAimState, aimHeld, true, AimInDuration);
            changed |= EnsureExitTimeTransition(idleToAimState, fullDrawState, 0.06f, AimEnterExitTime);
            changed |= EnsureBoolTransition(
                idleToAimState,
                aimToIdleReturnState,
                aimHeld,
                false,
                AimInDuration);
            changed |= EnsureBoolTransition(
                fullDrawState,
                aimToIdleReturnState,
                aimHeld,
                false,
                AimOutDuration,
                offset: 1f);
            changed |= EnsureExitTimeTransition(aimToIdleReturnState, noAimState, AimInDuration, AimReturnExitTime);

            layer.stateMachine = stateMachine;
            controller.layers[layerIndex] = layer;
            EditorUtility.SetDirty(stateMachine);
            EditorUtility.SetDirty(controller);

            if (changed)
            {
                AssetDatabase.SaveAssets();
            }

            return changed;
        }

        public static bool RemoveBaseLayerLegacyAimStates()
        {
            AnimatorController controller = LoadPlayerController();
            if (controller == null || controller.layers.Length == 0)
            {
                return false;
            }

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            if (stateMachine == null)
            {
                return false;
            }

            bool changed = false;
            changed |= RemoveRevolverAimHeldTransitionsFromStateMachine(stateMachine);

            string[] forbiddenStates =
            {
                CCS_CharacterControllerConstants.AnimatorAimStrafeLocomotionStateName,
                CCS_CharacterControllerConstants.AnimatorAimStrafeBlendTreeName,
                CCS_CharacterControllerConstants.AnimatorRevolverWildWestAimIdleStateName,
                CCS_CharacterControllerConstants.AnimatorRevolverIdleToAimStateName,
                CCS_CharacterControllerConstants.AnimatorRevolverAimIdleFullDrawStateName,
                "Revolver_WW_Empty",
                "Revolver_WW_Reload_RH",
                "Revolver_WW_Fire_Fanning_RH",
            };

            for (int i = 0; i < forbiddenStates.Length; i++)
            {
                changed |= RemoveStateIfPresent(stateMachine, forbiddenStates[i]);
            }

            if (changed)
            {
                EditorUtility.SetDirty(stateMachine);
                EditorUtility.SetDirty(controller);
            }

            return changed;
        }

        public static bool RemoveRevolverAimPitchFromController()
        {
            AnimatorController controller = LoadPlayerController();
            if (controller == null)
            {
                return false;
            }

            bool changed = false;
            int layerIndex = FindLayerIndex(controller, CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName);
            if (layerIndex >= 0)
            {
                AnimatorStateMachine stateMachine = controller.layers[layerIndex].stateMachine;
                if (stateMachine != null)
                {
                    changed |= RemoveStateIfPresent(stateMachine, "Revolver_AimPitch_Blend");
                    changed |= RemoveStateIfPresent(stateMachine, CCS_CharacterControllerConstants.AnimatorRevolverAimPitchBlendStateName);
                    changed |= RemoveStateIfPresent(stateMachine, CCS_CharacterControllerConstants.AnimatorRevolverAimToIdleStateName);
                    changed |= RemoveStateIfPresent(stateMachine, CCS_CharacterControllerConstants.AnimatorRevolverWalkToAimWalkStateName);
                    changed |= RemoveStateIfPresent(stateMachine, CCS_CharacterControllerConstants.AnimatorRevolverAimWalkStateName);
                    changed |= RemoveStateIfPresent(stateMachine, CCS_CharacterControllerConstants.AnimatorRevolverAimWalkToWalkStateName);
                    changed |= RemoveStateIfPresent(stateMachine, "Revolver_Fire");
                    changed |= RemoveStateIfPresent(stateMachine, "Revolver_WW_Fire_Fanning_RH");
                    changed |= RemoveStateIfPresent(stateMachine, "Revolver_Reload");
                }
            }

            changed |= RemoveAnimatorParameterIfUnused(controller, CCS_CharacterControllerConstants.AnimatorRevolverAimPitchParameter);
            changed |= RemoveAnimatorParameterIfUnused(controller, CCS_CharacterControllerConstants.AnimatorRevolverIsMovingParameter);

            if (changed)
            {
                EditorUtility.SetDirty(controller);
            }

            return changed;
        }

        public static bool DeleteObsoleteRevolverAimAssets()
        {
            bool changed = false;
            for (int i = 0; i < ObsoleteEditedClipPaths.Length; i++)
            {
                changed |= DeleteAssetIfExists(ObsoleteEditedClipPaths[i]);
            }

            string backupsFolder =
                "Assets/CCS/Modules/CharacterController/Content/Animations/Revolver/WildWest/Edited/Backups";
            if (AssetDatabase.IsValidFolder(backupsFolder))
            {
                changed |= AssetDatabase.DeleteAsset(backupsFolder) || changed;
            }

            string editedFolder =
                "Assets/CCS/Modules/CharacterController/Content/Animations/Revolver/WildWest/Edited";
            if (AssetDatabase.IsValidFolder(editedFolder))
            {
                changed |= AssetDatabase.DeleteAsset(editedFolder) || changed;
            }

            return changed;
        }

        private static bool RemoveAllRevolverAimLayers(AnimatorController controller)
        {
            bool changed = false;
            bool removedAny;
            do
            {
                removedAny = false;
                for (int nameIndex = 0; nameIndex < RevolverAimLayerNamesToRemove.Length; nameIndex++)
                {
                    int layerIndex = FindLayerIndex(controller, RevolverAimLayerNamesToRemove[nameIndex]);
                    if (layerIndex < 0)
                    {
                        continue;
                    }

                    controller.RemoveLayer(layerIndex);
                    changed = true;
                    removedAny = true;
                    break;
                }
            }
            while (removedAny);

            if (changed)
            {
                EditorUtility.SetDirty(controller);
            }

            return changed;
        }

        private static bool EnsureCanonicalLayerStack(AnimatorController controller)
        {
            bool changed = false;
            if (controller.layers.Length == 0)
            {
                return false;
            }

            int interactionIndex = FindLayerIndex(
                controller,
                CCS_CharacterControllerConstants.AnimatorInteractionReservedLayerName);
            if (interactionIndex < 0)
            {
                controller.AddLayer(CCS_CharacterControllerConstants.AnimatorInteractionReservedLayerName);
                interactionIndex = controller.layers.Length - 1;
                changed = true;
            }

            int revolverIndex = FindLayerIndex(
                controller,
                CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName);
            if (revolverIndex < 0)
            {
                controller.AddLayer(CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName);
                revolverIndex = controller.layers.Length - 1;
                changed = true;
            }

            changed |= MoveLayerToIndex(controller, revolverIndex, CanonicalRevolverUpperBodyLayerIndex);
            interactionIndex = FindLayerIndex(
                controller,
                CCS_CharacterControllerConstants.AnimatorInteractionReservedLayerName);
            if (interactionIndex >= 0 && interactionIndex != CanonicalInteractionLayerIndex)
            {
                changed |= MoveLayerToIndex(controller, interactionIndex, CanonicalInteractionLayerIndex);
            }

            changed |= RemoveUnexpectedLayersBeyondCanonicalStack(controller);

            if (changed)
            {
                EditorUtility.SetDirty(controller);
            }

            return changed;
        }

        private static bool RemoveUnexpectedLayersBeyondCanonicalStack(AnimatorController controller)
        {
            bool changed = false;
            while (controller.layers.Length > CanonicalInteractionLayerIndex + 1)
            {
                controller.RemoveLayer(controller.layers.Length - 1);
                changed = true;
            }

            return changed;
        }

        private static bool MoveLayerToIndex(AnimatorController controller, int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex || fromIndex < 0 || toIndex < 0)
            {
                return false;
            }

            SerializedObject serializedController = new SerializedObject(controller);
            SerializedProperty layersProperty = serializedController.FindProperty("m_AnimatorLayers");
            if (layersProperty == null
                || fromIndex >= layersProperty.arraySize
                || toIndex >= layersProperty.arraySize)
            {
                return false;
            }

            layersProperty.MoveArrayElement(fromIndex, toIndex);
            serializedController.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        private static bool EnsureAnimatorBoolParameter(
            AnimatorController controller,
            string parameterName,
            ref bool changed)
        {
            for (int i = 0; i < controller.parameters.Length; i++)
            {
                if (controller.parameters[i].name == parameterName)
                {
                    return false;
                }
            }

            controller.AddParameter(parameterName, AnimatorControllerParameterType.Bool);
            changed = true;
            EditorUtility.SetDirty(controller);
            return true;
        }

        private static AnimatorController LoadPlayerController()
        {
            return AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
        }

        private static bool MoveClipIfNeeded(string sourcePath, string destinationPath)
        {
            sourcePath = sourcePath.Replace('\\', '/');
            destinationPath = destinationPath.Replace('\\', '/');

            if (File.Exists(destinationPath))
            {
                if (File.Exists(sourcePath) && sourcePath != destinationPath)
                {
                    return DeleteAssetIfExists(sourcePath);
                }

                return false;
            }

            if (!File.Exists(sourcePath))
            {
                return false;
            }

            EnsureFolder(Path.GetDirectoryName(destinationPath)?.Replace('\\', '/'));
            string moveResult = AssetDatabase.MoveAsset(sourcePath, destinationPath);
            if (!string.IsNullOrEmpty(moveResult))
            {
                Debug.LogError(
                    "[Revolver Aim Simplification] Failed to move "
                    + sourcePath
                    + " -> "
                    + destinationPath
                    + ": "
                    + moveResult);
                return false;
            }

            return true;
        }

        private static bool AssignLayerMask(AnimatorController controller, int layerIndex, AvatarMask mask)
        {
            SerializedObject serializedController = new SerializedObject(controller);
            SerializedProperty layersProperty = serializedController.FindProperty("m_AnimatorLayers");
            if (layersProperty == null || layerIndex >= layersProperty.arraySize)
            {
                return false;
            }

            SerializedProperty layerProperty = layersProperty.GetArrayElementAtIndex(layerIndex);
            SerializedProperty maskProperty = layerProperty.FindPropertyRelative("m_Mask");
            if (maskProperty == null || maskProperty.objectReferenceValue == mask)
            {
                return false;
            }

            maskProperty.objectReferenceValue = mask;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        private static bool RenameLayerIfPresent(AnimatorController controller, string oldName, string newName)
        {
            int oldIndex = FindLayerIndex(controller, oldName);
            if (oldIndex < 0)
            {
                return false;
            }

            if (FindLayerIndex(controller, newName) >= 0)
            {
                return false;
            }

            AnimatorControllerLayer layer = controller.layers[oldIndex];
            layer.name = newName;
            controller.layers[oldIndex] = layer;
            EditorUtility.SetDirty(controller);
            return true;
        }

        private static bool ResetStateMachine(AnimatorStateMachine stateMachine)
        {
            bool changed = false;
            while (stateMachine.anyStateTransitions.Length > 0)
            {
                stateMachine.RemoveAnyStateTransition(stateMachine.anyStateTransitions[0]);
                changed = true;
            }

            ChildAnimatorState[] states = stateMachine.states;
            for (int i = states.Length - 1; i >= 0; i--)
            {
                stateMachine.RemoveState(states[i].state);
                changed = true;
            }

            return changed;
        }

        private static AnimatorState EnsureState(
            AnimatorStateMachine stateMachine,
            string stateName,
            AnimationClip clip,
            Vector3 position,
            float speed,
            ref bool changed)
        {
            AnimatorState state = FindState(stateMachine, stateName);
            if (state == null)
            {
                state = stateMachine.AddState(stateName, position);
                changed = true;
            }

            if (state.motion != clip)
            {
                state.motion = clip;
                changed = true;
            }

            if (state.speed != speed)
            {
                state.speed = speed;
                changed = true;
            }

            if (state.writeDefaultValues)
            {
                state.writeDefaultValues = false;
                changed = true;
            }

            return state;
        }

        private static bool EnsureBoolTransition(
            AnimatorState fromState,
            AnimatorState toState,
            string parameterName,
            bool expectedTrue,
            float duration,
            float offset = 0f)
        {
            if (fromState == null || toState == null)
            {
                return false;
            }

            AnimatorStateTransition[] transitions = fromState.transitions;
            for (int i = 0; i < transitions.Length; i++)
            {
                AnimatorStateTransition existing = transitions[i];
                if (existing.destinationState != toState)
                {
                    continue;
                }

                if (HasSingleBoolCondition(existing, parameterName, expectedTrue)
                    && !existing.hasExitTime
                    && Mathf.Approximately(existing.duration, duration)
                    && Mathf.Approximately(existing.offset, offset))
                {
                    return false;
                }
            }

            AnimatorStateTransition transition = fromState.AddTransition(toState);
            transition.hasExitTime = false;
            transition.duration = duration;
            transition.offset = offset;
            transition.canTransitionToSelf = false;
            transition.AddCondition(
                expectedTrue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
                0f,
                parameterName);
            EditorUtility.SetDirty(transition);
            EditorUtility.SetDirty(fromState);
            return true;
        }

        private static bool EnsureExitTimeTransition(
            AnimatorState fromState,
            AnimatorState toState,
            float duration,
            float exitTime)
        {
            if (fromState == null || toState == null)
            {
                return false;
            }

            AnimatorStateTransition[] transitions = fromState.transitions;
            for (int i = 0; i < transitions.Length; i++)
            {
                AnimatorStateTransition existing = transitions[i];
                if (existing.destinationState != toState)
                {
                    continue;
                }

                if (existing.conditions.Length == 0
                    && existing.hasExitTime
                    && Mathf.Approximately(existing.duration, duration)
                    && Mathf.Approximately(existing.exitTime, exitTime))
                {
                    return false;
                }
            }

            AnimatorStateTransition transition = fromState.AddTransition(toState);
            transition.hasExitTime = true;
            transition.exitTime = exitTime;
            transition.duration = duration;
            transition.canTransitionToSelf = false;
            EditorUtility.SetDirty(transition);
            EditorUtility.SetDirty(fromState);
            return true;
        }

        private static bool HasSingleBoolCondition(
            AnimatorStateTransition transition,
            string parameterName,
            bool expectedTrue)
        {
            AnimatorCondition[] conditions = transition.conditions;
            if (conditions.Length != 1)
            {
                return false;
            }

            AnimatorCondition condition = conditions[0];
            return condition.parameter == parameterName
                && (expectedTrue
                    ? condition.mode == AnimatorConditionMode.If
                    : condition.mode == AnimatorConditionMode.IfNot);
        }

        private static bool RemoveRevolverAimHeldTransitionsFromStateMachine(AnimatorStateMachine stateMachine)
        {
            bool changed = false;
            string aimHeldParameter = CCS_CharacterControllerConstants.AnimatorRevolverAimHeldParameter;
            ChildAnimatorState[] states = stateMachine.states;
            for (int stateIndex = 0; stateIndex < states.Length; stateIndex++)
            {
                AnimatorState state = states[stateIndex].state;
                if (state == null)
                {
                    continue;
                }

                AnimatorStateTransition[] transitions = state.transitions;
                for (int transitionIndex = transitions.Length - 1; transitionIndex >= 0; transitionIndex--)
                {
                    AnimatorStateTransition transition = transitions[transitionIndex];
                    if (transition == null)
                    {
                        continue;
                    }

                    if (TransitionUsesParameter(transition, aimHeldParameter))
                    {
                        state.RemoveTransition(transition);
                        changed = true;
                    }
                }
            }

            return changed;
        }

        private static bool TransitionUsesParameter(AnimatorStateTransition transition, string parameterName)
        {
            AnimatorCondition[] conditions = transition.conditions;
            for (int i = 0; i < conditions.Length; i++)
            {
                if (conditions[i].parameter == parameterName)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool RemoveStateIfPresent(AnimatorStateMachine stateMachine, string stateName)
        {
            AnimatorState state = FindState(stateMachine, stateName);
            if (state == null)
            {
                return false;
            }

            stateMachine.RemoveState(state);
            return true;
        }

        private static bool RemoveAnimatorParameterIfUnused(AnimatorController controller, string parameterName)
        {
            for (int i = 0; i < controller.parameters.Length; i++)
            {
                if (controller.parameters[i].name != parameterName)
                {
                    continue;
                }

                controller.RemoveParameter(i);
                return true;
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

        private static bool EnsureFolder(string assetFolderPath)
        {
            if (string.IsNullOrEmpty(assetFolderPath))
            {
                return false;
            }

            assetFolderPath = assetFolderPath.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(assetFolderPath))
            {
                return false;
            }

            string parent = Path.GetDirectoryName(assetFolderPath)?.Replace('\\', '/');
            string folderName = Path.GetFileName(assetFolderPath);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(folderName))
            {
                return false;
            }

            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
            return true;
        }

        private static bool DeleteAssetIfExists(string assetPath)
        {
            if (!File.Exists(assetPath) && !AssetDatabase.IsValidFolder(assetPath))
            {
                return false;
            }

            return AssetDatabase.DeleteAsset(assetPath);
        }

        private static bool FolderHasRemainingAssets(string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                return false;
            }

            string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { folderPath });
            return guids != null && guids.Length > 0;
        }
    }
}
