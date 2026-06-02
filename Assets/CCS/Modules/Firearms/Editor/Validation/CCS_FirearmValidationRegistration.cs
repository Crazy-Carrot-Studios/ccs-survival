using CCS.Survival.Editor.Development;
using UnityEditor;

namespace CCS.Modules.Firearms.Editor
{
    [InitializeOnLoad]
    public static class CCS_FirearmValidationRegistration
    {
        static CCS_FirearmValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_FirearmFoundationValidationValidator());
        }
    }
}
