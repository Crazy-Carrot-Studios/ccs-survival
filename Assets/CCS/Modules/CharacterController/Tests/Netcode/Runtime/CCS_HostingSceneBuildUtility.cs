using System.IO;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_HostingSceneBuildUtility
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Runtime-safe build settings checks for hosting and master test scenes.
// PLACEMENT: Runtime utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-19
// NOTES: Uses SceneUtility only. No EditorBuildSettings or AssetDatabase.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode
{
    public static class CCS_HostingSceneBuildUtility
    {
        #region Public Methods

        public static bool IsSceneInBuildSettings(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return false;
            }

            int sceneCount = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < sceneCount; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                if (string.IsNullOrEmpty(scenePath))
                {
                    continue;
                }

                string buildSceneName = Path.GetFileNameWithoutExtension(scenePath);
                if (buildSceneName == sceneName)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
