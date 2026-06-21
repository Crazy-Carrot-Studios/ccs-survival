// =============================================================================
// SCRIPT: CCS_WeaponsConstants
// CATEGORY: Modules / Weapons / Runtime
// PURPOSE: Canonical paths, IDs, and version metadata for the Weapons module.
// PLACEMENT: Runtime constants. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.0 — test revolver hitscan foundation. No inventory or equipment yet.
// =============================================================================

namespace CCS.Modules.Weapons
{
    public static class CCS_WeaponsConstants
    {
        public const string ModuleVersion = "0.6.0";

        public const string ModuleLogCategory = "Weapons";

        public const string ModuleRootPath = "Assets/CCS/Modules/Weapons";

        public const string RevolverDefinitionProfileId = "ccs.survival.profile.weapons.revolver.test";

        public const string RevolverDefinitionProfilePath =
            ModuleRootPath + "/Tests/Profiles/CCS_RevolverDefinition_Test.asset";

        public const string TestDamageTargetPrefabPath =
            ModuleRootPath + "/Tests/Prefabs/PF_CCS_TestWeaponDamageTarget.prefab";

        public const string NetworkedTestPlayerPrefabPath =
            "Assets/CCS/Modules/CharacterController/Tests/Prefabs/PF_CCS_CharacterController_TestPlayer_Networked.prefab";

        public const string MasterTestScenePath =
            "Assets/CCS/Scenes/CharacterController/SCN_CCS_CharacterController_MasterTest.unity";

        public const string MasterTestSpawnOriginObjectPath = "TestPoints/TP_Spawn_Host";

        public const string TestDamageTargetObjectName = "CCS_TestWeaponDamageTarget";

        public const string WeaponRootObjectName = "WeaponRoot";

        public const string MuzzlePointObjectName = "MuzzlePoint";

        public const string WeaponHudRootName = "WeaponHudRoot";

        public const string WeaponHudTextObjectName = "WeaponHudText";

        public const string WeaponReticleObjectName = "WeaponReticle";

        public const string PlayerRightHandSocketName = "RightHand";

        public const float TestDamageTargetForwardDistance = 8f;

        public const float TestDamageTargetLateralOffset = 3f;

        public const float TestDamageTargetHeightOffset = 1f;

        public const float WeaponHudFontSize = 28f;

        public const string DefaultRevolverDisplayName = "Test Revolver";
    }
}
