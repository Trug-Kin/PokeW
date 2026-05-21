using UnityEngine;

// 1. TẠO DANH SÁCH CÁC THẺ PHÂN LOẠI (Bạn có thể thêm bớt thoải mái sau này)
public enum ItemCategory
{
    DTH,            // Dưỡng Thú Hồ
    Heal,           // Vật Phẩm Hồi Phục
    StatBoost,      // Thuốc Tăng Chỉ Số (Ví dụ: Rare Candy)
    KeyItem         // Đồ Nhiệm Vụ
}

public abstract class ItemBase : ScriptableObject
{
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;
    public int price;

    [Header("Phân loại & Sắp xếp")]
    public ItemCategory category; // ĐÂY LÀ DÒNG SẼ TẠO RA Ô CHỌN LOẠI TRÊN INSPECTOR
    public int sortOrder; 

    public virtual bool Use(Pokemon pokemon)
    {
        return false; 
    }
}