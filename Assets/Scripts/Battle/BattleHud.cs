using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPbar hpBar;

    // 🔥 KHÔI PHỤC: Khung hiển thị Icon Ngũ Hành
    [Header("--- HỆ NGŨ HÀNH ---")]
    [SerializeField] Image typeIcon; 
    [SerializeField] TypeColorConfig typeConfig; 

    [Header("UI Hiệu ứng")]
    [SerializeField] BuffDebuffUI statusUI; 

    Pokemon _pokemon;
    
    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;
        nameText.text = pokemon.Base.Name;
        levelText.text = "Lvl " + pokemon.Level;
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);

        // 🔥 KHÔI PHỤC: Lấy Icon Ngũ Hành dán lên UI
        if (typeConfig != null && typeIcon != null)
        {
            typeIcon.sprite = typeConfig.GetIconForType(pokemon.Base.Type);
            typeIcon.gameObject.SetActive(true);
        }

        UpdateStatusIcons();
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
}