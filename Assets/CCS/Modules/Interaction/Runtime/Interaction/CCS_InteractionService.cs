using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_InteractionService
// CATEGORY: Modules / Interaction / Runtime / Interaction
// PURPOSE: Runtime owner of interaction scanning, current target, and request flow.
// PLACEMENT: Registered as CCS_ISurvivalService by future interaction module installer.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Never references inventory, crafting, equipment, save, or quest systems.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public sealed class CCS_InteractionService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_InteractionService]";

        #region Variables

        private readonly CCS_InteractionScanner scanner = new CCS_InteractionScanner();

        private CCS_InteractionProfile activeProfile;
        private CCS_IInteractable currentTarget;
        private bool isInitialized;

        #endregion

        #region Events

        public event InteractionInteractableFoundHandler InteractableFound;
        public event InteractionInteractableLostHandler InteractableLost;
        public event InteractionRequestedHandler InteractionRequested;
        public event InteractionSucceededHandler InteractionSucceeded;
        public event InteractionFailedHandler InteractionFailed;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_InteractionProfile ActiveProfile => activeProfile;

        public CCS_IInteractable CurrentTarget => currentTarget;

        public bool HasCurrentTarget => currentTarget != null;

        #endregion

        #region Public Methods

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            // Profile binding via InitializeFromProfile sets isInitialized when ready.
        }

        public void InitializeFromProfile(CCS_InteractionProfile profile)
        {
            if (profile == null)
            {
                Debug.LogWarning($"{LogPrefix} InitializeFromProfile called with null profile.");
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_InteractionValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            activeProfile = profile;
            ClearCurrentTarget(notifyListeners: true);
            isInitialized = true;
        }

        public void TickScan(Vector3 scanOrigin, Vector3 scanForward)
        {
            if (!isInitialized || activeProfile == null)
            {
                return;
            }

            CCS_InteractionDetectionResult result = scanner.ScanForward(scanOrigin, scanForward, activeProfile);
            UpdateCurrentTarget(result);
        }

        public bool RequestInteraction()
        {
            if (!isInitialized)
            {
                NotifyInteractionFailed(null, "Interaction service is not initialized.");
                return false;
            }

            if (currentTarget == null)
            {
                NotifyInteractionFailed(null, "No interactable target is focused.");
                return false;
            }

            CCS_InteractionEventArgs requestArgs = BuildEventArgs(currentTarget, 0f);
            InteractionRequested?.Invoke(requestArgs);

            if (!currentTarget.CanInteract())
            {
                NotifyInteractionFailed(currentTarget, "Target cannot be interacted with right now.");
                return false;
            }

            bool interactionSucceeded = ExecuteInteraction(currentTarget);
            if (!interactionSucceeded)
            {
                return false;
            }

            InteractionSucceeded?.Invoke(requestArgs);
            return true;
        }

        public void NotifyInteractionFailed(CCS_IInteractable interactable, string message)
        {
            RaiseInteractionFailed(interactable, message);
        }

        public void ClearCurrentTarget(bool notifyListeners = false)
        {
            if (currentTarget == null)
            {
                return;
            }

            CCS_IInteractable previous = currentTarget;
            currentTarget = null;

            if (notifyListeners)
            {
                InteractableLost?.Invoke(BuildEventArgs(previous, 0f));
            }
        }

        #endregion

        #region Private Methods

        private void UpdateCurrentTarget(CCS_InteractionDetectionResult result)
        {
            CCS_IInteractable detected = result.HasTarget ? result.Interactable : null;

            if (ReferenceEquals(currentTarget, detected))
            {
                return;
            }

            if (currentTarget != null)
            {
                InteractableLost?.Invoke(BuildEventArgs(currentTarget, result.Distance));
            }

            currentTarget = detected;

            if (currentTarget != null)
            {
                InteractableFound?.Invoke(BuildEventArgs(currentTarget, result.Distance));
            }
        }

        private static bool ExecuteInteraction(CCS_IInteractable interactable)
        {
            if (interactable is CCS_IInteractableResultProvider resultProvider)
            {
                return resultProvider.TryInteract();
            }

            interactable.Interact();
            return true;
        }

        private static CCS_InteractionEventArgs BuildEventArgs(CCS_IInteractable interactable, float distance)
        {
            string displayName = interactable != null
                ? interactable.GetInteractionDisplayName()
                : string.Empty;

            return new CCS_InteractionEventArgs(interactable, displayName, distance);
        }

        private void RaiseInteractionFailed(CCS_IInteractable interactable, string message)
        {
            InteractionFailed?.Invoke(new CCS_InteractionEventArgs(interactable, interactable?.GetInteractionDisplayName(), 0f, message));
        }

        #endregion
    }
}
