// =============================================================================
// SCRIPT: CCS_CharacterLookState
// CATEGORY: Modules / CharacterController / Runtime / Camera
// PURPOSE: Stores yaw/pitch look orientation for movement facing and camera follow.
// PLACEMENT: Owned by CCS_CharacterCameraController and movement service.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Yaw drives planar movement facing. Pitch drives camera pitch only.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public sealed class CCS_CharacterLookState
    {
        #region Variables

        private float yawDegrees;
        private float pitchDegrees;

        #endregion

        #region Public Methods

        public void SetOrientation(float yaw, float pitch)
        {
            yawDegrees = yaw;
            pitchDegrees = pitch;
        }

        public void AddLookDelta(float yawDelta, float pitchDelta, float minPitch, float maxPitch)
        {
            yawDegrees += yawDelta;
            pitchDegrees = ClampPitch(pitchDegrees + pitchDelta, minPitch, maxPitch);
        }

        public static float ClampPitch(float pitch, float minPitch, float maxPitch)
        {
            if (pitch < minPitch)
            {
                return minPitch;
            }

            if (pitch > maxPitch)
            {
                return maxPitch;
            }

            return pitch;
        }

        #endregion

        #region Properties

        public float YawDegrees => yawDegrees;

        public float PitchDegrees => pitchDegrees;

        #endregion
    }
}
