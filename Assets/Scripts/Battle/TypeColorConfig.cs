using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TypeConfig", menuName = "Pokemon/Type Configuration")]
public class TypeColorConfig : ScriptableObject
{
    [System.Serializable]
    public class TypeUI
    {
        public PokemonType type;
        public Sprite typeIcon;
        public Color typeColor; 
    }

    public List<TypeUI> typeUIList;

    // Hàm xuất ảnh Icon dựa theo hệ
    public Sprite GetIconForType(PokemonType type)
    {
        foreach (var t in typeUIList)
        {
            if (t.type == type) return t.typeIcon;
        }
        return null;
    }
}