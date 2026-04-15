using UnityEngine;
using System.Collections.Generic;
using System.Collections;

// 1. THÊM MỚI: Trạng thái PartyScreen để game biết lúc nào đang mở túi ngoài Map
public enum GameState { FreeRoam, Battle, PartyScreen }

public class GameControl : MonoBehaviour
{
    [SerializeField] PlayerControl playerControl;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;

    // 2. THÊM MỚI: Cầu nối tới giao diện màn hình Pokemon
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

            // 3. THÊM MỚI: Bấm phím Enter (Return) để mở túi Pokemon lúc đang đi bộ
            if (Input.GetKeyDown(KeyCode.Return))
            {
                state = GameState.PartyScreen;
                var playerParty = playerControl.GetComponent<PokemonParrty>();

                // Ép UI nạp Data mới nhất và làm sáng ô đầu tiên
                partyScreen.SetPartyData(playerParty.Pokemons);
                partyScreen.UpdateMemberSelection(0);

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
            // 4. THÊM MỚI: Xử lý hành động khi túi Pokemon đang mở ngoài Map
            HandlePartyScreen();
        }
    }

    // 5. THÊM MỚI: Hàm xử lý đóng màn hình Party
    void HandlePartyScreen()
    {
        // Bấm phím X để đóng túi và quay lại đi dạo
        if (Input.GetKeyDown(KeyCode.X))
        {
            partyScreen.gameObject.SetActive(false);
            state = GameState.FreeRoam;
        }
    }
}