using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StarterSelection : MonoBehaviour
{
    [Header("DỮ LIỆU GỐC (Kéo File từ thư mục Resources vào)")]
    public PokemonBase[] starterPokemons; // Chứa 5 file Data của Pokemon
    public TypeColorConfig typeConfig;    // File MasterTypeConfig lấy Icon hệ

    [Header("GIAO DIỆN (Kéo 5 ô vuông trắng trên Canvas vào)")]
    public Image[] pokemonButtonImages;   // 5 cái nút để hiển thị ảnh

    [Header("BẢNG THÔNG TIN INFOR (Kéo UI vào)")]
    public GameObject inforPanel;         // Object "Infor" tổng
    public Image pokeArt;                 // Ảnh to trong Infor
    public Image elementIcon;             // Icon hệ
    public TextMeshProUGUI elementText;   // Chữ hệ
    public TextMeshProUGUI nameText;      // Tên Pokemon
    public TextMeshProUGUI statText;      // Bảng 6 chỉ số
    public TextMeshProUGUI desText;       // Dòng tiểu sử

    // Biến tĩnh lưu trữ Pokemon được chọn để mang sang SampleScene
    public static PokemonBase chosenStarter;

    // Biến nội bộ nhớ xem người chơi đang xem con thứ mấy
    private int currentlyViewingIndex = -1;

    void Start()
    {
        // 1. Tự động ẩn bảng Infor đi
        if (inforPanel != null)
        {
            inforPanel.SetActive(false);
        }

        // 2. Tự động lấy ảnh từ Dữ Liệu Gốc dán lên 5 ô Giao Diện
        for (int i = 0; i < starterPokemons.Length; i++)
        {
            // Kiểm tra xem có đủ nút UI và đủ file Data không
            if (i < pokemonButtonImages.Length && starterPokemons[i] != null)
            {
                pokemonButtonImages[i].sprite = starterPokemons[i].FrontSprite;
            }
        }
    }

    // 🟢 HÀM XỬ LÝ KHI BẤM VÀO 1 TRONG 5 NÚT POKEMON
    public void OnClickPokemonButton(int index)
    {
        if (index < 0 || index >= starterPokemons.Length) return;

        currentlyViewingIndex = index;
        PokemonBase selectedBase = starterPokemons[index];

        // Cập nhật mọi thông tin từ Data lên UI
        pokeArt.sprite = selectedBase.FrontSprite;
        elementText.text = selectedBase.Type.ToString();

        if (typeConfig != null)
        {
            Sprite icon = typeConfig.GetIconForType(selectedBase.Type);
            if (icon != null)
            {
                elementIcon.gameObject.SetActive(true);
                elementIcon.sprite = icon;
            }
            else elementIcon.gameObject.SetActive(false); 
        }

        nameText.text = selectedBase.Name;
        desText.text = selectedBase.Description;
        statText.text = $"HP: {selectedBase.MaxHp}\n" +
                        $"Tấn công: {selectedBase.Attack}\n" +
                        $"Phòng thủ: {selectedBase.Defense}\n" +
                        $"TC Đặc biệt: {selectedBase.SpAttack}\n" +
                        $"PT Đặc biệt: {selectedBase.SpDefense}\n" +
                        $"Tốc độ: {selectedBase.Speed}";

        // Bật bảng Infor lên
        inforPanel.SetActive(true);
    }

    // 🟡 HÀM XỬ LÝ NÚT BACK (Quay lại)
    public void OnBackClicked()
    {
        inforPanel.SetActive(false);
    }

    // 🔴 HÀM XỬ LÝ NÚT CONFIRM (Xác nhận)
    public void OnConfirmClicked()
    {
        if (currentlyViewingIndex != -1)
        {
            chosenStarter = starterPokemons[currentlyViewingIndex];
            Debug.Log($"[HÀNH TRÌNH] Bạn đã chọn {chosenStarter.Name} làm bạn đồng hành!");
            
            // Chuyển sang map chính
            SceneManager.LoadScene("SampleScene"); 
        }
    }
}