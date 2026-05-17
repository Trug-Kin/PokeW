using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; // BẮT BUỘC: Thư viện quản lý tương tác tia chuột của Canvas
using System;

public class PokeWCircularMenu : MonoBehaviour
{
    [Header("Thành Phần Menu Chính")]
    public Image[] menuOptions; 
    public Vector3 selectedScale = new Vector3(1.2f, 1.2f, 1.2f);
    public Vector3 normalScale = Vector3.one;
    public Color selectedColor = Color.white;
    public Color unselectedColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);

    [Header("Màn Hình Con (Sub-Menus)")]
    public GameObject creditsPanel;  
    public GameObject settingsPanel; 

    private int currentIndex = 0; 
    private bool isSubMenuOpen = false; // Kiểm tra xem có đang mở menu con không

    void Start()
    {
        // 🔥 THẦN CHÚ TỰ ĐỘNG: Duyệt qua danh sách Image lựa chọn để cắm dây cảm biến chuột
        for (int i = 0; i < menuOptions.Length; i++)
        {
            if (menuOptions[i] == null) continue;
            int index = i;

            // Tự động gắn script nhận diện tia chuột vào từng Object lựa chọn
            PokewMenuMouseOption handler = menuOptions[i].gameObject.GetComponent<PokewMenuMouseOption>();
            if (handler == null) handler = menuOptions[i].gameObject.AddComponent<PokewMenuMouseOption>();

            // Khởi tạo cổng kết nối sự kiện Rê chuột (Hover) và Click chuột
            handler.Init(index, OnOptionHovered, OnOptionClicked);
        }

        UpdateMenuVisuals();
    }

    void Update()
    {
        // NẾU MENU CON ĐANG MỞ: Phím ESC hoặc Backspace vẫn được giữ làm dự phòng để đóng bảng con
        if (isSubMenuOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
            {
                CloseAllSubMenus();
            }
            return; 
        }

        // ĐÃ XÓA TOÀN BỘ: Logic kiểm tra phím bấm mũi tên di chuyển và nút Enter cũ đã được dọn dẹp sạch sẽ!
    }

    // ✔️ XỬ LÝ CHUỘT HOVER: Rê chuột đến ô nào, lập tức highlight ô đó sáng lên và phóng to!
    void OnOptionHovered(int index)
    {
        if (isSubMenuOpen) return; // Khóa tương tác menu chính nếu bảng tác giả/cài đặt đang mở

        currentIndex = index;
        UpdateMenuVisuals();
    }

    // ✔️ XỬ LÝ CHUỘT CLICK: Nhấn click chuột trái vào ô nào, thực thi tính năng của ô đó ngay
    void OnOptionClicked(int index)
    {
        if (isSubMenuOpen) return; // Khóa tương tác nếu bảng con đang mở

        currentIndex = index;
        UpdateMenuVisuals();
        ConfirmSelection(); // Kích hoạt chuyển cảnh hoặc mở bảng panel con tương ứng
    }

    void UpdateMenuVisuals()
    {
        for (int i = 0; i < menuOptions.Length; i++)
        {
            if (menuOptions[i] == null) continue;

            if (i == currentIndex)
            {
                menuOptions[i].transform.localScale = selectedScale;
                menuOptions[i].color = selectedColor;
            }
            else
            {
                menuOptions[i].transform.localScale = normalScale;
                menuOptions[i].color = unselectedColor;
            }
        }
    }

    void ConfirmSelection()
    {
        switch (currentIndex)
        {
            case 0: // Chơi Mới
                SceneManager.LoadScene("SampleScene"); 
                break;
            case 1: // Tiếp Tục
                Debug.Log("Tính năng tải file lưu đang phát triển!");
                break;
            case 2: // Cài Đặt
                OpenSettingsMenu(); 
                break;
            case 3: // Các Tác Giả
                OpenCreditsMenu(); 
                break;
            default:
                break;
        }
    }

    public void OpenCreditsMenu()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true); 
            isSubMenuOpen = true; 
        }
    }

    public void OpenSettingsMenu()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            isSubMenuOpen = true;
        }
    }

    public void CloseAllSubMenus()
    {
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        isSubMenuOpen = false;
    }

    // 🔥 HÀM PUBLIC MỚI: Bạn có thể gán hàm này vào linh kiện Image Back (nút quay lại) của bảng tác giả/cài đặt
    // Để người chơi có thể click chuột trực tiếp tắt bảng con đi thay vì bấm phím ESC!
    public void OnSubMenuCloseButtonClicked()
    {
        CloseAllSubMenus();
        Debug.Log("[CHUỘT] Đã click nút quay về menu chính!");
    }
}

// =========================================================================
// LỚP TRỢ LÝ PHỤ TRỢ: Biến các Image tĩnh thành vùng đón tia chuột cực nhạy
// =========================================================================
public class PokewMenuMouseOption : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    private int optionIndex;
    private Action<int> onHoverCallback;
    private Action<int> onClickCallback;

    public void Init(int index, Action<int> onHover, Action<int> onClick)
    {
        optionIndex = index;
        onHoverCallback = onHover;
        onClickCallback = onClick;

        // Đảm bảo tấm hình nền này được mở cổng đón tia quét chuột
        Image img = GetComponent<Image>();
        if (img != null)
        {
            img.raycastTarget = true;
        }
    }

    // Tự động kích hoạt khi chuột đi vào vùng ảnh
    public void OnPointerEnter(PointerEventData eventData)
    {
        onHoverCallback?.Invoke(optionIndex);
    }

    // Tự động kích hoạt khi chuột click vào vùng ảnh
    public void OnPointerClick(PointerEventData eventData)
    {
        onClickCallback?.Invoke(optionIndex);
    }
}