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
        // HỆ THỐNG TỰ ĐỘNG KHÔI PHỤC DỮ LIỆU KHI ĐỔI SCENE
        if (globalPokemonsHold != null)
        {
            // Nếu đã có dữ liệu lưu từ trước (máu thấp, trạng thái xấu...), bê nguyên vẹn vào Scene mới
            pokemons = globalPokemonsHold;
            Debug.Log($"[DỮ LIỆU] Đã khôi phục đội hình. Máu của Pokemon được giữ nguyên trạng thái cũ!");

            // 🔥 KHÔNG gọi pokemon.Init() ở đây nữa để tránh bị reset lại đầy máu!
        }
        else
        {
            // Trường hợp này chỉ chạy ĐÚNG 1 LẦN DUY NHẤT khi bạn mới bấm nút Play game
            globalPokemonsHold = pokemons;

            // CHỈ KHỞI TẠO CHỈ SỐ LẦN ĐẦU TIÊN
            foreach (var pokemon in pokemons)
            {
                if (pokemon != null)
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