using CCS.Modules.Inventory;
using CCS.Modules.SurvivalCore;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SleepTestHarness
// CATEGORY: Modules / Sleep / Runtime / Testing
// PURPOSE: Development-only harness for bedroll seeding, fatigue setup, and sleep verification.
// PLACEMENT: Bootstrap verification scenes only. Disable for shipping builds.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Disabled by default. Uses CCS_SleepRuntimeBridge. No noisy logs.
// =============================================================================

namespace CCS.Modules.Sleep
{
    [DefaultExecutionOrder(270)]
    public sealed class CCS_SleepTestHarness : MonoBehaviour
    {
        #region Variables

        [Header("Development Testing")]
        [Tooltip("When enabled, the harness seeds bedroll and attempts one sleep verification.")]
        [SerializeField] private bool enableHarness;

        [Tooltip("Optional bedroll item seeded into inventory before the sleep attempt.")]
        [SerializeField] private CCS_ItemDefinition seedBedrollItem;

        [Tooltip("Quantity of bedroll items seeded for the test.")]
        [SerializeField] private int seedBedrollQuantity = 1;

        [Tooltip("Fatigue applied before the sleep attempt to ensure restore is observable.")]
        [SerializeField] private float seedFatigueAmount = 60f;

        [Tooltip("Seconds to wait after startup before running the automated sleep attempt.")]
        [SerializeField] private float startupDelaySeconds = 2f;

        private bool setupComplete;
        private bool sleepAttempted;
        private float nextActionTime;

        #endregion

        #region Unity Callbacks

        private void Update()
        {
            if (!enableHarness)
            {
                return;
            }

            if (Time.time < nextActionTime)
            {
                return;
            }

            if (!setupComplete)
            {
                if (!TryCompleteSetup())
                {
                    nextActionTime = Time.time + 1f;
                }

                return;
            }

            if (sleepAttempted)
            {
                return;
            }

            if (!CCS_SleepRuntimeBridge.TryGetSleepService(out CCS_SleepService sleepService)
                || !sleepService.IsInitialized)
            {
                nextActionTime = Time.time + 1f;
                return;
            }

            sleepService.TrySleep(new CCS_SleepRequest());
            sleepAttempted = true;
        }

        #endregion

        #region Private Methods

        private bool TryCompleteSetup()
        {
            if (!CCS_SleepRuntimeBridge.TryGetInventoryService(out CCS_PlayerInventoryService inventoryService)
                || !inventoryService.IsInitialized)
            {
                return false;
            }

            if (seedBedrollItem != null && seedBedrollQuantity > 0)
            {
                inventoryService.AddItem(seedBedrollItem, seedBedrollQuantity);
            }

            if (seedFatigueAmount > 0f
                && CCS_SleepRuntimeBridge.TryGetSurvivalCoreService(out CCS_SurvivalCoreService survivalCoreService)
                && survivalCoreService.IsInitialized)
            {
                survivalCoreService.TryApplyModifier(
                    CCS_SurvivalStatType.Fatigue,
                    CCS_SurvivalStatModifier.Add(seedFatigueAmount));
            }

            setupComplete = true;
            nextActionTime = Time.time + startupDelaySeconds;
            return true;
        }

        #endregion
    }
}
