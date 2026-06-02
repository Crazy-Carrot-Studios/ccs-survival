using System;

namespace CCS.Modules.Industry
{
    [Serializable]
    public sealed class CCS_IndustryJob
    {
        public string jobId = string.Empty;
        public string processId = string.Empty;
        public string workstationInstanceId = string.Empty;
        public float elapsedSeconds;
        public float durationSeconds;
        public bool isComplete;
    }
}
