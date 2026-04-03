using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public enum GameState { FreeRoam, Battle }
public class GameControl : MonoBehaviour 
{

    [SerializeField] PlayerControl playerControl;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;

    GameState state;

    public void Start()
    {
        playerControl.OnEncountered+= StartBattle;
        battleSystem.OnBattleOver += EndBattle;
    }
    void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        battleSystem.BattleStart();
    }
    void EndBattle(bool won)
    {
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);

        
    }
    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
           playerControl.HandleUpdate();
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
    }

}
