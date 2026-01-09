using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class MotorLabelBrickForwarder : MonoBehaviour
{
    private MotorLabel parent;

    private void Awake()
    {
        parent = GetComponentInParent<MotorLabel>();
        if (parent == null)
            Debug.LogError($"{name}: No MotorLabel found in parents.");
    }

    private bool OverUI()
    {
        if (EventSystem.current == null) return false;

        // Works for mouse (Editor/Windows/WebGL). On iOS, touches are simulated as mouse too.
        return EventSystem.current.IsPointerOverGameObject();
    }

    private void OnMouseEnter()
    {
        if (OverUI()) return;
        parent?.HandleMouseEnter();
    }

    private void OnMouseExit()
    {
        parent?.HandleMouseExit();
    }

    private void OnMouseUpAsButton()
    {
        if (OverUI()) return;
        parent?.HandleMouseUp();
    }
}
