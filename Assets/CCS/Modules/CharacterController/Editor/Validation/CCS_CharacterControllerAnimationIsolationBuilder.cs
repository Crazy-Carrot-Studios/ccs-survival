using System.Collections.Generic;
using System.IO;
using CCS.Project;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterControllerAnimationIsolationBuilder
// CATEGORY: Modules / CharacterController / Editor
// PURPOSE: Duplicates vendor animation clips into CCS-owned folders and rewires the player AC.
// PLACEMENT: Editor utility invoked from animation isolation menu and master test setup.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.5.6 — vendor packs remain source libraries; production AC uses CCS .anim copies only.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerAnimationIsolationBuilder
    {
        #region Variables

        private readonly struct ClipIsolationEntry
        {
            public ClipIsolationEntry(
                string sourceAssetPath,
                string sourceClipName,
                string destinationAssetPath,
                string animatorStateName)
            {
                SourceAssetPath = sourceAssetPath;
                SourceClipName = sourceClipName;
                DestinationAssetPath = destinationAssetPath;
                AnimatorStateName = animatorStateName;
            }

            public string SourceAssetPath { get; }

            public string SourceClipName { get; }

            public string DestinationAssetPath { get; }

            public string AnimatorStateName { get; }
        }

        private static readonly ClipIsolationEntry[] IsolationPlan =
        {
            new ClipIsolationEntry(
                "Assets/StarterAssets/ThirdPersonController/Character/Animations/Stand--Idle.anim.fbx",
                "Idle",
                CCS_CharacterControllerConstants.LocomotionAnimationsPath + "/CCS_Locomotion_Idle.anim",
                "Idle"),
            new ClipIsolationEntry(
                "Assets/StarterAssets/ThirdPersonController/Character/Animations/Locomotion--Walk_N.anim.fbx",
                "Walk_N",
                CCS_CharacterControllerConstants.LocomotionAnimationsPath + "/CCS_Locomotion_Walk_N.anim",
                "Walk"),
            new ClipIsolationEntry(
                "Assets/StarterAssets/ThirdPersonController/Character/Animations/Locomotion--Run_N.anim.fbx",
                "Run_N",
                CCS_CharacterControllerConstants.LocomotionAnimationsPath + "/CCS_Locomotion_Run_N.anim",
                "Sprint"),
            new ClipIsolationEntry(
                "Assets/StarterAssets/ThirdPersonController/Character/Animations/Jump--Jump.anim.fbx",
                "JumpStart",
                CCS_CharacterControllerConstants.LocomotionAnimationsPath + "/CCS_Locomotion_JumpStart.anim",
                "Jump"),
            new ClipIsolationEntry(
                "Assets/StarterAssets/ThirdPersonController/Character/Animations/Jump--InAir.anim.fbx",
                "InAir",
                CCS_CharacterControllerConstants.LocomotionAnimationsPath + "/CCS_Locomotion_InAir.anim",
                "InAir"),
            new ClipIsolationEntry(
                "Assets/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx",
                "PickUp_RH",
                CCS_CharacterControllerConstants.InteractionAnimationsPath + "/CCS_Interaction_PickUp_RH.anim",
                "Interact_PickUp_RH"),
            new ClipIsolationEntry(
                "Assets/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx",
                "WalkThroughDoor_RH",
                CCS_CharacterControllerConstants.InteractionAnimationsPath + "/CCS_Interaction_WalkThroughDoor_RH.anim",
                "Interact_WalkThroughDoor_RH"),
        };

        #endregion

        #region Public Methods

        public static bool EnsurePlayerAnimationIsolation()
        {
            EnsureAnimationFolders();

            bool changed = false;
            Dictionary<string, AnimationClip> clipsByStateName = new Dictionary<string, AnimationClip>();

            for (int i = 0; i < IsolationPlan.Length; i++)
            {
                ClipIsolationEntry entry = IsolationPlan[i];
                if (!EnsureIsolatedClip(entry, out AnimationClip isolatedClip, out bool clipChanged))
                {
                    Debug.LogError(
                        "[Animation Isolation] Failed to isolate clip "
                        + entry.SourceClipName
                        + " from "
                        + entry.SourceAssetPath);
                    return changed;
                }

                changed |= clipChanged;
                clipsByStateName[entry.AnimatorStateName] = isolatedClip;
            }

            changed |= RewireAnimatorController(clipsByStateName);

            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return changed;
        }

        #endregion

        #region Private Methods

        private static void EnsureAnimationFolders()
        {
            EnsureFolder(CCS_CharacterControllerConstants.ContentAnimationsRootPath);
            EnsureFolder(CCS_CharacterControllerConstants.LocomotionAnimationsPath);
            EnsureFolder(CCS_CharacterControllerConstants.InteractionAnimationsPath);
            EnsureFolder(CCS_CharacterControllerConstants.ContentAnimationsRootPath + "/Combat");
            EnsureFolder(CCS_CharacterControllerConstants.CombatRevolverAnimationsPath);
        }

        private static void EnsureFolder(string assetFolderPath)
        {
            assetFolderPath = assetFolderPath.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(assetFolderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(assetFolderPath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent) && parent.StartsWith("Assets/"))
            {
                EnsureFolder(parent);
            }

            string folderName = Path.GetFileName(assetFolderPath);
            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(folderName))
            {
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        private static bool EnsureIsolatedClip(
            ClipIsolationEntry entry,
            out AnimationClip isolatedClip,
            out bool changed)
        {
            changed = false;
            isolatedClip = null;

            AnimationClip sourceClip = LoadClipFromAsset(entry.SourceAssetPath, entry.SourceClipName);
            if (sourceClip == null)
            {
                return false;
            }

            isolatedClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(entry.DestinationAssetPath);
            if (isolatedClip == null)
            {
                isolatedClip = new AnimationClip();
                AssetDatabase.CreateAsset(isolatedClip, entry.DestinationAssetPath);
                changed = true;
            }

            string beforeName = isolatedClip.name;
            EditorUtility.CopySerialized(sourceClip, isolatedClip);
            isolatedClip.name = Path.GetFileNameWithoutExtension(entry.DestinationAssetPath);

            if (beforeName != isolatedClip.name)
            {
                changed = true;
            }

            EditorUtility.SetDirty(isolatedClip);
            return true;
        }

        private static AnimationClip LoadClipFromAsset(string assetPath, string clipName)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is AnimationClip clip
                    && clip.name == clipName
                    && !clip.name.StartsWith("__"))
                {
                    return clip;
                }
            }

            return null;
        }

        private static bool RewireAnimatorController(Dictionary<string, AnimationClip> clipsByStateName)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            if (controller == null)
            {
                Debug.LogError(
                    "[Animation Isolation] Missing player Animator Controller at "
                    + CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
                return false;
            }

            bool changed = false;
            for (int layerIndex = 0; layerIndex < controller.layers.Length; layerIndex++)
            {
                changed |= RewireStateMachine(controller.layers[layerIndex].stateMachine, clipsByStateName);
            }

            if (changed)
            {
                EditorUtility.SetDirty(controller);
            }

            return changed;
        }

        private static bool RewireStateMachine(
            AnimatorStateMachine stateMachine,
            Dictionary<string, AnimationClip> clipsByStateName)
        {
            bool changed = false;

            ChildAnimatorState[] states = stateMachine.states;
            for (int i = 0; i < states.Length; i++)
            {
                AnimatorState state = states[i].state;
                if (state == null || !clipsByStateName.TryGetValue(state.name, out AnimationClip targetClip))
                {
                    continue;
                }

                if (state.motion == targetClip)
                {
                    continue;
                }

                state.motion = targetClip;
                EditorUtility.SetDirty(state);
                changed = true;
            }

            ChildAnimatorStateMachine[] childStateMachines = stateMachine.stateMachines;
            for (int i = 0; i < childStateMachines.Length; i++)
            {
                if (childStateMachines[i].stateMachine != null)
                {
                    changed |= RewireStateMachine(childStateMachines[i].stateMachine, clipsByStateName);
                }
            }

            return changed;
        }

        #endregion
    }
}
