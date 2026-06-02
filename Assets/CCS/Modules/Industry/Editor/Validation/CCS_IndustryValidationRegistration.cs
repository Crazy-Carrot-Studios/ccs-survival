using CCS.Survival.Editor.Development;
using UnityEditor;

namespace CCS.Modules.Industry.Editor
{
    [InitializeOnLoad]
    public static class CCS_IndustryValidationRegistration
    {
        static CCS_IndustryValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_FrontierIndustryValidationValidator());
        }
    }
}
