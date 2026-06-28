using System.Collections.Generic;
using System.IO;
using CCS.Modules.Interaction;
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
// NOTES: v0.7.2 — Base Layer locomotion only; RevolverUpperBody owns aim + aim strafe; Interaction owns pickup.
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
        private const float AimStrafeTransitionDuration = 0.1f;
        private const float InteractionTransitionDuration = 0.1f;
        private const float InteractionReturnExitTime = 0.9f;
        private const int CanonicalRevolverUpperBodyLayerIndex = 1;
        private const int CanonicalInteractionLayerIndex = 2;

        private readonly struct AimStrafeBlendPoint
        {
            public AimStrafeBlendPoint(string clipAssetPath, Vector2 position)
            {
                ClipAssetPath = clipAssetPath;
                Position = position;
            }

            public string ClipAssetPath { get; }

            public Vector2 Position { get; }
        }

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
            EnsureRevolverAimFolders();
            bool changed = EnsureRevolverAimClipMigration();
            changed |= EnsureRevolverAimRightArmMask();
            changed |= EnsureInteractionReservedLayer();
            changed |= EnsureSimplifiedRevolverAimLayer();
            changed |= RemoveRevolverAimPitchFromController();
            changed |= EnsureAimStrafeOnRevolverUpperBodyLayer();
            changed |= EnsureInteractionLayer();
            changed |= RemoveBaseLayerLegacyAimStates();
            changed |= DeleteObsoleteRevolverAimAssets();

            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return changed;
        }

        public static bool EnsureAnimatorLayerCleanupPass()
        {
            AnimatorController controller = LoadPlayerController();
            bool changed = false;
            if (controller != null)
            {
                changed |= CCS_AnimatorClipReconnectBuilder.EnsureLayerDefaultWeights(controller);
            }

            changed |= EnsureInteractionReservedLayer();
            changed |= EnsureAimStrafeOnRevolverUpperBodyLayer();
            changed |= EnsureInteractionLayer();
            changed |= RemoveBaseLayerLegacyAimStates();
            changed |= CCS_AnimatorClipReconnectBuilder.EnsurePlayerAnimatorClipReconnect(out _);

            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return changed;
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
            changed |= RemoveInteractionTriggerTransitionsFromStateMachine(stateMachine);

            string[] forbiddenStates =
            {
                CCS_CharacterControllerConstants.AnimatorAimStrafeLocomotionStateName,
                CCS_CharacterControllerConstants.AnimatorAimStrafeBlendTreeName,
                CCS_CharacterControllerConstants.AnimatorInteractPickUpStateName,
                CCS_CharacterControllerConstants.AnimatorInteractWalkThroughDoorStateName,
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

        public static bool EnsureAimStrafeOnRevolverUpperBodyLayer()
        {
            AnimatorController controller = LoadPlayerController();
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

            bool changed = false;
            changed |= EnsureAimLocomotionParameters(controller);

            AnimatorStateMachine stateMachine = controller.layers[layerIndex].stateMachine;
            if (stateMachine == null)
            {
                return changed;
            }

            AnimatorState noAimState = FindState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorRevolverNoAimStateName);

            AnimatorState aimState = FindState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorAimStrafeLocomotionStateName);
            if (aimState == null)
            {
                aimState = stateMachine.AddState(
                    CCS_CharacterControllerConstants.AnimatorAimStrafeLocomotionStateName,
                    new Vector3(600f, 120f, 0f));
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
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(point.ClipAssetPath);
                if (clip == null)
                {
                    Debug.LogError(
                        "[Animator Layer Cleanup] Missing aim strafe clip at "
                        + point.ClipAssetPath);
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

            if (aimState.writeDefaultValues)
            {
                aimState.writeDefaultValues = false;
                changed = true;
            }

            string aimMovementMode = CCS_CharacterControllerConstants.AnimatorIsAimingMovementModeParameter;
            changed |= EnsureAnyStateBoolTransition(
                stateMachine,
                aimState,
                aimMovementMode,
                expectedTrue: true,
                AimStrafeTransitionDuration);

            if (noAimState != null)
            {
                changed |= EnsureBoolTransition(
                    aimState,
                    noAimState,
                    aimMovementMode,
                    expectedTrue: false,
                    AimStrafeTransitionDuration);
            }

            if (changed)
            {
                EditorUtility.SetDirty(blendTree);
                EditorUtility.SetDirty(aimState);
                EditorUtility.SetDirty(stateMachine);
                EditorUtility.SetDirty(controller);
            }

            return changed;
        }

        public static bool EnsureInteractionLayer()
        {
            AnimatorController controller = LoadPlayerController();
            if (controller == null)
            {
                return false;
            }

            bool changed = EnsureInteractionReservedLayer();
            int layerIndex = FindLayerIndex(
                controller,
                CCS_CharacterControllerConstants.AnimatorInteractionReservedLayerName);
            if (layerIndex < 0)
            {
                return changed;
            }

            AnimationClip pickUpClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                CCS_CharacterControllerConstants.InteractionPickUpRightHandClipPath);
            AnimationClip walkThroughDoorClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                CCS_CharacterControllerConstants.InteractionWalkThroughDoorRightHandClipPath);
            if (pickUpClip == null || walkThroughDoorClip == null)
            {
                Debug.LogError(
                    "[Animator Layer Cleanup] Missing interaction clips. PickUp="
                    + (pickUpClip != null)
                    + " WalkThroughDoor="
                    + (walkThroughDoorClip != null));
                return changed;
            }

            changed |= EnsureAnimatorTriggerParameter(
                controller,
                CCS_CharacterControllerConstants.AnimatorPickUpRightHandTriggerParameter);
            changed |= EnsureAnimatorTriggerParameter(
                controller,
                CCS_CharacterControllerConstants.AnimatorWalkThroughDoorRightHandTriggerParameter);

            AnimatorControllerLayer layer = controller.layers[layerIndex];
            layer.defaultWeight = 0f;
            layer.blendingMode = AnimatorLayerBlendingMode.Override;
            controller.layers[layerIndex] = layer;

            AnimatorStateMachine stateMachine = layer.stateMachine;
            if (stateMachine == null)
            {
                return changed;
            }

            AnimatorState defaultState = FindState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorInteractionDefaultStateName);
            if (defaultState == null)
            {
                defaultState = stateMachine.AddState(
                    CCS_CharacterControllerConstants.AnimatorInteractionDefaultStateName,
                    new Vector3(300f, 0f, 0f));
                defaultState.motion = null;
                changed = true;
            }

            if (defaultState.writeDefaultValues)
            {
                defaultState.writeDefaultValues = false;
                changed = true;
            }

            if (stateMachine.defaultState != defaultState)
            {
                stateMachine.defaultState = defaultState;
                changed = true;
            }

            AnimatorState pickUpState = EnsureInteractionAnimationState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorInteractPickUpStateName,
                pickUpClip,
                new Vector3(300f, 120f, 0f),
                CCS_InteractionAnimationKey.PickUp_RH,
                ref changed);
            AnimatorState walkThroughDoorState = EnsureInteractionAnimationState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorInteractWalkThroughDoorStateName,
                walkThroughDoorClip,
                new Vector3(300f, 240f, 0f),
                CCS_InteractionAnimationKey.WalkThroughDoor_RH,
                ref changed);

            changed |= RemoveTriggerAnyStateTransitionExcept(
                stateMachine,
                pickUpState,
                CCS_CharacterControllerConstants.AnimatorPickUpRightHandTriggerParameter);
            changed |= RemoveTriggerAnyStateTransitionExcept(
                stateMachine,
                walkThroughDoorState,
                CCS_CharacterControllerConstants.AnimatorWalkThroughDoorRightHandTriggerParameter);
            changed |= EnsureAnyStateTriggerTransition(
                stateMachine,
                pickUpState,
                CCS_CharacterControllerConstants.AnimatorPickUpRightHandTriggerParameter,
                InteractionTransitionDuration);
            changed |= EnsureAnyStateTriggerTransition(
                stateMachine,
                walkThroughDoorState,
                CCS_CharacterControllerConstants.AnimatorWalkThroughDoorRightHandTriggerParameter,
                InteractionTransitionDuration);
            changed |= EnsureExitTimeTransition(
                pickUpState,
                defaultState,
                InteractionTransitionDuration,
                InteractionReturnExitTime);
            changed |= EnsureExitTimeTransition(
                walkThroughDoorState,
                defaultState,
                InteractionTransitionDuration,
                InteractionReturnExitTime);

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

        private static bool RemoveInteractionTriggerTransitionsFromStateMachine(AnimatorStateMachine stateMachine)
        {
            bool changed = false;
            changed |= RemoveAnyStateTransitionsUsingParameter(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorPickUpRightHandTriggerParameter);
            changed |= RemoveAnyStateTransitionsUsingParameter(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorWalkThroughDoorRightHandTriggerParameter);
            return changed;
        }

        private static bool RemoveAnyStateTransitionsUsingParameter(
            AnimatorStateMachine stateMachine,
            string parameterName)
        {
            bool changed = false;
            AnimatorStateTransition[] transitions = stateMachine.anyStateTransitions;
            for (int i = transitions.Length - 1; i >= 0; i--)
            {
                AnimatorStateTransition transition = transitions[i];
                if (transition != null && TransitionUsesParameter(transition, parameterName))
                {
                    stateMachine.RemoveAnyStateTransition(transition);
                    changed = true;
                }
            }

            return changed;
        }

        private static bool EnsureAimLocomotionParameters(AnimatorController controller)
        {
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

        private static bool EnsureAnimatorTriggerParameter(AnimatorController controller, string parameterName)
        {
            for (int i = 0; i < controller.parameters.Length; i++)
            {
                if (controller.parameters[i].name != parameterName)
                {
                    continue;
                }

                return controller.parameters[i].type == AnimatorControllerParameterType.Trigger;
            }

            controller.AddParameter(parameterName, AnimatorControllerParameterType.Trigger);
            EditorUtility.SetDirty(controller);
            return true;
        }

        private static AnimatorState EnsureInteractionAnimationState(
            AnimatorStateMachine stateMachine,
            string stateName,
            AnimationClip clip,
            Vector3 position,
            CCS_InteractionAnimationKey animationKey,
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

            if (state.writeDefaultValues)
            {
                state.writeDefaultValues = false;
                changed = true;
            }

            changed |= EnsureInteractionExitBehaviour(state, animationKey);
            return state;
        }

        private static bool EnsureInteractionExitBehaviour(
            AnimatorState state,
            CCS_InteractionAnimationKey animationKey)
        {
            StateMachineBehaviour[] behaviours = state.behaviours;
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is CCS_InteractionAnimationStateExitBehaviour existing)
                {
                    SerializedObject serializedBehaviour = new SerializedObject(existing);
                    SerializedProperty animationKeyProperty = serializedBehaviour.FindProperty("animationKey");
                    if (animationKeyProperty != null
                        && animationKeyProperty.enumValueIndex != (int)animationKey)
                    {
                        animationKeyProperty.enumValueIndex = (int)animationKey;
                        serializedBehaviour.ApplyModifiedPropertiesWithoutUndo();
                        return true;
                    }

                    return false;
                }
            }

            CCS_InteractionAnimationStateExitBehaviour behaviour =
                state.AddStateMachineBehaviour<CCS_InteractionAnimationStateExitBehaviour>();
            SerializedObject serializedNewBehaviour = new SerializedObject(behaviour);
            SerializedProperty newAnimationKeyProperty = serializedNewBehaviour.FindProperty("animationKey");
            if (newAnimationKeyProperty != null)
            {
                newAnimationKeyProperty.enumValueIndex = (int)animationKey;
                serializedNewBehaviour.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorUtility.SetDirty(state);
            return true;
        }

        private static bool EnsureAnyStateBoolTransition(
            AnimatorStateMachine stateMachine,
            AnimatorState destinationState,
            string parameterName,
            bool expectedTrue,
            float duration)
        {
            if (stateMachine == null || destinationState == null)
            {
                return false;
            }

            AnimatorStateTransition[] transitions = stateMachine.anyStateTransitions;
            for (int i = 0; i < transitions.Length; i++)
            {
                AnimatorStateTransition existing = transitions[i];
                if (existing.destinationState != destinationState)
                {
                    continue;
                }

                if (HasSingleBoolCondition(existing, parameterName, expectedTrue)
                    && !existing.hasExitTime
                    && Mathf.Approximately(existing.duration, duration))
                {
                    return false;
                }
            }

            AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(destinationState);
            transition.hasExitTime = false;
            transition.duration = duration;
            transition.canTransitionToSelf = false;
            transition.AddCondition(
                expectedTrue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
                0f,
                parameterName);
            EditorUtility.SetDirty(transition);
            EditorUtility.SetDirty(stateMachine);
            return true;
        }

        private static bool EnsureAnyStateTriggerTransition(
            AnimatorStateMachine stateMachine,
            AnimatorState destinationState,
            string triggerName,
            float duration)
        {
            if (stateMachine == null || destinationState == null)
            {
                return false;
            }

            AnimatorStateTransition[] transitions = stateMachine.anyStateTransitions;
            for (int i = 0; i < transitions.Length; i++)
            {
                AnimatorStateTransition existing = transitions[i];
                if (existing.destinationState != destinationState)
                {
                    continue;
                }

                if (HasSingleTriggerCondition(existing, triggerName)
                    && !existing.hasExitTime
                    && Mathf.Approximately(existing.duration, duration))
                {
                    return false;
                }
            }

            AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(destinationState);
            transition.hasExitTime = false;
            transition.duration = duration;
            transition.canTransitionToSelf = false;
            transition.AddCondition(AnimatorConditionMode.If, 0f, triggerName);
            EditorUtility.SetDirty(transition);
            EditorUtility.SetDirty(stateMachine);
            return true;
        }

        private static bool RemoveTriggerAnyStateTransitionExcept(
            AnimatorStateMachine stateMachine,
            AnimatorState destinationState,
            string triggerName)
        {
            bool changed = false;
            AnimatorStateTransition[] transitions = stateMachine.anyStateTransitions;
            for (int i = transitions.Length - 1; i >= 0; i--)
            {
                AnimatorStateTransition transition = transitions[i];
                if (transition == null || !TransitionUsesParameter(transition, triggerName))
                {
                    continue;
                }

                if (transition.destinationState == destinationState)
                {
                    continue;
                }

                stateMachine.RemoveAnyStateTransition(transition);
                changed = true;
            }

            return changed;
        }

        private static bool HasSingleTriggerCondition(AnimatorStateTransition transition, string triggerName)
        {
            AnimatorCondition[] conditions = transition.conditions;
            if (conditions.Length != 1)
            {
                return false;
            }

            AnimatorCondition condition = conditions[0];
            return condition.parameter == triggerName && condition.mode == AnimatorConditionMode.If;
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
