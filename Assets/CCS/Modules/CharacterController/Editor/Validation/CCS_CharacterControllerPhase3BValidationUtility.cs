using System.Collections.Generic;

using System.IO;

using System.Linq;

using CCS.Modules.CharacterController.Diagnostics;

using CCS.Modules.CharacterController.Local;

using CCS.Modules.CharacterController.Netcode;

using CCS.Project;

using UnityEditor;

using UnityEditor.Animations;

using UnityEditor.SceneManagement;

using UnityEngine;

using UnityEngine.SceneManagement;



// =============================================================================

// SCRIPT: CCS_CharacterControllerPhase3BValidationUtility

// CATEGORY: Modules / CharacterController / Editor / Validation

// PURPOSE: Validates v0.7.3 Phase 3B locomotion-only Animator Controller reset.

// PLACEMENT: Editor validation utility. Not attached to GameObjects.

// AUTHOR: James Schilz

// CREATED: 2026-06-25

// NOTES: Fails if removed aim/interaction/revolver animator wiring returns on production assets.

// =============================================================================



namespace CCS.Modules.CharacterController.Editor

{

    public static class CCS_CharacterControllerPhase3BValidationUtility

    {

        private const string RemovedUpperBodyAnimatorScriptPath =

            "Assets/CCS/Modules/CharacterController/Runtime/Animation/CCS_RevolverUpperBodyAnimator.cs";



        private const string RemovedUpperBodyAnimatorDebugReporterScriptPath =

            "Assets/CCS/Modules/CharacterController/Runtime/Diagnostics/CCS_RevolverUpperBodyAnimatorDebugReporter.cs";



        private const string RemovedAimPhaseScriptPath =

            "Assets/CCS/Modules/CharacterController/Runtime/Animation/CCS_RevolverAimPhase.cs";



        private const string AIBanditPrefabPath =

            "Assets/CCS/Modules/AI/Content/Prefabs/PF_CCS_AI_Bandit_Networked.prefab";



        private static readonly string[] ProductionPrefabPaths =

        {

            CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath,

            AIBanditPrefabPath,

            CCS_CharacterControllerMasterTestLayoutConstants.NpcPrefabPath,

        };



        private static readonly string[] RuntimeScriptScanRoots =

        {

            CCS_CharacterControllerConstants.ModuleRootPath + "/Runtime",

            "Assets/CCS/Modules/AI/Runtime",

            "Assets/CCS/Modules/Weapons/Runtime",

        };



        public static CCS_SurvivalValidationResult ValidatePhase3BLocomotionOnlyAnimatorReset()

        {

            List<string> failures = new List<string>();

            List<string> warnings = new List<string>();



            ValidateObsoleteAnimationBridgeScriptsRemoved(failures);

            ValidateAnimatorControllerLocomotionOnly(failures);

            ValidateRuntimeScriptsDoNotWriteRemovedParameters(failures);

            ValidateProductionPrefabAnimationBridgesRemoved(failures);

            ValidateSceneInstancesAnimationBridgesRemoved(failures);

            ValidateMissingScripts(failures);

            ValidateValidationSceneFoundation(failures);

            ValidateNetworkManagerPlayerPrefabReference(failures);

            CollectUnusedClipWarnings(warnings);



            if (failures.Count > 0)

            {

                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));

            }



            string message = "Phase 3B locomotion-only animator reset validated.";

            if (warnings.Count > 0)

            {

                message += " Warnings: " + string.Join(" ", warnings);

            }



            return CCS_SurvivalValidationResult.Pass(message);

        }



        private static void ValidateObsoleteAnimationBridgeScriptsRemoved(List<string> failures)

        {

            AppendIfMissing(

                failures,

                !File.Exists(RemovedUpperBodyAnimatorScriptPath),

                "CCS_RevolverUpperBodyAnimator runtime script must be removed for Phase 3B.");

            AppendIfMissing(

                failures,

                !File.Exists(RemovedUpperBodyAnimatorDebugReporterScriptPath),

                "CCS_RevolverUpperBodyAnimatorDebugReporter must be removed for Phase 3B.");

            AppendIfMissing(

                failures,

                !File.Exists(RemovedAimPhaseScriptPath),

                "CCS_RevolverAimPhase must be removed when upper-body animator bridge is retired.");

        }



        private static void ValidateAnimatorControllerLocomotionOnly(List<string> failures)

        {

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(

                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);

            AppendIfMissing(

                failures,

                controller != null,

                "Missing player Animator Controller at "

                + CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);



            if (controller == null)

            {

                return;

            }



            HashSet<string> allowedLayers = new HashSet<string>(

                CCS_CharacterControllerConstants.Phase3BAllowedAnimatorLayerNames);

            for (int i = 0; i < controller.layers.Length; i++)

            {

                if (!allowedLayers.Contains(controller.layers[i].name))

                {

                    failures.Add("Animator Controller layer must be removed: " + controller.layers[i].name + ".");

                }

            }



            AppendIfMissing(

                failures,

                controller.layers.Length == 1 && controller.layers[0].name == "Base Layer",

                "Animator Controller must contain exactly one Base Layer.");



            HashSet<string> allowedParameters = new HashSet<string>(

                CCS_CharacterControllerConstants.Phase3BAllowedAnimatorParameterNames);

            for (int i = 0; i < controller.parameters.Length; i++)

            {

                string parameterName = controller.parameters[i].name;

                if (!allowedParameters.Contains(parameterName))

                {

                    failures.Add("Animator Controller parameter must be removed: " + parameterName + ".");

                }

            }



            for (int layerIndex = 0;

                 layerIndex < CCS_CharacterControllerConstants.Phase3BRemovedAnimatorLayerNames.Length;

                 layerIndex++)

            {

                string removedLayerName =

                    CCS_CharacterControllerConstants.Phase3BRemovedAnimatorLayerNames[layerIndex];

                AppendIfMissing(

                    failures,

                    FindLayerIndex(controller, removedLayerName) < 0,

                    "Animator Controller must not contain removed layer " + removedLayerName + ".");

            }



            if (controller.layers.Length > 0 && controller.layers[0].stateMachine != null)

            {

                AnimatorStateMachine baseStateMachine = controller.layers[0].stateMachine;

                for (int i = 0;

                     i < CCS_CharacterControllerConstants.Phase3BRemovedAnimatorStateNames.Length;

                     i++)

                {

                    string removedStateName = CCS_CharacterControllerConstants.Phase3BRemovedAnimatorStateNames[i];

                    AppendIfMissing(

                        failures,

                        FindState(baseStateMachine, removedStateName) == null,

                        "Animator Controller must not contain state " + removedStateName + ".");

                }



                if (baseStateMachine.anyStateTransitions.Length > 0)

                {

                    failures.Add("Base Layer must not contain Any State transitions after locomotion-only reset.");

                }



                HashSet<string> allowedStates = new HashSet<string>(

                    CCS_CharacterControllerConstants.Phase3BAllowedBaseLayerLocomotionStateNames);

                ChildAnimatorState[] states = baseStateMachine.states;

                for (int stateIndex = 0; stateIndex < states.Length; stateIndex++)

                {

                    AnimatorState state = states[stateIndex].state;

                    if (state == null)

                    {

                        continue;

                    }



                    if (!allowedStates.Contains(state.name))

                    {

                        failures.Add("Base Layer contains unexpected non-locomotion state: " + state.name + ".");

                    }

                }

            }

        }



        private static void ValidateRuntimeScriptsDoNotWriteRemovedParameters(List<string> failures)

        {

            for (int rootIndex = 0; rootIndex < RuntimeScriptScanRoots.Length; rootIndex++)

            {

                string root = RuntimeScriptScanRoots[rootIndex];

                if (!Directory.Exists(root))

                {

                    continue;

                }



                string[] scriptPaths = Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories);

                for (int scriptIndex = 0; scriptIndex < scriptPaths.Length; scriptIndex++)

                {

                    string scriptPath = scriptPaths[scriptIndex].Replace('\\', '/');

                    string source = File.ReadAllText(scriptPath);



                    if (source.Contains("GetLayerIndex")

                        && source.Contains(CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName))

                    {

                        failures.Add(

                            scriptPath

                            + " must not query removed Animator layer "

                            + CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName

                            + ".");

                    }



                    for (int parameterIndex = 0;

                         parameterIndex < CCS_CharacterControllerConstants.Phase3BRemovedAnimatorParameterNames.Length;

                         parameterIndex++)

                    {

                        string parameterName =

                            CCS_CharacterControllerConstants.Phase3BRemovedAnimatorParameterNames[parameterIndex];

                        if (source.Contains("SetTrigger") && source.Contains(parameterName))

                        {

                            failures.Add(scriptPath + " must not SetTrigger removed parameter " + parameterName + ".");

                        }



                        if (source.Contains("SetBool") && source.Contains(parameterName))

                        {

                            failures.Add(scriptPath + " must not SetBool removed parameter " + parameterName + ".");

                        }



                        if (source.Contains("SetFloat") && source.Contains(parameterName))

                        {

                            failures.Add(scriptPath + " must not SetFloat removed parameter " + parameterName + ".");

                        }



                        if (source.Contains("SetInteger") && source.Contains(parameterName))

                        {

                            failures.Add(scriptPath + " must not SetInteger removed parameter " + parameterName + ".");

                        }

                    }



                    if (source.Contains("CCS_RevolverUpperBodyAnimator")
                        && !scriptPath.EndsWith("/CCS_CharacterControllerConstants.cs")
                        && !scriptPath.Contains("/Validation/"))
                    {

                        failures.Add(scriptPath + " must not reference CCS_RevolverUpperBodyAnimator.");

                    }

                }

            }

        }



        private static void ValidateProductionPrefabAnimationBridgesRemoved(List<string> failures)

        {

            for (int prefabIndex = 0; prefabIndex < ProductionPrefabPaths.Length; prefabIndex++)

            {

                string prefabPath = ProductionPrefabPaths[prefabIndex];

                GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                AppendIfMissing(

                    failures,

                    prefabRoot != null,

                    "Missing production prefab at " + prefabPath);



                if (prefabRoot == null)

                {

                    continue;

                }



                AppendPrefabMustNotContainRemovedAnimationBridges(failures, prefabRoot, prefabPath);

            }

        }



        private static void ValidateSceneInstancesAnimationBridgesRemoved(List<string> failures)

        {

            string[] scenePaths =

            {

                CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath,

                CCS_NetcodeConstants.MultiplayerHostingScenePath,

            };



            for (int sceneIndex = 0; sceneIndex < scenePaths.Length; sceneIndex++)

            {

                string scenePath = scenePaths[sceneIndex];

                if (!File.Exists(scenePath))

                {

                    failures.Add("Missing scene for Phase 3B animation bridge scan: " + scenePath + ".");

                    continue;

                }



                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

                if (!scene.IsValid())

                {

                    failures.Add("Could not open scene for Phase 3B animation bridge scan: " + scenePath + ".");

                    continue;

                }



                try

                {

                    Component[] components = Object.FindObjectsByType<Component>(FindObjectsSortMode.None);

                    for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)

                    {

                        Component component = components[componentIndex];

                        if (component == null)

                        {

                            continue;

                        }



                        string typeName = component.GetType().Name;

                        for (int bridgeIndex = 0;

                             bridgeIndex

                             < CCS_CharacterControllerConstants.Phase3BRemovedAnimationBridgeComponentTypeNames.Length;

                             bridgeIndex++)

                        {

                            if (typeName

                                == CCS_CharacterControllerConstants

                                    .Phase3BRemovedAnimationBridgeComponentTypeNames[bridgeIndex])

                            {

                                failures.Add(

                                    scenePath

                                    + " must not contain removed animation bridge component "

                                    + typeName

                                    + " on "

                                    + GetHierarchyPath(component.transform)

                                    + ".");

                            }

                        }

                    }

                }

                finally

                {

                    EditorSceneManager.CloseScene(scene, true);

                }

            }

        }



        private static void AppendPrefabMustNotContainRemovedAnimationBridges(

            List<string> failures,

            GameObject prefabRoot,

            string prefabPath)

        {

            for (int i = 0;

                 i < CCS_CharacterControllerConstants.Phase3BRemovedAnimationBridgeComponentTypeNames.Length;

                 i++)

            {

                string typeName =

                    CCS_CharacterControllerConstants.Phase3BRemovedAnimationBridgeComponentTypeNames[i];

                Component[] components = prefabRoot.GetComponentsInChildren<Component>(true);

                for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)

                {

                    Component component = components[componentIndex];

                    if (component != null && component.GetType().Name == typeName)

                    {

                        failures.Add(

                            prefabPath

                            + " must not contain removed animation bridge component "

                            + typeName

                            + ".");

                    }

                }

            }

        }



        private static void ValidateMissingScripts(List<string> failures)

        {

            CCS_SurvivalValidationResult missingScriptResult =

                CCS_MissingScriptScanUtility.ValidateProductionAssetsHaveNoMissingScripts();

            if (!missingScriptResult.IsSuccess)

            {

                failures.Add(missingScriptResult.Message);

            }

        }



        private static void ValidateValidationSceneFoundation(List<string> failures)

        {

            if (!File.Exists(CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath))

            {

                failures.Add("Missing validation scene at " + CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath);

                return;

            }



            Scene scene = EditorSceneManager.OpenScene(

                CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath,

                OpenSceneMode.Single);

            if (!scene.IsValid())

            {

                failures.Add("Could not open validation scene.");

                return;

            }



            AppendIfMissing(

                failures,

                Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Length > 0,

                "Validation scene must contain a light.");

            AppendIfMissing(

                failures,

                Camera.main != null || Object.FindAnyObjectByType<Camera>() != null,

                "Validation scene must contain a camera.");

            AppendIfMissing(

                failures,

                GameObject.Find(CCS_CharacterControllerMasterTestLayoutConstants.EnvironmentParentName) != null,

                "Validation scene must contain environment parent.");

            AppendIfMissing(

                failures,

                GameObject.Find(CCS_CharacterControllerMasterTestLayoutConstants.TestPointsParentName) != null,

                "Validation scene must contain test points parent.");

            AppendIfMissing(

                failures,

                Object.FindAnyObjectByType<CCS_CharacterControllerDiagnosticsManager>() != null,

                "Validation scene must contain CCS_CharacterControllerDiagnosticsManager.");

        }



        private static void ValidateNetworkManagerPlayerPrefabReference(List<string> failures)

        {

            CCS_SurvivalValidationResult result =

                CCS_CharacterControllerPlayerPrefabAuditUtility.ValidateNetworkManagerPlayerPrefabReference();

            if (!result.IsSuccess)

            {

                failures.Add(result.Message);

            }

        }



        private static void CollectUnusedClipWarnings(List<string> warnings)

        {

            string revolverRoot = CCS_CharacterControllerConstants.RevolverAimAnimationsPath;

            if (Directory.Exists(revolverRoot))

            {

                int clipCount = Directory.GetFiles(revolverRoot, "*.anim", SearchOption.AllDirectories).Length;

                if (clipCount > 0)

                {

                    warnings.Add(

                        "Non-locomotion revolver animation clips remain on disk (" + clipCount + ") for later review.");

                }

            }

        }



        private static int FindLayerIndex(AnimatorController controller, string layerName)

        {

            AnimatorControllerLayer[] layers = controller.layers;

            for (int i = 0; i < layers.Length; i++)

            {

                if (layers[i].name == layerName)

                {

                    return i;

                }

            }



            return -1;

        }



        private static AnimatorState FindState(AnimatorStateMachine stateMachine, string stateName)

        {

            ChildAnimatorState[] states = stateMachine.states;

            for (int i = 0; i < states.Length; i++)

            {

                if (states[i].state != null && states[i].state.name == stateName)

                {

                    return states[i].state;

                }

            }



            return null;

        }



        private static string GetHierarchyPath(Transform transform)

        {

            if (transform == null)

            {

                return "<null>";

            }



            string path = transform.name;

            Transform parent = transform.parent;

            while (parent != null)

            {

                path = parent.name + "/" + path;

                parent = parent.parent;

            }



            return path;

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


