using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcSocialProfile
// CATEGORY: Modules / NPCs / Runtime / Social
// PURPOSE: Profile catalog for settlement social gathering areas and leisure behavior.
// PLACEMENT: Assets/CCS/Survival/Profiles/NPCs/Social/
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.0.0 — no AI conversations or relationship simulation.
// =============================================================================

namespace CCS.Modules.NPCs
{
    [CreateAssetMenu(
        fileName = "CCS_NpcSocialProfile",
        menuName = "CCS/Survival/NPCs/NPC Social Profile")]
    public sealed class CCS_NpcSocialProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_NpcSocialGatheringDefinition[] gatheringDefinitions =
            Array.Empty<CCS_NpcSocialGatheringDefinition>();

        [SerializeField] private float socialArrivalTolerance = 1.25f;

        [SerializeField] private bool requireSocialAnchorForLeisure = true;

        public CCS_NpcSocialGatheringDefinition[] GatheringDefinitions =>
            gatheringDefinitions ?? Array.Empty<CCS_NpcSocialGatheringDefinition>();

        public float SocialArrivalTolerance => socialArrivalTolerance;

        public bool RequireSocialAnchorForLeisure => requireSocialAnchorForLeisure;
    }
}
