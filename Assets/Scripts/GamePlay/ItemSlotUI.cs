using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI itemNameText; 
    [SerializeField] Image icon;                  
    [SerializeField] Color highlightedColor = Color.blue; 
    [SerializeField] Color normalColor = Color.black;       

    // 🔥 HÀM MỚI DÀNH RIÊNG CHO MENU 1: Hiện Tên Danh Mục + Hình Ảnh đại diện
    public void SetCategoryData(string categoryName, Sprite categoryIcon)
    {
        if (itemNameText != null) itemNameText.text = categoryName;

        if (icon != null)
        {
            if (categoryIcon != null)
            {
                icon.sprite = categoryIcon;
                icon.gameObject.SetActive(true);
            }
            else
            {
                icon.gameObject.SetActive(false);
            }
        }
    }

    // Dành riêng cho Menu 2: Hiện Icon, Tên và Số lượng x
    public void SetData(ItemSlot itemSlot, bool showCount)
    {
        if (itemSlot == null || itemSlot.item == null) return;

        if (itemNameText != null)
        {
            if (showCount) itemNameText.text = $"{itemSlot.item.itemName} x{itemSlot.count}"; 
            else itemNameText.text = itemSlot.item.itemName; 
        }

        if (icon != null)
        {
            if (itemSlot.item.icon != null)
            {
                icon.sprite = itemSlot.item.icon;
                icon.gameObject.SetActive(true); 
            }
            else icon.gameObject.SetActive(false);
        }
    }

    public void SetSelected(bool selected)
    {
        if (itemNameText != null) itemNameText.color = selected ? highlightedColor : normalColor;       
    }
}