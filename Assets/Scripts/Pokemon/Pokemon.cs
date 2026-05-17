using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;
    
    public int HP { get; set; }
    public PokemonBase Base { get { return _base; } }
    public int Level { get { return level; } }
    public List<Move> Moves { get; set; }
    public ConditionID Status;
    public void Init()
    {

        HP = MaxHp;

        Moves = new List<Move>();
       foreach (var move in _base.LearnableMoves)
        {
            // ÁO GIÁP 1: Nếu chiêu thức bị lỗi hoặc trống, lập tức bỏ qua và xét chiêu tiếp theo!
            if (move.MoveBase == null) 
            {
                Debug.LogWarning($"[Cảnh báo] Cấp {move.Level} của {Base.Name} có chiêu bị rỗng, đang bỏ qua!");
                continue; 
            }

            if (move.Level <= Level)
                Moves.Add(new Move(move.MoveBase));

            if (Moves.Count >= 4)
                break;
        }
        Debug.Log($"[KIỂM TRA] Ếch Kì Cục cấp {Level} đã nạp thành công {Moves.Count} chiêu!");
    }

    public int Attack
    {
        get { return Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5; }
    }
    public int Defense
    {
        get { return Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5; }
    }
    public int SpAttack
    {
        get { return Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5; }
    }
    public int SpDefense
    {
        get { return Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5; }
    }
    public int Speed
    {
        get { return Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5; }
    }
    public int MaxHp
    {
        get { return Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10 + Level; }
    }
    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        var damageDetails = new DamageDetails();

        // --------- Critical Hit ----------
        float critical = 1f;
        if (Random.value <= 0.0625f)
            critical = 2f;

        // --------- Type Effectiveness ----------
        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) *
                      TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        damageDetails.TypeEffectiveness = type;
        damageDetails.Critical = critical;
        damageDetails.Fainted = false;

        // --------- Choose attack/defense stat based on move category ----------
        float attack = (move.Base.IsSpecial) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.IsSpecial) ? this.SpDefense : this.Defense;

        // --------- Random Modifier ----------
        float modifiers = Random.Range(0.85f, 1f) * type * critical;

        // --------- Damage Formula ----------
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * (attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        // --------- Apply Damage ----------
        HP -= damage;
        if (HP <= 0)
        {
            HP = 0;
            damageDetails.Fainted = true;
        }

        return damageDetails;
    }
    
    public Move GetRanDomMove ()
    {
        if (Moves == null || Moves.Count == 0)
            return null;

        int r = Random.Range(0, Moves.Count);
        return Moves[r];
    }

    public void Heal(int amount)
    {
        HP += amount;
        if (HP > MaxHp) HP = MaxHp;
        Debug.Log($"{Base.Name} ???c h?i m�u! HP hi?n t?i: {HP}/{MaxHp}");
    }

    // H�m gi?i hi?u ?ng
    public void CureStatus()
    {
        Status = ConditionID.None;
        Debug.Log($"{Base.Name} ?� ???c gi?i tr? m?i tr?ng th�i x?u!");
    }

    // H�m t?ng c?p
    public void LevelUp()
    {
        level++;
        Debug.Log($"{Base.Name} ?� t?ng l�n c?p {level}!");
    }

}
public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}


