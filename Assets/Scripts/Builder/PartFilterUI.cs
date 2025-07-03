using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PartFilterUI : MonoBehaviour
{
    public enum Category { All, Structure, Connector, Motion, Electronics }

    public Toggle structureToggle;
    public Toggle connectorToggle;
    public Toggle motionToggle;
    public Toggle electronicsToggle;

    public Transform partListParent; // Parent containing all part UI items

    private Category currentCategory = Category.All;
    private bool isUpdating = false;

    void Start()
    {
        structureToggle.onValueChanged.AddListener((isOn) => OnToggleChanged(Category.Structure, isOn));
        connectorToggle.onValueChanged.AddListener((isOn) => OnToggleChanged(Category.Connector, isOn));
        motionToggle.onValueChanged.AddListener((isOn) => OnToggleChanged(Category.Motion, isOn));
        electronicsToggle.onValueChanged.AddListener((isOn) => OnToggleChanged(Category.Electronics, isOn));
    }

    void OnToggleChanged(Category selected, bool isOn)
    {
        if (isUpdating) return;

        if (isOn)
        {
            currentCategory = selected;

            // Make sure only one toggle is on
            isUpdating = true;
            SetOtherTogglesOff(selected);
            isUpdating = false;
        }
        else
        {
            // If toggled off while already active, switch to "All"
            if (currentCategory == selected)
                currentCategory = Category.All;
        }

        FilterPartList();
    }

    void SetOtherTogglesOff(Category except)
    {
        structureToggle.isOn = (except == Category.Structure);
        connectorToggle.isOn = (except == Category.Connector);
        motionToggle.isOn = (except == Category.Motion);
        electronicsToggle.isOn = (except == Category.Electronics);
    }

    void FilterPartList()
    {
        foreach (Transform part in partListParent)
        {
            var item = part.GetComponent<PartItem>();
            bool shouldShow = (currentCategory == Category.All || item.category == currentCategory);
            part.gameObject.SetActive(shouldShow);
        }
    }
}
