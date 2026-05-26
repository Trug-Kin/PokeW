using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpandableMenu : MonoBehaviour
{
    [Header("Nút Menu Gốc (Nút để nhấn)")]
    public Button mainButton;

    [Header("Danh sách các nút sẽ bung ra")]
    public List<RectTransform> subButtons;

    [Header("Tốc độ bay (Càng lớn càng nhanh)")]
    public float moveSpeed = 8f;

    private bool isExpanded = false; 
    
    // 🔥 ĐỔI TỪ Vector3 SANG Vector2 (Hệ tọa độ 2D của UI)
    private List<Vector2> expandedPositions = new List<Vector2>();
    private Vector2 collapsedPosition;

    void Start()
    {
        // 1. SỬ DỤNG anchoredPosition ĐỂ LẤY TỌA ĐỘ ĐÃ ĐƯỢC NEO
        collapsedPosition = mainButton.GetComponent<RectTransform>().anchoredPosition;

        // 2. Lưu lại vị trí hiện tại của các nút con
        for (int i = 0; i < subButtons.Count; i++)
        {
            expandedPositions.Add(subButtons[i].anchoredPosition);
            
            // Ép các nút con chạy về nút gốc và ẩn đi
            subButtons[i].anchoredPosition = collapsedPosition;
            subButtons[i].gameObject.SetActive(false);
        }

        // 3. Gắn sự kiện nhấn nút
        mainButton.onClick.AddListener(ToggleMenu);
    }

    public void ToggleMenu()
    {
        isExpanded = !isExpanded;
        StopAllCoroutines(); 
        StartCoroutine(AnimateButtons());
    }

    IEnumerator AnimateButtons()
    {
        if (isExpanded)
        {
            foreach (var btn in subButtons) 
                btn.gameObject.SetActive(true);
        }

        float percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * moveSpeed;
            
            for (int i = 0; i < subButtons.Count; i++)
            {
                // Nội suy vị trí dựa trên UI Space (Vector2)
                Vector2 startPos = isExpanded ? collapsedPosition : expandedPositions[i];
                Vector2 endPos = isExpanded ? expandedPositions[i] : collapsedPosition;
                
                // CẬP NHẬT LẠI BẰNG anchoredPosition
                subButtons[i].anchoredPosition = Vector2.Lerp(startPos, endPos, percent);
            }
            yield return null;
        }

        if (!isExpanded)
        {
            foreach (var btn in subButtons) 
                btn.gameObject.SetActive(false);
        }
    }
}