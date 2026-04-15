using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ItemSlot
{
    public ItemBase item;
    public int count;
}

public class InventoryUI : MonoBehaviour
{
    [SerializeField] List<ItemSlotUI> slotUIList;
    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;

    // Khai báo biến này để lưu danh sách túi đồ
    List<ItemSlot> inventorySlots;
    int selectedItem = 0;

    // Hàm này gọi để nạp dữ liệu từ túi đồ của người chơi vào UI
    public void SetData(List<ItemSlot> slots)
    {
        inventorySlots = slots; // Lưu lại để dùng cho HandleUpdate

        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i < inventorySlots.Count)
            {
                slotUIList[i].gameObject.SetActive(true);
                slotUIList[i].SetData(inventorySlots[i]);
            }
            else
            {
                // Ẩn đi nếu không có vật phẩm ở vị trí này
                slotUIList[i].gameObject.SetActive(false);
            }
        }
    }

    // Hàm cập nhật con trỏ lựa chọn
    public void UpdateItemSelection(int selectedItem, ItemBase itemBase)
    {
        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i == selectedItem)
                slotUIList[i].SetSelected(true);
            else
                slotUIList[i].SetSelected(false);
        }

        // Cập nhật Icon và Mô tả cho item đang chọn
        if (itemBase != null)
        {
            itemIcon.sprite = itemBase.icon;
            itemDescription.text = itemBase.description;
        }
    }

    public void HandleUpdate(Action onBack, Action<ItemBase> onUsed)
    {
        int prevSelection = selectedItem;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            ++selectedItem;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            --selectedItem;

        // Giới hạn vòng lặp danh sách (tránh báo lỗi null nếu inventorySlots chưa có dữ liệu)
        if (inventorySlots != null && inventorySlots.Count > 0)
        {
            selectedItem = Mathf.Clamp(selectedItem, 0, inventorySlots.Count - 1);

            if (prevSelection != selectedItem)
            {
                UpdateItemSelection(selectedItem, inventorySlots[selectedItem].item);
            }

            if (Input.GetKeyDown(KeyCode.Z)) // Bấm Z để chọn dùng
            {
                onUsed?.Invoke(inventorySlots[selectedItem].item);
            }
            else if (Input.GetKeyDown(KeyCode.X)) // Bấm X để quay lại menu chính
            {
                onBack?.Invoke();
            }
        }
    }
}