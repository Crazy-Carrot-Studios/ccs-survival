using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioStyles
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Shared GUI styles for Equipment Fit Studio window.
// PLACEMENT: Editor-only styling helper.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: IMGUI styles initialized lazily by the window.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public static class CCS_EquipmentFitStudioStyles
    {
        public static GUIStyle TitleLabel;

        public static GUIStyle HintLabel;

        public static GUIStyle StatusOkLabel;

        public static GUIStyle StatusWarnLabel;

        public static GUIStyle StatusErrorLabel;

        public static bool IsInitialized;

        public static void EnsureInitialized()
        {
            if (IsInitialized)
            {
                return;
            }

            TitleLabel = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
            };

            HintLabel = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                fontStyle = FontStyle.Italic,
            };

            StatusOkLabel = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = new Color(0.2f, 0.75f, 0.35f) },
            };

            StatusWarnLabel = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = new Color(0.95f, 0.75f, 0.15f) },
            };

            StatusErrorLabel = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = new Color(0.95f, 0.25f, 0.25f) },
            };

            IsInitialized = true;
        }
    }
}
