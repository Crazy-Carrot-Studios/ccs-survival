using CCS.Core;
using CCS.Modules.Crafting;
using CCS.Modules.Equipment;
using CCS.Modules.Interaction;
using CCS.Modules.Inventory;
using CCS.Modules.SurvivalCore;
using CCS.Modules.WorldResources;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalGameplayServiceHost
// CATEGORY: Survival / Runtime / Composition
// PURPOSE: Registers gameplay module services on the runtime service registry from profiles.
// PLACEMENT: PF_CCS_Survival_BootstrapRoot alongside CCS_RuntimeHost and CCS_SurvivalBootstrap.
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: Runs after survival bootstrap install pipeline. No singletons or scene name lookups.
// =============================================================================

namespace CCS.Survival.Composition
{
    [DefaultExecutionOrder(101)]
    public sealed class CCS_SurvivalGameplayServiceHost : MonoBehaviour
    {
        #region Variables

        [Header("Gameplay Service Profiles")]
        [Tooltip("Default survival core profile used to register CCS_SurvivalCoreService.")]
        [SerializeField] private CCS_SurvivalCoreProfile survivalCoreProfile;

        [Tooltip("Default interaction profile used to register CCS_InteractionService.")]
        [SerializeField] private CCS_InteractionProfile interactionProfile;

        [Tooltip("Default inventory profile used to register CCS_PlayerInventoryService.")]
        [SerializeField] private CCS_InventoryProfile inventoryProfile;

        [Tooltip("Default equipment profile used to register CCS_PlayerEquipmentService.")]
        [SerializeField] private CCS_EquipmentProfile equipmentProfile;

        [Tooltip("Default world resource profile used to register resource harvest and respawn services.")]
        [SerializeField] private CCS_WorldResourceProfile worldResourceProfile;

        [Tooltip("Default crafting profile used to register CCS_CraftingService.")]
        [SerializeField] private CCS_CraftingProfile craftingProfile;

        [Header("Diagnostics")]
        [Tooltip("Emit gameplay service registration logs.")]
        [SerializeField] private bool enableDebugLogs;

        private CCS_RuntimeHost runtimeHost;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            runtimeHost = GetComponent<CCS_RuntimeHost>();
            if (runtimeHost == null)
            {
                runtimeHost = GetComponentInParent<CCS_RuntimeHost>();
            }

            if (runtimeHost == null)
            {
                CCS_Logger.LogWarning(
                    CCS_SurvivalRuntimeConstants.SurvivalBootstrapLogCategory,
                    "CCS_SurvivalGameplayServiceHost could not find CCS_RuntimeHost.");
                return;
            }

            CCS_SurvivalGameplayServiceRegistration.RegisterGameplayServices(
                runtimeHost,
                survivalCoreProfile,
                interactionProfile,
                inventoryProfile,
                equipmentProfile,
                worldResourceProfile,
                craftingProfile,
                enableDebugLogs);
        }

        #endregion
    }
}
