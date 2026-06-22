// =============================================================================
// SCRIPT: CCS_WeaponsConstants
// CATEGORY: Modules / Weapons / Runtime
// PURPOSE: Canonical paths, IDs, and version metadata for the Weapons module.
// PLACEMENT: Runtime constants. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.0 — test revolver hitscan foundation. No inventory or equipment yet.
// =============================================================================

using UnityEngine;

namespace CCS.Modules.Weapons
{
    public static class CCS_WeaponsConstants
    {
        public const string ModuleVersion = "0.6.1";

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

        public const string CapsuleVisualName = "CapsuleVisual";

        public const string GlassesVisualName = "VisualGlasses";

        public const string TestPlayerRedMaterialPath =
            "Assets/CCS/Modules/CharacterController/Materials/Player/M_CCS_TestPlayerRed.mat";

        public const string TestPlayerBlackMaterialPath =
            "Assets/CCS/Modules/CharacterController/Materials/Player/M_CCS_TestPlayerBlack.mat";

        public static readonly Vector3 GlassesVisualLocalPosition = new Vector3(0f, 1.6f, 0.222f);

        public static readonly Vector3 GlassesVisualLocalEuler = new Vector3(180f, 180f, 90f);

        public static readonly Vector3 GlassesVisualLocalScale = new Vector3(0.3f, 0.3f, 0.3f);

        public static readonly Vector3 CapsuleVisualLocalPosition = new Vector3(0f, 1f, 0f);

        public static readonly Vector3 CapsuleVisualLocalScale = new Vector3(0.7f, 1f, 0.7f);

        public const float DamageTargetCapsuleHeight = 2f;

        public const float DamageTargetCapsuleRadius = 0.35f;

        public const float DamageTargetCapsuleCenterY = 1f;

        public static readonly Color DamageTargetHealthyColor = new Color(0.85f, 0.2f, 0.2f, 1f);

        public static readonly Color DamageTargetDamagedColor = new Color(0.85f, 0.55f, 0.15f, 1f);

        public static readonly Color DamageTargetDeadColor = new Color(0.35f, 0.35f, 0.35f, 1f);

        public const string WeaponRootObjectName = "WeaponRoot";

        public const string MuzzlePointObjectName = "MuzzlePoint";

        public const string WeaponHudRootName = "WeaponHudRoot";

        public const string WeaponHudTextObjectName = "WeaponHudText";

        public const string WeaponReticleObjectName = "WeaponReticle";

        public const string PlayerRightHandSocketName = "RightHand";

        public const float TestDamageTargetForwardDistance = -10f;

        public const float TestDamageTargetLateralOffset = -5f;

        public const float TestDamageTargetHeightOffset = 0f;

        public const float WeaponHudFontSize = 28f;

        public const int WeaponHudLowAmmoThreshold = 2;

        public const float WeaponHudOutlineWidth = 0.22f;

        public static readonly Color WeaponHudAmmoNormalColor = new Color(0.08f, 0.52f, 0.12f, 1f);

        public static readonly Color WeaponHudAmmoLowColor = new Color(0.85f, 0.12f, 0.08f, 1f);

        public static readonly Color WeaponHudReloadColor = new Color(0.95f, 0.58f, 0.08f, 1f);

        public static readonly Color WeaponHudOutlineColor = new Color(0.02f, 0.02f, 0.02f, 1f);

        public static readonly Vector3 MuzzlePointLocalPosition = new Vector3(0.35f, 1.35f, 0.22f);

        public const float MuzzlePointMinimumLocalHeight = 1.2f;

        public const string DefaultRevolverDisplayName = "Test Revolver";
    }
}
