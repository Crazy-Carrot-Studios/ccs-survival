using System.Collections.Generic;
using CCS.Modules.CharacterController;
using CCS.Modules.NPCs;
using CCS.Survival.Player;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlaytestHud
// CATEGORY: Modules / Playtesting / Runtime / UI
// PURPOSE: Developer-only on-screen manual playtest checklist for bootstrap scenes.
// PLACEMENT: Bootstrap scene or PF_CCS_Survival_BootstrapRoot (PlaytestHarness child).
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Dev hotkeys route through CCS_DevHotkeyUtility (Input System keyboard reads).
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
            CCS_NpcServiceRepresentativeDebugHud.DrawIfVisible();
            CCS_NpcDialogueStubDebugHud.DrawIfVisible();
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
            if (CCS_DevHotkeyUtility.WasTogglePlaytestHudPressed())
            {
                hudVisible = !hudVisible;
            }

            if (CCS_DevHotkeyUtility.WasAdvancePlaytestStepPressed())
            {
                playtestService.AdvanceActiveStep();
            }

            if (CCS_DevHotkeyUtility.WasResetPlaytestStepsPressed())
            {
                playtestService.ResetSteps();
                spawnNotified = false;
                TryNotifySpawnReady();
            }

            if (CCS_DevHotkeyUtility.WasForceTestDeathPressed())
            {
                playtestService.ForceTestDeathCondition();
            }

            if (CCS_KeyboardInputUtility.WasKeyPressedThisFrame(KeyCode.F6))
            {
                if (CCS_KeyboardInputUtility.IsEitherShiftHeld())
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

            if (CCS_KeyboardInputUtility.WasKeyPressedThisFrame(KeyCode.Alpha1))
            {
                playtestService.TrySelectActiveFromMainHand();
            }

            if (CCS_KeyboardInputUtility.WasKeyPressedThisFrame(KeyCode.Alpha2))
            {
                playtestService.TrySelectActiveFromToolSlot();
            }

            if (CCS_DevHotkeyUtility.WasUnmodifiedPressed(KeyCode.B))
            {
                playtestService.TryPlacePlaytestFoundation();
            }

            if (CCS_KeyboardInputUtility.WasKeyPressedThisFrame(KeyCode.F4))
            {
                playtestService.TrySeedWorkbenchCraftingResources();
            }

            if (CCS_KeyboardInputUtility.WasKeyPressedThisFrame(KeyCode.F3))
            {
                playtestService.TryCraftWorkbenchPlaytestItem();
            }

            if (CCS_KeyboardInputUtility.WasKeyPressedThisFrame(KeyCode.F2))
            {
                if (CCS_KeyboardInputUtility.IsEitherShiftHeld())
                {
                    playtestService.TryPlaceOrSleepBedrollNearPlayer();
                }
                else
                {
                    playtestService.TryPlaceOrOpenStorageCrateNearPlayer();
                }
            }

            if (CCS_KeyboardInputUtility.WasKeyPressedThisFrame(KeyCode.F1))
            {
                if (CCS_KeyboardInputUtility.IsEitherShiftHeld())
                {
                    playtestService.TryMoveFirstStorageItemToPlayer();
                }
                else
                {
                    playtestService.TryMoveFirstPlayerItemToActiveStorageCrate();
                }
            }

            if (CCS_DevHotkeyUtility.WasControlAltPressed(KeyCode.M))
            {
                playtestService.TryPlaytestNpcMovementFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlAltPressed(KeyCode.S))
            {
                playtestService.TryPlaytestNpcScheduleFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlAltPressed(KeyCode.A))
            {
                playtestService.TryPlaytestNpcActivityFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlAltPressed(KeyCode.F))
            {
                playtestService.TryPlaytestNpcAffiliationFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlAltPressed(KeyCode.D))
            {
                playtestService.TryPlaytestNpcDialogueFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlAltPressed(KeyCode.P))
            {
                playtestService.TryPlaytestNpcSocialFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlAltPressed(KeyCode.E))
            {
                playtestService.TryPlaytestSettlementEventsFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlAltPressed(KeyCode.N))
            {
                playtestService.TryPlaytestSettlementNewsFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlAltPressed(KeyCode.C))
            {
                playtestService.TryPlaytestDynamicContractsFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlAltPressed(KeyCode.T))
            {
                playtestService.TryGrantPlaytestTrap();
            }
            else if (CCS_DevHotkeyUtility.WasAltPressed(KeyCode.T))
            {
                playtestService.TryForceTrapTriggerForPlaytest();
            }

            if (CCS_DevHotkeyUtility.WasControlPressed(KeyCode.B))
            {
                playtestService.TryGrantPlaytestBow();
            }

            if (CCS_DevHotkeyUtility.WasControlPressed(KeyCode.V))
            {
                playtestService.TryGrantPlaytestRawFish();
            }
            else if (CCS_DevHotkeyUtility.WasShiftPressed(KeyCode.V))
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
            else if (CCS_DevHotkeyUtility.WasUnmodifiedPressed(KeyCode.V))
            {
                playtestService.TryPlaytestBuyCordage();
            }

            if (CCS_DevHotkeyUtility.WasControlShiftPressed(KeyCode.H))
            {
                playtestService.TryPlaytestGrantHorseCurrency();
                playtestService.TryPlaytestBuyHorse();
                playtestService.TryPlaytestSummonHorse();
                playtestService.TryPlaytestMountHorseShortcut();
            }
            else if (CCS_DevHotkeyUtility.WasShiftPressed(KeyCode.H))
            {
                playtestService.TryGrantShelterCordage();
            }
            else if (CCS_DevHotkeyUtility.WasUnmodifiedPressed(KeyCode.H))
            {
                playtestService.TryPlaytestBuyHatchet();
            }

            if (CCS_DevHotkeyUtility.WasControlAltPressed(KeyCode.G))
            {
                playtestService.TryPlaytestFirearmFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlShiftPressed(KeyCode.G))
            {
                playtestService.TryPlaytestSettlementGrowthFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlShiftPressed(KeyCode.N))
            {
                playtestService.TryPlaytestMultiSettlementFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlShiftPressed(KeyCode.F))
            {
                playtestService.TryPlaytestTradeRoutesFreightShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlShiftPressed(KeyCode.Q))
            {
                playtestService.TryPlaytestRouteRiskFreightShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlShiftPressed(KeyCode.M))
            {
                playtestService.TryPlaytestMiningFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlShiftPressed(KeyCode.K))
            {
                playtestService.TryPlaytestPopulationFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlShiftPressed(KeyCode.J))
            {
                playtestService.TryPlaytestBusinessesFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlShiftPressed(KeyCode.V))
            {
                playtestService.TryPlaytestBusinessPresenceFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlShiftPressed(KeyCode.Z))
            {
                playtestService.TryPlaytestSettlementVisualGrowthFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlShiftPressed(KeyCode.X))
            {
                playtestService.TryPlaytestPopulationPresenceFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlShiftPressed(KeyCode.E))
            {
                playtestService.TryPlaytestNpcIdentityFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlAltPressed(KeyCode.R))
            {
                playtestService.TryPlaytestNpcServiceRepresentativeFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlAltPressed(KeyCode.H))
            {
                playtestService.TryPlaytestSettlementHousingFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlAltPressed(KeyCode.K))
            {
                playtestService.TryGrantHomesteadSupplyCrateKit();
            }

            if (CCS_DevHotkeyUtility.WasControlShiftPressed(KeyCode.R))
            {
                playtestService.TryPlaytestRanchFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlShiftPressed(KeyCode.P))
            {
                playtestService.TryPlaytestFarmingFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlShiftPressed(KeyCode.L))
            {
                playtestService.TryPlaytestLandOwnershipFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlShiftPressed(KeyCode.B))
            {
                playtestService.TryPlaytestBankingFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlShiftPressed(KeyCode.U))
            {
                playtestService.TryPlaytestUpkeepFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlShiftPressed(KeyCode.O))
            {
                playtestService.TryPlaytestLoansFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlShiftPressed(KeyCode.T))
            {
                playtestService.TryPlaytestReputationFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlShiftPressed(KeyCode.Y))
            {
                playtestService.TryPlaytestServiceAccessFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlShiftPressed(KeyCode.C))
            {
                playtestService.TryPlaytestContractsFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlShiftPressed(KeyCode.E))
            {
                playtestService.TryPlaytestRegionalEconomyFoundationShortcut();
            }

            if (CCS_DevHotkeyUtility.WasControlShiftPressed(KeyCode.W))
            {
                playtestService.TryPlaytestGrantWagonCurrency();
                playtestService.TryPlaytestBuyWagon();
                playtestService.TryPlaytestSummonWagon();
                playtestService.TryPlaytestWagonFoundationShortcut();
            }
            else if (CCS_DevHotkeyUtility.WasControlPressed(KeyCode.W))
            {
                playtestService.TryGrantHomesteadWorkbenchKit();
            }

            if (CCS_DevHotkeyUtility.WasControlShiftPressed(KeyCode.I))
            {
                playtestService.TryGrantWoodForIndustry();
                playtestService.TryProduceLumberAtSawTable();
                playtestService.TryProduceCharcoalAtKiln();
                playtestService.TryRefineIronAtForge();
                playtestService.TryCraftIronHatchetHeadAtForge();
                playtestService.TryGrantIronHatchetUpgrade();
            }
        }

        private void TryNotifySpawnReady()
        {
            if (spawnNotified)
            {
                return;
            }

            CCS_PlayerGameplayController[] players =
                Object.FindObjectsByType<CCS_PlayerGameplayController>();
            if (players == null || players.Length == 0)
            {
                return;
            }

            playtestService.NotifyBootstrapSessionReady();
            spawnNotified = true;
        }

        private void DrawHarnessPanel()
        {
            float panelHeight = Mathf.Min(Screen.height - PanelMargin * 2f, 560f);
            Rect panelRect = new Rect(
                Screen.width - PanelWidth - PanelMargin,
                PanelMargin,
                PanelWidth,
                panelHeight);

            GUI.Box(panelRect, GUIContent.none);
            GUILayout.BeginArea(new Rect(panelRect.x + 10f, panelRect.y + 10f, panelRect.width - 20f, panelRect.height - 20f));

            GUILayout.Label("CCS Manual Playtest Harness (1.7.2)");
            GUILayout.Label("F10 HUD | F11 Advance | F12 Reset | F7 Death | F6 Knife/Spear | Shift+F6 Tool");
            GUILayout.Label("V Buy | Shift+V Sell | Ctrl+V Fish | Ctrl+Alt+M Meat | Ctrl+B Bow | Ctrl+Alt+T Trap | Alt+T Trigger");
            GUILayout.Label("F2 crate | Shift+F2 bedroll/sleep | F1 deposit | Shift+F1 withdraw | F5 save | F9 load");
            GUILayout.Label("Interact gather/cook | Primary active use | F eat | R reload firearm");

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
            IReadOnlyList<CCS_PlaytestStepGroup> groups = CCS_PlaytestStepGroupingUtility.GetOrderedGroups();

            for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++)
            {
                CCS_PlaytestStepGroup group = groups[groupIndex];
                bool wroteHeader = false;

                for (int index = 0; index < states.Count; index++)
                {
                    CCS_PlaytestStepState state = states[index];
                    if (state == null
                        || CCS_PlaytestStepGroupingUtility.ResolveGroup(state.Definition.StepType) != group)
                    {
                        continue;
                    }

                    if (!wroteHeader)
                    {
                        GUILayout.Space(4f);
                        GUILayout.Label($"— {CCS_PlaytestStepGroupingUtility.GetDisplayName(group)} —");
                        wroteHeader = true;
                    }

                    string marker = GetStatusMarker(state.Status);
                    GUILayout.Label($"{marker} {index + 1}. {state.Definition.DisplayName}");
                }
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
