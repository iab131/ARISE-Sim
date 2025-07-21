using UnityEngine;

public class SimButtonController : MonoBehaviour
{
    [SerializeField] private Transform simRoot;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnResetRobot()
    {
        Transform robot = SimRobotManager.FindRobot(simRoot);
        if (robot != null) 
        robot.position = SimRobotManager.robotRegPos;
    }
    public void OnToggleCameraMode()
    {
        SimCameraController cam = simRoot.GetComponentInChildren<SimCameraController>(true);
        if (cam != null)
            cam.ToggleMode();
    }
}
