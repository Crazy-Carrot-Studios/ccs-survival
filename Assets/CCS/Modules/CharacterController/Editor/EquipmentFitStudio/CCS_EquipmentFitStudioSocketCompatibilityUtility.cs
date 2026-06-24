using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioSocketCompatibilityUtility
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Validates weapon/socket compatibility for Fit Studio preview spawning.
// PLACEMENT: Editor utility used by Equipment Fit Studio workflow.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Blocks silent preview failures for incompatible socket/weapon pairs.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public static class CCS_EquipmentFitStudioSocketCompatibilityUtility
    {
        public const string RevolverRequiredItemType = "weapon.revolver";

        public static readonly string[] RevolverRecommendedSocketIds =
        {
            CCS_EquipmentConstants.HolsterSocketRightHipId,
            CCS_EquipmentConstants.HandSocketRightId,
        };

        public static bool IsRevolverWeapon(string weaponId)
        {
            return weaponId == CCS_EquipmentConstants.RevolverM1879WeaponId;
        }

        public static bool IsRecommendedRevolverSocket(string socketId)
        {
            for (int i = 0; i < RevolverRecommendedSocketIds.Length; i++)
            {
                if (RevolverRecommendedSocketIds[i] == socketId)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool SocketAllowsItemType(CCS_EquipmentSocketAnchor anchor, string itemType)
        {
            if (anchor == null || string.IsNullOrEmpty(itemType))
            {
                return false;
            }

            IReadOnlyList<string> allowed = anchor.AllowedItemTypes;
            if (allowed == null || allowed.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < allowed.Count; i++)
            {
                if (allowed[i] == itemType)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CanPreviewRevolverOnSocket(
            CCS_EquipmentSocketAnchor anchor,
            string socketId,
            bool allowIncompatibleOverride,
            out string blockingReason)
        {
            blockingReason = string.Empty;
            if (anchor == null)
            {
                blockingReason = "Preview failed: selected socket transform not found on player.";
                return false;
            }

            if (SocketAllowsItemType(anchor, RevolverRequiredItemType))
            {
                return true;
            }

            if (allowIncompatibleOverride)
            {
                return true;
            }

            if (socketId == CCS_EquipmentConstants.HolsterSocketLeftHipId)
            {
                blockingReason =
                    "Left Hip does not allow weapon.revolver. Preview is blocked unless override is enabled.";
                return false;
            }

            blockingReason =
                "Preview failed: selected socket does not allow this weapon. "
                + "Use Right Hip Holster or Right Hand for revolver preview.";
            return false;
        }

        public static string GetSocketValidityLabel(CCS_EquipmentSocketAnchor anchor, string socketId)
        {
            if (anchor == null)
            {
                return "Missing socket on player";
            }

            if (SocketAllowsItemType(anchor, RevolverRequiredItemType))
            {
                return IsRecommendedRevolverSocket(socketId) ? "Valid for revolver" : "Allowed for revolver";
            }

            return "Not valid for revolver";
        }
    }
}
