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
            "Assets/CCS/Modules/CharacterController/Tests/Prefabs/PF_CCS_CharacterController_TestPlayer_Networked.prefab";

        public const string AttributeHudRootObjectName = "AttributeHudRoot";

        public const string AttributeHudTextObjectName = "HealthHudText";

        public const float TestDamageAmount = 10f;
    }
}
