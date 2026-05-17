using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButtonSound : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    // Bắt sự kiện khi rê chuột vào nút
    public void OnPointerEnter(PointerEventData eventData)
    {
        SoundManager.Instance.PlayClickSound(); // Gọi luôn tiếng click
    }

    // Bắt sự kiện khi dùng phím WASD / Mũi tên để di chuyển tới nút này
    public void OnSelect(BaseEventData eventData)
    {
        Debug.Log("Bàn phím vừa di chuyển tới nút: " + gameObject.name);
        SoundManager.Instance.PlayClickSound(); // Cũng gọi chung tiếng click
    }
}