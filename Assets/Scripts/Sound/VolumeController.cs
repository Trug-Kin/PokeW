using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VolumeController : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI volumeText;
    public Slider volumeSlider;

    // Tên chìa khóa để lưu vào bộ nhớ máy tính/điện thoại
    private const string VOLUME_KEY = "GlobalGameVolume";

    void Start()
    {
        // 1. Đọc âm lượng đã lưu từ trước, nếu chơi lần đầu tiên chưa lưu thì mặc định là 1f (100%)
        float savedVolume = PlayerPrefs.GetFloat(VOLUME_KEY, 1f);

        // 2. Đồng bộ giá trị lên thanh trượt UI
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
        }

        // 3. Áp dụng mức âm lượng này cho tất cả loa đang có ở Menu hiện tại
        SetAllVolumes(savedVolume);
    }

    // Hàm gọi khi kéo thanh trượt bằng chuột
    public void OnSliderValueChanged(float value)
    {
        SetAllVolumes(value);

        // [QUAN TRỌNG] Lưu giá trị âm lượng mới vào bộ nhớ máy
        PlayerPrefs.SetFloat(VOLUME_KEY, value);
        PlayerPrefs.Save(); // Lệnh ép hệ thống ghi nhớ ngay lập tức
    }

    // Hàm gắn vào nút TĂNG (+5%)
    public void IncreaseVolume()
    {
        if (volumeSlider != null)
        {
            volumeSlider.value = Mathf.Clamp01(volumeSlider.value + 0.05f);
            // Khi gán slider.value, hàm OnSliderValueChanged ở trên sẽ tự động kích hoạt để lưu
        }
    }

    // Hàm gắn vào nút GIẢM (-5%)
    public void DecreaseVolume()
    {
        if (volumeSlider != null)
        {
            volumeSlider.value = Mathf.Clamp01(volumeSlider.value - 0.05f);
            // Khi gán slider.value, hàm OnSliderValueChanged ở trên sẽ tự động kích hoạt để lưu
        }
    }

    private void SetAllVolumes(float value)
    {
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource source in allAudioSources)
        {
            if (source != null)
            {
                source.volume = value;
            }
        }
        UpdateVolumeText(value);
    }

    private void UpdateVolumeText(float volumeValue)
    {
        if (volumeText != null)
        {
            int percentage = Mathf.RoundToInt(volumeValue * 100);
            volumeText.text = percentage.ToString() + "%";
        }
    }
}