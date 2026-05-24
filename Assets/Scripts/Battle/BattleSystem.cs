using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

// Trạng thái trận đấu đồng bộ của PokeW
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
    [SerializeField] Animator catchEffectAnimator; // Kéo Object CatchEffect vào đây
    [Header("--- CÀI ĐẶT ÂM THANH BATTLE ---")]
    public AudioClip battleBGM;
    public AudioClip catchSound;
    [SerializeField] GameObject dthObject;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;
    int currentMember;
    bool battleIsOver = false;

    PokemonParrty playerParty; // Bảo lưu đúng chính tả Class gốc có 2 chữ r của bạn
    Pokemon wildPokemon;

    public void BattleStart(PokemonParrty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        this.isTrainerBattle = false; // Mặc định là trận hoang dã
        this.trainerTeam = null;
        battleIsOver = false;

        // 🔥 TÍCH HỢP POKEMON TỪ START SCENE VÀO PARTY 🔥
        if (StarterSelection.chosenStarter != null && !isStarterInitialized)
        {
            if (this.playerParty.Pokemons.Count > 0)
            {
                // Thay thế Pokemon slot 1 bằng Starter đã chọn
                this.playerParty.Pokemons[0].Base = StarterSelection.chosenStarter;

                // Khởi tạo lại HP, chỉ số và chiêu thức cho đúng với con mới này
                this.playerParty.Pokemons[0].Init();
            }
            else
            {
                Debug.LogWarning("Party đang trống! Vui lòng nhét sẵn 1 Pokemon (VD: level 5) vào danh sách Party của Player ở thanh Inspector để hệ thống đè Starter lên slot đó.");
            }

            isStarterInitialized = true; // Đánh dấu là đã thêm để không thay đổi nữa ở các trận đấu sau
        }

        StartCoroutine(SetupBattle());
    }

    // 🌟 HÀM MỚI: DÙNG ĐỂ KÍCH HOẠT TRẬN ĐẤU VỚI NPC TRAINER 🌟
    public void StartTrainerBattle(PokemonParrty playerParty, List<Pokemon> trainerTeam)
    {
        this.playerParty = playerParty;
        this.trainerTeam = trainerTeam;
        this.isTrainerBattle = true;
        this.wildPokemon = null;
        battleIsOver = false;
        StartCoroutine(SetupBattle());
    }

    public void StartBattle(PokemonParrty playerParty, Pokemon wildPokemon)
    {
        BattleStart(playerParty, wildPokemon);
    }

    public IEnumerator SetupBattle()
    {
        if (SoundManager.Instance != null && battleBGM != null)
        {
            SoundManager.Instance.StartBattleMusic(battleBGM);
        }

        var firstPokemon = playerParty.GetHealthyPokemon();
        if (firstPokemon == null)
        {
            Debug.LogError("BattleSystem: Người chơi không có Pokemon khỏe mạnh để ra trận.");
            yield break;
        }

        firstPokemon.Init();
        playerUnit.Setup(firstPokemon);
        playerHud.SetData(playerUnit.Pokemon);

        // 🌟 LẤY POKEMON ĐỊCH PHÙ HỢP THEO LOẠI TRẬN ĐẤU 🌟
        Pokemon activeEnemy = isTrainerBattle ? GetHealthyTrainerPokemon() : wildPokemon;

        if (activeEnemy != null && enemyUnit != null)
        {
            activeEnemy.Init();
            enemyUnit.Setup(activeEnemy);
        }

        partyScreen.Init();
        inventoryUI.SetData(testInventory, true);

        if (dialogBox != null && playerUnit.Pokemon != null)
        {
            dialogBox.SetMoveSlotsData(playerUnit.Pokemon.Moves, 
                (move) => dialogBox.UpdateMoveDetailsPanel(move),   
                (moveIndex) => MouseSelectMove(moveIndex)           
            );
        }

        dialogBox.EnableDialogText(true);

        var appearingName = enemyUnit != null && enemyUnit.Pokemon != null ? enemyUnit.Pokemon.Base.Name : "";

        // 🌟 THAY ĐỔI CÂU THOẠI CHÀO SÂN NẾU ĐỐI ĐẦU TRAINER 🌟
        if (isTrainerBattle)
        {
            yield return StartCoroutine(dialogBox.TypeDialog($"Trainer thách đấu! Họ cử {appearingName} ra trận!"));
        }
        else
        {
            yield return StartCoroutine(dialogBox.TypeDialog($"Pokemon hoang dã {appearingName} xuất hiện!"));
        }

        yield return new WaitForSeconds(1f);

        OpenParallelTurnMenu();
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        battleIsOver = true;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopBattleMusic();
        }

        StopAllCoroutines();
        OnBattleOver?.Invoke(won);
    }

    public void OpenParallelTurnMenu()
    {
        state = BattleState.ActionSelection;

        if (dialogBox != null && playerUnit.Pokemon != null)
        {
            dialogBox.SetMoveSlotsData(playerUnit.Pokemon.Moves, 
                (move) => dialogBox.UpdateMoveDetailsPanel(move), 
                (moveIndex) => MouseSelectMove(moveIndex)           
            );
        }

        dialogBox.EnableActionSelector(true);
        dialogBox.EnableMoveSelector(true);
        dialogBox.EnableDialogText(false);
        dialogBox.SetDialog("");

        dialogBox.UpdateActionSelection(-1);
        dialogBox.UpdateMoveDetailsPanel(null);
    }

    void ActionSelection() => OpenParallelTurnMenu();
    void MoveSelection() => OpenParallelTurnMenu();

    public void OpenBag()
    {
        state = BattleState.BagScreen;

        dialogBox.EnableActionSelector(false);
        dialogBox.EnableMoveSelector(false);

        Action onBack = () =>
        {
            inventoryUI.gameObject.SetActive(false);
            OpenParallelTurnMenu();
        };

        Action<ItemBase> onUsed = (ItemBase item) =>
        {
            StartCoroutine(UseItem(item));
        };

        inventoryUI.HandleUpdate(onBack, onUsed);

        inventoryUI.SetData(testInventory, true);
        inventoryUI.gameObject.SetActive(true);
    }

    public void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;

        dialogBox.EnableActionSelector(false);
        dialogBox.EnableMoveSelector(false);

        partyScreen.SetPartyData(
            playerParty.Pokemons,
            playerUnit.Pokemon.HP <= 0,
            OnPartyMemberClicked,
            OnPartyBackButtonClicked,
            OnPartyOutButtonClicked
        );
        partyScreen.gameObject.SetActive(true);
    }

    void HandleBagSelection()
    {
    }

    IEnumerator UseItem(ItemBase item)
    {
        state = BattleState.Busy;
        inventoryUI.gameObject.SetActive(false);

        dialogBox.EnableDialogText(true);

        if (item is GourdballItem gourdball)
        {
            // 🌟 KHÓA TÍNH NĂNG BẮT POKEMON CỦA TRAINER KHÁC 🌟
            if (isTrainerBattle)
            {
                yield return dialogBox.TypeDialog($"Bạn không thể bắt trái phép Pokemon của Trainer khác!");
                yield return new WaitForSeconds(1.0f);
                inventoryUI.gameObject.SetActive(true);
                state = BattleState.BagScreen;
                yield break;
            }

            yield return dialogBox.TypeDialog($"Bạn đã ném {gourdball.itemName}!");
            yield return new WaitForSeconds(0.5f);

            catchEffectAnimator.gameObject.transform.position = enemyUnit.transform.position;
            catchEffectAnimator.gameObject.SetActive(true);

            enemyUnit.gameObject.SetActive(false);

            float maxHP = enemyUnit.Pokemon.MaxHp;
            float currentHP = enemyUnit.Pokemon.HP;
            float catchChance = ((3 * maxHP - 2 * currentHP) / (3 * maxHP)) * gourdball.catchRateModifier;
            bool isCaught = UnityEngine.Random.value <= catchChance;

            yield return new WaitForSeconds(1.0f);

            if (isCaught)
            {
                catchEffectAnimator.SetTrigger("success");
                yield return new WaitForSeconds(0.5f);

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.StopBattleMusic();
                    if (catchSound != null) SoundManager.Instance.PlaySFX(catchSound);
                }

                yield return dialogBox.TypeDialog($"Tuyệt vời! Bạn đã bắt được {enemyUnit.Pokemon.Base.Name}!");

                yield return new WaitForSeconds(1.5f);
                catchEffectAnimator.gameObject.SetActive(false);

                playerParty.AddPokemon(enemyUnit.Pokemon);
                BattleOver(true);
                yield break;
            }
            else
            {
                catchEffectAnimator.SetTrigger("fail");
                yield return new WaitForSeconds(1.5f);
                catchEffectAnimator.gameObject.SetActive(false);

                enemyUnit.gameObject.SetActive(true);
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

    public void MouseSelectMove(int moveIndex)
    {
        if (state != BattleState.ActionSelection) return;
        if (playerUnit == null || playerUnit.Pokemon == null || playerUnit.Pokemon.Moves == null) return;
        if (moveIndex < 0 || moveIndex >= playerUnit.Pokemon.Moves.Count) return;

        currentMove = moveIndex;

        dialogBox.EnableActionSelector(false);
        dialogBox.EnableMoveSelector(false);
        dialogBox.EnableDialogText(true);

        StartCoroutine(PlayerMove());
    }

    public void MouseHoverMove(int moveIndex)
    {
        if (state != BattleState.ActionSelection) return;
        if (playerUnit == null || playerUnit.Pokemon == null || playerUnit.Pokemon.Moves == null) return;

        if (moveIndex >= 0 && moveIndex < playerUnit.Pokemon.Moves.Count)
            dialogBox.UpdateMoveDetailsPanel(playerUnit.Pokemon.Moves[moveIndex]);
        else
            dialogBox.UpdateMoveDetailsPanel(null);
    }

    public void MouseSelectAction(int actionIndex)
    {
        if (state != BattleState.ActionSelection) return;

        if (actionIndex == 0) OpenBag();
        else if (actionIndex == 1) OpenPartyScreen();
        else if (actionIndex == 2) StartCoroutine(HandleRunAway());
    }

    public void MouseHoverAction(int actionIndex)
    {
        if (state != BattleState.ActionSelection) return;
        dialogBox.UpdateActionSelection(actionIndex);
    }

    public void MouseClick_Run()
    {
        if (state != BattleState.ActionSelection) return;
        StartCoroutine(HandleRunAway());
    }

    IEnumerator HandleRunAway()
    {
        state = BattleState.Busy;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableMoveSelector(false);
        dialogBox.EnableDialogText(true);

        // 🌟 KHÓA TÍNH NĂNG CHẠY TRỐN KHI ĐANG ĐẤU VỚI TRAINER 🌟
        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog("Bạn không thể bỏ chạy khỏi một trận đấu nghiêm túc với Trainer!");
            yield return new WaitForSeconds(1.0f);
            OpenParallelTurnMenu();
            yield break;
        }

        yield return dialogBox.TypeDialog("Bạn đang tìm sơ hở để bỏ chạy khỏi chiến trường...");
        yield return new WaitForSeconds(0.6f);

        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            yield return dialogBox.TypeDialog("Bỏ chạy thành công thoát khỏi trận chiến!");
            yield return new WaitForSeconds(0.8f);
            BattleOver(true);
        }
        else
        {
            yield return dialogBox.TypeDialog("Bỏ chạy thất bại! Kẻ địch đã chặn đường thoát!");
            yield return new WaitForSeconds(0.6f);
            
            // 🔥 ĐÃ SỬA LUỒNG
            yield return EnemyMove(); 
        }
    }

    void OnPartyMemberClicked(int selectedIndex)
    {
        var selectedMember = playerParty.Pokemons[selectedIndex];
        if (selectedMember.HP <= 0 || selectedMember == playerUnit.Pokemon) return;

        partyScreen.gameObject.SetActive(false);
        state = BattleState.Busy;
        dialogBox.EnableDialogText(true);

        StartCoroutine(SwitchPokemon(selectedMember));
    }

    void OnPartyBackButtonClicked()
    {
        partyScreen.gameObject.SetActive(false);
        OpenParallelTurnMenu();
    }

    void OnPartyOutButtonClicked() { }

    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        if (playerUnit.Pokemon.HP > 0)
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} rút lui an toàn!");

        playerUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(1.5f);

        playerUnit.Setup(newPokemon);

        if (dialogBox != null && newPokemon != null)
        {
            dialogBox.SetMoveSlotsData(newPokemon.Moves,
                (move) => dialogBox.UpdateMoveDetailsPanel(move),
                (moveIndex) => MouseSelectMove(moveIndex)
            );
        }

        yield return new WaitForSeconds(0.5f);
        yield return dialogBox.TypeDialog($"Xuất trận chiến đấu, {playerUnit.Pokemon.Base.Name}!");
        yield return new WaitForSeconds(1.0f);

        // 🔥 ĐÃ SỬA LUỒNG
        yield return EnemyMove(); 
    }

    IEnumerator PlayerMove()
    {
        state = BattleState.Busy;
        var playerMove = playerUnit.Pokemon.Moves[currentMove];

        // 🌟 LƯU LẠI THÔNG TIN POKEMON ĐỊCH BAN ĐẦU LƯỢT ĐỂ TRÁNH LỖI ĐỔI CON MỚI 🌟
        Pokemon originalEnemy = enemyUnit.Pokemon;
        var enemyMove = originalEnemy.GetRanDomMove();

        bool playerFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;

        if (playerFirst)
        {
            yield return RunMove(playerUnit, enemyUnit, playerMove);
            if (state == BattleState.BattleOver) yield break;

            // 🌟 CHỈ CHO PHÉP ENEMY ĐÁNH NẾU CON CŨ CHƯA BỊ ĐÁNH BẠI ĐỔI CON KHÁC 🌟
            if (enemyUnit.Pokemon == originalEnemy && enemyUnit.Pokemon.HP > 0)
                yield return RunMove(enemyUnit, playerUnit, enemyMove);
        }
        else
        {
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            if (state == BattleState.BattleOver) yield break;
            if (playerUnit.Pokemon.HP > 0) yield return RunMove(playerUnit, enemyUnit, playerMove);
        }

        if (state != BattleState.BattleOver) OpenParallelTurnMenu();
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.Busy;
        var move = enemyUnit.Pokemon.GetRanDomMove();
        yield return RunMove(enemyUnit, playerUnit, move);

        if (state != BattleState.BattleOver) OpenParallelTurnMenu();
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} đã dùng {move.Base.Name}!");
        yield return new WaitForSeconds(0.8f);

        sourceUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(0.8f);

        targetUnit.PlayHitAnimation();

        var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
        yield return targetUnit.Hud.UpdateHP();

        if (damageDetails.Critical > 1f) yield return dialogBox.TypeDialog("Đòn đánh hiểm hóc! Chí mạng!");
        if (damageDetails.TypeEffectiveness == 0f) yield return dialogBox.TypeDialog("Hoàn toàn không có tác dụng...");
        else if (damageDetails.TypeEffectiveness > 1f) yield return dialogBox.TypeDialog("Một đòn khắc hệ! Rất hiệu quả!");
        else if (damageDetails.TypeEffectiveness < 1f) yield return dialogBox.TypeDialog("Sát thương bị giảm! Không hiệu quả lắm...");

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name} đã bị đánh bại, kiệt sức gục ngã!");
            targetUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(1.5f);

            // 🌟 THAY ĐỔI: SỬ DỤNG YIELD RETURN ĐỂ ĐỢI CHUYỂN POKEMON 🌟
            yield return StartCoroutine(CheckForBattleOver(targetUnit));
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

                partyScreen.SetPartyData(playerParty.Pokemons, true, OnPartyMemberClicked, null, OnPartyOutButtonClicked);
            }
            else
            {
                BattleOver(false);
            }
        }
        else
        {
            // Nếu kẻ địch bị gục ngã
            if (isTrainerBattle)
            {
                var nextEnemyPokemon = GetHealthyTrainerPokemon();
                if (nextEnemyPokemon != null)
                {
                    // 🌟 NẾU TRAINER VẪN CÒN POKEMON KHỎE MẠNH -> TỰ ĐỘNG THAY THẾ 🌟
                    yield return StartCoroutine(HandleEnemySwitch(nextEnemyPokemon));
                }
                else
                {
                    // Hết sạch Pokemon thì người chơi thắng
                    BattleOver(true);
                }
            }
            else
            {
                // Nếu là pokemon hoang dã thì kết thúc trận đấu luôn
                BattleOver(true);
            }
        }
    }

    // 🌟 HÀM MỚI: XỬ LÝ ĐƯA POKEMON TIẾP THEO CỦA NPC RA SÂN 🌟
    IEnumerator HandleEnemySwitch(Pokemon nextPokemon)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"Đối thủ chuẩn bị tung {nextPokemon.Base.Name} tiếp theo ra trận!");
        yield return new WaitForSeconds(1.0f);

        nextPokemon.Init();
        enemyUnit.Setup(nextPokemon);

        yield return dialogBox.TypeDialog($"Xuất chiêu đi chiến đấu, {nextPokemon.Base.Name}!");
        yield return new WaitForSeconds(1.0f);
    }

    // 🌟 HÀM TRỢ GIÚP: QUÉT DANH SÁCH TÌM POKEMON CÒN MÁU CỦA TRAINER 🌟
    Pokemon GetHealthyTrainerPokemon()
    {
        if (trainerTeam == null) return null;
        foreach (var pokemon in trainerTeam)
        {
            if (pokemon.HP > 0) return pokemon;
        }
        return null;
    }

    public void HandleUpdate()
    {
        if (battleIsOver) return;

        if (state == BattleState.BagScreen)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                OpenParallelTurnMenu();
            };

            Action<ItemBase> onUsed = (ItemBase item) =>
            {
                StartCoroutine(UseItem(item));
            };

            inventoryUI.HandleUpdate(onBack, onUsed);
        }
        else if (state == BattleState.PartyScreen)
        {
        }
    }
}