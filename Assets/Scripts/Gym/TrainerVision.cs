using System.Collections.Generic;
using UnityEngine;

public class TrainerVision : MonoBehaviour
{
    [Header("Kho chứa Pokemon có sẵn để bốc ngẫu nhiên")]
    public List<PokemonBase> pokemonPool;

    [Header("Cấp độ của Pokemon NPC")]
    public int pokemonLevel = 10;

    private bool battleTriggered = false;
    private bool isPlayerInRange = false;
    private PokemonParrty playerParty;

    // 1. Hàm Update liên tục kiểm tra phím bấm
    private void Update()
    {
        if (isPlayerInRange && !battleTriggered)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("💥 ĐÃ BẤM E! Bắt đầu gọi trận đấu...");
                StartBattle(); // Chỉ gọi hàm StartBattle ở đây
            }
        }
    }

    // 2. Hàm kích hoạt khi có vật thể chạm vào vùng nhìn
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("⚠️ Vừa có chạm vật lý! Tên vật chạm: " + collision.gameObject.name + " | Tag: " + collision.tag);

        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerParty = collision.GetComponent<PokemonParrty>();

            // KIỂM TRA NGAY LÚC VA CHẠM
            if (playerParty == null)
            {
                Debug.LogError("❌ CẢNH BÁO: NPC đã chạm trúng Player, nhưng Player này KHÔNG CÓ script PokemonParrty! (Khả năng cao đây là Player ảo/clone bị dư trong map Gym)");
            }
            else
            {
                Debug.Log("✅ Đúng là Player và ĐÃ TÌM THẤY PokemonParrty! Đang chờ bạn bấm phím E...");
            }
        }
    }

    // 3. Hàm kích hoạt khi vật thể đi ra khỏi vùng
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            playerParty = null;
            Debug.Log("Player đã rời khỏi tầm nhìn NPC.");
        }
    }

    // 4. Hàm xử lý logic bắt đầu trận đấu
    private void StartBattle()
    {
        if (pokemonPool.Count < 2)
        {
            Debug.LogError("Vui lòng thêm ít nhất 2 Pokemon vào danh sách Pokemon Pool!");
            return;
        }

        battleTriggered = true;
        List<Pokemon> selectedEnemyTeam = new List<Pokemon>();
        List<PokemonBase> temporaryPool = new List<PokemonBase>(pokemonPool);

        for (int i = 0; i < 2; i++)
        {
            int randomIndex = Random.Range(0, temporaryPool.Count);
            Pokemon newEnemyPokemon = new Pokemon(temporaryPool[randomIndex], pokemonLevel);
            selectedEnemyTeam.Add(newEnemyPokemon);
            temporaryPool.RemoveAt(randomIndex);
        }

        BattleSystem battleSystem = FindFirstObjectByType<BattleSystem>(FindObjectsInactive.Include);

        // --- TÁCH LỖI RÕ RÀNG Ở ĐÂY ---
        if (battleSystem == null)
        {
            Debug.LogError("🚨 LỖI SỐ 1: Hoàn toàn không tìm thấy BattleSystem trong Scene này!");
            battleTriggered = false;
            return;
        }

        if (playerParty == null)
        {
            Debug.LogError("🚨 LỖI SỐ 2: Có BattleSystem, nhưng PlayerParty bị NULL (Nhân vật đang chạm vào NPC không có Pokemon).");
            battleTriggered = false;
            return;
        }

        // ==========================================
        // 🔥 ĐÂY CHÍNH LÀ NƠI CHÚNG TA THÊM LỆNH FIX LỖI 🔥
        // Đánh thức BattleSystem lên trước!
        battleSystem.gameObject.SetActive(true);

        // Sau đó mới gọi Coroutine bắt đầu trận đấu!
        battleSystem.StartTrainerBattle(playerParty, selectedEnemyTeam);
        // ==========================================
    }
}