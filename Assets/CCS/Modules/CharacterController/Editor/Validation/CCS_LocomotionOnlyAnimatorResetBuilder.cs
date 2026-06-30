using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.Interaction;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_LocomotionOnlyAnimatorResetBuilder
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Resets player Animator Controller to locomotion-only foundation (v0.7.3 Phase 3B).
// PLACEMENT: Editor builder utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Removes aim/revolver/interaction animation layers and bridge components from player prefab.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public sealed class LocomotionOnlyAnimatorResetResult
    {
        public List<string> RemovedLayers = new List<string>();
        public List<string> RemovedStates = new List<string>();
        public List<string> RemovedParameters = new List<string>();
        public List<string> ChangedScripts = new List<string>();
        public int PlayerRootMonoBehaviourCount;
        public bool ControllerChanged;
        public bool PrefabChanged;
    }

    public static class CCS_LocomotionOnlyAnimatorResetBuilder
    {
        private static readonly string[] BaseLayerStatesToRemove =
        {
            "Interact_PickUp_RH",
            "Interact_WalkThroughDoor_RH",
            CCS_CharacterControllerConstants.AnimatorAimStrafeLocomotionStateName,
            CCS_CharacterControllerConstants.AnimatorAimStrafeBlendTreeName,
            CCS_CharacterControllerConstants.AnimatorRevolverWildWestAimIdleStateName,
            CCS_CharacterControllerConstants.AnimatorRevolverIdleToAimStateName,
            CCS_CharacterControllerConstants.AnimatorRevolverAimIdleFullDrawStateName,
            CCS_CharacterControllerConstants.AnimatorRevolverAimToIdleReturnStateName,
            CCS_CharacterControllerConstants.AnimatorRevolverAimToIdleStateName,
            CCS_CharacterControllerConstants.AnimatorRevolverWalkToAimWalkStateName,
            CCS_CharacterControllerConstants.AnimatorRevolverAimWalkStateName,
            CCS_CharacterControllerConstants.AnimatorRevolverAimWalkToWalkStateName,
            CCS_CharacterControllerConstants.AnimatorRevolverAimPitchBlendStateName,
            CCS_CharacterControllerConstants.AnimatorRevolverFireStateName,
            CCS_CharacterControllerConstants.AnimatorRevolverReloadStateName,
            "Revolver_WW_Empty",
            "Revolver_WW_Reload_RH",
            "Revolver_WW_Fire_Fanning_RH",
            "Revolver_WW_AimIdle_RH",
            "Revolver_AimPitch_BlendTree",
        };

        public static LocomotionOnlyAnimatorResetResult ApplyLocomotionOnlyAnimatorReset(bool writeBeforeReport = true)
        {
            LocomotionOnlyAnimatorResetResult result = new LocomotionOnlyAnimatorResetResult();
            if (writeBeforeReport)
            {
                CCS_AnimatorControllerInventoryReporter.WriteBeforeReport();
            }

            result.ControllerChanged |= ResetAnimatorController(result);
            result.PrefabChanged |= RemoveAnimationBridgeComponentsFromProductionPrefabs(result);
            result.PlayerRootMonoBehaviourCount = CountPlayerRootMonoBehaviours();

            result.ChangedScripts.Add("Updated CCS_PlayerLocomotionAnimator (aim movement parameter writes removed).");
            result.ChangedScripts.Add(
                "Updated CCS_PlayerInteractionAnimator (interaction animation triggers disabled; gameplay lock retained).");
            result.ChangedScripts.Add(
                "Updated CCS_MuzzleDrivenReticleController (gameplay aim fallback without upper-body animator phase).");

            if (result.ControllerChanged || result.PrefabChanged)
            {
                AssetDatabase.SaveAssets();
            }

            CCS_AnimatorControllerInventoryReporter.WriteAfterReport(
                result.RemovedLayers,
                result.RemovedStates,
                result.RemovedParameters,
                result.ChangedScripts,
                result.PlayerRootMonoBehaviourCount);

            return result;
        }

        public static bool MaintainLocomotionOnlyController()
        {
            LocomotionOnlyAnimatorResetResult result = ApplyLocomotionOnlyAnimatorReset(writeBeforeReport: false);
            return result.ControllerChanged || result.PrefabChanged;
        }

        private static bool ResetAnimatorController(LocomotionOnlyAnimatorResetResult result)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            if (controller == null)
            {
                Debug.LogError(
                    "[Locomotion Animator Reset] Missing controller at "
                    + CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
                return false;
            }

            bool changed = false;
            changed |= RemoveNonLocomotionLayers(controller, result);
            if (controller.layers.Length == 0)
            {
                Debug.LogError("[Locomotion Animator Reset] Controller has no layers after cleanup.");
                return changed;
            }

            AnimatorStateMachine baseStateMachine = controller.layers[0].stateMachine;
            if (baseStateMachine != null)
            {
                changed |= RemoveBaseLayerNonLocomotionContent(baseStateMachine, result);
            }

            changed |= RemoveNonLocomotionParameters(controller, result);

            if (changed)
            {
                EditorUtility.SetDirty(controller);
            }

            return changed;
        }

        private static bool RemoveNonLocomotionLayers(
            AnimatorController controller,
            LocomotionOnlyAnimatorResetResult result)
        {
            bool changed = false;
            for (int layerIndex = controller.layers.Length - 1; layerIndex >= 1; layerIndex--)
            {
                string layerName = controller.layers[layerIndex].name;
                if (layerName == CCS_CharacterControllerConstants.SingleRevolverUpperBodyLayerName)
                {
                    continue;
                }

                result.RemovedLayers.Add(layerName);
                controller.RemoveLayer(layerIndex);
                changed = true;
            }

            return changed;
        }

        private static bool RemoveBaseLayerNonLocomotionContent(
            AnimatorStateMachine stateMachine,
            LocomotionOnlyAnimatorResetResult result)
        {
            bool changed = false;
            changed |= RemoveAnyStateTransitions(stateMachine);
            changed |= RemoveRevolverAimHeldTransitions(stateMachine);

            for (int i = 0; i < BaseLayerStatesToRemove.Length; i++)
            {
                if (RemoveStateIfPresent(stateMachine, BaseLayerStatesToRemove[i]))
                {
                    result.RemovedStates.Add(BaseLayerStatesToRemove[i]);
                    changed = true;
                }
            }

            ChildAnimatorStateMachine[] childMachines = stateMachine.stateMachines;
            for (int i = childMachines.Length - 1; i >= 0; i--)
            {
                AnimatorStateMachine childMachine = childMachines[i].stateMachine;
                if (childMachine == null)
                {
                    continue;
                }

                result.RemovedStates.Add("Sub-State Machine: " + childMachine.name);
                stateMachine.RemoveStateMachine(childMachine);
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(stateMachine);
            }

            return changed;
        }

        private static bool RemoveNonLocomotionParameters(
            AnimatorController controller,
            LocomotionOnlyAnimatorResetResult result)
        {
            bool changed = false;
            HashSet<string> allowed = new HashSet<string>(CCS_CharacterControllerConstants.Phase3BAllowedAnimatorParameterNames);
            for (int i = controller.parameters.Length - 1; i >= 0; i--)
            {
                string parameterName = controller.parameters[i].name;
                if (allowed.Contains(parameterName))
                {
                    continue;
                }

                result.RemovedParameters.Add(parameterName);
                controller.RemoveParameter(i);
                changed = true;
            }

            return changed;
        }

        private static readonly string[] ProductionPrefabPaths =
        {
            CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath,
            "Assets/CCS/Modules/AI/Content/Prefabs/PF_CCS_AI_Bandit_Networked.prefab",
            CCS_CharacterControllerMasterTestLayoutConstants.NpcPrefabPath,
        };

        private static bool RemoveAnimationBridgeComponentsFromProductionPrefabs(LocomotionOnlyAnimatorResetResult result)
        {
            bool changed = false;
            for (int prefabIndex = 0; prefabIndex < ProductionPrefabPaths.Length; prefabIndex++)
            {
                changed |= RemoveAnimationBridgeComponentsFromPrefab(ProductionPrefabPaths[prefabIndex], result);
            }

            return changed;
        }

        private static bool RemoveAnimationBridgeComponentsFromPrefab(
            string prefabPath,
            LocomotionOnlyAnimatorResetResult result)
        {
            if (!File.Exists(prefabPath))
            {
                return false;
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                return false;
            }

            bool changed = false;
            try
            {
                for (int i = 0;
                     i < CCS_CharacterControllerConstants.Phase3BRemovedAnimationBridgeComponentTypeNames.Length;
                     i++)
                {
                    string typeName =
                        CCS_CharacterControllerConstants.Phase3BRemovedAnimationBridgeComponentTypeNames[i];
                    Component[] components = prefabRoot.GetComponentsInChildren<Component>(true);
                    for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
                    {
                        Component component = components[componentIndex];
                        if (component == null || component.GetType().Name != typeName)
                        {
                            continue;
                        }

                        Object.DestroyImmediate(component, true);
                        result.ChangedScripts.Add("Removed " + typeName + " from " + prefabPath + ".");
                        changed = true;
                    }
                }

                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            return changed;
        }

        private static bool RemoveAnimationBridgeComponentsFromPlayerPrefab(LocomotionOnlyAnimatorResetResult result)
        {
            return RemoveAnimationBridgeComponentsFromPrefab(
                CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath,
                result);
        }

        private static int CountPlayerRootMonoBehaviours()
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (prefabRoot == null)
            {
                return -1;
            }

            return prefabRoot.GetComponents<MonoBehaviour>().Length;
        }

        private static bool RemoveAnyStateTransitions(AnimatorStateMachine stateMachine)
        {
            bool changed = false;
            while (stateMachine.anyStateTransitions.Length > 0)
            {
                stateMachine.RemoveAnyStateTransition(stateMachine.anyStateTransitions[0]);
                changed = true;
            }

            return changed;
        }

        private static bool RemoveRevolverAimHeldTransitions(AnimatorStateMachine stateMachine)
        {
            bool changed = false;
            ChildAnimatorState[] states = stateMachine.states;
            for (int stateIndex = 0; stateIndex < states.Length; stateIndex++)
            {
                AnimatorState state = states[stateIndex].state;
                if (state == null)
                {
                    continue;
                }

                changed |= RemoveTransitionsUsingParameter(state, CCS_CharacterControllerConstants.AnimatorRevolverAimHeldParameter);
                changed |= RemoveTransitionsUsingParameter(state, CCS_InteractionConstants.PlayerPickupRightHandAnimationName);
                changed |= RemoveTransitionsUsingParameter(state, CCS_InteractionConstants.PlayerWalkThroughDoorRightHandAnimationName);
            }

            return changed;
        }

        private static bool RemoveTransitionsUsingParameter(AnimatorState state, string parameterName)
        {
            bool changed = false;
            AnimatorStateTransition[] transitions = state.transitions;
            for (int i = transitions.Length - 1; i >= 0; i--)
            {
                AnimatorStateTransition transition = transitions[i];
                if (transition == null)
                {
                    continue;
                }

                AnimatorCondition[] conditions = transition.conditions;
                for (int conditionIndex = 0; conditionIndex < conditions.Length; conditionIndex++)
                {
                    if (conditions[conditionIndex].parameter == parameterName)
                    {
                        state.RemoveTransition(transition);
                        changed = true;
                        break;
                    }
                }
            }

            return changed;
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
