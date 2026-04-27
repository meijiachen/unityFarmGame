using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : SingletonMonobehaviour<InventoryManager>
{
    private Dictionary<int, ItemDetails> itemDetailsDic;
    public List<InventoryItem>[] inventoryLists;
    [HideInInspector] public int[] inventoryListCapacityIntArray;
    [SerializeField] private SO_itemList itemList = null;

    protected override void Awake()
    {
        base.Awake();
        CreateInventoryLists();
        CreateItemDetailDictionary();
    }

    private void CreateInventoryLists()
    {
        inventoryLists = new List<InventoryItem>[(int)InventoryLocation.count];
        for (int i = 0; i < (int)InventoryLocation.count; i++)
        {
            inventoryLists[i] = new List<InventoryItem>();
        }
        inventoryListCapacityIntArray = new int[(int)InventoryLocation.count];
        inventoryListCapacityIntArray[(int)InventoryLocation.player] = Settings.playerInitialInventoryCapacity;
    }

    private void CreateItemDetailDictionary()
    {
        itemDetailsDic = new Dictionary<int, ItemDetails>();
        foreach(ItemDetails itemDetails in itemList.itemDetails)
        {
            itemDetailsDic.Add(itemDetails.itemCode, itemDetails);
        }
    }

    public void AddItem(InventoryLocation inventoryLocation,Item item,GameObject gameObject)
    {
       AddItem(inventoryLocation, item);
       Destroy(gameObject);
    }

    public void AddItem(InventoryLocation inventoryLocation,Item item)
    {
        int itemCode = item.ItemCode;
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];
        int itemIndex = inventoryList.FindIndex(iitem => iitem.itemCode == itemCode);
        AddItemAtIndex(inventoryList,itemCode,itemIndex);
        EventHandler.CallInventoryUpdateEvent(inventoryLocation,inventoryLists[(int)inventoryLocation]);
    }

    private void AddItemAtIndex(List<InventoryItem> inventoryList,int itemCode,int itemIndex)
    {
        if (itemIndex < 0){
            InventoryItem inventoryItem = new InventoryItem();
            inventoryItem.itemCode = itemCode;
            inventoryItem.itemQuantity = 1;
            inventoryList.Add(inventoryItem);
        }
        else
        {
            InventoryItem item = inventoryList.Find(iitem => iitem.itemCode == itemCode);
            item.itemQuantity += 1;
            inventoryList[itemIndex] = item;
        }
        DebugPrintInventoryList(inventoryList);
    }

    private void DebugPrintInventoryList(List<InventoryItem> inventoryList)
    {
        // Debug.Log("*DebugPrintInventoryList****");
        // foreach (InventoryItem item in inventoryList)
        // {
        //     Debug.Log("Item Description:" + InventoryManager.Instance.GetItemDetails(item.itemCode).itemDescription+
        //     " Item Quantity:" + item.itemQuantity);
        // }
        // Debug.Log("**************************");
    }

    public ItemDetails GetItemDetails(int itemCode)
    {
        ItemDetails itemDetails;
        if (itemDetailsDic.TryGetValue(itemCode, out itemDetails)){
            return itemDetails;
        }
        return null;
    }

    public int GetItemIndex(InventoryLocation inventoryLocation,int itemCode)
    {
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];
        return inventoryList.FindIndex(iitem=>iitem.itemCode == itemCode);
    }

    public void RemoveItem(InventoryLocation inventoryLocation,int itemCode)
    {
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];
        int idx = inventoryList.FindIndex(iitem=>iitem.itemCode == itemCode);
        if (idx >= 0)
        {
            InventoryItem item = inventoryList.Find(iitem=>iitem.itemCode == itemCode);
            if (item.itemQuantity - 1 > 0)
            {
                InventoryItem newItem = new InventoryItem();
                newItem.itemQuantity = item.itemQuantity - 1;
                newItem.itemCode = itemCode;
                inventoryList[idx] = newItem;
            }
            else
            {
                inventoryList.RemoveAt(idx);
            }
        }
        EventHandler.CallInventoryUpdateEvent(inventoryLocation,inventoryLists[(int)inventoryLocation]);
    }

    public void SwapInventoryItem(InventoryLocation inventoryLocation,int sourceSlotNum,int targetSlotNum)
    {
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];
        if (sourceSlotNum < 0 || targetSlotNum < 0 || sourceSlotNum >= inventoryList.Count || targetSlotNum >= inventoryList.Count)
        {
            return;
        }
        InventoryItem sourceItem = inventoryList[sourceSlotNum];
        InventoryItem targetItem = inventoryList[targetSlotNum];
        inventoryList[sourceSlotNum] = targetItem;
        inventoryList[targetSlotNum] = sourceItem;
        EventHandler.CallInventoryUpdateEvent(inventoryLocation,inventoryLists[(int)inventoryLocation]);
    }

    public string GetItemTypeDescription(ItemType itemType)
    {
        string itemTypeDescription;
        switch (itemType)
        {
            case ItemType.Breaking_tool:
                itemTypeDescription = Settings.BreakingTool;
                break;
            case ItemType.Chopping_tool:
                itemTypeDescription = Settings.ChoppingTool;
                break;
            case ItemType.Hoeing_tool:
                itemTypeDescription = Settings.HoeingTool;
                break;
            case ItemType.Reaping_tool:
                itemTypeDescription = Settings.ReapingTool;
                break;
            case ItemType.Collecting_tool:
                itemTypeDescription = Settings.CollectingTool;
                break;
            case ItemType.Watering_tool:
                itemTypeDescription = Settings.WateringTool;
                break;
            default:
                itemTypeDescription = itemType.ToString();
                break;
        }
        return itemTypeDescription;
    }
}
