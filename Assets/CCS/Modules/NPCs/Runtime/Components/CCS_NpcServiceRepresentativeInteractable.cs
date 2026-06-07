using CCS.Modules.Interaction;
using CCS.Modules.Settlements;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcServiceRepresentativeInteractable
// CATEGORY: Modules / NPCs / Runtime / Components
// PURPOSE: Routes representative NPC interaction through existing settlement service points.
// PLACEMENT: Added to population placeholder actors assigned as service representatives.
// AUTHOR: James Schilz
// CREATED: 2026-06-05
// NOTES: Milestone 4.3.0 — no duplicate vendor/bank/contract routing logic.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public sealed class CCS_NpcServiceRepresentativeInteractable : MonoBehaviour, CCS_IInteractable
    {
        [SerializeField] private string servicePointId = string.Empty;

        [SerializeField] private string representativeId = string.Empty;

        [SerializeField] private string businessId = string.Empty;

        [SerializeField] private string settlementId = string.Empty;

        [SerializeField] private float interactionDistance = 3f;

        [SerializeField] private bool isRepresentativeActive;

        public string ServicePointId => servicePointId ?? string.Empty;

        public string RepresentativeId => representativeId ?? string.Empty;

        public void Configure(CCS_NpcServiceRepresentativeSnapshot snapshot, bool active)
        {
            if (snapshot == null)
            {
                isRepresentativeActive = false;
                return;
            }

            representativeId = snapshot.RepresentativeId;
            servicePointId = snapshot.ServicePointId;
            businessId = snapshot.BusinessId;
            settlementId = snapshot.SettlementId;
            isRepresentativeActive = active && snapshot.IsActive;
        }

        public string GetInteractionDisplayName()
        {
            return CCS_NpcServiceRepresentativeRuntimeBridge.ResolveDisplayName(this) ?? "Service Representative";
        }

        public bool CanInteract()
        {
            return isActiveAndEnabled && isRepresentativeActive;
        }

        public void Interact()
        {
            TryInteract();
        }

        public float GetInteractionDistance()
        {
            return interactionDistance < 0.5f ? 3f : interactionDistance;
        }

        public bool TryInteract()
        {
            if (!CanInteract())
            {
                CCS_NpcServiceRepresentativeDebugHud.NotifyFallback(
                    representativeId,
                    servicePointId,
                    "Representative inactive — use service point fallback.");
                return false;
            }

            CCS_INpcMovementHost movementHost = GetComponent<CCS_INpcMovementHost>();
            if (movementHost != null && movementHost.HasIdentity)
            {
                CCS_NpcDialogueStubRequest request =
                    CCS_NpcDialogueStubValidationUtility.BuildRequestFromHost(movementHost);
                if (CCS_SettlementServicePointRuntimeBridge.TryGetServicePoint(
                        servicePointId,
                        out CCS_SettlementServicePoint previewPoint)
                    && previewPoint != null)
                {
                    request.ServiceRoute = CCS_SettlementServiceRouteResolver.ResolveRouteType(previewPoint);
                }

                CCS_NpcDialogueStubRuntimeBridge.ResolveDialogue?.Invoke(request);
            }

            if (!CCS_SettlementServicePointRuntimeBridge.TryGetServicePoint(servicePointId, out CCS_SettlementServicePoint servicePoint)
                || servicePoint == null)
            {
                CCS_NpcServiceRepresentativeDebugHud.NotifyRouteResult(
                    representativeId,
                    servicePointId,
                    businessId,
                    settlementId,
                    CCS_SettlementServiceRouteType.Unknown,
                    false,
                    true,
                    "Linked service point missing — use service point fallback.");
                return false;
            }

            CCS_SettlementServiceActivationResult result = CCS_SettlementServiceRouteResolver.TryActivate(servicePoint);
            CCS_NpcServiceRepresentativeDebugHud.NotifyRouteResult(
                representativeId,
                servicePointId,
                businessId,
                settlementId,
                result.RouteType,
                result.IsSuccess,
                false,
                result.Message);
            return result.IsSuccess;
        }
    }
}
