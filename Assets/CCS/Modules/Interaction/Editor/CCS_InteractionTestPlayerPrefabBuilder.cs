using CCS.Modules.CharacterController;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_InteractionTestPlayerPrefabBuilder
// CATEGORY: Modules / Interaction / Editor
// PURPOSE: Wires interaction scanner, prompt presenter, and animator on the test player prefab.
// PLACEMENT: Editor utility invoked from Interaction validation and master test setup.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Does not modify CharacterController movement, camera, or input action bindings.
// =============================================================================

namespace CCS.Modules.Interaction.Editor
{
    public static class CCS_InteractionTestPlayerPrefabBuilder
    {
        #region Public Methods

        public static bool EnsureTestPlayerDetectionWiring()
        {
            bool changed = EnsureTestPlayerInteractionScanner(CCS_InteractionConstants.NetworkedTestPlayerPrefabPath);
            changed |= EnsureDetectionScannerSettings(CCS_InteractionConstants.NetworkedTestPlayerPrefabPath);
            changed |= CCS_InteractionPromptHudPrefabBuilder.EnsureTestPlayerInteractionPromptHud(
                CCS_InteractionConstants.NetworkedTestPlayerPrefabPath);
            changed |= EnsureTestPlayerInteractionAnimator();
            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return changed;
        }

        public static bool EnsureTestPlayerInteractionWiring()
        {
            bool changed = EnsureTestPlayerInteractionWiring(CCS_InteractionConstants.NetworkedTestPlayerPrefabPath);
            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return changed;
        }

        public static bool EnsureTestPlayerInteractionScanner()
        {
            return EnsureTestPlayerInteractionScanner(CCS_InteractionConstants.NetworkedTestPlayerPrefabPath);
        }

        public static bool EnsureTestPlayerInteractionScanner(string prefabPath)
        {
            CCS_InteractionAssetBuilder.EnsureScannerProfileAsset();

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError("[Interaction Prefab Builder] Missing prefab: " + prefabPath);
                return false;
            }

            CCS_InteractionScannerProfile scannerProfile = AssetDatabase.LoadAssetAtPath<CCS_InteractionScannerProfile>(
                CCS_InteractionConstants.ScannerProfilePath);
            if (scannerProfile == null)
            {
                Debug.LogError(
                    "[Interaction Prefab Builder] Missing scanner profile: "
                    + CCS_InteractionConstants.ScannerProfilePath);
                PrefabUtility.UnloadPrefabContents(prefabRoot);
                return false;
            }

            bool changed = EnsureScanner(prefabRoot, scannerProfile);
            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return changed;
        }

        public static bool EnsureTestPlayerInteractionAnimator()
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(CCS_InteractionConstants.NetworkedTestPlayerPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError("[Interaction Prefab Builder] Missing prefab for animator wiring.");
                return false;
            }

            bool changed = EnsureInteractionAnimator(prefabRoot);
            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, CCS_InteractionConstants.NetworkedTestPlayerPrefabPath);
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return changed;
        }

        #endregion

        #region Private Methods

        private static bool EnsureTestPlayerInteractionWiring(string prefabPath)
        {
            bool changed = EnsureTestPlayerInteractionScanner(prefabPath);
            changed |= CCS_InteractionPromptHudPrefabBuilder.EnsureTestPlayerInteractionPromptHud(prefabPath);
            changed |= EnsureTestPlayerInteractionAnimator();
            return changed;
        }

        private static bool EnsureDetectionScannerSettings(string prefabPath)
        {
            LayerMask interactableLayerMask = CCS_InteractionLayerUtility.GetInteractableLayerMask();
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                return false;
            }

            CCS_NetworkInteractionScanner scanner = prefabRoot.GetComponent<CCS_NetworkInteractionScanner>();
            if (scanner == null)
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
                return false;
            }

            SerializedObject serializedScanner = new SerializedObject(scanner);
            bool changed = false;
            changed |= SetFloat(serializedScanner, "broadDetectionRadius", CCS_InteractionConstants.DefaultBroadDetectionRadius);
            changed |= EnsureScannerVolumeSerialized(serializedScanner);
            changed |= SetFloat(serializedScanner, "lineOfSightSphereRadius", CCS_InteractionConstants.DefaultLineOfSightSphereRadius);
            changed |= SetFloat(serializedScanner, "lineOfSightDistancePadding", CCS_InteractionConstants.DefaultLineOfSightDistancePadding);
            changed |= SetLayerMask(serializedScanner, "interactableLayerMask", interactableLayerMask);

            if (!scanner.enabled)
            {
                scanner.enabled = true;
                changed = true;
            }

            if (changed)
            {
                serializedScanner.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return changed;
        }

        private static bool EnsureScanner(GameObject prefabRoot, CCS_InteractionScannerProfile scannerProfile)
        {
            CCS_NetworkInteractionScanner scanner = prefabRoot.GetComponent<CCS_NetworkInteractionScanner>();
            bool addedScanner = false;
            if (scanner == null)
            {
                scanner = prefabRoot.AddComponent<CCS_NetworkInteractionScanner>();
                addedScanner = true;
            }

            SerializedObject serializedScanner = new SerializedObject(scanner);
            bool changed = addedScanner;
            SerializedProperty profileProperty = serializedScanner.FindProperty("scannerProfile");
            if (profileProperty != null && profileProperty.objectReferenceValue != scannerProfile)
            {
                profileProperty.objectReferenceValue = scannerProfile;
                changed = true;
            }

            SerializedProperty originProperty = serializedScanner.FindProperty("scanOriginTransform");
            (Transform scanOrigin, bool scanOriginChanged) = EnsureInteractionScanOrigin(prefabRoot.transform);
            changed |= scanOriginChanged;
            if (originProperty != null && originProperty.objectReferenceValue != scanOrigin)
            {
                originProperty.objectReferenceValue = scanOrigin;
                changed = true;
            }

            changed |= SetFloat(serializedScanner, "broadDetectionRadius", CCS_InteractionConstants.DefaultBroadDetectionRadius);
            changed |= EnsureScannerVolumeSerialized(serializedScanner);
            changed |= SetFloat(serializedScanner, "lineOfSightSphereRadius", CCS_InteractionConstants.DefaultLineOfSightSphereRadius);
            changed |= SetFloat(serializedScanner, "lineOfSightDistancePadding", CCS_InteractionConstants.DefaultLineOfSightDistancePadding);
            changed |= SetLayerMask(
                serializedScanner,
                "interactableLayerMask",
                CCS_InteractionLayerUtility.GetInteractableLayerMask());

            if (changed)
            {
                serializedScanner.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool EnsureInteractionAnimator(GameObject prefabRoot)
        {
            Transform visualRoot = prefabRoot.transform.Find("VisualRoot");
            if (visualRoot == null)
            {
                Debug.LogWarning("[Interaction Prefab Builder] VisualRoot was not found on test player prefab.");
                return false;
            }

            CCS_PlayerInteractionAnimator animatorBridge = visualRoot.GetComponent<CCS_PlayerInteractionAnimator>();
            if (animatorBridge == null)
            {
                animatorBridge = visualRoot.gameObject.AddComponent<CCS_PlayerInteractionAnimator>();
            }

            CCS_NetworkInteractionScanner scanner = prefabRoot.GetComponent<CCS_NetworkInteractionScanner>();
            Animator animator = visualRoot.GetComponentInChildren<Animator>(true);

            SerializedObject serializedAnimator = new SerializedObject(animatorBridge);
            bool changed = false;
            changed |= SetObjectReference(serializedAnimator, "animator", animator);
            changed |= SetObjectReference(serializedAnimator, "interactionSourceComponent", scanner);
            changed |= SetBool(serializedAnimator, "enableManualInteractionAnimationTest", false);

            if (scanner != null)
            {
                SerializedObject serializedScanner = new SerializedObject(scanner);
                bool scannerChanged = SetObjectReference(
                    serializedScanner,
                    "interactionBusySourceComponent",
                    animatorBridge);
                scannerChanged |= SetObjectReference(
                    serializedScanner,
                    "interactionLockControllerComponent",
                    animatorBridge);
                if (scannerChanged)
                {
                    serializedScanner.ApplyModifiedPropertiesWithoutUndo();
                    changed = true;
                }
            }

            if (changed)
            {
                serializedAnimator.ApplyModifiedPropertiesWithoutUndo();
            }

            changed |= EnsureMotorInteractionLockSource(prefabRoot, animatorBridge);

            return changed;
        }

        private static (Transform scanOrigin, bool changed) EnsureInteractionScanOrigin(Transform prefabRoot)
        {
            bool changed = false;
            Transform existingOrigin = prefabRoot.Find(CCS_InteractionConstants.InteractionScanOriginObjectName);
            if (existingOrigin == null)
            {
                GameObject scanOriginObject = new GameObject(CCS_InteractionConstants.InteractionScanOriginObjectName);
                scanOriginObject.transform.SetParent(prefabRoot, false);
                existingOrigin = scanOriginObject.transform;
                changed = true;
            }

            Vector3 chestLocalPosition = new Vector3(
                0f,
                CCS_InteractionConstants.InteractionScanOriginLocalHeight,
                0f);
            if (existingOrigin.localPosition != chestLocalPosition)
            {
                existingOrigin.localPosition = chestLocalPosition;
                changed = true;
            }

            if (existingOrigin.localRotation != Quaternion.identity)
            {
                existingOrigin.localRotation = Quaternion.identity;
                changed = true;
            }

            CCS_InteractionScanOriginGizmo gizmo = existingOrigin.GetComponent<CCS_InteractionScanOriginGizmo>();
            if (gizmo == null)
            {
                gizmo = existingOrigin.gameObject.AddComponent<CCS_InteractionScanOriginGizmo>();
                changed = true;
            }

            gizmo.Configure(
                prefabRoot,
                CCS_InteractionConstants.DefaultInteractionHalfWidth,
                CCS_InteractionConstants.DefaultInteractionHalfHeight,
                CCS_InteractionConstants.DefaultStrictPickupDistance,
                CCS_InteractionConstants.DefaultWalkThroughDoorStrictRange);

            return (existingOrigin, changed);
        }

        private static bool EnsureMotorInteractionLockSource(GameObject prefabRoot, CCS_PlayerInteractionAnimator animatorBridge)
        {
            CCS_CharacterMotor motor = prefabRoot.GetComponent<CCS_CharacterMotor>();
            if (motor == null || animatorBridge == null)
            {
                return false;
            }

            SerializedObject serializedMotor = new SerializedObject(motor);
            bool changed = SetObjectReference(
                serializedMotor,
                "interactionLockSourceComponent",
                animatorBridge);
            if (changed)
            {
                serializedMotor.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool SetObjectReference(SerializedObject serializedObject, string propertyName, Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == value)
            {
                return false;
            }

            property.objectReferenceValue = value;
            return true;
        }

        private static bool SetFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                return false;
            }

            if (Mathf.Approximately(property.floatValue, value))
            {
                return false;
            }

            property.floatValue = value;
            return true;
        }

        private static bool EnsureScannerVolumeSerialized(SerializedObject serializedScanner)
        {
            bool hasVolumeFields = serializedScanner.FindProperty("interactionHalfWidth") != null
                && serializedScanner.FindProperty("interactionHalfHeight") != null;
            if (!hasVolumeFields)
            {
                return false;
            }

            bool changed = false;
            changed |= SetFloat(serializedScanner, "interactionHalfWidth", CCS_InteractionConstants.DefaultInteractionHalfWidth);
            changed |= SetFloat(serializedScanner, "interactionHalfHeight", CCS_InteractionConstants.DefaultInteractionHalfHeight);

            if (serializedScanner.FindProperty("pickupMaxCameraAngle") != null)
            {
                changed = true;
            }

            return changed;
        }

        private static bool SetBool(SerializedObject serializedObject, string propertyName, bool value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.boolValue == value)
            {
                return false;
            }

            property.boolValue = value;
            return true;
        }

        private static bool SetLayerMask(SerializedObject serializedObject, string propertyName, LayerMask value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.intValue == value.value)
            {
                return false;
            }

            property.intValue = value.value;
            return true;
        }

        #endregion
    }
}
