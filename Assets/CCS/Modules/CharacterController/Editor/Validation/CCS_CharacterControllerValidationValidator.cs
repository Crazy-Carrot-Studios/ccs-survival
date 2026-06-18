using CCS.Modules.CharacterController;
using CCS.Project;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

// =============================================================================
// SCRIPT: CCS_CharacterControllerValidationValidator
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Editor-side orchestration for character controller module validation.
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Loads module assets by path and reports validation results in the console.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerValidationValidator
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateAll()
        {
            CCS_SurvivalValidationResult foundationValidation =
                CCS_CharacterControllerValidationUtility.ValidateModuleFoundation();
            if (!foundationValidation.IsSuccess)
            {
                return foundationValidation;
            }

            InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                CCS_CharacterControllerConstants.InputActionsAssetPath);
            CCS_SurvivalValidationResult inputValidation =
                CCS_CharacterControllerValidationUtility.ValidateInputActionsAsset(inputActions);
            if (!inputValidation.IsSuccess)
            {
                return inputValidation;
            }

            CCS_CharacterMovementProfile movementProfile = AssetDatabase.LoadAssetAtPath<CCS_CharacterMovementProfile>(
                CCS_CharacterControllerConstants.DefaultMovementProfilePath);
            CCS_SurvivalValidationResult movementValidation =
                CCS_CharacterControllerValidationUtility.ValidateMovementProfile(movementProfile);
            if (!movementValidation.IsSuccess)
            {
                return movementValidation;
            }

            CCS_CharacterCameraProfile cameraProfile = AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfile>(
                CCS_CharacterControllerConstants.DefaultCameraProfilePath);
            CCS_SurvivalValidationResult cameraValidation =
                CCS_CharacterControllerValidationUtility.ValidateCameraProfile(cameraProfile);
            if (!cameraValidation.IsSuccess)
            {
                return cameraValidation;
            }

            CCS_CharacterCameraProfileSet cameraProfileSet = AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfileSet>(
                CCS_CharacterControllerConstants.DefaultCameraProfileSetPath);
            CCS_SurvivalValidationResult setValidation =
                CCS_CharacterControllerValidationUtility.ValidateCameraProfileSet(cameraProfileSet);
            if (!setValidation.IsSuccess)
            {
                return setValidation;
            }

            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_CharacterControllerConstants.TestPrefabPath);
            CCS_SurvivalValidationResult prefabValidation =
                CCS_CharacterControllerValidationUtility.ValidateTestPrefab(prefabRoot);
            if (!prefabValidation.IsSuccess)
            {
                return prefabValidation;
            }

            return CCS_CharacterControllerTestSceneValidationUtility.ValidateTestSceneContent();
        }

        #endregion
    }
}
