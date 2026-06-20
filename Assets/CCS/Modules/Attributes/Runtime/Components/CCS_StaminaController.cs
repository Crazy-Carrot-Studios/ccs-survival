using UnityEngine;

// =============================================================================
// SCRIPT: CCS_StaminaController
// CATEGORY: Modules / Attributes / Runtime / Components
// PURPOSE: Owns stamina value, drain/regen, sprint permission, and exhaustion movement.
// PLACEMENT: Player root alongside CCS_AttributeContainer.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: CharacterController reports sprint intent; HUD reads container snapshots only.
// =============================================================================

namespace CCS.Modules.Attributes
{
    public sealed class CCS_StaminaController : MonoBehaviour
    {
        #region Variables

        [SerializeField] private CCS_AttributeContainer attributeContainer;

        [SerializeField] private CCS_AttributeDefinition staminaDefinition;

        [Header("Stamina Tuning")]
        [SerializeField] private float drainPerSecond = 18f;

        [SerializeField] private float regenPerSecond = 6f;

        [SerializeField] private float walkRecoveryThreshold = 35f;

        [SerializeField] private float sprintUnlockThreshold = 50f;

        [SerializeField] private float exhaustedWalkMultiplier = 0.5f;

        private bool isSprintLocked;

        private bool isExhausted;

        #endregion

        #region Properties

        public bool CanSprint => !isSprintLocked && CurrentStamina > 0f;

        public bool IsSprintLocked => isSprintLocked;

        public bool IsExhausted => isExhausted;

        public float MovementSpeedMultiplier => isExhausted ? exhaustedWalkMultiplier : 1f;

        public float CurrentStamina { get; private set; }

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            if (attributeContainer == null)
            {
                attributeContainer = GetComponent<CCS_AttributeContainer>();
            }

            SyncCurrentFromContainer();
            UpdateStateFlags();
        }

        #endregion

        #region Public Methods

        public void ReportMovementState(bool sprintIntentHeld, bool isMoving, float deltaTime)
        {
            if (deltaTime <= 0f || attributeContainer == null)
            {
                return;
            }

            SyncCurrentFromContainer();

            bool wantsSprint = sprintIntentHeld && isMoving;
            if (wantsSprint && CanSprint)
            {
                ApplyDelta(-drainPerSecond * deltaTime);
            }
            else
            {
                ApplyDelta(regenPerSecond * deltaTime);
            }

            UpdateStateFlags();
            PushToContainer();
        }

        #endregion

        #region Private Methods

        private void UpdateStateFlags()
        {
            if (CurrentStamina <= 0f)
            {
                isExhausted = true;
                isSprintLocked = true;
            }

            if (isExhausted && CurrentStamina >= walkRecoveryThreshold)
            {
                isExhausted = false;
            }

            if (isSprintLocked && CurrentStamina >= sprintUnlockThreshold)
            {
                isSprintLocked = false;
            }
        }

        private void SyncCurrentFromContainer()
        {
            string attributeId = ResolveStaminaAttributeId();
            if (attributeContainer.TryGetValue(attributeId, out CCS_AttributeValue value))
            {
                CurrentStamina = value.Current;
                return;
            }

            CurrentStamina = CCS_AttributesConstants.StaminaDefaultMax;
        }

        private void ApplyDelta(float delta)
        {
            float max = ResolveStaminaMax();
            CurrentStamina = Mathf.Clamp(CurrentStamina + delta, 0f, max);
        }

        private void PushToContainer()
        {
            string attributeId = ResolveStaminaAttributeId();
            if (staminaDefinition != null)
            {
                attributeContainer.SetValue(staminaDefinition, CurrentStamina);
                return;
            }

            attributeContainer.SetValue(attributeId, CurrentStamina);
        }

        private float ResolveStaminaMax()
        {
            string attributeId = ResolveStaminaAttributeId();
            if (attributeContainer.TryGetValue(attributeId, out CCS_AttributeValue value))
            {
                return value.Max;
            }

            return CCS_AttributesConstants.StaminaDefaultMax;
        }

        private string ResolveStaminaAttributeId()
        {
            if (staminaDefinition != null && !string.IsNullOrWhiteSpace(staminaDefinition.ProfileId))
            {
                return staminaDefinition.ProfileId;
            }

            return CCS_AttributesConstants.StaminaAttributeId;
        }

        #endregion
    }
}
