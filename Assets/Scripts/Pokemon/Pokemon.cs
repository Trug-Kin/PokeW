using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;
    
    // --- 1. KHAI BÁO SỔ TAY GHI NHỚ ---
    public Dictionary<Stat, int> StatBoosts { get; private set; }

    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;
        Init(); 
    }

    public int HP { get; set; }
    public PokemonBase Base { get { return _base; } }
    public int Level { get { return level; } }
    public List<Move> Moves { get; set; }
    public ConditionID Status;

    public void Init()
    {
        HP = MaxHp;
        Moves = new List<Move>();

        // --- 2. KHỞI TẠO SỔ TAY KHI MỚI RA TRẬN ---
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.Attack, 0},
            {Stat.Defense, 0},
            {Stat.SpAttack, 0},
            {Stat.SpDefense, 0},
            {Stat.Speed, 0}
        };

       foreach (var move in _base.LearnableMoves)
        {
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
        Debug.Log($"[KIỂM TRA] {Base.Name} cấp {Level} đã nạp thành công {Moves.Count} chiêu!");
    }

    // --- 3. HÀM TÍNH TOÁN CHỈ SỐ THỰC TẾ (CÓ BUFF/DEBUFF) ---
    int GetStat(int baseStatValue, Stat stat)
    {
        int statVal = baseStatValue;
        int boost = StatBoosts[stat];

        float[] multipliers = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0)
            statVal = Mathf.FloorToInt(statVal * multipliers[boost]);
        else
            statVal = Mathf.FloorToInt(statVal / multipliers[-boost]);

        return statVal;
    }

    // Các Getter đã được bọc qua hàm GetStat để tự động nhận Buff
    public int Attack { get { return GetStat(Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5, Stat.Attack); } }
    public int Defense { get { return GetStat(Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5, Stat.Defense); } }
    public int SpAttack { get { return GetStat(Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5, Stat.SpAttack); } }
    public int SpDefense { get { return GetStat(Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5, Stat.SpDefense); } }
    public int Speed { get { return GetStat(Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5, Stat.Speed); } }
    
    // MaxHp không bị ảnh hưởng bởi Buff/Debuff bậc
    public int MaxHp { get { return Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10 + Level; } }

    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        var damageDetails = new DamageDetails();

        float critical = 1f;
        if (Random.value <= 0.0625f)
            critical = 2f;

        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type);

        damageDetails.TypeEffectiveness = type;
        damageDetails.Critical = critical;
        damageDetails.Fainted = false;

        // Dùng chỉ số Attack/Defense thực tế (đã được tự động tính Buff/Debuff qua Getter)
        float attack = (move.Base.IsSpecial) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.IsSpecial) ? this.SpDefense : this.Defense;

        float modifiers = Random.Range(0.85f, 1f) * type * critical;

        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * (attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        HP -= damage;
        if (HP <= 0)
        {
            HP = 0;
            damageDetails.Fainted = true;
        }

        return damageDetails;
    }

    // --- 4. HÀM NHẬN HIỆU ỨNG TỪ CHIÊU THỨC ---
    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            // Chốt chặn bậc tối đa là +6 và tối thiểu là -6
            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);
            Debug.Log($"{Base.Name} bị thay đổi {stat} một lượng {boost} bậc!");
        }
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
        Debug.Log($"{Base.Name} được hồi máu! HP hiện tại: {HP}/{MaxHp}");
    }

    public void CureStatus()
    {
        Status = ConditionID.None;
        Debug.Log($"{Base.Name} đã được giải trừ mọi trạng thái xấu!");
    }

    public void LevelUp()
    {
        level++;
        Debug.Log($"{Base.Name} đã tăng lên cấp {level}!");
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}