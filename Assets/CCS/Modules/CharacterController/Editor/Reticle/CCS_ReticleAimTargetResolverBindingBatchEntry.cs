using CCS.Modules.Interaction.Editor;
using CCS.Modules.Weapons.Editor;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ReticleAimTargetResolverBindingBatchEntry
// CATEGORY: Modules / CharacterController / Editor / Reticle
// PURPOSE: Batch-mode entry for v0.7.12a reticle aim target resolver binding.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_ReticleAimTargetResolverBindingBatchEntry
    {
        public static void RunFromBatchMode()
        {
            CCS_CharacterControllerMasterTestBuilder.SetupMasterTestScene();
            CCS_InteractionDetectionTestBuilder.BuildMasterTestInteractions();
            CCS_WeaponsAssetBuilder.EnsureTestDamageTargetPrefab();
            CCS_SingleRevolverAimLayerBuilder.EnsureSingleRevolverAimLayer();
            CCS_SingleRevolverAimLayerBuilder.EnsureSingleRevolverAimAnimatorOnNetworkedPlayerPrefab();
            CCS_PlayerVisualKevinSwapBuilder.EnsureKevinModelOnNetworkedPlayerPrefab();
            CCS_RevolverReticlePresentationProfileBuilder.EnsureRevolverReticlePresentationProfile();
            CCS_WeaponsTestPlayerPrefabBuilder.EnsureTestPlayerWeaponWiring();
            CCS_RevolverAimTargetResolverBuilder.EnsureRevolverAimTargetResolver();
            AssetDatabase.SaveAssets();

            string auditPath = CCS_ReticleAimTargetResolverBindingReportBuilder.WriteAuditReport();
            CCS_SurvivalValidationResult validationResult =
                CCS_ReticleAimTargetResolverBindingValidationUtility.ValidateReticleAimTargetResolverBinding();
            if (!validationResult.IsSuccess)
            {
                Debug.LogError(
                    "[Reticle Aim Target Resolver Binding Batch] Validation failed: "
                    + validationResult.Message);
                EditorApplication.Exit(1);
                return;
            }

            string reportPath = CCS_ReticleAimTargetResolverBindingReportBuilder.WriteReport();
            Debug.Log(
                "[Reticle Aim Target Resolver Binding Batch] Validation passed. audit: "
                + auditPath
                + " report: "
                + reportPath
                + ". "
                + validationResult.Message);
            EditorApplication.Exit(0);
        }
    }
}
