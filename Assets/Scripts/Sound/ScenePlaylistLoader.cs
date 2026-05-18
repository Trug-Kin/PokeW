using System.Collections.Generic;
using UnityEngine;

public class ScenePlaylistLoader : MonoBehaviour
{
    [Header("Kéo th? các bài nh?c n?n c?a Scene này vào ?ây theo th? t?")]
    public List<AudioClip> sceneMusicList;

    void Start()
    {
        // Khi Scene v?a load xong, g?i danh sách này cho SoundManager phát
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StartPlaylist(sceneMusicList);
        }
    }
}