// =============================================================================
// SCRIPT: CCS_WeaponsConstants
// CATEGORY: Modules / Weapons / Runtime
// PURPOSE: Canonical paths, IDs, and version metadata for the Weapons module.
// PLACEMENT: Runtime constants. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.5 — revolver M1879 pickup, holster, and equipped visual foundation.
// =============================================================================

using UnityEngine;

namespace CCS.Modules.Weapons
{
    public static class CCS_WeaponsConstants
    {
        public const string ModuleVersion = "0.6.5";

        public const string ModuleLogCategory = "Weapons";

        public const string ModuleRootPath = "Assets/CCS/Modules/Weapons";

        public const string RevolverM1879WeaponId = "ccs.weapon.revolver.m1879";

        public const string RevolverM1879ContentRootPath = ModuleRootPath + "/Content/RevolverM1879";

        public const string RevolverM1879ModelsPath = RevolverM1879ContentRootPath + "/Models";

        public const string RevolverM1879MaterialsPath = RevolverM1879ContentRootPath + "/Materials";

        public const string RevolverM1879TexturesPath = RevolverM1879ContentRootPath + "/Textures";

        public const string RevolverM1879PrefabsPath = RevolverM1879ContentRootPath + "/Prefabs";

        public const string RevolverM1879ModelAssetPath = RevolverM1879ModelsPath + "/CCS_RevolverM1879_Model.fbx";

        public const string RevolverM1879ShellModelAssetPath =
            RevolverM1879ModelsPath + "/CCS_RevolverM1879_ShellVisual.fbx";

        public const string RevolverM1879BulletModelAssetPath =
            RevolverM1879ModelsPath + "/CCS_RevolverM1879_BulletVisual.fbx";

        public const string RevolverM1879MaterialAssetPath =
            RevolverM1879MaterialsPath + "/CCS_RevolverM1879_Material.mat";

        public const string RevolverM1879WoodGripMaterialAssetPath =
            RevolverM1879MaterialsPath + "/CCS_RevolverM1879_WoodGrip.mat";

        public const string RevolverM1879MetalMaterialAssetPath =
            RevolverM1879MaterialsPath + "/CCS_RevolverM1879_Metal.mat";

        public const string RevolverM1879ShellMaterialAssetPath =
            RevolverM1879MaterialsPath + "/CCS_RevolverM1879_ShellMaterial.mat";

        public const string RevolverM1879BulletMaterialAssetPath =
            RevolverM1879MaterialsPath + "/CCS_RevolverM1879_BulletMaterial.mat";

        public const string RevolverM1879AlbedoTexturePath =
            RevolverM1879TexturesPath + "/CCS_RevolverM1879_Albedo.tga";

        public const string RevolverM1879NormalTexturePath =
            RevolverM1879TexturesPath + "/CCS_RevolverM1879_Normal.tga";

        public const string RevolverM1879MetallicTexturePath =
            RevolverM1879TexturesPath + "/CCS_RevolverM1879_Metallic.tga";

        public const string RevolverM1879VisualDefinitionPath =
            RevolverM1879ContentRootPath + "/CCS_RevolverM1879VisualDefinition.asset";

        public const string RevolverM1879WorldPickupPrefabPath =
            RevolverM1879PrefabsPath + "/PF_CCS_RevolverM1879_WorldPickup.prefab";

        public const string RevolverM1879HolsteredPrefabPath =
            RevolverM1879PrefabsPath + "/PF_CCS_RevolverM1879_Holstered.prefab";

        public const string RevolverM1879EquippedPrefabPath =
            RevolverM1879PrefabsPath + "/PF_CCS_RevolverM1879_Equipped.prefab";

        public const string RevolverM1879BulletVisualPrefabPath =
            RevolverM1879PrefabsPath + "/PF_CCS_RevolverM1879_BulletVisual.prefab";

        public const string RevolverM1879ShellVisualPrefabPath =
            RevolverM1879PrefabsPath + "/PF_CCS_RevolverM1879_ShellVisual.prefab";

        public const string RevolverM1879WorldPickupInstanceName = "CCS_RevolverM1879_WorldPickup";

        public const string LegacyReichsrevolverSourceRootPath = "Assets/Reichsrevolver_M1879";

        public const string VendorSourceReichsrevolverRootPath = "Assets/VendorSource/Reichsrevolver_M1879";

        public const string VendorSourceReichsrevolverPrefabGuid = "20d6fada1e8caa1488140d8433a67d6b";

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

        public const string PlayerRightHipSocketName = "RightHip";

        public const string PlayerSocketsRootName = "Sockets";

        public const string RevolverHandSocketName = "CCS_RevolverHandSocket_Right";

        public const string RevolverHolsterSocketName = "CCS_RevolverHolsterSocket_RightHip";

        public const string ShellEjectPointObjectName = "ShellEjectPoint";

        public const string BulletVisualSpawnPointObjectName = "BulletVisualSpawnPoint";

        public const string CylinderPointObjectName = "CylinderPoint";

        public const string RevolverModelRootObjectName = "ModelRoot";

        public const string RevolverMaterializedVisualChildName = "RevolverVisual";

        public const string RevolverM1879MaterializedVisualPrefabPath =
            RevolverM1879PrefabsPath + "/PF_CCS_RevolverM1879_MaterializedVisual.prefab";

        public static readonly Vector3 RevolverHandSocketLocalPosition = new Vector3(0.02f, 0.02f, 0.04f);

        public static readonly Vector3 RevolverHandSocketLocalEuler = Vector3.zero;

        public static readonly Vector3 RevolverHolsterSocketLocalPosition = new Vector3(0.28f, 0.82f, -0.10f);

        public static readonly Vector3 RevolverHolsterSocketLocalEuler = new Vector3(0f, 90f, -12f);

        public static readonly Vector3 DefaultHolsteredLocalPosition = Vector3.zero;

        public static readonly Vector3 DefaultHolsteredLocalEuler = new Vector3(0f, 180f, 90f);

        public static readonly Vector3 DefaultHolsteredLocalScale = Vector3.one;

        public static readonly Vector3 DefaultEquippedLocalPosition = Vector3.zero;

        public static readonly Vector3 DefaultEquippedLocalEuler = new Vector3(90f, 0f, 0f);

        public static readonly Vector3 DefaultEquippedLocalScale = Vector3.one;

        public static readonly Vector3 DefaultMuzzleLocalPosition = new Vector3(0f, 0.24f, 0f);

        public static readonly Vector3 DefaultShellEjectLocalPosition = new Vector3(0.04f, 0.05f, 0f);

        public static readonly Vector3 DefaultBulletVisualSpawnLocalPosition = new Vector3(0f, 0.24f, 0f);

        public static readonly Vector3 DefaultCylinderLocalPosition = new Vector3(0f, 0.1f, 0.02f);

        public const float RevolverWorldPickupForwardDistance = 2f;

        public const float RevolverWorldPickupRightDistance = 2.25f;

        public const float RevolverWorldPickupHeightOffset = 0.85f;

        public const float DefaultBulletVisualSpeed = 120f;

        public const float DefaultBulletVisualLifetime = 0.25f;

        public const float DefaultShellVisualLifetime = 3f;

        public const float DefaultShellEjectForce = 1.25f;

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

        public const string DefaultRevolverDisplayName = "M1879 Revolver";
    }
}
