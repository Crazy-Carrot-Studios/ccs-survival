using System.Collections.Generic;
using System.IO;
using CCS.Project;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AttributesValidationUtility
// CATEGORY: Modules / Attributes / Runtime / Validation
// PURPOSE: Runtime validation helpers for the Attributes module foundation.
// PLACEMENT: Called from editor validators and future module installers.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.3.0 validates Health definition and generic container wiring.
// =============================================================================

namespace CCS.Modules.Attributes
{
    public static class CCS_AttributesValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateModuleFoundation()
        {
            List<string> failures = new List<string>();

            AppendIfMissing(
                failures,
                Directory.Exists(CCS_AttributesConstants.ModuleRootPath + "/Runtime"),
                "Missing Attributes Runtime folder.");
            AppendIfMissing(
                failures,
                Directory.Exists(CCS_AttributesConstants.ModuleRootPath + "/Editor"),
                "Missing Attributes Editor folder.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_AttributesConstants.ModuleRootPath + "/Runtime/CCS.Modules.Attributes.Runtime.asmdef"),
                "Missing CCS.Modules.Attributes.Runtime.asmdef.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_AttributesConstants.ModuleRootPath + "/Editor/CCS.Modules.Attributes.Editor.asmdef"),
                "Missing CCS.Modules.Attributes.Editor.asmdef.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_AttributesConstants.ModuleRootPath + "/Tests/Runtime/CCS.Modules.Attributes.Tests.Runtime.asmdef"),
                "Missing CCS.Modules.Attributes.Tests.Runtime.asmdef.");

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Attributes module foundation folders and asmdefs are present.");
        }

        public static CCS_SurvivalValidationResult ValidateHealthDefinition(CCS_AttributeDefinition healthDefinition)
        {
            List<string> failures = new List<string>();
            AppendIfMissing(failures, healthDefinition != null, "Health definition asset is missing.");

            if (healthDefinition == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            CCS_SurvivalValidationResult profileValidation =
                CCS_SurvivalProfileValidationUtility.ValidateProfile(healthDefinition);
            AppendIfMissing(
                failures,
                profileValidation.IsSuccess,
                profileValidation.Message);

            AppendIfMissing(
                failures,
                healthDefinition.ProfileId == CCS_AttributesConstants.HealthAttributeId,
                $"Health definition profileId must be {CCS_AttributesConstants.HealthAttributeId}.");
            AppendIfMissing(
                failures,
                Mathf.Approximately(healthDefinition.DefaultValue, 100f),
                "Health defaultValue must be 100.");
            AppendIfMissing(
                failures,
                Mathf.Approximately(healthDefinition.MaxValue, 100f),
                "Health maxValue must be 100.");
            AppendIfMissing(
                failures,
                Mathf.Approximately(healthDefinition.MinValue, 0f),
                "Health minValue must be 0.");

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Health attribute definition is valid.");
        }

        public static CCS_SurvivalValidationResult ValidateStaminaDefinition(CCS_AttributeDefinition staminaDefinition)
        {
            List<string> failures = new List<string>();
            AppendIfMissing(failures, staminaDefinition != null, "Stamina definition asset is missing.");

            if (staminaDefinition == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            CCS_SurvivalValidationResult profileValidation =
                CCS_SurvivalProfileValidationUtility.ValidateProfile(staminaDefinition);
            AppendIfMissing(
                failures,
                profileValidation.IsSuccess,
                profileValidation.Message);

            AppendIfMissing(
                failures,
                staminaDefinition.ProfileId == CCS_AttributesConstants.StaminaAttributeId,
                $"Stamina definition profileId must be {CCS_AttributesConstants.StaminaAttributeId}.");
            AppendIfMissing(
                failures,
                Mathf.Approximately(staminaDefinition.DefaultValue, CCS_AttributesConstants.StaminaDefaultMax),
                $"Stamina defaultValue must be {CCS_AttributesConstants.StaminaDefaultMax:0}.");
            AppendIfMissing(
                failures,
                Mathf.Approximately(staminaDefinition.MaxValue, CCS_AttributesConstants.StaminaDefaultMax),
                $"Stamina maxValue must be {CCS_AttributesConstants.StaminaDefaultMax:0}.");
            AppendIfMissing(
                failures,
                Mathf.Approximately(staminaDefinition.MinValue, 0f),
                "Stamina minValue must be 0.");

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Stamina attribute definition is valid.");
        }

        public static CCS_SurvivalValidationResult ValidateTestPlayerComponents(GameObject prefabRoot)
        {
            List<string> failures = new List<string>();
            AppendIfMissing(failures, prefabRoot != null, "Test player prefab is missing.");

            if (prefabRoot == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            AppendIfMissing(
                failures,
                prefabRoot.GetComponent<CCS_AttributeContainer>() != null,
                "Test player prefab must contain CCS_AttributeContainer.");
            AppendIfMissing(
                failures,
                prefabRoot.GetComponent<CCS_NetworkAttributeReplicator>() != null,
                "Test player prefab must contain CCS_NetworkAttributeReplicator.");
            AppendIfMissing(
                failures,
                prefabRoot.GetComponentInChildren<CCS_PlayerAttributeBarsHud>(true) != null,
                "Test player prefab must contain CCS_PlayerAttributeBarsHud.");
            AppendIfMissing(
                failures,
                !HasLegacyAttributeHudComponent(prefabRoot),
                "Test player prefab must not contain legacy CCS_PlayerAttributeHud.");
            AppendIfMissing(
                failures,
                !HasComponentNamed(prefabRoot, "CCS_CharacterControllerDebugHud"),
                "Test player prefab must not contain CCS_CharacterControllerDebugHud.");
            AppendIfMissing(
                failures,
                FindDirectChildByName(prefabRoot.transform, CCS_AttributesConstants.LegacyDebugHudTextObjectName) == null,
                "Test player prefab must not contain legacy HealthHudText debug HUD.");
            AppendIfMissing(
                failures,
                CountAttributeBarViews(prefabRoot) >= 4,
                "Test player prefab must contain Health, Stamina, Hunger, and Thirst attribute bars.");
            ValidateAttributeBarsPanelLayout(prefabRoot, failures);
            AppendIfMissing(
                failures,
                prefabRoot.GetComponent<CCS_StaminaController>() != null,
                "Test player prefab must contain CCS_StaminaController.");
            AppendIfMissing(
                failures,
                prefabRoot.GetComponent<CCS_HealthRegenController>() != null,
                "Test player prefab must contain CCS_HealthRegenController.");
            AppendIfMissing(
                failures,
                HasDebugInputComponent(prefabRoot),
                "Test player prefab must contain CCS_TestPlayerAttributeDebugInput.");

            CCS_AttributeContainer container = prefabRoot.GetComponent<CCS_AttributeContainer>();
            if (container != null && container.AttributeDefinitions != null)
            {
                bool hasHealth = false;
                bool hasStamina = false;
                for (int i = 0; i < container.AttributeDefinitions.Count; i++)
                {
                    CCS_AttributeDefinition definition = container.AttributeDefinitions[i];
                    if (definition == null)
                    {
                        continue;
                    }

                    if (definition.ProfileId == CCS_AttributesConstants.HealthAttributeId)
                    {
                        hasHealth = true;
                    }

                    if (definition.ProfileId == CCS_AttributesConstants.StaminaAttributeId)
                    {
                        hasStamina = true;
                    }
                }

                AppendIfMissing(failures, hasHealth, "Test player AttributeContainer must include Health definition.");
                AppendIfMissing(failures, hasStamina, "Test player AttributeContainer must include Stamina definition.");
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Test player attribute components are wired.");
        }

        #endregion

        #region Private Methods

        private static void ValidateAttributeBarsPanelLayout(GameObject prefabRoot, List<string> failures)
        {
            if (prefabRoot == null)
            {
                return;
            }

            Transform panelTransform = FindChildRecursive(
                prefabRoot.transform,
                CCS_AttributesConstants.AttributeBarsPanelObjectName);
            AppendIfMissing(
                failures,
                panelTransform != null,
                "Test player prefab must contain AttributeBarsPanel.");

            if (panelTransform == null)
            {
                return;
            }

            RectTransform panelRect = panelTransform.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                AppendIfMissing(
                    failures,
                    Mathf.Approximately(panelRect.sizeDelta.y, CCS_AttributeBarsHudStyle.PanelHeight),
                    $"AttributeBarsPanel height must be {CCS_AttributeBarsHudStyle.PanelHeight:0}.");
            }

            Transform thirstBar = FindChildRecursive(panelTransform, CCS_AttributesConstants.ThirstBarObjectName);
            AppendIfMissing(
                failures,
                thirstBar != null,
                "AttributeBarsPanel must contain ThirstBar.");

            if (thirstBar == null || panelRect == null)
            {
                return;
            }

            RectTransform thirstRect = thirstBar.GetComponent<RectTransform>();
            if (thirstRect == null)
            {
                return;
            }

            float thirstBottom = Mathf.Abs(thirstRect.anchoredPosition.y) + thirstRect.sizeDelta.y;
            float panelContentHeight = panelRect.sizeDelta.y - CCS_AttributeBarsHudStyle.PanelPaddingBottom;
            AppendIfMissing(
                failures,
                thirstBottom <= panelContentHeight + 0.5f,
                "ThirstBar must be fully contained within AttributeBarsPanel background.");
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

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                Transform match = FindChildRecursive(child, childName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static bool HasComponentNamed(GameObject prefabRoot, string typeName)
        {
            MonoBehaviour[] behaviours = prefabRoot.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null && behaviour.GetType().Name == typeName)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasLegacyAttributeHudComponent(GameObject prefabRoot)
        {
            MonoBehaviour[] behaviours = prefabRoot.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null && behaviour.GetType().Name == "CCS_PlayerAttributeHud")
                {
                    return true;
                }
            }

            return false;
        }

        private static int CountAttributeBarViews(GameObject prefabRoot)
        {
            CCS_AttributeBarView[] barViews = prefabRoot.GetComponentsInChildren<CCS_AttributeBarView>(true);
            return barViews != null ? barViews.Length : 0;
        }

        private static Transform FindDirectChildByName(Transform parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child != null && child.name == childName)
                {
                    return child;
                }
            }

            return null;
        }

        private static bool HasDebugInputComponent(GameObject prefabRoot)
        {
            Component[] components = prefabRoot.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component != null && component.GetType().Name == "CCS_TestPlayerAttributeDebugInput")
                {
                    return true;
                }
            }

            return false;
        }

        private static void AppendIfMissing(List<string> failures, bool condition, string message)
        {
            if (!condition)
            {
                failures.Add(message);
            }
        }

        #endregion
    }
}
