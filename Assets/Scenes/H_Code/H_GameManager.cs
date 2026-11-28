using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// ตัวคุมเกมหลัก: ความเร็ว, คะแนน, input ปุ่มเดียว (ส่งให้ตัวละครทั้งสองฝั่งพร้อมกัน), game over
/// วางไว้ 1 อันในฉาก
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("ความเร็วโลก (หน่วย/วินาที)")]
    public float startSpeed = 6f;
    public float maxSpeed = 14f;
    [Tooltip("ความเร็วเพิ่มขึ้นเท่าไรต่อวินาที")]
    public float acceleration = 0.1f;

    [Tooltip("รอกี่วินาทีก่อนเริ่มวิ่ง")]
    public float startDelay = 1f;
    float startTimer;

    [Header("UI คะแนน")]
    [Tooltip("ข้อความแสดงคะแนนแบบ TextMeshPro (ไม่บังคับ)")]
    public TMP_Text scoreTextTMP;
    [Tooltip("ข้อความแสดงคะแนนแบบ Legacy UI Text (ไม่บังคับ)")]
    public Text scoreTextLegacy;
    public string scoreFormat = "Score: {0}";

    [Header("UI Game Over")]
    [Tooltip("Panel หน้าจอ Game Over ที่ซ่อนไว้ตอนเริ่มเกม")]
    public GameObject gameOverPanel;

    public float Speed { get; private set; }
    public float Distance { get; private set; }
    public int BonusScore { get; private set; }
    public int Score => Mathf.FloorToInt(Distance) + BonusScore;
    public bool IsPlaying { get; private set; }

    /// <summary>ยิงเมื่อผู้เล่นกดฟริบ (ตัวละครทุกตัว subscribe)</summary>
    public event Action OnFlipPressed;
    public event Action OnGameOver;

    int lastShownScore = -1;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        Speed = startDelay > 0f ? 0f : startSpeed;
        IsPlaying = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        UpdateScoreUI(force: true);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        if (!IsPlaying) return;
        if (startTimer < startDelay)
        {
            startTimer += Time.deltaTime;
            if (startTimer >= startDelay) Speed = startSpeed;
            return;
        }

        Speed = Mathf.Min(maxSpeed, Speed + acceleration * Time.deltaTime);
        Distance += Speed * Time.deltaTime;

        UpdateScoreUI();

        if (FlipPressedThisFrame())
            OnFlipPressed?.Invoke();
    }

    static bool FlipPressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame))
            return true;
        var p = Pointer.current; // เมาส์ หรือ ทัชบนมือถือ
        return p != null && p.press.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)
            || Input.GetKeyDown(KeyCode.W) || Input.GetMouseButtonDown(0);
#endif
    }

    public void AddBonus(int amount)
    {
        BonusScore += amount;
        UpdateScoreUI(force: true);
    }

    /// <summary>ทางลัดให้สคริปต์ภายนอกเรียกจบเกมได้เช่นเดียวกับ GameOver()</summary>
    public void TriggerGameOver() => GameOver();

    public void GameOver()
    {
        if (!IsPlaying) return;
        IsPlaying = false;
        UpdateScoreUI(force: true);
        Time.timeScale = 0f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        OnGameOver?.Invoke();
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void UpdateScoreUI(bool force = false)
    {
        int current = Score;
        if (!force && current == lastShownScore) return;
        lastShownScore = current;

        string text = string.Format(scoreFormat, current);
        if (scoreTextTMP != null) scoreTextTMP.text = text;
        if (scoreTextLegacy != null) scoreTextLegacy.text = text;
    }
}