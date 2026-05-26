using UnityEngine;

public class SelfClose : MonoBehaviour
{
    public void CloseMe()
    {
        // 1. Tắt giao diện (Làm nó tàng hình)
        gameObject.SetActive(false);

        // ==========================================
        // 2. MỞ KHÓA GAME (Chọn 1 trong 2 cách dưới đây)
        // ==========================================

        // CÁCH A: Dùng nếu team cậu làm đơ game bằng cách ngưng đọng thời gian
        Time.timeScale = 1f; 

        // CÁCH B: Dùng nếu team cậu quản lý bằng State (Trạng thái) trong GameControl
        // (Xóa 2 dấu // ở đầu dòng dưới đi nếu team cậu dùng cách này)
        // GameControl.Instance.state = GameState.FreeRoam; 
    }
}