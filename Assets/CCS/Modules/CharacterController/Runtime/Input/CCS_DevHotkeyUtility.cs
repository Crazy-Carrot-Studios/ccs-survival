using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_DevHotkeyUtility
// CATEGORY: Modules / CharacterController / Runtime / Input
// PURPOSE: Centralizes dev-only keyboard chords and named playtest hotkey reads.
// PLACEMENT: Shared utility; no component required.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Gameplay input uses CCS_Survival_InputActions. Legacy UnityEngine.Input is banned.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_DevHotkeyUtility
    {
        public readonly struct DevHotkeyBinding
        {
            public DevHotkeyBinding(
                string ownerId,
                KeyCode keyCode,
                bool requiresShift = false,
                bool requiresControl = false,
                bool requiresAlt = false,
                bool allowShared = false)
            {
                OwnerId = ownerId;
                KeyCode = keyCode;
                RequiresShift = requiresShift;
                RequiresControl = requiresControl;
                RequiresAlt = requiresAlt;
                AllowShared = allowShared;
            }

            public string OwnerId { get; }

            public KeyCode KeyCode { get; }

            public bool RequiresShift { get; }

            public bool RequiresControl { get; }

            public bool RequiresAlt { get; }

            public bool AllowShared { get; }
        }

        private static readonly DevHotkeyBinding[] KnownBindings =
        {
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.F10),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.F11),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.F12),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.F7),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.F6),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.F6, requiresShift: true),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.Alpha1, allowShared: true),
            new DevHotkeyBinding("CCS_PlayerActiveItemDriver", KeyCode.Alpha1, allowShared: true),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.Alpha2),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.B),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.B, requiresControl: true),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.B, requiresControl: true, requiresShift: true),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.F4),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.F3),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.F2),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.F2, requiresShift: true),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.F1),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.F1, requiresShift: true),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.M, requiresControl: true, requiresAlt: true),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.T, requiresControl: true, requiresAlt: true),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.T, requiresAlt: true),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.V, allowShared: true),
            new DevHotkeyBinding("CCS_VendorDebugHud", KeyCode.V, allowShared: true),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.V, requiresShift: true, allowShared: true),
            new DevHotkeyBinding("CCS_VendorDebugHud", KeyCode.V, requiresShift: true, allowShared: true),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.V, requiresControl: true),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.H, allowShared: true),
            new DevHotkeyBinding("CCS_VendorDebugHud", KeyCode.H, allowShared: true),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.H, requiresShift: true),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.H, requiresControl: true, requiresShift: true),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.G, requiresControl: true, requiresShift: true),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.M, requiresControl: true, requiresShift: true),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.K, requiresControl: true, requiresShift: true),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.W, requiresControl: true),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.W, requiresControl: true, requiresShift: true),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.I, requiresControl: true, requiresShift: true),
            new DevHotkeyBinding("CCS_SaveDebugController", KeyCode.F5),
            new DevHotkeyBinding("CCS_SaveDebugController", KeyCode.F9),
            new DevHotkeyBinding("CCS_SaveDebugController", KeyCode.F8),
            new DevHotkeyBinding("CCS_PlayerActiveItemDriver", KeyCode.R),
            new DevHotkeyBinding("CCS_VendorDebugHud", KeyCode.Escape),
            new DevHotkeyBinding("CCS_BankingDebugHud", KeyCode.D, requiresShift: true, allowShared: true),
            new DevHotkeyBinding("CCS_BankingDebugHud", KeyCode.W, requiresShift: true, allowShared: true),
            new DevHotkeyBinding("CCS_BankingDebugHud", KeyCode.T, requiresShift: true, allowShared: true),
            new DevHotkeyBinding("CCS_BankingDebugHud", KeyCode.L, requiresShift: true, allowShared: true),
            new DevHotkeyBinding("CCS_BankingDebugHud", KeyCode.P, requiresShift: true, allowShared: true),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.U, requiresControl: true, requiresShift: true),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.O, requiresControl: true, requiresShift: true),
            new DevHotkeyBinding("CCS_PlaytestHud", KeyCode.T, requiresControl: true, requiresShift: true)
        };

        public static IReadOnlyList<DevHotkeyBinding> GetKnownBindings()
        {
            return KnownBindings;
        }

        public static bool WasUnmodifiedPressed(KeyCode keyCode)
        {
            return CCS_KeyboardInputUtility.WasKeyPressedThisFrame(keyCode)
                && !CCS_KeyboardInputUtility.IsEitherControlHeld()
                && !CCS_KeyboardInputUtility.IsEitherShiftHeld()
                && !CCS_KeyboardInputUtility.IsEitherAltHeld();
        }

        public static bool WasShiftPressed(KeyCode keyCode)
        {
            return CCS_KeyboardInputUtility.WasKeyPressedThisFrame(keyCode)
                && CCS_KeyboardInputUtility.IsEitherShiftHeld()
                && !CCS_KeyboardInputUtility.IsEitherControlHeld()
                && !CCS_KeyboardInputUtility.IsEitherAltHeld();
        }

        public static bool WasControlPressed(KeyCode keyCode)
        {
            return CCS_KeyboardInputUtility.WasKeyPressedThisFrame(keyCode)
                && CCS_KeyboardInputUtility.IsEitherControlHeld()
                && !CCS_KeyboardInputUtility.IsEitherShiftHeld()
                && !CCS_KeyboardInputUtility.IsEitherAltHeld();
        }

        public static bool WasAltPressed(KeyCode keyCode)
        {
            return CCS_KeyboardInputUtility.WasKeyPressedThisFrame(keyCode)
                && CCS_KeyboardInputUtility.IsEitherAltHeld()
                && !CCS_KeyboardInputUtility.IsEitherControlHeld()
                && !CCS_KeyboardInputUtility.IsEitherShiftHeld();
        }

        public static bool WasControlShiftPressed(KeyCode keyCode)
        {
            return CCS_KeyboardInputUtility.WasKeyPressedThisFrame(keyCode)
                && CCS_KeyboardInputUtility.IsEitherControlHeld()
                && CCS_KeyboardInputUtility.IsEitherShiftHeld()
                && !CCS_KeyboardInputUtility.IsEitherAltHeld();
        }

        public static bool WasControlAltPressed(KeyCode keyCode)
        {
            return CCS_KeyboardInputUtility.WasKeyPressedThisFrame(keyCode)
                && CCS_KeyboardInputUtility.IsEitherControlHeld()
                && CCS_KeyboardInputUtility.IsEitherAltHeld()
                && !CCS_KeyboardInputUtility.IsEitherShiftHeld();
        }

        public static bool WasControlAltShiftPressed(KeyCode keyCode)
        {
            return CCS_KeyboardInputUtility.WasKeyPressedThisFrame(keyCode)
                && CCS_KeyboardInputUtility.IsEitherControlHeld()
                && CCS_KeyboardInputUtility.IsEitherAltHeld()
                && CCS_KeyboardInputUtility.IsEitherShiftHeld();
        }

        public static bool WasTogglePlaytestHudPressed()
        {
            return CCS_KeyboardInputUtility.WasKeyPressedThisFrame(KeyCode.F10);
        }

        public static bool WasAdvancePlaytestStepPressed()
        {
            return CCS_KeyboardInputUtility.WasKeyPressedThisFrame(KeyCode.F11);
        }

        public static bool WasResetPlaytestStepsPressed()
        {
            return CCS_KeyboardInputUtility.WasKeyPressedThisFrame(KeyCode.F12);
        }

        public static bool WasForceTestDeathPressed()
        {
            return CCS_KeyboardInputUtility.WasKeyPressedThisFrame(KeyCode.F7);
        }

        public static bool WasSaveGamePressed()
        {
            return CCS_KeyboardInputUtility.WasKeyPressedThisFrame(KeyCode.F5);
        }

        public static bool WasLoadGamePressed()
        {
            return CCS_KeyboardInputUtility.WasKeyPressedThisFrame(KeyCode.F9);
        }

        public static bool WasDeleteSavePressed()
        {
            return CCS_KeyboardInputUtility.WasKeyPressedThisFrame(KeyCode.F8);
        }

        public static bool WasReloadActiveFirearmPressed()
        {
            return CCS_KeyboardInputUtility.WasKeyPressedThisFrame(KeyCode.R);
        }

        public static bool WasCloseVendorDebugPanelPressed()
        {
            return CCS_KeyboardInputUtility.WasKeyPressedThisFrame(KeyCode.Escape);
        }

        public static bool WasCloseBankingDebugPanelPressed()
        {
            return CCS_KeyboardInputUtility.WasKeyPressedThisFrame(KeyCode.Escape);
        }

        public static bool WasBankDepositPressed()
        {
            return WasShiftPressed(KeyCode.D);
        }

        public static bool WasBankWithdrawPressed()
        {
            return WasShiftPressed(KeyCode.W);
        }

        public static bool WasUpkeepPayPressed()
        {
            return WasShiftPressed(KeyCode.T);
        }

        public static bool WasBankBorrowLoanPressed()
        {
            return WasShiftPressed(KeyCode.L);
        }

        public static bool WasBankRepayLoanPressed()
        {
            return WasShiftPressed(KeyCode.P);
        }
    }
}
