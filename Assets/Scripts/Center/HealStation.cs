using System.Collections;
using UnityEngine;

public class HealStation : MonoBehaviour
{
    [Header("Cấu hình Khoảng cách")]
    public float interactDistance = 1.8f; // Khoảng cách đứng trước quầy lễ tân để nhấn E

    private Transform playerTransform;
    private bool isHealing = false; // Tránh việc người chơi spam phím E liên tục

    void Start()
    {
        FindPlayer();
    }

    void Update()
    {
        if (playerTransform == null)
        {
            FindPlayer();
            return;
        }

        // Tính khoảng cách đường thẳng từ Player tới cô Y tá
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        // Nếu đứng đủ gần quầy, nhấn E và y tá đang rảnh thì kích hoạt hồi máu
        if (distance <= interactDistance && Input.GetKeyDown(KeyCode.E) && !isHealing)
        {
            StartCoroutine(HealSequence());
        }
    }

    // Tiến trình hồi máu chạy theo thời gian thực (Coroutine)
    IEnumerator HealSequence()
    {
        isHealing = true;
        Debug.Log("[NURSE JOY]: Xin chào! Chào mừng bạn đến với Trung tâm Pokemon. Tôi sẽ giúp bạn phục hồi sức khỏe cho các Pokemon nhé...");

        // Giả lập thời gian máy quét hồi máu đang chạy (đợi 2 giây)
        yield return new WaitForSeconds(2.0f);

        // 🎯 GỌI SCRIPT CỦA BẠN: Tìm PokemonParrty trên người Player
        PokemonParrty playerParty = playerTransform.GetComponent<PokemonParrty>();

        if (playerParty != null)
        {
            // Kích hoạt hàm hồi máu + giải hiệu ứng xấu của bạn
            playerParty.HealAllPokemon();
            Debug.Log("[NURSE JOY]: Đã xong! Các Pokemon của bạn đã hoàn toàn khỏe mạnh trở lại. Chúc bạn một ngày tốt lành!");
        }
        else
        {
            Debug.LogError("[LỖI CHÍ MẠNG] Không tìm thấy script 'PokemonParrty' gắn trên Object Player! Bạn hãy kiểm tra lại nhé.");
        }

        isHealing = false;
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) playerObj = GameObject.Find("Player");

        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    // Vẽ một vòng tròn đỏ ở cửa sổ Scene để bạn dễ căn khoảng cách đứng của nhân vật
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}