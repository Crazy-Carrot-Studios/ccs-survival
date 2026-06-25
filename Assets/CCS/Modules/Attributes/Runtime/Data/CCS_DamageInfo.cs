using UnityEngine;

// =============================================================================
// SCRIPT: CCS_DamageInfo
// CATEGORY: Modules / Attributes / Runtime / Data
// PURPOSE: Immutable combat damage payload for shared network and gameplay calls.
// PLACEMENT: Passed through CCS_IDamageable.ApplyDamage.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Carries source metadata without introducing cross-module circular references.
// =============================================================================

namespace CCS.Modules.Attributes
{
    public readonly struct CCS_DamageInfo
    {
        public float Amount { get; }

        public Vector3 HitPoint { get; }

        public Vector3 HitDirection { get; }

        public CCS_DamageSourceType SourceType { get; }

        public GameObject SourceObject { get; }

        public ulong SourceNetworkObjectId { get; }

        public string AttributeId { get; }

        public CCS_DamageInfo(
            float amount,
            Vector3 hitPoint,
            Vector3 hitDirection,
            CCS_DamageSourceType sourceType,
            GameObject sourceObject,
            ulong sourceNetworkObjectId,
            string attributeId = CCS_AttributesConstants.HealthAttributeId)
        {
            Amount = amount;
            HitPoint = hitPoint;
            HitDirection = hitDirection;
            SourceType = sourceType;
            SourceObject = sourceObject;
            SourceNetworkObjectId = sourceNetworkObjectId;
            AttributeId = string.IsNullOrWhiteSpace(attributeId)
                ? CCS_AttributesConstants.HealthAttributeId
                : attributeId;
        }
    }
}
