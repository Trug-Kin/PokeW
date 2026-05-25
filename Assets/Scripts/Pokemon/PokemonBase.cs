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
    
    // 🔥 ĐÃ SỬA: Mỗi Pokemon giờ chỉ có 1 hệ duy nhất
    [SerializeField] PokemonType type;

    // Base stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    // 🔥 THÊM MỚI: EXP gốc quái rớt ra khi bị hạ
    [Header("--- Kinh nghiệm (EXP) ---")]
    [SerializeField] int expYield; 

    [SerializeField] List<LearnableMove> learnableMoves;

    public string Name { get { return name; } }
    public string Description { get { return description; } }
    public Sprite FrontSprite { get { return frontSprite; } }
    public Sprite BackSprite { get { return backSprite; } }
    
    // 🔥 Cập nhật Getter thành hệ đơn
    public PokemonType Type { get { return type; } }
    
    public int MaxHp { get { return maxHp; } }
    public int Attack { get { return attack; } }
    public int Defense { get { return defense; } }
    public int SpAttack { get { return spAttack; } }
    public int SpDefense { get { return spDefense; } }
    public int Speed { get { return speed; } }
    public List<LearnableMove> LearnableMoves { get { return learnableMoves; } }
    
    // 🔥 THÊM MỚI: Getter cho ExpYield
    public int ExpYield { get { return expYield; } } 

    // 🔥 THÊM MỚI: Hàm tính tổng EXP cần cho một cấp độ (Công thức: Level^3)
    public int GetExpForLevel(int level)
    {
        return level * level * level;
    }
}
    
[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;
    public MoveBase MoveBase { get { return moveBase; } }
    public int Level { get { return level; } }
}

// 🔥 ĐÃ SỬA: Đổi toàn bộ hệ thành Ngũ Hành
public enum PokemonType
{
    None, // Hệ mặc định không sinh khắc
    Kim,
    Thuy,
    Moc,
    Hoa,
    Tho
}

public class TypeChart
{
    // Ma trận 5x5 tương ứng với 5 hệ: Kim (0), Thủy (1), Mộc (2), Hỏa (3), Thổ (4)
    static float[][] chart = new float[5][]
    {
        // CỘT: Hệ phòng thủ (Bị đánh)
        // Kim, Thủy, Mộc, Hỏa, Thổ
        /* HÀNG: Kim tấn công  */ new float[5] { 1f,   0.5f, 2f,   1f,   1f }, 
        /* HÀNG: Thủy tấn công */ new float[5] { 1f,   1f,   0.5f, 2f,   1f }, 
        /* HÀNG: Mộc tấn công  */ new float[5] { 1f,   1f,   1f,   0.5f, 2f }, 
        /* HÀNG: Hỏa tấn công  */ new float[5] { 2f,   1f,   1f,   1f,   0.5f},
        /* HÀNG: Thổ tấn công  */ new float[5] { 0.5f, 2f,   1f,   1f,   1f }  
    };

    public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
    {
        if (attackType == PokemonType.None || defenseType == PokemonType.None)
            return 1f;

        // Trừ 1 vì Enum có giá trị None ở vị trí số 0
        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        if (row < 0 || row >= chart.Length || col < 0 || col >= chart[row].Length)
            return 1f;

        return chart[row][col];
    }
}