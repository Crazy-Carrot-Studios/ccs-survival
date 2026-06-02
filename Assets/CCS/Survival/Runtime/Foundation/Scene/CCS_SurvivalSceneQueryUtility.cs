using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalSceneQueryUtility
// CATEGORY: Survival / Runtime / Foundation / Scene
// PURPOSE: Unity-version-safe scene object queries for validation utilities.
// PLACEMENT: Shared by scene bootstrap validation helpers.
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: Centralizes FindObjectsByType overload differences across Unity 6 versions.
// =============================================================================

namespace CCS.Survival
{
    public static class CCS_SurvivalSceneQueryUtility
    {
        #region Public Methods

        public static T[] FindActiveObjectsByType<T>() where T : Object
        {
            return Object.FindObjectsByType<T>(FindObjectsInactive.Exclude);
        }

        public static T[] FindAllObjectsByType<T>() where T : Object
        {
            return Object.FindObjectsByType<T>(FindObjectsInactive.Include);
        }

        #endregion
    }
}
