using UnityEngine;
using UnityEngine.UI;
using TMPro; // Thư viện TextMeshPro để sử dụng TextMeshProUGUI

public class PokeWTeamCredits : MonoBehaviour
{
    // Cấu trúc dữ liệu chứa thông tin của một thành viên
    [System.Serializable]
    public struct TeamMember
    {
        public string name;          // Tên thành viên
        public string role;          // Chức vụ
        [TextArea(3, 5)] 
        public string contributions; // Chi tiết đóng góp (hiển thị ô nhập rộng trong Inspector)
        public Sprite avatar;        // Ảnh chân dung pixel art
    }

    [Header("Dữ Liệu Nhóm Phát Triển")]
    public TeamMember[] teamMembers; // Mảng chứa danh sách thành viên

    [Header("Thành Phần UI Kết Nối")]
    public Image avatarImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI roleText;
    public TextMeshProUGUI contributionsText;
    public TextMeshProUGUI pageIndicatorText;

    private int currentPage = 0; // Trang hiện tại

    // Hàm này tự chạy mỗi khi bảng Credits được bật lên
    void OnEnable()
    {
        currentPage = 0; // Reset về người đầu tiên
        DisplayCurrentMember();
    }

    void Update()
    {
        // Bấm phím mũi tên Trái hoặc phím A để lùi về trang trước
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            ChangePage(-1);
        }
        // Bấm phím mũi tên Phải hoặc phím D để tiến tới trang sau
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            ChangePage(1);
        }
    }

    void ChangePage(int direction)
    {
        if (teamMembers.Length == 0) return;

        currentPage += direction;

        // Vòng lặp vô hạn: Nếu lùi quá người đầu tiên thì nhảy xuống người cuối cùng và ngược lại
        if (currentPage < 0)
        {
            currentPage = teamMembers.Length - 1;
        }
        else if (currentPage >= teamMembers.Length)
        {
            currentPage = 0;
        }

        DisplayCurrentMember();
    }

    // Hàm cập nhật dữ liệu từ mảng lên màn hình UI
    void DisplayCurrentMember()
    {
        if (teamMembers.Length == 0) return;

        TeamMember currentMember = teamMembers[currentPage];

        nameText.text = currentMember.name;
        roleText.text = currentMember.role;
        contributionsText.text = currentMember.contributions;
        
        // Nếu thành viên có ảnh chân dung thì hiện lên, nếu không thì ẩn ô ảnh đi
        if (currentMember.avatar != null)
        {
            avatarImage.sprite = currentMember.avatar;
            avatarImage.gameObject.SetActive(true);
        }
        else
        {
            avatarImage.gameObject.SetActive(false);
        }

        // Cập nhật số trang hiển thị (Ví dụ: < 1 / 3 >)
        pageIndicatorText.text = "< " + (currentPage + 1) + " / " + teamMembers.Length + " >";
    }
}
