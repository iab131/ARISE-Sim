using UnityEngine;

public class NavBarController : MonoBehaviour
{
    public enum View
    {
        BlockCoding,
        Building,
        Simulation,
        AR
    }

    [Header("Default View on Start")]
    [SerializeField] private View defaultView = View.BlockCoding;

    [Header("2D UI Panels")]
    [SerializeField] private GameObject blockCodingUIPanel;
    [SerializeField] private GameObject buildingUIPanel;
    [SerializeField] private GameObject simulationUIPanel;
    [SerializeField] private GameObject arUIPanel;

    [Header("3D View Roots")]
    [SerializeField] private GameObject buildingRoot;
    [SerializeField] private GameObject simulationRoot;
    [SerializeField] private GameObject arRoot;

    private void Start()
    {
        SwitchToView(defaultView);
    }

    public void ShowBlockCoding() => SwitchToView(View.BlockCoding);
    public void ShowBuilding() => SwitchToView(View.Building);
    public void ShowSimulation() => SwitchToView(View.Simulation);
    public void ShowAR() => SwitchToView(View.AR);

    private void SwitchToView(View targetView)
    {
        // Toggle physics mode
        bool isBuilding = (targetView == View.Building);
        MotorHubPhysicsToggle.SetBuildModeForAll(isBuilding);

        // 2D UI Panels
        blockCodingUIPanel?.SetActive(targetView == View.BlockCoding);
        buildingUIPanel?.SetActive(targetView == View.Building);
        simulationUIPanel?.SetActive(targetView == View.Simulation);
        arUIPanel?.SetActive(targetView == View.AR);

        // 3D Roots
        buildingRoot?.SetActive(targetView == View.Building);
        simulationRoot?.SetActive(targetView == View.Simulation);
        arRoot?.SetActive(targetView == View.AR);

        // Motor assignment UI
        MotorLabelManager.Instance?.SetAssignMode(false);

        // Copy robot when entering Simulation or AR mode
        if (targetView == View.Simulation || targetView == View.AR)
        {
            // Find robot in builder
            Transform robot = FindRobot(buildingRoot.transform);
            if (robot != null)
            {
                // Decide the parent to copy to
                Transform parent = (targetView == View.Simulation) ? simulationRoot.transform : arRoot.transform;

                // Destroy existing robot if any
                Transform existing = FindRobot(parent);
                if (existing != null)
                {
                    Destroy(existing.gameObject);
                }

                // Instantiate a copy
                GameObject robotCopy = Instantiate(robot.gameObject, parent);
                robotCopy.name = robot.name; // Keep original name
                robotCopy.transform.localPosition = robot.localPosition;
                robotCopy.transform.localRotation = robot.localRotation;
                robotCopy.transform.localScale = robot.localScale;

                // add rigidboy
                Rigidbody rb = robotCopy.AddComponent<Rigidbody>();
                rb.mass = 10f;
                rb.useGravity = true;
                rb.isKinematic = false;
                rb.interpolation = RigidbodyInterpolation.None;
                rb.constraints = RigidbodyConstraints.None;
                
                MotorHubPhysicsToggle.SetConnectedBodyForAll(rb);

                if (targetView == View.Simulation)
                {
                    SimCameraController simCam = simulationRoot.GetComponentInChildren<SimCameraController>(true);
                    if (simCam != null)
                    {
                        simCam.target = robotCopy.transform;
                    }
                }
            }
        }
    }
    private Transform FindRobot(Transform root)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.Contains("Robot"))
                return child;
        }
        return null;
    }

}
