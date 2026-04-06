using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour 
{
    [SerializeField] Text messageText;

    PartyMember[] memberSlots;
    List<Pokemon> pokemons;

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMember>();
    }
    public void SetPartyData(List<Pokemon> pokemons)
    {
        this.pokemons = pokemons;
        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < pokemons.Count)
                memberSlots[i].SetData(pokemons[i]);
            else
                memberSlots[i].gameObject.SetActive(false);
        }

        messageText.text = "Chọn một Pokemon";
    }
    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            if (i == selectedMember)
            
                memberSlots[i].SetSelected(true);
               else
                    memberSlots[i].SetSelected(false);     
        }
    }
    public void MessageText (string message)
    {
        messageText.text = message;
    }
}
