using System;
using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingSnapPoint
// CATEGORY: Modules / Building / Runtime / Snap
// PURPOSE: Serializable snap point authored on building piece definitions.
// PLACEMENT: Serialized on CCS_BuildingPieceDefinition assets.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Occupancy tracked at runtime on CCS_BuildingRuntimeSnapPoint instances.
// =============================================================================

namespace CCS.Modules.Building
{
    [Serializable]
    public sealed class CCS_BuildingSnapPoint
    {
        #region Variables

        [Tooltip("Stable snap point ID unique within the piece definition.")]
        [SerializeField] private string snapPointId = string.Empty;

        [Tooltip("Snap point category used for compatibility matching.")]
        [SerializeField] private CCS_BuildingSnapPointType snapPointType = CCS_BuildingSnapPointType.Free;

        [Tooltip("Local position relative to the placed piece origin.")]
        [SerializeField] private Vector3 localPosition;

        [Tooltip("Local euler rotation relative to the placed piece origin.")]
        [SerializeField] private Vector3 localEulerAngles;

        [Tooltip("Optional explicit compatible target types. Empty uses global compatibility rules.")]
        [SerializeField] private List<CCS_BuildingSnapPointType> compatibleTargetTypes =
            new List<CCS_BuildingSnapPointType>();

        #endregion

        #region Properties

        public string SnapPointId => snapPointId ?? string.Empty;

        public CCS_BuildingSnapPointType SnapPointType => snapPointType;

        public Vector3 LocalPosition => localPosition;

        public Quaternion LocalRotation => Quaternion.Euler(localEulerAngles);

        public IReadOnlyList<CCS_BuildingSnapPointType> CompatibleTargetTypes => compatibleTargetTypes;

        #endregion
    }
}
