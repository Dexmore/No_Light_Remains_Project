using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public ItemData data; // 원본 데이터 (.asset)
    public int quantity;  // 개수

    public InventoryItem(ItemData data, int quantity)
    {
        this.data = data;
        this.quantity = quantity;
    }
}