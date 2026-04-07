using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public enum BattleState { Start, ActionSelection, MoveSelection, PerformMove, Busy, PartyScreen,BattleOver }
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud playerHud;
   
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

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

    // Compatibility wrapper: some callers use StartBattle
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
        
        // Set up enemy unit with the wild Pokémon
        if (wildPokemon != null && enemyUnit != null)
        {
            enemyUnit.Setup(wildPokemon);
        }
        

        partyScreen.Init();

        // Only set move names if the moves list exists
        dialogBox.SetMoveNames(playerUnit.Pokemon?.Moves);
        // Announce the wild Pokémon appearing (use enemy unit's Pokémon if available)
        var appearingName = enemyUnit != null && enemyUnit.Pokemon != null ? enemyUnit.Pokemon.Base.Name : (wildPokemon != null ? wildPokemon.Base.Name : "") ;
        yield return StartCoroutine(dialogBox.TypeDialog($"{appearingName} xuất hiện"));
        yield return new WaitForSeconds(1f);

        ActionSelection();

    }
    void BattleOver(bool won)
    { 
     state = BattleState.BattleOver;
        battleIsOver = true;
        // Stop any ongoing coroutines in this BattleSystem to prevent further actions
        StopAllCoroutines();
        OnBattleOver?.Invoke(won);


    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Chọn hành động");
        dialogBox.EnableActionSelector(true);
    }
    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
        dialogBox.SetDialog("Chọn chiêu");
    }
    IEnumerator PlayerMove()
    {
        state = BattleState.PerformMove;

        var playerMove = playerUnit.Pokemon.Moves[currentMove];
        var enemyMove = enemyUnit.Pokemon.GetRanDomMove();

        // Determine who goes first by Speed
        bool playerFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;

        if (playerFirst)
        {
            yield return RunMove(playerUnit, enemyUnit, playerMove);

            if (state == BattleState.BattleOver)
                yield break;

            if (enemyUnit.Pokemon.HP > 0)
                yield return RunMove(enemyUnit, playerUnit, enemyMove);
        }
        else
        {
            yield return RunMove(enemyUnit, playerUnit, enemyMove);

            if (state == BattleState.BattleOver)
                yield break;

            if (playerUnit.Pokemon.HP > 0)
                yield return RunMove(playerUnit, enemyUnit, playerMove);
        }

        if (state != BattleState.BattleOver)
            ActionSelection();
    }
    IEnumerator EnemyMove()
    {
        state = BattleState.PerformMove;

        var move = enemyUnit.Pokemon.GetRanDomMove();
        yield return RunMove(enemyUnit, playerUnit, move);
        if (state != BattleState.BattleOver)
            ActionSelection();  
    }
    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} đã sử dụng {move.Base.Name}");
        yield return new WaitForSeconds(1f);

        // Play attack animation on the source (attacker), not the target
        sourceUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        targetUnit.PlayHitAnimation();

        // Call TakeDamage using the Move and the attacker (player)
        var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
        yield return targetUnit.Hud.UpdateHP();

        // Show messages for critical hits and type effectiveness
        if (damageDetails.Critical > 1f)
        {
            yield return dialogBox.TypeDialog("Đòn chí mạng!");
        }

        if (damageDetails.TypeEffectiveness == 0f)
        {
            yield return dialogBox.TypeDialog("Không có tác dụng");
        }
        else if (damageDetails.TypeEffectiveness > 1f)
        {
            yield return dialogBox.TypeDialog("Rất hiệu quả!");
        }
        else if (damageDetails.TypeEffectiveness < 1f)
        {
            yield return dialogBox.TypeDialog("Không hiệu quả lắm...");
        }

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name} đã bị hạ gục");
            targetUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
            CheckForBattleOver(targetUnit);
            if (battleIsOver)
                yield break;
        }


    }
   void CheckForBattleOver(BattleUnit faintedUnit )
    {
        if (faintedUnit.IsPlayerUnit)
        { 
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                // Auto-switch to the next healthy Pokemon
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
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();

        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
    }
    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // move right (if possible)
            if (currentAction % 2 == 0 && currentAction < 3)
                ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // move left (if possible)
            if (currentAction % 2 == 1)
                --currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            // move down one row
            if (currentAction < 2)
                currentAction += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            // move up one row
            if (currentAction > 1)
                currentAction -= 2;
        }

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                // Fight
                MoveSelection();
            }
            else if (currentAction == 1)
            {
                // Bag or other (use same behavior for now)
                MoveSelection();
            }
            else if (currentAction == 2)
            {
                // Pokemon
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                // Run
                // TODO: implement run logic
            }
        }
    }
    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // move right (if possible)
            if (currentAction % 2 == 0 && currentAction < 3)
                ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // move left (if possible)
            if (currentAction % 2 == 1)
                --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            // move down one row
            if (currentAction < 2)
                currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            // move up one row
            if (currentMove > 1)
                currentMove -= 2;
        }

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        // Ensure currentMove stays within valid bounds
        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[(currentMove)]);
        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PlayerMove());
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PlayerMove());

        }
        else
            if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }
    void HandlePartySelection()
    {
        if (playerParty == null || playerParty.Pokemons == null || playerParty.Pokemons.Count == 0)
            return;
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // move right (if possible)
            if (currentMember % 2 == 0 && currentMember < 3)
                ++currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // move left (if possible)
            if (currentMember % 2 == 1)
                --currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            // move down one row
            if (currentMember < 2)
                currentMember += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            // move up one row
            if (currentMember > 1)
                currentMember -= 2;
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
       
       if (!battleIsOver)
           StartCoroutine(EnemyMove());
    }
    
}