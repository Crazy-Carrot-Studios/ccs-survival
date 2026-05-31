using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_WeatherValidationMenu
// CATEGORY: Modules / Weather / Editor / Validation
// PURPOSE: Menu entry for weather validation through the central pipeline.
// PLACEMENT: Menu path CCS/Survival/Weather/Validate Weather.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: ValidateWeather is the Unity batchmode -executeMethod entry point.
// =============================================================================

namespace CCS.Modules.Weather.Editor
{
    public static class CCS_WeatherValidationMenu
    {
        private const string MenuPath = "CCS/Survival/Weather/Validate Weather";
        private const string LogPrefix = "CCS_WeatherValidationMenu";

        #region Public Methods

        [MenuItem(MenuPath, priority = 170)]
        public static void ValidateWeather()
        {
            CCS_SurvivalValidationReport report = CCS_SurvivalValidationBatchUtility.RunAllValidators();
            int exitCode = CCS_SurvivalValidationBatchUtility.EvaluateReport(LogPrefix, report);
            CCS_SurvivalValidationBatchUtility.ShowResultDialog("Weather Validation", report);
            CCS_SurvivalValidationBatchUtility.CompleteBatchRun(exitCode);
        }

        #endregion
    }
}
