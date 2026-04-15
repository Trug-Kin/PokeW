using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PokemonParrty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;

    public List<Pokemon> Pokemons {
        get { 
        
        return pokemons;
        }
    
    
    }

    public void Start()
    {
        foreach (var pokemon in pokemons)
        {
            if (pokemon != null)
                pokemon.Init();
        }
    }

    public Pokemon GetHealthyPokemon()
    {
      return pokemons.Where(x => x.HP > 0).FirstOrDefault();
    }
    public void AddPokemon(Pokemon newPokemon)
    {
        if (pokemons.Count < 6)
        {
            pokemons.Add(newPokemon);
        }
        else
        {
            // TODO: Thêm code chuyển Pokemon vào PC Box khi đội hình đã đủ 6 con
            Debug.Log("Đội hình đã đầy 6 con!");
        }
    }
}
