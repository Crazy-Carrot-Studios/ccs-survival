using UnityEngine;

namespace CCS.Modules.Shelter
{
    public sealed class CCS_FrontierShelterInstance : MonoBehaviour
    {
        [SerializeField] private CCS_ShelterDefinition shelterDefinition;
        [SerializeField] private string instanceId = string.Empty;
        [SerializeField] private string campOwnerId = "ccs.survival.camp.player";

        private Renderer shelterRenderer;
        private CCS_FrontierShelterService shelterService;

        public CCS_ShelterDefinition ShelterDefinition => shelterDefinition;

        public string InstanceId => instanceId;

        public string CampOwnerId => campOwnerId;

        public Vector3 WorldPosition => transform.position;

        public void Initialize(CCS_FrontierShelterService service, CCS_ShelterDefinition definition, string assignedInstanceId)
        {
            shelterService = service;
            shelterDefinition = definition;
            instanceId = assignedInstanceId;
            shelterRenderer = GetComponent<Renderer>();
            ApplyVisual();
        }

        public CCS_FrontierShelterInstanceSaveState CaptureState()
        {
            Vector3 position = transform.position;
            return new CCS_FrontierShelterInstanceSaveState
            {
                InstanceId = instanceId,
                ShelterDefinitionId = shelterDefinition != null ? shelterDefinition.ShelterDefinitionId : string.Empty,
                Position = position,
                RotationY = transform.eulerAngles.y,
                CampOwnerId = campOwnerId
            };
        }

        public void ApplySaveState(CCS_FrontierShelterInstanceSaveState saveState)
        {
            if (saveState == null)
            {
                return;
            }

            campOwnerId = saveState.CampOwnerId ?? campOwnerId;
            transform.SetPositionAndRotation(
                saveState.Position,
                Quaternion.Euler(0f, saveState.RotationY, 0f));
            ApplyVisual();
        }

        private void ApplyVisual()
        {
            if (shelterRenderer == null)
            {
                return;
            }

            shelterRenderer.material.color = new Color(0.45f, 0.35f, 0.22f, 0.9f);
        }

        private void OnDestroy()
        {
            shelterService?.UnregisterShelterInstance(this);
        }
    }
}
