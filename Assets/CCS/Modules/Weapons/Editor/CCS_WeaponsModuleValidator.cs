using System.Collections.Generic;

using System.IO;

using System.Text.RegularExpressions;

using CCS.Modules.CharacterController;

using CCS.Project;

using UnityEditor;

using UnityEditor.SceneManagement;

using UnityEngine;

using UnityEngine.SceneManagement;



// =============================================================================

// SCRIPT: CCS_WeaponsModuleValidator

// CATEGORY: Modules / Weapons / Editor

// PURPOSE: Validates Weapons module foundation, assets, and test integration wiring.

// PLACEMENT: Editor validator invoked from CCS/Weapons/Validate Weapons Module.

// AUTHOR: James Schilz

// CREATED: 2026-06-07

// NOTES: v0.6.5 world-pickup-only scope. Holster/equipped visuals deferred.

// =============================================================================



namespace CCS.Modules.Weapons.Editor

{

    public static class CCS_WeaponsModuleValidator

    {

        #region Public Methods



        public static CCS_SurvivalValidationResult ValidateWeaponsModule()

        {

            List<string> failures = new List<string>();



            CCS_SurvivalValidationResult foundationResult = CCS_WeaponsValidationUtility.ValidateModuleFoundation();

            AppendResult(failures, foundationResult);



            CCS_RevolverDefinition revolverDefinition = AssetDatabase.LoadAssetAtPath<CCS_RevolverDefinition>(

                CCS_WeaponsConstants.RevolverDefinitionProfilePath);

            AppendIfMissing(

                failures,

                File.Exists(CCS_WeaponsConstants.RevolverDefinitionProfilePath),

                $"Missing revolver definition asset at {CCS_WeaponsConstants.RevolverDefinitionProfilePath}.");

            AppendResult(failures, CCS_WeaponsValidationUtility.ValidateRevolverDefinition(revolverDefinition));



            GameObject testPlayerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(

                CCS_WeaponsConstants.NetworkedTestPlayerPrefabPath);

            AppendIfMissing(

                failures,

                testPlayerPrefab != null,

                $"Missing networked test player prefab at {CCS_WeaponsConstants.NetworkedTestPlayerPrefabPath}.");

            AppendResult(

                failures,

                CCS_WeaponsValidationUtility.ValidatePlayerRevolverComponents(testPlayerPrefab));

            AppendResult(failures, CCS_WeaponsValidationUtility.ValidateRevolverFireFeedbackSourceContract());

            AppendResult(failures, CCS_WeaponsValidationUtility.ValidateRevolverFireVisualsFoundation());

            AppendResult(failures, CCS_WeaponsValidationUtility.ValidateHitscanUsesCameraCenterAim());

            AppendResult(failures, CCS_WeaponsValidationUtility.ValidateWeaponAimResolverFoundation());

            AppendResult(failures, CCS_WeaponsValidationUtility.ValidateWeaponAimConvergenceFoundation());

            AppendResult(failures, CCS_WeaponsValidationUtility.ValidateRevolverArmReticleIKFoundation(testPlayerPrefab));

            AppendResult(failures, CCS_WeaponsEditorAimValidationUtility.ValidateVisualOnlyMuzzlePointOrientation());

            AppendResult(failures, CCS_WeaponsValidationUtility.ValidateRevolverM1879VisualFoundation());

            AppendResult(failures, CCS_WeaponsValidationUtility.ValidatePlayerWeaponLoadoutComponents(testPlayerPrefab));

            AppendResult(failures, CCS_WeaponsValidationUtility.ValidateRevolverOwnershipAndMuzzleContract());

            ValidateRevolverMuzzleReference(failures, testPlayerPrefab);

            ValidateAimLocomotionWeaponGate(failures, testPlayerPrefab);



            ValidateInputActions(failures);

            ValidateNoLegacyInputUsage(failures);

            ValidateMasterTestWeaponTarget(failures);

            ValidateMasterTestRevolverPickup(failures);

            ValidateMasterTestHasNoLooseRevolverObjects(failures);

            AppendResult(
                failures,
                CCS_EquipmentSocketValidationUtility.ValidateAnimationRiggingPackageInstalled());

            if (testPlayerPrefab != null)
            {
                AppendResult(
                    failures,
                    CCS_EquipmentSocketValidationUtility.ValidatePlayerEquipmentSocketFoundation(testPlayerPrefab));
            }



            return failures.Count > 0

                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))

                : CCS_SurvivalValidationResult.Pass(

                    "Weapons module foundation, revolver profile, test prefabs, input actions, and scene wiring are valid.");

        }



        #endregion



        #region Private Methods



        private static void ValidateRevolverMuzzleReference(List<string> failures, GameObject testPlayerPrefab)

        {

            if (testPlayerPrefab == null)

            {

                return;

            }



            CCS_RevolverController revolverController = testPlayerPrefab.GetComponent<CCS_RevolverController>();

            if (revolverController == null)

            {

                return;

            }



            SerializedObject serializedController = new SerializedObject(revolverController);

            SerializedProperty muzzleProperty = serializedController.FindProperty("muzzlePoint");

            AppendIfMissing(

                failures,

                muzzleProperty != null && muzzleProperty.objectReferenceValue != null,

                "Test player revolver controller must assign MuzzlePoint.");

        }



        private static void ValidateAimLocomotionWeaponGate(List<string> failures, GameObject testPlayerPrefab)

        {

            if (testPlayerPrefab == null)

            {

                return;

            }



            CCS_CharacterAimLocomotionController aimLocomotion =

                testPlayerPrefab.GetComponent<CCS_CharacterAimLocomotionController>();

            if (aimLocomotion == null)

            {

                return;

            }



            SerializedObject serializedAim = new SerializedObject(aimLocomotion);

            SerializedProperty gateProperty = serializedAim.FindProperty("weaponAimGateComponent");

            AppendIfMissing(

                failures,

                gateProperty != null
                    && gateProperty.objectReferenceValue is CCS_WeaponCarryStateController,

                "Aim locomotion must wire weaponAimGateComponent to CCS_WeaponCarryStateController.");

        }



        private static void ValidateMasterTestHasNoLooseRevolverObjects(List<string> failures)

        {

            Scene scene = EditorSceneManager.OpenScene(

                CCS_WeaponsConstants.MasterTestScenePath,

                OpenSceneMode.Single);

            if (!scene.IsValid())

            {

                failures.Add($"Could not open master test scene at {CCS_WeaponsConstants.MasterTestScenePath}.");

                return;

            }



            GameObject[] roots = scene.GetRootGameObjects();

            for (int i = 0; i < roots.Length; i++)

            {

                ValidateNoLooseRevolverHierarchy(failures, roots[i]);

            }

        }



        private static void ValidateNoLooseRevolverHierarchy(List<string> failures, GameObject root)

        {

            if (root == null)

            {

                return;

            }



            if (IsLooseRevolverSceneObject(root))

            {

                failures.Add("Master test scene must not contain loose object: " + root.name + ".");

                return;

            }



            for (int i = 0; i < root.transform.childCount; i++)

            {

                ValidateNoLooseRevolverHierarchy(failures, root.transform.GetChild(i).gameObject);

            }

        }



        private static bool IsLooseRevolverSceneObject(GameObject sceneObject)

        {

            if (sceneObject == null || sceneObject.GetComponent<CCS_WeaponPickupInteractable>() != null)

            {

                return false;

            }



            string objectName = sceneObject.name;

            if (objectName == "ReichsrevolverM1879"

                || objectName == "ReichsrevolverM1879Shell"

                || objectName.StartsWith("ReichsrevolverM1879"))

            {

                return true;

            }



            if (objectName == "PF_CCS_RevolverM1879_Holstered_Instance"

                || objectName == "PF_CCS_RevolverM1879_Equipped_Instance")

            {

                return true;

            }



            Object source = PrefabUtility.GetCorrespondingObjectFromSource(sceneObject);

            return source != null && source.name == "ReichsrevolverM1879";

        }



        private static void ValidateMasterTestRevolverPickup(List<string> failures)

        {

            Scene scene = EditorSceneManager.OpenScene(

                CCS_WeaponsConstants.MasterTestScenePath,

                OpenSceneMode.Single);

            if (!scene.IsValid())

            {

                failures.Add($"Could not open master test scene at {CCS_WeaponsConstants.MasterTestScenePath}.");

                return;

            }



            CCS_WeaponPickupInteractable[] pickups = Object.FindObjectsByType<CCS_WeaponPickupInteractable>(

                FindObjectsInactive.Include,

                FindObjectsSortMode.None);

            int scenePickupCount = 0;

            for (int i = 0; i < pickups.Length; i++)

            {

                CCS_WeaponPickupInteractable pickup = pickups[i];

                if (pickup != null && pickup.gameObject.scene == scene)

                {

                    scenePickupCount++;

                }

            }



            AppendIfMissing(

                failures,

                scenePickupCount == 1,

                "Master test scene must contain exactly one revolver world pickup.");

        }



        private static void ValidateInputActions(List<string> failures)

        {

            string inputActionsPath = CCS_CharacterControllerConstants.InputActionsAssetPath;

            AppendIfMissing(

                failures,

                File.Exists(inputActionsPath),

                $"Missing CharacterController input actions asset at {inputActionsPath}.");



            if (!File.Exists(inputActionsPath))

            {

                return;

            }



            string inputActionsText = File.ReadAllText(inputActionsPath);

            AppendIfMissing(

                failures,

                inputActionsText.Contains("\"name\": \"Aim\""),

                "CharacterController input actions must define Aim.");

            AppendIfMissing(

                failures,

                inputActionsText.Contains("\"name\": \"Fire\""),

                "CharacterController input actions must define Fire.");

            AppendIfMissing(

                failures,

                inputActionsText.Contains("\"name\": \"Reload\""),

                "CharacterController input actions must define Reload.");

            AppendIfMissing(

                failures,

                inputActionsText.Contains("<Mouse>/rightButton"),

                "Aim must bind to mouse right button.");

            AppendIfMissing(

                failures,

                inputActionsText.Contains("<Mouse>/leftButton"),

                "Fire must bind to mouse left button.");

            AppendIfMissing(

                failures,

                inputActionsText.Contains("<Keyboard>/r"),

                "Reload must bind to keyboard R.");

        }



        private static void ValidateNoLegacyInputUsage(List<string> failures)

        {

            string runtimeRoot = CCS_WeaponsConstants.ModuleRootPath + "/Runtime";

            if (!Directory.Exists(runtimeRoot))

            {

                return;

            }



            string[] runtimeFiles = Directory.GetFiles(runtimeRoot, "*.cs", SearchOption.AllDirectories);

            Regex legacyInputPattern = new Regex(@"\bUnityEngine\.Input\b|\bInput\.Get");

            for (int i = 0; i < runtimeFiles.Length; i++)

            {

                string fileText = File.ReadAllText(runtimeFiles[i]);

                if (legacyInputPattern.IsMatch(fileText))

                {

                    failures.Add("Weapons runtime must not use legacy UnityEngine.Input: " + runtimeFiles[i]);

                }

            }

        }



        private static void ValidateMasterTestWeaponTarget(List<string> failures)

        {

            Scene scene = EditorSceneManager.OpenScene(

                CCS_WeaponsConstants.MasterTestScenePath,

                OpenSceneMode.Single);

            if (!scene.IsValid())

            {

                failures.Add($"Could not open master test scene at {CCS_WeaponsConstants.MasterTestScenePath}.");

                return;

            }



            CCS_TestDamageTarget[] targets = Object.FindObjectsByType<CCS_TestDamageTarget>(FindObjectsSortMode.None);

            int sceneTargetCount = 0;

            for (int i = 0; i < targets.Length; i++)

            {

                CCS_TestDamageTarget target = targets[i];

                if (target != null && target.gameObject.scene == scene)

                {

                    sceneTargetCount++;

                }

            }



            AppendIfMissing(

                failures,

                sceneTargetCount == 1,

                $"Master test scene must contain exactly one {CCS_WeaponsConstants.TestDamageTargetObjectName}.");

        }



        private static void AppendResult(List<string> failures, CCS_SurvivalValidationResult result)

        {

            if (!result.IsSuccess)

            {

                failures.Add(result.Message);

            }

        }



        private static void AppendIfMissing(List<string> failures, bool condition, string message)

        {

            if (!condition)

            {

                failures.Add(message);

            }

        }



        #endregion

    }

}


