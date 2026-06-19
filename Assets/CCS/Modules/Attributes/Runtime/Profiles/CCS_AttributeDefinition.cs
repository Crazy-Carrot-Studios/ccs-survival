using CCS.Project;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AttributeDefinition
// CATEGORY: Modules / Attributes / Runtime / Profiles
// PURPOSE: ScriptableObject definition for a single attribute type (Health first).
// PLACEMENT: ScriptableObject asset under module Profiles or Tests/Profiles.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Configuration only. Runtime values live in CCS_AttributeContainer.
// =============================================================================

namespace CCS.Modules.Attributes
{
    [CreateAssetMenu(
        fileName = "CCS_AttributeDefinition",
        menuName = "CCS/Attributes/Attribute Definition",
        order = 0)]
    public sealed class CCS_AttributeDefinition : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Attribute Bounds")]
        [Tooltip("Starting value when the container initializes.")]
        [SerializeField] private float defaultValue = 100f;

        [Tooltip("Minimum clamped value.")]
        [SerializeField] private float minValue;

        [Tooltip("Maximum clamped value.")]
        [SerializeField] private float maxValue = 100f;

        [Header("Future Systems")]
        [Tooltip("Placeholder for future passive regeneration systems.")]
        [SerializeField] private bool allowRegeneration;

        [Header("UI Metadata")]
        [Tooltip("Optional HUD label override.")]
        [SerializeField] private string uiLabel = "Health";

        [Tooltip("Optional HUD color.")]
        [SerializeField] private Color uiColor = new Color(0.85f, 0.2f, 0.2f, 1f);

        #endregion

        #region Properties

        public float DefaultValue => defaultValue;

        public float MinValue => minValue;

        public float MaxValue => maxValue;

        public bool AllowRegeneration => allowRegeneration;

        public string UiLabel => uiLabel;

        public Color UiColor => uiColor;

        #endregion
    }
}
