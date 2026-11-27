using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>คะแนน + หน้า LOSE (Game Over) + ปุ่ม Restart / MainMenu</summary>
public class GameUI : MonoBehaviour
{
    [Header("ระหว่างเล่น (ไม่บังคับ)")]
    public TMP_Text scoreText;

    [Header("หน้า LOSE")]
    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;     // ไม่บังคับ
    public Button restartButton;
    public Button mainMenuButton;

    [Tooltip("ชื่อฉากเมนูหลัก (ต้องอยู่ใน Build Profiles)")]
    public string mainMenuScene = "MainMenu";

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(Restart);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(BackToMenu);

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
        if (finalScoreText != null) finalScoreText.text = "SCORE: " + GameManager.Instance.Score;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }
}
