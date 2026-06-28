using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AnimatorClipReconnectBuilder
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Re-assigns player AC state motions by resolved clip name/path instead of stale GUIDs.
// PLACEMENT: Invoked from animator layer cleanup batch and master test setup.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: v0.7.3 — clip verification pass; logs clear errors when expected clips are missing.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_AnimatorClipReconnectBuilder
    {
        private readonly struct StateClipBinding
        {
            public StateClipBinding(string layerName, string stateName, string[] candidateAssetPaths, string[] candidateClipNames)
            {
                LayerName = layerName;
                StateName = stateName;
                CandidateAssetPaths = candidateAssetPaths;
                CandidateClipNames = candidateClipNames;
            }

            public string LayerName { get; }

            public string StateName { get; }

            public string[] CandidateAssetPaths { get; }

            public string[] CandidateClipNames { get; }
        }

        private readonly struct AimStrafeBlendBinding
        {
            public AimStrafeBlendBinding(string clipAssetPath, Vector2 position)
            {
                ClipAssetPath = clipAssetPath;
                Position = position;
            }

            public string ClipAssetPath { get; }

            public Vector2 Position { get; }
        }

        private static readonly string[] InteractionSearchFolders =
        {
            CCS_CharacterControllerConstants.InteractionAnimationsPath,
            CCS_CharacterControllerConstants.ContentAnimationsRootPath,
            "Assets/MovementAnimsetPro",
        };

        private static readonly string[] LocomotionSearchFolders =
        {
            CCS_CharacterControllerConstants.LocomotionAnimationsPath,
            CCS_CharacterControllerConstants.ContentAnimationsRootPath,
            "Assets/StarterAssets/ThirdPersonController/Character/Animations",
        };

        private static readonly string[] RevolverSearchFolders =
        {
            CCS_CharacterControllerConstants.RevolverAimAnimationsPath,
            CCS_CharacterControllerConstants.ContentAnimationsRootPath,
        };

        private static readonly StateClipBinding[] StateClipBindings =
        {
            new StateClipBinding(
                "Base Layer",
                "Idle",
                new[] { CCS_CharacterControllerConstants.LocomotionAnimationsPath + "/CCS_Locomotion_Idle.anim" },
                new[] { "CCS_Locomotion_Idle", "Idle" }),
            new StateClipBinding(
                "Base Layer",
                "Walk",
                new[] { CCS_CharacterControllerConstants.LocomotionAnimationsPath + "/CCS_Locomotion_Walk_N.anim" },
                new[] { "CCS_Locomotion_Walk_N", "Walk_N", "Walk" }),
            new StateClipBinding(
                "Base Layer",
                "Sprint",
                new[] { CCS_CharacterControllerConstants.LocomotionAnimationsPath + "/CCS_Locomotion_Run_N.anim" },
                new[] { "CCS_Locomotion_Run_N", "Run_N", "Sprint" }),
            new StateClipBinding(
                "Base Layer",
                "Jump",
                new[] { CCS_CharacterControllerConstants.LocomotionAnimationsPath + "/CCS_Locomotion_JumpStart.anim" },
                new[] { "CCS_Locomotion_JumpStart", "JumpStart", "Jump" }),
            new StateClipBinding(
                "Base Layer",
                "InAir",
                new[] { CCS_CharacterControllerConstants.LocomotionAnimationsPath + "/CCS_Locomotion_InAir.anim" },
                new[] { "CCS_Locomotion_InAir", "InAir" }),
            new StateClipBinding(
                CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName,
                CCS_CharacterControllerConstants.AnimatorRevolverIdleToAimStateName,
                new[] { CCS_CharacterControllerConstants.RevolverIdleToAimClipPath },
                new[] { "CCS_WW_Revolver_IdleToAim", "IdleToAim" }),
            new StateClipBinding(
                CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName,
                CCS_CharacterControllerConstants.AnimatorRevolverAimIdleFullDrawStateName,
                new[] { CCS_CharacterControllerConstants.RevolverAimIdleFullDrawClipPath },
                new[] { "CCS_WW_Revolver_AimIdle_FullDraw", "Fulldraw_Idle", "FullDraw" }),
            new StateClipBinding(
                CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName,
                CCS_CharacterControllerConstants.AnimatorRevolverAimToIdleReturnStateName,
                new[] { CCS_CharacterControllerConstants.RevolverIdleToAimClipPath },
                new[] { "CCS_WW_Revolver_IdleToAim", "IdleToAim" }),
            new StateClipBinding(
                CCS_CharacterControllerConstants.AnimatorInteractionReservedLayerName,
                CCS_CharacterControllerConstants.AnimatorInteractPickUpStateName,
                new[]
                {
                    CCS_CharacterControllerConstants.InteractionPickUpRightHandClipPath,
                    CCS_CharacterControllerConstants.InteractionAnimationsPath + "/CCS_Interaction_PickUp_RH.anim",
                },
                new[] { "CCS_Interaction_PickUp_RH", "Interact_PickUp_RH", "PickUp_RH" }),
            new StateClipBinding(
                CCS_CharacterControllerConstants.AnimatorInteractionReservedLayerName,
                CCS_CharacterControllerConstants.AnimatorInteractWalkThroughDoorStateName,
                new[]
                {
                    CCS_CharacterControllerConstants.InteractionWalkThroughDoorRightHandClipPath,
                    CCS_CharacterControllerConstants.InteractionAnimationsPath + "/CCS_Interaction_WalkThroughDoor_RH.anim",
                },
                new[] { "CCS_Interaction_WalkThroughDoor_RH", "Interact_WalkThroughDoor_RH", "WalkThroughDoor_RH" }),
        };

        private static readonly AimStrafeBlendBinding[] AimStrafeBlendBindings =
        {
            new AimStrafeBlendBinding(
                CCS_CharacterControllerConstants.LocomotionAnimationsPath + "/CCS_Locomotion_Idle.anim",
                Vector2.zero),
            new AimStrafeBlendBinding(CCS_CharacterControllerConstants.AimStrafeWalkFwdClipPath, new Vector2(0f, 1f)),
            new AimStrafeBlendBinding(CCS_CharacterControllerConstants.AimStrafeWalkBwdClipPath, new Vector2(0f, -1f)),
            new AimStrafeBlendBinding(CCS_CharacterControllerConstants.AimStrafeStrafeLeftClipPath, new Vector2(-1f, 0f)),
            new AimStrafeBlendBinding(CCS_CharacterControllerConstants.AimStrafeStrafeRightClipPath, new Vector2(1f, 0f)),
        };

        public static bool EnsurePlayerAnimatorClipReconnect(out List<string> errors)
        {
            errors = new List<string>();
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            if (controller == null)
            {
                errors.Add(
                    "Missing player Animator Controller at "
                    + CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath
                    + ".");
                return false;
            }

            bool changed = false;
            changed |= EnsureLayerDefaultWeights(controller);

            for (int i = 0; i < StateClipBindings.Length; i++)
            {
                StateClipBinding binding = StateClipBindings[i];
                if (!TryResolveClip(binding, out AnimationClip clip, out string resolveError))
                {
                    errors.Add(resolveError);
                    continue;
                }

                changed |= AssignClipToState(controller, binding.LayerName, binding.StateName, clip, errors);
            }

            changed |= ReconnectAimStrafeBlendTree(controller, errors);
            changed |= EnsureLayerDefaultWeights(controller);

            if (changed)
            {
                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssets();
            }

            return errors.Count == 0;
        }

        public static bool EnsureLayerDefaultWeights(AnimatorController controller)
        {
            if (controller == null || controller.layers.Length == 0)
            {
                return false;
            }

            bool changed = false;
            SerializedObject serializedController = new SerializedObject(controller);
            SerializedProperty layersProperty = serializedController.FindProperty("m_AnimatorLayers");
            if (layersProperty == null)
            {
                return false;
            }

            for (int layerIndex = 0; layerIndex < layersProperty.arraySize; layerIndex++)
            {
                SerializedProperty layerProperty = layersProperty.GetArrayElementAtIndex(layerIndex);
                SerializedProperty defaultWeightProperty = layerProperty.FindPropertyRelative("m_DefaultWeight");
                if (defaultWeightProperty == null)
                {
                    continue;
                }

                float expectedWeight = layerIndex == 0 ? 1f : 0f;
                if (!Mathf.Approximately(defaultWeightProperty.floatValue, expectedWeight))
                {
                    defaultWeightProperty.floatValue = expectedWeight;
                    changed = true;
                }
            }

            if (changed)
            {
                serializedController.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(controller);
            }

            return changed;
        }

        private static bool ReconnectAimStrafeBlendTree(AnimatorController controller, List<string> errors)
        {
            int layerIndex = FindLayerIndex(
                controller,
                CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName);
            if (layerIndex < 0)
            {
                errors.Add("RevolverUpperBody layer missing for AimStrafe clip reconnect.");
                return false;
            }

            AnimatorStateMachine stateMachine = controller.layers[layerIndex].stateMachine;
            AnimatorState aimState = FindState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorAimStrafeLocomotionStateName);
            if (aimState == null)
            {
                errors.Add("RevolverUpperBody missing AimStrafe_Locomotion state for clip reconnect.");
                return false;
            }

            BlendTree blendTree = aimState.motion as BlendTree;
            if (blendTree == null)
            {
                errors.Add("AimStrafe_Locomotion motion must be a BlendTree.");
                return false;
            }

            bool changed = false;
            ChildMotion[] children = new ChildMotion[AimStrafeBlendBindings.Length];
            for (int i = 0; i < AimStrafeBlendBindings.Length; i++)
            {
                AimStrafeBlendBinding binding = AimStrafeBlendBindings[i];
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(binding.ClipAssetPath);
                if (clip == null)
                {
                    errors.Add(
                        "Missing AimStrafe blend clip at "
                        + binding.ClipAssetPath
                        + " (searched "
                        + CCS_CharacterControllerConstants.ContentAnimationsRootPath
                        + ").");
                    continue;
                }

                children[i] = new ChildMotion
                {
                    motion = clip,
                    position = binding.Position,
                    timeScale = 1f,
                    cycleOffset = 0f,
                    directBlendParameter = string.Empty,
                    mirror = false
                };
            }

            if (!BlendTreeChildrenMatch(blendTree.children, children))
            {
                blendTree.children = children;
                changed = true;
                EditorUtility.SetDirty(blendTree);
            }

            return changed;
        }

        private static bool AssignClipToState(
            AnimatorController controller,
            string layerName,
            string stateName,
            AnimationClip clip,
            List<string> errors)
        {
            int layerIndex = FindLayerIndex(controller, layerName);
            if (layerIndex < 0)
            {
                errors.Add("Layer " + layerName + " missing while reconnecting state " + stateName + ".");
                return false;
            }

            AnimatorState state = FindState(controller.layers[layerIndex].stateMachine, stateName);
            if (state == null)
            {
                errors.Add("State " + stateName + " missing on layer " + layerName + ".");
                return false;
            }

            if (state.motion == clip)
            {
                return false;
            }

            state.motion = clip;
            EditorUtility.SetDirty(state);
            return true;
        }

        private static bool TryResolveClip(
            StateClipBinding binding,
            out AnimationClip clip,
            out string error)
        {
            clip = null;
            error = string.Empty;

            if (binding.CandidateAssetPaths != null)
            {
                for (int i = 0; i < binding.CandidateAssetPaths.Length; i++)
                {
                    string assetPath = binding.CandidateAssetPaths[i];
                    if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath))
                    {
                        continue;
                    }

                    clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
                    if (clip != null)
                    {
                        return true;
                    }
                }
            }

            string[] searchFolders = ResolveSearchFolders(binding.LayerName);
            if (binding.CandidateClipNames != null)
            {
                for (int nameIndex = 0; nameIndex < binding.CandidateClipNames.Length; nameIndex++)
                {
                    clip = FindClipByExactName(binding.CandidateClipNames[nameIndex], searchFolders);
                    if (clip != null)
                    {
                        return true;
                    }
                }
            }

            error =
                "Could not resolve clip for state "
                + binding.StateName
                + " on layer "
                + binding.LayerName
                + ". Searched clip names ["
                + string.Join(", ", binding.CandidateClipNames ?? System.Array.Empty<string>())
                + "] in folders ["
                + string.Join(", ", searchFolders)
                + "].";
            return false;
        }

        private static string[] ResolveSearchFolders(string layerName)
        {
            if (layerName == CCS_CharacterControllerConstants.AnimatorInteractionReservedLayerName)
            {
                return InteractionSearchFolders;
            }

            if (layerName == CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName)
            {
                return RevolverSearchFolders;
            }

            return LocomotionSearchFolders;
        }

        private static AnimationClip FindClipByExactName(string clipName, string[] searchFolders)
        {
            if (string.IsNullOrEmpty(clipName))
            {
                return null;
            }

            for (int folderIndex = 0; folderIndex < searchFolders.Length; folderIndex++)
            {
                string folder = searchFolders[folderIndex];
                if (string.IsNullOrEmpty(folder) || !AssetDatabase.IsValidFolder(folder))
                {
                    continue;
                }

                string[] guids = AssetDatabase.FindAssets(clipName + " t:AnimationClip", new[] { folder });
                for (int guidIndex = 0; guidIndex < guids.Length; guidIndex++)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guids[guidIndex]);
                    AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
                    if (clip != null && clip.name == clipName)
                    {
                        return clip;
                    }
                }
            }

            return null;
        }

        private static bool BlendTreeChildrenMatch(ChildMotion[] current, ChildMotion[] expected)
        {
            if (current == null || current.Length != expected.Length)
            {
                return false;
            }

            for (int i = 0; i < expected.Length; i++)
            {
                if (current[i].motion != expected[i].motion)
                {
                    return false;
                }

                if (Vector2.Distance(current[i].position, expected[i].position) > 0.001f)
                {
                    return false;
                }
            }

            return true;
        }

        private static AnimatorState FindState(AnimatorStateMachine stateMachine, string stateName)
        {
            if (stateMachine == null)
            {
                return null;
            }

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
    }
}
