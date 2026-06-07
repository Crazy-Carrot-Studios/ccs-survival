using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PopulationPlaceholderActor
// CATEGORY: Modules / Settlements / Runtime / PopulationPresence
// PURPOSE: Idle primitive placeholder with NPC identity name, role, and label.
// PLACEMENT: Child of CCS_PopulationPresenceAnchor actor container.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.1.0 — identity via CCS_NpcRuntimeBridge; no AI or dialogue.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_PopulationPlaceholderActor : MonoBehaviour, CCS_IPopulationPlaceholderIdentityHost, CCS_INpcMovementHost
    {
        [SerializeField] private CCS_SettlementPopulationCategory workforceCategory =
            CCS_SettlementPopulationCategory.Unknown;

        [SerializeField] private string npcIdentityId = string.Empty;

        [SerializeField] private string displayName = string.Empty;

        [SerializeField] private int roleType;

        [SerializeField] private string roleDisplayName = string.Empty;

        [SerializeField] private string settlementId = string.Empty;

        [SerializeField] private string businessId = string.Empty;

        [SerializeField] private string anchorId = string.Empty;

        [SerializeField] private int slotIndex;

        [SerializeField] private bool isServiceRepresentative;

        [SerializeField] private string representativeTitle = string.Empty;

        [SerializeField] private string homeHousingId = string.Empty;

        [SerializeField] private float labelHeight = 1.35f;

        [SerializeField] private Color farmerColor = new Color(0.55f, 0.75f, 0.35f, 1f);

        [SerializeField] private Color rancherColor = new Color(0.65f, 0.5f, 0.35f, 1f);

        [SerializeField] private Color minerColor = new Color(0.5f, 0.5f, 0.55f, 1f);

        [SerializeField] private Color lumberColor = new Color(0.4f, 0.6f, 0.35f, 1f);

        [SerializeField] private Color merchantColor = new Color(0.85f, 0.7f, 0.3f, 1f);

        [SerializeField] private Color laborerColor = new Color(0.55f, 0.55f, 0.6f, 1f);

        public CCS_SettlementPopulationCategory WorkforceCategory => workforceCategory;

        public string NpcIdentityId => npcIdentityId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public int RoleType => roleType;

        public string RoleDisplayName => roleDisplayName ?? string.Empty;

        public string SettlementId => settlementId ?? string.Empty;

        public string BusinessId => businessId ?? string.Empty;

        public int WorkforceCategoryValue => (int)workforceCategory;

        public bool HasIdentity => !string.IsNullOrWhiteSpace(npcIdentityId) && !string.IsNullOrWhiteSpace(displayName);

        public bool IsServiceRepresentative => isServiceRepresentative;

        public string RepresentativeTitle => representativeTitle ?? string.Empty;

        public string WorkforceAnchorId => anchorId ?? string.Empty;

        public string HomeHousingId => homeHousingId ?? string.Empty;

        public Transform MovementTransform => transform;

        public void Configure(CCS_SettlementPopulationCategory category)
        {
            workforceCategory = category;
            ApplyCategoryColor();
            ApplyIdentityLabel();
        }

        public void BindAnchorContext(
            string anchorContextId,
            int actorSlotIndex,
            string anchorSettlementId,
            string anchorBusinessId)
        {
            anchorId = anchorContextId ?? string.Empty;
            slotIndex = actorSlotIndex;
            settlementId = anchorSettlementId ?? string.Empty;
            businessId = anchorBusinessId ?? string.Empty;
        }

        public void ApplyIdentityData(
            string identityId,
            string name,
            int resolvedRoleType,
            string resolvedRoleDisplayName,
            string anchorSettlementId,
            string anchorBusinessId,
            int resolvedWorkforceCategory,
            string assignedHomeHousingId = "")
        {
            npcIdentityId = identityId ?? string.Empty;
            displayName = name ?? string.Empty;
            roleType = resolvedRoleType;
            roleDisplayName = resolvedRoleDisplayName ?? string.Empty;
            settlementId = anchorSettlementId ?? string.Empty;
            businessId = anchorBusinessId ?? string.Empty;
            workforceCategory = (CCS_SettlementPopulationCategory)resolvedWorkforceCategory;
            homeHousingId = assignedHomeHousingId ?? string.Empty;
            ApplyCategoryColor();
            ApplyIdentityLabel();
        }

        public void ApplyServiceRepresentativePresentation(string title)
        {
            isServiceRepresentative = true;
            representativeTitle = title ?? string.Empty;
            ApplyIdentityLabel();
        }

        public void ClearServiceRepresentativePresentation()
        {
            isServiceRepresentative = false;
            representativeTitle = string.Empty;
            ApplyIdentityLabel();
        }

        public void RefreshIdentityFromBridge()
        {
            if (string.IsNullOrWhiteSpace(anchorId))
            {
                return;
            }

            CCS_PopulationPlaceholderIdentityBridge.TryAssignIdentity(
                this,
                anchorId,
                slotIndex,
                settlementId,
                (int)workforceCategory,
                businessId);
        }

        private void Awake()
        {
            ApplyCategoryColor();
            EnsureIdentityLabel();
        }

        private void OnEnable()
        {
            CCS_PopulationPlaceholderIdentityBridge.RegisterHost(this);
        }

        private void OnDisable()
        {
            CCS_PopulationPlaceholderIdentityBridge.UnregisterHost(this);
        }

        private void ApplyCategoryColor()
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            renderer.sharedMaterial.color = workforceCategory switch
            {
                CCS_SettlementPopulationCategory.Farmers => farmerColor,
                CCS_SettlementPopulationCategory.Ranchers => rancherColor,
                CCS_SettlementPopulationCategory.Miners => minerColor,
                CCS_SettlementPopulationCategory.LumberWorkers => lumberColor,
                CCS_SettlementPopulationCategory.Merchants => merchantColor,
                CCS_SettlementPopulationCategory.Laborers => laborerColor,
                _ => laborerColor
            };
        }

        private void ApplyIdentityLabel()
        {
            EnsureIdentityLabel();
            if (identityLabel == null)
            {
                return;
            }

            if (!HasIdentity)
            {
                identityLabel.text = workforceCategory.ToString();
                return;
            }

            if (isServiceRepresentative && !string.IsNullOrWhiteSpace(representativeTitle))
            {
                identityLabel.text = AppendScheduleDebugLine($"{displayName}\n{representativeTitle}");
                return;
            }

            if (!string.IsNullOrWhiteSpace(homeHousingId))
            {
                identityLabel.text = AppendScheduleDebugLine(
                    $"{displayName} — {roleDisplayName}\nHome: {homeHousingId}");
                return;
            }

            identityLabel.text = AppendScheduleDebugLine($"{displayName} — {roleDisplayName}\n{workforceCategory}");
        }

        private string AppendScheduleDebugLine(string baseLabel)
        {
            string scheduleLine = CCS_NpcScheduleLabelBridge.BuildScheduleDebugLine(settlementId, npcIdentityId);
            return string.IsNullOrWhiteSpace(scheduleLine) ? baseLabel : $"{baseLabel}\n{scheduleLine}";
        }

        private TextMesh identityLabel;

        private void EnsureIdentityLabel()
        {
            if (identityLabel != null)
            {
                return;
            }

            Transform existing = transform.Find("CCS_NpcIdentity_Label");
            GameObject labelObject = existing != null ? existing.gameObject : new GameObject("CCS_NpcIdentity_Label");
            if (existing == null)
            {
                labelObject.transform.SetParent(transform, false);
            }

            labelObject.transform.localPosition = new Vector3(0f, labelHeight, 0f);
            labelObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            identityLabel = labelObject.GetComponent<TextMesh>();
            if (identityLabel == null)
            {
                identityLabel = labelObject.AddComponent<TextMesh>();
            }

            identityLabel.anchor = TextAnchor.MiddleCenter;
            identityLabel.alignment = TextAlignment.Center;
            identityLabel.fontSize = 32;
            identityLabel.characterSize = 0.06f;
            identityLabel.color = new Color(0.92f, 0.94f, 0.98f, 1f);
        }
    }
}
