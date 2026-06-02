using UnityEngine;

namespace CCS.Modules.Industry
{
    public sealed class CCS_IndustryWorkstation : MonoBehaviour
    {
        private CCS_IndustryService industryService;
        private string workstationInstanceId = string.Empty;
        private string workstationRoleId = string.Empty;
        private string campOwnerId = string.Empty;

        public string WorkstationInstanceId => workstationInstanceId;

        public string WorkstationRoleId => workstationRoleId;

        public Vector3 WorldPosition => transform.position;

        public void Initialize(
            CCS_IndustryService service,
            string instanceId,
            string roleId,
            string ownerId)
        {
            industryService = service;
            workstationInstanceId = instanceId ?? string.Empty;
            workstationRoleId = roleId ?? string.Empty;
            campOwnerId = ownerId ?? string.Empty;
            industryService?.RegisterWorkstation(this);
        }

        public void ApplySaveState(string activeProcessId, float elapsedSeconds)
        {
            industryService?.RestoreWorkstationJob(workstationInstanceId, activeProcessId, elapsedSeconds);
        }

        public CCS_IndustryJob CaptureActiveJob()
        {
            return industryService?.CaptureWorkstationJob(workstationInstanceId);
        }

        private void OnDestroy()
        {
            industryService?.UnregisterWorkstation(this);
        }
    }
}
