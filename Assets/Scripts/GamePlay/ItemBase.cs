using UnityEngine;

public abstract class ItemBase : ScriptableObject
{
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;
    public int price;

    // Hàm ?o (virtual) ?? các v?t ph?m con t? ??nh ngh?a cách chúng ho?t ??ng
    // Tr? v? true n?u s? d?ng thành công, false n?u không th? dùng
    public virtual bool Use(Pokemon pokemon)
    {
        return false; // Mặc định không làm gì
    }
}