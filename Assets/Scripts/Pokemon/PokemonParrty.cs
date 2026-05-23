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
        // 1. KIỂM TRA XEM CÓ QUÀ TỪ MÀN HÌNH CHỌN (StarterScene) GỬI SANG KHÔNG?
        if (StarterSelection.chosenStarter != null)
        {
            // Tránh lỗi NullReferenceException nếu list chưa được tạo trên Inspector
            if (pokemons == null) pokemons = new List<Pokemon>();

            // Tạo Pokemon mới cấp 1
            Pokemon newStarter = new Pokemon(StarterSelection.chosenStarter, 1);
            
            pokemons.Clear(); 
            pokemons.Add(newStarter);

            // Bơm đầy máu ngay lập tức để không bị đột quỵ khi vào trận
            HealAllPokemon();

            // Cập nhật lại bộ nhớ dùng chung (globalPokemonsHold)
            globalPokemonsHold = pokemons;

            Debug.Log($"[HỆ THỐNG] Đã nhận {newStarter.Base.Name} (Lv.1) từ màn hình chọn!");

            // Xóa dữ liệu tạm ở trạm trung chuyển
            StarterSelection.chosenStarter = null;
        }
        // 2. NẾU KHÔNG CÓ QUÀ (Tức là chuyển đổi qua lại giữa các map bình thường sau này)
        else if (globalPokemonsHold != null)
        {
            pokemons = globalPokemonsHold;
            Debug.Log($"[DỮ LIỆU] Đã khôi phục đội hình từ bộ nhớ. Giữ nguyên trạng thái máu!");
        }
        // 3. TRƯỜNG HỢP TEST GAME TRỰC TIẾP TỪ SAMPLE SCENE (Không đi qua màn hình chọn)
        else
        {
            globalPokemonsHold = pokemons;
            foreach (var pokemon in pokemons)
            {
                if (pokemon != null)
                    pokemon.Init();
            }
        }
    }    public void HealAllPokemon()
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