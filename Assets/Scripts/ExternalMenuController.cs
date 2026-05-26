using UnityEngine;

public class ExternalMenuController : MonoBehaviour
{
    [Header("Gắn các màn hình vào đây")]
    public GameObject bagScreen;
    public GameObject partyScreen;
    public GameObject settingPanel;

    // Hàm này đảm bảo chỉ 1 màn hình hiện lên tại 1 thời điểm
    public void CloseAllScreens()
    {
        if (bagScreen != null) bagScreen.SetActive(false);
        if (partyScreen != null) partyScreen.SetActive(false);
        if (settingPanel != null) settingPanel.SetActive(false);
    }

    // 🔥 THÊM MỚI: Hàm gắn vào nút "X" để tắt màn hình hiện tại
    public void CloseCurrentScreen()
    {
        CloseAllScreens();
    }

    public void ToggleBag()
    {
        bool isCurrentlyActive = bagScreen.activeSelf;
        CloseAllScreens(); // Đóng các màn hình khác lại
        bagScreen.SetActive(!isCurrentlyActive); // Bật/Tắt Bag
    }

    public void ToggleParty()
    {
        bool isCurrentlyActive = partyScreen.activeSelf;
        CloseAllScreens();
        partyScreen.SetActive(!isCurrentlyActive);
    }

    public void ToggleSettings()
    {
        bool isCurrentlyActive = settingPanel.activeSelf;
        CloseAllScreens();
        settingPanel.SetActive(!isCurrentlyActive);
    }
}