using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerDeathProfile
// CATEGORY: Modules / PlayerDeath / Runtime / Profiles
// PURPOSE: Tuning profile for starvation/dehydration death and respawn recovery values.
// PLACEMENT: Assets/CCS/Survival/Profiles/PlayerDeath/CCS_DefaultPlayerDeathProfile.asset
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: No UI in 1.0.1. Death logs only.
// =============================================================================

namespace CCS.Modules.PlayerDeath
{
    [CreateAssetMenu(
        fileName = "CCS_PlayerDeathProfile",
        menuName = "CCS/Survival/Player Death/Player Death Profile")]
    public sealed class CCS_PlayerDeathProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Respawn Needs")]
        [Tooltip("Hunger restored after respawn.")]
        [SerializeField] private float respawnHunger = 50f;

        [Tooltip("Thirst restored after respawn.")]
        [SerializeField] private float respawnThirst = 50f;

        [Tooltip("Stamina restored after respawn.")]
        [SerializeField] private float respawnStamina = 100f;

        [Header("Respawn Point")]
        [Tooltip("Default spawn id used when no explicit respawn point is found.")]
        [SerializeField] private string defaultSpawnId = "ccs.survival.spawn.bootstrap";

        [Header("Diagnostics")]
        [Tooltip("Emit categorized death/respawn debug logs.")]
        [SerializeField] private bool enableDebugLogging;

        #endregion

        #region Properties

        public float RespawnHunger => respawnHunger;

        public float RespawnThirst => respawnThirst;

        public float RespawnStamina => respawnStamina;

        public string DefaultSpawnId => defaultSpawnId;

        public bool EnableDebugLogging => enableDebugLogging;

        #endregion
    }
}
