using UnityEngine;
using System.Collections;
using System;

public class PlayerControl : MonoBehaviour
{
    public float Movespeed = 5f;
    public bool isMoving;
    public Vector2 input;
    private Vector2 lastMove = Vector2.down;
    private Animator animator;
    public LayerMask solidObjectLayer;
    public LayerMask GrassLayer; // Đảm bảo bạn đã tick đúng layer Grass trên Unity Editor!
    public bool canMove = true;
    public event Action OnEncountered;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("Animator component not found on Player. Animations will not play.");
        }
    }

     public void HandleUpdate()
    {
        if (!canMove) return;
        if (!isMoving)
        {

            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            if (input.x != 0) input.y = 0; // prevent diagonal movement
            if (input != Vector2.zero)
            {
                if (animator != null)
                {
                    animator.SetFloat("MoveX", input.x);
                    animator.SetFloat("MoveY", input.y);
                }

                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;
                if (IsWalkable(targetPos))
                    StartCoroutine(Move(targetPos));
            }
        }
        animator.SetBool("isMoving", isMoving);
    }

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, Movespeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
        isMoving = false;
        CheckForEncounter();
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        Collider2D hit = Physics2D.OverlapPoint(targetPos, solidObjectLayer);

        if (hit != null)
        {
            return false;
        }

        return true;
    }

    private void CheckForEncounter()
    {
        // 1. Kiểm tra xem điểm nhân vật vừa bước vào có thuộc Layer Bụi Cỏ không
        if (Physics2D.OverlapPoint(transform.position, GrassLayer) != null)
        {
            // 2. Tung xúc xắc tỷ lệ 10% gặp quái
            if (UnityEngine.Random.Range(1, 101) <= 10) 
            {
                var party = GetComponent<PokemonParrty>(); 
                
                // 3. LỚP ÁO GIÁP: Chỉ bắt đầu trận đấu NẾU trong đội hình còn ít nhất 1 Pokemon khỏe mạnh
                if (party != null && party.GetHealthyPokemon() != null)
                {
                    animator.SetBool("isMoving", false);
                    isMoving = false;
                    
                    // Phát tín hiệu cho GameControl mở màn hình Battle
                    OnEncountered?.Invoke(); 
                }
                else
                {
                    Debug.Log("Cả đội đã kiệt sức! Tránh giao tranh và lẩn trốn an toàn qua bụi cỏ...");
                }
            }
        }
    }
}