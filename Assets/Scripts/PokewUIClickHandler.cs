using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

// Script này giúp biến bất kỳ Image/Text thông thường nào ngoài Canvas thành nút bấm chuột
public class PokewUIClickHandler : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] UnityEvent OnClicked; // Sự kiện sẽ được gọi khi bạn click chuột vào hình ảnh

    // Hàm IPointerClickHandler sẽ tự động chạy khi bạn click chuột vào vật thể
    public void OnPointerClick(PointerEventData eventData)
    {
        // Kích hoạt sự kiện đã được gán ngoài Inspector của Unity
        OnClicked?.Invoke();
    }
}