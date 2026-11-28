using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;      // Input System แบบใหม่ (Unity 6)
using UnityEngine.SceneManagement;

// สคริปต์ผู้เล่นสไตล์มาริโอ้: เดิน, กระโดด 2 จังหวะ, เหยียบหัวมอน, โดนมอนแล้วตาย
// มอนต้องตั้ง Tag เป็น "Enemy" และมี Collider2D
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("การเดิน")]
    public float maxSpeed = 8f;
    public float acceleration = 60f;
    public float deceleration = 70f;
    public float airControl = 0.6f;

    [Header("การกระโดด")]
    public float jumpForce = 16f;
    public float doubleJumpForce = 14f;  // แรงกระโดดจังหวะที่ 2
    public int maxJumps = 2;             // กระโดดได้กี่จังหวะ
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 3f;
    public float maxFallSpeed = 20f;
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;

    [Header("เช็คพื้น")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("มอนสเตอร์")]
    public string enemyTag = "Enemy";
    public float stompBounce = 12f;          // เด้งขึ้นหลังเหยียบหัว (กดกระโดดค้างจะเด้งสูงขึ้น)
    public bool destroyEnemyOnStomp = true;  // ปิดได้ถ้าสคริปต์มอนจัดการตายเอง

    [Header("การตาย")]
    public float deathPopForce = 14f;    // เด้งขึ้นตอนตายแบบมาริโอ้
    public float restartDelay = 2f;      // รอกี่วิแล้วเริ่มด่านใหม่
    public float fallDeathY = -15f;      // ตกต่ำกว่านี้ = ตาย

    private Rigidbody2D rb;
    private Collider2D col;
    private float moveInput;
    private bool isGrounded;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private bool jumpHeld;
    private bool facingRight = true;
    private int jumpsLeft;
    private bool isDead;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.freezeRotation = true;
        jumpsLeft = maxJumps;
    }

    void Update()
    {
        if (isDead) return;

        // --- รับ input ---
        var kb = Keyboard.current;
        var pad = Gamepad.current;

        moveInput = 0f;
        bool jumpPressed = false;
        jumpHeld = false;

        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) moveInput -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) moveInput += 1f;
            jumpPressed |= kb.spaceKey.wasPressedThisFrame;
            jumpHeld |= kb.spaceKey.isPressed;
        }
        if (pad != null)
        {
            float stick = pad.leftStick.x.ReadValue() + pad.dpad.x.ReadValue();
            if (Mathf.Abs(stick) > 0.2f) moveInput = Mathf.Sign(stick);
            jumpPressed |= pad.buttonSouth.wasPressedThisFrame;
            jumpHeld |= pad.buttonSouth.isPressed;
        }

        // --- เช็คพื้น ---
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded && rb.linearVelocity.y <= 0.01f)
        {
            coyoteCounter = coyoteTime;
            jumpsLeft = maxJumps;            // แตะพื้น = รีเซ็ตจำนวนกระโดด
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
            // เดินตกขอบโดยไม่ได้กระโดด = เสียจังหวะแรกไป เหลือกระโดดกลางอากาศได้ 1 ครั้ง
            if (coyoteCounter <= 0f && jumpsLeft == maxJumps) jumpsLeft = maxJumps - 1;
        }

        if (jumpPressed) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        // --- กระโดด ---
        if (jumpBufferCounter > 0f)
        {
            if (coyoteCounter > 0f)                 // จังหวะที่ 1 (จากพื้น)
            {
                Jump(jumpForce);
                jumpsLeft = maxJumps - 1;
            }
            else if (jumpsLeft > 0)                 // จังหวะที่ 2 (กลางอากาศ)
            {
                Jump(doubleJumpForce);
                jumpsLeft--;
            }
        }

        // --- หันหน้า ---
        if (moveInput > 0 && !facingRight) Flip();
        else if (moveInput < 0 && facingRight) Flip();

        // --- ตกเหว ---
        if (transform.position.y < fallDeathY) Die();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        float targetSpeed = moveInput * maxSpeed;
        float rate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        if (!isGrounded) rate *= airControl;

        float newX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, rate * Time.fixedDeltaTime);

        float newY = rb.linearVelocity.y;
        float g = Physics2D.gravity.y * rb.gravityScale;

        if (newY < 0f)
            newY += g * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        else if (newY > 0f && !jumpHeld)
            newY += g * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;

        newY = Mathf.Max(newY, -maxFallSpeed);
        rb.linearVelocity = new Vector2(newX, newY);
    }

    void Jump(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
        jumpBufferCounter = 0f;
        coyoteCounter = 0f;
    }

    // ===== ชนมอน =====
    void OnCollisionEnter2D(Collision2D collision) => HandleEnemy(collision.collider);
    void OnTriggerEnter2D(Collider2D other) => HandleEnemy(other); // เผื่อมอนใช้ Is Trigger

    void HandleEnemy(Collider2D enemy)
    {
        if (isDead || !enemy.CompareTag(enemyTag)) return;

        // เท้าผู้เล่นอยู่สูงกว่ากลางตัวมอน = เหยียบหัว
        bool stomped = col.bounds.min.y >= enemy.bounds.center.y;

        if (stomped)
        {
            // เรียกฟังก์ชัน Die() ในสคริปต์มอน (ถ้ามี)
            enemy.SendMessage("Die", SendMessageOptions.DontRequireReceiver);
            if (destroyEnemyOnStomp) Destroy(enemy.gameObject);

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, stompBounce);
            jumpsLeft = maxJumps - 1;    // เหยียบแล้วยังกระโดดกลางอากาศต่อได้
        }
        else
        {
            Die();
        }
    }

    // ===== ตาย =====
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        col.enabled = false;                         // ร่วงทะลุพื้นแบบมาริโอ้
        rb.linearVelocity = new Vector2(0f, deathPopForce);
        StartCoroutine(RestartLevel());
    }

    IEnumerator RestartLevel()
    {
        yield return new WaitForSeconds(restartDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}