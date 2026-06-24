using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioTestAttachmentUtility
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Editor-only test holster/equipped fit attachments from saved profiles.
// PLACEMENT: Used by Equipment Fit Studio bottom action bar.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Uses the same attachment-root application path as runtime visual controller.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public static class CCS_EquipmentFitStudioTestAttachmentUtility
    {
        #region Public Methods

        public static bool HasHolsterTestAttachment(GameObject playerRoot)
        {
            return FindExisting(CCS_EquipmentConstants.EditorTestHolsterFitObjectName, playerRoot) != null;
        }

        public static bool HasEquippedTestAttachment(GameObject playerRoot)
        {
            return FindExisting(CCS_EquipmentConstants.EditorTestEquippedFitObjectName, playerRoot) != null;
        }

        public static bool HasAnyTestAttachment(GameObject playerRoot)
        {
            return HasHolsterTestAttachment(playerRoot) || HasEquippedTestAttachment(playerRoot);
        }

        public static bool TestSavedHolsterFit(GameObject playerRoot, GameObject previewSourcePrefab)
        {
            return TestSavedFit(
                playerRoot,
                previewSourcePrefab,
                CCS_EquipmentConstants.HolsterSocketRightHipId,
                CCS_EquipmentConstants.EditorTestHolsterFitObjectName,
                CCS_EquipmentConstants.RuntimeHolsterAttachmentRootObjectName,
                CCS_EquipmentConstants.RevolverM1879RightHipHolsterFitPath);
        }

        public static bool TestSavedEquippedFit(GameObject playerRoot, GameObject previewSourcePrefab)
        {
            return TestSavedFit(
                playerRoot,
                previewSourcePrefab,
                CCS_EquipmentConstants.HandSocketRightId,
                CCS_EquipmentConstants.EditorTestEquippedFitObjectName,
                CCS_EquipmentConstants.RuntimeEquippedAttachmentRootObjectName,
                CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath);
        }

        public static bool ClearTestAttachments(GameObject playerRoot)
        {
            bool changed = false;
            changed |= DestroyExisting(CCS_EquipmentConstants.EditorTestHolsterFitObjectName, playerRoot);
            changed |= DestroyExisting(CCS_EquipmentConstants.EditorTestEquippedFitObjectName, playerRoot);
            changed |= DestroyExisting(CCS_EquipmentConstants.RuntimeHolsterAttachmentRootObjectName, playerRoot);
            changed |= DestroyExisting(CCS_EquipmentConstants.RuntimeEquippedAttachmentRootObjectName, playerRoot);
            changed |= CCS_EquipmentFitStudioCleanupUtility.CleanupEditorTemporaryObjectsInOpenScenes();
            return changed;
        }

        public static bool TryGetRuntimeAttachmentRoot(
            GameObject playerRoot,
            string socketId,
            string attachmentRootName,
            out Transform attachmentRoot)
        {
            attachmentRoot = null;
            if (playerRoot == null)
            {
                return false;
            }

            CCS_EquipmentSocketRegistry registry = playerRoot.GetComponent<CCS_EquipmentSocketRegistry>();
            if (registry == null || !registry.TryGetSocket(socketId, out Transform socketTransform))
            {
                return false;
            }

            attachmentRoot = socketTransform.Find(attachmentRootName);
            return attachmentRoot != null;
        }

        #endregion

        #region Private Methods

        private static bool TestSavedFit(
            GameObject playerRoot,
            GameObject previewSourcePrefab,
            string socketId,
            string objectName,
            string attachmentRootName,
            string fitProfilePath)
        {
            if (playerRoot == null)
            {
                return false;
            }

            CCS_EquipmentSocketRegistry registry = playerRoot.GetComponent<CCS_EquipmentSocketRegistry>();
            if (registry == null || !registry.TryGetSocket(socketId, out Transform socketTransform))
            {
                return false;
            }

            CCS_WeaponAttachmentFitProfile profile =
                CCS_EquipmentFitProfilePersistenceUtility.LoadProfileFromDisk(fitProfilePath);
            if (profile == null)
            {
                return false;
            }

            if (!TryGetSocketDefinition(registry, socketId, out Vector3 defPos, out Vector3 defEuler, out Vector3 defScale))
            {
                return false;
            }

            Transform attachmentRoot = EnsureEditorAttachmentRoot(socketTransform, attachmentRootName);
            CCS_WeaponAttachmentFitProfileApplicator.ApplyProfileToAttachmentRoot(
                socketTransform,
                attachmentRoot,
                profile,
                defPos,
                defEuler,
                defScale);

            DestroyExisting(objectName, playerRoot);
            GameObject testRoot = CCS_EquipmentFitStudioVisualSourceUtility.SpawnEditorVisualUnderSocket(
                attachmentRoot,
                previewSourcePrefab,
                objectName,
                hideInHierarchy: false);
            if (testRoot != null)
            {
                testRoot.hideFlags = HideFlags.DontSave;
                AddTestFitLabel(testRoot.transform);
            }

            return testRoot != null;
        }

        private static Transform EnsureEditorAttachmentRoot(Transform socketTransform, string attachmentRootName)
        {
            Transform existing = socketTransform.Find(attachmentRootName);
            if (existing != null)
            {
                return existing;
            }

            GameObject rootObject = new GameObject(attachmentRootName);
            rootObject.hideFlags = HideFlags.DontSave;
            existing = rootObject.transform;
            existing.SetParent(socketTransform, false);
            return existing;
        }

        public static bool TryGetSocketDefinition(
            CCS_EquipmentSocketRegistry registry,
            string socketId,
            out Vector3 position,
            out Vector3 euler,
            out Vector3 scale)
        {
            return TryGetSocketDefinitionInternal(registry, socketId, out position, out euler, out scale);
        }

        private static bool TryGetSocketDefinitionInternal(
            CCS_EquipmentSocketRegistry registry,
            string socketId,
            out Vector3 position,
            out Vector3 euler,
            out Vector3 scale)
        {
            position = Vector3.zero;
            euler = Vector3.zero;
            scale = Vector3.one;

            CCS_EquipmentSocketProfile socketProfile = registry.EquipmentSocketProfile;
            if (socketProfile == null)
            {
                return false;
            }

            for (int i = 0; i < socketProfile.SocketDefinitions.Count; i++)
            {
                CCS_EquipmentSocketDefinition definition = socketProfile.SocketDefinitions[i];
                if (definition != null && definition.SocketId == socketId)
                {
                    position = definition.LocalPosition;
                    euler = definition.LocalEulerAngles;
                    scale = definition.LocalScale;
                    return true;
                }
            }

            return false;
        }

        private static void AddTestFitLabel(Transform testRoot)
        {
            GameObject labelObject = new GameObject("EDITOR_TEST_FIT_LABEL_DO_NOT_SAVE");
            labelObject.hideFlags = HideFlags.DontSave;
            labelObject.transform.SetParent(testRoot, false);
            labelObject.transform.localPosition = new Vector3(0f, 0.08f, 0f);
        }

        private static GameObject FindExisting(string objectName, GameObject playerRoot)
        {
            if (playerRoot == null)
            {
                return null;
            }

            Transform[] transforms = playerRoot.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform candidate = transforms[i];
                if (candidate != null && candidate.name == objectName)
                {
                    return candidate.gameObject;
                }
            }

            return null;
        }

        private static bool DestroyExisting(string objectName, GameObject playerRoot)
        {
            GameObject existing = FindExisting(objectName, playerRoot);
            if (existing == null)
            {
                return false;
            }

            Object.DestroyImmediate(existing);
            return true;
        }

        #endregion
    }
}
