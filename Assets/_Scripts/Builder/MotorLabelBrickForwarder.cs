using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MotorLabelBrickForwarder : MonoBehaviour
{
    private MotorLabel parent;

    private void Awake()
    {
        parent = transform.parent.GetComponentInParent<MotorLabel>();
    }

    private void OnMouseEnter() { parent?.HandleMouseEnter(); Debug.Log("enter"); }
    private void OnMouseExit() { parent?.HandleMouseExit(); }
    private void OnMouseUp() { parent?.HandleMouseUp(); }
}
