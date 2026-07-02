using CCS.Modules.Interaction.Editor;
using CCS.Modules.Weapons.Editor;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverAimTargetResolverBatchEntry
// CATEGORY: Modules / CharacterController / Editor / Aiming
// PURPOSE: Batch-mode entry for v0.7.12 aim target resolver validation.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_RevolverAimTargetResolverBatchEntry
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

            CCS_SurvivalValidationResult validationResult =
                CCS_RevolverAimTargetResolverValidationUtility.ValidateRevolverAimTargetResolver();
            if (!validationResult.IsSuccess)
            {
                Debug.LogError("[Aim Target Resolver Batch] Validation failed: " + validationResult.Message);
                EditorApplication.Exit(1);
                return;
            }

            string reportPath = CCS_RevolverAimTargetResolverReportBuilder.WriteReport();
            Debug.Log(
                "[Aim Target Resolver Batch] Validation passed. report: "
                + reportPath
                + ". "
                + validationResult.Message);
            EditorApplication.Exit(0);
        }
    }
}
