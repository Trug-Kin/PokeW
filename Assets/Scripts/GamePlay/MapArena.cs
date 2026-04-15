using UnityEngine;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;


public class MapArena : MonoBehaviour
{
    [SerializeField] List<Pokemon> wildPokemons;
    
    public Pokemon GetRandomWildPokemon()
    {
       var wildPokemon = wildPokemons[Random.Range(0, wildPokemons.Count)];
        wildPokemon.Init();
                return wildPokemon;
    }

}
