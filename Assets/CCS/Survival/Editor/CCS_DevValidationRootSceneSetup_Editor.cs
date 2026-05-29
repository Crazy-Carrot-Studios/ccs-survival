#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_DevValidationRootSceneSetup_Editor
// CATEGORY: Survival / Editor
// PURPOSE: Enables or disables CCS_DevValidationRoot for opt-in validation in bootstrap scene.
// PLACEMENT: Editor only. Menu CCS/Survival/Validation/*.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Does not toggle traversal test. Use CCS_TraversalTestAgent when dev root is enabled.
// =============================================================================

namespace CCS.Survival.Editor
{
    public static class CCS_DevValidationRootSceneSetup_Editor
    {
        private const string ScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string DevValidationRootName = "CCS_DevValidationRoot";

        [MenuItem("CCS/Survival/Validation/Enable Dev Validation Root")]
        public static void EnableDevValidationRoot()
        {
            SetDevValidationRootActive(true);
        }

        [MenuItem("CCS/Survival/Validation/Disable Dev Validation Root")]
        public static void DisableDevValidationRoot()
        {
            SetDevValidationRootActive(false);
        }

        private static void SetDevValidationRootActive(bool isActive)
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject devRoot = GameObject.Find(DevValidationRootName);
            if (devRoot == null)
            {
                Debug.LogError($"[CCS Survival] {DevValidationRootName} not found in {ScenePath}.");
                return;
            }

            devRoot.SetActive(isActive);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            string stateLabel = isActive ? "enabled" : "disabled";
            Debug.Log(
                $"[CCS Survival] {DevValidationRootName} {stateLabel}. " +
                "For traversal validation: enable traversal test on CCS_TraversalTestAgent.");
        }
    }
}
#endif
