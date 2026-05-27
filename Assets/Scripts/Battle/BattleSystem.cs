using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public enum BattleState { Start, ActionSelection, MoveSelection, PerformMove, Busy, PartyScreen, BagScreen, GourdBagScreen, BattleOver, SwitchPokemon, WildPokemonAppear }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud playerHud;

    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] Animator catchEffectAnimator; 
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

    PokemonParrty playerParty; 
    Pokemon wildPokemon;

    [SerializeField] bool isTrainerBattle = false;
    [SerializeField] List<Pokemon> trainerTeam;

    // 🔥 BIẾN TĨNH ĐẾM SỐ TRẬN (Dành cho Demo)
    private static int battleCountForDemo = 0;

    public void BattleStart(PokemonParrty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        this.isTrainerBattle = false; 
        this.trainerTeam = null;
        battleIsOver = false;

        StartCoroutine(SetupBattle());
    }

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
        if (firstPokemon == null) yield break;

        firstPokemon.Init();
        playerUnit.Setup(firstPokemon);
        playerHud.SetData(playerUnit.Pokemon);

        Pokemon activeEnemy = isTrainerBattle ? GetHealthyTrainerPokemon() : wildPokemon;

        // 🔥 LOGIC DEMO MỚI: Ép Level cho 2 trận đầu tiên
        if (activeEnemy != null)
        {
            if (battleCountForDemo < 2)
            {
                // Ép cấp độ địch bằng cấp độ của Pokemon người chơi đang ra trận
                activeEnemy.Level = firstPokemon.Level;
                Debug.Log($"[DEMO MODE] Đã ép cấp độ địch xuống {firstPokemon.Level} trong trận {battleCountForDemo + 1}");
            }
            
            activeEnemy.Init(); // Gọi lại Init để các chỉ số máu, sát thương được tính lại theo Level mới
            
            if (enemyUnit != null)
            {
                enemyUnit.Setup(activeEnemy);
            }
        }

        partyScreen.Init();
        
        PlayerInventory realInventory = playerParty.GetComponent<PlayerInventory>();
        if (realInventory != null)
        {
            inventoryUI.SetData(realInventory.inventory, true);
        }

        if (dialogBox != null && playerUnit.Pokemon != null)
        {
            dialogBox.SetMoveSlotsData(playerUnit.Pokemon.Moves, 
                (move) => dialogBox.UpdateMoveDetailsPanel(move),   
                (moveIndex) => MouseSelectMove(moveIndex)           
            );
        }

        dialogBox.EnableDialogText(true);

        var appearingName = enemyUnit != null && enemyUnit.Pokemon != null ? enemyUnit.Pokemon.Base.Name : "";

        if (isTrainerBattle)
            yield return StartCoroutine(dialogBox.TypeDialog($"Trainer thách đấu! Họ cử {appearingName} ra trận!"));
        else
            yield return StartCoroutine(dialogBox.TypeDialog($"Pokemon hoang dã {appearingName} xuất hiện!"));

        yield return new WaitForSeconds(1f);

        OpenParallelTurnMenu();
    }

void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        battleIsOver = true;
        battleCountForDemo++;

        // 🔥 THÊM VÀO ĐÂY: Ra lệnh cho toàn bộ đội hình xóa sạch Icon và Buff/Debuff
        if (playerParty != null && playerParty.Pokemons != null)
        {
            foreach (var p in playerParty.Pokemons)
            {
                if (p != null) p.OnBattleOver();
            }
        }

        if (SoundManager.Instance != null) SoundManager.Instance.StopBattleMusic();

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

        PlayerInventory realInventory = playerParty.GetComponent<PlayerInventory>();
        if (realInventory != null)
        {
            inventoryUI.SetData(realInventory.inventory, true);
        }
        
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

    IEnumerator UseItem(ItemBase item)
    {
        state = BattleState.Busy;
        inventoryUI.gameObject.SetActive(false);

        dialogBox.EnableDialogText(true);

        if (item is GourdballItem gourdball)
        {
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

        yield return EnemyMove(); 
    }

    IEnumerator PlayerMove()
    {
        state = BattleState.Busy;
        var playerMove = playerUnit.Pokemon.Moves[currentMove];

        Pokemon originalEnemy = enemyUnit.Pokemon;
        var enemyMove = originalEnemy.GetRanDomMove();

        bool playerFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;

        if (playerFirst)
        {
            yield return RunMove(playerUnit, enemyUnit, playerMove);
            if (state == BattleState.BattleOver) yield break;

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
        string beforeMoveMsg;
        bool canMove = sourceUnit.Pokemon.OnBeforeMove(out beforeMoveMsg);

        sourceUnit.Hud.UpdateStatusIcons();

        if (!string.IsNullOrEmpty(beforeMoveMsg))
        {
            yield return dialogBox.TypeDialog(beforeMoveMsg);
            yield return new WaitForSeconds(1.0f);
        }

        if (canMove)
        {
            move.PP--;
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} đã dùng {move.Base.Name}!");
            yield return new WaitForSeconds(0.8f);

            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(0.8f);
            targetUnit.PlayHitAnimation();

            if (move.Base.Power > 0)
            {
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                yield return targetUnit.Hud.UpdateHP();

                if (damageDetails.Critical > 1f) yield return dialogBox.TypeDialog("Đòn đánh hiểm hóc! Chí mạng!");
                if (damageDetails.TypeEffectiveness == 0f) yield return dialogBox.TypeDialog("Hoàn toàn không có tác dụng...");
                else if (damageDetails.TypeEffectiveness > 1f) yield return dialogBox.TypeDialog("Một đòn khắc hệ! Rất hiệu quả!");
                else if (damageDetails.TypeEffectiveness < 1f) yield return dialogBox.TypeDialog("Sát thương bị giảm! Không hiệu quả lắm...");
            }

            if (move.Base.Effects != null)
            {
                if (move.Base.Effects.boosts != null && move.Base.Effects.boosts.Count > 0)
                {
                    foreach (var boost in move.Base.Effects.boosts)
                    {
                        if (UnityEngine.Random.Range(1, 101) <= boost.chance)
                        {
                            var effectTarget = (boost.target == MoveTarget.Foe) ? targetUnit : sourceUnit;
                            effectTarget.Pokemon.ApplyBoosts(new List<StatBoost>() { boost });

                            effectTarget.Hud.UpdateStatusIcons();

                            string statName = boost.stat == Stat.Attack ? "Tấn Công" : (boost.stat == Stat.Defense ? "Phòng Thủ" : (boost.stat == Stat.SpAttack ? "Tấn Công Đ.Biệt" : (boost.stat == Stat.SpDefense ? "Phòng Thủ Đ.Biệt" : "Tốc Độ")));
                            string changeStr = boost.boost > 0 ? "tăng lên" : "giảm xuống";
                            yield return dialogBox.TypeDialog($"Chỉ số {statName} của {effectTarget.Pokemon.Base.Name} đã bị {changeStr}!");
                            yield return new WaitForSeconds(0.8f);
                        }
                    }
                }

                if (move.Base.Effects.statuses != null && move.Base.Effects.statuses.Count > 0)
                {
                    foreach (var statusEffect in move.Base.Effects.statuses)
                    {
                        if (UnityEngine.Random.Range(1, 101) <= statusEffect.chance)
                        {
                            var effectTarget = (statusEffect.target == MoveTarget.Foe) ? targetUnit : sourceUnit;
                            if (effectTarget.Pokemon.Status == ConditionID.None) 
                            {
                                effectTarget.Pokemon.SetStatus(statusEffect.id);

                                effectTarget.Hud.UpdateStatusIcons();

                                string conditionName = statusEffect.id == ConditionID.brn ? "bị thiêu đốt" : (statusEffect.id == ConditionID.frz ? "bị đóng băng" : "bị choáng");
                                yield return dialogBox.TypeDialog($"{effectTarget.Pokemon.Base.Name} đã {conditionName}!");
                                yield return new WaitForSeconds(0.8f);
                            }
                        }
                    }
                }
            }
        } 

        int afterTurnDamage;
        string afterTurnMsg;
        sourceUnit.Pokemon.OnAfterTurn(out afterTurnMsg, out afterTurnDamage);
        
        if (afterTurnDamage > 0)
        {
            yield return dialogBox.TypeDialog(afterTurnMsg);
            yield return sourceUnit.Hud.UpdateHP();
            yield return new WaitForSeconds(0.8f);
        }

        if (targetUnit.Pokemon.HP <= 0)
        {
            yield return HandlePokemonFainted(targetUnit, sourceUnit);
            if (battleIsOver) yield break;
        }
        else if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return HandlePokemonFainted(sourceUnit, targetUnit);
            if (battleIsOver) yield break;
        }
    }

    IEnumerator HandlePokemonFainted(BattleUnit faintedUnit, BattleUnit killerUnit)
    {
        yield return dialogBox.TypeDialog($"{faintedUnit.Pokemon.Base.Name} đã bị đánh bại, kiệt sức gục ngã!");
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(1.5f);

        if (!faintedUnit.IsPlayerUnit && killerUnit.IsPlayerUnit) 
        {
            int expYield = faintedUnit.Pokemon.Base.ExpYield;
            int enemyLevel = faintedUnit.Pokemon.Level;
            float expMultiplier = isTrainerBattle ? 1.5f : 1f;
            int expGained = Mathf.FloorToInt((expYield * enemyLevel * expMultiplier) / 7f);
            if (expGained <= 0) expGained = 1;

            killerUnit.Pokemon.Exp += expGained;
            yield return dialogBox.TypeDialog($"{killerUnit.Pokemon.Base.Name} nhận được {expGained} điểm kinh nghiệm!");
            
            yield return killerUnit.Hud.SetExpSmooth();
            yield return new WaitForSeconds(0.8f);

            int expNeeded = killerUnit.Pokemon.Base.GetExpForLevel(killerUnit.Pokemon.Level + 1);
            while (killerUnit.Pokemon.Exp >= expNeeded)
            {
                killerUnit.Pokemon.LevelUp();
                killerUnit.Hud.UpdateLevelText(); 
                
                yield return killerUnit.Hud.SetExpSmooth(true); 
                expNeeded = killerUnit.Pokemon.Base.GetExpForLevel(killerUnit.Pokemon.Level + 1);
                
                yield return dialogBox.TypeDialog($"Tuyệt quá! {killerUnit.Pokemon.Base.Name} đã tăng lên Cấp {killerUnit.Pokemon.Level}!");
                yield return new WaitForSeconds(1.0f);
                yield return killerUnit.Hud.UpdateHP(); 
            }
        }

        yield return StartCoroutine(CheckForBattleOver(faintedUnit));
    }

    IEnumerator CheckForBattleOver(BattleUnit faintedUnit)
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
            if (isTrainerBattle)
            {
                var nextEnemyPokemon = GetHealthyTrainerPokemon();
                if (nextEnemyPokemon != null)
                {
                    yield return StartCoroutine(HandleEnemySwitch(nextEnemyPokemon));
                }
                else
                {
                    BattleOver(true);
                }
            }
            else
            {
                BattleOver(true);
            }
        }
    }

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