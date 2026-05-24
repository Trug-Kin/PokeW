using UnityEngine;

public class HealZone : MonoBehaviour
{
    [Header("Liên kết với Máy")]
    public HealingMachine healingMachine; // Kéo thả Object có gắn script HealingMachine vào đây

    private bool isNear = false;          // Kiểm tra Player có đứng trong vùng không

    void Update()
    {
        // Nhấn phím E khi đang đứng gần quầy
        if (isNear && Input.GetKeyDown(KeyCode.E))
        {
            healingMachine.StartHealingProcess();
        }
    }

    // Click chuột vào vùng kích hoạt này
    // Click chuột vào vùng kích hoạt này
    void OnMouseDown()
    {
        // Bắt buộc phải đứng trong vùng Collider (isNear = true) thì click mới có tác dụng
        if (isNear)
        {
            healingMachine.StartHealingProcess();
        }
        else
        {
            Debug.Log("Bạn đứng quá xa, không thể dùng máy!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isNear = true;
            Debug.Log("Đến gần quầy: Nhấn E hoặc Click để hồi máu");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isNear = false;
        }
    }
}