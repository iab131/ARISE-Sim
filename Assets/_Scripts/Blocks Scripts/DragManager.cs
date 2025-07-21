using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class DragManager : MonoBehaviour
{
    public static DragManager Instance;

    public Transform dragParent;      // Canvas
    public Transform dropArea;         // Coding Area

    public GameObject ghostBlockPrefab; // Transparent ghost block
    public ScrollRect blockPaletteScrollRect;
    public float snapDistanceThreshold = 40f; // Snap range in pixels
    public float slotOffset = 10f;

    private GameObject currentDrag;
    private GameObject ghostBlockInstance;
    private Vector2 dragOffset;

    private PointerEventData pointerEventData;
    private List<RaycastResult> raycastResults = new List<RaycastResult>();
    private Transform snapTarget = null;
    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryStartDrag();
        }

        if (Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }

        if (currentDrag != null)
        {
            Vector2 newPos = Input.mousePosition;
            currentDrag.transform.position = newPos - dragOffset;
            UpdateGhostPreview();
        }
        
    }

    private void TryStartDrag()
    {
        pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        raycastResults.Clear();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        foreach (var result in raycastResults)
        {
            // Ignore clicks on input fields, dropdowns, etc.
            //if (
            //    result.gameObject.GetComponent<InputField>() != null ||
            //    result.gameObject.GetComponent<TMP_InputField>() != null ||
            //    result.gameObject.GetComponent<TMP_Dropdown>() != null ||
            //    result.gameObject.GetComponent<Button>() != null
            //)
            //{
            //    return;
            //}

            if (result.gameObject.tag.Contains("Block"))
            {
                bool isTemplate = result.gameObject.CompareTag("BlockPrefab");
                BeginDrag(result.gameObject, isTemplate);
                break;
            }
        }
    }

    private void BeginDrag(GameObject block, bool cloneFromTemplate) //create a clone or not
    {
        if (blockPaletteScrollRect != null)
            blockPaletteScrollRect.enabled = false;

        if (cloneFromTemplate)
        {
            currentDrag = Instantiate(block, dragParent);
            FixBlockAnchors(currentDrag);

            SetInputsInteractable(currentDrag, true);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                block.GetComponent<RectTransform>(),
                Input.mousePosition,
                null,
                out dragOffset
            );
        }
        else
        {
            currentDrag = block;
            currentDrag.transform.SetParent(dragParent);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                currentDrag.GetComponent<RectTransform>(),
                Input.mousePosition,
                null,
                out dragOffset
            );
        }
        
    }
    private void SetInputsInteractable(GameObject block, bool interactable)
    {
        foreach (var input in block.GetComponentsInChildren<TMP_InputField>(true))
            input.interactable = interactable;

        foreach (var dropdown in block.GetComponentsInChildren<TMP_Dropdown>(true))
            dropdown.interactable = interactable;

        foreach (var button in block.GetComponentsInChildren<Button>(true))
            button.interactable = interactable;
    }

    public void EndDrag()
    {
        if (blockPaletteScrollRect != null)
            blockPaletteScrollRect.enabled = true;

        if (currentDrag == null)
            return;

        if (RectTransformUtility.RectangleContainsScreenPoint(dropArea.GetComponent<RectTransform>(), Input.mousePosition))
        {
            if (ghostBlockInstance != null)
            {
                // Attach to the snapped block
                if (snapTarget != null)
                {
                    currentDrag.transform.SetParent(snapTarget);

                    RectTransform currentRect = currentDrag.GetComponent<RectTransform>();
                    RectTransform snapRect = snapTarget.GetComponent<RectTransform>();

                    currentRect.anchoredPosition = new Vector2(0, -snapRect.rect.height+ slotOffset);

                    
                    //  PUSH DOWN lower blocks if there is
                    foreach (Transform child in snapTarget)
                    {
                        if (child != currentDrag.transform && child.CompareTag("Block"))
                        {
                            //float moveDistance = getTotalHeight(currentDrag.transform);
                            Transform deepestChildOfCurrent = findDeepestChild(currentDrag.transform);
                            child.SetParent(deepestChildOfCurrent);

                            RectTransform childRect = child.GetComponent<RectTransform>();

                            childRect.anchoredPosition = new Vector2(0,-deepestChildOfCurrent.GetComponent<RectTransform>().rect.height+ slotOffset);
                            break;
                        }
                    }
                }
                else
                {
                    currentDrag.transform.SetParent(dropArea);
                }

                Destroy(ghostBlockInstance);
            }
            else
            {
                currentDrag.transform.SetParent(dropArea);
            }

            currentDrag.transform.localScale = Vector3.one;
        }
        else
        {
            Destroy(currentDrag);

            if (ghostBlockInstance != null)
                Destroy(ghostBlockInstance);
        }
        if (currentDrag.CompareTag("BlockPrefab"))
        {
            currentDrag.tag = "Block";
        }
        currentDrag = null;
    }
    private void FixBlockAnchors(GameObject block)
    {
        RectTransform rect = block.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
    }

    private void UpdateGhostPreview()
    {
        if (dropArea == null ||
            currentDrag.CompareTag("BlockNoBot") ||
            LayerMask.LayerToName(currentDrag.layer).Contains("StartBlock"))
            return;

        Transform closestBlock = null;
        float closestDistance = float.MaxValue;
        Vector3 snapOffset = Vector3.zero;

        RectTransform currentRect = currentDrag.GetComponent<RectTransform>();
        Vector2 mousePos = Input.mousePosition;

        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");

        foreach (GameObject blockObj in blocks)
        {
            Transform other = blockObj.transform;
            if (other == currentDrag.transform) continue;

            RectTransform otherRect = other.GetComponent<RectTransform>();

            //Vector3 otherTop = otherRect.position + new Vector3(0, otherRect.rect.height * 0.5f, 0);
            Vector3 otherBottom = otherRect.position + new Vector3(0, -otherRect.rect.height, 0);

            float otherWidth = otherRect.rect.width;
            float currentWidth = currentRect.rect.width;
            float alignLeft = (currentWidth - otherWidth) / 2;

            float horizontalDistanceToBottom = Mathf.Abs(mousePos.x - otherBottom.x);
            float verticalDistanceToBottom = Mathf.Abs(mousePos.y - otherBottom.y);

            // Only check bottom snapping
            if (horizontalDistanceToBottom < otherWidth && verticalDistanceToBottom < snapDistanceThreshold)
            {
                float totalDistance = verticalDistanceToBottom + horizontalDistanceToBottom;
                if (totalDistance < closestDistance)
                {
                    closestDistance = totalDistance;
                    closestBlock = other;
                    snapTarget = closestBlock;
                    snapOffset = new Vector3(0, -otherRect.rect.height + slotOffset, 0);
                }
            }
        }

        if (closestBlock != null)
        {
            if (ghostBlockInstance == null)
            {
                ghostBlockInstance = Instantiate(ghostBlockPrefab, dropArea);
                ghostBlockInstance.SetActive(true);
            }

            ghostBlockInstance.transform.position = closestBlock.position + snapOffset;
        }
        else
        {
            if (ghostBlockInstance != null)
            {
                Destroy(ghostBlockInstance);
            }
            snapTarget = null;
        }
    }

    // get the total height of blocks and childred
    private float getTotalHeight(Transform parent)
    {
        RectTransform rectTransform = parent.GetComponent<RectTransform>();
        float height = rectTransform.rect.height;

        foreach (Transform child in parent)
        {
            if (child != currentDrag.transform && child.CompareTag("Block"))
            {
                return height + getTotalHeight(child);
            }
        }
        return height;
    }

    private Transform findDeepestChild(Transform parent)
    {
        Transform deepestChild = parent;

        foreach (Transform child in parent)
        {
            if (child != currentDrag.transform && child.CompareTag("Block"))
            {
                Transform childDeepest = findDeepestChild(child);
                if (childDeepest != null)
                {
                    deepestChild = childDeepest;
                }
            }
        }
        return deepestChild;
    }
}
