using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RedObstacle : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("Tag ของตัวละครผู้เล่น (ถ้าไม่ใช้ Tag ให้เว้นว่างไว้เพื่อเช็กจาก Component/ชื่อได้)")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("หากผู้เล่นยังไม่ได้ตั้ง Tag ให้ติ๊กเปิดไว้เพื่อเช็กจากชื่อ Object ที่มีคำว่า Player")]
    [SerializeField] private bool fallbackCheckNameContainsPlayer = true;

    private bool hasTriggered;

    private void Reset()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryTriggerGameOver(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryTriggerGameOver(collision.gameObject);
    }

    private void TryTriggerGameOver(GameObject hitObject)
    {
        if (hasTriggered || hitObject == null) return;

        bool isPlayer = false;
        if (!string.IsNullOrEmpty(playerTag) && hitObject.CompareTag(playerTag))
        {
            isPlayer = true;
        }
        else if (fallbackCheckNameContainsPlayer &&
                 hitObject.name.IndexOf("Player", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            isPlayer = true;
        }

        if (!isPlayer) return;

        hasTriggered = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }
        else
        {
            // สำรองกรณีเทสต์ฉากเดี่ยวที่ยังไม่ได้วาง GameManager
            Time.timeScale = 0f;
            Debug.LogWarning("RedObstacle: ไม่พบ GameManager.Instance ใน Scene จึงหยุดเวลา (Time.timeScale = 0) ชั่วคราว");
        }
    }
}