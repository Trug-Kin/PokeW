using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemSlot
{
    public ItemBase item;
    public int count;
}

public enum BagState { Categories, Items }

public class InventoryUI : MonoBehaviour
{
    [Header("Code tự động quét UI, không cần kéo thả")]
    [SerializeField] List<ItemSlotUI> slotUIList; 

    List<ItemSlot> allInventorySlots;
    List<ItemSlot> filteredSlots;

    BagState bagState = BagState.Categories;
    int selectedItemIndex = -1;

    List<ItemCategory> availableCategories = new List<ItemCategory>();

    Action cachedOnBack;
    Action<ItemBase> cachedOnUsed;

    public void SetData(List<ItemSlot> slots, bool showCount)
    {
        ItemSlotUI[] autoSlots = GetComponentsInChildren<ItemSlotUI>(true);
        slotUIList = new List<ItemSlotUI>(autoSlots);

        allInventorySlots = slots;
        ShowCategoriesMenu();
    }

    string GetCategoryDisplayName(ItemCategory category)
    {
        switch (category)
        {
            case ItemCategory.DTH: return "Dưỡng Thú Hồ (DTH)";
            case ItemCategory.Heal: return "Vật Phẩm Hồi Phục (HEAL)";
            case ItemCategory.StatBoost: return "Thuốc Tăng Chỉ Số";
            case ItemCategory.KeyItem: return "Vật Phẩm Quan Trọng";
            default: return "Vật Phẩm Khác";
        }
    }

    // 🔥 HÀM SĂN ICON THÔNG MINH (Đã chặn lỗi ảnh tàng hình)
    Sprite GetCategoryIcon(ItemCategory category)
    {
        Sprite bestIcon = null;
        int lowestSortOrder = int.MaxValue;

        foreach (var slot in allInventorySlots)
        {
            // Chỉ xét đồ có thật, đúng danh mục và BẮT BUỘC PHẢI CÓ ICON
            if (slot != null && slot.item != null && slot.count > 0 && slot.item.category == category)
            {
                if (slot.item.icon != null) 
                {
                    // Lấy Icon của món đồ Sơ Cấp nhất
                    if (slot.item.sortOrder < lowestSortOrder || bestIcon == null)
                    {
                        lowestSortOrder = slot.item.sortOrder;
                        bestIcon = slot.item.icon;
                    }
                }
            }
        }
        return bestIcon;
    }

    void ShowCategoriesMenu()
    {
        bagState = BagState.Categories;
        selectedItemIndex = -1;
        availableCategories.Clear();

        if (slotUIList == null || allInventorySlots == null) return;

        foreach (var slot in allInventorySlots)
        {
            if (slot == null || slot.item == null || slot.count <= 0) continue;

            if (!availableCategories.Contains(slot.item.category))
            {
                availableCategories.Add(slot.item.category);
            }
        }

        availableCategories.Sort((a, b) => a.CompareTo(b));

        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (slotUIList[i] == null) continue;

            if (i < availableCategories.Count)
            {
                slotUIList[i].gameObject.SetActive(true);
                
                string catName = GetCategoryDisplayName(availableCategories[i]);
                Sprite catIcon = GetCategoryIcon(availableCategories[i]); 
                
                // Gọi hàm xuất dữ liệu ra ItemSlotUI
                slotUIList[i].SetCategoryData(catName, catIcon); 
            }
            else
            {
                slotUIList[i].gameObject.SetActive(false);
            }
        }
        UpdateSelectionVisuals(-1);
    }

    void ShowSubItemsMenu(ItemCategory selectedCategory)
    {
        bagState = BagState.Items;
        selectedItemIndex = -1;
        filteredSlots = new List<ItemSlot>();

        if (allInventorySlots == null) return;

        foreach (var slot in allInventorySlots)
        {
            if (slot == null || slot.item == null || slot.count <= 0) continue;

            if (slot.item.category == selectedCategory)
            {
                filteredSlots.Add(slot);
            }
        }

        filteredSlots.Sort((a, b) => 
        {
            if (a == null || a.item == null || b == null || b.item == null) return 0;
            return a.item.sortOrder.CompareTo(b.item.sortOrder);
        });

        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (slotUIList[i] == null) continue;

            if (i < filteredSlots.Count)
            {
                slotUIList[i].gameObject.SetActive(true);
                slotUIList[i].SetData(filteredSlots[i], true); 
            }
            else
            {
                slotUIList[i].gameObject.SetActive(false);
            }
        }
        UpdateSelectionVisuals(-1);
    }

    public void MouseSelectItem(int index)
    {
        if (bagState == BagState.Categories)
        {
            if (index < 0 || index >= availableCategories.Count) return;

            if (selectedItemIndex != index)
            {
                selectedItemIndex = index;
                UpdateSelectionVisuals(selectedItemIndex);
            }
            else
            {
                ShowSubItemsMenu(availableCategories[index]);
            }
        }
        else
        {
            if (filteredSlots == null || index < 0 || index >= filteredSlots.Count) return;

            if (selectedItemIndex != index)
            {
                selectedItemIndex = index;
                UpdateSelectionVisuals(selectedItemIndex);
            }
            else
            {
                if (filteredSlots[selectedItemIndex].item != null)
                {
                    cachedOnUsed?.Invoke(filteredSlots[selectedItemIndex].item);
                }
            }
        }
    }

    public void MouseClickBack()
    {
        if (bagState == BagState.Items) ShowCategoriesMenu(); 
        else cachedOnBack?.Invoke(); 
    }

    public void UpdateSelectionVisuals(int index)
    {
        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (slotUIList[i] == null) continue;
            slotUIList[i].SetSelected(i == index);
        }
    }

    public void HandleUpdate(Action onBack, Action<ItemBase> onUsed)
    {
        cachedOnBack = onBack;
        cachedOnUsed = onUsed;
    }
}