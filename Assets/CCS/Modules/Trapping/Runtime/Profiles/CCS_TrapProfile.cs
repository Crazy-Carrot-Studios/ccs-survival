using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TrapProfile
// CATEGORY: Modules / Trapping / Runtime / Profiles
// PURPOSE: Catalog and tuning profile for frontier trap placement and capture.
// PLACEMENT: Assets/CCS/Survival/Profiles/Trapping/ (project shell configuration).
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Trapping
{
    [CreateAssetMenu(
        fileName = "CCS_TrapProfile",
        menuName = "CCS/Survival/Trapping/Trap Profile")]
    public sealed class CCS_TrapProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Trapping")]
        [SerializeField] private bool enableTrapping = true;

        [SerializeField] private List<CCS_TrapDefinition> trapDefinitions = new List<CCS_TrapDefinition>();

        [Header("Placement Preview")]
        [SerializeField] private Color validPreviewColor = new Color(0.2f, 0.85f, 0.35f, 0.45f);
        [SerializeField] private Color invalidPreviewColor = new Color(0.85f, 0.2f, 0.2f, 0.45f);

        #endregion

        #region Properties

        public bool EnableTrapping => enableTrapping;

        public IReadOnlyList<CCS_TrapDefinition> TrapDefinitions => trapDefinitions;

        public Color ValidPreviewColor => validPreviewColor;

        public Color InvalidPreviewColor => invalidPreviewColor;

        #endregion

        #region Public Methods

        public bool TryGetByTrapId(string trapDefinitionId, out CCS_TrapDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(trapDefinitionId) || trapDefinitions == null)
            {
                return false;
            }

            for (int index = 0; index < trapDefinitions.Count; index++)
            {
                CCS_TrapDefinition candidate = trapDefinitions[index];
                if (candidate != null && candidate.TrapDefinitionId == trapDefinitionId)
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetByPlaceableItemId(string itemId, out CCS_TrapDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(itemId) || trapDefinitions == null)
            {
                return false;
            }

            for (int index = 0; index < trapDefinitions.Count; index++)
            {
                CCS_TrapDefinition candidate = trapDefinitions[index];
                if (candidate?.PlaceableItem != null && candidate.PlaceableItem.ItemId == itemId)
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
