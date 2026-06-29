using System.Collections.Generic;

using System.IO;

using System.Linq;

using CCS.Modules.CharacterController.Diagnostics;

using CCS.Modules.CharacterController.Local;

using CCS.Modules.CharacterController.Netcode;

using CCS.Project;

using UnityEditor;

using UnityEditor.SceneManagement;

using UnityEngine;

using UnityEngine.Rendering;

using UnityEngine.SceneManagement;



// =============================================================================

// SCRIPT: CCS_CharacterControllerPhase3AValidationUtility

// CATEGORY: Modules / CharacterController / Editor / Validation

// PURPOSE: Validates v0.7.2 Phase 3A productionization of Character Controller architecture.

// PLACEMENT: Editor validation utility. Not attached to GameObjects.

// AUTHOR: James Schilz

// CREATED: 2026-06-25

// NOTES: Fails if test/harness naming, Tests folder, or validation environment regress.

// =============================================================================



namespace CCS.Modules.CharacterController.Editor

{

    public static class CCS_CharacterControllerPhase3AValidationUtility

    {

        private const string CharacterControllerRoot = "Assets/CCS/Modules/CharacterController";

        private const string TestsFolderPath = CharacterControllerRoot + "/Tests";

        private const string DiagnosticsManagerPath =

            CharacterControllerRoot + "/Runtime/Diagnostics/CCS_CharacterControllerDiagnosticsManager.cs";

        private const string PrototypingRootPath = CharacterControllerRoot + "/Prototyping";

        private const string AnimationFitStudioRoot =

            CharacterControllerRoot + "/Editor/AnimationFitStudio";

        private const string EquipmentFitStudioWindowPath =

            CharacterControllerRoot + "/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioWindow.cs";



        public static CCS_SurvivalValidationResult ValidatePhase3AProductionization()

        {

            List<string> failures = new List<string>();

            List<string> warnings = new List<string>();



            ValidateTestsFolderRemoved(failures);

            ValidateNoTestsNamespacesInCharacterControllerCSharp(failures);

            ValidateNoTestsAsmdefs(failures);

            ValidateNoTestHarnessClassNames(failures);

            ValidateRuntimeAsmdefDoesNotReferenceTestsAsmdefs(failures);

            ValidatePlayerPrefabPaths(failures);

            ValidateAnimationFitStudioNotReintroduced(failures);

            ValidateEquipmentFitStudioPresent(failures);

            ValidateDiagnosticsManagerSource(failures);

            ValidateValidationSceneDiagnosticsManager(failures);

            ValidateNoLegacyTestingManagerComponents(failures);

            ValidateProductionMissingScripts(failures);

            ValidatePlayerPrefabMissingScripts(failures);

            ValidateNetworkManagerPlayerPrefabReference(failures);

            ValidateValidationSceneEnvironment(failures);

            CollectHistoricalNamingWarnings(warnings);



            if (failures.Count > 0)

            {

                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));

            }



            string message = "Character Controller Phase 3A productionization validated.";

            if (warnings.Count > 0)

            {

                message += " Warnings: " + string.Join(" ", warnings);

            }



            return CCS_SurvivalValidationResult.Pass(message);

        }



        private static void ValidateTestsFolderRemoved(List<string> failures)

        {

            AppendIfMissing(

                failures,

                !Directory.Exists(TestsFolderPath),

                "Character Controller Tests folder must be removed: " + TestsFolderPath);

            AppendIfMissing(

                failures,

                !File.Exists(TestsFolderPath + ".meta"),

                "Character Controller Tests.meta must be removed.");

        }



        private static void ValidateNoTestsNamespacesInCharacterControllerCSharp(List<string> failures)

        {

            System.Text.RegularExpressions.Regex namespaceRegex = new System.Text.RegularExpressions.Regex(

                @"^\s*namespace\s+CCS\.Modules\.CharacterController\.Tests\b",

                System.Text.RegularExpressions.RegexOptions.Multiline);



            string[] csharpFiles = Directory.GetFiles(CharacterControllerRoot, "*.cs", SearchOption.AllDirectories);

            for (int i = 0; i < csharpFiles.Length; i++)

            {

                string normalizedPath = csharpFiles[i].Replace('\\', '/');

                if (normalizedPath.EndsWith("/CCS_CharacterControllerPhase3AValidationUtility.cs"))

                {

                    continue;

                }



                string source = File.ReadAllText(csharpFiles[i]);

                if (namespaceRegex.IsMatch(source))

                {

                    failures.Add("Character Controller C# must not use namespace CCS.Modules.CharacterController.Tests: "

                        + normalizedPath);

                }

            }

        }



        private static void ValidateNoTestsAsmdefs(List<string> failures)

        {

            string[] asmdefFiles = Directory.GetFiles(CharacterControllerRoot, "*.asmdef", SearchOption.AllDirectories);

            for (int i = 0; i < asmdefFiles.Length; i++)

            {

                string fileName = Path.GetFileNameWithoutExtension(asmdefFiles[i]);

                if (fileName.Contains("CharacterController.Tests"))

                {

                    failures.Add("Character Controller asmdef must not contain CharacterController.Tests: "

                        + asmdefFiles[i].Replace('\\', '/'));

                }

            }

        }



        private static void ValidateNoTestHarnessClassNames(List<string> failures)

        {

            string[] csharpFiles = Directory.GetFiles(CharacterControllerRoot, "*.cs", SearchOption.AllDirectories);

            for (int i = 0; i < csharpFiles.Length; i++)

            {

                string fileName = Path.GetFileNameWithoutExtension(csharpFiles[i]);

                if (fileName.Contains("TestHarness"))

                {

                    failures.Add("Character Controller class/file must not contain TestHarness: " + fileName);

                }

            }

        }



        private static void ValidateRuntimeAsmdefDoesNotReferenceTestsAsmdefs(List<string> failures)

        {

            string runtimeAsmdefPath = CharacterControllerRoot + "/Runtime/CCS.Modules.CharacterController.Runtime.asmdef";

            if (!File.Exists(runtimeAsmdefPath))

            {

                failures.Add("Missing Character Controller Runtime asmdef.");

                return;

            }



            string source = File.ReadAllText(runtimeAsmdefPath);

            AppendIfMissing(

                failures,

                !source.Contains("CCS.Modules.CharacterController.Tests"),

                "Runtime asmdef must not reference CharacterController Tests assemblies.");

        }



        private static void ValidatePlayerPrefabPaths(List<string> failures)

        {

            AppendIfMissing(

                failures,

                File.Exists(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath),

                "Missing production networked player prefab at " + CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);

            AppendIfMissing(

                failures,

                !File.Exists(CCS_PlayerPrefabConstants.DeprecatedTestsNetworkedPlayerPrefabPath),

                "Deprecated Tests networked player prefab path must not exist: "

                    + CCS_PlayerPrefabConstants.DeprecatedTestsNetworkedPlayerPrefabPath);

            AppendIfMissing(

                failures,

                Directory.Exists(PrototypingRootPath),

                "Missing Character Controller Prototyping folder at " + PrototypingRootPath);

        }



        private static void ValidateAnimationFitStudioNotReintroduced(List<string> failures)

        {

            AppendIfMissing(

                failures,

                !Directory.Exists(AnimationFitStudioRoot),

                "Animation Fit Studio must remain removed.");

        }



        private static void ValidateEquipmentFitStudioPresent(List<string> failures)

        {

            AppendIfMissing(

                failures,

                File.Exists(EquipmentFitStudioWindowPath),

                "Equipment Fit Studio window must remain available.");

        }



        private static void ValidateDiagnosticsManagerSource(List<string> failures)

        {

            AppendIfMissing(

                failures,

                File.Exists(DiagnosticsManagerPath),

                "Missing CCS_CharacterControllerDiagnosticsManager at " + DiagnosticsManagerPath);

            AppendIfMissing(

                failures,

                !File.Exists(CharacterControllerRoot + "/Tests/Runtime/Managers/CCS_CharacterControllerTestingManager.cs"),

                "Legacy CCS_CharacterControllerTestingManager source must be removed after Phase 3A.");

        }



        private static void ValidateValidationSceneDiagnosticsManager(List<string> failures)

        {

            if (!File.Exists(CCS_NetcodeConstants.MasterTestScenePath))

            {

                failures.Add("Missing validation scene at " + CCS_NetcodeConstants.MasterTestScenePath);

                return;

            }



            Scene scene = EditorSceneManager.OpenScene(CCS_NetcodeConstants.MasterTestScenePath, OpenSceneMode.Single);

            if (!scene.IsValid())

            {

                failures.Add("Could not open validation scene for Phase 3A diagnostics validation.");

                return;

            }



            string diagnosticsObjectName = CCS_ProjectAudioConstants.MasterTestTestingManagerObjectName;

            AppendIfMissing(

                failures,

                GameObject.Find(diagnosticsObjectName) != null,

                "Validation scene must contain " + diagnosticsObjectName + ".");

            AppendIfMissing(

                failures,

                GameObject.Find("CCS_TestingManager") == null,

                "Validation scene must not contain legacy object name CCS_TestingManager.");



            GameObject diagnosticsObject = GameObject.Find(diagnosticsObjectName);

            if (diagnosticsObject == null)

            {

                return;

            }



            CCS_CharacterControllerDiagnosticsManager[] managers =

                diagnosticsObject.GetComponents<CCS_CharacterControllerDiagnosticsManager>();

            AppendIfMissing(

                failures,

                managers.Length == 1,

                "Validation scene must contain exactly one CCS_CharacterControllerDiagnosticsManager.");

        }



        private static void ValidateNoLegacyTestingManagerComponents(List<string> failures)

        {

            if (!File.Exists(CCS_NetcodeConstants.MasterTestScenePath))

            {

                return;

            }



            Scene scene = EditorSceneManager.OpenScene(CCS_NetcodeConstants.MasterTestScenePath, OpenSceneMode.Single);

            AppendIfMissing(

                failures,

                CCS_CharacterControllerPhase2DMigrationUtility.CountLegacyTestingManagerWrappersInScene(scene) == 0,

                "Validation scene must not contain legacy Testing Manager components.");

        }



        private static void ValidateProductionMissingScripts(List<string> failures)

        {

            CCS_SurvivalValidationResult missingScriptResult =

                CCS_MissingScriptScanUtility.ValidateProductionAssetsHaveNoMissingScripts();

            if (!missingScriptResult.IsSuccess)

            {

                failures.Add(missingScriptResult.Message);

            }

        }



        private static void ValidatePlayerPrefabMissingScripts(List<string> failures)

        {

            CCS_SurvivalValidationResult auditResult =

                CCS_CharacterControllerPlayerPrefabAuditUtility.ValidatePhase2CAuditFoundation();

            if (!auditResult.IsSuccess)

            {

                failures.Add(auditResult.Message);

            }

        }



        private static void ValidateNetworkManagerPlayerPrefabReference(List<string> failures)

        {

            if (!File.Exists(CCS_NetcodeConstants.NetworkManagerPrefabPath))

            {

                failures.Add("Missing network manager prefab at " + CCS_NetcodeConstants.NetworkManagerPrefabPath);

                return;

            }



            GameObject networkManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(

                CCS_NetcodeConstants.NetworkManagerPrefabPath);

            if (networkManagerPrefab == null)

            {

                failures.Add("Could not load network manager prefab for Phase 3A validation.");

                return;

            }



            CCS_SurvivalValidationResult referenceResult =

                CCS_CharacterControllerPlayerPrefabAuditUtility.ValidateNetworkManagerPlayerPrefabReference();

            if (!referenceResult.IsSuccess)

            {

                failures.Add(referenceResult.Message);

            }

        }



        private static void ValidateValidationSceneEnvironment(List<string> failures)

        {

            if (!File.Exists(CCS_NetcodeConstants.MasterTestScenePath))

            {

                return;

            }



            Scene scene = EditorSceneManager.OpenScene(CCS_NetcodeConstants.MasterTestScenePath, OpenSceneMode.Single);

            if (!scene.IsValid())

            {

                failures.Add("Could not open validation scene for environment validation.");

                return;

            }



            bool hasCamera = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length > 0;

            AppendIfMissing(failures, hasCamera, "Validation scene must contain at least one camera.");



            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            bool hasLight = lights.Any(light => light != null && light.type == LightType.Directional);

            AppendIfMissing(failures, hasLight, "Validation scene must contain a directional light.");



            Transform environment = GameObject.Find(CCS_CharacterControllerMasterTestLayoutConstants.EnvironmentParentName)?.transform;

            AppendIfMissing(

                failures,

                environment != null,

                "Validation scene must contain Environment parent object.");



            if (environment != null)

            {

                Transform ground = environment.Find(CCS_CharacterControllerMasterTestLayoutConstants.GroundInstanceName);

                AppendIfMissing(

                    failures,

                    ground != null,

                    "Validation scene Environment must contain "

                        + CCS_CharacterControllerMasterTestLayoutConstants.GroundInstanceName + ".");

            }

        }



        private static void CollectHistoricalNamingWarnings(List<string> warnings)

        {

            if (Directory.Exists(PrototypingRootPath))

            {

                string[] prototypePrefabs = Directory.GetFiles(

                    PrototypingRootPath,

                    "PF_CCS_Test*.prefab",

                    SearchOption.AllDirectories);

                if (prototypePrefabs.Length > 0)

                {

                    warnings.Add("Prototyping prefab names still contain Test prefix (acceptable for blockout assets).");

                }

            }



            warnings.Add("Project-level batch entry names may still use MasterTest (historical milestone naming).");

        }



        private static void AppendIfMissing(List<string> failures, bool condition, string message)

        {

            if (!condition)

            {

                failures.Add(message);

            }

        }

    }

}


