using UnityEngine;
using System.Collections.Generic;

// Chứa thông tin vật phẩm (Đỡ phải tạo hệ thống phức tạp sau này)
[System.Serializable]
public class ItemData
{
    public string itemName;
    public int amount;
}

public class PlayerInventory : MonoBehaviour
{
    // Túi đồ cơ bản lưu trữ vật phẩm
    public List<ItemData> items = new List<ItemData>();

    // Hàm thêm vật phẩm vào túi
    public void AddItem(string name, int amount)
    {
        // Kiểm tra xem đã có item này trong túi chưa
        foreach (var item in items)
        {
            if (item.itemName == name)
            {
                item.amount += amount;
                Debug.Log($"Đã thêm {amount} {name}. Tổng: {item.amount}");
                return;
            }
        }

        // Nếu chưa có thì tạo mới
        items.Add(new ItemData { itemName = name, amount = amount });
        Debug.Log($"Nhận được vật phẩm mới: {name} (x{amount})");
    }

    // Hàm kiểm tra xem người chơi có đủ vật phẩm nhiệm vụ không
    public bool HasItem(string name, int requiredAmount)
    {
        foreach (var item in items)
        {
            if (item.itemName == name && item.amount >= requiredAmount)
                return true;
        }
        return false;
    }
}