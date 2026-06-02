using UnityEngine;

namespace CCS.Modules.Shelter
{
    public sealed class CCS_FrontierWorkbenchInstance : MonoBehaviour
    {
        private CCS_FrontierHomesteadStructureService homesteadService;
        private CCS_WorkbenchDefinition workbenchDefinition;
        private string instanceId = string.Empty;
        private string campOwnerId = string.Empty;

        public string InstanceId => instanceId;

        public CCS_WorkbenchDefinition WorkbenchDefinition => workbenchDefinition;

        public Vector3 WorldPosition => transform.position;

        public void Initialize(
            CCS_FrontierHomesteadStructureService service,
            CCS_WorkbenchDefinition definition,
            string assignedInstanceId,
            string ownerId)
        {
            homesteadService = service;
            workbenchDefinition = definition;
            instanceId = assignedInstanceId ?? string.Empty;
            campOwnerId = ownerId ?? string.Empty;
        }

        public CCS_FrontierWorkbenchInstanceSaveState CaptureState()
        {
            return new CCS_FrontierWorkbenchInstanceSaveState
            {
                InstanceId = instanceId,
                WorkbenchDefinitionId = workbenchDefinition?.WorkbenchDefinitionId ?? string.Empty,
                Position = transform.position,
                RotationY = transform.eulerAngles.y,
                CampOwnerId = campOwnerId
            };
        }

        public void ApplySaveState(CCS_FrontierWorkbenchInstanceSaveState saveState)
        {
            if (saveState == null)
            {
                return;
            }

            campOwnerId = saveState.CampOwnerId ?? string.Empty;
        }

        private void OnDestroy()
        {
            homesteadService?.UnregisterWorkbenchInstance(this);
        }
    }
}
