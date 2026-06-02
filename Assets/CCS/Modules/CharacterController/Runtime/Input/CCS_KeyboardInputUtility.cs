using UnityEngine;
using UnityEngine.InputSystem;

// =============================================================================
// SCRIPT: CCS_KeyboardInputUtility
// CATEGORY: Modules / CharacterController / Runtime / Input
// PURPOSE: Reads keyboard state via the Input System for dev hotkeys and fallbacks.
// PLACEMENT: Shared utility; no component required.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Use instead of UnityEngine.Input when active input handling is Input System only.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_KeyboardInputUtility
    {
        public static bool WasKeyPressedThisFrame(KeyCode keyCode)
        {
            if (!TryResolveKey(keyCode, out Key key))
            {
                return false;
            }

            Keyboard keyboard = Keyboard.current;
            return keyboard != null && keyboard[key].wasPressedThisFrame;
        }

        public static bool IsKeyHeld(KeyCode keyCode)
        {
            if (!TryResolveKey(keyCode, out Key key))
            {
                return false;
            }

            Keyboard keyboard = Keyboard.current;
            return keyboard != null && keyboard[key].isPressed;
        }

        public static bool IsEitherShiftHeld()
        {
            return IsKeyHeld(KeyCode.LeftShift) || IsKeyHeld(KeyCode.RightShift);
        }

        public static bool IsEitherControlHeld()
        {
            return IsKeyHeld(KeyCode.LeftControl) || IsKeyHeld(KeyCode.RightControl);
        }

        public static bool IsEitherAltHeld()
        {
            return IsKeyHeld(KeyCode.LeftAlt) || IsKeyHeld(KeyCode.RightAlt);
        }

        private static bool TryResolveKey(KeyCode keyCode, out Key key)
        {
            switch (keyCode)
            {
                case KeyCode.Alpha1: key = Key.Digit1; return true;
                case KeyCode.Alpha2: key = Key.Digit2; return true;
                case KeyCode.B: key = Key.B; return true;
                case KeyCode.G: key = Key.G; return true;
                case KeyCode.H: key = Key.H; return true;
                case KeyCode.I: key = Key.I; return true;
                case KeyCode.K: key = Key.K; return true;
                case KeyCode.M: key = Key.M; return true;
                case KeyCode.R: key = Key.R; return true;
                case KeyCode.T: key = Key.T; return true;
                case KeyCode.V: key = Key.V; return true;
                case KeyCode.W: key = Key.W; return true;
                case KeyCode.F1: key = Key.F1; return true;
                case KeyCode.F2: key = Key.F2; return true;
                case KeyCode.F3: key = Key.F3; return true;
                case KeyCode.F4: key = Key.F4; return true;
                case KeyCode.F5: key = Key.F5; return true;
                case KeyCode.F6: key = Key.F6; return true;
                case KeyCode.F7: key = Key.F7; return true;
                case KeyCode.F8: key = Key.F8; return true;
                case KeyCode.F9: key = Key.F9; return true;
                case KeyCode.F10: key = Key.F10; return true;
                case KeyCode.F11: key = Key.F11; return true;
                case KeyCode.F12: key = Key.F12; return true;
                case KeyCode.LeftShift: key = Key.LeftShift; return true;
                case KeyCode.RightShift: key = Key.RightShift; return true;
                case KeyCode.LeftControl: key = Key.LeftCtrl; return true;
                case KeyCode.RightControl: key = Key.RightCtrl; return true;
                case KeyCode.LeftAlt: key = Key.LeftAlt; return true;
                case KeyCode.RightAlt: key = Key.RightAlt; return true;
                case KeyCode.Escape: key = Key.Escape; return true;
                default:
                    key = Key.None;
                    return false;
            }
        }
    }
}
