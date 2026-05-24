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

        // 1. Tự động tìm Player nếu lỡ quên kéo thả trên Unity
        if (playerParty == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null) playerObj = GameObject.Find("Player"); // Dự phòng
            
            if (playerObj != null)
            {
                playerParty = playerObj.GetComponent<PokemonParrty>();
            }
        }

        // 2. Phát âm thanh hồi máu từ SoundManager
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayHealSound();
        }

        // 3. Chạy đèn hiệu ứng hoặc đợi nhạc chạy xong
        if (machineAnimator != null)
        {
            machineAnimator.SetTrigger("Heal");
            yield return new WaitForSeconds(3.0f);
        }
        else
        {
            yield return new WaitForSeconds(2.0f); 
        }

        // 4. Thực hiện hồi máu trong dữ liệu
        if (playerParty != null)
        {
            playerParty.HealAllPokemon();
        }
        else
        {
            Debug.LogError("Máy hồi máu không tìm thấy PlayerParty để hồi!");
        }

        // 5. Hiển thị thông báo thành công lên màn hình UI
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