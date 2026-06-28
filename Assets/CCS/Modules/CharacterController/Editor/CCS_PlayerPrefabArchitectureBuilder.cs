using System;
using System.Collections.Generic;
using System.IO;
using CCS.Modules.Attributes;
using CCS.Modules.Attributes.Tests;
using CCS.Modules.CharacterController.Tests;
using CCS.Modules.CharacterController.Tests.Netcode;
using CCS.Modules.Interaction;
using CCS.Modules.Weapons;
using UnityEditor;
using UnityEditorInternal;
using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerPrefabArchitectureBuilder
// CATEGORY: Modules / CharacterController / Editor
// PURPOSE: Builds v0.8.0 production/test player prefab hierarchy from the legacy test prefab.
// PLACEMENT: Editor builder. Invoked from batch entry and architecture menu.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Preserves motor + CharacterController on root for NetworkTransform sync.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_PlayerPrefabArchitectureBuilder
    {
        private static readonly Type[] RuntimeSystemsComponentTypes =
        {
            typeof(CCS_CharacterInputActionProvider),
            typeof(CCS_CharacterCameraController),
            typeof(CCS_CharacterControllerService),
            typeof(CCS_NetworkInteractionScanner),
            typeof(CCS_AttributeContainer),
            typeof(CCS_AttributeService),
            typeof(CCS_NetworkAttributeReplicator),
            typeof(CCS_StaminaController),
            typeof(CCS_HealthRegenController),
            typeof(CCS_RevolverController),
            typeof(CCS_CharacterAimLocomotionController),
            typeof(CCS_PlayerWeaponLoadout),
            typeof(CCS_EquipmentSocketRegistry),
            typeof(CCS_PlayerEquipmentVisualController),
            typeof(CCS_WeaponCarryStateController),
            typeof(CCS_NetworkHealth),
            typeof(CCS_ControllerTestNetworkPlayerBehaviour),
        };

        private static readonly Type[] PresentationComponentTypes =
        {
            typeof(CCS_LocalFirstPersonHeadVisibility),
            typeof(CCS_FirstPersonBodyCameraAnchor),
            typeof(CCS_NetworkPlayerNameplate),
        };

        private static readonly Type[] LocalUiComponentTypes =
        {
            typeof(CCS_PlayerDeathScreenController),
        };

        private static readonly string[] LocalUiRootNames =
        {
            "AttributeHudRoot",
            "WeaponHudRoot",
            "InteractionPromptHudRoot",
        };

        #region Public Methods

        public static bool EnsurePlayerProductionArchitecture()
        {
            EnsurePlayerPrefabFolder();

            bool changed = false;
            changed |= RestructureLegacyMasterTestPrefab();
            changed |= EnsureProductionPrefabFromLegacy();
            changed |= EnsureTestHarnessCopyFromLegacy();

            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return changed;
        }

        #endregion

        #region Private Methods

        private static void EnsurePlayerPrefabFolder()
        {
            const string folderPath = "Assets/CCS/Prefabs/Player";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                if (!AssetDatabase.IsValidFolder("Assets/CCS/Prefabs"))
                {
                    AssetDatabase.CreateFolder("Assets/CCS", "Prefabs");
                }

                AssetDatabase.CreateFolder("Assets/CCS/Prefabs", "Player");
            }
        }

        private static bool RestructureLegacyMasterTestPrefab()
        {
            return ApplyArchitectureToPrefabContents(
                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath,
                CCS_TestPlayerPrefabConstants.NetworkedPlayerInstanceName,
                stripTestOnlyComponents: false,
                disableDebugFlags: false);
        }

        private static bool EnsureProductionPrefabFromLegacy()
        {
            string sourcePath = CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath;
            string targetPath = CCS_PlayerPrefabConstants.ProductionPlayerPrefabPath;

            if (!File.Exists(sourcePath))
            {
                Debug.LogError("[Player Prefab Architecture] Missing source prefab: " + sourcePath);
                return false;
            }

            bool changed = false;
            if (!File.Exists(targetPath))
            {
                AssetDatabase.CopyAsset(sourcePath, targetPath);
                changed = true;
            }

            changed |= ApplyArchitectureToPrefabContents(
                targetPath,
                CCS_PlayerPrefabConstants.ProductionPlayerInstanceName,
                stripTestOnlyComponents: true,
                disableDebugFlags: true);

            return changed;
        }

        private static bool EnsureTestHarnessCopyFromLegacy()
        {
            string sourcePath = CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath;
            string targetPath = CCS_PlayerPrefabConstants.TestHarnessPlayerPrefabPath;

            if (!File.Exists(sourcePath))
            {
                return false;
            }

            bool changed = false;
            if (!File.Exists(targetPath))
            {
                AssetDatabase.CopyAsset(sourcePath, targetPath);
                changed = true;
            }

            changed |= ApplyArchitectureToPrefabContents(
                targetPath,
                CCS_PlayerPrefabConstants.TestHarnessPlayerInstanceName,
                stripTestOnlyComponents: false,
                disableDebugFlags: false);

            return changed;
        }

        private static bool ApplyArchitectureToPrefabContents(
            string prefabPath,
            string rootName,
            bool stripTestOnlyComponents,
            bool disableDebugFlags)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError("[Player Prefab Architecture] Could not load prefab: " + prefabPath);
                return false;
            }

            bool changed = false;
            if (prefabRoot.name != rootName)
            {
                prefabRoot.name = rootName;
                changed = true;
            }

            Transform runtimeSystems = EnsureChildTransform(
                prefabRoot.transform,
                CCS_PlayerPrefabConstants.RuntimeSystemsObjectName,
                ref changed);
            Transform presentation = EnsureChildTransform(
                prefabRoot.transform,
                CCS_PlayerPrefabConstants.PresentationObjectName,
                ref changed);
            Transform localUi = EnsureChildTransform(
                prefabRoot.transform,
                CCS_PlayerPrefabConstants.PlayerLocalUiObjectName,
                ref changed);

            changed |= ReparentNamedChild(prefabRoot.transform, presentation, "VisualRoot");
            if (stripTestOnlyComponents)
            {
                changed |= RemoveTestVisualObjects(prefabRoot);
            }
            else
            {
                Transform testVisuals = EnsureChildTransform(
                    presentation,
                    CCS_PlayerPrefabConstants.TestVisualsObjectName,
                    ref changed);
                changed |= ReparentAllNamedChildren(prefabRoot.transform, testVisuals, CCS_TestPlayerPrefabConstants.CapsuleVisualName);
                changed |= ReparentAllNamedChildren(prefabRoot.transform, testVisuals, CCS_TestPlayerPrefabConstants.GlassesVisualName);
            }

            changed |= ReparentNamedChild(prefabRoot.transform, presentation, CCS_TestPlayerPrefabConstants.NameplateRootObjectName);

            for (int uiIndex = 0; uiIndex < LocalUiRootNames.Length; uiIndex++)
            {
                changed |= ReparentNamedChild(prefabRoot.transform, localUi, LocalUiRootNames[uiIndex]);
            }

            changed |= MoveRootComponents(prefabRoot, runtimeSystems.gameObject, RuntimeSystemsComponentTypes);
            changed |= MoveRootComponents(prefabRoot, presentation.gameObject, PresentationComponentTypes);
            changed |= MoveRootComponents(prefabRoot, localUi.gameObject, LocalUiComponentTypes);

            if (stripTestOnlyComponents)
            {
                changed |= DestroyComponentIfPresent<CCS_TestPlayerOfflineBootstrap>(prefabRoot);
                changed |= DestroyComponentIfPresent<CCS_TestPlayerAttributeDebugInput>(prefabRoot);
                changed |= DestroyComponentInChildren<CCS_TestPlayerAttributeDebugInput>(prefabRoot, true);
            }

            if (disableDebugFlags)
            {
                changed |= DisableProductionDebugFlags(prefabRoot);
            }

            changed |= EnsureFacade(prefabRoot);
            changed |= EnsureLocalUiBootstrap(localUi.gameObject, prefabRoot);
            changed |= WireFacadeReferences(prefabRoot);

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return changed;
        }

        private static Transform EnsureChildTransform(Transform parent, string childName, ref bool changed)
        {
            Transform existing = parent.Find(childName);
            if (existing != null)
            {
                return existing;
            }

            GameObject childObject = new GameObject(childName);
            childObject.transform.SetParent(parent, false);
            childObject.transform.localPosition = Vector3.zero;
            childObject.transform.localRotation = Quaternion.identity;
            childObject.transform.localScale = Vector3.one;
            changed = true;
            return childObject.transform;
        }

        private static bool ReparentNamedChild(Transform root, Transform newParent, string childName)
        {
            Transform child = FindDirectOrDeepChild(root, childName);
            if (child == null || newParent == null || child.parent == newParent)
            {
                return false;
            }

            child.SetParent(newParent, false);
            return true;
        }

        private static bool ReparentAllNamedChildren(Transform root, Transform newParent, string childName)
        {
            if (root == null || newParent == null)
            {
                return false;
            }

            bool changed = false;
            List<Transform> matches = CollectNamedChildren(root, childName);
            for (int index = 0; index < matches.Count; index++)
            {
                Transform child = matches[index];
                if (child == null || child.parent == newParent)
                {
                    continue;
                }

                child.SetParent(newParent, false);
                changed = true;
            }

            return changed;
        }

        private static List<Transform> CollectNamedChildren(Transform root, string childName)
        {
            List<Transform> results = new List<Transform>();
            CollectNamedChildrenRecursive(root, childName, results);
            return results;
        }

        private static void CollectNamedChildrenRecursive(Transform current, string childName, List<Transform> results)
        {
            if (current == null)
            {
                return;
            }

            if (current.name == childName)
            {
                results.Add(current);
            }

            for (int childIndex = 0; childIndex < current.childCount; childIndex++)
            {
                CollectNamedChildrenRecursive(current.GetChild(childIndex), childName, results);
            }
        }

        private static Transform FindDirectOrDeepChild(Transform root, string childName)
        {
            Transform direct = root.Find(childName);
            if (direct != null)
            {
                return direct;
            }

            List<Transform> matches = CollectNamedChildren(root, childName);
            return matches.Count > 0 ? matches[0] : null;
        }

        private static bool RemoveTestVisualObjects(GameObject prefabRoot)
        {
            return StripTestVisualObjects(prefabRoot);
        }

        private static bool StripTestVisualObjects(GameObject prefabRoot)
        {
            bool changed = false;
            Transform[] transforms = prefabRoot.GetComponentsInChildren<Transform>(true);
            for (int index = transforms.Length - 1; index >= 0; index--)
            {
                Transform current = transforms[index];
                if (current == null || !CCS_PlayerVisualAndAnimatorBindingBuilder.IsTestVisualObjectName(current.name))
                {
                    continue;
                }

                UnityEngine.Object.DestroyImmediate(current.gameObject, true);
                changed = true;
            }

            return changed;
        }

        private static bool MoveRootComponents(GameObject prefabRoot, GameObject destination, Type[] componentTypes)
        {
            if (destination == null)
            {
                return false;
            }

            bool changed = false;
            for (int typeIndex = 0; typeIndex < componentTypes.Length; typeIndex++)
            {
                Component sourceComponent = prefabRoot.GetComponent(componentTypes[typeIndex]);
                if (sourceComponent == null)
                {
                    continue;
                }

                if (destination.GetComponent(componentTypes[typeIndex]) != null)
                {
                    continue;
                }

                changed |= MoveComponent(sourceComponent, destination);
            }

            return changed;
        }

        private static bool MoveComponent(Component sourceComponent, GameObject destination)
        {
            if (sourceComponent == null || destination == null)
            {
                return false;
            }

            if (!ComponentUtility.CopyComponent(sourceComponent))
            {
                Debug.LogWarning(
                    "[Player Prefab Architecture] Could not copy component "
                    + sourceComponent.GetType().Name);
                return false;
            }

            if (!ComponentUtility.PasteComponentAsNew(destination))
            {
                return false;
            }

            UnityEngine.Object.DestroyImmediate(sourceComponent, true);
            return true;
        }

        private static bool EnsureFacade(GameObject prefabRoot)
        {
            CCS_PlayerRuntimeFacade facade = prefabRoot.GetComponent<CCS_PlayerRuntimeFacade>();
            if (facade != null)
            {
                return false;
            }

            prefabRoot.AddComponent<CCS_PlayerRuntimeFacade>();
            return true;
        }

        private static bool EnsureLocalUiBootstrap(GameObject localUiObject, GameObject prefabRoot)
        {
            if (localUiObject == null)
            {
                return false;
            }

            CCS_PlayerLocalOwnerUiBootstrap bootstrap = localUiObject.GetComponent<CCS_PlayerLocalOwnerUiBootstrap>();
            if (bootstrap == null)
            {
                bootstrap = localUiObject.AddComponent<CCS_PlayerLocalOwnerUiBootstrap>();
            }

            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            SerializedProperty facadeProperty = serializedBootstrap.FindProperty("runtimeFacade");
            if (facadeProperty != null && facadeProperty.objectReferenceValue == null)
            {
                facadeProperty.objectReferenceValue = prefabRoot.GetComponent<CCS_PlayerRuntimeFacade>();
                serializedBootstrap.ApplyModifiedPropertiesWithoutUndo();
                return true;
            }

            return bootstrap == null;
        }

        private static bool WireFacadeReferences(GameObject prefabRoot)
        {
            CCS_PlayerRuntimeFacade facade = prefabRoot.GetComponent<CCS_PlayerRuntimeFacade>();
            if (facade == null)
            {
                return false;
            }

            SerializedObject serializedFacade = new SerializedObject(facade);
            bool changed = false;
            changed |= SetObjectReference(serializedFacade, "networkObject", prefabRoot.GetComponent<NetworkObject>());
            changed |= SetObjectReference(serializedFacade, "motor", prefabRoot.GetComponent<CCS_CharacterMotor>());
            changed |= SetObjectReference(
                serializedFacade,
                "inputProvider",
                prefabRoot.GetComponentInChildren<CCS_CharacterInputActionProvider>(true));
            changed |= SetObjectReference(
                serializedFacade,
                "cameraController",
                prefabRoot.GetComponentInChildren<CCS_CharacterCameraController>(true));
            changed |= SetObjectReference(
                serializedFacade,
                "interactionScanner",
                prefabRoot.GetComponentInChildren<CCS_NetworkInteractionScanner>(true));
            changed |= SetObjectReference(
                serializedFacade,
                "attributeContainer",
                prefabRoot.GetComponentInChildren<CCS_AttributeContainer>(true));
            changed |= SetObjectReference(
                serializedFacade,
                "networkHealth",
                prefabRoot.GetComponentInChildren<CCS_NetworkHealth>(true));
            changed |= SetObjectReference(
                serializedFacade,
                "staminaController",
                prefabRoot.GetComponentInChildren<CCS_StaminaController>(true));
            changed |= SetObjectReference(
                serializedFacade,
                "healthRegenController",
                prefabRoot.GetComponentInChildren<CCS_HealthRegenController>(true));
            changed |= SetObjectReference(
                serializedFacade,
                "revolverController",
                prefabRoot.GetComponentInChildren<CCS_RevolverController>(true));
            changed |= SetObjectReference(
                serializedFacade,
                "weaponLoadout",
                prefabRoot.GetComponentInChildren<CCS_PlayerWeaponLoadout>(true));
            changed |= SetObjectReference(
                serializedFacade,
                "equipmentVisualController",
                prefabRoot.GetComponentInChildren<CCS_PlayerEquipmentVisualController>(true));

            Transform presentation = prefabRoot.transform.Find(CCS_PlayerPrefabConstants.PresentationObjectName);
            Animator animator = CCS_PlayerVisualAndAnimatorBindingBuilder.TryResolveAuthoritativeAnimator(
                prefabRoot,
                out Animator authoritativeAnimator)
                ? authoritativeAnimator
                : null;
            changed |= SetObjectReference(serializedFacade, "animator", animator);
            changed |= SetObjectReference(
                serializedFacade,
                "playerInteractionAnimator",
                prefabRoot.GetComponentInChildren<CCS_PlayerInteractionAnimator>(true));

            if (changed)
            {
                serializedFacade.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool DisableProductionDebugFlags(GameObject prefabRoot)
        {
            bool changed = false;
            changed |= SetBoolIfPresent(prefabRoot.GetComponentInChildren<CCS_CharacterMotor>(true), "enableMovementDebugLogs", false);
            changed |= SetBoolIfPresent(
                prefabRoot.GetComponentInChildren<CCS_CharacterCameraController>(true),
                "enableRuntimeCameraDebug",
                false);
            changed |= SetBoolIfPresent(
                prefabRoot.GetComponentInChildren<CCS_CharacterCameraController>(true),
                "debugCameraModeTransitions",
                false);
            changed |= SetBoolIfPresent(
                prefabRoot.GetComponentInChildren<CCS_CharacterCameraController>(true),
                "enableAimRayDebug",
                false);
            changed |= SetBoolIfPresent(
                prefabRoot.GetComponentInChildren<CCS_RevolverController>(true),
                "enableRuntimeWeaponDebug",
                false);
            changed |= SetBoolIfPresent(
                prefabRoot.GetComponentInChildren<CCS_RevolverController>(true),
                "enableAimRayDebug",
                false);
            changed |= SetBoolIfPresent(
                prefabRoot.GetComponentInChildren<CCS_RevolverController>(true),
                "enableMuzzleDebug",
                false);
            changed |= SetBoolIfPresent(
                prefabRoot.GetComponentInChildren<CCS_RevolverController>(true),
                "debugAimAlignment",
                false);
            changed |= SetBoolIfPresent(
                prefabRoot.GetComponentInChildren<CCS_RevolverController>(true),
                "debugVisualConvergence",
                false);
            changed |= SetBoolIfPresent(
                prefabRoot.GetComponentInChildren<CCS_RevolverController>(true),
                "debugAimCameraAlignment",
                false);
            changed |= SetBoolIfPresent(
                prefabRoot.GetComponentInChildren<CCS_PlayerEquipmentVisualController>(true),
                "debugRuntimeFitParity",
                false);
            changed |= SetBoolIfPresent(
                prefabRoot.GetComponentInChildren<CCS_PlayerEquipmentVisualController>(true),
                "debugEquipmentVisualProfileApplication",
                false);
            changed |= SetBoolIfPresent(
                prefabRoot.GetComponentInChildren<CCS_NetworkHealth>(true),
                "enableHealthDebugLogs",
                false);
            changed |= SetBoolIfPresent(
                prefabRoot.GetComponentInChildren<CCS_PlayerDeathScreenController>(true),
                "enableDeathScreenDebugLogs",
                false);
            changed |= SetBoolIfPresent(
                prefabRoot.GetComponentInChildren<CCS_CharacterAimLocomotionController>(true),
                "enableMovementDebugLogs",
                false);
            changed |= SetBoolIfPresent(
                prefabRoot.GetComponentInChildren<CCS_PlayerInteractionAnimator>(true),
                "enableInteractionDebugLogs",
                false);
            return changed;
        }

        private static bool SetBoolIfPresent(Component component, string propertyName, bool value)
        {
            if (component == null)
            {
                return false;
            }

            SerializedObject serializedObject = new SerializedObject(component);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.propertyType != SerializedPropertyType.Boolean || property.boolValue == value)
            {
                return false;
            }

            property.boolValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        private static bool SetObjectReference(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == value)
            {
                return false;
            }

            property.objectReferenceValue = value;
            return true;
        }

        private static bool DestroyComponentIfPresent<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            if (component == null)
            {
                return false;
            }

            UnityEngine.Object.DestroyImmediate(component, true);
            return true;
        }

        private static bool DestroyComponentInChildren<T>(GameObject target, bool includeInactive) where T : Component
        {
            T[] components = target.GetComponentsInChildren<T>(includeInactive);
            if (components.Length == 0)
            {
                return false;
            }

            bool changed = false;
            for (int index = 0; index < components.Length; index++)
            {
                if (components[index] == null)
                {
                    continue;
                }

                UnityEngine.Object.DestroyImmediate(components[index], true);
                changed = true;
            }

            return changed;
        }

        #endregion
    }
}
