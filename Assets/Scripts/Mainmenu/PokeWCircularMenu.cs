using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public class PokeWCircularMenu : MonoBehaviour
{
    [Header("Thành Phần Menu Chính")]
    public Image[] menuOptions; 
    public Vector3 selectedScale = new Vector3(1.2f, 1.2f, 1.2f);
    public Vector3 normalScale = Vector3.one;
    public Color selectedColor = Color.white;
    public Color unselectedColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);

    [Header("Màn Hình Con (Sub-Menus)")]
    // Kéo thả các Panel từ Hierarchy vào đây trong Inspector
    public GameObject creditsPanel;  
    public GameObject settingsPanel; 

    private int currentIndex = 0; 
    private bool isSubMenuOpen = false; // Kiểm tra xem có đang mở menu con không

    void Start()
    {
        UpdateMenuVisuals();
    }

    void Update()
    {
        // NẾU MENU CON ĐANG MỞ: Khóa di chuyển menu chính
        if (isSubMenuOpen)
        {
            // Bấm ESC hoặc Backspace để đóng menu con quay về menu chính
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
            {
                CloseAllSubMenus();
            }
            return; // Dừng không chạy logic di chuyển phía dưới
        }

        // Logic điều khiển menu chính bằng phím mũi tên
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.S))
        {
            currentIndex = (currentIndex + 1) % menuOptions.Length;
            UpdateMenuVisuals();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.W))
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = menuOptions.Length - 1;
            UpdateMenuVisuals();
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            ConfirmSelection();
        }
    }

    void UpdateMenuVisuals()
    {
        for (int i = 0; i < menuOptions.Length; i++)
        {
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
                OpenSettingsMenu(); // THAY THẾ DÒNG DEBUG.LOG BẰNG LỆNH NÀY
                break;
            case 3: // Các Tác Giả
            OpenCreditsMenu(); // THAY THẾ DÒNG DEBUG.LOG BẰNG LỆNH NÀY
            break;
        }
    }

    // --- CÁC HÀM HỖ TRỢ HIỆN/ẨN PANEL ---
    public void OpenCreditsMenu()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true); // Bật bảng [cite: 430]
            isSubMenuOpen = true; // Khóa menu xoay vòng [cite: 431]
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
        isSubMenuOpen = false; // Mở khóa cho menu chính hoạt động lại
    }
}