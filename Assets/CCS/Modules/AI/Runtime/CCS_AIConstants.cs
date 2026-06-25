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
        public const string ModuleVersion = "0.7.0";

        public const string ModuleLogCategory = "AI";

        public const string ModuleRootPath = "Assets/CCS/Modules/AI";

        public const string AIBanditPrefabPath =
            ModuleRootPath + "/Content/Prefabs/PF_CCS_AI_Bandit_Networked.prefab";

        public const string SourceNetworkedPlayerPrefabPath =
            "Assets/CCS/Modules/CharacterController/Tests/Prefabs/PF_CCS_CharacterController_TestPlayer_Networked.prefab";

        public const string AIBanditPrefabName = "PF_CCS_AI_Bandit_Networked";

        public const string AIBanditProfilePath =
            ModuleRootPath + "/Content/Profiles/CCS_AIBanditProfile_Default.asset";

        public const string AIBanditSpawnerObjectName = "CCS_AIBanditSpawner";

        public const float DefaultAimChestHeight = 1.45f;

        public const string AIBanditLabel = "AI_Bandit";

        public const float DefaultSpawnDistanceFromPlayer = 22f;

        public const float DefaultSpawnSideOffset = 8f;
    }
}
