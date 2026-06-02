using CCS.Survival.Editor.Development;
using UnityEditor;

namespace CCS.Modules.Trapping.Editor
{
    [InitializeOnLoad]
    public static class CCS_FrontierTrappingValidationRegistration
    {
        static CCS_FrontierTrappingValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_FrontierTrappingValidationValidator());
        }
    }
}
