using CCS.Survival.Editor.Development;
using UnityEditor;

namespace CCS.Modules.Prospecting.Editor
{
    [InitializeOnLoad]
    public static class CCS_ProspectingValidationRegistration
    {
        static CCS_ProspectingValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_ProspectingFoundationValidationValidator());
        }
    }
}
