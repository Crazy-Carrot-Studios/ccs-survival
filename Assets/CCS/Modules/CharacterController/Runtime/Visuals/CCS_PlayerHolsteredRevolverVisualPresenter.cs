using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerHolsteredRevolverVisualPresenter
// CATEGORY: Modules / CharacterController / Runtime / Visuals
// PURPOSE: Visual-only holstered revolver presentation using socket registry and fit profiles.
// PLACEMENT: PF_CCS_CharacterController_Player_Networked / Model.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: v0.7.13 stripped baseline. No ownership, ammo, fire, or gameplay loadout wiring.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [DefaultExecutionOrder(205)]
    public sealed class CCS_PlayerHolsteredRevolverVisualPresenter : MonoBehaviour
    {
        [SerializeField] private CCS_EquipmentSocketRegistry equipmentSocketRegistry;
        [SerializeField] private CCS_WeaponAttachmentFitProfile rightHipHolsterFitProfile;
        [SerializeField] private GameObject revolverVisualOnlyPrefab;
        [SerializeField] private bool showHolsteredVisualOnStart = true;

        private Transform rightHolsterAttachmentRoot;
        private GameObject rightHolsteredVisualInstance;
        private bool previousEquipWeaponEnabled;
        private bool visualsVisible;

        private void Awake()
        {
            ResolveReferences();
            LoadDefaultProfilesIfMissing();
        }

        private void Start()
        {
            previousEquipWeaponEnabled = CCS_DiagnosticsEquipWeaponRegistry.EquipWeapon;
            if (showHolsteredVisualOnStart && previousEquipWeaponEnabled)
            {
                RefreshHolsteredVisuals();
            }
        }

        private void LateUpdate()
        {
            bool equipWeaponEnabled = CCS_DiagnosticsEquipWeaponRegistry.EquipWeapon;
            if (equipWeaponEnabled != previousEquipWeaponEnabled)
            {
                previousEquipWeaponEnabled = equipWeaponEnabled;
                if (equipWeaponEnabled)
                {
                    RefreshHolsteredVisuals();
                }
                else
                {
                    SetHolsteredVisualsVisible(false);
                }
            }
        }

        public void RefreshHolsteredVisuals()
        {
            if (!CCS_DiagnosticsEquipWeaponRegistry.EquipWeapon)
            {
                HideRightHipHolsteredVisual();
                return;
            }

            if (!visualsVisible && !showHolsteredVisualOnStart)
            {
                return;
            }

            ShowRightHipHolsteredVisual();
        }

        public void SetRightHipHolsteredVisible(bool visible)
        {
            if (!CCS_DiagnosticsEquipWeaponRegistry.EquipWeapon)
            {
                HideRightHipHolsteredVisual();
                return;
            }

            if (visible)
            {
                ShowRightHipHolsteredVisual();
            }
            else
            {
                HideRightHipHolsteredVisual();
            }
        }

        public void SetHolsteredVisualsVisible(bool visible)
        {
            visualsVisible = visible;
            if (!visible || !CCS_DiagnosticsEquipWeaponRegistry.EquipWeapon)
            {
                HideRightHipHolsteredVisual();
                return;
            }

            RefreshHolsteredVisuals();
        }

        private void ResolveReferences()
        {
            if (equipmentSocketRegistry == null)
            {
                equipmentSocketRegistry = GetComponentInParent<CCS_EquipmentSocketRegistry>();
            }
        }

        private void LoadDefaultProfilesIfMissing()
        {
#if UNITY_EDITOR
            if (rightHipHolsterFitProfile == null)
            {
                rightHipHolsterFitProfile = UnityEditor.AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(
                    CCS_RevolverFitProfilePaths.RightHipHolsterFitPath);
            }

            if (revolverVisualOnlyPrefab == null)
            {
                revolverVisualOnlyPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                    CCS_EquipmentConstants.RevolverM1879VisualOnlyPrefabPath);
            }
#endif
        }

        private void ShowRightHipHolsteredVisual()
        {
            if (!TryShowHolsteredVisual(
                    CCS_EquipmentConstants.HolsterSocketRightHipId,
                    rightHipHolsterFitProfile,
                    ref rightHolsterAttachmentRoot,
                    ref rightHolsteredVisualInstance,
                    "_Right"))
            {
                HideRightHipHolsteredVisual();
            }
        }

        private bool TryShowHolsteredVisual(
            string socketId,
            CCS_WeaponAttachmentFitProfile profile,
            ref Transform attachmentRoot,
            ref GameObject visualInstance,
            string attachmentSuffix)
        {
            if (equipmentSocketRegistry == null
                || profile == null
                || revolverVisualOnlyPrefab == null
                || !equipmentSocketRegistry.TryGetSocket(socketId, out Transform socketTransform))
            {
                return false;
            }

            if (!TryGetSocketDefinitionBaseline(socketId, out Vector3 definitionPosition, out Vector3 definitionEuler, out Vector3 definitionScale))
            {
                return false;
            }

            string attachmentRootName = CCS_EquipmentConstants.RuntimeHolsterAttachmentRootObjectName + attachmentSuffix;
            attachmentRoot = EnsureAttachmentRoot(socketTransform, attachmentRootName);
            CCS_WeaponAttachmentFitProfileApplicator.ApplyProfileToAttachmentRoot(
                socketTransform,
                attachmentRoot,
                profile,
                definitionPosition,
                definitionEuler,
                definitionScale);

            visualInstance = EnsureVisualInstance(
                attachmentRoot,
                CCS_EquipmentConstants.RuntimeHolsteredVisualObjectName + attachmentSuffix);
            if (visualInstance != null)
            {
                visualInstance.SetActive(true);
            }

            return visualInstance != null;
        }

        private void HideRightHipHolsteredVisual()
        {
            HideHolsteredVisual(ref rightHolsterAttachmentRoot, ref rightHolsteredVisualInstance);
        }

        private static void HideHolsteredVisual(ref Transform attachmentRoot, ref GameObject visualInstance)
        {
            if (visualInstance != null)
            {
                visualInstance.SetActive(false);
            }

            if (attachmentRoot != null)
            {
                CCS_WeaponAttachmentFitProfileApplicator.ResetAttachmentRoot(attachmentRoot);
            }
        }

        private static Transform EnsureAttachmentRoot(Transform socketTransform, string attachmentRootName)
        {
            Transform existing = socketTransform.Find(attachmentRootName);
            if (existing != null)
            {
                return existing;
            }

            GameObject rootObject = new GameObject(attachmentRootName);
            Transform created = rootObject.transform;
            created.SetParent(socketTransform, false);
            return created;
        }

        private bool TryGetSocketDefinitionBaseline(
            string socketId,
            out Vector3 position,
            out Vector3 euler,
            out Vector3 scale)
        {
            position = Vector3.zero;
            euler = Vector3.zero;
            scale = Vector3.one;

            CCS_EquipmentSocketProfile socketProfile = equipmentSocketRegistry?.EquipmentSocketProfile;
            if (socketProfile == null)
            {
                return false;
            }

            IReadOnlyList<CCS_EquipmentSocketDefinition> definitions = socketProfile.SocketDefinitions;
            for (int i = 0; i < definitions.Count; i++)
            {
                CCS_EquipmentSocketDefinition definition = definitions[i];
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

        private GameObject EnsureVisualInstance(Transform attachmentRoot, string visualInstanceName)
        {
            Transform existing = attachmentRoot.Find(visualInstanceName);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject instance = Instantiate(revolverVisualOnlyPrefab, attachmentRoot);
            instance.name = visualInstanceName;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            return instance;
        }
    }
}
