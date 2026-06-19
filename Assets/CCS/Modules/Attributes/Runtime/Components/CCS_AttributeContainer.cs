using System;
using System.Collections.Generic;
using CCS.Core;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AttributeContainer
// CATEGORY: Modules / Attributes / Runtime / Components
// PURPOSE: Holds runtime attribute values and applies clamped damage/heal changes.
// PLACEMENT: Player or authority root with assigned CCS_AttributeDefinition assets.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Server-authoritative in multiplayer via CCS_NetworkAttributeReplicator.
//        Solo/offline sessions mutate this container directly.
// =============================================================================

namespace CCS.Modules.Attributes
{
    public sealed class CCS_AttributeContainer : MonoBehaviour
    {
        #region Variables

        [SerializeField] private CCS_AttributeDefinition[] attributeDefinitions;

        private readonly Dictionary<string, CCS_AttributeValue> valuesById =
            new Dictionary<string, CCS_AttributeValue>();

        private readonly Dictionary<string, CCS_AttributeDefinition> definitionsById =
            new Dictionary<string, CCS_AttributeDefinition>();

        #endregion

        #region Properties

        public event Action<CCS_AttributeChangedEvent> AttributeChanged;

        public event Action<CCS_DamageAppliedEvent> DamageApplied;

        public event Action<CCS_PlayerDeathEvent> PlayerDeath;

        public IReadOnlyList<CCS_AttributeDefinition> AttributeDefinitions => attributeDefinitions;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            InitializeFromDefinitions();
        }

        #endregion

        #region Public Methods

        public void InitializeFromDefinitions()
        {
            valuesById.Clear();
            definitionsById.Clear();

            if (attributeDefinitions == null)
            {
                return;
            }

            for (int i = 0; i < attributeDefinitions.Length; i++)
            {
                CCS_AttributeDefinition definition = attributeDefinitions[i];
                if (definition == null || string.IsNullOrWhiteSpace(definition.ProfileId))
                {
                    continue;
                }

                definitionsById[definition.ProfileId] = definition;
                float clamped = ClampToDefinition(definition, definition.DefaultValue);
                valuesById[definition.ProfileId] = new CCS_AttributeValue(
                    definition.ProfileId,
                    clamped,
                    definition.MinValue,
                    definition.MaxValue);
            }
        }

        public bool TryGetValue(string attributeId, out CCS_AttributeValue value)
        {
            if (string.IsNullOrWhiteSpace(attributeId))
            {
                value = default;
                return false;
            }

            return valuesById.TryGetValue(attributeId, out value);
        }

        public bool TryGetDefinition(string attributeId, out CCS_AttributeDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(attributeId))
            {
                definition = null;
                return false;
            }

            return definitionsById.TryGetValue(attributeId, out definition);
        }

        public bool SetValue(string attributeId, float newValue, bool raiseEvents = true)
        {
            if (!TryGetDefinition(attributeId, out CCS_AttributeDefinition definition))
            {
                return false;
            }

            return SetValue(definition, newValue, raiseEvents);
        }

        public bool SetValue(CCS_AttributeDefinition definition, float newValue, bool raiseEvents = true)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.ProfileId))
            {
                return false;
            }

            string attributeId = definition.ProfileId;
            if (!valuesById.TryGetValue(attributeId, out CCS_AttributeValue previous))
            {
                previous = new CCS_AttributeValue(
                    attributeId,
                    definition.DefaultValue,
                    definition.MinValue,
                    definition.MaxValue);
            }

            float clamped = ClampToDefinition(definition, newValue);
            CCS_AttributeValue current = previous.WithCurrent(clamped);
            valuesById[attributeId] = current;

            if (!raiseEvents || Mathf.Approximately(previous.Current, current.Current))
            {
                return true;
            }

            CCS_AttributeChangedEvent changedEvent = new CCS_AttributeChangedEvent(attributeId, previous, current);
            AttributeChanged?.Invoke(changedEvent);
            TryRaiseDeathEvent(definition, previous, current);
            return true;
        }

        public bool ApplyDamage(CCS_DamageRequest request, out CCS_DamageAppliedEvent damageEvent)
        {
            damageEvent = default;
            if (request.Amount <= 0f || !TryGetDefinition(request.AttributeId, out CCS_AttributeDefinition definition))
            {
                return false;
            }

            if (!valuesById.TryGetValue(request.AttributeId, out CCS_AttributeValue previous))
            {
                return false;
            }

            float newValue = previous.Current - request.Amount;
            SetValue(definition, newValue);

            if (!TryGetValue(request.AttributeId, out CCS_AttributeValue resulting))
            {
                return false;
            }

            float appliedAmount = previous.Current - resulting.Current;
            damageEvent = new CCS_DamageAppliedEvent(
                request.AttributeId,
                request.Amount,
                appliedAmount,
                resulting);
            DamageApplied?.Invoke(damageEvent);

            CCS_Logger.Log(
                CCS_AttributesConstants.ModuleLogCategory,
                $"{name} damage applied: {request.AttributeId} -{appliedAmount:0.##} -> {resulting.Current:0.##}/{resulting.Max:0.##}",
                definition.EnableDebugLogs);

            return appliedAmount > 0f;
        }

        public bool ApplyHeal(CCS_HealRequest request, out CCS_AttributeChangedEvent changedEvent)
        {
            changedEvent = default;
            if (request.Amount <= 0f || !TryGetDefinition(request.AttributeId, out CCS_AttributeDefinition definition))
            {
                return false;
            }

            if (!TryGetValue(request.AttributeId, out CCS_AttributeValue previous))
            {
                return false;
            }

            float newValue = previous.Current + request.Amount;
            SetValue(definition, newValue);

            if (!TryGetValue(request.AttributeId, out CCS_AttributeValue current))
            {
                return false;
            }

            changedEvent = new CCS_AttributeChangedEvent(request.AttributeId, previous, current);
            return !Mathf.Approximately(previous.Current, current.Current);
        }

        public void ConfigureDefinitions(CCS_AttributeDefinition[] definitions, bool reinitialize = true)
        {
            attributeDefinitions = definitions;
            if (reinitialize)
            {
                InitializeFromDefinitions();
            }
        }

        #endregion

        #region Private Methods

        private static float ClampToDefinition(CCS_AttributeDefinition definition, float value)
        {
            return Mathf.Clamp(value, definition.MinValue, definition.MaxValue);
        }

        private void TryRaiseDeathEvent(
            CCS_AttributeDefinition definition,
            CCS_AttributeValue previous,
            CCS_AttributeValue current)
        {
            if (!current.IsAtMin || previous.IsAtMin)
            {
                return;
            }

            if (!string.Equals(definition.ProfileId, CCS_AttributesConstants.HealthAttributeId, StringComparison.Ordinal))
            {
                return;
            }

            PlayerDeath?.Invoke(new CCS_PlayerDeathEvent(definition.ProfileId, current));
            CCS_Logger.LogWarning(
                CCS_AttributesConstants.ModuleLogCategory,
                $"{name} reached death threshold on {definition.ProfileId}.");
        }

        #endregion
    }
}
