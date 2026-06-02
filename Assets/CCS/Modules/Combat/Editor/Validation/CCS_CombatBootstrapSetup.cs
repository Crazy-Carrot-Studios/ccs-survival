using System.IO;
using CCS.Modules.Combat;
using CCS.Modules.Equipment;
using CCS.Modules.Inventory;
using CCS.Modules.Wildlife;
using CCS.Survival.Composition;
using CCS.Survival.Player;
using CCS.Survival.Player.Loadout;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_CombatBootstrapSetup
// CATEGORY: Modules / Combat / Editor / Validation
// PURPOSE: Creates combat profile, weapon melee stats, spear equipment, and bootstrap wiring.
// PLACEMENT: Batch entry for 0.9.8 primitive combat foundation milestone.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Spear remains regression content; starter loadout is frontier-themed (1.2.6).
// =============================================================================

namespace CCS.Modules.Combat.Editor
{
    public static class CCS_CombatBootstrapSetup
    {
        private const string ProfilesRoot = "Assets/CCS/Survival/Profiles/Combat";
        private const string DefaultProfilePath = ProfilesRoot + "/CCS_DefaultCombatProfile.asset";
        private const string KnifeItemPath = "Assets/CCS/Survival/Content/Items/Starter/CCS_Item_Knife.asset";
        private const string SpearItemPath = "Assets/CCS/Survival/Content/Items/Starter/CCS_Item_Spear.asset";
        private const string SpearEquipmentPath = "Assets/CCS/Survival/Content/Equipment/Primitive/CCS_Equipment_Spear.asset";
        private const string WildlifeProfilePath = "Assets/CCS/Survival/Profiles/Wildlife/CCS_DefaultWildlifeProfile.asset";
        private const string RabbitCarcassDefinitionPath =
            "Assets/CCS/Survival/Content/Wildlife/Definitions/CCS_TestRabbit.asset";
        private const string DeerCarcassDefinitionPath =
            "Assets/CCS/Survival/Content/Wildlife/Definitions/CCS_TestDeerCarcass.asset";
        private const string StarterLoadoutPath =
            "Assets/CCS/Survival/Profiles/StarterLoadout/CCS_DefaultStarterLoadoutProfile.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string PlayerPrefabPath = "Assets/CCS/Survival/Prefabs/Player/PF_CCS_Player.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string LogPrefix = "[CCS_CombatBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureFolders();
            CCS_WildlifeProfile wildlifeProfile = LoadRequiredAsset<CCS_WildlifeProfile>(WildlifeProfilePath);
            CCS_WildlifeDefinition rabbitCarcass = LoadRequiredAsset<CCS_WildlifeDefinition>(RabbitCarcassDefinitionPath);
            CCS_WildlifeDefinition deerCarcass = LoadRequiredAsset<CCS_WildlifeDefinition>(DeerCarcassDefinitionPath);
            CCS_ItemDefinition knifeItem = UpdateKnifeItem();
            CCS_ItemDefinition spearItem = UpdateSpearItem();
            CCS_EquipmentItemDefinition spearEquipment = EnsureSpearEquipment(spearItem);
            EnsureStarterLoadoutExcludesSpear(spearItem);
            CCS_CombatProfile combatProfile = EnsureDefaultProfile(wildlifeProfile, rabbitCarcass, deerCarcass);
            EnsureBootstrapPrefabProfile(combatProfile);
            EnsurePlayerCombatDriver();
            EnsureLivingWildlifeDamageable();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Combat bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/CCS/Survival/Profiles/Combat"))
            {
                AssetDatabase.CreateFolder("Assets/CCS/Survival/Profiles", "Combat");
            }
        }

        private static T LoadRequiredAsset<T>(string assetPath) where T : Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null)
            {
                Debug.LogError($"{LogPrefix} Missing required asset: {assetPath}");
                EditorApplication.Exit(1);
            }

            return asset;
        }

        private static CCS_ItemDefinition UpdateKnifeItem()
        {
            CCS_ItemDefinition knife = LoadRequiredAsset<CCS_ItemDefinition>(KnifeItemPath);
            SerializedObject serializedItem = new SerializedObject(knife);
            serializedItem.FindProperty("meleeDamage").floatValue = 8f;
            serializedItem.FindProperty("meleeRange").floatValue = 1.8f;
            serializedItem.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(knife);
            return knife;
        }

        private static CCS_ItemDefinition UpdateSpearItem()
        {
            CCS_ItemDefinition spear = LoadRequiredAsset<CCS_ItemDefinition>(SpearItemPath);
            SerializedObject serializedItem = new SerializedObject(spear);
            serializedItem.FindProperty("meleeDamage").floatValue = 20f;
            serializedItem.FindProperty("meleeRange").floatValue = 3f;
            serializedItem.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(spear);
            return spear;
        }

        private static CCS_EquipmentItemDefinition EnsureSpearEquipment(CCS_ItemDefinition spearItem)
        {
            CCS_EquipmentItemDefinition equipment =
                AssetDatabase.LoadAssetAtPath<CCS_EquipmentItemDefinition>(SpearEquipmentPath);
            if (equipment == null)
            {
                equipment = ScriptableObject.CreateInstance<CCS_EquipmentItemDefinition>();
                AssetDatabase.CreateAsset(equipment, SpearEquipmentPath);
            }

            SerializedObject serializedEquipment = new SerializedObject(equipment);
            serializedEquipment.FindProperty("itemDefinition").objectReferenceValue = spearItem;
            serializedEquipment.FindProperty("allowedSlot").enumValueIndex = (int)CCS_EquipmentSlotType.MainHand;
            serializedEquipment.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(equipment);
            return equipment;
        }

        private static void EnsureStarterLoadoutExcludesSpear(CCS_ItemDefinition spearItem)
        {
            CCS_StarterLoadoutProfile loadout = LoadRequiredAsset<CCS_StarterLoadoutProfile>(StarterLoadoutPath);
            SerializedObject serializedLoadout = new SerializedObject(loadout);
            SerializedProperty startingItems = serializedLoadout.FindProperty("startingItems");
            for (int index = startingItems.arraySize - 1; index >= 0; index--)
            {
                SerializedProperty entry = startingItems.GetArrayElementAtIndex(index);
                CCS_ItemDefinition item =
                    entry.FindPropertyRelative("itemDefinition").objectReferenceValue as CCS_ItemDefinition;
                if (item == spearItem)
                {
                    startingItems.DeleteArrayElementAtIndex(index);
                }
            }

            serializedLoadout.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(loadout);
        }

        private static CCS_CombatProfile EnsureDefaultProfile(
            CCS_WildlifeProfile wildlifeProfile,
            CCS_WildlifeDefinition rabbitCarcass,
            CCS_WildlifeDefinition deerCarcass)
        {
            CCS_CombatProfile profile = AssetDatabase.LoadAssetAtPath<CCS_CombatProfile>(DefaultProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_CombatProfile>();
                AssetDatabase.CreateAsset(profile, DefaultProfilePath);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileDisplayName").stringValue = "Default Combat";
            serializedProfile.FindProperty("profileId").stringValue = "ccs.survival.profile.combat.default";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Default primitive melee combat rules for 0.9.8 hunting foundation.";
            serializedProfile.FindProperty("profileVersion").stringValue = "0.9.8";
            serializedProfile.FindProperty("attackCooldownSeconds").floatValue = 0.35f;
            serializedProfile.FindProperty("hitSphereRadius").floatValue = 0.35f;
            serializedProfile.FindProperty("wildlifeHitLayers").intValue = ~0;
            serializedProfile.FindProperty("wildlifeProfile").objectReferenceValue = wildlifeProfile;
            serializedProfile.FindProperty("rabbitCarcassDefinition").objectReferenceValue = rabbitCarcass;
            serializedProfile.FindProperty("deerCarcassDefinition").objectReferenceValue = deerCarcass;

            ApplySpeciesSettings(serializedProfile.FindProperty("rabbitSettings"), 20f);
            ApplySpeciesSettings(serializedProfile.FindProperty("deerSettings"), 50f);
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void ApplySpeciesSettings(SerializedProperty speciesProperty, float maxHealth)
        {
            speciesProperty.FindPropertyRelative("maxHealth").floatValue = maxHealth;
        }

        private static void EnsureBootstrapPrefabProfile(CCS_CombatProfile profile)
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError($"{LogPrefix} Missing bootstrap prefab: {BootstrapPrefabPath}");
                EditorApplication.Exit(1);
                return;
            }

            CCS_SurvivalGameplayServiceHost serviceHost = prefabRoot.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (serviceHost == null)
            {
                Debug.LogError($"{LogPrefix} Bootstrap prefab is missing CCS_SurvivalGameplayServiceHost.");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serializedHost = new SerializedObject(serviceHost);
            serializedHost.FindProperty("combatProfile").objectReferenceValue = profile;
            serializedHost.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(prefabRoot);
        }

        private static void EnsurePlayerCombatDriver()
        {
            string prefabPath = PlayerPrefabPath;
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError($"{LogPrefix} Missing player prefab: {prefabPath}");
                return;
            }

            if (prefabRoot.GetComponent<CCS_PlayerCombatDriver>() == null)
            {
                prefabRoot.AddComponent<CCS_PlayerCombatDriver>();
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        private static void EnsureLivingWildlifeDamageable()
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            CCS_WildlifeAgent[] agents = Object.FindObjectsByType<CCS_WildlifeAgent>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            CCS_CombatProfile combatProfile = AssetDatabase.LoadAssetAtPath<CCS_CombatProfile>(DefaultProfilePath);

            for (int index = 0; index < agents.Length; index++)
            {
                CCS_WildlifeAgent agent = agents[index];
                if (agent == null)
                {
                    continue;
                }

                CCS_WildlifeDamageable damageable = agent.GetComponent<CCS_WildlifeDamageable>();
                if (damageable == null)
                {
                    damageable = agent.gameObject.AddComponent<CCS_WildlifeDamageable>();
                }

                float maxHealth = combatProfile != null
                    ? combatProfile.GetSpeciesSettings(agent.Species).maxHealth
                    : agent.Species == CCS_WildlifeAiSpecies.Deer ? 50f : 20f;
                damageable.ConfigureForBootstrap(agent.AgentDisplayName, agent.Species, maxHealth);
                EditorUtility.SetDirty(agent.gameObject);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        #endregion
    }
}
