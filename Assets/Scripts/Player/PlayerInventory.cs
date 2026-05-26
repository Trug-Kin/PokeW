using System.Collections.Generic;
using UnityEngine;

// KHÔNG KHAI BÁO CLASS ItemSlot Ở ĐÂY NỮA (VÌ DÙNG CHUNG VỚI BATTLE/UI RỒI)

public class PlayerInventory : MonoBehaviour
{
    [Header("Túi đồ của nhân vật (Thực)")]
    // Gọi thẳng ItemSlot gốc của project ra xài
    public List<ItemSlot> inventory = new List<ItemSlot>(); 
}