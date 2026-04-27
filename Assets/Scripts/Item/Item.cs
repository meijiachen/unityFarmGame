using UnityEngine;
public class Item : MonoBehaviour
{
    [ItemCodeDescription]
    [SerializeField]
    private int _itemCode;
    private SpriteRenderer spriteRenderer;
    public int ItemCode { get { return _itemCode; } set { _itemCode = value; } }
    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
    private void Start()
    {
        if (ItemCode != 0){
            Init(ItemCode);
        }
    }
    public void Init(int id){
        if (id != 0)
        {
            ItemCode = id;
            ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(ItemCode);
            if (itemDetails != null)
            {
                spriteRenderer.sprite = itemDetails.itemSprite;
                if(itemDetails.itemType == ItemType.Reapable_scenary)
                {
                    gameObject.AddComponent<ItemNudge>();
                }
            }
        }
    }
}
