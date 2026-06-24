using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioStyles
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Shared GUI styles and colored action buttons for Fit Studio.
// PLACEMENT: Editor-only styling helper.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.8 professional UI pass. All button colors centralized here.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public enum CCS_EquipmentFitStudioButtonKind
    {
        Neutral = 0,
        SpawnPreview = 1,
        ClearPreview = 2,
        Capture = 3,
        SaveProfile = 4,
        ApplyProfile = 5,
        TestFit = 6,
        Validate = 7,
        Warning = 8,
    }

    public static class CCS_EquipmentFitStudioStyles
    {
        public static GUIStyle TitleLabel;

        public static GUIStyle HeaderMetaLabel;

        public static GUIStyle SectionLabel;

        public static GUIStyle HintLabel;

        public static GUIStyle ModeGuideTitle;

        public static GUIStyle ModeGuideBody;

        public static GUIStyle StatusOkLabel;

        public static GUIStyle StatusWarnLabel;

        public static GUIStyle StatusErrorLabel;

        public static GUIStyle PreviewFrame;

        public static GUIStyle PreviewOverlayLabel;

        public static GUIStyle WorkflowStepActive;

        public static GUIStyle WorkflowStepInactive;

        public static GUIStyle BottomBarBackground;

        public static GUIStyle MainActionButton;

        public static GUIStyle BottomActionButton;

        public static bool IsInitialized;

        public static void EnsureInitialized()
        {
            if (IsInitialized)
            {
                return;
            }

            TitleLabel = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
            };

            HeaderMetaLabel = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight,
            };

            SectionLabel = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 11,
            };

            HintLabel = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                fontStyle = FontStyle.Italic,
                fontSize = 11,
            };

            ModeGuideTitle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
            };

            ModeGuideBody = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                fontSize = 11,
                richText = true,
            };

            StatusOkLabel = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = new Color(0.25f, 0.85f, 0.45f) },
                fontStyle = FontStyle.Bold,
            };

            StatusWarnLabel = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = new Color(0.95f, 0.78f, 0.2f) },
                fontStyle = FontStyle.Bold,
            };

            StatusErrorLabel = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = new Color(0.95f, 0.3f, 0.3f) },
                fontStyle = FontStyle.Bold,
            };

            PreviewFrame = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(4, 4, 4, 4),
            };

            PreviewOverlayLabel = new GUIStyle(EditorStyles.whiteLabel)
            {
                fontSize = 11,
                normal = { textColor = new Color(0.92f, 0.94f, 0.98f) },
            };

            WorkflowStepActive = new GUIStyle(EditorStyles.helpBox)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.35f, 0.85f, 1f) },
            };

            WorkflowStepInactive = new GUIStyle(EditorStyles.helpBox)
            {
                fontSize = 10,
            };

            BottomBarBackground = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(8, 8, 6, 6),
            };

            MainActionButton = new GUIStyle(GUI.skin.button)
            {
                fixedHeight = 32f,
                fontStyle = FontStyle.Bold,
            };

            BottomActionButton = new GUIStyle(GUI.skin.button)
            {
                fixedHeight = 36f,
                fontStyle = FontStyle.Bold,
            };

            IsInitialized = true;
        }

        public static bool DrawMainActionButton(
            string label,
            CCS_EquipmentFitStudioButtonKind kind,
            string tooltip,
            params GUILayoutOption[] options)
        {
            return DrawColoredButton(label, kind, tooltip, MainActionButton, options);
        }

        public static bool DrawBottomActionButton(
            string label,
            CCS_EquipmentFitStudioButtonKind kind,
            string tooltip,
            bool enabled,
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginDisabledGroup(!enabled);
            bool clicked = DrawBottomActionButton(label, kind, enabled ? tooltip : tooltip + " (not ready yet)", options);
            EditorGUI.EndDisabledGroup();
            return enabled && clicked;
        }

        public static bool DrawBottomActionButton(
            string label,
            CCS_EquipmentFitStudioButtonKind kind,
            string tooltip,
            params GUILayoutOption[] options)
        {
            return DrawColoredButton(label, kind, tooltip, BottomActionButton, options);
        }

        public static Color GetButtonTint(CCS_EquipmentFitStudioButtonKind kind)
        {
            switch (kind)
            {
                case CCS_EquipmentFitStudioButtonKind.SpawnPreview:
                    return new Color(0.35f, 0.72f, 0.88f);
                case CCS_EquipmentFitStudioButtonKind.ClearPreview:
                    return new Color(0.55f, 0.58f, 0.62f);
                case CCS_EquipmentFitStudioButtonKind.Capture:
                    return new Color(0.92f, 0.72f, 0.28f);
                case CCS_EquipmentFitStudioButtonKind.SaveProfile:
                    return new Color(0.35f, 0.78f, 0.45f);
                case CCS_EquipmentFitStudioButtonKind.ApplyProfile:
                    return new Color(0.35f, 0.62f, 0.92f);
                case CCS_EquipmentFitStudioButtonKind.TestFit:
                    return new Color(0.28f, 0.82f, 0.86f);
                case CCS_EquipmentFitStudioButtonKind.Validate:
                    return new Color(0.58f, 0.48f, 0.92f);
                case CCS_EquipmentFitStudioButtonKind.Warning:
                    return new Color(0.92f, 0.48f, 0.32f);
                default:
                    return new Color(0.62f, 0.65f, 0.7f);
            }
        }

        private static bool DrawColoredButton(
            string label,
            CCS_EquipmentFitStudioButtonKind kind,
            string tooltip,
            GUIStyle style,
            params GUILayoutOption[] options)
        {
            Color previous = GUI.backgroundColor;
            GUI.backgroundColor = GetButtonTint(kind);
            GUIContent content = new GUIContent(label, tooltip);
            bool clicked = GUILayout.Button(content, style, options);
            GUI.backgroundColor = previous;
            return clicked;
        }
    }
}
