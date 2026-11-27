using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public int direction = 1; // 1 = ขวา, -1 = ซ้าย

    [Header("Ground & Wall Check")]
    public Transform groundCheck;
    public Transform wallCheck;
    public float groundCheckDistance = 0.5f;
    public float wallCheckDistance = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ApplyDirectionScale();
    }

    private void FixedUpdate()
    {
        // เดินไปตามทิศทางปัจจุบัน
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        bool hasGroundAhead = groundCheck != null &&
            Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);

        bool hitWallAhead = wallCheck != null &&
            Physics2D.Raycast(wallCheck.position, Vector2.right * direction, wallCheckDistance, groundLayer);

        // ถ้าข้างหน้าไม่มีพื้น หรือชนกำแพง ให้กลับตัว
        if (!hasGroundAhead || hitWallAhead)
        {
            Flip();
        }
    }

    private void Flip()
    {
        direction *= -1;
        ApplyDirectionScale();
    }

    private void ApplyDirectionScale()
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.right * direction * wallCheckDistance);
        }
    }
}