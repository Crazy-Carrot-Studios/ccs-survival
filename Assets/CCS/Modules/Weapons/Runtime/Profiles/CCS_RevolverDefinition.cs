using CCS.Project;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverDefinition
// CATEGORY: Modules / Weapons / Runtime / Profiles
// PURPOSE: Profile-driven revolver tuning for test hitscan weapon foundation.
// PLACEMENT: ScriptableObject asset under Tests/Profiles/.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.0 test revolver only. No inventory or equipment integration yet.
// =============================================================================

namespace CCS.Modules.Weapons
{
    [CreateAssetMenu(
        fileName = "CCS_RevolverDefinition",
        menuName = "CCS/Weapons/Revolver Definition",
        order = 0)]
    public sealed class CCS_RevolverDefinition : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Identity")]
        [SerializeField] private string displayName = CCS_WeaponsConstants.DefaultRevolverDisplayName;

        [Header("Ammunition")]
        [SerializeField] private int cylinderCapacity = 6;

        [Header("Combat")]
        [SerializeField] private float damage = 25f;
        [SerializeField] private float fireCooldownSeconds = 0.35f;
        [SerializeField] private float reloadSeconds = 1.6f;
        [SerializeField] private float maxRange = 60f;
        [SerializeField] private float aimSpreadDegrees = 1.5f;
        [SerializeField] private float hipSpreadDegrees = 4f;
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField] private bool allowFireWhileReloading;

        #endregion

        #region Properties

        public string DisplayName => displayName;

        public int CylinderCapacity => cylinderCapacity;

        public float Damage => damage;

        public float FireCooldownSeconds => fireCooldownSeconds;

        public float ReloadSeconds => reloadSeconds;

        public float MaxRange => maxRange;

        public float AimSpreadDegrees => aimSpreadDegrees;

        public float HipSpreadDegrees => hipSpreadDegrees;

        public LayerMask HitMask => hitMask;

        public bool AllowFireWhileReloading => allowFireWhileReloading;

        #endregion
    }
}
