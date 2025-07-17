using UnityEngine;

public class GizmoHandler : MonoBehaviour
{
    public enum ActionType { RotateLeft, RotateRight, MoveUp, MoveDown }

    [SerializeField] private ActionType actionType;
    [SerializeField] private Material highlightMaterial;

    private Material defaultMaterial;
    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();

        if (rend != null)
        {
            // Cache the material that's already assigned in the prefab
            defaultMaterial = rend.material;
        }
    }

    private void OnMouseEnter()
    {
        if (rend != null && highlightMaterial != null)
        {
            rend.material = highlightMaterial;
        }
    }

    private void OnMouseExit()
    {
        if (rend != null && defaultMaterial != null)
        {
            rend.material = defaultMaterial;
        }
    }

    private void OnMouseDown()
    {
        GizmoManager.Instance.OnHandleClicked(actionType);
    }
}
