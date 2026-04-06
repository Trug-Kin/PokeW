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

        // get player party and a wild pokemon from the map, then start battle
        var playerParty = playerControl.GetComponent<PokemonParrty>();
        var wildPokemon = FindObjectOfType<MapArena>().GetRandomWildPokemon();

        battleSystem.StartBattle(playerParty, wildPokemon);
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
