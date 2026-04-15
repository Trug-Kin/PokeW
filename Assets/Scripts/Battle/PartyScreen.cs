using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;
    [SerializeField] PartyMember[] memberSlots;
    List<Pokemon> pokemons;

    public void Init()
    {
        // Lấy TẤT CẢ các PartyMember có trong UI (kể cả những cái đang bị ẩn)
        memberSlots = GetComponentsInChildren<PartyMember>(true);
    }

    public void SetPartyData(List<Pokemon> pokemons)
    {
        this.pokemons = pokemons;
        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < pokemons.Count)
            {
                // SỬA LỖI 1: Phải bật hiển thị lại ô Pokemon lên!
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].SetData(pokemons[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }

        messageText.text = "Chọn một Pokemon";
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        // SỬA LỖI 2: Lặp theo độ dài của mảng UI (memberSlots.Length) thay vì số lượng Pokemon
        // Điều này chặn đứng 100% khả năng bị lỗi IndexOutOfRangeException
        for (int i = 0; i < memberSlots.Length; i++)
        {
            // Nếu chỉ số i vượt quá số lượng Pokemon đang có thì bỏ qua
            if (i >= pokemons.Count) continue;

            if (i == selectedMember)
                memberSlots[i].SetSelected(true);
            else
                memberSlots[i].SetSelected(false);
        }
    }

    public void MessageText(string message)
    {
        messageText.text = message;
    }
}