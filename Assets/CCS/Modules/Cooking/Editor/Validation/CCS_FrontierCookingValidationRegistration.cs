using CCS.Survival.Editor.Development;
using UnityEditor;

namespace CCS.Modules.Cooking.Editor
{
    [InitializeOnLoad]
    public static class CCS_FrontierCookingValidationRegistration
    {
        static CCS_FrontierCookingValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_FrontierCookingValidationValidator());
        }
    }
}
