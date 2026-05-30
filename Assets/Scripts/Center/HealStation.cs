using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Tilemaps; // 1. BẮT BUỘC phải thêm thư viện này để điều khiển Tilemap

public class HealStation : MonoBehaviour
{
    [Header("UI Giao Tiếp")]
    public GameObject dialogBox;
    public TextMeshProUGUI dialogText;

    [Header("Hiệu Ứng Nhấp Nháy (Đã đổi sang Tilemap)")]
    public Tilemap machineTilemap; // 2. Đổi từ SpriteRenderer sang Tilemap
    public Color flashColor = Color.red;
    public float flashSpeed = 0.2f;

    private bool isHealing = false;
    private bool isPlayerInRange = false;
    private PlayerControl currentPlayer;

    private void Start()
    {
        // Tự động tìm linh kiện Tilemap trên chính nó nếu bạn quên kéo thả
        if (machineTilemap == null)
        {
            machineTilemap = GetComponent<Tilemap>();
        }
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E) && !isHealing)
        {
            if (currentPlayer != null)
            {
                StartCoroutine(HealRoutine(currentPlayer));
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            currentPlayer = other.GetComponent<PlayerControl>();
            Debug.Log("Đến gần quầy: Nhấn E để hồi máu");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            currentPlayer = null;
        }
    }

    private IEnumerator HealRoutine(PlayerControl player)
    {
        isHealing = true;
        player.canMove = false;

        // 1. Tắt âm thanh bản đồ
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PauseMapAudio();
        }

        // 2. Bật UI thông báo
        dialogBox.SetActive(true);
        dialogText.text = "Đang hồi phục cho Pokemon...";

        // 3. Phát âm thanh hồi máu
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayHealSound();
        }

        // ==========================================================
        // 4. HIỆU ỨNG NHẤP NHÁY ĐỎ CHO TILEMAP
        // ==========================================================
        float duration = 2.0f;
        float elapsed = 0f;
        bool isRed = false;

        while (elapsed < duration)
        {
            // Thay vì đổi màu Sprite, ta đổi màu trực tiếp trên Tilemap chứa cái máy
            if (machineTilemap != null)
            {
                machineTilemap.color = isRed ? Color.white : flashColor;
            }

            isRed = !isRed;

            yield return new WaitForSeconds(flashSpeed);
            elapsed += flashSpeed;
        }

        // Đảm bảo sau khi xong, máy quay trở lại màu trắng gốc
        if (machineTilemap != null) machineTilemap.color = Color.white;
        // ==========================================================

        dialogText.text = "Pokemon đã hoàn toàn khỏe mạnh!";
        yield return new WaitForSeconds(1.5f);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ResumeMapAudio();
        }

        dialogBox.SetActive(false);
        player.canMove = true;
        isHealing = false;
    }
}