using UnityEngine;

[CreateAssetMenu(fileName = "New Stat Item", menuName = "Items/Stat Item")]
public class StatItem : ItemBase
{
    public bool isRareCandy;
    // Có thể thêm các biến khác như: int attackBoost, speedBoost...

    public override bool Use(Pokemon targetPokemon)
    {
        if (isRareCandy)
        {
            // Kiểm tra xem đã đạt cấp tối đa chưa (ví dụ Lv 100)
            if (targetPokemon.Level < 100)
            {
                targetPokemon.LevelUp();
                return true;
            }
        }
        return false;
    }
}