using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController.Tests;
using CCS.Modules.Weapons;
using CCS.Project;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerVisualAndAnimatorBindingValidationUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Validates v0.8.1 player visual recovery and authoritative Animator binding.
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-28
// NOTES: Fails production prefabs with test visuals or mismatched Animator references.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_PlayerVisualAndAnimatorBindingValidationUtility
    {
        private static readonly System.Type[] AnimationDrivingComponentTypes =
        {
            typeof(CCS_PlayerLocomotionAnimator),
            typeof(CCS_PlayerInteractionAnimator),
            typeof(CCS_RevolverUpperBodyAnimator),
            typeof(CCS_LocalFirstPersonHeadVisibility),
            typeof(CCS_FirstPersonBodyCameraAnchor),
            typeof(CCS_RevolverArmReticleIK),
            typeof(CCS_RevolverBodyAimFollowController),
        };

        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateAllPlayerVisualAndAnimatorBinding()
        {
            List<string> failures = new List<string>();
            AppendResult(failures, ValidateControllerParameterAgreement());
            AppendResult(
                failures,
                ValidatePrefabVisualAndAnimatorBinding(
                    CCS_PlayerPrefabConstants.ProductionPlayerPrefabPath,
                    strictProductionRules: true,
                    label: "Production player prefab"));
            AppendResult(
                failures,
                ValidatePrefabVisualAndAnimatorBinding(
                    CCS_PlayerPrefabConstants.TestHarnessPlayerPrefabPath,
                    strictProductionRules: false,
                    label: "Test harness player prefab"));
            AppendResult(
                failures,
                ValidatePrefabVisualAndAnimatorBinding(
                    CCS_PlayerPrefabConstants.LegacyMasterTestPlayerPrefabPath,
                    strictProductionRules: false,
                    label: "Legacy Master Test player prefab"));
            AppendResult(failures, ValidateProductionBuilderDoesNotContainTestVisuals());

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Player visual and Animator binding validation passed.");
        }

        public static CCS_SurvivalValidationResult ValidateControllerParameterAgreement()
        {
            List<string> failures = new List<string>();
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            AppendIfMissing(
                failures,
                controller != null,
                "Missing player locomotion AnimatorController at "
                + CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath
                + ".");

            if (controller == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            HashSet<string> controllerParameterNames = new HashSet<string>();
            AnimatorControllerParameter[] parameters = controller.parameters;
            for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
            {
                controllerParameterNames.Add(parameters[parameterIndex].name);
            }

            string[] requiredNames = CCS_PlayerAnimatorParameterIds.RequiredControllerParameterNames;
            for (int requiredIndex = 0; requiredIndex < requiredNames.Length; requiredIndex++)
            {
                string requiredName = requiredNames[requiredIndex];
                if (!controllerParameterNames.Contains(requiredName))
                {
                    failures.Add(
                        "AnimatorController missing required parameter "
                        + requiredName
                        + " referenced by CCS_PlayerAnimatorParameterIds.");
                }
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Animator controller parameter agreement validated.");
        }

        public static CCS_SurvivalValidationResult ValidateProductionBuilderDoesNotContainTestVisuals()
        {
            if (!File.Exists(CCS_PlayerPrefabConstants.ProductionPlayerPrefabPath))
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Production builder validation failed: missing "
                    + CCS_PlayerPrefabConstants.ProductionPlayerPrefabPath
                    + ".");
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(
                CCS_PlayerPrefabConstants.ProductionPlayerPrefabPath);
            if (prefabRoot == null)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Production builder validation failed: could not load production prefab.");
            }

            try
            {
                List<string> failures = new List<string>();
                ValidateNoTestVisualContamination(failures, prefabRoot, "Production builder regression");
                return failures.Count > 0
                    ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                    : CCS_SurvivalValidationResult.Pass(
                        "Production prefab builder regression check passed (no test visuals).");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        #endregion

        #region Private Methods

        private static CCS_SurvivalValidationResult ValidatePrefabVisualAndAnimatorBinding(
            string prefabPath,
            bool strictProductionRules,
            string label)
        {
            List<string> failures = new List<string>();
            AppendIfMissing(failures, File.Exists(prefabPath), label + " missing at " + prefabPath + ".");

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                return CCS_SurvivalValidationResult.Fail(label + " PrefabUtility.LoadPrefabContents failed.");
            }

            try
            {
                if (strictProductionRules)
                {
                    ValidateNoTestVisualContamination(failures, prefabRoot, label);
                    ValidateNoUnauthorizedPrimitiveMeshRenderers(failures, prefabRoot, label);
                }
                else
                {
                    ValidateHarnessTestVisualPlacement(failures, prefabRoot, label);
                }

                ValidateAuthoritativeAnimator(failures, prefabRoot, label);
                ValidateAnimationScriptAnimatorReferences(failures, prefabRoot, label);
                ValidatePlayerVisualInstance(failures, prefabRoot, label);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass(label + " visual/Animator binding validated.");
        }

        private static void ValidateNoTestVisualContamination(
            List<string> failures,
            GameObject prefabRoot,
            string label)
        {
            Transform[] transforms = prefabRoot.GetComponentsInChildren<Transform>(true);
            for (int transformIndex = 0; transformIndex < transforms.Length; transformIndex++)
            {
                Transform current = transforms[transformIndex];
                if (current == null || !CCS_PlayerVisualAndAnimatorBindingBuilder.IsTestVisualObjectName(current.name))
                {
                    continue;
                }

                failures.Add(
                    label
                    + " must not contain test visual object '"
                    + current.name
                    + "' at "
                    + BuildTransformPath(current, prefabRoot.transform)
                    + ".");
            }
        }

        private static void ValidateNoUnauthorizedPrimitiveMeshRenderers(
            List<string> failures,
            GameObject prefabRoot,
            string label)
        {
            Transform presentation = prefabRoot.transform.Find(CCS_PlayerPrefabConstants.PresentationObjectName);
            if (presentation == null)
            {
                return;
            }

            MeshRenderer[] renderers = presentation.GetComponentsInChildren<MeshRenderer>(true);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                MeshRenderer meshRenderer = renderers[rendererIndex];
                if (meshRenderer == null)
                {
                    continue;
                }

                Transform rendererTransform = meshRenderer.transform;
                if (CCS_PlayerVisualAndAnimatorBindingBuilder.IsTestVisualObjectName(rendererTransform.name))
                {
                    failures.Add(
                        label
                        + " Presentation contains test visual MeshRenderer on "
                        + BuildTransformPath(rendererTransform, prefabRoot.transform)
                        + ".");
                    continue;
                }

                MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();
                if (meshFilter == null || meshFilter.sharedMesh == null)
                {
                    continue;
                }

                string meshName = meshFilter.sharedMesh.name;
                if (meshName == "Capsule" || meshName == "Cube" || meshName == "Sphere" || meshName == "Cylinder")
                {
                    failures.Add(
                        label
                        + " Presentation contains unauthorized primitive MeshRenderer ("
                        + meshName
                        + ") on "
                        + BuildTransformPath(rendererTransform, prefabRoot.transform)
                        + ".");
                }
            }
        }

        private static void ValidateHarnessTestVisualPlacement(
            List<string> failures,
            GameObject prefabRoot,
            string label)
        {
            Transform[] transforms = prefabRoot.GetComponentsInChildren<Transform>(true);
            for (int transformIndex = 0; transformIndex < transforms.Length; transformIndex++)
            {
                Transform current = transforms[transformIndex];
                if (current == null || !CCS_PlayerVisualAndAnimatorBindingBuilder.IsTestVisualObjectName(current.name))
                {
                    continue;
                }

                if (IsUnderNamedAncestor(current, CCS_PlayerPrefabConstants.TestVisualsObjectName)
                    || IsUnderNamedAncestor(current, "TestHarnessOnly"))
                {
                    continue;
                }

                failures.Add(
                    label
                    + " test visual '"
                    + current.name
                    + "' must live under TestVisuals or TestHarnessOnly (found at "
                    + BuildTransformPath(current, prefabRoot.transform)
                    + ").");
            }
        }

        private static void ValidateAuthoritativeAnimator(
            List<string> failures,
            GameObject prefabRoot,
            string label)
        {
            if (!CCS_PlayerVisualAndAnimatorBindingBuilder.TryResolveAuthoritativeAnimator(
                    prefabRoot,
                    out Animator authoritativeAnimator)
                || authoritativeAnimator == null)
            {
                failures.Add(label + " is missing an authoritative humanoid gameplay Animator.");
                return;
            }

            Animator[] animators = prefabRoot.GetComponentsInChildren<Animator>(true);
            int authoritativeCount = 0;
            RuntimeAnimatorController expectedController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);

            for (int animatorIndex = 0; animatorIndex < animators.Length; animatorIndex++)
            {
                Animator candidate = animators[animatorIndex];
                if (candidate == null
                    || CCS_PlayerAnimatorResolver.IsUnderTestVisuals(candidate.transform)
                    || candidate.avatar == null
                    || !candidate.avatar.isHuman
                    || candidate.runtimeAnimatorController == null)
                {
                    continue;
                }

                authoritativeCount++;
            }

            if (authoritativeCount != 1)
            {
                failures.Add(
                    label
                    + " must contain exactly one authoritative gameplay Animator (found "
                    + authoritativeCount
                    + ").");
            }

            if (expectedController != null && authoritativeAnimator.runtimeAnimatorController != expectedController)
            {
                failures.Add(
                    label
                    + " authoritative Animator must use "
                    + CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath
                    + ".");
            }

            Transform presentation = prefabRoot.transform.Find(CCS_PlayerPrefabConstants.PresentationObjectName);
            Transform visualRoot = presentation != null ? presentation.Find("VisualRoot") : null;
            if (visualRoot != null && !authoritativeAnimator.transform.IsChildOf(visualRoot))
            {
                failures.Add(
                    label
                    + " authoritative Animator must be under Presentation/VisualRoot (found at "
                    + BuildTransformPath(authoritativeAnimator.transform, prefabRoot.transform)
                    + ").");
            }

            if (authoritativeAnimator.applyRootMotion)
            {
                failures.Add(label + " authoritative Animator.applyRootMotion must be false.");
            }

            CCS_PlayerRuntimeFacade facade = prefabRoot.GetComponent<CCS_PlayerRuntimeFacade>();
            if (facade != null && facade.Animator != authoritativeAnimator)
            {
                failures.Add(label + " CCS_PlayerRuntimeFacade.Animator must reference the authoritative Animator.");
            }
        }

        private static void ValidateAnimationScriptAnimatorReferences(
            List<string> failures,
            GameObject prefabRoot,
            string label)
        {
            if (!CCS_PlayerVisualAndAnimatorBindingBuilder.TryResolveAuthoritativeAnimator(
                    prefabRoot,
                    out Animator authoritativeAnimator)
                || authoritativeAnimator == null)
            {
                return;
            }

            for (int typeIndex = 0; typeIndex < AnimationDrivingComponentTypes.Length; typeIndex++)
            {
                System.Type componentType = AnimationDrivingComponentTypes[typeIndex];
                Component[] components = prefabRoot.GetComponentsInChildren(componentType, true);
                for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
                {
                    Component component = components[componentIndex];
                    if (component == null)
                    {
                        continue;
                    }

                    Animator referencedAnimator = ReadAnimatorReference(component);
                    if (referencedAnimator == null)
                    {
                        failures.Add(
                            label
                            + " "
                            + componentType.Name
                            + " on "
                            + component.gameObject.name
                            + " has a null Animator reference.");
                        continue;
                    }

                    if (referencedAnimator != authoritativeAnimator)
                    {
                        failures.Add(
                            label
                            + " "
                            + componentType.Name
                            + " on "
                            + component.gameObject.name
                            + " must reference the authoritative Animator.");
                    }
                }
            }
        }

        private static void ValidatePlayerVisualInstance(
            List<string> failures,
            GameObject prefabRoot,
            string label)
        {
            Transform presentation = prefabRoot.transform.Find(CCS_PlayerPrefabConstants.PresentationObjectName);
            Transform visualRoot = presentation != null
                ? presentation.Find("VisualRoot")
                : prefabRoot.transform.Find("VisualRoot");
            if (visualRoot == null)
            {
                failures.Add(label + " is missing VisualRoot.");
                return;
            }

            Transform visualInstance = visualRoot.Find(CCS_PlayerPrefabConstants.PlayerVisualPrefabInstanceName);
            if (visualInstance == null)
            {
                for (int childIndex = 0; childIndex < visualRoot.childCount; childIndex++)
                {
                    Transform child = visualRoot.GetChild(childIndex);
                    if (child.name == CCS_PlayerPrefabConstants.PlayerVisualPrefabInstanceName)
                    {
                        visualInstance = child;
                        break;
                    }
                }
            }

            AppendIfMissing(
                failures,
                visualInstance != null,
                label
                + " must contain "
                + CCS_PlayerPrefabConstants.PlayerVisualPrefabInstanceName
                + " under VisualRoot.");
        }

        private static Animator ReadAnimatorReference(Component component)
        {
            SerializedObject serializedObject = new SerializedObject(component);
            SerializedProperty animatorProperty = serializedObject.FindProperty("animator");
            if (animatorProperty != null && animatorProperty.propertyType == SerializedPropertyType.ObjectReference)
            {
                return animatorProperty.objectReferenceValue as Animator;
            }

            SerializedProperty characterAnimatorProperty = serializedObject.FindProperty("characterAnimator");
            if (characterAnimatorProperty != null
                && characterAnimatorProperty.propertyType == SerializedPropertyType.ObjectReference)
            {
                return characterAnimatorProperty.objectReferenceValue as Animator;
            }

            return null;
        }

        private static bool IsUnderNamedAncestor(Transform transform, string ancestorName)
        {
            Transform current = transform.parent;
            while (current != null)
            {
                if (current.name == ancestorName)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private static string BuildTransformPath(Transform target, Transform root)
        {
            if (target == null)
            {
                return "<null>";
            }

            Stack<string> segments = new Stack<string>();
            Transform current = target;
            while (current != null)
            {
                segments.Push(current.name);
                if (current == root)
                {
                    break;
                }

                current = current.parent;
            }

            return string.Join("/", segments);
        }

        private static void AppendResult(List<string> failures, CCS_SurvivalValidationResult result)
        {
            if (result.IsSuccess)
            {
                return;
            }

            failures.Add(result.Message);
        }

        private static void AppendIfMissing(List<string> target, bool condition, string message)
        {
            if (!condition)
            {
                target.Add(message);
            }
        }

        #endregion
    }
}
