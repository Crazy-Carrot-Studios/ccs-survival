using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverVisualAimConvergenceSettings
// CATEGORY: Modules / Weapons / Runtime / Aiming
// PURPOSE: Tunable third-person visual aim convergence limits for equipped revolver.
// PLACEMENT: Runtime struct passed from CCS_RevolverController / visual definition.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Camera reticle remains gameplay truth. Experimental only — default OFF; can break hand fit.
// =============================================================================

namespace CCS.Modules.Weapons
{
    public readonly struct CCS_RevolverVisualAimConvergenceSettings
    {
        public CCS_RevolverVisualAimConvergenceSettings(
            bool enableVisualAimConvergence,
            float convergenceSpeed,
            float maxYawCorrectionDegrees,
            float maxPitchCorrectionDegrees,
            float maxRollCorrectionDegrees,
            float nearTargetDistance)
        {
            EnableVisualAimConvergence = enableVisualAimConvergence;
            ConvergenceSpeed = convergenceSpeed;
            MaxYawCorrectionDegrees = maxYawCorrectionDegrees;
            MaxPitchCorrectionDegrees = maxPitchCorrectionDegrees;
            MaxRollCorrectionDegrees = maxRollCorrectionDegrees;
            NearTargetDistance = nearTargetDistance;
        }

        public bool EnableVisualAimConvergence { get; }

        public float ConvergenceSpeed { get; }

        public float MaxYawCorrectionDegrees { get; }

        public float MaxPitchCorrectionDegrees { get; }

        public float MaxRollCorrectionDegrees { get; }

        public float NearTargetDistance { get; }

        public static CCS_RevolverVisualAimConvergenceSettings Default =>
            new CCS_RevolverVisualAimConvergenceSettings(
                false,
                18f,
                15f,
                10f,
                3f,
                2f);
    }
}
