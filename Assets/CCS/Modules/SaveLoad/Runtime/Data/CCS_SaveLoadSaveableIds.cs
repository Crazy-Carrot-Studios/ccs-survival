// =============================================================================
// SCRIPT: CCS_SaveLoadSaveableIds
// CATEGORY: Modules / SaveLoad / Runtime / Data
// PURPOSE: Stable saveable identifiers and restore ordering for module payloads.
// PLACEMENT: Referenced by gameplay services, validators, and debug tooling.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Inventory restores before equipment so items exist before equip restore.
// =============================================================================

namespace CCS.Modules.SaveLoad
{
    public static class CCS_SaveLoadSaveableIds
    {
        #region Variables

        public const string PlayerInventory = "ccs.survival.saveable.inventory.player";

        public const string PlayerEquipment = "ccs.survival.saveable.equipment.player";

        public const string TestDevelopment = "ccs.survival.saveable.test.development";

        public const string GlobalTimeOfDay = "ccs.survival.saveable.timeofday.global";

        public const string GlobalWeather = "ccs.survival.saveable.weather.global";

        public const string GlobalShelter = "ccs.survival.saveable.shelter.global";

        public const string GlobalEnvironment = "ccs.survival.saveable.environment.global";

        public const string GlobalBuilding = "ccs.survival.saveable.building.global";

        public static readonly string[] ModuleRestoreOrder =
        {
            PlayerInventory,
            PlayerEquipment,
            GlobalTimeOfDay,
            GlobalWeather,
            GlobalShelter,
            GlobalEnvironment,
            GlobalBuilding
        };

        #endregion
    }
}
