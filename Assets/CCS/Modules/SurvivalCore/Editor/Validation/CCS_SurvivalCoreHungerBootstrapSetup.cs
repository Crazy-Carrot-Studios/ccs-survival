using CCS.Modules.SurvivalCore;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalCoreHungerBootstrapSetup
// CATEGORY: Modules / SurvivalCore / Editor / Validation
// PURPOSE: Updates default survival core profile with 0.9.5 hunger usage tuning.
// PLACEMENT: Batch entry for 0.9.5 consumables and hunger usage milestone.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Passive hunger drain only. No starvation health damage in 0.9.5.
// =============================================================================

namespace CCS.Modules.SurvivalCore.Editor
{
    public static class CCS_SurvivalCoreHungerBootstrapSetup
    {
        private const string DefaultProfilePath =
            "Assets/CCS/Survival/Profiles/SurvivalCore/CCS_DefaultSurvivalCoreProfile.asset";
        private const string LogPrefix = "[CCS_SurvivalCoreHungerBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureDefaultProfile();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Survival core hunger bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureDefaultProfile()
        {
            CCS_SurvivalCoreProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_SurvivalCoreProfile>(DefaultProfilePath);
            if (profile == null)
            {
                Debug.LogError($"{LogPrefix} Missing default survival core profile: {DefaultProfilePath}");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Default survival core stat tuning with passive hunger drain for 0.9.5.";
            serializedProfile.FindProperty("profileVersion").stringValue = "0.9.5";
            serializedProfile.FindProperty("hungerDrainPerSecond").floatValue = 0.01f;
            serializedProfile.FindProperty("hungerLowThreshold").floatValue = 30f;
            serializedProfile.FindProperty("hungerCriticalThreshold").floatValue = 10f;
            serializedProfile.FindProperty("hungerConsumeCooldownSeconds").floatValue = 1f;
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        #endregion
    }
}
