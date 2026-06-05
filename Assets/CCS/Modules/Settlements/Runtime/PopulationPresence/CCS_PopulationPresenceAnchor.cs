using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PopulationPresenceAnchor
// CATEGORY: Modules / Settlements / Runtime / PopulationPresence
// PURPOSE: Spawns idle placeholder actors from settlement workforce population counts.
// PLACEMENT: Bootstrap scene children near settlement workforce zones.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.0.0 — registers with CCS_PopulationPresenceRuntimeBridge.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_PopulationPresenceAnchor : MonoBehaviour
    {
        [SerializeField] private string anchorId = string.Empty;

        [SerializeField] private string settlementId = string.Empty;

        [SerializeField] private CCS_SettlementPopulationCategory workforceCategory =
            CCS_SettlementPopulationCategory.Unknown;

        [SerializeField] private int minimumPopulationCount = 1;

        [SerializeField] private int maxVisibleActors = 4;

        [SerializeField] private float spawnRadius = 2.5f;

        [SerializeField] private string businessId = string.Empty;

        [SerializeField] private CCS_SettlementGrowthStage requiredGrowthStage =
            CCS_SettlementGrowthStage.Unknown;

        [SerializeField] private string displayName = string.Empty;

        [SerializeField] private Transform actorsContainer;

        [SerializeField] private CCS_PopulationPresenceLabel presenceLabel;

        [SerializeField] private PrimitiveType placeholderPrimitive = PrimitiveType.Capsule;

        public string AnchorId => anchorId ?? string.Empty;

        public string SettlementId => settlementId ?? string.Empty;

        public CCS_SettlementPopulationCategory WorkforceCategory => workforceCategory;

        public int MaxVisibleActors => maxVisibleActors < 1 ? 1 : maxVisibleActors;

        public int GetSpawnedActorCount()
        {
            EnsureComponents();
            return actorsContainer != null ? actorsContainer.childCount : 0;
        }

        private void Awake()
        {
            EnsureComponents();
        }

        private void OnEnable()
        {
            CCS_PopulationPresenceRuntimeBridge.RegisterAnchor(this);
            RefreshFromPopulationState();
        }

        private void OnDisable()
        {
            CCS_PopulationPresenceRuntimeBridge.UnregisterAnchor(this);
        }

        public void RefreshFromPopulationState()
        {
            EnsureComponents();
            int sourceCount = CCS_PopulationPresenceRuntimeBridge.ResolveWorkforceCount(
                SettlementId,
                WorkforceCategory);
            bool growthMet = CCS_PopulationPresenceRuntimeBridge.IsGrowthStageMet(
                SettlementId,
                requiredGrowthStage);
            bool discovered = CCS_PopulationPresenceRuntimeBridge.IsSettlementDiscovered(SettlementId);
            int visibleCount = CCS_PopulationPresenceValidationUtility.ResolveVisibleActorCount(
                CCS_PopulationPresenceRuntimeBridge.GetPopulationSnapshotForSettlement(SettlementId),
                WorkforceCategory,
                minimumPopulationCount,
                MaxVisibleActors,
                discovered,
                growthMet);

            SyncPlaceholderActors(visibleCount);
            string labelName = string.IsNullOrWhiteSpace(displayName)
                ? WorkforceCategory.ToString()
                : displayName;
            presenceLabel?.ApplyPresence(SettlementId, WorkforceCategory, sourceCount, visibleCount);
        }

        private void EnsureComponents()
        {
            if (actorsContainer == null)
            {
                Transform existing = transform.Find("CCS_PopulationPresence_Actors");
                actorsContainer = existing != null
                    ? existing
                    : new GameObject("CCS_PopulationPresence_Actors").transform;
                if (existing == null)
                {
                    actorsContainer.SetParent(transform, false);
                }
            }

            if (presenceLabel == null)
            {
                presenceLabel = GetComponentInChildren<CCS_PopulationPresenceLabel>(true);
            }
        }

        private void SyncPlaceholderActors(int targetCount)
        {
            if (actorsContainer == null)
            {
                return;
            }

            int currentCount = actorsContainer.childCount;
            while (currentCount < targetCount)
            {
                CreatePlaceholderActor(currentCount);
                currentCount++;
            }

            while (currentCount > targetCount)
            {
                Transform child = actorsContainer.GetChild(currentCount - 1);
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }

                currentCount--;
            }

            for (int index = 0; index < actorsContainer.childCount; index++)
            {
                Transform actorTransform = actorsContainer.GetChild(index);
                actorTransform.localPosition = ResolveSpawnOffset(index, actorsContainer.childCount);
            }
        }

        private void CreatePlaceholderActor(int index)
        {
            GameObject actorObject = GameObject.CreatePrimitive(placeholderPrimitive);
            actorObject.name = $"CCS_PopulationPlaceholder_{WorkforceCategory}_{index}";
            actorObject.transform.SetParent(actorsContainer, false);
            actorObject.transform.localPosition = ResolveSpawnOffset(index, maxVisibleActors);
            actorObject.transform.localScale = new Vector3(0.45f, 0.9f, 0.45f);

            Collider collider = actorObject.GetComponent<Collider>();
            if (collider != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(collider);
                }
                else
                {
                    DestroyImmediate(collider);
                }
            }

            Rigidbody rigidbody = actorObject.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(rigidbody);
                }
                else
                {
                    DestroyImmediate(rigidbody);
                }
            }

            CCS_PopulationPlaceholderActor placeholder = actorObject.GetComponent<CCS_PopulationPlaceholderActor>();
            if (placeholder == null)
            {
                placeholder = actorObject.AddComponent<CCS_PopulationPlaceholderActor>();
            }

            placeholder.Configure(WorkforceCategory);
        }

        private Vector3 ResolveSpawnOffset(int index, int totalSlots)
        {
            float radius = spawnRadius < 0.5f ? 0.5f : spawnRadius;
            int slots = totalSlots < 1 ? 1 : totalSlots;
            float angle = (Mathf.PI * 2f * index) / slots;
            return new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }
    }
}
