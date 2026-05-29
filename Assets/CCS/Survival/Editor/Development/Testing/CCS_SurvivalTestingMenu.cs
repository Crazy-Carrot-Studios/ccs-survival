using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalTestingMenu
// CATEGORY: Survival / Editor / Development / Testing
// PURPOSE: Editor menu helpers for dev-only survival test toggle profiles and runtime flags.
// PLACEMENT: Editor menu only. No gameplay automation in 0.3.6.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Prepares future traversal, simulation, and inventory smoke workflows.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public static class CCS_SurvivalTestingMenu
    {
        private const string ResetFlagsMenuPath = "CCS/Survival/Testing/Reset Runtime Test Flags";
        private const string ApplySelectedProfileMenuPath = "CCS/Survival/Testing/Apply Selected Test Toggle Profile";

        #region Public Methods

        [MenuItem(ResetFlagsMenuPath, priority = 200)]
        public static void ResetRuntimeTestFlags()
        {
            CCS.Survival.Development.CCS_SurvivalTestRuntimeFlags.Reset();
            Debug.Log("[CCS_SurvivalTestingMenu] Runtime test flags reset.");
        }

        [MenuItem(ApplySelectedProfileMenuPath, priority = 201)]
        public static void ApplySelectedTestToggleProfile()
        {
            CCS.Survival.Development.CCS_SurvivalTestToggleProfile selectedProfile =
                Selection.activeObject as CCS.Survival.Development.CCS_SurvivalTestToggleProfile;

            if (selectedProfile == null)
            {
                EditorUtility.DisplayDialog(
                    "Survival Testing",
                    "Select a CCS_SurvivalTestToggleProfile asset in the Project window, then run this menu again.",
                    "OK");
                return;
            }

            selectedProfile.ApplyToRuntimeFlags();
            Debug.Log(
                "[CCS_SurvivalTestingMenu] Applied test toggle profile: "
                + selectedProfile.name);
        }

        [MenuItem(ApplySelectedProfileMenuPath, validate = true)]
        public static bool ValidateApplySelectedTestToggleProfile()
        {
            return Selection.activeObject is CCS.Survival.Development.CCS_SurvivalTestToggleProfile;
        }

        #endregion
    }
}
