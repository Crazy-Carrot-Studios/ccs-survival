// =============================================================================
// SCRIPT: CCS_AttributesConstants
// CATEGORY: Modules / Attributes / Runtime
// PURPOSE: Canonical paths, IDs, and version metadata for the Attributes module.
// PLACEMENT: Runtime constants. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.3.0 foundation — Health is the first attribute definition.
// =============================================================================

namespace CCS.Modules.Attributes
{
    public static class CCS_AttributesConstants
    {
        public const string ModuleVersion = "0.3.0";

        public const string ModuleLogCategory = "Attributes";

        public const string ModuleRootPath = "Assets/CCS/Modules/Attributes";

        public const string HealthDefinitionPath =
            ModuleRootPath + "/Tests/Profiles/CCS_AttributeDefinition_Health.asset";

        public const string HealthAttributeId = "ccs.survival.profile.attributes.health";

        public const string HealthDisplayName = "Health";

        public const string LegacyDebugHudTextObjectName = "HealthHudText";

        public const string AttributeBarsPanelObjectName = "AttributeBarsPanel";

        public const string ThirstBarObjectName = "ThirstBar";
    }
}
