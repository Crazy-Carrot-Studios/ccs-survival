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
// NOTES: v0.6.4 — isolates Invector revolver upper-body clips and wires RevolverUpperBody layer.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerAnimationIsolationBuilder
    {
        #region Variables

        private const float AimStrafeTransitionDuration = 0.1f;
        private const float RevolverAimInDuration = 0.08f;
        private const float RevolverAimOutDuration = 0.10f;
        private const float RevolverFireInDuration = 0.03f;
        private const float RevolverFireOutDuration = 0.05f;
        private const float RevolverFireExitTime = 0.85f;
        private const float RevolverReloadInDuration = 0.05f;
        private const float RevolverReloadOutDuration = 0.08f;
        private const float RevolverAimEnterExitTime = 0.92f;
        private const float RevolverAimLoopCrossfadeDuration = 0.10f;
        private const float RevolverReloadExitTime = 0.90f;

        private readonly struct ClipIsolationEntry
        {
            public ClipIsolationEntry(
                string sourceAssetPath,
                string sourceClipName,
                string destinationAssetPath,
                string animatorStateName,
                bool? loopTimeOverride = null)
            {
                SourceAssetPath = sourceAssetPath;
                SourceClipName = sourceClipName;
                DestinationAssetPath = destinationAssetPath;
                AnimatorStateName = animatorStateName;
                LoopTimeOverride = loopTimeOverride;
            }

            public string SourceAssetPath { get; }

            public string SourceClipName { get; }

            public string DestinationAssetPath { get; }

            public string AnimatorStateName { get; }

            public bool? LoopTimeOverride { get; }
        }

        private readonly struct AimStrafeBlendPoint
        {
            public AimStrafeBlendPoint(string destinationAssetPath, Vector2 position)
            {
                DestinationAssetPath = destinationAssetPath;
                Position = position;
            }

            public string DestinationAssetPath { get; }

            public Vector2 Position { get; }
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

        private static readonly ClipIsolationEntry[] AimStrafeIsolationPlan =
        {
            new ClipIsolationEntry(
                CCS_CharacterControllerConstants.MapMainFbxPath,
                "WalkFwdLoop",
                CCS_CharacterControllerConstants.AimStrafeWalkFwdClipPath,
                string.Empty,
                loopTimeOverride: true),
            new ClipIsolationEntry(
                CCS_CharacterControllerConstants.MapAdditionalsFbxPath,
                "WalkBwdLoop",
                CCS_CharacterControllerConstants.AimStrafeWalkBwdClipPath,
                string.Empty,
                loopTimeOverride: true),
            new ClipIsolationEntry(
                CCS_CharacterControllerConstants.MapAdditionalsFbxPath,
                "StrafeLeftLoop",
                CCS_CharacterControllerConstants.AimStrafeStrafeLeftClipPath,
                string.Empty,
                loopTimeOverride: true),
            new ClipIsolationEntry(
                CCS_CharacterControllerConstants.MapAdditionalsFbxPath,
                "StrafeRightLoop",
                CCS_CharacterControllerConstants.AimStrafeStrafeRightClipPath,
                string.Empty,
                loopTimeOverride: true),
        };

        private static readonly ClipIsolationEntry[] RevolverUpperBodyIsolationPlan =
        {
            new ClipIsolationEntry(
                CCS_CharacterControllerConstants.WildWestAnimationPackRootPath + "/Idle/Fulldraw_Idle.fbx",
                "Fulldraw_Idle",
                CCS_CharacterControllerConstants.RevolverAimIdleFullDrawClipPath,
                CCS_CharacterControllerConstants.AnimatorRevolverAimIdleFullDrawStateName,
                loopTimeOverride: true),
            new ClipIsolationEntry(
                CCS_CharacterControllerConstants.WildWestAnimationPackRootPath + "/Idle/Idle_Fulldraw_Revolver.fbx",
                "Idle_Fulldraw_Revolver",
                CCS_CharacterControllerConstants.RevolverIdleToAimClipPath,
                CCS_CharacterControllerConstants.AnimatorRevolverIdleToAimStateName,
                loopTimeOverride: false),
            new ClipIsolationEntry(
                CCS_CharacterControllerConstants.WildWestAnimationPackRootPath + "/Idle/Fanning.fbx",
                "Fanning",
                CCS_CharacterControllerConstants.WildWestRevolverFireFanningClipPath,
                CCS_CharacterControllerConstants.AnimatorRevolverFireStateName,
                loopTimeOverride: false),
            new ClipIsolationEntry(
                CCS_CharacterControllerConstants.InvectorShotReloadFbxPath,
                "Reload_Pistol",
                CCS_CharacterControllerConstants.RevolverReloadClipPath,
                CCS_CharacterControllerConstants.AnimatorRevolverReloadStateName,
                loopTimeOverride: false),
        };

        private static readonly ClipIsolationEntry[] WildWestRevolverArchiveIsolationPlan =
        {
            new ClipIsolationEntry(
                CCS_CharacterControllerConstants.WildWestAnimationPackRootPath + "/Walk/Walk_wRevolver_Hip_Aimed.fbx",
                "Walk_wRevolver_Hip_Aimed",
                CCS_CharacterControllerConstants.WildWestRevolverHipAimedWalkClipPath,
                string.Empty,
                loopTimeOverride: true),
            new ClipIsolationEntry(
                CCS_CharacterControllerConstants.WildWestAnimationPackRootPath + "/Idle/Quickdraw.fbx",
                "Quickdraw",
                CCS_CharacterControllerConstants.WildWestRevolverDrawQuickdrawClipPath,
                string.Empty,
                loopTimeOverride: false),
            new ClipIsolationEntry(
                CCS_CharacterControllerConstants.WildWestAnimationPackRootPath + "/Idle/Revolver_Hip_Holster.fbx",
                "Revolver_Hip_Holster",
                CCS_CharacterControllerConstants.WildWestRevolverHolsterClipPath,
                string.Empty,
                loopTimeOverride: false),
            new ClipIsolationEntry(
                CCS_CharacterControllerConstants.WildWestAnimationPackRootPath + "/Reload/Idle_Reload.fbx",
                "Idle_Reload",
                CCS_CharacterControllerConstants.WildWestRevolverReloadIdleClipPath,
                string.Empty,
                loopTimeOverride: false),
        };

        private static readonly AimStrafeBlendPoint[] AimStrafeBlendPoints =
        {
            new AimStrafeBlendPoint(
                CCS_CharacterControllerConstants.LocomotionAnimationsPath + "/CCS_Locomotion_Idle.anim",
                Vector2.zero),
            new AimStrafeBlendPoint(CCS_CharacterControllerConstants.AimStrafeWalkFwdClipPath, new Vector2(0f, 1f)),
            new AimStrafeBlendPoint(CCS_CharacterControllerConstants.AimStrafeWalkBwdClipPath, new Vector2(0f, -1f)),
            new AimStrafeBlendPoint(CCS_CharacterControllerConstants.AimStrafeStrafeLeftClipPath, new Vector2(-1f, 0f)),
            new AimStrafeBlendPoint(CCS_CharacterControllerConstants.AimStrafeStrafeRightClipPath, new Vector2(1f, 0f)),
        };

        #endregion

        #region Public Methods

        public static bool EnsurePlayerAnimationIsolation()
        {
            EnsureAnimationFolders();
            bool changed = EnsureVendorSourceInvectorAnimations();
            changed |= CleanupLegacyInvectorContent();
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

            Dictionary<string, AnimationClip> aimStrafeClips = new Dictionary<string, AnimationClip>();
            for (int i = 0; i < AimStrafeIsolationPlan.Length; i++)
            {
                ClipIsolationEntry entry = AimStrafeIsolationPlan[i];
                if (!EnsureIsolatedClip(entry, out AnimationClip isolatedClip, out bool clipChanged))
                {
                    Debug.LogError(
                        "[Animation Isolation] Failed to isolate aim strafe clip "
                        + entry.SourceClipName
                        + " from "
                        + entry.SourceAssetPath);
                    return changed;
                }

                changed |= clipChanged;
                aimStrafeClips[entry.DestinationAssetPath] = isolatedClip;
            }

            changed |= EnsureRevolverAnimatorParameters();
            changed |= CCS_RevolverAimSimplificationBuilder.EnsureRevolverAimFolders();
            changed |= CCS_RevolverAimSimplificationBuilder.EnsureRevolverAimClipMigration();

            Dictionary<string, AnimationClip> revolverClips = new Dictionary<string, AnimationClip>();
            for (int i = 0; i < RevolverUpperBodyIsolationPlan.Length; i++)
            {
                ClipIsolationEntry entry = RevolverUpperBodyIsolationPlan[i];
                AnimationClip isolatedClip = null;
                bool clipChanged = false;
                if (File.Exists(entry.DestinationAssetPath))
                {
                    isolatedClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(entry.DestinationAssetPath);
                }
                else if (!EnsureIsolatedClip(entry, out isolatedClip, out clipChanged))
                {
                    Debug.LogWarning(
                        "[Animation Isolation] Could not isolate revolver clip "
                        + entry.SourceClipName
                        + " from "
                        + entry.SourceAssetPath
                        + ". Destination missing at "
                        + entry.DestinationAssetPath);
                    continue;
                }

                changed |= clipChanged;
                if (!string.IsNullOrEmpty(entry.AnimatorStateName) && isolatedClip != null)
                {
                    revolverClips[entry.AnimatorStateName] = isolatedClip;
                }
            }

            changed |= CCS_RevolverAimSimplificationBuilder.EnsureRevolverAimRightArmMask();
            changed |= CCS_RevolverAimSimplificationBuilder.EnsureInteractionReservedLayer();
            changed |= CCS_RevolverAimSimplificationBuilder.EnsureSimplifiedRevolverAimLayer();
            changed |= CCS_RevolverAimSimplificationBuilder.RemoveRevolverAimPitchFromController();
            changed |= CCS_RevolverAimSimplificationBuilder.DeleteObsoleteRevolverAimAssets();
            changed |= RewireAnimatorController(clipsByStateName);
            changed |= EnsureAimLocomotionAnimatorParameters();
            changed |= CCS_RevolverAimSimplificationBuilder.EnsureAnimatorLayerCleanupPass();
            changed |= PurgeLegacyRevolverClipAssignmentsFromController();
            changed |= RemoveRevolverRightHandPreviewLayerFromController();

            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return changed;
        }

        public static bool EnsureRevolverAimPitchBlendWiring(out bool wiringChanged)
        {
            wiringChanged = false;
            AnimationClip downClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                CCS_CharacterControllerConstants.RevolverAimPitchDownFitTestClipPath);
            AnimationClip centerClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                CCS_CharacterControllerConstants.RevolverAimPitchCenterFitTestClipPath);
            AnimationClip upClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                CCS_CharacterControllerConstants.RevolverAimPitchUpFitTestClipPath);
            if (downClip == null || centerClip == null || upClip == null)
            {
                return false;
            }

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            if (controller == null)
            {
                return false;
            }

            int layerIndex = FindLayerIndex(
                controller,
                CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName);
            if (layerIndex < 0)
            {
                return false;
            }

            AnimatorStateMachine stateMachine = controller.layers[layerIndex].stateMachine;
            AnimatorState aimPitchBlendState = FindState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorRevolverAimPitchBlendStateName);
            if (aimPitchBlendState == null)
            {
                return false;
            }

            BlendTree blendTree = aimPitchBlendState.motion as BlendTree;
            if (blendTree == null)
            {
                return false;
            }

            wiringChanged |= EnsureAimPitchBlendTreeChildren(blendTree, downClip, centerClip, upClip);
            if (wiringChanged)
            {
                EditorUtility.SetDirty(blendTree);
                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssets();
            }

            return true;
        }

        public static bool EnsureRevolverRuntimeEditedAimIdleWiring(out bool wiringChanged)
        {
            wiringChanged = false;
            return EnsureRevolverAimPitchBlendWiring(out wiringChanged);
        }

        public static bool EnsureWildWestRevolverArchiveClips()
        {
            EnsureFolder(CCS_CharacterControllerConstants.WildWestRevolverAnimationsPath);

            bool changed = false;
            for (int i = 0; i < WildWestRevolverArchiveIsolationPlan.Length; i++)
            {
                ClipIsolationEntry entry = WildWestRevolverArchiveIsolationPlan[i];
                if (!File.Exists(entry.SourceAssetPath))
                {
                    continue;
                }

                if (!EnsureIsolatedClip(entry, out _, out bool clipChanged))
                {
                    Debug.LogWarning(
                        "[Wild West Archive] Failed to isolate clip "
                        + entry.SourceClipName
                        + " from "
                        + entry.SourceAssetPath);
                    continue;
                }

                changed |= clipChanged;
            }

            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return changed;
        }

        public static bool RemoveRevolverRightHandPreviewLayerFromController()
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            if (controller == null)
            {
                return false;
            }

            int layerIndex = FindLayerIndex(
                controller,
                CCS_CharacterControllerConstants.AnimatorRevolverRightHandPreviewLayerName);
            if (layerIndex < 0)
            {
                return false;
            }

            controller.RemoveLayer(layerIndex);
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            return true;
        }

        public static bool PurgeLegacyRevolverClipAssignmentsFromController()
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            if (controller == null)
            {
                return false;
            }

            string[] legacyClipPaths =
            {
                CCS_CharacterControllerConstants.RevolverAimIdleLegacyClipPath,
                CCS_CharacterControllerConstants.RevolverFireLegacyClipPath,
                CCS_CharacterControllerConstants.RevolverIdlePistolLegacyClipPath,
            };

            bool changed = false;
            for (int layerIndex = 0; layerIndex < controller.layers.Length; layerIndex++)
            {
                AnimatorStateMachine stateMachine = controller.layers[layerIndex].stateMachine;
                if (stateMachine == null)
                {
                    continue;
                }

                changed |= ClearLegacyClipAssignmentsFromStateMachine(stateMachine, legacyClipPaths);
            }

            if (changed)
            {
                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssets();
            }

            return changed;
        }

        private static bool ClearLegacyClipAssignmentsFromStateMachine(
            AnimatorStateMachine stateMachine,
            string[] legacyClipPaths)
        {
            bool changed = false;
            ChildAnimatorState[] states = stateMachine.states;
            for (int i = 0; i < states.Length; i++)
            {
                AnimatorState state = states[i].state;
                if (state?.motion is not AnimationClip clip)
                {
                    continue;
                }

                string clipPath = AssetDatabase.GetAssetPath(clip).Replace('\\', '/');
                for (int legacyIndex = 0; legacyIndex < legacyClipPaths.Length; legacyIndex++)
                {
                    if (clipPath != legacyClipPaths[legacyIndex])
                    {
                        continue;
                    }

                    state.motion = null;
                    EditorUtility.SetDirty(state);
                    changed = true;
                    break;
                }
            }

            ChildAnimatorStateMachine[] childMachines = stateMachine.stateMachines;
            for (int i = 0; i < childMachines.Length; i++)
            {
                if (childMachines[i].stateMachine != null)
                {
                    changed |= ClearLegacyClipAssignmentsFromStateMachine(
                        childMachines[i].stateMachine,
                        legacyClipPaths);
                }
            }

            return changed;
        }

        public static bool EnsureRevolverRightHandPreviewLayerDisabled()
        {
            return RemoveRevolverRightHandPreviewLayerFromController();
        }

        #endregion

        #region Private Methods

        private static void EnsureAnimationFolders()
        {
            EnsureFolder(CCS_CharacterControllerConstants.ContentAnimationsRootPath);
            EnsureFolder(CCS_CharacterControllerConstants.LocomotionAnimationsPath);
            EnsureFolder(CCS_CharacterControllerConstants.InteractionAnimationsPath);
            EnsureFolder(CCS_CharacterControllerConstants.ContentAnimationsRootPath + "/Combat");
            EnsureFolder(CCS_CharacterControllerConstants.AimStrafeAnimationsPath);
            EnsureFolder(CCS_CharacterControllerConstants.RevolverAimAnimationsPath);
            EnsureFolder(CCS_CharacterControllerConstants.RevolverAimMasksPath);
            EnsureFolder(CCS_CharacterControllerConstants.CombatRevolverAnimationsPath);
            EnsureFolder(CCS_CharacterControllerConstants.VendorSourceInvectorAnimationsPath);
        }

        private static bool EnsureVendorSourceInvectorAnimations()
        {
            bool changed = false;
            changed |= MoveAssetIfExists(
                CCS_CharacterControllerConstants.LegacyInvectorAnimationsRootPath + "/Shooter_UpperBodyPoses.fbx",
                CCS_CharacterControllerConstants.InvectorUpperBodyPosesFbxPath);
            changed |= MoveAssetIfExists(
                CCS_CharacterControllerConstants.LegacyInvectorAnimationsRootPath + "/Shooter_Shot&Reload.fbx",
                CCS_CharacterControllerConstants.InvectorShotReloadFbxPath);
            return changed;
        }

        private static bool MoveAssetIfExists(string sourcePath, string destinationPath)
        {
            sourcePath = sourcePath.Replace('\\', '/');
            destinationPath = destinationPath.Replace('\\', '/');

            if (!File.Exists(sourcePath))
            {
                return false;
            }

            if (File.Exists(destinationPath))
            {
                AssetDatabase.DeleteAsset(sourcePath);
                return true;
            }

            EnsureFolder(Path.GetDirectoryName(destinationPath)?.Replace('\\', '/'));
            string moveResult = AssetDatabase.MoveAsset(sourcePath, destinationPath);
            if (!string.IsNullOrEmpty(moveResult))
            {
                Debug.LogError("[Animation Isolation] Failed to move " + sourcePath + " -> " + destinationPath + ": " + moveResult);
                return false;
            }

            return true;
        }

        private static bool CleanupLegacyInvectorContent()
        {
            const string legacyRoot = "Assets/Invector-3rdPersonController";
            bool changed = false;

            if (AssetDatabase.IsValidFolder(legacyRoot))
            {
                changed = AssetDatabase.DeleteAsset(legacyRoot) || changed;
            }

            string legacyRootFullPath = Path.GetFullPath(legacyRoot);
            if (Directory.Exists(legacyRootFullPath))
            {
                Directory.Delete(legacyRootFullPath, recursive: true);
                changed = true;
            }

            string legacyRootMetaPath = legacyRoot + ".meta";
            if (File.Exists(legacyRootMetaPath))
            {
                File.Delete(legacyRootMetaPath);
                changed = true;
            }

            if (changed)
            {
                AssetDatabase.Refresh();
            }

            return changed;
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
                isolatedClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(entry.DestinationAssetPath);
                return isolatedClip != null;
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

            if (entry.LoopTimeOverride.HasValue)
            {
                changed |= EnsureClipLoopTime(isolatedClip, entry.LoopTimeOverride.Value);
            }

            EditorUtility.SetDirty(isolatedClip);
            return true;
        }

        private static bool EnsureClipLoopTime(AnimationClip clip, bool loopTime)
        {
            if (clip == null)
            {
                return false;
            }

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            if (settings.loopTime == loopTime)
            {
                return false;
            }

            settings.loopTime = loopTime;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
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

        private static bool EnsureAimLocomotionAnimatorParameters()
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            if (controller == null)
            {
                return false;
            }

            bool changed = false;
            changed |= EnsureAnimatorParameter(
                controller,
                CCS_CharacterControllerConstants.AnimatorIsAimingMovementModeParameter,
                AnimatorControllerParameterType.Bool);
            changed |= EnsureAnimatorParameter(
                controller,
                CCS_CharacterControllerConstants.AnimatorAimMoveXParameter,
                AnimatorControllerParameterType.Float);
            changed |= EnsureAnimatorParameter(
                controller,
                CCS_CharacterControllerConstants.AnimatorAimMoveYParameter,
                AnimatorControllerParameterType.Float);

            if (changed)
            {
                EditorUtility.SetDirty(controller);
            }

            return changed;
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

        private static bool EnsureAimStrafeLocomotionState(
            Dictionary<string, AnimationClip> aimStrafeClips,
            Dictionary<string, AnimationClip> clipsByStateName)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            if (controller == null)
            {
                return false;
            }

            AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;
            bool changed = false;

            AnimatorState aimState = FindState(rootStateMachine, CCS_CharacterControllerConstants.AnimatorAimStrafeLocomotionStateName);
            if (aimState == null)
            {
                aimState = rootStateMachine.AddState(
                    CCS_CharacterControllerConstants.AnimatorAimStrafeLocomotionStateName,
                    new Vector3(300f, 350f, 0f));
                changed = true;
            }

            BlendTree blendTree = aimState.motion as BlendTree;
            if (blendTree == null || AssetDatabase.GetAssetPath(blendTree) != AssetDatabase.GetAssetPath(controller))
            {
                blendTree = new BlendTree
                {
                    name = CCS_CharacterControllerConstants.AnimatorAimStrafeBlendTreeName,
                    hideFlags = HideFlags.HideInHierarchy,
                    blendType = BlendTreeType.FreeformDirectional2D,
                    blendParameter = CCS_CharacterControllerConstants.AnimatorAimMoveXParameter,
                    blendParameterY = CCS_CharacterControllerConstants.AnimatorAimMoveYParameter
                };
                AssetDatabase.AddObjectToAsset(blendTree, controller);
                aimState.motion = blendTree;
                changed = true;
            }

            if (blendTree.blendType != BlendTreeType.FreeformDirectional2D)
            {
                blendTree.blendType = BlendTreeType.FreeformDirectional2D;
                changed = true;
            }

            if (blendTree.blendParameter != CCS_CharacterControllerConstants.AnimatorAimMoveXParameter)
            {
                blendTree.blendParameter = CCS_CharacterControllerConstants.AnimatorAimMoveXParameter;
                changed = true;
            }

            if (blendTree.blendParameterY != CCS_CharacterControllerConstants.AnimatorAimMoveYParameter)
            {
                blendTree.blendParameterY = CCS_CharacterControllerConstants.AnimatorAimMoveYParameter;
                changed = true;
            }

            ChildMotion[] children = new ChildMotion[AimStrafeBlendPoints.Length];
            for (int i = 0; i < AimStrafeBlendPoints.Length; i++)
            {
                AimStrafeBlendPoint point = AimStrafeBlendPoints[i];
                AnimationClip clip = ResolveBlendClip(point.DestinationAssetPath, aimStrafeClips, clipsByStateName);
                if (clip == null)
                {
                    Debug.LogError(
                        "[Animation Isolation] Missing aim strafe blend clip at "
                        + point.DestinationAssetPath);
                    continue;
                }

                children[i] = new ChildMotion
                {
                    motion = clip,
                    position = point.Position,
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
            }

            if (changed)
            {
                EditorUtility.SetDirty(blendTree);
                EditorUtility.SetDirty(aimState);
                EditorUtility.SetDirty(controller);
            }

            AnimatorState idleState = FindState(rootStateMachine, "Idle");
            AnimatorState walkState = FindState(rootStateMachine, "Walk");
            AnimatorState sprintState = FindState(rootStateMachine, "Sprint");

            changed |= EnsureBoolTransition(
                idleState,
                aimState,
                CCS_CharacterControllerConstants.AnimatorIsAimingMovementModeParameter,
                expectedTrue: true);
            changed |= EnsureBoolTransition(
                walkState,
                aimState,
                CCS_CharacterControllerConstants.AnimatorIsAimingMovementModeParameter,
                expectedTrue: true);
            changed |= EnsureBoolTransition(
                sprintState,
                aimState,
                CCS_CharacterControllerConstants.AnimatorIsAimingMovementModeParameter,
                expectedTrue: true);
            changed |= EnsureBoolTransition(
                aimState,
                idleState,
                CCS_CharacterControllerConstants.AnimatorIsAimingMovementModeParameter,
                expectedTrue: false);

            if (changed)
            {
                EditorUtility.SetDirty(rootStateMachine);
                EditorUtility.SetDirty(controller);
            }

            return changed;
        }

        private static AnimationClip ResolveBlendClip(
            string destinationAssetPath,
            Dictionary<string, AnimationClip> aimStrafeClips,
            Dictionary<string, AnimationClip> clipsByStateName)
        {
            if (aimStrafeClips.TryGetValue(destinationAssetPath, out AnimationClip aimClip))
            {
                return aimClip;
            }

            return AssetDatabase.LoadAssetAtPath<AnimationClip>(destinationAssetPath);
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

        private static bool EnsureBoolTransition(
            AnimatorState fromState,
            AnimatorState toState,
            string parameterName,
            bool expectedTrue,
            float duration = -1f,
            bool hasExitTime = false,
            float exitTime = 0f)
        {
            if (fromState == null || toState == null)
            {
                return false;
            }

            if (duration < 0f)
            {
                duration = AimStrafeTransitionDuration;
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
                    && Mathf.Approximately(existing.duration, duration)
                    && existing.hasExitTime == hasExitTime
                    && (!hasExitTime || Mathf.Approximately(existing.exitTime, exitTime)))
                {
                    return false;
                }
            }

            AnimatorStateTransition transition = fromState.AddTransition(toState);
            transition.hasExitTime = hasExitTime;
            transition.exitTime = exitTime;
            transition.duration = duration;
            transition.AddCondition(
                expectedTrue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
                0f,
                parameterName);
            EditorUtility.SetDirty(transition);
            EditorUtility.SetDirty(fromState);
            return true;
        }

        private static bool HasBoolCondition(
            AnimatorStateTransition transition,
            string parameterName,
            bool expectedTrue)
        {
            AnimatorCondition[] conditions = transition.conditions;
            for (int i = 0; i < conditions.Length; i++)
            {
                AnimatorCondition condition = conditions[i];
                if (condition.parameter != parameterName)
                {
                    continue;
                }

                if (expectedTrue && condition.mode == AnimatorConditionMode.If)
                {
                    return true;
                }

                if (!expectedTrue && condition.mode == AnimatorConditionMode.IfNot)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool EnsureRevolverUpperBodyMask()
        {
            string maskPath = CCS_CharacterControllerConstants.RevolverUpperBodyMaskPath;
            AvatarMask mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(maskPath);
            bool changed = false;
            if (mask == null)
            {
                mask = new AvatarMask();
                AssetDatabase.CreateAsset(mask, maskPath);
                changed = true;
            }

            bool maskChanged = ConfigureRevolverUpperBodyMask(mask);
            if (maskChanged)
            {
                EditorUtility.SetDirty(mask);
                changed = true;
            }

            return changed;
        }

        private static bool ConfigureRevolverUpperBodyMask(AvatarMask mask)
        {
            bool changed = false;
            AvatarMaskBodyPart[] activeParts =
            {
                AvatarMaskBodyPart.Body,
                AvatarMaskBodyPart.Head,
                AvatarMaskBodyPart.LeftArm,
                AvatarMaskBodyPart.RightArm,
                AvatarMaskBodyPart.LeftFingers,
                AvatarMaskBodyPart.RightFingers
            };

            AvatarMaskBodyPart[] inactiveParts =
            {
                AvatarMaskBodyPart.Root,
                AvatarMaskBodyPart.LeftLeg,
                AvatarMaskBodyPart.RightLeg
            };

            for (int i = 0; i < activeParts.Length; i++)
            {
                if (!mask.GetHumanoidBodyPartActive(activeParts[i]))
                {
                    mask.SetHumanoidBodyPartActive(activeParts[i], true);
                    changed = true;
                }
            }

            for (int i = 0; i < inactiveParts.Length; i++)
            {
                if (mask.GetHumanoidBodyPartActive(inactiveParts[i]))
                {
                    mask.SetHumanoidBodyPartActive(inactiveParts[i], false);
                    changed = true;
                }
            }

            return changed;
        }

        private static bool EnsureRevolverRightArmMask()
        {
            return CCS_RevolverUpperBodyRightArmAimMaskUtility.EnsureMaskAsset();
        }

        private static bool ConfigureRevolverRightArmMask(AvatarMask mask)
        {
            return CCS_RevolverUpperBodyRightArmAimMaskUtility.ConfigureMask(mask);
        }

        private static bool EnsureRevolverRightHandPreviewLayer(Dictionary<string, AnimationClip> previewClips)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            if (controller == null)
            {
                return false;
            }

            AvatarMask mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(
                CCS_CharacterControllerConstants.WildWestRevolverRightArmMaskPath);
            if (mask == null)
            {
                Debug.LogError("[Wild West Preview] Missing right-arm mask.");
                return false;
            }

            bool changed = false;
            int layerIndex = FindLayerIndex(controller, CCS_CharacterControllerConstants.AnimatorRevolverRightHandPreviewLayerName);
            if (layerIndex < 0)
            {
                controller.AddLayer(CCS_CharacterControllerConstants.AnimatorRevolverRightHandPreviewLayerName);
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

                SerializedProperty defaultWeightProperty = layerProperty.FindPropertyRelative("m_DefaultWeight");
                if (defaultWeightProperty != null && defaultWeightProperty.floatValue > 0.001f)
                {
                    defaultWeightProperty.floatValue = 0f;
                    changed = true;
                }
            }

            serializedController.ApplyModifiedPropertiesWithoutUndo();
            layer = controller.layers[layerIndex];
            EditorUtility.SetDirty(controller);

            AnimatorStateMachine stateMachine = layer.stateMachine;
            if (stateMachine == null)
            {
                return changed;
            }

            previewClips.TryGetValue(
                CCS_CharacterControllerConstants.AnimatorRevolverWildWestAimIdleStateName,
                out AnimationClip aimIdleClip);
            previewClips.TryGetValue(
                CCS_CharacterControllerConstants.AnimatorRevolverWildWestFireStateName,
                out AnimationClip fireClip);
            previewClips.TryGetValue(
                CCS_CharacterControllerConstants.AnimatorRevolverWildWestReloadStateName,
                out AnimationClip reloadClip);

            AnimatorState emptyState = EnsureRevolverState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorRevolverWildWestEmptyStateName,
                null,
                new Vector3(900f, 0f, 0f),
                ref changed);
            AnimatorState aimIdleState = EnsureRevolverState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorRevolverWildWestAimIdleStateName,
                aimIdleClip,
                new Vector3(900f, 120f, 0f),
                ref changed);
            AnimatorState fireState = EnsureRevolverState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorRevolverWildWestFireStateName,
                fireClip,
                new Vector3(1160f, 120f, 0f),
                ref changed);
            AnimatorState reloadState = EnsureRevolverState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorRevolverWildWestReloadStateName,
                reloadClip,
                new Vector3(1160f, 260f, 0f),
                ref changed);

            if (stateMachine.defaultState != emptyState)
            {
                stateMachine.defaultState = emptyState;
                changed = true;
            }

            changed |= EnsureBoolTransition(
                emptyState,
                aimIdleState,
                CCS_CharacterControllerConstants.AnimatorRevolverAimHeldParameter,
                expectedTrue: true,
                duration: RevolverAimInDuration);
            changed |= EnsureBoolTransition(
                aimIdleState,
                emptyState,
                CCS_CharacterControllerConstants.AnimatorRevolverAimHeldParameter,
                expectedTrue: false,
                duration: RevolverAimOutDuration);
            changed |= EnsureTriggerTransition(
                stateMachine,
                fireState,
                CCS_CharacterControllerConstants.AnimatorRevolverFireTriggerParameter,
                duration: RevolverFireInDuration,
                fromAnyState: true);
            changed |= EnsureTriggerTransition(
                stateMachine,
                fireState,
                CCS_CharacterControllerConstants.AnimatorRevolverFireTriggerParameter,
                duration: RevolverFireInDuration,
                fromAnyState: false,
                fromState: aimIdleState);
            changed |= EnsureTriggerTransition(
                stateMachine,
                reloadState,
                CCS_CharacterControllerConstants.AnimatorRevolverReloadTriggerParameter,
                duration: RevolverReloadInDuration,
                fromAnyState: false,
                fromState: aimIdleState);
            changed |= EnsureBoolTransition(
                fireState,
                aimIdleState,
                CCS_CharacterControllerConstants.AnimatorRevolverAimHeldParameter,
                expectedTrue: true,
                duration: RevolverFireOutDuration,
                hasExitTime: true,
                exitTime: RevolverFireExitTime);
            changed |= EnsureBoolTransition(
                fireState,
                emptyState,
                CCS_CharacterControllerConstants.AnimatorRevolverAimHeldParameter,
                expectedTrue: false,
                duration: RevolverFireOutDuration,
                hasExitTime: true,
                exitTime: RevolverFireExitTime);

            if (changed)
            {
                controller.layers[layerIndex] = layer;
                EditorUtility.SetDirty(stateMachine);
                EditorUtility.SetDirty(controller);
            }

            return changed;
        }

        private static bool EnsureRevolverAnimatorParameters()
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            if (controller == null)
            {
                return false;
            }

            bool changed = false;
            changed |= EnsureAnimatorParameter(
                controller,
                CCS_CharacterControllerConstants.AnimatorRevolverAimHeldParameter,
                AnimatorControllerParameterType.Bool);
            changed |= EnsureAnimatorParameter(
                controller,
                CCS_CharacterControllerConstants.AnimatorRevolverFireTriggerParameter,
                AnimatorControllerParameterType.Trigger);
            changed |= EnsureAnimatorParameter(
                controller,
                CCS_CharacterControllerConstants.AnimatorRevolverReloadTriggerParameter,
                AnimatorControllerParameterType.Trigger);
            changed |= EnsureAnimatorParameter(
                controller,
                CCS_CharacterControllerConstants.AnimatorRevolverIsReloadingParameter,
                AnimatorControllerParameterType.Bool);

            if (changed)
            {
                EditorUtility.SetDirty(controller);
            }

            return changed;
        }

        private static bool EnsureRevolverUpperBodyLayer(Dictionary<string, AnimationClip> revolverClips)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            if (controller == null)
            {
                return false;
            }

            AvatarMask mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(
                CCS_CharacterControllerConstants.RevolverUpperBodyMaskPath);
            if (mask == null)
            {
                Debug.LogError("[Animation Isolation] Missing revolver upper-body mask.");
                return false;
            }

            bool changed = false;
            int layerIndex = FindLayerIndex(controller, CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName);
            if (layerIndex < 0)
            {
                controller.AddLayer(CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName);
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

                SerializedProperty defaultWeightProperty = layerProperty.FindPropertyRelative("m_DefaultWeight");
                if (defaultWeightProperty != null && defaultWeightProperty.floatValue > 0.001f)
                {
                    defaultWeightProperty.floatValue = 0f;
                    changed = true;
                }

                SerializedProperty blendingModeProperty = layerProperty.FindPropertyRelative("m_BlendingMode");
                if (blendingModeProperty != null && blendingModeProperty.intValue != (int)AnimatorLayerBlendingMode.Override)
                {
                    blendingModeProperty.intValue = (int)AnimatorLayerBlendingMode.Override;
                    changed = true;
                }

                SerializedProperty ikPassProperty = layerProperty.FindPropertyRelative("m_IKPass");
                if (ikPassProperty != null && ikPassProperty.boolValue)
                {
                    ikPassProperty.boolValue = false;
                    changed = true;
                }
            }

            serializedController.ApplyModifiedPropertiesWithoutUndo();
            layer = controller.layers[layerIndex];
            EditorUtility.SetDirty(controller);

            AnimatorStateMachine stateMachine = layer.stateMachine;
            if (stateMachine == null)
            {
                return changed;
            }

            changed |= ResetRevolverUpperBodyStateMachine(stateMachine);

            AnimatorState emptyState = EnsureRevolverState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorRevolverEmptyStateName,
                null,
                new Vector3(300f, 0f, 0f),
                ref changed);
            AnimatorState idleToAimState = EnsureRevolverState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorRevolverIdleToAimStateName,
                revolverClips[CCS_CharacterControllerConstants.AnimatorRevolverIdleToAimStateName],
                new Vector3(300f, 110f, 0f),
                ref changed);
            AnimatorState aimIdleFullDrawState = EnsureRevolverState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorRevolverAimIdleFullDrawStateName,
                revolverClips[CCS_CharacterControllerConstants.AnimatorRevolverAimIdleFullDrawStateName],
                new Vector3(300f, 220f, 0f),
                ref changed);
            AnimatorState aimPitchBlendState = EnsureRevolverAimPitchBlendState(stateMachine, controller, ref changed);
            AnimatorState aimLoopState = aimPitchBlendState != null ? aimPitchBlendState : aimIdleFullDrawState;
            AnimatorState aimToIdleState = EnsureRevolverState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorRevolverAimToIdleStateName,
                revolverClips[CCS_CharacterControllerConstants.AnimatorRevolverAimToIdleStateName],
                new Vector3(300f, 330f, 0f),
                ref changed);
            AnimatorState walkToAimWalkState = EnsureRevolverState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorRevolverWalkToAimWalkStateName,
                revolverClips[CCS_CharacterControllerConstants.AnimatorRevolverWalkToAimWalkStateName],
                new Vector3(600f, 110f, 0f),
                ref changed);
            AnimatorState aimWalkState = EnsureRevolverState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorRevolverAimWalkStateName,
                revolverClips[CCS_CharacterControllerConstants.AnimatorRevolverAimWalkStateName],
                new Vector3(600f, 220f, 0f),
                ref changed);
            AnimatorState aimWalkToWalkState = EnsureRevolverState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorRevolverAimWalkToWalkStateName,
                revolverClips[CCS_CharacterControllerConstants.AnimatorRevolverAimWalkToWalkStateName],
                new Vector3(600f, 330f, 0f),
                ref changed);
            AnimatorState fireState = EnsureRevolverState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorRevolverFireStateName,
                revolverClips[CCS_CharacterControllerConstants.AnimatorRevolverFireStateName],
                new Vector3(900f, 220f, 0f),
                ref changed);
            AnimatorState reloadState = EnsureRevolverState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorRevolverReloadStateName,
                revolverClips[CCS_CharacterControllerConstants.AnimatorRevolverReloadStateName],
                new Vector3(900f, 360f, 0f),
                ref changed);

            if (stateMachine.defaultState != emptyState)
            {
                stateMachine.defaultState = emptyState;
                changed = true;
            }

            string aimHeld = CCS_CharacterControllerConstants.AnimatorRevolverAimHeldParameter;
            string isMoving = CCS_CharacterControllerConstants.AnimatorRevolverIsMovingParameter;
            string isReloading = CCS_CharacterControllerConstants.AnimatorRevolverIsReloadingParameter;

            changed |= EnsureDualBoolTransition(
                emptyState,
                idleToAimState,
                aimHeld,
                expectedFirstTrue: true,
                isMoving,
                expectedSecondTrue: false,
                duration: RevolverAimInDuration);
            changed |= EnsureDualBoolTransition(
                emptyState,
                walkToAimWalkState,
                aimHeld,
                expectedFirstTrue: true,
                isMoving,
                expectedSecondTrue: true,
                duration: RevolverAimInDuration);

            changed |= EnsureExitTimeTransition(
                idleToAimState,
                aimLoopState,
                duration: RevolverAimInDuration,
                exitTime: RevolverAimEnterExitTime);
            changed |= EnsureBoolTransition(
                idleToAimState,
                emptyState,
                aimHeld,
                expectedTrue: false,
                duration: RevolverAimOutDuration);
            changed |= EnsureDualBoolTransition(
                idleToAimState,
                walkToAimWalkState,
                aimHeld,
                expectedFirstTrue: true,
                isMoving,
                expectedSecondTrue: true,
                duration: RevolverAimLoopCrossfadeDuration);

            changed |= EnsureBoolTransition(
                aimLoopState,
                aimToIdleState,
                aimHeld,
                expectedTrue: false,
                duration: RevolverAimOutDuration);
            changed |= EnsureDualBoolTransition(
                aimLoopState,
                aimWalkState,
                aimHeld,
                expectedFirstTrue: true,
                isMoving,
                expectedSecondTrue: true,
                duration: RevolverAimLoopCrossfadeDuration);

            changed |= EnsureExitTimeTransition(
                aimToIdleState,
                emptyState,
                duration: RevolverAimOutDuration,
                exitTime: RevolverAimEnterExitTime);

            changed |= EnsureExitTimeTransition(
                walkToAimWalkState,
                aimWalkState,
                duration: RevolverAimInDuration,
                exitTime: RevolverAimEnterExitTime);
            changed |= EnsureBoolTransition(
                walkToAimWalkState,
                emptyState,
                aimHeld,
                expectedTrue: false,
                duration: RevolverAimOutDuration);

            changed |= EnsureBoolTransition(
                aimWalkState,
                aimWalkToWalkState,
                aimHeld,
                expectedTrue: false,
                duration: RevolverAimOutDuration);
            changed |= EnsureDualBoolTransition(
                aimWalkState,
                aimLoopState,
                aimHeld,
                expectedFirstTrue: true,
                isMoving,
                expectedSecondTrue: false,
                duration: RevolverAimLoopCrossfadeDuration);

            changed |= EnsureExitTimeTransition(
                aimWalkToWalkState,
                emptyState,
                duration: RevolverAimOutDuration,
                exitTime: RevolverAimEnterExitTime);

            changed |= EnsureDualBoolTransition(
                fireState,
                aimLoopState,
                aimHeld,
                expectedFirstTrue: true,
                isMoving,
                expectedSecondTrue: false,
                duration: RevolverFireOutDuration,
                hasExitTime: true,
                exitTime: RevolverFireExitTime);
            changed |= EnsureDualBoolTransition(
                fireState,
                aimWalkState,
                aimHeld,
                expectedFirstTrue: true,
                isMoving,
                expectedSecondTrue: true,
                duration: RevolverFireOutDuration,
                hasExitTime: true,
                exitTime: RevolverFireExitTime);
            changed |= EnsureBoolTransition(
                fireState,
                emptyState,
                aimHeld,
                expectedTrue: false,
                duration: RevolverFireOutDuration,
                hasExitTime: true,
                exitTime: RevolverFireExitTime);

            changed |= EnsureTriggerTransition(
                stateMachine,
                reloadState,
                CCS_CharacterControllerConstants.AnimatorRevolverReloadTriggerParameter,
                duration: RevolverReloadInDuration,
                fromAnyState: false,
                fromState: aimLoopState);
            changed |= EnsureTriggerTransition(
                stateMachine,
                reloadState,
                CCS_CharacterControllerConstants.AnimatorRevolverReloadTriggerParameter,
                duration: RevolverReloadInDuration,
                fromAnyState: false,
                fromState: aimWalkState);
            changed |= EnsureBoolTransition(
                aimLoopState,
                reloadState,
                isReloading,
                expectedTrue: true,
                duration: RevolverReloadInDuration);
            changed |= EnsureBoolTransition(
                aimWalkState,
                reloadState,
                isReloading,
                expectedTrue: true,
                duration: RevolverReloadInDuration);
            changed |= EnsureTripleBoolTransition(
                reloadState,
                aimLoopState,
                aimHeld,
                expectedFirstTrue: true,
                isReloading,
                expectedSecondTrue: false,
                isMoving,
                expectedThirdTrue: false,
                duration: RevolverReloadOutDuration,
                hasExitTime: true,
                exitTime: RevolverReloadExitTime);
            changed |= EnsureTripleBoolTransition(
                reloadState,
                aimWalkState,
                aimHeld,
                expectedFirstTrue: true,
                isReloading,
                expectedSecondTrue: false,
                isMoving,
                expectedThirdTrue: true,
                duration: RevolverReloadOutDuration,
                hasExitTime: true,
                exitTime: RevolverReloadExitTime);
            changed |= EnsureDualBoolTransition(
                reloadState,
                emptyState,
                aimHeld,
                expectedFirstTrue: false,
                isReloading,
                expectedSecondTrue: false,
                duration: RevolverReloadOutDuration,
                hasExitTime: true,
                exitTime: RevolverReloadExitTime);

            if (changed)
            {
                controller.layers[layerIndex] = layer;
                EditorUtility.SetDirty(stateMachine);
                EditorUtility.SetDirty(controller);
            }

            return changed;
        }

        private static bool ResetRevolverUpperBodyStateMachine(AnimatorStateMachine stateMachine)
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

            if (changed)
            {
                EditorUtility.SetDirty(stateMachine);
            }

            return changed;
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
            EditorUtility.SetDirty(transition);
            EditorUtility.SetDirty(fromState);
            return true;
        }

        private static bool EnsureTripleBoolTransition(
            AnimatorState fromState,
            AnimatorState toState,
            string firstParameterName,
            bool expectedFirstTrue,
            string secondParameterName,
            bool expectedSecondTrue,
            string thirdParameterName,
            bool expectedThirdTrue,
            float duration,
            bool hasExitTime = false,
            float exitTime = 0f)
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

                if (HasTripleBoolConditions(
                        existing,
                        firstParameterName,
                        expectedFirstTrue,
                        secondParameterName,
                        expectedSecondTrue,
                        thirdParameterName,
                        expectedThirdTrue)
                    && Mathf.Approximately(existing.duration, duration)
                    && existing.hasExitTime == hasExitTime
                    && (!hasExitTime || Mathf.Approximately(existing.exitTime, exitTime)))
                {
                    return false;
                }
            }

            AnimatorStateTransition transition = fromState.AddTransition(toState);
            transition.hasExitTime = hasExitTime;
            transition.exitTime = exitTime;
            transition.duration = duration;
            transition.AddCondition(
                expectedFirstTrue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
                0f,
                firstParameterName);
            transition.AddCondition(
                expectedSecondTrue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
                0f,
                secondParameterName);
            transition.AddCondition(
                expectedThirdTrue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
                0f,
                thirdParameterName);
            EditorUtility.SetDirty(transition);
            EditorUtility.SetDirty(fromState);
            return true;
        }

        private static bool HasTripleBoolConditions(
            AnimatorStateTransition transition,
            string firstParameterName,
            bool expectedFirstTrue,
            string secondParameterName,
            bool expectedSecondTrue,
            string thirdParameterName,
            bool expectedThirdTrue)
        {
            AnimatorCondition[] conditions = transition.conditions;
            if (conditions.Length != 3)
            {
                return false;
            }

            bool hasFirst = false;
            bool hasSecond = false;
            bool hasThird = false;
            for (int i = 0; i < conditions.Length; i++)
            {
                AnimatorCondition condition = conditions[i];
                if (condition.parameter == firstParameterName)
                {
                    hasFirst = expectedFirstTrue
                        ? condition.mode == AnimatorConditionMode.If
                        : condition.mode == AnimatorConditionMode.IfNot;
                }

                if (condition.parameter == secondParameterName)
                {
                    hasSecond = expectedSecondTrue
                        ? condition.mode == AnimatorConditionMode.If
                        : condition.mode == AnimatorConditionMode.IfNot;
                }

                if (condition.parameter == thirdParameterName)
                {
                    hasThird = expectedThirdTrue
                        ? condition.mode == AnimatorConditionMode.If
                        : condition.mode == AnimatorConditionMode.IfNot;
                }
            }

            return hasFirst && hasSecond && hasThird;
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

        private static AnimatorState EnsureRevolverState(
            AnimatorStateMachine stateMachine,
            string stateName,
            AnimationClip clip,
            Vector3 position,
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
                EditorUtility.SetDirty(state);
                changed = true;
            }

            return state;
        }

        private static bool EnsureTriggerTransition(
            AnimatorStateMachine stateMachine,
            AnimatorState toState,
            string triggerParameterName,
            float duration,
            bool fromAnyState,
            AnimatorState fromState = null)
        {
            if (toState == null)
            {
                return false;
            }

            if (fromAnyState)
            {
                AnimatorStateTransition[] anyTransitions = stateMachine.anyStateTransitions;
                for (int i = 0; i < anyTransitions.Length; i++)
                {
                    AnimatorStateTransition existing = anyTransitions[i];
                    if (existing.destinationState != toState)
                    {
                        continue;
                    }

                    if (HasTriggerCondition(existing, triggerParameterName)
                        && Mathf.Approximately(existing.duration, duration))
                    {
                        return false;
                    }
                }

                AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(toState);
                transition.hasExitTime = false;
                transition.duration = duration;
                transition.AddCondition(AnimatorConditionMode.If, 0f, triggerParameterName);
                EditorUtility.SetDirty(transition);
                EditorUtility.SetDirty(stateMachine);
                return true;
            }

            if (fromState == null)
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

                if (HasTriggerCondition(existing, triggerParameterName)
                    && Mathf.Approximately(existing.duration, duration))
                {
                    return false;
                }
            }

            AnimatorStateTransition stateTransition = fromState.AddTransition(toState);
            stateTransition.hasExitTime = false;
            stateTransition.duration = duration;
            stateTransition.AddCondition(AnimatorConditionMode.If, 0f, triggerParameterName);
            EditorUtility.SetDirty(stateTransition);
            EditorUtility.SetDirty(fromState);
            return true;
        }

        private static bool EnsureDualBoolTransition(
            AnimatorState fromState,
            AnimatorState toState,
            string firstParameterName,
            bool expectedFirstTrue,
            string secondParameterName,
            bool expectedSecondTrue,
            float duration,
            bool hasExitTime = false,
            float exitTime = 0f)
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

                if (HasDualBoolConditions(
                        existing,
                        firstParameterName,
                        expectedFirstTrue,
                        secondParameterName,
                        expectedSecondTrue)
                    && Mathf.Approximately(existing.duration, duration)
                    && existing.hasExitTime == hasExitTime
                    && (!hasExitTime || Mathf.Approximately(existing.exitTime, exitTime)))
                {
                    return false;
                }
            }

            AnimatorStateTransition transition = fromState.AddTransition(toState);
            transition.hasExitTime = hasExitTime;
            transition.exitTime = exitTime;
            transition.duration = duration;
            transition.AddCondition(
                expectedFirstTrue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
                0f,
                firstParameterName);
            transition.AddCondition(
                expectedSecondTrue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
                0f,
                secondParameterName);
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

            return HasBoolCondition(transition, parameterName, expectedTrue);
        }

        private static bool HasDualBoolConditions(
            AnimatorStateTransition transition,
            string firstParameterName,
            bool expectedFirstTrue,
            string secondParameterName,
            bool expectedSecondTrue)
        {
            AnimatorCondition[] conditions = transition.conditions;
            if (conditions.Length != 2)
            {
                return false;
            }

            bool hasFirst = false;
            bool hasSecond = false;
            for (int i = 0; i < conditions.Length; i++)
            {
                AnimatorCondition condition = conditions[i];
                if (condition.parameter == firstParameterName)
                {
                    hasFirst = expectedFirstTrue
                        ? condition.mode == AnimatorConditionMode.If
                        : condition.mode == AnimatorConditionMode.IfNot;
                }

                if (condition.parameter == secondParameterName)
                {
                    hasSecond = expectedSecondTrue
                        ? condition.mode == AnimatorConditionMode.If
                        : condition.mode == AnimatorConditionMode.IfNot;
                }
            }

            return hasFirst && hasSecond;
        }

        private static bool HasTriggerCondition(AnimatorTransitionBase transition, string triggerParameterName)
        {
            AnimatorCondition[] conditions = transition.conditions;
            for (int i = 0; i < conditions.Length; i++)
            {
                AnimatorCondition condition = conditions[i];
                if (condition.parameter == triggerParameterName
                    && condition.mode == AnimatorConditionMode.If)
                {
                    return true;
                }
            }

            return false;
        }

        private static AnimatorState EnsureRevolverAimPitchBlendState(
            AnimatorStateMachine stateMachine,
            AnimatorController controller,
            ref bool changed)
        {
            AnimatorState aimPitchBlendState = FindState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorRevolverAimPitchBlendStateName);
            if (aimPitchBlendState == null)
            {
                aimPitchBlendState = stateMachine.AddState(
                    CCS_CharacterControllerConstants.AnimatorRevolverAimPitchBlendStateName,
                    new Vector3(520f, 220f, 0f));
                changed = true;
            }

            BlendTree blendTree = aimPitchBlendState.motion as BlendTree;
            if (blendTree == null || AssetDatabase.GetAssetPath(blendTree) != AssetDatabase.GetAssetPath(controller))
            {
                blendTree = new BlendTree
                {
                    name = CCS_CharacterControllerConstants.AnimatorRevolverAimPitchBlendTreeName,
                    hideFlags = HideFlags.HideInHierarchy,
                    blendType = BlendTreeType.Simple1D,
                    blendParameter = CCS_CharacterControllerConstants.AnimatorRevolverAimPitchParameter,
                    useAutomaticThresholds = false
                };
                AssetDatabase.AddObjectToAsset(blendTree, controller);
                aimPitchBlendState.motion = blendTree;
                changed = true;
            }

            if (blendTree.blendType != BlendTreeType.Simple1D)
            {
                blendTree.blendType = BlendTreeType.Simple1D;
                changed = true;
            }

            if (blendTree.blendParameter != CCS_CharacterControllerConstants.AnimatorRevolverAimPitchParameter)
            {
                blendTree.blendParameter = CCS_CharacterControllerConstants.AnimatorRevolverAimPitchParameter;
                changed = true;
            }

            AnimationClip downClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                CCS_CharacterControllerConstants.RevolverAimPitchDownFitTestClipPath);
            AnimationClip centerClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                CCS_CharacterControllerConstants.RevolverAimPitchCenterFitTestClipPath);
            AnimationClip upClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                CCS_CharacterControllerConstants.RevolverAimPitchUpFitTestClipPath);
            if (downClip != null && centerClip != null && upClip != null)
            {
                changed |= EnsureAimPitchBlendTreeChildren(blendTree, downClip, centerClip, upClip);
            }

            return aimPitchBlendState;
        }

        private static bool EnsureAimPitchBlendTreeChildren(
            BlendTree blendTree,
            AnimationClip downClip,
            AnimationClip centerClip,
            AnimationClip upClip)
        {
            ChildMotion[] children =
            {
                new ChildMotion { motion = downClip, threshold = -1f, timeScale = 1f },
                new ChildMotion { motion = centerClip, threshold = 0f, timeScale = 1f },
                new ChildMotion { motion = upClip, threshold = 1f, timeScale = 1f },
            };

            if (BlendTreeChildrenMatch(blendTree.children, children))
            {
                return false;
            }

            blendTree.children = children;
            return true;
        }

        #endregion
    }
}
