using System.Collections.Generic;
using CCS.Modules.Equipment;
using CCS.Survival.Development;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_BootstrapScenePolishSetup
// CATEGORY: Survival / Editor / Development / Bootstrap
// PURPOSE: Organizes bootstrap test areas into readable zones with world-space labels.
// PLACEMENT: Batch entry for milestone 2.1.2 bootstrap scene polish.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Reparents existing content only; does not add gameplay systems.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public static class CCS_BootstrapScenePolishSetup
    {
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string SceneRootName = "CCS_BuildVerificationScene";
        private const string LogPrefix = "[CCS_BootstrapScenePolishSetup]";

        private readonly struct ZoneDefinition
        {
            public readonly string ZoneName;
            public readonly Vector3 AnchorPosition;
            public readonly string LabelText;
            public readonly Color LabelColor;
            public readonly string[] MemberNames;
            public readonly Vector3[] MemberWorldPositions;

            public ZoneDefinition(
                string zoneName,
                Vector3 anchorPosition,
                string labelText,
                Color labelColor,
                string[] memberNames,
                Vector3[] memberWorldPositions)
            {
                ZoneName = zoneName;
                AnchorPosition = anchorPosition;
                LabelText = labelText;
                LabelColor = labelColor;
                MemberNames = memberNames;
                MemberWorldPositions = memberWorldPositions;
            }
        }

        #region Public Methods

        public static void ExecuteBatch()
        {
            if (!System.IO.File.Exists(BootstrapScenePath))
            {
                Debug.LogError($"{LogPrefix} Missing bootstrap scene: {BootstrapScenePath}");
                EditorApplication.Exit(1);
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            Transform sceneRoot = FindSceneRoot();
            if (sceneRoot == null)
            {
                Debug.LogError($"{LogPrefix} Missing scene root '{SceneRootName}'.");
                EditorApplication.Exit(1);
                return;
            }

            ZoneDefinition[] zones = BuildZoneDefinitions();
            for (int i = 0; i < zones.Length; i++)
            {
                ApplyZone(sceneRoot, zones[i]);
            }

            DisableNoisyDefaultHarnesses(sceneRoot);
            EnsurePlayerSpawnReadable(sceneRoot);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                Debug.LogError($"{LogPrefix} Failed to save bootstrap scene.");
                EditorApplication.Exit(1);
                return;
            }

            AssetDatabase.SaveAssets();
            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);

            Debug.Log($"{LogPrefix} Bootstrap scene polish complete ({zones.Length} zones).");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static ZoneDefinition[] BuildZoneDefinitions()
        {
            return new[]
            {
                new ZoneDefinition(
                    "CCS_BootstrapZone_PlayerSpawn",
                    new Vector3(0f, 0f, 0f),
                    "Player Spawn",
                    new Color(0.85f, 0.95f, 1f),
                    new[]
                    {
                        "CCS_PlayerRespawnPoint_Bootstrap",
                        "CCS_BootstrapTestGround"
                    },
                    new[]
                    {
                        new Vector3(0f, 1f, 2f),
                        new Vector3(0f, 0f, 0f)
                    }),
                new ZoneDefinition(
                    "CCS_BootstrapZone_Resources",
                    new Vector3(-22f, 0f, 4f),
                    "Resources",
                    new Color(0.55f, 0.9f, 0.55f),
                    new[]
                    {
                        "CCS_GatheringTestArea",
                        "CCS_WorldResourceTestArea",
                        "CCS_FrontierResourceTestArea",
                        "CCS_FrontierProspectingTestArea",
                        "CCS_WildlifeTestArea",
                        "CCS_FishingTestArea",
                        "CCS_ResourceHarvestingTestHarness"
                    },
                    new[]
                    {
                        new Vector3(-18f, 0f, 0f),
                        new Vector3(-18f, 0f, 6f),
                        new Vector3(-18f, 0f, 12f),
                        new Vector3(-18f, 0f, -6f),
                        new Vector3(-26f, 0f, 0f),
                        new Vector3(-26f, 0f, 8f),
                        new Vector3(-22f, 0f, 4f)
                    }),
                new ZoneDefinition(
                    "CCS_BootstrapZone_TradingPost",
                    new Vector3(28f, 0f, 18f),
                    "Trading Post",
                    new Color(1f, 0.85f, 0.45f),
                    new[]
                    {
                        "CCS_TestTradingPost",
                        "CCS_TestGeneralStore"
                    },
                    new[]
                    {
                        new Vector3(28f, 0f, 18f),
                        new Vector3(32f, 0f, 18f)
                    }),
                new ZoneDefinition(
                    "CCS_BootstrapZone_Homestead",
                    new Vector3(-22f, 0f, -14f),
                    "Homestead / Shelter",
                    new Color(0.75f, 0.65f, 0.45f),
                    new[]
                    {
                        "CCS_SleepTestArea",
                        "CCS_BuildingTestArea",
                        "CCS_CampfireTestArea",
                        "CCS_TestShelterVolume",
                        "CCS_TestBedrollRestPoint"
                    },
                    new[]
                    {
                        new Vector3(-26f, 0f, -12f),
                        new Vector3(-18f, 0f, -12f),
                        new Vector3(-10f, 0f, -12f),
                        new Vector3(-22f, 0f, -16f),
                        new Vector3(-24f, 0f, -10f)
                    }),
                new ZoneDefinition(
                    "CCS_BootstrapZone_Industry",
                    new Vector3(12f, 0f, -16f),
                    "Industry / Crafting",
                    new Color(0.65f, 0.75f, 0.95f),
                    new[]
                    {
                        "CCS_TestWorkbench",
                        "CCS_CraftingTestHarness"
                    },
                    new[]
                    {
                        new Vector3(12f, 0f, -16f),
                        new Vector3(8f, 0f, -18f)
                    }),
                new ZoneDefinition(
                    "CCS_BootstrapZone_Ranching",
                    new Vector3(-22f, 0f, 20f),
                    "Ranching (vendor + build)",
                    new Color(0.95f, 0.75f, 0.55f),
                    new string[0],
                    new Vector3[0]),
                new ZoneDefinition(
                    "CCS_BootstrapZone_HorseWagon",
                    new Vector3(20f, 0f, 28f),
                    "Horse / Wagon",
                    new Color(0.8f, 0.8f, 0.95f),
                    new[]
                    {
                        "CCS_TestFrontierSalvageWagon"
                    },
                    new[]
                    {
                        new Vector3(20f, 0f, 28f)
                    }),
                new ZoneDefinition(
                    "CCS_BootstrapZone_Firearms",
                    new Vector3(34f, 0f, 18f),
                    "Firearms (Gunsmith)",
                    new Color(0.95f, 0.55f, 0.55f),
                    new[]
                    {
                        "CCS_TestTradingPost_Gunsmith"
                    },
                    new[]
                    {
                        new Vector3(34f, 0.5f, 18f)
                    }),
                new ZoneDefinition(
                    "CCS_BootstrapZone_Regions",
                    new Vector3(0f, 0f, 0f),
                    "Regions",
                    new Color(0.7f, 0.55f, 0.95f),
                    new[]
                    {
                        "CCS_RegionVolume_PineRidgeForest",
                        "CCS_RegionVolume_BrokenCreek",
                        "CCS_RegionVolume_IronRidgeMine",
                        "CCS_RegionVolume_FrontierTradingPost"
                    },
                    new[]
                    {
                        new Vector3(-20f, 1f, -20f),
                        new Vector3(-36f, 1f, 0f),
                        new Vector3(46f, 1f, 36f),
                        new Vector3(36f, 1f, 24f)
                    }),
                new ZoneDefinition(
                    "CCS_BootstrapZone_DevHarnesses",
                    new Vector3(0f, 0f, -28f),
                    "Dev Harnesses",
                    new Color(0.55f, 0.55f, 0.55f),
                    new[]
                    {
                        "CCS_InventoryEquipmentPersistenceTestHarness",
                        "CCS_PrimitiveToolEquipTestHarness",
                        "CCS_SleepTestHarness",
                        "CCS_SaveLoadDebugController"
                    },
                    new[]
                    {
                        new Vector3(0f, 0f, -28f),
                        new Vector3(4f, 0f, -28f),
                        new Vector3(-4f, 0f, -28f),
                        new Vector3(8f, 0f, -28f)
                    })
            };
        }

        private static void ApplyZone(Transform sceneRoot, ZoneDefinition zone)
        {
            Transform zoneTransform = EnsureZoneRoot(sceneRoot, zone);
            EnsureZoneLabel(zoneTransform, zone);

            if (zone.MemberNames == null || zone.MemberNames.Length == 0)
            {
                return;
            }

            for (int i = 0; i < zone.MemberNames.Length; i++)
            {
                string memberName = zone.MemberNames[i];
                if (string.IsNullOrWhiteSpace(memberName))
                {
                    continue;
                }

                Transform member = FindMemberTransform(sceneRoot, memberName);
                if (member == null)
                {
                    Debug.LogWarning($"{LogPrefix} Missing bootstrap object '{memberName}' for zone '{zone.ZoneName}'.");
                    continue;
                }

                Vector3 targetWorldPosition = i < zone.MemberWorldPositions.Length
                    ? zone.MemberWorldPositions[i]
                    : zone.AnchorPosition;

                member.SetParent(zoneTransform, true);
                member.position = targetWorldPosition;
                EditorUtility.SetDirty(member.gameObject);
            }
        }

        private static Transform EnsureZoneRoot(Transform sceneRoot, ZoneDefinition zone)
        {
            Transform existing = sceneRoot.Find(zone.ZoneName);
            GameObject zoneObject = existing != null ? existing.gameObject : new GameObject(zone.ZoneName);
            if (existing == null)
            {
                zoneObject.transform.SetParent(sceneRoot, false);
            }

            zoneObject.transform.position = zone.AnchorPosition;
            zoneObject.transform.rotation = Quaternion.identity;
            return zoneObject.transform;
        }

        private static void EnsureZoneLabel(Transform zoneTransform, ZoneDefinition zone)
        {
            CCS_BootstrapZoneLabel label = zoneTransform.GetComponent<CCS_BootstrapZoneLabel>();
            if (label == null)
            {
                label = zoneTransform.gameObject.AddComponent<CCS_BootstrapZoneLabel>();
            }

            label.ConfigureLabel(zone.LabelText, zone.LabelColor);
            EditorUtility.SetDirty(label);
        }

        private static Transform FindMemberTransform(Transform sceneRoot, string objectName)
        {
            Transform direct = sceneRoot.Find(objectName);
            if (direct != null)
            {
                return direct;
            }

            Transform[] children = sceneRoot.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].name == objectName)
                {
                    return children[i];
                }
            }

            GameObject found = GameObject.Find(objectName);
            return found != null ? found.transform : null;
        }

        private static void DisableNoisyDefaultHarnesses(Transform sceneRoot)
        {
            CCS_InventoryEquipmentPersistenceTestHarness[] persistenceHarnesses =
                sceneRoot.GetComponentsInChildren<CCS_InventoryEquipmentPersistenceTestHarness>(true);

            for (int i = 0; i < persistenceHarnesses.Length; i++)
            {
                SerializedObject serialized = new SerializedObject(persistenceHarnesses[i]);
                serialized.FindProperty("enableHarness").boolValue = false;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(persistenceHarnesses[i]);
            }
        }

        private static void EnsurePlayerSpawnReadable(Transform sceneRoot)
        {
            Transform spawnPoint = FindMemberTransform(sceneRoot, "CCS_PlayerRespawnPoint_Bootstrap");
            if (spawnPoint != null)
            {
                spawnPoint.position = new Vector3(0f, 1f, 2f);
            }
        }

        private static Transform FindSceneRoot()
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == SceneRootName)
                {
                    return roots[i].transform;
                }
            }

            return null;
        }

        #endregion
    }
}
