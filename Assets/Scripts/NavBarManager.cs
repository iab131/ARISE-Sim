using UnityEngine;
using UnityEngine.UI;

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

    [Header("2D UI Panel (Block Coding Only)")]
    public GameObject blockCodingUIPanel;
    public GameObject buildingUIPanel;
    public GameObject simulationUIPanel;
    public GameObject arUIPanel;

    [Header("3D Panels / Root Objects")]
    public GameObject buildingRoot;
    public GameObject simulationRoot;
    public GameObject arRoot;

    private void Start()
    {
      

        // Show default view
        switch (defaultView)
        {
            case View.BlockCoding: ShowBlockCoding(); break;
            case View.Building: ShowBuilding(); break;
            case View.Simulation: ShowSimulation(); break;
            case View.AR: ShowAR(); break;
        }
    }

    public void ShowBlockCoding()
    {
        SetActiveView(blockCodingUIPanel, true);
        SetActiveView(buildingUIPanel, false);
        SetActiveView(simulationUIPanel, false);
        SetActiveView(arUIPanel, false);
        SetActiveView(buildingRoot, false);
        SetActiveView(simulationRoot, false);
        SetActiveView(arRoot, false);
        MotorLabelManager.Instance?.SetAssignMode(false);
    }

    public void ShowBuilding()
    {
        SetActiveView(blockCodingUIPanel, false);
        SetActiveView(buildingUIPanel, true);
        SetActiveView(buildingRoot, true);
        SetActiveView(simulationRoot, false);
        SetActiveView(arRoot, false);
        SetActiveView(simulationUIPanel, false);
        SetActiveView(arUIPanel, false);
        MotorLabelManager.Instance?.SetAssignMode(false);
    }

    public void ShowSimulation()
    {
        SetActiveView(blockCodingUIPanel, false);
        SetActiveView(buildingUIPanel, false);
        SetActiveView(simulationUIPanel, true);
        SetActiveView(arUIPanel, false);
        SetActiveView(buildingRoot, false);
        SetActiveView(simulationRoot, true);
        SetActiveView(arRoot, false);
        MotorLabelManager.Instance?.SetAssignMode(false);
    }

    public void ShowAR()
    {
        SetActiveView(blockCodingUIPanel, false);
        SetActiveView(buildingUIPanel, false);
        SetActiveView(buildingRoot, false);
        SetActiveView(simulationRoot, false);
        SetActiveView(simulationUIPanel, false);
        SetActiveView(arUIPanel, true);
        SetActiveView(arRoot, true);
        MotorLabelManager.Instance?.SetAssignMode(false);
    }

    private void SetActiveView(GameObject obj, bool active)
    {
        if (obj != null)
            obj.SetActive(active);
    }
}
