using System.IO;
using CCS.Modules.CharacterController;
using CCS.Modules.Weapons;
using CCS.Modules.Weapons.Editor;
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
// NOTES: Ensures the shared network-capable test player prefab layout and wiring.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerPlayerPrefabBuilder
    {
        #region Public Methods

        public static bool EnsurePlayerPrefabs()
        {
            bool changed = false;
            changed |= EnsureNetworkedTestPlayerPrefab(
                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath,
                includeMasterTestBootstrap: true);
            changed |= EnsureNetworkedTestPlayerPrefab(
                CCS_PlayerPrefabConstants.TestHarnessPlayerPrefabPath,
                includeMasterTestBootstrap: true);
            changed |= EnsureNetworkedTestPlayerPrefab(
                CCS_PlayerPrefabConstants.ProductionPlayerPrefabPath,
                includeMasterTestBootstrap: false);
            changed |= EnsureTestNpcPrefab(CCS_CharacterControllerMasterTestLayoutConstants.NpcPrefabPath);
            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return changed;
        }

        #endregion

        #region Private Methods

        private static bool EnsureNetworkedTestPlayerPrefab(string prefabPath, bool includeMasterTestBootstrap)
        {
            if (!System.IO.File.Exists(prefabPath))
            {
                return false;
            }

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
            changed |= ApplyDisplayProfileLayout(prefabRoot);
            changed |= EnsureCameraFollowAnchorSetup(prefabRoot.transform);
            changed |= EnsurePlayerCameraCollisionExclusion(prefabRoot);
            changed |= EnsurePlayerCameraPivotReferences(prefabRoot);
            changed |= RemoveEmbeddedCinemachine(prefabRoot);
            changed |= EnsureNetworkComponents(prefabRoot);
            changed |= EnsurePlayerLocomotionComponents(prefabRoot);
            changed |= EnsureRevolverUpperBodyAnimator(prefabRoot);
            changed |= CCS_WeaponsTestPlayerPrefabBuilder.RemovePlayerWeaponVisualArtifacts(prefabRoot);
            changed |= CCS_EquipmentSocketPlayerBuilder.EnsurePlayerEquipmentSocketFoundation(prefabRoot);
            changed |= EnsureNetworkObjectTransformSettings(prefabRoot);
            changed |= RemoveCharacterControllerDebugHud(prefabRoot);
            changed |= WireNetworkNameplate(prefabRoot.transform);
            changed |= WireNetworkPlayerBehaviour(prefabRoot);
            if (includeMasterTestBootstrap)
            {
                changed |= EnsureOfflineBootstrap(prefabRoot);
                changed |= WireDisplayProfile(prefabRoot);
            }

            RemoveMissingScriptsRecursive(prefabRoot.transform);

            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return changed;
        }

        private static bool EnsureTestNpcPrefab(string prefabPath)
        {
            if (IsBrokenNpcVariantPrefab(prefabPath))
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }

            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset == null)
            {
                GameObject npcRoot = BuildTestNpcPrefabRoot();
                PrefabUtility.SaveAsPrefabAsset(npcRoot, prefabPath);
                Object.DestroyImmediate(npcRoot);
                return true;
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError("[Player Prefab Builder] Missing NPC prefab: " + prefabPath);
                return false;
            }

            bool changed = false;
            if (prefabRoot.name != CCS_CharacterControllerMasterTestLayoutConstants.NpcInstanceName)
            {
                prefabRoot.name = CCS_CharacterControllerMasterTestLayoutConstants.NpcInstanceName;
                changed = true;
            }

            changed |= EnsureNpcCoreComponents(prefabRoot);
            changed |= EnsureCapsuleBodyVisualForMaterial(
                prefabRoot.transform,
                AssetDatabase.LoadAssetAtPath<Material>(
                    CCS_CharacterControllerMasterTestLayoutConstants.PlayerGreenMaterialPath));
            changed |= EnsureGlassesVisual(prefabRoot.transform);
            changed |= EnsureNpcRunner(prefabRoot);
            changed |= StripNpcPlayerOnlyComponents(prefabRoot);
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
            Vector3 expectedPivotPosition = Vector3.zero;
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

            if (followAnchorTransform.localPosition != Vector3.zero)
            {
                followAnchorTransform.localPosition = Vector3.zero;
                changed = true;
            }

            Transform pitchTarget = followAnchorTransform.Find(CCS_CharacterControllerConstants.CameraPitchTargetObjectName);
            if (pitchTarget == null)
            {
                GameObject pitchObject = new GameObject(CCS_CharacterControllerConstants.CameraPitchTargetObjectName);
                pitchTarget = pitchObject.transform;
                pitchTarget.SetParent(followAnchorTransform, false);
                changed = true;
            }

            Vector3 expectedPitchLocalPosition = new Vector3(
                0f,
                CCS_CharacterControllerConstants.CameraPitchTargetLocalHeight,
                0f);
            if (pitchTarget.localPosition != expectedPitchLocalPosition)
            {
                pitchTarget.localPosition = expectedPitchLocalPosition;
                changed = true;
            }

            if (pitchTarget.localRotation != Quaternion.identity)
            {
                pitchTarget.localRotation = Quaternion.identity;
                changed = true;
            }

            Transform lookTarget = pitchTarget.Find(CCS_CharacterControllerConstants.CameraLookTargetObjectName);
            Transform legacyLookTarget = followAnchorTransform.Find(CCS_CharacterControllerConstants.CameraLookTargetObjectName);
            if (lookTarget == null && legacyLookTarget != null && legacyLookTarget.parent != pitchTarget)
            {
                legacyLookTarget.SetParent(pitchTarget, false);
                lookTarget = legacyLookTarget;
                changed = true;
            }

            if (lookTarget == null)
            {
                Transform legacyPivotLookTarget = playerRoot.Find("CameraPivot/CameraLookTarget");
                if (legacyPivotLookTarget != null)
                {
                    legacyPivotLookTarget.SetParent(pitchTarget, false);
                    lookTarget = legacyPivotLookTarget;
                    changed = true;
                }
            }

            if (lookTarget == null)
            {
                GameObject lookTargetObject = new GameObject(CCS_CharacterControllerConstants.CameraLookTargetObjectName);
                lookTarget = lookTargetObject.transform;
                lookTarget.SetParent(pitchTarget, false);
                changed = true;
            }

            Vector3 expectedLookTargetLocalPosition = CCS_CharacterControllerConstants.CameraLookTargetLocalPosition;

            if (lookTarget.localPosition != expectedLookTargetLocalPosition)
            {
                lookTarget.localPosition = expectedLookTargetLocalPosition;
                changed = true;
            }

            if (lookTarget.localRotation != Quaternion.identity)
            {
                lookTarget.localRotation = Quaternion.identity;
                changed = true;
            }

            changed |= EnsureFirstPersonCameraAnchors(pitchTarget);

            CCS_CharacterCameraProfileSet profileSet = AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfileSet>(
                CCS_CharacterControllerConstants.DefaultCameraProfileSetPath);
            CCS_CharacterCameraProfile lookProfile = profileSet != null && profileSet.DefaultProfile != null
                ? profileSet.DefaultProfile
                : AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfile>(
                    CCS_CharacterControllerConstants.ThirdPersonSurvivalCameraProfilePath);
            followAnchor.Configure(playerRoot, lookTarget, lookProfile);

            changed |= RemoveLegacyFirstPersonBodyVisibilityController(playerRoot);

            CCS_LocalFirstPersonHeadVisibility headVisibility =
                playerRoot.GetComponent<CCS_LocalFirstPersonHeadVisibility>();
            if (headVisibility == null)
            {
                headVisibility = playerRoot.gameObject.AddComponent<CCS_LocalFirstPersonHeadVisibility>();
                changed = true;
            }

            changed |= EnsureLocalFirstPersonHeadVisibility(playerRoot, headVisibility);

            changed |= EnsureFirstPersonBodyCameraAnchor(playerRoot, followAnchor, pitchTarget);
            changed |= EnsureWeaponCarryStateController(playerRoot);

            return changed;
        }

        private static bool EnsureWeaponCarryStateController(Transform playerRoot)
        {
            if (playerRoot == null)
            {
                return false;
            }

            bool changed = false;
            CCS_WeaponCarryStateController carryController =
                playerRoot.GetComponent<CCS_WeaponCarryStateController>();
            if (carryController == null)
            {
                carryController = playerRoot.gameObject.AddComponent<CCS_WeaponCarryStateController>();
                changed = true;
            }

            SerializedObject serializedCarry = new SerializedObject(carryController);
            changed |= SetObjectReference(
                serializedCarry,
                "inputProvider",
                playerRoot.GetComponent<CCS_CharacterInputActionProvider>());
            changed |= SetObjectReference(
                serializedCarry,
                "playerWeaponLoadout",
                playerRoot.GetComponent<CCS_PlayerWeaponLoadout>());
            serializedCarry.ApplyModifiedPropertiesWithoutUndo();

            CCS_CharacterAimLocomotionController aimLocomotion =
                playerRoot.GetComponent<CCS_CharacterAimLocomotionController>();
            if (aimLocomotion != null)
            {
                SerializedObject serializedAim = new SerializedObject(aimLocomotion);
                changed |= SetObjectReference(serializedAim, "weaponAimGateComponent", carryController);
                serializedAim.ApplyModifiedPropertiesWithoutUndo();
            }

            CCS_PlayerEquipmentVisualController equipmentVisual =
                playerRoot.GetComponent<CCS_PlayerEquipmentVisualController>();
            if (equipmentVisual != null)
            {
                SerializedObject serializedVisual = new SerializedObject(equipmentVisual);
                changed |= SetObjectReference(serializedVisual, "weaponCarryStateController", carryController);
                serializedVisual.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool EnsureFirstPersonBodyCameraAnchor(
            Transform playerRoot,
            CCS_CharacterCameraFollowAnchor followAnchor,
            Transform pitchTarget)
        {
            bool changed = false;
            CCS_FirstPersonBodyCameraAnchor headTracker =
                playerRoot.GetComponent<CCS_FirstPersonBodyCameraAnchor>();
            if (headTracker == null)
            {
                headTracker = playerRoot.gameObject.AddComponent<CCS_FirstPersonBodyCameraAnchor>();
                changed = true;
            }

            Animator animator = playerRoot.GetComponentInChildren<Animator>(true);
            Transform fpAnchor = pitchTarget != null
                ? pitchTarget.Find(CCS_CharacterControllerConstants.FirstPersonCameraAnchorObjectName)
                : null;

            SerializedObject serializedTracker = new SerializedObject(headTracker);
            changed |= SetObjectReference(serializedTracker, "characterAnimator", animator);
            changed |= SetObjectReference(serializedTracker, "cameraFollowAnchor", followAnchor);
            changed |= SetObjectReference(serializedTracker, "pitchTarget", pitchTarget);
            changed |= SetObjectReference(serializedTracker, "firstPersonCameraAnchor", fpAnchor);
            serializedTracker.ApplyModifiedPropertiesWithoutUndo();

            return changed;
        }

        private static bool RemoveLegacyFirstPersonBodyVisibilityController(Transform playerRoot)
        {
            if (playerRoot == null)
            {
                return false;
            }

            bool changed = false;
            MonoBehaviour[] behaviours = playerRoot.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                string typeName = behaviour.GetType().Name;
                if (typeName == "CCS_FirstPersonBodyVisibilityController")
                {
                    Object.DestroyImmediate(behaviour, true);
                    changed = true;
                }
            }

            return changed;
        }

        private static bool EnsureLocalFirstPersonHeadVisibility(
            Transform playerRoot,
            CCS_LocalFirstPersonHeadVisibility headVisibility)
        {
            if (playerRoot == null || headVisibility == null)
            {
                return false;
            }

            bool changed = false;
            Animator animator = playerRoot.GetComponentInChildren<Animator>(true);
            Transform visualRoot = playerRoot.Find("VisualRoot");
            if (visualRoot == null)
            {
                visualRoot = playerRoot;
            }

            changed |= CCS_FirstPersonHeadlessBodyMeshBuilder.EnsureHeadlessBodyMeshAsset();
            changed |= EnsureFirstPersonHeadlessBodyRenderer(visualRoot);

            SkinnedMeshRenderer combinedBodyRenderer =
                CCS_FirstPersonHeadlessBodyMeshBuilder.FindCombinedBodyRendererInHierarchy(visualRoot);
            SkinnedMeshRenderer headlessBodyRenderer = ResolveHeadlessBodyRenderer(visualRoot);
            Mesh headlessMesh = AssetDatabase.LoadAssetAtPath<Mesh>(
                CCS_CharacterControllerConstants.FirstPersonHeadlessBodyMeshAssetPath);

            SerializedObject serializedVisibility = new SerializedObject(headVisibility);
            changed |= SetObjectReference(serializedVisibility, "characterAnimator", animator);
            changed |= SetObjectReference(serializedVisibility, "visualRoot", visualRoot);
            changed |= SetObjectReference(serializedVisibility, "combinedBodyRenderer", combinedBodyRenderer);
            changed |= SetObjectReference(serializedVisibility, "headlessBodyRenderer", headlessBodyRenderer);
            changed |= SetObjectReference(serializedVisibility, "headlessBodyMesh", headlessMesh);
            changed |= SetStringValue(
                serializedVisibility,
                "headlessMeshAssetPath",
                CCS_CharacterControllerConstants.FirstPersonHeadlessBodyMeshAssetPath);
            serializedVisibility.ApplyModifiedPropertiesWithoutUndo();

            if (combinedBodyRenderer != null && headlessMesh != null)
            {
                CCS_FirstPersonHeadlessMeshStats stats = CCS_FirstPersonHeadlessBodyMeshBuilder.BakeHeadlessMesh(
                    combinedBodyRenderer,
                    CCS_FirstPersonHeadlessBodyMeshBuilder.DefaultHeadBoneWeightThreshold,
                    CCS_FirstPersonHeadlessBodyMeshBuilder.DefaultProtectedLimbWeightThreshold,
                    CCS_FirstPersonHeadlessBodyMeshBuilder.DefaultNeckTrimLocalHeight);
                headVisibility.ApplyHeadlessMeshStats(stats);
            }

            headVisibility.ResolveReferences();
            headVisibility.DiscoverHeadRenderers();
            EditorUtility.SetDirty(headVisibility);

            return changed;
        }

        private static bool EnsureFirstPersonHeadlessBodyRenderer(Transform visualRoot)
        {
            if (visualRoot == null)
            {
                return false;
            }

            SkinnedMeshRenderer sourceRenderer =
                CCS_FirstPersonHeadlessBodyMeshBuilder.FindCombinedBodyRendererInHierarchy(visualRoot);
            if (sourceRenderer == null)
            {
                return false;
            }

            bool changed = false;
            Transform headlessRoot = visualRoot.Find(CCS_CharacterControllerConstants.FirstPersonHeadlessBodyObjectName);
            if (headlessRoot == null)
            {
                GameObject headlessObject = new GameObject(CCS_CharacterControllerConstants.FirstPersonHeadlessBodyObjectName);
                headlessRoot = headlessObject.transform;
                headlessRoot.SetParent(visualRoot, false);
                changed = true;
            }

            SkinnedMeshRenderer headlessRenderer = headlessRoot.GetComponent<SkinnedMeshRenderer>();
            if (headlessRenderer == null)
            {
                headlessRenderer = headlessRoot.gameObject.AddComponent<SkinnedMeshRenderer>();
                changed = true;
            }

            Mesh headlessMesh = AssetDatabase.LoadAssetAtPath<Mesh>(
                CCS_CharacterControllerConstants.FirstPersonHeadlessBodyMeshAssetPath);

            if (headlessRenderer.sharedMesh != headlessMesh && headlessMesh != null)
            {
                headlessRenderer.sharedMesh = headlessMesh;
                changed = true;
            }

            if (headlessRenderer.rootBone != sourceRenderer.rootBone)
            {
                headlessRenderer.rootBone = sourceRenderer.rootBone;
                changed = true;
            }

            if (headlessRenderer.bones == null || headlessRenderer.bones.Length != sourceRenderer.bones.Length)
            {
                headlessRenderer.bones = sourceRenderer.bones;
                changed = true;
            }

            Material[] sourceMaterials = sourceRenderer.sharedMaterials;
            if (headlessRenderer.sharedMaterials == null
                || headlessRenderer.sharedMaterials.Length != sourceMaterials.Length)
            {
                headlessRenderer.sharedMaterials = sourceMaterials;
                changed = true;
            }

            if (headlessRenderer.updateWhenOffscreen != sourceRenderer.updateWhenOffscreen)
            {
                headlessRenderer.updateWhenOffscreen = sourceRenderer.updateWhenOffscreen;
                changed = true;
            }

            if (headlessRenderer.enabled)
            {
                headlessRenderer.enabled = false;
                changed = true;
            }

            if (headlessRoot.gameObject.layer != 0)
            {
                headlessRoot.gameObject.layer = 0;
                changed = true;
            }

            return changed;
        }

        private static SkinnedMeshRenderer ResolveHeadlessBodyRenderer(Transform visualRoot)
        {
            if (visualRoot == null)
            {
                return null;
            }

            Transform headlessRoot = visualRoot.Find(CCS_CharacterControllerConstants.FirstPersonHeadlessBodyObjectName);
            return headlessRoot != null ? headlessRoot.GetComponent<SkinnedMeshRenderer>() : null;
        }

        private static bool SetStringValue(SerializedObject serializedObject, string propertyName, string value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.stringValue == value)
            {
                return false;
            }

            property.stringValue = value;
            return true;
        }

        private static bool EnsureFirstPersonCameraAnchors(Transform pitchTarget)
        {
            if (pitchTarget == null)
            {
                return false;
            }

            bool changed = false;
            Transform fpAnchor = pitchTarget.Find(CCS_CharacterControllerConstants.FirstPersonCameraAnchorObjectName);
            if (fpAnchor == null)
            {
                GameObject anchorObject = new GameObject(CCS_CharacterControllerConstants.FirstPersonCameraAnchorObjectName);
                fpAnchor = anchorObject.transform;
                fpAnchor.SetParent(pitchTarget, false);
                changed = true;
            }

            Vector3 expectedAnchorPosition = new Vector3(
                0f,
                CCS_CharacterControllerConstants.FirstPersonVerticalEyeOffsetDefault,
                CCS_CharacterControllerConstants.FirstPersonForwardEyeOffsetDefault);
            if (fpAnchor.localPosition != expectedAnchorPosition)
            {
                fpAnchor.localPosition = expectedAnchorPosition;
                changed = true;
            }

            if (fpAnchor.localRotation != Quaternion.identity)
            {
                fpAnchor.localRotation = Quaternion.identity;
                changed = true;
            }

            Transform probe = pitchTarget.Find(CCS_CharacterControllerConstants.FirstPersonBodyLookProbeObjectName);
            if (probe == null)
            {
                GameObject probeObject = new GameObject(CCS_CharacterControllerConstants.FirstPersonBodyLookProbeObjectName);
                probe = probeObject.transform;
                probe.SetParent(pitchTarget, false);
                changed = true;
            }

            Vector3 expectedProbePosition = new Vector3(0f, 0.02f, 0.03f);
            if (probe.localPosition != expectedProbePosition)
            {
                probe.localPosition = expectedProbePosition;
                changed = true;
            }

            if (probe.localRotation != Quaternion.identity)
            {
                probe.localRotation = Quaternion.identity;
                changed = true;
            }

            Transform fpAimAnchor = pitchTarget.Find(CCS_CharacterControllerConstants.FirstPersonAimCameraAnchorObjectName);
            if (fpAimAnchor != null)
            {
                Object.DestroyImmediate(fpAimAnchor.gameObject, true);
                changed = true;
            }

            return changed;
        }

        private static bool EnsurePlayerCameraCollisionExclusion(GameObject playerRoot)
        {
            CCS_CharacterCameraLayerUtility.EnsurePlayerLayerAndTag();
            int playerLayer = LayerMask.NameToLayer(CCS_CharacterControllerConstants.PlayerLayerName);
            if (playerLayer < 0)
            {
                return false;
            }

            bool changed = false;
            if (!playerRoot.CompareTag(CCS_CharacterControllerConstants.PlayerTag))
            {
                playerRoot.tag = CCS_CharacterControllerConstants.PlayerTag;
                changed = true;
            }

            if (playerRoot.layer != playerLayer)
            {
                playerRoot.layer = playerLayer;
                changed = true;
            }

            Collider[] colliders = playerRoot.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider == null || collider.gameObject.layer == playerLayer)
                {
                    continue;
                }

                collider.gameObject.layer = playerLayer;
                changed = true;
            }

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
            Transform followAnchor = FindChildRecursive(
                playerRoot.transform,
                CCS_CharacterControllerConstants.CameraFollowAnchorObjectName);
            Transform pitchTarget = followAnchor != null
                ? followAnchor.Find(CCS_CharacterControllerConstants.CameraPitchTargetObjectName)
                : null;
            Transform cameraLookTarget = pitchTarget != null
                ? pitchTarget.Find(CCS_CharacterControllerConstants.CameraLookTargetObjectName)
                : null;

            if (pivotProperty != null && pitchTarget != null && pivotProperty.objectReferenceValue != pitchTarget)
            {
                pivotProperty.objectReferenceValue = pitchTarget;
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

        private static bool EnsurePlayerLocomotionComponents(GameObject prefabRoot)
        {
            CCS_CharacterInputActionProvider inputProvider =
                prefabRoot.GetComponentInChildren<CCS_CharacterInputActionProvider>(true);
            CCS_CharacterMotor motor = prefabRoot.GetComponent<CCS_CharacterMotor>();
            if (inputProvider == null || motor == null)
            {
                return false;
            }

            bool changed = false;

            CCS_CharacterMovementProfile movementProfile = AssetDatabase.LoadAssetAtPath<CCS_CharacterMovementProfile>(
                CCS_CharacterControllerConstants.DefaultMovementProfilePath);
            if (movementProfile != null && motor.MovementProfile != movementProfile)
            {
                motor.SetMovementProfile(movementProfile);
                changed = true;
            }

            SerializedObject serializedMotor = new SerializedObject(motor);
            SerializedProperty inputProperty = serializedMotor.FindProperty("inputProvider");
            if (inputProperty != null && inputProperty.objectReferenceValue != inputProvider)
            {
                inputProperty.objectReferenceValue = inputProvider;
                serializedMotor.ApplyModifiedPropertiesWithoutUndo();
                changed = true;
            }

            changed |= EnsureAimLocomotionController(prefabRoot, inputProvider, motor);
            return changed;
        }

        private static bool EnsureRevolverUpperBodyAnimator(GameObject prefabRoot)
        {
            Transform visualRoot = FindChildRecursive(prefabRoot.transform, "VisualRoot");
            if (visualRoot == null)
            {
                return false;
            }

            bool changed = false;
            CCS_RevolverUpperBodyAnimator upperBodyAnimator =
                visualRoot.GetComponent<CCS_RevolverUpperBodyAnimator>();
            if (upperBodyAnimator == null)
            {
                upperBodyAnimator = visualRoot.gameObject.AddComponent<CCS_RevolverUpperBodyAnimator>();
                changed = true;
            }

            Animator animator = visualRoot.GetComponentInChildren<Animator>(true);
            CCS_RevolverController revolverController =
                prefabRoot.GetComponentInChildren<CCS_RevolverController>(true);
            CCS_PlayerInteractionAnimator interactionAnimator =
                visualRoot.GetComponent<CCS_PlayerInteractionAnimator>();

            SerializedObject serializedAnimator = new SerializedObject(upperBodyAnimator);
            changed |= SetObjectReference(serializedAnimator, "animator", animator);
            changed |= SetObjectReference(serializedAnimator, "revolverAnimationStateComponent", revolverController);
            changed |= SetObjectReference(serializedAnimator, "interactionLockSourceComponent", interactionAnimator);
            serializedAnimator.ApplyModifiedPropertiesWithoutUndo();
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
                networkedRoot.GetComponentInChildren<CCS_ControllerTestNetworkPlayerBehaviour>(true);
            if (behaviour == null)
            {
                return false;
            }

            Material yellowMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                CCS_CharacterControllerMasterTestLayoutConstants.PlayerYellowMaterialPath);
            Material blackMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                CCS_CharacterControllerMasterTestLayoutConstants.PlayerBlackMaterialPath);
            Transform bodyVisual = FindChildRecursive(
                networkedRoot.transform,
                CCS_CharacterControllerMasterTestLayoutConstants.CapsuleVisualName);
            Transform glassesVisual = FindChildRecursive(
                networkedRoot.transform,
                CCS_CharacterControllerMasterTestLayoutConstants.GlassesVisualName);
            Transform followAnchor = FindChildRecursive(
                networkedRoot.transform,
                CCS_CharacterControllerConstants.CameraFollowAnchorObjectName);
            Transform cameraPivot = followAnchor != null
                ? followAnchor.Find(CCS_CharacterControllerConstants.CameraPitchTargetObjectName)
                : null;
            Transform cameraLookTarget = cameraPivot != null
                ? cameraPivot.Find(CCS_CharacterControllerConstants.CameraLookTargetObjectName)
                : null;

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
                networkedRoot.GetComponentInChildren<CCS_CharacterInputActionProvider>(true));
            changed |= SetObjectReference(
                serializedBehaviour,
                "motor",
                networkedRoot.GetComponent<CCS_CharacterMotor>());
            changed |= SetObjectReference(
                serializedBehaviour,
                "controllerService",
                networkedRoot.GetComponentInChildren<CCS_CharacterControllerService>(true));
            changed |= SetObjectReference(
                serializedBehaviour,
                "playerCameraController",
                networkedRoot.GetComponentInChildren<CCS_CharacterCameraController>(true));
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

        private static bool ForceSetBool(SerializedObject serializedObject, string propertyName, bool value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.boolValue == value)
            {
                return false;
            }

            property.boolValue = value;
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

        private static bool IsBrokenNpcVariantPrefab(string prefabPath)
        {
            if (!File.Exists(prefabPath))
            {
                return false;
            }

            string source = File.ReadAllText(prefabPath);
            return source.Contains("m_SourcePrefab:")
                && source.Contains("b4ca70f1e440f9a4c9d02f585dc1226f");
        }

        private static GameObject BuildTestNpcPrefabRoot()
        {
            GameObject root = new GameObject(CCS_CharacterControllerMasterTestLayoutConstants.NpcInstanceName);
            EnsureNpcCoreComponents(root);
            EnsureCapsuleBodyVisualForMaterial(
                root.transform,
                AssetDatabase.LoadAssetAtPath<Material>(
                    CCS_CharacterControllerMasterTestLayoutConstants.PlayerGreenMaterialPath));
            EnsureGlassesVisual(root.transform);
            EnsureNpcRunner(root);
            return root;
        }

        private static bool EnsureNpcCoreComponents(GameObject prefabRoot)
        {
            bool changed = false;

            UnityEngine.CharacterController characterController =
                prefabRoot.GetComponent<UnityEngine.CharacterController>();
            if (characterController == null)
            {
                characterController = prefabRoot.AddComponent<UnityEngine.CharacterController>();
                changed = true;
            }

            if (characterController.height != 2f
                || characterController.radius != 0.35f
                || characterController.center != new Vector3(0f, 1f, 0f)
                || characterController.slopeLimit != 45f
                || characterController.stepOffset != 0.3f
                || characterController.skinWidth != 0.08f)
            {
                characterController.height = 2f;
                characterController.radius = 0.35f;
                characterController.center = new Vector3(0f, 1f, 0f);
                characterController.slopeLimit = 45f;
                characterController.stepOffset = 0.3f;
                characterController.skinWidth = 0.08f;
                changed = true;
            }

            CCS_CharacterInputActionProvider inputProvider =
                prefabRoot.GetComponent<CCS_CharacterInputActionProvider>();
            if (inputProvider == null)
            {
                inputProvider = prefabRoot.AddComponent<CCS_CharacterInputActionProvider>();
                changed = true;
            }

            SerializedObject serializedInput = new SerializedObject(inputProvider);
            if (serializedInput.FindProperty("lockCursorOnEnable").boolValue)
            {
                serializedInput.FindProperty("lockCursorOnEnable").boolValue = false;
                serializedInput.ApplyModifiedPropertiesWithoutUndo();
                changed = true;
            }

            CCS_CharacterMotor motor = prefabRoot.GetComponent<CCS_CharacterMotor>();
            if (motor == null)
            {
                motor = prefabRoot.AddComponent<CCS_CharacterMotor>();
                changed = true;
            }

            CCS_CharacterMovementProfile movementProfile = AssetDatabase.LoadAssetAtPath<CCS_CharacterMovementProfile>(
                CCS_CharacterControllerConstants.DefaultMovementProfilePath);
            if (movementProfile != null && motor.MovementProfile != movementProfile)
            {
                motor.SetMovementProfile(movementProfile);
                changed = true;
            }

            SerializedObject serializedMotor = new SerializedObject(motor);
            SerializedProperty inputProperty = serializedMotor.FindProperty("inputProvider");
            if (inputProperty != null && inputProperty.objectReferenceValue != inputProvider)
            {
                inputProperty.objectReferenceValue = inputProvider;
                serializedMotor.ApplyModifiedPropertiesWithoutUndo();
                changed = true;
            }

            changed |= EnsureAimLocomotionController(prefabRoot, inputProvider, motor);

            return changed;
        }

        private static bool EnsureAimLocomotionController(
            GameObject prefabRoot,
            CCS_CharacterInputActionProvider inputProvider,
            CCS_CharacterMotor motor)
        {
            CCS_CharacterAimLocomotionController aimLocomotion =
                prefabRoot.GetComponentInChildren<CCS_CharacterAimLocomotionController>(true);
            bool changed = false;
            if (aimLocomotion == null)
            {
                aimLocomotion = prefabRoot.AddComponent<CCS_CharacterAimLocomotionController>();
                changed = true;
            }

            CCS_CharacterCameraFollowAnchor followAnchor =
                prefabRoot.GetComponentInChildren<CCS_CharacterCameraFollowAnchor>(true);

            SerializedObject serializedAimLocomotion = new SerializedObject(aimLocomotion);
            CCS_WeaponCarryStateController carryController =
                prefabRoot.GetComponentInChildren<CCS_WeaponCarryStateController>(true);
            bool aimChanged = SetObjectReference(serializedAimLocomotion, "inputProvider", inputProvider);
            aimChanged |= SetObjectReference(serializedAimLocomotion, "cameraFollowAnchor", followAnchor);
            aimChanged |= SetObjectReference(serializedAimLocomotion, "weaponAimGateComponent", carryController);
            if (aimChanged)
            {
                serializedAimLocomotion.ApplyModifiedPropertiesWithoutUndo();
                changed = true;
            }

            SerializedObject serializedMotor = new SerializedObject(motor);
            bool motorChanged = SetObjectReference(serializedMotor, "aimLocomotionController", aimLocomotion);
            motorChanged |= SetObjectReference(serializedMotor, "cameraFollowAnchor", followAnchor);
            if (motorChanged)
            {
                serializedMotor.ApplyModifiedPropertiesWithoutUndo();
                changed = true;
            }

            return changed;
        }

        private static bool EnsureCapsuleBodyVisualForMaterial(Transform playerRoot, Material bodyMaterial)
        {
            if (bodyMaterial == null)
            {
                return false;
            }

            Transform bodyVisual = playerRoot.Find(CCS_CharacterControllerMasterTestLayoutConstants.CapsuleVisualName);
            bool changed = false;
            if (bodyVisual == null)
            {
                GameObject bodyObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                bodyObject.name = CCS_CharacterControllerMasterTestLayoutConstants.CapsuleVisualName;
                bodyVisual = bodyObject.transform;
                bodyVisual.SetParent(playerRoot, false);
                changed = true;
            }

            Collider bodyCollider = bodyVisual.GetComponent<Collider>();
            if (bodyCollider != null)
            {
                Object.DestroyImmediate(bodyCollider);
                changed = true;
            }

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
            if (renderer != null && renderer.sharedMaterial != bodyMaterial)
            {
                renderer.sharedMaterial = bodyMaterial;
                changed = true;
            }

            return changed;
        }

        private static bool EnsureNpcRunner(GameObject prefabRoot)
        {
            CCS_ControllerTestNpcRunner runner = prefabRoot.GetComponent<CCS_ControllerTestNpcRunner>();
            bool changed = false;
            if (runner == null)
            {
                runner = prefabRoot.AddComponent<CCS_ControllerTestNpcRunner>();
                changed = true;
            }

            CCS_CharacterMotor motor = prefabRoot.GetComponent<CCS_CharacterMotor>();
            CCS_CharacterInputActionProvider inputProvider =
                prefabRoot.GetComponent<CCS_CharacterInputActionProvider>();
            SerializedObject serializedRunner = new SerializedObject(runner);
            changed |= SetObjectReference(serializedRunner, "motor", motor);
            changed |= SetObjectReference(serializedRunner, "inputProvider", inputProvider);

            SerializedProperty routeIds = serializedRunner.FindProperty("routeTestPointIds");
            string[] expectedRouteIds =
            {
                "ccs.test.character.spawn",
                "ccs.test.character.stairs.bottom",
                "ccs.test.character.stairs.top",
                "ccs.test.character.roof.center",
                "ccs.test.character.ramp.top",
                "ccs.test.character.ramp.bottom",
                "ccs.test.character.door.outside",
                "ccs.test.character.door.inside",
                "ccs.test.character.cover.inside",
                "ccs.test.character.loop.complete"
            };

            if (routeIds == null || routeIds.arraySize != expectedRouteIds.Length)
            {
                if (routeIds != null)
                {
                    routeIds.arraySize = expectedRouteIds.Length;
                    for (int i = 0; i < expectedRouteIds.Length; i++)
                    {
                        routeIds.GetArrayElementAtIndex(i).stringValue = expectedRouteIds[i];
                    }

                    changed = true;
                }
            }
            else
            {
                for (int i = 0; i < expectedRouteIds.Length; i++)
                {
                    if (routeIds.GetArrayElementAtIndex(i).stringValue != expectedRouteIds[i])
                    {
                        routeIds.GetArrayElementAtIndex(i).stringValue = expectedRouteIds[i];
                        changed = true;
                    }
                }
            }

            SerializedProperty loopRoute = serializedRunner.FindProperty("loopRoute");
            if (loopRoute != null && loopRoute.boolValue)
            {
                loopRoute.boolValue = false;
                changed = true;
            }

            if (changed)
            {
                serializedRunner.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool StripNpcPlayerOnlyComponents(GameObject prefabRoot)
        {
            bool changed = false;
            changed |= DestroyComponentIfPresent<CCS_CharacterCameraController>(prefabRoot);
            changed |= DestroyComponentIfPresent<CCS_CharacterControllerService>(prefabRoot);
            changed |= DestroyComponentByTypeName(prefabRoot, "CCS_CharacterControllerDebugHud");
            changed |= DestroyComponentIfPresent<CCS_TestPlayerOfflineBootstrap>(prefabRoot);
            changed |= DestroyComponentIfPresent<NetworkObject>(prefabRoot);
            changed |= DestroyComponentIfPresent<CCS_ClientOwnerNetworkTransform>(prefabRoot);
            changed |= DestroyComponentIfPresent<CCS_ControllerTestNetworkPlayerBehaviour>(prefabRoot);
            changed |= DestroyComponentIfPresent<CCS_NetworkPlayerNameplate>(prefabRoot);

            Transform cameraPivot = prefabRoot.transform.Find("CameraPivot");
            if (cameraPivot != null)
            {
                Object.DestroyImmediate(cameraPivot.gameObject);
                changed = true;
            }

            Transform followAnchor = prefabRoot.transform.Find("CameraFollowAnchor");
            if (followAnchor != null)
            {
                Object.DestroyImmediate(followAnchor.gameObject);
                changed = true;
            }

            Transform nameplateRoot = FindChildRecursive(
                prefabRoot.transform,
                CCS_CharacterControllerMasterTestLayoutConstants.NameplateRootObjectName);
            if (nameplateRoot != null)
            {
                Object.DestroyImmediate(nameplateRoot.gameObject);
                changed = true;
            }

            return changed;
        }

        private static bool RemoveCharacterControllerDebugHud(GameObject prefabRoot)
        {
            return DestroyComponentByTypeName(prefabRoot, "CCS_CharacterControllerDebugHud");
        }

        private static bool DestroyComponentByTypeName(GameObject target, string typeName)
        {
            MonoBehaviour[] behaviours = target.GetComponents<MonoBehaviour>();
            bool changed = false;
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null && behaviour.GetType().Name == typeName)
                {
                    Object.DestroyImmediate(behaviour, true);
                    changed = true;
                }
            }

            return changed;
        }

        private static bool DestroyComponentIfPresent<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            if (component == null)
            {
                return false;
            }

            Object.DestroyImmediate(component, true);
            return true;
        }

        #endregion
    }
}
