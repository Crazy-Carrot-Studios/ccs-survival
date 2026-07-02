using CCS.Modules.Interaction.Editor;
using CCS.Modules.Weapons.Editor;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ReticleRevealAnimationEventBatchEntry
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Batch-mode entry for v0.7.10f reticle reveal animation event validation.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_ReticleRevealAnimationEventBatchEntry
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
                CCS_ReticleRevealAnimationEventValidationUtility.ValidateReticleRevealAnimationEvent();
            if (!validationResult.IsSuccess)
            {
                Debug.LogError("[Reticle Reveal Animation Event Batch] Validation failed: " + validationResult.Message);
                EditorApplication.Exit(1);
                return;
            }

            string reportPath = CCS_ReticleRevealAnimationEventReportBuilder.WriteReport();
            Debug.Log(
                "[Reticle Reveal Animation Event Batch] Validation passed. report: "
                + reportPath
                + ". "
                + validationResult.Message);
            EditorApplication.Exit(0);
        }
    }
}
