using UnityEngine;
using TMPro;      // Dùng cho TextMeshPro
using UnityEngine.UI; // Bắt buộc phải có cái này để dùng Image

public class PartyMemberUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Image hpBar; // Đã đổi thành Image theo đúng thiết kế của team

    public void SetData(Pokemon pokemon)
    {
        if (pokemon == null || pokemon.Base == null) return;

        // 1. Gán chữ (Text)
        nameText.text = pokemon.Base.Name;
        levelText.text = "Cấp độ: " + pokemon.Level;

        // 2. Tính toán và gán ảnh thanh máu (Image)
        // Bắt buộc phải ép kiểu (float) để không bị lỗi chia số nguyên ra 0
        hpBar.fillAmount = (float)pokemon.HP / pokemon.MaxHp;
    }
}