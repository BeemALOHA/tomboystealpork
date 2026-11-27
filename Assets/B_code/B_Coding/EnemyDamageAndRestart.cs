using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyDamageAndRestart : MonoBehaviour
{
    [Header("Game Over UI")]
    public GameObject gameOverPanel;

    [Header("Restart Settings")]
    public float restartDelay = 3f;
    public string playerTag = "Player";

    private bool isTriggered = false;

    private void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TriggerGameOverIfPlayer(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TriggerGameOverIfPlayer(other.gameObject);
    }

    private void TriggerGameOverIfPlayer(GameObject target)
    {
        if (isTriggered) return;

        if (target.CompareTag(playerTag))
        {
            isTriggered = true;
            StartCoroutine(GameOverRoutine(target));
        }
    }

    private IEnumerator GameOverRoutine(GameObject player)
    {
        // แสดงหน้า Game Over
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // หยุดการเคลื่อนที่ของผู้เล่น (ถ้ามี Rigidbody2D)
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.simulated = false;
        }

        // รอ 3 วินาที (ใช้ WaitForSecondsRealtime เพื่อไม่ให้ติดถ้าเคยปรับ Time.timeScale = 0)
        yield return new WaitForSecondsRealtime(restartDelay);

        // รีโหลดซีนปัจจุบันใหม่
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }
}