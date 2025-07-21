using UnityEngine;

/// <summary>
/// Orbit camera that follows a target's position, but only rotates on user input.
/// Zooms with scroll wheel, looks at target.
/// </summary>
public class SimCameraController : MonoBehaviour
{
    public Transform target;

    [Header("Rotation")]
    public float mouseSensitivity = 3f;
    public float minYAngle = 10f;
    public float maxYAngle = 80f;

    [Header("Zoom")]
    public float distance = 2000f;
    public float zoomSpeed = 1000f;
    public float minDistance = 700f;
    public float maxDistance = 7000f;

    [Header("Smoothing")]
    public float positionSmoothSpeed = 10f;

    public enum CameraMode
    {
        Orbit,      // Current mouse-controlled camera
        GodView     // Fixed angle, no controls
    }
    public CameraMode currentMode = CameraMode.Orbit;

    [Header("God View")]
    public Vector3 godViewOffset = new Vector3(0f, 3000f, -3000f);
    public Vector3 godViewEulerAngles = new Vector3(60f, 0f, 0f);

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

        if (currentMode == CameraMode.Orbit)
        {
            // Right mouse drag to rotate
            if (Input.GetMouseButton(1))
            {
                currentYaw += Input.GetAxis("Mouse X") * mouseSensitivity;
                currentPitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
                currentPitch = Mathf.Clamp(currentPitch, minYAngle, maxYAngle);
            }

            // Scroll to zoom
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

    }

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
            transform.position = target.position + godViewOffset;
            transform.rotation = Quaternion.Euler(godViewEulerAngles);
        }

    }

    public void ToggleMode()
    {
        currentMode = currentMode == CameraMode.Orbit ? CameraMode.GodView : CameraMode.Orbit;
    }
}
