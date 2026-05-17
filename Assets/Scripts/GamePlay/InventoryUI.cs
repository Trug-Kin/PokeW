using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class ItemSlot
{
    public ItemBase item;
    public int count;
}

public class InventoryUI : MonoBehaviour
{
    [Header("Cấu hình danh sách ô vật phẩm (Kéo thả GameObject/Image thoải mái 100%)")]
    [SerializeField] List<GameObject> slotUIList; // Chuyển sang GameObject để Inspector không bao giờ chặn kéo thả!

    List<ItemSlot> inventorySlots;
    int selectedItem = -1; // Khởi tạo bằng -1 để chờ người chơi click chuột tương tác lần đầu

    Action cachedOnBack;
    Action<ItemBase> cachedOnUsed;

    // ĐÃ NÂNG CẤP: Nhận thêm cờ showCount để truyền tiếp lệnh xuống nạp giao diện con
    public void SetData(List<ItemSlot> slots, bool showCount)
    {
        inventorySlots = slots; 
        selectedItem = -1; // Reset về -1 mỗi khi mở hoặc chuyển menu để bảo vệ dữ liệu click chuột

        if (inventorySlots == null || slotUIList == null) return;

        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (slotUIList[i] == null) continue;

            if (i < inventorySlots.Count)
            {
                slotUIList[i].SetActive(true);

                // Tìm script xử lý hiển thị gắn trên ô con
                ItemSlotUI slotScript = slotUIList[i].GetComponent<ItemSlotUI>();
                if (slotScript != null)
                {
                    slotScript.SetData(inventorySlots[i], showCount); // Truyền cờ hiển thị số lượng xuống ô con
                }

                // KHAI THÔNG TIA CHUỘT: Tắt thuộc tính Raycast Target của chữ và icon con để không cản trở ô cha nhận click
                foreach (Text childText in slotUIList[i].GetComponentsInChildren<Text>(true)) childText.raycastTarget = false;
                foreach (TextMeshProUGUI childTMP in slotUIList[i].GetComponentsInChildren<TextMeshProUGUI>(true)) childTMP.raycastTarget = false;
                foreach (Image childImg in slotUIList[i].GetComponentsInChildren<Image>(true))
                {
                    if (childImg.gameObject != slotUIList[i]) childImg.raycastTarget = false;
                }

                Image mainImg = slotUIList[i].GetComponent<Image>();
                if (mainImg != null) mainImg.raycastTarget = true;

                // TỰ ĐỘNG CẮM DÂY: Nếu chưa có Button, tự động thêm bằng code luôn cho mượt
                Button btn = slotUIList[i].GetComponent<Button>();
                if (btn == null) btn = slotUIList[i].AddComponent<Button>();

                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    int index = i;
                    btn.onClick.AddListener(() => OnSlotClicked(index));
                }
            }
            else
            {
                slotUIList[i].SetActive(false); // Ẩn các ô thừa đi
            }
        }

        UpdateItemSelection(-1, null);
    }

    void OnSlotClicked(int index)
    {
        if (inventorySlots == null || index >= inventorySlots.Count) return;

        if (selectedItem != index)
        {
            // BẤM PHÁT LẦN 1: Highlight chọn xem và đổi màu chữ TextMeshPro con
            selectedItem = index;
            UpdateItemSelection(selectedItem, inventorySlots[selectedItem].item);
            Debug.Log($"[CLICK LẦN 1] Đã chọn vật phẩm: {inventorySlots[index].item.itemName}. Bấm tiếp phát nữa để kích hoạt!");
        }
        else
        {
            // BẤM PHÁT LẦN 2 VÀO CHÍNH Ô ĐÓ: Kích hoạt lệnh truyền về BattleSystem xử lý lồng menu
            if (inventorySlots[selectedItem].item != null)
            {
                cachedOnUsed?.Invoke(inventorySlots[selectedItem].item);
            }
        }
    }

    public void UpdateItemSelection(int selectedItem, ItemBase itemBase)
    {
        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (slotUIList[i] == null) continue;

            ItemSlotUI slotScript = slotUIList[i].GetComponent<ItemSlotUI>();
            if (slotScript != null)
            {
                if (i == selectedItem)
                    slotScript.SetSelected(true); 
                else
                    slotScript.SetSelected(false); 
            }
        }
    }

    public void HandleUpdate(Action onBack, Action<ItemBase> onUsed)
    {
        cachedOnBack = onBack;
        cachedOnUsed = onUsed;

        int prevSelection = selectedItem;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            ++selectedItem;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            --selectedItem;

        if (inventorySlots != null && inventorySlots.Count > 0 && selectedItem >= 0)
        {
            selectedItem = Mathf.Clamp(selectedItem, 0, inventorySlots.Count - 1);

            if (prevSelection != selectedItem)
            {
                UpdateItemSelection(selectedItem, inventorySlots[selectedItem].item);
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                onUsed?.Invoke(inventorySlots[selectedItem].item);
            }
        }
    }
}