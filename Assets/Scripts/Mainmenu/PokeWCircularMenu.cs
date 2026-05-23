using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

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
    private bool isSubMenuOpen = false;

    void Start()
    {
        UpdateMenuVisuals();

        // Gắn sự kiện chuột cho từng button
        for (int i = 0; i < menuOptions.Length; i++)
        {
            int index = i;

            EventTrigger trigger = menuOptions[i].gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = menuOptions[i].gameObject.AddComponent<EventTrigger>();

            trigger.triggers = new System.Collections.Generic.List<EventTrigger.Entry>();

            // Hover
            EventTrigger.Entry hoverEntry = new EventTrigger.Entry();
            hoverEntry.eventID = EventTriggerType.PointerEnter;
            hoverEntry.callback.AddListener((data) => { OnHover(index); });
            trigger.triggers.Add(hoverEntry);

            // Click
            EventTrigger.Entry clickEntry = new EventTrigger.Entry();
            clickEntry.eventID = EventTriggerType.PointerClick;
            clickEntry.callback.AddListener((data) => { OnClick(index); });
            trigger.triggers.Add(clickEntry);
        }
    }

    // Khi rê chuột vào
    void OnHover(int index)
    {
        if (isSubMenuOpen) return;

        currentIndex = index;
        UpdateMenuVisuals();

        SoundManager.Instance.PlayClickSound(); // âm hover
    }

    // Khi click
    void OnClick(int index)
    {
        if (isSubMenuOpen) return;

        currentIndex = index;
        ConfirmSelection();
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
            case 0:
                // 🔥 SỬA Ở ĐÂY: Đổi tên scene đích thành màn hình chọn Starter
                SceneManager.LoadScene("Start"); 
                break;
            case 1:
                Debug.Log("Tính năng tải file lưu đang phát triển!");
                break;
            case 2:
                OpenSettingsMenu();
                break;
            case 3:
                OpenCreditsMenu();
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

    void Update()
    {
        // Kiểm tra xem người chơi có bấm nút ESC không
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Nếu có một menu con (Settings hoặc Credits) đang mở thì mới đóng nó lại
            if (isSubMenuOpen)
            {
                CloseAllSubMenus();

                // (Tùy chọn) Phát một tiếng click nhẹ khi tắt menu cho game sinh động
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayClickSound();
                }
            }
        }
    }

}