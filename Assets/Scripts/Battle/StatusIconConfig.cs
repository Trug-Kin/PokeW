using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StatusIconConfig", menuName = "Pokemon/Status Icon Configuration")]
public class StatusIconConfig : ScriptableObject
{
    // ==========================================
    // 1. DỮ LIỆU CHO TRẠNG THÁI DỊ THƯỜNG (Độc, Cháy, Choáng...)
    // ==========================================
    [System.Serializable]
    public class ConditionUI
    {
        public ConditionID conditionID; // Ví dụ: Poison, Burn
        public Sprite icon;             // Ảnh Icon hiển thị
        public Color color = Color.white; // Màu chữ (nếu bạn muốn dùng text thay vì icon)
    }

    // ==========================================
    // 2. DỮ LIỆU CHO BUFF / DEBUFF CHỈ SỐ (Tăng/Giảm Công, Thủ...)
    // ==========================================
    [System.Serializable]
    public class StatBoostUI
    {
        public Stat stat;               // Ví dụ: Attack, Defense
        public Sprite buffIcon;         // Icon Mũi tên hướng lên màu Xanh 
        public Sprite debuffIcon;       // Icon Mũi tên hướng xuống màu Đỏ
    }

    [Header("Danh sách Icon Trạng Thái Dị Thường")]
    public List<ConditionUI> conditionUIList;

    [Header("Danh sách Icon Buff/Debuff Chỉ Số")]
    public List<StatBoostUI> statBoostUIList;


    // --- CÁC HÀM XUẤT DỮ LIỆU RA NGOÀI ---

    // Lấy Icon cho trạng thái dị thường
    public Sprite GetIconForCondition(ConditionID id)
    {
        foreach (var c in conditionUIList)
        {
            if (c.conditionID == id) return c.icon;
        }
        return null; // Trả về null nếu không tìm thấy (ví dụ trạng thái None)
    }

    // Lấy Màu cho trạng thái dị thường (Dùng để tô màu thanh máu hoặc text)
    public Color GetColorForCondition(ConditionID id)
    {
        foreach (var c in conditionUIList)
        {
            if (c.conditionID == id) return c.color;
        }
        return Color.white; 
    }

    // Lấy Icon cho Buff/Debuff chỉ số
    // Tham số isBuff: truyền true nếu là tăng bậc (buff), false nếu giảm bậc (debuff)
    public Sprite GetIconForStat(Stat stat, bool isBuff)
    {
        foreach (var s in statBoostUIList)
        {
            if (s.stat == stat)
            {
                return isBuff ? s.buffIcon : s.debuffIcon;
            }
        }
        return null;
    }
}