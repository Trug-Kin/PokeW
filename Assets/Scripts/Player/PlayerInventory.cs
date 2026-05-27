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

    // 🔥 BÍ KÍP TRUYỀN DỮ LIỆU: Biến tĩnh (static) giúp giữ dữ liệu xuyên suốt các Scene
    private static List<ItemSlot> globalInventoryHold = null;
    private static List<QuestProgressData> globalQuestProgressHold = null;

    public void Start()
    {
        // 1. ĐỒNG BỘ TÚI ĐỒ VẬT PHẨM
        if (globalInventoryHold != null)
        {
            // Nếu đã có dữ liệu từ trước, bê nguyên vẹn vào Scene mới
            inventory = globalInventoryHold;
        }
        else
        {
            // Chỉ chạy 1 lần duy nhất khi mới mở game để nạp hành trang khởi điểm
            globalInventoryHold = inventory;
        }

        // 2. ĐỒNG BỘ TIẾN TRÌNH NHIỆM VỤ
        if (globalQuestProgressHold != null)
        {
            questProgress = globalQuestProgressHold;
        }
        else
        {
            globalQuestProgressHold = questProgress;
        }
    }

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