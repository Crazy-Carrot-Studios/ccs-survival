using CCS.Core;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalPickupInteractable
// CATEGORY: Survival / Runtime / Interaction
// PURPOSE: Prototype world pickup that logs collection and hides after interact.
// PLACEMENT: Attach to prototype pickup objects under CCS_PrototypePickupsRoot.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Inventory hook via CCS_SurvivalPickupCollectedEvent. No inventory mutation yet.
// =============================================================================

namespace CCS.Survival.Interaction
{
    [DisallowMultipleComponent]
    public sealed class CCS_SurvivalPickupInteractable : MonoBehaviour, CCS_ISurvivalInteractable
    {
        private const string LogCategory = "Survival Interaction";

        #region Variables

        [Header("Pickup")]
        [Tooltip("Stable id for future inventory/resource services.")]
        [SerializeField] private string pickupId = "prototype.pickup";

        [Tooltip("Readable label used in prompts and logs.")]
        [SerializeField] private string displayName = "Prototype Pickup";

        [Tooltip("Collected amount for future inventory/resource hooks.")]
        [SerializeField] private int amount = 1;

        [Tooltip("Optional prompt override. Defaults to Pick up {displayName}.")]
        [SerializeField] private string interactionPromptOverride;

        [Header("Collection")]
        [Tooltip("When enabled, disables renderers and colliders instead of destroying the object.")]
        [SerializeField] private bool hideAfterCollect = true;

        [Header("Events")]
        [Tooltip("Optional runtime host for pickup collected dispatch.")]
        [SerializeField] private CCS_RuntimeHost runtimeHost;

        [Header("Debug")]
        [Tooltip("Logs one concise line when this pickup is collected.")]
        [SerializeField] private bool enableDebugLogs = true;

        private bool isCollected;

        #endregion

        #region Properties

        public string PickupId => pickupId;

        public string DisplayName => displayName;

        public int Amount => amount;

        public bool IsCollected => isCollected;

        public string InteractionPrompt => string.IsNullOrWhiteSpace(interactionPromptOverride)
            ? $"Pick up {displayName}"
            : interactionPromptOverride;

        #endregion

        #region Public Methods

        public bool CanInteract(GameObject interactor)
        {
            return !isCollected && interactor != null;
        }

        public void Interact(GameObject interactor)
        {
            if (isCollected)
            {
                return;
            }

            isCollected = true;
            DispatchPickupCollected();

            if (enableDebugLogs)
            {
                CCS_Logger.Log(
                    LogCategory,
                    $"Collected pickup '{displayName}' (id={pickupId}, amount={amount}).",
                    true);
            }

            if (hideAfterCollect)
            {
                HidePickupVisuals();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        #endregion

        #region Private Methods

        private void HidePickupVisuals()
        {
            Collider[] colliders = GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }

            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = false;
            }
        }

        private void DispatchPickupCollected()
        {
            ResolveRuntimeHostReference();
            if (!CCS_Validation.IsObjectValid(runtimeHost) || !runtimeHost.IsRuntimeInitialized)
            {
                return;
            }

            runtimeHost.EventDispatcher.Dispatch(
                new CCS_SurvivalPickupCollectedEvent(pickupId, displayName, amount));
        }

        private void ResolveRuntimeHostReference()
        {
            if (CCS_Validation.IsObjectValid(runtimeHost))
            {
                return;
            }

            GameObject bootstrapRoot = GameObject.Find("PF_CCS_Survival_BootstrapRoot");
            if (bootstrapRoot != null
                && bootstrapRoot.TryGetComponent(out CCS_RuntimeHost bootstrapHost))
            {
                runtimeHost = bootstrapHost;
            }
        }

        #endregion
    }
}
