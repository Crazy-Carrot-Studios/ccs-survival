using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentVisualValidationUtility
// CATEGORY: Modules / Equipment / Runtime / Validation
// PURPOSE: Validates equipment visual sockets, definitions, and player prefab rig wiring.
// PLACEMENT: Used by CCS_EquipmentValidationValidator and editor bootstrap checks.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Placeholder primitive visuals only. No IK or final art validation.
// =============================================================================

namespace CCS.Modules.Equipment
{
    public static class CCS_EquipmentVisualValidationUtility
    {
        private const string VisualsFolderPath = "Assets/CCS/Modules/Equipment/Runtime/Visuals";
        private const string VisualPrefabsFolderPath = "Assets/CCS/Survival/Prefabs/Equipment/Visuals";
        private const string VisualDefinitionsFolderPath = "Assets/CCS/Survival/Content/Equipment/Visuals";
        private const string DefaultVisualProfilePath =
            "Assets/CCS/Survival/Profiles/Equipment/CCS_DefaultEquipmentVisualProfile.asset";
        private const string PlayerPrefabPath = "Assets/CCS/Survival/Prefabs/Player/PF_CCS_Player.prefab";

        private static readonly CCS_EquipmentAttachmentSocketType[] RequiredSocketTypes =
        {
            CCS_EquipmentAttachmentSocketType.RightHand,
            CCS_EquipmentAttachmentSocketType.LeftHand,
            CCS_EquipmentAttachmentSocketType.Back,
            CCS_EquipmentAttachmentSocketType.LeftHip,
            CCS_EquipmentAttachmentSocketType.RightHip,
            CCS_EquipmentAttachmentSocketType.Chest,
            CCS_EquipmentAttachmentSocketType.Head,
            CCS_EquipmentAttachmentSocketType.Backpack
        };

        private static readonly string[] RequiredPrimitiveItemIds =
        {
            "ccs.survival.item.starter.spear",
            "ccs.survival.item.starter.knife",
            "ccs.survival.item.tool.hatchet.bone",
            "ccs.survival.item.tool.pick.bone",
            "ccs.survival.item.progression.primitivetorch",
            "ccs.survival.item.starter.bedroll"
        };

        public static CCS_SurvivalValidationResult ValidateVisualFoundationFolders()
        {
            if (!System.IO.Directory.Exists(VisualsFolderPath))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Missing equipment visuals runtime folder: {VisualsFolderPath}");
            }

            if (!System.IO.Directory.Exists(VisualPrefabsFolderPath))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Missing equipment visual prefab folder: {VisualPrefabsFolderPath}");
            }

            if (!System.IO.Directory.Exists(VisualDefinitionsFolderPath))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Missing equipment visual definition folder: {VisualDefinitionsFolderPath}");
            }

            return CCS_SurvivalValidationResult.Pass("Equipment visual foundation folders are present.");
        }

        public static CCS_SurvivalValidationResult ValidateSocketEnum()
        {
            foreach (CCS_EquipmentAttachmentSocketType socketType in RequiredSocketTypes)
            {
                if (!System.Enum.IsDefined(typeof(CCS_EquipmentAttachmentSocketType), socketType))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"CCS_EquipmentAttachmentSocketType is missing required value: {socketType}");
                }
            }

            return CCS_SurvivalValidationResult.Pass(
                "CCS_EquipmentAttachmentSocketType includes required socket values.");
        }

        public static CCS_SurvivalValidationResult ValidatePlayerAttachmentRig(GameObject playerPrefabRoot)
        {
            if (playerPrefabRoot == null)
            {
                return CCS_SurvivalValidationResult.Fail("Player prefab root is null.");
            }

            CCS_EquipmentAttachmentRig rig =
                playerPrefabRoot.GetComponentInChildren<CCS_EquipmentAttachmentRig>(true);
            if (rig == null)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "PF_CCS_Player is missing CCS_EquipmentAttachmentRig.");
            }

            rig.RebuildSocketCache();
            if (rig.HasDuplicateSocketTypes())
            {
                return CCS_SurvivalValidationResult.Fail(
                    "PF_CCS_Player equipment rig has duplicate socket types.");
            }

            for (int index = 0; index < RequiredSocketTypes.Length; index++)
            {
                CCS_EquipmentAttachmentSocketType requiredSocket = RequiredSocketTypes[index];
                if (!rig.TryGetSocket(requiredSocket, out _))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"PF_CCS_Player equipment rig is missing socket: {requiredSocket}.");
                }
            }

            return CCS_SurvivalValidationResult.Pass(
                "PF_CCS_Player includes a complete equipment attachment rig.");
        }

        public static CCS_SurvivalValidationResult ValidateVisualProfile(CCS_EquipmentVisualProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Missing equipment visual profile: {DefaultVisualProfilePath}");
            }

            HashSet<string> itemIds = new HashSet<string>();
            IReadOnlyList<CCS_EquipmentVisualDefinition> definitions = profile.VisualDefinitions;
            for (int index = 0; index < definitions.Count; index++)
            {
                CCS_EquipmentVisualDefinition definition = definitions[index];
                if (definition == null)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        "Equipment visual profile contains a null visual definition entry.");
                }

                if (definition.VisualPrefab == null)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Visual definition for '{definition.ItemId}' is missing a prefab reference.");
                }

                if (string.IsNullOrWhiteSpace(definition.ItemId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        "Equipment visual profile contains a definition with an empty item ID.");
                }

                if (!itemIds.Add(definition.ItemId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Duplicate equipment visual definition item ID: {definition.ItemId}");
                }
            }

            for (int index = 0; index < RequiredPrimitiveItemIds.Length; index++)
            {
                string requiredItemId = RequiredPrimitiveItemIds[index];
                bool found = false;
                for (int definitionIndex = 0; definitionIndex < definitions.Count; definitionIndex++)
                {
                    CCS_EquipmentVisualDefinition definition = definitions[definitionIndex];
                    if (definition != null && definition.ItemId == requiredItemId)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Missing required primitive equipment visual definition for item ID: {requiredItemId}");
                }
            }

            return CCS_SurvivalValidationResult.Pass(
                "Equipment visual profile references valid prefabs and required primitive items.");
        }

        public static CCS_SurvivalValidationResult ValidateVisualProfileSocketBindings(
            GameObject playerPrefabRoot,
            CCS_EquipmentVisualProfile profile)
        {
            if (playerPrefabRoot == null)
            {
                return CCS_SurvivalValidationResult.Fail("Player prefab root is null.");
            }

            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Missing equipment visual profile: {DefaultVisualProfilePath}");
            }

            CCS_EquipmentAttachmentRig rig =
                playerPrefabRoot.GetComponentInChildren<CCS_EquipmentAttachmentRig>(true);
            if (rig == null)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "PF_CCS_Player is missing CCS_EquipmentAttachmentRig for socket binding validation.");
            }

            rig.RebuildSocketCache();
            IReadOnlyList<CCS_EquipmentVisualDefinition> definitions = profile.VisualDefinitions;
            for (int index = 0; index < definitions.Count; index++)
            {
                CCS_EquipmentVisualDefinition definition = definitions[index];
                if (definition == null)
                {
                    continue;
                }

                if (!rig.TryGetSocket(definition.AttachmentSocket, out CCS_EquipmentAttachmentSocket socket)
                    || socket == null
                    || socket.SocketTransform == null)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Visual definition '{definition.ItemId}' references missing rig socket: {definition.AttachmentSocket}.");
                }
            }

            return CCS_SurvivalValidationResult.Pass(
                "All equipment visual definitions reference sockets present on the player rig.");
        }

        public static bool TryGetRequiredItemIdsForExistingEquipmentDefinitions(
            CCS_EquipmentProfile equipmentProfile,
            out List<string> requiredItemIds)
        {
            requiredItemIds = new List<string>();
            if (equipmentProfile?.SaveRestoreEquipmentDefinitions == null)
            {
                return false;
            }

            CCS_EquipmentItemDefinition[] definitions = equipmentProfile.SaveRestoreEquipmentDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_EquipmentItemDefinition equipmentDefinition = definitions[index];
                if (equipmentDefinition?.ItemDefinition == null)
                {
                    continue;
                }

                string itemId = equipmentDefinition.ItemDefinition.ItemId;
                if (!ShouldRequireEquipmentVisual(itemId) || requiredItemIds.Contains(itemId))
                {
                    continue;
                }

                requiredItemIds.Add(itemId);
            }

            return requiredItemIds.Count > 0;
        }

        public static CCS_SurvivalValidationResult ValidateVisualDefinitionsExistForEquipmentItems(
            CCS_EquipmentVisualProfile visualProfile,
            IReadOnlyList<string> equipmentItemIds)
        {
            if (visualProfile == null)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Missing equipment visual profile: {DefaultVisualProfilePath}");
            }

            if (equipmentItemIds == null || equipmentItemIds.Count == 0)
            {
                return CCS_SurvivalValidationResult.Pass(
                    "No equipment item IDs supplied for visual definition coverage check.");
            }

            CCS_EquipmentVisualDefinitionLookup lookup = visualProfile.BuildLookup();
            for (int index = 0; index < equipmentItemIds.Count; index++)
            {
                string itemId = equipmentItemIds[index];
                if (string.IsNullOrWhiteSpace(itemId))
                {
                    continue;
                }

                if (!lookup.TryGetDefinition(itemId, out _))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Equipment item '{itemId}' has no matching equipment visual definition.");
                }
            }

            return CCS_SurvivalValidationResult.Pass(
                "Equipment visual definitions exist for all registered equipment item IDs.");
        }

        public static string DefaultVisualProfileAssetPath => DefaultVisualProfilePath;

        public static string PlayerPrefabAssetPath => PlayerPrefabPath;

        private static bool ShouldRequireEquipmentVisual(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            if (itemId.Contains(".test.", System.StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return itemId.Contains(".starter.", System.StringComparison.OrdinalIgnoreCase)
                || itemId.Contains(".tool.", System.StringComparison.OrdinalIgnoreCase)
                || itemId.Contains(".progression.primitive", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
