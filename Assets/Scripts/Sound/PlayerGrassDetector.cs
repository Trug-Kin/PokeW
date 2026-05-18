using UnityEngine;

public class PlayerGrassDetector : MonoBehaviour
{
    [Header("File âm thanh xoạt xoạt khi đi trong cỏ")]
    public AudioClip grassWalkSound;

    private int grassLayerIndex;

    void Start()
    {
        // Lấy số ID của layer LongGrass để máy so sánh cho nhanh
        grassLayerIndex = LayerMask.NameToLayer("LongGrass");
    }

    // Khi chân người chơi chạm vào vùng cỏ cao
    private void OnTriggerEnter2D(Collider2D other) // Bỏ chữ "2D" nếu game của bạn là game 3D
    {
        if (other.gameObject.layer == grassLayerIndex)
        {
            SoundManager.Instance.EnterLongGrass(grassWalkSound);
        }
    }

    // Khi người chơi bước ra khỏi vùng cỏ cao
    private void OnTriggerExit2D(Collider2D other) // Bỏ chữ "2D" nếu game của bạn là game 3D
    {
        if (other.gameObject.layer == grassLayerIndex)
        {
            SoundManager.Instance.ExitLongGrass();
        }
    }
}