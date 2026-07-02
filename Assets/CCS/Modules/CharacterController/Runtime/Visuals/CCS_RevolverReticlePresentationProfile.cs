using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverReticlePresentationProfile
// CATEGORY: Modules / CharacterController / Runtime / Visuals
// PURPOSE: Tunable reticle reveal timing and screen-space stability settings.
// PLACEMENT: ScriptableObject under Profiles/Reticle/.
// AUTHOR: James Schilz
// CREATED: 2026-06-30
// NOTES: Presentation-only. Does not drive gameplay aim, fire, ammo, damage, or ownership.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public enum CCS_RevolverReticleRevealSource
    {
        StateReadiness = 0,
        AnimationEvent = 1,
        AnimationEventWithStateFallback = 2,
    }

    [CreateAssetMenu(
        fileName = "CCS_RevolverReticlePresentationProfile",
        menuName = "CCS/Character Controller/Revolver Reticle Presentation Profile",
        order = 21)]
    public sealed class CCS_RevolverReticlePresentationProfile : ScriptableObject
    {
        [SerializeField] private CCS_RevolverReticleRevealSource reticleRevealSource =
            CCS_RevolverReticleRevealSource.AnimationEvent;

        [SerializeField] private bool revealDuringDraw = false;

        [SerializeField] [Range(0.1f, 0.95f)] private float drawRevealNormalizedTime = 0.55f;

        [SerializeField] private float drawRevealLeadSeconds = 0.5f;

        [SerializeField] private float reticleFadeInSeconds = 0.08f;

        [SerializeField] private float reticleFadeOutSeconds = 0.05f;

        [SerializeField] private float screenSmoothTime = 0.06f;

        [SerializeField] private float maxScreenSnapPixelsPerFrame = 120f;

        [SerializeField] private float noHitFallbackDistance = 80f;

        [SerializeField] private float pitchSnapDeadZoneDegrees = 2f;

        [SerializeField] private bool holdLastValidTargetOnNoHit = true;

        [SerializeField] private float lastValidTargetHoldSeconds = 0.2f;

        public CCS_RevolverReticleRevealSource ReticleRevealSource => reticleRevealSource;

        public bool RevealDuringDraw => revealDuringDraw;

        public float DrawRevealNormalizedTime => drawRevealNormalizedTime;

        public float DrawRevealLeadSeconds => drawRevealLeadSeconds;

        public float ReticleFadeInSeconds => reticleFadeInSeconds;

        public float ReticleFadeOutSeconds => reticleFadeOutSeconds;

        public float ScreenSmoothTime => screenSmoothTime;

        public float MaxScreenSnapPixelsPerFrame => maxScreenSnapPixelsPerFrame;

        public float NoHitFallbackDistance => noHitFallbackDistance;

        public float PitchSnapDeadZoneDegrees => pitchSnapDeadZoneDegrees;

        public bool HoldLastValidTargetOnNoHit => holdLastValidTargetOnNoHit;

        public float LastValidTargetHoldSeconds => lastValidTargetHoldSeconds;

        public float ComputeDrawRevealNormalizedThreshold(float drawClipLengthSeconds)
        {
            float normalizedThreshold = drawRevealNormalizedTime;
            if (drawClipLengthSeconds > 0.01f && drawRevealLeadSeconds > 0f)
            {
                float leadThreshold = 1f - (drawRevealLeadSeconds / drawClipLengthSeconds);
                leadThreshold = Mathf.Clamp(leadThreshold, 0.1f, 0.95f);
                normalizedThreshold = Mathf.Min(normalizedThreshold, leadThreshold);
            }

            return Mathf.Clamp(normalizedThreshold, 0.1f, 0.95f);
        }
    }
}
