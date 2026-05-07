using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIInventorySlot : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler,IPointerEnterHandler,IPointerExitHandler,IPointerClickHandler
{

    private Camera mainCamera;
    private Canvas parentCanvas;
    private Transform parentItem;
    private GameObject draggedItem;
    public Image inventorySlotHighlight;
    public Image inventorySlotImage;
    public TextMeshProUGUI textMeshProUGUI;

    private bool _isSelected = false;
    public bool isSelected { get { return _isSelected; } set { _isSelected = value; } }

    private int _slotNumber = 0;

    public int slotNumber { get { return _slotNumber; } set { _slotNumber = value; } }

    private UIInventoryBar inventoryBar = null;
    [HideInInspector]public ItemDetails itemDetails;
    [SerializeField]private GameObject itemPrefab = null;
    [SerializeField]private GameObject inventoryTextBoxPrefab = null;
    [HideInInspector]public int itemQuantity;

    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += SceneLoaded;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= SceneLoaded;
    }

    private void SceneLoaded()
    {
        parentItem = GameObject.FindGameObjectWithTag(Tags.ItemsParentTransform).transform;
        inventoryBar = transform.parent.parent.GetComponent<UIInventoryBar>();
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        DestoryInventoryTextBox();
        inventoryBar.IsDragging = true;
        if (itemDetails != null)
        {
            Player.Instance.DisablePlayerInputAndRestMovement();
            draggedItem = Instantiate(inventoryBar.inventoryBarDraggedItem,inventoryBar.transform);

            Image draggedItemImage = draggedItem.GetComponentInChildren<Image>();
            if (draggedItemImage != null)
            {
                draggedItemImage.sprite = inventorySlotImage.sprite;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(draggedItem != null)
        {
            draggedItem.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(draggedItem != null)
        {
            Destroy(draggedItem);

            if(eventData.pointerCurrentRaycast.gameObject != null && eventData.pointerCurrentRaycast.gameObject.GetComponent<UIInventorySlot>() != null)
            {
                UIInventorySlot slotEnd = eventData.pointerCurrentRaycast.gameObject.GetComponent<UIInventorySlot>();

                InventoryManager.Instance.SwapInventoryItem(InventoryLocation.player, slotNumber,slotEnd.slotNumber);

                slotEnd.ShowInventoryTextBox();
            }
            else
            {
                if(itemDetails.canBeDropped)
                {
                    DropSelectedItemAtMousePosition();
                }
            }
        }
        Player.Instance.EnablePlayerInput();
        inventoryBar.IsDragging = false;
    }

    private void DropSelectedItemAtMousePosition()
    {
        if (itemDetails != null)
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,-mainCamera.transform.position.z));

            GameObject itemOb = Instantiate(itemPrefab,worldPos,Quaternion.identity,parentItem);
            Item item = itemOb.GetComponent<Item>();
            item.ItemCode = itemDetails.itemCode;

            InventoryManager.Instance.RemoveItem(InventoryLocation.player,item.ItemCode);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowInventoryTextBox();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DestoryInventoryTextBox();
    }

    private void ShowInventoryTextBox()
    {
        if(inventoryBar.IsDragging) return;
        if (itemDetails == null) return;

        DestoryInventoryTextBox();
        inventoryBar.inventoryTextBoxGameObject = Instantiate(inventoryTextBoxPrefab,transform.position,Quaternion.identity);
        inventoryBar.inventoryTextBoxGameObject.transform.SetParent(parentCanvas.transform,false);
        UIInventoryTextBox inventoryTextBox = inventoryBar.inventoryTextBoxGameObject.GetComponent<UIInventoryTextBox>();
        if (inventoryTextBox != null)
        {
            string itemTypeDescription = InventoryManager.Instance.GetItemTypeDescription(itemDetails.itemType);
            inventoryTextBox.SetTextboxText(itemDetails.itemDescription,itemTypeDescription,"",itemDetails.itemLongDescription,"","");
            if(inventoryBar.IsInventoryBarPositionBottom)
            {
                inventoryBar.inventoryTextBoxGameObject.GetComponent<RectTransform>().pivot = new Vector2(0.5f,0f);
                inventoryBar.inventoryTextBoxGameObject.transform.position = new Vector3(transform.position.x,transform.position.y+50f,transform.position.z);
            }
            else
            {
                inventoryBar.inventoryTextBoxGameObject.GetComponent<RectTransform>().pivot = new Vector2(0.5f,1f);
                inventoryBar.inventoryTextBoxGameObject.transform.position = new Vector3(transform.position.x,transform.position.y-50f,transform.position.z);
            }
        }
    }

    private void DestoryInventoryTextBox()
    {
        if(inventoryBar.inventoryTextBoxGameObject != null)
        {
            Destroy(inventoryBar.inventoryTextBoxGameObject);
            inventoryBar.inventoryTextBoxGameObject = null;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isSelected == true)
        {
            inventoryBar.ClearHighlightOnInventorySlots();
            Player.Instance.ClearCarriedItem();
        }
        else
        {
            if(itemDetails != null)
            {
                inventoryBar.setHighlightOnInventorySlots(itemDetails.itemCode);
                if(itemDetails.canBeCarried == true)
                {
                    Player.Instance.ShowCarriedItem(itemDetails.itemCode);
                }
                else
                {
                    Player.Instance.ClearCarriedItem();
                }
            }
        }
    }
}
