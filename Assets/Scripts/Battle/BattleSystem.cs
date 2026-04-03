using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;


public class BattleSystem : MonoBehaviour 
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleHud playerHud;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud enemyHud;
    [SerializeField] BattleDialogBox dialogBox;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;
    public void BattleStart()
    {
        StartCoroutine(SetupBattle());
    }
   public IEnumerator SetupBattle()
    {
        playerUnit.Setup();
        playerHud.SetData(playerUnit.Pokemon);
        enemyUnit.Setup();
        enemyHud.SetData(enemyUnit.Pokemon);

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        yield return StartCoroutine (dialogBox.TypeDialog($" {enemyUnit.Pokemon.Base.Name} xuất hiện"));
        yield return new WaitForSeconds(1f);

        PlayerAction();

    }

    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        dialogBox.SetDialog("Chọn hành động");
        dialogBox.EnableActionSelector(true);
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
        state = BattleState.Bussy;

        var move = playerUnit.Pokemon.Moves[currentMove];
        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} đã sử dụng {move.Base.Name}");
        yield return new WaitForSeconds(1f);

        playerUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        enemyUnit.PlayHitAnimation();

        // Call TakeDamage using the Move and the attacker (player)
        bool isFainted = enemyUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon);
        yield return enemyHud.UpdateHP();
        if (isFainted)
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

        var move = enemyUnit.Pokemon.Moves[0];
        yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} đã sử dụng {move.Base.Name}");
        yield return new WaitForSeconds(1f);

        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        playerUnit.PlayHitAnimation();

        bool isFainted = playerUnit.Pokemon.TakeDamage(move, enemyUnit.Pokemon);
        yield return playerHud.UpdateHP();
        if (isFainted)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} đã bị hạ gục");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
            OnBattleOver(false);
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
    }
    void HandleActionSelection()
    { 
    
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 1)
                ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAction > 0)
                --currentAction;
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
                // Run
                PlayerMove();
            }

        }
        
    }
    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 1)
                ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentMove > 0)
                --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 2)
                currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMove > 1)
                currentMove -= 2;
        }

        // Ensure currentMove stays within valid bounds
        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[(currentMove)]);
        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());
        }
    }
}
