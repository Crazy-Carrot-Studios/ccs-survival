using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverVisualDefinition
// CATEGORY: Modules / Weapons / Runtime / Profiles
// PURPOSE: Profile-driven revolver M1879 visual prefabs, sockets, and VFX tuning.
// PLACEMENT: ScriptableObject asset under Content/RevolverM1879/.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.5 visual-only pickup/holster/equipped wiring. Hitscan remains authoritative.
// =============================================================================

namespace CCS.Modules.Weapons
{
    [CreateAssetMenu(
        fileName = "CCS_RevolverM1879VisualDefinition",
        menuName = "CCS/Weapons/Revolver Visual Definition",
        order = 1)]
    public sealed class CCS_RevolverVisualDefinition : ScriptableObject
    {
        #region Variables

        [Header("Identity")]
        [SerializeField] private string weaponId = CCS_WeaponsConstants.RevolverM1879WeaponId;

        [Header("Prefabs")]
        [SerializeField] private GameObject worldPickupPrefab;
        [SerializeField] private GameObject holsteredPrefab;
        [SerializeField] private GameObject equippedPrefab;
        [SerializeField] private GameObject bulletVisualPrefab;
        [SerializeField] private GameObject shellVisualPrefab;

        [Header("Sockets")]
        [SerializeField] private string holsterSocketName = CCS_WeaponsConstants.RevolverHolsterSocketName;
        [SerializeField] private string handSocketName = CCS_WeaponsConstants.RevolverHandSocketName;
        [SerializeField] private string muzzlePointName = CCS_WeaponsConstants.MuzzlePointObjectName;
        [SerializeField] private string shellEjectPointName = CCS_WeaponsConstants.ShellEjectPointObjectName;

        [Header("Holstered Visual")]
        [SerializeField] private Vector3 holsteredLocalPosition = CCS_WeaponsConstants.DefaultHolsteredLocalPosition;
        [SerializeField] private Vector3 holsteredLocalEulerAngles = CCS_WeaponsConstants.DefaultHolsteredLocalEuler;
        [SerializeField] private Vector3 holsteredLocalScale = CCS_WeaponsConstants.DefaultHolsteredLocalScale;

        [Header("Equipped Visual")]
        [SerializeField] private Vector3 equippedLocalPosition = CCS_WeaponsConstants.DefaultEquippedLocalPosition;
        [SerializeField] private Vector3 equippedLocalEulerAngles = CCS_WeaponsConstants.DefaultEquippedLocalEuler;
        [SerializeField] private Vector3 equippedLocalScale = CCS_WeaponsConstants.DefaultEquippedLocalScale;

        [Header("Equipped Anchor Points")]
        [SerializeField] private Vector3 muzzleLocalPosition = CCS_WeaponsConstants.DefaultMuzzleLocalPosition;
        [SerializeField] private Vector3 shellEjectLocalPosition = CCS_WeaponsConstants.DefaultShellEjectLocalPosition;
        [SerializeField] private Vector3 bulletVisualSpawnLocalPosition =
            CCS_WeaponsConstants.DefaultBulletVisualSpawnLocalPosition;

        [Header("Behavior")]
        [SerializeField] private bool equipOnAim = true;
        [SerializeField] private bool holsterWhenAimReleased = true;

        [Header("Bullet Visual")]
        [SerializeField] private float bulletVisualSpeed = CCS_WeaponsConstants.DefaultBulletVisualSpeed;
        [SerializeField] private float bulletVisualLifetime = CCS_WeaponsConstants.DefaultBulletVisualLifetime;
        [SerializeField] private bool enableDebugTracer = true;

        [Header("Shell Visual")]
        [SerializeField] private bool enableShellVisual = true;
        [SerializeField] private float shellLifetimeSeconds = CCS_WeaponsConstants.DefaultShellVisualLifetime;
        [SerializeField] private float shellEjectForce = CCS_WeaponsConstants.DefaultShellEjectForce;

        #endregion

        #region Properties

        public string WeaponId => weaponId;

        public GameObject WorldPickupPrefab => worldPickupPrefab;

        public GameObject HolsteredPrefab => holsteredPrefab;

        public GameObject EquippedPrefab => equippedPrefab;

        public GameObject BulletVisualPrefab => bulletVisualPrefab;

        public GameObject ShellVisualPrefab => shellVisualPrefab;

        public string HolsterSocketName => holsterSocketName;

        public string HandSocketName => handSocketName;

        public string MuzzlePointName => muzzlePointName;

        public string ShellEjectPointName => shellEjectPointName;

        public Vector3 HolsteredLocalPosition => holsteredLocalPosition;

        public Vector3 HolsteredLocalEulerAngles => holsteredLocalEulerAngles;

        public Vector3 HolsteredLocalScale => holsteredLocalScale;

        public Vector3 EquippedLocalPosition => equippedLocalPosition;

        public Vector3 EquippedLocalEulerAngles => equippedLocalEulerAngles;

        public Vector3 EquippedLocalScale => equippedLocalScale;

        public Vector3 MuzzleLocalPosition => muzzleLocalPosition;

        public Vector3 ShellEjectLocalPosition => shellEjectLocalPosition;

        public Vector3 BulletVisualSpawnLocalPosition => bulletVisualSpawnLocalPosition;

        public bool EquipOnAim => equipOnAim;

        public bool HolsterWhenAimReleased => holsterWhenAimReleased;

        public float BulletVisualSpeed => bulletVisualSpeed;

        public float BulletVisualLifetime => bulletVisualLifetime;

        public bool EnableDebugTracer => enableDebugTracer;

        public bool EnableShellVisual => enableShellVisual;

        public float ShellLifetimeSeconds => shellLifetimeSeconds;

        public float ShellEjectForce => shellEjectForce;

        #endregion
    }
}
