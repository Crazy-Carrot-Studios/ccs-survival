using UnityEngine;

namespace CCS.Modules.Shelter
{
    public sealed class CCS_FrontierShelterPlacementPreview : MonoBehaviour
    {
        private Renderer previewRenderer;

        public void EnsurePreviewObject(PrimitiveType primitiveType)
        {
            if (previewRenderer != null)
            {
                return;
            }

            GameObject previewObject = GameObject.CreatePrimitive(primitiveType);
            previewObject.name = "CCS_FrontierShelterPlacementPreview";
            previewObject.transform.SetParent(transform, false);
            Collider collider = previewObject.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            previewRenderer = previewObject.GetComponent<Renderer>();
            SetVisible(false);
        }

        public void UpdatePreview(Vector3 worldPosition, Quaternion worldRotation, Vector3 localScale, bool isValid, Color validColor, Color invalidColor)
        {
            if (previewRenderer == null)
            {
                return;
            }

            transform.SetPositionAndRotation(worldPosition, worldRotation);
            previewRenderer.transform.localScale = localScale;
            previewRenderer.material.color = isValid ? validColor : invalidColor;
            SetVisible(true);
        }

        public void SetVisible(bool visible)
        {
            if (previewRenderer != null)
            {
                previewRenderer.enabled = visible;
            }
        }
    }
}
