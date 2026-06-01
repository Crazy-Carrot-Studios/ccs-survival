using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_BootstrapSceneValidationUtility
// CATEGORY: Survival / Editor / Development / Validation
// PURPOSE: Validates bootstrap scene playable ground collider and player spawn clearance.
// PLACEMENT: Called by foundation and character controller validators.
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: Fails when bootstrap scene has no solid ground collider for CharacterController.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public static class CCS_BootstrapSceneValidationUtility
    {
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string GroundName = "CCS_BootstrapTestGround";
        private const string LegacyGroundName = "CCS_BuildVerificationGround";
        private const string PlayerName = "PF_CCS_Player";
        private const float MinimumSpawnClearance = 0.01f;

        #region Public Methods

        public static void ValidatePlayableGround(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(BootstrapScenePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Ground Collider",
                    $"Missing bootstrap scene: {BootstrapScenePath}");
                return;
            }

            Scene activeScene = EditorSceneManager.GetActiveScene();
            bool openedBootstrapScene = false;
            Scene bootstrapScene = activeScene;

            if (activeScene.path != BootstrapScenePath)
            {
                bootstrapScene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Additive);
                openedBootstrapScene = true;
            }

            try
            {
                GameObject groundObject = FindSceneObject(GroundName) ?? FindSceneObject(LegacyGroundName);
                if (groundObject == null)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Bootstrap Ground Collider",
                        $"Bootstrap scene is missing {GroundName} (or legacy {LegacyGroundName}).");
                    return;
                }

                if (groundObject.name != GroundName)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Warning,
                        "Bootstrap Ground Collider",
                        $"Ground object should be named {GroundName}. Found '{groundObject.name}'.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Bootstrap Ground Collider",
                        $"Bootstrap scene contains {GroundName}.");
                }

                Collider groundCollider = groundObject.GetComponent<Collider>();
                if (groundCollider == null)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Bootstrap Ground Collider",
                        $"{groundObject.name} is missing a collider.");
                    return;
                }

                if (!groundCollider.enabled)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Bootstrap Ground Collider",
                        $"{groundObject.name} collider is disabled.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Bootstrap Ground Collider",
                        $"{groundObject.name} collider is enabled.");
                }

                if (groundCollider.isTrigger)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Bootstrap Ground Collider",
                        $"{groundObject.name} collider must not be a trigger.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Bootstrap Ground Collider",
                        $"{groundObject.name} collider is solid (not trigger).");
                }

                GameObject playerObject = FindSceneObject(PlayerName);
                if (playerObject == null)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Bootstrap Player Spawn",
                        $"Bootstrap scene is missing {PlayerName}.");
                    return;
                }

                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Bootstrap Player Spawn",
                    $"Bootstrap scene contains {PlayerName}.");

                CharacterController characterController = playerObject.GetComponent<CharacterController>();
                if (characterController == null)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Bootstrap Player Spawn",
                        $"{PlayerName} is missing CharacterController.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Bootstrap Player Spawn",
                        $"{PlayerName} includes CharacterController.");
                }

                if (playerObject.GetComponent<Rigidbody>() != null)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Bootstrap Player Spawn",
                        $"{PlayerName} must not include Rigidbody.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Bootstrap Player Spawn",
                        $"{PlayerName} has no Rigidbody.");
                }

                if (characterController == null || groundCollider == null)
                {
                    return;
                }

                float controllerBottomY = playerObject.transform.position.y
                    + characterController.center.y
                    - (characterController.height * 0.5f);
                float groundTopY = groundCollider.bounds.max.y;

                if (controllerBottomY <= groundTopY + MinimumSpawnClearance)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Bootstrap Player Spawn",
                        $"{PlayerName} spawn bottom ({controllerBottomY:F3}) is not above ground collider top ({groundTopY:F3}).");
                    return;
                }

                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Bootstrap Player Spawn",
                    $"{PlayerName} spawn clears ground collider (bottom {controllerBottomY:F3} > top {groundTopY:F3}).");
            }
            finally
            {
                if (openedBootstrapScene && bootstrapScene.IsValid())
                {
                    EditorSceneManager.CloseScene(bootstrapScene, true);
                }
            }
        }

        #endregion

        #region Private Methods

        private static GameObject FindSceneObject(string objectName)
        {
            GameObject foundObject = GameObject.Find(objectName);
            if (foundObject != null)
            {
                return foundObject;
            }

            GameObject[] sceneObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int index = 0; index < sceneObjects.Length; index++)
            {
                if (sceneObjects[index].name == objectName)
                {
                    return sceneObjects[index];
                }
            }

            return null;
        }

        #endregion
    }
}
