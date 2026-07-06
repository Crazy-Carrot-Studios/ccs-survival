using CCS.Modules.CharacterController;
using CCS.Modules.Weapons;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;

// =============================================================================
// SCRIPT: CCS_ProceduralArmConvergenceCleanupBuilder
// CATEGORY: Modules / CharacterController / Editor / StripBaseline
// PURPOSE: Removes failed procedural arm convergence from player prefab and keeps passive visual aim reference only.
// PLACEMENT: Editor builder invoked from stripped baseline strip pass.
// AUTHOR: James Schilz
// CREATED: 2026-07-03
// NOTES: v0.7.13 ships authored animation only; no runtime arm IK/convergence on production prefab.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_ProceduralArmConvergenceCleanupBuilder
    {
        private static readonly string[] ProceduralRigObjectNamesToRemove =
        {
            CCS_CharacterControllerConstants.ManualArmRotationBiasRigLayerObjectName,
            CCS_CharacterControllerConstants.ManualArmAimRigLayerObjectName,
            CCS_CharacterControllerConstants.DualRevolverAimRigLayerObjectName,
            "CCS_DualRevolverArmAimRigLayer",
            CCS_CharacterControllerConstants.DualRevolverAimRigRootObjectName,
            CCS_CharacterControllerConstants.DualRevolverManualAimTargetsRootObjectName,
            CCS_CharacterControllerConstants.RightUpperArmRotationBiasTestObjectName,
            CCS_CharacterControllerConstants.RightClavicleRotationBiasTestObjectName,
            CCS_CharacterControllerConstants.LeftUpperArmRotationBiasTestObjectName,
            CCS_CharacterControllerConstants.LeftClavicleRotationBiasTestObjectName,
            CCS_CharacterControllerConstants.RightUpperArmRotationBiasPoseObjectName,
            CCS_CharacterControllerConstants.RightClavicleRotationBiasPoseObjectName,
            CCS_CharacterControllerConstants.LeftUpperArmRotationBiasPoseObjectName,
            CCS_CharacterControllerConstants.LeftClavicleRotationBiasPoseObjectName,
            CCS_CharacterControllerConstants.RightUpperArmAimTestObjectName,
            CCS_CharacterControllerConstants.RightClavicleAimTestObjectName,
            CCS_CharacterControllerConstants.LeftUpperArmAimTestObjectName,
            CCS_CharacterControllerConstants.LeftClavicleAimTestObjectName,
            CCS_CharacterControllerConstants.RightUpperArmAimSourceObjectName,
            CCS_CharacterControllerConstants.LeftUpperArmAimSourceObjectName,
            CCS_EquipmentConstants.WeaponIkRigObjectName,
            CCS_EquipmentConstants.WeaponIkTargetsObjectName,
            "CCS_DualRevolverArmAimRigTargets",
            "RightArmTwoBoneIK",
            "LeftArmTwoBoneIK",
            "RightHandTwoBoneIK",
            "LeftHandTwoBoneIK",
            "WeaponAimConstraint",
            CCS_EquipmentConstants.RightHandIkTargetObjectName,
            CCS_EquipmentConstants.RightElbowHintObjectName,
            CCS_EquipmentConstants.LeftHandIkTargetObjectName,
            CCS_EquipmentConstants.LeftElbowHintObjectName,
            CCS_EquipmentConstants.WeaponAimTargetObjectName,
            CCS_CharacterControllerConstants.DualRevolverRightAimRigTargetObjectName,
            CCS_CharacterControllerConstants.DualRevolverLeftAimRigTargetObjectName,
            CCS_CharacterControllerConstants.DualRevolverRightElbowHintObjectName,
            CCS_CharacterControllerConstants.DualRevolverLeftElbowHintObjectName,
            CCS_WeaponsConstants.RevolverArmReticleIkRigObjectName,
            CCS_WeaponsConstants.RevolverArmReticleIkRootObjectName,
            CCS_WeaponsConstants.RightHandReticleIkTargetObjectName,
            CCS_WeaponsConstants.ReticleAimWorldTargetObjectName,
        };

        private static readonly string[] ProceduralArmScriptTypeNamesToRemove =
        {
            "CCS_DualRevolverAimConvergenceRigPresenter",
            "CCS_DualRevolverArmAimBiasPresenter",
            "CCS_DualRevolverArmAimConstraintPresenter",
            "CCS_ManualArmRotationBiasPresenter",
        };

        public static bool CleanProceduralArmConvergence(
            Transform modelRoot,
            GameObject prefabRoot,
            StrippedCharacterControllerBaselineStripResult result)
        {
            if (modelRoot == null || prefabRoot == null)
            {
                return false;
            }

            bool changed = false;
            changed |= RemoveComponentsByTypeNames(modelRoot.gameObject, ProceduralArmScriptTypeNamesToRemove, result);
            changed |= RemoveComponentsByTypeNames(prefabRoot, ProceduralArmScriptTypeNamesToRemove, result);
            changed |= RemoveProceduralRigConstraints(prefabRoot.transform, result);

            for (int i = 0; i < ProceduralRigObjectNamesToRemove.Length; i++)
            {
                changed |= DestroyAllByName(prefabRoot.transform, ProceduralRigObjectNamesToRemove[i], result);
            }

            changed |= DestroyObjectsByNameSuffix(prefabRoot.transform, "Aim_Test", result);
            changed |= DestroyObjectsByNameSuffix(prefabRoot.transform, "RotationBias_Test", result);
            changed |= DestroyObjectsByNameSuffix(prefabRoot.transform, "RotationBiasPose", result);
            changed |= DestroyObjectsByNameSuffix(prefabRoot.transform, "UpperArmAimSource", result);
            changed |= DestroyObjectsByNameSuffix(prefabRoot.transform, "ClavicleAimSource", result);

            changed |= EnsurePassiveVisualAimReferenceHierarchy(modelRoot, result);
            changed |= RemoveUnusedRigBuilder(modelRoot, result);
            changed |= DeleteUnusedManualRotationBiasProfileAsset(result);

            return changed;
        }

        private static bool EnsurePassiveVisualAimReferenceHierarchy(
            Transform modelRoot,
            StrippedCharacterControllerBaselineStripResult result)
        {
            bool changed = false;
            Transform aimingRoot = modelRoot.Find(CCS_CharacterControllerConstants.DualRevolverAimAimingObjectName);
            if (aimingRoot == null)
            {
                GameObject aimingObject = new GameObject(CCS_CharacterControllerConstants.DualRevolverAimAimingObjectName);
                aimingRoot = aimingObject.transform;
                aimingRoot.SetParent(modelRoot, false);
                changed = true;
                result.Notes.Add("Created Model/Aiming for passive visual aim reference.");
            }

            Transform sharedAimPoint = FindSharedAimPointCandidate(modelRoot);
            Transform visualReferenceRoot = aimingRoot.Find(
                CCS_CharacterControllerConstants.DualRevolverVisualAimReferenceObjectName);
            if (visualReferenceRoot == null)
            {
                GameObject visualReferenceObject = new GameObject(
                    CCS_CharacterControllerConstants.DualRevolverVisualAimReferenceObjectName);
                visualReferenceRoot = visualReferenceObject.transform;
                visualReferenceRoot.SetParent(aimingRoot, false);
                changed = true;
                result.Notes.Add("Created Model/Aiming/CCS_DualRevolverVisualAimReference.");
            }

            if (sharedAimPoint == null)
            {
                sharedAimPoint = EnsureChild(
                    visualReferenceRoot,
                    CCS_CharacterControllerConstants.SharedDualAimPointObjectName);
                changed = true;
                result.Notes.Add("Created CCS_SharedDualAimPoint under passive visual aim reference.");
            }
            else
            {
                if (sharedAimPoint.parent != visualReferenceRoot)
                {
                    sharedAimPoint.SetParent(visualReferenceRoot, true);
                    changed = true;
                    result.Notes.Add("Moved CCS_SharedDualAimPoint under CCS_DualRevolverVisualAimReference.");
                }

                if (sharedAimPoint.name != CCS_CharacterControllerConstants.SharedDualAimPointObjectName)
                {
                    sharedAimPoint.name = CCS_CharacterControllerConstants.SharedDualAimPointObjectName;
                    changed = true;
                }
            }

            if (!visualReferenceRoot.gameObject.activeSelf)
            {
                visualReferenceRoot.gameObject.SetActive(true);
                changed = true;
            }

            if (!sharedAimPoint.gameObject.activeSelf)
            {
                sharedAimPoint.gameObject.SetActive(true);
                changed = true;
            }

            changed |= DisableDuplicateSharedAimPoints(modelRoot, sharedAimPoint, result);
            return changed;
        }

        private static bool RemoveProceduralRigConstraints(
            Transform prefabRoot,
            StrippedCharacterControllerBaselineStripResult result)
        {
            bool changed = false;
            MultiAimConstraint[] multiAimConstraints = prefabRoot.GetComponentsInChildren<MultiAimConstraint>(true);
            for (int i = multiAimConstraints.Length - 1; i >= 0; i--)
            {
                MultiAimConstraint constraint = multiAimConstraints[i];
                if (constraint == null)
                {
                    continue;
                }

                string constraintName = constraint.name;
                Object.DestroyImmediate(constraint.gameObject, true);
                changed = true;
                result.Notes.Add("Removed MultiAimConstraint object '" + constraintName + "'.");
            }

            MultiRotationConstraint[] multiRotationConstraints =
                prefabRoot.GetComponentsInChildren<MultiRotationConstraint>(true);
            for (int i = multiRotationConstraints.Length - 1; i >= 0; i--)
            {
                MultiRotationConstraint constraint = multiRotationConstraints[i];
                if (constraint == null)
                {
                    continue;
                }

                string constraintName = constraint.name;
                Object.DestroyImmediate(constraint.gameObject, true);
                changed = true;
                result.Notes.Add("Removed MultiRotationConstraint object '" + constraintName + "'.");
            }

            TwoBoneIKConstraint[] twoBoneConstraints = prefabRoot.GetComponentsInChildren<TwoBoneIKConstraint>(true);
            for (int i = twoBoneConstraints.Length - 1; i >= 0; i--)
            {
                TwoBoneIKConstraint constraint = twoBoneConstraints[i];
                if (constraint == null)
                {
                    continue;
                }

                string constraintName = constraint.name;
                Object.DestroyImmediate(constraint.gameObject, true);
                changed = true;
                result.Notes.Add("Removed TwoBoneIKConstraint object '" + constraintName + "'.");
            }

            return changed;
        }

        private static bool RemoveUnusedRigBuilder(Transform modelRoot, StrippedCharacterControllerBaselineStripResult result)
        {
            Animator animator = modelRoot.GetComponentInChildren<Animator>(true);
            if (animator == null)
            {
                return false;
            }

            RigBuilder rigBuilder = animator.GetComponent<RigBuilder>();
            if (rigBuilder == null)
            {
                return false;
            }

            bool changed = false;
            SerializedObject serializedRigBuilder = new SerializedObject(rigBuilder);
            SerializedProperty layersProperty = serializedRigBuilder.FindProperty("m_RigLayers");
            if (layersProperty != null)
            {
                for (int i = layersProperty.arraySize - 1; i >= 0; i--)
                {
                    SerializedProperty layerProperty = layersProperty.GetArrayElementAtIndex(i);
                    SerializedProperty rigProperty = layerProperty.FindPropertyRelative("m_Rig");
                    Rig rig = rigProperty != null ? rigProperty.objectReferenceValue as Rig : null;
                    if (rig == null)
                    {
                        layersProperty.DeleteArrayElementAtIndex(i);
                        changed = true;
                    }
                }

                if (changed)
                {
                    serializedRigBuilder.ApplyModifiedPropertiesWithoutUndo();
                    result.Notes.Add("Removed RigBuilder layers with missing rig references.");
                }
            }

            if (layersProperty == null || layersProperty.arraySize == 0)
            {
                Object.DestroyImmediate(rigBuilder, true);
                changed = true;
                result.Notes.Add("Removed unused RigBuilder from Kevin Animator.");
            }

            return changed;
        }

        private static bool DeleteUnusedManualRotationBiasProfileAsset(
            StrippedCharacterControllerBaselineStripResult result)
        {
            string assetPath = CCS_CharacterControllerConstants.ManualDualRevolverArmRotationBiasProfilePath;
            if (!AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath))
            {
                return false;
            }

            if (!AssetDatabase.DeleteAsset(assetPath))
            {
                return false;
            }

            result.Notes.Add("Deleted unused CCS_ManualDualRevolverArmRotationBiasProfile asset.");
            return true;
        }

        private static Transform FindSharedAimPointCandidate(Transform modelRoot)
        {
            Transform[] transforms = modelRoot.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform candidate = transforms[i];
                if (candidate.name == CCS_CharacterControllerConstants.SharedDualAimPointObjectName
                    || candidate.name == "CCS_DualRevolverAimConvergenceTarget")
                {
                    return candidate;
                }
            }

            return null;
        }

        private static bool DisableDuplicateSharedAimPoints(
            Transform modelRoot,
            Transform canonicalSharedAimPoint,
            StrippedCharacterControllerBaselineStripResult result)
        {
            bool changed = false;
            Transform[] transforms = modelRoot.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform candidate = transforms[i];
                if (candidate == null
                    || candidate == canonicalSharedAimPoint
                    || candidate.name != CCS_CharacterControllerConstants.SharedDualAimPointObjectName)
                {
                    continue;
                }

                Object.DestroyImmediate(candidate.gameObject, true);
                changed = true;
                result.Notes.Add("Removed duplicate CCS_SharedDualAimPoint.");
            }

            return changed;
        }

        private static bool RemoveComponentsByTypeNames(
            GameObject targetObject,
            string[] typeNames,
            StrippedCharacterControllerBaselineStripResult result)
        {
            bool changed = false;
            Component[] components = targetObject.GetComponentsInChildren<Component>(true);
            for (int i = components.Length - 1; i >= 0; i--)
            {
                Component component = components[i];
                if (component == null)
                {
                    continue;
                }

                string typeName = component.GetType().Name;
                for (int j = 0; j < typeNames.Length; j++)
                {
                    if (typeName != typeNames[j])
                    {
                        continue;
                    }

                    Object.DestroyImmediate(component, true);
                    changed = true;
                    result.Notes.Add("Removed procedural arm script " + typeName + ".");
                    break;
                }
            }

            return changed;
        }

        private static bool DestroyAllByName(
            Transform searchRoot,
            string objectName,
            StrippedCharacterControllerBaselineStripResult result)
        {
            bool changed = false;
            Transform[] transforms = searchRoot.GetComponentsInChildren<Transform>(true);
            for (int i = transforms.Length - 1; i >= 0; i--)
            {
                Transform candidate = transforms[i];
                if (candidate == null || candidate.name != objectName)
                {
                    continue;
                }

                Object.DestroyImmediate(candidate.gameObject, true);
                changed = true;
                result.Notes.Add("Removed procedural rig object '" + objectName + "'.");
            }

            return changed;
        }

        private static bool DestroyObjectsByNameSuffix(
            Transform searchRoot,
            string suffix,
            StrippedCharacterControllerBaselineStripResult result)
        {
            bool changed = false;
            Transform[] transforms = searchRoot.GetComponentsInChildren<Transform>(true);
            for (int i = transforms.Length - 1; i >= 0; i--)
            {
                Transform candidate = transforms[i];
                if (candidate == null || !candidate.name.EndsWith(suffix))
                {
                    continue;
                }

                string objectName = candidate.name;
                Object.DestroyImmediate(candidate.gameObject, true);
                changed = true;
                result.Notes.Add("Removed procedural rig object '" + objectName + "'.");
            }

            return changed;
        }

        private static Transform EnsureChild(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child != null)
            {
                return child;
            }

            GameObject childObject = new GameObject(childName);
            child = childObject.transform;
            child.SetParent(parent, false);
            return child;
        }
    }
}
