using System;
using CCS.Modules.Wildlife;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CombatProfile
// CATEGORY: Modules / Combat / Runtime / Profiles
// PURPOSE: Tuning profile for primitive melee combat and wildlife hunting rules.
// PLACEMENT: Assets/CCS/Survival/Profiles/Combat/ (project shell configuration).
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No humanoid enemies, combos, or animation systems in 0.9.8 foundation.
// =============================================================================

namespace CCS.Modules.Combat
{
    [CreateAssetMenu(
        fileName = "CCS_CombatProfile",
        menuName = "CCS/Survival/Combat/Combat Profile")]
    public sealed class CCS_CombatProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Melee Attack")]
        [Tooltip("Seconds between primary melee attack attempts.")]
        [SerializeField] private float attackCooldownSeconds = 0.35f;

        [Tooltip("Sphere cast radius used for wildlife hit detection.")]
        [SerializeField] private float hitSphereRadius = 0.35f;

        [Tooltip("Physics layers included in melee wildlife sphere casts.")]
        [SerializeField] private LayerMask wildlifeHitLayers = ~0;

        [Header("Rabbit")]
        [SerializeField] private CCS_CombatWildlifeSpeciesSettings rabbitSettings = new CCS_CombatWildlifeSpeciesSettings
        {
            maxHealth = 20f,
            carcassObjectName = "CCS_TestRabbitCarcass",
            carcassPrimitive = PrimitiveType.Sphere,
            carcassLocalScale = new Vector3(0.5f, 0.5f, 0.5f)
        };

        [Header("Deer")]
        [SerializeField] private CCS_CombatWildlifeSpeciesSettings deerSettings = new CCS_CombatWildlifeSpeciesSettings
        {
            maxHealth = 50f,
            carcassObjectName = "CCS_TestDeerCarcass",
            carcassPrimitive = PrimitiveType.Capsule,
            carcassLocalScale = new Vector3(0.8f, 1.5f, 0.8f)
        };

        [Header("Wildlife Content")]
        [Tooltip("Default wildlife harvest profile assigned to spawned carcasses.")]
        [SerializeField] private CCS_WildlifeProfile wildlifeProfile;

        [Tooltip("Carcass wildlife definition spawned when a rabbit is killed.")]
        [SerializeField] private CCS_WildlifeDefinition rabbitCarcassDefinition;

        [Tooltip("Carcass wildlife definition spawned when a deer is killed.")]
        [SerializeField] private CCS_WildlifeDefinition deerCarcassDefinition;

        #endregion

        #region Properties

        public float AttackCooldownSeconds => attackCooldownSeconds;

        public float HitSphereRadius => hitSphereRadius;

        public LayerMask WildlifeHitLayers => wildlifeHitLayers;

        public CCS_WildlifeProfile WildlifeProfile => wildlifeProfile;

        public CCS_WildlifeDefinition RabbitCarcassDefinition => rabbitCarcassDefinition;

        public CCS_WildlifeDefinition DeerCarcassDefinition => deerCarcassDefinition;

        #endregion

        #region Public Methods

        [Header("Turkey")]
        [SerializeField] private CCS_CombatWildlifeSpeciesSettings turkeySettings = new CCS_CombatWildlifeSpeciesSettings
        {
            maxHealth = 25f,
            carcassObjectName = "CCS_TestTurkeyCarcass",
            carcassPrimitive = PrimitiveType.Sphere,
            carcassLocalScale = new Vector3(0.55f, 0.55f, 0.55f)
        };

        [Tooltip("Carcass wildlife definition spawned when a turkey is killed.")]
        [SerializeField] private CCS_WildlifeDefinition turkeyCarcassDefinition;

        public CCS_CombatWildlifeSpeciesSettings GetSpeciesSettings(CCS_WildlifeAiSpecies species)
        {
            switch (species)
            {
                case CCS_WildlifeAiSpecies.Deer:
                    return deerSettings;
                case CCS_WildlifeAiSpecies.Turkey:
                    return turkeySettings;
                default:
                    return rabbitSettings;
            }
        }

        public CCS_WildlifeDefinition GetCarcassDefinition(CCS_WildlifeAiSpecies species)
        {
            switch (species)
            {
                case CCS_WildlifeAiSpecies.Deer:
                    return deerCarcassDefinition;
                case CCS_WildlifeAiSpecies.Turkey:
                    return turkeyCarcassDefinition != null ? turkeyCarcassDefinition : rabbitCarcassDefinition;
                default:
                    return rabbitCarcassDefinition;
            }
        }

        #endregion
    }

    [Serializable]
    public struct CCS_CombatWildlifeSpeciesSettings
    {
        [Tooltip("Maximum health for living wildlife of this species.")]
        public float maxHealth;

        [Tooltip("Bootstrap carcass object name used as a template when spawning kills.")]
        public string carcassObjectName;

        [Tooltip("Primitive mesh used when spawning a fresh carcass.")]
        public PrimitiveType carcassPrimitive;

        [Tooltip("Local scale applied to spawned carcass primitives.")]
        public Vector3 carcassLocalScale;
    }
}
