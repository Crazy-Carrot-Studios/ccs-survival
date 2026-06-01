using System.Collections.Generic;
using CCS.Survival.Player;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlaytestHud
// CATEGORY: Modules / Playtesting / Runtime / UI
// PURPOSE: Developer-only on-screen manual playtest checklist for bootstrap scenes.
// PLACEMENT: Bootstrap scene or PF_CCS_Survival_BootstrapRoot (PlaytestHarness child).
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: F10 HUD, F11 advance, F12 reset, F7 death, F6 equip spear, B place foundation.
// =============================================================================

namespace CCS.Modules.Playtesting
{
    public sealed class CCS_PlaytestHud : MonoBehaviour
    {
        private const float PanelWidth = 420f;
        private const float PanelMargin = 12f;

        #region Variables

        [Header("Harness")]
        [Tooltip("When false, HUD and hotkeys remain inactive even if the service is registered.")]
        [SerializeField] private bool enableHud = true;

        private CCS_PlaytestService playtestService;
        private bool hudVisible = true;
        private bool spawnNotified;

        #endregion

        #region Unity Callbacks

        private void Update()
        {
            if (!enableHud || !TryResolveService())
            {
                return;
            }

            if (!playtestService.HarnessEnabled)
            {
                return;
            }

            HandleHotkeys();
            TryNotifySpawnReady();
        }

        private void OnGUI()
        {
            if (!enableHud || !hudVisible || playtestService == null || !playtestService.HarnessEnabled)
            {
                return;
            }

            DrawHarnessPanel();
        }

        #endregion

        #region Private Methods

        private bool TryResolveService()
        {
            if (playtestService != null && playtestService.IsInitialized)
            {
                return true;
            }

            return CCS_PlaytestRuntimeBridge.TryGetPlaytestService(out playtestService)
                && playtestService.IsInitialized;
        }

        private void HandleHotkeys()
        {
            if (Input.GetKeyDown(KeyCode.F10))
            {
                hudVisible = !hudVisible;
            }

            if (Input.GetKeyDown(KeyCode.F11))
            {
                playtestService.AdvanceActiveStep();
            }

            if (Input.GetKeyDown(KeyCode.F12))
            {
                playtestService.ResetSteps();
                spawnNotified = false;
                TryNotifySpawnReady();
            }

            if (Input.GetKeyDown(KeyCode.F7))
            {
                playtestService.ForceTestDeathCondition();
            }
        }

        private void TryNotifySpawnReady()
        {
            if (spawnNotified)
            {
                return;
            }

            CCS_PlayerGameplayController[] players =
                Object.FindObjectsByType<CCS_PlayerGameplayController>(FindObjectsSortMode.None);
            if (players == null || players.Length == 0)
            {
                return;
            }

            playtestService.NotifyBootstrapSessionReady();
            spawnNotified = true;
        }

        private void DrawHarnessPanel()
        {
            float panelHeight = Mathf.Min(Screen.height - PanelMargin * 2f, 520f);
            Rect panelRect = new Rect(
                Screen.width - PanelWidth - PanelMargin,
                PanelMargin,
                PanelWidth,
                panelHeight);

            GUI.Box(panelRect, GUIContent.none);
            GUILayout.BeginArea(new Rect(panelRect.x + 10f, panelRect.y + 10f, panelRect.width - 20f, panelRect.height - 20f));

            GUILayout.Label("CCS Manual Playtest Harness (1.0.3)");
            GUILayout.Label("F10 HUD | F11 Advance | F12 Reset | F7 Death | F6 Equip | B Build");
            GUILayout.Label("Interact gather/cook | Primary hunt | F eat | F5 save | F9 load");
            GUILayout.Space(6f);

            CCS_PlaytestStepState activeState = GetActiveStepState();
            if (activeState != null)
            {
                GUILayout.Label($"Active: {activeState.Definition.DisplayName}");
                GUILayout.Label(activeState.Definition.InstructionText, GUI.skin.box);
            }
            else
            {
                GUILayout.Label("All checklist steps complete or skipped.");
            }

            GUILayout.Space(8f);
            DrawChecklist();

            GUILayout.EndArea();
        }

        private void DrawChecklist()
        {
            IReadOnlyList<CCS_PlaytestStepState> states = playtestService.StepStates;
            for (int index = 0; index < states.Count; index++)
            {
                CCS_PlaytestStepState state = states[index];
                string marker = GetStatusMarker(state.Status);
                GUILayout.Label($"{marker} {index + 1}. {state.Definition.DisplayName}");
            }
        }

        private static CCS_PlaytestStepState GetActiveStepStateFromService(CCS_PlaytestService service)
        {
            int activeIndex = service.ActiveStepIndex;
            IReadOnlyList<CCS_PlaytestStepState> states = service.StepStates;
            if (activeIndex < 0 || activeIndex >= states.Count)
            {
                return null;
            }

            return states[activeIndex];
        }

        private CCS_PlaytestStepState GetActiveStepState()
        {
            return GetActiveStepStateFromService(playtestService);
        }

        private static string GetStatusMarker(CCS_PlaytestStepStatus status)
        {
            switch (status)
            {
                case CCS_PlaytestStepStatus.Passed:
                    return "[PASS]";
                case CCS_PlaytestStepStatus.Active:
                    return "[>>]";
                case CCS_PlaytestStepStatus.Failed:
                    return "[FAIL]";
                case CCS_PlaytestStepStatus.Skipped:
                    return "[SKIP]";
                default:
                    return "[  ]";
            }
        }

        #endregion
    }
}
