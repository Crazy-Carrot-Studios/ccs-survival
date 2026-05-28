using CCS.Core;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalInteractionScanner
// CATEGORY: Survival / Runtime / Interaction
// PURPOSE: Finds the nearest valid interactable near the player and performs interactions.
// PLACEMENT: Attach to CCS_PlayerRoot. Disabled automatically when the player root is inactive.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Uses OverlapSphereNonAlloc. Does not scan during traversal when player is hidden.
// =============================================================================

namespace CCS.Survival.Interaction
{
    public sealed class CCS_SurvivalInteractionScanner : MonoBehaviour
    {
        private const string LogCategory = "Survival Interaction";
        private const int OverlapBufferSize = 16;

        #region Variables

        [Header("Scan")]
        [Tooltip("Origin for overlap scans. Defaults to this transform when unset.")]
        [SerializeField] private Transform scanOrigin;

        [Tooltip("Radius in meters for interactable discovery.")]
        [SerializeField] private float scanRadius = 2.25f;

        [Tooltip("Optional layer mask for interactable colliders.")]
        [SerializeField] private LayerMask interactableLayers = ~0;

        [Header("Events")]
        [Tooltip("Optional runtime host for interaction event dispatch.")]
        [SerializeField] private CCS_RuntimeHost runtimeHost;

        [Header("Debug")]
        [Tooltip("Logs concise interaction target changes and performed interactions.")]
        [SerializeField] private bool enableDebugLogs;

        private readonly Collider[] overlapBuffer = new Collider[OverlapBufferSize];
        private CCS_ISurvivalInteractable currentInteractable;
        private string currentInteractionPrompt = "None";
        private bool runtimeHostResolveAttempted;

        #endregion

        #region Properties

        public string CurrentInteractionPrompt => currentInteractionPrompt;

        public CCS_ISurvivalInteractable CurrentInteractable => currentInteractable;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            if (!CCS_Validation.IsObjectValid(scanOrigin))
            {
                scanOrigin = transform;
            }
        }

        private void OnEnable()
        {
            ResolveRuntimeHostReference();
            RefreshCurrentTarget(forceNotify: true);
        }

        private void OnDisable()
        {
            SetCurrentTarget(null, forceNotify: true);
        }

        private void Update()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            RefreshCurrentTarget(forceNotify: false);
        }

        #endregion

        #region Public Methods

        public bool TryPerformInteraction()
        {
            if (currentInteractable == null || !currentInteractable.CanInteract(gameObject))
            {
                return false;
            }

            string interactableLabel = currentInteractionPrompt;
            currentInteractable.Interact(gameObject);
            DispatchInteractionPerformed(interactableLabel);
            RefreshCurrentTarget(forceNotify: true);
            return true;
        }

        #endregion

        #region Private Methods

        private void RefreshCurrentTarget(bool forceNotify)
        {
            CCS_ISurvivalInteractable bestInteractable = FindBestInteractable(out float bestDistanceSqr);
            if (bestInteractable == null)
            {
                SetCurrentTarget(null, forceNotify);
                return;
            }

            if (!bestInteractable.CanInteract(gameObject))
            {
                SetCurrentTarget(null, forceNotify);
                return;
            }

            if (!forceNotify && ReferenceEquals(bestInteractable, currentInteractable))
            {
                return;
            }

            SetCurrentTarget(bestInteractable, forceNotify);
        }

        private CCS_ISurvivalInteractable FindBestInteractable(out float bestDistanceSqr)
        {
            bestDistanceSqr = float.MaxValue;
            CCS_ISurvivalInteractable bestInteractable = null;
            Vector3 origin = scanOrigin.position;
            int hitCount = Physics.OverlapSphereNonAlloc(
                origin,
                scanRadius,
                overlapBuffer,
                interactableLayers,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < hitCount; i++)
            {
                Collider collider = overlapBuffer[i];
                if (collider == null)
                {
                    continue;
                }

                CCS_ISurvivalInteractable candidate = ResolveInteractable(collider);
                if (candidate == null || !candidate.CanInteract(gameObject))
                {
                    continue;
                }

                Vector3 closestPoint = collider.ClosestPoint(origin);
                float distanceSqr = (closestPoint - origin).sqrMagnitude;
                if (distanceSqr >= bestDistanceSqr)
                {
                    continue;
                }

                bestDistanceSqr = distanceSqr;
                bestInteractable = candidate;
            }

            return bestInteractable;
        }

        private static CCS_ISurvivalInteractable ResolveInteractable(Collider collider)
        {
            if (collider.TryGetComponent(out CCS_SurvivalPickupInteractable pickupOnCollider))
            {
                return pickupOnCollider;
            }

            return collider.GetComponentInParent<CCS_SurvivalPickupInteractable>();
        }

        private void SetCurrentTarget(CCS_ISurvivalInteractable interactable, bool forceNotify)
        {
            string nextPrompt = interactable == null ? "None" : interactable.InteractionPrompt;
            if (!forceNotify
                && ReferenceEquals(interactable, currentInteractable)
                && currentInteractionPrompt == nextPrompt)
            {
                return;
            }

            currentInteractable = interactable;
            currentInteractionPrompt = nextPrompt;
            DispatchTargetChanged(currentInteractionPrompt);

            if (enableDebugLogs)
            {
                CCS_Logger.Log(LogCategory, $"Interaction target: {currentInteractionPrompt}", true);
            }
        }

        private void ResolveRuntimeHostReference()
        {
            if (runtimeHostResolveAttempted)
            {
                return;
            }

            runtimeHostResolveAttempted = true;

            if (CCS_Validation.IsObjectValid(runtimeHost))
            {
                return;
            }

            runtimeHost = GetComponent<CCS_RuntimeHost>();
            if (CCS_Validation.IsObjectValid(runtimeHost))
            {
                return;
            }

            runtimeHost = GetComponentInParent<CCS_RuntimeHost>();
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

        private void DispatchTargetChanged(string prompt)
        {
            ResolveRuntimeHostReference();
            if (!CCS_Validation.IsObjectValid(runtimeHost) || !runtimeHost.IsRuntimeInitialized)
            {
                return;
            }

            runtimeHost.EventDispatcher.Dispatch(new CCS_SurvivalInteractionTargetChangedEvent(prompt));
        }

        private void DispatchInteractionPerformed(string interactableLabel)
        {
            ResolveRuntimeHostReference();
            if (!CCS_Validation.IsObjectValid(runtimeHost) || !runtimeHost.IsRuntimeInitialized)
            {
                return;
            }

            runtimeHost.EventDispatcher.Dispatch(new CCS_SurvivalInteractionPerformedEvent(interactableLabel));

            if (enableDebugLogs)
            {
                CCS_Logger.Log(LogCategory, $"Interaction performed: {interactableLabel}", true);
            }
        }

        #endregion
    }
}
