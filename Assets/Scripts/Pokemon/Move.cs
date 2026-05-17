using UnityEngine;

public class Move
{
    public MoveBase Base { get; set; }
    public int PP { get; set; }

    public Move(MoveBase pBase)
    {
        Base = pBase;
        
        // ÁO GIÁP 2: Chỉ lấy PP khi chắc chắn pBase có tồn tại
        if (pBase != null)
        {
            PP = pBase.PP;
        }
        else 
        {
            PP = 0; // Bọc lót tránh Crash ngầm
        }
    }
}