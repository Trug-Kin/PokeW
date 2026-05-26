using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InventoryUI))]
public class BagUIUpdater : MonoBehaviour
{
    private PlayerInventory playerInventory;
    private InventoryUI inventoryUI;

    private void Awake()
    {
        inventoryUI = GetComponent<InventoryUI>();
    }

    private void OnEnable()
    {
        // 🔥 QUAN TRỌNG: Quét lại Player mỗi khi bật UI để đề phòng lỗi chuyển Scene
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerInventory = player.GetComponent<PlayerInventory>();
        }
        else
        {
            // Dự phòng: Thử tìm bằng Type nếu lỡ quên gắn Tag
            playerInventory = FindAnyObjectByType<PlayerInventory>();
        }

        if (playerInventory == null)
        {
            Debug.LogError("[LỖI BagUIUpdater] Không tìm thấy túi đồ của Player! Hãy kiểm tra lại Tag.");
            return;
        }

        UpdateBagUI();
    }

    public void UpdateBagUI()
    {
        if (playerInventory == null || inventoryUI == null) return;

        // Bàn giao toàn bộ túi đồ THẬT cho UI vẽ lên màn hình
        inventoryUI.SetData(playerInventory.inventory, true); 
    }
}