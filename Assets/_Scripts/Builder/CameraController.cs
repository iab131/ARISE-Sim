using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraControl : MonoBehaviour
{


    #if UNITY_IOS || UNITY_ANDROID || UNITY_EDITOR
private bool rotatingByTouch;
private Vector2 lastTouchPos;
#endif

    public GameObject parentModel;
    public static CameraControl Instance { get; private set; }
    [Header("Sensitivity Settings")]
    [SerializeField] private float rotationSpeed = 1000f;
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float panSpeed = 1f;

    //[Header("Zoom Limits (CAD Style)")]
    //[SerializeField] private float minZoomDistance = 1f;
    //[SerializeField] private float maxZoomDistance = 100f;

    [Header("Fit View")]
    [SerializeField] private float defaultFieldOfView = 60f;

    private Vector3 mouseWorldPosStart;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    void Update()
{
    if (NavBarController.currentview != NavBarController.View.Building)
        return;

#if UNITY_IOS || UNITY_ANDROID || UNITY_EDITOR
    HandleTouchCamera();
#if !UNITY_EDITOR
    return; // ❗ prevents mouse input on phone
#endif
#endif

    // ===== DESKTOP CONTROLS (UNCHANGED) =====
    if (Input.GetMouseButton(1))
        CamOrbit();

    if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.F))
        FitToScreen();

    if (Input.GetMouseButtonDown(2))
        mouseWorldPosStart = GetPerspectivePos();

    if (Input.GetMouseButton(2))
        Pan();

    if (!IsPointerOverInteractiveUI())
        Zoom(Input.GetAxis("Mouse ScrollWheel"));
}


    private void CamOrbit()
    {
        float verticalInput = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
        float horizontalInput = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        transform.Rotate(Vector3.right, -verticalInput);
        transform.Rotate(Vector3.up, horizontalInput, Space.World);
    }

    private void Pan()
    {
        float moveX = -Input.GetAxis("Mouse X") * panSpeed;
        float moveY = -Input.GetAxis("Mouse Y") * panSpeed ;

        Camera cam = Camera.main;
        Vector3 right = cam.transform.right;
        Vector3 up = cam.transform.up;

        cam.transform.position += right * moveX + up * moveY;
    }

    private void Zoom(float zoomDiff)
    {
        if (zoomDiff == 0) return;

        Camera cam = Camera.main;

        float distance = zoomDiff * zoomSpeed;
        Vector3 newPos = cam.transform.position + cam.transform.forward * distance;

        // Apply position
        cam.transform.position = newPos;

        // ✅ Dynamically adjust clipping planes
        float zoomDistance = Vector3.Distance(cam.transform.position, parentModel.transform.position);

        cam.nearClipPlane = Mathf.Max(0.01f, zoomDistance * 0.01f);  // 1% of distance, min 0.01
        cam.farClipPlane = Mathf.Max(zoomDistance * 4f, 100f);       // 4x zoom distance, min 100
    }


    public void FitToScreen()
    {
        Camera.main.fieldOfView = defaultFieldOfView;
        Bounds bound = GetBound(parentModel);
        Vector3 boundSize = bound.size;
        float boundDiagonal = Mathf.Sqrt((boundSize.x * boundSize.x) + (boundSize.y * boundSize.y) + (boundSize.z * boundSize.z));
        float camDistanceToBoundCentre = boundDiagonal / (2.0f * Mathf.Tan(Camera.main.fieldOfView / 2.0f * Mathf.Deg2Rad));
        float camDistanceToBoundWithOffset = camDistanceToBoundCentre + boundDiagonal / 2.0f - (Camera.main.transform.position - transform.position).magnitude;
        transform.position = bound.center + (-transform.forward * camDistanceToBoundWithOffset);
        // Adjust clipping planes after fitting
        float zoomDistance = Vector3.Distance(Camera.main.transform.position, parentModel.transform.position);
        Camera.main.nearClipPlane = Mathf.Max(0.01f, zoomDistance * 0.01f);
        Camera.main.farClipPlane = Mathf.Max(zoomDistance * 4f, 100f);

    }

    private Bounds GetBound(GameObject parentGameObj)
    {
        Bounds bound = new Bounds(parentGameObj.transform.position, Vector3.zero);
        var rList = parentGameObj.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rList)
        {
            bound.Encapsulate(r.bounds);
        }
        return bound;
    }

    public Vector3 GetPerspectivePos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(transform.forward, 0f);
        plane.Raycast(ray, out float dist);
        return ray.GetPoint(dist);
    }

    bool IsPointerOverInteractiveUI()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult r in results)
        {
            if (r.gameObject.layer == LayerMask.NameToLayer("UI"))
                return true;
        }

        return false;
    }



  
void HandleTouchCamera()
{
#if UNITY_EDITOR
    // simulate 1-finger touch with mouse
    if (Input.GetMouseButtonDown(0))
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (!IsPartBeingDragged())
        {
            rotatingByTouch = true;
            lastTouchPos = Input.mousePosition;
        }
    }
    else if (Input.GetMouseButton(0) && rotatingByTouch)
    {
        Vector2 delta =
            (Vector2)Input.mousePosition - lastTouchPos;
        lastTouchPos = Input.mousePosition;

        RotateCameraByDelta(delta);
    }
    else if (Input.GetMouseButtonUp(0))
    {
        rotatingByTouch = false;
    }
#else
    if (Input.touchCount == 1)
    {
        Touch t = Input.GetTouch(0);

        if (EventSystem.current.IsPointerOverGameObject(t.fingerId))
            return;

        if (IsPartBeingDragged())
            return;

        if (t.phase == TouchPhase.Began)
        {
            rotatingByTouch = true;
            lastTouchPos = t.position;
        }
        else if (t.phase == TouchPhase.Moved && rotatingByTouch)
        {
            RotateCameraByDelta(t.deltaPosition);
        }
        else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
        {
            rotatingByTouch = false;
        }
    }
    else
    {
        HandleTwoFingerPanZoom();
    }
#endif
}




void HandleTwoFingerPanZoom()
{
    Touch t0 = Input.GetTouch(0);
    Touch t1 = Input.GetTouch(1);

    // ---- PAN ----
    Vector2 avgDelta = (t0.deltaPosition + t1.deltaPosition) * 0.5f;
    Vector3 pan =
        (-Camera.main.transform.right * avgDelta.x +
         -Camera.main.transform.up * avgDelta.y) * panSpeed * 0.01f;

    Camera.main.transform.position += pan;

    // ---- ZOOM ----
    Vector2 prev0 = t0.position - t0.deltaPosition;
    Vector2 prev1 = t1.position - t1.deltaPosition;

    float prevDist = Vector2.Distance(prev0, prev1);
    float currDist = Vector2.Distance(t0.position, t1.position);

    float diff = currDist - prevDist;
    Zoom(diff * 0.01f);
}



void RotateCameraByDelta(Vector2 delta)
{
    float speed = rotationSpeed * 0.0015f;

    transform.Rotate(Vector3.up, delta.x * speed, Space.World);
    transform.Rotate(Vector3.right, -delta.y * speed, Space.Self);
}

bool IsPartBeingDragged()
{
     return ControlManager.Instance != null &&
           ControlManager.Instance.IsDraggingPart;
}

}
