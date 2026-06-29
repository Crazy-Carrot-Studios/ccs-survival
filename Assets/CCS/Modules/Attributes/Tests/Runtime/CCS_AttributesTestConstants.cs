// =============================================================================
// SCRIPT: CCS_AttributesTestConstants
// CATEGORY: Modules / Attributes / Tests / Runtime
// PURPOSE: Canonical test paths for Attributes module validation and prefab wiring.
// PLACEMENT: Runtime test constants. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Networked test player path must match CharacterController test prefab path.
// =============================================================================

namespace CCS.Modules.Attributes.Tests
{
    public static class CCS_AttributesTestConstants
    {
        public const string NetworkedTestPlayerPrefabPath =
            "Assets/CCS/Modules/CharacterController/Prefabs/Player/PF_CCS_CharacterController_Player_Networked.prefab";

        public const string AttributeHudRootObjectName = "AttributeHudRoot";

        public const string AttributeBarsPanelObjectName = "AttributeBarsPanel";

        public const string HealthBarObjectName = "HealthBar";

        public const string StaminaBarObjectName = "StaminaBar";

        public const string HungerBarObjectName = "HungerBar";

        public const string ThirstBarObjectName = "ThirstBar";

        public const string LegacyAttributeHudTextObjectName = "HealthHudText";

        public const float TestDamageAmount = 10f;
    }
}
