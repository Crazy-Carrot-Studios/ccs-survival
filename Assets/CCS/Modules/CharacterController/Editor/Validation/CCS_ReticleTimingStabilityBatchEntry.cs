using CCS.Modules.Interaction.Editor;
using CCS.Modules.Weapons.Editor;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ReticleTimingStabilityBatchEntry
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Batch-mode entry for v0.7.10e reticle timing/stability validation.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-30
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_ReticleTimingStabilityBatchEntry
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
            AssetDatabase.SaveAssets();

            CCS_SurvivalValidationResult validationResult =
                CCS_ReticleTimingStabilityValidationUtility.ValidateReticleTimingStability();
            if (!validationResult.IsSuccess)
            {
                Debug.LogError("[Reticle Timing Stability Batch] Validation failed: " + validationResult.Message);
                EditorApplication.Exit(1);
                return;
            }

            string reportPath = CCS_ReticleTimingStabilityReportBuilder.WriteReport();
            Debug.Log(
                "[Reticle Timing Stability Batch] Validation passed. report: "
                + reportPath
                + ". "
                + validationResult.Message);
            EditorApplication.Exit(0);
        }
    }
}
