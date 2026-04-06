using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy, PartyScreen }
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleHud playerHud;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud enemyHud;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;
    int currentMember;

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
        enemyUnit.Setup(wildPokemon);
        enemyHud.SetData(enemyUnit.Pokemon);

        partyScreen.Init();

        // Only set move names if the moves list exists
        dialogBox.SetMoveNames(playerUnit.Pokemon?.Moves);
        yield return StartCoroutine(dialogBox.TypeDialog($" {enemyUnit.Pokemon.Base.Name} xuất hiện"));
        yield return new WaitForSeconds(1f);

        PlayerAction();

    }

    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        dialogBox.SetDialog("Chọn hành động");
        dialogBox.EnableActionSelector(true);
    }
    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
        dialogBox.SetDialog("Chọn chiêu");
    }
    IEnumerator PerformPlayerMove()
    {
        state = BattleState.Busy;

        if (playerUnit.Pokemon == null || playerUnit.Pokemon.Moves == null || currentMove < 0 || currentMove >= playerUnit.Pokemon.Moves.Count)
        {
            Debug.LogError("BattleSystem: Invalid move selection");
            yield break;
        }

        var move = playerUnit.Pokemon.Moves[currentMove];
        move.PP--;
        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} đã sử dụng {move.Base.Name}");
        yield return new WaitForSeconds(1f);

        playerUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        enemyUnit.PlayHitAnimation();

        // Call TakeDamage using the Move and the attacker (player)
        var damageDetails = enemyUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon);
        yield return enemyHud.UpdateHP();
        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} đã bị hạ gục");
            enemyUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
            OnBattleOver(true);
        }
        else
        {
            StartCoroutine(EnemyMove());
        }

    }
    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;

        var move = enemyUnit.Pokemon.GetRanDomMove();
        move.PP--;
        yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} đã sử dụng {move.Base.Name}");
        yield return new WaitForSeconds(1f);

        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        playerUnit.PlayHitAnimation();

        var damageDetails = playerUnit.Pokemon.TakeDamage(move, enemyUnit.Pokemon);
        yield return playerHud.UpdateHP();
        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} đã bị hạ gục");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);

            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                OpenPartyScreen();
            }
            else
            {
                OnBattleOver(false);
            }

        }
        else
        {
            // back to player action
            yield return new WaitForSeconds(0.5f);
            PlayerAction();
        }
    }

    public void HandleUpdate()
    {
        if (state == BattleState.PlayerAction)
        {
            HandleActionSelection();

        }
        else if (state == BattleState.PlayerMove)
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
                PlayerMove();
            }
            else if (currentAction == 1)
            {
                // Bag or other (use same behavior for now)
                PlayerMove();
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
            StartCoroutine(PerformPlayerMove());
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());

        }
        else
            if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            PlayerAction();
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
              
                PlayerAction();
        }

    }
    IEnumerator SwitchPokemon(Pokemon newPokemon)
    { 

        if (playerUnit.Pokemon.HP > 0)
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} rút lui");
        playerUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        playerUnit.Setup(newPokemon);
        playerHud.SetData(newPokemon);
        dialogBox.SetMoveNames(newPokemon.Moves);
        yield return new WaitForSeconds(1f);
        yield return dialogBox.TypeDialog($"Tiếp theo là {playerUnit.Pokemon.Base.Name}");
       
        StartCoroutine(EnemyMove());
    }
    
}