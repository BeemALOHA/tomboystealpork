using UnityEngine;

/// <summary>
/// ฉากหนึ่งท่อน (แบบคุกกี้รัน): มีพื้น + เพดาน + อุปสรรค ออกแบบเองเป็น prefab
/// Pivot (ตำแหน่งของ GameObject นี้) = ขอบซ้ายของท่อน
/// เลื่อนไปทางซ้ายตามความเร็วเกม แล้วลบตัวเองเมื่อพ้นจอ
/// </summary>
public class Chunk : MonoBehaviour
{
    [Tooltip("ความกว้างของท่อนนี้ (หน่วย world)")]
    public float width = 20f;
    [Tooltip("จะเริ่มสุ่มเจอท่อนนี้เมื่อวิ่งไปได้ระยะเท่านี้ (ใช้ไล่ความยาก)")]
    public float minDistance = 0f;
    [Tooltip("ลบทิ้งเมื่อขอบขวาเลยพิกัด X นี้ไปทางซ้าย")]
    public float destroyX = -30f;

    public float RightEdge => transform.position.x + width;

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;
        transform.position += Vector3.left * gm.Speed * Time.deltaTime;
        if (RightEdge < destroyX) Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.8f);
        Vector3 p = transform.position;
        Gizmos.DrawLine(p + Vector3.down * 5, p + Vector3.up * 5);
        Gizmos.DrawLine(p + new Vector3(width, -5), p + new Vector3(width, 5));
        Gizmos.DrawLine(p, p + Vector3.right * width);
    }
}
