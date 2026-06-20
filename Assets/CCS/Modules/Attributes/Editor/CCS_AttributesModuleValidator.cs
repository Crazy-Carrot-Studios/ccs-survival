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

            CCS_AttributeDefinition staminaDefinition =
                AssetDatabase.LoadAssetAtPath<CCS_AttributeDefinition>(CCS_AttributesConstants.StaminaDefinitionPath);
            AppendIfMissing(
                failures,
                File.Exists(CCS_AttributesConstants.StaminaDefinitionPath),
                $"Missing stamina definition asset at {CCS_AttributesConstants.StaminaDefinitionPath}.");
            AppendResult(failures, CCS_AttributesValidationUtility.ValidateStaminaDefinition(staminaDefinition));

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
                AppendIfMissing(
                    failures,
                    source.Contains("ApplyAuthorityHealthValue"),
                    "CCS_NetworkAttributeReplicator must expose ApplyAuthorityHealthValue for health regen.");
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
                AppendIfMissing(
                    failures,
                    source.Contains("CCS_StaminaController"),
                    "CCS_PlayerAttributeBarsHud must read stamina status from CCS_StaminaController.");
                AppendIfMissing(
                    failures,
                    source.Contains("CCS_HealthRegenController"),
                    "CCS_PlayerAttributeBarsHud must read health status from CCS_HealthRegenController.");
                AppendIfMissing(
                    failures,
                    !source.Contains("walkRecoveryThreshold") && !source.Contains("sprintUnlockThreshold"),
                    "CCS_PlayerAttributeBarsHud must not embed exhaustion threshold logic.");
                AppendIfMissing(
                    failures,
                    !source.Contains("regenDelaySeconds") && !source.Contains("regenPerSecond"),
                    "CCS_PlayerAttributeBarsHud must not embed health regen logic.");
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

            string staminaControllerPath = CCS_AttributesConstants.ModuleRootPath
                + "/Runtime/Components/CCS_StaminaController.cs";
            if (File.Exists(staminaControllerPath))
            {
                string staminaSource = File.ReadAllText(staminaControllerPath);
                AppendIfMissing(
                    failures,
                    staminaSource.Contains("CanSprint"),
                    "CCS_StaminaController must expose CanSprint for movement gating.");
                AppendIfMissing(
                    failures,
                    staminaSource.Contains("IsSprintLocked"),
                    "CCS_StaminaController must expose IsSprintLocked.");
                AppendIfMissing(
                    failures,
                    staminaSource.Contains("IsExhausted"),
                    "CCS_StaminaController must expose IsExhausted.");
                AppendIfMissing(
                    failures,
                    staminaSource.Contains("MovementSpeedMultiplier"),
                    "CCS_StaminaController must expose MovementSpeedMultiplier.");
                AppendIfMissing(
                    failures,
                    staminaSource.Contains("ReportMovementState"),
                    "CCS_StaminaController must accept sprint intent from CharacterController.");
                AppendIfMissing(
                    failures,
                    Mathf.Approximately(CCS_AttributesConstants.StaminaRegenPerSecond, 6f),
                    "Stamina regen rate must be reduced to 6 per second.");
            }

            string healthRegenControllerPath = CCS_AttributesConstants.ModuleRootPath
                + "/Runtime/Components/CCS_HealthRegenController.cs";
            if (File.Exists(healthRegenControllerPath))
            {
                string healthRegenSource = File.ReadAllText(healthRegenControllerPath);
                AppendIfMissing(
                    failures,
                    healthRegenSource.Contains("IsDead"),
                    "CCS_HealthRegenController must expose IsDead.");
                AppendIfMissing(
                    failures,
                    healthRegenSource.Contains("IsRegenerating"),
                    "CCS_HealthRegenController must expose IsRegenerating.");
                AppendIfMissing(
                    failures,
                    healthRegenSource.Contains("IsServer"),
                    "CCS_HealthRegenController must gate regen to server authority.");
                AppendIfMissing(
                    failures,
                    healthRegenSource.Contains("ApplyAuthorityHealthValue"),
                    "CCS_HealthRegenController must replicate health regen through CCS_NetworkAttributeReplicator.");
            }

            string barViewPath = CCS_AttributesConstants.ModuleRootPath + "/Runtime/UI/CCS_AttributeBarView.cs";
            if (File.Exists(barViewPath))
            {
                string barViewSource = File.ReadAllText(barViewPath);
                AppendIfMissing(
                    failures,
                    barViewSource.Contains("fillImage.fillAmount"),
                    "CCS_AttributeBarView must apply fill amount to the bar fill image.");
                AppendIfMissing(
                    failures,
                    barViewSource.Contains("SetSizeWithCurrentAnchors"),
                    "CCS_AttributeBarView must resize fill width from current/max percent.");
            }

            string motorPath = "Assets/CCS/Modules/CharacterController/Runtime/Components/CCS_CharacterMotor.cs";
            if (File.Exists(motorPath))
            {
                string motorSource = File.ReadAllText(motorPath);
                AppendIfMissing(
                    failures,
                    motorSource.Contains("staminaController.CanSprint"),
                    "CCS_CharacterMotor must gate sprint speed through CCS_StaminaController.CanSprint.");
                AppendIfMissing(
                    failures,
                    motorSource.Contains("ReportMovementState"),
                    "CCS_CharacterMotor must report sprint intent to CCS_StaminaController.");
                AppendIfMissing(
                    failures,
                    motorSource.Contains("MovementSpeedMultiplier"),
                    "CCS_CharacterMotor must apply stamina MovementSpeedMultiplier to walk speed.");
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
