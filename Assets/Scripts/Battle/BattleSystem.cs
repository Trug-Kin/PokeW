using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public enum BattleState { Start, ActionSelection, MoveSelection, PerformMove, Busy, PartyScreen, BagScreen, BattleOver }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud playerHud;

    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    // --- ĐÃ SỬA: Dùng InventoryUI thay vì GameObject ---
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] List<ItemSlot> testInventory; // Danh sách túi đồ dùng để test

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
        StartCoroutine(SetupBattle());
    }

    public void StartBattle(PokemonParrty playerParty, Pokemon wildPokemon)
    {
        BattleStart(playerParty, wildPokemon);
    }

    public IEnumerator SetupBattle()
    {
        var firstPokemon = playerParty.GetHealthyPokemon();
        if (firstPokemon == null)
        {
            Debug.LogError("BattleSystem: Player has no healthy Pokemon to start the battle.");
            yield break;
        }

        playerUnit.Setup(firstPokemon);
        playerHud.SetData(playerUnit.Pokemon);

        if (wildPokemon != null && enemyUnit != null)
        {
            enemyUnit.Setup(wildPokemon);
        }

        partyScreen.Init();

        // Nạp dữ liệu vào túi đồ để test
        inventoryUI.SetData(testInventory);

        dialogBox.SetMoveNames(playerUnit.Pokemon?.Moves);
        var appearingName = enemyUnit != null && enemyUnit.Pokemon != null ? enemyUnit.Pokemon.Base.Name : (wildPokemon != null ? wildPokemon.Base.Name : "");
        yield return StartCoroutine(dialogBox.TypeDialog($"{appearingName} xuất hiện"));
        yield return new WaitForSeconds(1f);

        ActionSelection();
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        battleIsOver = true;
        StopAllCoroutines();
        OnBattleOver?.Invoke(won);
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Chọn hành động");
        dialogBox.EnableActionSelector(true);
    }
    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
        dialogBox.SetDialog("Chọn chiêu");
    }
    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    // --- ĐÃ SỬA: Mở túi đồ bằng InventoryUI ---
    void OpenBag()
    {
        state = BattleState.BagScreen;
        inventoryUI.gameObject.SetActive(true);
    }

    // --- MỚI: Xử lý logic chọn vật phẩm ---
    void HandleBagSelection()
    {
        Action onBack = () =>
        {
            inventoryUI.gameObject.SetActive(false);
            ActionSelection();
        };

        Action<ItemBase> onUsed = (ItemBase item) =>
        {
            StartCoroutine(UseItem(item));
        };

        inventoryUI.HandleUpdate(onBack, onUsed);
    }

    // --- MỚI: Coroutine thực thi hiệu ứng của vật phẩm ---
    IEnumerator UseItem(ItemBase item)
    {
        state = BattleState.Busy;
        inventoryUI.gameObject.SetActive(false);

        // Nếu là Gourdball (Bắt Pokemon)
        if (item is GourdballItem)
        {
            yield return dialogBox.TypeDialog($"Bạn đã ném {item.itemName}!");

            // Logic bắt tạm thời (Bạn có thể bổ sung rung rắc, tính toán % bắt sau)
            bool isCaught = true;

            if (isCaught)
            {
                yield return dialogBox.TypeDialog($"Tuyệt vời! Bạn đã bắt được {enemyUnit.Pokemon.Base.Name}!");
                playerParty.AddPokemon(enemyUnit.Pokemon);
                BattleOver(true); // Kết thúc trận chiến chiến thắng
                yield break;
            }
            else
            {
                yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} đã thoát ra được!");
            }
        }
        else // Nếu là Vật phẩm hồi phục/thanh tẩy
        {
            bool isItemUsed = item.Use(playerUnit.Pokemon);

            if (isItemUsed)
            {
                yield return dialogBox.TypeDialog($"Đã dùng {item.itemName} lên {playerUnit.Pokemon.Base.Name}!");
                yield return playerHud.UpdateHP(); // Cập nhật lại thanh máu
            }
            else
            {
                yield return dialogBox.TypeDialog($"Vật phẩm không có tác dụng lúc này!");

                // Trả lại lượt vì dùng hụt
                inventoryUI.gameObject.SetActive(true);
                state = BattleState.BagScreen;
                yield break;
            }
        }

        // Dùng xong vật phẩm thành công thì chuyển lượt cho địch
        StartCoroutine(EnemyMove());
    }

    // --- ĐÃ SỬA LỖI DOUBLE-X ---
    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentAction % 2 == 0 && currentAction < 3) ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentAction % 2 == 1) --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 2) currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMove > 1) currentMove -= 2;
        }

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);
        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PlayerMove());
        }
        else if (Input.GetKeyDown(KeyCode.X)) // Đã gộp và xóa phần X lặp lại
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
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
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} đã sử dụng {move.Base.Name}");
        yield return new WaitForSeconds(1f);

        sourceUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

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
            yield return new WaitForSeconds(2f);
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
                state = BattleState.Busy;
                StartCoroutine(SwitchPokemon(nextPokemon));
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
        if (state == BattleState.ActionSelection) HandleActionSelection();
        else if (state == BattleState.MoveSelection) HandleMoveSelection();
        else if (state == BattleState.PartyScreen) HandlePartySelection();
        else if (state == BattleState.BagScreen) HandleBagSelection();
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentAction % 2 == 0 && currentAction < 3) ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentAction % 2 == 1) --currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 2) currentAction += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAction > 1) currentAction -= 2;
        }

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0) MoveSelection();
            else if (currentAction == 1) OpenBag();
            else if (currentAction == 2) OpenPartyScreen();
            else if (currentAction == 3) { /* TODO: Run */ }
        }
    }

    void HandlePartySelection()
    {
        if (playerParty == null || playerParty.Pokemons == null || playerParty.Pokemons.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMember % 2 == 0 && currentMember +1 < playerParty.Pokemons.Count) ++currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentMember % 2 == 1) --currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMember + 2 < playerParty.Pokemons.Count)
                currentMember += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMember > 1) currentMember -= 2;
        }

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count - 1);
        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var selectedMember = playerParty.Pokemons[currentMember];
            if (selectedMember.HP <= 0)
            {
                partyScreen.MessageText("Không thể chọn Pokemon đã bị hạ gục");
                return;
            }
            if (selectedMember == playerUnit.Pokemon)
            {
                partyScreen.MessageText("Pokemon này đang ra trận");
                return;
            }
            partyScreen.gameObject.SetActive(false);
            state = BattleState.Busy;
            StartCoroutine(SwitchPokemon(selectedMember));
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        if (playerUnit.Pokemon.HP > 0)
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} rút lui");

        playerUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        playerUnit.Setup(newPokemon);
        dialogBox.SetMoveNames(newPokemon.Moves);
        yield return new WaitForSeconds(1f);
        yield return dialogBox.TypeDialog($"Tiếp theo là {playerUnit.Pokemon.Base.Name}");

        if (!battleIsOver) StartCoroutine(EnemyMove());
        StartCoroutine(EnemyMove());
    }
}