using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/Create new move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description; // Đã sửa: [TextArea] dùng cho chuỗi văn bản mô tả

    [SerializeField] PokemonType type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int pp;
    
    // ĐÃ SỬA: Cho phép bạn tick chọn chiêu này là Sát thương Đặc biệt (Phép) hay Vật lý ngay trên Unity
    [SerializeField] bool isSpecial; 

    // Đã xóa typeIcon để dùng chung kho MasterTypeConfig

    public string Name
    {
        get { return name; }
    }
    
    public string Description
    {
        get { return description; }
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
        get { return isSpecial; }
    }
}