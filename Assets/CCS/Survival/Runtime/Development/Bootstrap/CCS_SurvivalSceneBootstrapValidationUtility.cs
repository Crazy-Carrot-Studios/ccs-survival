using CCS.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_SurvivalSceneBootstrapValidationUtility
// CATEGORY: Survival / Runtime / Development / Bootstrap
// PURPOSE: Development-layer scene bootstrap validation for profile service/object requirements.
// PLACEMENT: Invoked by editor menus and CCS_SurvivalSceneBootstrapper. No gameplay rules.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Complements foundation CCS.Survival scene bootstrap validation without replacing it.
// =============================================================================

namespace CCS.Survival.Development
{
    public static class CCS_SurvivalSceneBootstrapValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfileRequirements(CCS_SurvivalSceneBootstrapProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Pass("No development scene bootstrap profile to validate.");
            }

            CCS_SurvivalValidationResult profileValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!profileValidation.IsSuccess)
            {
                return profileValidation;
            }

            CCS_SurvivalValidationResult compositionValidation = ValidateCompositionRootRequirements(profile);
            if (!compositionValidation.IsSuccess)
            {
                return compositionValidation;
            }

            CCS_SurvivalValidationResult requiredObjectsValidation = ValidateSceneObjectRequirements(
                profile.RequiredSceneObjects,
                isRequired: true);
            if (!requiredObjectsValidation.IsSuccess)
            {
                return requiredObjectsValidation;
            }

            CCS_SurvivalValidationResult optionalObjectsValidation = ValidateSceneObjectRequirements(
                profile.OptionalSceneObjects,
                isRequired: false);
            if (optionalObjectsValidation.IsWarning)
            {
                return optionalObjectsValidation;
            }

            CCS_SurvivalValidationResult servicesValidation = ValidateServiceRequirements(profile);
            if (!servicesValidation.IsSuccess)
            {
                return servicesValidation;
            }

            if (compositionValidation.IsWarning || optionalObjectsValidation.IsWarning || servicesValidation.IsWarning)
            {
                return CCS_SurvivalValidationResult.Warn(
                    "Development scene bootstrap profile validated with warnings.");
            }

            return CCS_SurvivalValidationResult.Pass("Development scene bootstrap profile requirements validated.");
        }

        public static CCS_SurvivalValidationResult ValidateActiveSceneBootstrap(CCS_SurvivalSceneBootstrapProfile profile)
        {
            CCS_SurvivalValidationResult profileRequirements = ValidateProfileRequirements(profile);
            if (!profileRequirements.IsSuccess)
            {
                return profileRequirements;
            }

            CCS_SurvivalBootstrap survivalBootstrap = Object.FindAnyObjectByType<CCS_SurvivalBootstrap>();
            if (survivalBootstrap == null)
            {
                return CCS_SurvivalValidationResult.Fail("Active scene is missing CCS_SurvivalBootstrap.");
            }

            CCS_RuntimeHost runtimeHost = survivalBootstrap.GetComponent<CCS_RuntimeHost>();
            CCS_SurvivalValidationResult compositionValidation =
                CCS.Survival.CCS_SurvivalSceneBootstrapValidationUtility.ValidateCompositionRoot(
                    runtimeHost,
                    survivalBootstrap);
            if (!compositionValidation.IsSuccess)
            {
                return compositionValidation;
            }

            CCS_SurvivalValidationResult duplicateValidation =
                CCS.Survival.CCS_SurvivalSceneBootstrapValidationUtility.ValidateNoDuplicateSceneBootstrapComponents();
            if (!duplicateValidation.IsSuccess)
            {
                return duplicateValidation;
            }

            CCS_SurvivalRuntimeContext survivalContext = survivalBootstrap.SurvivalContext;
            if (survivalContext == null)
            {
                if (compositionValidation.IsWarning || duplicateValidation.IsWarning || profileRequirements.IsWarning)
                {
                    return CCS_SurvivalValidationResult.Warn(
                        "Active scene bootstrap composition validated with warnings (runtime context pending Play Mode).");
                }

                return CCS_SurvivalValidationResult.Pass(
                    "Active scene bootstrap composition validated (runtime context pending Play Mode).");
            }

            CCS_SurvivalValidationResult foundationValidation =
                CCS.Survival.CCS_SurvivalSceneBootstrapValidationUtility.ValidateSceneBootstrap(
                    survivalContext,
                    survivalBootstrap);

            if (!foundationValidation.IsSuccess)
            {
                return foundationValidation;
            }

            if (foundationValidation.IsWarning || profileRequirements.IsWarning)
            {
                return CCS_SurvivalValidationResult.Warn(
                    "Active scene development bootstrap validation completed with warnings.");
            }

            return CCS_SurvivalValidationResult.Pass("Active scene development bootstrap validation passed.");
        }

        #endregion

        #region Private Methods

        private static CCS_SurvivalValidationResult ValidateCompositionRootRequirements(
            CCS_SurvivalSceneBootstrapProfile profile)
        {
            if (profile.RequireRuntimeHost)
            {
                CCS_RuntimeHost[] runtimeHosts = Object.FindObjectsByType<CCS_RuntimeHost>();

                if (runtimeHosts.Length != CCS_SurvivalRuntimeConstants.ExpectedRuntimeHostCount)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Expected {CCS_SurvivalRuntimeConstants.ExpectedRuntimeHostCount} runtime host, found {runtimeHosts.Length}.");
                }
            }

            if (profile.RequireSurvivalBootstrap)
            {
                CCS_SurvivalBootstrap[] survivalBootstraps = Object.FindObjectsByType<CCS_SurvivalBootstrap>();

                if (survivalBootstraps.Length != CCS_SurvivalRuntimeConstants.ExpectedSurvivalBootstrapCount)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Expected {CCS_SurvivalRuntimeConstants.ExpectedSurvivalBootstrapCount} survival bootstrap, found {survivalBootstraps.Length}.");
                }
            }

            return CCS_SurvivalValidationResult.Pass();
        }

        private static CCS_SurvivalValidationResult ValidateSceneObjectRequirements(
            System.Collections.Generic.IReadOnlyList<CCS_SurvivalSceneBootstrapRequirementEntry> entries,
            bool isRequired)
        {
            if (entries == null || entries.Count == 0)
            {
                return CCS_SurvivalValidationResult.Pass();
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                return CCS_SurvivalValidationResult.Warn("Active scene is not valid for scene object validation.");
            }

            GameObject[] rootObjects = activeScene.GetRootGameObjects();

            for (int index = 0; index < entries.Count; index++)
            {
                CCS_SurvivalSceneBootstrapRequirementEntry entry = entries[index];
                if (!entry.HasSceneObjectName)
                {
                    continue;
                }

                if (!TryFindSceneObjectByName(rootObjects, entry.SceneObjectName, out _))
                {
                    string label = string.IsNullOrWhiteSpace(entry.DisplayName)
                        ? entry.SceneObjectName
                        : entry.DisplayName;

                    if (isRequired)
                    {
                        return CCS_SurvivalValidationResult.Fail(
                            $"Required scene object missing: {label} (name: {entry.SceneObjectName}).");
                    }

                    return CCS_SurvivalValidationResult.Warn(
                        $"Optional scene object missing: {label} (name: {entry.SceneObjectName}).");
                }
            }

            return CCS_SurvivalValidationResult.Pass();
        }

        private static CCS_SurvivalValidationResult ValidateServiceRequirements(CCS_SurvivalSceneBootstrapProfile profile)
        {
            if (profile.RequiredServices == null || profile.RequiredServices.Count == 0)
            {
                return CCS_SurvivalValidationResult.Pass("No required services listed on bootstrap profile.");
            }

            CCS_RuntimeHost runtimeHost = Object.FindAnyObjectByType<CCS_RuntimeHost>();
            if (runtimeHost == null || !runtimeHost.IsRuntimeInitialized)
            {
                return CCS_SurvivalValidationResult.Warn(
                    "Required service list is populated but runtime host is not initialized. Validate in Play Mode.");
            }

            for (int index = 0; index < profile.RequiredServices.Count; index++)
            {
                CCS_SurvivalSceneBootstrapServiceRequirement requirement = profile.RequiredServices[index];
                if (!requirement.HasServiceContractName)
                {
                    continue;
                }

                ReportPlaceholderServiceRequirement(requirement);
            }

            return CCS_SurvivalValidationResult.Pass(
                "Required service contract names recorded. Type-specific registry checks arrive with gameplay modules.");
        }

        private static void ReportPlaceholderServiceRequirement(CCS_SurvivalSceneBootstrapServiceRequirement requirement)
        {
            CCS_Logger.Log(
                CCS_SurvivalRuntimeConstants.DevelopmentDiagnosticsLogCategory,
                $"Bootstrap requires service contract: {requirement.ServiceContractName} (id: {requirement.RequirementId}).");
        }

        private static bool TryFindSceneObjectByName(
            GameObject[] rootObjects,
            string sceneObjectName,
            out GameObject foundObject)
        {
            foundObject = null;

            if (rootObjects == null || string.IsNullOrWhiteSpace(sceneObjectName))
            {
                return false;
            }

            for (int rootIndex = 0; rootIndex < rootObjects.Length; rootIndex++)
            {
                GameObject root = rootObjects[rootIndex];
                if (root == null)
                {
                    continue;
                }

                if (string.Equals(root.name, sceneObjectName, System.StringComparison.Ordinal))
                {
                    foundObject = root;
                    return true;
                }

                Transform[] children = root.GetComponentsInChildren<Transform>(true);
                for (int childIndex = 0; childIndex < children.Length; childIndex++)
                {
                    Transform child = children[childIndex];
                    if (child != null && string.Equals(child.name, sceneObjectName, System.StringComparison.Ordinal))
                    {
                        foundObject = child.gameObject;
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion
    }
}
