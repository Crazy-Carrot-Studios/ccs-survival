using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_BootstrapGroundColliderBootstrapSetup
// CATEGORY: Survival / Editor / Development / Bootstrap
// PURPOSE: Ensures bootstrap test ground has a solid collider and player spawns above it.
// PLACEMENT: Batch entry for 0.9.2a bootstrap ground collider hotfix.
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: Does not change gameplay progression systems.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public static class CCS_BootstrapGroundColliderBootstrapSetup
    {
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string SceneRootName = "CCS_BuildVerificationScene";
        private const string GroundName = "CCS_BootstrapTestGround";
        private const string LegacyGroundName = "CCS_BuildVerificationGround";
        private const string PlayerName = "PF_CCS_Player";
        private const float PlayerSpawnClearanceY = 0.1f;
        private const float GroundColliderHeight = 0.2f;
        private const string LogPrefix = "[CCS_BootstrapGroundColliderBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            if (!System.IO.File.Exists(BootstrapScenePath))
            {
                Debug.LogError($"{LogPrefix} Missing bootstrap scene: {BootstrapScenePath}");
                EditorApplication.Exit(1);
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            GameObject groundObject = EnsureBootstrapTestGround();
            EnsurePlayerSpawnAboveGround();

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                Debug.LogError($"{LogPrefix} Failed to save bootstrap scene.");
                EditorApplication.Exit(1);
                return;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Bootstrap ground collider hotfix complete on '{groundObject.name}'.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static GameObject EnsureBootstrapTestGround()
        {
            GameObject sceneRoot = GameObject.Find(SceneRootName);
            if (sceneRoot == null)
            {
                sceneRoot = new GameObject(SceneRootName);
            }

            Transform groundTransform = sceneRoot.transform.Find(GroundName);
            if (groundTransform == null)
            {
                groundTransform = sceneRoot.transform.Find(LegacyGroundName);
            }

            GameObject groundObject;
            if (groundTransform != null)
            {
                groundObject = groundTransform.gameObject;
            }
            else
            {
                groundObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
                groundObject.transform.SetParent(sceneRoot.transform, false);
            }

            groundObject.name = GroundName;
            groundObject.transform.localPosition = Vector3.zero;
            groundObject.transform.localRotation = Quaternion.identity;
            groundObject.transform.localScale = new Vector3(2f, 1f, 2f);
            GameObjectUtility.SetStaticEditorFlags(groundObject, StaticEditorFlags.BatchingStatic);

            MeshCollider meshCollider = groundObject.GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                Object.DestroyImmediate(meshCollider);
            }

            BoxCollider boxCollider = groundObject.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = groundObject.AddComponent<BoxCollider>();
            }

            boxCollider.enabled = true;
            boxCollider.isTrigger = false;
            boxCollider.size = new Vector3(10f, GroundColliderHeight, 10f);
            boxCollider.center = new Vector3(0f, -GroundColliderHeight * 0.5f, 0f);

            MeshRenderer meshRenderer = groundObject.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = true;
            }

            return groundObject;
        }

        private static void EnsurePlayerSpawnAboveGround()
        {
            GameObject playerObject = GameObject.Find(PlayerName);
            if (playerObject == null)
            {
                Debug.LogWarning($"{LogPrefix} Bootstrap scene is missing {PlayerName}.");
                return;
            }

            if (playerObject.GetComponent<Rigidbody>() != null)
            {
                Debug.LogError($"{LogPrefix} {PlayerName} must not include a Rigidbody.");
                EditorApplication.Exit(1);
                return;
            }

            if (playerObject.GetComponent<CharacterController>() == null)
            {
                Debug.LogError($"{LogPrefix} {PlayerName} must include a CharacterController.");
                EditorApplication.Exit(1);
                return;
            }

            Vector3 spawnPosition = playerObject.transform.position;
            float minimumSpawnY = PlayerSpawnClearanceY;
            if (spawnPosition.y < minimumSpawnY)
            {
                spawnPosition.y = minimumSpawnY;
                playerObject.transform.position = spawnPosition;
            }

            CharacterController characterController = playerObject.GetComponent<CharacterController>();
            float controllerBottomY = spawnPosition.y + characterController.center.y - (characterController.height * 0.5f);
            if (controllerBottomY <= 0f)
            {
                spawnPosition.y += 0.1f - controllerBottomY;
                playerObject.transform.position = spawnPosition;
            }
        }

        #endregion
    }
}
