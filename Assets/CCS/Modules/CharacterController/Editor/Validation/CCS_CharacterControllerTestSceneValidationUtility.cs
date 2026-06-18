using System.Collections.Generic;
using CCS.Modules.CharacterController;
using CCS.Project;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_CharacterControllerTestSceneValidationUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Editor validation for module test scene ground setup (v0.2.1).
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Validates reusable ground prefab, scene instance, and preview scene content.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerTestSceneValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateTestSceneAssetFiles()
        {
            List<string> failures = new List<string>();

            if (!System.IO.File.Exists(CCS_CharacterControllerConstants.TestScenePath))
            {
                failures.Add("Test scene asset is missing.");
            }

            if (!System.IO.File.Exists(CCS_CharacterControllerConstants.TestGroundPrefabPath))
            {
                failures.Add("Test ground prefab is missing.");
            }

            if (!System.IO.File.Exists(CCS_CharacterControllerConstants.TestGroundGridMaterialPath))
            {
                failures.Add("Test ground material is missing.");
            }

            if (!System.IO.File.Exists(CCS_CharacterControllerConstants.TestGroundGridTexturePath))
            {
                failures.Add("Test ground grid texture is missing.");
            }

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            return CCS_SurvivalValidationResult.Pass("Test scene asset files validated.");
        }

        public static CCS_SurvivalValidationResult ValidateTestSceneContent()
        {
            CCS_SurvivalValidationResult fileValidation = ValidateTestSceneAssetFiles();
            if (!fileValidation.IsSuccess)
            {
                return fileValidation;
            }

            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_CharacterControllerConstants.TestGroundPrefabPath);
            if (prefabRoot == null)
            {
                return CCS_SurvivalValidationResult.Fail("Test ground prefab could not be loaded.");
            }

            CCS_SurvivalValidationResult prefabValidation = ValidateGroundPrefab(prefabRoot);
            if (!prefabValidation.IsSuccess)
            {
                return prefabValidation;
            }

            Material expectedMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                CCS_CharacterControllerConstants.TestGroundGridMaterialPath);
            if (expectedMaterial == null)
            {
                return CCS_SurvivalValidationResult.Fail("Test ground material could not be loaded.");
            }

            Vector2 baseMapScale = expectedMaterial.GetTextureScale("_BaseMap");
            if (Mathf.Abs(baseMapScale.x - CCS_CharacterControllerConstants.TestGroundMaterialTiling) > 0.01f
                || Mathf.Abs(baseMapScale.y - CCS_CharacterControllerConstants.TestGroundMaterialTiling) > 0.01f)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Ground material tiling must be {CCS_CharacterControllerConstants.TestGroundMaterialTiling} for 1m grid cells across the 200m plane.");
            }

            Scene activeScene = SceneManager.GetActiveScene();
            bool closeSceneAfterValidation = false;
            Scene testScene = activeScene;

            if (activeScene.path != CCS_CharacterControllerConstants.TestScenePath)
            {
                testScene = EditorSceneManager.OpenScene(
                    CCS_CharacterControllerConstants.TestScenePath,
                    OpenSceneMode.Additive);
                closeSceneAfterValidation = true;
            }

            List<string> failures = new List<string>();
            GameObject ground = FindRootObjectByName(testScene, CCS_CharacterControllerConstants.TestGroundObjectName);
            if (ground == null)
            {
                failures.Add($"Ground object '{CCS_CharacterControllerConstants.TestGroundObjectName}' is missing.");
            }
            else
            {
                if (!PrefabUtility.IsPartOfPrefabInstance(ground))
                {
                    failures.Add("Ground object must be an instance of PF_CCS_TestGround_OneMeterGrid.");
                }
                else
                {
                    GameObject sourceRoot = PrefabUtility.GetCorrespondingObjectFromSource(ground);
                    if (sourceRoot == null || sourceRoot.name != CCS_CharacterControllerConstants.TestGroundPrefabName)
                    {
                        failures.Add("Ground object must reference PF_CCS_TestGround_OneMeterGrid.");
                    }
                }

                Vector3 scale = ground.transform.localScale;
                if (!Mathf.Approximately(scale.x, CCS_CharacterControllerConstants.TestGroundPlaneScale)
                    || !Mathf.Approximately(scale.z, CCS_CharacterControllerConstants.TestGroundPlaneScale))
                {
                    failures.Add("Ground plane scale must be (20, 1, 20) for a 200m x 200m field.");
                }

                MeshRenderer renderer = ground.GetComponent<MeshRenderer>();
                if (renderer == null || renderer.sharedMaterial != expectedMaterial)
                {
                    failures.Add("Ground material is not assigned to M_CCS_TestGround_1mGrid.");
                }
            }

            if (FindRootObjectByName(testScene, "Directional Light") == null)
            {
                failures.Add("Directional Light is missing from test scene.");
            }

            GameObject camera = FindRootObjectByName(testScene, "Main Camera");
            if (camera == null)
            {
                failures.Add("Preview Main Camera is missing from test scene.");
            }

            if (FindRootObjectByName(testScene, CCS_CharacterControllerConstants.TestSceneLabelObjectName) == null)
            {
                failures.Add("Test scene label is missing.");
            }

            if (SceneContainsComponent<CCS_CharacterMotor>(testScene)
                || SceneContainsComponent<CCS_CharacterControllerService>(testScene))
            {
                failures.Add("Test player should not be present in the ground-only test scene.");
            }

            if (closeSceneAfterValidation)
            {
                EditorSceneManager.CloseScene(testScene, true);
            }

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            return CCS_SurvivalValidationResult.Pass("Character controller test scene ground validated.");
        }

        #endregion

        #region Private Methods

        private static CCS_SurvivalValidationResult ValidateGroundPrefab(GameObject prefabRoot)
        {
            List<string> failures = new List<string>();

            if (prefabRoot.name != CCS_CharacterControllerConstants.TestGroundPrefabName)
            {
                failures.Add("Ground prefab root name must be PF_CCS_TestGround_OneMeterGrid.");
            }

            Vector3 scale = prefabRoot.transform.localScale;
            if (!Mathf.Approximately(scale.x, CCS_CharacterControllerConstants.TestGroundPlaneScale)
                || !Mathf.Approximately(scale.z, CCS_CharacterControllerConstants.TestGroundPlaneScale))
            {
                failures.Add("Ground prefab scale must be (20, 1, 20).");
            }

            Material expectedMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                CCS_CharacterControllerConstants.TestGroundGridMaterialPath);
            MeshRenderer renderer = prefabRoot.GetComponent<MeshRenderer>();
            if (renderer == null || renderer.sharedMaterial != expectedMaterial)
            {
                failures.Add("Ground prefab must use M_CCS_TestGround_1mGrid.");
            }

            if (prefabRoot.GetComponent<MeshCollider>() == null)
            {
                failures.Add("Ground prefab must include a MeshCollider.");
            }

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            return CCS_SurvivalValidationResult.Pass("Test ground prefab validated.");
        }

        private static bool SceneContainsComponent<T>(Scene scene) where T : Component
        {
            if (!scene.isLoaded)
            {
                return false;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].GetComponentInChildren<T>(true) != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static GameObject FindRootObjectByName(Scene scene, string objectName)
        {
            if (!scene.isLoaded)
            {
                return null;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == objectName)
                {
                    return roots[i];
                }
            }

            return null;
        }

        #endregion
    }
}
