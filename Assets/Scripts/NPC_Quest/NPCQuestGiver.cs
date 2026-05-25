using UnityEngine;

public enum QuestType
{
    DanhBaiPokemon,
    DanhBaiPokemonBatKy,
    ChinhPhucGym,
    ThuPhucPokemon,
    GiaoNopVatPham
}

public class NPCQuestGiver : MonoBehaviour
{
    public enum QuestState { NotStarted, Accepted, Completed }

    [Header("--- THOẠI MẪU TỰ CHỈNH (GÕ TRONG UNITY) ---")]
    [TextArea(2, 5)]
    [Tooltip("Lời chào và giao nhiệm vụ khi chưa nhận")]
    public string thoaiChuaNhan = "";

    [TextArea(2, 5)]
    [Tooltip("Lời nhắc nhở khi Player đang làm việc, chưa xong")]
    public string thoaiDangLam = "";

    [TextArea(2, 5)]
    [Tooltip("Lời cảm ơn khi Player đã làm xong và trả nhiệm vụ")]
    public string thoaiDaXong = "";

    [Header("--- CẤU HÌNH NPC ĐẶC BIỆT (XUẤT HIỆN 1 LẦN) ---")]
    [Tooltip("Tích chọn nếu muốn NPC này nói chuyện xong là BIẾN MẤT MÃI MÃI")]
    public bool laNPCDacBiet = false;

    [Tooltip("Nhập ID duy nhất, không trùng nhau cho mỗi NPC đặc biệt (Vd: NPC_BiAn_01)")]
    public string idDuyNhatNPC = "NPC_DacBiet_1";

    private bool daNoiChuyenXong = false; // Trạng thái tạm thời trong trận

    [Header("--- CẤU HÌNH NHIỆM VỤ ---")]
    public QuestType loaiNhiemVu;
    public string tenMucTieu;
    public ItemBase vatPhamYeuCau;
    public int soLuongYeuCau = 1;

    [Header("--- PHẦN THƯỞNG ---")]
    public ItemBase phanThuong;
    public int rewardAmount = 1;

    [HideInInspector] public QuestState currentState = QuestState.NotStarted;
    [Header("--- KẾT NỐI UI ---")]
    public QuestUIManager uiManager;

    private void Start()
    {
        // KIỂM TRA LÀM MỚI GAME: Nếu là NPC đặc biệt và máy đã lưu là "đã gặp", xóa NPC khỏi bản đồ ngay lập tức
        if (laNPCDacBiet)
        {
            if (PlayerPrefs.GetInt(idDuyNhatNPC, 0) == 1)
            {
                gameObject.SetActive(false); // Ẩn hoàn toàn NPC
            }
        }
    }

    public string LayTenNhiemVu()
    {
        switch (loaiNhiemVu)
        {
            case QuestType.DanhBaiPokemon: return "Thử thách chiến đấu";
            case QuestType.DanhBaiPokemonBatKy: return "Thử thách tân thủ";
            case QuestType.ChinhPhucGym: return "Chinh phục đỉnh cao";
            case QuestType.ThuPhucPokemon: return "Nhà huấn luyện tài ba";
            case QuestType.GiaoNopVatPham: return "Thu thập vật phẩm";
            default: return "Nhiệm vụ mới";
        }
    }

    public string LayMoTaNhiemVu()
    {
        // HỆ THỐNG ĐỌC THOẠI THÔNG MINH:
        // Nếu bạn gõ chữ vào ô thoại mẫu nào thì game lấy thoại đó, nếu ĐỂ TRỐNG ô đó thì game tự dùng thoại hệ thống tự động.

        if (currentState == QuestState.NotStarted)
        {
            if (!string.IsNullOrEmpty(thoaiChuaNhan)) return thoaiChuaNhan;
            return $"Hãy đi đánh bại {soLuongYeuCau} con {tenMucTieu} rồi quay lại đây gặp tôi.";
        }
        else if (currentState == QuestState.Accepted)
        {
            if (!string.IsNullOrEmpty(thoaiDangLam)) return thoaiDangLam;
            return $"Bạn vẫn chưa hoàn thành mục tiêu yêu cầu, hãy tiếp tục cố gắng!";
        }
        else if (currentState == QuestState.Completed)
        {
            if (!string.IsNullOrEmpty(thoaiDaXong)) return thoaiDaXong;
            return "Cảm ơn bạn đã giúp đỡ tôi trước đó!";
        }
        return "Tôi có một yêu cầu dành cho bạn.";
    }

    public void InteractWithNPC(PlayerInventory player)
    {
        if (currentState == QuestState.NotStarted)
        {
            if (uiManager != null)
                uiManager.ShowQuestPanel(this, LayTenNhiemVu(), LayMoTaNhiemVu());

            if (laNPCDacBiet) daNoiChuyenXong = true;
        }
        else if (currentState == QuestState.Accepted)
        {
            bool duDieuKien = false;

            if (loaiNhiemVu == QuestType.GiaoNopVatPham && vatPhamYeuCau != null)
                duDieuKien = player.HasItem(vatPhamYeuCau.itemName, soLuongYeuCau);
            else if (loaiNhiemVu == QuestType.DanhBaiPokemonBatKy)
                duDieuKien = player.HasItem("AnyPokemon", soLuongYeuCau);
            else
                duDieuKien = player.HasItem(tenMucTieu, soLuongYeuCau);

            if (duDieuKien)
            {
                CompleteQuest(player);
                if (uiManager != null)
                    uiManager.ShowQuestPanel(this, "Hoàn thành!", LayMoTaNhiemVu());

                if (laNPCDacBiet) BienMatMaiMai(); // Xong nhiệm vụ đặc biệt -> Biến mất ngay
            }
            else
            {
                // Hiện bảng nhắc nhở lên UI thay vì Debug Log ngầm
                if (uiManager != null)
                    uiManager.ShowQuestPanel(this, "Nhắc nhở", LayMoTaNhiemVu());

                if (laNPCDacBiet) daNoiChuyenXong = true;
            }
        }
        else if (currentState == QuestState.Completed)
        {
            if (uiManager != null)
                uiManager.ShowQuestPanel(this, "Thành công", LayMoTaNhiemVu());
        }
    }

    public void AcceptQuest()
    {
        currentState = QuestState.Accepted;
        Debug.Log($"Đã nhận: {LayTenNhiemVu()}");

        // Nếu chấp nhận nhiệm vụ của NPC đặc biệt, họ sẽ biến mất luôn sau khi bạn tắt UI hoặc đi ra xa
        if (laNPCDacBiet) daNoiChuyenXong = true;
    }

    private void CompleteQuest(PlayerInventory player)
    {
        currentState = QuestState.Completed;
        if (phanThuong != null)
        {
            player.AddItem(phanThuong.itemName, rewardAmount);
        }
    }

    // Hàm xử lý biến mất vĩnh viễn
    private void BienMatMaiMai()
    {
        PlayerPrefs.SetInt(idDuyNhatNPC, 1); // Lưu vào bộ nhớ máy: Đã hoàn thành gặp gỡ
        PlayerPrefs.Save();
        gameObject.SetActive(false); // Ẩn khỏi map
        Debug.Log($"[NPC Event] NPC {idDuyNhatNPC} đã biến mất vĩnh viễn khỏi game.");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerInventory player = collision.GetComponent<PlayerInventory>();
            if (player != null && uiManager != null)
            {
                InteractWithNPC(player);
            }
        }
    }

    // Khi người chơi đi ra xa vùng tương tác
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Nếu là NPC đặc biệt và chuỗi hội thoại vừa diễn ra xong -> Biến mất khi player bước đi
            if (laNPCDacBiet && daNoiChuyenXong)
            {
                BienMatMaiMai();
            }
        }
    }
}