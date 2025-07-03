using UnityEngine;
using UnityEngine.EventSystems;

public class PartItem : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private GameObject partPrefab; // assign the actual 3D prefab here
    public PartFilterUI.Category category;
    public void OnPointerDown(PointerEventData eventData)
    {
        // Spawn and start dragging the 3D model
        ControlManager.Instance.StartDraggingSpawnedPart(partPrefab);
    }
}
