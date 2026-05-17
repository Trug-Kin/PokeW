using UnityEngine;
using UnityEngine.UI;
using TMPro; // Sử dụng thư viện TextMeshPro cho phần tử con ItemnameText của bạn

public class ItemSlotUI : MonoBehaviour
{
    [Header("Linh kiện phần tử con (Gán từ Hierarchy)")]
    [SerializeField] TextMeshProUGUI itemNameText; // Ô chữ ItemnameText của bạn
    [SerializeField] Image icon;                  // Ô hình ảnh Icon con của vật phẩm

    [Header("Cấu hình màu sắc khi chọn")]
    [SerializeField] Color highlightedColor = Color.blue; // Màu chữ khi ô được click chọn
    [SerializeField] Color normalColor = Color.black;       // Màu chữ mặc định ban đầu

    // ĐÃ NÂNG CẤP: Nhận thêm cờ bool showCount để quyết định cách vẽ chữ
    public void SetData(ItemSlot itemSlot, bool showCount)
    {
        if (itemSlot == null || itemSlot.item == null) return;

        if (itemNameText != null)
        {
            if (showCount)
                itemNameText.text = $"{itemSlot.item.itemName} x{itemSlot.count}"; // MENU 2: Hiện đầy đủ Tên + Số lượng
            else
                itemNameText.text = itemSlot.item.itemName; // MENU 1: Chỉ hiện Tên vật phẩm đại diện
        }

        if (icon != null && itemSlot.item.icon != null)
        {
            icon.sprite = itemSlot.item.icon;
            icon.gameObject.SetActive(true); // Đảm bảo icon con được bật sáng
        }
    }

    // Hàm đổi màu chữ TextMeshPro highlight
    public void SetSelected(bool selected)
    {
        if (itemNameText == null) return;

        if (selected)
            itemNameText.color = highlightedColor; // Chuyển sang màu Xanh khi được chọn
        else
            itemNameText.color = normalColor;       // Trả về màu ban đầu (Đen/Trắng)
    }
}