using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>คะแนน + หน้า Game Over + ปุ่มเริ่มใหม่</summary>
public class GameUI : MonoBehaviour
{
    public TMP_Text scoreText;
    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;
    public Button restartButton;

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (restartButton != null) restartButton.onClick.AddListener(() => GameManager.Instance.Restart());
        if (GameManager.Instance != null) GameManager.Instance.OnGameOver += ShowGameOver;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnGameOver -= ShowGameOver;
    }

    void Update()
    {
        if (scoreText != null && GameManager.Instance != null)
            scoreText.text = GameManager.Instance.Score.ToString();
    }

    void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = "คะแนน: " + GameManager.Instance.Score;
    }
}
