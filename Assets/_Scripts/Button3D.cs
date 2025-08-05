using UnityEngine;
using UnityEngine.Events;

/**
 * A simple 3D button script that changes color on hover and click,
 * and invokes an event when clicked.
 */
[RequireComponent(typeof(Renderer), typeof(Collider))]
public class Button3D : MonoBehaviour
{
    [Header("Button Colors")]
    [Tooltip("Default color of the button")]
    public Color defaultColor = Color.white;

    [Tooltip("Color when hovered over")]
    public Color hoverColor = Color.yellow;

    [Tooltip("Color when pressed")]
    public Color pressedColor = Color.gray;

    [Header("On Click Event")]
    [Tooltip("Function to call when button is clicked")]
    public UnityEvent onClick;

    private Renderer rend;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        rend.material.color = defaultColor;
    }

    private void OnMouseEnter()
    {
        rend.material.color = hoverColor;
    }

    private void OnMouseExit()
    {
        rend.material.color = defaultColor;
    }

    private void OnMouseDown()
    {
        rend.material.color = pressedColor;
    }

    private void OnMouseUp()
    {
        rend.material.color = defaultColor;
        onClick?.Invoke();
    }
}
