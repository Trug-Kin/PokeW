using System;
using System.Collections;
using UnityEngine;

public class GourdBallAnimation : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Hàm Public điều khiển toàn bộ luồng bay, lắc lư, và phân nhánh thành công/thất bại của DTH
    /// </summary>
    /// <param name="startPos">Vị trí đứng của quái ta / người chơi</param>
    /// <param name="targetPos">Vị trí đứng của quái hoang dã</param>
    /// <param name="isCaughtSuccess">Kết quả tính toán tỷ lệ bắt từ BattleSystem</param>
    /// <param name="onAnimationComplete">Hành động callback sau khi hoạt cảnh kết thúc hoàn toàn</param>
    public IEnumerator PlayCatchSequence(Vector3 startPos, Vector3 targetPos, bool isCaughtSuccess, Action<bool> onAnimationComplete)
    {
        // 1. Đưa quả cầu về điểm xuất phát và hiển thị nó lên
        transform.position = startPos;
        gameObject.SetActive(true);

        // Reset lại các thông số Parameter đề phòng lượt ném trước đó găm lại biến cũ
        if (animator != null)
        {
            animator.SetBool("IsTrue", false);
            animator.SetBool("IsFailed", false);
            // Chu kỳ mặc định của Animator sẽ tự chạy vào State tương ứng với DTH_wait (ô Default màu cam)
        }

        // 2. Di chuyển tịnh tiến quả cầu bay tới mục tiêu kẻ địch
        float speed = 6f; 
        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        // 3. Quả cầu chạm trúng mục tiêu -> Diễn hoạt cảnh Lắc Lư Chờ Đợi (DTH_wait)
        // Hãy để quả cầu lắc lư tầm 2 đến 3 giây để tạo cảm giác hồi hộp kịch tính cho người chơi
        yield return new WaitForSeconds(2.5f);

        // 4. KÍCH HOẠT PARAMETER: Dựa vào kết quả bắt để bẻ nhánh mũi tên Transition của bạn
        if (animator != null)
        {
            if (isCaughtSuccess)
            {
                animator.SetBool("IsTrue", true); // Kích hoạt mũi tên chuyển sang DTH_Success
            }
            else
            {
                animator.SetBool("IsFailed", true); // Kích hoạt mũi tên chuyển sang DTH_Failed
            }
        }

        // 5. Đợi diễn nốt Hoạt cảnh Thành công / Thất bại (Ví dụ mất thêm 1.5 giây nữa)
        yield return new WaitForSeconds(1.5f);

        // 6. Ẩn hoàn toàn quả cầu đi
        gameObject.SetActive(false);

        // 7. Bắn tín hiệu trả kết quả về cho BattleSystem xử lý tiếp luồng trận đấu
        onAnimationComplete?.Invoke(isCaughtSuccess);
    }
}