using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInventoryBar : MonoBehaviour
{
    [SerializeField]private Sprite blank16x16sprite = null;
    private UIInventorySlot[] inventorySlots = null;

    public Transform invetoryItemsTransform;
    public GameObject slotPrefab;
    public GameObject inventoryBarDraggedItem;
    public int slotCount = 0;
    private RectTransform rectTransform;
    private bool _isInventoryBarPositionBottom = true;
    
    private float switchCooldown = 0.3f;
    private float lastSwitchTime;
    [HideInInspector]public GameObject inventoryTextBoxGameObject;

    private bool _isDragging = false;
    public bool IsDragging { get { return _isDragging;} set { _isDragging = value;}}

    public bool IsInventoryBarPositionBottom { get { return _isInventoryBarPositionBottom;} set { _isInventoryBarPositionBottom = value;}}
    public void Awake()
    {   
        inventorySlots = new UIInventorySlot[slotCount];
        for (int i = 0; i < slotCount; i++)
        {
            GameObject ob = Instantiate(slotPrefab, invetoryItemsTransform);
            UIInventorySlot slot = ob.GetComponent<UIInventorySlot>();
            inventorySlots[i] = slot;
        }
        ClearInventorySlots();
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        EventHandler.InventoryUpdatedEvent += InvetoryUpdated;
    }

    private void OnDisable()
    {
        EventHandler.InventoryUpdatedEvent -= InvetoryUpdated;
    }

    private void InvetoryUpdated(InventoryLocation location, List<InventoryItem> list)
    {
        if (location == InventoryLocation.player)
        {
            ClearInventorySlots();
            if (inventorySlots.Length > 0 && list.Count > 0)
            {
                for (int i = 0; i < inventorySlots.Length; i++)
                {
                    if( i < list.Count)
                    {
                        int itemCode = list[i].itemCode;
                        ItemDetails iDetails = InventoryManager.Instance.GetItemDetails(itemCode);
                        inventorySlots[i].itemDetails = iDetails;
                        inventorySlots[i].itemQuantity = list[i].itemQuantity;
                        inventorySlots[i].inventorySlotImage.sprite = iDetails.itemSprite;
                        inventorySlots[i].textMeshProUGUI.text = list[i].itemQuantity.ToString();
                        inventorySlots[i].slotNumber = InventoryManager.Instance.GetItemIndex(InventoryLocation.player,itemCode);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            int selectedItemCode  = InventoryManager.Instance.GetSelectedInventoryItem(InventoryLocation.player);
            if (selectedItemCode != -1)
            {
                setHighlightOnInventorySlots(selectedItemCode);
            }
        }
    }

    private void ClearInventorySlots()
    {
        if(inventorySlots.Length > 0)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                inventorySlots[i].inventorySlotImage.sprite = blank16x16sprite;
                inventorySlots[i].textMeshProUGUI.text = "";
                inventorySlots[i].itemDetails = null;
                inventorySlots[i].itemQuantity = 0;
                inventorySlots[i].slotNumber = 0;
            }
        }
    }

    private void Update()
    {
        if (Time.time - lastSwitchTime < switchCooldown) return;

        SwitchInventoryBarPosition();
    }

    private void SwitchInventoryBarPosition()
    {
        Vector3 playerViewportPosition = Player.Instance.GetPlayerViewportPosition();
        if (playerViewportPosition.y > 0.3f && IsInventoryBarPositionBottom == false)
        {
            rectTransform.pivot = new Vector2(0.5f,0f);
            rectTransform.anchorMin = new Vector2(0.5f,0f);
            rectTransform.anchorMax = new Vector2(0.5f,0f);
            rectTransform.anchoredPosition = new Vector2(0,2.5f);

            IsInventoryBarPositionBottom = true;
        }
        else if (playerViewportPosition.y <= 0.3f && IsInventoryBarPositionBottom == true)
        {
            rectTransform.pivot = new Vector2(0.5f,1f);
            rectTransform.anchorMin = new Vector2(0.5f,1f);
            rectTransform.anchorMax = new Vector2(0.5f,1f);
            rectTransform.anchoredPosition = new Vector2(0,-2.5f);

            IsInventoryBarPositionBottom = false;
        }
        lastSwitchTime = Time.time;
    }

    public void ClearHighlightOnInventorySlots()
    {
        if(inventorySlots.Length > 0)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i].itemDetails != null)
                {
                    if (inventorySlots[i].isSelected == true)
                    {
                        inventorySlots[i].isSelected = false;
                        inventorySlots[i].inventorySlotHighlight.color = new Color(0f,0f,0f,0f);
                        InventoryManager.Instance.ClearSelectedInventoryItem(InventoryLocation.player);
                    }
                }
            }
        }
    }

    public void setHighlightOnInventorySlots(int itemCode)
    {
        if(inventorySlots.Length > 0)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i].itemDetails != null)
                {
                    if (inventorySlots[i].itemDetails.itemCode == itemCode)
                    {
                        inventorySlots[i].isSelected = true;
                        inventorySlots[i].inventorySlotHighlight.color = new Color(1f,1f,1f,1f);
                        InventoryManager.Instance.SetSelectedInventoryItem(InventoryLocation.player,itemCode);
                    }
                    else
                    {
                        inventorySlots[i].isSelected = false;
                        inventorySlots[i].inventorySlotHighlight.color = new Color(0f,0f,0f,0f);
                    }
                }
                else if (inventorySlots[i].isSelected == true)
                {
                    inventorySlots[i].isSelected = false;
                    inventorySlots[i].inventorySlotHighlight.color = new Color(0f,0f,0f,0f);
                }
            }
        }
    }

    public void SetHighlightedInventorySlotsByPos(int itemPosition)
    {
        if (inventorySlots.Length > 0 && inventorySlots[itemPosition].itemDetails != null)
        {
            if (inventorySlots[itemPosition].isSelected)
            {
                inventorySlots[itemPosition].inventorySlotHighlight.color = new Color(1f, 1f, 1f, 1f);
                InventoryManager.Instance.SetSelectedInventoryItem(InventoryLocation.player, inventorySlots[itemPosition].itemDetails.itemCode);
            }
        }
    }
}
