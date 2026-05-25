using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPbar hpBar;

    [Header("--- HỆ NGŨ HÀNH ---")]
    [SerializeField] Image typeIcon; 
    [SerializeField] TypeColorConfig typeConfig; 

    [Header("UI Hiệu ứng")]
    [SerializeField] BuffDebuffUI statusUI; 

    // 🔥 THÊM MỚI: Khai báo UI EXP
    [Header("--- KINH NGHIỆM (EXP) ---")]
    [SerializeField] GameObject expBar; // Kéo object "EXP" (phần có màu) vào đây
    [SerializeField] Text expText;      // Kéo object "EXPText" vào đây (nếu có)

    Pokemon _pokemon;
    
    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;
        nameText.text = pokemon.Base.Name;
        levelText.text = "Lvl " + pokemon.Level;
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);

        if (typeConfig != null && typeIcon != null)
        {
            typeIcon.sprite = typeConfig.GetIconForType(pokemon.Base.Type);
            typeIcon.gameObject.SetActive(true);
        }

        UpdateStatusIcons();
        SetExp(); // Gọi hàm set độ dài thanh EXP lúc mới xuất trận
    }
    
    public IEnumerator UpdateHP()
    {
        yield return hpBar.SetHPSmooth((float)_pokemon.HP / _pokemon.MaxHp);
    }

    public void UpdateStatusIcons()
    {
        if (statusUI != null && _pokemon != null)
        {
            statusUI.UpdateStatusUI(_pokemon);
        }
    }

    // ==========================================
    // 🔥 CÁC HÀM XỬ LÝ THANH EXP BÊN DƯỚI NÀY 🔥
    // ==========================================

    // Tính toán phần trăm (từ 0 đến 1) của thanh EXP hiện tại
    float GetNormalizedExp()
    {
        int currLevelExp = _pokemon.Base.GetExpForLevel(_pokemon.Level);
        int nextLevelExp = _pokemon.Base.GetExpForLevel(_pokemon.Level + 1);
        float normalizedExp = (float)(_pokemon.Exp - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }

    // Thiết lập độ dài thanh EXP lập tức (không có hiệu ứng)
    public void SetExp()
    {
        if (expBar == null) return;
        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
        
        if (expText != null)
        {
            int currLevelExp = _pokemon.Base.GetExpForLevel(_pokemon.Level);
            int nextLevelExp = _pokemon.Base.GetExpForLevel(_pokemon.Level + 1);
            expText.text = $"{_pokemon.Exp - currLevelExp} / {nextLevelExp - currLevelExp}";
        }
    }

    // Hiệu ứng chạy thanh EXP từ từ mượt mà
    public IEnumerator SetExpSmooth(bool reset = false)
    {
        if (expBar == null) yield break;

        // Nếu vừa lên cấp, kéo tụt thanh EXP về 0 ngay lập tức trước khi chạy tiếp
        if (reset)
            expBar.transform.localScale = new Vector3(0, 1, 1);

        float normalizedExp = GetNormalizedExp();
        Vector3 currentScale = expBar.transform.localScale;

        // Cập nhật số Text
        if (expText != null)
        {
            int currLevelExp = _pokemon.Base.GetExpForLevel(_pokemon.Level);
            int nextLevelExp = _pokemon.Base.GetExpForLevel(_pokemon.Level + 1);
            expText.text = $"{_pokemon.Exp - currLevelExp} / {nextLevelExp - currLevelExp}";
        }

        // Vòng lặp thu/phóng thanh màu
        while (Mathf.Abs(currentScale.x - normalizedExp) > 0.01f)
        {
            // Tốc độ chạy thanh exp: 1.5f (bạn có thể đổi to hơn để chạy nhanh hơn)
            currentScale.x = Mathf.MoveTowards(currentScale.x, normalizedExp, Time.deltaTime * 1.5f);
            expBar.transform.localScale = currentScale;
            yield return null;
        }

        // Chốt lại kích thước chuẩn
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
    }
    
    // 🔥 THÊM MỚI: Hàm update text Lvl khi lên cấp
    public void UpdateLevelText()
    {
        levelText.text = "Lvl " + _pokemon.Level;
    }
}