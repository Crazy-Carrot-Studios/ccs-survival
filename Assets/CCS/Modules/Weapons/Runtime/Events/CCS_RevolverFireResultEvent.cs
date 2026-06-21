using System;

// =============================================================================
// SCRIPT: CCS_RevolverFireResultEvent
// CATEGORY: Modules / Weapons / Runtime / Events
// PURPOSE: Event payload after a revolver shot resolves hitscan and ammo state.
// PLACEMENT: Raised by CCS_RevolverController after each successful fire attempt.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.0 local-owner only. Future multiplayer must validate on server.
// =============================================================================

namespace CCS.Modules.Weapons
{
    public readonly struct CCS_RevolverFireResultEvent
    {
        public CCS_RevolverFireResultEvent(
            CCS_WeaponHitscanResult hitscanResult,
            int remainingAmmo,
            bool wasDryFire)
        {
            HitscanResult = hitscanResult;
            RemainingAmmo = remainingAmmo;
            WasDryFire = wasDryFire;
        }

        public CCS_WeaponHitscanResult HitscanResult { get; }

        public int RemainingAmmo { get; }

        public bool WasDryFire { get; }
    }

    public readonly struct CCS_RevolverDryFireEvent
    {
        public CCS_RevolverDryFireEvent(int remainingAmmo)
        {
            RemainingAmmo = remainingAmmo;
        }

        public int RemainingAmmo { get; }
    }

    public readonly struct CCS_RevolverStateChangedEvent
    {
        public CCS_RevolverStateChangedEvent(int currentAmmo, int maxAmmo, bool isAiming, bool isReloading)
        {
            CurrentAmmo = currentAmmo;
            MaxAmmo = maxAmmo;
            IsAiming = isAiming;
            IsReloading = isReloading;
        }

        public int CurrentAmmo { get; }

        public int MaxAmmo { get; }

        public bool IsAiming { get; }

        public bool IsReloading { get; }
    }
}
