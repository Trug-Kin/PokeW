# 🎮 PokeW - 2D Turn-based RPG

> Đồ án môn học: Xây dựng Game 2D Pixel RPG trên nền tảng Unity.

## 📖 Mô tả (Description)
**PokeW** là một tựa game nhập vai đánh theo lượt (Turn-based RPG) đồ họa Pixel 2D mang âm hưởng của dòng game bắt thú cổ điển. Điểm nhấn đặc biệt của PokeW là việc thay thế hệ thống nguyên tố truyền thống bằng **Ma trận Tương Khắc Ngũ Hành (Kim, Mộc, Thủy, Hỏa, Thổ)**, mang lại chiều sâu chiến thuật mới mẻ. 

Game được thiết kế với kiến trúc dữ liệu ScriptableObject tối ưu, hệ thống máy trạng thái (State Machine) quản lý trận đấu mượt mà cùng giao diện UI động (Dynamic Data Binding) hiện đại.

## ✨ Tính năng nổi bật
* **Hệ thống Battle Turn-based:** Tính toán sát thương, tốc độ đánh và bắt thú dựa trên các công thức thuật toán chi tiết.
* **Tương khắc Ngũ Hành:** Hệ thống tính toán sát thương $O(1)$ thông qua ma trận 2D 5x5 chặt chẽ.
* **Quản lý Inventory & Party:** Giao diện túi đồ và đội hình 6 thành viên hoạt động linh hoạt, tự động sinh (instantiate) theo dữ liệu thực.
* **Hoạt ảnh & Âm thanh:** Tối ưu hóa Sprite Sheet góc nhìn Top-down và hệ thống SoundManager quản lý nhạc nền, hiệu ứng chiêu thức.

* 





## ⚙️ Hướng dẫn cài đặt (Installation Guide)

**Yêu cầu hệ thống:**
* Unity Editor phiên bản `2022.3.x` (LTS) *(Lưu ý: Thay đổi thành phiên bản nhóm đang dùng)*.
* Git cài đặt trên máy.

**Các bước chạy project:**
1. Clone repository về máy cá nhân:
   ```bash
   git clone [https://github.com/Tên-Github-Của-Nhóm/PokeW.git](https://github.com/Tên-Github-Của-Nhóm/PokeW.git)

