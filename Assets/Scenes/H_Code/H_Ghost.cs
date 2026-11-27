using UnityEngine;

/// <summary>
/// ผี: มิโกะเหยียบหัวได้ / ผู้ชายชนแล้วตาย
/// ลอยขึ้นลงเบา ๆ  Collider2D ติ๊ก Is Trigger
/// </summary>
public class Ghost : Hazard
{
    public int scoreValue = 50;

    [Header("ลอยขึ้นลง")]
    public float bobHeight = 0.25f;
    public float bobSpeed = 3f;

    [Header("ตอนโดนเหยียบ")]
    public GameObject stompEffect;
    public Animator animator;          // ถ้ามี trigger ชื่อ "Stomped" จะเล่น
    public float destroyDelay = 0.3f;

    public bool IsDead { get; private set; }

    float baseY;
    float phase;

    void Start()
    {
        baseY = transform.localPosition.y;
        phase = Random.value * Mathf.PI * 2f;
    }

    protected override void Update()
    {
        if (IsDead) return;
        base.Update();
        var p = transform.localPosition;
        p.y = baseY + Mathf.Sin(Time.time * bobSpeed + phase) * bobHeight;
        transform.localPosition = p;
    }

    public void Stomp()
    {
        if (IsDead) return;
        IsDead = true;
        foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
        if (stompEffect != null) Instantiate(stompEffect, transform.position, Quaternion.identity);
        if (animator != null) animator.SetTrigger("Stomped");
        else transform.localScale = new Vector3(transform.localScale.x * 1.3f, transform.localScale.y * 0.3f, 1f); // แบนแบบมาริโอ้
        Destroy(gameObject, destroyDelay);
    }
}
