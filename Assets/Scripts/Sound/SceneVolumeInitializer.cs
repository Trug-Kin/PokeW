using UnityEngine;

public class SceneVolumeInitializer : MonoBehaviour
{
    private const string VOLUME_KEY = "GlobalGameVolume";

    void Start()
    {
        // 1. Ngay khi scene này được tải xong, mở bộ nhớ máy ra đọc mức âm lượng đã lưu ở Menu ngoài
        float savedVolume = PlayerPrefs.GetFloat(VOLUME_KEY, 1f);

        // 2. Tìm TẤT CẢ các loa (AudioSource) đang có trong Scene mới này
        AudioSource[] allAudioSourcesInScene = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

        // 3. Ép tất cả các loa phải chạy theo mức âm lượng đã lưu
        foreach (AudioSource source in allAudioSourcesInScene)
        {
            if (source != null)
            {
                source.volume = savedVolume;
            }
        }

        Debug.Log($"[Volume Sync] Đã đồng bộ thành công âm lượng {savedVolume * 100}% cho Scene mới!");
    }
}