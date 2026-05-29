using System.Collections.Generic;
using CCS.Survival.SurvivalCore;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalCoreProfileCreationMenu
// CATEGORY: Survival / Editor / SurvivalCore / Tools
// PURPOSE: Creates default CCS_SurvivalCoreProfile asset with placeholder tuning values.
// PLACEMENT: Menu path CCS/Survival/Survival Core/Create Default Survival Core Profile.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Safe placeholder decay rates for 0.3.7 foundation.
// =============================================================================

namespace CCS.Survival.Editor.SurvivalCore
{
    public static class CCS_SurvivalCoreProfileCreationMenu
    {
        private const string MenuPath = "CCS/Survival/Survival Core/Create Default Survival Core Profile";
        private const string ProfileFolder = "Assets/CCS/Survival/Profiles/SurvivalCore";
        private const string ProfileAssetPath = ProfileFolder + "/CCS_DefaultSurvivalCoreProfile.asset";

        #region Public Methods

        [MenuItem(MenuPath, priority = 111)]
        public static void CreateDefaultSurvivalCoreProfile()
        {
            CreateDefaultSurvivalCoreProfileInternal(showDialogs: true);
        }

        public static void CreateDefaultSurvivalCoreProfileForBatch()
        {
            CreateDefaultSurvivalCoreProfileInternal(showDialogs: false);
        }

        private static void CreateDefaultSurvivalCoreProfileInternal(bool showDialogs)
        {
            EnsureProfileFolderExists();

            CCS_SurvivalCoreProfile existing =
                AssetDatabase.LoadAssetAtPath<CCS_SurvivalCoreProfile>(ProfileAssetPath);

            if (existing != null)
            {
                if (showDialogs)
                {
                    EditorUtility.DisplayDialog(
                        "Survival Core Profile",
                        $"Profile already exists:\n{ProfileAssetPath}",
                        "OK");
                }

                return;
            }

            CCS_SurvivalCoreProfile profile = ScriptableObject.CreateInstance<CCS_SurvivalCoreProfile>();
            ApplyDefaultSerializedValues(profile);

            AssetDatabase.CreateAsset(profile, ProfileAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            CCS.Survival.CCS_SurvivalValidationResult validation =
                CCS_SurvivalCoreValidationUtility.ValidateProfile(profile);

            if (!validation.IsSuccess)
            {
                Debug.LogWarning(
                    $"[CCS_SurvivalCoreProfileCreationMenu] Profile created with validation warning: {validation.Message}");
            }
            else
            {
                Debug.Log($"[CCS_SurvivalCoreProfileCreationMenu] Created {ProfileAssetPath}");
            }

            if (showDialogs)
            {
                Selection.activeObject = profile;
                EditorGUIUtility.PingObject(profile);
            }
        }

        #endregion

        #region Private Methods

        private static void EnsureProfileFolderExists()
        {
            if (!AssetDatabase.IsValidFolder("Assets/CCS/Survival/Profiles"))
            {
                AssetDatabase.CreateFolder("Assets/CCS/Survival", "Profiles");
            }

            if (!AssetDatabase.IsValidFolder(ProfileFolder))
            {
                AssetDatabase.CreateFolder("Assets/CCS/Survival/Profiles", "SurvivalCore");
            }
        }

        private static void ApplyDefaultSerializedValues(CCS_SurvivalCoreProfile profile)
        {
            SerializedObject serializedProfile = new SerializedObject(profile);

            serializedProfile.FindProperty("profileDisplayName").stringValue = "Default Survival Core";
            serializedProfile.FindProperty("profileId").stringValue = "ccs.survival.profile.core.default";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Default survival core stat tuning for 0.3.7 foundation.";
            serializedProfile.FindProperty("profileVersion").stringValue = "0.3.7";
            serializedProfile.FindProperty("passiveHealthHealPerSecond").floatValue = 0f;
            serializedProfile.FindProperty("passiveHealthDamagePerSecond").floatValue = 0f;
            serializedProfile.FindProperty("staminaRecoveryPerSecond").floatValue = 2f;
            serializedProfile.FindProperty("staminaDrainPerSecond").floatValue = 4f;

            SerializedProperty statDefinitions = serializedProfile.FindProperty("statDefinitions");
            statDefinitions.ClearArray();
            AddStatDefinition(statDefinitions, CCS_SurvivalStatType.Health, 0f, 100f, 100f);
            AddStatDefinition(statDefinitions, CCS_SurvivalStatType.Stamina, 0f, 100f, 100f);
            AddStatDefinition(statDefinitions, CCS_SurvivalStatType.Hunger, 0f, 100f, 100f);
            AddStatDefinition(statDefinitions, CCS_SurvivalStatType.Thirst, 0f, 100f, 100f);
            AddStatDefinition(statDefinitions, CCS_SurvivalStatType.Temperature, 0f, 100f, 50f);
            AddStatDefinition(statDefinitions, CCS_SurvivalStatType.Fatigue, 0f, 100f, 0f);

            SerializedProperty decayDefinitions = serializedProfile.FindProperty("decayDefinitions");
            decayDefinitions.ClearArray();
            AddDecayDefinition(decayDefinitions, CCS_SurvivalStatType.Hunger, true, 0.01f, false, 50f);
            AddDecayDefinition(decayDefinitions, CCS_SurvivalStatType.Thirst, true, 0.015f, false, 50f);
            AddDecayDefinition(decayDefinitions, CCS_SurvivalStatType.Fatigue, false, 0.008f, false, 50f);
            AddDecayDefinition(decayDefinitions, CCS_SurvivalStatType.Temperature, true, 0.02f, true, 50f);

            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddStatDefinition(
            SerializedProperty statDefinitions,
            CCS_SurvivalStatType statType,
            float minValue,
            float maxValue,
            float startingValue)
        {
            int index = statDefinitions.arraySize;
            statDefinitions.InsertArrayElementAtIndex(index);
            SerializedProperty element = statDefinitions.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("statType").enumValueIndex = (int)statType;
            element.FindPropertyRelative("minValue").floatValue = minValue;
            element.FindPropertyRelative("maxValue").floatValue = maxValue;
            element.FindPropertyRelative("startingValue").floatValue = startingValue;
        }

        private static void AddDecayDefinition(
            SerializedProperty decayDefinitions,
            CCS_SurvivalStatType statType,
            bool subtractPerSecond,
            float changePerSecond,
            bool useTemperatureComfortDrift,
            float temperatureComfortTarget)
        {
            int index = decayDefinitions.arraySize;
            decayDefinitions.InsertArrayElementAtIndex(index);
            SerializedProperty element = decayDefinitions.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("statType").enumValueIndex = (int)statType;
            element.FindPropertyRelative("subtractPerSecond").boolValue = subtractPerSecond;
            element.FindPropertyRelative("changePerSecond").floatValue = changePerSecond;
            element.FindPropertyRelative("useTemperatureComfortDrift").boolValue = useTemperatureComfortDrift;
            element.FindPropertyRelative("temperatureComfortTarget").floatValue = temperatureComfortTarget;
        }

        #endregion
    }
}
