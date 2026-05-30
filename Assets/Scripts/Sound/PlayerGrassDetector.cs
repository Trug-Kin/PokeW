using UnityEngine;

public class PlayerGrassDetector : MonoBehaviour
{
    [Header("File âm thanh xoạt xoạt khi đi trong cỏ")]
    public AudioClip grassWalkSound;

    private int grassLayerIndex;

    void Start()
    {
        // Lấy số ID của layer LongGrass
        grassLayerIndex = LayerMask.NameToLayer("LongGrass");

        // KHẢO SÁT 1: Kiểm tra xem Unity có tìm thấy Layer này không
        if (grassLayerIndex == -1)
        {
            Debug.LogError("⚠️ LỖI NGẦM: Unity KHÔNG tìm thấy Layer nào tên là 'LongGrass'. Hãy check lại chính tả trong bảng Layer (phải viết liền, đúng hoa thường)!");
        }
    }

    // Khi chân người chơi chạm vào vùng cỏ cao
    private void OnTriggerEnter2D(Collider2D other)
    {
        // KHẢO SÁT 2: Kiểm tra xem có bất kỳ va chạm nào xảy ra không
        Debug.Log($"[Grass Detector] Vừa chạm vào Object: {other.name} | Layer của Object đó là: {LayerMask.LayerToName(other.gameObject.layer)}");

        if (other.gameObject.layer == grassLayerIndex)
        {
            // KHẢO SÁT 3: Kiểm tra xem file âm thanh đã được kéo vào chưa
            if (grassWalkSound == null)
            {
                Debug.LogError("⚠️ LỖI: Bạn chưa kéo file âm thanh vào ô 'Grass Walk Sound' trên đối tượng Player!");
                return;
            }

            Debug.Log("🔥 TUYỆT VỜI: Đã trùng khớp Layer 'LongGrass'! Đang kích hoạt loa phát âm thanh...");
            SoundManager.Instance.EnterLongGrass(grassWalkSound);
        }
    }

    // Khi người chơi bước ra khỏi vùng cỏ cao
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == grassLayerIndex)
        {
            if (SoundManager.Instance != null)
            {
                Debug.Log("🚶 Đã bước ra khỏi cỏ. Đang tắt âm thanh bụi cỏ...");
                SoundManager.Instance.ExitLongGrass();
            }
        }
    }
}