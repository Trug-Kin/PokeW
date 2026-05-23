using UnityEngine;
using System.Collections.Generic; 

[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/Create new move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description; 

    [SerializeField] PokemonType type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int pp;
    [SerializeField] bool isSpecial; 

    // CHỈ GIỮ LẠI EFFECTS, BỎ TARGET CHUNG ĐI
    [Header("Hiệu ứng kỹ năng (Buff/Debuff)")]
    [SerializeField] MoveEffects effects;

    // Các Getter
    public string Name { get { return name; } }
    public string Description { get { return description; } }
    public PokemonType Type { get { return type; } }
    public int Power { get { return power; } }
    public int Accuracy { get { return accuracy; } }
    public int PP { get { return pp; } }
    public bool IsSpecial { get { return isSpecial; } }

    public MoveEffects Effects { get { return effects; } }
}

// ==========================================
// CÁC ENUM VÀ CLASS PHỤ TRỢ
// ==========================================

public enum MoveTarget 
{ 
    Foe,    // Kẻ địch
    Self    // Bản thân
}

public enum Stat 
{ 
    Attack, 
    Defense, 
    SpAttack, 
    SpDefense, 
    Speed 
}

[System.Serializable]
public class StatBoost
{
    public Stat stat;
    public int boost; // Số bậc (>0 là Buff, <0 là Debuff)
    
    public MoveTarget target; // 🔥 Target độc lập
    
    [Header("Tỷ lệ trúng hiệu ứng (0-100%)")]
    [Range(0, 100)] public int chance = 100; 
}

[System.Serializable]
public class StatusEffect
{
    public ConditionID id;
    
    public MoveTarget target; // 🔥 Target độc lập
    
    [Header("Tỷ lệ dính trạng thái (0-100%)")]
    [Range(0, 100)] public int chance = 100; 
}

[System.Serializable]
public class MoveEffects
{
    [Header("Tăng/Giảm Chỉ Số")]
    public List<StatBoost> boosts; 

    [Header("Trạng Thái Dị Thường")]
    public List<StatusEffect> statuses; 
}