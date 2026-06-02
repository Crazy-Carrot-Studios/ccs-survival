using System.IO;
using CCS.Modules.Inventory;
using CCS.Modules.Playtesting;
using CCS.Modules.Trapping;
using CCS.Modules.Wildlife;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

namespace CCS.Modules.Trapping.Editor
{
    public static class CCS_FrontierTrappingBootstrapSetup
    {
        private const string LogPrefix = "[CCS_FrontierTrappingBootstrapSetup]";
        private const string ProfilesRoot = "Assets/CCS/Survival/Profiles/Trapping";
        private const string DefaultTrapProfilePath = ProfilesRoot + "/CCS_DefaultTrapProfile.asset";
        private const string TrapContentRoot = "Assets/CCS/Survival/Content/Trapping";
        private const string SimpleTrapDefinitionPath = TrapContentRoot + "/CCS_TrapDefinition_Simple.asset";
        private const string SimpleTrapItemPath = "Assets/CCS/Survival/Content/Items/Frontier/CCS_Item_SimpleTrap.asset";
        private const string RabbitDefinitionPath = "Assets/CCS/Survival/Content/Wildlife/Definitions/CCS_TestRabbit.asset";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
        private const string BootstrapRootPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string SimpleTrapRecipePath =
            "Assets/CCS/Survival/Profiles/Crafting/FrontierPrimitiveRecipes/CCS_FrontierSimpleTrapRecipe.asset";
        private const string SimpleTrapItemId = "ccs.survival.item.frontier.simpletrap";
        private const string KnifeItemId = "ccs.survival.item.starter.knife";
        private const string HideItemId = "ccs.survival.item.resource.hide";
        private const string HuntingCurrencyStepId = "ccs.survival.playtest.hunting.currency.verify";

        public static void ExecuteBatch()
        {
            UpdateProjectVersion();
            EnsureFolders();

            CCS_ItemDefinition trapItem = LoadRequiredItem(SimpleTrapItemPath);
            UpdateSimpleTrapItem(trapItem);

            CCS_WildlifeDefinition rabbitDefinition = LoadRequiredAsset<CCS_WildlifeDefinition>(RabbitDefinitionPath);
            CCS_TrapDefinition simpleTrap = EnsureSimpleTrapDefinition(trapItem, rabbitDefinition);
            CCS_TrapProfile trapProfile = EnsureDefaultTrapProfile(simpleTrap);
            AssignTrapProfileToBootstrapHost(trapProfile);
            EnsurePlaytestTrappingSteps();
            UpdatePlaytestProfileVersion();
            BumpEconomyProfileVersions();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Frontier trapping bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        private static void UpdateProjectVersion()
        {
            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/CCS/Survival/Profiles/Trapping"))
            {
                AssetDatabase.CreateFolder("Assets/CCS/Survival/Profiles", "Trapping");
            }

            if (!AssetDatabase.IsValidFolder(TrapContentRoot))
            {
                AssetDatabase.CreateFolder("Assets/CCS/Survival/Content", "Trapping");
            }
        }

        private static void UpdateSimpleTrapItem(CCS_ItemDefinition trapItem)
        {
            SerializedObject serialized = new SerializedObject(trapItem);
            serialized.FindProperty("gameplayKind").enumValueIndex = (int)CCS_ItemGameplayKind.Placeable;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(trapItem);
        }

        private static CCS_TrapDefinition EnsureSimpleTrapDefinition(
            CCS_ItemDefinition trapItem,
            CCS_WildlifeDefinition rabbitDefinition)
        {
            CCS_TrapDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_TrapDefinition>(SimpleTrapDefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_TrapDefinition>();
                AssetDatabase.CreateAsset(definition, SimpleTrapDefinitionPath);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("trapDefinitionId").stringValue = "ccs.survival.trap.frontier.simple";
            serialized.FindProperty("displayName").stringValue = "Simple Trap";
            serialized.FindProperty("placeableItem").objectReferenceValue = trapItem;
            serialized.FindProperty("capturedWildlifeDefinition").objectReferenceValue = rabbitDefinition;
            serialized.FindProperty("triggerDelaySeconds").floatValue = 6f;
            serialized.FindProperty("captureChance").floatValue = 0.75f;
            serialized.FindProperty("breakChance").floatValue = 0.05f;
            serialized.FindProperty("captureRadius").floatValue = 8f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_TrapProfile EnsureDefaultTrapProfile(CCS_TrapDefinition simpleTrap)
        {
            CCS_TrapProfile profile = LoadOrCreateProfile();
            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileVersion").stringValue = "1.3.3";
            serialized.FindProperty("profileDescription").stringValue =
                "Frontier trapping profile for simple snare placement and rabbit capture (1.3.3).";
            serialized.FindProperty("enableTrapping").boolValue = true;
            SerializedProperty definitions = serialized.FindProperty("trapDefinitions");
            definitions.arraySize = 1;
            definitions.GetArrayElementAtIndex(0).objectReferenceValue = simpleTrap;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static CCS_TrapProfile LoadOrCreateProfile()
        {
            CCS_TrapProfile profile = AssetDatabase.LoadAssetAtPath<CCS_TrapProfile>(DefaultTrapProfilePath);
            if (profile != null)
            {
                return profile;
            }

            profile = ScriptableObject.CreateInstance<CCS_TrapProfile>();
            AssetDatabase.CreateAsset(profile, DefaultTrapProfilePath);
            return profile;
        }

        private static void AssignTrapProfileToBootstrapHost(CCS_TrapProfile trapProfile)
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapRootPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogWarning($"{LogPrefix} Missing bootstrap root prefab; assign trap profile manually.");
                return;
            }

            CCS.Survival.Composition.CCS_SurvivalGameplayServiceHost host =
                prefabRoot.GetComponent<CCS.Survival.Composition.CCS_SurvivalGameplayServiceHost>();
            if (host == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(host);
            serialized.FindProperty("trapProfile").objectReferenceValue = trapProfile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(host);
            EditorUtility.SetDirty(prefabRoot);
        }

        private static void BumpEconomyProfileVersions()
        {
            string economyPath = "Assets/CCS/Survival/Profiles/Economy/CCS_DefaultEconomyProfile.asset";
            string vendorPath = "Assets/CCS/Survival/Profiles/Economy/CCS_DefaultVendorProfile.asset";
            ScriptableObject economy = AssetDatabase.LoadAssetAtPath<ScriptableObject>(economyPath);
            ScriptableObject vendor = AssetDatabase.LoadAssetAtPath<ScriptableObject>(vendorPath);
            if (economy != null)
            {
                SerializedObject serialized = new SerializedObject(economy);
                serialized.FindProperty("profileVersion").stringValue = "1.4.0";
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(economy);
            }

            if (vendor != null)
            {
                SerializedObject serialized = new SerializedObject(vendor);
                serialized.FindProperty("profileVersion").stringValue = "1.4.0";
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(vendor);
            }
        }

        private static void UpdatePlaytestProfileVersion()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileVersion").stringValue = "1.3.3";
            serialized.FindProperty("profileDescription").stringValue =
                "Frontier starter progression with economy, hunting, and trapping playtest checklist for milestone 1.3.3.";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsurePlaytestTrappingSteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            SerializedProperty stepList = serializedProfile.FindProperty("stepDefinitions");
            RemoveTrappingSteps(stepList);

            int insertIndex = FindStepIndex(stepList, HuntingCurrencyStepId);
            if (insertIndex < 0)
            {
                insertIndex = stepList.arraySize;
            }
            else
            {
                insertIndex += 1;
            }

            InsertStep(stepList, insertIndex++, "ccs.survival.playtest.trapping.trap.obtain", "Obtain simple trap", CCS_PlaytestStepType.ObtainTrapForTrapping, "Craft simple trap or press Ctrl+Alt+T to grant one.", SimpleTrapItemId);
            InsertStep(stepList, insertIndex++, "ccs.survival.playtest.trapping.trap.equip", "Equip simple trap", CCS_PlaytestStepType.EquipTrapForTrapping, "Select the trap as active item (playtest harness).", SimpleTrapItemId);
            InsertStep(stepList, insertIndex++, "ccs.survival.playtest.trapping.trap.place", "Place simple trap", CCS_PlaytestStepType.PlaceTrapForTrapping, "Primary use once for preview, again to confirm placement near CCS_TestRabbit.");
            InsertStep(stepList, insertIndex++, "ccs.survival.playtest.trapping.trigger", "Trigger trap capture", CCS_PlaytestStepType.ForceTrapTrigger, "Wait for timer or press Alt+T to force capture roll near rabbit.");
            InsertStep(stepList, insertIndex++, "ccs.survival.playtest.trapping.harvest", "Harvest triggered trap", CCS_PlaytestStepType.HarvestTriggeredTrap, "Equip knife (F6) and interact with the triggered trap.");
            InsertStep(stepList, insertIndex++, "ccs.survival.playtest.trapping.hide.verify", "Verify trap harvest inventory", CCS_PlaytestStepType.VerifyTrapHarvestInventory, "Confirm hide is in inventory after trap harvest.", HideItemId);
            InsertStep(stepList, insertIndex++, "ccs.survival.playtest.trapping.hide.sell", "Sell trap hide at vendor", CCS_PlaytestStepType.SellTrappingResourceAtVendor, "Sell hide at general store (Shift+V on this step).", HideItemId);
            InsertStep(stepList, insertIndex, "ccs.survival.playtest.trapping.currency.verify", "Verify trapping currency increased", CCS_PlaytestStepType.VerifyTrappingCurrencyIncreased, "Confirm Trade Dollars increased after selling trap hide.");

            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void RemoveTrappingSteps(SerializedProperty stepList)
        {
            for (int index = stepList.arraySize - 1; index >= 0; index--)
            {
                string stepId = stepList.GetArrayElementAtIndex(index).FindPropertyRelative("stepId").stringValue;
                if (!string.IsNullOrEmpty(stepId) && stepId.StartsWith("ccs.survival.playtest.trapping."))
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

        private static void InsertStep(
            SerializedProperty stepList,
            int index,
            string stepId,
            string displayName,
            CCS_PlaytestStepType stepType,
            string instructionText,
            string targetItemId = "")
        {
            stepList.InsertArrayElementAtIndex(index);
            SerializedProperty stepProperty = stepList.GetArrayElementAtIndex(index);
            stepProperty.FindPropertyRelative("stepId").stringValue = stepId;
            stepProperty.FindPropertyRelative("displayName").stringValue = displayName;
            stepProperty.FindPropertyRelative("stepType").enumValueIndex = (int)stepType;
            stepProperty.FindPropertyRelative("instructionText").stringValue = instructionText;
            stepProperty.FindPropertyRelative("targetItemId").stringValue = targetItemId ?? string.Empty;
            stepProperty.FindPropertyRelative("targetObjectId").stringValue = string.Empty;
            stepProperty.FindPropertyRelative("requiredCount").intValue = 1;
        }

        private static T LoadRequiredAsset<T>(string path) where T : Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                Debug.LogError($"{LogPrefix} Missing required asset: {path}");
                EditorApplication.Exit(1);
            }

            return asset;
        }

        private static CCS_ItemDefinition LoadRequiredItem(string path)
        {
            return LoadRequiredAsset<CCS_ItemDefinition>(path);
        }
    }
}
