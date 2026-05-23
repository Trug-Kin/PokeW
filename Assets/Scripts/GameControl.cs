using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public enum GameState { FreeRoam, Battle, PartyScreen }

public class GameControl : MonoBehaviour
{
    [SerializeField] PlayerControl playerControl;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] PartyScreen partyScreen;

    GameState state;

    public void Start()
    {
        playerControl.OnEncountered += StartBattle;
        battleSystem.OnBattleOver += EndBattle;
        partyScreen.Init();
    }

    void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerControl.GetComponent<PokemonParrty>();
        var wildPokemon = FindObjectOfType<MapArena>().GetRandomWildPokemon();

        battleSystem.StartBattle(playerParty, wildPokemon);
    }

    void EndBattle(bool won)
    {
        if (won)
        {
            // Thắng trận: Quay lại map đi dạo bình thường
            state = GameState.FreeRoam;
            battleSystem.gameObject.SetActive(false);
            worldCamera.gameObject.SetActive(true);
        }
        else
        {
            // Thua trận: Xử lý cơ chế Black Out
            Debug.Log("Toàn bộ Pokemon đã gục ngã! Bạn hốt hoảng bỏ chạy khỏi chiến trường...");

            // 1. Tự động hồi máu khẩn cấp (Tránh việc bạn bị kẹt cứng nếu lỡ dẫm vào cỏ lần nữa)
            var playerParty = playerControl.GetComponent<PokemonParrty>();
            if (playerParty != null)
            {
                playerParty.HealAllPokemon();
                Debug.Log("Hệ thống đã tự động hồi phục đội hình để chống lỗi kẹt game.");
            }

            // 2. Thoát màn hình chiến đấu
            state = GameState.FreeRoam;
            battleSystem.gameObject.SetActive(false);
            worldCamera.gameObject.SetActive(true);

            // MẸO: Sau này bạn có thể dùng lệnh SceneManager.LoadScene() ở đây 
            // để dịch chuyển nhân vật thẳng về Quầy Y Tá thay vì đứng yên tại chỗ!
        }
    }

    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            playerControl.HandleUpdate();

            if (Input.GetKeyDown(KeyCode.Return))
            {
                state = GameState.PartyScreen;
                var playerParty = playerControl.GetComponent<PokemonParrty>();

                partyScreen.SetPartyData(playerParty.Pokemons, false); 
                partyScreen.gameObject.SetActive(true);
            }
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if (state == GameState.PartyScreen)
        {
            HandlePartyScreen();
        }
    }

    void HandlePartyScreen()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            partyScreen.gameObject.SetActive(false);
            state = GameState.FreeRoam;
        }
    }
}