using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro; // ← Needed for TMP_InputField

public class PartFilterUI : MonoBehaviour
{
    public enum Category { All, Structure, Connector, Motion, Electronics }

    public Toggle structureToggle;
    public Toggle connectorToggle;
    public Toggle motionToggle;
    public Toggle electronicsToggle;

    public Transform partListParent; // Parent containing all part UI items
    public TMP_InputField searchInput; // ← TMP version

    private Category currentCategory = Category.All;
    private bool isUpdating = false;
    private string searchQuery = "";

    void Start()
    {
        structureToggle.onValueChanged.AddListener((isOn) => OnToggleChanged(Category.Structure, isOn));
        connectorToggle.onValueChanged.AddListener((isOn) => OnToggleChanged(Category.Connector, isOn));
        motionToggle.onValueChanged.AddListener((isOn) => OnToggleChanged(Category.Motion, isOn));
        electronicsToggle.onValueChanged.AddListener((isOn) => OnToggleChanged(Category.Electronics, isOn));

        if (searchInput != null)
            searchInput.onValueChanged.AddListener(OnSearchChanged);
    }

    void OnToggleChanged(Category selected, bool isOn)
    {
        if (isUpdating) return;

        if (isOn)
        {
            currentCategory = selected;

            isUpdating = true;
            SetOtherTogglesOff(selected);
            isUpdating = false;
        }
        else
        {
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

    void OnSearchChanged(string text)
    {
        searchQuery = text.ToLower();
        FilterPartList();
    }

    void FilterPartList()
    {
        foreach (Transform part in partListParent)
        {
            var item = part.GetComponent<PartItem>();
            bool matchesCategory = (currentCategory == Category.All || item.category == currentCategory);
            bool matchesSearch = string.IsNullOrEmpty(searchQuery) || part.name.ToLower().Contains(searchQuery);

            part.gameObject.SetActive(matchesCategory && matchesSearch);
        }
    }
}
