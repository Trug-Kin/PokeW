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

        if (wildPokemon != null && enemyUnit != null)
        {
            wildPokemon.Init();
            enemyUnit.Setup(wildPokemon);
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

        var appearingName = enemyUnit != null && enemyUnit.Pokemon != null ? enemyUnit.Pokemon.Base.Name : (wildPokemon != null ? wildPokemon.Base.Name : "");
        
        yield return StartCoroutine(dialogBox.TypeDialog($"Pokemon hoang dã {appearingName} xuất hiện!"));
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

        // 🔥 ĐÃ THÊM DÒNG NÀY ĐỂ BẬT LẠI KHUNG CHỮ TRƯỚC KHI THỰC HIỆN LỆNH
        dialogBox.EnableDialogText(true);

        if (item is GourdballItem gourdball)
        {
            yield return dialogBox.TypeDialog($"Bạn đã ném {gourdball.itemName}!");
            yield return new WaitForSeconds(0.5f);

            // Đặt vị trí của hiệu ứng dịch chuyển tới ngay vị trí của Pokemon địch
            catchEffectAnimator.gameObject.transform.position = enemyUnit.transform.position;
            catchEffectAnimator.gameObject.SetActive(true);

            // Ẩn Pokemon địch đi (giống như bị hút vào trong hồ lô)
            enemyUnit.gameObject.SetActive(false);

            // Tính toán tỷ lệ bắt như cũ
            float maxHP = enemyUnit.Pokemon.MaxHp;
            float currentHP = enemyUnit.Pokemon.HP;
            float catchChance = ((3 * maxHP - 2 * currentHP) / (3 * maxHP)) * gourdball.catchRateModifier;
            bool isCaught = UnityEngine.Random.value <= catchChance;

            // Đợi 1 giây xem hiệu ứng ném ban đầu quay quay
            yield return new WaitForSeconds(1.0f);

            if (isCaught)
            {
                // KÍCH HOẠT HIỆU ỨNG BẮT THÀNH CÔNG (image_3)
                catchEffectAnimator.SetTrigger("success");

                // Đợi hiệu ứng màng bọc vàng chạy được một nửa thì tắt nhạc nền và hiện chữ
                yield return new WaitForSeconds(0.5f);

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.StopBattleMusic();
                    if (catchSound != null) SoundManager.Instance.PlaySFX(catchSound);
                }

                yield return dialogBox.TypeDialog($"Tuyệt vời! Bạn đã bắt được {enemyUnit.Pokemon.Base.Name}!");

                // Đợi thêm cho hiệu ứng thu nhỏ biến mất hoàn toàn
                yield return new WaitForSeconds(1.5f);
                catchEffectAnimator.gameObject.SetActive(false);

                playerParty.AddPokemon(enemyUnit.Pokemon);
                BattleOver(true);
                yield break;
            }
            else
            {
                // KÍCH HOẠT HIỆU ỨNG BẮT THẤT BẠI - NỔ TUNG (image_4)
                catchEffectAnimator.SetTrigger("fail");

                // Đợi hiệu ứng nổ tung nứt vỡ chạy xong
                yield return new WaitForSeconds(1.5f);
                catchEffectAnimator.gameObject.SetActive(false);

                // Pokemon địch thoát ra ngoài -> Hiện lại hình ảnh Pokemon địch trên sân
                enemyUnit.gameObject.SetActive(true);
                yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} đã thoát ra được!");
            }
        }
        else
        {
            // ... Đây là phần code dùng thuốc (Item thường) giữ nguyên của bạn ...
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

        // Kết thúc lượt của mình, chuyển sang lượt địch đánh
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

    IEnumerator HandleRunAway()
    {
        state = BattleState.Busy;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableMoveSelector(false);
        dialogBox.EnableDialogText(true);

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
                
                partyScreen.SetPartyData(playerParty.Pokemons, true, OnPartyMemberClicked, null, OnPartyOutButtonClicked);
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
        // Nếu trận đấu đã kết thúc thì không cập nhật gì nữa
        if (battleIsOver) return;

        // Nếu đang ở màn hình chọn Vật phẩm (Bag)
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

            // Gọi hàm cập nhật liên tục mỗi khung hình cho InventoryUI để nhận phím/chuột
            inventoryUI.HandleUpdate(onBack, onUsed);
        }
        // Nếu đang ở màn hình chọn Pokemon (Party)
        else if (state == BattleState.PartyScreen)
        {
            // Nếu PartyScreen của bạn cũng có hàm HandleUpdate() bằng bàn phím, hãy gọi nó ở đây:
            // partyScreen.HandleUpdate(OnPartyMemberClicked, OnPartyBackButtonClicked);
        }
    }
}