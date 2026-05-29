using CCS.Core;
using CCS.Survival.Environment.Hazards;
using CCS.Survival.Environment.VitalsZones;
using CCS.Survival.Interaction;
using CCS.Survival.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalDebugOverlay
// CATEGORY: Survival / Runtime / Survival / Debug
// PURPOSE: Temporary OnGUI debug overlay for survival vitals, hazards, and traversal test isolation.
// PLACEMENT: Attach to PF_CCS_Survival_BootstrapRoot or play mode test scenes. Not final UI.
// AUTHOR: James Schilz
// CREATED: 2026-05-27
// NOTES: Anchored top-right, compact panel. Phase 1H.4 hazard and test-mode readouts. No console logging.
// =============================================================================

namespace CCS.Survival
{
    public sealed class CCS_SurvivalDebugOverlay : MonoBehaviour
    {
        private const int SurvivalPanelLineCount = 15;

        #region Variables

        [Header("Debug Overlay")]
        [Tooltip("When enabled, draws the temporary survival vitals panel.")]
        [SerializeField] private bool showOverlay = true;

        [Tooltip("Survival module used for vitals readouts. Resolves from scene only when unset (temporary debug behavior).")]
        [SerializeField] private CCS_SurvivalModule survivalModule;

        [Tooltip("Optional player hazard receiver. Resolved once from scene when unset.")]
        [SerializeField] private CCS_SurvivalHazardReceiver playerHazardReceiver;

        [Tooltip("Optional traversal agent hazard receiver. Used when the player root is inactive during traversal tests.")]
        [SerializeField] private CCS_SurvivalHazardReceiver traversalHazardReceiver;

        [Tooltip("Optional player vitals modifier receiver. Resolved once from scene when unset.")]
        [SerializeField] private CCS_SurvivalVitalsZoneReceiver playerVitalsZoneReceiver;

        [Tooltip("Optional traversal agent vitals modifier receiver.")]
        [SerializeField] private CCS_SurvivalVitalsZoneReceiver traversalVitalsZoneReceiver;

        [Tooltip("Optional player interaction scanner for prompt readout.")]
        [SerializeField] private CCS_SurvivalInteractionScanner interactionScanner;

        [Header("Layout")]
        [Tooltip("Screen padding from top and right edges in pixels.")]
        [SerializeField] private float screenPadding = 16f;

        [Tooltip("Inner padding around overlay text in pixels.")]
        [SerializeField] private float panelPadding = 16f;

        [Tooltip("Panel width in pixels.")]
        [SerializeField] private float panelWidth = 280f;

        [Tooltip("Font size for overlay labels.")]
        [SerializeField] private int fontSize = 32;

        [Tooltip("Extra vertical space between stat lines in pixels.")]
        [SerializeField] private float lineSpacing = 8f;

        [Tooltip("Alpha for the semi-transparent dark background (0 = invisible, 1 = opaque).")]
        [Range(0.35f, 0.95f)]
        [SerializeField] private float backgroundAlpha = 0.72f;

        private GUIStyle labelStyle;
        private Texture2D backgroundTexture;
        private int cachedFontSize = -1;
        private float cachedBackgroundAlpha = -1f;
        private bool hazardReceiversResolved;
        private bool vitalsZoneReceiversResolved;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveSurvivalModuleReference();
            ResolveHazardReceiverReferences();
            ResolveVitalsZoneReceiverReferences();
            ResolveInteractionScannerReference();
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

        private void ResolveHazardReceiverReferences()
        {
            if (hazardReceiversResolved)
            {
                return;
            }

            if (!CCS_Validation.IsObjectValid(playerHazardReceiver)
                || !CCS_Validation.IsObjectValid(traversalHazardReceiver))
            {
                CCS_SurvivalHazardReceiver[] receivers = FindObjectsByType<CCS_SurvivalHazardReceiver>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

                for (int i = 0; i < receivers.Length; i++)
                {
                    CCS_SurvivalHazardReceiver receiver = receivers[i];
                    if (receiver == null)
                    {
                        continue;
                    }

                    if (!CCS_Validation.IsObjectValid(playerHazardReceiver)
                        && receiver.AppliesToSurvivalVitals)
                    {
                        playerHazardReceiver = receiver;
                        continue;
                    }

                    if (!CCS_Validation.IsObjectValid(traversalHazardReceiver)
                        && !receiver.AppliesToSurvivalVitals)
                    {
                        traversalHazardReceiver = receiver;
                    }
                }
            }

            hazardReceiversResolved = true;
        }

        private CCS_SurvivalHazardReceiver ResolveDisplayHazardReceiver()
        {
            ResolveHazardReceiverReferences();

            if (CCS_Validation.IsObjectValid(traversalHazardReceiver)
                && traversalHazardReceiver.isActiveAndEnabled)
            {
                return traversalHazardReceiver;
            }

            if (CCS_Validation.IsObjectValid(playerHazardReceiver)
                && playerHazardReceiver.isActiveAndEnabled)
            {
                return playerHazardReceiver;
            }

            if (CCS_Validation.IsObjectValid(traversalHazardReceiver))
            {
                return traversalHazardReceiver;
            }

            return playerHazardReceiver;
        }

        private void ResolveVitalsZoneReceiverReferences()
        {
            if (vitalsZoneReceiversResolved)
            {
                return;
            }

            if (!CCS_Validation.IsObjectValid(playerVitalsZoneReceiver)
                || !CCS_Validation.IsObjectValid(traversalVitalsZoneReceiver))
            {
                CCS_SurvivalVitalsZoneReceiver[] receivers = FindObjectsByType<CCS_SurvivalVitalsZoneReceiver>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

                for (int i = 0; i < receivers.Length; i++)
                {
                    CCS_SurvivalVitalsZoneReceiver receiver = receivers[i];
                    if (receiver == null)
                    {
                        continue;
                    }

                    if (!CCS_Validation.IsObjectValid(playerVitalsZoneReceiver)
                        && receiver.AppliesToSurvivalVitals)
                    {
                        playerVitalsZoneReceiver = receiver;
                        continue;
                    }

                    if (!CCS_Validation.IsObjectValid(traversalVitalsZoneReceiver)
                        && !receiver.AppliesToSurvivalVitals)
                    {
                        traversalVitalsZoneReceiver = receiver;
                    }
                }
            }

            vitalsZoneReceiversResolved = true;
        }

        private void ResolveInteractionScannerReference()
        {
            if (CCS_Validation.IsObjectValid(interactionScanner))
            {
                return;
            }

            GameObject playerRoot = GameObject.Find("CCS_PlayerRoot");
            if (playerRoot != null)
            {
                interactionScanner = playerRoot.GetComponent<CCS_SurvivalInteractionScanner>();
            }
        }

        private CCS_SurvivalVitalsZoneReceiver ResolveDisplayVitalsZoneReceiver()
        {
            ResolveVitalsZoneReceiverReferences();

            if (CCS_Validation.IsObjectValid(traversalVitalsZoneReceiver)
                && traversalVitalsZoneReceiver.isActiveAndEnabled)
            {
                return traversalVitalsZoneReceiver;
            }

            if (CCS_Validation.IsObjectValid(playerVitalsZoneReceiver)
                && playerVitalsZoneReceiver.isActiveAndEnabled)
            {
                return playerVitalsZoneReceiver;
            }

            if (CCS_Validation.IsObjectValid(traversalVitalsZoneReceiver))
            {
                return traversalVitalsZoneReceiver;
            }

            return playerVitalsZoneReceiver;
        }

        private bool TryGetTraversalIsolationActive(out bool isIsolationActive)
        {
            isIsolationActive = false;

            if (survivalModule is CCS_ISurvivalVitalsTestModeService testModeService)
            {
                isIsolationActive = testModeService.IsTraversalVitalsIsolationActive;
                return true;
            }

            if (!CCS_Validation.IsObjectValid(survivalModule))
            {
                return false;
            }

            CCS_RuntimeHost host = survivalModule.GetComponent<CCS_RuntimeHost>();
            if (!CCS_Validation.IsObjectValid(host))
            {
                host = survivalModule.GetComponentInParent<CCS_RuntimeHost>();
            }

            if (!CCS_Validation.IsObjectValid(host)
                || !host.IsRuntimeInitialized
                || !host.ServiceRegistry.TryGetService(out CCS_ISurvivalVitalsTestModeService resolvedTestMode))
            {
                return false;
            }

            isIsolationActive = resolvedTestMode.IsTraversalVitalsIsolationActive;
            return true;
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
            CCS_SurvivalHazardReceiver hazardReceiver = ResolveDisplayHazardReceiver();
            CCS_SurvivalVitalsZoneReceiver vitalsZoneReceiver = ResolveDisplayVitalsZoneReceiver();

            string hazardSummary = "None";
            string safeLabel = "No";
            string modifierSummary = "None";

            if (CCS_Validation.IsObjectValid(hazardReceiver))
            {
                hazardSummary = hazardReceiver.GetActiveHazardSummary();
                safeLabel = hazardReceiver.IsSafeZoneActive ? "Yes" : "No";
            }

            if (CCS_Validation.IsObjectValid(vitalsZoneReceiver))
            {
                modifierSummary = vitalsZoneReceiver.GetActiveModifierSummary();
            }

            bool testIsolationActive = false;
            if (TryGetTraversalIsolationActive(out bool isolationActive))
            {
                testIsolationActive = isolationActive;
            }

            float panelHeight = GetPanelHeight(SurvivalPanelLineCount);
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
            GUI.Label(new Rect(x, y, contentWidth, lineHeight), $"Temp {state.BodyTemperature:F1}C", labelStyle);
            y += lineHeight + lineSpacing;
            GUI.Label(new Rect(x, y, contentWidth, lineHeight), $"Exposure {state.Exposure:F1}", labelStyle);
            y += lineHeight + lineSpacing;
            GUI.Label(new Rect(x, y, contentWidth, lineHeight), $"Hazard {hazardSummary}", labelStyle);
            y += lineHeight + lineSpacing;
            GUI.Label(new Rect(x, y, contentWidth, lineHeight), $"Modifier {modifierSummary}", labelStyle);
            y += lineHeight + lineSpacing;
            GUI.Label(new Rect(x, y, contentWidth, lineHeight), $"Safe {safeLabel}", labelStyle);
            y += lineHeight + lineSpacing;
            GUI.Label(
                new Rect(x, y, contentWidth, lineHeight),
                $"Test Iso {(testIsolationActive ? "On" : "Off")}",
                labelStyle);
            y += lineHeight + lineSpacing;
            GUI.Label(new Rect(x, y, contentWidth, lineHeight), $"Interaction {ResolveInteractionPrompt()}", labelStyle);
            y += lineHeight + lineSpacing;
            GUI.Label(new Rect(x, y, contentWidth, lineHeight), ResolveInventoryOccupancyLine(), labelStyle);
            y += lineHeight + lineSpacing;
            GUI.Label(new Rect(x, y, contentWidth, lineHeight), $"Items {ResolveInventoryItemsLine()}", labelStyle);
            y += lineHeight + lineSpacing;
            GUI.Label(new Rect(x, y, contentWidth, lineHeight), state.IsAlive ? "Alive" : "Dead", labelStyle);
        }

        private string ResolveInteractionPrompt()
        {
            if (!CCS_Validation.IsObjectValid(interactionScanner))
            {
                return "None";
            }

            if (!interactionScanner.isActiveAndEnabled)
            {
                return "None";
            }

            return interactionScanner.CurrentInteractionPrompt;
        }

        private string ResolveInventoryOccupancyLine()
        {
            if (!TryResolveInventoryService(out CCS_ISurvivalInventoryService inventoryService))
            {
                return "Inventory N/A";
            }

            return $"Inventory {inventoryService.OccupiedSlotCount}/{inventoryService.SlotCount}";
        }

        private string ResolveInventoryItemsLine()
        {
            if (!TryResolveInventoryService(out CCS_ISurvivalInventoryService inventoryService))
            {
                return "None";
            }

            return inventoryService.BuildCompactItemSummary(4);
        }

        private bool TryResolveInventoryService(out CCS_ISurvivalInventoryService inventoryService)
        {
            inventoryService = null;

            if (CCS_Validation.IsObjectValid(survivalModule))
            {
                CCS_RuntimeHost host = survivalModule.GetComponent<CCS_RuntimeHost>();
                if (!CCS_Validation.IsObjectValid(host))
                {
                    host = survivalModule.GetComponentInParent<CCS_RuntimeHost>();
                }

                if (CCS_Validation.IsObjectValid(host)
                    && host.IsRuntimeInitialized
                    && host.ServiceRegistry.TryGetService(out inventoryService))
                {
                    return true;
                }
            }

            CCS_RuntimeHost sceneHost = FindFirstObjectByType<CCS_RuntimeHost>();
            if (CCS_Validation.IsObjectValid(sceneHost)
                && sceneHost.IsRuntimeInitialized
                && sceneHost.ServiceRegistry.TryGetService(out inventoryService))
            {
                return true;
            }

            return false;
        }

        private float GetLineHeight()
        {
            return fontSize + 6f;
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
