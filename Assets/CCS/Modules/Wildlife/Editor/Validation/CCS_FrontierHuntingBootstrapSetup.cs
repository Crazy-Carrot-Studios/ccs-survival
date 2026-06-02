using System.IO;
using CCS.Modules.Combat;
using CCS.Modules.Economy;
using CCS.Modules.Hotbar;
using CCS.Modules.Inventory;
using CCS.Modules.Playtesting;
using CCS.Modules.Resources;
using CCS.Modules.WorldResources;
using CCS.Modules.Wildlife;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_FrontierHuntingBootstrapSetup
// CATEGORY: Modules / Wildlife / Editor / Validation
// PURPOSE: Creates frontier hunting harvest profile, bow weapon tuning, turkey content, and playtest steps.
// PLACEMENT: Batch entry for milestone 1.3.2 frontier hunting foundation.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Extends wildlife harvest routing, combat carcass spawning, economy sell loop, and bootstrap scene content.
// =============================================================================

namespace CCS.Modules.Wildlife.Editor
{
    public static class CCS_FrontierHuntingBootstrapSetup
    {
        private const string ProfilesRoot = "Assets/CCS/Survival/Profiles/Wildlife";
        private const string DefaultWildlifeProfilePath = ProfilesRoot + "/CCS_DefaultWildlifeProfile.asset";
        private const string DefaultHarvestProfilePath = ProfilesRoot + "/CCS_DefaultWildlifeHarvestProfile.asset";
        private const string DefaultCombatProfilePath = "Assets/CCS/Survival/Profiles/Combat/CCS_DefaultCombatProfile.asset";
        private const string DefaultActiveItemProfilePath =
            "Assets/CCS/Survival/Profiles/Hotbar/CCS_DefaultActiveItemProfile.asset";
        private const string DefaultPlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
        private const string DefaultAiProfilePath = ProfilesRoot + "/CCS_DefaultWildlifeAiProfile.asset";
        private const string HarvestContentRoot = "Assets/CCS/Survival/Content/Wildlife/Harvest";
        private const string WildlifeDefinitionsRoot = "Assets/CCS/Survival/Content/Wildlife/Definitions";
        private const string GeneralStoreVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_GeneralStore.asset";
        private const string DefaultEconomyProfilePath = "Assets/CCS/Survival/Profiles/Economy/CCS_DefaultEconomyProfile.asset";
        private const string DefaultVendorProfilePath = "Assets/CCS/Survival/Profiles/Economy/CCS_DefaultVendorProfile.asset";
        private const string BowItemPath = "Assets/CCS/Survival/Content/Items/Frontier/CCS_Item_Bow.asset";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string TestAreaObjectName = "CCS_WildlifeTestArea";
        private const string RabbitDefinitionPath = WildlifeDefinitionsRoot + "/CCS_TestRabbit.asset";
        private const string DeerDefinitionPath = WildlifeDefinitionsRoot + "/CCS_TestDeerCarcass.asset";
        private const string TurkeyDefinitionPath = WildlifeDefinitionsRoot + "/CCS_TestTurkeyCarcass.asset";
        private const string TurkeyCarcassObjectName = "CCS_TestTurkeyCarcass";
        private const string TurkeyLivingObjectName = "CCS_TestTurkey";
        private const string EconomyHatchetTreeStepId = "ccs.survival.playtest.economy.hatchet.tree";
        private const string LogPrefix = "[CCS_FrontierHuntingBootstrapSetup]";

        private const string BowItemId = "ccs.survival.item.frontier.bow";
        private const string KnifeItemId = "ccs.survival.item.starter.knife";
        private const string HideItemId = "ccs.survival.item.resource.hide";
        private const string BoneItemPath = "Assets/CCS/Survival/Content/Items/Resources/Primitive/CCS_Item_Bone.asset";

        #region Public Methods

        public static void ExecuteBatch()
        {
            UpdateProjectVersion();
            EnsureFolders();

            CCS_ItemDefinition rawRabbitMeat = LoadRequiredItem(
                "Assets/CCS/Survival/Content/Items/Resources/Wildlife/CCS_Item_RawRabbitMeat.asset");
            CCS_ItemDefinition rawMeat = LoadRequiredItem(
                "Assets/CCS/Survival/Content/Items/Resources/Wildlife/CCS_Item_RawMeat.asset");
            CCS_ItemDefinition rawVenison = LoadRequiredItem(
                "Assets/CCS/Survival/Content/Items/Resources/Wildlife/CCS_Item_RawVenison.asset");
            CCS_ItemDefinition hideItem = LoadRequiredItem(
                "Assets/CCS/Survival/Content/Items/Resources/Primitive/CCS_Item_Hide.asset");
            CCS_ItemDefinition boneItem = LoadRequiredItem(BoneItemPath);
            CCS_ItemDefinition featherItem = LoadRequiredItem(
                "Assets/CCS/Survival/Content/Items/Frontier/CCS_Item_Feather.asset");
            CCS_ItemDefinition animalFatItem = LoadRequiredItem(
                "Assets/CCS/Survival/Content/Items/Frontier/CCS_Item_AnimalFat.asset");

            UpdateBowItem();
            EnsureBoneVendorEconomy(boneItem);
            EnsureEconomyProfileVersions();

            CCS_WildlifeDefinition rabbitDefinition = LoadRequiredAsset<CCS_WildlifeDefinition>(RabbitDefinitionPath);
            CCS_WildlifeDefinition deerDefinition = LoadRequiredAsset<CCS_WildlifeDefinition>(DeerDefinitionPath);
            CCS_WildlifeDefinition turkeyDefinition = EnsureTurkeyCarcassDefinition();

            CCS_WildlifeHarvestDefinition rabbitHarvest = EnsureHarvestDefinition(
                "CCS_HarvestDefinition_Rabbit",
                "ccs.survival.wildlife.harvest.rabbit",
                rabbitDefinition,
                new[]
                {
                    CreateHarvestDrop(hideItem, 1, 1, CCS_HarvestMethodType.Skin)
                },
                new[]
                {
                    CreateHarvestDrop(rawRabbitMeat, 1, 1, CCS_HarvestMethodType.Butcher),
                    CreateHarvestDrop(boneItem, 1, 1, CCS_HarvestMethodType.Butcher)
                });

            CCS_WildlifeHarvestDefinition turkeyHarvest = EnsureHarvestDefinition(
                "CCS_HarvestDefinition_Turkey",
                "ccs.survival.wildlife.harvest.turkey",
                turkeyDefinition,
                new[]
                {
                    CreateHarvestDrop(featherItem, 1, 2, CCS_HarvestMethodType.Skin)
                },
                new[]
                {
                    CreateHarvestDrop(rawMeat, 1, 1, CCS_HarvestMethodType.Butcher),
                    CreateHarvestDrop(boneItem, 1, 1, CCS_HarvestMethodType.Butcher)
                });

            CCS_WildlifeHarvestDefinition deerHarvest = EnsureHarvestDefinition(
                "CCS_HarvestDefinition_Deer",
                "ccs.survival.wildlife.harvest.deer",
                deerDefinition,
                new[]
                {
                    CreateHarvestDrop(hideItem, 1, 2, CCS_HarvestMethodType.Skin)
                },
                new[]
                {
                    CreateHarvestDrop(rawVenison, 2, 3, CCS_HarvestMethodType.Butcher),
                    CreateHarvestDrop(boneItem, 1, 2, CCS_HarvestMethodType.Butcher),
                    CreateHarvestDrop(animalFatItem, 1, 1, CCS_HarvestMethodType.Butcher)
                });

            CCS_WildlifeHarvestProfile harvestProfile = EnsureDefaultHarvestProfile(
                rabbitHarvest,
                turkeyHarvest,
                deerHarvest);

            LinkHarvestDefinition(rabbitDefinition, rabbitHarvest);
            LinkHarvestDefinition(turkeyDefinition, turkeyHarvest);
            LinkHarvestDefinition(deerDefinition, deerHarvest);

            CCS_WildlifeProfile wildlifeProfile = EnsureDefaultWildlifeProfile(harvestProfile);
            EnsureDefaultCombatProfileTurkey(turkeyDefinition);
            EnsureActiveItemProfileWildlifeHarvestRouting();
            EnsureBootstrapHarvestContent(wildlifeProfile, turkeyDefinition);
            EnsurePlaytestHuntingSteps();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Frontier hunting bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void UpdateProjectVersion()
        {
            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Content/Wildlife");
            EnsureFolder(HarvestContentRoot);
            EnsureFolder(ProfilesRoot);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            string folderName = Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(folderName))
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
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

        private static CCS_ItemDefinition LoadRequiredItem(string assetPath)
        {
            return LoadRequiredAsset<CCS_ItemDefinition>(assetPath);
        }

        private static void UpdateBowItem()
        {
            CCS_ItemDefinition bow = LoadRequiredItem(BowItemPath);
            SerializedObject serializedItem = new SerializedObject(bow);
            serializedItem.FindProperty("hasWeaponIdentity").boolValue = true;
            serializedItem.FindProperty("hasToolIdentity").boolValue = false;
            serializedItem.FindProperty("gameplayKind").enumValueIndex = (int)CCS_ItemGameplayKind.Weapon;
            serializedItem.FindProperty("weaponArchetype").enumValueIndex = (int)CCS_WeaponArchetype.Bow;
            serializedItem.FindProperty("weaponType").enumValueIndex = (int)CCS_WeaponType.Ranged;
            serializedItem.FindProperty("damageType").enumValueIndex = (int)CCS_DamageType.Pierce;
            serializedItem.FindProperty("rangeType").enumValueIndex = (int)CCS_RangeType.LongRanged;
            serializedItem.FindProperty("meleeDamage").floatValue = 14f;
            serializedItem.FindProperty("meleeRange").floatValue = 28f;
            serializedItem.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(bow);
        }

        private static void EnsureEconomyProfileVersions()
        {
            CCS_EconomyProfile economyProfile =
                AssetDatabase.LoadAssetAtPath<CCS_EconomyProfile>(DefaultEconomyProfilePath);
            if (economyProfile != null)
            {
                SerializedObject serializedEconomy = new SerializedObject(economyProfile);
                serializedEconomy.FindProperty("profileVersion").stringValue = "1.3.2";
                serializedEconomy.FindProperty("profileDescription").stringValue =
                    "Frontier economy profile (1.3.2) with fishing and hunting trade goods.";
                serializedEconomy.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(economyProfile);
            }

            CCS_VendorProfile vendorProfile =
                AssetDatabase.LoadAssetAtPath<CCS_VendorProfile>(DefaultVendorProfilePath);
            if (vendorProfile != null)
            {
                SerializedObject serializedVendorProfile = new SerializedObject(vendorProfile);
                serializedVendorProfile.FindProperty("profileVersion").stringValue = "1.3.2";
                serializedVendorProfile.FindProperty("profileDescription").stringValue =
                    "Frontier vendor catalog profile for milestone 1.3.2 (hunting trade goods).";
                serializedVendorProfile.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(vendorProfile);
            }
        }

        private static void EnsureBoneVendorEconomy(CCS_ItemDefinition boneItem)
        {
            SerializedObject serializedBone = new SerializedObject(boneItem);
            serializedBone.FindProperty("hasEconomyValues").boolValue = true;
            serializedBone.FindProperty("buyValue").intValue = 0;
            serializedBone.FindProperty("sellValue").intValue = 1;
            serializedBone.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(boneItem);

            CCS_VendorDefinition vendor = AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(GeneralStoreVendorPath);
            if (vendor == null)
            {
                Debug.LogWarning($"{LogPrefix} Missing general store vendor; skipping bone catalog row.");
                return;
            }

            SerializedObject serializedVendor = new SerializedObject(vendor);
            SerializedProperty catalogItems =
                serializedVendor.FindProperty("vendorInventory").FindPropertyRelative("items");

            if (TryFindVendorCatalogIndex(catalogItems, boneItem, out int existingIndex))
            {
                SetVendorCatalogEntry(catalogItems, existingIndex, boneItem, allowBuy: false, allowSell: true);
            }
            else
            {
                int newIndex = catalogItems.arraySize;
                catalogItems.InsertArrayElementAtIndex(newIndex);
                SetVendorCatalogEntry(catalogItems, newIndex, boneItem, allowBuy: false, allowSell: true);
            }

            serializedVendor.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(vendor);
        }

        private static bool TryFindVendorCatalogIndex(
            SerializedProperty catalogItems,
            CCS_ItemDefinition item,
            out int index)
        {
            for (index = 0; index < catalogItems.arraySize; index++)
            {
                SerializedProperty entry = catalogItems.GetArrayElementAtIndex(index);
                if (entry.FindPropertyRelative("itemDefinition").objectReferenceValue == item)
                {
                    return true;
                }
            }

            index = -1;
            return false;
        }

        private static void SetVendorCatalogEntry(
            SerializedProperty catalogItems,
            int index,
            CCS_ItemDefinition item,
            bool allowBuy,
            bool allowSell)
        {
            SerializedProperty entry = catalogItems.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("itemDefinition").objectReferenceValue = item;
            entry.FindPropertyRelative("stockQuantity").intValue = -1;
            entry.FindPropertyRelative("allowBuy").boolValue = allowBuy;
            entry.FindPropertyRelative("allowSell").boolValue = allowSell;
            entry.FindPropertyRelative("buyPriceOverride").intValue = -1;
            entry.FindPropertyRelative("sellPriceOverride").intValue = -1;
        }

        private static CCS_WildlifeDefinition EnsureTurkeyCarcassDefinition()
        {
            CCS_WildlifeDefinition definition =
                AssetDatabase.LoadAssetAtPath<CCS_WildlifeDefinition>(TurkeyDefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_WildlifeDefinition>();
                AssetDatabase.CreateAsset(definition, TurkeyDefinitionPath);
            }

            SerializedObject serializedDefinition = new SerializedObject(definition);
            serializedDefinition.FindProperty("wildlifeId").stringValue = "ccs.survival.wildlife.test.turkeycarcass";
            serializedDefinition.FindProperty("displayName").stringValue = "Test Turkey Carcass";
            serializedDefinition.FindProperty("wildlifeType").enumValueIndex = (int)CCS_WildlifeType.Bird;
            serializedDefinition.FindProperty("harvestToolRequirement").enumValueIndex =
                (int)CCS_RequiredToolType.Knife;
            serializedDefinition.FindProperty("maxHarvestCount").intValue = 1;
            serializedDefinition.FindProperty("respawnTimeSeconds").floatValue = 0f;
            serializedDefinition.FindProperty("isAggressive").boolValue = false;
            serializedDefinition.FindProperty("resourceSourceType").enumValueIndex =
                (int)CCS_ResourceSourceType.Wildlife;
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static (CCS_ItemDefinition item, int min, int max, CCS_HarvestMethodType method) CreateHarvestDrop(
            CCS_ItemDefinition item,
            int minQuantity,
            int maxQuantity,
            CCS_HarvestMethodType harvestMethodType)
        {
            return (item, minQuantity, maxQuantity, harvestMethodType);
        }

        private static CCS_WildlifeHarvestDefinition EnsureHarvestDefinition(
            string assetName,
            string harvestDefinitionId,
            CCS_WildlifeDefinition wildlifeDefinition,
            (CCS_ItemDefinition item, int min, int max, CCS_HarvestMethodType method)[] skinDrops,
            (CCS_ItemDefinition item, int min, int max, CCS_HarvestMethodType method)[] butcherDrops)
        {
            string assetPath = $"{HarvestContentRoot}/{assetName}.asset";
            CCS_WildlifeHarvestDefinition harvestDefinition =
                AssetDatabase.LoadAssetAtPath<CCS_WildlifeHarvestDefinition>(assetPath);
            if (harvestDefinition == null)
            {
                harvestDefinition = ScriptableObject.CreateInstance<CCS_WildlifeHarvestDefinition>();
                AssetDatabase.CreateAsset(harvestDefinition, assetPath);
            }

            SerializedObject serializedDefinition = new SerializedObject(harvestDefinition);
            serializedDefinition.FindProperty("harvestDefinitionId").stringValue = harvestDefinitionId;
            serializedDefinition.FindProperty("wildlifeDefinition").objectReferenceValue = wildlifeDefinition;
            serializedDefinition.FindProperty("resourceSourceType").enumValueIndex =
                (int)CCS_ResourceSourceType.Wildlife;
            ApplyHarvestDropTable(serializedDefinition.FindProperty("skinDrops"), skinDrops);
            ApplyHarvestDropTable(serializedDefinition.FindProperty("butcherDrops"), butcherDrops);
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(harvestDefinition);
            return harvestDefinition;
        }

        private static void ApplyHarvestDropTable(
            SerializedProperty dropList,
            (CCS_ItemDefinition item, int min, int max, CCS_HarvestMethodType method)[] drops)
        {
            dropList.arraySize = drops.Length;
            for (int index = 0; index < drops.Length; index++)
            {
                SerializedProperty dropEntry = dropList.GetArrayElementAtIndex(index);
                dropEntry.FindPropertyRelative("itemDefinition").objectReferenceValue = drops[index].item;
                dropEntry.FindPropertyRelative("minQuantity").intValue = drops[index].min;
                dropEntry.FindPropertyRelative("maxQuantity").intValue = drops[index].max;
                dropEntry.FindPropertyRelative("harvestMethodType").enumValueIndex = (int)drops[index].method;
            }
        }

        private static void LinkHarvestDefinition(
            CCS_WildlifeDefinition wildlifeDefinition,
            CCS_WildlifeHarvestDefinition harvestDefinition)
        {
            SerializedObject serializedDefinition = new SerializedObject(wildlifeDefinition);
            serializedDefinition.FindProperty("harvestDefinition").objectReferenceValue = harvestDefinition;
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(wildlifeDefinition);
        }

        private static CCS_WildlifeHarvestProfile EnsureDefaultHarvestProfile(
            params CCS_WildlifeHarvestDefinition[] harvestDefinitions)
        {
            CCS_WildlifeHarvestProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_WildlifeHarvestProfile>(DefaultHarvestProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_WildlifeHarvestProfile>();
                AssetDatabase.CreateAsset(profile, DefaultHarvestProfilePath);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileDisplayName").stringValue = "Default Wildlife Harvest";
            serializedProfile.FindProperty("profileId").stringValue = "ccs.survival.profile.wildlifeharvest.default";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Frontier skin and butcher harvest tables for milestone 1.3.2.";
            serializedProfile.FindProperty("profileVersion").stringValue = "1.3.2";

            SerializedProperty definitionList = serializedProfile.FindProperty("harvestDefinitions");
            definitionList.arraySize = harvestDefinitions.Length;
            for (int index = 0; index < harvestDefinitions.Length; index++)
            {
                definitionList.GetArrayElementAtIndex(index).objectReferenceValue = harvestDefinitions[index];
            }

            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static CCS_WildlifeProfile EnsureDefaultWildlifeProfile(CCS_WildlifeHarvestProfile harvestProfile)
        {
            CCS_WildlifeProfile profile = LoadRequiredAsset<CCS_WildlifeProfile>(DefaultWildlifeProfilePath);
            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileVersion").stringValue = "1.3.2";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Default wildlife harvesting rules with frontier skin/butcher tables (1.3.2).";
            serializedProfile.FindProperty("harvestProfile").objectReferenceValue = harvestProfile;
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureDefaultCombatProfileTurkey(CCS_WildlifeDefinition turkeyCarcassDefinition)
        {
            CCS_CombatProfile profile = AssetDatabase.LoadAssetAtPath<CCS_CombatProfile>(DefaultCombatProfilePath);
            if (profile == null)
            {
                Debug.LogWarning($"{LogPrefix} Missing combat profile; skipping turkey combat settings.");
                return;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);

            ApplyCombatSpeciesSettings(
                serializedProfile.FindProperty("turkeySettings"),
                maxHealth: 25f,
                carcassObjectName: TurkeyCarcassObjectName,
                carcassPrimitive: PrimitiveType.Sphere,
                carcassLocalScale: new Vector3(0.55f, 0.55f, 0.55f));
            serializedProfile.FindProperty("turkeyCarcassDefinition").objectReferenceValue = turkeyCarcassDefinition;
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void ApplyCombatSpeciesSettings(
            SerializedProperty speciesProperty,
            float maxHealth,
            string carcassObjectName,
            PrimitiveType carcassPrimitive,
            Vector3 carcassLocalScale)
        {
            speciesProperty.FindPropertyRelative("maxHealth").floatValue = maxHealth;
            speciesProperty.FindPropertyRelative("carcassObjectName").stringValue = carcassObjectName;
            speciesProperty.FindPropertyRelative("carcassPrimitive").enumValueIndex = (int)carcassPrimitive;
            speciesProperty.FindPropertyRelative("carcassLocalScale").vector3Value = carcassLocalScale;
        }

        private static void EnsureActiveItemProfileWildlifeHarvestRouting()
        {
            CCS_ActiveItemProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_ActiveItemProfile>(DefaultActiveItemProfilePath);
            if (profile == null)
            {
                Debug.LogWarning($"{LogPrefix} Missing active item profile; skipping harvest routing flag.");
                return;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileVersion").stringValue = "1.3.2";
            serializedProfile.FindProperty("enableWildlifeHarvestRouting").boolValue = true;
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsureBootstrapHarvestContent(
            CCS_WildlifeProfile wildlifeProfile,
            CCS_WildlifeDefinition turkeyDefinition)
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            Transform sceneRoot = FindSceneRoot();
            if (sceneRoot == null)
            {
                Debug.LogError($"{LogPrefix} Could not find CCS_BuildVerificationScene root.");
                EditorApplication.Exit(1);
                return;
            }

            Transform testArea = sceneRoot.Find(TestAreaObjectName);
            if (testArea == null)
            {
                GameObject testAreaObject = new GameObject(TestAreaObjectName);
                testAreaObject.transform.SetParent(sceneRoot, false);
                testAreaObject.transform.localPosition = new Vector3(4f, 0f, 4f);
                testArea = testAreaObject.transform;
            }

            EnsureHarvestableWildlife(
                testArea,
                TurkeyCarcassObjectName,
                PrimitiveType.Sphere,
                new Vector3(0f, 0.4f, 0f),
                new Vector3(0.55f, 0.55f, 0.55f),
                turkeyDefinition,
                wildlifeProfile);

            CCS_WildlifeAiProfile aiProfile = AssetDatabase.LoadAssetAtPath<CCS_WildlifeAiProfile>(DefaultAiProfilePath);
            Transform playerTransform = FindPlayerTransform(sceneRoot);
            EnsureLivingWildlife(
                testArea,
                TurkeyLivingObjectName,
                PrimitiveType.Sphere,
                new Vector3(0f, 0.4f, -2.5f),
                new Vector3(0.55f, 0.55f, 0.55f),
                "Turkey",
                CCS_WildlifeAiSpecies.Turkey,
                aiProfile,
                playerTransform);

            EnsureLivingWildlifeDamageable();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureLivingWildlifeDamageable()
        {
            CCS_CombatProfile combatProfile =
                AssetDatabase.LoadAssetAtPath<CCS_CombatProfile>(DefaultCombatProfilePath);
            CCS_WildlifeAgent[] agents = Object.FindObjectsByType<CCS_WildlifeAgent>(
                FindObjectsInactive.Exclude);

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
        }

        private static Transform FindSceneRoot()
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int index = 0; index < roots.Length; index++)
            {
                if (roots[index].name == "CCS_BuildVerificationScene")
                {
                    return roots[index].transform;
                }
            }

            return null;
        }

        private static Transform FindPlayerTransform(Transform sceneRoot)
        {
            Transform player = sceneRoot.Find("PF_CCS_Player");
            if (player != null)
            {
                return player;
            }

            CharacterController[] characterControllers = Object.FindObjectsByType<CharacterController>(
                FindObjectsInactive.Exclude);
            if (characterControllers == null || characterControllers.Length == 0)
            {
                return null;
            }

            for (int index = 0; index < characterControllers.Length; index++)
            {
                if (characterControllers[index].gameObject.name == "PF_CCS_Player")
                {
                    return characterControllers[index].transform;
                }
            }

            return characterControllers[0].transform;
        }

        private static void EnsureHarvestableWildlife(
            Transform parent,
            string objectName,
            PrimitiveType primitiveType,
            Vector3 localPosition,
            Vector3 localScale,
            CCS_WildlifeDefinition wildlifeDefinition,
            CCS_WildlifeProfile profile)
        {
            Transform existing = parent.Find(objectName);
            GameObject wildlifeObject;

            if (existing != null)
            {
                wildlifeObject = existing.gameObject;
            }
            else
            {
                wildlifeObject = GameObject.CreatePrimitive(primitiveType);
                wildlifeObject.name = objectName;
                wildlifeObject.transform.SetParent(parent, false);
            }

            wildlifeObject.transform.localPosition = localPosition;
            wildlifeObject.transform.localScale = localScale;

            Collider collider = wildlifeObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = false;
                collider.enabled = true;
            }

            CCS_HarvestableWildlife harvestable = wildlifeObject.GetComponent<CCS_HarvestableWildlife>();
            if (harvestable == null)
            {
                harvestable = wildlifeObject.AddComponent<CCS_HarvestableWildlife>();
            }

            SerializedObject serializedHarvestable = new SerializedObject(harvestable);
            serializedHarvestable.FindProperty("wildlifeDefinition").objectReferenceValue = wildlifeDefinition;
            serializedHarvestable.FindProperty("wildlifeProfile").objectReferenceValue = profile;
            serializedHarvestable.FindProperty("assumeRequiredToolEquipped").boolValue = false;
            serializedHarvestable.FindProperty("interactionDistance").floatValue = 3f;
            serializedHarvestable.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(wildlifeObject);
        }

        private static void EnsureLivingWildlife(
            Transform parent,
            string objectName,
            PrimitiveType primitiveType,
            Vector3 localPosition,
            Vector3 localScale,
            string displayName,
            CCS_WildlifeAiSpecies species,
            CCS_WildlifeAiProfile profile,
            Transform playerTransform)
        {
            Transform existing = parent.Find(objectName);
            GameObject wildlifeObject;

            if (existing != null)
            {
                wildlifeObject = existing.gameObject;
            }
            else
            {
                wildlifeObject = GameObject.CreatePrimitive(primitiveType);
                wildlifeObject.name = objectName;
                wildlifeObject.transform.SetParent(parent, false);
            }

            wildlifeObject.transform.localPosition = localPosition;
            wildlifeObject.transform.localScale = localScale;

            Rigidbody rigidbody = wildlifeObject.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                Object.DestroyImmediate(rigidbody);
            }

            Collider collider = wildlifeObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = false;
                collider.enabled = true;
            }

            CCS_WildlifeAgent agent = wildlifeObject.GetComponent<CCS_WildlifeAgent>();
            if (agent == null)
            {
                agent = wildlifeObject.AddComponent<CCS_WildlifeAgent>();
            }

            agent.ConfigureForBootstrap(displayName, species, profile, playerTransform);
            EditorUtility.SetDirty(wildlifeObject);
        }

        private static void EnsurePlaytestHuntingSteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                Debug.LogWarning($"{LogPrefix} Missing playtest profile; skipping hunting checklist steps.");
                return;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileVersion").stringValue = "1.3.2";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Frontier starter progression with economy and hunting playtest checklist for milestone 1.3.2.";

            SerializedProperty stepList = serializedProfile.FindProperty("stepDefinitions");
            RemoveHuntingSteps(stepList);

            int insertIndex = FindStepIndex(stepList, EconomyHatchetTreeStepId);
            if (insertIndex < 0)
            {
                insertIndex = stepList.arraySize;
            }
            else
            {
                insertIndex += 1;
            }

            InsertHuntingSteps(stepList, insertIndex);
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void RemoveHuntingSteps(SerializedProperty stepList)
        {
            for (int index = stepList.arraySize - 1; index >= 0; index--)
            {
                string stepId = stepList.GetArrayElementAtIndex(index).FindPropertyRelative("stepId").stringValue;
                if (!string.IsNullOrEmpty(stepId) && stepId.StartsWith("ccs.survival.playtest.hunting."))
                {
                    stepList.DeleteArrayElementAtIndex(index);
                }
            }
        }

        private static int FindStepIndex(SerializedProperty stepList, string stepId)
        {
            for (int index = 0; index < stepList.arraySize; index++)
            {
                if (stepList.GetArrayElementAtIndex(index).FindPropertyRelative("stepId").stringValue == stepId)
                {
                    return index;
                }
            }

            return -1;
        }

        private static void InsertHuntingSteps(SerializedProperty stepList, int insertIndex)
        {
            InsertStep(
                stepList,
                insertIndex++,
                "ccs.survival.playtest.hunting.bow.obtain",
                "Obtain bow for hunt",
                CCS_PlaytestStepType.ObtainBowForHunt,
                "Craft or grant the frontier bow (Shift+F6 debug or frontier bow recipe). Press F11 when bow is in inventory.",
                BowItemId);
            InsertStep(
                stepList,
                insertIndex++,
                "ccs.survival.playtest.hunting.bow.equip",
                "Equip bow for hunt",
                CCS_PlaytestStepType.EquipBowForHunt,
                "Shift+F6 equips the frontier bow for ranged hunting foundation verification.",
                BowItemId);
            InsertStep(
                stepList,
                insertIndex++,
                "ccs.survival.playtest.hunting.rabbit.kill",
                "Hunt rabbit with bow",
                CCS_PlaytestStepType.HuntWildlife,
                "Kill CCS_TestRabbit with primary attack while the bow is equipped.");
            InsertStep(
                stepList,
                insertIndex++,
                "ccs.survival.playtest.hunting.knife.equip",
                "Equip knife for harvest",
                CCS_PlaytestStepType.EquipWeapon,
                "Press F6 to equip the pocket knife before harvesting the carcass.",
                KnifeItemId);
            InsertStep(
                stepList,
                insertIndex++,
                "ccs.survival.playtest.hunting.carcass.harvest",
                "Harvest rabbit carcass",
                CCS_PlaytestStepType.HarvestCarcass,
                "Interact with the rabbit carcass and harvest skin/butcher drops with the knife equipped.");
            InsertStep(
                stepList,
                insertIndex++,
                "ccs.survival.playtest.hunting.hide.verify",
                "Verify hide in inventory",
                CCS_PlaytestStepType.VerifyVendorInventoryUpdated,
                "Confirm hide appears in player inventory after harvesting the rabbit carcass.",
                HideItemId);
            InsertStep(
                stepList,
                insertIndex++,
                "ccs.survival.playtest.hunting.hide.sell",
                "Sell hide at vendor",
                CCS_PlaytestStepType.SellHuntingResourceAtVendor,
                "Face CCS_TestGeneralStore, open vendor debug, and sell one hide (Shift+V or vendor Sell).",
                HideItemId);
            InsertStep(
                stepList,
                insertIndex,
                "ccs.survival.playtest.hunting.currency.verify",
                "Verify hunting currency increased",
                CCS_PlaytestStepType.VerifyHuntingCurrencyIncreased,
                "Confirm Trade Dollars balance increased after selling hide at the general store.");
        }

        private static void InsertStep(
            SerializedProperty stepList,
            int index,
            string stepId,
            string displayName,
            CCS_PlaytestStepType stepType,
            string instructionText,
            string targetItemId = "",
            string targetObjectId = "")
        {
            stepList.InsertArrayElementAtIndex(index);
            SerializedProperty stepProperty = stepList.GetArrayElementAtIndex(index);
            stepProperty.FindPropertyRelative("stepId").stringValue = stepId;
            stepProperty.FindPropertyRelative("displayName").stringValue = displayName;
            stepProperty.FindPropertyRelative("stepType").enumValueIndex = (int)stepType;
            stepProperty.FindPropertyRelative("instructionText").stringValue = instructionText;
            stepProperty.FindPropertyRelative("targetItemId").stringValue = targetItemId ?? string.Empty;
            stepProperty.FindPropertyRelative("targetObjectId").stringValue = targetObjectId ?? string.Empty;
            stepProperty.FindPropertyRelative("requiredCount").intValue = 1;
            stepProperty.FindPropertyRelative("timeoutSeconds").floatValue = 0f;
        }

        #endregion
    }
}
