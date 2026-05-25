using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;
    
    public Dictionary<Stat, int> StatBoosts { get; private set; }

    public int HP { get; set; }
    
    public int Exp { get; set; }

    public PokemonBase Base 
    { 
        get { return _base; } 
        set { _base = value; } 
    }
    
    public int Level { get { return level; } }
    public List<Move> Moves { get; set; }
    
    // 🔥 Biến lưu trữ trạng thái dị thường của Pokemon
    public ConditionID Status;
    
    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;

        Init();
    }
    
    public void Init()
    {
        HP = MaxHp;
        Moves = new List<Move>();

        Exp = Base.GetExpForLevel(level);

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
    }

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

    public int Attack { get { return GetStat(Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5, Stat.Attack); } }
    public int Defense { get { return GetStat(Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5, Stat.Defense); } }
    public int SpAttack { get { return GetStat(Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5, Stat.SpAttack); } }
    public int SpDefense { get { return GetStat(Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5, Stat.SpDefense); } }
    public int Speed { get { return GetStat(Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5, Stat.Speed); } }
    
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

    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;
            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);
        }
    }
    
    public Move GetRanDomMove ()
    {
        if (Moves == null || Moves.Count == 0) return null;
        int r = Random.Range(0, Moves.Count);
        return Moves[r];
    }

    public void Heal(int amount)
    {
        HP += amount;
        if (HP > MaxHp) HP = MaxHp;
    }

    public void CureStatus()
    {
        Status = ConditionID.None;
    }

    public void LevelUp()
    {
        level++;
    }

    // ==========================================
    // 🔥 HỆ THỐNG TRẠNG THÁI DỊ THƯỜNG 🔥
    // ==========================================
    public void SetStatus(ConditionID conditionId)
    {
        if (Status != ConditionID.None) return; 
        Status = conditionId;
    }

    public bool OnBeforeMove(out string message)
    {
        message = "";
        
        if (Status == ConditionID.par)
        {
            if (Random.Range(1, 101) <= 25)
            {
                message = $"{Base.Name} đang bị choáng và không thể cử động!";
                return false; 
            }
        }
        else if (Status == ConditionID.frz)
        {
            if (Random.Range(1, 101) <= 20)
            {
                CureStatus();
                message = $"{Base.Name} đã phá vỡ lớp băng và rã đông!";
                return true; 
            }
            message = $"{Base.Name} đang bị đóng băng cứng ngắc!";
            return false; 
        }

        return true; 
    }

    public void OnAfterTurn(out string message, out int damage)
    {
        message = "";
        damage = 0;
        
        if (Status == ConditionID.brn)
        {
            damage = Mathf.CeilToInt(MaxHp * 0.03f);
            if (damage <= 0) damage = 1; 
            
            HP -= damage;
            if (HP < 0) HP = 0;
            
            message = $"{Base.Name} bị mất máu do vết thương thiêu đốt!";
        }
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}