using System.Collections.Generic;
using CCS.Modules.CharacterController.Editor.EquipmentFitStudio;
using CCS.Modules.Weapons;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;

// =============================================================================
// SCRIPT: CCS_EquipmentSocketPlayerBuilder
// CATEGORY: Modules / CharacterController / Editor / Equipment
// PURPOSE: Places bone-parented equipment sockets and zero-weight weapon IK rig.
// PLACEMENT: Editor utility invoked from player prefab and master test setup.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.6 foundation only. No weapon visuals attached to sockets.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_EquipmentSocketPlayerBuilder
    {
        #region Public Methods

        public static bool EnsurePlayerEquipmentSocketFoundation(GameObject prefabRoot)
        {
            if (prefabRoot == null)
            {
                return false;
            }

            bool changed = false;
            changed |= CCS_EquipmentFitStudioCleanupUtility.CleanupPreviewObjectsOnInstance(prefabRoot);

            CCS_EquipmentSocketProfileBuilder.EnsureDefaultEquipmentSocketProfile();
            CCS_EquipmentSocketProfile profile = AssetDatabase.LoadAssetAtPath<CCS_EquipmentSocketProfile>(
                CCS_EquipmentConstants.DefaultEquipmentSocketProfilePath);
            if (profile == null)
            {
                Debug.LogError(
                    "[Equipment Socket Player Builder] Missing profile: "
                    + CCS_EquipmentConstants.DefaultEquipmentSocketProfilePath);
                return false;
            }

            Transform visualRoot = FindDeepChild(prefabRoot.transform, CCS_EquipmentConstants.VisualRootObjectName);
            if (visualRoot == null)
            {
                Debug.LogError("[Equipment Socket Player Builder] Missing VisualRoot on player prefab.");
                return false;
            }

            Animator animator = visualRoot.GetComponentInChildren<Animator>(true);
            bool hasHumanoidRig = HasUsableHumanoidRig(animator);

            changed |= RemoveLegacySocketContainers(prefabRoot.transform);
            changed |= EnsureEquipmentSockets(prefabRoot, visualRoot, animator, hasHumanoidRig, profile);
            changed |= EnsureSocketRegistry(prefabRoot, profile);
            changed |= EnsureWeaponIkTargets(visualRoot);
            changed |= EnsureWeaponIkRig(visualRoot, animator, hasHumanoidRig);
            changed |= CCS_RevolverArmReticleIKBuilder.EnsureRevolverArmReticleIk(prefabRoot, visualRoot, animator);

            if (hasHumanoidRig)
            {
                changed |= RemoveTestFallbackAnchors(visualRoot);
            }

            return changed;
        }

        #endregion

        #region Private Methods

        private static bool HasUsableHumanoidRig(Animator animator)
        {
            if (animator == null || !animator.isHuman || animator.avatar == null || !animator.avatar.isValid)
            {
                return false;
            }

            for (int i = 0; i < RequiredHumanoidBones.Length; i++)
            {
                if (animator.GetBoneTransform(RequiredHumanoidBones[i]) == null)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool EnsureEquipmentSockets(
            GameObject prefabRoot,
            Transform visualRoot,
            Animator animator,
            bool hasHumanoidRig,
            CCS_EquipmentSocketProfile profile)
        {
            bool changed = false;
            IReadOnlyList<CCS_EquipmentSocketDefinition> definitions = profile.SocketDefinitions;
            Transform fallbackRoot = EnsureTestFallbackAnchors(visualRoot, hasHumanoidRig, ref changed);

            for (int i = 0; i < definitions.Count; i++)
            {
                CCS_EquipmentSocketDefinition definition = definitions[i];
                if (definition == null || string.IsNullOrEmpty(definition.SocketId))
                {
                    continue;
                }

                if (!TryResolveSocketParent(
                        animator,
                        hasHumanoidRig,
                        fallbackRoot,
                        definition,
                        out Transform parentTransform,
                        out CCS_EquipmentSocketParentMode parentMode,
                        out bool isFallbackSocket))
                {
                    Debug.LogWarning(
                        "[Equipment Socket Player Builder] Could not resolve parent for "
                        + definition.SocketId);
                    continue;
                }

                Transform socketTransform = FindDeepChild(prefabRoot.transform, definition.SocketId);
                if (socketTransform == null)
                {
                    GameObject socketObject = new GameObject(definition.SocketId);
                    socketTransform = socketObject.transform;
                    changed = true;
                }

                if (socketTransform.parent != parentTransform)
                {
                    socketTransform.SetParent(parentTransform, false);
                    changed = true;
                }

                if (socketTransform.name != definition.SocketId)
                {
                    socketTransform.name = definition.SocketId;
                    changed = true;
                }

                if (ApplySocketLocalTransform(socketTransform, definition))
                {
                    changed = true;
                }

                CCS_EquipmentSocketAnchor anchor = socketTransform.GetComponent<CCS_EquipmentSocketAnchor>();
                if (anchor == null)
                {
                    anchor = socketTransform.gameObject.AddComponent<CCS_EquipmentSocketAnchor>();
                    changed = true;
                }

                anchor.Configure(definition, parentMode, isFallbackSocket);
                EditorUtility.SetDirty(anchor);

                for (int childIndex = socketTransform.childCount - 1; childIndex >= 0; childIndex--)
                {
                    Object.DestroyImmediate(socketTransform.GetChild(childIndex).gameObject, true);
                    changed = true;
                }
            }

            return changed;
        }

        private static bool EnsureSocketRegistry(GameObject prefabRoot, CCS_EquipmentSocketProfile profile)
        {
            bool changed = false;
            CCS_EquipmentSocketRegistry registry = prefabRoot.GetComponent<CCS_EquipmentSocketRegistry>();
            if (registry == null)
            {
                registry = prefabRoot.AddComponent<CCS_EquipmentSocketRegistry>();
                changed = true;
            }

            if (registry.EquipmentSocketProfile != profile)
            {
                registry.SetEquipmentSocketProfile(profile);
                changed = true;
            }

            EditorUtility.SetDirty(registry);
            return changed;
        }

        private static bool EnsureWeaponIkTargets(Transform visualRoot)
        {
            bool changed = false;
            Transform ikTargetsRoot = visualRoot.Find(CCS_EquipmentConstants.WeaponIkTargetsObjectName);
            if (ikTargetsRoot == null)
            {
                GameObject ikTargetsObject = new GameObject(CCS_EquipmentConstants.WeaponIkTargetsObjectName);
                ikTargetsRoot = ikTargetsObject.transform;
                ikTargetsRoot.SetParent(visualRoot, false);
                changed = true;
            }

            changed |= EnsureChildTransform(
                ikTargetsRoot,
                CCS_EquipmentConstants.RightHandIkTargetObjectName,
                new Vector3(0.35f, 1.35f, 0.45f));
            changed |= EnsureChildTransform(
                ikTargetsRoot,
                CCS_EquipmentConstants.RightElbowHintObjectName,
                new Vector3(0.45f, 1.25f, 0.15f));
            changed |= EnsureChildTransform(
                ikTargetsRoot,
                CCS_EquipmentConstants.LeftHandIkTargetObjectName,
                new Vector3(-0.35f, 1.35f, 0.45f));
            changed |= EnsureChildTransform(
                ikTargetsRoot,
                CCS_EquipmentConstants.LeftElbowHintObjectName,
                new Vector3(-0.45f, 1.25f, 0.15f));
            changed |= EnsureChildTransform(
                ikTargetsRoot,
                CCS_EquipmentConstants.WeaponAimTargetObjectName,
                new Vector3(0f, 1.45f, 1.0f));

            return changed;
        }

        private static bool EnsureWeaponIkRig(Transform visualRoot, Animator animator, bool hasHumanoidRig)
        {
            if (animator == null)
            {
                return false;
            }

            Transform ikTargetsRoot = visualRoot.Find(CCS_EquipmentConstants.WeaponIkTargetsObjectName);
            if (ikTargetsRoot == null)
            {
                return false;
            }

            bool changed = false;
            GameObject animatorObject = animator.gameObject;
            RigBuilder rigBuilder = animatorObject.GetComponent<RigBuilder>();
            if (rigBuilder == null)
            {
                rigBuilder = animatorObject.AddComponent<RigBuilder>();
                changed = true;
            }

            Transform rigTransform = FindDeepChild(animatorObject.transform, CCS_EquipmentConstants.WeaponIkRigObjectName);
            Rig weaponIkRig = null;
            if (rigTransform == null)
            {
                GameObject rigObject = new GameObject(CCS_EquipmentConstants.WeaponIkRigObjectName);
                rigTransform = rigObject.transform;
                rigTransform.SetParent(animatorObject.transform, false);
                weaponIkRig = rigObject.AddComponent<Rig>();
                changed = true;
            }
            else
            {
                weaponIkRig = rigTransform.GetComponent<Rig>();
                if (weaponIkRig == null)
                {
                    weaponIkRig = rigTransform.gameObject.AddComponent<Rig>();
                    changed = true;
                }
            }

            if (weaponIkRig.weight != 0f)
            {
                weaponIkRig.weight = 0f;
                changed = true;
            }

            changed |= EnsureRigBuilderLayer(rigBuilder, weaponIkRig);

            if (hasHumanoidRig)
            {
                changed |= EnsureTwoBoneIkConstraint(
                    rigTransform,
                    "RightHandTwoBoneIK",
                    animator.GetBoneTransform(HumanBodyBones.RightUpperArm),
                    animator.GetBoneTransform(HumanBodyBones.RightLowerArm),
                    animator.GetBoneTransform(HumanBodyBones.RightHand),
                    ikTargetsRoot.Find(CCS_EquipmentConstants.RightHandIkTargetObjectName),
                    ikTargetsRoot.Find(CCS_EquipmentConstants.RightElbowHintObjectName));
                changed |= EnsureTwoBoneIkConstraint(
                    rigTransform,
                    "LeftHandTwoBoneIK",
                    animator.GetBoneTransform(HumanBodyBones.LeftUpperArm),
                    animator.GetBoneTransform(HumanBodyBones.LeftLowerArm),
                    animator.GetBoneTransform(HumanBodyBones.LeftHand),
                    ikTargetsRoot.Find(CCS_EquipmentConstants.LeftHandIkTargetObjectName),
                    ikTargetsRoot.Find(CCS_EquipmentConstants.LeftElbowHintObjectName));
                changed |= EnsureWeaponAimConstraint(
                    rigTransform,
                    animator,
                    ikTargetsRoot.Find(CCS_EquipmentConstants.WeaponAimTargetObjectName));
            }
            else
            {
                changed |= RemoveConstraintIfExists<TwoBoneIKConstraint>(rigTransform, "RightHandTwoBoneIK");
                changed |= RemoveConstraintIfExists<TwoBoneIKConstraint>(rigTransform, "LeftHandTwoBoneIK");
                changed |= RemoveConstraintIfExists<MultiAimConstraint>(rigTransform, "WeaponAimConstraint");
            }

            if (changed)
            {
                rigBuilder.Build();
            }

            return changed;
        }

        private static bool EnsureTwoBoneIkConstraint(
            Transform rigTransform,
            string constraintObjectName,
            Transform rootBone,
            Transform midBone,
            Transform tipBone,
            Transform target,
            Transform hint)
        {
            if (rootBone == null || midBone == null || tipBone == null || target == null || hint == null)
            {
                return RemoveConstraintIfExists<TwoBoneIKConstraint>(rigTransform, constraintObjectName);
            }

            bool changed = false;
            Transform constraintTransform = rigTransform.Find(constraintObjectName);
            if (constraintTransform == null)
            {
                GameObject constraintObject = new GameObject(constraintObjectName);
                constraintTransform = constraintObject.transform;
                constraintTransform.SetParent(rigTransform, false);
                constraintTransform.gameObject.AddComponent<TwoBoneIKConstraint>();
                changed = true;
            }

            TwoBoneIKConstraint constraint = constraintTransform.GetComponent<TwoBoneIKConstraint>();
            if (constraint == null)
            {
                constraint = constraintTransform.gameObject.AddComponent<TwoBoneIKConstraint>();
                changed = true;
            }

            constraint.Reset();
            TwoBoneIKConstraintData data = constraint.data;
            if (data.root != rootBone)
            {
                data.root = rootBone;
                changed = true;
            }

            if (data.mid != midBone)
            {
                data.mid = midBone;
                changed = true;
            }

            if (data.tip != tipBone)
            {
                data.tip = tipBone;
                changed = true;
            }

            if (data.target != target)
            {
                data.target = target;
                changed = true;
            }

            if (data.hint != hint)
            {
                data.hint = hint;
                changed = true;
            }

            if (data.targetPositionWeight != 0f)
            {
                data.targetPositionWeight = 0f;
                changed = true;
            }

            if (data.targetRotationWeight != 0f)
            {
                data.targetRotationWeight = 0f;
                changed = true;
            }

            if (data.hintWeight != 0f)
            {
                data.hintWeight = 0f;
                changed = true;
            }

            constraint.data = data;
            if (constraint.weight != 0f)
            {
                constraint.weight = 0f;
                changed = true;
            }

            return changed;
        }

        private static bool EnsureWeaponAimConstraint(
            Transform rigTransform,
            Animator animator,
            Transform aimTarget)
        {
            Transform constrainedBone = animator.GetBoneTransform(HumanBodyBones.Chest);
            if (constrainedBone == null)
            {
                constrainedBone = animator.GetBoneTransform(HumanBodyBones.Spine);
            }

            if (constrainedBone == null || aimTarget == null)
            {
                return RemoveConstraintIfExists<MultiAimConstraint>(rigTransform, "WeaponAimConstraint");
            }

            bool changed = false;
            Transform constraintTransform = rigTransform.Find("WeaponAimConstraint");
            if (constraintTransform == null)
            {
                GameObject constraintObject = new GameObject("WeaponAimConstraint");
                constraintTransform = constraintObject.transform;
                constraintTransform.SetParent(rigTransform, false);
                constraintTransform.gameObject.AddComponent<MultiAimConstraint>();
                changed = true;
            }

            MultiAimConstraint constraint = constraintTransform.GetComponent<MultiAimConstraint>();
            if (constraint == null)
            {
                constraint = constraintTransform.gameObject.AddComponent<MultiAimConstraint>();
                changed = true;
            }

            constraint.Reset();
            MultiAimConstraintData data = constraint.data;
            if (data.constrainedObject != constrainedBone)
            {
                data.constrainedObject = constrainedBone;
                changed = true;
            }

            WeightedTransformArray sourceObjects = data.sourceObjects;
            if (sourceObjects.Count != 1 || sourceObjects[0].transform != aimTarget)
            {
                sourceObjects.Clear();
                sourceObjects.Add(new WeightedTransform(aimTarget, 1f));
                data.sourceObjects = sourceObjects;
                changed = true;
            }

            constraint.data = data;
            if (constraint.weight != 0f)
            {
                constraint.weight = 0f;
                changed = true;
            }

            return changed;
        }

        private static bool EnsureRigBuilderLayer(RigBuilder rigBuilder, Rig weaponIkRig)
        {
            SerializedObject serializedRigBuilder = new SerializedObject(rigBuilder);
            SerializedProperty layersProperty = serializedRigBuilder.FindProperty("m_RigLayers");
            if (layersProperty == null)
            {
                return false;
            }

            bool changed = false;
            bool hasWeaponLayer = false;
            for (int i = 0; i < layersProperty.arraySize; i++)
            {
                SerializedProperty layerProperty = layersProperty.GetArrayElementAtIndex(i);
                SerializedProperty rigProperty = layerProperty.FindPropertyRelative("m_Rig");
                if (rigProperty != null && rigProperty.objectReferenceValue == weaponIkRig)
                {
                    hasWeaponLayer = true;
                    break;
                }
            }

            if (!hasWeaponLayer)
            {
                int insertIndex = layersProperty.arraySize;
                layersProperty.InsertArrayElementAtIndex(insertIndex);
                SerializedProperty newLayer = layersProperty.GetArrayElementAtIndex(insertIndex);
                SerializedProperty rigProperty = newLayer.FindPropertyRelative("m_Rig");
                SerializedProperty activeProperty = newLayer.FindPropertyRelative("m_Active");
                if (rigProperty != null)
                {
                    rigProperty.objectReferenceValue = weaponIkRig;
                }

                if (activeProperty != null)
                {
                    activeProperty.boolValue = true;
                }

                changed = true;
            }

            if (changed)
            {
                serializedRigBuilder.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool TryResolveSocketParent(
            Animator animator,
            bool hasHumanoidRig,
            Transform fallbackRoot,
            CCS_EquipmentSocketDefinition definition,
            out Transform parentTransform,
            out CCS_EquipmentSocketParentMode parentMode,
            out bool isFallbackSocket)
        {
            parentTransform = null;
            parentMode = CCS_EquipmentSocketParentMode.RealHumanoidBone;
            isFallbackSocket = false;

            if (hasHumanoidRig && animator != null)
            {
                Transform boneTransform = animator.GetBoneTransform(definition.ParentBone);
                if (boneTransform == null && definition.HasFallbackParentBone)
                {
                    boneTransform = animator.GetBoneTransform(definition.FallbackParentBone);
                }

                if (boneTransform != null)
                {
                    parentTransform = boneTransform;
                    return true;
                }
            }

            if (fallbackRoot == null)
            {
                return false;
            }

            string fallbackAnchorName = GetFallbackAnchorName(definition);
            Transform fallbackAnchor = fallbackRoot.Find(fallbackAnchorName);
            if (fallbackAnchor == null)
            {
                return false;
            }

            parentTransform = fallbackAnchor;
            parentMode = CCS_EquipmentSocketParentMode.TestFallbackAnchor;
            isFallbackSocket = true;
            return true;
        }

        private static Transform EnsureTestFallbackAnchors(
            Transform visualRoot,
            bool hasHumanoidRig,
            ref bool changed)
        {
            if (hasHumanoidRig)
            {
                return null;
            }

            Transform fallbackRoot = visualRoot.Find(CCS_EquipmentConstants.TestBoneSocketFallbacksObjectName);
            if (fallbackRoot == null)
            {
                GameObject fallbackRootObject = new GameObject(CCS_EquipmentConstants.TestBoneSocketFallbacksObjectName);
                fallbackRoot = fallbackRootObject.transform;
                fallbackRoot.SetParent(visualRoot, false);
                changed = true;
            }

            changed |= EnsureChildTransform(
                fallbackRoot,
                CCS_EquipmentConstants.FallbackHipsAnchorName,
                CCS_EquipmentConstants.FallbackHipsLocalPosition);
            changed |= EnsureChildTransform(
                fallbackRoot,
                CCS_EquipmentConstants.FallbackRightHandAnchorName,
                CCS_EquipmentConstants.FallbackRightHandLocalPosition);
            changed |= EnsureChildTransform(
                fallbackRoot,
                CCS_EquipmentConstants.FallbackLeftHandAnchorName,
                CCS_EquipmentConstants.FallbackLeftHandLocalPosition);
            changed |= EnsureChildTransform(
                fallbackRoot,
                CCS_EquipmentConstants.FallbackChestAnchorName,
                CCS_EquipmentConstants.FallbackChestLocalPosition);
            changed |= EnsureChildTransform(
                fallbackRoot,
                CCS_EquipmentConstants.FallbackSpineAnchorName,
                CCS_EquipmentConstants.FallbackSpineLocalPosition);

            return fallbackRoot;
        }

        private static bool RemoveTestFallbackAnchors(Transform visualRoot)
        {
            Transform fallbackRoot = visualRoot.Find(CCS_EquipmentConstants.TestBoneSocketFallbacksObjectName);
            if (fallbackRoot == null)
            {
                return false;
            }

            Object.DestroyImmediate(fallbackRoot.gameObject, true);
            return true;
        }

        private static bool RemoveLegacySocketContainers(Transform prefabRoot)
        {
            bool changed = false;
            Transform staticSocketsRoot = FindDeepChild(prefabRoot, "Sockets");
            if (staticSocketsRoot != null && staticSocketsRoot.GetComponent<CCS_EquipmentSocketAnchor>() == null)
            {
                Object.DestroyImmediate(staticSocketsRoot.gameObject, true);
                changed = true;
            }

            return changed;
        }

        private static bool ApplySocketLocalTransform(Transform socketTransform, CCS_EquipmentSocketDefinition definition)
        {
            bool changed = false;
            if (socketTransform.localPosition != definition.LocalPosition)
            {
                socketTransform.localPosition = definition.LocalPosition;
                changed = true;
            }

            Quaternion targetRotation = Quaternion.Euler(definition.LocalEulerAngles);
            if (socketTransform.localRotation != targetRotation)
            {
                socketTransform.localRotation = targetRotation;
                changed = true;
            }

            if (socketTransform.localScale != definition.LocalScale)
            {
                socketTransform.localScale = definition.LocalScale;
                changed = true;
            }

            return changed;
        }

        private static bool EnsureChildTransform(Transform parent, string childName, Vector3 localPosition)
        {
            Transform child = parent.Find(childName);
            if (child == null)
            {
                GameObject childObject = new GameObject(childName);
                child = childObject.transform;
                child.SetParent(parent, false);
                child.localPosition = localPosition;
                child.localRotation = Quaternion.identity;
                child.localScale = Vector3.one;
                return true;
            }

            bool changed = false;
            if (child.localPosition != localPosition)
            {
                child.localPosition = localPosition;
                changed = true;
            }

            if (child.localRotation != Quaternion.identity)
            {
                child.localRotation = Quaternion.identity;
                changed = true;
            }

            if (child.localScale != Vector3.one)
            {
                child.localScale = Vector3.one;
                changed = true;
            }

            return changed;
        }

        private static bool RemoveConstraintIfExists<T>(Transform rigTransform, string constraintObjectName)
            where T : Component
        {
            Transform constraintTransform = rigTransform.Find(constraintObjectName);
            if (constraintTransform == null)
            {
                return false;
            }

            Object.DestroyImmediate(constraintTransform.gameObject, true);
            return true;
        }

        private static string GetFallbackAnchorName(CCS_EquipmentSocketDefinition definition)
        {
            HumanBodyBones bone = definition.ParentBone;
            if (bone == HumanBodyBones.LastBone && definition.HasFallbackParentBone)
            {
                bone = definition.FallbackParentBone;
            }

            switch (bone)
            {
                case HumanBodyBones.Hips:
                    return CCS_EquipmentConstants.FallbackHipsAnchorName;
                case HumanBodyBones.RightHand:
                    return CCS_EquipmentConstants.FallbackRightHandAnchorName;
                case HumanBodyBones.LeftHand:
                    return CCS_EquipmentConstants.FallbackLeftHandAnchorName;
                case HumanBodyBones.Chest:
                    return CCS_EquipmentConstants.FallbackChestAnchorName;
                case HumanBodyBones.Spine:
                    return CCS_EquipmentConstants.FallbackSpineAnchorName;
                default:
                    if (definition.HasFallbackParentBone)
                    {
                        return GetFallbackAnchorNameForBone(definition.FallbackParentBone);
                    }

                    return CCS_EquipmentConstants.FallbackSpineAnchorName;
            }
        }

        private static string GetFallbackAnchorNameForBone(HumanBodyBones bone)
        {
            switch (bone)
            {
                case HumanBodyBones.Hips:
                    return CCS_EquipmentConstants.FallbackHipsAnchorName;
                case HumanBodyBones.RightHand:
                    return CCS_EquipmentConstants.FallbackRightHandAnchorName;
                case HumanBodyBones.LeftHand:
                    return CCS_EquipmentConstants.FallbackLeftHandAnchorName;
                case HumanBodyBones.Chest:
                    return CCS_EquipmentConstants.FallbackChestAnchorName;
                case HumanBodyBones.Spine:
                    return CCS_EquipmentConstants.FallbackSpineAnchorName;
                default:
                    return CCS_EquipmentConstants.FallbackSpineAnchorName;
            }
        }

        private static Transform FindDeepChild(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform match = FindDeepChild(root.GetChild(i), childName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static readonly HumanBodyBones[] RequiredHumanoidBones =
        {
            HumanBodyBones.Hips,
            HumanBodyBones.RightHand,
            HumanBodyBones.LeftHand,
            HumanBodyBones.Chest,
            HumanBodyBones.Spine,
            HumanBodyBones.RightUpperArm,
            HumanBodyBones.RightLowerArm,
            HumanBodyBones.LeftUpperArm,
            HumanBodyBones.LeftLowerArm,
        };

        #endregion
    }
}
