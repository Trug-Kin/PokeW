using UnityEngine;
using System.Collections.Generic;
using System.Collections;

// 1. Trạng thái PartyScreen để game biết lúc nào đang mở túi ngoài Map
public enum GameState { FreeRoam, Battle, PartyScreen }

public class GameControl : MonoBehaviour
{
    [SerializeField] PlayerControl playerControl;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;

    // 2. Cầu nối tới giao diện màn hình Pokemon
    [SerializeField] PartyScreen partyScreen;

    GameState state;

    public void Start()
    {
        playerControl.OnEncountered += StartBattle;
        battleSystem.OnBattleOver += EndBattle;

        // Quét gom đủ 6 ô giao diện ngay khi game vừa bật
        partyScreen.Init();
    }

    void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        // get player party and a wild pokemon from the map, then start battle
        var playerParty = playerControl.GetComponent<PokemonParrty>();
        var wildPokemon = FindObjectOfType<MapArena>().GetRandomWildPokemon();

        battleSystem.StartBattle(playerParty, wildPokemon);
    }

    void EndBattle(bool won)
    {
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            playerControl.HandleUpdate();

            // 3. Bấm phím Enter (Return) để mở túi Pokemon lúc đang đi bộ ngoài thế giới
            if (Input.GetKeyDown(KeyCode.Return))
            {
                state = GameState.PartyScreen;
                var playerParty = playerControl.GetComponent<PokemonParrty>();

                // ĐÃ SỬA: Thêm "false" vì đang đi bộ ngoài Map, Pokémon hiện tại không bị hạ gục (Fainted)
                partyScreen.SetPartyData(playerParty.Pokemons, false); 
                
                // ĐÃ SỬA: Xóa hoàn toàn dòng UpdateMemberSelection(0) cũ vì hệ thống hiện tại đã chạy bằng chuột!

                // Bật hiển thị màn hình lên
                partyScreen.gameObject.SetActive(true);
            }
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if (state == GameState.PartyScreen)
        {
            // 4. Xử lý hành động khi túi Pokemon đang mở ngoài Map
            HandlePartyScreen();
        }
    }

    // 5. Hàm xử lý đóng màn hình Party khi đang ở ngoài thế giới
    void HandlePartyScreen()
    {
        // Bấm phím X để đóng túi và quay lại đi dạo tiếp tục
        if (Input.GetKeyDown(KeyCode.X))
        {
            partyScreen.gameObject.SetActive(false);
            state = GameState.FreeRoam;
        }
    }
}