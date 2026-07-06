using System;
using System.Collections.Generic;
using CCS.Modules.AI;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Diagnostics;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.Interaction;
using CCS.Modules.Weapons;
using CCS.Project;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_StrippedCharacterControllerBaselineStripBuilder
// CATEGORY: Modules / CharacterController / Editor / StripBaseline
// PURPOSE: Applies v0.7.13 stripped Character Controller baseline to assets and validation scene.
// PLACEMENT: Editor strip utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public sealed class StrippedCharacterControllerBaselineStripResult
    {
        public bool AnimatorChanged;
        public bool PrefabChanged;
        public bool SceneChanged;
        public int PlayerRootMonoBehaviourCount = -1;
        public List<string> Notes = new List<string>();
    }

    public static class CCS_StrippedCharacterControllerBaselineStripBuilder
    {
        private static readonly string[] RootComponentTypeNamesToRemove =
        {
            "CCS_RevolverController",
            "CCS_PlayerWeaponLoadout",
            "CCS_WeaponCarryStateController",
            "CCS_PlayerEquipmentVisualController",
            "CCS_NetworkInteractionScanner",
        };

        private static readonly string[] ModelComponentTypeNamesToRemove =
        {
            "CCS_SingleRevolverAimAnimator",
            "CCS_RevolverArmReticleIK",
            "CCS_RevolverBodyAimFollowController",
            "CCS_PlayerInteractionAnimator",
            "CCS_RevolverReticleAnimationEventReceiver",
            "CCS_RevolverAimTargetResolver",
            "CCS_DualRevolverAimConvergenceRigPresenter",
            "CCS_DualRevolverArmAimBiasPresenter",
            "CCS_DualRevolverArmAimConstraintPresenter",
        };

        private static readonly string[] IkObjectNamesToRemove =
        {
            CCS_EquipmentConstants.WeaponIkTargetsObjectName,
            CCS_EquipmentConstants.WeaponIkRigObjectName,
            CCS_WeaponsConstants.RevolverArmReticleIkRigObjectName,
            CCS_WeaponsConstants.RevolverArmReticleIkRootObjectName,
            CCS_WeaponsConstants.RightHandReticleIkTargetObjectName,
            CCS_WeaponsConstants.ReticleAimWorldTargetObjectName,
        };

        private static readonly string[] SceneObjectNamesToRemove =
        {
            "CCS_TestPickupItemSpawner",
            "PF_CCS_TestInteractable_PickupItem",
            "PF_CCS_TestWeaponDamageTarget",
            "CCS_TestWeaponDamageTarget",
            CCS_WeaponsConstants.RevolverM1879WorldPickupInstanceName,
            "PF_CCS_RevolverM1879_WorldPickup",
        };

        public static StrippedCharacterControllerBaselineStripResult RunFullStrip()
        {
            StrippedCharacterControllerBaselineStripResult result = new StrippedCharacterControllerBaselineStripResult();

            result.AnimatorChanged |= RemoveFulldrawIdleReticleRevealAnimationEvent(result);
            result.AnimatorChanged |= StripAnimatorControllerToBaseline(result);
            result.PrefabChanged |= StripNetworkedPlayerPrefab(result);
            result.SceneChanged |= StripValidationScene(result);

            result.PlayerRootMonoBehaviourCount = CountPlayerRootMonoBehaviours();

            if (result.AnimatorChanged || result.PrefabChanged || result.SceneChanged)
            {
                AssetDatabase.SaveAssets();
            }

            return result;
        }

        public static bool RemoveFulldrawIdleReticleRevealAnimationEvent(StrippedCharacterControllerBaselineStripResult result)
        {
            bool changed = CCS_RevolverFulldrawIdleReticleEventBuilder.RemoveFulldrawIdleReticleRevealAnimationEvent(
                out int removedEventCount);
            if (changed)
            {
                result.Notes.Add("Removed " + removedEventCount + " Fulldraw_Idle reticle reveal animation event(s).");
            }

            return changed;
        }

        public static bool StripAnimatorControllerToBaseline(StrippedCharacterControllerBaselineStripResult result)
        {
            bool changed = CCS_SingleRevolverAimLayerBuilder.EnsureSingleRevolverAimLayer();
            changed |= CCS_RevolverFulldrawIdleReticleEventBuilder.RemoveFulldrawIdleReticleRevealAnimationEvent(out _);
            if (changed)
            {
                result.Notes.Add("Animator controller ensured Base Layer + SingleRevolverUpperBody with approved parameters.");
            }

            result.AnimatorChanged |= changed;
            return changed;
        }

        public static bool StripNetworkedPlayerPrefab(StrippedCharacterControllerBaselineStripResult result)
        {
            string prefabPath = CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath;
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError("[Strip Baseline] Missing networked player prefab at " + prefabPath);
                return false;
            }

            bool changed = false;
            try
            {
                changed |= RemoveComponentsByTypeNames(prefabRoot, RootComponentTypeNamesToRemove, result);
                changed |= RemoveChildByName(
                    prefabRoot.transform,
                    CCS_CharacterControllerConstants.RevolverAimTargetResolverObjectName,
                    result);

                changed |= EnsureRevolverAimPresentationGate(prefabRoot, result);

                Transform modelRoot = CCS_PlayerModelRootUtility.FindModelRoot(prefabRoot.transform);
                if (modelRoot != null)
                {
                    changed |= RemoveComponentsByTypeNames(modelRoot.gameObject, ModelComponentTypeNamesToRemove, result);
                    changed |= RemoveChildByName(modelRoot, CCS_CharacterControllerConstants.RevolverAimTargetResolverObjectName, result);
                    changed |= EnsureRevolverAimLayerAnimator(modelRoot, prefabRoot, result);
                    changed |= EnsureHolsteredRevolverVisualPresenter(modelRoot, prefabRoot, result);
                    changed |= EnsureEquippedRevolverAimVisualPresenter(modelRoot, prefabRoot, result);
                    changed |= RemoveIkRigObjects(modelRoot, result);
                    changed |= CCS_ProceduralArmConvergenceCleanupBuilder.CleanProceduralArmConvergence(
                        modelRoot,
                        prefabRoot,
                        result);
                    changed |= CCS_SharedDualAimPresentationBuilder.EnsureSharedDualAimPresentation(
                        modelRoot,
                        prefabRoot,
                        result);
                }

                changed |= RewireAimLocomotionGate(prefabRoot, result);
                changed |= ClearMotorInteractionLockSource(prefabRoot, result);
                changed |= StripWeaponHudRoot(prefabRoot, result);

                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                    result.Notes.Add("Saved stripped networked player prefab.");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            result.PrefabChanged |= changed;
            return changed;
        }

        public static bool StripValidationScene(StrippedCharacterControllerBaselineStripResult result)
        {
            Scene scene = EditorSceneManager.OpenScene(
                CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath,
                OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError("[Strip Baseline] Could not open validation scene.");
                return false;
            }

            bool changed = false;
            for (int i = 0; i < SceneObjectNamesToRemove.Length; i++)
            {
                changed |= DestroyAllSceneObjectsByName(SceneObjectNamesToRemove[i], result);
            }

            changed |= DisableBanditSpawnerAutoSpawn(result);
            changed |= EnsureDiagnosticsEnemyBanditController(scene, result);
            changed |= EnsureDiagnosticsManagerStrippedDefaults(result);

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                result.Notes.Add("Saved stripped validation scene.");
            }

            result.SceneChanged |= changed;
            return changed;
        }

        private static bool EnsureRevolverAimPresentationGate(GameObject prefabRoot, StrippedCharacterControllerBaselineStripResult result)
        {
            CCS_RevolverAimPresentationGate gate = prefabRoot.GetComponent<CCS_RevolverAimPresentationGate>();
            bool changed = false;
            if (gate == null)
            {
                gate = prefabRoot.AddComponent<CCS_RevolverAimPresentationGate>();
                changed = true;
                result.Notes.Add("Added CCS_RevolverAimPresentationGate on player root.");
            }

            CCS_CharacterInputActionProvider inputProvider = prefabRoot.GetComponent<CCS_CharacterInputActionProvider>();
            SerializedObject serializedGate = new SerializedObject(gate);
            SerializedProperty inputProperty = serializedGate.FindProperty("inputProvider");
            if (inputProperty != null && inputProperty.objectReferenceValue != inputProvider)
            {
                inputProperty.objectReferenceValue = inputProvider;
                serializedGate.ApplyModifiedPropertiesWithoutUndo();
                changed = true;
            }

            return changed;
        }

        private static bool RewireAimLocomotionGate(GameObject prefabRoot, StrippedCharacterControllerBaselineStripResult result)
        {
            CCS_CharacterAimLocomotionController aimLocomotion =
                prefabRoot.GetComponent<CCS_CharacterAimLocomotionController>();
            CCS_RevolverAimPresentationGate gate = prefabRoot.GetComponent<CCS_RevolverAimPresentationGate>();
            if (aimLocomotion == null || gate == null)
            {
                return false;
            }

            SerializedObject serializedAim = new SerializedObject(aimLocomotion);
            SerializedProperty gateProperty = serializedAim.FindProperty("weaponAimGateComponent");
            if (gateProperty == null || gateProperty.objectReferenceValue == gate)
            {
                return false;
            }

            gateProperty.objectReferenceValue = gate;
            serializedAim.ApplyModifiedPropertiesWithoutUndo();
            result.Notes.Add("Rewired aim locomotion gate to CCS_RevolverAimPresentationGate.");
            return true;
        }

        private static bool ClearMotorInteractionLockSource(GameObject prefabRoot, StrippedCharacterControllerBaselineStripResult result)
        {
            CCS_CharacterMotor motor = prefabRoot.GetComponent<CCS_CharacterMotor>();
            if (motor == null)
            {
                return false;
            }

            SerializedObject serializedMotor = new SerializedObject(motor);
            SerializedProperty lockProperty = serializedMotor.FindProperty("interactionLockSourceComponent");
            if (lockProperty == null || lockProperty.objectReferenceValue == null)
            {
                return false;
            }

            lockProperty.objectReferenceValue = null;
            serializedMotor.ApplyModifiedPropertiesWithoutUndo();
            result.Notes.Add("Cleared motor interaction lock source after removing interaction animator.");
            return true;
        }

        private static bool EnsureRevolverAimLayerAnimator(
            Transform modelRoot,
            GameObject prefabRoot,
            StrippedCharacterControllerBaselineStripResult result)
        {
            CCS_RevolverAimLayerAnimator layerAnimator = modelRoot.GetComponent<CCS_RevolverAimLayerAnimator>();
            bool changed = false;
            if (layerAnimator == null)
            {
                layerAnimator = modelRoot.gameObject.AddComponent<CCS_RevolverAimLayerAnimator>();
                changed = true;
                result.Notes.Add("Added CCS_RevolverAimLayerAnimator on Model.");
            }

            Animator resolvedAnimator = modelRoot.GetComponentInChildren<Animator>(true);
            CCS_RevolverAimPresentationGate gate = prefabRoot.GetComponent<CCS_RevolverAimPresentationGate>();
            SerializedObject serializedAnimator = new SerializedObject(layerAnimator);
            SerializedProperty animatorProperty = serializedAnimator.FindProperty("animator");
            SerializedProperty inputProperty = serializedAnimator.FindProperty("aimPresentationInputComponent");
            if (animatorProperty != null && animatorProperty.objectReferenceValue != resolvedAnimator)
            {
                animatorProperty.objectReferenceValue = resolvedAnimator;
                changed = true;
            }

            if (inputProperty != null && inputProperty.objectReferenceValue != gate)
            {
                inputProperty.objectReferenceValue = gate;
                changed = true;
            }

            if (changed)
            {
                serializedAnimator.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool EnsureHolsteredRevolverVisualPresenter(
            Transform modelRoot,
            GameObject prefabRoot,
            StrippedCharacterControllerBaselineStripResult result)
        {
            CCS_PlayerHolsteredRevolverVisualPresenter presenter =
                modelRoot.GetComponent<CCS_PlayerHolsteredRevolverVisualPresenter>();
            bool changed = false;
            if (presenter == null)
            {
                presenter = modelRoot.gameObject.AddComponent<CCS_PlayerHolsteredRevolverVisualPresenter>();
                changed = true;
                result.Notes.Add("Added CCS_PlayerHolsteredRevolverVisualPresenter on Model.");
            }

            CCS_EquipmentSocketRegistry socketRegistry = prefabRoot.GetComponent<CCS_EquipmentSocketRegistry>();
            CCS_WeaponAttachmentFitProfile rightHipProfile = AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(
                CCS_RevolverFitProfilePaths.RightHipHolsterFitPath);
            GameObject visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_EquipmentConstants.RevolverM1879VisualOnlyPrefabPath);

            SerializedObject serializedPresenter = new SerializedObject(presenter);
            changed |= SetObjectReference(serializedPresenter, "equipmentSocketRegistry", socketRegistry);
            changed |= SetObjectReference(serializedPresenter, "rightHipHolsterFitProfile", rightHipProfile);
            changed |= SetObjectReference(serializedPresenter, "revolverVisualOnlyPrefab", visualPrefab);
            if (changed)
            {
                serializedPresenter.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool EnsureEquippedRevolverAimVisualPresenter(
            Transform modelRoot,
            GameObject prefabRoot,
            StrippedCharacterControllerBaselineStripResult result)
        {
            CCS_PlayerEquippedRevolverAimVisualPresenter presenter =
                modelRoot.GetComponent<CCS_PlayerEquippedRevolverAimVisualPresenter>();
            bool changed = false;
            if (presenter == null)
            {
                presenter = modelRoot.gameObject.AddComponent<CCS_PlayerEquippedRevolverAimVisualPresenter>();
                changed = true;
                result.Notes.Add("Added CCS_PlayerEquippedRevolverAimVisualPresenter on Model.");
            }

            CCS_EquipmentSocketRegistry socketRegistry = prefabRoot.GetComponent<CCS_EquipmentSocketRegistry>();
            CCS_RevolverAimPresentationGate aimGate = prefabRoot.GetComponent<CCS_RevolverAimPresentationGate>();
            CCS_PlayerHolsteredRevolverVisualPresenter holsteredPresenter =
                modelRoot.GetComponent<CCS_PlayerHolsteredRevolverVisualPresenter>();
            CCS_WeaponAttachmentFitProfile rightHandProfile = AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(
                CCS_RevolverFitProfilePaths.RightHandEquippedFitPath);
            GameObject visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_EquipmentConstants.RevolverM1879VisualOnlyPrefabPath);

            SerializedObject serializedPresenter = new SerializedObject(presenter);
            changed |= SetObjectReference(serializedPresenter, "equipmentSocketRegistry", socketRegistry);
            changed |= SetObjectReference(serializedPresenter, "rightHandEquippedFitProfile", rightHandProfile);
            changed |= SetObjectReference(
                serializedPresenter,
                "leftHandEquippedFitProfile",
                AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(
                    CCS_RevolverFitProfilePaths.LeftHandEquippedFitPath));
            changed |= SetObjectReference(serializedPresenter, "revolverVisualOnlyPrefab", visualPrefab);
            changed |= SetObjectReference(serializedPresenter, "aimPresentationInputComponent", aimGate);
            changed |= SetObjectReference(serializedPresenter, "holsteredRevolverVisualPresenter", holsteredPresenter);
            if (changed)
            {
                serializedPresenter.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool StripWeaponHudRoot(GameObject prefabRoot, StrippedCharacterControllerBaselineStripResult result)
        {
            Transform hudRoot = prefabRoot.transform.Find(CCS_WeaponsConstants.WeaponHudRootName);
            if (hudRoot == null)
            {
                return false;
            }

            bool changed = false;
            changed |= RemoveComponentsByTypeNames(
                hudRoot.gameObject,
                new[] { "CCS_MuzzleDrivenReticleController" },
                result);

            Transform weaponReticle = hudRoot.Find("WeaponReticle");
            if (weaponReticle != null)
            {
                UnityEngine.Object.DestroyImmediate(weaponReticle.gameObject, true);
                changed = true;
                result.Notes.Add("Destroyed WeaponReticle child under WeaponHudRoot.");
            }

            CCS_RevolverHudPresenter hudPresenter = hudRoot.GetComponent<CCS_RevolverHudPresenter>();
            if (hudPresenter != null)
            {
                SerializedObject serializedHud = new SerializedObject(hudPresenter);
                changed |= SetObjectReference(serializedHud, "revolverController", null);
                SerializedProperty reticleImageProperty = serializedHud.FindProperty("reticleImage");
                if (reticleImageProperty != null && reticleImageProperty.objectReferenceValue != null)
                {
                    reticleImageProperty.objectReferenceValue = null;
                    changed = true;
                }

                SerializedProperty hudTextProperty = serializedHud.FindProperty("hudText");
                if (hudTextProperty != null && hudTextProperty.objectReferenceValue != null)
                {
                    hudTextProperty.objectReferenceValue = null;
                    changed = true;
                }

                if (changed)
                {
                    serializedHud.ApplyModifiedPropertiesWithoutUndo();
                    result.Notes.Add("Cleared ammo/reticle references on CCS_RevolverHudPresenter.");
                }
            }

            return changed;
        }

        private static bool RemoveIkRigObjects(Transform modelRoot, StrippedCharacterControllerBaselineStripResult result)
        {
            bool changed = false;
            for (int i = 0; i < IkObjectNamesToRemove.Length; i++)
            {
                Transform target = FindChildByName(modelRoot, IkObjectNamesToRemove[i]);
                if (target == null)
                {
                    continue;
                }

                UnityEngine.Object.DestroyImmediate(target.gameObject, true);
                changed = true;
                result.Notes.Add("Removed IK rig object '" + IkObjectNamesToRemove[i] + "' from player prefab.");
            }

            return changed;
        }

        private static bool DisableBanditSpawnerAutoSpawn(StrippedCharacterControllerBaselineStripResult result)
        {
            CCS_AIBanditSpawner spawner = UnityEngine.Object.FindAnyObjectByType<CCS_AIBanditSpawner>();
            if (spawner == null)
            {
                return false;
            }

            SerializedObject serializedSpawner = new SerializedObject(spawner);
            SerializedProperty autoSpawnProperty = serializedSpawner.FindProperty("autoSpawnOnStart");
            if (autoSpawnProperty == null || !autoSpawnProperty.boolValue)
            {
                return false;
            }

            autoSpawnProperty.boolValue = false;
            serializedSpawner.ApplyModifiedPropertiesWithoutUndo();
            result.Notes.Add("Disabled CCS_AIBanditSpawner autoSpawnOnStart in validation scene.");
            return true;
        }

        private static bool EnsureDiagnosticsEnemyBanditController(
            Scene scene,
            StrippedCharacterControllerBaselineStripResult result)
        {
            CCS_CharacterControllerDiagnosticsManager diagnosticsManager =
                UnityEngine.Object.FindAnyObjectByType<CCS_CharacterControllerDiagnosticsManager>();
            if (diagnosticsManager == null)
            {
                return false;
            }

            bool changed = false;
            CCS_DiagnosticsEnemyBanditController controller =
                diagnosticsManager.GetComponent<CCS_DiagnosticsEnemyBanditController>();
            if (controller == null)
            {
                controller = diagnosticsManager.gameObject.AddComponent<CCS_DiagnosticsEnemyBanditController>();
                changed = true;
                result.Notes.Add("Added CCS_DiagnosticsEnemyBanditController to diagnostics manager.");
            }

            SerializedObject serializedController = new SerializedObject(controller);
            GameObject banditPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_AIConstants.AIBanditPrefabPath);
            Transform hostSpawn = GameObject.Find("TP_Spawn_Host")?.transform;
            changed |= SetObjectReference(serializedController, "aiBanditPrefab", banditPrefab);
            changed |= SetObjectReference(serializedController, "spawnReference", hostSpawn);
            if (changed)
            {
                serializedController.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool EnsureDiagnosticsManagerStrippedDefaults(StrippedCharacterControllerBaselineStripResult result)
        {
            CCS_CharacterControllerDiagnosticsManager manager =
                UnityEngine.Object.FindAnyObjectByType<CCS_CharacterControllerDiagnosticsManager>();
            if (manager == null)
            {
                return false;
            }

            bool changed = RemoveDiagnosticManagerObsoleteComponents(manager.gameObject, result);

            SerializedObject serializedManager = new SerializedObject(manager);
            changed |= SetBool(serializedManager, "enableEnemy", false);
            changed |= SetBool(serializedManager, "enableAimPose", false);
            changed |= SetBool(serializedManager, "equipWeapon", true);
            if (changed)
            {
                serializedManager.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(manager);
                result.Notes.Add("Reset diagnostics manager stripped baseline controls.");
            }

            return changed;
        }

        private static bool RemoveDiagnosticManagerObsoleteComponents(
            GameObject diagnosticsRoot,
            StrippedCharacterControllerBaselineStripResult result)
        {
            string[] obsoleteTypeNames =
            {
                "CCS_CharacterCameraDebugReporter",
                "CCS_PlayerDiagnosticsInputRouter",
            };

            return RemoveComponentsByTypeNames(diagnosticsRoot, obsoleteTypeNames, result);
        }

        private static bool RemoveComponentsByTypeNames(
            GameObject root,
            string[] typeNames,
            StrippedCharacterControllerBaselineStripResult result)
        {
            bool changed = false;
            for (int typeIndex = 0; typeIndex < typeNames.Length; typeIndex++)
            {
                string typeName = typeNames[typeIndex];
                Component[] components = root.GetComponentsInChildren<Component>(true);
                for (int componentIndex = components.Length - 1; componentIndex >= 0; componentIndex--)
                {
                    Component component = components[componentIndex];
                    if (component == null || component.GetType().Name != typeName)
                    {
                        continue;
                    }

                    string transformPath = BuildTransformPath(component.transform);
                    UnityEngine.Object.DestroyImmediate(component, true);
                    changed = true;
                    result.Notes.Add("Removed " + typeName + " from " + transformPath + ".");
                }
            }

            return changed;
        }

        private static bool RemoveChildByName(
            Transform parent,
            string childName,
            StrippedCharacterControllerBaselineStripResult result)
        {
            Transform child = FindChildByName(parent, childName);
            if (child == null)
            {
                return false;
            }

            UnityEngine.Object.DestroyImmediate(child.gameObject, true);
            result.Notes.Add("Removed child '" + childName + "' from " + parent.name + ".");
            return true;
        }

        private static bool DestroyAllSceneObjectsByName(string objectName, StrippedCharacterControllerBaselineStripResult result)
        {
            bool changed = false;
            GameObject[] allObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < allObjects.Length; rootIndex++)
            {
                Transform[] transforms = allObjects[rootIndex].GetComponentsInChildren<Transform>(true);
                for (int i = transforms.Length - 1; i >= 0; i--)
                {
                    Transform current = transforms[i];
                    if (current == null || current.name != objectName)
                    {
                        continue;
                    }

                    UnityEngine.Object.DestroyImmediate(current.gameObject);
                    changed = true;
                    result.Notes.Add("Removed scene object '" + objectName + "'.");
                }
            }

            return changed;
        }

        private static int CountPlayerRootMonoBehaviours()
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (prefabRoot == null)
            {
                return -1;
            }

            return prefabRoot.GetComponents<MonoBehaviour>().Length;
        }

        private static bool SetObjectReference(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == value)
            {
                return false;
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        private static bool SetBool(SerializedObject serializedObject, string propertyName, bool value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.boolValue == value)
            {
                return false;
            }

            property.boolValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        private static Transform FindChildByName(Transform root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == objectName)
            {
                return root;
            }

            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] != null && children[i].name == objectName)
                {
                    return children[i];
                }
            }

            return null;
        }

        private static string BuildTransformPath(Transform transform)
        {
            if (transform == null)
            {
                return "(null)";
            }

            string path = transform.name;
            Transform current = transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }
    }
}
