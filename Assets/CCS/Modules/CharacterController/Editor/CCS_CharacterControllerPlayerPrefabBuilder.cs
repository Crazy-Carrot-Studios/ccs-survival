using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Tests;
using CCS.Modules.CharacterController.Tests.Netcode;
using TMPro;
using Unity.Cinemachine;
using UnityEditor;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterControllerPlayerPrefabBuilder
// CATEGORY: Modules / CharacterController / Editor
// PURPOSE: Ensures shared player prefab visuals and syncs networked player layout.
// PLACEMENT: Editor utility. Invoked from master test and netcode setup.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Adds NameplateRoot/PlayerNameText to base player and keeps networked in sync.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerPlayerPrefabBuilder
    {
        #region Public Methods

        public static bool EnsurePlayerPrefabs()
        {
            bool changed = false;
            changed |= EnsureNetworkedTestPlayerPrefab(CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return changed;
        }

        #endregion

        #region Private Methods

        private static bool EnsureNetworkedTestPlayerPrefab(string prefabPath)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError("[Player Prefab Builder] Missing prefab: " + prefabPath);
                return false;
            }

            bool changed = false;
            changed |= EnsureNameplateHierarchy(prefabRoot.transform);
            changed |= EnsureCapsuleBodyVisual(prefabRoot.transform);
            changed |= EnsureGlassesVisual(prefabRoot.transform);
            changed |= EnsureCameraPivotSetup(prefabRoot.transform);
            changed |= EnsureCameraFollowAnchorSetup(prefabRoot.transform);
            changed |= EnsurePlayerCameraPivotReferences(prefabRoot);
            changed |= RemoveEmbeddedCinemachine(prefabRoot);
            changed |= EnsureNetworkComponents(prefabRoot);
            changed |= EnsureNetworkObjectTransformSettings(prefabRoot);
            changed |= WireNetworkNameplate(prefabRoot.transform);
            changed |= WireNetworkPlayerBehaviour(prefabRoot);
            changed |= EnsureOfflineBootstrap(prefabRoot);
            changed |= WireDisplayProfile(prefabRoot);
            changed |= ApplyDisplayProfileLayout(prefabRoot);

            RemoveMissingScriptsRecursive(prefabRoot.transform);

            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return changed;
        }

        private static bool EnsureOfflineBootstrap(GameObject prefabRoot)
        {
            if (prefabRoot.GetComponent<CCS_TestPlayerOfflineBootstrap>() != null)
            {
                return false;
            }

            prefabRoot.AddComponent<CCS_TestPlayerOfflineBootstrap>();
            return true;
        }

        private static bool WireDisplayProfile(GameObject prefabRoot)
        {
            CCS_TestPlayerDisplayProfile displayProfile = AssetDatabase.LoadAssetAtPath<CCS_TestPlayerDisplayProfile>(
                CCS_TestPlayerPrefabConstants.DefaultDisplayProfilePath);
            if (displayProfile == null)
            {
                return false;
            }

            bool changed = false;
            CCS_TestPlayerOfflineBootstrap bootstrap = prefabRoot.GetComponent<CCS_TestPlayerOfflineBootstrap>();
            if (bootstrap != null)
            {
                SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
                changed |= SetObjectReference(serializedBootstrap, "displayProfile", displayProfile);
            }

            return changed;
        }

        private static bool ApplyDisplayProfileLayout(GameObject prefabRoot)
        {
            CCS_TestPlayerDisplayProfile displayProfile = AssetDatabase.LoadAssetAtPath<CCS_TestPlayerDisplayProfile>(
                CCS_TestPlayerPrefabConstants.DefaultDisplayProfilePath);
            if (displayProfile == null)
            {
                return false;
            }

            CCS_TestPlayerDisplayProfileApplicator.ApplyVisualLayout(prefabRoot, displayProfile);
            return true;
        }

        private static bool EnsureVisualSetupOnPrefab(string prefabPath)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError("[Player Prefab Builder] Missing prefab: " + prefabPath);
                return false;
            }

            bool changed = false;
            changed |= EnsureNameplateHierarchy(prefabRoot.transform);
            changed |= EnsureCapsuleBodyVisual(prefabRoot.transform);
            changed |= EnsureGlassesVisual(prefabRoot.transform);
            changed |= EnsureCameraPivotSetup(prefabRoot.transform);
            changed |= EnsureCameraFollowAnchorSetup(prefabRoot.transform);
            changed |= EnsurePlayerCameraPivotReferences(prefabRoot);
            changed |= RemoveEmbeddedCinemachine(prefabRoot);
            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return changed;
        }

        private static bool SyncNetworkedPlayerPrefabFromBase()
        {
            GameObject baseRoot = PrefabUtility.LoadPrefabContents(
                CCS_CharacterControllerMasterTestLayoutConstants.PlayerPrefabPath);
            GameObject networkedRoot = PrefabUtility.LoadPrefabContents(
                CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath);
            if (baseRoot == null || networkedRoot == null)
            {
                if (baseRoot != null)
                {
                    PrefabUtility.UnloadPrefabContents(baseRoot);
                }

                if (networkedRoot != null)
                {
                    PrefabUtility.UnloadPrefabContents(networkedRoot);
                }

                Debug.LogError("[Player Prefab Builder] Could not load base or networked player prefab.");
                return false;
            }

            bool changed = false;
            changed |= EnsureNameplateHierarchy(networkedRoot.transform);
            changed |= EnsureCapsuleBodyVisual(networkedRoot.transform);
            changed |= EnsureGlassesVisual(networkedRoot.transform);
            changed |= EnsureCameraPivotSetup(networkedRoot.transform);
            changed |= EnsureCameraFollowAnchorSetup(networkedRoot.transform);
            changed |= EnsurePlayerCameraPivotReferences(networkedRoot);
            changed |= RemoveEmbeddedCinemachine(networkedRoot);
            changed |= AlignNameplateLayout(baseRoot.transform, networkedRoot.transform);
            changed |= AlignGlassesVisualLayout(baseRoot.transform, networkedRoot.transform);
            changed |= EnsureNetworkComponents(networkedRoot);
            changed |= EnsureNetworkObjectTransformSettings(networkedRoot);
            changed |= WireNetworkNameplate(networkedRoot.transform);
            changed |= WireNetworkPlayerBehaviour(networkedRoot);

            RemoveMissingScriptsRecursive(networkedRoot.transform);

            PrefabUtility.SaveAsPrefabAsset(
                networkedRoot,
                CCS_NetcodeTestConstants.NetworkedPlayerPrefabPath);

            PrefabUtility.UnloadPrefabContents(baseRoot);
            PrefabUtility.UnloadPrefabContents(networkedRoot);
            return true;
        }

        private static bool EnsureNameplateHierarchy(Transform playerRoot)
        {
            bool changed = false;
            Transform nameplateRoot = FindChildRecursive(
                playerRoot,
                CCS_CharacterControllerMasterTestLayoutConstants.NameplateRootObjectName);
            if (nameplateRoot == null)
            {
                GameObject nameplateObject = new GameObject(
                    CCS_CharacterControllerMasterTestLayoutConstants.NameplateRootObjectName);
                nameplateRoot = nameplateObject.transform;
                nameplateRoot.SetParent(playerRoot, false);
                changed = true;
            }

            if (nameplateRoot.parent != playerRoot)
            {
                nameplateRoot.SetParent(playerRoot, false);
                changed = true;
            }

            if (nameplateRoot.localPosition != CCS_CharacterControllerMasterTestLayoutConstants.NameplateRootLocalPosition)
            {
                nameplateRoot.localPosition = CCS_CharacterControllerMasterTestLayoutConstants.NameplateRootLocalPosition;
                changed = true;
            }

            if (nameplateRoot.localRotation != Quaternion.identity)
            {
                nameplateRoot.localRotation = Quaternion.identity;
                changed = true;
            }

            if (nameplateRoot.localScale != Vector3.one)
            {
                nameplateRoot.localScale = Vector3.one;
                changed = true;
            }

            Transform legacyText = nameplateRoot.Find(CCS_NetcodeTestConstants.LegacyNameplateTextObjectName);
            Transform existingPlayerNameText = nameplateRoot.Find(
                CCS_CharacterControllerMasterTestLayoutConstants.PlayerNameTextObjectName);
            if (legacyText != null)
            {
                if (existingPlayerNameText != null && legacyText != existingPlayerNameText)
                {
                    Object.DestroyImmediate(legacyText.gameObject);
                    changed = true;
                }
                else if (existingPlayerNameText == null)
                {
                    legacyText.name = CCS_CharacterControllerMasterTestLayoutConstants.PlayerNameTextObjectName;
                    changed = true;
                }
            }

            Transform nameplateTextTransform = nameplateRoot.Find(
                CCS_CharacterControllerMasterTestLayoutConstants.PlayerNameTextObjectName);
            if (nameplateTextTransform == null)
            {
                GameObject textObject = new GameObject(
                    CCS_CharacterControllerMasterTestLayoutConstants.PlayerNameTextObjectName);
                nameplateTextTransform = textObject.transform;
                nameplateTextTransform.SetParent(nameplateRoot, false);
                changed = true;
            }

            if (nameplateTextTransform == null)
            {
                return changed;
            }

            if (nameplateTextTransform.localPosition != Vector3.zero
                || nameplateTextTransform.localRotation != Quaternion.identity
                || nameplateTextTransform.localScale != Vector3.one)
            {
                nameplateTextTransform.localPosition = Vector3.zero;
                nameplateTextTransform.localRotation = Quaternion.identity;
                nameplateTextTransform.localScale = Vector3.one;
                changed = true;
            }

            TextMeshPro textMesh = nameplateTextTransform.GetComponent<TextMeshPro>();
            if (textMesh == null)
            {
                textMesh = nameplateTextTransform.gameObject.AddComponent<TextMeshPro>();
                nameplateTextTransform = textMesh.transform;
                changed = true;
            }

            if (textMesh.text != CCS_CharacterControllerMasterTestLayoutConstants.DefaultPlayerDisplayName)
            {
                textMesh.text = CCS_CharacterControllerMasterTestLayoutConstants.DefaultPlayerDisplayName;
                changed = true;
            }

            if (textMesh.fontSize != 2.5f)
            {
                textMesh.fontSize = 2.5f;
                changed = true;
            }

            if (textMesh.alignment != TextAlignmentOptions.Center)
            {
                textMesh.alignment = TextAlignmentOptions.Center;
                changed = true;
            }

            if (textMesh.rectTransform.sizeDelta != new Vector2(2f, 0.5f))
            {
                textMesh.rectTransform.sizeDelta = new Vector2(2f, 0.5f);
                changed = true;
            }

            if (textMesh.raycastTarget)
            {
                textMesh.raycastTarget = false;
                changed = true;
            }

            Collider textCollider = nameplateTextTransform.GetComponent<Collider>();
            if (textCollider != null)
            {
                Object.DestroyImmediate(textCollider);
                changed = true;
            }

            Collider rootCollider = nameplateRoot.GetComponent<Collider>();
            if (rootCollider != null)
            {
                Object.DestroyImmediate(rootCollider);
                changed = true;
            }

            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(nameplateRoot.gameObject);
            for (int i = 0; i < nameplateRoot.childCount; i++)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(nameplateRoot.GetChild(i).gameObject);
            }

            if (nameplateRoot.GetComponent<CCS_PlayerNameplateBillboard>() == null)
            {
                nameplateRoot.gameObject.AddComponent<CCS_PlayerNameplateBillboard>();
                changed = true;
            }

            return changed;
        }

        private static bool EnsureCapsuleBodyVisual(Transform playerRoot)
        {
            Material yellowMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                CCS_CharacterControllerMasterTestLayoutConstants.PlayerYellowMaterialPath);
            if (yellowMaterial == null)
            {
                return false;
            }

            Transform bodyVisual = playerRoot.Find(CCS_CharacterControllerMasterTestLayoutConstants.CapsuleVisualName);
            if (bodyVisual == null)
            {
                return false;
            }

            bool changed = false;
            if (bodyVisual.localPosition != CCS_CharacterControllerMasterTestLayoutConstants.CapsuleVisualLocalPosition)
            {
                bodyVisual.localPosition = CCS_CharacterControllerMasterTestLayoutConstants.CapsuleVisualLocalPosition;
                changed = true;
            }

            if (bodyVisual.localScale != CCS_CharacterControllerMasterTestLayoutConstants.CapsuleVisualLocalScale)
            {
                bodyVisual.localScale = CCS_CharacterControllerMasterTestLayoutConstants.CapsuleVisualLocalScale;
                changed = true;
            }

            MeshRenderer renderer = bodyVisual.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                return changed;
            }

            if (renderer.sharedMaterial != yellowMaterial)
            {
                renderer.sharedMaterial = yellowMaterial;
                changed = true;
            }

            return changed;
        }

        private static bool EnsureGlassesVisual(Transform playerRoot)
        {
            Material blackMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                CCS_CharacterControllerMasterTestLayoutConstants.PlayerBlackMaterialPath);
            if (blackMaterial == null)
            {
                return false;
            }

            Transform glasses = playerRoot.Find(CCS_CharacterControllerMasterTestLayoutConstants.GlassesVisualName);
            Transform legacyGlasses = playerRoot.Find("GlassesVisual");
            bool changed = false;
            if (legacyGlasses != null && legacyGlasses != glasses)
            {
                if (glasses != null)
                {
                    Object.DestroyImmediate(legacyGlasses.gameObject);
                }
                else
                {
                    legacyGlasses.name = CCS_CharacterControllerMasterTestLayoutConstants.GlassesVisualName;
                    glasses = legacyGlasses;
                    changed = true;
                }
            }

            if (glasses != null && !IsCapsuleGlassesVisual(glasses))
            {
                Object.DestroyImmediate(glasses.gameObject);
                glasses = null;
            }

            if (glasses == null)
            {
                GameObject glassesObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                glassesObject.name = CCS_CharacterControllerMasterTestLayoutConstants.GlassesVisualName;
                glasses = glassesObject.transform;
                glasses.SetParent(playerRoot, false);
                changed = true;
            }

            Collider collider = glasses.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
                changed = true;
            }

            MeshFilter meshFilter = glasses.GetComponent<MeshFilter>();
            if (meshFilter != null && (meshFilter.sharedMesh == null || meshFilter.sharedMesh.name != "Capsule"))
            {
                meshFilter.sharedMesh = GetBuiltinCapsuleMesh();
                changed = true;
            }

            if (glasses.localPosition != CCS_CharacterControllerMasterTestLayoutConstants.GlassesVisualLocalPosition)
            {
                glasses.localPosition = CCS_CharacterControllerMasterTestLayoutConstants.GlassesVisualLocalPosition;
                changed = true;
            }

            Quaternion expectedRotation = Quaternion.Euler(
                CCS_CharacterControllerMasterTestLayoutConstants.GlassesVisualLocalEuler);
            if (glasses.localRotation != expectedRotation)
            {
                glasses.localRotation = expectedRotation;
                changed = true;
            }

            if (glasses.localScale != CCS_CharacterControllerMasterTestLayoutConstants.GlassesVisualLocalScale)
            {
                glasses.localScale = CCS_CharacterControllerMasterTestLayoutConstants.GlassesVisualLocalScale;
                changed = true;
            }

            MeshRenderer renderer = glasses.GetComponent<MeshRenderer>();
            if (renderer != null && renderer.sharedMaterial != blackMaterial)
            {
                renderer.sharedMaterial = blackMaterial;
                changed = true;
            }

            return changed;
        }

        private static bool EnsureCameraPivotSetup(Transform playerRoot)
        {
            Transform cameraPivot = FindChildRecursive(playerRoot, "CameraPivot");
            if (cameraPivot == null)
            {
                GameObject pivotObject = new GameObject("CameraPivot");
                cameraPivot = pivotObject.transform;
                cameraPivot.SetParent(playerRoot, false);
            }

            bool changed = false;
            Vector3 expectedPivotPosition = new Vector3(
                0f,
                CCS_CharacterControllerMasterTestLayoutConstants.CameraPivotFollowTargetHeight,
                0f);
            if (cameraPivot.localPosition != expectedPivotPosition)
            {
                cameraPivot.localPosition = expectedPivotPosition;
                changed = true;
            }

            Transform lookTarget = cameraPivot.Find("CameraLookTarget");
            if (lookTarget == null)
            {
                lookTarget = FindChildRecursive(playerRoot, "CameraLookTarget");
            }

            if (lookTarget == null)
            {
                GameObject lookTargetObject = new GameObject("CameraLookTarget");
                lookTarget = lookTargetObject.transform;
                changed = true;
            }

            if (lookTarget.parent != cameraPivot)
            {
                lookTarget.SetParent(cameraPivot, false);
                changed = true;
            }

            if (lookTarget.localPosition != Vector3.zero)
            {
                lookTarget.localPosition = Vector3.zero;
                changed = true;
            }

            if (lookTarget.localRotation != Quaternion.identity)
            {
                lookTarget.localRotation = Quaternion.identity;
                changed = true;
            }

            return changed;
        }

        private static bool EnsureCameraFollowAnchorSetup(Transform playerRoot)
        {
            bool changed = false;
            Transform followAnchorTransform = FindChildRecursive(playerRoot, "CameraFollowAnchor");
            if (followAnchorTransform == null)
            {
                GameObject anchorObject = new GameObject("CameraFollowAnchor");
                followAnchorTransform = anchorObject.transform;
                followAnchorTransform.SetParent(playerRoot, false);
                changed = true;
            }

            CCS_CharacterCameraFollowAnchor followAnchor =
                followAnchorTransform.GetComponent<CCS_CharacterCameraFollowAnchor>();
            if (followAnchor == null)
            {
                followAnchor = followAnchorTransform.gameObject.AddComponent<CCS_CharacterCameraFollowAnchor>();
                changed = true;
            }

            Transform lookTarget = followAnchorTransform.Find("CameraLookTarget");
            Transform legacyLookTarget = playerRoot.Find("CameraPivot/CameraLookTarget");
            if (lookTarget == null && legacyLookTarget != null)
            {
                legacyLookTarget.SetParent(followAnchorTransform, false);
                lookTarget = legacyLookTarget;
                changed = true;
            }

            if (lookTarget == null)
            {
                GameObject lookTargetObject = new GameObject("CameraLookTarget");
                lookTarget = lookTargetObject.transform;
                lookTarget.SetParent(followAnchorTransform, false);
                changed = true;
            }

            if (lookTarget.localPosition != Vector3.zero)
            {
                lookTarget.localPosition = Vector3.zero;
                changed = true;
            }

            if (lookTarget.localRotation != Quaternion.identity)
            {
                lookTarget.localRotation = Quaternion.identity;
                changed = true;
            }

            followAnchor.Configure(
                playerRoot,
                lookTarget,
                CCS_CharacterControllerMasterTestLayoutConstants.CameraPivotFollowTargetHeight);

            return changed;
        }

        private static bool EnsurePlayerCameraPivotReferences(GameObject playerRoot)
        {
            CCS_CharacterCameraController cameraController = playerRoot.GetComponent<CCS_CharacterCameraController>();
            if (cameraController == null)
            {
                return false;
            }

            bool changed = false;
            SerializedObject serializedCamera = new SerializedObject(cameraController);
            SerializedProperty pivotProperty = serializedCamera.FindProperty("cameraPivot");
            SerializedProperty lookTargetProperty = serializedCamera.FindProperty("cameraLookTarget");
            Transform cameraPivot = playerRoot.transform.Find("CameraPivot");
            Transform cameraLookTarget = cameraPivot != null ? cameraPivot.Find("CameraLookTarget") : null;

            if (pivotProperty != null && cameraPivot != null && pivotProperty.objectReferenceValue != cameraPivot)
            {
                pivotProperty.objectReferenceValue = cameraPivot;
                changed = true;
            }

            if (lookTargetProperty != null && cameraLookTarget != null
                && lookTargetProperty.objectReferenceValue != cameraLookTarget)
            {
                lookTargetProperty.objectReferenceValue = cameraLookTarget;
                changed = true;
            }

            if (changed)
            {
                serializedCamera.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool RemoveEmbeddedCinemachine(GameObject playerRoot)
        {
            bool changed = false;
            Transform embeddedCinemachine = playerRoot.transform.Find("CM_ThirdPersonSurvival");
            if (embeddedCinemachine != null)
            {
                Object.DestroyImmediate(embeddedCinemachine.gameObject);
                changed = true;
            }

            Transform embeddedCamera = playerRoot.transform.Find("Main Camera");
            if (embeddedCamera != null)
            {
                Object.DestroyImmediate(embeddedCamera.gameObject);
                changed = true;
            }

            CCS_CharacterCameraController cameraController = playerRoot.GetComponent<CCS_CharacterCameraController>();
            if (cameraController != null)
            {
                SerializedObject serializedCamera = new SerializedObject(cameraController);
                SerializedProperty cinemachineProperty = serializedCamera.FindProperty("cinemachineCamera");
                if (cinemachineProperty.objectReferenceValue != null)
                {
                    cinemachineProperty.objectReferenceValue = null;
                    serializedCamera.ApplyModifiedPropertiesWithoutUndo();
                    changed = true;
                }
            }

            return changed;
        }

        private static bool AlignGlassesVisualLayout(Transform baseRoot, Transform networkedRoot)
        {
            Transform baseGlasses = FindChildRecursive(
                baseRoot,
                CCS_CharacterControllerMasterTestLayoutConstants.GlassesVisualName);
            Transform networkedGlasses = FindChildRecursive(
                networkedRoot,
                CCS_CharacterControllerMasterTestLayoutConstants.GlassesVisualName);
            if (baseGlasses == null || networkedGlasses == null)
            {
                return false;
            }

            bool changed = false;
            if (networkedGlasses.localPosition != baseGlasses.localPosition)
            {
                networkedGlasses.localPosition = baseGlasses.localPosition;
                changed = true;
            }

            if (networkedGlasses.localRotation != baseGlasses.localRotation)
            {
                networkedGlasses.localRotation = baseGlasses.localRotation;
                changed = true;
            }

            if (networkedGlasses.localScale != baseGlasses.localScale)
            {
                networkedGlasses.localScale = baseGlasses.localScale;
                changed = true;
            }

            return changed;
        }

        private static bool IsCapsuleGlassesVisual(Transform glasses)
        {
            MeshFilter meshFilter = glasses.GetComponent<MeshFilter>();
            return meshFilter != null && meshFilter.sharedMesh != null && meshFilter.sharedMesh.name == "Capsule";
        }

        private static Mesh GetBuiltinCapsuleMesh()
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Mesh capsuleMesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(temp);
            return capsuleMesh;
        }

        private static bool AlignNameplateLayout(Transform baseRoot, Transform networkedRoot)
        {
            Transform baseNameplate = FindChildRecursive(
                baseRoot,
                CCS_CharacterControllerMasterTestLayoutConstants.NameplateRootObjectName);
            Transform networkedNameplate = FindChildRecursive(
                networkedRoot,
                CCS_CharacterControllerMasterTestLayoutConstants.NameplateRootObjectName);
            if (baseNameplate == null || networkedNameplate == null)
            {
                return false;
            }

            bool changed = false;
            if (networkedNameplate.localPosition != baseNameplate.localPosition)
            {
                networkedNameplate.localPosition = baseNameplate.localPosition;
                changed = true;
            }

            if (networkedNameplate.localRotation != baseNameplate.localRotation)
            {
                networkedNameplate.localRotation = baseNameplate.localRotation;
                changed = true;
            }

            if (networkedNameplate.localScale != baseNameplate.localScale)
            {
                networkedNameplate.localScale = baseNameplate.localScale;
                changed = true;
            }

            Transform baseText = baseNameplate.Find(
                CCS_CharacterControllerMasterTestLayoutConstants.PlayerNameTextObjectName);
            Transform networkedText = networkedNameplate.Find(
                CCS_CharacterControllerMasterTestLayoutConstants.PlayerNameTextObjectName);
            if (baseText != null && networkedText != null)
            {
                if (networkedText.localPosition != baseText.localPosition)
                {
                    networkedText.localPosition = baseText.localPosition;
                    changed = true;
                }

                if (networkedText.localRotation != baseText.localRotation)
                {
                    networkedText.localRotation = baseText.localRotation;
                    changed = true;
                }

                if (networkedText.localScale != baseText.localScale)
                {
                    networkedText.localScale = baseText.localScale;
                    changed = true;
                }
            }

            return changed;
        }

        private static bool EnsureNetworkComponents(GameObject networkedRoot)
        {
            bool changed = false;
            if (networkedRoot.GetComponent<NetworkObject>() == null)
            {
                networkedRoot.AddComponent<NetworkObject>();
                changed = true;
            }

            NetworkTransform existingTransform = networkedRoot.GetComponent<NetworkTransform>();
            if (existingTransform != null && existingTransform.GetType() != typeof(CCS_ClientOwnerNetworkTransform))
            {
                Object.DestroyImmediate(existingTransform, true);
                changed = true;
            }

            if (networkedRoot.GetComponent<CCS_ClientOwnerNetworkTransform>() == null)
            {
                networkedRoot.AddComponent<CCS_ClientOwnerNetworkTransform>();
                changed = true;
            }

            if (networkedRoot.GetComponent<CCS_ControllerTestNetworkPlayerBehaviour>() == null)
            {
                networkedRoot.AddComponent<CCS_ControllerTestNetworkPlayerBehaviour>();
                changed = true;
            }

            if (networkedRoot.GetComponent<CCS_NetworkPlayerNameplate>() == null)
            {
                networkedRoot.AddComponent<CCS_NetworkPlayerNameplate>();
                changed = true;
            }

            return changed;
        }

        private static bool EnsureNetworkObjectTransformSettings(GameObject networkedRoot)
        {
            bool changed = false;
            NetworkObject networkObject = networkedRoot.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                SerializedObject serializedNetworkObject = new SerializedObject(networkObject);
                SerializedProperty synchronizeTransform = serializedNetworkObject.FindProperty("SynchronizeTransform");
                if (synchronizeTransform != null && synchronizeTransform.boolValue)
                {
                    synchronizeTransform.boolValue = false;
                    serializedNetworkObject.ApplyModifiedPropertiesWithoutUndo();
                    changed = true;
                }
            }

            CCS_ClientOwnerNetworkTransform networkTransform =
                networkedRoot.GetComponent<CCS_ClientOwnerNetworkTransform>();
            if (networkTransform == null)
            {
                return changed;
            }

            SerializedObject serializedTransform = new SerializedObject(networkTransform);
            SerializedProperty authorityMode = serializedTransform.FindProperty("AuthorityMode");
            if (authorityMode != null && authorityMode.enumValueIndex != (int)NetworkTransform.AuthorityModes.Owner)
            {
                authorityMode.enumValueIndex = (int)NetworkTransform.AuthorityModes.Owner;
                changed = true;
            }

            SerializedProperty interpolate = serializedTransform.FindProperty("Interpolate");
            if (interpolate != null && !interpolate.boolValue)
            {
                interpolate.boolValue = true;
                changed = true;
            }

            changed |= SetNetworkTransformBool(serializedTransform, "SyncRotAngleX", false);
            changed |= SetNetworkTransformBool(serializedTransform, "SyncRotAngleY", false);
            changed |= SetNetworkTransformBool(serializedTransform, "SyncRotAngleZ", false);

            if (changed)
            {
                serializedTransform.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool SetNetworkTransformBool(SerializedObject serializedTransform, string propertyName, bool value)
        {
            SerializedProperty property = serializedTransform.FindProperty(propertyName);
            if (property == null || property.boolValue == value)
            {
                return false;
            }

            property.boolValue = value;
            return true;
        }

        private static bool WireNetworkPlayerBehaviour(GameObject networkedRoot)
        {
            CCS_ControllerTestNetworkPlayerBehaviour behaviour =
                networkedRoot.GetComponent<CCS_ControllerTestNetworkPlayerBehaviour>();
            if (behaviour == null)
            {
                return false;
            }

            Material yellowMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                CCS_CharacterControllerMasterTestLayoutConstants.PlayerYellowMaterialPath);
            Material blackMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                CCS_CharacterControllerMasterTestLayoutConstants.PlayerBlackMaterialPath);
            Transform bodyVisual = networkedRoot.transform.Find(
                CCS_CharacterControllerMasterTestLayoutConstants.CapsuleVisualName);
            Transform glassesVisual = networkedRoot.transform.Find(
                CCS_CharacterControllerMasterTestLayoutConstants.GlassesVisualName);
            Transform cameraPivot = networkedRoot.transform.Find("CameraPivot");
            Transform cameraLookTarget = cameraPivot != null ? cameraPivot.Find("CameraLookTarget") : null;

            SerializedObject serializedBehaviour = new SerializedObject(behaviour);
            bool changed = false;
            changed |= SetObjectReference(serializedBehaviour, "yellowBodyMaterial", yellowMaterial);
            changed |= SetObjectReference(serializedBehaviour, "blackGlassesMaterial", blackMaterial);
            changed |= SetObjectReference(
                serializedBehaviour,
                "bodyRenderer",
                bodyVisual != null ? bodyVisual.GetComponent<Renderer>() : null);
            changed |= SetObjectReference(
                serializedBehaviour,
                "glassesRenderer",
                glassesVisual != null ? glassesVisual.GetComponent<Renderer>() : null);
            changed |= SetObjectReference(
                serializedBehaviour,
                "inputProvider",
                networkedRoot.GetComponent<CCS_CharacterInputActionProvider>());
            changed |= SetObjectReference(
                serializedBehaviour,
                "motor",
                networkedRoot.GetComponent<CCS_CharacterMotor>());
            changed |= SetObjectReference(
                serializedBehaviour,
                "controllerService",
                networkedRoot.GetComponent<CCS_CharacterControllerService>());
            changed |= SetObjectReference(
                serializedBehaviour,
                "debugHud",
                networkedRoot.GetComponent<CCS_CharacterControllerDebugHud>());
            changed |= SetObjectReference(
                serializedBehaviour,
                "playerCameraController",
                networkedRoot.GetComponent<CCS_CharacterCameraController>());
            changed |= SetObjectReference(serializedBehaviour, "cameraPivot", cameraPivot);
            changed |= SetObjectReference(serializedBehaviour, "cameraLookTarget", cameraLookTarget);

            if (changed)
            {
                serializedBehaviour.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool SetObjectReference(
            SerializedObject serializedObject,
            string propertyName,
            Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == value)
            {
                return false;
            }

            property.objectReferenceValue = value;
            return true;
        }

        private static bool WireNetworkNameplate(Transform networkedRoot)
        {
            CCS_NetworkPlayerNameplate nameplate = networkedRoot.GetComponent<CCS_NetworkPlayerNameplate>();
            Transform nameplateRoot = FindChildRecursive(
                networkedRoot,
                CCS_CharacterControllerMasterTestLayoutConstants.NameplateRootObjectName);
            Transform nameplateText = nameplateRoot != null
                ? nameplateRoot.Find(CCS_CharacterControllerMasterTestLayoutConstants.PlayerNameTextObjectName)
                : null;
            TextMeshPro textMesh = nameplateText != null ? nameplateText.GetComponent<TextMeshPro>() : null;
            CCS_PlayerNameplateBillboard nameplateBillboard = nameplateRoot != null
                ? nameplateRoot.GetComponent<CCS_PlayerNameplateBillboard>()
                : null;
            if (nameplate == null || nameplateRoot == null || textMesh == null || nameplateBillboard == null)
            {
                return false;
            }

            SerializedObject serializedNameplate = new SerializedObject(nameplate);
            SerializedProperty rootProperty = serializedNameplate.FindProperty("nameplateRoot");
            SerializedProperty textProperty = serializedNameplate.FindProperty("nameplateText");
            SerializedProperty billboardProperty = serializedNameplate.FindProperty("nameplateBillboard");
            bool changed = false;
            if (rootProperty.objectReferenceValue != nameplateRoot)
            {
                rootProperty.objectReferenceValue = nameplateRoot;
                changed = true;
            }

            if (textProperty.objectReferenceValue != textMesh)
            {
                textProperty.objectReferenceValue = textMesh;
                changed = true;
            }

            if (billboardProperty != null && billboardProperty.objectReferenceValue != nameplateBillboard)
            {
                billboardProperty.objectReferenceValue = nameplateBillboard;
                changed = true;
            }

            if (changed)
            {
                serializedNameplate.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static void RemoveMissingScriptsRecursive(Transform root)
        {
            if (root == null)
            {
                return;
            }

            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root.gameObject);
            for (int i = 0; i < root.childCount; i++)
            {
                RemoveMissingScriptsRecursive(root.GetChild(i));
            }
        }

        private static Transform FindChildRecursive(Transform parent, string childName)
        {
            if (parent.name == childName)
            {
                return parent;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                Transform match = FindChildRecursive(child, childName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        #endregion
    }
}
