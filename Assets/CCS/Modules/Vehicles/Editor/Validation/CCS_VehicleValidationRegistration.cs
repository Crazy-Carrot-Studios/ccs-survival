using CCS.Survival.Editor.Development;
using UnityEditor;

namespace CCS.Modules.Vehicles.Editor
{
    [InitializeOnLoad]
    public static class CCS_VehicleValidationRegistration
    {
        static CCS_VehicleValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_WagonFoundationValidationValidator());
        }
    }
}
