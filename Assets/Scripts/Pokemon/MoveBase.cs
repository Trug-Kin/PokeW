using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/Create new move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string name;
    [TextArea]
    [SerializeField] PokemonType type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int pp;
    [SerializeField] Sprite typeIcon; // Hình ảnh Logo hệ chiêu thức

    public string Name
    {
        get { return name; }
    }
   
    public PokemonType Type
    {
        get { return type; }
    }
    public int Power
    {
        get { return power; }
    }
    public int Accuracy
    {
        get { return accuracy; }
    }
    public int PP
    {
        get { return pp; }
    }
    public bool IsSpecial
    {
        get
        {
            if (type == PokemonType.Fire || type == PokemonType.Water || type == PokemonType.Grass ||
                type == PokemonType.Ice || type == PokemonType.Electric || type == PokemonType.Dragon)
            {
                return true;
            }
            else 
                return false;
        }
    }
    public Sprite TypeIcon
    {
        get { return typeIcon; }
    }
}
