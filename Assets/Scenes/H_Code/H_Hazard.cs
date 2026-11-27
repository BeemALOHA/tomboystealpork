using UnityEngine;

/// <summary>
/// อุปสรรคที่ชนแล้วตาย (สัตว์ป่า, รถยนต์, ฝูงคน, ป้ายห้อย ฯลฯ)
/// Collider2D ต้องติ๊ก Is Trigger
/// วางเป็นลูกของ Chunk  (ตัวมันเองไม่ต้องมี Scroller)
/// </summary>
public class Hazard : MonoBehaviour
{
    [Tooltip("วิ่งเข้าหาผู้เล่นเร็วกว่าฉาก เช่น รถที่ขับสวนมา (0 = อยู่นิ่งกับพื้น)")]
    public float extraSpeedX = 0f;

    protected virtual void Update()
    {
        if (extraSpeedX != 0f)
            transform.position += Vector3.left * extraSpeedX * Time.deltaTime;
    }
}
