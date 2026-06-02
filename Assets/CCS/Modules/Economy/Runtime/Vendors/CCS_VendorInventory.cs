using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_VendorInventory
// CATEGORY: Modules / Economy / Runtime / Vendors
// PURPOSE: Serializable vendor catalog container.
// PLACEMENT: Embedded on CCS_VendorDefinition.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Economy
{
    [Serializable]
    public sealed class CCS_VendorInventory
    {
        [SerializeField] private CCS_VendorItemEntry[] items = Array.Empty<CCS_VendorItemEntry>();

        public CCS_VendorItemEntry[] Items => items ?? Array.Empty<CCS_VendorItemEntry>();
    }
}
