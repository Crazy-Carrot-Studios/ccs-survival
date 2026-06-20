using UnityEngine;

// =============================================================================
// SCRIPT: CCS_StaminaController
// CATEGORY: Modules / Attributes / Runtime / Components
// PURPOSE: Owns stamina value, drain/regen, and sprint permission for movement.
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

        [SerializeField] private float regenPerSecond = 12f;

        [SerializeField] private float sprintUnlockThreshold = 20f;

        private bool isSprintLocked;

        #endregion

        #region Properties

        public bool CanSprint => !isSprintLocked && CurrentStamina > 0f;

        public bool IsSprintLocked => isSprintLocked;

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

            if (CurrentStamina <= 0f && wantsSprint)
            {
                isSprintLocked = true;
            }

            if (isSprintLocked && CurrentStamina >= sprintUnlockThreshold)
            {
                isSprintLocked = false;
            }

            PushToContainer();
        }

        #endregion

        #region Private Methods

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
