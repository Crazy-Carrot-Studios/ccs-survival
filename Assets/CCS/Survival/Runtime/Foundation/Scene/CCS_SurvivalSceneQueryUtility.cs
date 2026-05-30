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
#pragma warning disable CS0618
            return Object.FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#pragma warning restore CS0618
        }

        public static T[] FindAllObjectsByType<T>() where T : Object
        {
#pragma warning disable CS0618
            return Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#pragma warning restore CS0618
        }

        #endregion
    }
}
