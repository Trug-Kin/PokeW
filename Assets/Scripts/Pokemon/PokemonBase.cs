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
    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    // Base stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    [SerializeField] List<LearnableMove> learnableMoves;

    public string Name
        {
            get { return name; }
    }
    public string Description
        {
        get { return description; }
    }
    public Sprite FrontSprite
        {
        get { return frontSprite; }
    }
    public Sprite BackSprite
        {
        get { return backSprite; }
    }
    public PokemonType Type1
        {
        get { return type1; }
    }
    public PokemonType Type2
        {
        get { return type2; }
    }
    public int MaxHp
        {
        get { return maxHp; }
    }
    public int Attack
        {
        get { return attack; }
    }
    public int Defense
        {
        get { return defense; }
    }
    public int SpAttack
        {
        get { return spAttack; }
    }
    public int SpDefense
        {
        get { return spDefense; }
    }
    public int Speed
        {
        get { return speed; }
    }
    public List<LearnableMove> LearnableMoves
        {
        get { return learnableMoves; }
    }

}
    

[System.Serializable]
    public class LearnableMove
    {
        [SerializeField] MoveBase moveBase;
        [SerializeField] int level;
        public MoveBase MoveBase
        {
            get { return moveBase; }
        }
        public int Level
        {
            get { return level; }
        }
}


public enum PokemonType
{
    None,
    Fire,
    Water,
    Grass,
    Electric,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon,
    Dark,

}
public class TypeChart
{
    // The enum (excluding None) currently has 16 types. Provide a 16x16 matrix
    // initialized to 1f so indexing is safe. Fill with correct values later.
    static float[][] chart = new float[16][]
    {
        // Fire
        new float[16] {0.5f,0.5f,2f,1f,2f,1f,1f,1f,1f,1f,2f,0.5f,1f,0.5f,1f,1f},
        // Water
        new float[16] {2f,0.5f,0.5f,1f,1f,1f,1f,2f,1f,1f,1f,2f,1f,0.5f,1f,1f},
        // Grass
        new float[16] {0.5f,2f,0.5f,1f,1f,1f,0.5f,2f,0.5f,1f,0.5f,2f,1f,0.5f,1f,1f},
        // Electric
        new float[16] {1f,2f,0.5f,0.5f,1f,1f,1f,0f,2f,1f,1f,1f,1f,0.5f,1f,1f},
        // Ice
        new float[16] {0.5f,0.5f,2f,1f,0.5f,1f,1f,2f,2f,1f,1f,1f,1f,2f,1f,1f},
        // Fighting
        new float[16] {1f,1f,1f,1f,2f,1f,0.5f,1f,0.5f,0.5f,0.5f,2f,0f,1f,2f,1f},
        // Poison
        new float[16] {1f,1f,2f,1f,1f,1f,0.5f,0.5f,1f,1f,1f,0.5f,0.5f,1f,1f,1f},
        // Ground
        new float[16] {2f,1f,0.5f,2f,1f,1f,2f,1f,0f,1f,0.5f,2f,1f,1f,1f,1f},
        // Flying
        new float[16] {1f,1f,2f,0.5f,1f,2f,1f,1f,1f,1f,2f,0.5f,1f,1f,1f,1f},
        // Psychic
        new float[16] {1f,1f,1f,1f,1f,2f,2f,1f,1f,0.5f,1f,1f,1f,1f,0f,1f},
        // Bug
        new float[16] {0.5f,1f,2f,1f,1f,0.5f,0.5f,1f,0.5f,2f,1f,1f,0.5f,1f,2f,1f},
        // Rock
        new float[16] {2f,1f,1f,1f,2f,0.5f,1f,0.5f,2f,1f,2f,1f,1f,1f,1f,1f},
        // Ghost
        new float[16] {1f,1f,1f,1f,1f,0f,1f,1f,1f,2f,1f,1f,2f,1f,0.5f,1f},
        // Dragon
        new float[16] {1f,1f,1f,1f,1f,1f,1f,1f,1f,1f,1f,1f,1f,2f,1f,1f},
        // Dark
        new float[16] {1f,1f,1f,1f,1f,0.5f,1f,1f,1f,2f,1f,1f,2f,1f,0.5f,1f},
        // Extra neutral row (fallback)
        new float[16] {1f,1f,1f,1f,1f,1f,1f,1f,1f,1f,1f,1f,1f,1f,1f,1f},
    };

    public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
    {
        if (attackType == PokemonType.None || defenseType == PokemonType.None)
            return 1f;

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        if (row < 0 || row >= chart.Length || col < 0 || col >= chart[row].Length)
            return 1f;

        return chart[row][col];
    }
}
