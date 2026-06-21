using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WeaponsMenus
// CATEGORY: Modules / Weapons / Editor
// PURPOSE: Registers the Weapons module validation and setup editor menus.
// PLACEMENT: Editor-only static registration. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Repairs assets, player wiring, and Master Test target before validation.
// =============================================================================

namespace CCS.Modules.Weapons.Editor
{
    public static class CCS_WeaponsMenus
    {
        private const string MenuRoot = "CCS/Weapons/";

        #region Public Methods

        [MenuItem(MenuRoot + "Build Master Test Weapon Target")]
        public static void BuildMasterTestWeaponTargetMenu()
        {
            bool changed = CCS_WeaponsMasterTestBuilder.EnsureMasterTestWeaponTarget();
            Debug.Log(
                changed
                    ? "[Weapons] Master Test weapon target updated."
                    : "[Weapons] Master Test weapon target already up to date.");
        }

        [MenuItem(MenuRoot + "Validate Weapons Module")]
        public static void ValidateWeaponsModuleMenu()
        {
            CCS_WeaponsAssetBuilder.EnsureWeaponsAssets();
            CCS_WeaponsTestPlayerPrefabBuilder.EnsureTestPlayerWeaponWiring();
            CCS_WeaponsMasterTestBuilder.EnsureMasterTestWeaponTarget();
            LogResult(CCS_WeaponsModuleValidator.ValidateWeaponsModule());
        }

        #endregion

        #region Private Methods

        private static void LogResult(CCS_SurvivalValidationResult result)
        {
            if (result.IsSuccess)
            {
                Debug.Log($"[Validation] Passed: {result.Message}");
            }
            else
            {
                Debug.LogError($"[Validation] Failed: {result.Message}");
            }
        }

        #endregion
    }
}
