using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioImGuiUtility
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Deferred IMGUI actions and layout error tracking for Fit Studio.
// PLACEMENT: Editor utility used by Equipment Fit Studio window.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Prevents DisplayDialog/popup side effects from breaking GUILayout stacks.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public static class CCS_EquipmentFitStudioImGuiUtility
    {
        private static readonly Queue<Action> DeferredActions = new Queue<Action>();

        private static bool subscribedToLog;

        public static bool HasLayoutError { get; private set; }

        public static void EnsureLogSubscription()
        {
            if (subscribedToLog)
            {
                return;
            }

            Application.logMessageReceived += HandleLogMessage;
            subscribedToLog = true;
        }

        public static void ClearLayoutError()
        {
            HasLayoutError = false;
        }

        public static void ResetForWindowClose()
        {
            DeferredActions.Clear();
            ClearLayoutError();
        }

        public static void EnqueueDeferredAction(Action action)
        {
            if (action == null)
            {
                return;
            }

            DeferredActions.Enqueue(action);
            EditorApplication.delayCall -= ProcessDeferredActions;
            EditorApplication.delayCall += ProcessDeferredActions;
        }

        public static void DrawVerticalScope(Action drawContent)
        {
            EditorGUILayout.BeginVertical();
            try
            {
                drawContent?.Invoke();
            }
            finally
            {
                EditorGUILayout.EndVertical();
            }
        }

        public static void DrawHorizontalScope(Action drawContent)
        {
            EditorGUILayout.BeginHorizontal();
            try
            {
                drawContent?.Invoke();
            }
            finally
            {
                EditorGUILayout.EndHorizontal();
            }
        }

        private static void ProcessDeferredActions()
        {
            EditorApplication.delayCall -= ProcessDeferredActions;
            while (DeferredActions.Count > 0)
            {
                Action action = DeferredActions.Dequeue();
                action?.Invoke();
            }
        }

        private static void HandleLogMessage(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception)
            {
                return;
            }

            if (condition.Contains("EndLayoutGroup")
                || condition.Contains("Invalid GUILayout state")
                || (condition.Contains("Getting control") && condition.Contains("position in a group")))
            {
                if (stackTrace.Contains("CCS_EquipmentFitStudioWindow"))
                {
                    HasLayoutError = true;
                }
            }
        }
    }
}
