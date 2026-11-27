using UnityEngine;

/// <summary>
/// ฉากหลังเลื่อนวนไม่รู้จบ
/// วิธีใช้: สร้าง GameObject เปล่า ใส่สคริปต์นี้ แล้วใส่รูปเดียวกัน 2 ชิ้นเป็นลูก วางชิดกันซ้าย-ขวา
/// factor: ภูเขาไกล ๆ = 0.1–0.3, ต้นไม้ใกล้ = 0.6–0.8, พื้น = 1
/// </summary>
public class ParallaxLayer : MonoBehaviour
{
    [Range(0f, 1.5f)] public float factor = 0.5f;
    [Tooltip("ความกว้างรูป 1 ชิ้น (0 = อ่านจาก SpriteRenderer ลูกตัวแรกเอง)")]
    public float tileWidth = 0f;

    float startX;

    void Start()
    {
        startX = transform.position.x;
        if (tileWidth <= 0f)
        {
            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null) tileWidth = sr.bounds.size.x;
        }
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null || tileWidth <= 0f) return;

        transform.position += Vector3.left * gm.Speed * factor * Time.deltaTime;
        while (startX - transform.position.x >= tileWidth)
            transform.position += Vector3.right * tileWidth;
    }
}
