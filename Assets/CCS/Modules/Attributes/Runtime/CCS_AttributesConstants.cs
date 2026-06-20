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

        public const string StaminaDefinitionPath =
            ModuleRootPath + "/Tests/Profiles/CCS_AttributeDefinition_Stamina.asset";

        public const string StaminaAttributeId = "ccs.survival.profile.attributes.stamina";

        public const string StaminaDisplayName = "Stamina";

        public const float StaminaDefaultMax = 100f;

        public const float StaminaDrainPerSecond = 18f;

        public const float StaminaRegenPerSecond = 6f;

        public const float StaminaSprintUnlockThreshold = 50f;

        public const float StaminaWalkRecoveryThreshold = 35f;

        public const float StaminaExhaustedWalkMultiplier = 0.5f;

        public const float HealthRegenDelaySeconds = 5f;

        public const float HealthRegenPerSecond = 3f;

        public const string StaminaExhaustedStatusText = "Exhausted";

        public const string StaminaRecoveringStatusText = "Recovering";

        public const string HealthDeadStatusText = "Dead";

        public const string HealthRecoveringStatusText = "Recovering";

        public const string LegacyDebugHudTextObjectName = "HealthHudText";

        public const string AttributeBarsPanelObjectName = "AttributeBarsPanel";

        public const string ThirstBarObjectName = "ThirstBar";
    }
}
