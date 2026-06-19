using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_InteractionTestPlayerPrefabBuilder
// CATEGORY: Modules / Interaction / Editor
// PURPOSE: Wires interaction scanner onto the canonical networked test player prefab.
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

        public static bool EnsureTestPlayerInteractionScanner()
        {
            bool changed = EnsureTestPlayerInteractionScanner(CCS_InteractionConstants.NetworkedTestPlayerPrefabPath);
            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return changed;
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

        #endregion

        #region Private Methods

        private static bool EnsureScanner(GameObject prefabRoot, CCS_InteractionScannerProfile scannerProfile)
        {
            CCS_NetworkInteractionScanner scanner = prefabRoot.GetComponent<CCS_NetworkInteractionScanner>();
            if (scanner == null)
            {
                scanner = prefabRoot.AddComponent<CCS_NetworkInteractionScanner>();
            }

            SerializedObject serializedScanner = new SerializedObject(scanner);
            bool changed = scanner == null;
            SerializedProperty profileProperty = serializedScanner.FindProperty("scannerProfile");
            if (profileProperty != null && profileProperty.objectReferenceValue != scannerProfile)
            {
                profileProperty.objectReferenceValue = scannerProfile;
                changed = true;
            }

            SerializedProperty originProperty = serializedScanner.FindProperty("scanOriginTransform");
            if (originProperty != null && originProperty.objectReferenceValue != prefabRoot.transform)
            {
                originProperty.objectReferenceValue = prefabRoot.transform;
                changed = true;
            }

            if (changed)
            {
                serializedScanner.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        #endregion
    }
}
