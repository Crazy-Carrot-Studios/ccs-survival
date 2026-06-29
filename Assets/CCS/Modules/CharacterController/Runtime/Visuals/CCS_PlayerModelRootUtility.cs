// =============================================================================
// SCRIPT: CCS_PlayerModelRootUtility
// CATEGORY: Modules / CharacterController / Runtime / Visuals
// PURPOSE: Resolves the player presentation/model root transform on networked prefabs.
// PLACEMENT: Static utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: v0.7.6 prefers Model root; Legacy VisualRoot is supported during migration reads only.
// =============================================================================

using UnityEngine;

namespace CCS.Modules.CharacterController
{
    public static class CCS_PlayerModelRootUtility
    {
        public static Transform FindModelRoot(Transform playerRoot)
        {
            if (playerRoot == null)
            {
                return null;
            }

            Transform modelRoot = playerRoot.Find(CCS_EquipmentConstants.ModelRootObjectName);
            if (modelRoot != null)
            {
                return modelRoot;
            }

            return playerRoot.Find(CCS_EquipmentConstants.LegacyVisualRootObjectName);
        }
    }
}
