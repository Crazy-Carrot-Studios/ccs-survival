using CCS.Modules.Weapons;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;

// =============================================================================
// SCRIPT: CCS_RevolverArmReticleIKBuilder
// CATEGORY: Modules / CharacterController / Editor / Equipment
// PURPOSE: Builds arm-only Animation Rigging reticle IK and removes legacy direct weapon aim rigs.
// PLACEMENT: Editor utility invoked from equipment socket player builder.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.14 — Rig_RevolverArmReticleIK with TwoBoneIK + chest/shoulder MultiAim only.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_RevolverArmReticleIKBuilder
    {
        #region Public Methods

        public static bool EnsureRevolverArmReticleIk(GameObject prefabRoot, Transform visualRoot, Animator animator)
        {
            if (prefabRoot == null || visualRoot == null)
            {
                return false;
            }

            bool changed = false;
            changed |= RemoveLegacyDirectWeaponAimArtifacts(visualRoot, animator);
            changed |= EnsureArmReticleIkTargets(visualRoot);

            if (animator == null || !HasUsableHumanoidRig(animator))
            {
                return changed;
            }

            changed |= EnsureArmReticleIkRig(visualRoot, animator);
            changed |= EnsureArmReticleIkComponent(prefabRoot, visualRoot, animator);
            return changed;
        }

        #endregion

        #region Private Methods

        private static bool HasUsableHumanoidRig(Animator animator)
        {
            return animator != null
                && animator.isHuman
                && animator.avatar != null
                && animator.avatar.isValid
                && animator.GetBoneTransform(HumanBodyBones.RightHand) != null
                && animator.GetBoneTransform(HumanBodyBones.RightUpperArm) != null
                && animator.GetBoneTransform(HumanBodyBones.RightLowerArm) != null;
        }

        private static bool RemoveLegacyDirectWeaponAimArtifacts(Transform visualRoot, Animator animator)
        {
            bool changed = false;
            Component[] visualComponents = visualRoot.GetComponents<Component>();
            for (int i = 0; i < visualComponents.Length; i++)
            {
                Component component = visualComponents[i];
                if (component == null)
                {
                    continue;
                }

                string typeName = component.GetType().Name;
                if (typeName == "CCS_RevolverReticleAimRig" || typeName == "CCS_RevolverArmAimBias")
                {
                    Object.DestroyImmediate(component, true);
                    changed = true;
                }
            }

            Transform legacyRigRoot = visualRoot.Find(CCS_WeaponsConstants.RevolverAimRigRootObjectName);
            if (legacyRigRoot != null)
            {
                Object.DestroyImmediate(legacyRigRoot.gameObject, true);
                changed = true;
            }

            if (animator != null)
            {
                Transform legacyWeaponRig = animator.transform.Find(CCS_WeaponsConstants.RevolverAimRigObjectName);
                if (legacyWeaponRig != null)
                {
                    changed |= RemoveRigBuilderLayerReference(animator.gameObject, legacyWeaponRig.GetComponent<Rig>());
                    Object.DestroyImmediate(legacyWeaponRig.gameObject, true);
                    changed = true;
                }
            }

            return changed;
        }

        private static bool EnsureArmReticleIkTargets(Transform visualRoot)
        {
            bool changed = false;
            Transform ikRoot = visualRoot.Find(CCS_WeaponsConstants.RevolverArmReticleIkRootObjectName);
            if (ikRoot == null)
            {
                GameObject ikRootObject = new GameObject(CCS_WeaponsConstants.RevolverArmReticleIkRootObjectName);
                ikRoot = ikRootObject.transform;
                ikRoot.SetParent(visualRoot, false);
                changed = true;
            }

            changed |= EnsureChildTransform(
                ikRoot,
                CCS_WeaponsConstants.ReticleAimWorldTargetObjectName,
                new Vector3(0f, 1.45f, 12f));
            changed |= EnsureChildTransform(
                ikRoot,
                CCS_WeaponsConstants.RightHandReticleIkTargetObjectName,
                new Vector3(0.35f, 1.35f, 0.45f));
            changed |= EnsureChildTransform(
                ikRoot,
                CCS_WeaponsConstants.RightElbowHintObjectName,
                new Vector3(0.42f, 1.28f, 0.12f));
            changed |= EnsureChildTransform(
                ikRoot,
                CCS_WeaponsConstants.ChestAimTargetObjectName,
                new Vector3(0f, 1.45f, 12f));

            return changed;
        }

        private static bool EnsureArmReticleIkRig(Transform visualRoot, Animator animator)
        {
            Transform ikRoot = visualRoot.Find(CCS_WeaponsConstants.RevolverArmReticleIkRootObjectName);
            if (ikRoot == null)
            {
                return false;
            }

            Transform reticleTarget = ikRoot.Find(CCS_WeaponsConstants.ReticleAimWorldTargetObjectName);
            Transform handIkTarget = ikRoot.Find(CCS_WeaponsConstants.RightHandReticleIkTargetObjectName);
            Transform elbowHint = ikRoot.Find(CCS_WeaponsConstants.RightElbowHintObjectName);
            if (reticleTarget == null || handIkTarget == null || elbowHint == null)
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

            Transform rigTransform = animatorObject.transform.Find(CCS_WeaponsConstants.RevolverArmReticleIkRigObjectName);
            Rig armReticleIkRig = null;
            if (rigTransform == null)
            {
                GameObject rigObject = new GameObject(CCS_WeaponsConstants.RevolverArmReticleIkRigObjectName);
                rigTransform = rigObject.transform;
                rigTransform.SetParent(animatorObject.transform, false);
                armReticleIkRig = rigObject.AddComponent<Rig>();
                changed = true;
            }
            else
            {
                armReticleIkRig = rigTransform.GetComponent<Rig>();
                if (armReticleIkRig == null)
                {
                    armReticleIkRig = rigTransform.gameObject.AddComponent<Rig>();
                    changed = true;
                }
            }

            if (armReticleIkRig.weight != 0f)
            {
                armReticleIkRig.weight = 0f;
                changed = true;
            }

            changed |= EnsureRigBuilderLayer(rigBuilder, armReticleIkRig);

            Transform rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            Transform rightLowerArm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            Transform rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            Transform chest = animator.GetBoneTransform(HumanBodyBones.Chest);
            if (chest == null)
            {
                chest = animator.GetBoneTransform(HumanBodyBones.Spine);
            }

            Transform rightShoulder = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
            if (rightShoulder == null)
            {
                rightShoulder = rightUpperArm;
            }

            changed |= EnsureRightArmTwoBoneIk(
                rigTransform,
                rightUpperArm,
                rightLowerArm,
                rightHand,
                handIkTarget,
                elbowHint);
            changed |= EnsureChestAimBias(rigTransform, chest, reticleTarget);
            changed |= EnsureRightShoulderAimBias(rigTransform, rightShoulder, reticleTarget);
            changed |= RemoveConstraintIfExists<MultiAimConstraint>(
                rigTransform,
                CCS_WeaponsConstants.RevolverRightHandAimConstraintObjectName);

            if (changed)
            {
                rigBuilder.Build();
            }

            return changed;
        }

        private static bool EnsureArmReticleIkComponent(GameObject prefabRoot, Transform visualRoot, Animator animator)
        {
            bool changed = false;
            CCS_RevolverArmReticleIK armReticleIk = visualRoot.GetComponent<CCS_RevolverArmReticleIK>();
            if (armReticleIk == null)
            {
                armReticleIk = visualRoot.gameObject.AddComponent<CCS_RevolverArmReticleIK>();
                changed = true;
            }

            Transform ikRoot = visualRoot.Find(CCS_WeaponsConstants.RevolverArmReticleIkRootObjectName);
            Transform reticleTarget = ikRoot != null
                ? ikRoot.Find(CCS_WeaponsConstants.ReticleAimWorldTargetObjectName)
                : null;
            Transform handIkTarget = ikRoot != null
                ? ikRoot.Find(CCS_WeaponsConstants.RightHandReticleIkTargetObjectName)
                : null;
            Transform elbowHint = ikRoot != null
                ? ikRoot.Find(CCS_WeaponsConstants.RightElbowHintObjectName)
                : null;

            Rig armReticleIkRig = null;
            TwoBoneIKConstraint rightArmTwoBoneIk = null;
            MultiAimConstraint chestAimBias = null;
            MultiAimConstraint rightShoulderAimBias = null;
            if (animator != null)
            {
                Transform rigTransform = animator.transform.Find(CCS_WeaponsConstants.RevolverArmReticleIkRigObjectName);
                if (rigTransform != null)
                {
                    armReticleIkRig = rigTransform.GetComponent<Rig>();
                    rightArmTwoBoneIk = rigTransform
                        .Find(CCS_WeaponsConstants.RightArmTwoBoneIkConstraintObjectName)
                        ?.GetComponent<TwoBoneIKConstraint>();
                    chestAimBias = rigTransform
                        .Find(CCS_WeaponsConstants.ChestAimBiasConstraintObjectName)
                        ?.GetComponent<MultiAimConstraint>();
                    rightShoulderAimBias = rigTransform
                        .Find(CCS_WeaponsConstants.RightShoulderAimBiasConstraintObjectName)
                        ?.GetComponent<MultiAimConstraint>();
                }
            }

            CCS_RevolverController revolverController = prefabRoot.GetComponent<CCS_RevolverController>();
            CCS_PlayerEquipmentVisualController equipmentVisual = prefabRoot.GetComponent<CCS_PlayerEquipmentVisualController>();
            CCS_RevolverHudPresenter hudPresenter = prefabRoot.GetComponentInChildren<CCS_RevolverHudPresenter>(true);
            CCS_RevolverDefinition revolverDefinition = AssetDatabase.LoadAssetAtPath<CCS_RevolverDefinition>(
                CCS_WeaponsConstants.RevolverDefinitionProfilePath);

            SerializedObject serializedIk = new SerializedObject(armReticleIk);
            bool wiringChanged = false;
            wiringChanged |= SetObjectReference(serializedIk, "animator", animator);
            wiringChanged |= SetObjectReference(serializedIk, "revolverAnimationStateComponent", revolverController);
            wiringChanged |= SetObjectReference(serializedIk, "equipmentVisualController", equipmentVisual);
            wiringChanged |= SetObjectReference(serializedIk, "hudPresenter", hudPresenter);
            wiringChanged |= SetObjectReference(serializedIk, "revolverDefinition", revolverDefinition);
            wiringChanged |= SetObjectReference(serializedIk, "reticleAimWorldTarget", reticleTarget);
            wiringChanged |= SetObjectReference(serializedIk, "rightHandReticleIkTarget", handIkTarget);
            wiringChanged |= SetObjectReference(serializedIk, "rightElbowHint", elbowHint);
            wiringChanged |= SetObjectReference(serializedIk, "armReticleIkRig", armReticleIkRig);
            wiringChanged |= SetObjectReference(serializedIk, "rightArmTwoBoneIk", rightArmTwoBoneIk);
            wiringChanged |= SetObjectReference(serializedIk, "chestAimBias", chestAimBias);
            wiringChanged |= SetObjectReference(serializedIk, "rightShoulderAimBias", rightShoulderAimBias);

            if (wiringChanged)
            {
                serializedIk.ApplyModifiedPropertiesWithoutUndo();
                changed = true;
            }

            if (!armReticleIk.enabled)
            {
                armReticleIk.enabled = true;
                changed = true;
            }

            EditorUtility.SetDirty(armReticleIk);
            return changed;
        }

        private static bool EnsureRightArmTwoBoneIk(
            Transform rigTransform,
            Transform rootBone,
            Transform midBone,
            Transform tipBone,
            Transform target,
            Transform hint)
        {
            if (rootBone == null || midBone == null || tipBone == null || target == null || hint == null)
            {
                return RemoveConstraintIfExists<TwoBoneIKConstraint>(
                    rigTransform,
                    CCS_WeaponsConstants.RightArmTwoBoneIkConstraintObjectName);
            }

            bool changed = false;
            Transform constraintTransform = rigTransform.Find(
                CCS_WeaponsConstants.RightArmTwoBoneIkConstraintObjectName);
            if (constraintTransform == null)
            {
                GameObject constraintObject = new GameObject(
                    CCS_WeaponsConstants.RightArmTwoBoneIkConstraintObjectName);
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

            const float positionWeight = 0.28f;
            const float rotationWeight = 0f;
            const float hintWeight = 0.75f;
            if (!Mathf.Approximately(data.targetPositionWeight, positionWeight))
            {
                data.targetPositionWeight = positionWeight;
                changed = true;
            }

            if (!Mathf.Approximately(data.targetRotationWeight, rotationWeight))
            {
                data.targetRotationWeight = rotationWeight;
                changed = true;
            }

            if (!Mathf.Approximately(data.hintWeight, hintWeight))
            {
                data.hintWeight = hintWeight;
                changed = true;
            }

            constraint.data = data;
            changed |= EnsureTwoBoneIkMaintainTargetOffset(constraint, true, false);

            if (constraint.weight != 0f)
            {
                constraint.weight = 0f;
                changed = true;
            }

            return changed;
        }

        private static bool EnsureTwoBoneIkMaintainTargetOffset(
            TwoBoneIKConstraint constraint,
            bool maintainPositionOffset,
            bool maintainRotationOffset)
        {
            SerializedObject serializedConstraint = new SerializedObject(constraint);
            SerializedProperty dataProperty = serializedConstraint.FindProperty("m_Data");
            if (dataProperty == null)
            {
                return false;
            }

            SerializedProperty maintainPosition = dataProperty.FindPropertyRelative("m_MaintainTargetPositionOffset");
            SerializedProperty maintainRotation = dataProperty.FindPropertyRelative("m_MaintainTargetRotationOffset");
            bool changed = false;
            if (maintainPosition != null && maintainPosition.boolValue != maintainPositionOffset)
            {
                maintainPosition.boolValue = maintainPositionOffset;
                changed = true;
            }

            if (maintainRotation != null && maintainRotation.boolValue != maintainRotationOffset)
            {
                maintainRotation.boolValue = maintainRotationOffset;
                changed = true;
            }

            if (changed)
            {
                serializedConstraint.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool EnsureChestAimBias(Transform rigTransform, Transform chest, Transform aimTarget)
        {
            return EnsureMultiAimBias(
                rigTransform,
                CCS_WeaponsConstants.ChestAimBiasConstraintObjectName,
                chest,
                aimTarget);
        }

        private static bool EnsureRightShoulderAimBias(Transform rigTransform, Transform shoulder, Transform aimTarget)
        {
            return EnsureMultiAimBias(
                rigTransform,
                CCS_WeaponsConstants.RightShoulderAimBiasConstraintObjectName,
                shoulder,
                aimTarget);
        }

        private static bool EnsureMultiAimBias(
            Transform rigTransform,
            string constraintObjectName,
            Transform constrainedBone,
            Transform aimTarget)
        {
            if (constrainedBone == null || aimTarget == null)
            {
                return RemoveConstraintIfExists<MultiAimConstraint>(rigTransform, constraintObjectName);
            }

            bool changed = false;
            Transform constraintTransform = rigTransform.Find(constraintObjectName);
            if (constraintTransform == null)
            {
                GameObject constraintObject = new GameObject(constraintObjectName);
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
            changed |= EnsureMultiAimMaintainOffset(constraint, true);

            if (constraint.weight != 0f)
            {
                constraint.weight = 0f;
                changed = true;
            }

            return changed;
        }

        private static bool EnsureMultiAimMaintainOffset(MultiAimConstraint constraint, bool maintainOffset)
        {
            SerializedObject serializedConstraint = new SerializedObject(constraint);
            SerializedProperty dataProperty = serializedConstraint.FindProperty("m_Data");
            if (dataProperty == null)
            {
                return false;
            }

            SerializedProperty maintainOffsetProperty = dataProperty.FindPropertyRelative("m_MaintainOffset");
            if (maintainOffsetProperty == null)
            {
                return false;
            }

            if (maintainOffsetProperty.boolValue == maintainOffset)
            {
                return false;
            }

            maintainOffsetProperty.boolValue = maintainOffset;
            serializedConstraint.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        private static bool EnsureChildTransform(Transform parent, string childName, Vector3 localPosition)
        {
            bool changed = false;
            Transform child = parent.Find(childName);
            if (child == null)
            {
                GameObject childObject = new GameObject(childName);
                child = childObject.transform;
                child.SetParent(parent, false);
                changed = true;
            }

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

            return changed;
        }

        private static bool EnsureRigBuilderLayer(RigBuilder rigBuilder, Rig rig)
        {
            SerializedObject serializedRigBuilder = new SerializedObject(rigBuilder);
            SerializedProperty layersProperty = serializedRigBuilder.FindProperty("m_RigLayers");
            if (layersProperty == null)
            {
                return false;
            }

            bool changed = false;
            bool hasLayer = false;
            for (int i = 0; i < layersProperty.arraySize; i++)
            {
                SerializedProperty layerProperty = layersProperty.GetArrayElementAtIndex(i);
                SerializedProperty rigProperty = layerProperty.FindPropertyRelative("m_Rig");
                if (rigProperty != null && rigProperty.objectReferenceValue == rig)
                {
                    hasLayer = true;
                    break;
                }
            }

            if (!hasLayer)
            {
                int insertIndex = layersProperty.arraySize;
                layersProperty.InsertArrayElementAtIndex(insertIndex);
                SerializedProperty newLayer = layersProperty.GetArrayElementAtIndex(insertIndex);
                SerializedProperty rigProperty = newLayer.FindPropertyRelative("m_Rig");
                SerializedProperty activeProperty = newLayer.FindPropertyRelative("m_Active");
                if (rigProperty != null)
                {
                    rigProperty.objectReferenceValue = rig;
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

        private static bool RemoveRigBuilderLayerReference(GameObject animatorObject, Rig rig)
        {
            if (animatorObject == null || rig == null)
            {
                return false;
            }

            RigBuilder rigBuilder = animatorObject.GetComponent<RigBuilder>();
            if (rigBuilder == null)
            {
                return false;
            }

            SerializedObject serializedRigBuilder = new SerializedObject(rigBuilder);
            SerializedProperty layersProperty = serializedRigBuilder.FindProperty("m_RigLayers");
            if (layersProperty == null)
            {
                return false;
            }

            bool changed = false;
            for (int i = layersProperty.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty layerProperty = layersProperty.GetArrayElementAtIndex(i);
                SerializedProperty rigProperty = layerProperty.FindPropertyRelative("m_Rig");
                if (rigProperty != null && rigProperty.objectReferenceValue == rig)
                {
                    layersProperty.DeleteArrayElementAtIndex(i);
                    changed = true;
                }
            }

            if (changed)
            {
                serializedRigBuilder.ApplyModifiedPropertiesWithoutUndo();
                rigBuilder.Build();
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

        private static bool SetObjectReference(SerializedObject serializedObject, string propertyName, Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == value)
            {
                return false;
            }

            property.objectReferenceValue = value;
            return true;
        }

        #endregion
    }
}
