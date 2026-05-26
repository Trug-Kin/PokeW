using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InventoryUI))] // Yêu cầu Unity tự động gắn kèm InventoryUI
public class BagUIUpdater : MonoBehaviour
{
    private PlayerInventory playerInventory;
    private InventoryUI inventoryUI;

    private void Awake()
    {
        // 1. Tự động tìm nhân vật để lấy túi đồ
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerInventory = player.GetComponent<PlayerInventory>();
        }
        else
        {
            Debug.LogWarning("BagUIUpdater: Không tìm thấy nhân vật có tag 'Player'!");
        }

        // 2. Liên kết với hệ thống InventoryUI có sẵn trên cùng GameObject
        inventoryUI = GetComponent<InventoryUI>();
    }

    private void OnEnable()
    {
        UpdateBagUI();
    }

    public void UpdateBagUI()
    {
        if (playerInventory == null || inventoryUI == null) return;

        // Bàn giao toàn bộ túi đồ thực tế của Player cho InventoryUI lo liệu.
        // Biến "true" để ra lệnh cho InventoryUI hiển thị thêm số lượng (Ví dụ: x10).
        inventoryUI.SetData(playerInventory.inventory, true); 
    }
}