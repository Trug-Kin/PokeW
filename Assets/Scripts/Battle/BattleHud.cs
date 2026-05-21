using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPbar hpBar;

    [Header("--- HỆ NGŨ HÀNH ---")]
    [SerializeField] Image typeIcon; // Chỉ còn 1 Icon hệ
    [SerializeField] TypeColorConfig typeConfig; 

    Pokemon _pokemon;
    
    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;
        nameText.text = pokemon.Base.Name;
        levelText.text = "Lvl " + pokemon.Level;
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);

        // Cập nhật Icon hệ duy nhất
        if (typeConfig != null && typeIcon != null)
        {
            typeIcon.sprite = typeConfig.GetIconForType(pokemon.Base.Type);
            typeIcon.gameObject.SetActive(true);
        }
    }
    
    public IEnumerator UpdateHP()
    {
        yield return hpBar.SetHPSmooth((float)_pokemon.HP / _pokemon.MaxHp);
    }
}