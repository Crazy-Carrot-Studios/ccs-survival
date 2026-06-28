using CCS.Modules.CharacterController.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AIBanditMasterTestBuilder
// CATEGORY: Modules / AI / Editor
// PURPOSE: Ensures single AI spawner exists in Master Test scene and is wired.
// PLACEMENT: Editor utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Called from AI batch entry to avoid cross-assembly circular references.
// =============================================================================

namespace CCS.Modules.AI.Editor
{
    public static class CCS_AIBanditMasterTestBuilder
    {
        public static bool EnsureMasterTestBanditSpawner()
        {
            var scene = EditorSceneManager.OpenScene(
                CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath,
                OpenSceneMode.Single);

            if (!scene.IsValid())
            {
                Debug.LogError("[AI Builder] Could not open Master Test scene.");
                return false;
            }

            bool changed = false;
            GameObject spawnerObject = GameObject.Find(CCS_AIConstants.AIBanditSpawnerObjectName);
            if (spawnerObject == null)
            {
                spawnerObject = new GameObject(CCS_AIConstants.AIBanditSpawnerObjectName);
                changed = true;
            }

            CCS_AIBanditSpawner spawner = spawnerObject.GetComponent<CCS_AIBanditSpawner>();
            if (spawner == null)
            {
                spawner = spawnerObject.AddComponent<CCS_AIBanditSpawner>();
                changed = true;
            }

            SerializedObject serializedSpawner = new SerializedObject(spawner);
            bool propertiesChanged = false;
            propertiesChanged |= SetObjectReference(
                serializedSpawner,
                "aiBanditPrefab",
                AssetDatabase.LoadAssetAtPath<GameObject>(CCS_AIConstants.AIBanditPrefabPath));
            Transform hostSpawn = GameObject.Find("TP_Spawn_Host")?.transform;
            propertiesChanged |= SetObjectReference(serializedSpawner, "spawnReference", hostSpawn);
            propertiesChanged |= SetVector3(
                serializedSpawner,
                "spawnOffset",
                new Vector3(
                    CCS_AIConstants.DefaultSpawnSideOffset,
                    0f,
                    CCS_AIConstants.DefaultSpawnDistanceFromPlayer));

            if (propertiesChanged)
            {
                serializedSpawner.ApplyModifiedPropertiesWithoutUndo();
                changed = true;
            }

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            return changed;
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

        private static bool SetVector3(SerializedObject serializedObject, string propertyName, Vector3 value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.vector3Value == value)
            {
                return false;
            }

            property.vector3Value = value;
            return true;
        }
    }
}
