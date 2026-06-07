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
    public sealed class CCS_PopulationPlaceholderActor : MonoBehaviour,
        CCS_IPopulationPlaceholderIdentityHost, CCS_INpcMovementHost, CCS_INpcPresentationHost
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

        [SerializeField] private float activityIndicatorHeight = 1.85f;

        [SerializeField] private float activityIndicatorScale = 0.18f;

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

        public void RefreshPresentation()
        {
            ApplyIdentityLabel();
            ApplyActivityIndicator();
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
                identityLabel.text = BuildLabelText($"{displayName}\n{representativeTitle}");
                ApplyActivityIndicator();
                return;
            }

            identityLabel.text = BuildLabelText($"{displayName}\n{roleDisplayName}");
            ApplyActivityIndicator();
        }

        private string BuildLabelText(string baseLabel)
        {
            string socialLine = CCS_NpcSocialLabelBridge.BuildSocialDisplayLine(settlementId, npcIdentityId);
            if (!string.IsNullOrWhiteSpace(socialLine))
            {
                baseLabel = $"{baseLabel}\n{socialLine}";
            }
            else
            {
                string settlementLine =
                    CCS_NpcAffiliationLabelBridge.BuildSettlementDisplayLine(settlementId, npcIdentityId);
                if (!string.IsNullOrWhiteSpace(settlementLine))
                {
                    baseLabel = $"{baseLabel}\n{settlementLine}";
                }
            }

            if (!string.IsNullOrWhiteSpace(homeHousingId)
                && string.IsNullOrWhiteSpace(
                    CCS_NpcAffiliationLabelBridge.BuildAffiliationDebugLine(settlementId, npcIdentityId)))
            {
                return AppendDebugLine($"{baseLabel}\nHome: {homeHousingId}");
            }

            return AppendDebugLine(baseLabel);
        }

        private string AppendDebugLine(string baseLabel)
        {
            string affiliationDebugLine =
                CCS_NpcAffiliationLabelBridge.BuildAffiliationDebugLine(settlementId, npcIdentityId);
            string affiliationDetailLine =
                CCS_NpcAffiliationLabelBridge.BuildAffiliationDetailDebugLine(settlementId, npcIdentityId);
            string activityDebugLine = CCS_NpcActivityLabelBridge.BuildActivityDebugLine(settlementId, npcIdentityId);
            string socialDebugLine = CCS_NpcSocialLabelBridge.BuildSocialDebugLine(settlementId, npcIdentityId);

            string debugBlock = string.Empty;
            if (!string.IsNullOrWhiteSpace(affiliationDebugLine))
            {
                debugBlock = affiliationDebugLine;
            }

            if (!string.IsNullOrWhiteSpace(affiliationDetailLine))
            {
                debugBlock = string.IsNullOrWhiteSpace(debugBlock)
                    ? affiliationDetailLine
                    : $"{debugBlock}\n{affiliationDetailLine}";
            }

            if (!string.IsNullOrWhiteSpace(activityDebugLine))
            {
                debugBlock = string.IsNullOrWhiteSpace(debugBlock)
                    ? activityDebugLine
                    : $"{debugBlock}\n{activityDebugLine}";
            }

            if (!string.IsNullOrWhiteSpace(socialDebugLine))
            {
                debugBlock = string.IsNullOrWhiteSpace(debugBlock)
                    ? socialDebugLine
                    : $"{debugBlock}\n{socialDebugLine}";
            }

            if (string.IsNullOrWhiteSpace(debugBlock))
            {
                string scheduleLine = CCS_NpcScheduleLabelBridge.BuildScheduleDebugLine(settlementId, npcIdentityId);
                return string.IsNullOrWhiteSpace(scheduleLine) ? baseLabel : $"{baseLabel}\n{scheduleLine}";
            }

            return $"{baseLabel}\n{debugBlock}";
        }

        private void ApplyActivityIndicator()
        {
            EnsureActivityIndicator();
            if (activityIndicatorRenderer == null)
            {
                return;
            }

            string activityLine = CCS_NpcActivityLabelBridge.BuildActivityDisplayLine(settlementId, npcIdentityId);
            if (string.IsNullOrWhiteSpace(activityLine))
            {
                activityIndicatorRenderer.enabled = false;
                return;
            }

            activityIndicatorRenderer.enabled = true;
            activityIndicatorRenderer.sharedMaterial.color = ResolveActivityIndicatorColor(activityLine);
        }

        private Color ResolveActivityIndicatorColor(string activityLine)
        {
            return activityLine switch
            {
                "Serving" => new Color(0.95f, 0.75f, 0.25f, 1f),
                "Working" => new Color(0.35f, 0.75f, 0.95f, 1f),
                "Traveling" => new Color(0.85f, 0.55f, 0.2f, 1f),
                "Resting" => new Color(0.55f, 0.65f, 0.85f, 1f),
                "Sleeping" => new Color(0.45f, 0.4f, 0.75f, 1f),
                "Leisure" => new Color(0.45f, 0.85f, 0.55f, 1f),
                _ => new Color(0.7f, 0.7f, 0.75f, 1f)
            };
        }

        private Renderer activityIndicatorRenderer;

        private void EnsureActivityIndicator()
        {
            if (activityIndicatorRenderer != null)
            {
                return;
            }

            Transform existing = transform.Find("CCS_NpcActivity_Indicator");
            GameObject indicatorObject = existing != null ? existing.gameObject : GameObject.CreatePrimitive(PrimitiveType.Cube);
            indicatorObject.name = "CCS_NpcActivity_Indicator";
            if (existing == null)
            {
                indicatorObject.transform.SetParent(transform, false);
                Collider collider = indicatorObject.GetComponent<Collider>();
                if (collider != null)
                {
                    Destroy(collider);
                }
            }

            indicatorObject.transform.localPosition = new Vector3(0f, activityIndicatorHeight, 0f);
            indicatorObject.transform.localScale = Vector3.one * activityIndicatorScale;
            activityIndicatorRenderer = indicatorObject.GetComponent<Renderer>();
            if (activityIndicatorRenderer != null)
            {
                activityIndicatorRenderer.enabled = false;
            }
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
