using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] Text nameText;

    public void SetData(ItemSlot itemSlot)
    {
        // Hiển thị tên và số lượng. Ví dụ: Potion x5
        nameText.text = $"{itemSlot.item.itemName} x{itemSlot.count}";
    }

    // Đổi màu chữ khi con trỏ chọn vào vật phẩm này
    public void SetSelected(bool selected)
    {
        if (selected)
            nameText.color = Color.blue; // Hoặc màu nổi bật nào đó
        else
            nameText.color = Color.black; // Màu mặc định
    }
}