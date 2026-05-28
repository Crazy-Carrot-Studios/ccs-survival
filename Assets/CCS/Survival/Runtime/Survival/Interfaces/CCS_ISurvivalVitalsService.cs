using CCS.Core;

// =============================================================================
// SCRIPT: CCS_ISurvivalVitalsService
// CATEGORY: Survival / Runtime / Survival / Interfaces
// PURPOSE: Vitals service contract for survival state access and core survival actions (Phase 1A).
// PLACEMENT: Runtime interface. Implemented by CCS_SurvivalModule. Registered on CCS_ServiceRegistry when host is available.
// AUTHOR: James Schilz
// CREATED: 2026-05-27
// NOTES: Extends foundation marker CCS_ISurvivalService. Planning doc name CCS_ISurvivalService maps to this vitals contract.
// =============================================================================

namespace CCS.Survival
{
    public interface CCS_ISurvivalVitalsService : CCS_ISurvivalService
    {
        CCS_SurvivalState CurrentState { get; }

        float CurrentStamina { get; }

        bool IsAlive { get; }

        bool HasStamina(float requiredAmount);

        bool TryConsumeStamina(float amount);

        void RestoreStamina(float amount);

        void ApplyDamage(float amount);

        void RestoreHealth(float amount);

        void SetBodyTemperature(float bodyTemperature);

        void SetExposure(float exposureValue);

        void ConsumeFood(float nutritionValue);

        void ConsumeWater(float hydrationValue);

        void DrainHunger(float amount);

        void RestoreHunger(float amount);

        void DrainThirst(float amount);

        void RestoreThirst(float amount);

        void DrainStamina(float amount);

        void AddExposure(float amount);

        void ReduceExposure(float amount);

        void ModifyBodyTemperature(float delta);

        void SetHunger(float hungerValue);

        void SetThirst(float thirstValue);

        void SetStamina(float staminaValue);

        void SetHealth(float healthValue);

        void Kill();

        void Respawn();
    }
}
