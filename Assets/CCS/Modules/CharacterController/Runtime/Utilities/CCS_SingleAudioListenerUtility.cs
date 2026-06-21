using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SingleAudioListenerUtility
// CATEGORY: Modules / CharacterController / Runtime / Utilities
// PURPOSE: Ensures exactly one enabled AudioListener exists in the active scene.
// PLACEMENT: Runtime utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Prefers an explicitly supplied listener, then MainCamera, then any active camera.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_SingleAudioListenerUtility
    {
        #region Public Methods

        public static void EnsureSingleActiveListener(AudioListener preferredListener = null)
        {
            AudioListener[] listeners = Object.FindObjectsByType<AudioListener>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            if (listeners.Length == 0)
            {
                return;
            }

            AudioListener activeListener = ResolvePreferredListener(preferredListener, listeners);
            for (int i = 0; i < listeners.Length; i++)
            {
                AudioListener listener = listeners[i];
                if (listener == null)
                {
                    continue;
                }

                listener.enabled = listener == activeListener;
            }
        }

        public static AudioListener FindListenerOnCamera(Camera camera)
        {
            if (camera == null)
            {
                return null;
            }

            AudioListener listener = camera.GetComponent<AudioListener>();
            if (listener != null)
            {
                return listener;
            }

            return camera.gameObject.AddComponent<AudioListener>();
        }

        public static void SetListenerEnabled(AudioListener listener, bool enabled)
        {
            if (listener != null)
            {
                listener.enabled = enabled;
            }
        }

        #endregion

        #region Private Methods

        private static AudioListener ResolvePreferredListener(
            AudioListener preferredListener,
            AudioListener[] listeners)
        {
            if (IsListenerCandidate(preferredListener))
            {
                return preferredListener;
            }

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                AudioListener mainCameraListener = mainCamera.GetComponent<AudioListener>();
                if (IsListenerCandidate(mainCameraListener))
                {
                    return mainCameraListener;
                }
            }

            for (int i = 0; i < listeners.Length; i++)
            {
                AudioListener listener = listeners[i];
                if (!IsListenerCandidate(listener))
                {
                    continue;
                }

                Camera camera = listener.GetComponent<Camera>();
                if (camera != null && camera.enabled)
                {
                    return listener;
                }
            }

            for (int i = 0; i < listeners.Length; i++)
            {
                AudioListener listener = listeners[i];
                if (IsListenerCandidate(listener))
                {
                    return listener;
                }
            }

            return listeners[0];
        }

        private static bool IsListenerCandidate(AudioListener listener)
        {
            if (listener == null)
            {
                return false;
            }

            if (!listener.gameObject.activeInHierarchy)
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
