using System.Collections.Generic;

using UnityEngine;



// =============================================================================

// SCRIPT: CCS_PlayerEquippedRevolverAimVisualPresenter

// SUMMARY: Visual-only right/left-hand equipped revolvers during aim presentation.

//          Parents PF_CCS_RevolverM1879_VisualOnly to hand sockets via fit profiles.

// REQUIRED: CCS_EquipmentSocketRegistry on player root, CCS_RevolverAimPresentationGate,

//           CCS_PlayerHolsteredRevolverVisualPresenter on Model (optional holster hide).

// PLACEMENT: PF_CCS_CharacterController_Player_Networked / Model

// AUTHOR: James Schilz

// CREATED: 2026-06-25

// =============================================================================



namespace CCS.Modules.CharacterController

{

    [DefaultExecutionOrder(215)]

    public sealed class CCS_PlayerEquippedRevolverAimVisualPresenter : MonoBehaviour

    {

        [SerializeField] private CCS_EquipmentSocketRegistry equipmentSocketRegistry;

        [SerializeField] private CCS_WeaponAttachmentFitProfile rightHandEquippedFitProfile;

        [SerializeField] private CCS_WeaponAttachmentFitProfile leftHandEquippedFitProfile;

        [SerializeField] private GameObject revolverVisualOnlyPrefab;

        [SerializeField] private Component aimPresentationInputComponent;

        [SerializeField] private CCS_PlayerHolsteredRevolverVisualPresenter holsteredRevolverVisualPresenter;



        private CCS_IRevolverAimPresentationInput aimPresentationInput;

        private Transform rightEquippedAttachmentRoot;

        private Transform leftEquippedAttachmentRoot;

        private GameObject rightEquippedVisualInstance;

        private GameObject leftEquippedVisualInstance;

        private bool previousAimPresentationRequested;



        private void Awake()

        {

            ResolveReferences();

            LoadDefaultProfilesIfMissing();

        }



        private void LateUpdate()

        {

            if (!CCS_DiagnosticsEquipWeaponRegistry.EquipWeapon)

            {

                if (HasAnyEquippedVisualActive())

                {

                    HideEquippedVisuals();

                    holsteredRevolverVisualPresenter?.SetRightHipHolsteredVisible(false);

                }



                previousAimPresentationRequested = false;

                return;

            }



            if (aimPresentationInput == null)

            {

                return;

            }



            bool aimRequested = aimPresentationInput.IsAimPresentationActive;

            if (aimRequested == previousAimPresentationRequested)

            {

                return;

            }



            previousAimPresentationRequested = aimRequested;

            if (aimRequested)

            {

                holsteredRevolverVisualPresenter?.SetRightHipHolsteredVisible(false);

                ShowEquippedVisuals();

            }

            else

            {

                HideEquippedVisuals();

                holsteredRevolverVisualPresenter?.SetRightHipHolsteredVisible(true);

            }

        }



        private void OnDisable()

        {

            HideEquippedVisuals();

            holsteredRevolverVisualPresenter?.SetRightHipHolsteredVisible(true);

            previousAimPresentationRequested = false;

        }



        private void ResolveReferences()

        {

            if (equipmentSocketRegistry == null)

            {

                equipmentSocketRegistry = GetComponentInParent<CCS_EquipmentSocketRegistry>();

            }



            if (holsteredRevolverVisualPresenter == null)

            {

                holsteredRevolverVisualPresenter = GetComponent<CCS_PlayerHolsteredRevolverVisualPresenter>();

            }



            if (aimPresentationInputComponent is CCS_IRevolverAimPresentationInput fromComponent)

            {

                aimPresentationInput = fromComponent;

            }

            else if (aimPresentationInput == null)

            {

                aimPresentationInput = GetComponentInParent<CCS_IRevolverAimPresentationInput>();

            }

        }



        private void LoadDefaultProfilesIfMissing()

        {

#if UNITY_EDITOR

            if (rightHandEquippedFitProfile == null)

            {

                rightHandEquippedFitProfile = UnityEditor.AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(

                    CCS_RevolverFitProfilePaths.RightHandEquippedFitPath);

            }



            if (leftHandEquippedFitProfile == null)

            {

                leftHandEquippedFitProfile = UnityEditor.AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(

                    CCS_RevolverFitProfilePaths.LeftHandEquippedFitPath);

            }



            if (revolverVisualOnlyPrefab == null)

            {

                revolverVisualOnlyPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(

                    CCS_EquipmentConstants.RevolverM1879VisualOnlyPrefabPath);

            }

#endif

        }



        private void ShowEquippedVisuals()

        {

            ShowHandEquippedVisual(

                CCS_EquipmentConstants.HandSocketRightId,

                rightHandEquippedFitProfile,

                CCS_EquipmentConstants.RightHandRevolverAttachmentOffsetObjectName,

                ref rightEquippedAttachmentRoot,

                ref rightEquippedVisualInstance,

                CCS_EquipmentConstants.RuntimeEquippedVisualObjectName);



            ShowHandEquippedVisual(

                CCS_EquipmentConstants.HandSocketLeftId,

                leftHandEquippedFitProfile,

                CCS_EquipmentConstants.LeftHandRevolverAttachmentOffsetObjectName,

                ref leftEquippedAttachmentRoot,

                ref leftEquippedVisualInstance,

                CCS_EquipmentConstants.LeftHandEquippedVisualObjectName);

        }



        private void HideEquippedVisuals()

        {

            HideHandEquippedVisual(ref rightEquippedAttachmentRoot, ref rightEquippedVisualInstance);

            HideHandEquippedVisual(ref leftEquippedAttachmentRoot, ref leftEquippedVisualInstance);

        }



        private bool HasAnyEquippedVisualActive()

        {

            return (rightEquippedVisualInstance != null && rightEquippedVisualInstance.activeSelf)

                || (leftEquippedVisualInstance != null && leftEquippedVisualInstance.activeSelf);

        }



        private void ShowHandEquippedVisual(

            string socketId,

            CCS_WeaponAttachmentFitProfile fitProfile,

            string attachmentOffsetObjectName,

            ref Transform attachmentRoot,

            ref GameObject visualInstance,

            string visualInstanceName)

        {

            if (equipmentSocketRegistry == null

                || fitProfile == null

                || revolverVisualOnlyPrefab == null

                || !equipmentSocketRegistry.TryGetSocket(socketId, out Transform socketTransform))

            {

                return;

            }



            if (!TryGetSocketDefinitionBaseline(

                    socketId,

                    out Vector3 definitionPosition,

                    out Vector3 definitionEuler,

                    out Vector3 definitionScale))

            {

                return;

            }



            attachmentRoot = EnsureAttachmentOffsetRoot(socketTransform, attachmentOffsetObjectName);

            CCS_WeaponAttachmentFitProfileApplicator.ApplyProfileToAttachmentRoot(

                socketTransform,

                attachmentRoot,

                fitProfile,

                definitionPosition,

                definitionEuler,

                definitionScale);



            visualInstance = EnsureVisualInstance(attachmentRoot, visualInstanceName);

            CCS_WeaponAttachmentFitProfileApplicator.ResetDirectVisualChildToIdentity(attachmentRoot);



            if (visualInstance != null)

            {

                visualInstance.SetActive(true);

            }

        }



        private static void HideHandEquippedVisual(ref Transform attachmentRoot, ref GameObject visualInstance)

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



        private static Transform EnsureAttachmentOffsetRoot(Transform socketTransform, string attachmentOffsetObjectName)

        {

            Transform existing = socketTransform.Find(attachmentOffsetObjectName);

            if (existing == null

                && attachmentOffsetObjectName == CCS_EquipmentConstants.RightHandRevolverAttachmentOffsetObjectName)

            {

                existing = socketTransform.Find(CCS_EquipmentConstants.LegacyRuntimeEquippedAttachmentRootObjectName);

                if (existing != null)

                {

                    existing.name = attachmentOffsetObjectName;

                }

            }



            if (existing != null)

            {

                return existing;

            }



            GameObject rootObject = new GameObject(attachmentOffsetObjectName);

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


