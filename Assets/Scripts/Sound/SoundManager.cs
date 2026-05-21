using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("--- CÁC KÊNH ÂM THANH (LOA) ---")]
    public AudioSource sfxSource;
    public AudioSource bgmSource;
    public AudioSource ambientSource; // Loa phụ phát tiếng bụi cỏ
    
    [Header("--- CÀI ĐẶT ÂM LƯỢNG ---")]
    [Range(0f, 1f)] public float normalVolume = 1f;
    [Range(0f, 1f)] public float duckedVolume = 0.2f; // Volume nhạc nền khi vào cỏ cao

    [Header("--- ÂM THANH MENU ---")]
    public AudioClip clickSound; // Âm thanh khi click UI
    public AudioClip healSound; // Âm thanh khi hồi máu

    // Biến quản lý Playlist nhạc nền
    private List<AudioClip> playlist = new List<AudioClip>();
    private int currentTrackIndex = 0;
    private bool isPlaylistActive = false;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void Update()
    {
        // TỰ ĐỘNG CHUYỂN BÀI KHI HẾT NHẠC (PLAYLIST LOOP)
        if (isPlaylistActive && bgmSource != null && !bgmSource.isPlaying && playlist.Count > 0)
        {
            NextTrack();
        }
    }

    // ==============================================
    // 1. QUẢN LÝ PLAYLIST NHẠC NỀN
    // ==============================================
    public void StartPlaylist(List<AudioClip> scenePlaylist)
    {
        if (scenePlaylist == null || scenePlaylist.Count == 0) return;

        playlist = scenePlaylist;
        currentTrackIndex = 0;
        isPlaylistActive = true;
        PlayCurrentTrack();
    }

    private void PlayCurrentTrack()
    {
        if (playlist.Count == 0 || bgmSource == null) return;
        bgmSource.clip = playlist[currentTrackIndex];
        bgmSource.volume = normalVolume;
        bgmSource.Play();
    }

    private void NextTrack()
    {
        // Tăng vị trí bài hát, nếu hết danh sách sẽ tự động quay về bài đầu tiên
        currentTrackIndex = (currentTrackIndex + 1) % playlist.Count;
        PlayCurrentTrack();
    }

    // ==============================================
    // 2. QUẢN LÝ ÂM THANH HIỆU ỨNG (SFX & MENU)
    // ==============================================
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null) sfxSource.PlayOneShot(clip);
    }

    // Hàm phục vụ cho MenuButtonsSound không bị lỗi
    public void PlayClickSound()
    {
        if (clickSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }

    // ==============================================
    // 3. XỬ LÝ VÙNG CỎ CAO (LONG GRASS)
    // ==============================================
    public void EnterLongGrass(AudioClip grassSound)
    {
        // Giảm âm lượng nhạc nền xuống mức nhỏ
        bgmSource.volume = duckedVolume;

        // Phát tiếng cỏ lặp đi lặp lại
        if (grassSound != null && ambientSource != null)
        {
            ambientSource.clip = grassSound;
            ambientSource.loop = true;
            ambientSource.Play();
        }
    }

    public void ExitLongGrass()
    {
        // Trả âm lượng nhạc nền về bình thường
        bgmSource.volume = normalVolume;

        // Tắt tiếng bụi cỏ đi
        if (ambientSource != null) ambientSource.Stop();
    }

    // ==============================================
    // 4. XỬ LÝ NHẠC CHIẾN ĐẤU (BATTLE SYSTEM)
    // ==============================================
    public void StartBattleMusic(AudioClip battleMusic)
    {
        isPlaylistActive = false; // Tạm dừng Playlist
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeToBattle(battleMusic));
    }

    public void StopBattleMusic()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        isPlaylistActive = true; // Kích hoạt lại Playlist
        PlayCurrentTrack();
    }

    private IEnumerator FadeToBattle(AudioClip battleClip)
    {
        float fadeTime = 0.4f; // Tốc độ nhỏ nhạc cũ (0.4 giây)

        // Nhỏ nhạc nền cũ dần về 0 rồi tắt
        while (bgmSource.volume > 0)
        {
            bgmSource.volume -= Time.deltaTime / fadeTime;
            yield return null;
        }
        bgmSource.Stop();

        // Đổi sang nhạc Battle và to dần lên
        bgmSource.clip = battleClip;
        bgmSource.Play();
        while (bgmSource.volume < normalVolume)
        {
            bgmSource.volume += Time.deltaTime / fadeTime;
            yield return null;
        }
    }
    public void PlayHealSound()
    {
        if (healSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(healSound);
        }
    }
}