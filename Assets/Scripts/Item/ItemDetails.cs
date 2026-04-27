using UnityEngine;

[System.Serializable]
public class ItemDetails
{
    public int itemCode;
    public ItemType itemType;
    public string itemDescription;
    public Sprite itemSprite;
    public string itemLongDescription;
    public short itemUseGridRadius;
    public float itemUseRadius;
    public bool isStartingItem;
    public bool canBePickedUp;
    public bool canBeDropped;
    public bool canBeEaten;
    public bool canBeCarried;

    public ItemDetails() { }

    public ItemDetails(ItemDetails original)
    {
        if (original == null) return;

        itemCode = original.itemCode;
        itemType = original.itemType;
        itemDescription = original.itemDescription;
        itemSprite = original.itemSprite; // 浅拷贝 Sprite 引用
        itemLongDescription = original.itemLongDescription;
        itemUseGridRadius = original.itemUseGridRadius;
        itemUseRadius = original.itemUseRadius;
        isStartingItem = original.isStartingItem;
        canBePickedUp = original.canBePickedUp;
        canBeDropped = original.canBeDropped;
        canBeEaten = original.canBeEaten;
        canBeCarried = original.canBeCarried;
    }
}
