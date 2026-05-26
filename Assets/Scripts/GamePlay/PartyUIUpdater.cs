using System.Collections.Generic;
using UnityEngine;

public class PartyUIUpdater : MonoBehaviour
{
    // Đã bỏ [SerializeField] nên cậu sẽ không thấy mảng này trên Unity nữa (Khỏi cần kéo thả)
    private PartyMemberUI[] memberSlots;
    private PokemonParrty playerParty;

    private void Awake()
    {
        // 1. TỰ ĐỘNG QUÉT: Lấy toàn bộ các ô PartyMemberUI đang là con của Panel này
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);

        // 2. Tìm nhân vật chính để lấy đội hình
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerParty = player.GetComponent<PokemonParrty>();
        }
    }

    private void OnEnable()
    {
        UpdatePartyUI();
    }

    public void UpdatePartyUI()
    {
        if (playerParty == null || memberSlots == null) return;

        List<Pokemon> pokemons = playerParty.Pokemons;

        for (int i = 0; i < memberSlots.Length; i++)
        {
            // Nếu có Pokemon ở vị trí này thì bật ô đó lên và truyền dữ liệu
            if (i < pokemons.Count && pokemons[i] != null && pokemons[i].Base != null)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].SetData(pokemons[i]);
            }
            else
            {
                // Nếu dư ô (Ví dụ team có 1 con, mà có 6 ô) thì ẩn 5 ô kia đi
                memberSlots[i].gameObject.SetActive(false);
            }
        }
    }
}