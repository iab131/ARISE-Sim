using UnityEngine;

/// <summary>
/// Orbit camera that follows a target's position, but only rotates on user input.
/// Zooms with scroll wheel, looks at target.
/// </summary>
public class SimCameraController : MonoBehaviour
{
    public Transform target;
    
#if UNITY_IOS || UNITY_ANDROID
[Header("Touch Controls")]
[SerializeField] private float touchSensitivity = 0.2f;

private bool isTouchRotating = false;
private Vector2 lastTouchPosition;
#endif

    [Header("Rotation")]
    public float mouseSensitivity = 3f;
    public float minYAngle = 10f;
    public float maxYAngle = 80f;

    [Header("Zoom")]
    public float distance = 21f;
    public float zoomSpeed = 25f;
    public float minDistance = 7f;
    public float maxDistance = 70f;

    [Header("Smoothing")]
    public float positionSmoothSpeed = 10f;

    public enum CameraMode
    {
        Orbit,      // Current mouse-controlled camera
        GodView     // Fixed angle, no controls
    }
    public CameraMode currentMode = CameraMode.Orbit;

    [Header("God View")]
    public Vector3 godViewOffset = new Vector3(0f, 30f, -30f);
    public Vector3 godViewEulerAngles = new Vector3(45f, 0f, 0f);

    private float currentYaw = 0f;
    private float currentPitch = 45f;

    private Vector3 currentTargetPos;

    void Start()
    {
        if (target != null)
        {
            currentTargetPos = target.position;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleMode();
        }

if (currentMode != CameraMode.Orbit)
        return;

#if UNITY_IOS || UNITY_ANDROID
    HandleTouchRotation();
#else
    HandleMouseRotation();
#endif

    }


private void HandleMouseRotation()
{
    // Rotate
    if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
    {
        currentYaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        currentPitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        currentPitch = Mathf.Clamp(currentPitch, minYAngle, maxYAngle);
    }

    // Zoom
    float scroll = Input.GetAxis("Mouse ScrollWheel");
    distance -= scroll * zoomSpeed;
    distance = Mathf.Clamp(distance, minDistance, maxDistance);
}


#if UNITY_IOS || UNITY_ANDROID
private void HandleTouchRotation()
{
    if (Input.touchCount != 1)
    {
        isTouchRotating = false;
        return;
    }

    Touch touch = Input.GetTouch(0);

    if (touch.phase == TouchPhase.Began)
    {
        lastTouchPosition = touch.position;
        isTouchRotating = true;
    }
    else if (touch.phase == TouchPhase.Moved && isTouchRotating)
    {
        Vector2 delta = touch.position - lastTouchPosition;

        currentYaw += delta.x * touchSensitivity;
        currentPitch -= delta.y * touchSensitivity;
        currentPitch = Mathf.Clamp(currentPitch, minYAngle, maxYAngle);

        lastTouchPosition = touch.position;
    }
}
#endif

    void LateUpdate()
    {
        if (target == null) return;

        if (currentMode == CameraMode.Orbit)
        {
            //currentTargetPos = Vector3.Lerp(currentTargetPos, target.position, positionSmoothSpeed * Time.deltaTime);
            currentTargetPos = target.position;

            Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
            Vector3 offset = rotation * new Vector3(0f, 0f, -distance);

            transform.position = currentTargetPos + offset;
            transform.LookAt(currentTargetPos);
        }
        else if (currentMode == CameraMode.GodView)
        {
            transform.position = godViewOffset;
            transform.rotation = Quaternion.Euler(godViewEulerAngles);
        }

    }

    public void ToggleMode()
    {
        currentMode = currentMode == CameraMode.Orbit ? CameraMode.GodView : CameraMode.Orbit;
    }
}
