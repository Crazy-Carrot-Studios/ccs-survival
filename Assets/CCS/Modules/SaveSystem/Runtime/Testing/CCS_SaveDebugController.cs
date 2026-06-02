using CCS.Modules.CharacterController;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SaveDebugController
// CATEGORY: Modules / SaveSystem / Runtime / Testing
// PURPOSE: Development hotkeys for manual unified save and load verification.
// PLACEMENT: Bootstrap scene debug area alongside other module test controllers.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: F5 save, F9 load, Delete save file with F8. Not player-facing UI.
// =============================================================================

namespace CCS.Modules.SaveSystem
{
    public sealed class CCS_SaveDebugController : MonoBehaviour
    {
        private const string LogPrefix = "[CCS_SaveDebugController]";

        [Header("Debug Controls")]
        [Tooltip("When false, hotkeys are ignored.")]
        [SerializeField] private bool debugControlsEnabled = true;

        #region Unity Callbacks

        private void Update()
        {
            if (!debugControlsEnabled)
            {
                return;
            }

            if (!CCS_SaveRuntimeBridge.TryGetSaveService(out CCS_SaveService saveService))
            {
                return;
            }

            if (CCS_KeyboardInputUtility.WasKeyPressedThisFrame(KeyCode.F5))
            {
                bool saved = saveService.SaveGame();
                Debug.Log($"{LogPrefix} Manual save {(saved ? "succeeded" : "failed")}.");
            }

            if (CCS_KeyboardInputUtility.WasKeyPressedThisFrame(KeyCode.F9))
            {
                bool loaded = saveService.LoadGame();
                Debug.Log($"{LogPrefix} Manual load {(loaded ? "succeeded" : "failed")}.");
            }

            if (CCS_KeyboardInputUtility.WasKeyPressedThisFrame(KeyCode.F8))
            {
                bool deleted = saveService.DeleteSave();
                Debug.Log($"{LogPrefix} Delete save {(deleted ? "succeeded" : "failed")}.");
            }
        }

        #endregion
    }
}
