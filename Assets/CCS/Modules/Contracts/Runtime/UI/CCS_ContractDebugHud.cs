using CCS.Modules.CharacterController;
using CCS.Modules.Settlements;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ContractDebugHud
// CATEGORY: Modules / Contracts / Runtime / UI
// PURPOSE: Temporary debug panel for contract accept/complete until final UI exists.
// PLACEMENT: Auto-created at runtime; opened from settlement contract board routes.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.5.0 — freight route risk and reward preview on debug contract board.
// =============================================================================

namespace CCS.Modules.Contracts
{
    public sealed class CCS_ContractDebugHud : MonoBehaviour
    {
        private static bool s_showPanel;
        private static string s_boardTitle = "Contract Board";
        private static string s_settlementId = string.Empty;
        private static CCS_ContractType s_boardType = CCS_ContractType.TradingPostSupply;
        private static bool s_useSettlementBoard;
        private static string s_lastSummary = "Last contract action: none";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (FindAnyObjectByType<CCS_ContractDebugHud>() != null)
            {
                return;
            }

            GameObject host = new GameObject("CCS_ContractDebugHud");
            host.AddComponent<CCS_ContractDebugHud>();
            DontDestroyOnLoad(host);
        }

        public static void ShowBoard(string boardTitle, string settlementId, CCS_ContractType boardType)
        {
            s_boardTitle = string.IsNullOrWhiteSpace(boardTitle) ? "Contract Board" : boardTitle;
            s_settlementId = settlementId ?? string.Empty;
            s_boardType = boardType;
            s_useSettlementBoard = false;
            s_showPanel = true;
        }

        public static void ShowSettlementBoard(string boardTitle, string settlementId)
        {
            s_boardTitle = string.IsNullOrWhiteSpace(boardTitle) ? "Contract Board" : boardTitle;
            s_settlementId = settlementId ?? string.Empty;
            s_useSettlementBoard = true;
            s_showPanel = true;
        }

        public static void HidePanel()
        {
            s_showPanel = false;
        }

        public static void NotifyContractAccepted(CCS_ContractDefinition definition, string settlementId)
        {
            if (definition == null)
            {
                return;
            }

            s_lastSummary = $"Accepted: {definition.DisplayName} at {settlementId}";
        }

        public static void NotifyContractCompleted(
            CCS_ContractCompletionResult result,
            CCS_ContractDefinition definition,
            string settlementId)
        {
            if (result == null)
            {
                s_lastSummary = "Last contract action: null result";
                return;
            }

            string label = definition != null ? definition.DisplayName : result.ContractId;
            if (result.HasFreightRewardBreakdown)
            {
                s_lastSummary =
                    $"{result.Message} | route {result.LinkedTradeRouteId} ({result.RouteRiskLevel}) | "
                    + $"base {result.BaseTradeDollarsReward} -> final {result.TradeDollarsGranted} | "
                    + $"mult route x{result.RouteRewardMultiplier:0.##} risk x{result.RiskRewardMultiplier:0.##} | "
                    + $"rep +{result.ReputationGainApplied} | {settlementId}";
            }
            else
            {
                s_lastSummary =
                    $"{result.Message} | dollars +{result.TradeDollarsGranted} | rep +{result.ReputationGainApplied} | "
                    + $"prosperity +{result.ProsperityGainApplied:0.0} | supply +{result.SupplyAmountApplied:0.0} | {settlementId}";
            }
        }

        private void Update()
        {
            if (!s_showPanel)
            {
                return;
            }

            if (CCS_DevHotkeyUtility.WasCloseVendorDebugPanelPressed())
            {
                HidePanel();
            }
        }

        private void OnGUI()
        {
            if (!s_showPanel)
            {
                return;
            }

            const float width = 460f;
            const float height = 480f;
            Rect panel = new Rect(Screen.width - width - 20f, 20f, width, height);
            GUI.Box(panel, GUIContent.none);
            GUILayout.BeginArea(new Rect(panel.x + 10f, panel.y + 10f, panel.width - 20f, panel.height - 20f));

            GUILayout.Label(s_boardTitle);
            GUILayout.Label($"Settlement: {s_settlementId}");
            GUILayout.Label($"Board type: {s_boardType}");
            GUILayout.Label(s_lastSummary, GUI.skin.box);

            if (!CCS_ContractRuntimeBridge.TryGetContractService(out CCS_ContractService contractService))
            {
                GUILayout.Label("Contract service unavailable.");
            }
            else if (s_useSettlementBoard)
            {
                DrawRecentNewsSection(s_settlementId);
                CCS_ContractDefinition[] contracts = contractService.GetSettlementBoardContracts(s_settlementId);
                DrawBoardSection(contractService, "Local supply", contracts, CCS_ContractBoardSectionKind.LocalSupply);
                DrawBoardSection(contractService, "Outbound freight", contracts, CCS_ContractBoardSectionKind.OutboundFreight);
                DrawBoardSection(
                    contractService,
                    "Inbound freight delivery",
                    contracts,
                    CCS_ContractBoardSectionKind.InboundFreight);
                DrawGeneratedDynamicSection(contractService, s_settlementId);
            }
            else
            {
                CCS_ContractDefinition[] contracts =
                    contractService.GetBoardContracts(s_settlementId, s_boardType);
                for (int index = 0; index < contracts.Length; index++)
                {
                    DrawContractRow(contractService, contracts[index], s_settlementId);
                }
            }

            if (GUILayout.Button("Close contract board"))
            {
                HidePanel();
            }

            GUILayout.EndArea();
        }

        private static void DrawRecentNewsSection(string settlementId)
        {
            if (string.IsNullOrWhiteSpace(settlementId))
            {
                return;
            }

            GUILayout.Label("Recent News", GUI.skin.box);
            if (!CCS_SettlementNewsRuntimeBridge.TryGetRecentNews(settlementId, 3, out CCS_SettlementNewsEntry[] entries)
                || entries == null
                || entries.Length == 0)
            {
                GUILayout.Label("No active settlement news.");
                return;
            }

            for (int index = 0; index < entries.Length; index++)
            {
                CCS_SettlementNewsEntry entry = entries[index];
                if (entry == null || string.IsNullOrWhiteSpace(entry.Headline))
                {
                    continue;
                }

                GUILayout.Label($"- {entry.Headline}", GUI.skin.box);
            }
        }

        private enum CCS_ContractBoardSectionKind
        {
            LocalSupply = 0,
            OutboundFreight = 1,
            InboundFreight = 2
        }

        private static void DrawBoardSection(
            CCS_ContractService contractService,
            string sectionTitle,
            CCS_ContractDefinition[] contracts,
            CCS_ContractBoardSectionKind sectionKind)
        {
            bool drewHeader = false;
            for (int index = 0; index < contracts.Length; index++)
            {
                CCS_ContractDefinition definition = contracts[index];
                if (!ShouldDrawInSection(contractService, definition, sectionKind))
                {
                    continue;
                }

                if (CCS_DynamicContractValidationUtility.IsGeneratedContractId(definition.ContractId))
                {
                    continue;
                }

                if (!drewHeader)
                {
                    GUILayout.Label(sectionTitle, GUI.skin.box);
                    drewHeader = true;
                }

                DrawContractRow(contractService, definition, s_settlementId);
            }
        }

        private static void DrawGeneratedDynamicSection(CCS_ContractService contractService, string settlementId)
        {
            CCS_ContractDefinition[] contracts = contractService.GetSettlementBoardContracts(settlementId);
            bool drewHeader = false;
            for (int index = 0; index < contracts.Length; index++)
            {
                CCS_ContractDefinition definition = contracts[index];
                if (definition == null
                    || !CCS_DynamicContractValidationUtility.IsGeneratedContractId(definition.ContractId))
                {
                    continue;
                }

                if (!drewHeader)
                {
                    GUILayout.Label("Generated dynamic contracts", GUI.skin.box);
                    drewHeader = true;
                }

                DrawGeneratedContractRow(contractService, definition, settlementId);
            }
        }

        private static void DrawGeneratedContractRow(
            CCS_ContractService contractService,
            CCS_ContractDefinition definition,
            string settlementId)
        {
            if (definition == null)
            {
                return;
            }

            string debugSuffix = string.Empty;
            if (CCS_DynamicContractRuntimeBridge.TryGetSettlementSnapshots(
                    settlementId,
                    out CCS_DynamicContractSnapshot[] snapshots)
                && snapshots != null)
            {
                for (int index = 0; index < snapshots.Length; index++)
                {
                    CCS_DynamicContractSnapshot snapshot = snapshots[index];
                    if (snapshot == null
                        || !string.Equals(
                            snapshot.GeneratedContractId,
                            definition.ContractId,
                            System.StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    debugSuffix =
                        $" | rule {snapshot.SourceRuleId} | expires day {snapshot.ExpirationDay}";
                    if (!string.IsNullOrWhiteSpace(snapshot.NewsHeadlineReference))
                    {
                        debugSuffix += $" | news: {snapshot.NewsHeadlineReference}";
                    }

                    break;
                }
            }

            DrawContractRow(contractService, definition, settlementId, debugSuffix);
        }

        private static bool ShouldDrawInSection(
            CCS_ContractService contractService,
            CCS_ContractDefinition definition,
            CCS_ContractBoardSectionKind sectionKind)
        {
            if (definition == null || contractService == null)
            {
                return false;
            }

            switch (sectionKind)
            {
                case CCS_ContractBoardSectionKind.LocalSupply:
                    return !definition.IsFreightContract;
                case CCS_ContractBoardSectionKind.OutboundFreight:
                    if (!definition.IsFreightContract)
                    {
                        return false;
                    }

                    CCS_ContractState outboundState = contractService.GetContractState(definition.ContractId);
                    return string.Equals(
                               definition.FreightSourceSettlementId,
                               s_settlementId,
                               System.StringComparison.OrdinalIgnoreCase)
                        && outboundState != CCS_ContractState.Completed;
                case CCS_ContractBoardSectionKind.InboundFreight:
                    if (!definition.IsFreightContract)
                    {
                        return false;
                    }

                    CCS_ContractState inboundState = contractService.GetContractState(definition.ContractId);
                    return string.Equals(
                               definition.FreightDestinationSettlementId,
                               s_settlementId,
                               System.StringComparison.OrdinalIgnoreCase)
                        && inboundState == CCS_ContractState.Accepted;
                default:
                    return false;
            }
        }

        private static void DrawContractRow(
            CCS_ContractService contractService,
            CCS_ContractDefinition definition,
            string settlementId,
            string debugSuffix = "")
        {
            if (definition == null)
            {
                return;
            }

            CCS_ContractState state = contractService.GetContractState(definition.ContractId);
            string routeLabel = definition.IsFreightContract
                ? $" {definition.FreightSourceSettlementId} -> {definition.FreightDestinationSettlementId}"
                : string.Empty;
            if (definition.IsFreightContract)
            {
                DrawFreightRouteRewardInfo(definition);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(
                $"{definition.DisplayName}{routeLabel}{debugSuffix} [{state}]",
                GUILayout.Width(300f));
            if (state == CCS_ContractState.Available
                && definition.CanAcceptAtSettlement(settlementId)
                && GUILayout.Button("Accept", GUILayout.Width(70f)))
            {
                CCS_ContractCompletionResult result =
                    contractService.TryAcceptContract(definition.ContractId, settlementId);
                s_lastSummary = result.Message;
            }

            bool canComplete = state == CCS_ContractState.Accepted
                && (!definition.IsFreightContract || definition.CanCompleteAtSettlement(settlementId));
            if (canComplete && GUILayout.Button("Complete", GUILayout.Width(70f)))
            {
                CCS_ContractCompletionResult result = definition.IsFreightContract
                    ? contractService.TryCompleteContract(definition.ContractId, settlementId)
                    : contractService.TryCompleteContract(definition.ContractId);
                NotifyContractCompleted(result, definition, settlementId);
            }

            GUILayout.EndHorizontal();
        }

        private static void DrawFreightRouteRewardInfo(CCS_ContractDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            CCS_TradeRouteService tradeRouteService = null;
            CCS_TradeRouteRuntimeBridge.TryGetTradeRouteService(out tradeRouteService);

            CCS_TradeRouteFreightRewardBreakdown breakdown =
                CCS_TradeRouteRewardModifierUtility.TryCalculateForLinkedRoute(
                    definition.Reward.TradeDollars,
                    definition.LinkedTradeRouteId,
                    tradeRouteService);

            int usageCount = 0;
            bool hasUsage = tradeRouteService != null
                && tradeRouteService.IsInitialized
                && !string.IsNullOrWhiteSpace(definition.LinkedTradeRouteId)
                && tradeRouteService.TryGetUsageCount(definition.LinkedTradeRouteId, out usageCount);

            string usageLabel = hasUsage ? usageCount.ToString() : "n/a";
            GUILayout.Label(
                $"Route: {breakdown.LinkedRouteId} | Risk: {breakdown.RiskLevel} | Base: {breakdown.BaseTradeDollars} | "
                + $"Final: {breakdown.FinalTradeDollars} | Mult: route x{breakdown.RouteMultiplier:0.##} "
                + $"risk x{breakdown.RiskMultiplier:0.##} | Usage: {usageLabel}");
        }
    }
}
