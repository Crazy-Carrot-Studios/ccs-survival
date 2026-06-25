using System.IO;
using CCS.Modules.Attributes;
using CCS.Modules.CharacterController;
using CCS.Modules.Weapons;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AIBanditPrefabBuilder
// CATEGORY: Modules / AI / Editor
// PURPOSE: Builds PF_CCS_AI_Bandit_Networked from canonical network test player prefab.
// PLACEMENT: Editor utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Strips player-only control scripts and adds AI orchestration components.
// =============================================================================

namespace CCS.Modules.AI.Editor
{
    public static class CCS_AIBanditPrefabBuilder
    {
        public static bool EnsureAIBanditPrefab()
        {
            EnsureTargetFolder();
            EnsureDefaultProfileAsset();

            if (!File.Exists(CCS_AIConstants.AIBanditPrefabPath))
            {
                if (!AssetDatabase.CopyAsset(CCS_AIConstants.SourceNetworkedPlayerPrefabPath, CCS_AIConstants.AIBanditPrefabPath))
                {
                    Debug.LogError("[AI Prefab Builder] Failed to copy source player prefab.");
                    return false;
                }
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(CCS_AIConstants.AIBanditPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError("[AI Prefab Builder] Failed to load AI prefab contents.");
                return false;
            }

            bool changed = false;
            prefabRoot.name = CCS_AIConstants.AIBanditPrefabName;
            changed |= StripPlayerOnlyComponents(prefabRoot);
            changed |= EnsureCombatAndAiComponents(prefabRoot);

            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, CCS_AIConstants.AIBanditPrefabPath);
                AssetDatabase.SaveAssets();
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return changed;
        }

        private static void EnsureTargetFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/CCS/Modules/AI"))
            {
                AssetDatabase.CreateFolder("Assets/CCS/Modules", "AI");
            }

            if (!AssetDatabase.IsValidFolder("Assets/CCS/Modules/AI/Content"))
            {
                AssetDatabase.CreateFolder("Assets/CCS/Modules/AI", "Content");
            }

            if (!AssetDatabase.IsValidFolder("Assets/CCS/Modules/AI/Content/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets/CCS/Modules/AI/Content", "Prefabs");
            }
        }

        private static bool StripPlayerOnlyComponents(GameObject prefabRoot)
        {
            bool changed = false;
            changed |= DestroyIfPresent<CCS_CharacterInputActionProvider>(prefabRoot);
            changed |= DestroyIfPresent<CCS_RevolverHudPresenter>(prefabRoot);
            changed |= DestroyByTypeName(prefabRoot, "CCS_ControllerTestNetworkPlayerBehaviour");
            changed |= DestroyByTypeName(prefabRoot, "CCS_NetworkPlayerNameplate");
            changed |= DestroyByTypeName(prefabRoot, "CCS_PlayerNameplateBillboard");
            changed |= DestroyByTypeName(prefabRoot, "CCS_PlayerAttributeBarsHud");
            changed |= DestroyByTypeName(prefabRoot, "CCS_TestPlayerAttributeDebugInput");

            Canvas[] canvases = prefabRoot.GetComponentsInChildren<Canvas>(true);
            for (int i = 0; i < canvases.Length; i++)
            {
                if (canvases[i] != null && canvases[i].name == CCS_WeaponsConstants.WeaponHudRootName)
                {
                    Object.DestroyImmediate(canvases[i].gameObject, true);
                    changed = true;
                }
            }

            return changed;
        }

        private static bool DestroyByTypeName(GameObject root, string typeName)
        {
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            bool changed = false;
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                if (behaviour.GetType().Name == typeName)
                {
                    Object.DestroyImmediate(behaviour, true);
                    changed = true;
                }
            }

            return changed;
        }

        private static bool EnsureCombatAndAiComponents(GameObject prefabRoot)
        {
            bool changed = false;

            changed |= EnsureComponent<CCS_NetworkHealth>(prefabRoot, out CCS_NetworkHealth networkHealth);
            changed |= EnsureComponent<CCS_AITargetSensor>(prefabRoot, out _);
            changed |= EnsureComponent<CCS_AILineOfSightSensor>(prefabRoot, out _);
            changed |= EnsureComponent<CCS_AIMotorController>(prefabRoot, out _);
            changed |= EnsureComponent<CCS_AIWeaponController>(prefabRoot, out _);
            changed |= EnsureComponent<CCS_AIBanditBrain>(prefabRoot, out _);
            changed |= EnsureComponent<CCS_AIBanditController>(prefabRoot, out CCS_AIBanditController banditController);
            changed |= EnsureComponent<CCS_AIBanditNameplate>(prefabRoot, out _);

            CCS_RevolverController revolver = prefabRoot.GetComponent<CCS_RevolverController>();
            if (revolver != null)
            {
                revolver.SetWeaponOwnershipActive(true);
            }

            if (networkHealth != null)
            {
                CCS_AttributeDefinition healthDefinition =
                    AssetDatabase.LoadAssetAtPath<CCS_AttributeDefinition>(
                        CCS_AttributesConstants.HealthDefinitionPath);
                SerializedObject serializedHealth = new SerializedObject(networkHealth);
                bool healthChanged = SetObjectReference(
                    serializedHealth,
                    "attributeContainer",
                    prefabRoot.GetComponent<CCS_AttributeContainer>());
                healthChanged |= SetObjectReference(serializedHealth, "healthDefinition", healthDefinition);
                if (healthChanged)
                {
                    serializedHealth.ApplyModifiedPropertiesWithoutUndo();
                    changed = true;
                }
            }

            if (banditController != null)
            {
                SerializedObject serializedController = new SerializedObject(banditController);
                bool controllerChanged = SetObjectReference(serializedController, "networkHealth", networkHealth);
                controllerChanged |= SetObjectReference(
                    serializedController,
                    "profile",
                    AssetDatabase.LoadAssetAtPath<CCS_AIBanditProfile>(CCS_AIConstants.AIBanditProfilePath));
                if (controllerChanged)
                {
                    serializedController.ApplyModifiedPropertiesWithoutUndo();
                    changed = true;
                }
            }

            CCS_AIBanditBrain brain = prefabRoot.GetComponent<CCS_AIBanditBrain>();
            if (brain != null)
            {
                SerializedObject serializedBrain = new SerializedObject(brain);
                if (SetObjectReference(
                    serializedBrain,
                    "profile",
                    AssetDatabase.LoadAssetAtPath<CCS_AIBanditProfile>(CCS_AIConstants.AIBanditProfilePath)))
                {
                    serializedBrain.ApplyModifiedPropertiesWithoutUndo();
                    changed = true;
                }
            }

            return changed;
        }

        private static void EnsureDefaultProfileAsset()
        {
            if (!AssetDatabase.IsValidFolder("Assets/CCS/Modules/AI/Content/Profiles"))
            {
                AssetDatabase.CreateFolder("Assets/CCS/Modules/AI/Content", "Profiles");
            }

            CCS_AIBanditProfile existingProfile =
                AssetDatabase.LoadAssetAtPath<CCS_AIBanditProfile>(CCS_AIConstants.AIBanditProfilePath);
            if (existingProfile != null)
            {
                return;
            }

            CCS_AIBanditProfile profile = ScriptableObject.CreateInstance<CCS_AIBanditProfile>();
            AssetDatabase.CreateAsset(profile, CCS_AIConstants.AIBanditProfilePath);
            AssetDatabase.SaveAssets();
        }

        private static bool EnsureComponent<T>(GameObject root, out T component) where T : Component
        {
            component = root.GetComponent<T>();
            if (component != null)
            {
                return false;
            }

            component = root.AddComponent<T>();
            return true;
        }

        private static bool DestroyIfPresent<T>(GameObject root) where T : Component
        {
            T component = root.GetComponent<T>();
            if (component == null)
            {
                return false;
            }

            Object.DestroyImmediate(component, true);
            return true;
        }

        private static bool SetObjectReference(SerializedObject serializedObject, string propertyName, Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == value)
            {
                return false;
            }

            property.objectReferenceValue = value;
            return true;
        }
    }
}
