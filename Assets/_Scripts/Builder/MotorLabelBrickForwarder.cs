using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class MotorLabelBrickForwarder : MonoBehaviour
{
    private MotorLabel parent;
    private bool isHovered;

    private void Awake()
    {
        parent = GetComponentInParent<MotorLabel>();
    }

    private void Update()
    {
        if (IsPointerOverUI())
        {
            ClearHover();
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(GetPointerPosition());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider == GetComponent<Collider>())
            {
                if (!isHovered)
                {
                    isHovered = true;
                    parent?.HandleMouseEnter();
                   
                }

                if (IsPointerReleased())
                {
                    parent?.HandleMouseUp();
                }

                return;
            }
        }

        ClearHover();
    }

    private void ClearHover()
    {
        if (!isHovered) return;
        isHovered = false;
        parent?.HandleMouseExit();
    }

    private bool IsPointerOverUI()
    {
#if UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount > 0)
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
#endif
        return EventSystem.current.IsPointerOverGameObject();
    }

    private Vector3 GetPointerPosition()
    {
#if UNITY_IOS || UNITY_ANDROID
        return Input.touchCount > 0
            ? (Vector3)Input.GetTouch(0).position
            : Input.mousePosition;
#else
        return Input.mousePosition;
#endif
    }

    private bool IsPointerReleased()
    {
#if UNITY_IOS || UNITY_ANDROID
        return Input.touchCount > 0 &&
               Input.GetTouch(0).phase == TouchPhase.Ended;
#else
        return Input.GetMouseButtonUp(0);
#endif
    }
}
