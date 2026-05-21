using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new Pokemon")] 
public class PokemonBase : ScriptableObject
{
    [SerializeField] string name;
    [TextArea]
    [SerializeField] string description;
    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;
    
    // Đã gộp thành 1 hệ duy nhất
    [SerializeField] PokemonType type;

    // Base stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    [SerializeField] List<LearnableMove> learnableMoves;

    public string Name { get { return name; } }
    public string Description { get { return description; } }
    public Sprite FrontSprite { get { return frontSprite; } }
    public Sprite BackSprite { get { return backSprite; } }
    
    // Thuộc tính gọi hệ duy nhất
    public PokemonType Type { get { return type; } }
    
    public int MaxHp { get { return maxHp; } }
    public int Attack { get { return attack; } }
    public int Defense { get { return defense; } }
    public int SpAttack { get { return spAttack; } }
    public int SpDefense { get { return spDefense; } }
    public int Speed { get { return speed; } }
    public List<LearnableMove> LearnableMoves { get { return learnableMoves; } }
}
    
[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;
    
    public MoveBase MoveBase { get { return moveBase; } }
    public int Level { get { return level; } }
}

// Danh sách hệ đã được chuyển sang Ngũ Hành
public enum PokemonType
{
    None,
    Kim,
    Moc,
    Thuy,
    Hoa,
    Tho
}

public class TypeChart
{
    static float[][] chart = new float[][]
    {
                     // BÊN BỊ ĐÁNH:
                     // Kim   Mộc   Thủy  Hỏa   Thổ
        /* ĐÁNH: Kim  */ new float[] {0.5f, 2.0f, 0.5f, 0.5f, 1.0f},
        /* ĐÁNH: Mộc  */ new float[] {0.5f, 0.5f, 1.0f, 0.5f, 2.0f},
        /* ĐÁNH: Thủy */ new float[] {1.0f, 0.5f, 0.5f, 2.0f, 0.5f},
        /* ĐÁNH: Hỏa  */ new float[] {2.0f, 1.0f, 0.5f, 0.5f, 0.5f},
        /* ĐÁNH: Thổ  */ new float[] {0.5f, 0.5f, 2.0f, 1.0f, 0.5f}
    };

    public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
    {
        if (attackType == PokemonType.None || defenseType == PokemonType.None)
            return 1f;

        // Trừ 1 vì hệ None có index là 0, Kim bắt đầu từ 1
        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        if (row < 0 || row >= chart.Length || col < 0 || col >= chart[row].Length)
            return 1f;

        return chart[row][col];
    }
}