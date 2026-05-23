using UnityEngine;
using System.Collections;
using TMPro; // Quản lý chữ UI

public class HealingMachine : MonoBehaviour
{
    [Header("Dữ liệu & Hiệu ứng")]
    public PokemonParrty playerParty;       // Kéo thả Object chứa PokemonParty của Player
    public Animator machineAnimator;       // Animator chớp đèn Pokéball (nếu có)

    [Header("Cấu hình UI Thông Báo")]
    public GameObject notificationPanel;   // Ô khung chứa thông báo (Panel)
    public TextMeshProUGUI notificationText;// Bản chữ hiển thị thông báo
    public float displayTime = 2.5f;       // Thời gian hiện chữ trên màn hình

    private bool isHealing = false;        // Biến kiểm tra để chống spam

    // Hàm public để script khác (như HealZone) có thể gọi tới
    public void StartHealingProcess()
    {
        if (!isHealing)
        {
            StartCoroutine(HealSequence());
        }
    }

    private IEnumerator HealSequence()
    {
        isHealing = true;
        Debug.Log("Máy bắt đầu hoạt động...");

        // 1. Phát âm thanh hồi máu từ SoundManager
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayHealSound();
        }

        // 2. Chạy đèn hiệu ứng hoặc đợi nhạc chạy xong
        if (machineAnimator != null)
        {
            machineAnimator.SetTrigger("Heal");
            yield return new WaitForSeconds(3.0f);
        }
        else
        {
            yield return new WaitForSeconds(2.0f); // Thời gian chờ mặc định
        }

       // 3. Thực hiện hồi máu trong dữ liệu
        if (playerParty != null)
        {
            playerParty.HealAllPokemon();
            
            // 🔥 THÊM DÒNG NÀY: Báo ra Console với chữ màu Xanh Lá cực kỳ nổi bật
            Debug.Log("<color=#00FF00><b>[TRUNG TÂM POKEMON] 💖 Tüm tích... Tèn tén ten! Tất cả Pokemon đã được hồi phục 100% HP và PP!</b></color>");
        }
        else
        {
            Debug.LogWarning("PlayerParty chưa được gán trong HealingMachine! Vui lòng kéo thả đúng đối tượng vào Inspector.");
        }
        // 4. Hiển thị thông báo thành công lên màn hình UI
        if (notificationPanel != null && notificationText != null)
        {
            notificationText.text = "Tất cả Pokémon đã được hồi phục thành công!";
            notificationPanel.SetActive(true);

            yield return new WaitForSeconds(displayTime);

            notificationPanel.SetActive(false);
        }

        isHealing = false;
    }
}