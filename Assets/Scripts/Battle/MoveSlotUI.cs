using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using TMPro; // Sử dụng TextMeshPro cho cả tên chiêu và số PP

public class MoveSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Thành phần UI lồng trong ô chiêu")]
    [SerializeField] TextMeshProUGUI moveNameText; // ĐÃ SỬA: Chuyển hẳn sang TextMeshProUGUI đồng bộ với Editor
    [SerializeField] Image typeIconImage;    // Hình ảnh Logo hệ
    [SerializeField] Image ppBarFill;        // Thanh màu xanh hiển thị PP (Filled Image)
    [SerializeField] TextMeshProUGUI ppText; // Ô số PP hiển thị bằng TextMeshPro

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

        // Tự động ẩn hoàn toàn nếu Pokémon không học chiêu này (Ví dụ chỉ có 3 chiêu)
        if (move == null || move.Base == null)
        {
            gameObject.SetActive(false); 
            return;
        }

        // ĐÁNH THỨC UI: Kích hoạt ô chiêu thức hiển thị lên màn hìnhCanvas
        gameObject.SetActive(true);

        if (moveNameText != null)
        {
            moveNameText.text = move.Base.Name;
            moveNameText.color = Color.black; 
        }

        // Hiển thị Logo hình ảnh cho hệ chiêu thức
        if (typeIconImage != null)
        {
            if (move.Base.TypeIcon != null)
            {
                typeIconImage.gameObject.SetActive(true);
                typeIconImage.sprite = move.Base.TypeIcon;
            }
            else
            {
                typeIconImage.gameObject.SetActive(false);
            }
        }

        // Cập nhật số PP bằng TextMeshPro
        if (ppText != null)
        {
            ppText.text = $"{move.PP}/{move.Base.PP}";
        }

        // Cập nhật độ dài thanh Fill năng lượng màu xanh
        if (ppBarFill != null)
        {
            float ppRatio = (float)move.PP / move.Base.PP;
            ppBarFill.fillAmount = ppRatio;

            // Đổi màu thanh sang đỏ nếu chiêu thức sắp cạn PP
            if (ppRatio <= 0.2f) ppBarFill.color = Color.red;
            else ppBarFill.color = new Color(0f, 0.75f, 1f); 
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (moveNameText != null) moveNameText.color = Color.blue; // Đổi màu chữ khi rê chuột vào
        onHoverCallback?.Invoke(moveData); 
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (moveNameText != null) moveNameText.color = Color.black; 
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClickCallback?.Invoke(slotIndex); // Bấm chuột để kích hoạt chọn chiêu
    }
}