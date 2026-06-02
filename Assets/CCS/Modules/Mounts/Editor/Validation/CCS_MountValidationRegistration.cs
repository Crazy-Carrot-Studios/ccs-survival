using CCS.Survival.Editor.Development;
using UnityEditor;

namespace CCS.Modules.Mounts.Editor
{
    [InitializeOnLoad]
    public static class CCS_MountValidationRegistration
    {
        static CCS_MountValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_HorseFoundationValidationValidator());
        }
    }
}
