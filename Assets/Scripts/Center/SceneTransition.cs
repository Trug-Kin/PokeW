using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [Header("Cấu hình Chuyển Scene")]
    public string sceneToLoad = "SampleScene";

    [Header("Khoảng cách nhận lệnh")]
    public float maxDistance = 1.8f;

    private Transform playerTransform;
    private bool positionRestored = false;

    // 🔥 BIẾN TĨNH (STATIC): Bí kíp giúp giữ giá trị vị trí trong bộ nhớ game, không bị xóa khi đổi Scene
    public static Vector3 savedPlayerPosition;
    public static bool hasSavedPosition = false;

    void Start()
    {
        FindPlayerInstance();
        TryRestorePosition();
    }

    void Update()
    {
        // Liên tục quét tìm Player nếu lúc Start game chưa load xong nhân vật
        if (playerTransform == null)
        {
            FindPlayerInstance();
            if (playerTransform != null)
            {
                TryRestorePosition();
            }
        }

        if (playerTransform != null)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);

            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log($"[HỆ THỐNG] Bạn bấm E! Khoảng cách tới cửa: {distance}");

                if (distance <= maxDistance)
                {
                    PrepareAndChangeScene();
                }
            }
        }
    }

    private void FindPlayerInstance()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) playerObj = GameObject.Find("Player");

        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    // 🟢 HÀM KHÔI PHỤC VỊ TRÍ CŨ
    private void TryRestorePosition()
    {
        if (positionRestored) return;

        // Nếu game vừa quay lại map chính (SampleScene) và kiểm tra thấy có vị trí cũ đã lưu
        if (SceneManager.GetActiveScene().name == "SampleScene" && hasSavedPosition && playerTransform != null)
        {
            // Dịch chuyển Player về đúng tọa độ trước khi vào cửa!
            playerTransform.position = savedPlayerPosition;
            Debug.Log($"[ĐỊNH VỊ] Đã khôi phục Player về đúng vị trí cũ trước cửa: {savedPlayerPosition}");

            // ⚠️ LƯU Ý ĐẶC BIỆT CHO GAME DI CHUYỂN THEO Ô (GRID MOVEMENT):
            // Vì game của bạn di chuyển theo ô gạch, nếu sau khi dịch chuyển mà bạn bấm nút di chuyển
            // nhân vật bỗng nhiên bị "giật lùi" hoặc "bắn" về vị trí xuất phát ban đầu, đó là vì 
            // bạn cần phải cập nhật lại biến 'Vị trí đích' (Target Position) trong script di chuyển của Player nữa.
            // CÁCH SỬA: Hãy xóa hai dấu lệnh /* */ ở dưới này ra và điền đúng tên Script di chuyển của bạn vào:
            /*
            var moveScript = playerTransform.GetComponent<DienTenScriptDiChuyenCuaPlayerVaoDay>(); 
            if (moveScript != null) {
                moveScript.targetPos = savedPlayerPosition; // Đổi 'targetPos' thành tên biến tọa độ đích của bạn
            }
            */

            positionRestored = true;
            hasSavedPosition = false; // Reset lại để không ảnh hưởng lần chơi sau
        }
    }

    void OnMouseDown()
    {
        PrepareAndChangeScene();
    }

    // 🟡 HÀM CHUẨN BỊ VÀ ĐỔI SCENE
    void PrepareAndChangeScene()
    {
        // Trước khi rời khỏi map chính (SampleScene) để vào phòng, hãy chụp lại vị trí đứng hiện tại của Player
        if (SceneManager.GetActiveScene().name == "SampleScene" && playerTransform != null)
        {
            savedPlayerPosition = playerTransform.position;
            hasSavedPosition = true;
            Debug.Log($"[LƯU TRỮ] Đã ghi nhớ vị trí Player trước khi vào phòng: {savedPlayerPosition}");
        }

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}