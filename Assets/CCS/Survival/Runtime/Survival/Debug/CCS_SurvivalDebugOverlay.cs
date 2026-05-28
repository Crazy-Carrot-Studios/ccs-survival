using CCS.Core;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalDebugOverlay
// CATEGORY: Survival / Runtime / Survival / Debug
// PURPOSE: Temporary OnGUI debug overlay for Phase 1A survival vitals validation.
// PLACEMENT: Attach to any GameObject in play mode test scenes. Not final UI.
// AUTHOR: James Schilz
// CREATED: 2026-05-27
// NOTES: Anchored top-right, compact panel. Future production UI must remain clean and out of gameplay sightlines.
// =============================================================================

namespace CCS.Survival
{
    public sealed class CCS_SurvivalDebugOverlay : MonoBehaviour
    {
        #region Variables

        [Header("Debug Overlay")]
        [Tooltip("When enabled, draws the temporary survival vitals panel.")]
        [SerializeField] private bool showOverlay = true;

        [Tooltip("Survival module used for readouts. Resolves from scene only when unset (temporary debug behavior).")]
        [SerializeField] private CCS_SurvivalModule survivalModule;

        [Header("Layout")]
        [Tooltip("Screen padding from top and right edges in pixels.")]
        [SerializeField] private float screenPadding = 14f;

        [Tooltip("Inner padding around overlay text in pixels.")]
        [SerializeField] private float panelPadding = 8f;

        [Tooltip("Panel width in pixels.")]
        [SerializeField] private float panelWidth = 200f;

        [Tooltip("Font size for overlay labels.")]
        [SerializeField] private int fontSize = 16;

        [Tooltip("Extra vertical space between stat lines in pixels.")]
        [SerializeField] private float lineSpacing = 4f;

        [Tooltip("Alpha for the semi-transparent dark background (0 = invisible, 1 = opaque).")]
        [Range(0.35f, 0.95f)]
        [SerializeField] private float backgroundAlpha = 0.72f;

        private GUIStyle labelStyle;
        private Texture2D backgroundTexture;
        private int cachedFontSize = -1;
        private float cachedBackgroundAlpha = -1f;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveSurvivalModuleReference();
        }

        private void OnDestroy()
        {
            if (backgroundTexture != null)
            {
                Destroy(backgroundTexture);
                backgroundTexture = null;
            }
        }

        private void OnGUI()
        {
            if (!showOverlay)
            {
                return;
            }

            if (!CCS_Validation.IsObjectValid(survivalModule))
            {
                ResolveSurvivalModuleReference();
            }

            EnsureGuiResources();

            if (!CCS_Validation.IsObjectValid(survivalModule))
            {
                DrawMissingModulePanel();
                return;
            }

            DrawSurvivalPanel();
        }

        #endregion

        #region Private Methods

        private void ResolveSurvivalModuleReference()
        {
            if (CCS_Validation.IsObjectValid(survivalModule))
            {
                return;
            }

            survivalModule = FindFirstObjectByType<CCS_SurvivalModule>();
        }

        private void EnsureGuiResources()
        {
            if (cachedFontSize == fontSize && labelStyle != null)
            {
                if (Mathf.Approximately(cachedBackgroundAlpha, backgroundAlpha))
                {
                    return;
                }
            }

            cachedFontSize = fontSize;
            cachedBackgroundAlpha = backgroundAlpha;

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                richText = false,
                wordWrap = false,
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0)
            };

            if (backgroundTexture != null)
            {
                Destroy(backgroundTexture);
            }

            backgroundTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            backgroundTexture.SetPixel(0, 0, new Color(0.05f, 0.05f, 0.08f, backgroundAlpha));
            backgroundTexture.Apply();
        }

        private void DrawPanelBackground(Rect panelRect)
        {
            if (backgroundTexture == null)
            {
                return;
            }

            GUI.DrawTexture(panelRect, backgroundTexture, ScaleMode.StretchToFill);
        }

        private void DrawMissingModulePanel()
        {
            float panelHeight = GetPanelHeight(2);
            Rect panelRect = BuildTopRightRect(panelHeight);
            DrawPanelBackground(panelRect);

            float y = panelRect.y + panelPadding;
            float x = panelRect.x + panelPadding;
            float lineHeight = GetLineHeight();

            GUI.Label(new Rect(x, y, panelRect.width - panelPadding * 2f, lineHeight), "Survival Debug", labelStyle);
            y += lineHeight + lineSpacing;
            GUI.Label(new Rect(x, y, panelRect.width - panelPadding * 2f, lineHeight), "No CCS_SurvivalModule found.", labelStyle);
        }

        private void DrawSurvivalPanel()
        {
            CCS_SurvivalState state = survivalModule.CurrentState;
            float panelHeight = GetPanelHeight(6);
            Rect panelRect = BuildTopRightRect(panelHeight);
            DrawPanelBackground(panelRect);

            float y = panelRect.y + panelPadding;
            float x = panelRect.x + panelPadding;
            float lineHeight = GetLineHeight();
            float contentWidth = panelRect.width - panelPadding * 2f;

            GUI.Label(new Rect(x, y, contentWidth, lineHeight), "Survival", labelStyle);
            y += lineHeight + lineSpacing;
            GUI.Label(new Rect(x, y, contentWidth, lineHeight), $"HP {state.Health:F0}", labelStyle);
            y += lineHeight + lineSpacing;
            GUI.Label(new Rect(x, y, contentWidth, lineHeight), $"Food {state.Hunger:F0}", labelStyle);
            y += lineHeight + lineSpacing;
            GUI.Label(new Rect(x, y, contentWidth, lineHeight), $"Water {state.Thirst:F0}", labelStyle);
            y += lineHeight + lineSpacing;
            GUI.Label(new Rect(x, y, contentWidth, lineHeight), $"STM {state.Stamina:F0}", labelStyle);
            y += lineHeight + lineSpacing;
            GUI.Label(new Rect(x, y, contentWidth, lineHeight), state.IsAlive ? "Alive" : "Dead", labelStyle);
        }

        private float GetLineHeight()
        {
            return fontSize + 4f;
        }

        private float GetPanelHeight(int lineCount)
        {
            float lineHeight = GetLineHeight();
            float linesHeight = lineCount * lineHeight + Mathf.Max(0, lineCount - 1) * lineSpacing;
            return panelPadding * 2f + linesHeight;
        }

        private Rect BuildTopRightRect(float height)
        {
            float x = Screen.width - panelWidth - screenPadding;
            float y = screenPadding;
            return new Rect(x, y, panelWidth, height);
        }

        #endregion
    }
}
