using CCS.Modules.Attributes;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AIBanditDamageHitbox
// CATEGORY: Modules / AI / Runtime / Components
// PURPOSE: Alive-state capsule hitbox so player weapon raycasts can damage AI health.
// PLACEMENT: Child of AI bandit root (AI_Hitbox).
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Ragdoll bone colliders stay disabled while alive; this hitbox stays active.
// =============================================================================

namespace CCS.Modules.AI
{
    [DisallowMultipleComponent]
    public sealed class CCS_AIBanditDamageHitbox : MonoBehaviour
    {
        public const string HitboxObjectName = "AI_Hitbox";

        [SerializeField] private CapsuleCollider capsuleCollider;
        [SerializeField] private CCS_NetworkHealth networkHealth;

        public CapsuleCollider CapsuleCollider => capsuleCollider;

        public void Configure(CapsuleCollider configuredCollider, CCS_NetworkHealth configuredHealth)
        {
            capsuleCollider = configuredCollider;
            networkHealth = configuredHealth;
            EnsureColliderSettings();
        }

        public void SetHitboxEnabled(bool enabled)
        {
            if (capsuleCollider != null)
            {
                capsuleCollider.enabled = enabled;
            }
        }

        private void Awake()
        {
            ResolveReferences();
            EnsureColliderSettings();
        }

        private void ResolveReferences()
        {
            if (capsuleCollider == null)
            {
                capsuleCollider = GetComponent<CapsuleCollider>();
            }

            if (networkHealth == null)
            {
                networkHealth = GetComponentInParent<CCS_NetworkHealth>();
            }
        }

        private void EnsureColliderSettings()
        {
            if (capsuleCollider == null)
            {
                return;
            }

            capsuleCollider.isTrigger = false;
            capsuleCollider.direction = 1;
            capsuleCollider.radius = 0.35f;
            capsuleCollider.height = 1.8f;
            capsuleCollider.center = new Vector3(0f, 1f, 0f);
            capsuleCollider.enabled = true;
        }
    }
}
