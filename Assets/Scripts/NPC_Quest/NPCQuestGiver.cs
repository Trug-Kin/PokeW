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

    [Header("--- THOẠI MẪU TỰ CHỈNH ---")]
    [TextArea(2, 5)] public string thoaiChuaNhan = "";
    [TextArea(2, 5)] public string thoaiDangLam = "";
    [TextArea(2, 5)] public string thoaiDaXong = "";

    [Header("--- CẤU HÌNH NPC ĐẶC BIỆT ---")]
    public bool laNPCDacBiet = false;
    public string idDuyNhatNPC = "NPC_DacBiet_1";
    private bool daNoiChuyenXong = false; 

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
        if (laNPCDacBiet)
        {
            if (PlayerPrefs.GetInt(idDuyNhatNPC, 0) == 1)
            {
                gameObject.SetActive(false); 
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

            // 🔥 ĐÃ FIX LỖI: Truyền thẳng ItemBase thay vì truyền chữ
            if (loaiNhiemVu == QuestType.GiaoNopVatPham && vatPhamYeuCau != null)
                duDieuKien = player.HasItem(vatPhamYeuCau, soLuongYeuCau); 
            else if (loaiNhiemVu == QuestType.DanhBaiPokemonBatKy)
                duDieuKien = player.HasItem("AnyPokemon", soLuongYeuCau);
            else
                duDieuKien = player.HasItem(tenMucTieu, soLuongYeuCau);

            if (duDieuKien)
            {
                CompleteQuest(player);
                if (uiManager != null)
                    uiManager.ShowQuestPanel(this, "Hoàn thành!", LayMoTaNhiemVu());

                if (laNPCDacBiet) BienMatMaiMai(); 
            }
            else
            {
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

        if (laNPCDacBiet) daNoiChuyenXong = true;
    }

    private void CompleteQuest(PlayerInventory player)
    {
        currentState = QuestState.Completed;
        if (phanThuong != null)
        {
            // 🔥 ĐÃ FIX LỖI: Truyền thẳng ItemBase
            player.AddItem(phanThuong, rewardAmount); 
        }
    }

    private void BienMatMaiMai()
    {
        PlayerPrefs.SetInt(idDuyNhatNPC, 1); 
        PlayerPrefs.Save();
        gameObject.SetActive(false); 
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

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (laNPCDacBiet && daNoiChuyenXong)
            {
                BienMatMaiMai();
            }
        }
    }
}