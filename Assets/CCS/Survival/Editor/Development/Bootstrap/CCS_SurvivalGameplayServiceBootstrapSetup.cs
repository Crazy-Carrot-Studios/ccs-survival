using CCS.Survival.Composition;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalGameplayServiceBootstrapSetup
// CATEGORY: Survival / Editor / Development / Bootstrap
// PURPOSE: Assigns default gameplay service profiles to the survival bootstrap prefab host.
// PLACEMENT: Batch entry for 0.4.3 HUD runtime wiring milestone.
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: Adds CCS_SurvivalGameplayServiceHost to PF_CCS_Survival_BootstrapRoot when missing.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public static class CCS_SurvivalGameplayServiceBootstrapSetup
    {
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string SurvivalCoreProfilePath = "Assets/CCS/Survival/Profiles/SurvivalCore/CCS_DefaultSurvivalCoreProfile.asset";
        private const string InteractionProfilePath = "Assets/CCS/Survival/Profiles/Interaction/CCS_DefaultInteractionProfile.asset";
        private const string InventoryProfilePath = "Assets/CCS/Survival/Profiles/Inventory/CCS_DefaultInventoryProfile.asset";
        private const string EquipmentProfilePath = "Assets/CCS/Survival/Profiles/Equipment/CCS_DefaultEquipmentProfile.asset";
        private const string WorldResourceProfilePath =
            "Assets/CCS/Survival/Profiles/WorldResources/CCS_DefaultWorldResourceProfile.asset";
        private const string CraftingProfilePath =
            "Assets/CCS/Survival/Profiles/Crafting/CCS_DefaultCraftingProfile.asset";
        private const string LogPrefix = "[CCS_SurvivalGameplayServiceBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError($"{LogPrefix} Missing bootstrap prefab: {BootstrapPrefabPath}");
                EditorApplication.Exit(1);
                return;
            }

            string prefabPath = AssetDatabase.GetAssetPath(prefabRoot);
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
            CCS_SurvivalGameplayServiceHost host = prefabContents.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (host == null)
            {
                host = prefabContents.AddComponent<CCS_SurvivalGameplayServiceHost>();
            }

            SerializedObject serializedHost = new SerializedObject(host);
            serializedHost.FindProperty("survivalCoreProfile").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Object>(SurvivalCoreProfilePath);
            serializedHost.FindProperty("interactionProfile").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Object>(InteractionProfilePath);
            serializedHost.FindProperty("inventoryProfile").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Object>(InventoryProfilePath);
            serializedHost.FindProperty("equipmentProfile").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Object>(EquipmentProfilePath);
            serializedHost.FindProperty("worldResourceProfile").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Object>(WorldResourceProfilePath);
            serializedHost.FindProperty("craftingProfile").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Object>(CraftingProfilePath);
            serializedHost.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);
            AssetDatabase.SaveAssets();

            Debug.Log($"{LogPrefix} Gameplay service host configured on bootstrap prefab.");
            EditorApplication.Exit(0);
        }

        #endregion
    }
}
