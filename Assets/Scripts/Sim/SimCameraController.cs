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

    void LateUpdate()
    {
        if (target == null) return;

        // Smoothly follow the target's position
        //currentTargetPos = Vector3.Lerp(currentTargetPos, target.position, positionSmoothSpeed * Time.deltaTime);
        currentTargetPos = target.position;

        // Calculate offset from rotation and distance
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, 0f, -distance);

        // Position and look
        transform.position = currentTargetPos + offset;
        transform.LookAt(currentTargetPos);
    }
}
