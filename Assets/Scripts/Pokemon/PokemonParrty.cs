using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PokemonParrty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;

    // 🔥 BÍ KÍP TRUYỀN DỮ LIỆU: Biến tĩnh (static) này sẽ sống xuyên suốt các Scene, không bao giờ bị xóa
    private static List<Pokemon> globalPokemonsHold = null;

    public List<Pokemon> Pokemons
    {
        get
        {
            return pokemons;
        }
    }

    public void Start()
    {
        if (globalPokemonsHold != null)
        {
            pokemons = globalPokemonsHold;
            Debug.Log($"[DỮ LIỆU] Đã khôi phục đội hình.");
        }
        else
        {
            globalPokemonsHold = pokemons;

            // 🔥 LOGIC MỚI: Tự động gán Starter vào vị trí đầu tiên ngay khi vào game
            // Bỏ dòng chữ "StarterSelection" nếu file của cậu tên khác nhé!
            if (StarterSelection.chosenStarter != null && pokemons.Count > 0)
            {
                pokemons[0].Base = StarterSelection.chosenStarter;
            }

            foreach (var pokemon in pokemons)
            {
                if (pokemon != null && pokemon.Base != null)
                    pokemon.Init();
            }
        }
    }
    public void HealAllPokemon()
    {
        foreach (var pokemon in pokemons)
        {
            if (pokemon != null)
            {
                pokemon.Heal(pokemon.MaxHp);   // Hồi full máu
                pokemon.CureStatus();          // Giải trạng thái
            }
        }

        Debug.Log("Toàn bộ Pokemon đã được hồi phục!");
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
            Debug.Log($"[ĐÃ BẮT] Thêm thành công {newPokemon.Base.Name} vào danh sách!");
        }
        else
        {
            // TODO: Thêm code chuyển Pokemon vào PC Box khi đội hình đã đủ 6 con
            Debug.Log("Đội hình đã đầy 6 con!");
        }
    }
}