using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

// Đã tích hợp GourdBagScreen vào vòng tuần tự trạng thái trận đấu
public enum BattleState { Start, ActionSelection, MoveSelection, PerformMove, Busy, PartyScreen, BagScreen, GourdBagScreen, BattleOver, SwitchPokemon, WildPokemonAppear }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud playerHud;

    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] List<ItemSlot> testInventory;

    // --- ĐÃ THÊM: Biến chứa nhạc nền trận đấu ---
    [Header("--- CÀI ĐẶT ÂM THANH BATTLE ---")]
    public AudioClip battleBGM;
    public AudioClip catchSound; // ĐÃ THÊM: Biến chứa âm thanh bắt thành công
    // --------------------------------------------

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;
    int currentMember;
    bool battleIsOver = false;

    PokemonParrty playerParty;
    Pokemon wildPokemon;

    public void BattleStart(PokemonParrty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;

        // --- ĐÃ THÊM: Bật nhạc khi bắt đầu trận ---
        if (SoundManager.Instance != null && battleBGM != null)
        {
            SoundManager.Instance.StartBattleMusic(battleBGM);
        }
        // ------------------------------------------

        StartCoroutine(SetupBattle());
    }

    public void StartBattle(PokemonParrty playerParty, Pokemon wildPokemon)
    {
        BattleStart(playerParty, wildPokemon);
    }

    public IEnumerator SetupBattle()
    {
        var firstPokemon = playerParty.GetHealthyPokemon();
        if (firstPokemon == null) yield break;

        firstPokemon.Init();

        playerUnit.Setup(firstPokemon);
        playerHud.SetData(playerUnit.Pokemon);

        if (wildPokemon != null && enemyUnit != null)
        {
            enemyUnit.Setup(wildPokemon);
        }

        partyScreen.Init();
        inventoryUI.SetData(testInventory, false); // Khởi tạo ban đầu ở Menu 1: chỉ hiện Tên

        var appearingName = enemyUnit != null && enemyUnit.Pokemon != null ? enemyUnit.Pokemon.Base.Name : (wildPokemon != null ? wildPokemon.Base.Name : "");
        yield return StartCoroutine(dialogBox.TypeDialog($"{appearingName} hoang dã xuất hiện!"));
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} ra trận!"));
        yield return new WaitForSeconds(1.0f);

        ActionSelection(); // Đưa về màn hình chọn hành động chính ngay khi vào trận
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        battleIsOver = true;

        // --- ĐÃ THÊM: Tắt nhạc khi kết thúc trận ---
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopBattleMusic();
        }
        // -------------------------------------------

        StopAllCoroutines();
        OnBattleOver?.Invoke(won);
    }

    // ĐÃ NÂNG CẤP ĐỒNG BỘ: Reset sạch sẽ giao diện cũ để chuột không bao giờ bị nghẽn
    public void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Bạn muốn làm gì tiếp theo?");
        dialogBox.EnableActionSelector(true); // Bật menu chính (FIGHT, POKEMON, BAG, RUN)
        dialogBox.EnableMoveSelector(false); // Đảm bảo tắt bảng chọn chiêu thức cũ
        dialogBox.EnableDialogText(true);    // Đảm bảo hiện lại khung thoại chữ
    }

    public void OnFightButtonClicked()
    {
        if (state != BattleState.ActionSelection) return;
        MoveSelection(); // Bấm FIGHT mới chuyển sang bảng chọn kỹ năng đòn đánh
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(true);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
        dialogBox.SetDialog("Chọn chiêu thức...");

        dialogBox.SetMoveSlotsData(
            playerUnit.Pokemon.Moves,
            OnMoveHoveredByMouse,
            OnMoveClickedByMouse
        );
    }

    void OnMoveHoveredByMouse(Move move)
    {
        if (state != BattleState.MoveSelection) return;
        dialogBox.UpdateMoveDetailsPanel(move);
    }

    void OnMoveClickedByMouse(int moveIndex)
    {
        if (state != BattleState.MoveSelection) return;

        currentMove = moveIndex;
        dialogBox.EnableMoveSelector(false);
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(true);
        StartCoroutine(PlayerMove());
    }

    // --- CÁC HÀM XỬ LÝ CLICK CHUỘT CHO HÌNH ẢNH MENU ---

    // Click hình BAG ngoài menu chính -> Mở MENU 1 (Túi đồ tổng, CHỈ HIỆN TÊN)
    public void OnBagButtonClicked()
    {
        if (state != BattleState.MoveSelection && state != BattleState.ActionSelection && state != BattleState.GourdBagScreen) return;

        state = BattleState.BagScreen;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableMoveSelector(false);

        inventoryUI.SetData(testInventory, false); // false = Chỉ hiện Tên vật phẩm đa diện, giấu x...
        inventoryUI.gameObject.SetActive(true);
        Debug.Log("Chuột click: Mở MENU 1 (Túi đồ chung)");
        HandleBagSelection();
    }

    // Click chọn lồng sâu vào MENU 2 (Danh sách Hồ Lô con, HIỆN TÊN + SỐ LƯỢNG)
    public void OnGourdBagButtonClicked()
    {
        if (state != BattleState.BagScreen) return;

        state = BattleState.GourdBagScreen; // Chuyển sang chế độ menu con Hồ Lô

        // Lọc LINQ để bóc tách riêng danh sách Hồ lô đang sở hữu
        List<ItemSlot> gourdOnlyList = testInventory.FindAll(slot => slot.item is GourdballItem);

        inventoryUI.SetData(gourdOnlyList, true); // true = Hiển thị ĐẦY ĐỦ TÊN + SỐ LƯỢNG x...
        inventoryUI.gameObject.SetActive(true);
        Debug.Log("Chuột click: Mở MENU 2 (Danh sách Dưỡng Thú Hồ chi tiết)");
        HandleGourdBagSelection();
    }

    // 🔥 ĐÃ SỬA CHÍNH XÁC LUỒNG BACK: Rẽ nhánh thông minh giải phóng click chuột hoàn toàn!
    public void OnInventoryBackButtonClicked()
    {
        if (state == BattleState.BagScreen)
        {
            // Bấm Back từ MENU 1 -> Ẩn túi đồ, quay hẳn về ActionSelection giúp chọn được Pokémon hoặc Run tự do!
            inventoryUI.gameObject.SetActive(false);
            ActionSelection();
            Debug.Log("[BACK THOÁT] Đã đóng túi đồ Menu 1, giải phóng pháo đài chuột quay về Menu chính.");
        }
        else if (state == BattleState.GourdBagScreen)
        {
            // Bấm Back từ MENU 2 -> Trả người chơi quay ngược lại Menu 1 túi đồ tổng chứ không đóng sập giao diện!
            OnBagButtonClicked();
            Debug.Log("[BACK QUAY LẠI] Đã lùi từ Menu 2 chi tiết quay ngược lại Menu 1.");
        }
    }

    public void OnPokemonButtonClicked()
    {
        if (state != BattleState.MoveSelection && state != BattleState.ActionSelection) return;

        state = BattleState.PartyScreen;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableMoveSelector(false);
        partyScreen.gameObject.SetActive(true);
        partyScreen.SetPartyData(playerParty.Pokemons, false, OnPartyMemberClicked, OnPartyBackButtonClicked, OnPartyOutButtonClicked);
    }

    public void OnRunButtonClicked()
    {
        if (state != BattleState.MoveSelection && state != BattleState.ActionSelection) return;

        dialogBox.EnableActionSelector(false);
        dialogBox.EnableMoveSelector(false);
        dialogBox.EnableDialogText(true);
        StartCoroutine(RunAwayRoutine());
    }

    IEnumerator RunAwayRoutine()
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog("Bạn đang tìm đường bỏ chạy...");
        yield return new WaitForSeconds(0.5f);

        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            yield return dialogBox.TypeDialog("Bỏ chạy thành công!");
            yield return new WaitForSeconds(0.5f);
            BattleOver(false);
        }
        else
        {
            yield return dialogBox.TypeDialog("Không thể chạy thoát lúc này!");
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(EnemyMove());
        }
    }

    public void OnPartyBackButtonClicked()
    {
        if (state != BattleState.PartyScreen) return;
        partyScreen.gameObject.SetActive(false);
        ActionSelection(); // Đưa về ActionSelection khi đóng bảng Party cho mượt chuột
    }

    public void OnPartyOutButtonClicked()
    {
        if (state != BattleState.PartyScreen) return;
        StartCoroutine(PartyOutRoutine());
    }

    IEnumerator PartyOutRoutine()
    {
        state = BattleState.Busy;
        partyScreen.gameObject.SetActive(false);
        dialogBox.EnableDialogText(true);
        yield return dialogBox.TypeDialog("Bạn chấp nhận thất bại và rút lui khỏi trận đấu...");
        yield return new WaitForSeconds(0.5f);
        BattleOver(false);
    }

    public void OnPartyMemberClicked(int index)
    {
        if (state != BattleState.PartyScreen) return;

        var selectedPokemon = playerParty.Pokemons[index];
        if (selectedPokemon.HP <= 0)
        {
            dialogBox.EnableDialogText(true);
            dialogBox.SetDialog($"{selectedPokemon.Base.Name} đã kiệt sức, không thể chiến đấu!");
            return;
        }
        if (selectedPokemon == playerUnit.Pokemon)
        {
            dialogBox.EnableDialogText(true);
            dialogBox.SetDialog($"{selectedPokemon.Base.Name} đang ở trên sân đấu rồi!");
            return;
        }

        partyScreen.gameObject.SetActive(false);
        StartCoroutine(SwitchPokemonRoutine(selectedPokemon));
    }

    IEnumerator SwitchPokemonRoutine(Pokemon newPokemon)
    {
        state = BattleState.Busy;
        dialogBox.EnableDialogText(true);

        if (playerUnit.Pokemon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Về nghỉ ngơi thôi {playerUnit.Pokemon.Base.Name}!");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(1.0f);
        }

        playerUnit.Setup(newPokemon);
        playerHud.SetData(playerUnit.Pokemon);

        yield return dialogBox.TypeDialog($"Tiến lên, {playerUnit.Pokemon.Base.Name}!");
        yield return new WaitForSeconds(1.0f);

        StartCoroutine(EnemyMove());
    }

    // Bộ lắng nghe phím/chuột cho MENU 1 (Túi đồ tổng)
    void HandleBagSelection()
    {
        Action onBack = () => { OnInventoryBackButtonClicked(); };

        Action<ItemBase> onUsed = (ItemBase item) =>
        {
            if (item is GourdballItem)
            {
                // Nếu click đúp chọn vật phẩm Hồ lô ở Menu 1 -> Kích hoạt mở lồng sang Menu 2!
                OnGourdBagButtonClicked();
            }
            else
            {
                // Nếu bấm vật phẩm thường (Thuốc men...) -> Dùng luôn và mất lượt
                StartCoroutine(UseItem(item));
            }
        };

        inventoryUI.HandleUpdate(onBack, onUsed);
    }

    // Bộ lắng nghe phím/chuột cho MENU 2 (Danh sách Hồ Lô con)
    void HandleGourdBagSelection()
    {
        Action onBack = () => { OnInventoryBackButtonClicked(); };

        Action<ItemBase> onUsed = (ItemBase item) =>
        {
            // Ở Menu 2 chi tiết, click đúp chọn phát nữa mới thực sự ném bóng bắt quái mất lượt!
            StartCoroutine(UseItem(item));
        };

        inventoryUI.HandleUpdate(onBack, onUsed);
    }

    IEnumerator UseItem(ItemBase item)
    {
        state = BattleState.Busy;
        inventoryUI.gameObject.SetActive(false);

        if (item is GourdballItem gourdball)
        {
            yield return dialogBox.TypeDialog($"Bạn đã ném {gourdball.itemName}!");
            yield return new WaitForSeconds(1.0f);

            float maxHP = enemyUnit.Pokemon.MaxHp;
            float currentHP = enemyUnit.Pokemon.HP;

            float catchChance = ((3 * maxHP - 2 * currentHP) / (3 * maxHP)) * gourdball.catchRateModifier;
            bool isCaught = UnityEngine.Random.value <= catchChance;

            if (isCaught)
            {
                // --- ĐÃ THAY ĐỔI: Tắt hẳn nhạc nền BGM để tạo điểm nhấn ---
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.StopBattleMusic(); // Tắt nhạc nền ngay lập tức

                    if (catchSound != null)
                    {
                        SoundManager.Instance.PlaySFX(catchSound); // Phát âm thanh chiến thắng
                    }
                }
                // ------------------------------------------------
                yield return dialogBox.TypeDialog($"Tuyệt vời! Bạn đã bắt được {enemyUnit.Pokemon.Base.Name}!");
                // --- ĐÃ THÊM: Đợi âm thanh bắt phát hết hoàn toàn rồi mới thoát trận ---
                if (catchSound != null)
                {
                    yield return new WaitForSeconds(catchSound.length);
                }
                // -----------------------------------------------------------------------
                playerParty.AddPokemon(enemyUnit.Pokemon);
                BattleOver(true);
                yield break;
            }
            else
            {
                yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} đã thoát ra được!");
            }
        }
        else
        {
            bool isItemUsed = item.Use(playerUnit.Pokemon);

            if (isItemUsed)
            {
                yield return dialogBox.TypeDialog($"Đã dùng {item.itemName} lên {playerUnit.Pokemon.Base.Name}!");
                yield return playerHud.UpdateHP();
            }
            else
            {
                yield return dialogBox.TypeDialog($"Vật phẩm không có tác dụng lúc này!");
                inventoryUI.gameObject.SetActive(true);
                state = BattleState.BagScreen;
                yield break;
            }
        }

        StartCoroutine(EnemyMove());
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection(); // Nhấn hủy chiêu thức sẽ trả lại Menu chính hành động
        }
    }

    IEnumerator PlayerMove()
    {
        state = BattleState.PerformMove;

        var playerMove = playerUnit.Pokemon.Moves[currentMove];
        var enemyMove = enemyUnit.Pokemon.GetRanDomMove();

        bool playerFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;

        if (playerFirst)
        {
            yield return RunMove(playerUnit, enemyUnit, playerMove);
            if (state == BattleState.BattleOver) yield break;
            if (enemyUnit.Pokemon.HP > 0) yield return RunMove(enemyUnit, playerUnit, enemyMove);
        }
        else
        {
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            if (state == BattleState.BattleOver) yield break;
            if (playerUnit.Pokemon.HP > 0) yield return RunMove(playerUnit, enemyUnit, playerMove);
        }

        if (state != BattleState.BattleOver) ActionSelection();
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.PerformMove;
        var move = enemyUnit.Pokemon.GetRanDomMove();
        yield return RunMove(enemyUnit, playerUnit, move);

        if (state != BattleState.BattleOver) ActionSelection();
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} đã sử dụng {move.Base.Name} và gây ra sát thương!");
        yield return new WaitForSeconds(0.5f);

        sourceUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(0.5f);

        targetUnit.PlayHitAnimation();

        var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
        yield return targetUnit.Hud.UpdateHP();

        if (damageDetails.Critical > 1f) yield return dialogBox.TypeDialog("Đòn chí mạng!");
        if (damageDetails.TypeEffectiveness == 0f) yield return dialogBox.TypeDialog("Không có tác dụng");
        else if (damageDetails.TypeEffectiveness > 1f) yield return dialogBox.TypeDialog("Rất hiệu quả!");
        else if (damageDetails.TypeEffectiveness < 1f) yield return dialogBox.TypeDialog("Không hiệu quả lắm...");

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name} đã bị hạ gục");
            targetUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(1.5f);
            CheckForBattleOver(targetUnit);
            if (battleIsOver) yield break;
        }
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                state = BattleState.PartyScreen;
                dialogBox.EnableActionSelector(false);
                dialogBox.EnableMoveSelector(false);
                partyScreen.gameObject.SetActive(true);
                partyScreen.SetPartyData(playerParty.Pokemons, true, OnPartyMemberClicked, OnPartyBackButtonClicked, OnPartyOutButtonClicked);
            }
            else
            {
                BattleOver(false);
            }
        }
        else
        {
            BattleOver(true);
        }
    }

    public void HandleUpdate()
    {
        if (state == BattleState.BagScreen) HandleBagSelection();
        else if (state == BattleState.GourdBagScreen) HandleGourdBagSelection(); // ĐỒNG BỘ: Chạy frame vòng lặp cho menu 2
    }
}