// =============================================================================
// SCRIPT: CCS_FirearmContentIds
// CATEGORY: Modules / Firearms / Runtime / Data
// PURPOSE: Stable content identifiers for firearm foundation validation and wiring.
// PLACEMENT: Referenced by bootstrap, validators, and composition binds.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Firearms
{
    public static class CCS_FirearmContentIds
    {
        public const string FrontierRevolverFirearmId = "ccs.survival.firearm.revolver.frontier";
        public const string FrontierRifleFirearmId = "ccs.survival.firearm.rifle.frontier";
        public const string FrontierShotgunFirearmId = "ccs.survival.firearm.shotgun.frontier";

        public const string FrontierRevolverItemId = "ccs.survival.item.firearm.revolver.frontier";
        public const string FrontierRifleItemId = "ccs.survival.item.firearm.rifle.frontier";
        public const string FrontierShotgunItemId = "ccs.survival.item.firearm.shotgun.frontier";

        public const string RevolverCartridgeAmmoId = "ccs.survival.ammo.revolver.cartridge";
        public const string RifleCartridgeAmmoId = "ccs.survival.ammo.rifle.cartridge";
        public const string ShotgunShellAmmoId = "ccs.survival.ammo.shotgun.shell";

        public const string RevolverCartridgeItemId = "ccs.survival.item.ammo.revolver.cartridge";
        public const string RifleCartridgeItemId = "ccs.survival.item.ammo.rifle.cartridge";
        public const string ShotgunShellItemId = "ccs.survival.item.ammo.shotgun.shell";

        public const string FrontierGunsmithVendorId = "ccs.survival.vendor.frontier.gunsmith";

        public const string RevolverPrefabName = "PF_CCS_FrontierRevolver";
        public const string RiflePrefabName = "PF_CCS_FrontierRifle";
        public const string ShotgunPrefabName = "PF_CCS_FrontierShotgun";
    }
}
