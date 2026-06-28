using System.Collections.Generic;
using CCS.Modules.CharacterController.Tests;
using CCS.Modules.Weapons;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerVisualAndAnimatorBindingBuilder
// CATEGORY: Modules / CharacterController / Editor
// PURPOSE: Repairs v0.8.1 player visual contamination and authoritative Animator wiring.
// PLACEMENT: Editor builder. Invoked from batch entry and binding menu.
// AUTHOR: James Schilz
// CREATED: 2026-06-28
// NOTES: Strips test capsule/glasses from production; organizes harness TestVisuals; wires Animator refs.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_PlayerVisualAndAnimatorBindingBuilder
    {
        private static readonly string[] AnimatorReferencePropertyNames =
        {
            "animator",
            "characterAnimator",
        };

        #region Public Methods

        public static bool EnsurePlayerVisualAndAnimatorBinding()
        {
            bool changed = false;
            changed |= RepairPrefab(CCS_PlayerPrefabConstants.ProductionPlayerPrefabPath, isProduction: true);
            changed |= RepairPrefab(CCS_PlayerPrefabConstants.TestHarnessPlayerPrefabPath, isProduction: false);
            changed |= RepairPrefab(CCS_PlayerPrefabConstants.LegacyMasterTestPlayerPrefabPath, isProduction: false);

            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return changed;
        }

        public static bool IsTestVisualObjectName(string objectName)
        {
            return CCS_PlayerAnimatorResolver.IsTestVisualObjectName(objectName);
        }

        public static bool TryResolveAuthoritativeAnimator(GameObject prefabRoot, out Animator animator)
        {
            animator = null;
            if (prefabRoot == null)
            {
                return false;
            }

            if (CCS_PlayerAnimatorResolver.TryResolveAuthoritativeAnimator(
                    prefabRoot.transform,
                    out animator,
                    out _))
            {
                return true;
            }

            RuntimeAnimatorController expectedController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            Animator[] animators = prefabRoot.GetComponentsInChildren<Animator>(true);
            for (int animatorIndex = 0; animatorIndex < animators.Length; animatorIndex++)
            {
                Animator candidate = animators[animatorIndex];
                if (candidate == null
                    || CCS_PlayerAnimatorResolver.IsUnderTestVisuals(candidate.transform)
                    || candidate.avatar == null
                    || !candidate.avatar.isHuman)
                {
                    continue;
                }

                if (expectedController != null
                    && candidate.runtimeAnimatorController != expectedController
                    && candidate.runtimeAnimatorController != null
                    && candidate.runtimeAnimatorController.name != expectedController.name)
                {
                    continue;
                }

                animator = candidate;
                return true;
            }

            return false;
        }

        #endregion

        #region Private Methods

        private static bool RepairPrefab(string prefabPath, bool isProduction)
        {
            if (string.IsNullOrEmpty(prefabPath) || !System.IO.File.Exists(prefabPath))
            {
                Debug.LogError("[Player Visual/Animator Binding] Missing prefab: " + prefabPath);
                return false;
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError("[Player Visual/Animator Binding] Could not load prefab: " + prefabPath);
                return false;
            }

            bool changed = false;
            Transform presentation = EnsureChildTransform(
                prefabRoot.transform,
                CCS_PlayerPrefabConstants.PresentationObjectName,
                ref changed);
            Transform visualRoot = EnsureChildTransform(presentation, "VisualRoot", ref changed);

            if (isProduction)
            {
                changed |= RemoveTestVisualContamination(prefabRoot);
            }
            else
            {
                changed |= OrganizeTestVisualsUnderHarnessOnly(prefabRoot, presentation);
            }

            changed |= EnsurePlayerVisualPrefabInstance(visualRoot);
            changed |= EnsureAuthoritativeAnimatorSettings(prefabRoot);
            changed |= EnsureSpawnDiagnosticsComponents(prefabRoot, isProduction);
            changed |= WireAuthoritativeAnimatorReferences(prefabRoot);

            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return changed;
        }

        private static bool RemoveTestVisualContamination(GameObject prefabRoot)
        {
            bool changed = false;
            List<Transform> testVisualTransforms = CollectTestVisualTransforms(prefabRoot.transform);
            for (int index = testVisualTransforms.Count - 1; index >= 0; index--)
            {
                Transform testVisual = testVisualTransforms[index];
                if (testVisual == null)
                {
                    continue;
                }

                Object.DestroyImmediate(testVisual.gameObject, true);
                changed = true;
            }

            return changed;
        }

        private static bool OrganizeTestVisualsUnderHarnessOnly(GameObject prefabRoot, Transform presentation)
        {
            bool changed = false;
            Transform testVisualsRoot = EnsureChildTransform(
                presentation != null ? presentation : prefabRoot.transform,
                CCS_PlayerPrefabConstants.TestVisualsObjectName,
                ref changed);

            List<Transform> testVisualTransforms = CollectTestVisualTransforms(prefabRoot.transform);
            HashSet<string> movedNames = new HashSet<string>();
            for (int index = 0; index < testVisualTransforms.Count; index++)
            {
                Transform testVisual = testVisualTransforms[index];
                if (testVisual == null || testVisual == testVisualsRoot)
                {
                    continue;
                }

                if (testVisual.IsChildOf(testVisualsRoot))
                {
                    if (movedNames.Contains(testVisual.name))
                    {
                        Object.DestroyImmediate(testVisual.gameObject, true);
                        changed = true;
                    }
                    else
                    {
                        movedNames.Add(testVisual.name);
                    }

                    continue;
                }

                if (movedNames.Contains(testVisual.name))
                {
                    Object.DestroyImmediate(testVisual.gameObject, true);
                    changed = true;
                    continue;
                }

                testVisual.SetParent(testVisualsRoot, false);
                movedNames.Add(testVisual.name);
                changed = true;
            }

            return changed;
        }

        private static List<Transform> CollectTestVisualTransforms(Transform root)
        {
            List<Transform> results = new List<Transform>();
            CollectTestVisualTransformsRecursive(root, results);
            return results;
        }

        private static void CollectTestVisualTransformsRecursive(Transform current, List<Transform> results)
        {
            if (current == null)
            {
                return;
            }

            if (IsTestVisualObjectName(current.name))
            {
                results.Add(current);
            }

            for (int childIndex = 0; childIndex < current.childCount; childIndex++)
            {
                Transform child = current.GetChild(childIndex);
                if (child.name == CCS_PlayerPrefabConstants.PlayerVisualPrefabInstanceName)
                {
                    continue;
                }

                CollectTestVisualTransformsRecursive(child, results);
            }
        }

        private static bool EnsurePlayerVisualPrefabInstance(Transform visualRoot)
        {
            if (visualRoot == null)
            {
                return false;
            }

            Transform existingVisual = FindChildRecursive(
                visualRoot,
                CCS_PlayerPrefabConstants.PlayerVisualPrefabInstanceName);
            if (existingVisual != null)
            {
                return false;
            }

            GameObject visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_CharacterControllerConstants.PlayerVisualPrefabPath);
            if (visualPrefab == null)
            {
                Debug.LogError(
                    "[Player Visual/Animator Binding] Missing player visual prefab at "
                    + CCS_CharacterControllerConstants.PlayerVisualPrefabPath);
                return false;
            }

            GameObject visualInstance = PrefabUtility.InstantiatePrefab(visualPrefab, visualRoot) as GameObject;
            if (visualInstance == null)
            {
                return false;
            }

            visualInstance.name = CCS_PlayerPrefabConstants.PlayerVisualPrefabInstanceName;
            visualInstance.transform.localPosition = new Vector3(0f, -0.079f, 0f);
            visualInstance.transform.localRotation = Quaternion.identity;
            visualInstance.transform.localScale = Vector3.one;
            return true;
        }

        private static bool EnsureAuthoritativeAnimatorSettings(GameObject prefabRoot)
        {
            if (!TryResolveAuthoritativeAnimator(prefabRoot, out Animator animator) || animator == null)
            {
                return false;
            }

            bool changed = false;
            RuntimeAnimatorController expectedController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            if (expectedController != null && animator.runtimeAnimatorController != expectedController)
            {
                animator.runtimeAnimatorController = expectedController;
                changed = true;
            }

            if (animator.applyRootMotion)
            {
                animator.applyRootMotion = false;
                changed = true;
            }

            if (!animator.enabled)
            {
                animator.enabled = true;
                changed = true;
            }

            return changed;
        }

        private static bool WireAuthoritativeAnimatorReferences(GameObject prefabRoot)
        {
            if (!TryResolveAuthoritativeAnimator(prefabRoot, out Animator authoritativeAnimator)
                || authoritativeAnimator == null)
            {
                return false;
            }

            bool changed = false;
            changed |= WireAnimatorOnComponent(
                prefabRoot.GetComponent<CCS_PlayerRuntimeFacade>(),
                authoritativeAnimator);
            changed |= WireAnimatorOnComponent(
                FindComponentOnVisualRoot<CCS_PlayerLocomotionAnimator>(prefabRoot),
                authoritativeAnimator);
            changed |= WireAnimatorOnComponent(
                FindComponentOnVisualRoot<CCS_PlayerInteractionAnimator>(prefabRoot),
                authoritativeAnimator);
            changed |= WireAnimatorOnComponent(
                FindComponentOnVisualRoot<CCS_RevolverUpperBodyAnimator>(prefabRoot),
                authoritativeAnimator);
            changed |= WireAnimatorOnComponent(
                FindComponentOnVisualRoot<CCS_PlayerAnimatorRuntimeDiagnostics>(prefabRoot),
                authoritativeAnimator);
            changed |= WireAnimatorOnComponent(
                prefabRoot.GetComponentInChildren<CCS_LocalFirstPersonHeadVisibility>(true),
                authoritativeAnimator);
            changed |= WireAnimatorOnComponent(
                prefabRoot.GetComponentInChildren<CCS_FirstPersonBodyCameraAnchor>(true),
                authoritativeAnimator);
            changed |= WireAnimatorOnComponent(
                prefabRoot.GetComponentInChildren<CCS_RevolverArmReticleIK>(true),
                authoritativeAnimator);
            changed |= WireAnimatorOnComponent(
                prefabRoot.GetComponentInChildren<CCS_RevolverBodyAimFollowController>(true),
                authoritativeAnimator);
            return changed;
        }

        private static T FindComponentOnVisualRoot<T>(GameObject prefabRoot) where T : Component
        {
            Transform presentation = prefabRoot.transform.Find(CCS_PlayerPrefabConstants.PresentationObjectName);
            Transform visualRoot = presentation != null
                ? presentation.Find("VisualRoot")
                : prefabRoot.transform.Find("VisualRoot");
            return visualRoot != null
                ? visualRoot.GetComponent<T>()
                : prefabRoot.GetComponentInChildren<T>(true);
        }

        private static bool WireAnimatorOnComponent(Component component, Animator authoritativeAnimator)
        {
            if (component == null || authoritativeAnimator == null)
            {
                return false;
            }

            SerializedObject serializedObject = new SerializedObject(component);
            bool changed = false;
            for (int propertyIndex = 0; propertyIndex < AnimatorReferencePropertyNames.Length; propertyIndex++)
            {
                SerializedProperty property = serializedObject.FindProperty(AnimatorReferencePropertyNames[propertyIndex]);
                if (property == null || property.objectReferenceValue == authoritativeAnimator)
                {
                    continue;
                }

                property.objectReferenceValue = authoritativeAnimator;
                changed = true;
            }

            if (changed)
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool EnsureSpawnDiagnosticsComponents(GameObject prefabRoot, bool isProduction)
        {
            Transform visualRoot = FindChildRecursive(prefabRoot.transform, "VisualRoot");
            if (visualRoot == null)
            {
                return false;
            }

            bool changed = false;
            changed |= EnsureComponentWithDefaults<CCS_PlayerAnimatorSpawnBindingReport>(
                visualRoot.gameObject,
                report => SetBoolIfFalse(report, "enableSpawnReport", true));

            if (isProduction)
            {
                return changed;
            }

            changed |= EnsureComponentWithDefaults<CCS_PlayerAnimatorRuntimeDiagnostics>(
                visualRoot.gameObject,
                diagnostics =>
                {
                    SetBoolIfFalse(diagnostics, "enableDiagnostics", false);
                    SetBoolIfFalse(diagnostics, "logToConsole", false);
                    SetBoolIfFalse(diagnostics, "writeMarkdownReport", false);
                });
            changed |= EnsureComponentWithDefaults<CCS_PlayerAnimationPoseDeltaDiagnostic>(
                visualRoot.gameObject,
                poseDelta =>
                {
                    SetBoolIfFalse(poseDelta, "enablePoseDeltaReport", false);
                    SetBoolIfFalse(poseDelta, "logSampleResultsToConsole", false);
                });
            return changed;
        }

        private static bool EnsureComponentWithDefaults<T>(GameObject target, System.Action<T> applyDefaults)
            where T : Component
        {
            T component = target.GetComponent<T>();
            if (component == null)
            {
                component = target.AddComponent<T>();
            }

            applyDefaults?.Invoke(component);
            return true;
        }

        private static void SetBoolIfFalse(Component component, string propertyName, bool value)
        {
            SerializedObject serializedObject = new SerializedObject(component);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.propertyType != SerializedPropertyType.Boolean)
            {
                return;
            }

            if (property.boolValue == value)
            {
                return;
            }

            property.boolValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Transform EnsureChildTransform(Transform parent, string childName, ref bool changed)
        {
            Transform existing = parent.Find(childName);
            if (existing != null)
            {
                return existing;
            }

            GameObject childObject = new GameObject(childName);
            childObject.transform.SetParent(parent, false);
            childObject.transform.localPosition = Vector3.zero;
            childObject.transform.localRotation = Quaternion.identity;
            childObject.transform.localScale = Vector3.one;
            changed = true;
            return childObject.transform;
        }

        private static Transform FindChildRecursive(Transform parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }

            if (parent.name == childName)
            {
                return parent;
            }

            for (int childIndex = 0; childIndex < parent.childCount; childIndex++)
            {
                Transform match = FindChildRecursive(parent.GetChild(childIndex), childName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        #endregion
    }
}
