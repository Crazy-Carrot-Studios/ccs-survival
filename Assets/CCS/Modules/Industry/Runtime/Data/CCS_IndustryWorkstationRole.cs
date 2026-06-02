// =============================================================================
// SCRIPT: CCS_IndustryWorkstationRole
// CATEGORY: Modules / Industry / Runtime / Data
// PURPOSE: Stable role identifiers for frontier industry workstations.
// PLACEMENT: Referenced by shelter workbench definitions and industry profiles.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Industry
{
    public static class CCS_IndustryWorkstationRole
    {
        public const string FrontierWorkbench = "ccs.survival.industry.workstation.frontierworkbench";
        public const string SawTable = "ccs.survival.industry.workstation.sawtable";
        public const string CharcoalKiln = "ccs.survival.industry.workstation.charcoalkiln";
        public const string PrimitiveForge = "ccs.survival.industry.workstation.primitiveforge";

        public static bool IsIndustryRole(string roleId)
        {
            return !string.IsNullOrWhiteSpace(roleId);
        }
    }
}
