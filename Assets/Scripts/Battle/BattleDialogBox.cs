using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro; // Sử dụng thư viện TextMeshPro cho khung chỉ số đỏ bên phải

public class BattleDialogBox : MonoBehaviour
{
    [Header("Cấu hình Dialogue")]
    [SerializeField] Text dialogText; // ĐÃ GIỮ NGUYÊN: Kiểu Text thường của UnityEngine.UI đúng cấu trúc Canvas của bạn
    [SerializeField] int lettersPerSecond = 30;
    [SerializeField] float postDialogDelay = 1.0f;

    [Header("Cấu hình Lựa chọn Action")]
    [SerializeField] Color highlightedColor = Color.blue;
    [SerializeField] GameObject actionSelector;
    [SerializeField] List<Image> actionImages; // Danh sách hình ảnh nút hành động Fight, Bag, Run...

    [Header("Cấu hình Ô Chọn Chiêu thức (Chuột)")]
    [SerializeField] GameObject moveSelector;
    [SerializeField] List<Image> moveSlots; // List<Image> giúp bạn dễ dàng kéo thả ô hình ảnh vào Element

    [Header("Thành phần hiển thị trong Khung đỏ bên phải (moveDetails)")]
    [SerializeField] GameObject moveDetails;
    [SerializeField] TextMeshProUGUI detailNameText;       
    [SerializeField] TextMeshProUGUI detailPowerText;      
    [SerializeField] TextMeshProUGUI detailAccuracyText;   
    [SerializeField] TextMeshProUGUI detailCategoryText;   

    public void SetDialog(string dialog)
    {
        if (dialogText != null) dialogText.text = dialog;
    }

    public IEnumerator TypeDialog(string dialog)
    {
        // 1. TỰ ĐỘNG ẨN KHUNG 3 NÚT KHI CHỮ BẮT ĐẦU CHẠY
        EnableActionSelector(false);
        if (moveSelector != null) moveSelector.SetActive(false);
        if (moveDetails != null) moveDetails.SetActive(false);

        dialogText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }

        yield return new WaitForSeconds(postDialogDelay);
    }

    public void EnableDialogText(bool enabled) => dialogText.enabled = enabled;
    public void EnableActionSelector(bool enabled) => actionSelector.SetActive(enabled);
    
    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled); 
    }

    public void UpdateActionSelection(int selectedAction)
    {
        for (int i = 0; i < actionImages.Count; i++)
        {
            if (actionImages[i] != null)
                actionImages[i].color = (i == selectedAction) ? highlightedColor : Color.white;
        }
    }

    // ĐÃ SỬA LỖI: Lấy script xử lý MoveSlotUI nằm trên linh kiện Image để nạp dữ liệu và tự ẩn ô chiêu thức thừa
    // Phân phối dữ liệu Pokémon xuống các ô hình ảnh thông qua script gắn kèm
    public void SetMoveSlotsData(List<Move> moves, Action<Move> onHover, Action<int> onClick)
    {
        if (moveSlots == null || moveSlots.Count == 0) return;

        for (int i = 0; i < moveSlots.Count; i++)
        {
            if (moveSlots[i] == null) continue;

            // BẮT BỆNH: Tìm mọi cách truy vết script MoveSlotUI (tự tìm chính nó, tìm lên cha, tìm xuống con)
            MoveSlotUI slotScript = moveSlots[i].GetComponent<MoveSlotUI>();
            if (slotScript == null) slotScript = moveSlots[i].GetComponentInParent<MoveSlotUI>();
            if (slotScript == null) slotScript = moveSlots[i].GetComponentInChildren<MoveSlotUI>();
            
            if (slotScript != null)
            {
                if (moves != null && i < moves.Count)
                {
                    // Nạp dữ liệu tên chiêu, hệ và PP thực tế vào ô UI
                    slotScript.SetupSlot(i, moves[i], onHover, onClick);
                }
                else
                {
                    // Ô thừa tự động ẩn sạch (Nền Image, Icon, PP) nếu Pokémon chỉ có 3 chiêu
                    slotScript.SetupSlot(i, null, null, null); 
                }
            }
            else
            {
                // Nếu đã quét hết các tầng mà vẫn trống, hệ thống mới phát cảnh báo lên Console cho bạn biết
                Debug.LogWarning($"GamePokew ơi! Linh kiện ở Element {i} bạn kéo vào hoàn toàn không chứa script 'MoveSlotUI' ở bất kỳ tầng nào đâu nè!");
                moveSlots[i].gameObject.SetActive(false);
            }
        }
    }

    public void UpdateMoveDetailsPanel(Move move)
    {
        if (move == null || move.Base == null) return;

        if (detailNameText != null) detailNameText.text = move.Base.Name.ToUpper();
        if (detailPowerText != null) detailPowerText.text = move.Base.Power > 0 ? $"POW: {move.Base.Power}" : "POW: --";
        if (detailAccuracyText != null) detailAccuracyText.text = move.Base.Accuracy > 0 ? $"ACC: {move.Base.Accuracy}%" : "ACC: --";
        if (detailCategoryText != null) detailCategoryText.text = move.Base.IsSpecial ? "LOẠI: ĐẶC BIỆT" : "LOẠI: VẬT LÝ";
    }
}