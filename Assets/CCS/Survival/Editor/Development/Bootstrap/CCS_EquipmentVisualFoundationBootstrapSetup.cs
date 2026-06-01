using System.Collections.Generic;
using System.IO;
using CCS.Modules.Equipment;
using CCS.Survival.Player;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentVisualFoundationBootstrapSetup
// CATEGORY: Survival / Editor / Development / Bootstrap
// PURPOSE: Creates primitive equipment visuals, socket rig on player, and visual profile wiring.
// PLACEMENT: Batch entry for 1.2.0 primitive equipment visual foundation milestone.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Transform sockets only. Equipment state remains service-driven.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public static class CCS_EquipmentVisualFoundationBootstrapSetup
    {
        private const string PlayerPrefabPath = "Assets/CCS/Survival/Prefabs/Player/PF_CCS_Player.prefab";
        private const string VisualPrefabsRoot = "Assets/CCS/Survival/Prefabs/Equipment/Visuals";
        private const string VisualDefinitionsRoot = "Assets/CCS/Survival/Content/Equipment/Visuals";
        private const string VisualProfilePath =
            "Assets/CCS/Survival/Profiles/Equipment/CCS_DefaultEquipmentVisualProfile.asset";
        private const string LogPrefix = "[CCS_EquipmentVisualFoundationBootstrapSetup]";

        private static readonly (string Name, CCS_EquipmentAttachmentSocketType SocketType, Vector3 LocalPosition)[] SocketLayout =
        {
            ("Socket_RightHand", CCS_EquipmentAttachmentSocketType.RightHand, new Vector3(0.4f, 1.0f, 0.2f)),
            ("Socket_LeftHand", CCS_EquipmentAttachmentSocketType.LeftHand, new Vector3(-0.4f, 1.0f, 0.2f)),
            ("Socket_Back", CCS_EquipmentAttachmentSocketType.Back, new Vector3(0f, 1.25f, -0.35f)),
            ("Socket_LeftHip", CCS_EquipmentAttachmentSocketType.LeftHip, new Vector3(-0.42f, 0.85f, 0.15f)),
            ("Socket_RightHip", CCS_EquipmentAttachmentSocketType.RightHip, new Vector3(0.42f, 0.85f, 0.15f)),
            ("Socket_Chest", CCS_EquipmentAttachmentSocketType.Chest, new Vector3(0f, 1.3f, 0.25f)),
            ("Socket_Head", CCS_EquipmentAttachmentSocketType.Head, new Vector3(0f, 1.7f, 0.1f)),
            ("Socket_Backpack", CCS_EquipmentAttachmentSocketType.Backpack, new Vector3(0f, 1.2f, -0.45f))
        };

        public static void ExecuteBatch()
        {
            EnsureFolders();

            GameObject spearPrefab = CreateSpearVisualPrefab();
            GameObject hatchetPrefab = CreateHatchetVisualPrefab();
            GameObject pickPrefab = CreatePickVisualPrefab();
            GameObject torchPrefab = CreateTorchVisualPrefab();
            GameObject bedrollPrefab = CreateBedrollVisualPrefab();
            GameObject knifePrefab = CreateKnifeVisualPrefab();
            GameObject genericToolPrefab = CreateGenericToolVisualPrefab();

            List<CCS_EquipmentVisualDefinition> definitions = new List<CCS_EquipmentVisualDefinition>
            {
                CreateVisualDefinition(
                    "CCS_EquipmentVisual_Spear",
                    "ccs.survival.item.starter.spear",
                    spearPrefab,
                    CCS_EquipmentAttachmentSocketType.RightHand,
                    new Vector3(0f, 0f, 0.15f),
                    new Vector3(90f, 0f, 0f),
                    new Vector3(0.04f, 0.35f, 0.04f)),
                CreateVisualDefinition(
                    "CCS_EquipmentVisual_Knife",
                    "ccs.survival.item.starter.knife",
                    knifePrefab,
                    CCS_EquipmentAttachmentSocketType.RightHand,
                    new Vector3(0f, 0f, 0.05f),
                    new Vector3(0f, 90f, 0f),
                    Vector3.one * 0.8f),
                CreateVisualDefinition(
                    "CCS_EquipmentVisual_BoneHatchet",
                    "ccs.survival.item.tool.hatchet.bone",
                    hatchetPrefab,
                    CCS_EquipmentAttachmentSocketType.RightHand,
                    new Vector3(0f, 0f, 0.08f),
                    new Vector3(0f, 0f, 0f),
                    Vector3.one),
                CreateVisualDefinition(
                    "CCS_EquipmentVisual_HatchetPrimitive",
                    "ccs.survival.item.tool.hatchet.primitive",
                    hatchetPrefab,
                    CCS_EquipmentAttachmentSocketType.RightHand,
                    new Vector3(0f, 0f, 0.08f),
                    Vector3.zero,
                    Vector3.one),
                CreateVisualDefinition(
                    "CCS_EquipmentVisual_BonePick",
                    "ccs.survival.item.tool.pick.bone",
                    pickPrefab,
                    CCS_EquipmentAttachmentSocketType.RightHand,
                    new Vector3(0f, 0f, 0.08f),
                    Vector3.zero,
                    Vector3.one),
                CreateVisualDefinition(
                    "CCS_EquipmentVisual_PickPrimitive",
                    "ccs.survival.item.tool.pick.primitive",
                    pickPrefab,
                    CCS_EquipmentAttachmentSocketType.RightHand,
                    new Vector3(0f, 0f, 0.08f),
                    Vector3.zero,
                    Vector3.one),
                CreateVisualDefinition(
                    "CCS_EquipmentVisual_Torch",
                    "ccs.survival.item.progression.primitivetorch",
                    torchPrefab,
                    CCS_EquipmentAttachmentSocketType.LeftHand,
                    new Vector3(0f, 0f, 0.05f),
                    new Vector3(0f, 0f, 0f),
                    Vector3.one),
                CreateVisualDefinition(
                    "CCS_EquipmentVisual_Bedroll",
                    "ccs.survival.item.starter.bedroll",
                    bedrollPrefab,
                    CCS_EquipmentAttachmentSocketType.Backpack,
                    new Vector3(0f, 0f, 0f),
                    new Vector3(0f, 90f, 0f),
                    Vector3.one),
                CreateVisualDefinition(
                    "CCS_EquipmentVisual_GenericTool",
                    "ccs.survival.item.tool.generic.placeholder",
                    genericToolPrefab,
                    CCS_EquipmentAttachmentSocketType.RightHip,
                    Vector3.zero,
                    Vector3.zero,
                    Vector3.one)
            };

            CCS_EquipmentVisualProfile visualProfile = EnsureVisualProfile(definitions);

            if (!File.Exists(PlayerPrefabPath))
            {
                Debug.LogError($"{LogPrefix} Missing player prefab: {PlayerPrefabPath}");
                EditorApplication.Exit(1);
                return;
            }

            GameObject prefabContents = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            ConfigurePlayerEquipmentRig(prefabContents, visualProfile);
            PrefabUtility.SaveAsPrefabAsset(prefabContents, PlayerPrefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{LogPrefix} Primitive equipment visual foundation setup complete.");
            EditorApplication.Exit(0);
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(VisualPrefabsRoot);
            Directory.CreateDirectory(VisualDefinitionsRoot);
            Directory.CreateDirectory(Path.GetDirectoryName(VisualProfilePath) ?? string.Empty);
            Directory.CreateDirectory("Assets/CCS/Modules/Equipment/Runtime/Visuals");
        }

        private static void ConfigurePlayerEquipmentRig(GameObject playerRoot, CCS_EquipmentVisualProfile visualProfile)
        {
            Transform equipmentRigTransform = playerRoot.transform.Find("EquipmentRig");
            GameObject equipmentRigObject;
            if (equipmentRigTransform == null)
            {
                equipmentRigObject = new GameObject("EquipmentRig");
                equipmentRigObject.transform.SetParent(playerRoot.transform, false);
            }
            else
            {
                equipmentRigObject = equipmentRigTransform.gameObject;
            }

            CCS_EquipmentAttachmentRig rig =
                equipmentRigObject.GetComponent<CCS_EquipmentAttachmentRig>()
                ?? equipmentRigObject.AddComponent<CCS_EquipmentAttachmentRig>();

            for (int index = 0; index < SocketLayout.Length; index++)
            {
                (string socketName, CCS_EquipmentAttachmentSocketType socketType, Vector3 localPosition) = SocketLayout[index];
                Transform socketTransform = equipmentRigObject.transform.Find(socketName);
                GameObject socketObject;
                if (socketTransform == null)
                {
                    socketObject = new GameObject(socketName);
                    socketObject.transform.SetParent(equipmentRigObject.transform, false);
                }
                else
                {
                    socketObject = socketTransform.gameObject;
                }

                socketObject.transform.localPosition = localPosition;
                socketObject.transform.localRotation = Quaternion.identity;
                socketObject.transform.localScale = Vector3.one;

                CCS_EquipmentAttachmentSocket socketComponent =
                    socketObject.GetComponent<CCS_EquipmentAttachmentSocket>()
                    ?? socketObject.AddComponent<CCS_EquipmentAttachmentSocket>();

                SerializedObject serializedSocket = new SerializedObject(socketComponent);
                serializedSocket.FindProperty("socketType").enumValueIndex = (int)socketType;
                serializedSocket.ApplyModifiedPropertiesWithoutUndo();
            }

            rig.RebuildSocketCache();

            CCS_PlayerEquipmentVisualBinder binder =
                playerRoot.GetComponent<CCS_PlayerEquipmentVisualBinder>()
                ?? playerRoot.AddComponent<CCS_PlayerEquipmentVisualBinder>();

            SerializedObject serializedBinder = new SerializedObject(binder);
            serializedBinder.FindProperty("attachmentRig").objectReferenceValue = rig;
            serializedBinder.FindProperty("equipmentVisualProfile").objectReferenceValue = visualProfile;
            serializedBinder.ApplyModifiedPropertiesWithoutUndo();
        }

        private static CCS_EquipmentVisualProfile EnsureVisualProfile(List<CCS_EquipmentVisualDefinition> definitions)
        {
            CCS_EquipmentVisualProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_EquipmentVisualProfile>(VisualProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_EquipmentVisualProfile>();
                AssetDatabase.CreateAsset(profile, VisualProfilePath);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileId").stringValue =
                "ccs.survival.profile.equipment.visual.default";
            SerializedProperty definitionsProperty = serializedProfile.FindProperty("visualDefinitions");
            definitionsProperty.arraySize = definitions.Count;
            for (int index = 0; index < definitions.Count; index++)
            {
                definitionsProperty.GetArrayElementAtIndex(index).objectReferenceValue = definitions[index];
            }

            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static CCS_EquipmentVisualDefinition CreateVisualDefinition(
            string assetName,
            string itemId,
            GameObject visualPrefab,
            CCS_EquipmentAttachmentSocketType socketType,
            Vector3 localPosition,
            Vector3 localEuler,
            Vector3 localScale)
        {
            string assetPath = $"{VisualDefinitionsRoot}/{assetName}.asset";
            CCS_EquipmentVisualDefinition definition =
                AssetDatabase.LoadAssetAtPath<CCS_EquipmentVisualDefinition>(assetPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_EquipmentVisualDefinition>();
                AssetDatabase.CreateAsset(definition, assetPath);
            }

            SerializedObject serializedDefinition = new SerializedObject(definition);
            serializedDefinition.FindProperty("itemId").stringValue = itemId;
            serializedDefinition.FindProperty("visualPrefab").objectReferenceValue = visualPrefab;
            serializedDefinition.FindProperty("attachmentSocket").enumValueIndex = (int)socketType;
            serializedDefinition.FindProperty("localPositionOffset").vector3Value = localPosition;
            serializedDefinition.FindProperty("localEulerOffset").vector3Value = localEuler;
            serializedDefinition.FindProperty("localScale").vector3Value = localScale;
            serializedDefinition.FindProperty("hideWhenUnequipped").boolValue = true;
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static GameObject SaveVisualPrefab(string prefabName, System.Action<GameObject> buildRoot)
        {
            string prefabPath = $"{VisualPrefabsRoot}/{prefabName}.prefab";
            GameObject root = new GameObject(prefabName);
            buildRoot(root);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreateSpearVisualPrefab()
        {
            return SaveVisualPrefab("PF_CCS_Visual_Spear", root =>
            {
                GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                shaft.name = "Shaft";
                shaft.transform.SetParent(root.transform, false);
                shaft.transform.localScale = new Vector3(0.04f, 0.35f, 0.04f);
                shaft.transform.localPosition = new Vector3(0f, 0f, 0.15f);
                shaft.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

                GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tip.name = "Tip";
                tip.transform.SetParent(root.transform, false);
                tip.transform.localScale = new Vector3(0.06f, 0.08f, 0.06f);
                tip.transform.localPosition = new Vector3(0f, 0f, 0.42f);
                tip.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            });
        }

        private static GameObject CreateHatchetVisualPrefab()
        {
            return SaveVisualPrefab("PF_CCS_Visual_Hatchet", root =>
            {
                GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                handle.name = "Handle";
                handle.transform.SetParent(root.transform, false);
                handle.transform.localScale = new Vector3(0.03f, 0.18f, 0.03f);
                handle.transform.localPosition = new Vector3(0f, 0f, 0.05f);
                handle.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

                GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blade.name = "Blade";
                blade.transform.SetParent(root.transform, false);
                blade.transform.localScale = new Vector3(0.12f, 0.04f, 0.06f);
                blade.transform.localPosition = new Vector3(0.08f, 0f, 0.18f);
            });
        }

        private static GameObject CreatePickVisualPrefab()
        {
            return SaveVisualPrefab("PF_CCS_Visual_Pick", root =>
            {
                GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                handle.name = "Handle";
                handle.transform.SetParent(root.transform, false);
                handle.transform.localScale = new Vector3(0.03f, 0.2f, 0.03f);
                handle.transform.localPosition = new Vector3(0f, 0f, 0.05f);
                handle.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

                GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
                head.name = "Head";
                head.transform.SetParent(root.transform, false);
                head.transform.localScale = new Vector3(0.14f, 0.05f, 0.05f);
                head.transform.localPosition = new Vector3(0f, 0f, 0.22f);
                head.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
            });
        }

        private static GameObject CreateTorchVisualPrefab()
        {
            return SaveVisualPrefab("PF_CCS_Visual_Torch", root =>
            {
                GameObject stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stick.name = "Stick";
                stick.transform.SetParent(root.transform, false);
                stick.transform.localScale = new Vector3(0.03f, 0.14f, 0.03f);
                stick.transform.localPosition = new Vector3(0f, 0f, 0.04f);
                stick.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

                GameObject flameMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                flameMarker.name = "FlameMarker";
                flameMarker.transform.SetParent(root.transform, false);
                flameMarker.transform.localScale = Vector3.one * 0.08f;
                flameMarker.transform.localPosition = new Vector3(0f, 0f, 0.16f);

                Light placeholderLight = flameMarker.AddComponent<Light>();
                placeholderLight.type = LightType.Point;
                placeholderLight.range = 2f;
                placeholderLight.intensity = 0.35f;
                placeholderLight.enabled = false;
            });
        }

        private static GameObject CreateBedrollVisualPrefab()
        {
            return SaveVisualPrefab("PF_CCS_Visual_Bedroll", root =>
            {
                GameObject roll = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                roll.name = "Roll";
                roll.transform.SetParent(root.transform, false);
                roll.transform.localScale = new Vector3(0.18f, 0.08f, 0.08f);
                roll.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            });
        }

        private static GameObject CreateKnifeVisualPrefab()
        {
            return SaveVisualPrefab("PF_CCS_Visual_Knife", root =>
            {
                GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                handle.name = "Handle";
                handle.transform.SetParent(root.transform, false);
                handle.transform.localScale = new Vector3(0.025f, 0.05f, 0.025f);

                GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blade.name = "Blade";
                blade.transform.SetParent(root.transform, false);
                blade.transform.localScale = new Vector3(0.02f, 0.12f, 0.04f);
                blade.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            });
        }

        private static GameObject CreateGenericToolVisualPrefab()
        {
            return SaveVisualPrefab("PF_CCS_Visual_GenericTool", root =>
            {
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.name = "Body";
                body.transform.SetParent(root.transform, false);
                body.transform.localScale = new Vector3(0.08f, 0.08f, 0.2f);
            });
        }
    }
}
