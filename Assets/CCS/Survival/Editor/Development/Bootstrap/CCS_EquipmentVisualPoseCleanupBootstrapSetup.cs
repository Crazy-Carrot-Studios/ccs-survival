using System.Collections.Generic;
using System.IO;
using CCS.Modules.Equipment;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentVisualPoseCleanupBootstrapSetup
// CATEGORY: Survival / Editor / Development / Bootstrap
// PURPOSE: Tunes equipment socket transforms and visual definition offsets for 1.2.1 held-item pose.
// PLACEMENT: Batch entry for 1.2.1 socket and offset cleanup milestone.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Capsule-relative sockets only. Does not recreate visual prefabs or alter equipment services.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public static class CCS_EquipmentVisualPoseCleanupBootstrapSetup
    {
        private const string PlayerPrefabPath = "Assets/CCS/Survival/Prefabs/Player/PF_CCS_Player.prefab";
        private const string VisualDefinitionsRoot = "Assets/CCS/Survival/Content/Equipment/Visuals";
        private const string LogPrefix = "[CCS_EquipmentVisualPoseCleanupBootstrapSetup]";

        private static readonly (string Name, CCS_EquipmentAttachmentSocketType SocketType, Vector3 LocalPosition, Vector3 LocalEuler)[]
            SocketLayout =
            {
                ("Socket_RightHand", CCS_EquipmentAttachmentSocketType.RightHand, new Vector3(0.34f, 0.92f, 0.26f), new Vector3(12f, 0f, 0f)),
                ("Socket_LeftHand", CCS_EquipmentAttachmentSocketType.LeftHand, new Vector3(-0.34f, 0.92f, 0.26f), new Vector3(12f, 0f, 0f)),
                ("Socket_Back", CCS_EquipmentAttachmentSocketType.Back, new Vector3(0f, 1.18f, -0.32f), new Vector3(8f, 0f, 0f)),
                ("Socket_LeftHip", CCS_EquipmentAttachmentSocketType.LeftHip, new Vector3(-0.38f, 0.78f, 0.1f), new Vector3(0f, 0f, 12f)),
                ("Socket_RightHip", CCS_EquipmentAttachmentSocketType.RightHip, new Vector3(0.38f, 0.78f, 0.1f), new Vector3(0f, 0f, -12f)),
                ("Socket_Chest", CCS_EquipmentAttachmentSocketType.Chest, new Vector3(0f, 1.28f, 0.22f), Vector3.zero),
                ("Socket_Head", CCS_EquipmentAttachmentSocketType.Head, new Vector3(0f, 1.68f, 0.08f), Vector3.zero),
                ("Socket_Backpack", CCS_EquipmentAttachmentSocketType.Backpack, new Vector3(0f, 1.22f, -0.42f), new Vector3(6f, 0f, 0f))
            };

        public static void ExecuteBatch()
        {
            if (!File.Exists(PlayerPrefabPath))
            {
                Debug.LogError($"{LogPrefix} Missing player prefab: {PlayerPrefabPath}");
                EditorApplication.Exit(1);
                return;
            }

            GameObject prefabContents = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            ApplySocketPose(prefabContents);
            PrefabUtility.SaveAsPrefabAsset(prefabContents, PlayerPrefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);

            ApplyVisualDefinitionOffsets();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{LogPrefix} Equipment socket pose and visual offset cleanup complete.");
            EditorApplication.Exit(0);
        }

        private static void ApplySocketPose(GameObject playerRoot)
        {
            Transform equipmentRigTransform = playerRoot.transform.Find("EquipmentRig");
            if (equipmentRigTransform == null)
            {
                Debug.LogError($"{LogPrefix} PF_CCS_Player is missing EquipmentRig.");
                return;
            }

            for (int index = 0; index < SocketLayout.Length; index++)
            {
                (string socketName, CCS_EquipmentAttachmentSocketType socketType, Vector3 localPosition, Vector3 localEuler) =
                    SocketLayout[index];

                Transform socketTransform = equipmentRigTransform.Find(socketName);
                if (socketTransform == null)
                {
                    Debug.LogWarning($"{LogPrefix} Missing socket transform: {socketName}");
                    continue;
                }

                socketTransform.localPosition = localPosition;
                socketTransform.localRotation = Quaternion.Euler(localEuler);
                socketTransform.localScale = Vector3.one;

                CCS_EquipmentAttachmentSocket socketComponent =
                    socketTransform.GetComponent<CCS_EquipmentAttachmentSocket>();
                if (socketComponent != null)
                {
                    SerializedObject serializedSocket = new SerializedObject(socketComponent);
                    serializedSocket.FindProperty("socketType").enumValueIndex = (int)socketType;
                    serializedSocket.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            CCS_EquipmentAttachmentRig rig = equipmentRigTransform.GetComponent<CCS_EquipmentAttachmentRig>();
            rig?.RebuildSocketCache();
        }

        private static void ApplyVisualDefinitionOffsets()
        {
            ApplyDefinitionOffsets(
                "CCS_EquipmentVisual_Spear",
                new Vector3(0.02f, -0.01f, 0.04f),
                new Vector3(90f, 0f, 0f),
                Vector3.one);
            ApplyDefinitionOffsets(
                "CCS_EquipmentVisual_Knife",
                new Vector3(0.03f, 0.01f, 0.05f),
                new Vector3(0f, 90f, -12f),
                Vector3.one * 0.75f);
            ApplyDefinitionOffsets(
                "CCS_EquipmentVisual_BoneHatchet",
                new Vector3(0.02f, -0.02f, 0.06f),
                new Vector3(0f, 0f, -18f),
                Vector3.one * 0.9f);
            ApplyDefinitionOffsets(
                "CCS_EquipmentVisual_HatchetPrimitive",
                new Vector3(0.02f, -0.02f, 0.06f),
                new Vector3(0f, 0f, -18f),
                Vector3.one * 0.9f);
            ApplyDefinitionOffsets(
                "CCS_EquipmentVisual_BonePick",
                new Vector3(0.02f, -0.03f, 0.07f),
                new Vector3(0f, 0f, 14f),
                Vector3.one * 0.9f);
            ApplyDefinitionOffsets(
                "CCS_EquipmentVisual_PickPrimitive",
                new Vector3(0.02f, -0.03f, 0.07f),
                new Vector3(0f, 0f, 14f),
                Vector3.one * 0.9f);
            ApplyDefinitionOffsets(
                "CCS_EquipmentVisual_Torch",
                new Vector3(0f, 0.02f, 0.05f),
                new Vector3(72f, 0f, 0f),
                Vector3.one);
            ApplyDefinitionOffsets(
                "CCS_EquipmentVisual_Bedroll",
                new Vector3(0f, -0.04f, 0f),
                new Vector3(0f, 90f, 12f),
                Vector3.one);
            ApplyDefinitionOffsets(
                "CCS_EquipmentVisual_GenericTool",
                new Vector3(0f, 0f, 0.03f),
                new Vector3(0f, 0f, -8f),
                Vector3.one * 0.85f);
        }

        private static void ApplyDefinitionOffsets(
            string assetName,
            Vector3 localPosition,
            Vector3 localEuler,
            Vector3 localScale)
        {
            string assetPath = $"{VisualDefinitionsRoot}/{assetName}.asset";
            CCS_EquipmentVisualDefinition definition =
                AssetDatabase.LoadAssetAtPath<CCS_EquipmentVisualDefinition>(assetPath);
            if (definition == null)
            {
                Debug.LogWarning($"{LogPrefix} Missing visual definition: {assetPath}");
                return;
            }

            SerializedObject serializedDefinition = new SerializedObject(definition);
            serializedDefinition.FindProperty("localPositionOffset").vector3Value = localPosition;
            serializedDefinition.FindProperty("localEulerOffset").vector3Value = localEuler;
            serializedDefinition.FindProperty("localScale").vector3Value = localScale;
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
        }
    }
}
