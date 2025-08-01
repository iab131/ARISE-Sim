using UnityEngine;
using UnityEngine.EventSystems;

public class CameraControl : MonoBehaviour
{
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
        {
            return;
        }
        if (Input.GetMouseButton(1)) // Right click to orbit
            CamOrbit();

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.F))
            FitToScreen();

        if (Input.GetMouseButtonDown(2)) // Middle click pan start
            mouseWorldPosStart = GetPerspectivePos();

        if (Input.GetMouseButton(2)) // Middle click pan
            Pan();

        if (!EventSystem.current.IsPointerOverGameObject())
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
}
