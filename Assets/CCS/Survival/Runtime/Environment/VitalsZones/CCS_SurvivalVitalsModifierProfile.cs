using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalVitalsModifierProfile
// CATEGORY: Survival / Environment / VitalsZones
// PURPOSE: Optional ScriptableObject preset for vitals modifier zone tuning.
// PLACEMENT: Assets/CCS/Survival/Settings/Environment/ (create via Assets menu when needed).
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Inline zone values remain editable when no profile is assigned.
// =============================================================================

namespace CCS.Survival.Environment.VitalsZones
{
    [CreateAssetMenu(
        fileName = "CCS_SurvivalVitalsModifierProfile",
        menuName = "CCS/Survival/Vitals Modifier Profile")]
    public sealed class CCS_SurvivalVitalsModifierProfile : ScriptableObject
    {
        #region Variables

        [SerializeField] private CCS_SurvivalVitalsModifierType modifierType = CCS_SurvivalVitalsModifierType.HungerDrain;

        [SerializeField] private float ratePerSecond = 5f;

        [SerializeField] private float minVitalClamp = -1f;

        [SerializeField] private float maxVitalClamp = -1f;

        #endregion

        #region Properties

        public CCS_SurvivalVitalsModifierType ModifierType => modifierType;

        public float RatePerSecond => ratePerSecond;

        public float MinVitalClamp => minVitalClamp;

        public float MaxVitalClamp => maxVitalClamp;

        #endregion
    }
}
