using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// ระบบคะแนนหลัก:
/// - นับคะแนนตามเวลาที่อยู่รอด (survivalTimer * pointsPerSecond)
/// - รวมโบนัสเหยียบผี (AddStompBonus) เข้ากับคะแนนรวม (TotalScore) ทันที
/// </summary>
public class ScoreSystem : MonoBehaviour
{
    public static ScoreSystem Instance { get; private set; }

    [Header("คะแนนตามเวลาที่อยู่รอด")]
    [Tooltip("คะแนนต่อวินาทีที่อยู่รอด")]
    public float pointsPerSecond = 10f;
    [Tooltip("โบนัสเริ่มต้นเมื่อเหยียบผี (ใช้กรณีที่ Ghost ตั้ง scoreValue ไว้ <= 0)")]
    public int defaultStompBonus = 50;

    [Header("การแสดงผล UI (เลือกใส่อย่างใดอย่างหนึ่งหรือทั้งคู่)")]
    public TMP_Text scoreTextTMP;
    public Text scoreText;
    [Tooltip("รูปแบบข้อความ เช่น Score: {0} หรือ {0}")]
    public string scoreFormat = "{0}";

    private float survivalTimer;
    private int bonusScore;
    private int lastShownScore = -1;

    public int TotalScore => Mathf.FloorToInt(survivalTimer * pointsPerSecond) + bonusScore;
    public bool IsPlaying => GameManager.Instance == null || GameManager.Instance.IsPlaying;

    public event Action<int> OnScoreChanged;
    public event Action<int> OnBonusAdded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        RefreshScoreUI(force: true);
    }

    private void Update()
    {
        if (!IsPlaying) return;

        survivalTimer += Time.deltaTime;
        RefreshScoreUI();
    }

    public void AddStompBonus(int amount)
    {
        int added = amount > 0 ? amount : defaultStompBonus;
        if (added <= 0) return;

        bonusScore += added;
        OnBonusAdded?.Invoke(added);
        RefreshScoreUI(force: true);
    }

    public void ResetScore()
    {
        survivalTimer = 0f;
        bonusScore = 0;
        lastShownScore = -1;
        RefreshScoreUI(force: true);
    }

    private void RefreshScoreUI(bool force = false)
    {
        int current = TotalScore;
        if (!force && current == lastShownScore) return;
        lastShownScore = current;

        string formatted = string.Format(scoreFormat, current);

        if (scoreTextTMP != null)
            scoreTextTMP.text = formatted;

        if (scoreText != null)
            scoreText.text = formatted;

        OnScoreChanged?.Invoke(current);
    }
}