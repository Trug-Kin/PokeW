using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BuffDebuffUI : MonoBehaviour
{
    [Header("DỮ LIỆU ICON")]
    [SerializeField] StatusIconConfig iconConfig; // Kéo file MasterStatusConfig vào đây

    [Header("CÁC Ô CHỨA TRÊN GIAO DIỆN")]
    [SerializeField] Image[] iconSlots; // Kéo 8 cái Image từ (0) đến (7) vào đây

    // Hàm này sẽ được gọi mỗi khi Pokemon có sự thay đổi về chỉ số hoặc bị dính hiệu ứng
   public void UpdateStatusUI(Pokemon pokemon)
    {
        if (pokemon == null) return;

        // Tạm thời tắt toàn bộ các ô Icon đi để dọn dẹp
        foreach (var slot in iconSlots)
        {
            slot.gameObject.SetActive(false);
        }

        int currentIndex = 0;

        // 2. Kiểm tra và hiển thị Trạng thái dị thường
        if (pokemon.Status != ConditionID.None)
        {
            Sprite statusIcon = iconConfig.GetIconForCondition(pokemon.Status);
            if (statusIcon != null && currentIndex < iconSlots.Length)
            {
                iconSlots[currentIndex].sprite = statusIcon;
                iconSlots[currentIndex].gameObject.SetActive(true);
                currentIndex++;
            }
        }

        // 3. Kiểm tra và hiển thị các bậc Buff/Debuff chỉ số
        if (pokemon.StatBoosts != null)
        {
            foreach (var kvp in pokemon.StatBoosts)
            {
                Stat stat = kvp.Key;
                int boostLevel = kvp.Value; 

                if (boostLevel != 0 && currentIndex < iconSlots.Length)
                {
                    bool isBuff = boostLevel > 0;
                    Sprite statIcon = iconConfig.GetIconForStat(stat, isBuff);

                    if (statIcon != null)
                    {
                        iconSlots[currentIndex].sprite = statIcon;
                        iconSlots[currentIndex].gameObject.SetActive(true);
                        currentIndex++;
                    }
                }
            }
        }

        // 🔥 QUYẾT ĐỊNH BẬT HAY TẮT OBJECT CHA
        // Nếu có ít nhất 1 icon được dùng (currentIndex > 0), bật hiển thị khung Debuff/Buff lên. 
        // Nếu không có gì, tắt luôn khung đi cho gọn màn hình.
        gameObject.SetActive(currentIndex > 0);
    }
}