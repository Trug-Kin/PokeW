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

    [Header("Kho Pokemon Hoang Dã")]
    public List<PokemonBase> wildPokemonDatabase;

    GameState state;

    public void Start()
    {
        playerControl.OnEncountered += StartBattle;
        battleSystem.OnBattleOver += EndBattle;
        partyScreen.Init();
    }

    void StartBattle()
    {
        var playerParty = playerControl.GetComponent<PokemonParrty>();
        var activePokemon = playerParty.GetHealthyPokemon();

        // Kiểm tra xem đội hình có Pokemon nào còn sống không
        if (activePokemon == null)
        {
            Debug.LogWarning("Không có Pokemon khỏe mạnh để ra trận!");
            return;
        }

        // Đóng băng map, mở màn hình Battle
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        // --- BẮT ĐẦU LOGIC SINH QUÁI HOANG DÃ ---

        // 1. Tìm Pokemon có CẤP ĐỘ CAO NHẤT trong túi đồ của người chơi
        int highestLevel = 1;
        foreach (var p in playerParty.Pokemons)
        {
            if (p.Level > highestLevel)
            {
                highestLevel = p.Level;
            }
        }

        // 2. Bốc ngẫu nhiên 1 Pokemon từ Kho dữ liệu
        if (wildPokemonDatabase == null || wildPokemonDatabase.Count == 0)
        {
            Debug.LogError("Kho Pokemon hoang dã trống! Vui lòng kéo thả file PokemonBase vào list Wild Pokemon Database ở GameControl.");
            
            // Xử lý thoát game/trận nếu lỗi để không kẹt màn hình
            state = GameState.FreeRoam;
            battleSystem.gameObject.SetActive(false);
            worldCamera.gameObject.SetActive(true);
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, wildPokemonDatabase.Count);
        PokemonBase randomBase = wildPokemonDatabase[randomIndex];

        // 3. Tính toán Cấp độ ngẫu nhiên dựa trên con MẠNH NHẤT
        // Min level: Cấp cao nhất - 2 (Nhưng thấp nhất phải là 1)
        // Max level: Cấp cao nhất + 5
        int minLevel = Mathf.Max(1, highestLevel - 2); 
        int maxLevel = highestLevel + 5;
        int randomLevel = UnityEngine.Random.Range(minLevel, maxLevel + 1); 

        // 4. Khởi tạo con Pokemon hoang dã
        // 4. Khởi tạo con Pokemon hoang dã (Truyền thẳng dữ liệu vào Constructor)
        Pokemon wildPokemon = new Pokemon(randomBase, randomLevel);
        wildPokemon.Init();

        // 5. Mở trận đánh và ném nó vào
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