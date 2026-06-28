using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController.Tests.Netcode;
using CCS.Modules.CharacterController.Tests;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerPrefabArchitectureValidationUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Validates production and test player prefab architecture for v0.8.0.
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Strict for production prefab; permissive for test harness prefabs.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_PlayerPrefabArchitectureValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProductionPlayerPrefabArchitecture()
        {
            return ValidatePlayerPrefabArchitecture(
                CCS_PlayerPrefabConstants.ProductionPlayerPrefabPath,
                strictProductionRules: true,
                label: "Production player prefab");
        }

        public static CCS_SurvivalValidationResult ValidateTestHarnessPlayerPrefabArchitecture()
        {
            CCS_SurvivalValidationResult copyResult = ValidatePlayerPrefabArchitecture(
                CCS_PlayerPrefabConstants.TestHarnessPlayerPrefabPath,
                strictProductionRules: false,
                label: "Test harness player prefab copy");

            if (!copyResult.IsSuccess)
            {
                return copyResult;
            }

            return ValidatePlayerPrefabArchitecture(
                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath,
                strictProductionRules: false,
                label: "Legacy Master Test player prefab");
        }

        public static CCS_SurvivalValidationResult ValidateAllPlayerPrefabArchitecture()
        {
            List<string> failures = new List<string>();
            AppendResult(failures, ValidateProductionPlayerPrefabArchitecture());
            AppendResult(failures, ValidateTestHarnessPlayerPrefabArchitecture());

            AppendIfMissing(
                failures,
                File.Exists(CCS_PlayerPrefabConstants.ProductionPlayerPrefabPath),
                "Missing production player prefab: " + CCS_PlayerPrefabConstants.ProductionPlayerPrefabPath);

            AppendIfMissing(
                failures,
                File.Exists(CCS_PlayerPrefabConstants.TestHarnessPlayerPrefabPath),
                "Missing test harness player prefab: " + CCS_PlayerPrefabConstants.TestHarnessPlayerPrefabPath);

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Player prefab architecture validation passed.");
        }

        #endregion

        #region Private Methods

        private static CCS_SurvivalValidationResult ValidatePlayerPrefabArchitecture(
            string prefabPath,
            bool strictProductionRules,
            string label)
        {
            List<string> failures = new List<string>();
            List<string> warnings = new List<string>();

            AppendIfMissing(
                failures,
                File.Exists(prefabPath),
                label + " missing at " + prefabPath + ".");

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            AppendIfMissing(failures, prefabAsset != null, label + " could not be loaded.");

            if (prefabAsset == null)
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
                ValidateHierarchy(failures, warnings, prefabRoot, strictProductionRules, label);
                ValidateRootComponentBudget(failures, warnings, prefabRoot, strictProductionRules, label);
                ValidateComponentClassification(failures, warnings, prefabRoot, strictProductionRules, label);
                ValidateRequiredSystems(failures, prefabRoot, strictProductionRules, label);
                ValidateFacadeReferences(failures, prefabRoot, label);
                ValidateNetworkComponentsOnRoot(failures, prefabRoot, label);
                ValidateLocalUiOwnerGate(failures, warnings, prefabRoot, strictProductionRules, label);
                ValidateAnimatorLayerIsolation(failures, label);
                ValidateMissingScripts(failures, prefabRoot, label);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            if (warnings.Count > 0)
            {
                return CCS_SurvivalValidationResult.Warn(label + ": " + string.Join(" ", warnings));
            }

            return CCS_SurvivalValidationResult.Pass(label + " architecture validated.");
        }

        private static void ValidateHierarchy(
            List<string> failures,
            List<string> warnings,
            GameObject prefabRoot,
            bool strictProductionRules,
            string label)
        {
            Transform runtimeSystems = prefabRoot.transform.Find(CCS_PlayerPrefabConstants.RuntimeSystemsObjectName);
            Transform presentation = prefabRoot.transform.Find(CCS_PlayerPrefabConstants.PresentationObjectName);
            Transform localUi = prefabRoot.transform.Find(CCS_PlayerPrefabConstants.PlayerLocalUiObjectName);

            AppendIfMissing(
                failures,
                runtimeSystems != null,
                label + " must contain " + CCS_PlayerPrefabConstants.RuntimeSystemsObjectName + ".");

            AppendIfMissing(
                failures,
                presentation != null,
                label + " must contain " + CCS_PlayerPrefabConstants.PresentationObjectName + ".");

            AppendIfMissing(
                failures,
                localUi != null,
                label + " must contain transitional " + CCS_PlayerPrefabConstants.PlayerLocalUiObjectName + ".");

            Transform visualRoot = presentation != null
                ? presentation.Find("VisualRoot")
                : prefabRoot.transform.Find("VisualRoot");

            AppendIfMissing(
                failures,
                visualRoot != null,
                label + " must contain VisualRoot under Presentation.");

            if (strictProductionRules
                && prefabRoot.name != CCS_PlayerPrefabConstants.ProductionPlayerInstanceName)
            {
                failures.Add(
                    label + " root name must be "
                    + CCS_PlayerPrefabConstants.ProductionPlayerInstanceName
                    + ".");
            }
        }

        private static void ValidateRootComponentBudget(
            List<string> failures,
            List<string> warnings,
            GameObject prefabRoot,
            bool strictProductionRules,
            string label)
        {
            Component[] rootComponents = prefabRoot.GetComponents<Component>();
            int customBehaviourCount = 0;
            for (int componentIndex = 0; componentIndex < rootComponents.Length; componentIndex++)
            {
                Component component = rootComponents[componentIndex];
                if (component is Transform || component is UnityEngine.CharacterController)
                {
                    continue;
                }

                customBehaviourCount++;
            }

            int totalRootComponents = rootComponents.Length;
            if (strictProductionRules
                && totalRootComponents > CCS_PlayerPrefabConstants.ProductionRootComponentTransitionalTarget)
            {
                failures.Add(
                    label + " root has "
                    + totalRootComponents
                    + " components (transitional max "
                    + CCS_PlayerPrefabConstants.ProductionRootComponentTransitionalTarget
                    + ").");
            }
            else if (strictProductionRules
                && totalRootComponents > CCS_PlayerPrefabConstants.ProductionRootComponentHardTarget)
            {
                warnings.Add(
                    label + " root has "
                    + totalRootComponents
                    + " components (hard target "
                    + CCS_PlayerPrefabConstants.ProductionRootComponentHardTarget
                    + ").");
            }

            if (strictProductionRules && customBehaviourCount > 6)
            {
                warnings.Add(label + " root custom MonoBehaviour count is " + customBehaviourCount + ".");
            }
        }

        private static void ValidateComponentClassification(
            List<string> failures,
            List<string> warnings,
            GameObject prefabRoot,
            bool strictProductionRules,
            string label)
        {
            MonoBehaviour[] behaviours = prefabRoot.GetComponentsInChildren<MonoBehaviour>(true);
            for (int behaviourIndex = 0; behaviourIndex < behaviours.Length; behaviourIndex++)
            {
                MonoBehaviour behaviour = behaviours[behaviourIndex];
                if (behaviour == null)
                {
                    failures.Add(label + " contains a missing script reference.");
                    continue;
                }

                if (!CCS_PlayerPrefabComponentClassification.TryClassify(behaviour, out CCS_PlayerPrefabComponentRule rule))
                {
                    if (strictProductionRules)
                    {
                        warnings.Add(
                            label + " has unclassified component "
                            + behaviour.GetType().Name
                            + " on "
                            + behaviour.gameObject.name
                            + ".");
                    }

                    continue;
                }

                if (strictProductionRules && rule.Category == CCS_PlayerPrefabComponentCategory.TestOnly)
                {
                    failures.Add(
                        label + " must not contain TestOnly component "
                        + behaviour.GetType().Name
                        + " on "
                        + behaviour.gameObject.name
                        + ".");
                }

                if (strictProductionRules && rule.Category == CCS_PlayerPrefabComponentCategory.Deprecated)
                {
                    warnings.Add(
                        label + " contains Deprecated component "
                        + behaviour.GetType().Name
                        + " (rename/migrate in a later pass).");
                }

                CCS_PlayerPrefabArchitectureLayer actualLayer =
                    CCS_PlayerPrefabComponentClassification.ResolveLayer(behaviour.transform, prefabRoot.transform);

                if (strictProductionRules
                    && rule.PreferredLayer != CCS_PlayerPrefabArchitectureLayer.Any
                    && actualLayer != CCS_PlayerPrefabArchitectureLayer.Root
                    && actualLayer != rule.PreferredLayer
                    && !(rule.PreferredLayer == CCS_PlayerPrefabArchitectureLayer.Presentation
                        && actualLayer == CCS_PlayerPrefabArchitectureLayer.Presentation))
                {
                    if (rule.Category != CCS_PlayerPrefabComponentCategory.ProductionRequired
                        || behaviour.transform != prefabRoot.transform)
                    {
                        warnings.Add(
                            label + " component "
                            + behaviour.GetType().Name
                            + " is on layer "
                            + actualLayer
                            + " but prefers "
                            + rule.PreferredLayer
                            + ".");
                    }
                }
            }
        }

        private static void ValidateRequiredSystems(
            List<string> failures,
            GameObject prefabRoot,
            bool strictProductionRules,
            string label)
        {
            if (!strictProductionRules)
            {
                return;
            }

            for (int ruleIndex = 0; ruleIndex < CCS_PlayerPrefabComponentClassification.AllRules.Count; ruleIndex++)
            {
                CCS_PlayerPrefabComponentRule rule = CCS_PlayerPrefabComponentClassification.AllRules[ruleIndex];
                if (!rule.RequiredOnProduction)
                {
                    continue;
                }

                Component component = prefabRoot.GetComponentInChildren(rule.ComponentType, true);
                AppendIfMissing(
                    failures,
                    component != null,
                    label + " must contain required component " + rule.ComponentType.Name + ".");
            }
        }

        private static void ValidateFacadeReferences(List<string> failures, GameObject prefabRoot, string label)
        {
            CCS_PlayerRuntimeFacade facade = prefabRoot.GetComponent<CCS_PlayerRuntimeFacade>();
            AppendIfMissing(
                failures,
                facade != null,
                label + " root must contain CCS_PlayerRuntimeFacade.");

            if (facade != null && !facade.HasRequiredProductionReferences())
            {
                failures.Add(label + " CCS_PlayerRuntimeFacade is missing required references.");
            }
        }

        private static void ValidateNetworkComponentsOnRoot(List<string> failures, GameObject prefabRoot, string label)
        {
            AppendIfMissing(
                failures,
                prefabRoot.GetComponent<Unity.Netcode.NetworkObject>() != null,
                label + " NetworkObject must be on root.");

            AppendIfMissing(
                failures,
                prefabRoot.GetComponent<CCS_ClientOwnerNetworkTransform>() != null,
                label + " CCS_ClientOwnerNetworkTransform must be on root.");

            Transform runtimeSystems = prefabRoot.transform.Find(CCS_PlayerPrefabConstants.RuntimeSystemsObjectName);
            if (runtimeSystems == null)
            {
                return;
            }

            if (runtimeSystems.GetComponent<Unity.Netcode.NetworkObject>() != null)
            {
                failures.Add(label + " must not contain nested NetworkObject under RuntimeSystems.");
            }
        }

        private static void ValidateLocalUiOwnerGate(
            List<string> failures,
            List<string> warnings,
            GameObject prefabRoot,
            bool strictProductionRules,
            string label)
        {
            Transform localUi = prefabRoot.transform.Find(CCS_PlayerPrefabConstants.PlayerLocalUiObjectName);
            if (localUi == null)
            {
                return;
            }

            CCS_PlayerLocalOwnerUiBootstrap bootstrap = localUi.GetComponent<CCS_PlayerLocalOwnerUiBootstrap>();
            if (bootstrap == null && strictProductionRules)
            {
                failures.Add(
                    label + " "
                    + CCS_PlayerPrefabConstants.PlayerLocalUiObjectName
                    + " must contain CCS_PlayerLocalOwnerUiBootstrap.");
            }
            else if (bootstrap == null)
            {
                warnings.Add(label + " PlayerLocalUI missing owner bootstrap.");
            }
        }

        private static void ValidateAnimatorLayerIsolation(List<string> failures, string label)
        {
            CCS_SurvivalValidationResult revolverResult =
                CCS_CharacterControllerAnimationValidationUtility.ValidateRevolverUpperBodyAnimationIsolation();
            if (!revolverResult.IsSuccess)
            {
                failures.Add(label + " revolver upper-body validation failed: " + revolverResult.Message);
            }

            CCS_SurvivalValidationResult interactionResult =
                CCS_CharacterControllerAnimationValidationUtility.ValidateInteractionLayerAnimationIsolation();
            if (!interactionResult.IsSuccess)
            {
                failures.Add(label + " interaction layer validation failed: " + interactionResult.Message);
            }

            CCS_SurvivalValidationResult motionResult =
                CCS_CharacterControllerAnimationValidationUtility.ValidateAnimatorMotionPlaybackReadiness();
            if (!motionResult.IsSuccess)
            {
                failures.Add(label + " animator motion readiness failed: " + motionResult.Message);
            }
        }

        private static void ValidateMissingScripts(List<string> failures, GameObject prefabRoot, string label)
        {
            Component[] components = prefabRoot.GetComponentsInChildren<Component>(true);
            for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
            {
                if (components[componentIndex] == null)
                {
                    failures.Add(label + " contains missing script references.");
                    return;
                }
            }
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
