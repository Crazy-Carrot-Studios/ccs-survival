using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AIConstants
// CATEGORY: Modules / AI / Runtime
// PURPOSE: Canonical paths, IDs, and baseline tuning constants for AI module.
// PLACEMENT: Runtime constants. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: v0.7.0 Network AI bandit combat foundation.
// =============================================================================

namespace CCS.Modules.AI
{
    public static class CCS_AIConstants
    {
        public const string ModuleVersion = "0.7.1";

        public const string ModuleLogCategory = "AI";

        public const string ModuleRootPath = "Assets/CCS/Modules/AI";

        public const string AIBanditPrefabPath =
            ModuleRootPath + "/Content/Prefabs/PF_CCS_AI_Bandit_Networked.prefab";

        public const string SourceNetworkedPlayerPrefabPath =
            "Assets/CCS/Modules/CharacterController/Prefabs/Player/PF_CCS_CharacterController_Player_Networked.prefab";

        public const string AIBanditPrefabName = "PF_CCS_AI_Bandit_Networked";

        public const string AIBanditProfilePath =
            ModuleRootPath + "/Content/Profiles/CCS_AIBanditProfile_Default.asset";

        public const string AIBanditSpawnerObjectName = "CCS_AIBanditSpawner";

        public const float DefaultAimChestHeight = 1.45f;

        public const string AIBanditLabel = "AI_Bandit";

        public const string NameplateAnchorObjectName = "UIAnchor_Nameplate";

        public const string NameplateRootObjectName = "AI_Bandit_Nameplate";

        public const string NameplateCanvasObjectName = "Canvas_WorldSpace";

        public const string NameplateHealthBackgroundObjectName = "HealthBar_Background";

        public const string NameplateHealthFillObjectName = "HealthBar_Fill";

        public const string NameplateHealthSliderObjectName = "HealthBar_Slider";

        public const string NameplateNameTextObjectName = "NameText";

        public const float NameplateWorldHeight = 2.15f;

        public const float NameplateCanvasScale = 0.01f;

        public const int NameplateCanvasSortingOrder = 1000;

        public const string NavigationRootObjectName = "CCS_AINavigationRoot";

        public const string NavigationSurfaceObjectName = "NavMeshSurface_MasterTest";

        public const string NavigationLinksObjectName = "NavMeshLinks_MasterTest";

        public const string NavigationProbesObjectName = "NavMeshProbes_MasterTest";

        public const string NavigationProbeOutsideSpawn = "OutsideSpawnPoint";

        public const string NavigationProbeBuildingDoor = "BuildingDoorPoint";

        public const string NavigationProbeInsideBuilding = "InsideBuildingPoint";

        public const string NavigationProbeTopOfStairs = "TopOfStairsPoint";

        public const string NavigationProbeRampTop = "RampTopPoint";

        public const float DefaultSpawnDistanceFromPlayer = 24f;

        public const float DefaultSpawnSideOffset = 8f;
    }
}
