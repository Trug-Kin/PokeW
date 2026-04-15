using UnityEngine;

[CreateAssetMenu(fileName = "New Healing Item", menuName = "Items/Healing Item")]
public class RecoveryItem : ItemBase
{
    [Header("Hồi phục")]
    public int hpRestoreAmount;
    public bool restoreMaxHP;

    [Header("Giải hiệu ứng")]
    public ConditionID statusToCure; // ConditionID là một Enum chứa: Burn, Poison, Sleep, Paralysis...
    public bool cureAllStatus;

    public override bool Use(Pokemon pokemon)
    {
        bool itemUsed = false;

        // 1. Logic Hồi HP
        if (hpRestoreAmount > 0 || restoreMaxHP)
        {
            if (pokemon.HP < pokemon.MaxHp)
            {
                int healAmount = restoreMaxHP ? pokemon.MaxHp : hpRestoreAmount;
                pokemon.Heal(healAmount);
                itemUsed = true;
            }
        }

        // 2. Logic Giải hiệu ứng
        if (cureAllStatus || pokemon.Status == statusToCure)
        {
            if (pokemon.Status != ConditionID.None)
            {
                pokemon.CureStatus();
                itemUsed = true;
            }
        }

        return itemUsed;
    }
}