using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ตัวละครวิ่ง (ใช้ทั้งมิโกะและผู้ชาย)
/// - ตัวละครอยู่กับที่แกน X  โลกเป็นฝ่ายเลื่อนมาหา
/// - กดปุ่ม = กลับแรงโน้มถ่วง ไปติดเพดาน/พื้น
/// - canStompGhosts = true สำหรับมิโกะ (เหยียบหัวผีได้แบบมาริโอ้)
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class RunnerController : MonoBehaviour
{
    [Header("ความสามารถ")]
    [Tooltip("ติ๊กให้มิโกะ  ปล่อยว่างให้ผู้ชาย")]
    public bool canStompGhosts = false;

    [Header("ฟิสิกส์")]
    public float gravityScale = 5f;
    [Tooltip("แรงพุ่งตอนฟริบ ทำให้ไปถึงเพดานเร็วขึ้น")]
    public float flipKick = 4f;
    [Tooltip("ฟริบกลางอากาศได้ไหม (ปิดไว้ = ต้องแตะพื้น/เพดานก่อน)")]
    public bool allowAirFlip = false;
    [Tooltip("กดล่วงหน้าก่อนแตะพื้นได้กี่วินาที (ช่วยให้ควบคุมลื่น)")]
    public float flipBufferTime = 0.12f;
    public float stompBounce = 9f;

    [Header("เช็คพื้น/เพดาน")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.08f;

    [Header("ตกหลุม = ตาย (พิกัด world Y ของช่องนี้)")]
    public float killBelowY = -6f;
    public float killAboveY = 6f;

    [Header("ภาพ (ไม่บังคับ)")]
    [Tooltip("ลูก (child) ที่มี SpriteRenderer  จะถูกกลับหัวตอนฟริบ")]
    public Transform visual;
    public float flipVisualTime = 0.12f;
    public Animator animator;
    public GameObject deathEffect;

    public bool IsGrounded { get; private set; }
    public bool IsFlipped => gravityDir > 0;
    public bool IsDead { get; private set; }

    Rigidbody2D rb;
    Collider2D col;
    int gravityDir = -1;          // -1 = ลงพื้น, +1 = ขึ้นเพดาน
    float bufferTimer;
    float baseScaleY = 1f;
    readonly HashSet<string> animParams = new HashSet<string>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = gravityScale;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (visual == null) visual = transform;
        baseScaleY = Mathf.Abs(visual.localScale.y);

        if (animator != null)
            foreach (var p in animator.parameters) animParams.Add(p.name);
    }

    void Start()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnFlipPressed += RequestFlip;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnFlipPressed -= RequestFlip;
    }

    void RequestFlip() => bufferTimer = flipBufferTime;

    void FixedUpdate()
    {
        IsGrounded = CheckGround();
    }

    void Update()
    {
        if (IsDead) return;

        // input buffer: กดตอนยังลอยอยู่ แล้วแตะพื้นทันเวลา ก็ยังฟริบให้
        if (bufferTimer > 0f)
        {
            bufferTimer -= Time.deltaTime;
            if (IsGrounded || allowAirFlip)
            {
                bufferTimer = 0f;
                DoFlip();
            }
        }

        // ตกหลุม
        float y = transform.position.y;
        if (y < killBelowY || y > killAboveY) Die();

        // หมุนภาพกลับหัวแบบนุ่ม ๆ (scale Y ผ่าน 0 = ดูเหมือนบิดตัว)
        var s = visual.localScale;
        float target = baseScaleY * -gravityDir;
        float step = (baseScaleY * 2f / Mathf.Max(0.01f, flipVisualTime)) * Time.deltaTime;
        s.y = Mathf.MoveTowards(s.y, target, step);
        visual.localScale = s;

        SetBool("Grounded", IsGrounded);
        SetBool("Flipped", IsFlipped);
    }

    void DoFlip()
    {
        gravityDir = -gravityDir;
        rb.gravityScale = gravityScale * -gravityDir; // -1 => บวก (ลง), +1 => ลบ (ขึ้น)
        SetVelY(gravityDir * flipKick);
        IsGrounded = false;
        SetTrigger("Flip");
    }

    bool CheckGround()
    {
        Bounds b = col.bounds;
        Vector2 size = new Vector2(b.size.x * 0.9f, b.size.y * 0.9f);
        float dist = groundCheckDistance + b.size.y * 0.05f;
        RaycastHit2D hit = Physics2D.BoxCast(b.center, size, 0f, new Vector2(0f, gravityDir), dist, groundLayer);
        return hit.collider != null && !hit.collider.isTrigger;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsDead) return;

        var ghost = other.GetComponentInParent<Ghost>();
        if (ghost != null)
        {
            if (ghost.IsDead) return;
            if (canStompGhosts && IsStomping(other))
            {
                ghost.Stomp();
                SetVelY(-gravityDir * stompBounce); // เด้งกลับ
                GameManager.Instance?.AddBonus(ghost.scoreValue);
                return;
            }
            Die();
            return;
        }

        if (other.GetComponentInParent<Hazard>() != null) Die();
    }

    /// <summary>
    /// เหยียบ = กำลังเคลื่อนไปทางแรงโน้มถ่วง และอยู่ "เหนือ" ผี (เทียบกับแรงโน้มถ่วงตอนนั้น)
    /// เลยใช้ได้ทั้งตอนตกลงพื้น และตอนฟริบพุ่งขึ้นไปเหยียบผีที่ลอยใกล้เพดาน
    /// </summary>
    bool IsStomping(Collider2D ghostCol)
    {
        float vAlongGravity = GetVelY() * gravityDir;
        float relative = (col.bounds.center.y - ghostCol.bounds.center.y) * -gravityDir;
        return vAlongGravity > 0.1f && relative > ghostCol.bounds.extents.y * 0.3f;
    }

    public void Die()
    {
        if (IsDead) return;
        IsDead = true;
        SetTrigger("Dead");
        if (deathEffect != null) Instantiate(deathEffect, transform.position, Quaternion.identity);
        GameManager.Instance?.GameOver();
    }

    // ---------- helpers ----------
    float GetVelY()
    {
#if UNITY_6000_0_OR_NEWER
        return rb.linearVelocity.y;
#else
        return rb.velocity.y;
#endif
    }

    void SetVelY(float y)
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = new Vector2(0f, y);
#else
        rb.velocity = new Vector2(0f, y);
#endif
    }

    void SetBool(string n, bool v) { if (animParams.Contains(n)) animator.SetBool(n, v); }
    void SetTrigger(string n) { if (animParams.Contains(n)) animator.SetTrigger(n); }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float x = transform.position.x;
        Gizmos.DrawLine(new Vector3(x - 3, killBelowY), new Vector3(x + 3, killBelowY));
        Gizmos.DrawLine(new Vector3(x - 3, killAboveY), new Vector3(x + 3, killAboveY));
    }
}
