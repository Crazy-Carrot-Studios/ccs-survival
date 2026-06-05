using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PopulationPlaceholderActor
// CATEGORY: Modules / Settlements / Runtime / PopulationPresence
// PURPOSE: Idle primitive placeholder representing one workforce worker.
// PLACEMENT: Child of CCS_PopulationPresenceAnchor actor container.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.0.0 — no AI, movement, dialogue, or combat.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_PopulationPlaceholderActor : MonoBehaviour
    {
        [SerializeField] private CCS_SettlementPopulationCategory workforceCategory =
            CCS_SettlementPopulationCategory.Unknown;

        [SerializeField] private Color farmerColor = new Color(0.55f, 0.75f, 0.35f, 1f);

        [SerializeField] private Color rancherColor = new Color(0.65f, 0.5f, 0.35f, 1f);

        [SerializeField] private Color minerColor = new Color(0.5f, 0.5f, 0.55f, 1f);

        [SerializeField] private Color lumberColor = new Color(0.4f, 0.6f, 0.35f, 1f);

        [SerializeField] private Color merchantColor = new Color(0.85f, 0.7f, 0.3f, 1f);

        [SerializeField] private Color laborerColor = new Color(0.55f, 0.55f, 0.6f, 1f);

        public CCS_SettlementPopulationCategory WorkforceCategory => workforceCategory;

        public void Configure(CCS_SettlementPopulationCategory category)
        {
            workforceCategory = category;
            ApplyCategoryColor();
        }

        private void Awake()
        {
            ApplyCategoryColor();
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
    }
}
