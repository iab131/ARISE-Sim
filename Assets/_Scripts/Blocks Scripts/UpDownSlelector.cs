using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Color = UnityEngine.Color;

public class UpDownSelector : MonoBehaviour
{
    [Header("References")]
    public Button upButton;
    public Button downButton;
    public GameObject popupPanel;
    public Image directionImage;
    public Sprite upSprite;
    public Sprite downSprite;
    

    [Header("Colors")]
    public Color normalColor;
    public Color selectedColor;

    private Button selectedButton;
    private Transform originalParent;
    private Transform newParent;

    public enum Direction { Forward = 1, Backward = -1 }

    //public Direction direction = Direction.Forward;
    public Direction CurrentDirection = Direction.Forward;

    private void Start()
    {
        originalParent = popupPanel.transform.parent;
        popupPanel.SetActive(false);
        selectedButton = upButton;
        SelectButton(upButton);

        //if (direction == Direction.Forward)
        //{
        //    CurrentDirection = Direction.Forward;
        //}
        //else
        //{
        //    CurrentDirection = Direction.Backward;
        //}
        GameObject found = GameObject.Find("BlockCodingUI");
        newParent = found?.transform;
    }

    private void Update()
    {
        if (popupPanel.activeSelf && Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverPopup())
            {
                TogglePopup();
            }
        }
    }

    public void TogglePopup()
    {
        if (!popupPanel.activeSelf)
        {
            popupPanel.transform.SetParent(newParent); // <<<< Move to DragArea
            popupPanel.SetActive(true);
        }
        else
        {
            popupPanel.transform.SetParent(originalParent); // <<<< Move back
            popupPanel.SetActive(false);
        }
    }

    public void SelectUp()
    {
        SelectButton(upButton);
        directionImage.sprite = upSprite;
        CurrentDirection = Direction.Forward;
        //direction = Direction.Forward;
    }

    public void SelectDown()
    {
        SelectButton(downButton);
        directionImage.sprite = downSprite;
        CurrentDirection = Direction.Backward;
        //direction = Direction.Backward;
    }

    private void SelectButton(Button button)
    {
        if (selectedButton != null)
        {
            SetButtonVisual(selectedButton, normalColor); // always reset old one
        }

        selectedButton = button;

        if (selectedButton != null)
        {
            SetButtonVisual(selectedButton, selectedColor); // now color the new one
        }
    }


    private void SetButtonVisual(Button button, Color color)
    {
        if (button == null || button.image == null) return;

        button.image.color = color;
    }


    private bool IsPointerOverPopup()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject == popupPanel || result.gameObject.transform.IsChildOf(popupPanel.transform))
            {
                return true; // Clicked inside popup
            }
        }
        return false; // Clicked outside popup
    }
}
