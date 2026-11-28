using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Wallobstacle : MonoBehaviour
{
    [Header("ผู้เล่นที่ชนแล้วแพ้ได้")]
    [Tooltip("ลากตัวละครผู้เล่นที่ต้องการให้ชนสิ่งกีดขวางนี้แล้ว Game Over มาใส่ (เว้นว่างไว้ได้ถ้าไม่ต้องการให้ชนตัวไหน)")]
    [SerializeField] private RunnerController[] allowedPlayers;

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
        if (!IsAllowedPlayer(hitObject)) return;

        hasTriggered = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        else
        {
            Time.timeScale = 0f;
            Debug.LogWarning("RedObstacle: ไม่พบ GameManager.Instance ใน Scene จึงหยุดเวลา (Time.timeScale = 0) ชั่วคราว");
        }
    }

    private bool IsAllowedPlayer(GameObject hitObject)
    {
        if (allowedPlayers == null || allowedPlayers.Length == 0) return false;

        RunnerController hitRunner = hitObject.GetComponentInParent<RunnerController>();
        if (hitRunner == null) return false;

        for (int i = 0; i < allowedPlayers.Length; i++)
        {
            if (allowedPlayers[i] == hitRunner)
            {
                return true;
            }
        }

        return false;
    }
}