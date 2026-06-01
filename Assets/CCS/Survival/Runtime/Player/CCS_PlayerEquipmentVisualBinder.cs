using CCS.Modules.Equipment;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerEquipmentVisualBinder
// CATEGORY: Survival / Runtime / Player
// PURPOSE: Binds local player equipment service events to primitive equipped visuals.
// PLACEMENT: PF_CCS_Player alongside CCS_EquipmentAttachmentRig.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Equipment state remains service-driven; this component only mirrors visuals.
// =============================================================================

namespace CCS.Survival.Player
{
    [DisallowMultipleComponent]
    public sealed class CCS_PlayerEquipmentVisualBinder : MonoBehaviour
    {
        #region Variables

        [SerializeField] private CCS_EquipmentAttachmentRig attachmentRig;
        [SerializeField] private CCS_EquipmentVisualProfile equipmentVisualProfile;
        [SerializeField] private bool enableDebugLogs;

        private CCS_EquipmentVisualController visualController = new CCS_EquipmentVisualController();
        private bool isBound;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            if (!isBound && CCS_EquipmentRuntimeBridge.TryGetEquipmentService(out CCS_PlayerEquipmentService equipmentService))
            {
                BindEquipmentService(equipmentService);
            }
        }

        private void OnDestroy()
        {
            Unbind();
        }

        #endregion

        #region Public Methods

        public void BindEquipmentService(CCS_PlayerEquipmentService equipmentService)
        {
            if (equipmentService == null)
            {
                return;
            }

            if (attachmentRig == null)
            {
                attachmentRig = GetComponentInChildren<CCS_EquipmentAttachmentRig>(true);
            }

            visualController.Bind(equipmentService, attachmentRig, equipmentVisualProfile, enableDebugLogs);
            isBound = true;
        }

        public void Unbind()
        {
            if (!isBound)
            {
                return;
            }

            visualController.Unbind();
            isBound = false;
        }

        public bool HasVisualForItem(string itemId) => visualController.HasVisualForItem(itemId);

        #endregion

        #region Properties

        public CCS_EquipmentAttachmentRig AttachmentRig => attachmentRig;

        public CCS_EquipmentVisualProfile EquipmentVisualProfile => equipmentVisualProfile;

        #endregion
    }
}
