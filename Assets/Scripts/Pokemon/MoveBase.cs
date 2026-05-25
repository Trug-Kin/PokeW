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

public enum MoveTarget 
{ 
    Foe,    
    Self    
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
    public int boost; 
    
    public MoveTarget target; 
    
    [Header("Tỷ lệ trúng hiệu ứng (0-100%)")]
    [Range(0, 100)] public int chance = 100; 
}

[System.Serializable]
public class StatusEffect
{
    public ConditionID id;
    
    public MoveTarget target; 
    
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

// 🔥 THÊM MỚI: Danh sách các Dị Thường
