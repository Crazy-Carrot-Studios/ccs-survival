using System.Collections.Generic;
using System.IO;
using CCS.Modules.Attributes.Tests;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AttributesModuleValidator
// CATEGORY: Modules / Attributes / Editor
// PURPOSE: Validates Attributes module foundation, assets, and test player wiring.
// PLACEMENT: Editor validator invoked from CCS/Attributes/Validate Attributes Module.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.3.0 validates Health-only foundation and server-authoritative path presence.
// =============================================================================

namespace CCS.Modules.Attributes.Editor
{
    public static class CCS_AttributesModuleValidator
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateAttributesModule()
        {
            List<string> failures = new List<string>();

            CCS_SurvivalValidationResult foundationResult = CCS_AttributesValidationUtility.ValidateModuleFoundation();
            AppendResult(failures, foundationResult);

            CCS_AttributeDefinition healthDefinition =
                AssetDatabase.LoadAssetAtPath<CCS_AttributeDefinition>(CCS_AttributesConstants.HealthDefinitionPath);
            AppendIfMissing(
                failures,
                File.Exists(CCS_AttributesConstants.HealthDefinitionPath),
                $"Missing health definition asset at {CCS_AttributesConstants.HealthDefinitionPath}.");
            AppendResult(failures, CCS_AttributesValidationUtility.ValidateHealthDefinition(healthDefinition));

            GameObject testPlayerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_AttributesTestConstants.NetworkedTestPlayerPrefabPath);
            AppendIfMissing(
                failures,
                testPlayerPrefab != null,
                $"Missing networked test player prefab at {CCS_AttributesTestConstants.NetworkedTestPlayerPrefabPath}.");
            AppendResult(
                failures,
                CCS_AttributesValidationUtility.ValidateTestPlayerComponents(testPlayerPrefab));

            ValidateSourceContracts(failures);

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass(
                    "Attributes module foundation, Health definition, and test player wiring are valid.");
        }

        #endregion

        #region Private Methods

        private static void ValidateSourceContracts(List<string> failures)
        {
            string replicatorPath = CCS_AttributesConstants.ModuleRootPath
                + "/Runtime/Components/CCS_NetworkAttributeReplicator.cs";
            if (File.Exists(replicatorPath))
            {
                string source = File.ReadAllText(replicatorPath);
                AppendIfMissing(
                    failures,
                    source.Contains("RequestSelfDamageServerRpc"),
                    "CCS_NetworkAttributeReplicator must expose a server-authoritative self-damage ServerRpc.");
                AppendIfMissing(
                    failures,
                    source.Contains("NetworkVariableWritePermission.Server"),
                    "CCS_NetworkAttributeReplicator must use server write permission for replicated health.");
            }

            string debugInputPath = CCS_AttributesConstants.ModuleRootPath
                + "/Tests/Runtime/CCS_TestPlayerAttributeDebugInput.cs";
            if (File.Exists(debugInputPath))
            {
                string source = File.ReadAllText(debugInputPath);
                AppendIfMissing(
                    failures,
                    source.Contains("RequestSelfDamage"),
                    "CCS_TestPlayerAttributeDebugInput must route damage through CCS_NetworkAttributeReplicator.");
            }

            string barsHudPath = CCS_AttributesConstants.ModuleRootPath + "/Runtime/UI/CCS_PlayerAttributeBarsHud.cs";
            if (File.Exists(barsHudPath))
            {
                string source = File.ReadAllText(barsHudPath);
                AppendIfMissing(
                    failures,
                    source.Contains("IsLocalOwner"),
                    "CCS_PlayerAttributeBarsHud must gate visibility to the local owner.");
                AppendIfMissing(
                    failures,
                    !source.Contains("Input.GetKey"),
                    "CCS_PlayerAttributeBarsHud must not use legacy UnityEngine.Input polling.");
                AppendIfMissing(
                    failures,
                    !source.Contains("Input.GetAxisRaw"),
                    "CCS_PlayerAttributeBarsHud must not use legacy UnityEngine.Input polling.");
            }

            string barsHudStylePath = CCS_AttributesConstants.ModuleRootPath + "/Runtime/CCS_AttributeBarsHudStyle.cs";
            if (File.Exists(barsHudStylePath))
            {
                string styleSource = File.ReadAllText(barsHudStylePath);
                AppendIfMissing(
                    failures,
                    styleSource.Contains($"PanelHeight = {CCS_AttributeBarsHudStyle.PanelHeight:0}f"),
                    $"Attribute bar HUD panel height must be {CCS_AttributeBarsHudStyle.PanelHeight:0}.");
            }

            string legacyHudPath = CCS_AttributesConstants.ModuleRootPath + "/Runtime/UI/CCS_PlayerAttributeHud.cs";
            AppendIfMissing(
                failures,
                !File.Exists(legacyHudPath),
                "Legacy CCS_PlayerAttributeHud must be removed when attribute bar HUD is active.");
        }

        private static void AppendResult(List<string> failures, CCS_SurvivalValidationResult result)
        {
            if (!result.IsSuccess)
            {
                failures.Add(result.Message);
            }
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
