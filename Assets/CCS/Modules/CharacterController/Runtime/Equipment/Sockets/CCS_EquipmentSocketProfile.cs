using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentSocketProfile
// CATEGORY: Modules / CharacterController / Runtime / Equipment / Sockets
// PURPOSE: Groups the default equipment socket definitions for a character rig.
// PLACEMENT: ScriptableObject asset under Profiles/EquipmentSockets/.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.6 default profile contains six socket definitions.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [CreateAssetMenu(
        fileName = "CCS_DefaultEquipmentSocketProfile",
        menuName = "CCS/Character Controller/Equipment Socket Profile",
        order = 11)]
    public sealed class CCS_EquipmentSocketProfile : ScriptableObject
    {
        #region Variables

        [SerializeField] private List<CCS_EquipmentSocketDefinition> socketDefinitions = new List<CCS_EquipmentSocketDefinition>();

        #endregion

        #region Properties

        public IReadOnlyList<CCS_EquipmentSocketDefinition> SocketDefinitions => socketDefinitions;

        #endregion
    }
}
