using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WeaponsValidationBatchEntry
// CATEGORY: Modules / Weapons / Editor
// PURPOSE: Batch-mode compile and validation entry for Weapons module releases.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Invoked via Unity -batchmode -executeMethod ...RunFromBatchMode.
// =============================================================================

namespace CCS.Modules.Weapons.Editor
{
    public static class CCS_WeaponsValidationBatchEntry
    {
        public static void RunFromBatchMode()
        {
            CCS_WeaponsAssetBuilder.EnsureWeaponsAssets();
            CCS_WeaponsTestPlayerPrefabBuilder.EnsureTestPlayerWeaponWiring();
            CCS_WeaponsMasterTestBuilder.EnsureMasterTestWeaponTarget();

            CCS_SurvivalValidationResult result = CCS_WeaponsModuleValidator.ValidateWeaponsModule();
            if (result.IsSuccess)
            {
                Debug.Log("[Weapons Batch] Validation passed: " + result.Message);
                EditorApplication.Exit(0);
                return;
            }

            Debug.LogError("[Weapons Batch] Validation failed: " + result.Message);
            EditorApplication.Exit(1);
        }
    }
}
