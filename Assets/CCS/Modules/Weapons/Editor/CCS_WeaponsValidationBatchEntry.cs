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
            TryEnsureRevolverAimSimplificationPassViaReflection();
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

        private static void TryEnsureRevolverAimSimplificationPassViaReflection()
        {
            System.Type builderType = System.Type.GetType(
                "CCS.Modules.CharacterController.Editor.CCS_RevolverAimSimplificationBuilder, CCS.Modules.CharacterController.Editor");
            if (builderType == null)
            {
                return;
            }

            System.Reflection.MethodInfo method = builderType.GetMethod(
                "EnsureRevolverAimSimplificationPass",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (method == null)
            {
                return;
            }

            if (method.Invoke(null, null) is bool changed && changed)
            {
                AssetDatabase.SaveAssets();
            }
        }
    }
}
