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
    [SerializeField] private Transform codeRoot;

    [SerializeField] private BlockCodeExecutor blockCodeExecutor;

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
        //copy code
        if (View.AR == targetView || View.Simulation == targetView)
        {
            foreach (Transform child in codeRoot)
            {
                Destroy(child.gameObject);
            }
            Transform codingArea = blockCodingUIPanel.transform.Find("Coding Area");

            // 2. Instantiate a copy and parent it to codeRoot
            GameObject codingAreaCopy = Instantiate(codingArea.gameObject, codeRoot);

            // Optional: rename and activate if needed
            codingAreaCopy.name = "Code";
            codingAreaCopy.SetActive(true);

            blockCodeExecutor.codingArea = codingAreaCopy.transform;
        }
        BlockCodeExecutor.StopExecution();
        // Toggle physics mode
        bool isBuilding = (targetView == View.Building);
        SimMotor.SetBuildModeForAll(isBuilding);

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
        MotorLabelManager.Instance.SetAssignMode(false);

        SimRobotManager.SpawnSimulationRobot(targetView, buildingRoot, simulationRoot, arRoot);

    }
}
