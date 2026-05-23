using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using TMPro; // Sử dụng TextMeshPro cho cả tên chiêu và số PP

public class MoveSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Thành phần UI lồng trong ô chiêu")]
    [SerializeField] TextMeshProUGUI moveNameText; 
    [SerializeField] Image typeIconImage;    
    [SerializeField] Image ppBarFill;        
    [SerializeField] TextMeshProUGUI ppText; 

    // 🔥 MỚI THÊM: Biến kết nối với kho ảnh hệ Ngũ Hành
    [Header("Kho dữ liệu hệ")]
    [SerializeField] TypeColorConfig typeConfig; 

    private int slotIndex;
    private Move moveData;
    private Action<Move> onHoverCallback;
    private Action<int> onClickCallback;

    public void SetupSlot(int index, Move move, Action<Move> onHover, Action<int> onClick)
    {
        slotIndex = index;
        moveData = move;
        onHoverCallback = onHover;
        onClickCallback = onClick;

        if (move == null || move.Base == null)
        {
            gameObject.SetActive(false); 
            return;
        }

        gameObject.SetActive(true);

        if (moveNameText != null)
        {
            moveNameText.text = move.Base.Name;
            moveNameText.color = Color.black; 
        }

        // 🔥 ĐÃ SỬA: Lấy Logo hệ chiêu thức từ kho MasterTypeConfig thay vì từ MoveBase
        if (typeIconImage != null)
        {
            if (typeConfig != null)
            {
                Sprite icon = typeConfig.GetIconForType(move.Base.Type);
                if (icon != null)
                {
                    typeIconImage.gameObject.SetActive(true);
                    typeIconImage.sprite = icon;
                }
                else
                {
                    typeIconImage.gameObject.SetActive(false);
                }
            }
            else
            {
                typeIconImage.gameObject.SetActive(false);
                Debug.LogWarning("Chưa gắn MasterTypeConfig vào MoveSlotUI!");
            }
        }

        if (ppText != null)
        {
            ppText.text = $"{move.PP}/{move.Base.PP}";
        }

        if (ppBarFill != null)
        {
            float ppRatio = (float)move.PP / move.Base.PP;
            ppBarFill.fillAmount = ppRatio;

            if (ppRatio <= 0.2f) ppBarFill.color = Color.red;
            else ppBarFill.color = new Color(0f, 0.75f, 1f); 
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (moveNameText != null) moveNameText.color = Color.blue; 
        onHoverCallback?.Invoke(moveData); 
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (moveNameText != null) moveNameText.color = Color.black; 
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClickCallback?.Invoke(slotIndex); 
    }
}