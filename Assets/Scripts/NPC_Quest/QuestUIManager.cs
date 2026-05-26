using UnityEngine;
using TMPro; // BẮT BUỘC thêm dòng này để code nhận diện được TextMeshPro

public class QuestUIManager : MonoBehaviour
{
    [Header("UI Nhiệm vụ (Đã sửa đổi TextMeshPro)")]
    public GameObject questPanel; // Khung UI chứa chữ và nút

    // Đổi kiểu dữ liệu từ Text sang TextMeshProUGUI để khớp với Unity của bạn
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    private NPCQuestGiver currentNPC;

    // Bật bảng UI và điền chữ vào
    public void ShowQuestPanel(NPCQuestGiver npc, string title, string description)
    {
        currentNPC = npc;

        // Kiểm tra an toàn bảo vệ game không bị Crash/Null nếu lỡ quên kéo thả
        if (titleText != null) titleText.text = title;
        if (descriptionText != null) descriptionText.text = description;

        if (questPanel != null) questPanel.SetActive(true); // Hiện bảng UI
    }

    // Hàm gắn vào Nút "Chấp nhận" (Button Accept)
    public void OnAcceptButtonClicked()
    {
        if (currentNPC != null)
        {
            currentNPC.AcceptQuest();
        }
        if (questPanel != null) questPanel.SetActive(false); // Ẩn bảng UI
    }

    // Hàm gắn vào Nút "Từ chối" (Button Decline)
    public void OnDeclineButtonClicked()
    {
        if (questPanel != null) questPanel.SetActive(false); // Chỉ ẩn bảng UI, không làm gì cả
    }

    // Hiện popup nhận thưởng
    public void ShowRewardUI(string itemName, int amount)
    {
        Debug.Log($"==== BẢNG THÔNG BÁO ====\nĐã nhận: {itemName} x{amount}");
    }
}