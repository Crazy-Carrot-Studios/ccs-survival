using CCS.Modules.CharacterController;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ContractDebugHud
// CATEGORY: Modules / Contracts / Runtime / UI
// PURPOSE: Temporary debug panel for contract accept/complete until final UI exists.
// PLACEMENT: Auto-created at runtime; opened from settlement contract board routes.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.0.0 frontier contracts foundation.
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
            s_lastSummary =
                $"{result.Message} | dollars +{result.TradeDollarsGranted} | rep +{result.ReputationGainApplied} | "
                + $"prosperity +{result.ProsperityGainApplied:0.0} | supply +{result.SupplyAmountApplied:0.0} | {settlementId}";
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
            const float height = 420f;
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
                CCS_ContractDefinition[] contracts = contractService.GetSettlementBoardContracts(s_settlementId);
                DrawBoardSection(contractService, "Local supply", contracts, CCS_ContractBoardSectionKind.LocalSupply);
                DrawBoardSection(contractService, "Outbound freight", contracts, CCS_ContractBoardSectionKind.OutboundFreight);
                DrawBoardSection(
                    contractService,
                    "Inbound freight delivery",
                    contracts,
                    CCS_ContractBoardSectionKind.InboundFreight);
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

                if (!drewHeader)
                {
                    GUILayout.Label(sectionTitle, GUI.skin.box);
                    drewHeader = true;
                }

                DrawContractRow(contractService, definition, s_settlementId);
            }
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
            string settlementId)
        {
            if (definition == null)
            {
                return;
            }

            CCS_ContractState state = contractService.GetContractState(definition.ContractId);
            string routeLabel = definition.IsFreightContract
                ? $" {definition.FreightSourceSettlementId} -> {definition.FreightDestinationSettlementId}"
                : string.Empty;
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{definition.DisplayName}{routeLabel} [{state}]", GUILayout.Width(300f));
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
    }
}
