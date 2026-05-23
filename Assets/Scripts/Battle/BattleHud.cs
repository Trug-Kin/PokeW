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
    [SerializeField] BuffDebuffUI statusUI; 

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
        
        // Cập nhật UI trạng thái ngay khi load HUD
        UpdateStatusIcons();
    }
    
    // MỚI: Hàm chuyên dụng để gọi cập nhật UI Trạng thái
    public void UpdateStatusIcons()
    {
        if (statusUI != null && _pokemon != null)
        {
            statusUI.UpdateStatusUI(_pokemon);
        }
    }

    public IEnumerator UpdateHP()
    {
        yield return hpBar.SetHPSmooth((float)_pokemon.HP / _pokemon.MaxHp);
    }
}