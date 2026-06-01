using System.IO;
using CCS.Modules.Hotbar;
using CCS.Survival.Composition;
using CCS.Survival.Player;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ActiveItemFoundationBootstrapSetup
// CATEGORY: Survival / Editor / Development / Bootstrap
// PURPOSE: Creates default active item profile and wires bootstrap/player prefab references.
// PLACEMENT: Batch entry for 1.2.2 active item foundation milestone.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public static class CCS_ActiveItemFoundationBootstrapSetup
    {
        private const string ProfileFolder = "Assets/CCS/Survival/Profiles/Hotbar";
        private const string ProfilePath = ProfileFolder + "/CCS_DefaultActiveItemProfile.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string PlayerPrefabPath = "Assets/CCS/Survival/Prefabs/Player/PF_CCS_Player.prefab";
        private const string LogPrefix = "[CCS_ActiveItemFoundationBootstrapSetup]";

        public static void ExecuteBatch()
        {
            Directory.CreateDirectory(ProfileFolder);

            CCS_ActiveItemProfile profile = AssetDatabase.LoadAssetAtPath<CCS_ActiveItemProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_ActiveItemProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileId").stringValue = "ccs.survival.profile.activeitem.default";
            serializedProfile.FindProperty("profileDisplayName").stringValue = "Default Active Item";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Active item selection and use flow foundation for milestone 1.2.2.";
            serializedProfile.FindProperty("profileVersion").stringValue = "1.2.2";
            serializedProfile.FindProperty("useCooldownSeconds").floatValue = 0f;
            serializedProfile.FindProperty("autoSelectMainHandOnEquip").boolValue = true;
            serializedProfile.FindProperty("enableEquipmentSlotCycling").boolValue = true;
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);

            WireBootstrapHostProfile(profile);
            WirePlayerPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{LogPrefix} Active item foundation bootstrap complete.");
            EditorApplication.Exit(0);
        }

        private static void WireBootstrapHostProfile(CCS_ActiveItemProfile profile)
        {
            if (!File.Exists(BootstrapPrefabPath))
            {
                Debug.LogWarning($"{LogPrefix} Missing bootstrap prefab: {BootstrapPrefabPath}");
                return;
            }

            GameObject prefabContents = PrefabUtility.LoadPrefabContents(BootstrapPrefabPath);
            CCS_SurvivalGameplayServiceHost host =
                prefabContents.GetComponentInChildren<CCS_SurvivalGameplayServiceHost>(true);
            if (host == null)
            {
                Debug.LogWarning($"{LogPrefix} Bootstrap prefab missing CCS_SurvivalGameplayServiceHost.");
            }
            else
            {
                SerializedObject serializedHost = new SerializedObject(host);
                serializedHost.FindProperty("activeItemProfile").objectReferenceValue = profile;
                serializedHost.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(prefabContents, BootstrapPrefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);
        }

        private static void WirePlayerPrefab()
        {
            if (!File.Exists(PlayerPrefabPath))
            {
                Debug.LogWarning($"{LogPrefix} Missing player prefab: {PlayerPrefabPath}");
                return;
            }

            GameObject prefabContents = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            if (prefabContents.GetComponent<CCS_PlayerActiveItemDriver>() == null)
            {
                prefabContents.AddComponent<CCS_PlayerActiveItemDriver>();
            }

            PrefabUtility.SaveAsPrefabAsset(prefabContents, PlayerPrefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);
        }
    }
}
