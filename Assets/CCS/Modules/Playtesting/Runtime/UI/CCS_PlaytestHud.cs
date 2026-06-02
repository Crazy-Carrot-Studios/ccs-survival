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
// NOTES: F10 HUD, F11 advance, F12 reset, F7 death, F6 equip, Shift+F6 tool, Alpha1/Alpha2 active, B build, F4/F3 bench, F2/F1 storage.
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

            if (Input.GetKeyDown(KeyCode.F6))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    CCS_PlaytestStepState activeStep = GetActiveStepState();
                    if (activeStep != null
                        && activeStep.Definition.StepType == CCS_PlaytestStepType.EquipBowForHunt)
                    {
                        playtestService.TryEquipBowForHunt();
                    }
                    else if (activeStep != null
                        && (activeStep.Definition.StepType == CCS_PlaytestStepType.EquipFishingPole
                            || activeStep.Definition.StepType == CCS_PlaytestStepType.UseFishingPoleOnSpot))
                    {
                        playtestService.TryEquipFishingPole();
                    }
                    else if (activeStep != null
                        && activeStep.Definition.StepType == CCS_PlaytestStepType.UsePickOnRock)
                    {
                        playtestService.TryEquipBonePick();
                    }
                    else
                    {
                        playtestService.TryEquipBoneHatchet();
                    }
                }
                else
                {
                    CCS_PlaytestStepState activeStep = GetActiveStepState();
                    if (activeStep != null
                        && activeStep.Definition.StepType == CCS_PlaytestStepType.EquipSpearRegression)
                    {
                        playtestService.TryEquipStarterSpear();
                    }
                    else
                    {
                        playtestService.TryEquipPocketKnife();
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                playtestService.TrySelectActiveFromMainHand();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                playtestService.TrySelectActiveFromToolSlot();
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                playtestService.TryPlacePlaytestFoundation();
            }

            if (Input.GetKeyDown(KeyCode.F4))
            {
                playtestService.TrySeedWorkbenchCraftingResources();
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                playtestService.TryCraftWorkbenchPlaytestItem();
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    playtestService.TryPlaceOrSleepBedrollNearPlayer();
                }
                else
                {
                    playtestService.TryPlaceOrOpenStorageCrateNearPlayer();
                }
            }

            if (Input.GetKeyDown(KeyCode.F1))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    playtestService.TryMoveFirstStorageItemToPlayer();
                }
                else
                {
                    playtestService.TryMoveFirstPlayerItemToActiveStorageCrate();
                }
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                    {
                        playtestService.TryGrantPlaytestRawMeat();
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
                {
                    playtestService.TryGrantPlaytestTrap();
                }
                else if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                {
                    playtestService.TryForceTrapTriggerForPlaytest();
                }
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    playtestService.TryGrantPlaytestBow();
                }
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    playtestService.TryGrantPlaytestRawFish();
                }
                else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    CCS_PlaytestStepState activeStep = GetActiveStepState();
                    if (activeStep != null
                        && activeStep.Definition.StepType == CCS_PlaytestStepType.SellPreservedFoodAtVendor)
                    {
                        playtestService.TryPlaytestSellPreservedFood();
                    }
                    else if (activeStep != null
                        && (activeStep.Definition.StepType == CCS_PlaytestStepType.SellHuntingResourceAtVendor
                            || activeStep.Definition.StepType == CCS_PlaytestStepType.SellTrappingResourceAtVendor))
                    {
                        playtestService.TryPlaytestSellHide();
                    }
                    else
                    {
                        playtestService.TryPlaytestSellRawFish();
                    }
                }
                else
                {
                    playtestService.TryPlaytestBuyCordage();
                }
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        playtestService.TryPlaytestGrantHorseCurrency();
                        playtestService.TryPlaytestBuyHorse();
                        playtestService.TryPlaytestSummonHorse();
                        playtestService.TryPlaytestMountHorseShortcut();
                    }
                }
                else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    playtestService.TryGrantShelterCordage();
                }
                else
                {
                    playtestService.TryPlaytestBuyHatchet();
                }
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        playtestService.TryPlaytestFirearmFoundationShortcut();
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        playtestService.TryPlaytestMiningFoundationShortcut();
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        playtestService.TryGrantHomesteadSupplyCrateKit();
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        playtestService.TryPlaytestGrantWagonCurrency();
                        playtestService.TryPlaytestBuyWagon();
                        playtestService.TryPlaytestSummonWagon();
                        playtestService.TryPlaytestWagonFoundationShortcut();
                    }
                    else
                    {
                        playtestService.TryGrantHomesteadWorkbenchKit();
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        playtestService.TryGrantWoodForIndustry();
                        playtestService.TryProduceLumberAtSawTable();
                        playtestService.TryProduceCharcoalAtKiln();
                        playtestService.TryRefineIronAtForge();
                        playtestService.TryCraftIronHatchetHeadAtForge();
                        playtestService.TryGrantIronHatchetUpgrade();
                    }
                }
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

            GUILayout.Label("CCS Manual Playtest Harness (1.3.3)");
            GUILayout.Label("F10 HUD | F11 Advance | F12 Reset | F7 Death | F6 Knife/Spear | Shift+F6 Tool | V Buy | Shift+V Sell | Ctrl+V Fish | Ctrl+Alt+M Meat | Ctrl+B Bow | Ctrl+Alt+T Trap | Alt+T Trigger");
            GUILayout.Label("F2 crate | Shift+F2 bedroll/sleep | F1 deposit | Shift+F1 withdraw | F5 save | F9 load");
            GUILayout.Label("Interact gather/cook | Primary active use | F eat");

            if (CCS.Modules.Hotbar.CCS_ActiveItemRuntimeBridge.TryGetActiveItemService(
                    out CCS.Modules.Hotbar.CCS_ActiveItemService activeItemService)
                && activeItemService.IsInitialized)
            {
                if (activeItemService.ActiveState.HasActiveItem)
                {
                    GUILayout.Label(
                        $"Active: {activeItemService.ActiveState.ActiveItemId} ({activeItemService.ActiveState.BehaviorType})");
                }

                CCS.Modules.Hotbar.CCS_ActiveItemUseResult lastUse = activeItemService.LastUseResult;
                if (lastUse.ResultType != CCS.Modules.Hotbar.CCS_ActiveItemUseResultType.None)
                {
                    string targetSuffix = string.IsNullOrWhiteSpace(lastUse.TargetDisplayName)
                        ? string.Empty
                        : $" | Target: {lastUse.TargetDisplayName} ({lastUse.TargetTypeLabel})";
                    GUILayout.Label($"Last use: {lastUse.ResultType} — {lastUse.Message}{targetSuffix}");
                }
            }
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
