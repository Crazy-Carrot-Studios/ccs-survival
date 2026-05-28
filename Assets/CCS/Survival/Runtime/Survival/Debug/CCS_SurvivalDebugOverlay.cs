using CCS.Core;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalDebugOverlay
// CATEGORY: Survival / Runtime / Survival / Debug
// PURPOSE: Temporary OnGUI debug overlay for Phase 1A survival vitals validation.
// PLACEMENT: Attach to any GameObject in play mode test scenes. Not final UI.
// AUTHOR: James Schilz
// CREATED: 2026-05-27
// NOTES: Anchored top-right, compact panel. Future production UI must remain clean and out of gameplay sightlines.
// =============================================================================

namespace CCS.Survival
{
    public sealed class CCS_SurvivalDebugOverlay : MonoBehaviour
    {
        #region Variables

        [Header("Debug Overlay")]
        [Tooltip("When enabled, draws the temporary survival vitals panel.")]
        [SerializeField] private bool showOverlay = true;

        [Tooltip("Survival module used for readouts. Resolves from scene only when unset (temporary debug behavior).")]
        [SerializeField] private CCS_SurvivalModule survivalModule;

        [Header("Layout")]
        [Tooltip("Screen padding from top and right edges in pixels.")]
        [SerializeField] private float screenPadding = 12f;

        [Tooltip("Panel width in pixels.")]
        [SerializeField] private float panelWidth = 220f;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveSurvivalModuleReference();
        }

        private void OnGUI()
        {
            if (!showOverlay)
            {
                return;
            }

            if (!CCS_Validation.IsObjectValid(survivalModule))
            {
                ResolveSurvivalModuleReference();
            }

            if (!CCS_Validation.IsObjectValid(survivalModule))
            {
                DrawMissingModulePanel();
                return;
            }

            DrawSurvivalPanel();
        }

        #endregion

        #region Private Methods

        private void ResolveSurvivalModuleReference()
        {
            if (CCS_Validation.IsObjectValid(survivalModule))
            {
                return;
            }

            survivalModule = FindFirstObjectByType<CCS_SurvivalModule>();
        }

        private void DrawMissingModulePanel()
        {
            Rect panelRect = BuildTopRightRect(80f);
            GUI.Box(panelRect, string.Empty);
            GUILayout.BeginArea(panelRect);
            GUILayout.Label("Survival Debug");
            GUILayout.Label("No CCS_SurvivalModule found.");
            GUILayout.EndArea();
        }

        private void DrawSurvivalPanel()
        {
            CCS_SurvivalState state = survivalModule.CurrentState;
            Rect panelRect = BuildTopRightRect(130f);
            GUI.Box(panelRect, string.Empty);

            GUILayout.BeginArea(panelRect);
            GUILayout.Label("Survival Debug");
            GUILayout.Label($"Health: {state.Health:F1}");
            GUILayout.Label($"Hunger: {state.Hunger:F1}");
            GUILayout.Label($"Thirst: {state.Thirst:F1}");
            GUILayout.Label($"Stamina: {state.Stamina:F1}");
            GUILayout.Label(state.IsAlive ? "Status: Alive" : "Status: Dead");
            GUILayout.EndArea();
        }

        private Rect BuildTopRightRect(float height)
        {
            float x = Screen.width - panelWidth - screenPadding;
            float y = screenPadding;
            return new Rect(x, y, panelWidth, height);
        }

        #endregion
    }
}
