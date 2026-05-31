using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TestSaveableState
// CATEGORY: Modules / SaveLoad / Runtime / Testing
// PURPOSE: Serializable payload for CCS_TestSaveableComponent persistence checks.
// PLACEMENT: Internal to development test saveable. Not gameplay content.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: JsonUtility-compatible data shape only.
// =============================================================================

namespace CCS.Modules.SaveLoad
{
    [Serializable]
    internal sealed class CCS_TestSaveableState
    {
        public string testString = string.Empty;

        public int testInteger;

        public string timestampUtc = string.Empty;
    }
}
