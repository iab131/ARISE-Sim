using UnityEngine;

public class SimButtonController : MonoBehaviour
{
    [SerializeField] private Transform simRoot;
    [SerializeField] private Transform arRoot;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnResetRobot()
    {
        Transform robot = SimRobotManager.FindRobot(simRoot);
        if (robot != null)
        {
            // reset transform
            robot.localPosition = SimRobotManager.robotRegPos;
            robot.localRotation = SimRobotManager.robotRegRot;

            // reset rigidbody (if present)
            Rigidbody rb = robot.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
    public void OnToggleCameraMode()
    {
        SimCameraController cam = simRoot.GetComponentInChildren<SimCameraController>(true);
        if (cam != null)
            cam.ToggleMode();
    }

    public void OnResetRobotAR()
    {
        Transform robot = SimRobotManager.FindRobot(arRoot);
        if (robot != null)
        {
            // reset transform
            robot.localPosition = SimRobotManager.robotRegPos;
            robot.localRotation = SimRobotManager.robotRegRot;

            // reset rigidbody (if present)
            Rigidbody rb = robot.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}
