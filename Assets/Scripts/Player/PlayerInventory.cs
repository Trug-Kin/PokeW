using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class QuestProgressData
{
    public string objectiveName;
    public int amount;
}

public class PlayerInventory : MonoBehaviour
{
    [Header("Túi đồ vật phẩm (Thực)")]
    public List<ItemSlot> inventory = new List<ItemSlot>(); 

    [Header("Tiến trình nhiệm vụ (Ảo)")]
    public List<QuestProgressData> questProgress = new List<QuestProgressData>();

    // --- 1. DÀNH CHO VẬT PHẨM THẬT (Giao diện UI & Trận đấu) ---
    public void AddItem(ItemBase item, int amount)
    {
        if (item == null) return;
        foreach (var slot in inventory)
        {
            if (slot.item == item)
            {
                slot.count += amount;
                return;
            }
        }
        inventory.Add(new ItemSlot { item = item, count = amount });
    }

    public bool HasItem(ItemBase item, int amount)
    {
        if (item == null) return false;
        foreach (var slot in inventory)
        {
            if (slot.item == item && slot.count >= amount)
                return true;
        }
        return false;
    }

    // --- 2. DÀNH CHO TIẾN TRÌNH NHIỆM VỤ (Đánh quái, đếm số lượng...) ---
    // Hàm này sẽ tự động dập tắt lỗi ở dòng 584 trong BattleSystem của cậu
    public void AddItem(string itemName, int amount)
    {
        foreach (var q in questProgress)
        {
            if (q.objectiveName == itemName)
            {
                q.amount += amount;
                return;
            }
        }
        questProgress.Add(new QuestProgressData { objectiveName = itemName, amount = amount });
    }

    public bool HasItem(string itemName, int amount)
    {
        foreach (var q in questProgress)
        {
            if (q.objectiveName == itemName && q.amount >= amount)
                return true;
        }
        return false;
    }
}