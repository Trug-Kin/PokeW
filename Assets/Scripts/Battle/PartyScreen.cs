using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;
    [SerializeField] PartyMember[] memberSlots; 

    [Header("Cấu hình Nút Bấm (Kéo thả GameObject thoải mái 100%)")]
    [SerializeField] GameObject backButton; 
    [SerializeField] GameObject outButton;  

    List<Pokemon> pokemons;

    public void Init()
    {
        // Tự động gom đủ tất cả các ô slot Pokémon đang có dưới cây thư mục
        memberSlots = GetComponentsInChildren<PartyMember>(true);
    }

    public void SetPartyData(List<Pokemon> pokemons, bool isActivePokemonFainted, 
        Action<int> onMemberClicked = null, Action onBackClicked = null, Action onOutClicked = null)
    {
        this.pokemons = pokemons;

        // 1. TỰ ĐỘNG KIỂM TRA VÀ CẮM DÂY CLICK CHO 6 Ô POKÉMON
        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < pokemons.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].SetData(pokemons[i]);

                // THẦN CHÚ TỰ ĐỘNG: Nếu ô Pokemon chưa có linh kiện Button, tự động thêm vào luôn!
                Button slotButton = memberSlots[i].GetComponent<Button>();
                if (slotButton == null) 
                {
                    slotButton = memberSlots[i].gameObject.AddComponent<Button>();
                    // Cấu hình màu sắc khi rê chuột qua ô Pokemon cho đẹp mắt
                    ColorBlock cb = slotButton.colors;
                    cb.highlightedColor = new Color(0.8f, 0.9f, 1f, 1f); // Sáng xanh nhẹ khi rê chuột
                    slotButton.colors = cb;
                }

                slotButton.onClick.RemoveAllListeners(); 
                int index = i; 
                slotButton.onClick.AddListener(() => {
                    Debug.Log($"[CLICK] Đã nhận tín hiệu click chuột vào ô Pokémon số: {index}");
                    onMemberClicked?.Invoke(index);
                });
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false); 
            }
        }

        // 2. TỰ ĐỘNG XỬ LÝ VÀ CẮM CHUỘT CHO NÚT BACK
        if (backButton != null)
        {
            backButton.SetActive(!isActivePokemonFainted);
            
            Button btn = backButton.GetComponent<Button>();
            if (btn == null) btn = backButton.AddComponent<Button>();

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => {
                Debug.Log("[CLICK] Bạn đã click chọn nút BACK!");
                onBackClicked?.Invoke();
            });
        }

        // 3. TỰ ĐỘNG XỬ LÝ VÀ CẮM CHUỘT CHO NÚT OUT
        if (outButton != null)
        {
            outButton.SetActive(isActivePokemonFainted);
            
            Button btn = outButton.GetComponent<Button>();
            if (btn == null) btn = outButton.AddComponent<Button>();

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => {
                Debug.Log("[CLICK] Bạn đã click chọn nút OUT!");
                onOutClicked?.Invoke();
            });
        }

        if (isActivePokemonFainted)
            messageText.text = "Pokémon đã bại trận! Hãy chọn chiến binh tiếp theo hoặc chọn THOÁT.";
        else
            messageText.text = "Chọn một Pokémon đồng đội để đổi mốc lượt...";
    }
}